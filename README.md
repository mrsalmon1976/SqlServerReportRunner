# SqlServerReportRunner

Windows service that can be used to run reports from SQL Server and persist the files to disk.  Reports are run from a queue and saved to disk in a variety of formats.  This can be useful in preventing too many reports running simultaneously, and save on network traffic in the case of SSRS reports.

**NOTE: This project is currently being developed - technical notes below are what will be, not what there is now**

## Installation

1. Copy all binary files into a folder on the server that will be processing the reports (not necessarily the database server)
2. Open the "SqlServerReportRunner.exe.config file, and:
    1. Under the connectionStrings section, add connection strings for each server/database pair that will host a ReportQueue table
    2. Set application configuration options in the appSettings section (See Configuration Options section below).  Save and close the file.
3. Run the SQL Script "sql/CreateReportQueueTable.sql" on any database that will host a ReportQueue table
4. (Optional) Configure NLog.config file to add logging.  By default, the application will log to a text file in the application folder, but the service can be configured to (for example) log errors via email if you have access to a mail server
5. Open a console in Administrator mode and change the current directory to your installation folder
6. Run "SqlServerReportRunner.exe install"
7. To start the service, run "SqlServerReportRunner.exe start", or go to the Services control panel and start the service

## Configuration Options

Key | Description | Example
------------- | ------------- | -------------
ServiceUserName | Windows account under which the service should run as when installed.  Should be represented as DOMAIN\UserName.  Leaving this blank will result in the service running as Local System. | MYDOMAIN\matt
ServicePassword | Windows password for the configured account.  Leave blank if ServiceUserName is blank. | mypassword
PollInterval | How often the database table is polled to see if there are any new reports to run (in seconds) | 15
PollSchedule | When polling should take place, represented as a cron string | *Future functionality*
MaxConcurrentReports | The maximum number of reports that can be run *connection* concurrently | 3

## Program Flow

Assuming the application is correctly installed and started as a Windows service:

1. Application loads up all connection strings, and for each connection string:
2. ConcurrencyService determines how many reports are running for the current server/host 
3. If maximum allowed limit reached, sleep and return to step 1
4. Queries "ReportQueue" table (see config items) and fetches *N* reports that have not been processed, are not currently running per the ConcurrencyManager, and have not experienced an error. *N* is the maximum number of allowed reports minus the number of reports already running as per the ConcurrencyManager
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

## Output Formatting

The formatting of data in the output files is not the responsibility of the program - all it does is convert data values to strings and write them to the output file.  In the case of a delimited text output file, 
carriage return and line feed characters are removed before writing the line.  If you are wanting dates or numbers formatted in a particular way, you will need to do that at the source query level.

### Some examples:

Dates in YYYY-MM-DD format: 

```sql
SELECT CONVERT(char(10), GETDATE(), 126) AS MyDate
```
Dates in YYYY-MM-DD HH:mm:ss format:

Numbers in 0.00 format:

```sql
SELECT CONVERT(CHAR(19), CONVERT(DATETIME, GETDATE(), 101), 120) AS MyDate
```

```sql
SELECT FORMAT(MyMoneyField, '0.00') AS MyNumber
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
	, [Status]
	, CreateDate
	)
VALUES
	(
	'Test Report'
	, 'StoredProcedure'
	, 'dbo.MyStoredProcedure'
	, @xml
	, @fileName
	, '\\' + @@SERVERNAME + '\Test\Reporting'
	, 'Delimited'
	, '|'
	, 'matt'
	, 'Pending'
	, GETUTCDATE()
	)
```
