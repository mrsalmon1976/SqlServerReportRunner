# SqlServerReportRunner

Windows service that can be used to run reports from SQL Server and persist the files to disk.  Reports are run from a queue and saved to disk in a variety of formats.  This can be useful in preventing too many reports running simultaneously, and save on network traffic in the case of SSRS reports.

[![Build status](https://ci.appveyor.com/api/projects/status/le79ah3krcgg3k3p?svg=true)](https://ci.appveyor.com/project/mrsalmon1976/sqlserverreportrunner)

## Configuration Options

Key | Description | Example
------------- | ------------- | -------------
ServiceUserName | Windows account under which the service should run as when installed.  Should be represented as DOMAIN\UserName.  Leaving this blank will result in the service running as Local System. | MYDOMAIN\matt
ServicePassword | Windows password for the configured account.  Leave blank if ServiceUserName is blank. | mypassword
PollInterval | How often the database table is polled to see if there are any new reports to run (in seconds) | 15
PollSchedule | When polling should take place, represented as a cron string | *Future functionality*
MaxConcurrentReports | The maximum number of reports that can be run *connection* concurrently | 3
GlobalizationCultureNumeric | Culture used for the formatting of numeric values in the output files.  If left empty, this defaults to CultureInfo.InvariantCulture | en-ZA
GlobalizationCultureDateTime | Culture used for the formatting of date/time values in the output files.  If left empty, this defaults to CultureInfo.InvariantCulture | en-ZA
ExcelDefaultDateTimeFormat | The EPPlus component used for Excel output requires a default date/time format, otherwise dates display as numbers - value can be any [custom .NET format](https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings) | yyyy-MM-dd HH:mm:ss

## Program Flow

Assuming the application is correctly installed and started as a Windows service:

1. Application loads up all connection strings, and for each connection string:
2. ConcurrencyService determines how many reports are running for the current server/host 
3. If maximum allowed limit reached, sleep and return to step 1
4. Queries "ReportQueue" table (see config items) and fetches *N* reports that:
    1. have not been processed
    2. are not currently running per the ConcurrencyManager
    3. have not experienced an error
    4. have a ScheduleDate of NULL or less than or equal to the current UTC date/time
	where *N* is the maximum number of allowed reports minus the number of reports already running as per the ConcurrencyManager
5. Each unprocessed report from the ReportQueue is run on a background thread as follows:
    1. Report is added to the ConcurrencyManager
    2. ReportType is determined (StoredProcedure, SQL, *Future: SSRS*)
    3. Command is analysed depending on ReportType
    4. Parameters are loaded (applicable to all report types, even SQL as sp_executesql is used to run the text)
    5. StartTime is updated on ReportQueue to the current UTC date/time, and Status is set to "Processing"
    6. Report is executed
    7. Once data returns from the database server, the report is saved to disk in the format specified in the "ReportFormat" column
    8. On report completion:
        1. If report runs successfully, EndTime is updated on ReportQueue to the current UTC date/time, and Status is set to Complete
        2. If report fails, Status is set to Error and ErrorDetails column is updated with error information and the associated stack trace
    9. Report is removed from ConcurrencyManager as a processing report
6. Application sleeps for configured time period before polling again (goto Step 1)

## Command Types

The following source commands can be used:

1. Raw SQL statements (SQL)
2. Stored procedures (StoredProcedure)

## Output Formatting

SqlServerReportRunner supports the following file output formats:

1. CSV - Comma Separated Values
2. Delimited - Text-delimited file, where any delimiter can be specified
3. Excel - OpenDocument .xlsx format

## Data Formatting

The program will export data into these files using the locale of the machine on which it is running, or the locale specified in the GlobalizationCulture configuration setting.  However, the file format must also be taken into account.

1. CSV/Text-delimited - all values will be output using the current locale (or the GlobalizationCulture configuration setting if this has a value)
2. Excel - Excel itself will use the current locale to display all values, as data values are passed through to Excel using the same data types returned by from the database.  The only exception to this is dates.  If you are using Excel as an output option, then you must specify your default date/time format in the application configuration setting "ExcelDefaultDateTimeFormat".  This is because the underlying component requires a format to be specified - if this is not done you may end up with numbers displayed in date/time cells.

If you want to completely override the output values you can always format the data in the source query.  Just be careful if you convert values to string data types (e.g. varchar) if you are using Excel, as they will be written to Excel as text (as opposed to a number or a date).

### Some examples:

Dates in YYYY-MM-DD format, converted to string: 

```sql
SELECT CONVERT(char(10), GETDATE(), 126) AS MyDate
```

Dates in YYYY-MM-DD HH:mm:ss format, converted to string:

```sql
SELECT CONVERT(CHAR(19), CONVERT(DATETIME, GETDATE(), 101), 120) AS MyDate
```

Numbers in 0.00 format:

```sql
SELECT FORMAT(MyMoneyField, '0.00') AS MyNumber
```

Round numbers to 2 decimal places:

```sql
SELECT ROUND(MyMoneyField, 2) AS MyNumber
```

## Running Reports

In order to run a report against a database that has been configured for the SqlServerReportRunner, you need to insert a record into the 

```sql
DECLARE @xml XML 
SELECT @xml = (
	select 1 AS Param1
	, '29' AS Param2 
	, '2017-02-01' AS DateParam 
	for XML PATH, type)

DECLARE @fileName varchar(255) = 'MyFileName.txt'
INSERT INTO ReportJobQueue
	(
	ReportName
	, CommandType
	, Command
	, [Parameters]
	, OutputFileName
	, OutputFilePath
	, OutputFormat
	, Delimiter
	, UserName
	, EmailAddress
	, [Status]
	, CreateDate
	, ScheduleDate
	)
VALUES
	(
	'Test Report'					-- the name of the report - for your usage
	, 'StoredProcedure'				-- SQL or StoredProcedure
	, 'dbo.MyStoredProcedure'			-- the actual command (will be a SQL query or the name of a stored procedure)
	, @xml						-- the actual parameters as an XML data type
	, @fileName					-- the name of the output file
	, '\\' + @@SERVERNAME + '\Test\Reporting'	-- the location of the output folder (UNC)
	, 'Delimited'					-- CSV, Delimited or Excel
	, '|'						-- only used in the case of Delimited files, can be an string value
	, 'matt'					-- the name of the user (optional)
	, 'matt@test.com'				-- the email address of the user (optional)
	, 'Pending'					-- status of the report
	, GETUTCDATE()					-- date the report is created (must be UTC)
	, DATEADD(minute, 2, GETUTCDATE())	-- when you want the report to run (leave as NULL for immediate execution) (must be UTC)
	)
```
## Installation

1. Copy all binary files into a folder on the server that will be processing the reports (not necessarily the database server)
2. Open the "SqlServerReportRunner.exe.config file, and:
    1. Under the connectionStrings section, add connection strings for each server/database pair that will host a ReportQueue table
    2. Set application configuration options in the appSettings section (See Configuration Options section below).  Save and close the file.
3. Run the SQL Script "scripts/ReportJobQueueTable.sql" on any database that will host a ReportQueue table
4. (Optional) Configure NLog.config file to add logging.  By default, the application will log to a text file in the application folder, but the service can be configured to (for example) log errors via email if you have access to a mail server
5. Open a console in Administrator mode and change the current directory to your installation folder
6. Run "SqlServerReportRunner.exe install"
7. To start the service, run "SqlServerReportRunner.exe start", or go to the Services control panel and start the service

