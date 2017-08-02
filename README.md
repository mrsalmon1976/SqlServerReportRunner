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
ServiceUserName | Windows account under which the service should run as when installed.  Should be represented as DOMAIN\UserName.  Leaving this blank will result in the service running as Local System. | MYDOMAIN\Eric
ServicePassword | Windows password for the configured account.  Leave blank if ServiceUserName is blank. | mypassword
PollInterval | How often the database table is polled to see if there are any new reports to run (in seconds) | 15
PollSchedule | When polling should take place, represented as a cron string | *Future functionality*
MaxConcurrentReports | The maximum number of reports that can be run *per host* concurrently | 3

## Program Flow

Assuming the application is correctly installed and started as a Windows service:

1. Application loads up all connection strings, and for each connection string:
2. ConcurrencyService determines how many reports are running for the current server/host 
3. If maximum allowed limit reached, sleep and return to step 1
4. Queries "ReportQueue" table (see config items) and fetches *N* reports that have not been processed, are not currently running per the ConcurrencyManager, and have not experienced an error. *N* is the maximum number of allowed reports minus the number of reports already running as per the ConcurrencyManager
5. Each unprocessed report from the ReportQueue is run on a background thread as follows:
    1. Report is added to the ConcurrencyManager
    2. ReportType is determined (StoredProcedure, *Future: SSRS, SQL*)
    3. Command is analysed depending on ReportType
    4. Parameters are loaded (applicable to all report types, even SQL as sp_executesql is used to run the text)
    5. StartTime is updated on ReportQueue to the current UTC date/time, and Status is set to "Processing"
    6. Report is executed
        1. If report runs successfully, EndTime is updated on ReportQueue to the current UTC date/time, and Status is set to Processed
        2. If report fails, Status is set to Error and ErrorDetails column is updated with error information and the associated stack trace
    7. Report is removed from ConcurrencyManager as a processing report
6. Application sleeps for configured time period before polling again (goto Step 1)

