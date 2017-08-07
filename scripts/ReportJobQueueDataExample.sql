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