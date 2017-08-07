-- select * from ReportJobQueue where status = 'Pending'
-- truncate table ReportJobQueue

--		declare @ClientId int = 1
--declare @xml xml 
--select @xml = (
--	select @ClientId AS ClientId
--	, '29' AS ProdEntityID 
--	, '177,180,181' AS ProdCellCaptiveID 
--	, '' AS WesbankEntityID 
--	, '' AS WesbankCellCaptiveID 
--	, '2017-02-01' AS SDate 
--	, '2017-02-28' AS EDate
--	, 'matt' AS IUAUserName 
--	for XML PATH, type)

declare @xml xml 
select @xml = (
	select 1 AS DBSourceID
	, '29' AS EntityID 
	, '2017-02-01' AS startDate 
	, '2017-02-28' AS endDate
	, 'matt' AS IUAUserName 
	for XML PATH, type)
--select @xml AS [Parameters]

--declare @fileName varchar(255) = 'SalesReport_Client_' + cast(@ClientId as varchar(100)) + '.csv'
declare @fileName varchar(255) = 'CPS_' + REPLACE(CAST(RAND(100) AS varchar(10)), '.', '') + '.txt'
insert into ReportJobQueue
	(ReportName, CommandType, Command, [Parameters], OutputFileName, OutputFilePath, OutputFormat, Delimiter, UserName, Status, CreateDate)
values
	('CPS Banking Report', 'StoredProcedure', 'ssrs.[CPS_Banking_Report_MattTest]', @xml, @fileName, '\\' + @@SERVERNAME + '\Test\Reporting', 'Delimited', '|', 'matt', 'Pending', GETUTCDATE())

