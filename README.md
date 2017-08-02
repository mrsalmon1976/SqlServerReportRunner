# SqlServerReportRunner
Windows service that can be used to run reports from SQL Server and persist the files to disk.  Reports are run from a queue and saved to disk in a variety of formats.  This can be useful in preventing too many reports running simultaneously, and save on network traffic in the case of SSRS reports.
