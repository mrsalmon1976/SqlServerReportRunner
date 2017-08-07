SET QUOTED_IDENTIFIER ON
GO

-- DROP TABLE [dbo].[ReportJobQueue]
CREATE TABLE [dbo].[ReportJobQueue](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ReportName] [varchar](255) NOT NULL,
	[CommandType] [varchar](255) NOT NULL,		-- StoredProcedure, SQL, SSRS
	[Command] [varchar](255) NOT NULL,			-- Stored Proc name, SQL Text, SSRS report name
	[Parameters] [xml] NULL,
	[OutputFileName] [varchar](255) NOT NULL,
	[OutputFilePath] [varchar](255) NOT NULL,
	[OutputFormat] [varchar](50) NOT NULL,
	[Delimiter] [varchar](20) NOT NULL,
	[UserName] [varchar](100) NULL,
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

GO

