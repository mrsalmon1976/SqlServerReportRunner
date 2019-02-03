SET QUOTED_IDENTIFIER ON
GO

IF (NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'ReportJobQueue'))
BEGIN
	CREATE TABLE [dbo].[ReportJobQueue](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[ReportName] [varchar](255) NOT NULL,
		[CommandType] [varchar](255) NOT NULL,		-- StoredProcedure, SQL, SSRS
		[Command] [varchar](255) NOT NULL,			-- Stored Proc name, SQL Text, SSRS report name
		[Parameters] [xml] NULL,
		[OutputFileName] [varchar](255) NULL,
		[OutputFilePath] [varchar](255) NULL,
		[OutputFormat] [varchar](50) NULL,
		[Delimiter] [varchar](20) NULL,
		[UserName] [varchar](100) NULL,
		[EmailAddress] [varchar](255) NULL,
		[Status] [varchar](100) NOT NULL,
		[CreateDate] [datetime] NOT NULL,
		[ProcessStartDate] [datetime] NULL,
		[ProcessEndDate] [datetime] NULL,
		[ErrorMessage] [varchar](MAX) NULL,
		[ErrorStackTrace] [varchar](MAX) NULL
	 CONSTRAINT [PK_ReportJobQueue] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

END
GO

-- ScheduleDateTime column
IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'ScheduleDate'
          AND Object_ID = Object_ID(N'dbo.ReportJobQueue'))
BEGIN
	ALTER TABLE [dbo].[ReportJobQueue] ADD ScheduleDate [datetime] NULL
END

-- add default constraint on CreateDate
IF OBJECT_ID('dbo.DF_ReportJobQueue_CreateDate', 'D') IS NULL
BEGIN
	ALTER TABLE [dbo].[ReportJobQueue] ADD CONSTRAINT [DF_ReportJobQueue_CreateDate] DEFAULT (GETUTCDATE()) FOR [CreateDate]
END

-- bump up the size of the Command column, allowing for URLs for SSRS support
ALTER TABLE dbo.ReportJobQueue ALTER COLUMN Command varchar(4096) NOT NULL




