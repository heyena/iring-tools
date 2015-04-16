USE [iRINGConfig]
GO
/****** Object:  User [iRINGConfig]    Script Date: 04/16/2015 17:45:22 ******/
CREATE USER [iRINGConfig] FOR LOGIN [iRINGConfig] WITH DEFAULT_SCHEMA=[dbo]
GO
/****** Object:  Table [dbo].[Applications]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Applications](
	[ContextId] [uniqueidentifier] NOT NULL,
	[ApplicationId] [uniqueidentifier] NOT NULL,
	[DisplayName] [nvarchar](255) NOT NULL,
	[InternalName] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](255) NOT NULL,
	[DXFRUrl] [nvarchar](255) NOT NULL,
	[Active] [tinyint] NOT NULL,
	[Assembly] [nvarchar](550) NULL,
	[DataMode] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK__Applicat__C93A4C994AB81AF0] PRIMARY KEY CLUSTERED 
(
	[ApplicationId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AliasDictionary]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AliasDictionary](
	[Sno] [int] IDENTITY(1,1) NOT NULL,
	[ResourceId] [uniqueidentifier] NULL,
	[Key] [nvarchar](100) NULL,
	[Value] [nvarchar](200) NULL,
 CONSTRAINT [PK_AliasDictionary] PRIMARY KEY CLUSTERED 
(
	[Sno] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DataFiltersType]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DataFiltersType](
	[DataFilterTypeId] [int] NOT NULL,
	[DataFilterTypeName] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[DataFilterTypeId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DataFilters]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DataFilters](
	[DataFilterId] [uniqueidentifier] NOT NULL,
	[ResourceId] [uniqueidentifier] NOT NULL,
	[DataFilterTypeId] [int] NOT NULL,
	[Active] [tinyint] NOT NULL,
	[IsAdmin] [bit] NULL,
 CONSTRAINT [PK_DataFilters] PRIMARY KEY CLUSTERED 
(
	[DataFilterId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ExtensionProperty]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExtensionProperty](
	[ExtensionPropertyId] [uniqueidentifier] NOT NULL,
	[DataObjectId] [uniqueidentifier] NOT NULL,
	[ColumnName] [nvarchar](250) NULL,
	[PropertyName] [nvarchar](250) NULL,
	[DataType] [nvarchar](50) NULL,
	[DataLength] [int] NULL,
	[IsNullable] [bit] NULL,
	[NumberOfDecimals] [int] NULL,
	[KeyType] [nvarchar](50) NULL,
	[Precision] [int] NULL,
	[Scale] [int] NULL,
	[Definition] [nvarchar](1000) NULL,
	[ShowOnIndex] [bit] NULL,
 CONSTRAINT [PK_ExtensionProperty] PRIMARY KEY CLUSTERED 
(
	[ExtensionPropertyId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ExpressionValues]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExpressionValues](
	[Sno] [int] IDENTITY(1,1) NOT NULL,
	[ExpressionId] [uniqueidentifier] NULL,
	[Value] [nvarchar](200) NULL,
 CONSTRAINT [PK_DataFilterValues] PRIMARY KEY CLUSTERED 
(
	[Sno] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Expression]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Expression](
	[DataFilterId] [uniqueidentifier] NULL,
	[ExpressionId] [uniqueidentifier] NOT NULL,
	[OpenGroupCount] [int] NULL,
	[PropertyName] [nvarchar](350) NULL,
	[RelationalOperator] [nvarchar](150) NULL,
	[LogicalOperator] [nvarchar](50) NULL,
	[CloseGroupCount] [int] NULL,
	[IsCaseSensitive] [bit] NULL,
 CONSTRAINT [PK_Expressions] PRIMARY KEY CLUSTERED 
(
	[ExpressionId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Groups]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Groups](
	[GroupId] [int] IDENTITY(1,1) NOT NULL,
	[GroupName] [nvarchar](100) NOT NULL,
	[GroupDesc] [nvarchar](255) NULL,
	[Active] [tinyint] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[GroupId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Permissions]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Permissions](
	[PermissionId] [int] IDENTITY(1,1) NOT NULL,
	[PermissionName] [nvarchar](100) NOT NULL,
	[PermissionDesc] [nvarchar](100) NULL,
	[Active] [tinyint] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[PermissionId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderExpression]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderExpression](
	[DataFilterId] [uniqueidentifier] NULL,
	[OrderExpressionId] [uniqueidentifier] NOT NULL,
	[PropertyName] [nvarchar](350) NULL,
	[SortOrder] [nvarchar](25) NULL,
 CONSTRAINT [PK_OrderExpressions] PRIMARY KEY CLUSTERED 
(
	[OrderExpressionId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MimeType]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MimeType](
	[FileId] [int] IDENTITY(1,1) NOT NULL,
	[FileType] [nvarchar](10) NOT NULL,
	[Description] [nvarchar](100) NULL,
	[Active] [int] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ExchangeHistoryInformation]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExchangeHistoryInformation](
	[ExchangeId] [uniqueidentifier] NOT NULL,
	[Level] [nvarchar](50) NOT NULL,
	[DataTimeStamp] [datetime] NOT NULL,
	[StatusLevel] [nvarchar](50) NOT NULL,
	[identifier] [nvarchar](50) NOT NULL,
	[results] [uniqueidentifier] NOT NULL,
	[Messages] [uniqueidentifier] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ExchangeHistory]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ExchangeHistory](
	[ExchangeId] [uniqueidentifier] NOT NULL,
	[Level] [varchar](50) NOT NULL,
	[StartTime] [datetime] NOT NULL,
	[EndTime] [datetime] NOT NULL,
	[SenderGraphId] [uniqueidentifier] NOT NULL,
	[ReceiverGraphId] [uniqueidentifier] NOT NULL,
	[ItemCount] [int] NULL,
	[itemCountSync] [int] NULL,
	[ItemCountAdd] [int] NULL,
	[ItemCountChange] [int] NULL,
	[itemCountDelete] [int] NULL,
	[PoolSize] [int] NOT NULL,
	[Summary] [varchar](450) NULL,
	[UserName] [nvarchar](50) NULL
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[RoleNames]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RoleNames](
	[RoleName] [nvarchar](100) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ResourceType]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ResourceType](
	[ResourceTypeId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[ResourceTypeId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Sites]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Sites](
	[SiteId] [int] IDENTITY(1,1) NOT NULL,
	[SiteName] [nvarchar](10) NOT NULL,
	[SiteDesc] [nchar](255) NULL,
	[Active] [tinyint] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[SiteId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Settings]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Settings](
	[SettingId] [int] NOT NULL,
	[Name] [nvarchar](250) NOT NULL,
 CONSTRAINT [PK_Settings] PRIMARY KEY CLUSTERED 
(
	[SettingId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ScheduleExchange_Expired]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ScheduleExchange_Expired](
	[Schedule_Exchange_Id] [uniqueidentifier] NOT NULL,
	[Task_Name] [nvarchar](100) NOT NULL,
	[Scope] [nvarchar](64) NULL,
	[Base_Url] [nvarchar](256) NULL,
	[Exchange_Id] [nvarchar](64) NULL,
	[Sso_Url] [nvarchar](256) NULL,
	[Client_Id] [nvarchar](64) NULL,
	[Client_Secret] [nvarchar](64) NULL,
	[Grant_Type] [nvarchar](64) NULL,
	[Request_Timeout] [int] NULL,
	[Start_Time] [datetime] NOT NULL,
	[End_Time] [datetime] NULL,
	[Created_Date] [datetime] NOT NULL,
	[Created_By] [nvarchar](100) NOT NULL,
	[Occurance] [nvarchar](24) NOT NULL,
	[NextStart_Date_Time] [datetime] NULL,
	[End_Date_Time] [datetime] NOT NULL,
	[Status] [nvarchar](64) NOT NULL,
	[Active] [tinyint] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Schedule_Exchange_Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ScheduleCache_Expired]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ScheduleCache_Expired](
	[Schedule_Cache_Id] [uniqueidentifier] NOT NULL,
	[Task_Name] [nvarchar](100) NOT NULL,
	[Project] [nvarchar](64) NOT NULL,
	[App] [nvarchar](64) NOT NULL,
	[Cache_Page_Size] [int] NULL,
	[Sso_Url] [nvarchar](256) NULL,
	[Client_Id] [nvarchar](64) NULL,
	[Client_Secret] [nvarchar](64) NULL,
	[Grant_Type] [nvarchar](64) NULL,
	[App_Key] [nvarchar](64) NULL,
	[Access_Token] [nvarchar](64) NULL,
	[Request_Timeout] [int] NULL,
	[Start_Time] [datetime] NOT NULL,
	[End_Time] [datetime] NULL,
	[Created_Date] [datetime] NOT NULL,
	[Created_By] [nvarchar](100) NOT NULL,
	[Occurance] [nvarchar](24) NOT NULL,
	[NextStart_Date_Time] [datetime] NULL,
	[End_Date_Time] [datetime] NOT NULL,
	[Status] [nvarchar](64) NOT NULL,
	[Active] [tinyint] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Schedule_Cache_Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Schedule_Expired]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Schedule_Expired](
	[Schedule_Id] [uniqueidentifier] NOT NULL,
	[Created_DateTime] [datetime] NOT NULL,
	[Created_By] [nvarchar](250) NOT NULL,
	[Occurance] [nvarchar](50) NULL,
	[Weekday] [nvarchar](50) NULL,
	[Start_DateTime] [datetime] NULL,
	[End_DateTime] [datetime] NULL,
	[Status] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Schedule] PRIMARY KEY CLUSTERED 
(
	[Schedule_Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Schedule]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Schedule](
	[ScheduleId] [uniqueidentifier] NOT NULL,
	[Created_DateTime] [datetime] NOT NULL,
	[Created_By] [nvarchar](250) NOT NULL,
	[Occurance] [nvarchar](50) NULL,
	[Weekday] [nvarchar](50) NULL,
	[Start_DateTime] [datetime] NULL,
	[End_DateTime] [datetime] NULL,
	[Status] [nvarchar](50) NOT NULL,
 CONSTRAINT [PKSchedule] PRIMARY KEY CLUSTERED 
(
	[ScheduleId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RollupExpression]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RollupExpression](
	[DataFilterId] [uniqueidentifier] NULL,
	[RollupExpressionId] [uniqueidentifier] NOT NULL,
	[GroupBy] [nvarchar](100) NULL,
 CONSTRAINT [PK_RollupExpressions] PRIMARY KEY CLUSTERED 
(
	[RollupExpressionId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Rollup]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Rollup](
	[RollupExpressionId] [uniqueidentifier] NULL,
	[RollupId] [uniqueidentifier] NOT NULL,
	[PropertyName] [nvarchar](350) NULL,
	[RollupType] [nvarchar](50) NULL,
 CONSTRAINT [PK_Rollup] PRIMARY KEY CLUSTERED 
(
	[RollupId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Roles]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Roles](
	[RoleId] [int] IDENTITY(1,1) NOT NULL,
	[RoleName] [nvarchar](100) NOT NULL,
	[RoleDesc] [nvarchar](100) NULL,
	[Active] [tinyint] NULL,
 CONSTRAINT [PK__Roles__8AFACE1A1273C1CD] PRIMARY KEY CLUSTERED 
(
	[RoleId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[spuJob_client_Info]    Script Date: 04/16/2015 17:45:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spuJob_client_Info] 
(
	@Job_Id uniqueidentifier,
	@SSo_Url NVARCHAR(250),
	@Client_id NVARCHAR(250),
	@Client_Secret NVARCHAR(250),
	@Grant_Type NVARCHAR(50),
	@Request_Timeout int
)	 
AS
BEGIN TRY
	BEGIN
	
	UPDATE [Job_Client_Info]
	   SET 
		  [Job_Id] = @Job_Id,
		  [SSo_Url] = @SSo_Url,
		  [Client_id] = @Client_id,
		  [Client_Secret] = @Client_Secret,
		  [Grant_Type] = @Grant_Type,
		  [Request_Timeout] = @Request_Timeout
	 WHERE Job_Id = @Job_Id 

	  Select  '1'--'Job Client Info updated successfully!'
	END
END TRY
BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spuScheduleExchange]    Script Date: 04/16/2015 17:45:17 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
--* ==================================================
--*  Procedure ScheduleExchange Update
--* ==================================================
CREATE PROCEDURE [dbo].[spuScheduleExchange]
	@SCHEDULE_EXCHANGE_ID	nvarchar(64),
	@TASK_NAME				nvarchar(100),
	@SCOPE					nvarchar(64),
	@SSO_URL				nvarchar(256),
	@CLIENT_ID				nvarchar(64),
	@CLIENT_SECRET			nvarchar(64), 
	@GRANT_TYPE				nvarchar(64),
	@REQUEST_TIMEOUT		int,
	@START_TIME				DateTime,
	@END_TIME				DateTime,
	@CREATED_DATE			DateTime,
	@CREATED_BY				nvarchar(100),
	@OCCURANCE				nvarchar(24),
	@NEXTSTART_DATE_TIME 	DateTime,
	@END_DATE_TIME			DateTime,
	@STATUS					nvarchar(64),
	@ACTIVE					tinyint
AS
UPDATE [dbo].[ScheduleExchange] SET
	[TASK_NAME] = @TASK_NAME,
	[SCOPE] = @SCOPE,
	[SSO_URL] = @SSO_URL,
	[CLIENT_ID] = @CLIENT_ID,
	[CLIENT_SECRET] = @CLIENT_SECRET,
	[GRANT_TYPE] = @GRANT_TYPE,
	[REQUEST_TIMEOUT] = @REQUEST_TIMEOUT,
	[START_TIME] = @START_TIME,
	[END_TIME] = @END_TIME,
	[CREATED_DATE] = @CREATED_DATE,
	[CREATED_BY] = @CREATED_BY,
	[OCCURANCE] = @OCCURANCE,
	[NEXTSTART_DATE_TIME] = @NEXTSTART_DATE_TIME,
	[END_DATE_TIME] = @END_DATE_TIME,
	[STATUS] = @STATUS,
	[ACTIVE] = @ACTIVE
WHERE [dbo].[ScheduleExchange].[SCHEDULE_EXCHANGE_ID] = @SCHEDULE_EXCHANGE_ID
GO
/****** Object:  StoredProcedure [dbo].[spuScheduleCache]    Script Date: 04/16/2015 17:45:16 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
--* ==================================================
--*  Procedure ScheduleCache Update
--* ==================================================
CREATE PROCEDURE [dbo].[spuScheduleCache]
	@SCHEDULE_CACHE_ID	nvarchar(64),
	@TASK_NAME			nvarchar(100),
	@PROJECT			nvarchar(64),
	@APP				nvarchar(64), 
	@CACHE_PAGE_SIZE	int,
	@SSO_URL			nvarchar(256),
	@CLIENT_ID			nvarchar(64),
	@CLIENT_SECRET		nvarchar(64), 
	@GRANT_TYPE			nvarchar(64),
	@APP_KEY			nvarchar(64),
	@ACCESS_TOKEN		nvarchar(64),
	@REQUEST_TIMEOUT	int,
	@START_TIME			DateTime,
	@END_TIME			DateTime,
	@CREATED_DATE		DateTime,
	@CREATED_BY			nvarchar(100),
	@OCCURANCE			nvarchar(24),
	@NEXTSTART_DATE_TIME DateTime,
	@END_DATE_TIME		DateTime,
	@STATUS				nvarchar(64),
	@ACTIVE				tinyint
AS
UPDATE [dbo].[ScheduleCache] SET
	[TASK_NAME] = @TASK_NAME,
	[PROJECT] = @PROJECT,
	[APP] = @APP,
	[CACHE_PAGE_SIZE] = @CACHE_PAGE_SIZE,
	[SSO_URL] = @SSO_URL,
	[CLIENT_ID] = @CLIENT_ID,
	[CLIENT_SECRET] = @CLIENT_SECRET,
	[GRANT_TYPE] = @GRANT_TYPE,
	[APP_KEY] = @APP_KEY,
	[ACCESS_TOKEN] = @ACCESS_TOKEN,
	[REQUEST_TIMEOUT] = @REQUEST_TIMEOUT,
	[START_TIME] = @START_TIME,
	[END_TIME] = @END_TIME,
	[CREATED_DATE] = @CREATED_DATE,
	[CREATED_BY] = @CREATED_BY,
	[OCCURANCE] = @OCCURANCE,
	[NEXTSTART_DATE_TIME] = @NEXTSTART_DATE_TIME,
	[END_DATE_TIME] = @END_DATE_TIME,
	[STATUS] = @STATUS,
	[ACTIVE] = @ACTIVE
WHERE [dbo].[ScheduleCache].[Schedule_Cache_Id] = @SCHEDULE_CACHE_ID
GO
/****** Object:  Table [dbo].[Users]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[UserId] [int] IDENTITY(1,1) NOT NULL,
	[UserName] [nvarchar](100) NOT NULL,
	[UserFirstName] [nvarchar](50) NULL,
	[UserLastName] [nvarchar](50) NULL,
	[UserEmail] [nvarchar](50) NULL,
	[UserPhone] [nvarchar](50) NULL,
	[UserDesc] [nvarchar](255) NULL,
	[Active] [tinyint] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Platforms]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Platforms](
	[PlatformId] [int] IDENTITY(1,1) NOT NULL,
	[PlatformName] [nvarchar](10) NOT NULL,
	[PlatformDesc] [nchar](255) NULL,
	[Active] [tinyint] NOT NULL,
 CONSTRAINT [PK_Platforms] PRIMARY KEY CLUSTERED 
(
	[PlatformId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[spuJobSchedule]    Script Date: 04/16/2015 17:45:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spuJobSchedule] 
(
	@Schedule_Id uniqueidentifier,
	@Job_Id uniqueidentifier,
	@PlatformId int,
	@SiteId int,
	@Next_Start_DateTime NVARCHAR(100),
	@Last_Start_DateTime NVARCHAR(100),
	@TotalRecords int,
	@CachedRecords int,
	@Active int
)	 
AS
BEGIN TRY
	BEGIN
	
	UPDATE [JobSchedule]
	   SET 
		  [Schedule_Id] = @Schedule_Id,
		  [Job_Id ] = @Job_Id ,
		  [PlatformId] = @PlatformId,
		  [SiteId] = @SiteId,
		  [Next_Start_DateTime] = convert(datetime, @Next_Start_DateTime, 121),
		  [Last_Start_DateTime] = convert(datetime, @Last_Start_DateTime, 121),
		  [TotalRecords] = @TotalRecords,
		  [CachedRecords] = @CachedRecords,
		  [Active] = @Active
	 WHERE Schedule_Id = @Schedule_Id and Job_Id = @Job_Id

	  Select  '1'--'JobSchedule updated successfully!'
	END
END TRY
BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO
/****** Object:  Table [dbo].[JobClientInfo_Expired]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[JobClientInfo_Expired](
	[SSo_Url] [nvarchar](250) NOT NULL,
	[Client_id] [nvarchar](250) NOT NULL,
	[Client_Secret] [nvarchar](250) NOT NULL,
	[Grant_Type] [nvarchar](50) NULL,
	[Request_Timeout] [int] NULL,
	[exchange_url] [nvarchar](2000) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Job_Expired]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Job_Expired](
	[Job_id] [uniqueidentifier] NOT NULL,
	[Is_Exchange] [tinyint] NOT NULL,
	[Scope] [nvarchar](50) NULL,
	[App] [nvarchar](50) NULL,
	[DataObject] [nvarchar](50) NULL,
	[Xid] [nvarchar](50) NULL,
	[Exchange_Url] [nvarchar](250) NULL,
	[Cache_Page_size] [nvarchar](50) NULL,
 CONSTRAINT [PK_Job] PRIMARY KEY CLUSTERED 
(
	[Job_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[spdJobSchedule]    Script Date: 04/16/2015 17:44:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE  PROCEDURE [dbo].[spdJobSchedule] 
(
	@JobId uniqueidentifier,
	@ScheduleId uniqueidentifier
)
AS
BEGIN TRY
	BEGIN
	  -- delete
	  DELETE FROM JobSchedule where Schedule_Id = @ScheduleId and Job_Id = @JobId
	  
	  Select  '1'--'JobSchedule Deleted successfully!'
	END
		
END TRY
BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spdJobClientInfo]    Script Date: 04/16/2015 17:44:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE  PROCEDURE [dbo].[spdJobClientInfo] 
(
	@JobId uniqueidentifier
)
AS
BEGIN TRY
	BEGIN
	  -- delete
	  DELETE FROM Job_client_info where Job_Id = @JobId
	  Select  '1'--'Job Client Info Deleted successfully!'
	END
		
END TRY
BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spdScheduleExchange]    Script Date: 04/16/2015 17:44:23 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
--* ==================================================
--*  Procedure ScheduleExchange Delete
--* ==================================================
CREATE PROCEDURE [dbo].[spdScheduleExchange]
	@SCHEDULE_EXCHANGE_ID	[varchar](126)
AS

UPDATE [dbo].[ScheduleExchange] 
SET 
	[ACTIVE] = 0
WHERE [dbo].[ScheduleExchange].[SCHEDULE_EXCHANGE_ID] = @SCHEDULE_EXCHANGE_ID
GO
/****** Object:  StoredProcedure [dbo].[spdScheduleCache]    Script Date: 04/16/2015 17:44:23 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
--* ==================================================
--*  Procedure ScheduleCache Delete
--* ==================================================
CREATE PROCEDURE [dbo].[spdScheduleCache]
	@SCHEDULE_CACHE_ID	[varchar](126)
AS

UPDATE [dbo].[ScheduleCache] 
SET 
	[ACTIVE] = 0
WHERE [SCHEDULE_CACHE_ID] = @SCHEDULE_CACHE_ID
GO
/****** Object:  StoredProcedure [dbo].[spgScheduleExchange]    Script Date: 04/16/2015 17:44:47 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [dbo].[spgScheduleExchange]
	@Schedule_Exchange_Id	[varchar](126)
AS

SELECT
	[Schedule_Exchange_Id] AS [Schedule_Exchange_Id],
	[Task_Name] AS [Task_Name],
	[SCOPE] AS [SCOPE],
	[Base_Url] AS [Base_Url],
	[Exchange_Id] AS [Exchange_Id],
	[SSO_URL] AS [SSO_URL],
	[CLIENT_ID] AS [CLIENT_ID],
	[CLIENT_SECRET] AS [CLIENT_SECRET],
	[GRANT_TYPE] AS [GRANT_TYPE],
	[REQUEST_TIMEOUT] AS [REQUEST_TIMEOUT],
	[Start_Time] AS [Start_Time],
	[End_Time] AS [End_Time],
	[Created_Date] AS [Created_Date] ,
	[Created_By] AS [Created_By],
	[Occurance] AS [Occurance],
	[NextStart_Date_Time] AS [NextStart_Date_Time],
	[End_Date_Time] AS [End_Date_Time],
	[STATUS] AS [STATUS],
	[ACTIVE] AS [ACTIVE]
FROM [dbo].[ScheduleExchange] WITH (NoLock)
WHERE
	[dbo].[ScheduleExchange].[Schedule_Exchange_Id] = @Schedule_Exchange_Id
GO
/****** Object:  StoredProcedure [dbo].[spgScheduleCache]    Script Date: 04/16/2015 17:44:46 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [dbo].[spgScheduleCache]
	@Schedule_Cache_Id	[varchar](126)
AS

SELECT
	[Schedule_Cache_Id] AS [Schedule_Cache_Id],
	[Task_Name] AS [Task_Name],
	[PROJECT] AS [PROJECT],
	[APP] AS [APP],
	[CACHE_PAGE_SIZE] AS [CACHE_PAGE_SIZE],
	[SSO_URL] AS [SSO_URL],
	[CLIENT_ID] AS [CLIENT_ID],
	[CLIENT_SECRET] AS [CLIENT_SECRET],
	[GRANT_TYPE] AS [GRANT_TYPE],
	[APP_KEY] AS [APP_KEY],
	[ACCESS_TOKEN] AS [ACCESS_TOKEN],
	[REQUEST_TIMEOUT] AS [REQUEST_TIMEOUT],
	[Start_Time] AS [Start_Time],
	[End_Time] AS [End_Time],
	[Created_Date] AS [Created_Date],
	[Created_By] AS [Created_By],
	[Occurance] AS [Occurance],
	[NextStart_Date_Time] AS [NextStart_Date_Time],
	[End_Date_Time] AS [End_Date_Time],
	[STATUS] AS [STATUS],
	[ACTIVE] AS [ACTIVE]
FROM [dbo].[ScheduleCache] WITH (NoLock)
WHERE
	[dbo].[ScheduleCache].[Schedule_Cache_Id] = @Schedule_Cache_Id
GO
/****** Object:  StoredProcedure [dbo].[spgJobSchedule]    Script Date: 04/16/2015 17:44:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE  PROCEDURE [dbo].[spgJobSchedule] 
(
	@Job_Id uniqueidentifier,
	@Schedule_Id uniqueidentifier
)
AS
BEGIN TRY
	BEGIN
	  SELECT [Schedule_id],[Job_Id],[PlatformId],[SiteId],[Next_Start_DateTime],[Last_Start_DateTime],[TotalRecords],[CachedRecords],[Active] FROM JobSchedule
		  WHERE Job_Id = @Job_Id and Schedule_Id = @Schedule_Id
	END
		
END TRY
BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spgJob_Client_Info]    Script Date: 04/16/2015 17:44:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE  PROCEDURE [dbo].[spgJob_Client_Info] 
(
	@Job_Id uniqueidentifier
)
AS
BEGIN TRY
	BEGIN
	  SELECT [Job_id],[SSo_Url],[Client_id],[Client_Secret],[Grant_Type],[Request_Timeout] FROM Job_Client_Info
		  WHERE Job_Id=@Job_Id			
	END
		
END TRY
BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spiJobSchedule]    Script Date: 04/16/2015 17:45:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spiJobSchedule] 
(
	@Schedule_Id uniqueidentifier,
	@Job_Id uniqueidentifier,
	@PlatformId int,
	@SiteId int,
	@Next_Start_DateTime NVARCHAR(100),
	@Last_Start_DateTime NVARCHAR(100),
	@Active int
) 
AS
BEGIN TRY
	BEGIN
		
		If not Exists(Select top 1 * from JobSchedule Where Schedule_Id = @Schedule_Id and Job_Id = @Job_Id)
		  Begin
			
			INSERT INTO  JobSchedule (Schedule_Id,Job_Id,PlatformId,SiteId,Next_Start_DateTime,Last_Start_DateTime,Active)
			VALUES (@Schedule_Id, @Job_Id, @PlatformId, @SiteId, convert(datetime, @Next_Start_DateTime, 121), convert(datetime, @Last_Start_DateTime, 121), @Active )
			
			Select '1'--'JobSchedule added successfully!'
		  End
		Else
			Select '0'--'JobSchedule already exists!'
	END
END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spiJob_client_Info]    Script Date: 04/16/2015 17:45:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spiJob_client_Info] 
(
	@Job_Id uniqueidentifier,
	@SSo_Url NVARCHAR(250),
	@Client_id NVARCHAR(250),
	@Client_Secret NVARCHAR(250),
	@Grant_Type NVARCHAR(50),
	@Request_Timeout int
)
	 
AS
BEGIN TRY
	BEGIN
						
		If not Exists(Select top 1 * from Job_Client_Info Where Job_Id = @Job_Id )
		  Begin
			
			INSERT INTO  Job_Client_Info (Job_Id,SSo_Url,Client_id,Client_Secret,Grant_Type,Request_Timeout )
			VALUES (@Job_Id,@SSo_Url,@Client_id,@Client_Secret,@Grant_Type,@Request_Timeout )
			
			Select '1'--'Job Client Info added successfully!'
		  End
		Else
			Select '0'--'Job Client Info with this name already exists!'
	END
END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO
/****** Object:  UserDefinedFunction [dbo].[Split]    Script Date: 04/16/2015 17:45:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[Split](@String nvarchar(4000), @Delimiter char(1))
RETURNS @Results TABLE (Id Int identity(1,1), Items nvarchar(4000))
AS
BEGIN
DECLARE @INDEX INT
DECLARE @SLICE nvarchar(4000)
-- HAVE TO SET TO 1 SO IT DOESNT EQUAL Z
--     ERO FIRST TIME IN LOOP
SELECT @INDEX = 1
WHILE @INDEX !=0
BEGIN
-- GET THE INDEX OF THE FIRST OCCURENCE OF THE SPLIT CHARACTER
SELECT @INDEX = CHARINDEX(@Delimiter,@STRING)
-- NOW PUSH EVERYTHING TO THE LEFT OF IT INTO THE SLICE VARIABLE
IF @INDEX !=0
SELECT @SLICE = LEFT(@STRING,@INDEX - 1)
ELSE
SELECT @SLICE = @STRING
-- PUT THE ITEM INTO THE RESULTS SET
INSERT INTO @Results(Items) VALUES(@SLICE)
-- CHOP THE ITEM REMOVED OFF THE MAIN STRING
SELECT @STRING = RIGHT(@STRING,LEN(@STRING) - @INDEX)
-- BREAK OUT IF WE ARE DONE
IF LEN(@STRING) = 0 BREAK
END
RETURN
END
GO
/****** Object:  StoredProcedure [dbo].[spiScheduleExchange]    Script Date: 04/16/2015 17:45:06 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
--* ==================================================
--*  Procedure ScheduleExchange Insert
--* ==================================================
CREATE PROCEDURE [dbo].[spiScheduleExchange]
	@TASK_NAME				nvarchar(100),
	@SCOPE					nvarchar(64),
	@SSO_URL				nvarchar(256),
	@CLIENT_ID				nvarchar(64),
	@CLIENT_SECRET			nvarchar(64), 
	@GRANT_TYPE				nvarchar(64),
	@REQUEST_TIMEOUT		int,
	@START_TIME				DateTime,
	@END_TIME				DateTime,
	@CREATED_DATE			DateTime,
	@CREATED_BY				nvarchar(100),
	@OCCURANCE				nvarchar(24),
	@NEXTSTART_DATE_TIME 	DateTime,
	@END_DATE_TIME			DateTime,
	@STATUS					nvarchar(64),
	@ACTIVE					tinyint
AS
INSERT INTO [dbo].[ScheduleExchange] (
	[TASK_NAME],
	[SCOPE],
	[SSO_URL],
	[CLIENT_ID],
	[CLIENT_SECRET],
	[GRANT_TYPE],
	[REQUEST_TIMEOUT],
	[START_TIME],
	[END_TIME],
	[CREATED_DATE],
	[CREATED_BY],
	[OCCURANCE],
	[NEXTSTART_DATE_TIME],
	[END_DATE_TIME],
	[STATUS],
	[ACTIVE]
	)
VALUES (
	@TASK_NAME,
	@SCOPE,
	@SSO_URL,
	@CLIENT_ID,
	@CLIENT_SECRET,
	@GRANT_TYPE,
	@REQUEST_TIMEOUT,
	@START_TIME,
	@END_TIME,
	@CREATED_DATE,
	@CREATED_BY,
	@OCCURANCE,
	@NEXTSTART_DATE_TIME,
	@END_DATE_TIME,
	@STATUS,
	@ACTIVE)
GO
/****** Object:  StoredProcedure [dbo].[spiScheduleCache]    Script Date: 04/16/2015 17:45:06 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
--* ==================================================
--*  Procedure ScheduleCache Insert
--* ==================================================
CREATE PROCEDURE [dbo].[spiScheduleCache]
	@SCHEDULE_CACHE_ID	nvarchar(64),
	@TASK_NAME			nvarchar(100),
	@PROJECT			nvarchar(64),
	@APP				nvarchar(64), 
	@CACHE_PAGE_SIZE	int,
	@SSO_URL			nvarchar(256),
	@CLIENT_ID			nvarchar(64),
	@CLIENT_SECRET		nvarchar(64), 
	@GRANT_TYPE			nvarchar(64),
	@APP_KEY			nvarchar(64),
	@ACCESS_TOKEN		nvarchar(64),
	@REQUEST_TIMEOUT	int,
	@START_TIME			DateTime,
	@END_TIME			DateTime,
	@CREATED_DATE		DateTime,
	@CREATED_BY			nvarchar(100),
	@OCCURANCE			nvarchar(24),
	@NEXTSTART_DATE_TIME DateTime,
	@END_DATE_TIME		DateTime,
	@STATUS				nvarchar(64),
	@ACTIVE				tinyint
AS
INSERT INTO [dbo].[ScheduleCache] (
	[SCHEDULE_CACHE_ID],
	[TASK_NAME],
	[PROJECT],
	[APP],
	[CACHE_PAGE_SIZE],
	[SSO_URL],
	[CLIENT_ID],
	[CLIENT_SECRET],
	[GRANT_TYPE],
	[APP_KEY],
	[ACCESS_TOKEN],
	[REQUEST_TIMEOUT],
	[START_TIME],
	[END_TIME],
	[CREATED_DATE],
	[CREATED_BY],
	[OCCURANCE],
	[NEXTSTART_DATE_TIME],
	[END_DATE_TIME],
	[STATUS],
	[ACTIVE]
	)
VALUES (
	@SCHEDULE_CACHE_ID,
	@TASK_NAME,
	@PROJECT,
	@APP,
	@CACHE_PAGE_SIZE,
	@SSO_URL,
	@CLIENT_ID,
	@CLIENT_SECRET,
	@GRANT_TYPE,
	@APP_KEY,
	@ACCESS_TOKEN,
	@REQUEST_TIMEOUT,
	@START_TIME,
	@END_TIME,
	@CREATED_DATE,
	@CREATED_BY,
	@OCCURANCE,
	@NEXTSTART_DATE_TIME,
	@END_DATE_TIME,
	@STATUS,
	@ACTIVE)
GO
/****** Object:  StoredProcedure [dbo].[spuDataFilter]    Script Date: 04/16/2015 17:45:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Deepak Kumar>
-- Create date: <17-Sep-2014>
-- Description:	<updating datafilter>
-- ===========================================================
CREATE PROCEDURE [dbo].[spuDataFilter] 
(
	@ResourceId uniqueidentifier,
	@DataFilterTypeId int,
	@SiteId int,
	@RawXml xml
)	 
AS

	BEGIN 
	
	
--Set @RawXml = '<dataFilters>
--	<dataFilter>
--		<dataFilterId>00000000-0000-0000-0000-000000000000</dataFilterId>
--		<resourceId>d208b9b8-0372-4e2c-9b34-78b9bb453b61</resourceId>
--		<dataFilterTypeId>2</dataFilterTypeId>
--		<siteId>1</siteId>
--		<active>1</active>
--		<expressions>
--			<expression>
--				<dfOrder>1</dfOrder>
--				<openGroupCount>10</openGroupCount>
--				<propertyName>xyz.abc.valuwIdentifier</propertyName>
--				<relationalOperator>contains</relationalOperator>
--				<values>
--					<value>2</value>
--					<value>3</value>
--				</values>
--				<logicalOperator>Or</logicalOperator>
--				<closeGroupCount>23</closeGroupCount>
--				<isCaseSensitive>false</isCaseSensitive>
--			</expression>
--			<expression>
--				<dfOrder>2</dfOrder>
--				<openGroupCount>1</openGroupCount>
--				<propertyName>xyz.abc.valuwIdentifier</propertyName>
--				<relationalOperator>contains</relationalOperator>
--				<values>
--					<value>3</value>
--				</values>
--				<logicalOperator>AND</logicalOperator>
--				<closeGroupCount>2</closeGroupCount>
--				<isCaseSensitive>true</isCaseSensitive>
--			</expression>
--		</expressions>
--		<orderExpressions>
--			<orderExpression>
--				<dfOrder>1</dfOrder>
--				<propertyName>xyz.abc.valueIdentifier</propertyName>
--				<sortOrder>ASC</sortOrder>
--			</orderExpression>
--		</orderExpressions>
--		<isAdmin>true</isAdmin>
--	</dataFilter>
--</dataFilters>'

--This is temporary solution, need to find some workaround
Set @RawXml = REPLACE(Cast(@RawXml as varchar(max)),'<value>','<value>,')

	
	--[[deleting the exitsting stuff [had a discussion with vidisha for the same, date 23-Sep-2014] 
	Delete d from DataFilterValues d inner join  DataFilters df on d.DataFilterId = df.DataFilterId and df.ResourceId = @ResourceId
	
	Delete from DataFilters where ResourceId = @ResourceId
	--]]
	
	Create table #Expression
	(
		DataFilterId	uniqueidentifier default newid(),
		ResourceId	uniqueidentifier,
		DataFilterTypeId	int,
		IsExpression	int default 1,
		DFOrder	int,
		OpenGroupCount	int default null,
		LogicalOperator	nvarchar(50) default null,
		PropertyName	nvarchar(350) default null,
		RelationalOperator	nvarchar(150) default null,
		[Values] varchar(1000),
		CloseGroupCount	int default null,
		SiteId	int,
		IsCaseSensitive bit,
		IsAdmin bit
		
	)	

	Create table #OrderExpression
	(
		DataFilterId	uniqueidentifier default newid(),
		ResourceId	uniqueidentifier,
		DataFilterTypeId	int,
		IsExpression	int default 0,
		DFOrder	int,
		PropertyName	nvarchar(350) default null,
		SortOrder	nvarchar(25) default null,
		SiteId	int,
		IsAdmin bit
	)


SELECT       
 @ResourceId as ResourceId,      
 @SiteId as SiteId,     
 @DataFilterTypeId as DataFilterTypeId,
 nref.value('isAdmin[1]', 'bit') IsAdmin           
into #Parent FROM   @rawXML.nodes('//dataFilter') AS R(nref)
	

	INSERT INTO #Expression
			   (
				[ResourceId]
			   ,[DataFilterTypeId]
			   ,[DFOrder]
			   ,[OpenGroupCount]
			   ,[LogicalOperator]
			   ,[PropertyName]
			   ,[RelationalOperator]
			   ,[Values]
			   ,[CloseGroupCount]
			   ,[SiteId]
			   ,[IsCaseSensitive]
			   ,[IsAdmin]  
			   )
	SELECT
	 ResourceId,
	 DataFilterTypeId,
	 nref.value('dfOrder[1]', 'int') DFOrder,      
	 nref.value('openGroupCount[1]', 'int') OpenGroupCount,      
	 nref.value('logicalOperator[1]', 'nvarchar(50)') LogicalOperator,
	 nref.value('propertyName[1]', 'nvarchar(350)') PropertyName,
	 nref.value('relationalOperator[1]', 'nvarchar(150)') RelationalOperator,
	 nref.value('values[1]','varchar(500)') [values],
	 nref.value('closeGroupCount[1]', 'int') CloseGroupCount,
	 SiteId,
	 nref.value('isCaseSensitive[1]', 'bit') IsCaseSensitive,
	 IsAdmin
	FROM  @rawXML.nodes('//expression') AS R(nref),#Parent 

	

	Insert Into #OrderExpression
	(
		ResourceId,
		DataFilterTypeId,
		DFOrder,
		PropertyName,
		SortOrder,
		SiteId,
		IsAdmin
	)
	SELECT
	 ResourceId,
	 DataFilterTypeId,
	 nref.value('dfOrder[1]', 'int') DFOrder,      
	 nref.value('propertyName[1]', 'nvarchar(350)') PropertyName,
	 nref.value('sortOrder[1]', 'nvarchar(25)') SortOrder,
	 SiteId,
	 IsAdmin
	FROM  @rawXML.nodes('//orderExpression') AS R(nref),#Parent 	
   
   
   

	
	INSERT INTO [DataFilters]
           ([DataFilterId]
           ,[ResourceId]
           ,[DataFilterTypeId]
           ,[IsExpression]
           ,[DFOrder]
           ,[OpenGroupCount]
           ,[LogicalOperator]
           ,[PropertyName]
           ,[RelationalOperator]
           ,[CloseGroupCount]
           ,[SortOrder]
           ,[SiteId]
           ,[IsCaseSensitive]
           ,[IsAdmin]  
           )
	select 
		DataFilterId,
		ResourceId,
		DataFilterTypeId,
		IsExpression,
		DFOrder,
		OpenGroupCount,
		LogicalOperator,
		PropertyName,
		RelationalOperator,
		CloseGroupCount,
		Null as SortOrder,
		SiteId,
		IsCaseSensitive,
		IsAdmin
	 from #Expression
	 union all
	select 
		DataFilterId,
		ResourceId,
		DataFilterTypeId,
		IsExpression,
		DFOrder,
		Null as OpenGroupCount,
		Null as LogicalOperator,
		PropertyName,
		Null as RelationalOperator,
		Null as CloseGroupCount,
		SortOrder,
		SiteId,
		Null as IsCaseSensitive,
		IsAdmin
	 from #OrderExpression


Insert into  DataFilterValues 
Select DataFilterId,Items as value from #Expression E Cross Apply dbo.Split(E.[Values],',') S 
where S.Items <> ''

--drop table #Parent
--drop table #Expression
--drop table #OrderExpression
							

END
GO
/****** Object:  StoredProcedure [dbo].[spuGroups]    Script Date: 04/16/2015 17:45:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Gagan Dhamija>
-- Create date: <10-Jul-2014>
-- Description:	<Update Groups>

-- =============================================
CREATE PROCEDURE [dbo].[spuGroups] 
(
	@GroupId INT,
	@GroupName NVARCHAR(100),
	@GroupDesc NVARCHAR(100)
)
	 
AS
BEGIN TRY
	BEGIN
	
	--declare @aa int
 --   set @aa = 10 / 0
    
		UPDATE Groups 
			SET GroupName = @GroupName,
				GroupDesc = @GroupDesc
		  WHERE	GroupId = @GroupId
		  
		Select '1'--'Group updated successfully!'
	END
END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spiSchedule]    Script Date: 04/16/2015 17:45:05 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spiSchedule] 
(
	@Created_DateTime NVARCHAR(100),
	@Created_By NVARCHAR(250),
	@Occurance NVARCHAR(50),
	@Weekday NVARCHAR(50),
	@Start_DateTime NVARCHAR(100),
	@End_DateTime NVARCHAR(100),
	@Status NVARCHAR(50)
)
	 
AS
BEGIN TRY
	BEGIN
						
		--If not Exists(Select top 1 * from Schedule Where ScheduleId = @ScheduleId )
		  Begin
			Declare @ScheduleId uniqueidentifier = NewID() 
			
			if @Status = ''
			  Begin
				Set @Status = 'Ready'
			  End
			
			INSERT INTO  Schedule (ScheduleId, Created_DateTime, Created_By, Occurance, Weekday, Start_DateTime, End_DateTime, Status)
			VALUES (@ScheduleId, convert(datetime, @Created_DateTime, 121), @Created_By, @Occurance, @Weekday, convert(datetime, @Start_DateTime, 121), convert(datetime, @End_DateTime, 121), @Status )
			
			Select '1'--'Schedule added successfully!'
		  End
		--Else
			--Select '0'--'Schedule already exists!'
	END
END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spiUser]    Script Date: 04/16/2015 17:45:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <26-June-2014>
-- Description:	<Inserting user details>

-- =============================================
CREATE PROCEDURE [dbo].[spiUser]  --1,'test_user1','df','df','emil',34,'df'
(
	@UserName NVARCHAR(100),
	@UserFirstName  NVARCHAR(50),
	@UserLastName NVARCHAR(50),
	@UserEmail NVARCHAR(50),
	@UserPhone NVARCHAR(50),
	@UserDesc  NVARCHAR(255)
)	
	 
AS
BEGIN TRY
	BEGIN
	
		--declare @aa int
		--set @aa = 10 / 0
		
		If not Exists(Select top 1 * from users Where UserName = @UserName and Active = 1)
		Begin
			
			INSERT INTO Users (UserName,UserFirstName,UserLastName,UserEmail,UserPhone,UserDesc)
			VALUES(@UserName,@UserFirstName,@UserLastName,@UserEmail,@UserPhone,@UserDesc)			
			
			Select  '1'--'User added successfully!'
		End
		Else
			Select  '0'--'User with this name already exists!'
				
	END
END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spiSites]    Script Date: 04/16/2015 17:45:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <06-June-2014>
-- Description:	<Inserting Site details,,>
-- =============================================
CREATE PROCEDURE [dbo].[spiSites]
(
	@SiteName NVARCHAR(10),
	@SiteDesc NVARCHAR(255)

)	
	 
AS
BEGIN TRY
	BEGIN
		INSERT INTO Sites(SiteName,SiteDesc) VALUES (@SiteName,@SiteDesc)
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spiRole]    Script Date: 04/16/2015 17:45:04 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <03-Jul-2014>
-- Description:	<Inserting Roles>

-- =============================================
CREATE PROCEDURE [dbo].[spiRole] 
(
	@RoleName NVARCHAR(100),
	@RoleDesc NVARCHAR(100)
	
)	
	 
AS
BEGIN TRY
	BEGIN
	
		--declare @aa int
		--set @aa = 10 / 0
		
		If not Exists(Select top 1 * from Roles Where RoleName = @RoleName and Active = 1)
		Begin
		
		INSERT INTO Roles (RoleName,RoleDesc)
		VALUES(@RoleName,@RoleDesc)
			
			Select '1'--'Role added successfully!'
		End
		Else
			Select '0'--'Role with this name already exists!'

	END
END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spiPermissions]    Script Date: 04/16/2015 17:45:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <07-Jul-2014>
-- Description:	<Deleting Roles> 

-- =============================================
CREATE PROCEDURE [dbo].[spiPermissions] 
(
	@PermissionName NVARCHAR(100),
	@PermissionDesc NVARCHAR(100)
)
	 
AS
BEGIN TRY
	BEGIN
		
		--declare @aa int
		--set @aa = 10 / 0
		
		If not Exists(Select top 1 * from [Permissions] Where PermissionName = @PermissionName and Active = 1)
		Begin
			INSERt INTO  [Permissions] (PermissionName,PermissionDesc)
			VALUES (@PermissionName,@PermissionDesc)
						
			Select '1'--'Permission added successfully!'
		End
		Else
			Select '0'--'Permission with this name already exists!'
		
	END
END TRY

BEGIN CATCH
      SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spiGroups]    Script Date: 04/16/2015 17:45:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <07-Jul-2014>
-- Description:	<Deleting Roles> 

-- =============================================
CREATE PROCEDURE [dbo].[spiGroups] 
(
	@GroupName NVARCHAR(100),
	@GroupDesc NVARCHAR(100)
)
	 
AS
BEGIN TRY
	BEGIN

		--declare @aa int
		--set @aa = 10 / 0
						
		If not Exists(Select top 1 * from Groups Where GroupName = @GroupName and Active = 1)
		Begin
			INSERt INTO  Groups (GroupName,GroupDesc )
			VALUES (@GroupName,@GroupDesc)
			
			Select '1'--'Group added successfully!'
		End
		Else
			Select '0'--'Group with this name already exists!'
	END
END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spgDataFilterByUser]    Script Date: 04/16/2015 17:44:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Vidisha>
-- Create date: <27-Aug-2014>
-- Description:	<Selecting datafilters based on users>
-- ===========================================================
CREATE PROCEDURE [dbo].[spgDataFilterByUser]
(
	@ResourceId uniqueidentifier--'d208b9b8-0372-4e2c-9b34-78b9bb453b61'
)	 
AS

	BEGIN
	 
			WITH XMLNAMESPACES ( DEFAULT 'http://www.iringtools.org/data/filter' ) 
			Select
			  (
				Select 
				   EX.[openGroupCount]
				  ,EX.[propertyName]
				  ,EX.[relationalOperator]
				  ,(
						Select 
							EXV.[value]
						from ExpressionValues EXV where EXV.ExpressionId = EX.ExpressionId for XML path(''),root('values'),type   --,ELEMENTS XSINIL
				   )
				 ,EX.[logicalOperator]
				 ,EX.[closeGroupCount]
				 ,EX.[isCaseSensitive]
				 ,EX.[dataFilterId]
				 ,EX.[expressionId]
				 
				 from Expression EX where EX.DataFilterId = DF.DataFilterId  For xml path('expression'),type	--,ELEMENTS XSINIL
			  ) as 'expressions'
			 ,(
				Select 
					 OE.[propertyName]
					,OE.[sortOrder]
					,OE.[dataFilterId]
					,OE.[orderExpressionId]
				from OrderExpression OE where OE.DataFilterId = DF.DataFilterId For xml path('orderExpression'),type	--,ELEMENTS XSINIL
			  ) as 'orderExpressions'
			 ,(
				Select 
				   RE.[groupBy] AS 'groupBy'
				   ,(
					  Select 
							 R.[propertyName]
							,R.RollupType as [type]
							,R.[rollupExpressionId]
							,R.[rollupId]
					   from [Rollup] R where R.RollupExpressionId = RE.RollupExpressionId For xml path('rollup'),type    --,ELEMENTS XSINIL		
					) as 'rollups'	
				   ,RE.[dataFilterId]
				   ,RE.[rollupExpressionId]
				From RollupExpression RE Where RE.DataFilterId = DF.DataFilterId For xml path('rollupExpression'),type   --,ELEMENTS XSINIL	
			   ) as 'rollupExpressions'
			  ,DF.[dataFilterId]
			  ,DF.[resourceId]
			  ,DF.[dataFilterTypeId]
			  ,DF.[active]
			From DataFilters DF 
			WHERE DF.ResourceId = @ResourceId
				AND DF.Active = 1
			Group by
			  DF.[dataFilterId]
			  ,DF.[resourceId]
			  ,DF.[dataFilterTypeId]
			  ,DF.[active]
			for xml PATH('dataFilter'), ROOT('dataFilters'),type, ELEMENTS XSINIL 

	END
GO
/****** Object:  StoredProcedure [dbo].[spgRolesById]    Script Date: 04/16/2015 17:44:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <17-June-2014>
-- Description:	<Getting Role by specific id details,,>
-- =============================================
CREATE PROCEDURE [dbo].[spgRolesById]
(
	@RoleId INT
)
	 
AS
BEGIN TRY
	BEGIN
		SELECT RoleId,SiteId, RoleName,RoleDesc FROM Roles WHERE Active=1 AND RoleId =@RoleId
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spgRoles]    Script Date: 04/16/2015 17:44:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <17-June-2014>
-- Description:	<Getting Site details,,>
-- =============================================
CREATE PROCEDURE [dbo].[spgRoles]
	 
AS
BEGIN TRY
	BEGIN
		SELECT RoleId, RoleName, RoleDesc   FROM Roles WHERE Active=1
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spgSchedule]    Script Date: 04/16/2015 17:44:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE  PROCEDURE [dbo].[spgSchedule] 
(
	@ScheduleId uniqueidentifier
)
AS
BEGIN TRY
	BEGIN
	  SELECT [ScheduleId],[Created_DateTime],[Created_By],[Occurance],[Weekday],[Start_DateTime],[End_DateTime],[Status] FROM Schedule
		  WHERE ScheduleId = @ScheduleId			
	END
		
END TRY
BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spgSitesById]    Script Date: 04/16/2015 17:44:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <13-June-2014>
-- Description:	<Getting Site by specific Id,,>
-- =============================================
CREATE PROCEDURE [dbo].[spgSitesById] 
(
	@SiteId INT
)
	 
AS
BEGIN TRY
	BEGIN
		SELECT SiteName,SiteDesc FROM Sites WHERE Active=1 AND SiteId = @SiteId
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber, ERROR_MESSAGE() AS ErrorMessage;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spgSites]    Script Date: 04/16/2015 17:44:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <12-June-2014>
-- Description:	<Getting Site details,,>
-- =============================================
CREATE PROCEDURE [dbo].[spgSites]

	 
AS
BEGIN TRY
	BEGIN
		SELECT SiteId,SiteName,SiteDesc,Active FROM Sites WHERE Active=1
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spgSiteRoles]    Script Date: 04/16/2015 17:44:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <18-June-2014>
-- Description:	<Getting Site Roles,,>
-- =============================================
CREATE PROCEDURE [dbo].[spgSiteRoles]
(
	@SiteId INT
)
	 
AS
BEGIN TRY
	BEGIN
		SELECT RoleName,RoleDesc FROM Roles WHERE Active=1 AND SiteId=@SiteId
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber, ERROR_MESSAGE() as ErrorMessage
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spgSiteGroups]    Script Date: 04/16/2015 17:44:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =================================================================
-- Author:		<Atul Srivastava>
-- Create date: <27-June-2014>
-- Description:	<Getting site groups based on site id

-- ==================================================================
CREATE PROCEDURE [dbo].[spgSiteGroups] 
(
	@SiteId INT
)	
	 
AS
BEGIN TRY
	BEGIN
		SELECT GroupId,SiteId,GroupName,GroupDesc FROM Groups WHERE Active =1 AND SiteId =@SiteId
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spgPermissions]    Script Date: 04/16/2015 17:44:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Prashant Dubey>
--Modified by Atul Srivastava - Adding Site id paramenter
-- Create date: <27-June-2014>
-- Description:	<Getting Permissions>
-- =============================================
CREATE PROCEDURE [dbo].[spgPermissions]
	 
AS
BEGIN TRY
	BEGIN
		SELECT PermissionId,PermissionName,PermissionDesc FROM Permissions WHERE Active=1
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spgPermissionRoles]    Script Date: 04/16/2015 17:44:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <02-Jul-2014>
-- Description:	<Return permission based on role id and permission id>

-- =============================================
CREATE PROCEDURE [dbo].[spgPermissionRoles] 
(
	@RoleId INT,
	@PermissionId INT,
	@Siteid INT
)	
	 
AS
BEGIN TRY
	BEGIN
		SELECT PermissionId,PermissionName,PermissionDesc FROM [Permissions] P
			INNER JOIN Roles R ON R.SiteId = P.SiteId
		WHERE R.RoleId = @RoleId AND P.Active =1 AND R.Active=1 AND P.PermissionId = @PermissionId
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spgUserById]    Script Date: 04/16/2015 17:44:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <18-June-2014>
-- Description:	<Getting User by specific user id >
-- =============================================
CREATE PROCEDURE [dbo].[spgUserById]
(
	@UserId INT
)
	 
AS
BEGIN TRY
	BEGIN
		SELECT UserId,UserName,SiteId,UserFirstName,UserLastName,UserLastName,UserEmail,UserPhone,UserDesc  FROM Users WHERE Active=1 AND UserId =@UserId
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spgSiteUsers]    Script Date: 04/16/2015 17:44:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <30-June-2014>
-- Description:	<Getting Users based on site id>
-- =============================================
CREATE PROCEDURE [dbo].[spgSiteUsers]
(
	@SiteId INT
)
	 
AS
BEGIN TRY
	BEGIN
		
		SELECT  U.UserId,U.UserFirstName ,U.UserLastName,U.UserEmail,U.UserPhone,U.UserDesc FROM Users U
		WHERE U.Active=1 AND U.SiteId = @SiteId
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spiDataFilter]    Script Date: 04/16/2015 17:44:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		 Deepak Kumar
-- Create date:  17-Sep-2014
-- Modified by:  Deepak Kuamr
-- Modified date: 3-Apr-2015
-- Description:	 Inserting datafilter
-- ===========================================================
CREATE PROCEDURE [dbo].[spiDataFilter] 
(
	@rawXml xml
)	 
AS

BEGIN TRY
	BEGIN

/*declare @rawXml xml = 
 '<dataFilter>
        <expressions>
          <expression>
            <openGroupCount>1</openGroupCount>
            <propertyName>ExPPropName</propertyName>
            <relationalOperator>GreaterThan</relationalOperator>
            <values>
              <value>One</value>
              <value>Two</value>
            </values>
            <logicalOperator>And</logicalOperator>
            <closeGroupCount>2</closeGroupCount>
            <isCaseSensitive>true</isCaseSensitive>
            <dataFilterId>00000000-0000-0000-0000-000000000000</dataFilterId>
            <expressionId>00000000-0000-0000-0000-000000000000</expressionId>
          </expression>
        </expressions>
        <orderExpressions>
          <orderExpression>
            <propertyName>Prop1</propertyName>
            <sortOrder>Asc</sortOrder>
            <dataFilterId>00000000-0000-0000-0000-000000000000</dataFilterId>
            <orderExpressionId>00000000-0000-0000-0000-000000000000</orderExpressionId>
          </orderExpression>
        </orderExpressions>
        <rollupExpressions>
          <rollupExpression>
            <groupBy>Group1</groupBy>
            <rollups>
              <rollup>
                <propertyName>Group1_prop1</propertyName>
                <type>Average</type>
                <rollupExpressionId>00000000-0000-0000-0000-000000000000</rollupExpressionId>
                <rollupId>00000000-0000-0000-0000-000000000000</rollupId>
              </rollup>
              <rollup>
                <propertyName>Group1_prop2</propertyName>
                <type>Max</type>
                <rollupExpressionId>00000000-0000-0000-0000-000000000000</rollupExpressionId>
                <rollupId>00000000-0000-0000-0000-000000000000</rollupId>
              </rollup>
            </rollups>
            <dataFilterId>00000000-0000-0000-0000-000000000000</dataFilterId>
            <rollupExpressionId>00000000-0000-0000-0000-000000000000</rollupExpressionId>
          </rollupExpression>
        </rollupExpressions>
        <isAdmin>true</isAdmin>
        <dataFilterId>00000000-0000-0000-0000-000000000000</dataFilterId>
        <resourceId>F2BBF544-39C5-4C5F-9B1F-72E2EF8132FE</resourceId>
        <dataFilterTypeId>1</dataFilterTypeId>
      </dataFilter>' */
				select
					dataFilterId = NEWID(),
					resourceId = a.value('resourceId[1]','uniqueidentifier'), 
					dataFilterTypeId = a.value('dataFilterTypeId[1]','int'),
					expressions = a.query('expressions'),
					orderExpressions = a.query('orderExpressions'),
					rollupExpressions = a.query('rollupExpressions'),
					isAdmin = a.value('isAdmin[1]','bit')
				into #DataFilters
				from @rawXml.nodes('//dataFilter') as T(a)
				
				--Delete before inserting				    
				--Delete Section for DataFilter Starts******************
							Begin
							
							--Delete Section for Expression Starts******************
								Begin
									Delete d 
									from ExpressionValues d inner join Expression e on d.ExpressionId = e.ExpressionId
															inner join DataFilters df on df.DataFilterId = e.DataFilterId
															inner join #DataFilters do on do.resourceId = df.ResourceId
									--Print 'ExpressionValues Deleted'						
									Delete e 
									from Expression e inner join DataFilters df on df.DataFilterId = e.DataFilterId
													  inner join #DataFilters do on do.resourceId = df.ResourceId
									--print 'Expression Deleted'				  
								End
							--Delete Section for Expression Ends******************					
							 
							--Delete Section for OrderExpression Starts******************
								Begin
															
									Delete oe 
									from OrderExpression oe inner join DataFilters df on df.DataFilterId = oe.DataFilterId
													  inner join #DataFilters do on do.resourceId = df.ResourceId
									--print 'Order Expression Deleted'				  				  
								End
							--Delete Section for OrderExpression Ends******************					 

							--Delete Section for RollupExpression Starts*******************
								Begin
															
									Delete ru 
									from [Rollup] ru inner join RollupExpression re on ru.RollupExpressionId = re.RollupExpressionId
													 inner join DataFilters df on df.DataFilterId  = re.DataFilterId
													 inner join #DataFilters do on do.resourceId = df.ResourceId
									--print 'Rollup Deleted'				 
									Delete re 
									from RollupExpression re inner join DataFilters df on df.DataFilterId = re.DataFilterId
													  inner join #DataFilters do on do.resourceId = df.ResourceId
									--print 'RollupExpression Deleted'				  				 
								End
							--Delete Section for RollupExpression Ends**********************			
							
							Delete df 
							from DataFilters df inner join #DataFilters do on do.resourceId = df.ResourceId
																	
							End							
				--Delete Section for DataFilter Ends******************

				-- DataFilter Section_Starts*********************
						Begin

								
								-- Expression Section_Starts*********************
								Begin
									select
										expressionId = NEWID(),
										dataFilterId,
										openGroupCount = a.value('openGroupCount[1]','int'),
										propertyName = a.value('propertyName[1]','nvarchar(350)'),
										relationalOperator = a.value('relationalOperator[1]','nvarchar(150)'),
										[values] = a.query('values'),
										logicalOperator = a.value('logicalOperator[1]','nvarchar(50)'),
										closeGroupCount = a.value('closeGroupCount[1]','int'),
										isCaseSensitive = a.value('isCaseSensitive[1]','bit')
									into #Expression
									from #DataFilters cross apply @rawXML.nodes('//expression') as T(a)
									
									select
										expressionId,
										value = a.value('(text())[1]','nvarchar(200)')
									into #ExpressionValues
									from #Expression cross apply [values].nodes('/values/value') as T(a)
								End
								-- Expression Section_Ends***********************
								
								-- OrderExpression Section_Starts*********************
								Begin
									select
										orderExpressionId = NEWID(),
										dataFilterId,
										propertyName = a.value('propertyName[1]','nvarchar(350)'),
										sortOrder = a.value('sortOrder[1]','nvarchar(25)')
									into #OrderExpression
									from #DataFilters cross apply @rawXML.nodes('//orderExpression') as T(a)
								End
								-- OrderExpression Section_Ends***********************
								
								-- RollupExpression Section_Starts*********************
								Begin
									select
										rollupExpressionId = NEWID(),
										dataFilterId,
										groupBy = a.value('groupBy[1]','nvarchar(100)'),
										rollups = a.query('rollups')
									into #RollupExpression
									from #DataFilters cross apply @rawXML.nodes('//rollupExpression') as T(a)
									
											-- Rollup Section_Starts*********************
											Begin
												select
													rollupExpressionId,
													propertyName = a.value('propertyName[1]','nvarchar(350)'),
													[type] = a.value('type[1]','nvarchar(50)')
												into #Rollup
												from #RollupExpression cross apply rollups.nodes('/rollups/rollup') as T(a)
																			
											End
											-- Rollup Section_Ends*********************					
								End
								-- RollupExpression Section_Ends***********************				
						End		
				-- DataFilter Section_Ends***********************
				
-- Insert Section Starts**************************
INSERT INTO [DataFilters]([DataFilterId],[ResourceId],[DataFilterTypeId],[IsAdmin])
                   Select [dataFilterId],[resourceId],[dataFilterTypeId],[isAdmin] from #DataFilters
                   

INSERT INTO [Expression]([DataFilterId],[ExpressionId],[OpenGroupCount],[PropertyName],[RelationalOperator],[LogicalOperator],[CloseGroupCount],[IsCaseSensitive])
                  Select [dataFilterId],[expressionId],[openGroupCount],[propertyName],[relationalOperator],[logicalOperator],[closeGroupCount],[isCaseSensitive] from #Expression
               
 
INSERT INTO [ExpressionValues]([ExpressionId],[Value])
                        Select [expressionId],[value] from #ExpressionValues
                        

INSERT INTO [OrderExpression]([DataFilterId],[OrderExpressionId],[PropertyName],[SortOrder])
	                   Select [dataFilterId],[orderExpressionId],[propertyName],[sortOrder] from #OrderExpression
                    

INSERT INTO [RollupExpression]([DataFilterId],[RollupExpressionId],[GroupBy])
                        Select [dataFilterId],[rollupExpressionId],[groupBy] from #RollupExpression
                        
INSERT INTO [Rollup]([RollupExpressionId],[PropertyName],[RollupType])
		Select       [rollupExpressionId],[propertyName],[type] from #Rollup
				
-- Insert Section Ends****************************
   
   --select * from #DataFilters
   --select * from #Expression
   --select * from #ExpressionValues
   --select * from #OrderExpression
   --select * from #RollupExpression
   --select * from #Rollup
   
   
   drop table #DataFilters
   drop table #Expression
   drop table #ExpressionValues
   drop table #OrderExpression
   drop table #RollupExpression
   drop table #Rollup
  
  Select '1'----'DataFilters added successfully!'
  
 END
   
END TRY
BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spgAppSettings]    Script Date: 04/16/2015 17:44:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<M Ramesh>
-- Create date: <08-Apr-2015,>
-- Description:	<Getting for AppSettings for Application>
-- =============================================

CREATE PROCEDURE [dbo].[spgAppSettings]
AS
BEGIN
	BEGIN TRY	
		BEGIN TRANSACTION;
		
		WITH XMLNAMESPACES ( DEFAULT 'http://www.iringtools.org/library')
		(
			SELECT DISTINCT
				SettingId AS id,
				Name AS name
			FROM Settings
		)
		FOR XML PATH('applicationSetting'),  ROOT('applicationSettings'), TYPE, ELEMENTS XSINIL
   
	    COMMIT TRANSACTION;	    
	END TRY 	   
	BEGIN CATCH
		ROLLBACK TRANSACTION;
		SELECT 'Error occured at database: ' + ERROR_MESSAGE()
	END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[spgAllGroups ]    Script Date: 04/16/2015 17:44:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <17-June-2014>
-- Description:	<Getting All Groups>

-- =============================================
CREATE  PROCEDURE [dbo].[spgAllGroups ] 
 
AS
BEGIN TRY
	BEGIN
		SELECT GroupId,GroupName,GroupDesc FROM Groups WHERE Active=1
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spgAllUser]    Script Date: 04/16/2015 17:44:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <24-June-2014>
-- Description:	<Getting All User >
-- =============================================
CREATE PROCEDURE [dbo].[spgAllUser]

	 
AS
BEGIN TRY
	BEGIN
		SELECT UserId, UserName, UserFirstName, UserLastName,UserLastName +','+ UserFirstName AS UserFullName, UserEmail, UserPhone, UserDesc, Active FROM [Users]
		where active = 1
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spgGroups]    Script Date: 04/16/2015 17:44:36 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <17-June-2014>
-- Description:	<Getting All Groups>

-- =============================================
CREATE  PROCEDURE [dbo].[spgGroups] 
(
	@GroupId INT
)
 
AS
BEGIN TRY
	BEGIN
		SELECT GroupId,SiteId,GroupName,GroupDesc FROM Groups WHERE Active=1 AND GroupId = @GroupId
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spdSchedule]    Script Date: 04/16/2015 17:44:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE  PROCEDURE [dbo].[spdSchedule] 
(
	@ScheduleId uniqueidentifier
)
AS
BEGIN TRY
	BEGIN
	  -- delete
	 
	  DELETE FROM Schedule where ScheduleId = @ScheduleId
	  Select  '1'--'Schedule Deleted successfully!'
	END
		
END TRY
BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spdSites]    Script Date: 04/16/2015 17:44:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <06-June-2014>
-- Description:	<Deleting/Deactivating Sites>

-- =============================================
CREATE PROCEDURE [dbo].[spdSites] 

(
	@SiteId INT
)	
	 
AS
BEGIN TRY
	BEGIN
		UPDATE Sites 
			SET Active =0
			WHERE SiteId = @SiteId
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber;
END CATCH;
GO
/****** Object:  Table [dbo].[JobSchedule_Expired]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[JobSchedule_Expired](
	[Schedule_Id] [uniqueidentifier] NOT NULL,
	[Job_Id] [uniqueidentifier] NOT NULL,
	[Next_Start_DateTime] [datetime] NULL,
	[Last_Start_DateTime] [datetime] NULL,
	[Active] [tinyint] NOT NULL,
	[PlatformId] [int] NOT NULL,
	[SiteId] [int] NOT NULL,
	[TotalRecords] [int] NULL,
	[CachedRecords] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DataLayers]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DataLayers](
	[Assembly] [nvarchar](650) NOT NULL,
	[Name] [nvarchar](250) NOT NULL,
	[Configurable] [bit] NULL,
	[IsLightWeight] [bit] NULL,
	[SiteId] [int] NOT NULL,
	[PlatformId] [int] NOT NULL,
	[Active] [tinyint] NOT NULL,
 CONSTRAINT [PK_DataLayers_1] PRIMARY KEY CLUSTERED 
(
	[Name] ASC,
	[SiteId] ASC,
	[PlatformId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[spdDataFilter]    Script Date: 04/16/2015 17:44:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Deepak Kumar>
-- Create date: <17-Sep-2014>
-- Description:	<deleting datafilter>
-- ===========================================================
CREATE PROCEDURE [dbo].[spdDataFilter] 
(
	@ResourceId	uniqueidentifier
)	 
AS

	BEGIN
	
	
		UPDATE [DataFilters]
		   SET 
			Active = 0			  
	 WHERE ResourceId = @ResourceId


--As this is child table, and totaly dependent on DataFilters table, hence deleting the records permanantly
--and inserting new one
		delete
		from DataFilterValues where 
		DataFilterId in 
		(select DataFilterId from DataFilters where ResourceId = @ResourceId)



	END
GO
/****** Object:  Table [dbo].[Job_client_Info_expired]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Job_client_Info_expired](
	[Job_Id] [uniqueidentifier] NOT NULL,
	[SSo_Url] [nvarchar](250) NOT NULL,
	[Client_id] [nvarchar](250) NOT NULL,
	[Client_Secret] [nvarchar](250) NOT NULL,
	[Grant_Type] [nvarchar](50) NULL,
	[Request_Timeout] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserGroups]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserGroups](
	[UserGroupId] [int] IDENTITY(1,1) NOT NULL,
	[GroupId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[UserGroupsDesc] [nvarchar](255) NULL,
	[Active] [tinyint] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[UserGroupId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[spuUser]    Script Date: 04/16/2015 17:45:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================  
-- Author:  <Atul Srivastava>  
-- Create date: <07-Jul-2014>  
-- Description: <Inserting user details>  
  
-- =============================================  
CREATE PROCEDURE [dbo].[spuUser]   
(  
 @UserId INT,  --[[Deepak kumar uncommented userid although not used here but sent from service]]
 @UserName NVARCHAR(100),  
 @UserFirstName  NVARCHAR(50),  
 @UserLastName NVARCHAR(50),  
 @UserEmail NVARCHAR(50),  
 @UserPhone NVARCHAR(50),  
 @UserDesc  NVARCHAR(255)  
)   
    
AS  
BEGIN TRY  
 BEGIN  
 	--declare @aa int
		--set @aa = 10 / 0

	  UPDATE Users SET  
	  UserName = @UserName,  
	  UserFirstName = @UserFirstName,  
	  UserLastName =@UserLastName,  
	  UserEmail = @UserEmail,  
	  UserPhone = @UserPhone,  
	  UserDesc = @UserDesc WHERE UserName = @UserName AND Active =1  
   
	 Select  '1'--'User updated successfully!'	
 END  
END TRY  
  
BEGIN CATCH  
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spuSites]    Script Date: 04/16/2015 17:45:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <06-June-2014>
-- Description:	<Updating Sites details,,>

-- =============================================
CREATE PROCEDURE [dbo].[spuSites] 

(
	@SiteId INT,
	@SiteName NVARCHAR(10),
	@SiteDesc NVARCHAR(255)
	
)	
	 
AS
BEGIN TRY
	BEGIN
		UPDATE Sites 
			SET SiteName = @SiteName,SiteDesc= @SiteDesc
			WHERE SiteId = @SiteId
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spuSchedule]    Script Date: 04/16/2015 17:45:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spuSchedule] 
(
	@ScheduleId uniqueidentifier,
	@Created_DateTime NVARCHAR(100),
	@Created_By NVARCHAR(250),
	@Occurance NVARCHAR(50),
	@Weekday NVARCHAR(50),
	@Start_DateTime NVARCHAR(100),
	@End_DateTime NVARCHAR(100),
	@Status NVARCHAR(50)
)	 
AS
BEGIN TRY
	BEGIN
	
	UPDATE [Schedule]
	   SET 
		  [ScheduleId] = @ScheduleId,
		  [Created_DateTime] = convert(datetime, @Created_DateTime, 121),
		  [Created_By] = @Created_By,
		  [Occurance] = @Occurance,
		  [Weekday] = @Weekday,
		  [Start_DateTime] = convert(datetime, @Start_DateTime, 121),
		  [End_DateTime] = convert(datetime, @End_DateTime, 121),
		  [Status] = @Status
	 WHERE ScheduleId = @ScheduleId 

	  Select  '1'--'Schedule updated successfully!'
	END
END TRY
BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spuRoles]    Script Date: 04/16/2015 17:45:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <07-Jul-2014>
-- Description:	<Deleting Roles>

-- =============================================
CREATE PROCEDURE [dbo].[spuRoles] 
(
	@RoleId INT,
	@RoleName NVARCHAR(100),
	@RoleDesc NVARCHAR(100)
)
	 
AS
BEGIN TRY
	BEGIN
	
	--declare @aa int
 --   set @aa = 10 / 0
	
		UPDATE Roles 
			SET RoleName =@RoleName,
				RoleDesc = 	@RoleDesc
		WHERE RoleId= @RoleId 
		
		Select '1'--'Role updated successfully!'
		
	END
END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spuPermissions]    Script Date: 04/16/2015 17:45:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Gagan Dhamija>
-- Create date: <08-Jul-2014>
-- Description:	<Update Permissions>

-- =============================================
CREATE PROCEDURE [dbo].[spuPermissions] 
(
	@PermissionId INT,
	@PermissionName NVARCHAR(100),
	@PermissionDesc NVARCHAR(100)
)
	 
AS
BEGIN TRY
	BEGIN
	
	--declare @aa int
 --   set @aa = 10 / 0
    	
		UPDATE Permissions 
			SET PermissionName = @PermissionName,
				PermissionDesc = @PermissionDesc
		  WHERE	PermissionId = @PermissionId
		  
		Select '1'--'Permission updated successfully!'
	END
END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO
/****** Object:  Table [dbo].[ValueListMap]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ValueListMap](
	[ApplicationId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](250) NOT NULL,
	[Label] [nvarchar](250) NULL,
	[internalValue] [nvarchar](250) NOT NULL,
	[Uri] [nvarchar](550) NULL,
 CONSTRAINT [PK_ValueListMap] PRIMARY KEY CLUSTERED 
(
	[ApplicationId] ASC,
	[Name] ASC,
	[internalValue] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RolePermissions]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RolePermissions](
	[RoleId] [int] NOT NULL,
	[PermissionId] [int] NOT NULL,
	[Active] [tinyint] NOT NULL,
 CONSTRAINT [PK_RolePermissions] PRIMARY KEY CLUSTERED 
(
	[PermissionId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ResourceGroups]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ResourceGroups](
	[ResourceId] [uniqueidentifier] NOT NULL,
	[GroupId] [int] NOT NULL,
	[Active] [tinyint] NOT NULL,
	[ResourceTypeId] [int] NOT NULL,
 CONSTRAINT [PK_ResourceGroups] PRIMARY KEY CLUSTERED 
(
	[ResourceId] ASC,
	[GroupId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Job]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Job](
	[JobId] [uniqueidentifier] NOT NULL,
	[ScheduleId] [uniqueidentifier] NOT NULL,
	[DataObjectId] [uniqueidentifier] NULL,
	[Is_Exchange] [tinyint] NOT NULL,
	[Xid] [nvarchar](50) NULL,
	[Cache_Page_size] [nvarchar](50) NULL,
	[PlatformId] [int] NOT NULL,
	[SiteId] [int] NOT NULL,
	[Next_Start_DateTime] [datetime] NULL,
	[Last_Start_DateTime] [datetime] NULL,
	[TotalRecords] [int] NULL,
	[CachedRecords] [int] NULL,
	[Active] [tinyint] NOT NULL,
 CONSTRAINT [PKJob] PRIMARY KEY CLUSTERED 
(
	[JobId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Dictionary]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Dictionary](
	[Dictionaryid] [uniqueidentifier] NOT NULL,
	[ApplicationID] [uniqueidentifier] NOT NULL,
	[IsDBDictionary] [bit] NOT NULL,
	[EnableSearch] [nvarchar](50) NULL,
	[EnableSummary] [nvarchar](50) NULL,
	[DataVersion] [nvarchar](50) NULL,
	[Provider] [nvarchar](50) NULL,
	[ConnectionString] [nvarchar](1000) NULL,
	[SchemaName] [nvarchar](50) NULL,
	[Description] [nvarchar](1000) NULL,
 CONSTRAINT [PK_Dictionary] PRIMARY KEY CLUSTERED 
(
	[Dictionaryid] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[GroupRoles]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GroupRoles](
	[GroupId] [int] NOT NULL,
	[RoleId] [int] NOT NULL,
	[Active] [tinyint] NOT NULL,
 CONSTRAINT [PK_GroupRoles] PRIMARY KEY CLUSTERED 
(
	[GroupId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Graphs]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Graphs](
	[ApplicationId] [uniqueidentifier] NOT NULL,
	[GraphId] [uniqueidentifier] NOT NULL,
	[GraphName] [nvarchar](255) NOT NULL,
	[Graph] [varbinary](max) NOT NULL,
	[Active] [tinyint] NOT NULL,
	[ExchangeVisible] [tinyint] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[GraphId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[GlobalSettings]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GlobalSettings](
	[SiteId] [int] NOT NULL,
	[PlatformId] [int] NOT NULL,
	[AdpaterServiceUri] [nvarchar](250) NULL,
	[DataServiceUri] [nvarchar](250) NULL,
	[HibernateServiceUri] [nvarchar](250) NULL,
	[FacadeServiceUri] [nvarchar](250) NULL,
	[AgentConfigServiceUri] [nvarchar](250) NULL,
	[SecurityUri] [nvarchar](250) NULL,
	[ApplicaionConfigServiceUri] [nvarchar](250) NULL,
	[DictionaryServiceUri] [nvarchar](250) NULL,
	[ExchangeManagerUri] [nvarchar](250) NULL,
	[XmlPath] [nvarchar](250) NULL,
	[Upload] [nvarchar](250) NULL,
	[Active] [nvarchar](250) NULL,
	[SsoUri] [nvarchar](250) NULL,
	[ClientId] [nvarchar](250) NULL,
	[ClientSecret] [nvarchar](250) NULL,
	[GrantType] [nvarchar](50) NULL,
	[RequestTimeout] [int] NULL,
 CONSTRAINT [PKGlobalSettings] PRIMARY KEY CLUSTERED 
(
	[SiteId] ASC,
	[PlatformId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Folders]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Folders](
	[FolderId] [uniqueidentifier] NOT NULL,
	[ParentFolderId] [uniqueidentifier] NOT NULL,
	[FolderName] [nvarchar](50) NOT NULL,
	[SiteId] [int] NOT NULL,
	[Active] [tinyint] NOT NULL,
	[PlatformId] [int] NOT NULL,
 CONSTRAINT [PK__Folders__ACD7107F2B3F6F97] PRIMARY KEY CLUSTERED 
(
	[FolderId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[BindingConfig]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BindingConfig](
	[ApplicationId] [uniqueidentifier] NOT NULL,
	[ModuleName] [nvarchar](250) NOT NULL,
	[BindName] [nvarchar](250) NOT NULL,
	[Service] [nvarchar](650) NOT NULL,
	[To] [nvarchar](650) NOT NULL,
	[Active] [tinyint] NOT NULL,
 CONSTRAINT [PK_BindingConfig] PRIMARY KEY CLUSTERED 
(
	[ApplicationId] ASC,
	[ModuleName] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ApplicationSettings]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ApplicationSettings](
	[ApplicationId] [uniqueidentifier] NOT NULL,
	[settingId] [int] NOT NULL,
	[settingValue] [nvarchar](550) NULL,
 CONSTRAINT [PK_ApplicationSettings] PRIMARY KEY CLUSTERED 
(
	[ApplicationId] ASC,
	[settingId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Contexts]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Contexts](
	[ContextId] [uniqueidentifier] NOT NULL,
	[DisplayName] [nvarchar](255) NOT NULL,
	[InternalName] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](255) NOT NULL,
	[CacheConnStr] [nvarchar](255) NOT NULL,
	[Active] [tinyint] NOT NULL,
	[FolderId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK__Contexts__23A237491BFD2C07] PRIMARY KEY CLUSTERED 
(
	[ContextId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[spuJob]    Script Date: 04/16/2015 17:45:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spuJob] 
(
	@JobId uniqueidentifier,
	@ScheduleId uniqueidentifier,
	@DataObjectId uniqueidentifier,
	@Is_Exchange INT,
	@Xid NVARCHAR(50),
	@Cache_Page_Size NVARCHAR(50),
	@PlatformId int,
	@SiteId int,
	@Next_Start_DateTime NVARCHAR(100),
	@Last_Start_DateTime NVARCHAR(100),
	@TotalRecords int,
	@CachedRecords int,
	@Active tinyint,
	@Schedules xml
)	 
AS
BEGIN TRY
	BEGIN
	
	UPDATE [Job]
	   SET 
		  [ScheduleId] = @ScheduleId,
		  [DataObjectId] = @DataObjectId,
		  [Is_Exchange] = @Is_Exchange,
		  [Xid] = @Xid,
		  [Cache_Page_Size] = @Cache_Page_Size,
		  [PlatformId] = @PlatformId,
		  [SiteId] = @SiteId,
		  [Next_Start_DateTime] = convert(datetime, @Next_Start_DateTime, 121),
		  [Last_Start_DateTime] = convert(datetime, @Last_Start_DateTime, 121),
		  [TotalRecords] = @TotalRecords,
		  [CachedRecords] = @CachedRecords,
		  [Active] = @Active
	WHERE JobId = @JobId 
	 
	Declare @Schedule_Id uniqueidentifier
	SET @ScheduleId =  (SELECT  T.N.value('(Schedule_Id)[1]', 'NVARCHAR(250)') as Schedule_Id from @Schedules.nodes('schedules/schedule') as T(N))
	
	 --update table schedule 
	 UPDATE [schedule]
		SET 
		Created_DateTime = convert(datetime, T.N.value('(created_DateTime)[1]', 'NVARCHAR(100)'), 121),
		Created_By = T.N.value('(created_By)[1]', 'NVARCHAR(250)'),
		Occurance = T.N.value('(occurance)[1]', 'NVARCHAR(50)'),
		Weekday = T.N.value('(weekday)[1]', 'NVARCHAR(50)'),
		Start_DateTime = convert(datetime, T.N.value('(start_DateTime)[1]', 'NVARCHAR(100)'), 121), 
		End_DateTime = convert(datetime, T.N.value('(end_DateTime)[1]', 'NVARCHAR(100)'), 121),
		Status = T.N.value('(status)[1]', 'NVARCHAR(50)') 
		from @Schedules.nodes('schedules/schedule') as T(N)	
		WHERE ScheduleId = @ScheduleId 
		
	
	  Select  '1'--'Job updated successfully!'
	END
END TRY
BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO
/****** Object:  Table [dbo].[PickList]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PickList](
	[DictionaryId] [uniqueidentifier] NOT NULL,
	[PickListId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Description] [nvarchar](250) NULL,
	[ValuePropertyIndex] [nvarchar](50) NULL,
	[TableName] [nvarchar](50) NULL,
 CONSTRAINT [PK_PickList] PRIMARY KEY CLUSTERED 
(
	[PickListId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DataObjects]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DataObjects](
	[DataObjectId] [uniqueidentifier] NOT NULL,
	[DictionaryId] [uniqueidentifier] NOT NULL,
	[TableName] [nvarchar](50) NOT NULL,
	[ObjectNameSpace] [nvarchar](100) NULL,
	[ObjectName] [nvarchar](50) NULL,
	[KeyDelimeter] [nvarchar](50) NULL,
	[Description] [nvarchar](200) NULL,
	[IsReadOnly] [bit] NULL,
	[HasContent] [bit] NULL,
	[IsListOnly] [bit] NULL,
	[DefaultProjectionFormat] [nvarchar](200) NULL,
	[DefaultListProjectionFormat] [nvarchar](200) NULL,
	[IsRelatedOnly] [bit] NULL,
	[GroupName] [nvarchar](200) NULL,
	[Version] [nvarchar](100) NULL,
	[IsHidden] [bit] NULL,
 CONSTRAINT [PK_DataObjects] PRIMARY KEY CLUSTERED 
(
	[DataObjectId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[spdJob]    Script Date: 04/16/2015 17:44:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE  PROCEDURE [dbo].[spdJob] 
(
	@JobId uniqueidentifier
)
AS
BEGIN TRY
	BEGIN
		UPDATE [Job]
		SET Active = 0
		WHERE JobId = @JobId
		
	  -- delete from child tables
	  --declare @ScheduleId uniqueidentifier
	  --SET @ScheduleId = (select ScheduleId from Job where JobId = @JobId)
	  
	  --DELETE from Job where JobId = @JobId
	  
	  --DELETE From Schedule where ScheduleId = @ScheduleId
	  
	  Select  '1'--'Job Deleted updated successfully!'
	END
		
END TRY
BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spdGroups]    Script Date: 04/16/2015 17:44:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Gagan Dhamija>
-- Create date: <10-Jul-2014>
-- Description:	<Deleting Groups>

-- =============================================
CREATE PROCEDURE [dbo].[spdGroups] 
(
	@GroupId INT
)	
	 
AS
BEGIN TRY
	BEGIN
		
		--declare @aa int
		--set @aa = 10 / 0
		
		UPDATE Groups SET Active = 0 WHERE GroupId= @GroupId
		
		UPDATE UserGroups SET Active = 0 WHERE GroupId = @GroupId
		
		UPDATE GroupRoles SET Active = 0 WHERE GroupId = @GroupId
 
		Select '1'--'Group deleted successfully!'
		
	END
END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spdRoles]    Script Date: 04/16/2015 17:44:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <07-Jul-2014>
-- Description:	<Deleting Roles>

-- =============================================
CREATE PROCEDURE [dbo].[spdRoles] 
(
	@RoleId INT
)	
	 
AS
BEGIN TRY
	BEGIN
	
	--declare @aa int
	--	set @aa = 10 / 0
		
		UPDATE Roles SET Active = 0 WHERE RoleId = @RoleId
		
		UPDATE GroupRoles SET Active = 0 WHERE RoleId = @RoleId
		
		UPDATE RolePermissions SET Active = 0 WHERE RoleId = @RoleId
		
		Select '1'--'Role deleted successfully!'
		
	END
END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spdPermissions]    Script Date: 04/16/2015 17:44:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Gagan Dhamija>
-- Create date: <09-Jul-2014>
-- Description:	<Deleting Permissions>

-- =============================================
CREATE PROCEDURE [dbo].[spdPermissions] 
(
	@PermissionId INT
)	
	 
AS
BEGIN TRY
	BEGIN
	
	 --   declare @aa int
		--set @aa = 10 / 0
		
		UPDATE [Permissions] SET Active = 0 WHERE PermissionId = @PermissionId
		
		UPDATE RolePermissions SET Active = 0 WHERE PermissionId = @PermissionId
		
		Select '1'--'Role deleted successfully!'
		
	END
END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spgGroupRoles]    Script Date: 04/16/2015 17:44:36 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <19-June-2014>
-- Description:	<Getting group roles>

-- =============================================
CREATE  PROCEDURE [dbo].[spgGroupRoles]
(
	@GroupId INT,
	@RoleId INT
)

 
AS
BEGIN TRY
	BEGIN
		SELECT GroupId,RoleId FROM GroupRoles WHERE Active=1 AND GroupId=@GroupId AND RoleId=@RoleId
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spgGraphMappingByUser]    Script Date: 04/16/2015 17:44:35 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Gagan Dhamija>
-- Create date: <27-Aug-2014>
-- Description:	<Selecting Graphs based on users>
-- ===========================================================
CREATE PROCEDURE [dbo].[spgGraphMappingByUser] 
(
	@UserName varchar(100),
	@SiteId INT,
	@GraphId uniqueidentifier
)	 
AS
BEGIN TRY
	BEGIN
	    
    WITH XMLNAMESPACES (DEFAULT 'http://www.iringtools.org/library') 
    select g.GraphId as graphId, g.graphName as graphName,g.graph as graphObject from Graphs g
    inner join ResourceGroups crg on crg.siteid = g.siteid and crg.active = g.active 
    inner join Groups cg on cg.groupId = crg.groupId and cg.siteid = g.siteid and cg.active = g.active
    inner join grouproles cgr on  cgr.groupid = crg.groupid and cgr.siteid = g.siteid and cgr.active = g.active    
    inner join roles cr on cr.roleid = cgr.roleid and cr.siteid = g.siteid and cr.active = g.active         
    inner join usergroups cug on cug.groupid = crg.groupid and cug.siteid = g.siteid and cug.active = g.active
    inner join users cu on cu.userid = cug.userid and cu.username = @UserName and cu.siteid = g.siteid and cu.active = g.active
    where g.graphId = @GraphId and g.siteid = @SiteId and g.active = 1
    Group BY g.GraphId, g.graphName, g.graph  
    for xml PATH('graph'), ROOT('graphs'), type, ELEMENTS XSINIL
	
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spgGraphByUser]    Script Date: 04/16/2015 17:44:35 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Gagan Dhamija>
-- Create date: <27-Aug-2014>
-- Description:	<Selecting Graphs based on users>
-- ===========================================================
CREATE PROCEDURE [dbo].[spgGraphByUser] 
(
	@UserName varchar(100),
	@ApplicationId uniqueidentifier
)	 
AS
BEGIN
	    
    WITH XMLNAMESPACES (DEFAULT 'http://www.iringtools.org/library') 
    select 
		g.ApplicationId as applicationId, 
		g.GraphId as graphId, 
		g.graphName as graphName, 
		g.Active as active,
		(
			select 
			distinct  
			p.PermissionId as permissionId, 
			p.PermissionName as permissionName, 
			p.PermissionDesc as permissionDesc, 
			p.Active as active from 
			permissions p
			inner join ResourceGroups rg on  rg.active = g.active and rg.resourceid = g.graphId 
			inner join Groups g on g.groupId = rg.groupId  and g.active = g.active
			inner join grouproles gr on  gr.groupid = rg.groupid and  gr.active = g.active    
			inner join roles r on r.roleid = gr.roleid and  r.active = g.active     
			inner join rolepermissions rp on rp.permissionid = p.permissionid and rp.roleid = r.roleid and  rp.active = g.active          
			inner join usergroups ug on ug.groupid = rg.groupid   and ug.active = g.active
			inner join users u on u.userid = ug.userid and u.username = @UserName  and u.active = g.active
			where g.active = 1   for xml PATH('permission'), type 
		) as 'permissions',
		(
			Select gg.groupId,gg.groupName from ResourceGroups rr inner join Groups gg on rr.GroupId = gg.GroupId 
			where rr.ResourceId = g.graphId 
			and rr.Active = 1
			and gg.Active = 1 
			for xml PATH('group'), type
		)  as 'groups'
    from Graphs g
    inner join ResourceGroups crg on  crg.active = g.active and crg.resourceid = g.graphId
    inner join Groups cg on cg.groupId = crg.groupId and cg.active = g.active
    inner join grouproles cgr on  cgr.groupid = crg.groupid and cgr.active = g.active    
    inner join roles cr on cr.roleid = cgr.roleid and cr.active = g.active         
    inner join usergroups cug on cug.groupid = crg.groupid and  cug.active = g.active
    inner join users cu on cu.userid = cug.userid and cu.username = @UserName and cu.active = g.active
    where g.applicationId = @ApplicationId and  g.active = 1
    Group BY g.ApplicationId, g.GraphId, g.graphName, g.Active
    for xml PATH('graph'), ROOT('graphs'), type, ELEMENTS XSINIL
	
END
GO
/****** Object:  StoredProcedure [dbo].[spgGraphByGraphID]    Script Date: 04/16/2015 17:44:34 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Deepak Kumar>
-- Create date: <11-Mar-2015>
-- Description:	<Selecting Graphs based on graphid>
-- ===========================================================
CREATE PROCEDURE [dbo].[spgGraphByGraphID] --'vmallire', 1,'54D581EF-64BC-45C2-9DF5-0255F7454E0E'
(
	@GraphId uniqueidentifier,-- = '1FAC1B09-6275-4079-AF57-01FE46BD22BA'
	@UserName nvarchar(100)
)	 
AS
BEGIN
	    
    WITH XMLNAMESPACES (DEFAULT 'http://www.iringtools.org/library') 
    select 
		GRP.ApplicationId as applicationId, 
		GRP.GraphId as graphId, 
		GRP.graphName as graphName,
		GRP.Graph as graphObject,
		GRP.Active as active,
		(
			select 
			distinct  
			p.PermissionId as permissionId, 
			p.PermissionName as permissionName, 
			p.PermissionDesc as permissionDesc, 
			p.Active as active from 
			ResourceGroups 
				RG inner join Groups G on RG.GroupId = G.GroupId 
							And RG.Active = 1 
							And G.Active = 1
							And RG.ResourceId = GRP.GraphId--'1FAC1B09-6275-4079-AF57-01FE46BD22BA'
				   inner join UserGroups UG on UG.GroupId = RG.GroupId
							And UG.Active = 1
				   inner join Users U on U.UserId = UG.UserId
							And U.Active = 1
							And U.UserName = @UserName					
				   inner join GroupRoles GR on GR.GroupId = UG.GroupId
							And GR.Active = 1
				   inner join Roles R on R.RoleId = GR.RoleId
							And R.Active = 1
				   inner join RolePermissions RP on RP.RoleId = R.RoleId
							And RP.Active = 1
				   inner join [Permissions] P on P.PermissionId = RP.PermissionId
							And P.Active = 1 for xml PATH('permission'), type 
		) as 'permissions',
		(
			Select gg.groupId,gg.groupName from 
					ResourceGroups rr inner join Groups gg on rr.GroupId = gg.GroupId 
										And rr.ResourceId = GRP.graphId
										and rr.Active = 1
										and gg.Active = 1 						
								inner join UserGroups UG1 on UG1.GroupId = rr.GroupId
										And UG1.Active = 1
								inner join Users U1 on U1.UserId = UG1.UserId
									And U1.Active = 1
									And U1.UserName = @UserName					
			
			for xml PATH('group'), type
		)  as 'groups'
    from Graphs GRP Where GRP.GraphId = @GraphId
    for xml PATH('graph')--, ROOT('graphs'), type, ELEMENTS XSINIL
	
END
GO
/****** Object:  StoredProcedure [dbo].[spgGraphBinary]    Script Date: 04/16/2015 17:44:34 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ================================================================
-- Author:		 Gagan Dhamija
-- Create date:  18-Sep-2014
-- Modified By:  Deepak Kuamr
-- Modified date:04-Apr-2015
-- Description:	<Selecting Graphs based on username and graphid
-- ================================================================
CREATE PROCEDURE [dbo].[spgGraphBinary] --'vmallire','8784E696-ED17-4C37-A86B-634C55326378'
(
	@UserName varchar(100),
	@GraphId uniqueidentifier
)	 
AS
BEGIN TRY
	BEGIN
	    
    select
	   Distinct	g.graph as graphObject 
	from Graphs G  Inner join ResourceGroups RG on G.GraphId = RG.ResourceId 
										    and G.GraphId = @GraphId
										    and RG.Active = 1
								  Inner join UserGroups UG on UG.GroupId = rg.GroupId
											and UG.Active = 1
								  Inner join Groups GP on UG.GroupId = GP.GroupId
								            and G.Active = 1  
								  Inner join Users U1 on U1.UserId = UG.UserId 
								            and U1.Active = 1
								            and U1.UserName = @UserName	
    --inner join ResourceGroups crg on crg.siteid = g.siteid and crg.active = g.active 
    --inner join Groups cg on cg.groupId = crg.groupId and cg.siteid = g.siteid and cg.active = g.active
    --inner join grouproles cgr on  cgr.groupid = crg.groupid and cgr.siteid = g.siteid and cgr.active = g.active    
    --inner join roles cr on cr.roleid = cgr.roleid and cr.siteid = g.siteid and cr.active = g.active         
    --inner join usergroups cug on cug.groupid = crg.groupid and cug.siteid = g.siteid and cug.active = g.active
    --inner join users cu on cu.userid = cug.userid and cu.username = @UserName and cu.siteid = g.siteid and cu.active = g.active
    --where UPPER(g.graphId) = UPPER(@GraphId) and g.siteid = @SiteId and g.active = 1
    
    --Group BY g.GraphId, g.graphName, g.graph  
	
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spgGraph]    Script Date: 04/16/2015 17:44:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Gagan Dhamija>
-- Create date: <15-July-2014>
-- Description:	<Getting All Graphs>

-- =============================================
CREATE  PROCEDURE [dbo].[spgGraph] 
 
AS
BEGIN TRY
	BEGIN
		SELECT ApplicationId,GraphId,GraphName,Graph,SiteId FROM Graphs WHERE Active=1
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spgFolderByUser]    Script Date: 04/16/2015 17:44:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Abhishek Pandey>
-- Create date: <18-Mar-2015>
-- Description:	<Selecting Group based on users>
-- ===========================================================
CREATE PROCEDURE [dbo].[spgFolderByUser]
(
	@UserName VARCHAR(100),
	@SiteId INT,
	@PlatformId INT,
	@ParentFolderOrSiteId UNIQUEIDENTIFIER
)	 
AS
BEGIN TRY
	BEGIN
		BEGIN TRANSACTION;
		
		IF @PlatformId = 3		
		BEGIN
			WITH XMLNAMESPACES ( DEFAULT 'http://www.iringtools.org/library')
			SELECT 
				f.FolderId AS folderId, 
				f.ParentFolderId AS parentFolderId, 
				f.FolderName AS folderName, 
				f.SiteId AS siteId,
				f.PlatformId AS platformId, 
				f.Active AS active, 
				(
					SELECT DISTINCT
						p.permissionId AS permissionId,
						p.permissionName AS permissionName, 
						p.permissionDesc AS permissionDesc, 
						p.Active AS active 
					FROM Permissions p
						INNER JOIN ResourceGroups rg ON rg.active = f.active AND rg.resourceid = f.folderId 
						INNER JOIN Groups g ON g.groupId = rg.groupId AND g.active = f.active
						INNER JOIN grouproles gr ON  gr.groupid = rg.groupid AND gr.active = f.active    
						INNER JOIN roles r ON r.roleid = gr.roleid AND r.active = f.active     
						INNER JOIN rolepermissions rp ON rp.permissionid = p.permissionid AND rp.roleid = r.roleid AND rp.active = f.active          
						INNER JOIN usergroups ug ON ug.groupid = rg.groupid AND ug.active = f.active
						INNER JOIN users u ON u.userid = ug.userid AND u.username = @UserName AND u.active = f.active
					WHERE f.parentFolderId = @ParentFolderOrSiteId AND  f.siteid = @SiteId AND f.platformid IN (1, 2) AND f.active = 1   FOR XML PATH('permission'), TYPE 
				) AS 'permissions',
				(
					SELECT
						gg.groupId,
						gg.groupName
					FROM ResourceGroups rr
						INNER JOIN Groups gg ON rr.GroupId = gg.GroupId
						INNER JOIN UserGroups ug ON ug.GroupId = gg.GroupId
						INNER JOIN Users u ON u.UserId = ug.UserId
					WHERE rr.ResourceId = f.FolderId
						AND  u.UserName = @UserName
						AND ug.Active = 1
						AND u.Active = 1
						AND rr.Active = 1
						AND gg.Active = 1  
					FOR XML PATH('group'), TYPE
				) AS 'groups'	  
			FROM Folders f
				INNER JOIN ResourceGroups crg ON crg.active = f.active AND crg.resourceid = f.folderId
				INNER JOIN Groups cg ON cg.groupId = crg.groupId AND cg.active = f.active
				INNER JOIN grouproles cgr ON  cgr.groupid = crg.groupid AND cgr.active = f.active    
				INNER JOIN roles cr ON cr.roleid = cgr.roleid AND cr.active = f.active         
				INNER JOIN usergroups cug ON cug.groupid = crg.groupid AND cug.active = f.active
				INNER JOIN users cu ON cu.userid = cug.userid AND cu.active = f.active
			WHERE f.parentFolderId = @ParentFolderOrSiteId AND f.siteid = @SiteId AND f.active = 1 AND f.platformId IN (1, 2) AND cu.username = @UserName
			Group BY f.FolderId, f.ParentFolderId, f.FolderName, f.SiteId, f.PlatformId, f.Active
			FOR XML PATH('folder'), ROOT('folders'), TYPE, ELEMENTS XSINIL
		END
		ELSE
			WITH XMLNAMESPACES ( DEFAULT 'http://www.iringtools.org/library')
			SELECT 
				f.FolderId AS folderId, 
				f.ParentFolderId AS parentFolderId, 
				f.FolderName AS folderName, 
				f.SiteId AS siteId,
				f.PlatformId AS platformId, 
				f.Active AS active, 
				(
					SELECT DISTINCT  
						p.permissionId AS permissionId, 
						p.permissionName AS permissionName, 
						p.permissionDesc AS permissionDesc, 
						p.Active AS active 
					FROM permissions p
						INNER JOIN ResourceGroups rg ON rg.active = f.active AND rg.resourceid = f.folderId 
						INNER JOIN Groups g ON g.groupId = rg.groupId AND g.active = f.active
						INNER JOIN grouproles gr ON  gr.groupid = rg.groupid AND gr.active = f.active    
						INNER JOIN roles r ON r.roleid = gr.roleid AND r.active = f.active     
						INNER JOIN rolepermissions rp ON rp.permissionid = p.permissionid AND rp.roleid = r.roleid AND rp.active = f.active          
						INNER JOIN usergroups ug ON ug.groupid = rg.groupid AND ug.active = f.active
						INNER JOIN users u ON u.userid = ug.userid AND u.username = @UserName AND u.active = f.active
					WHERE f.parentFolderId = @ParentFolderOrSiteId AND  f.siteid = @SiteId AND f.platformid = @PlatformId AND f.active = 1   FOR XML PATH('permission'), TYPE 
				) AS 'permissions',
				(
				SELECT
					gg.groupId,
					gg.groupName
				FROM ResourceGroups rr
					INNER JOIN Groups gg ON rr.GroupId = gg.GroupId
					INNER JOIN UserGroups ug ON ug.GroupId = gg.GroupId
					INNER JOIN Users u ON u.UserId = ug.UserId
				WHERE rr.ResourceId = f.FolderId
					AND  u.UserName = @UserName
					AND ug.Active = 1
					AND u.Active = 1
					AND rr.Active = 1
					AND gg.Active = 1 
				FOR XML PATH('group'), TYPE
				)  AS 'groups'
			FROM Folders f
				INNER JOIN ResourceGroups crg ON crg.active = f.active AND crg.resourceid = f.folderId
				INNER JOIN Groups cg ON cg.groupId = crg.groupId AND cg.active = f.active
				INNER JOIN grouproles cgr ON  cgr.groupid = crg.groupid AND cgr.active = f.active    
				INNER JOIN roles cr ON cr.roleid = cgr.roleid AND cr.active = f.active         
				INNER JOIN usergroups cug ON cug.groupid = crg.groupid AND cug.active = f.active
				INNER JOIN users cu ON cu.userid = cug.userid AND cu.active = f.active
			WHERE f.parentFolderId = @ParentFolderOrSiteId AND f.siteid = @SiteId AND f.active = 1 AND f.platformId = @PlatformId AND cu.username = @UserName
			Group BY f.FolderId, f.ParentFolderId, f.FolderName, f.SiteId, f.PlatformId, f.Active
			FOR XML PATH('folder'), ROOT('folders'), TYPE, ELEMENTS XSINIL
		
		COMMIT TRANSACTION;
	END
END TRY
BEGIN CATCH
	ROLLBACK TRANSACTION;
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spgAllJobs]    Script Date: 04/16/2015 17:44:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE  PROCEDURE [dbo].[spgAllJobs] 
(
	@UserName NVARCHAR(100),
	@SiteId int,
	@PlatformId int,
	@IsExchange int
)
AS
BEGIN TRY
	BEGIN
	
		DECLARE @isXSitesAdminGroupMember BIT

		SET @isXSitesAdminGroupMember = (
		SELECT
			CASE
				WHEN COUNT(*) = 0 
				THEN 0 
				ELSE 1 
			END
		FROM Groups
		INNER JOIN UserGroups ON UserGroups.GroupId = Groups.GroupId
		INNER JOIN Users ON Users.UserId = UserGroups.UserId
		WHERE Users.UserName = @UserName
		AND Groups.GroupName = 'XSitesAdminGroup'
		AND Users.Active = 1
		AND UserGroups.Active = 1
		AND Groups.Active = 1)
		
	  
		IF @isXSitesAdminGroupMember = 1
			BEGIN
				IF @IsExchange = 0 
					BEGIN
						WITH XMLNAMESPACES ( DEFAULT 'http://www.iringtools.org/library')
						select distinct(
						SELECT 
							j.JobId as jobId,
							j.ScheduleId as scheduleId,
							j.DataObjectId as dataObjectId,
							j.Is_Exchange as isExchange,
							j.Xid as xid,
							j.Cache_Page_Size as cache_page_size,
							j.PlatformId as platformId,
							j.SiteId as siteId,
							j.Next_Start_DateTime as next_Start_DateTime,
							j.Last_Start_DateTime as last_Start_DateTime,
							j.TotalRecords as totalRecords,
							j.CachedRecords as cachedRecords,
							j.Active as Active,
							(
							  select 
								s.scheduleid as scheduleId,
								s.created_datetime as created_DateTime, 
								s.Created_By as created_By, 
								s.occurance as occurance, 
								s.[weekday] as weekday, 
								s.start_datetime as start_DateTime, 
								s.end_datetime as end_DateTime, 
								s.[status] as status
								from schedule s
								where j.scheduleid = s.scheduleid for xml PATH('schedule'), type
							) as 'schedules'
							from job j
							where j.Active = 1 and j.platformid = @PlatformId and j.Is_Exchange = 0
							for xml PATH('job')) from job  for xml PATH('jobs') , type, ELEMENTS XSINIL
					END
				ELSE
					WITH XMLNAMESPACES ( DEFAULT 'http://www.iringtools.org/library')
						select distinct(
						SELECT 
							j.JobId as jobId,
							j.ScheduleId as scheduleId,
							j.DataObjectId as dataObjectId,
							j.Is_Exchange as isExchange,
							j.Xid as xid,
							j.Cache_Page_Size as cache_page_size,
							j.PlatformId as platformId,
							j.SiteId as siteId,
							j.Next_Start_DateTime as next_Start_DateTime,
							j.Last_Start_DateTime as last_Start_DateTime,
							j.TotalRecords as totalRecords,
							j.CachedRecords as cachedRecords,
							j.Active as Active,
							(
							  select 
								s.scheduleid as scheduleId,
								s.created_datetime as created_DateTime, 
								s.Created_By as created_By, 
								s.occurance as occurance, 
								s.[weekday] as weekday, 
								s.start_datetime as start_DateTime, 
								s.end_datetime as end_DateTime, 
								s.[status] as status
								from schedule s
								where j.scheduleid = s.scheduleid for xml PATH('schedule'), type
							) as 'schedules'
							from job j
							where j.Active = 1 and j.platformid = 3 and j.Is_Exchange = 1
							for xml PATH('job')) from job  for xml PATH('jobs') , type, ELEMENTS XSINIL
						
			END
		ELSE
			IF @IsExchange = 0 
				Begin
					if @PlatformId = 3
						Begin
							WITH XMLNAMESPACES ( DEFAULT 'http://www.iringtools.org/library')   
							select distinct(
								SELECT 
								j.JobId as jobId,
								j.ScheduleId as scheduleId,
								j.DataObjectId as dataObjectId,
								j.Is_Exchange as isExchange,
								j.Xid as xid,
								j.Cache_Page_Size as cache_page_size,
								j.PlatformId as platformId,
								j.SiteId as siteId,
								j.Next_Start_DateTime as next_Start_DateTime,
								j.Last_Start_DateTime as last_Start_DateTime,
								j.TotalRecords as totalRecords,
								j.CachedRecords as cachedRecords,
								j.Active as Active,
								(
								  select 
									s.scheduleid as scheduleId,
									s.created_datetime as created_DateTime, 
									s.Created_By as created_By, 
									s.occurance as occurance, 
									s.[weekday] as weekday, 
									s.start_datetime as start_DateTime, 
									s.end_datetime as end_DateTime, 
									s.[status] as status
									from schedule s
									where j.scheduleid = s.scheduleid for xml PATH('schedule'), type
								) as 'schedules'
								from job j
								where j.PlatformId = 1 and j.PlatformId = 2 and j.Is_Exchange = 0 and j.Active = 1 
								for xml PATH('job')) from job  for xml PATH('jobs') , type, ELEMENTS XSINIL
						End
					Else 
						Begin
							WITH XMLNAMESPACES ( DEFAULT 'http://www.iringtools.org/library')   
							select distinct(
								SELECT 
								j.JobId as jobId,
								j.ScheduleId as scheduleId,
								j.DataObjectId as dataObjectId,
								j.Is_Exchange as isExchange,
								j.Xid as xid,
								j.Cache_Page_Size as cache_page_size,
								j.PlatformId as platformId,
								j.SiteId as siteId,
								j.Next_Start_DateTime as next_Start_DateTime,
								j.Last_Start_DateTime as last_Start_DateTime,
								j.TotalRecords as totalRecords,
								j.CachedRecords as cachedRecords,
								j.Active as Active,
								(
								  select 
									s.scheduleid as scheduleId,
									s.created_datetime as created_DateTime, 
									s.Created_By as created_By, 
									s.occurance as occurance, 
									s.[weekday] as weekday, 
									s.start_datetime as start_DateTime, 
									s.end_datetime as end_DateTime, 
									s.[status] as status
									from schedule s
									where j.scheduleid = s.scheduleid for xml PATH('schedule'), type
								) as 'schedules'
								from job j
								where j.PlatformId = @PlatformId and j.Is_Exchange = 0 and j.Active = 1 
								for xml PATH('job')) from job  for xml PATH('jobs') , type, ELEMENTS XSINIL
						End
				End		
			Else
				Begin
					If @PlatformId = 3
						Begin
							WITH XMLNAMESPACES ( DEFAULT 'http://www.iringtools.org/library')   
							select distinct(
								SELECT 
								j.JobId as jobId,
								j.ScheduleId as scheduleId,
								j.DataObjectId as dataObjectId,
								j.Is_Exchange as isExchange,
								j.Xid as xid,
								j.Cache_Page_Size as cache_page_size,
								j.PlatformId as platformId,
								j.SiteId as siteId,
								j.Next_Start_DateTime as next_Start_DateTime,
								j.Last_Start_DateTime as last_Start_DateTime,
								j.TotalRecords as totalRecords,
								j.CachedRecords as cachedRecords,
								j.Active as Active,
								(
								  select 
									s.scheduleid as scheduleId,
									s.created_datetime as created_DateTime, 
									s.Created_By as created_By, 
									s.occurance as occurance, 
									s.[weekday] as weekday, 
									s.start_datetime as start_DateTime, 
									s.end_datetime as end_DateTime, 
									s.[status] as status
									from schedule s
									where j.scheduleid = s.scheduleid for xml PATH('schedule'), type
								) as 'schedules'
								from job j
								where j.SiteId = @SiteId and j.PlatformId = @PlatformId and j.Is_Exchange = 1 and j.Active = 1 
								for xml PATH('job')) from job  for xml PATH('jobs') , type, ELEMENTS XSINIL
						End	
				
			End
			 			
	END
END TRY
BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spdUser]    Script Date: 04/16/2015 17:44:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <07-Jul-2014>
-- Description:	<Deleting user>

-- =============================================
CREATE PROCEDURE [dbo].[spdUser] 
(
	@UserName NVARCHAR(100)
)	
	 
AS
BEGIN TRY
	BEGIN
		
		--declare @aa int
		--set @aa = 10 / 0
		
		Declare @UserId Int 
		
		Select @UserId = UserId from users WHERE UserName = @UserName And Active = 1
		
		UPDATE Users SET Active = 0 WHERE UserId = @UserId
		
		UPDATE UserGroups SET Active = 0 WHERE UserId = @UserId
		
		Select '1'--'User deleted successfully!'
		
	END
END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spgApplicationByUser]    Script Date: 04/16/2015 17:44:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Gagan Dhamija>
-- Create date: <24-July-2014>
-- Description:	<Selecting Applications based ON users>
-- Last Modified By: <Abhishek Pandey>
-- Modification Date: <07-Apr-2015>
-- ===========================================================
CREATE PROCEDURE [dbo].[spgApplicationByUser]
(
	@UserName VARCHAR(100),
	@ContextId UNIQUEIDENTIFIER
)	 
AS
BEGIN TRY
	BEGIN
	
	 WITH XMLNAMESPACES (DEFAULT 'http://www.iringtools.org/library') 
     SELECT  
		 a.ContextId AS contextId , 
		 a.ApplicationId AS applicationId , 
		 a.DisplayName AS displayName,
		 a.InternalName AS internalName,
		 a.[Description] AS [description],
		 a.DXFRUrl AS dxfrUrl,
		 a.Active AS active,
		 a.[Assembly] AS [assembly],
		 (
		 SELECT DISTINCT  
			 p.PermissionId AS permissionId,  
			 p.PermissionName AS permissionName, 
			 p.PermissionDesc AS permissionDesc, 
			 p.Active AS active 
		 FROM permissions p
		 INNER JOIN ResourceGroups rg ON  rg.active = a.active AND rg.resourceid = a.ApplicationId
		 INNER JOIN Groups g ON g.groupId = rg.groupId  AND g.active = a.active
		 INNER JOIN grouproles gr ON  gr.groupid = rg.groupid AND  gr.active = a.active    
		 INNER JOIN roles r ON r.roleid = gr.roleid AND  r.active = a.active     
		 INNER JOIN rolepermissions rp ON rp.permissionid = p.permissionid AND rp.roleid = r.roleid AND rp.active = a.active          
		 INNER JOIN usergroups ug ON ug.groupid = rg.groupid  AND  ug.active = a.active
		 INNER JOIN users u ON u.userid = ug.userid AND u.username = @UserName AND  u.active = a.active
		 WHERE a.ContextId = @ContextId AND a.active = 1   FOR XML PATH('permission'), TYPE 
		 ) AS 'permissions',
		 (
		 SELECT 
			 DISTINCT 
			 s.Name AS [key],
			 appset.SettingValue AS value 
		 FROM ApplicationSettings appSet
		 INNER JOIN Settings s  ON s.SettingId = appset.SettingID 
		 WHERE appSet.ApplicationId = a.ApplicationId FOR XML PATH('setting'), TYPE 
		 ) AS 'applicationSettings/add',
		(
			SELECT gg.groupId,gg.groupName FROM ResourceGroups rr INNER JOIN Groups gg ON rr.GroupId = gg.GroupId 
			WHERE rr.ResourceId = a.ApplicationId
			AND rr.Active = 1
			AND gg.Active = 1 
			FOR XML PATH('group'), TYPE
		) AS 'groups',
		a.DataMode AS applicationDataMode,
		(
			SELECT
				ModuleName AS moduleName,
				BindName AS bindName,
				[Service] AS [service],
				[To] AS [to]
			FROM BindingConfig
			WHERE ApplicationId = a.ApplicationId
			AND Active = 1
			FOR XML PATH('applicationBinding'), TYPE
		)
     FROM  Applications a
     INNER JOIN ResourceGroups arg ON arg.active = a.active AND arg.resourceid = a.ApplicationId
     INNER JOIN Groups ag ON ag.groupId = arg.groupId  AND ag.active = a.active
     INNER JOIN grouproles agr ON  agr.groupid = arg.groupid AND agr.active = a.active    
     INNER JOIN roles ar ON ar.roleid = agr.roleid AND  ar.active = a.active         
     INNER JOIN usergroups aug ON aug.groupid = arg.groupid AND aug.active = a.active
     INNER JOIN users au ON au.userid = aug.userid AND au.username = @UserName AND  au.active = a.active
     WHERE a.ContextId = @ContextId AND   a.active = 1	 
	 Group BY  a.ContextId , a.ApplicationId , a.DisplayName,a.InternalName,a.Description,
	 a.DXFRUrl,a.Active,a.Assembly, a.DataMode
	 FOR XML PATH('application'), ROOT('applications'), TYPE, ELEMENTS XSINIL
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spgApplicationByApplicationID]    Script Date: 04/16/2015 17:44:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Deepak Kumar>
-- Create date: <16-Mar-2015>
-- Description:	<Selecting Application based on ApplicationId>
-- ===========================================================
CREATE PROCEDURE [dbo].[spgApplicationByApplicationID]
(
	@ApplicationId uniqueidentifier,
	@UserName nvarchar(100)
)	 
AS
BEGIN
	    
    WITH XMLNAMESPACES (DEFAULT 'http://www.iringtools.org/library') 
    select 
		 APL.ContextId as contextId, 
		 APL.ApplicationId as applicationId , 
		 APL.DisplayName as displayName,
		 APL.InternalName as internalName,
		 APL.Description as description,
		 APL.DXFRUrl as dxfrUrl,
		 APL.Active as active,
		 APL.Assembly as assembly,
		(
			select 
			distinct  
			p.PermissionId as permissionId, 
			p.PermissionName as permissionName, 
			p.PermissionDesc as permissionDesc, 
			p.Active as active from 
			ResourceGroups 
				RG inner join Groups G on RG.GroupId = G.GroupId 
							And RG.Active = 1 
							And G.Active = 1
							And RG.ResourceId = APL.ApplicationId
				   inner join UserGroups UG on UG.GroupId = RG.GroupId
							And UG.Active = 1
				   inner join Users U on U.UserId = UG.UserId
							And U.Active = 1
							And U.UserName = @UserName					
				   inner join GroupRoles GR on GR.GroupId = UG.GroupId
							And GR.Active = 1
				   inner join Roles R on R.RoleId = GR.RoleId
							And R.Active = 1
				   inner join RolePermissions RP on RP.RoleId = R.RoleId
							And RP.Active = 1
				   inner join [Permissions] P on P.PermissionId = RP.PermissionId
							And P.Active = 1 for xml PATH('permission'), type 
		) as 'permissions',
		(
			Select gg.groupId,gg.groupName from 
					ResourceGroups rr inner join Groups gg on rr.GroupId = gg.GroupId 
										And rr.ResourceId = APL.ApplicationId
										and rr.Active = 1
										and gg.Active = 1 						
								inner join UserGroups UG1 on UG1.GroupId = rr.GroupId
										And UG1.Active = 1
								inner join Users U1 on U1.UserId = UG1.UserId
									And U1.Active = 1
									And U1.UserName = @UserName					
			
			for xml PATH('group'), type
		)  as 'groups'
    from Applications APL Where APL.ApplicationId = @ApplicationId
    for xml PATH('application')--, ROOT('graphs'), type, ELEMENTS XSINIL
	
END
GO
/****** Object:  StoredProcedure [dbo].[spiApplicationAfterDragAndDrop]    Script Date: 04/16/2015 17:44:53 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <01-Apr-2015>
-- Description:	<Inserting Application details after drag and drop>
-- Last Modified By: <>
-- Modification Date: <>
-- =============================================

 CREATE  PROCEDURE [dbo].[spiApplicationAfterDragAndDrop] 
(
	@ContextId	UNIQUEIDENTIFIER,
	@DisplayName	NVARCHAR(255),
	@InternalName	NVARCHAR(255),
	@Description	NVARCHAR(255),
	@DXFRUrl	NVARCHAR(255),
	@Assembly NVARCHAR(255),
	@GroupList XML,
	--@AppSettingsList XML,
	--@Binding XML,
	@NewAppId UNIQUEIDENTIFIER OUTPUT,
	@IsAppExist TINYINT OUTPUT
	
)	 
AS
BEGIN TRY
	--BEGIN TRANSACTION;
	
	BEGIN 					
		IF NOT EXISTS (SELECT TOP 1 * FROM Applications WHERE DisplayName = @DisplayName AND InternalName = @InternalName AND Active = 1 AND ContextId = @ContextId) 
			BEGIN
				
				DECLARE @Binding XML
				DECLARE @ApplicationId UNIQUEIDENTIFIER = NEWID() 
				SET @NewAppId = @ApplicationId
				
				DECLARE @ResourceTypeId INT = 3 --See ResourceType for detail
				 
				INSERT INTO [Applications]
				(
				   [ContextId],
				   [ApplicationId],
				   [DisplayName],
				   [InternalName],
				   [Description],
				   [DXFRUrl],
				   [Assembly],
				   [DataMode]
				)
				VALUES
				(
				   @ContextId,
				   @ApplicationId,
				   @DisplayName,
				   @InternalName,
				   @Description,
				   @DXFRUrl,
				   @Assembly,
				   'Live'
				)

				SELECT       
				nref.value('groupId[1]', 'INT') GroupId      
				INTO #Groups FROM   @GroupList.nodes('//group') AS R(nref) 
				
				INSERT INTO [ResourceGroups]
			   (
				   [ResourceId],
				   [GroupId],
				   [ResourceTypeId]
			   )
				SELECT 
					@ApplicationId,
					tg.GroupId,
					@ResourceTypeId
				FROM Groups g
				INNER JOIN #Groups tg ON g.GroupId = tg.GroupId

				-- Inserting XSitesAdminGroup id for resource in ResourceGroups table if it has not been inserted already
				IF NOT EXISTS(SELECT TOP 1 * FROM ResourceGroups rg INNER JOIN Groups g ON rg.GroupId = g.GroupId WHERE g.GroupName = 'XSitesAdminGroup' AND rg.ResourceId = @ApplicationId AND rg.ResourceTypeId = @ResourceTypeId AND rg.Active = 1)
				BEGIN
					INSERT INTO [ResourceGroups]
					(
					   [ResourceId],
					   [GroupId],
					   [ResourceTypeId]
					)
					SELECT 
						@ApplicationId,
						g.GroupId,
						@ResourceTypeId
					FROM Groups g
					WHERE g.GroupName = 'XSitesAdminGroup'
				END
				
				SET @Binding = (select ModuleName,BindName,Service,[to] from BindingConfig where ApplicationId = '9A535F92-17FA-4499-9A01-0CBBB636336F'
		FOR XML PATH('applicationBinding'),root('applicationBindingS'))
				
				SELECT
			nref.value('ModuleName[1]', 'VARCHAR(250)') ModuleName,
			nref.value('BindName[1]', 'VARCHAR(250)') BindName,
			nref.value('Service[1]', 'VARCHAR(650)') [Service],
			nref.value('to[1]', 'VARCHAR(650)') [To]
			INTO #Binding
			FROM @Binding.nodes('//applicationBinding') AS R(nref)
			
				
				INSERT INTO BindingConfig
				(
					ApplicationId,
					ModuleName,
					BindName,
					[Service],
					[To]
				)
				SELECT
					@ApplicationId,
					ModuleName,
					BindName,
					[Service],
					[To]
				FROM #Binding b
				
				SET @IsAppExist =1
				DROP TABLE #Binding
				
				--SELECT  '1'--'Application added successfully!'
			END--end of if
		ELSE
			--SELECT  '0'--'Application with this name already exists!'
			SET @IsAppExist =0
	--COMMIT TRANSACTION;
	END
END TRY
BEGIN CATCH
	--ROLLBACK TRANSACTION;
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spiApplication]    Script Date: 04/16/2015 17:44:53 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Abhishek Pandey>
-- Create date: <19-Mar-2015>
-- Description:	<Inserting Application details>
-- Last Modified By: <Abhishek Pandey>
-- Modification Date: <13-Apr-2015>
-- =============================================

CREATE PROCEDURE [dbo].[spiApplication] 
(
	@ContextId	UNIQUEIDENTIFIER,
	@DisplayName	NVARCHAR(255),
	@InternalName	NVARCHAR(255),
	@Description	NVARCHAR(255),
	@DXFRUrl	NVARCHAR(255),
	@Assembly NVARCHAR(255),
	@GroupList XML,
	@AppSettingsList XML,
	@DataMode NVARCHAR(50),
	@Binding XML
)	 
AS
BEGIN TRY
	BEGIN TRANSACTION;
	
	BEGIN 					
		IF NOT EXISTS (SELECT TOP 1 * FROM Applications WHERE DisplayName = @DisplayName AND InternalName = @InternalName AND Active = 1 AND ContextId = @ContextId) 
			BEGIN
				
				DECLARE @ApplicationId UNIQUEIDENTIFIER = NEWID() 
				DECLARE @ResourceTypeId INT = 3 --See ResourceType for detail
				 
				INSERT INTO [Applications]
				(
				   [ContextId],
				   [ApplicationId],
				   [DisplayName],
				   [InternalName],
				   [Description],
				   [DXFRUrl],
				   [Assembly],
				   [DataMode]
				)
				VALUES
				(
				   @ContextId,
				   @ApplicationId,
				   @DisplayName,
				   @InternalName,
				   @Description,
				   @DXFRUrl,
				   @Assembly,
				   @DataMode
				)

				SELECT       
				nref.value('groupId[1]', 'INT') GroupId      
				INTO #Groups FROM   @GroupList.nodes('//group') AS R(nref) 
				
				INSERT INTO [ResourceGroups]
			   (
				   [ResourceId],
				   [GroupId],
				   [ResourceTypeId]
			   )
				SELECT 
					@ApplicationId,
					tg.GroupId,
					@ResourceTypeId
				FROM Groups g
				INNER JOIN #Groups tg ON g.GroupId = tg.GroupId

				-- Inserting XSitesAdminGroup id for resource in ResourceGroups table if it has not been inserted already
				IF NOT EXISTS(SELECT TOP 1 * FROM ResourceGroups rg INNER JOIN Groups g ON rg.GroupId = g.GroupId WHERE g.GroupName = 'XSitesAdminGroup' AND rg.ResourceId = @ApplicationId AND rg.ResourceTypeId = @ResourceTypeId AND rg.Active = 1)
				BEGIN
					INSERT INTO [ResourceGroups]
					(
					   [ResourceId],
					   [GroupId],
					   [ResourceTypeId]
					)
					SELECT 
						@ApplicationId,
						g.GroupId,
						@ResourceTypeId
					FROM Groups g
					WHERE g.GroupName = 'XSitesAdminGroup'
				END
					
				SELECT
					nref.value('key[1]', 'VARCHAR(250)') Name,
					nref.value('value[1]', 'VARCHAR(550)') settingValue
				INTO #AppSettingsList
				FROM @AppSettingsList.nodes('//add/setting') AS R(nref)
				
				INSERT INTO ApplicationSettings
				(
					ApplicationId,
					settingId,
					settingValue
				)
				SELECT
					@ApplicationId,
					s.SettingId,
					a.settingValue
				FROM #AppSettingsList a
				INNER JOIN Settings s ON s.Name = a.Name
				
				SELECT       
					nref.value('moduleName[1]', 'VARCHAR(250)') ModuleName,
					nref.value('bindName[1]', 'VARCHAR(250)') BindName,
					nref.value('service[1]', 'VARCHAR(650)') [Service],
					nref.value('to[1]', 'VARCHAR(650)') [To]
				INTO #Binding
				FROM @Binding.nodes('//applicationBinding') AS R(nref)
				
				INSERT INTO BindingConfig
				(
					ApplicationId,
					ModuleName,
					BindName,
					[Service],
					[To]
				)
				SELECT
					@ApplicationId,
					ModuleName,
					BindName,
					[Service],
					[To]
				FROM #Binding b
				
				SELECT  '1'--'Application added successfully!'
			END--end of if
		ELSE
			SELECT  '0'--'Application with this name already exists!'
			
	COMMIT TRANSACTION;
	END
END TRY
BEGIN CATCH
	ROLLBACK TRANSACTION;
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spgValuelistforManifest]    Script Date: 04/16/2015 17:44:52 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		 Gagan Dhamija
-- Create date:  17-Sep-2014
-- Modified by   Deepak kumar
-- Modifed data: 4-Apr-2015>
-- Description:	 Selecting Valuelist based on Application Id>
-- ===========================================================
CREATE PROCEDURE [dbo].[spgValuelistforManifest] --'vmallire','3094199C-29CD-46F9-8107-609E303B61E1'
(
	@UserName varchar(100),
	@ApplicationId uniqueidentifier
)	 
AS
BEGIN TRY
	BEGIN
	 
      WITH XMLNAMESPACES ( DEFAULT 'http://www.iringtools.org/mapping')
      select  
		  VM.Name as name
		  ,(
			select distinct  
			VM1.internalvalue as internalValue, 
			VM1.Uri as uri, 
			VM1.Label as label 
			from ValueListMap VM1 Inner join Applications A1 on VM1.ApplicationId = A1.ApplicationId 
											and A1.Active = 1 
											and A1.ApplicationId = @ApplicationId
											and VM1.Name = VM.Name
								  Inner	join ResourceGroups RG1 on A1.ApplicationId = RG1.ResourceId 
										    and RG1.Active = 1
								  Inner join UserGroups UG1 on UG1.GroupId = rg1.GroupId
											and UG1.Active = 1
								  Inner join Groups G1 on UG1.GroupId = G1.GroupId
								            and G1.Active = 1  
								  Inner join Users U1 on U1.UserId = UG1.UserId 
								            and U1.Active = 1
								            and U1.UserName = @UserName
											 		     	
		  --inner join ResourceGroups crg on crg.siteid = @SiteId and crg.active = 1 and crg.resourceid = vm.ApplicationId 
		  --inner join usergroups cug on cug.groupid = crg.groupid and cug.siteid = crg.siteid and cug.active = crg.active
		  --inner join users cu on cu.userid = cug.userid and cu.username = @UserName and cu.siteid = crg.siteid and cu.active = crg.active
		  --inner Join  Applications a  on  a.ApplicationId = vm.ApplicationId  
		  --where vm.ApplicationId = @ApplicationId  and vm.Name = v.Name
		  Group BY   VM1.Label, VM1.internalvalue ,VM1.Uri 
		  for xml PATH('valueMap'), type 
		   ) as 'valueMaps'
		   
 	   from ValueListMap VM Inner join Applications A on VM.ApplicationId = A.ApplicationId 
											and A.Active = 1 
											and A.ApplicationId = @ApplicationId
								  Inner	join ResourceGroups RG on A.ApplicationId = RG.ResourceId 
										    and RG.Active = 1
								  Inner join UserGroups UG on UG.GroupId = rg.GroupId
											and UG.Active = 1
								  Inner join Groups G on UG.GroupId = G.GroupId
								            and G.Active = 1  
								  Inner join Users U1 on U1.UserId = UG.UserId 
								            and U1.Active = 1
								            and U1.UserName = @UserName	   
      --from ValueListMap v
      --inner join ResourceGroups crg on crg.siteid = @SiteId and crg.active = 1 and crg.resourceid = v.ApplicationId      
      --inner join usergroups cug on cug.groupid = crg.groupid and cug.siteid = crg.siteid and cug.active = crg.active
      --inner join users cu on cu.userid = cug.userid and cu.username = @UserName and cu.siteid = crg.siteid and cu.active = crg.active
      --inner Join  Applications a  on  a.ApplicationId = v.ApplicationId 
      --where UPPER(v.ApplicationId) = UPPER(@ApplicationId)  
      Group BY VM.ApplicationId, VM.Name
      for xml PATH('valueListMap'), ROOT('valueListMaps'), type, ELEMENTS XSINIL

	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spgValuelist]    Script Date: 04/16/2015 17:44:52 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Gagan Dhamija>
-- Create date: <1-Sep-2014>
-- Description:	<Selecting Valuelist based ON Application Id>
-- Last Modified By: <Abhishek Pandey>
-- Modification Date: <06th-Apr-2015>
-- ===========================================================
CREATE PROCEDURE [dbo].[spgValuelist]
(
	@UserName varchar(100),
	@ApplicationId uniqueidentifier
)	 
AS
BEGIN TRY
	BEGIN
	 
	  WITH XMLNAMESPACES ( DEFAULT 'http://www.iringtools.org/mapping')
      SELECT v.Name AS name, 
      (SELECT DISTINCT vm.Label AS label, vm.internalvalue AS internalValue, vm.Uri AS uri FROM ValueListMap vm
      INNER JOIN ResourceGroups crg ON crg.active = 1 AND crg.resourceid = vm.ApplicationId 
      INNER JOIN usergroups cug ON cug.groupid = crg.groupid AND cug.active = crg.active
      INNER JOIN users cu ON cu.userid = cug.userid AND cu.username = @UserName AND cu.active = crg.active
      INNER JOIN  Applications a ON  a.ApplicationId = vm.ApplicationId  
      WHERE vm.ApplicationId = @ApplicationId  AND vm.Name = v.Name
      GROUP BY vm.Label, vm.internalvalue ,vm.Uri 
      FOR XML PATH('valueMap'), TYPE ) AS 'valueMaps'
      FROM ValueListMap v
      INNER JOIN ResourceGroups crg ON crg.active = 1 AND crg.resourceid = v.ApplicationId      
      INNER JOIN usergroups cug ON cug.groupid = crg.groupid AND cug.active = crg.active
      INNER JOIN users cu ON cu.userid = cug.userid AND cu.username = @UserName AND cu.active = crg.active
      INNER JOIN  Applications a ON  a.ApplicationId = v.ApplicationId 
      WHERE v.ApplicationId = @ApplicationId  
      GROUP BY v.ApplicationId, v.Name
      FOR xml PATH('valueListMap'), ROOT('valueListMaps'), TYPE, ELEMENTS XSINIL

	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spgUserGroups]    Script Date: 04/16/2015 17:44:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Gagan Dhamija>
-- Create date: <28-July-2014>
-- Description:	<Getting User Groups by specific user>
-- =============================================
CREATE PROCEDURE [dbo].[spgUserGroups] --'test user6'
(
	@userName Varchar(50)
)
	 
AS
BEGIN TRY
	BEGIN
         SELECT ug.UserGroupId, ug.GroupId, ug.UserId, ug.UserGroupsDesc FROM UserGroups ug
         JOIN Users ON ug.UserId=Users.UserId 
         where ug.Active = 1 and Users.UserName = @userName and Users.Active = 1;
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spgSitesByUser]    Script Date: 04/16/2015 17:44:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Abhishek Pandey>
-- Create date: <16-March-2015>
-- Description:	<Getting Site details based on user name>
-- =============================================
CREATE PROCEDURE [dbo].[spgSitesByUser]
(
	@UserName varchar(100),
	@SiteId INT
)
	 
AS
BEGIN TRY
	BEGIN
		DECLARE @isXSitesAdminRole BIT

		SET @isXSitesAdminRole = (
		SELECT
			CASE
				WHEN COUNT(*) = 0 
				THEN 0 
				ELSE 1 
			END
		FROM Roles
		INNER JOIN GroupRoles ON Roles.RoleId = GroupRoles.RoleId
		INNER JOIN Groups ON GroupRoles.GroupId = Groups.GroupId
		INNER JOIN UserGroups ON UserGroups.GroupId = Groups.GroupId
		INNER JOIN Users ON Users.UserId = UserGroups.UserId
		WHERE Users.UserName = @UserName AND Roles.RoleName = 'XSitesAdminRole' AND Users.Active = 1)

		IF @isXSitesAdminRole = 1
		BEGIN
			SELECT * FROM Sites WHERE Active=1 
		END
		ELSE
			SELECT * FROM Sites WHERE Active=1 AND SiteId = @SiteId
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spgRolesGroup]    Script Date: 04/16/2015 17:44:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <01-JuL-2014>
-- Description:	<Return role based on group >
-- =============================================
CREATE PROCEDURE [dbo].[spgRolesGroup] 
(
	@GroupId INT
)
	 
AS
BEGIN TRY
	BEGIN
		SELECT DISTINCT R.RoleId, R.RoleName,R.RoleDesc FROM Roles R
			INNER JOIN GroupRoles GR ON GR.RoleId = R.RoleId WHERE GR.Active =1 AND R.Active =1 AND GR.GroupId = @GroupId
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spgRolePermissions]    Script Date: 04/16/2015 17:44:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <02-Jul-2014>
-- Description:	<Return permission based on role>

-- =============================================
CREATE PROCEDURE [dbo].[spgRolePermissions] --1,1
(
	@RoleId INT
)	
	 
AS
BEGIN TRY
	BEGIN
		select p.PermissionId,p.PermissionName,p.PermissionDesc from RolePermissions rp inner join [Permissions] p on rp.PermissionId = p.PermissionId
											and rp.Active = 1 and p.Active = 1
										 inner join Roles r on r.RoleId = rp.RoleId and r.Active = 1	
											where rp.RoleId = @RoleId
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH

		
		
		--select * from RolePermissions where RoleId = 1 and SiteId =1
GO
/****** Object:  StoredProcedure [dbo].[spgRoleGroups]    Script Date: 04/16/2015 17:44:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Deepak Kumar>
-- Create date: <22-Aug-2014>
-- Description:	<Return all group belongs 
-- to role>
-- =============================================
CREATE PROCEDURE [dbo].[spgRoleGroups]
(
	@RoleId INT
)
	 
AS
BEGIN
		
		SELECT G.GroupId, G.GroupName,G.GroupDesc FROM Groups G
			INNER JOIN GroupRoles GR ON GR.GroupId = G.GroupId
			INNER JOIN Roles R ON R.RoleId = GR.RoleId 
		WHERE GR.Active=1 AND R.Active=1 AND G.Active=1 AND R.RoleId = @RoleId
END
GO
/****** Object:  StoredProcedure [dbo].[spgJob]    Script Date: 04/16/2015 17:44:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE  PROCEDURE [dbo].[spgJob] 
(
	@JobId uniqueidentifier
)
AS
BEGIN TRY
   BEGIN
	 
    WITH XMLNAMESPACES ( DEFAULT 'http://www.iringtools.org/library')

	SELECT 
		j.JobId as jobId,
		j.ScheduleId as scheduleId,
		j.DataObjectId as dataObjectId,
		j.Is_Exchange as isExchange,
		j.Xid as xid,
		j.Cache_Page_Size as cache_page_size,
		j.PlatformId as platformId,
		j.SiteId as siteId,
		j.Next_Start_DateTime as next_Start_DateTime,
		j.Last_Start_DateTime as last_Start_DateTime,
		j.TotalRecords as totalRecords,
		j.CachedRecords as cachedRecords,
		j.Active as Active,
		(
		  select 
		    s.scheduleid as scheduleId,
		    s.created_datetime as created_DateTime, 
		    s.Created_By as created_By, 
		    s.occurance as occurance, 
		    s.[weekday] as weekday, 
		    s.start_datetime as start_DateTime, 
		    s.end_datetime as end_DateTime, 
		    s.[status] as status
		    from schedule s
		    where j.scheduleid = s.scheduleid for xml PATH('schedule'), type
		) as 'schedules'
		from job j
		where j.JobId = @JobId 
		for xml PATH('job'), type, ELEMENTS XSINIL
   END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spgGroupUsers]    Script Date: 04/16/2015 17:44:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <27-June-2014>
-- Description:	<Getting Group Users>
-- =============================================
CREATE PROCEDURE [dbo].[spgGroupUsers]
(
	@GroupId INT
)
	 
AS
BEGIN
		
	SELECT  
		U.UserId,
		U.UserFirstName ,
		U.UserLastName,
		U.UserEmail,
		U.UserPhone,
		U.UserDesc 
	FROM Users U INNER JOIN UserGroups UG ON UG.UserId = U.UserId
				 INNER JOIN Groups G ON G.GroupId = UG.GroupId
	WHERE UG.GroupId = @GroupId
		AND UG.Active=1 
		AND U.Active=1 
		AND G.Active =1   
		
END
GO
/****** Object:  StoredProcedure [dbo].[spgGroupUserIdGroupId]    Script Date: 04/16/2015 17:44:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <01-JuL-2014>
-- Description:	<return user based on user id and grop id>
-- =============================================
CREATE PROCEDURE [dbo].[spgGroupUserIdGroupId]
(
	@UserId INT,
	@GroupId INT
)
	 
AS
BEGIN TRY
	BEGIN
		
		SELECT  U.UserId,U.UserFirstName ,U.UserLastName,U.UserEmail,U.UserPhone,U.UserDesc FROM Users U
			INNER JOIN UserGroups UG ON UG.UserId = U.UserId
			INNER JOIN Groups G ON G.GroupId = UG.GroupId
		WHERE UG.Active=1 AND U.Active=1 AND G.Active =1 AND  UG.GroupId = @GroupId AND U.UserId =@UserId
	
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spgGroupUser]    Script Date: 04/16/2015 17:44:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================    
-- Author:  <Deepak Kumar>    
-- Create date: <08-Apr-15>
-- Modified By:
-- Modified Date:    
-- Description: <Return all group that the user belongs to>    
-- =============================================    
CREATE PROCEDURE [dbo].[spgGroupUser]    
(    
 @UserName NVARCHAR(100)  
)     
AS    
BEGIN  
   WITH XMLNAMESPACES ( DEFAULT 'http://www.iringtools.org/library')     
   (  
    SELECT DISTINCT G.GroupId AS groupId, G.GroupName AS groupName,G.GroupDesc AS groupDesc FROM Groups G    
    INNER JOIN UserGroups UG ON UG.GroupId = G.GroupId    
    INNER JOIN Users U ON U.UserId = UG.UserId     
    WHERE U.UserName = @UserName  
    AND U.Active=1   
    AND G.Active=1   
    AND UG.Active=1  
   )  
   FOR XML PATH('group'),  ROOT('Groups'), TYPE, ELEMENTS XSINIL  
END
GO
/****** Object:  StoredProcedure [dbo].[spgGroupsByUser]    Script Date: 04/16/2015 17:44:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================  
-- Author:  <Abhishek Pandey>  
-- Create date: <18-Mar-2015>  
-- Description: <Return all group that the user belongs to>  
-- =============================================  
CREATE PROCEDURE [dbo].[spgGroupsByUser]  
(  
 @UserName NVARCHAR(100)
)   
AS  
BEGIN
	BEGIN TRY
		BEGIN TRANSACTION;
		
		DECLARE @isXSitesAdminGroupMember BIT

			SET @isXSitesAdminGroupMember = (
			SELECT
				CASE
					WHEN COUNT(*) = 0 
					THEN 0 
					ELSE 1 
				END
			FROM Groups
			INNER JOIN UserGroups ON UserGroups.GroupId = Groups.GroupId
			INNER JOIN Users ON Users.UserId = UserGroups.UserId
			WHERE Users.UserName = @UserName
			AND Groups.GroupName = 'XSitesAdminGroup'
			AND Users.Active = 1
			AND UserGroups.Active = 1
			AND Groups.Active = 1)
			
		IF @isXSitesAdminGroupMember = 1
			BEGIN
				WITH XMLNAMESPACES ( DEFAULT 'http://www.iringtools.org/library')   
				(
					SELECT DISTINCT G.GroupId AS groupId, G.GroupName AS groupName,G.GroupDesc AS groupDesc FROM Groups G  
					INNER JOIN UserGroups UG ON UG.GroupId = G.GroupId  
					INNER JOIN Users U ON U.UserId = UG.UserId   
					WHERE G.Active=1 
					AND UG.Active=1
				)
				FOR XML PATH('group'),  ROOT('Groups'), TYPE, ELEMENTS XSINIL
			END
			ELSE
				WITH XMLNAMESPACES ( DEFAULT 'http://www.iringtools.org/library')   
				(
					SELECT DISTINCT G.GroupId AS groupId, G.GroupName AS groupName,G.GroupDesc AS groupDesc FROM Groups G  
					INNER JOIN UserGroups UG ON UG.GroupId = G.GroupId  
					INNER JOIN Users U ON U.UserId = UG.UserId   
					WHERE U.UserName = @UserName
					AND U.Active=1 
					AND G.Active=1 
					AND UG.Active=1
				)
				FOR XML PATH('group'),  ROOT('Groups'), TYPE, ELEMENTS XSINIL
		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION;
		SELECT 'Error occured at database: ' + ERROR_MESSAGE()
	END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[spiGroupRoles]    Script Date: 04/16/2015 17:45:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================    
-- Author:  <Deepak Kumar>    
-- Create date: <25-Aug-2014>    
-- Description: <mapping role with groups>    
-- =============================================    
CREATE PROCEDURE [dbo].[spiGroupRoles]-- ''    
(    
 @rawXML xml  
)     
      
AS    


BEGIN TRY
	BEGIN
    
			SET NOCOUNT ON;    
			    
			--Set @rawXML = ''    
		  --declare @aa int
		  --set @aa = 10 / 0   
			      
			SELECT       
			 nref.value('groupId[1]', 'int') GroupId,      
			 nref.value('roleId[1]', 'int') RoleId--,      
			INTO #Source FROM   @rawXML.nodes('//groupRole') AS R(nref)      
			      
			UPDATE t    
			SET t.Active = 0    
			FROM GroupRoles t INNER JOIN #Source s ON t.GroupId = s.GroupId
			     
			Declare @IsRoleExists int    
			Select top 1 @IsRoleExists = RoleId from #Source      

				if (@IsRoleExists <> -1)      
				Begin         
					MERGE [GroupRoles] AS T      
					USING #Source AS S      
					ON (T.RoleId = S.RoleId AND T.GroupId = S.GroupId )       
					WHEN NOT MATCHED BY TARGET       
						THEN INSERT([GroupId],[RoleId]) VALUES(S.[GroupId],S.[RoleId])      
					WHEN MATCHED       
						THEN UPDATE SET T.Active = 1;     
					Select '1' --'Roles mapped with the group successfully!' 		    
				End
				Else
					 Select '0' --'All roles unmapped with the group successfully!'

	 END
END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spiGraph]    Script Date: 04/16/2015 17:44:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Gagan Dhamija>
-- Create date: <16-Jul-2014>
-- Description:	<Inserting  Graphs> 
-- ===========================================================
CREATE PROCEDURE [dbo].[spiGraph] 
(
	@UserName varchar(100),--'vmallire'
	@ApplicationId uniqueidentifier,
	@GraphName nvarchar(255),
	@Graph varbinary(MAX),
	@GroupList xml
)	 
AS
BEGIN TRY
		BEGIN
		
			If Not Exists (Select top 1 * from Graphs where GraphName = @GraphName And Active = 1 and ApplicationId = @ApplicationId) 
			Begin
			
					Declare @GraphId uniqueidentifier = NewID()  
					Declare @ResourceTypeId int = 4 --See ResourceType for detail
					
					
					INSERT INTO [Graphs]
						   ([ApplicationId]
						   ,[GraphId]
						   ,[GraphName]
						   ,[Graph]
							)
					 VALUES
						   (
						   @ApplicationId
						   ,@GraphId
						   ,@GraphName
						   ,@Graph
						   )

					
					SELECT       
					nref.value('groupId[1]', 'int') GroupId      
					INTO #Groups FROM   @GroupList.nodes('//group') AS R(nref)
					
					INSERT INTO [ResourceGroups]
						   ([ResourceId]
						   ,[GroupId]
						   ,[ResourceTypeId]
						   )
					Select 
						@GraphId,
						tg.GroupId,
						@ResourceTypeId
					from  UserGroups ug 
							inner join Users u on ug.UserId = u.UserId
								And u.UserName = @UserName
								And u.Active = 1
								And ug.Active = 1
							inner join #Groups tg on ug.GroupId = tg.GroupId
											
					Select  '1'--'Graph added successfully!'
			end--end of if		
			Else
				Select  '0'--'Graph with this name already exists!'
			END

END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spiFolderAfterDragAndDrop]    Script Date: 04/16/2015 17:44:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Atul Srivastava>
-- Create date: <25-Mar-2015>
-- Description:	<Inserting folder after drag and drop>
-- ===========================================================

 CREATE PROCEDURE [dbo].[spiFolderAfterDragAndDrop] 
(
	--@UserName VARCHAR(100),
	@SiteId INT,
	@PlatformId INT,
	@ParentFolderId UNIQUEIDENTIFIER,
    @FolderName NVARCHAR(50),
	@GroupList XML,
	@NewFolderId UNIQUEIDENTIFIER OUTPUT,
	@IsFolderExist TINYINT OUTPUT
	
	
	
)	 
AS
BEGIN TRY
	BEGIN TRANSACTION;
	
	BEGIN	
	
	If NOT EXISTS(SELECT top 1 * FROM Folders Where FolderName = @FolderName AND Active = 1 AND ParentFolderId = @ParentFolderId AND SiteId = @SiteId AND PlatformId = @PlatformId)
		BEGIN	
		DECLARE @FolderId UNIQUEIDENTIFIER = NEWID()  
		SET @NewFolderId =@FolderId
		DECLARE @ResourceTypeId INT = 2 --See ResourceType for detail
		
		
		INSERT INTO [Folders]
				   ([FolderId]
				   ,[ParentFolderId]
				   ,[FolderName]
				   ,[SiteId]
				   ,[PlatformId]
				   )
		SELECT @FolderId, @ParentFolderId, @FolderName, @SiteId, @PlatformId			
		
		SELECT       
		nref.value('groupId[1]', 'INT') GroupId      
		INTO #Groups FROM   @GroupList.nodes('//group') AS R(nref)
		
		-- Inserting all groups id for resources in ResourceGroups table
		INSERT INTO [ResourceGroups]
			   ([ResourceId]
			   ,[GroupId]
			   ,[ResourceTypeId]
			   )
		SELECT 
			@FolderId,
			tg.GroupId,
			@ResourceTypeId
		FROM  Groups g
		INNER JOIN #Groups tg ON g.GroupId = tg.GroupId
		
		-- Inserting XSitesAdminGroup id for resource in ResourceGroups table if it has not been inserted already
		IF NOT EXISTS(SELECT top 1 * FROM ResourceGroups rg INNER JOIN Groups g ON rg.GroupId = g.GroupId WHERE g.GroupName = 'XSitesAdminGroup' AND rg.ResourceId = @FolderId AND rg.ResourceTypeId = @ResourceTypeId AND rg.Active = 1)
		BEGIN
			INSERT INTO [ResourceGroups]
			   ([ResourceId]
			   ,[GroupId]
			   ,[ResourceTypeId]
			   )
			SELECT 
				@FolderId,
				g.GroupId,
				@ResourceTypeId
			FROM Groups g
			WHERE g.GroupName = 'XSitesAdminGroup'
		END
	
	SET @IsFolderExist = 1  --'Folder added successfully!'
		--SELECT  '1'--'Folder added successfully!'
		END--END of if
		ELSE
			--SELECT  '0'--'Folder with this name already exists!'
			SET @IsFolderExist = 0 --'Folder with this name already exists!'
	END
	
	COMMIT TRANSACTION;
END TRY
BEGIN CATCH
	ROLLBACK TRANSACTION;
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH;



--BEGIN TRY
--	BEGIN TRANSACTION;
	
--	BEGIN	
--	--Siblings are not allowed
--	If NOT EXISTS(SELECT top 1 * FROM Folders Where FolderName = @FolderName AND Active = 1 AND ParentFolderId = @ParentFolderId AND SiteId = @SiteId AND PlatformId = @PlatformId)
--		BEGIN	
--		DECLARE @FolderId UNIQUEIDENTIFIER = NEWID()  
--		SET @NewFolderId =@FolderId
--		DECLARE @ResourceTypeId INT = 2 --See ResourceType for detail
		
		
--		INSERT INTO [Folders]
--				   ([FolderId]
--				   ,[ParentFolderId]
--				   ,[FolderName]
--				   ,[SiteId]
--				   ,[PlatformId]
--				   )
--		SELECT @FolderId, @ParentFolderId, @FolderName, @SiteId, @PlatformId			
		
--		SELECT       
--		nref.value('groupId[1]', 'INT') GroupId      
--		INTO #Groups FROM   @GroupList.nodes('//group') AS R(nref)
		
--		DECLARE @isXSitesAdminGroupPresent BIT
		
--		SET @isXSitesAdminGroupPresent = (
--			SELECT
--				CASE
--					WHEN COUNT(*) = 0 
--					THEN 0 
--					ELSE 1 
--				END
--			FROM UserGroups ug 
--				INNER JOIN Users u ON ug.UserId = u.UserId AND u.UserName = @UserName AND u.Active = 1 AND ug.Active = 1
--				INNER JOIN #Groups tg ON ug.GroupId = tg.GroupId
--				INNER JOIN Groups g ON g.GroupId = ug.GroupId AND g.GroupName = 'XSitesAdminGroup')
		
--		-- Inserting all groups id for resources in ResourceGroups table
--		INSERT INTO [ResourceGroups]
--			   ([ResourceId]
--			   ,[GroupId]
--			   ,[ResourceTypeId]
--			   )
--		SELECT 
--			@FolderId,
--			tg.GroupId,
--			@ResourceTypeId
--		FROM  UserGroups ug 
--				INNER JOIN Users u ON ug.UserId = u.UserId
--					AND u.UserName = @UserName
--					AND u.Active = 1
--					AND ug.Active = 1
--				INNER JOIN #Groups tg ON ug.GroupId = tg.GroupId
		
--		-- Inserting XSitesAdminGroup id for resource in ResourceGroups table if it has not been inserted already
--		IF @isXSitesAdminGroupPresent = 0
--		BEGIN
--			INSERT INTO [ResourceGroups]
--			   ([ResourceId]
--			   ,[GroupId]
--			   ,[ResourceTypeId]
--			   )
--			SELECT 
--				@FolderId,
--				g.GroupId,
--				@ResourceTypeId
--			FROM Groups g
--			WHERE g.GroupName = 'XSitesAdminGroup'
--		END
	
--		SET @IsFolderExist = 1--'Folder added successfully!'
--		END--END of if
--		ELSE
--			SET @IsFolderExist =0--'Folder with this name already exists!'
--	END
	
--	COMMIT TRANSACTION;
--END TRY
--BEGIN CATCH
--	ROLLBACK TRANSACTION;
--    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
--END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spiFolder]    Script Date: 04/16/2015 17:44:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Deepak Kumar>
-- Create date: 
-- Modified by: Abhishek Panday
-- Modified Data: 18-Mar-2015
-- Description:	<Inserting folder>
-- ===========================================================

CREATE PROCEDURE [dbo].[spiFolder] 
(
	@SiteId INT,
	@PlatformId INT,
	@ParentFolderId UNIQUEIDENTIFIER,
    @FolderName NVARCHAR(50),
	@GroupList XML
)	 
AS
BEGIN TRY
	BEGIN TRANSACTION;
	
	BEGIN	
	--Siblings are not allowed
	If NOT EXISTS(SELECT top 1 * FROM Folders Where FolderName = @FolderName AND Active = 1 AND ParentFolderId = @ParentFolderId AND SiteId = @SiteId AND PlatformId = @PlatformId)
		BEGIN	
		DECLARE @FolderId UNIQUEIDENTIFIER = NEWID()  
		DECLARE @ResourceTypeId INT = 2 --See ResourceType for detail
		
		
		INSERT INTO [Folders]
				   ([FolderId]
				   ,[ParentFolderId]
				   ,[FolderName]
				   ,[SiteId]
				   ,[PlatformId]
				   )
		SELECT @FolderId, @ParentFolderId, @FolderName, @SiteId, @PlatformId			
		
		SELECT       
		nref.value('groupId[1]', 'INT') GroupId      
		INTO #Groups FROM   @GroupList.nodes('//group') AS R(nref)
		
		-- Inserting all groups id for resources in ResourceGroups table
		INSERT INTO [ResourceGroups]
			   ([ResourceId]
			   ,[GroupId]
			   ,[ResourceTypeId]
			   )
		SELECT 
			@FolderId,
			tg.GroupId,
			@ResourceTypeId
		FROM  Groups g
		INNER JOIN #Groups tg ON g.GroupId = tg.GroupId
		
		-- Inserting XSitesAdminGroup id for resource in ResourceGroups table if it has not been inserted already
		IF NOT EXISTS(SELECT top 1 * FROM ResourceGroups rg INNER JOIN Groups g ON rg.GroupId = g.GroupId WHERE g.GroupName = 'XSitesAdminGroup' AND rg.ResourceId = @FolderId AND rg.ResourceTypeId = @ResourceTypeId AND rg.Active = 1)
		BEGIN
			INSERT INTO [ResourceGroups]
			   ([ResourceId]
			   ,[GroupId]
			   ,[ResourceTypeId]
			   )
			SELECT 
				@FolderId,
				g.GroupId,
				@ResourceTypeId
			FROM Groups g
			WHERE g.GroupName = 'XSitesAdminGroup'
		END
	
		SELECT  '1'--'Folder added successfully!'
		END--END of if
		ELSE
			SELECT  '0'--'Folder with this name already exists!'
	END
	
	COMMIT TRANSACTION;
END TRY
BEGIN CATCH
	ROLLBACK TRANSACTION;
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spiJob]    Script Date: 04/16/2015 17:45:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spiJob] 
(
	@DataObjectIds NVARCHAR(max),
	@Is_Exchange INT,
	@Xid NVARCHAR(50),
	@Cache_Page_Size NVARCHAR(50),
	@PlatformId int,
	@SiteId int,
	@Active int,
	@Schedules xml 
)
AS
BEGIN TRY
	Declare @Occurance nvarchar(50)
	Declare @Next_Start_DateTime datetime
	Declare @JobId uniqueidentifier = NewID() 
	Declare @ScheduleId uniqueidentifier = NewID()
	
	If @Is_Exchange = 0
		BEGIN			
			-- loop through dataobjectids to create seperate entry
			DECLARE @pos INT
			DECLARE @len INT
			DECLARE @DataObjectId nvarchar(1000)
			
			set @pos = 0
			set @len = 0
			
			set @DataObjectIds = @DataObjectIds + ','
			WHILE CHARINDEX(',', @DataObjectIds, @pos+1) > 0
			BEGIN
				set @len = CHARINDEX(',', @DataObjectIds, @pos+1) - @pos
				set @DataObjectId = SUBSTRING(@DataObjectIds, @pos, @len)
				
				If not Exists(Select top 1 * from Job Where DataObjectId = @DataObjectId and Is_Exchange = @Is_Exchange)
				  Begin
					--insert into schedule table
					Insert into schedule (ScheduleId, Created_DateTime, Created_By, Occurance, Weekday, Start_DateTime, End_DateTime, Status)
					select 
					@ScheduleId,
					convert(varchar, getdate(), 121),
					T.N.value('(created_By)[1]', 'NVARCHAR(250)') as Created_By,
					T.N.value('(occurance)[1]', 'NVARCHAR(50)') as Occurance,
					T.N.value('(weekday)[1]', 'NVARCHAR(50)') as Weekday,
					convert(datetime, T.N.value('(start_DateTime)[1]', 'NVARCHAR(100)'), 121) ,
					convert(datetime, T.N.value('(end_DateTime)[1]', 'NVARCHAR(100)'), 121),
					'Ready'
					from @Schedules.nodes('schedules/schedule') as T(N)
					
					
					SET @Occurance =  (SELECT  T.N.value('(occurance)[1]', 'NVARCHAR(50)') as Occurance from @Schedules.nodes('schedules/schedule') as T(N))
					
					-- insert into job table
					
					
					If @Occurance = 'Immediate'
						Begin
							SET @Next_Start_DateTime =  convert(varchar, getdate(), 121)
						End
					Else
						SET @Next_Start_DateTime =  (SELECT  convert(datetime, T.N.value('(start_DateTime)[1]', 'NVARCHAR(100)'), 121) as Next_Start_DateTime from @Schedules.nodes('schedules/schedule') as T(N))
					
					INSERT INTO  Job (JobId,ScheduleId,DataObjectId,Is_Exchange,Xid,Cache_Page_Size,PlatformId,SiteId,Next_Start_DateTime,Active)
					VALUES (@JobId,@ScheduleId,@DataObjectId,@Is_Exchange,@Xid,@Cache_Page_Size,@PlatformId,@SiteId,@Next_Start_DateTime,@Active)
						
					set @pos = CHARINDEX(',', @DataObjectIds, @pos+@len) +1
					
					
					Select '1'--'Job added successfully!'
				  End
			Else
				set @pos = CHARINDEX(',', @DataObjectIds, @pos+@len) +1
				Select '0'--'Job with this dataobject already exists!'
			END
			
		END
	Else
		BEGIN
			If not Exists(Select top 1 * from Job Where Xid = @Xid)
				  Begin
			
					--insert into schedule table
					Insert into schedule (ScheduleId, Created_DateTime, Created_By, Occurance, Weekday, Start_DateTime, End_DateTime, Status)
					select 
					@ScheduleId,
					convert(varchar, getdate(), 121),
					T.N.value('(created_By)[1]', 'NVARCHAR(250)') as Created_By,
					T.N.value('(occurance)[1]', 'NVARCHAR(50)') as Occurance,
					T.N.value('(weekday)[1]', 'NVARCHAR(50)') as Weekday,
					convert(datetime, T.N.value('(start_DateTime)[1]', 'NVARCHAR(100)'), 121) ,
					convert(datetime, T.N.value('(end_DateTime)[1]', 'NVARCHAR(100)'), 121),
					'Ready'
					from @Schedules.nodes('schedules/schedule') as T(N)

					SET @Occurance =  (SELECT  T.N.value('(occurance)[1]', 'NVARCHAR(50)') as Occurance from @Schedules.nodes('schedules/schedule') as T(N))
					
					-- insert into job table
					If @Occurance = 'Immediate'
						Begin
							SET @Next_Start_DateTime =  convert(varchar, getdate(), 121)
						End
					Else
						SET @Next_Start_DateTime =  (SELECT  convert(datetime, T.N.value('(start_DateTime)[1]', 'NVARCHAR(100)'), 121) as Next_Start_DateTime from @Schedules.nodes('schedules/schedule') as T(N))
					
					INSERT INTO  Job (JobId,ScheduleId,DataObjectId,Is_Exchange,Xid,Cache_Page_Size,PlatformId,SiteId,Next_Start_DateTime,Active)
					VALUES (@JobId,@ScheduleId,@DataObjectId,@Is_Exchange,@Xid,@Cache_Page_Size,@PlatformId,@SiteId,@Next_Start_DateTime,@Active)
											
					Select '1'--'Job added successfully!'
				  End
			Else
				Select '0'--'Job with this dataobject already exists!'
			
		
		END
END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spiGroupUsers]    Script Date: 04/16/2015 17:45:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================  
-- Author:  <Deepak Kumar>  
-- Create date: <19-Aug-2014>  
-- Description: <Inserting Group users>  
-- =============================================  
CREATE PROCEDURE [dbo].[spiGroupUsers]-- ''  
(  
 @rawXML xml
)   
    
AS  

BEGIN TRY
	BEGIN
 
SET NOCOUNT ON;  
  
--Set @rawXML = '<userGroups>  
--  <userGroup>  
--    <userGroupId>20</userGroupId>  
--    <groupId>5</groupId>  
--    <userId>39</userId>  
--    <siteId>1</siteId>  
--    <userGroupsDesc>dfssaa</userGroupsDesc>  
--    <active>1</active>  
--  </userGroup>  
--</userGroups>'  

    
SELECT     
 nref.value('groupId[1]', 'int') GroupId,    
 nref.value('userId[1]', 'int') UserId--,    
 --nref.value('userGroupsDesc[1]', 'nvarchar(255)') UserGroupsDesc    
INTO #Source FROM   @rawXML.nodes('//userGroup') AS R(nref)    
    
UPDATE t  
SET t.Active = 0  
FROM UserGroups t INNER JOIN #Source s ON t.GroupId = s.GroupId  


Declare @IsUserExists int    
Select top 1 @IsUserExists = UserId from #Source

if (@IsUserExists <> -1)
Begin    
	MERGE [UserGroups] AS T    
	USING #Source AS S    
	ON (T.GroupId = S.GroupId AND T.UserId = S.UserId)     
	WHEN NOT MATCHED BY TARGET     
		THEN INSERT([GroupId],[UserId]) VALUES(S.[GroupId],S.[UserId])    
	WHEN MATCHED     
		THEN UPDATE SET --T.SiteId = @SiteId,
		--T.UserGroupsDesc = S.UserGroupsDesc, 
		T.Active = 1;
	    
	Select '1' --'Users mapped with the group successfully!' 		    
End
Else
	 Select '0' --'All users unmapped with the group successfully!'   
	    
END
END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spiUserGroups]    Script Date: 04/16/2015 17:45:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================    
-- Author:  <Deepak Kumar>    
-- Create date: <21-Aug-2014>    
-- Description: <Inserting Groups for user, as 
-- one user can lie in multiple groups>    
-- =============================================    
CREATE PROCEDURE [dbo].[spiUserGroups]-- ''    
(    
 @rawXML xml   
)     
      
AS    
BEGIN TRY
	BEGIN
	    
SET NOCOUNT ON;    
    
--Set @rawXML = '<userGroups>    
--  <userGroup>    
--    <userGroupId>20</userGroupId>    
--    <groupId>5</groupId>    
--    <userId>39</userId>    
--    <siteId>1</siteId>    
--    <userGroupsDesc>dfssaa</userGroupsDesc>    
--    <active>1</active>    
--  </userGroup>    
--</userGroups>'    

      
SELECT       
 nref.value('groupId[1]', 'int') GroupId,      
 nref.value('userId[1]', 'int') UserId--,      
 --nref.value('userGroupsDesc[1]', 'nvarchar(255)') UserGroupsDesc      
INTO #Source FROM   @rawXML.nodes('//userGroup') AS R(nref)      
      
UPDATE t    
SET t.Active = 0    
FROM UserGroups t INNER JOIN #Source s ON t.UserId = s.UserId    

Declare @IsGroupExists int    
Select top 1 @IsGroupExists = GroupId from #Source

	if (@IsGroupExists <> -1)      
	Begin      
		MERGE [UserGroups] AS T      
		USING #Source AS S      
		ON (T.GroupId = S.GroupId AND T.UserId = S.UserId)       
		WHEN NOT MATCHED BY TARGET       
			THEN INSERT([GroupId],[UserId]) VALUES(S.[GroupId],S.[UserId])      
		WHEN MATCHED       
			THEN UPDATE SET --T.SiteId = @SiteId,  
			--T.UserGroupsDesc = S.UserGroupsDesc,   
			T.Active = 1; 
			
	 Select '1' --'Groups mapped with the user successfully!' 		    
	End
	Else
	 Select '0' --'All groups unmapped with the user successfully!'
	    
END
END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spiRolePermissions]    Script Date: 04/16/2015 17:45:05 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================    
-- Author:  <Deepak Kumar>    
-- Create date: <26-Aug-2014>    
-- Description: <mapping role with permissions>    
-- =============================================    
CREATE PROCEDURE [dbo].[spiRolePermissions]-- ''    
(    
 @rawXML xml  
)     
      
AS    
BEGIN TRY
	BEGIN
	   
				SET NOCOUNT ON;    
				    
				--Set @rawXML = ''    
				
				--declare @aa int
				--set @aa = 10 / 0    
				      
				SELECT       
				 nref.value('roleId[1]', 'int') RoleId,      
				 nref.value('permissionId[1]', 'int') PermissionId--,      
				INTO #Source FROM   @rawXML.nodes('//rolePermission') AS R(nref)      
				      
				UPDATE t    
				SET t.Active = 0    
				FROM RolePermissions t INNER JOIN #Source s ON t.RoleId = s.RoleId

				Declare @IsPermissionExists int    
				Select top 1 @IsPermissionExists = PermissionId from #Source      



					if (@IsPermissionExists <> -1)      
					Begin 
						MERGE [RolePermissions] AS T      
						USING #Source AS S      
						ON (T.RoleId = S.RoleId AND T.PermissionId = S.PermissionId )       
						WHEN NOT MATCHED BY TARGET       
							THEN INSERT([RoleId],[PermissionID]) VALUES(S.[RoleId],S.[PermissionID])      
						WHEN MATCHED       
							THEN UPDATE SET T.Active = 1;     
						Select '1' --'Permissions mapped with the role successfully!' 		    
					End
					Else
						 Select '0' --'All permissions unmapped with the role successfully!'


END
END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spiRoleGroups]    Script Date: 04/16/2015 17:45:04 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================    
-- Author:  <Deepak Kumar>    
-- Create date: <22-Aug-2014>    
-- Description: <Inserting Groups in a role>    
-- =============================================    
CREATE PROCEDURE [dbo].[spiRoleGroups]-- ''    
(    
 @rawXML xml    
)     
      
AS    
BEGIN TRY
	BEGIN
	    
		SET NOCOUNT ON;    
		    
		--Set @rawXML = '<groupRoles xmlns1="http://www.iringtools.org/library" xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
		--  <groupRole>
		--    <groupId>1</groupId>
		--    <roleId>1</roleId>
		--  </groupRole>
		--  <groupRole>
		--    <groupId>3</groupId>
		--    <roleId>1</roleId>
		--  </groupRole>
		--  <groupRole>
		--    <groupId>4</groupId>
		--    <roleId>1</roleId>
		--  </groupRole>
		--  <groupRole>
		--    <groupId>5</groupId>
		--    <roleId>1</roleId>
		--  </groupRole>
		--</groupRoles>'    
		    
				--declare @aa int
				--set @aa = 10 / 0    
						      
		SELECT       
		 nref.value('groupId[1]', 'int') GroupId,      
		 nref.value('roleId[1]', 'int') RoleId--,      
		INTO #Source FROM   @rawXML.nodes('//groupRole') AS R(nref)      
		      
		UPDATE t    
		SET t.Active = 0    
		FROM GroupRoles t INNER JOIN #Source s ON t.RoleId = s.RoleId


		Declare @IsGroupExists int    
		Select top 1 @IsGroupExists = GroupId from #Source

			if (@IsGroupExists <> -1)      
			Begin      
				MERGE [GroupRoles] AS T      
				USING #Source AS S      
				ON (T.RoleId = S.RoleId AND T.GroupId = S.GroupId )       
				WHEN NOT MATCHED BY TARGET       
					THEN INSERT([GroupId],[RoleId]) VALUES(S.[GroupId],S.[RoleId])      
				WHEN MATCHED       
					THEN UPDATE SET --T.SiteId = @SiteId,  
					--T.UserGroupsDesc = S.UserGroupsDesc,   
					T.Active = 1;     
				Select '1' --'Groups mapped with the role successfully!' 		    
			End
			Else
				 Select '0' --'All groups unmapped with the role successfully!'


END
END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spuGraph]    Script Date: 04/16/2015 17:45:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Gagan Dhamija>
-- Create date: <16-Jul-2014>
-- Description:	<updating  Graphs> 
-- ===========================================================
CREATE PROCEDURE [dbo].[spuGraph] 
(
	@UserName varchar(100),--'vmallire'
	@GraphId uniqueidentifier,
	@GraphName nvarchar(255),
	@Graph varbinary(MAX),
	@SiteId int,
	@GroupList xml,
	--@IsGraphNameChanged bit,
	@ApplicationId uniqueidentifier
)	 
AS
BEGIN TRY

	BEGIN
	
		If Exists (Select top 1 * from Graphs where GraphName = @GraphName and ApplicationId = @ApplicationId and GraphId != @GraphId And Active = 1 ) 
			Begin
				Select '0' --graph with this name already exists
				return;
			End
		Else
			Begin
				UPDATE [Graphs]
				SET  
					[GraphName] = @GraphName
					,[Graph] = @Graph
					,[SiteId] = @SiteId
				WHERE  GraphId = @GraphId			


				SELECT       
				nref.value('groupId[1]', 'int') GroupId      
				INTO #Groups FROM   @GroupList.nodes('//group') AS R(nref) 

				/*Below query can not be included in merge statement
				as it will inactive all the non matching rows, hence
				separate query has written*/
				Update ResourceGroups
				Set   Active = 0
				Where ResourceId = @GraphId 
				
				Declare @ResourceTypeId int = 4 --See ResourceType for detail
				
				MERGE ResourceGroups AS T      
				USING 
				(
					--this query is making sure that only those groups will be considered which are
					--assigned to this user
					Select ug.GroupId from Users u inner join UserGroups ug on u.UserId = ug.UserId
											And u.UserName = @UserName
											And u.SiteId = @SiteId
											And u.Active = 1
											And ug.Active = 1
											And ug.SiteId = @SiteId  
										inner join #Groups tg on ug.GroupId = tg.GroupId
				
				)AS S  (GroupId)    
				ON (T.GroupId = S.GroupId AND T.SiteId = @SiteId AND T.ResourceId = @GraphId)       
				WHEN NOT MATCHED BY TARGET       
					THEN INSERT(ResourceId,GroupId,SiteId,ResourceTypeId) VALUES(@GraphId,S.GroupId,@SiteId,@ResourceTypeId)
				WHEN MATCHED       
					THEN UPDATE SET T.SiteId = @SiteId,  
					T.Active = 1;  

 					Select  '1'--'Graph updated successfully!'
 			
 		End 
	END

END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spuFolder]    Script Date: 04/16/2015 17:45:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Abhishek Pandey>
-- Create date: <19-Mar-2015>
-- Description:	<Updating folder>
-- ===========================================================
CREATE PROCEDURE [dbo].[spuFolder] 
(
	@SiteId INT,
	@PlatformId INT,
	@FolderId UNIQUEIDENTIFIER,
	@ParentFolderId UNIQUEIDENTIFIER,
    @FolderName NVARCHAR(50),
	@GroupList XML
)	 
AS
BEGIN TRY
	BEGIN TRANSACTION;
	
	BEGIN
		IF EXISTS(SELECT TOP 1 * FROM Folders WHERE FolderName = @FolderName AND FolderId != @FolderId AND ParentFolderId = @ParentFolderId AND SiteId = @SiteId AND PlatformId = @PlatformId AND Active = 1)
			BEGIN
				SELECT '0' --folder with this name already exists
				RETURN;
			END
		ELSE
			BEGIN
			
			UPDATE Folders
				SET FolderName = @FolderName
			WHERE FolderId = @FolderId 
					AND Active = 1					 
		
			SELECT       
			nref.value('groupId[1]', 'int') GroupId      
			INTO #Groups FROM   @GroupList.nodes('//group') AS R(nref) 

			/*Below query can not be included in merge statement
			as it will inactivate all the non matching rows, hence
			separate query has been written*/
			UPDATE ResourceGroups
			SET   Active = 0
			WHERE ResourceId = @FolderId
			
			DECLARE @ResourceTypeId INT = 2 --See ResourceType for detail
			
			
			MERGE ResourceGroups AS T      
			USING #Groups AS S
			ON (T.GroupId = S.GroupId  AND T.ResourceId = @FolderId)       
			WHEN NOT MATCHED BY TARGET       
				THEN INSERT(ResourceId,GroupId,ResourceTypeId) VALUES(@FolderId,S.GroupId,@ResourceTypeId)
			WHEN MATCHED       
				THEN UPDATE SET T.Active = 1;  
				 
			--UPDATE [ResourceGroups]
			--SET Active = 1
			--WHERE ResourceId = @FolderId
			--AND ResourceTypeId = @ResourceTypeId
			--AND GroupId IN (
			--	SELECT g.GroupId
			--	FROM Groups g INNER JOIN #Groups tg ON g.GroupId = tg.GroupId)
				
			-- Updating XSitesAdminGroup id for resource in ResourceGroups table if it has not been updated already				
			UPDATE [ResourceGroups]
			SET Active = 1
			WHERE ResourceId = @FolderId
			AND ResourceTypeId = @ResourceTypeId
			AND GroupId = (
				SELECT GroupId
				FROM Groups g
				WHERE g.GroupName = 'XSitesAdminGroup')
			
				SELECT  '1'--'folder updated successfully!'	
			END
	COMMIT TRANSACTION;
    END
END TRY
BEGIN CATCH
	ROLLBACK TRANSACTION;
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spuApplication]    Script Date: 04/16/2015 17:45:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <10-June-2014>
-- Description:	<updating Application details>
-- Last Modified By: <Abhishek Pandey>
-- Modification Date: <13-Apr-2015>
-- =============================================

CREATE PROCEDURE [dbo].[spuApplication] 
(
	@ContextId	UNIQUEIDENTIFIER,
	@ApplicationId	UNIQUEIDENTIFIER,
	@DisplayName	NVARCHAR(255),
	@Description	NVARCHAR(255),
	@DXFRUrl	NVARCHAR(255),
	@Assembly NVARCHAR(255),
	@GroupList XML,
	@AppSettingsList XML,
	@DataMode NVARCHAR(50),
	@Binding XML
)	 
AS
BEGIN TRY 
	BEGIN TRANSACTION;
	
	BEGIN
		IF EXISTS(SELECT TOP 1 * FROM Applications WHERE DisplayName = @DisplayName AND ApplicationId != @ApplicationId AND ContextId = @ContextId AND Active = 1)
			BEGIN
				SELECT '0' --application with this name already exists
				RETURN;
			END
		ELSE
		BEGIN		
			UPDATE [Applications]
			SET 
			 [DisplayName] = @DisplayName
			,[Description] = @Description
			,[DXFRUrl] = @DXFRUrl
			,[Assembly] = @Assembly
			,DataMode = @DataMode
			WHERE [ApplicationId] = @ApplicationId

			SELECT       
			nref.value('groupId[1]', 'INT') GroupId      
			INTO #Groups FROM   @GroupList.nodes('//group') AS R(nref) 

			--TODO: Need to handle @AppSettingsList
			/*Below query can not be included in merge statement
			as it will inactivate all the non matching rows, hence
			separate query has been written*/
			Update ResourceGroups
			Set   Active = 0
			Where ResourceId = @ApplicationId 
			
			Declare @ResourceTypeId INT = 3 --See ResourceType for detail
			
			MERGE ResourceGroups AS T      
			USING #Groups AS S
				ON (T.GroupId = S.GroupId  AND T.ResourceId = @ApplicationId)       
				WHEN NOT MATCHED BY TARGET       
					THEN INSERT(ResourceId, GroupId, ResourceTypeId) VALUES(@ApplicationId, S.GroupId, @ResourceTypeId)
				WHEN MATCHED       
					THEN UPDATE SET T.Active = 1; 

				-- Updating XSitesAdminGroup id for resource in ResourceGroups table if it has not been updated already				
				UPDATE [ResourceGroups]
				SET Active = 1
				WHERE ResourceId = @ApplicationId
				AND ResourceTypeId = @ResourceTypeId
				AND GroupId = (
					SELECT GroupId
					FROM Groups g
					WHERE g.GroupName = 'XSitesAdminGroup')			
	
			
			SELECT
				nref.value('key[1]', 'VARCHAR(250)') Name,
				nref.value('value[1]', 'VARCHAR(550)') settingValue
			INTO #AppSettingsList
			FROM @AppSettingsList.nodes('//add/setting') AS R(nref)
			
			DELETE FROM ApplicationSettings WHERE ApplicationId = @ApplicationId
			
			INSERT INTO ApplicationSettings
			(
				ApplicationId,
				settingId,
				settingValue
			)
			SELECT
				@ApplicationId,
				s.SettingId,
				a.settingValue
			FROM #AppSettingsList a
			INNER JOIN Settings s ON s.Name = a.Name
			
			SELECT       
				nref.value('moduleName[1]', 'VARCHAR(250)') ModuleName,
				nref.value('bindName[1]', 'VARCHAR(250)') BindName,
				nref.value('service[1]', 'VARCHAR(650)') [Service],
				nref.value('to[1]', 'VARCHAR(650)') [To]
			INTO #Binding
			FROM @Binding.nodes('//applicationBinding') AS R(nref)
			
			UPDATE BindingConfig
			SET
				ModuleName = b.ModuleName,
				BindName = b.BindName,
				[Service] = b.[Service],
				[To] = b.[To]
			FROM #Binding b
			WHERE ApplicationId = @ApplicationId
			
				Select  '1'--'Application updated successfully!'	
			END  
	COMMIT TRANSACTION;
	END
END TRY
BEGIN CATCH  
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spgDataLayers]    Script Date: 04/16/2015 17:44:31 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Abhishek Pandey>
-- Create date: <07-Apr-2015>
-- Description:	<This stored procedure gets all the datalayers for a particular site and platform>
-- =============================================

CREATE PROCEDURE [dbo].[spgDataLayers]
	@SiteId INT,
	@PlatformId INT
AS
BEGIN
	BEGIN TRY
		BEGIN TRANSACTION;
		
		WITH XMLNAMESPACES (DEFAULT 'http://www.iringtools.org/library') 
		SELECT
			[Assembly] AS [assembly],
			Name AS name,
			Configurable AS configurable,
			IsLightWeight AS isLightweight
		FROM  DataLayers
		WHERE SiteId = @SiteId AND PlatformId = @PlatformId AND Active = 1
		FOR XML PATH('dataLayer'), ROOT('dataLayers'), TYPE, ELEMENTS XSINIL
		
		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION;
		SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
	END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[spuContext]    Script Date: 04/16/2015 17:45:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================================================
-- Author:		<Abhishek pandey>
-- Create date: <19-Mar-2015>
-- Description:	<Updating Context details>
-- =============================================================================

CREATE PROCEDURE [dbo].[spuContext] 
(	
	@DisplayName NVARCHAR(255),
	@Description NVARCHAR(255),
	@CacheConnStr NVARCHAR(255),
	@ContextId UNIQUEIDENTIFIER,
	@ParentFolderId UNIQUEIDENTIFIER,
	@GroupList XML
)
AS
BEGIN TRY
	BEGIN TRANSACTION;
	
	BEGIN
		IF EXISTS(SELECT TOP 1 * FROM Contexts WHERE DisplayName = @DisplayName AND ContextID != @ContextId AND FolderId = @ParentFolderId AND Active = 1)
			BEGIN
				SELECT '0' --context with this name already exists
				RETURN;
			END
		ELSE
			BEGIN
				UPDATE Contexts
				SET
					DisplayName = @DisplayName,
					[Description] = @Description,
					CacheConnStr = @CacheConnStr
				WHERE ContextID = @ContextId				
					
				SELECT       
				nref.value('groupId[1]', 'int') GroupId      
				INTO #Groups FROM   @GroupList.nodes('//group') AS R(nref)

				/*Below query can not be included in merge statement
				as it will inactive all the non matching rows, hence
				separate query has written*/
				UPDATE ResourceGroups
				SET   Active = 0
				WHERE ResourceId = @ContextId 
				
				DECLARE @ResourceTypeId int = 1 --See ResourceType for detail

				MERGE ResourceGroups AS T      
				USING #Groups AS S
				ON (T.GroupId = S.GroupId  AND T.ResourceId = @ContextId)       
				WHEN NOT MATCHED BY TARGET       
					THEN INSERT(ResourceId,GroupId,ResourceTypeId) VALUES(@ContextId,S.GroupId,@ResourceTypeId)
				WHEN MATCHED       
					THEN UPDATE SET T.Active = 1;  
							
				-- Updating XSitesAdminGroup id for resource in ResourceGroups table if it has not been updated already				
				UPDATE [ResourceGroups]
				SET Active = 1
				WHERE ResourceId = @ContextId
				AND ResourceTypeId = @ResourceTypeId
				AND GroupId = (
					SELECT GroupId
					FROM Groups g
					WHERE g.GroupName = 'XSitesAdminGroup')
			
				SELECT  '1'--'context updated successfully!'
			END
	COMMIT TRANSACTION;
	END
END TRY
BEGIN CATCH
	ROLLBACK TRANSACTION;
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spgContextByUser]    Script Date: 04/16/2015 17:44:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Gagan Dhamija>
-- Create date: <22-July-2014>
-- Description:	<Selecting Contexts based on users>
-- ===========================================================
CREATE  PROCEDURE [dbo].[spgContextByUser]
(
	@UserName varchar(100),
	@FolderId uniqueidentifier
)	 
AS
BEGIN TRY
	BEGIN
	 
	  WITH XMLNAMESPACES ( DEFAULT 'http://www.iringtools.org/library')
      select  
		c.ContextId as contextId ,
		c.DisplayName as displayName,
		c.InternalName as internalName,
		c.Description as description,
		c.CacheConnStr as cacheConnStr,
		c.Active as active,
		c.FolderId as folderId,
		(
		  select 
				distinct 
				p.PermissionId as permissionId,
				p.PermissionName as permissionName,
				p.PermissionDesc as permissionDesc,
				p.Active as active 
		  from permissions p
		  inner join ResourceGroups rg on rg.active = c.active and rg.resourceid = c.contextid 
		  inner join Groups g on g.groupId = rg.groupId and  g.active = c.active
		  inner join grouproles gr on  gr.groupid = rg.groupid and  gr.active = c.active    
		  inner join roles r on r.roleid = gr.roleid  and r.active = c.active     
		  inner join rolepermissions rp on rp.permissionid = p.permissionid and rp.roleid = r.roleid and rp.active = c.active          
		  inner join usergroups ug on ug.groupid = rg.groupid  and ug.active = c.active
		  inner join users u on u.userid = ug.userid and u.username = @UserName and  u.active = c.active
		where c.folderid = @FolderId and  c.active = 1   for xml PATH('permission'), type 
		) as 'permissions',
		(
			Select gg.groupId,gg.groupName from ResourceGroups rr inner join Groups gg on rr.GroupId = gg.GroupId 
			where rr.ResourceId = c.contextid 
			and rr.Active = 1
			and gg.Active = 1 
			for xml PATH('group'), type
		) as 'groups'
      from contexts c
      inner join ResourceGroups crg on  crg.active = c.active and crg.resourceid = c.contextid
      inner join Groups cg on cg.groupId = crg.groupId and  cg.active = c.active
      inner join grouproles cgr on  cgr.groupid = crg.groupid and  cgr.active = c.active    
      inner join roles cr on cr.roleid = cgr.roleid and  cr.active = c.active         
      inner join usergroups cug on cug.groupid = crg.groupid and  cug.active = c.active
      inner join users cu on cu.userid = cug.userid and cu.username = @UserName and cu.active = c.active
      where c.folderid = @FolderId  and c.active = 1
      GROUP BY  c.ContextId, c.DisplayName, c.InternalName,  c.Description, c.CacheConnStr,c.Active,c.FolderId
      for xml PATH('context'), ROOT('contexts'), type, ELEMENTS XSINIL
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spgContext]    Script Date: 04/16/2015 17:44:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Atul Srivastava>
-- Create date: <11-June-2014>
-- Modified Date <18-Jul-2014> Added folder id in select
-- Description:	<Selecting Context details>

-- ===========================================================
CREATE PROCEDURE [dbo].[spgContext] 
(
	@SiteId INT
)	 
AS
BEGIN TRY
	BEGIN
		SELECT ContextId, DisplayName, InternalName, Description, CacheConnStr, SiteId,FolderId, Active 
		FROM Contexts WHERE Active =1 AND SiteId=@SiteId

	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spgNames]    Script Date: 04/16/2015 17:44:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Gagan Dhamija>
-- Create date: <19-Sep-2014>
-- Description:	<Getting Names>

-- =============================================
CREATE  PROCEDURE [dbo].[spgNames] 
 (
 	@GraphId uniqueidentifier
 )
AS
BEGIN TRY
	BEGIN

	   select GraphName, Applications.ApplicationId as AppId, Applications.InternalName as AppName, Contexts.InternalName as ScopeName from Graphs
       join Applications on Applications.ApplicationId = Graphs.ApplicationId
       join Contexts on Contexts.ContextId = Applications.ContextId
       where UPPER(GraphId) = UPPER(@GraphId)
  		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spiContextAfterDragAndDrop]    Script Date: 04/16/2015 17:44:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================  
-- Author:  Atul Srivastava 
-- Create date: <25-Mar-2015>  
-- Description: <Inserting Context details after drag and drop>  
-- =============================================  
  
CREATE PROCEDURE [dbo].[spiContextAfterDragAndDrop]   
(  
 --@UserName VARCHAR(100),  
 @DisplayName NVARCHAR(255),  
 @InternalName NVARCHAR(255),  
 @Description NVARCHAR(255),  
 @CacheConnStr NVARCHAR(255),  
 @FolderId UNIQUEIDENTIFIER,  
 @GroupList XML,
 @NewContextId UNIQUEIDENTIFIER OUTPUT,
 @IsExist TINYINT OUTPUT
     
)    
AS 

BEGIN TRY
	BEGIN TRANSACTION;
	
	BEGIN		
		--Duplicate siblings are not allowed
		IF NOT EXISTS(SELECT TOP 1 * FROM Contexts WHERE DisplayName = @DisplayName AND InternalName = @InternalName AND Active = 1 AND FolderId = @FolderId)
		BEGIN					
			DECLARE @ContextId UNIQUEIDENTIFIER = NEWID()  
			DECLARE @ResourceTypeId INT = 1 --See ResourceType for detail
			SET   @NewContextId =@ContextId
			INSERT INTO Contexts(ContextId, DisplayName, InternalName, Description, CacheConnStr, FolderId)
			VALUES(@ContextId, @DisplayName, @InternalName, @Description, @CacheConnStr, @FolderId)
			
			SELECT       
			nref.value('groupId[1]', 'int') GroupId      
			INTO #Groups FROM   @GroupList.nodes('//group') AS R(nref) 

			DECLARE @isXSitesAdminGroupPresent BIT
			
			INSERT INTO [ResourceGroups]
		   ([ResourceId]
		   ,[GroupId]
		   ,[ResourceTypeId])
			SELECT 
				@ContextId,
				tg.GroupId,
				@ResourceTypeId
			FROM  Groups g
			INNER JOIN #Groups tg ON g.GroupId = tg.GroupId		
				
			-- Inserting XSitesAdminGroup id for resource in ResourceGroups table if it has not been inserted already
			IF NOT EXISTS(SELECT top 1 * FROM ResourceGroups rg INNER JOIN Groups g ON rg.GroupId = g.GroupId WHERE g.GroupName = 'XSitesAdminGroup' AND rg.ResourceId = @ContextId AND rg.ResourceTypeId = @ResourceTypeId AND rg.Active = 1)
			BEGIN
				INSERT INTO [ResourceGroups]
				   ([ResourceId]
				   ,[GroupId]
				   ,[ResourceTypeId])
				SELECT 
					@ContextId,
					g.GroupId,
					@ResourceTypeId
				FROM Groups g
				WHERE g.GroupName = 'XSitesAdminGroup'
			END
		
		--SELECT  '1'--'Context added successfully!'
		
		SET @IsExist = 1 --'Context added successfully!'
	END--end of if		
	ELSE
		--SELECT  '0'--'Context with this name already exists!'
		SET @IsExist = 0  --'Context with this name already exists!'
	END
	
	COMMIT TRANSACTION;
END TRY
BEGIN CATCH
	ROLLBACK TRANSACTION;
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH;


 
--BEGIN TRY  
-- BEGIN TRANSACTION;  
   
-- BEGIN    
--  --Duplicate siblings are not allowed  
--  IF NOT EXISTS(SELECT TOP 1 * FROM Contexts WHERE DisplayName = @DisplayName AND InternalName = @InternalName AND Active = 1 AND FolderId = @FolderId)  
--  BEGIN       
--   DECLARE @ContextId UNIQUEIDENTIFIER = NEWID()  
--   SET   @NewContextId =@ContextId
--   DECLARE @ResourceTypeId INT = 1 --See ResourceType for detail  
     
--   INSERT INTO Contexts(ContextId, DisplayName, InternalName, Description, CacheConnStr, FolderId)  
--   VALUES(@ContextId, @DisplayName, @InternalName, @Description, @CacheConnStr, @FolderId)  
     
--   SELECT         
--   nref.value('groupId[1]', 'int') GroupId        
--   INTO #Groups FROM   @GroupList.nodes('//group') AS R(nref)   
  
--   DECLARE @isXSitesAdminGroupPresent BIT  
     
--   SET @isXSitesAdminGroupPresent = (  
--    SELECT  
--     CASE  
--      WHEN COUNT(*) = 0   
--      THEN 0   
--      ELSE 1   
--     END  
--    FROM UserGroups ug   
--     INNER JOIN Users u ON ug.UserId = u.UserId AND u.UserName = @UserName AND u.Active = 1 AND ug.Active = 1  
--     INNER JOIN #Groups tg ON ug.GroupId = tg.GroupId  
--     INNER JOIN Groups g ON g.GroupId = ug.GroupId AND g.GroupName = 'XSitesAdminGroup')  
     
--   INSERT INTO [ResourceGroups]  
--     ([ResourceId]  
--     ,[GroupId]  
--     ,[ResourceTypeId])  
--   SELECT   
--    @ContextId,  
--    tg.GroupId,  
--    @ResourceTypeId  
--   FROM  UserGroups ug   
--     INNER JOIN Users u ON ug.UserId = u.UserId  
--     AND u.UserName = @UserName  
--     AND u.Active = 1  
--     AND ug.Active = 1  
--     INNER JOIN #Groups tg ON ug.GroupId = tg.GroupId    
      
--   -- Inserting XSitesAdminGroup id for resource in ResourceGroups table if it has not been inserted already  
--   IF @isXSitesAdminGroupPresent = 0  
--   BEGIN  
--    INSERT INTO [ResourceGroups]  
--       ([ResourceId]  
--       ,[GroupId]  
--       ,[ResourceTypeId])  
--    SELECT   
--     @ContextId,  
--     g.GroupId,  
--     @ResourceTypeId  
--    FROM Groups g  
--    WHERE g.GroupName = 'XSitesAdminGroup'  
--   END  
    
--  SET @IsExist = 1--'Context added successfully!'  
-- END--end of if    
-- ELSE  
--  SET @IsExist =  '0'--'Context with this name already exists!'  
-- END  
   
-- COMMIT TRANSACTION;  
--END TRY  
--BEGIN CATCH  
-- ROLLBACK TRANSACTION;  
--    SELECT 'Error occured at database: ' + ERROR_MESSAGE()   
--END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spiContext]    Script Date: 04/16/2015 17:44:54 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Abhishek Pandey>
-- Create date: <19-Mar-2015>
-- Description:	<Inserting Context details>
-- Last Modified By: <Abhishek Pandey>
-- Modification Date: <26-Mar-2015>
-- =============================================

CREATE PROCEDURE [dbo].[spiContext] 
(
	@DisplayName NVARCHAR(255),
	@InternalName NVARCHAR(255),
	@Description NVARCHAR(255),
	@CacheConnStr NVARCHAR(255),
	@FolderId UNIQUEIDENTIFIER,
	@GroupList XML   
)	 
AS
BEGIN TRY
	BEGIN TRANSACTION;
	
	BEGIN		
		--Duplicate siblings are not allowed
		IF NOT EXISTS(SELECT TOP 1 * FROM Contexts WHERE DisplayName = @DisplayName AND InternalName = @InternalName AND Active = 1 AND FolderId = @FolderId)
		BEGIN					
			DECLARE @ContextId UNIQUEIDENTIFIER = NEWID()  
			DECLARE @ResourceTypeId INT = 1 --See ResourceType for detail
			
			INSERT INTO Contexts(ContextId, DisplayName, InternalName, Description, CacheConnStr, FolderId)
			VALUES(@ContextId, @DisplayName, @InternalName, @Description, @CacheConnStr, @FolderId)
			
			SELECT       
			nref.value('groupId[1]', 'int') GroupId      
			INTO #Groups FROM   @GroupList.nodes('//group') AS R(nref) 

			INSERT INTO [ResourceGroups]
			(
			   [ResourceId],
			   [GroupId],
			   [ResourceTypeId]
			)
			SELECT 
				@ContextId,
				tg.GroupId,
				@ResourceTypeId
			FROM  Groups g
			INNER JOIN #Groups tg ON g.GroupId = tg.GroupId		
				
			-- Inserting XSitesAdminGroup id for resource in ResourceGroups table if it has not been inserted already
			IF NOT EXISTS(SELECT TOP 1 * FROM ResourceGroups rg INNER JOIN Groups g ON rg.GroupId = g.GroupId WHERE g.GroupName = 'XSitesAdminGroup' AND rg.ResourceId = @ContextId AND rg.ResourceTypeId = @ResourceTypeId AND rg.Active = 1)
			BEGIN
				INSERT INTO [ResourceGroups]
				(
				   [ResourceId],
				   [GroupId],
				   [ResourceTypeId]
				)
				SELECT 
					@ContextId,
					g.GroupId,
					@ResourceTypeId
				FROM Groups g
				WHERE g.GroupName = 'XSitesAdminGroup'
			END
		
		SELECT  '1'--'Context added successfully!'
	END--end of if		
	ELSE
		SELECT  '0'--'Context with this name already exists!'
	END
	
	COMMIT TRANSACTION;
END TRY
BEGIN CATCH
	ROLLBACK TRANSACTION;
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spgApplication]    Script Date: 04/16/2015 17:44:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <11-June-2014>
-- Description:	<Selecting Application>

-- =============================================
CREATE  PROCEDURE [dbo].[spgApplication] 
(
	@ScopeInternalName NVARCHAR(255),
	@SiteId INT
)
 
AS
BEGIN TRY
	BEGIN
		
		DECLARE @ContextId uniqueidentifier
		
		SELECT @ContextId=Contextid FROM Contexts WHERE InternalName=@ScopeInternalName AND SiteId=@Siteid
		
		IF @ContextId IS NOT NULL
			BEGIN
				SELECT InternalName,DisplayName,[Description],DXFRUrl FROM Applications
					WHERE ContextId=@ContextId AND SiteId=@Siteid AND Active=1
					
			END
		ELSE
			BEGIN
				SELECT 'Scope Not Found'
			END
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spdParentAndItsAllChildren]    Script Date: 04/16/2015 17:44:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE proc [dbo].[spdParentAndItsAllChildren] --'810021A2-DD14-4D38-8CDD-8F10EB2A43B1','Folder'
(
 @ParameterResourceID uniqueidentifier,--Guid of the resource to be deleted
 @ParameterResourceName varchar(20)-- Possible values are >> Folder,Context,Application,Graph
)
as
Begin
Set NoCount On;
/*
if @ParameterResourceName contains
	Folder > folder and all its direct or indirect children (folders, contexts) will be deleted (logically) 
	Context > context and all its direct or indirect children (Application) will be deleted (logically)
	Application > application and all its direct or indirect children (Graphs, Bindings etc) will be deleted (logically) 
    Graph > graph will be deleted (logically) 
*/

/*The idea behind the logic is, first We will get all the ResouceId's (that needs to be deleted like FolderID, ContextID etc.)
  in a separate temporary table which will contain ResourceName (ResourceNames are nothing but hardcoded values like:
  Folder,Context,Application,Graph) and it’s ID and then we will perform our delete operation */


--This will contain all the resouces to be deleted
Declare @ResourcesToBeDeleted table
(
	ResouceID uniqueidentifier,
	ResourceName varchar(20)
)

--Constant Variables (Resources) 
Declare @Folder varchar(20) = 'Folder'
Declare @Context varchar(20) = 'Context'
Declare @Application varchar(20) = 'Application'
Declare @Graph varchar(20) = 'Graph'


--*******************Section STARTS - Making initial entry in @ResourcesToBeDeleted table******************
If @ParameterResourceName = @Folder --Getting all the folder and its children up to nth level
Begin
		
		WITH FolderStructure AS
		( 
		--anchor select
		SELECT FolderId
		FROM Folders
		WHERE  FolderId = @ParameterResourceID
		UNION ALL
		--recursive select
		SELECT F.FolderId
		FROM Folders F 
		INNER JOIN FolderStructure FS ON F.ParentFolderId = FS.FolderId where F.Active = 1
		)
		
		Insert into @ResourcesToBeDeleted
		Select FolderId,@Folder from FolderStructure
End

Else If @ParameterResourceName = @Context --Getting Context to be deleted
Begin
		Insert into @ResourcesToBeDeleted
		Select ContextId,@Context from Contexts where ContextId = @ParameterResourceID		
End

Else If @ParameterResourceName = @Application --Getting Application to be deleted
Begin
		Insert into @ResourcesToBeDeleted
		Select ApplicationId,@Application from Applications where ApplicationId = @ParameterResourceID
End

Else If @ParameterResourceName = @Graph --Getting Graph to be deleted
Begin
		Insert into @ResourcesToBeDeleted
		Select GraphId,@Graph from Graphs where GraphId = @ParameterResourceID
End

--*******************Section ENDS - Making initial entry in @ResourcesToBeDeleted table******************


--*******************Section STARTS - Hierachial delete******************

--If we are deleting folder then all its contexts should also be deleted
If Exists(Select Top 1 ResourceName from @ResourcesToBeDeleted Where ResourceName = @Folder)
Begin	
		--Delete(logical delete) folder and its child folders up to nth level
		Update F 
			Set F.Active = 0 
		from Folders F Inner Join @ResourcesToBeDeleted RTD 
			On F.FolderId = RTD.ResouceID
			And RTD.ResourceName = @Folder--this check not required at this level, but applied for understandability
		
		
		--Insert all contexts (associated with the folder) in temp table
		Insert into @ResourcesToBeDeleted
		Select ContextId,@Context from @ResourcesToBeDeleted RTD 
							Inner join Contexts C on RTD.ResouceID = C.FolderId 
							And RTD.ResourceName = @Folder
							And C.Active = 1
							
							
End

--If we are deleting a Context then all its applications should also be deleted
If Exists(Select Top 1 ResourceName from @ResourcesToBeDeleted Where ResourceName = @Context)
Begin
		--Delete(logical delete) context(s)
		Update C 
			Set C.Active = 0 
		from Contexts C Inner Join @ResourcesToBeDeleted RTD 
			On C.ContextId = RTD.ResouceID
			And RTD.ResourceName = @Context

		
		--Insert all applications (associated with the context) in temp table
		Insert into @ResourcesToBeDeleted
		Select ApplicationId,@Application from @ResourcesToBeDeleted RTD 
							Inner join Applications A on RTD.ResouceID = A.ContextId
							And RTD.ResourceName = @Context
							And A.Active = 1
							
							
End

--If we are deleting an Application then all its graphs should also be deleted
If Exists(Select Top 1 ResourceName from @ResourcesToBeDeleted Where ResourceName = @Application)
Begin

		--Delete(logical delete) application(s)
		Update A 
			Set A.Active = 0 
		from Applications A Inner Join @ResourcesToBeDeleted RTD 
			On A.ApplicationId = RTD.ResouceID
			And RTD.ResourceName = @Application
			
			
		--Insert all graphs (associated with the application) in temp table	
		Insert into @ResourcesToBeDeleted
		Select GraphId,@Graph from @ResourcesToBeDeleted RTD 
							Inner join Graphs G on RTD.ResouceID = G.ApplicationId
							And RTD.ResourceName = @Application
							And G.Active = 1
							
							
End

If Exists(Select Top 1 ResourceName from @ResourcesToBeDeleted Where ResourceName = @Graph)
Begin

		--Delete(logical delete) graph(s)
		Update G 
			Set G.Active = 0 
		from Graphs G Inner Join @ResourcesToBeDeleted RTD 
			On G.GraphId = RTD.ResouceID
			And RTD.ResourceName = @Graph
			
End


/* Delete(logical delete) all resources from ResourceGroups table
   because all resources has entry in this table*/
Update RG 
	Set RG.Active = 0 
from ResourceGroups RG Inner Join @ResourcesToBeDeleted RTD 
	On RG.ResourceId = RTD.ResouceID


-- Delete(logical delete) Binding information of applicaiton(s)
Update BC 
	Set BC.Active = 0 
from BindingConfig BC Inner Join @ResourcesToBeDeleted RTD 
	On BC.ApplicationId = RTD.ResouceID
	And RTD.ResourceName = @Application


/* Delete(logical delete) DataFilters 
   Note: This is doubtful and can be changed later as datafilters can only be applied on DataObjects
   which are not available in @ResourcesToBeDeleted table, but for the time being running this
   query will have no impact */
Update DF 
	Set DF.Active = 0 
from DataFilters DF Inner Join @ResourcesToBeDeleted RTD 
	On DF.DataFilterId = RTD.ResouceID


--*******************Section ENDS - Hierachial delete******************

--For testing
--Select * from @ResourcesToBeDeleted     



End
GO
/****** Object:  Table [dbo].[KeyProperty]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[KeyProperty](
	[DataObjectId] [uniqueidentifier] NOT NULL,
	[KeyPropertyName] [nvarchar](250) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Commodity]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Commodity](
	[ContextId] [uniqueidentifier] NOT NULL,
	[CommodityId] [uniqueidentifier] NOT NULL,
	[CommodityName] [nvarchar](255) NULL,
	[Active] [tinyint] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[CommodityId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DataRelationships]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DataRelationships](
	[RelationshipId] [uniqueidentifier] NOT NULL,
	[DataObjectID] [uniqueidentifier] NOT NULL,
	[RelationShipName] [nvarchar](50) NULL,
	[RelatedObjectName] [nvarchar](50) NULL,
	[RelationShipType] [nvarchar](50) NULL,
 CONSTRAINT [PK_DataRelationships] PRIMARY KEY CLUSTERED 
(
	[RelationshipId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DataProperties]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DataProperties](
	[DataPropertyId] [uniqueidentifier] NOT NULL,
	[DataObjectId] [uniqueidentifier] NULL,
	[ColumnName] [nvarchar](50) NULL,
	[PropertyName] [nvarchar](50) NULL,
	[DataType] [nvarchar](50) NULL,
	[DataLength] [int] NULL,
	[IsNullable] [bit] NULL,
	[KeyType] [nvarchar](50) NULL,
	[ShowOnIndex] [bit] NULL,
	[NumberOfDecimals] [int] NULL,
	[IsReadOnly] [bit] NULL,
	[ShowOnSearch] [bit] NULL,
	[IsHidden] [bit] NULL,
	[Description] [nvarchar](250) NULL,
	[ReferenceType] [nvarchar](50) NULL,
	[IsVirtual] [bit] NULL,
	[Precision] [int] NULL,
	[Scale] [int] NULL,
	[PickListId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_DataProperties] PRIMARY KEY CLUSTERED 
(
	[DataPropertyId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Exchanges]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Exchanges](
	[ExchangeId] [uniqueidentifier] NOT NULL,
	[CommodityId] [uniqueidentifier] NOT NULL,
	[SourceGraphId] [uniqueidentifier] NOT NULL,
	[DestinationGraphId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](255) NULL,
	[PoolSize] [int] NOT NULL,
	[XTypeAdd] [nvarchar](25) NULL,
	[XTypeChange] [nvarchar](25) NULL,
	[XTypeSync] [nvarchar](25) NULL,
	[XTypeDelete] [nvarchar](25) NULL,
	[XTypeSetNull] [nvarchar](25) NULL,
	[Active] [tinyint] NOT NULL,
 CONSTRAINT [PK__Exchange__72E6008B0F975522] PRIMARY KEY CLUSTERED 
(
	[ExchangeId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[spdContext]    Script Date: 04/16/2015 17:44:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Abhishek Pandey>
-- Create date: <23-Mar-2015>
-- Description:	<Deleting Context >
-- =============================================

CREATE PROCEDURE [dbo].[spdContext] 
(
		@ContextId uniqueidentifier
)	 
AS
BEGIN TRY
	BEGIN TRANSACTION;

	BEGIN	
		Exec spdParentAndItsAllChildren @ParameterResourceID = @ContextId, @ParameterResourceName = 'Context'
	
		Select '1'--'Context deleted successfully!'						
	END

	COMMIT TRANSACTION;
END TRY
BEGIN CATCH
	ROLLBACK TRANSACTION;
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spdCommodity]    Script Date: 04/16/2015 17:44:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ================================
-- Author:		<Deepak Kumar>
-- Create date: <11-Sep-2014>
-- Description:	<delete commodity>
-- ================================
create PROCEDURE [dbo].[spdCommodity] 
(
	@CommodityId uniqueidentifier
)	 
AS

	BEGIN
	
	Update Commodity
		Set Active = 0
	Where CommodityId = @CommodityId 


	Update ResourceGroups
	Set   Active = 0
	Where ResourceId = @CommodityId 
		 
	END
GO
/****** Object:  StoredProcedure [dbo].[spdApplication]    Script Date: 04/16/2015 17:44:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <09-June-2014>
-- Description:	<Deleting Context>
-- Last Modified By: <Abhishek Pandey>
-- Modification Date: <13-apr-2015>
-- =============================================

 CREATE PROCEDURE [dbo].[spdApplication] 
(
		@ApplicationId uniqueidentifier
)	 
AS
BEGIN TRY
	BEGIN TRANSACTION;
	
	BEGIN
		EXEC spdParentAndItsAllChildren @ParameterResourceID = @ApplicationId, @ParameterResourceName = 'Application'
		
		-- Can be uncommented if required. But as applications are set as inactive at delete so, we should keep the appsettings in the database
		--DELETE FROM ApplicationSettings WHERE ApplicationId = @ApplicationId
			
		UPDATE BindingConfig
		SET Active = 0
		WHERE ApplicationId = @ApplicationId
			 
		SELECT '1'--'Application deleted successfully!'
	END
	
	COMMIT TRANSACTION;
END TRY
BEGIN CATCH
	ROLLBACK TRANSACTION;
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO
/****** Object:  Table [dbo].[PropertyMap]    Script Date: 04/16/2015 17:44:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PropertyMap](
	[RelationshipID] [uniqueidentifier] NULL,
	[DataPropertyName] [nvarchar](50) NULL,
	[RelatedPropertyName] [nvarchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[spdGraph]    Script Date: 04/16/2015 17:44:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Gagan Dhamija>
-- Create date: <17-Jul-2014>
-- Description:	<Deleting Graphs>
-- ===========================================================
CREATE PROCEDURE [dbo].[spdGraph] 
(
	@GraphId uniqueidentifier
)	 
AS
BEGIN TRY
	BEGIN
	
	Exec spdParentAndItsAllChildren @ParameterResourceID = @GraphId, @ParameterResourceName = 'Graph'
	
	--Update Graphs
	--	Set Active = 0
	--Where GraphId = @GraphId 


	--Update ResourceGroups
	--Set   Active = 0
	--Where ResourceId = @GraphId
							

		Select  '1'--'Graph deleted successfully!'
	END

END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spdFolder]    Script Date: 04/16/2015 17:44:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Abhishek Pandey>
-- Create date: <23-Mar-2015>
-- Description:	<Deleting folder>
-- ===========================================================

CREATE PROCEDURE [dbo].[spdFolder]
(
	@FolderId uniqueidentifier
)	 
AS
BEGIN TRY
	BEGIN TRANSACTION;
	
	BEGIN		
		Exec spdParentAndItsAllChildren @ParameterResourceID = @FolderId, @ParameterResourceName = 'Folder'
		
		Select '1'--'Folder deleted successfully!'						
	END

	COMMIT TRANSACTION;
END TRY
BEGIN CATCH
	ROLLBACK TRANSACTION;
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spiCommodity]    Script Date: 04/16/2015 17:44:54 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Deepak Kumar>
-- Create date: <10-Sep-2014>
-- Description:	<Inserting commodity>
-- ===========================================================
CREATE PROCEDURE [dbo].[spiCommodity] 
(
	--@UserName varchar(100),--'vmallire'
	@ContextId uniqueidentifier,
	@CommodityName nvarchar(255),
	@GroupList xml
)	 
AS

BEGIN TRY

	BEGIN
	--Siblings are not allowed
	If not Exists(Select top 1 * from Commodity Where CommodityName = @CommodityName and Active = 1 and ContextId = @ContextId)
		Begin
	
	
	
			Declare @CommodityId uniqueidentifier = NewID()  
			Declare @ResourceTypeId int = 5 --See ResourceType for detail
			
			INSERT INTO [Commodity]
				   ([ContextId]
				   ,[CommodityId]
				   ,[CommodityName]				
				   )
			Select 
				@ContextId,
				@CommodityId,
				@CommodityName			

			SELECT       
			nref.value('groupId[1]', 'int') GroupId      
			INTO #Groups FROM   @GroupList.nodes('//group') AS R(nref) 

			
			INSERT INTO [ResourceGroups]
				   ([ResourceId]
				   ,[GroupId]				   
				   ,[ResourceTypeId]
				   )
			Select 
				@CommodityId,
				tg.GroupId,			
				@ResourceTypeId
				FROM  Groups g
			INNER JOIN #Groups tg ON g.GroupId = tg.GroupId	
			--from  UserGroups ug 
			--		inner join Users u on ug.UserId = u.UserId
			--			And u.UserName = @UserName					
			--			And u.Active = 1
			--			And ug.Active = 1					
			--		inner join #Groups tg on ug.GroupId = tg.GroupId
			
			-- Inserting XSitesAdminGroup id for resource in ResourceGroups table if it has not been inserted already
			IF NOT EXISTS(SELECT top 1 * FROM ResourceGroups rg INNER JOIN Groups g ON rg.GroupId = g.GroupId WHERE g.GroupName = 'XSitesAdminGroup' AND rg.ResourceId = @CommodityId AND rg.ResourceTypeId = @ResourceTypeId AND rg.Active = 1)
			BEGIN
				INSERT INTO [ResourceGroups]
				   ([ResourceId]
				   ,[GroupId]
				   ,[ResourceTypeId])
				SELECT 
					@CommodityId,
					g.GroupId,
					@ResourceTypeId
				FROM Groups g
				WHERE g.GroupName = 'XSitesAdminGroup'
			END

			Select  '1'--'Commodity added successfully!'
			
		end--end of if		
		Else
			Select  '0'--'Commodity with this name already exists!'					

	END

END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spgCommoditiesByUser]    Script Date: 04/16/2015 17:44:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Vidisha>
-- Create date: <01-Sep-2014>
-- Description:	<Selecting commodities based on users>
-- ===========================================================
CREATE PROCEDURE [dbo].[spgCommoditiesByUser]  --'vmallire',1,'2567c504-133d-41b9-8780-7bfd73840ed3'  
(
	@UserName varchar(100),--'vmallire'
	@ContextId uniqueidentifier--'2567c504-133d-41b9-8780-7bfd73840ed3'
)	 
AS

	BEGIN
	 
		WITH XMLNAMESPACES ( DEFAULT 'http://www.iringtools.org/library' ) 
		select 
		c.ContextId as contextId, 
		c.CommodityId as commodityId, 
		c.CommodityName as commodityName, 
		c.Active as active,
		(
		select distinct  p.PermissionId as permissionId,  p.PermissionName as permissionName, p.PermissionDesc as permissionDesc, p.Active as active from permissions p
		inner join ResourceGroups rg on  rg.active = c.active and rg.resourceid = c.CommodityId
		inner join Groups g on g.groupId = rg.groupId and g.active = g.active
		inner join grouproles gr on  gr.groupid = rg.groupid and  gr.active = g.active    
		inner join roles r on r.roleid = gr.roleid and  r.active = g.active     
		inner join rolepermissions rp on rp.permissionid = p.permissionid and rp.roleid = r.roleid and  rp.active = g.active          
		inner join usergroups ug on ug.groupid = rg.groupid  and ug.active = g.active
		inner join users u on u.userid = ug.userid and u.username = @UserName and u.active = g.active
		where g.active = 1   for xml PATH('permission'), type 
		) as 'permissions',
		(
			Select gg.groupId,gg.groupName from ResourceGroups rr inner join Groups gg on rr.GroupId = gg.GroupId 
			where rr.ResourceId = c.CommodityId 
			and rr.Active = 1
			and gg.Active = 1 
			for xml PATH('group'), type
		) as 'groups'
		
		from Commodity c
			inner join ResourceGroups crg on crg.active = c.active and crg.resourceid = c.commodityId
			inner join Groups cg on cg.groupId = crg.groupId and  cg.active = c.active
			inner join grouproles cgr on  cgr.groupid = crg.groupid and cgr.active = c.active    
			inner join roles cr on cr.roleid = cgr.roleid and  cr.active = c.active         
			inner join usergroups cug on cug.groupid = crg.groupid and cug.active = c.active
			inner join users cu on cu.userid = cug.userid and cu.username = @UserName and  cu.active = c.active
		where c.contextId = @ContextId and  c.active = 1
		Group BY c.ContextId,c.CommodityId, c.CommodityName, c.Active
		for xml PATH('commodity'), ROOT('commodities'), type, ELEMENTS XSINIL
 

	END
GO
/****** Object:  StoredProcedure [dbo].[spiEntityAfterDropTest]    Script Date: 04/16/2015 17:44:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spiEntityAfterDropTest]
	@ResourceType NVARCHAR(255),
	@DroppedEntityId UNIQUEIDENTIFIER,  
	@DestinationParentEntityId UNIQUEIDENTIFIER,
	@SiteId INT = NULL,
	@PlatFormId INT = NULL,
	@UserName VARCHAR(100)
AS  
BEGIN    
   -- Insert statements for procedure here  
   DECLARE  @FolderName NVARCHAR(50), @GroupList XML,@GroupId int,@DisplayName NVARCHAR(255),
   @InternalName NVARCHAR(255),@Description NVARCHAR(255),@CacheConnStr NVARCHAR(255)  
   
   IF @ResourceType= 'Folder'
   BEGIN
		SET @UserName ='asrivas2' -- this is temporaty will remove later
		SELECT @FolderName =foldername FROM Folders WHERE FolderId = @DroppedEntityId
		--SELECT @FolderName
			
		SET @GroupList = (select GroupId from ResourceGroups where ResourceId = @DroppedEntityId
		FOR XML PATH('group'),root('groups'))
		EXEC spiFolder @UserName, @SiteId,@PlatFormId,@DestinationParentEntityId,@FolderName,@GroupList
		
		--Contexts
		DECLARE @Contexts CURSOR
			SET @Contexts = CURSOR FAST_FORWARD FOR
			SELECT displayname,internalname,description,CacheConnStr FROM Contexts WHERE FolderId= @DroppedEntityId
			OPEN @Contexts
			FETCH NEXT FROM @Contexts
			INTO @DisplayName,@InternalName,@Description,@CacheConnStr 
			EXEC spiContext @UserName,@DisplayName,@InternalName,@Description,@CacheConnStr,@DestinationParentEntityId,@GroupList
				WHILE @@FETCH_STATUS = 0
				BEGIN
					EXEC spiContext @UserName,@DisplayName,@InternalName,@Description,@CacheConnStr,@DestinationParentEntityId,@GroupList
					FETCH NEXT FROM @Contexts INTO @DisplayName,@InternalName,@Description,@CacheConnStr 
				END
		CLOSE @Contexts
		DEALLOCATE @Contexts
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
   END
   
   
    
END
GO
/****** Object:  StoredProcedure [dbo].[spiEntityAfterDrop]    Script Date: 04/16/2015 17:44:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--=============================================
 --Author:		  Atul Srivastava
 --Create date:   23-Mar-2015
 --Modified By    <>
 --Modified date: <>
 --Description:	  Implementing drag and drop.
 --=============================================


 CREATE    PROCEDURE [dbo].[spiEntityAfterDrop] --'folder','E8FC94A7-777F-4A2D-9BEF-DCF75090022B','9A073797-43E6-464B-BD80-49BA46B82FCF',1,1
	@ResourceType NVARCHAR(255),
	@DroppedEntityId UNIQUEIDENTIFIER,  
	@DestinationParentEntityId UNIQUEIDENTIFIER,
	@SiteId INT = NULL,
	@PlatFormId INT = NULL
	--@UserName VARCHAR(100)
AS  
BEGIN    
   -- Insert statements for procedure here  
   DECLARE  @FolderName NVARCHAR(50), @GroupList XML,@GroupId int,@DisplayName NVARCHAR(255),@ContextId UNIQUEIDENTIFIER,
   @InternalName NVARCHAR(255),@Description NVARCHAR(255),@CacheConnStr NVARCHAR(255), @NewContextId UNIQUEIDENTIFIER,
   @IsExist TINYINT,@NewFolderId UNIQUEIDENTIFIER,@IsFolderExist TINYINT,@ApplicationId UNIQUEIDENTIFIER,@DXFRUrl NVARCHAR(510),
   @AppDisplayName NVARCHAR(255),@AppInternalName NVARCHAR(255),@AppDescription NVARCHAR(255),@Assembly NVARCHAR(1100)
   
   DECLARE @NewAppId UNIQUEIDENTIFIER ,@IsAppExist TINYINT 
   
   IF @ResourceType= 'Folder'
   BEGIN
		--SET @UserName ='asrivas2' -- this is temporaty will remove later
		SELECT @FolderName =foldername FROM Folders WHERE FolderId = @DroppedEntityId
		--SELECT @FolderName
			
		SET @GroupList = (select GroupId from ResourceGroups where ResourceId = @DroppedEntityId
		FOR XML PATH('group'),root('groups'))
		
		--EXEC spiFolderAfterDragAndDrop @UserName, @SiteId,@PlatFormId,@DestinationParentEntityId,@FolderName,@GroupList,@NewFolderId OUTPUT,@IsFolderExist OUTPUT
		EXEC spiFolderAfterDragAndDrop @SiteId,@PlatFormId,@DestinationParentEntityId,@FolderName,@GroupList,@NewFolderId OUTPUT,@IsFolderExist OUTPUT
		
		
		--Contexts
		IF ((@NewFolderId IS NOT NULL) AND (@IsFolderExist <> 0))  --dont add context if duplicate folder is found
		BEGIN
			
			
			DECLARE @Contexts CURSOR
				SET @Contexts = CURSOR FAST_FORWARD FOR
				SELECT contextid,displayname,internalname,description,CacheConnStr FROM Contexts WHERE FolderId= @DroppedEntityId
				OPEN @Contexts
				FETCH NEXT FROM @Contexts INTO @ContextId,@DisplayName,@InternalName,@Description,@CacheConnStr 
				----EXEC spiContext @UserName,@DisplayName,@InternalName,@Description,@CacheConnStr,@DestinationParentEntityId,@GroupList
					WHILE @@FETCH_STATUS = 0
					BEGIN
						--EXEC spiContextAfterDragAndDrop @UserName,@DisplayName,@InternalName,@Description,@CacheConnStr,@DestinationParentEntityId,@GroupList,@NewContextId OUTPUT,@IsExist OUTPUT --Adding Context
						EXEC spiContextAfterDragAndDrop @DisplayName,@InternalName,@Description,@CacheConnStr,@NewFolderId,@GroupList,@NewContextId OUTPUT,@IsExist OUTPUT --Adding Context
						
						IF ((@NewContextId IS NOT NULL) AND (@IsExist <> 0)) --Adding Application   --- this is commented as development of adding application is under progress.
						BEGIN
								--Applications
		
									DECLARE @Application CURSOR
										SET @Application= CURSOR FAST_FORWARD FOR
										SELECT applicationid,displayname,internalname,description,DXFRUrl,Assembly FROM Applications WHERE contextid= @ContextId
										OPEN @Application
										FETCH NEXT FROM @Application
										INTO @ApplicationId,@AppDisplayName,@AppInternalName,@AppDescription,@DXFRUrl,@Assembly 
										----EXEC spiContext @UserName,@DisplayName,@InternalName,@Description,@CacheConnStr,@DestinationParentEntityId,@GroupList
											WHILE @@FETCH_STATUS = 0
											BEGIN
												EXEC spiApplicationAfterDragAndDrop @NewContextId,@AppDisplayName,@AppInternalName,@AppDescription,@DXFRUrl,@Assembly,@GroupList,@NewAppId OUTPUT,@IsAppExist OUTPUT
											--	SELECT @NewAppId
												FETCH NEXT FROM @Application INTO @ApplicationId,@AppDisplayName,@AppInternalName,@AppDescription,@CacheConnStr,@Assembly 
											END
									CLOSE @Application
									DEALLOCATE @Application
		
							
						END
									
						
						FETCH NEXT FROM @Contexts INTO @ContextId,@DisplayName,@InternalName,@Description,@CacheConnStr 
					END
			
			
			
			
			
			
			
			
			
			
			CLOSE @Contexts
			DEALLOCATE @Contexts
		END
		
	
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
   END
   
   
    
END
GO
/****** Object:  StoredProcedure [dbo].[spuCommodity]    Script Date: 04/16/2015 17:45:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ================================
-- Author:		<Deepak Kumar>
-- Create date: <11-Sep-2014>
-- Description:	<update commodity>
-- ================================
CREATE PROCEDURE [dbo].[spuCommodity] --'vmallire',1,'A6AC8CA5-662C-4249-9D1F-2B7CB962F2CB','dee1ii','4,5,1,6'
(
	--@UserName varchar(100),--'vmallire'
	@CommodityId uniqueidentifier,
	@CommodityName nvarchar(255),
	@GroupList xml,
	@ContextId uniqueidentifier
	
)	 
AS

BEGIN TRY  

		BEGIN
		
				If Exists(Select top 1 * from Commodity Where CommodityName = @CommodityName and CommodityId!= @CommodityId and ContextId= @ContextId and Active = 1)
					Begin
						Select '0' --Commodity with this name already exists
						return;
					End
				Else
					Begin
	
	
	
					Update Commodity
						Set CommodityName = @CommodityName
					Where CommodityId = @CommodityId 
							And Active = 1
					
					SELECT       
					nref.value('groupId[1]', 'int') GroupId      
					INTO #Groups FROM   @GroupList.nodes('//group') AS R(nref) 

					/*Below query can not be included in merge statement
					as it will inactive all the non matching rows, hence
					separate query has written*/
					Update ResourceGroups
					Set   Active = 0
					Where ResourceId = @CommodityId 
					
					Declare @ResourceTypeId int = 5 --See ResourceType for detail
					
					MERGE ResourceGroups AS T      
					USING #Groups AS S
					ON (T.GroupId = S.GroupId  AND T.ResourceId = @CommodityId)       
					WHEN NOT MATCHED BY TARGET       
						THEN INSERT(ResourceId,GroupId,ResourceTypeId) VALUES(@CommodityId,S.GroupId,@ResourceTypeId)
					WHEN MATCHED       
						THEN UPDATE SET T.Active = 1;    
					
					
					-- Updating XSitesAdminGroup id for resource in ResourceGroups table if it has not been updated already				
					UPDATE [ResourceGroups]
					SET Active = 1
					WHERE ResourceId = @CommodityId
					AND ResourceTypeId = @ResourceTypeId
					AND GroupId = (
						SELECT GroupId
						FROM Groups g
						WHERE g.GroupName = 'XSitesAdminGroup')
		 
				Select  '1'--'Commodity updated successfully!'	
			End	
	    END  
END TRY  
  
BEGIN CATCH  
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spuExchange]    Script Date: 04/16/2015 17:45:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Deepak Kumar>
-- Create date: <10-Sep-2014>
-- Description:	<updating Exchange>
-- ===========================================================
CREATE PROCEDURE [dbo].[spuExchange] 
(
	/*Note: All the nullable Fields in [Exchanges] table
	  needs to have default value to null as done below.
	  so that if value is not supplied in the parameter
	  then null will be assigned in corresspoing field
	*/
	--@UserName varchar(100),--'vmallire'
	@ExchangeId uniqueidentifier,
	@SourceGraphId	uniqueidentifier,
	@DestinationGraphId	uniqueidentifier,
	@Name	nvarchar(255),
	@Description	nvarchar(255) = null,
	@PoolSize	int,
	@XTypeAdd	nvarchar(25) = null,
	@XTypeChange	nvarchar(25) = null,
	@XTypeSync	nvarchar(25) = null,
	@XTypeDelete	nvarchar(25) = null,
	@XTypeSetNull	nvarchar(25) = null,
	@GroupList xml,
	@CommodityId uniqueidentifier
	
)	 
AS

BEGIN TRY  

		BEGIN
		
				If Exists(Select top 1 * from Exchanges Where Name = @Name and ExchangeId!= @ExchangeId and CommodityId= @CommodityId and Active = 1)
					Begin
						Select '0' --Exchange with this name already exists
						return;
					End
				Else
					Begin
	
	--select * from ResourceType
	
				UPDATE [Exchanges]
				   SET [ExchangeId] = @ExchangeId
					  ,[SourceGraphId] = @SourceGraphId
					  ,[DestinationGraphId] = @DestinationGraphId
					  ,[Name] = @Name
					  ,[Description] = @Description
					  ,[PoolSize] = @PoolSize
					  ,[XTypeAdd] = @XTypeAdd
					  ,[XTypeChange] = @XTypeChange
					  ,[XTypeSync] = @XTypeSync
					  ,[XTypeDelete] = @XTypeDelete
					  ,[XTypeSetNull] = @XTypeSetNull					
				 WHERE ExchangeId = @ExchangeId and Active = 1

				SELECT       
				nref.value('groupId[1]', 'int') GroupId      
				INTO #Groups FROM   @GroupList.nodes('//group') AS R(nref)

				/*Below query can not be included in merge statement
				as it will inactive all the non matching rows, hence
				separate query has written*/
				Update ResourceGroups
				Set   Active = 0
				Where ResourceId = @ExchangeId 

				
				Declare @ResourceTypeId int = 6 --See ResourceType for detail
				

				
				MERGE ResourceGroups AS T      
				USING #Groups AS S    
				ON (T.GroupId = S.GroupId AND  T.ResourceId = @ExchangeId)       
				WHEN NOT MATCHED BY TARGET       
					THEN INSERT(ResourceId,GroupId,ResourceTypeId) VALUES(@ExchangeId,S.GroupId,@ResourceTypeId)
				WHEN MATCHED       
					THEN UPDATE SET T.Active = 1;    

				-- Updating XSitesAdminGroup id for resource in ResourceGroups table if it has not been updated already				
				UPDATE [ResourceGroups]
				SET Active = 1
				WHERE ResourceId = @ExchangeId
				AND ResourceTypeId = @ResourceTypeId
				AND GroupId = (
					SELECT GroupId
					FROM Groups g
					WHERE g.GroupName = 'XSitesAdminGroup')	
					
											

				Select  '1'--'Exchange updated successfully!'	
			End	
	    END  
END TRY  
  
BEGIN CATCH  
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spiDictionary]    Script Date: 04/16/2015 17:44:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		  Deepak Kumar
-- Create date:   23-Mar-2015
-- Modified By    <>
-- Modified date: <>
-- Description:	  Saving datadictionary
-- =============================================
CREATE PROCEDURE [dbo].[spiDictionary]
	@rawXml xml
AS
BEGIN TRY

BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;



/*
Known issues
1. in alias dictionary
	<a:KeyValueOfstringstring> is not working 
	<KeyValueOfstringstring> is working 
2.	
*/


SELECT       

nref.value('applicationId[1]','uniqueidentifier') applicationId,
nref.value('isDBDictionary[1]','bit') isDBDictionary,
nref.value('enableSearch[1]','nvarchar(50)') enableSearch,
nref.value('enableSummary[1]','nvarchar(50)') enableSummary,
nref.value('dataVersion[1]','nvarchar(50)') dataVersion,
nref.value('description[1]','nvarchar(1000)') description,
nref.value('provider[1]','nvarchar(50)') provider,
nref.value('connectionString[1]','nvarchar(1000)') connectionString,
nref.value('schemaName[1]','nvarchar(50)') schemaName,
nref.query('dataObjects') dataObjects,
nref.query('picklists') picklists

into #dictionary FROM  @rawXML.nodes('//databaseDictionary') AS R(nref)

--select * from #dictionary
Declare @applicationId uniqueidentifier
Declare @dictionaryId uniqueidentifier = newid()
Select @applicationId = applicationId from #dictionary


--Delete dictionary before inserting - Section Starts**********************
		Begin

				select 
					DO.DataObjectId
				into #DeleteDataObjectsBeforeInsert from 
				Dictionary D inner join Applications A on D.ApplicationID = A.ApplicationId and A.ApplicationId = @applicationId
							 inner join DataObjects DO on DO.DictionaryId = D.Dictionaryid


				--Delete Section for KeyProperty Starts******************
							Delete d 
							from KeyProperty d inner join #DeleteDataObjectsBeforeInsert do on d.DataObjectId = do.DataObjectid
				--Delete Section for KeyProperty Ends******************


				--Delete Section for DataProperty Starts******************
							Begin
								Delete d 
								from AliasDictionary d inner join DataProperties dp on d.ResourceId = dp.DataPropertyId
														inner join #DeleteDataObjectsBeforeInsert do on dp.DataObjectId = do.DataObjectid
								
								Delete d 
								from DataProperties d inner join #DeleteDataObjectsBeforeInsert do on d.DataObjectId = do.DataObjectid
							End
				--Delete Section for DataProperty Ends******************


				--Delete Section for DataRelationships Starts******************
							Begin
								Delete d 
								from PropertyMap d inner join DataRelationships dr on d.RelationshipID = dr.RelationshipId
														inner join #DeleteDataObjectsBeforeInsert do on dr.DataObjectId = do.DataObjectid

								Delete d 
								from DataRelationships d inner join #DeleteDataObjectsBeforeInsert do on d.DataObjectId = do.DataObjectid
							End
				--Delete Section for DataRelationships Ends******************


				--Delete Section for DataObject Alias Dictionary Starts******************
							Delete d 
							from AliasDictionary d inner join #DeleteDataObjectsBeforeInsert do on d.ResourceId = do.DataObjectid
				--Delete Section for DataObject Alias Dictionary Ends******************


				--Delete Section for DataFilter Starts******************
							Begin
							
							--Delete Section for Expression Starts******************
								Begin
									Delete d 
									from ExpressionValues d inner join Expression e on d.ExpressionId = e.ExpressionId
															inner join DataFilters df on df.DataFilterId = e.DataFilterId
															inner join #DeleteDataObjectsBeforeInsert do on do.DataObjectId = df.ResourceId
									--Print 'ExpressionValues Deleted'						
									Delete e 
									from Expression e inner join DataFilters df on df.DataFilterId = e.DataFilterId
													  inner join #DeleteDataObjectsBeforeInsert do on do.DataObjectId = df.ResourceId
									--print 'Expression Deleted'				  
								End
							--Delete Section for Expression Ends******************					
							 
							--Delete Section for OrderExpression Starts******************
								Begin
															
									Delete oe 
									from OrderExpression oe inner join DataFilters df on df.DataFilterId = oe.DataFilterId
													  inner join #DeleteDataObjectsBeforeInsert do on do.DataObjectId = df.ResourceId
									--print 'Order Expression Deleted'				  				  
								End
							--Delete Section for OrderExpression Ends******************					 

							--Delete Section for RollupExpression Starts*******************
								Begin
															
									Delete ru 
									from [Rollup] ru inner join RollupExpression re on ru.RollupExpressionId = re.RollupExpressionId
													 inner join DataFilters df on df.DataFilterId  = re.DataFilterId
													 inner join #DeleteDataObjectsBeforeInsert do on do.DataObjectId = df.ResourceId
									--print 'Rollup Deleted'				 
									Delete re 
									from RollupExpression re inner join DataFilters df on df.DataFilterId = re.DataFilterId
													  inner join #DeleteDataObjectsBeforeInsert do on do.DataObjectId = df.ResourceId
									--print 'RollupExpression Deleted'				  				 
								End
							--Delete Section for RollupExpression Ends**********************			
							
							Delete df 
							from DataFilters df inner join #DeleteDataObjectsBeforeInsert do on do.DataObjectId = df.ResourceId
																	
							End							
				--Delete Section for DataFilter Ends******************


				--Delete Section for ExtensionProperties Starts******************
							Delete ep 
							from ExtensionProperty ep inner join #DeleteDataObjectsBeforeInsert do on ep.DataObjectid = do.DataObjectid
				--Delete Section for ExtensionProperties Ends******************

				--Delete Section for DataObjects Starts******************

							Delete do 
							from DataObjects do inner join #DeleteDataObjectsBeforeInsert dt on dt.DataObjectId = do.DataObjectid
							
				--Delete Section for DataObjects Ends******************

				--Delete Section for Picklists Starts******************

							Delete pl 
							from PickList pl inner join Dictionary D on pl.DictionaryId = D.Dictionaryid  
											 inner join Applications A on D.ApplicationID = A.ApplicationId 
											 and A.ApplicationId = @applicationId
							
				--Delete Section for Picklists Ends******************
				
				--Delete Section for Dictionary Starts******************
							Delete From Dictionary
							Where ApplicationID = @applicationId 
				--Delete Section for Dictionary Ends******************
				

		End
--Delete dictionary before inserting - Section Ends************************


--DataObjects Section Starts *************************
Begin

		select
			dataObjectId = newid(),
			dictionaryId = @dictionaryId,
			tableName = a.value('tableName[1]','nvarchar(50)'),
			objectNamespace = a.value('objectNamespace[1]','nvarchar(100)'),
			objectName = a.value('objectName[1]','nvarchar(50)'), 
			keyDelimeter = a.value('keyDelimeter[1]','nvarchar(50)'), 
			keyProperties = a.query('keyProperties'),
			dataProperties = a.query('dataProperties'),
			dataRelationships = a.query('dataRelationships'),
			isReadOnly = a.value('isReadOnly[1]','bit'),
			hasContent = a.value('hasContent[1]','bit'), 
			isListOnly = a.value('isListOnly[1]','bit'), 
			defaultProjectionFormat = a.value('defaultProjectionFormat[1]','nvarchar(200)'), 
			defaultListProjectionFormat = a.value('defaultListProjectionFormat[1]','nvarchar(200)'), 
			description = a.value('description[1]','nvarchar(200)'), 
			isRelatedOnly = a.value('isRelatedOnly[1]','bit'), 
			groupName = a.value('groupName[1]','nvarchar(200)'), 
			aliasDictionary = a.query('aliasDictionary'),
			version = a.value('version[1]','nvarchar(100)'),
			dataFilter = a.query('dataFilter'), 
			isHidden = a.value('isHidden[1]','bit'), 
			extensionProperties = a.query('extensionProperties') 
		into #DataObjects from #dictionary cross apply DataObjects.nodes('//dataObjects/dataObject') as T(a)


				-- KeyProperties Section_Starts*********************
						Begin
							select
								DataObjectId, 
								keyPropertyName = a.value('keyPropertyName[1]','nvarchar(250)')
							into #KeyProperties 
							from #DataObjects cross apply KeyProperties.nodes('/keyProperties/keyProperty') as T(a)
						End	
				-- KeyProperties Section_Ends*********************


				-- DataProperties Section_Starts*********************
						Begin
								select
									DataPropertyId = NEWID(),
									DataObjectId,
									--pickListId = a.value('pickListId[1]','uniqueidentifier'), 
									columnName = a.value('columnName[1]','nvarchar(50)'),
									propertyName = a.value('propertyName[1]','nvarchar(50)'),
									dataType = a.value('dataType[1]','nvarchar(50)'),
									[dataLength] = a.value('dataLength[1]','int'),
									isNullable = a.value('isNullable[1]','bit'),
									keyType = a.value('keyType[1]','nvarchar(50)'),
									showOnIndex = a.value('showOnIndex[1]','bit'),
									numberOfDecimals = a.value('numberOfDecimals[1]','int'),
									isReadOnly = a.value('isReadOnly[1]','bit'),
									showOnSearch = a.value('showOnSearch[1]','bit'),
									isHidden = a.value('isHidden[1]','bit'),
									description = a.value('description[1]','nvarchar(250)'),
									aliasDictionary = a.query('aliasDictionary'), 
									referenceType = a.value('referenceType[1]','nvarchar(50)'),
									isVirtual = a.value('isVirtual[1]','bit'),
									precision = a.value('precision[1]','int'),
									scale = a.value('scale[1]','int')
								into #DataProperties from #DataObjects cross apply DataProperties.nodes('/dataProperties/dataProperty') as T(a)

								
								select
									DataPropertyId, 
									[Key] = a.value('Key[1]','nvarchar(100)'),
									Value = a.value('Value[1]','nvarchar(200)')
								into #DataPropertiesAliesDictionary from #DataProperties cross apply aliasDictionary.nodes('/aliasDictionary/KeyValueOfstringstring') as T(a)
						End		
				-- DataProperties Section_Ends*********************



				-- DataRelationships Section_Starts*********************
						Begin
								select
									relationshipId = NEWID(), 
									dataObjectId,
									propertyMaps = a.query('propertyMaps'),
									relatedObjectName = a.value('relatedObjectName[1]','nvarchar(50)'),
									relationshipName = a.value('relationshipName[1]','nvarchar(50)'),
									relationshipType = a.value('relationshipType[1]','nvarchar(50)')
								into #DataRelationsips
								from #DataObjects cross apply dataRelationships.nodes('/dataRelationships/dataRelationship') as T(a)

								select
									RelationshipId, 
									dataPropertyName = a.value('dataPropertyName[1]','nvarchar(50)'),
									relatedPropertyName = a.value('relatedPropertyName[1]','nvarchar(50)')
								into #PropertyMap	
								from #DataRelationsips cross apply propertyMaps.nodes('/propertyMaps/propertyMap') as T(a)
						End		
				-- DataRelationships Section_Ends*********************



				-- DataObject Alias Dictionary Section_Starts*********************
						Begin
								select
									dataObjectId, 
									[Key] = a.value('Key[1]','nvarchar(100)'),
									Value = a.value('Value[1]','nvarchar(200)')
								into #DataObjectAliasDictionary
								from #DataObjects cross apply aliasDictionary.nodes('/aliasDictionary/KeyValueOfstringstring') as T(a)
						End		
				-- DataObject Alias Dictionary Section_Ends*********************



				-- DataFilter Section_Starts*********************
						Begin
								select
									dataFilterId = NEWID(),
									dataObjectId, 
									dataFilterTypeId = a.value('dataFilterTypeId[1]','int'),
									expressions = a.query('expressions'),
									orderExpressions = a.query('orderExpressions'),
									rollupExpressions = a.query('rollupExpressions'),
									isAdmin = a.value('isAdmin[1]','bit')
								into #DataFilters
								from #DataObjects cross apply dataFilter.nodes('/dataFilter') as T(a)
								
								-- Expression Section_Starts*********************
								Begin
									select
										expressionId = NEWID(),
										dataFilterId,
										openGroupCount = a.value('openGroupCount[1]','int'),
										propertyName = a.value('propertyName[1]','nvarchar(350)'),
										relationalOperator = a.value('relationalOperator[1]','nvarchar(150)'),
										[values] = a.query('values'),
										logicalOperator = a.value('logicalOperator[1]','nvarchar(50)'),
										closeGroupCount = a.value('closeGroupCount[1]','int'),
										isCaseSensitive = a.value('isCaseSensitive[1]','bit')
									into #Expression
									from #DataFilters cross apply expressions.nodes('/expressions/expression') as T(a)
									
									select
										expressionId,
										value = a.value('(text())[1]','nvarchar(200)')
									into #ExpressionValues
									from #Expression cross apply [values].nodes('/values/value') as T(a)
								End
								-- Expression Section_Ends***********************
								
								-- OrderExpression Section_Starts*********************
								Begin
									select
										orderExpressionId = NEWID(),
										dataFilterId,
										propertyName = a.value('propertyName[1]','nvarchar(350)'),
										sortOrder = a.value('sortOrder[1]','nvarchar(25)')
									into #OrderExpression
									from #DataFilters cross apply orderExpressions.nodes('/orderExpressions/orderExpression') as T(a)
								End
								-- OrderExpression Section_Ends***********************
								
								-- RollupExpression Section_Starts*********************
								Begin
									select
										rollupExpressionId = NEWID(),
										dataFilterId,
										groupBy = a.value('groupBy[1]','nvarchar(100)'),
										rollups = a.query('rollups')
									into #RollupExpression
									from #DataFilters cross apply rollupExpressions.nodes('/rollupExpressions/rollupExpression') as T(a)
									
											-- Rollup Section_Starts*********************
											Begin
												select
													rollupExpressionId,
													propertyName = a.value('propertyName[1]','nvarchar(350)'),
													[type] = a.value('type[1]','nvarchar(50)')
												into #Rollup
												from #RollupExpression cross apply rollups.nodes('/rollups/rollup') as T(a)
																			
											End
											-- Rollup Section_Ends*********************					
								End
								-- RollupExpression Section_Ends***********************				
						End		
				-- DataFilter Section_Ends***********************



				-- ExtensionProperties Section_Starts*********************
						Begin
								select
									extensionPropertyId = NEWID(),
									dataObjectId, 
									columnName = a.value('columnName[1]','nvarchar(250)'),
									propertyName = a.value('propertyName[1]','nvarchar(250)'),
									dataType = a.value('dataType[1]','nvarchar(50)'),
									[dataLength] = a.value('dataLength[1]','int'),
									isNullable = a.value('isNullable[1]','bit'),
									numberOfDecimals = a.value('numberOfDecimals[1]','int'),
									keyType = a.value('keyType[1]','nvarchar(50)'),
									showOnIndex = a.value('showOnIndex[1]','bit'),
									[precision] = a.value('precision[1]','int'),
									scale = a.value('scale[1]','int'),
									[definition] = a.value('definition[1]','nvarchar(250)')
								into #ExtensionProperty
								from #DataObjects cross apply extensionProperties.nodes('/extensionProperties/extensionProperty') as T(a)
						End		
				-- ExtensionProperties Section_Ends*********************

 End
--DataObjects Section Ends *************************


--Picllists Section Starts *************************
 Begin
		select
			dictionaryId = @dictionaryId,
			pickListId = NEWID(),
			name = a.value('name[1]','nvarchar(50)'), 
			[description] = a.value('description[1]','nvarchar(50)'), 
			valuePropertyIndex = a.value('valuePropertyIndex[1]','nvarchar(50)'), 
			tableName = a.value('tableName[1]','nvarchar(50)'), 
			pickListProperties = a.query('pickListProperties')
		into #Picklists 
		from #dictionary cross apply picklists.nodes('//picklists/PicklistObject') as T(a)
		
		--PickListProperties Section Starts************
		Begin
				select
					pickListId,
					columnName = a.value('columnName[1]','nvarchar(50)'),
					propertyName = a.value('propertyName[1]','nvarchar(50)'),
					dataType = a.value('dataType[1]','nvarchar(50)'),
					[dataLength] = a.value('dataLength[1]','int'),
					isNullable = a.value('isNullable[1]','bit'),
					keyType = a.value('keyType[1]','nvarchar(50)'),
					showOnIndex = a.value('showOnIndex[1]','bit'),
					numberOfDecimals = a.value('numberOfDecimals[1]','int'),
					isReadOnly = a.value('isReadOnly[1]','bit'),
					showOnSearch = a.value('showOnSearch[1]','bit'),
					isHidden = a.value('isHidden[1]','bit'),
					[description] = a.value('description[1]','nvarchar(250)'),
					--aliasDictionary = a.query('aliasDictionary'), 
					referenceType = a.value('referenceType[1]','nvarchar(50)'),
					isVirtual = a.value('isVirtual[1]','bit'),
					[precision] = a.value('precision[1]','int'),
					scale = a.value('scale[1]','int')
				into #PickListProperties 
				from #Picklists cross apply pickListProperties.nodes('/pickListProperties/dataProperty') as T(a)				
		End
		--PickListProperties Section Ends**************
 End
--Picllists Section Ends *************************

-- Insert Values in corresponding tables- Section Starts************************* 
Begin 

	INSERT INTO [Dictionary]([Dictionaryid],[ApplicationID],[IsDBDictionary],[EnableSearch],[EnableSummary],[DataVersion],[Provider],[ConnectionString],[SchemaName],[Description])
					   Select @dictionaryId, @applicationId, isDBDictionary,  enableSearch,  enableSummary,  dataVersion,  provider,  connectionString,  schemaName,  description from #dictionary      
	
	
	INSERT INTO [DataObjects]([DataObjectId],[DictionaryId],[TableName],[ObjectNameSpace],[ObjectName],[KeyDelimeter],[Description],[IsReadOnly],[HasContent],[IsListOnly],[DefaultProjectionFormat],[DefaultListProjectionFormat],[IsRelatedOnly],[GroupName],[Version],[IsHidden])
				Select		   dataObjectId,  @dictionaryId, tableName,  objectNamespace,  objectName,  keyDelimeter, [description], isReadOnly,  hasContent,  isListOnly,  defaultProjectionFormat,  defaultListProjectionFormat,  isRelatedOnly,  groupName, [version], isHidden from #DataObjects
	
	
	INSERT INTO KeyProperty([DataObjectId],[KeyPropertyName])
					 Select [dataObjectId],[keyPropertyName] from #KeyProperties
	
	INSERT INTO DataProperties([DataPropertyId],[DataObjectId],[ColumnName],[PropertyName],[DataType],[DataLength],[IsNullable],[KeyType],[ShowOnIndex],[NumberOfDecimals],[IsReadOnly],[ShowOnSearch],[IsHidden],[Description],[ReferenceType],[IsVirtual],[Precision],[Scale])
						select [DataPropertyId],[dataObjectId],[columnName],[propertyName],[dataType],[dataLength],[isNullable],[keyType],[showOnIndex],[numberOfDecimals],[isReadOnly],[showOnSearch],[isHidden],[description],[referenceType],[isVirtual],[precision],[scale] from #DataProperties		
	
	
	INSERT INTO [AliasDictionary]([ResourceId],[Key],[Value])
	                   Select [DataPropertyId],[Key],[Value] from #DataPropertiesAliesDictionary
	
	INSERT INTO [DataRelationships]([RelationshipId],[DataObjectID],[RelationShipName],[RelatedObjectName],[RelationShipType])
	                         Select [relationshipId],[dataObjectId],[relationshipName],[relatedObjectName],[relationshipType] from #DataRelationsips

	INSERT INTO [PropertyMap]([RelationshipID],[DataPropertyName],[RelatedPropertyName])
	                   Select [relationshipId],[dataPropertyName],[relatedPropertyName] from #PropertyMap

	INSERT INTO [AliasDictionary]([ResourceId],[Key],[Value])
	                   Select   [dataObjectId],[Key],[Value] from #DataObjectAliasDictionary


	INSERT INTO [DataFilters]([DataFilterId],[ResourceId],[DataFilterTypeId],[IsAdmin])
	                   Select [dataFilterId],[dataObjectid],[dataFilterTypeId],[isAdmin] from #DataFilters
	                   
	
	INSERT INTO [Expression]([DataFilterId],[ExpressionId],[OpenGroupCount],[PropertyName],[RelationalOperator],[LogicalOperator],[CloseGroupCount],[IsCaseSensitive])
	                  Select [dataFilterId],[expressionId],[openGroupCount],[propertyName],[relationalOperator],[logicalOperator],[closeGroupCount],[isCaseSensitive] from #Expression
                   
	 
    INSERT INTO [ExpressionValues]([ExpressionId],[Value])
                            Select [expressionId],[value] from #ExpressionValues
                            

    INSERT INTO [OrderExpression]([DataFilterId],[OrderExpressionId],[PropertyName],[SortOrder])
		                   Select [dataFilterId],[orderExpressionId],[propertyName],[sortOrder] from #OrderExpression
                        

    INSERT INTO [RollupExpression]([DataFilterId],[RollupExpressionId],[GroupBy])
	                        Select [dataFilterId],[rollupExpressionId],[groupBy] from #RollupExpression
	                        
	INSERT INTO [Rollup]([RollupExpressionId],[PropertyName],[RollupType])
			Select       [rollupExpressionId],[propertyName],[type] from #Rollup
			
			
			
	INSERT INTO [ExtensionProperty]([ExtensionPropertyId],[DataObjectId],[ColumnName],[PropertyName],[DataType],[DataLength],[IsNullable],[NumberOfDecimals],[KeyType],[Precision],[Scale],[Definition],[ShowOnIndex])
		                     Select [extensionPropertyId],[dataObjectId],[columnName],[propertyName],[dataType],[dataLength],[isNullable],[numberOfDecimals],[keyType],[precision],[scale],[definition],[showOnIndex] from #ExtensionProperty
	
	
	INSERT INTO [PickList]([DictionaryId],[PickListId],[Name],[Description],[ValuePropertyIndex],[TableName])
		            Select [dictionaryId],[pickListId],[name],[description],[valuePropertyIndex],[TableName] from #Picklists
	
	
	
	;WITH PickListDataProperties_CTE (DataPropertyId, pickListId)
	AS
	(
		Select DP.DataPropertyId,PP.pickListId
		--into #tmp
		from #DataProperties DP inner join #PickListProperties PP on DP.columnName = PP.columnName
										 And DP.propertyName = PP.propertyName 
										 And ISNULL(DP.[description],'') = ISNULL(PP.[description],'') --checking this just to be on more safer side
										 And ISNULL(DP.[precision],0) = ISNULL(PP.[precision],0)--checking this just to be on more safer side
										 And ISNULL(DP.scale,0) = ISNULL(PP.scale,0)--checking this just to be on more safer side
	)
    
	
	Update DP
	Set DP.PickListId = PP.pickListId
	From DataProperties DP inner join PickListDataProperties_CTE PP on DP.DataPropertyId = PP.DataPropertyId              
  
                  
End
-- Insert Values in corresponding tables- Section Ends*************************


--Select [dataFilterId],[expressionId],[openGroupCount],[propertyName],[relationalOperator],[logicalOperator],[closeGroupCount],[isCaseSensitive] from #Expression



 
/*Although for few entities like KeyProperty we don't need temp tables
  but to maintain consistency(like first parent and then child entry 
  should be made like in case of KeyProperty, first dataobjects should 
  be inserted then KeyProperty should be insertd).
*/

drop table #dictionary
drop table #DataObjects
drop table #DataProperties
drop table #KeyProperties 
drop table #DataPropertiesAliesDictionary 
drop table #DataRelationsips
drop table #PropertyMap 
drop table #DataObjectAliasDictionary 
drop table #DataFilters
drop table #Expression
drop table #ExpressionValues
drop table #OrderExpression
drop table #RollupExpression
drop table #Rollup
drop table #ExtensionProperty
drop table #Picklists
drop table #PickListProperties
drop table #DeleteDataObjectsBeforeInsert

Select '1' --DataDictionary saved successfully   
	
END

END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spiExchange]    Script Date: 04/16/2015 17:44:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Deepak Kumar>
-- Create date: <10-Sep-2014>
-- Description:	<Inserting Exchange>
-- ===========================================================
CREATE PROCEDURE [dbo].[spiExchange] 
(
	/*Note: All the nullable Fields in [Exchanges] table
	  needs to have default value to null as done below.
	  so that if value is not supplied in the parameter
	  then null will be assigned in corresspoing field
	*/
	--@UserName varchar(100),--'vmallire'
	@CommodityId	uniqueidentifier,
	@SourceGraphId	uniqueidentifier,
	@DestinationGraphId	uniqueidentifier,
	@Name	nvarchar(255),
	@Description	nvarchar(255) = null,
	@PoolSize	int,
	@XTypeAdd	nvarchar(25) = null,
	@XTypeChange	nvarchar(25)  = null,
	@XTypeSync	nvarchar(25) = null,
	@XTypeDelete	nvarchar(25) = null,
	@XTypeSetNull	nvarchar(25) = null,
	@GroupList xml
	
)	 
AS

BEGIN TRY

	BEGIN
	--Siblings are not allowed
	If not Exists(Select top 1 * from Exchanges Where Name = @Name and Active = 1 and CommodityId = @CommodityId)
		Begin
	
	--select * from ResourceType
	
	Declare @ExchangeId uniqueidentifier = NewID()  
	Declare @ResourceTypeId int = 6 --See ResourceType for detail
	
	INSERT INTO [Exchanges]
           ([ExchangeId]
           ,[CommodityId]
           ,[SourceGraphId]
           ,[DestinationGraphId]
           ,[Name]
           ,[Description]
           ,[PoolSize]
           ,[XTypeAdd]
           ,[XTypeChange]
           ,[XTypeSync]
           ,[XTypeDelete]
           ,[XTypeSetNull]
           )
	Select
			@ExchangeId
           ,@CommodityId
           ,@SourceGraphId
           ,@DestinationGraphId
           ,@Name
           ,@Description
           ,@PoolSize
           ,@XTypeAdd
           ,@XTypeChange
           ,@XTypeSync
           ,@XTypeDelete
           ,@XTypeSetNull


	SELECT       
			nref.value('groupId[1]', 'int') GroupId      
			INTO #Groups FROM   @GroupList.nodes('//group') AS R(nref)
			
	
	INSERT INTO [ResourceGroups]
           ([ResourceId]
           ,[GroupId]
           ,[ResourceTypeId]
           )
	Select 
		@ExchangeId,
		tg.GroupId,
		@ResourceTypeId
	FROM  Groups g
	INNER JOIN #Groups tg ON g.GroupId = tg.GroupId	
	--from  UserGroups ug 
	--		inner join Users u on ug.UserId = u.UserId
	--			And u.UserName = @UserName
	--			And u.Active = 1
	--			And ug.Active = 1
	--		inner join #Groups tg on ug.GroupId = tg.GroupId
							
		IF NOT EXISTS(SELECT top 1 * FROM ResourceGroups rg INNER JOIN Groups g ON rg.GroupId = g.GroupId WHERE g.GroupName = 'XSitesAdminGroup' AND rg.ResourceId = @ExchangeId AND rg.ResourceTypeId = @ResourceTypeId AND rg.Active = 1)
		BEGIN
			INSERT INTO [ResourceGroups]
			   ([ResourceId]
			   ,[GroupId]
			   ,[ResourceTypeId])
			SELECT 
				@ExchangeId,
				g.GroupId,
				@ResourceTypeId
			FROM Groups g
			WHERE g.GroupName = 'XSitesAdminGroup'
		END
			
			Select  '1'--'Exchange added successfully!'
			
		end--end of if		
		Else
			Select  '0'--'Exchange with this name already exists!'					

	END

END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spgExchangesByUser]    Script Date: 04/16/2015 17:44:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Vidisha>
-- Create date: <27-Aug-2014>
-- Description:	<Selecting exchanges based on users>
-- ===========================================================
CREATE PROCEDURE [dbo].[spgExchangesByUser] --'vmallire',1,'ee565602-ba52-422c-a01f-932490796666'
(
	@UserName varchar(100),--'vmallire'
	@CommodityId uniqueidentifier--'ee565602-ba52-422c-a01f-932490796666'
)	 
AS

	BEGIN
	 
		WITH XMLNAMESPACES ( DEFAULT 'http://www.iringtools.org/library' ) 
		select  
		e.ExchangeId as exchangeId,
		e.CommodityId as commodityId, 
		e.SourceGraphId as sourceGraphId, 
		e.DestinationGraphId as destinationGraphId,
		e.Name as name,
		e.Description as description, 
		e.PoolSize as poolSize, 
		e.XTypeAdd as xtypeAdd ,
		e.XTypeChange as xtypeChange ,
		e.XTypeSync as xtypeSync ,
		e.XTypeDelete as xtypeDelete ,
		e.XTypeSetNull as xtypeSetNull,
		e.Active as active,
		(
			select distinct  p.PermissionId as permissionId,  p.PermissionName as permissionName, p.PermissionDesc as permissionDesc, p.Active as active from permissions p
			  inner join ResourceGroups rg on rg.active = e.active and rg.resourceid = e.ExchangeId
			  inner join Groups g on g.groupId = rg.groupId and  g.active = e.active
			  inner join grouproles gr on  gr.groupid = rg.groupid and  gr.active = e.active    
			  inner join roles r on r.roleid = gr.roleid and  r.active = e.active     
			  inner join rolepermissions rp on rp.permissionid = p.permissionid and rp.roleid = r.roleid and  rp.active = e.active          
			  inner join usergroups ug on ug.groupid = rg.groupid   and ug.active = e.active
			  inner join users u on u.userid = ug.userid and u.username = @UserName and  u.active = e.active
			  where e.CommodityId = @CommodityId  and e.active = 1   for xml PATH('permission'), type 
		) as 'permissions',
		(
		Select gg.groupId,gg.groupName from ResourceGroups rr inner join Groups gg on rr.GroupId = gg.GroupId 
		where rr.ResourceId = e.ExchangeId 
		and rr.Active = 1
		and gg.Active = 1 
		for xml PATH('group'), type
		)  as 'groups'
		
		from  Exchanges e
			inner join ResourceGroups arg on  arg.active = e.active and arg.resourceid =  e.ExchangeId
			inner join Groups ag on ag.groupId = arg.groupId and ag.active = e.active
			inner join grouproles agr on  agr.groupid = arg.groupid and  agr.active = e.active    
			inner join roles ar on ar.roleid = agr.roleid and ar.active = e.active         
			inner join usergroups aug on aug.groupid = arg.groupid and  aug.active = e.active
			inner join users au on au.userid = aug.userid and au.username = @UserName and  au.active = e.active
		where e.CommodityId = @CommodityId  and e.active = 1
		Group BY e.CommodityId, e.ExchangeId , e.Name,e.Description, e.PoolSize, e.SourceGraphId, e.DestinationGraphId,e.Active, e.xtypeAdd, e.xtypeChange, e.xtypeSync, e.xtypeDelete, e.xtypeSetNull
		for xml PATH('exchange'), ROOT('exchanges'), type, ELEMENTS XSINIL


	END
GO
/****** Object:  StoredProcedure [dbo].[spgDictionary]    Script Date: 04/16/2015 17:44:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Deepak Kumar>
-- Create date: <24-Mar-2015>
-- Modified By: <Deepak Kumar>
-- Modified Data: <04-April-2015>
-- Description:	<gettting dictionary>
-- =============================================
CREATE PROCEDURE [dbo].[spgDictionary] --'3E1E418D-FA51-403B-BB7B-0CADE34283A7' 
(
	@ApplicationID uniqueidentifier
)	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	 

			WITH XMLNAMESPACES ('http://www.iringtools.org/data/filter' AS DataFilterUri, DEFAULT 'http://www.iringtools.org/library') 
			SELECT
			(
				SELECT
				   DO.[tableName]
				  ,DO.[objectNamespace]
				  ,DO.[objectName]
				  ,DO.[keyDelimeter]
				  ,(
					 SELECT
					    KP.[keyPropertyName]
					   ,KP.[dataObjectId]
					   FROM [KeyProperty] KP Where KP.DataObjectId = DO.DataObjectId for xml path('keyProperty'),type   --,ELEMENTS XSINIL
				   ) as 'keyProperties'
				  ,(
					  SELECT
					   DP.[columnName]
					  ,DP.[propertyName]
					  ,DP.[dataType]
					  ,DP.[dataLength]
					  ,DP.isNullable
					  ,DP.[keyType]
					  ,DP.[showOnIndex]
					  ,DP.[numberOfDecimals]
					  ,DP.[isReadOnly]
					  ,DP.[showOnSearch]
					  ,DP.[isHidden]
					  ,DP.[description]
					  ,(
						Select 
							AD.[Key],
							AD.[Value]  from AliasDictionary AD where AD.ResourceId = DP.DataPropertyId For xml path('KeyValueOfstringstring'),type    --,ELEMENTS XSINIL 
					   ) as 'aliasDictionary' 
					  ,DP.[referenceType] 
					  ,DP.[isVirtual]
					  ,DP.[precision]
					  ,DP.[scale]
					  ,DP.[dataPropertyId]
					  ,DP.[dataObjectId] 
					  ,pickListId = ISNULL(DP.[pickListId],'00000000-0000-0000-0000-000000000000')
					FROM [DataProperties] DP Where DP.DataObjectId = DO.DataObjectId For xml path('dataProperty'),type   --, ELEMENTS XSINIL 
				   ) as 'dataProperties' 
				  ,(
						Select
					    (
							SELECT 
							 PM.[dataPropertyName]
							,PM.[relatedPropertyName]
							,PM.[relationshipId]
							FROM [PropertyMap] PM Where PM.RelationshipID = DR.RelationshipId For xml path('propertyMap'),type   --,ELEMENTS XSINIL
						)as 'propertyMaps' 
						,DR.[relatedObjectName]
						,DR.[relationshipName]
						,DR.[relationshipType]
						,DR.[relationshipId]
					    ,DR.[dataObjectId]
						FROM [DataRelationships] DR Where DR.DataObjectId = DO.DataObjectId For xml path('dataRelationship'),type  --,ELEMENTS XSINIL
				   ) as 'dataRelationships'
			      ,DO.[isReadOnly]
			      ,DO.[hasContent]
			      ,DO.[isListOnly]
			      ,DO.[defaultProjectionFormat]
			      ,DO.[defaultListProjectionFormat]
				  ,DO.[description]
				  ,DO.[isRelatedOnly]
				  ,DO.[groupName]
				  ,(
					 Select 
							AD1.[Key],
							AD1.[Value]  from AliasDictionary AD1 where AD1.ResourceId = DO.DataObjectId For xml path('KeyValueOfstringstring'),type   --,ELEMENTS XSINIL	
				   )as 'aliasDictionary'
				  ,DO.[version] 
				  ,(
					 Select
						  (
							Select 
							   EX.[openGroupCount] AS 'DataFilterUri:openGroupCount'
							  ,EX.[propertyName] AS 'DataFilterUri:propertyName'
							  ,EX.[relationalOperator] AS 'DataFilterUri:relationalOperator'
							  ,(
									Select 
										EXV.[value] AS 'DataFilterUri:value'
									from ExpressionValues EXV where EXV.ExpressionId = EX.ExpressionId for XML path(''),root('DataFilterUri:values'),type   --,ELEMENTS XSINIL
							   )
							 ,EX.[logicalOperator] AS 'DataFilterUri:logicalOperator'
							 ,EX.[closeGroupCount] AS 'DataFilterUri:closeGroupCount'
							 ,EX.[isCaseSensitive] AS 'DataFilterUri:isCaseSensitive'
							 ,EX.[dataFilterId] AS 'DataFilterUri:dataFilterId'
							 ,EX.[expressionId] AS 'DataFilterUri:expressionId'
							 
							 from Expression EX where EX.DataFilterId = DF.DataFilterId  For xml path('DataFilterUri:expression'),type	--,ELEMENTS XSINIL
				          ) as 'DataFilterUri:expressions'
					     ,(
							Select 
								 OE.[propertyName] AS 'DataFilterUri:propertyName'
								,OE.[sortOrder] AS 'DataFilterUri:sortOrder'
								,OE.[dataFilterId] AS 'DataFilterUri:dataFilterId'
								,OE.[orderExpressionId] AS 'DataFilterUri:orderExpressionId'
							from OrderExpression OE where OE.DataFilterId = DF.DataFilterId For xml path('DataFilterUri:orderExpression'),type	--,ELEMENTS XSINIL
						  ) as 'DataFilterUri:orderExpressions'
					     ,(
							Select 
							   RE.[groupBy] AS 'DataFilterUri:groupBy'
							   ,(
								  Select 
										 R.[propertyName] AS 'DataFilterUri:propertyName'
										,R.RollupType as 'DataFilterUri:type'
										,R.[rollupExpressionId] AS 'DataFilterUri:rollupExpressionId'
										,R.[rollupId] AS 'DataFilterUri:rollupId'
								   from [Rollup] R where R.RollupExpressionId = RE.RollupExpressionId For xml path('DataFilterUri:rollup'),type    --,ELEMENTS XSINIL		
								) as 'DataFilterUri:rollups'	
							   ,RE.[dataFilterId] AS 'DataFilterUri:dataFilterId'
							   ,RE.[rollupExpressionId]	AS 'DataFilterUri:rollupExpressionId'
							From RollupExpression RE Where RE.DataFilterId = DF.DataFilterId For xml path('DataFilterUri:rollupExpression'),type   --,ELEMENTS XSINIL	
						   ) as 'DataFilterUri:rollupExpressions'
				          ,DF.[isAdmin] AS 'DataFilterUri:isAdmin'
				          ,DF.[dataFilterId] AS 'DataFilterUri:dataFilterId'
						  ,DF.[resourceId] AS 'DataFilterUri:resourceId'
						  ,DF.[dataFilterTypeId] AS 'DataFilterUri:dataFilterTypeId'
						  ,DF.[active] AS 'DataFilterUri:active'
				    From DataFilters DF  Where DF.ResourceId = DO.DataObjectId For xml path(''),type  --,ELEMENTS XSINIL	
				   ) as 'dataFilter'
				   ,DO.[isHidden]
				  ,(
						Select 
							 EP.[columnName]
							,EP.[propertyName]
							,EP.[dataType]
							,EP.[dataLength]
							,EP.[isNullable]
							,EP.[keyType]
							,EP.[showOnIndex]
							,EP.[precision]
							,EP.[scale]
							,EP.[definition]
							,EP.[numberOfDecimals]
							,EP.[extensionPropertyId] 
							,EP.[dataObjectId]
						from ExtensionProperty EP where EP.DataObjectId = DO.DataObjectId For xml path('extensionProperty'),type   --,ELEMENTS XSINIL	
				   ) as 'extensionProperties'
				  ,DO.[dataObjectId]	
				  ,DO.[dictionaryId]
			   FROM [DataObjects] DO Where DO.DictionaryId = D.Dictionaryid
									 --Order by DO.ObjectName 
									 for xml path('dataObject'),type  --,ELEMENTS XSINIL

			 ) as 'dataObjects'
			,(  
				--This needs to confirm
				SELECT 
				 PL.[name]
				,PL.[description]
				,PL.[valuePropertyIndex]
				,PL.[tableName]
				,(
					  SELECT
					   DP.[columnName]
					  ,DP.[propertyName]
					  ,DP.[dataType]
					  ,DP.[dataLength]
					  ,DP.isNullable
					  ,DP.[keyType]
					  ,DP.[showOnIndex]
					  ,DP.[numberOfDecimals]
					  ,DP.[isReadOnly]
					  ,DP.[showOnSearch]
					  ,DP.[isHidden]
					  ,DP.[description]
					  ,(
						Select 
							AD.[Key],
							AD.[Value]  from AliasDictionary AD where AD.ResourceId = DP.DataPropertyId For xml path('KeyValueOfstringstring'),type  --,ELEMENTS XSINIL
					   ) as 'aliasDictionary' 
					  ,DP.[referenceType] 
					  ,DP.[isVirtual]
					  ,DP.[precision]
					  ,DP.[scale]
					  ,DP.[dataPropertyId]
					  ,DP.[dataObjectId] 
					  ,pickListId = ISNULL(DP.[pickListId],'00000000-0000-0000-0000-000000000000')
					FROM [DataProperties] DP Where DP.PickListId = PL.PickListId For xml path('dataProperty'),type  --,ELEMENTS XSINIL 
				   ) as 'pickListProperties'
				  ,PL.[dictionaryId]
				  ,PL.[pickListId] 
				FROM [PickList] PL Where PL.DictionaryId = D.Dictionaryid for xml path('PicklistObject'),type   --,ELEMENTS XSINIL
			 ) as 'picklists'
			,D.[enableSearch]
			,D.[enableSummary]
			,D.[dataVersion]
			,D.[description]
			,D.[dictionaryId]
			,D.[applicationId]
			,D.[isDBDictionary]
			,D.[provider]
			,D.[connectionString]
			,D.[schemaName]
			 	 
			FROM [Dictionary] D
			where ApplicationID = @ApplicationID--'3E1E418D-FA51-403B-BB7B-0CADE34283A7' 
			for xml path('databaseDictionary'),type  --,ELEMENTS XSINIL 

END
GO
/****** Object:  StoredProcedure [dbo].[spdExchange]    Script Date: 04/16/2015 17:44:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Deepak Kumar>
-- Create date: <10-Sep-2014>
-- Description:	<delete Exchange>
-- ===========================================================
CREATE PROCEDURE [dbo].[spdExchange] 
(
	@ExchangeId uniqueidentifier
)	 
AS
BEGIN

	
	Update Exchanges
		Set Active = 0
	Where ExchangeId = @ExchangeId 


	Update ResourceGroups
		Set   Active = 0
	Where ResourceId = @ExchangeId   
						

END
GO
/****** Object:  Default [DF_Applications_Active]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[Applications] ADD  CONSTRAINT [DF_Applications_Active]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF_BindingConfig_Active]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[BindingConfig] ADD  CONSTRAINT [DF_BindingConfig_Active]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF__Commodity__Activ__42E1EEFE]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[Commodity] ADD  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF_Contexts_ContextId]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[Contexts] ADD  CONSTRAINT [DF_Contexts_ContextId]  DEFAULT (newid()) FOR [ContextId]
GO
/****** Object:  Default [DF__Contexts__Active__46E78A0C]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[Contexts] ADD  CONSTRAINT [DF__Contexts__Active__46E78A0C]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF_DataFilters_DataFilterId]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[DataFilters] ADD  CONSTRAINT [DF_DataFilters_DataFilterId]  DEFAULT (newid()) FOR [DataFilterId]
GO
/****** Object:  Default [DF_DataFilters_Active]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[DataFilters] ADD  CONSTRAINT [DF_DataFilters_Active]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF_DataFilters_IsAdmin]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[DataFilters] ADD  CONSTRAINT [DF_DataFilters_IsAdmin]  DEFAULT ((0)) FOR [IsAdmin]
GO
/****** Object:  Default [DF_DataObjects_DataObjectId]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[DataObjects] ADD  CONSTRAINT [DF_DataObjects_DataObjectId]  DEFAULT (newid()) FOR [DataObjectId]
GO
/****** Object:  Default [DF_DataProperties_DataPropertyId]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[DataProperties] ADD  CONSTRAINT [DF_DataProperties_DataPropertyId]  DEFAULT (newid()) FOR [DataPropertyId]
GO
/****** Object:  Default [DF_DataRelationships_RelationshipId]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[DataRelationships] ADD  CONSTRAINT [DF_DataRelationships_RelationshipId]  DEFAULT (newid()) FOR [RelationshipId]
GO
/****** Object:  Default [DF_Dictionary_Dictionaryid]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[Dictionary] ADD  CONSTRAINT [DF_Dictionary_Dictionaryid]  DEFAULT (newid()) FOR [Dictionaryid]
GO
/****** Object:  Default [DF_Dictionary_IsDBDictionary]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[Dictionary] ADD  CONSTRAINT [DF_Dictionary_IsDBDictionary]  DEFAULT ((1)) FOR [IsDBDictionary]
GO
/****** Object:  Default [DF__Exchanges__Activ__60A75C0F]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[Exchanges] ADD  CONSTRAINT [DF__Exchanges__Activ__60A75C0F]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF_Expressions_ExpressionId]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[Expression] ADD  CONSTRAINT [DF_Expressions_ExpressionId]  DEFAULT (newid()) FOR [ExpressionId]
GO
/****** Object:  Default [DF_ExtensionProperty_ExtensionPropertyId]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[ExtensionProperty] ADD  CONSTRAINT [DF_ExtensionProperty_ExtensionPropertyId]  DEFAULT (newid()) FOR [ExtensionPropertyId]
GO
/****** Object:  Default [DF_Folders_FolderId]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[Folders] ADD  CONSTRAINT [DF_Folders_FolderId]  DEFAULT (newid()) FOR [FolderId]
GO
/****** Object:  Default [DF__Folders__Active__49C3F6B7]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[Folders] ADD  CONSTRAINT [DF__Folders__Active__49C3F6B7]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF_Graphs_GraphId]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[Graphs] ADD  CONSTRAINT [DF_Graphs_GraphId]  DEFAULT (newid()) FOR [GraphId]
GO
/****** Object:  Default [DF__Graphs__Active__753864A1]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[Graphs] ADD  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF_Graphs_ExchangeVisible]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[Graphs] ADD  CONSTRAINT [DF_Graphs_ExchangeVisible]  DEFAULT ((1)) FOR [ExchangeVisible]
GO
/****** Object:  Default [DF__GroupRole__Activ__4CA06362]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[GroupRoles] ADD  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF_Groups_Active]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[Groups] ADD  CONSTRAINT [DF_Groups_Active]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF_MimeType_Active]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[MimeType] ADD  CONSTRAINT [DF_MimeType_Active]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF_Table1_ExpressionId]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[OrderExpression] ADD  CONSTRAINT [DF_Table1_ExpressionId]  DEFAULT (newid()) FOR [OrderExpressionId]
GO
/****** Object:  Default [DF__Permissio__Activ__1B0907CE]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[Permissions] ADD  CONSTRAINT [DF__Permissio__Activ__1B0907CE]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF_PickList_PickListId]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[PickList] ADD  CONSTRAINT [DF_PickList_PickListId]  DEFAULT (newid()) FOR [PickListId]
GO
/****** Object:  Default [DF_Platforms_Active]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[Platforms] ADD  CONSTRAINT [DF_Platforms_Active]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF__ResourceG__Activ__4F7CD00D]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[ResourceGroups] ADD  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF_ResourceGroups_ResourceTypeId]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[ResourceGroups] ADD  CONSTRAINT [DF_ResourceGroups_ResourceTypeId]  DEFAULT ((1)) FOR [ResourceTypeId]
GO
/****** Object:  Default [DF__RolePermi__Activ__14270015]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[RolePermissions] ADD  CONSTRAINT [DF__RolePermi__Activ__14270015]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF__Roles__Active__15502E78]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[Roles] ADD  CONSTRAINT [DF__Roles__Active__15502E78]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF_Rollup_RollupId]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[Rollup] ADD  CONSTRAINT [DF_Rollup_RollupId]  DEFAULT (newid()) FOR [RollupId]
GO
/****** Object:  Default [DF_Table_1_ExpressionId]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[RollupExpression] ADD  CONSTRAINT [DF_Table_1_ExpressionId]  DEFAULT (newid()) FOR [RollupExpressionId]
GO
/****** Object:  Default [DF__ScheduleC__Sched__23F3538A]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[ScheduleCache_Expired] ADD  DEFAULT (newsequentialid()) FOR [Schedule_Cache_Id]
GO
/****** Object:  Default [DF__ScheduleE__Sched__28B808A7]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[ScheduleExchange_Expired] ADD  DEFAULT (newsequentialid()) FOR [Schedule_Exchange_Id]
GO
/****** Object:  Default [DF_Sites_Active]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[Sites] ADD  CONSTRAINT [DF_Sites_Active]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF__UserGroup__Activ__267ABA7A]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[UserGroups] ADD  CONSTRAINT [DF__UserGroup__Activ__267ABA7A]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF__Users__Active__0EA330E9]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF__Users__Active__0EA330E9]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  ForeignKey [FK_ApplicationSettings_Applications]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[ApplicationSettings]  WITH CHECK ADD  CONSTRAINT [FK_ApplicationSettings_Applications] FOREIGN KEY([ApplicationId])
REFERENCES [dbo].[Applications] ([ApplicationId])
GO
ALTER TABLE [dbo].[ApplicationSettings] CHECK CONSTRAINT [FK_ApplicationSettings_Applications]
GO
/****** Object:  ForeignKey [FK_ApplicationSettings_Settings]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[ApplicationSettings]  WITH CHECK ADD  CONSTRAINT [FK_ApplicationSettings_Settings] FOREIGN KEY([settingId])
REFERENCES [dbo].[Settings] ([SettingId])
GO
ALTER TABLE [dbo].[ApplicationSettings] CHECK CONSTRAINT [FK_ApplicationSettings_Settings]
GO
/****** Object:  ForeignKey [FK_BindingConfig_Applications]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[BindingConfig]  WITH CHECK ADD  CONSTRAINT [FK_BindingConfig_Applications] FOREIGN KEY([ApplicationId])
REFERENCES [dbo].[Applications] ([ApplicationId])
GO
ALTER TABLE [dbo].[BindingConfig] CHECK CONSTRAINT [FK_BindingConfig_Applications]
GO
/****** Object:  ForeignKey [FK__Commodity__Conte__40F9A68C]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[Commodity]  WITH CHECK ADD  CONSTRAINT [FK__Commodity__Conte__40F9A68C] FOREIGN KEY([ContextId])
REFERENCES [dbo].[Contexts] ([ContextId])
GO
ALTER TABLE [dbo].[Commodity] CHECK CONSTRAINT [FK__Commodity__Conte__40F9A68C]
GO
/****** Object:  ForeignKey [FK__Contexts__Folder__5DEAEAF5]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[Contexts]  WITH CHECK ADD FOREIGN KEY([FolderId])
REFERENCES [dbo].[Folders] ([FolderId])
GO
/****** Object:  ForeignKey [FK_DataLayers_Platforms]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[DataLayers]  WITH CHECK ADD  CONSTRAINT [FK_DataLayers_Platforms] FOREIGN KEY([PlatformId])
REFERENCES [dbo].[Platforms] ([PlatformId])
GO
ALTER TABLE [dbo].[DataLayers] CHECK CONSTRAINT [FK_DataLayers_Platforms]
GO
/****** Object:  ForeignKey [FK_DataLayers_Sites]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[DataLayers]  WITH CHECK ADD  CONSTRAINT [FK_DataLayers_Sites] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
ALTER TABLE [dbo].[DataLayers] CHECK CONSTRAINT [FK_DataLayers_Sites]
GO
/****** Object:  ForeignKey [FK_DataObjects_Dictionary]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[DataObjects]  WITH CHECK ADD  CONSTRAINT [FK_DataObjects_Dictionary] FOREIGN KEY([DictionaryId])
REFERENCES [dbo].[Dictionary] ([Dictionaryid])
GO
ALTER TABLE [dbo].[DataObjects] CHECK CONSTRAINT [FK_DataObjects_Dictionary]
GO
/****** Object:  ForeignKey [FK_DataProperties_DataObjects]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[DataProperties]  WITH CHECK ADD  CONSTRAINT [FK_DataProperties_DataObjects] FOREIGN KEY([DataObjectId])
REFERENCES [dbo].[DataObjects] ([DataObjectId])
GO
ALTER TABLE [dbo].[DataProperties] CHECK CONSTRAINT [FK_DataProperties_DataObjects]
GO
/****** Object:  ForeignKey [FK_DataProperties_PickList]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[DataProperties]  WITH CHECK ADD  CONSTRAINT [FK_DataProperties_PickList] FOREIGN KEY([PickListId])
REFERENCES [dbo].[PickList] ([PickListId])
GO
ALTER TABLE [dbo].[DataProperties] CHECK CONSTRAINT [FK_DataProperties_PickList]
GO
/****** Object:  ForeignKey [FK_DataRelationships_DataObjects]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[DataRelationships]  WITH CHECK ADD  CONSTRAINT [FK_DataRelationships_DataObjects] FOREIGN KEY([DataObjectID])
REFERENCES [dbo].[DataObjects] ([DataObjectId])
GO
ALTER TABLE [dbo].[DataRelationships] CHECK CONSTRAINT [FK_DataRelationships_DataObjects]
GO
/****** Object:  ForeignKey [FK_Dictionary_Applications]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[Dictionary]  WITH CHECK ADD  CONSTRAINT [FK_Dictionary_Applications] FOREIGN KEY([ApplicationID])
REFERENCES [dbo].[Applications] ([ApplicationId])
GO
ALTER TABLE [dbo].[Dictionary] CHECK CONSTRAINT [FK_Dictionary_Applications]
GO
/****** Object:  ForeignKey [FK__Exchanges__Commo__47A6A41B]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[Exchanges]  WITH CHECK ADD  CONSTRAINT [FK__Exchanges__Commo__47A6A41B] FOREIGN KEY([CommodityId])
REFERENCES [dbo].[Commodity] ([CommodityId])
GO
ALTER TABLE [dbo].[Exchanges] CHECK CONSTRAINT [FK__Exchanges__Commo__47A6A41B]
GO
/****** Object:  ForeignKey [FK__Folders__SiteId__59FA5E80]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[Folders]  WITH CHECK ADD  CONSTRAINT [FK__Folders__SiteId__59FA5E80] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
ALTER TABLE [dbo].[Folders] CHECK CONSTRAINT [FK__Folders__SiteId__59FA5E80]
GO
/****** Object:  ForeignKey [FK_Folders_Platforms]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[Folders]  WITH CHECK ADD  CONSTRAINT [FK_Folders_Platforms] FOREIGN KEY([PlatformId])
REFERENCES [dbo].[Platforms] ([PlatformId])
GO
ALTER TABLE [dbo].[Folders] CHECK CONSTRAINT [FK_Folders_Platforms]
GO
/****** Object:  ForeignKey [FKGS_PlatformId]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[GlobalSettings]  WITH CHECK ADD  CONSTRAINT [FKGS_PlatformId] FOREIGN KEY([PlatformId])
REFERENCES [dbo].[Platforms] ([PlatformId])
GO
ALTER TABLE [dbo].[GlobalSettings] CHECK CONSTRAINT [FKGS_PlatformId]
GO
/****** Object:  ForeignKey [FKGS_SiteId]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[GlobalSettings]  WITH CHECK ADD  CONSTRAINT [FKGS_SiteId] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
ALTER TABLE [dbo].[GlobalSettings] CHECK CONSTRAINT [FKGS_SiteId]
GO
/****** Object:  ForeignKey [FK__Graphs__Applicat__4D94879B]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[Graphs]  WITH CHECK ADD  CONSTRAINT [FK__Graphs__Applicat__4D94879B] FOREIGN KEY([ApplicationId])
REFERENCES [dbo].[Applications] ([ApplicationId])
GO
ALTER TABLE [dbo].[Graphs] CHECK CONSTRAINT [FK__Graphs__Applicat__4D94879B]
GO
/****** Object:  ForeignKey [FK__GroupRole__Group__5CD6CB2B]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[GroupRoles]  WITH CHECK ADD FOREIGN KEY([GroupId])
REFERENCES [dbo].[Groups] ([GroupId])
GO
/****** Object:  ForeignKey [FK__GroupRole__RoleI__5DCAEF64]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[GroupRoles]  WITH CHECK ADD FOREIGN KEY([RoleId])
REFERENCES [dbo].[Roles] ([RoleId])
GO
/****** Object:  ForeignKey [FKSchedule_ScheduleId]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[Job]  WITH CHECK ADD  CONSTRAINT [FKSchedule_ScheduleId] FOREIGN KEY([ScheduleId])
REFERENCES [dbo].[Schedule] ([ScheduleId])
GO
ALTER TABLE [dbo].[Job] CHECK CONSTRAINT [FKSchedule_ScheduleId]
GO
/****** Object:  ForeignKey [FK_Job_client_Info_Job]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[Job_client_Info_expired]  WITH CHECK ADD  CONSTRAINT [FK_Job_client_Info_Job] FOREIGN KEY([Job_Id])
REFERENCES [dbo].[Job_Expired] ([Job_id])
GO
ALTER TABLE [dbo].[Job_client_Info_expired] CHECK CONSTRAINT [FK_Job_client_Info_Job]
GO
/****** Object:  ForeignKey [FK_JobSchedule_Job]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[JobSchedule_Expired]  WITH CHECK ADD  CONSTRAINT [FK_JobSchedule_Job] FOREIGN KEY([Job_Id])
REFERENCES [dbo].[Job_Expired] ([Job_id])
GO
ALTER TABLE [dbo].[JobSchedule_Expired] CHECK CONSTRAINT [FK_JobSchedule_Job]
GO
/****** Object:  ForeignKey [FK_JobSchedule_Schedule]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[JobSchedule_Expired]  WITH CHECK ADD  CONSTRAINT [FK_JobSchedule_Schedule] FOREIGN KEY([Schedule_Id])
REFERENCES [dbo].[Schedule_Expired] ([Schedule_Id])
GO
ALTER TABLE [dbo].[JobSchedule_Expired] CHECK CONSTRAINT [FK_JobSchedule_Schedule]
GO
/****** Object:  ForeignKey [FK_KeyProperty_DataObjects]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[KeyProperty]  WITH CHECK ADD  CONSTRAINT [FK_KeyProperty_DataObjects] FOREIGN KEY([DataObjectId])
REFERENCES [dbo].[DataObjects] ([DataObjectId])
GO
ALTER TABLE [dbo].[KeyProperty] CHECK CONSTRAINT [FK_KeyProperty_DataObjects]
GO
/****** Object:  ForeignKey [FK_PickList_Dictionary]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[PickList]  WITH CHECK ADD  CONSTRAINT [FK_PickList_Dictionary] FOREIGN KEY([DictionaryId])
REFERENCES [dbo].[Dictionary] ([Dictionaryid])
GO
ALTER TABLE [dbo].[PickList] CHECK CONSTRAINT [FK_PickList_Dictionary]
GO
/****** Object:  ForeignKey [FK_PropertyMap_DataRelationships]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[PropertyMap]  WITH CHECK ADD  CONSTRAINT [FK_PropertyMap_DataRelationships] FOREIGN KEY([RelationshipID])
REFERENCES [dbo].[DataRelationships] ([RelationshipId])
GO
ALTER TABLE [dbo].[PropertyMap] CHECK CONSTRAINT [FK_PropertyMap_DataRelationships]
GO
/****** Object:  ForeignKey [FK__ResourceG__Group__619B8048]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[ResourceGroups]  WITH CHECK ADD FOREIGN KEY([GroupId])
REFERENCES [dbo].[Groups] ([GroupId])
GO
/****** Object:  ForeignKey [FK__ResourceG__Resou__7C1A6C5A]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[ResourceGroups]  WITH CHECK ADD FOREIGN KEY([ResourceTypeId])
REFERENCES [dbo].[ResourceType] ([ResourceTypeId])
GO
/****** Object:  ForeignKey [FK__RolePermi__Permi__1332DBDC]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[RolePermissions]  WITH CHECK ADD  CONSTRAINT [FK__RolePermi__Permi__1332DBDC] FOREIGN KEY([PermissionId])
REFERENCES [dbo].[Permissions] ([PermissionId])
GO
ALTER TABLE [dbo].[RolePermissions] CHECK CONSTRAINT [FK__RolePermi__Permi__1332DBDC]
GO
/****** Object:  ForeignKey [FK_Role_RoleId]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[RolePermissions]  WITH CHECK ADD  CONSTRAINT [FK_Role_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[Roles] ([RoleId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[RolePermissions] CHECK CONSTRAINT [FK_Role_RoleId]
GO
/****** Object:  ForeignKey [FK__UserGroup__Group__6477ECF3]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[UserGroups]  WITH CHECK ADD FOREIGN KEY([GroupId])
REFERENCES [dbo].[Groups] ([GroupId])
GO
/****** Object:  ForeignKey [FK__UserGroup__UserI__66603565]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[UserGroups]  WITH CHECK ADD FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([UserId])
GO
/****** Object:  ForeignKey [FK_ValueListMap_Applications]    Script Date: 04/16/2015 17:44:01 ******/
ALTER TABLE [dbo].[ValueListMap]  WITH CHECK ADD  CONSTRAINT [FK_ValueListMap_Applications] FOREIGN KEY([ApplicationId])
REFERENCES [dbo].[Applications] ([ApplicationId])
GO
ALTER TABLE [dbo].[ValueListMap] CHECK CONSTRAINT [FK_ValueListMap_Applications]
GO
