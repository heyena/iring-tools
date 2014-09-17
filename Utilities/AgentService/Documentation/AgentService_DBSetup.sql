USE [master]
GO

IF EXISTS(SELECT name FROM sys.databases WHERE name = 'iRINGAgent')
	DROP DATABASE [iRINGAgent]
GO

CREATE DATABASE [iRINGAgent] 
GO

IF EXISTS(SELECT * FROM sys.syslogins WHERE name = N'iRINGAgent')
	DROP LOGIN [iRINGAgent]
GO

CREATE LOGIN [iRINGAgent] WITH PASSWORD = 'iRINGAgent', CHECK_POLICY = OFF
GO

USE [iRINGAgent]
GO

IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'iRINGAgent') 
	DROP USER [iRINGAgent]
GO

CREATE USER [iRINGAgent] FOR LOGIN [iRINGAgent] WITH DEFAULT_SCHEMA=[dbo]
GO

EXEC sp_addrolemember 'db_owner', N'iRINGAgent'
GO

IF EXISTS (SELECT * FROM sys.all_objects WHERE name = N'TASKS')
	DROP TABLE [dbo].[TASKS]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'spgTasks')
DROP PROCEDURE spgTasks
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'spiTasks')
DROP PROCEDURE spiTasks
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'spuTasks')
DROP PROCEDURE spuTasks
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'spdTasks')
DROP PROCEDURE spdTasks
GO



IF EXISTS (SELECT * FROM sys.all_objects WHERE name = N'ScheduleCache')
	DROP TABLE [dbo].[ScheduleCache]
GO



--* ==================================================
--*  Create Table ScheduleCache
--* ==================================================

CREATE TABLE [dbo].[ScheduleCache](
	[Schedule_Cache_Id] UNIQUEIDENTIFIER DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
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
	[Start_Time] DateTime NOT NULL,
	[End_Time] DateTime NULL,
	[Status] [nvarchar] (64) NOT NULL,
	[Active] [tinyint] NOT NULL
 )
 
GO

IF EXISTS (SELECT * FROM sys.all_objects WHERE name = N'ScheduleExchange')
	DROP TABLE [dbo].[ScheduleExchange]
GO

--* ==================================================
--*  Create Table ScheduleExchange
--* ==================================================

CREATE TABLE [dbo].[ScheduleExchange](
	[Schedule_Exchange_Id] UNIQUEIDENTIFIER DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
	[Task_Name] [nvarchar](100) NOT NULL,
	[Scope] [nvarchar](64) NULL,
	[Base_Url] [nvarchar](256) NULL,
	[Exchange_Id] [nvarchar](64) NULL,
	[Sso_Url] [nvarchar](256) NULL,
	[Client_Id] [nvarchar](64) NULL,
	[Client_Secret] [nvarchar](64) NULL,
	[Grant_Type] [nvarchar](64) NULL,
	[Request_Timeout] [int] NULL,
	[Start_Time] DateTime NOT NULL,
	[End_Time] DateTime NULL,
	[Status] [nvarchar] (64) NOT NULL,
	[Active] [tinyint] NOT NULL
 )
 
GO



--*ALTER TABLE [dbo].[TASK_CONNECTION]  WITH CHECK ADD  CONSTRAINT [FK_TASK_CONNECTION_TASKS] FOREIGN KEY([TASK_ID])
--*REFERENCES [dbo].[TASKS] ([TASK_ID])
--*GO

--*ALTER TABLE [dbo].[TASK_CONNECTION] CHECK CONSTRAINT [FK_TASK_CONNECTION_TASKS]
GO

--* ==================================================
--*  Create Procedures
--* ==================================================

SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER OFF
GO

--* ==================================================
--*  Procedure ScheduleCache Get
--* ==================================================

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'spgScheduleCache')
DROP PROCEDURE spgScheduleCache
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
	[STATUS] AS [STATUS],
	[ACTIVE] AS [ACTIVE]
FROM [dbo].[ScheduleCache] WITH (NoLock)
WHERE
	[dbo].[ScheduleCache].[Schedule_Cache_Id] = @Schedule_Cache_Id

GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'spiScheduleCache')
DROP PROCEDURE spiScheduleCache
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
	@STATUS,
	@ACTIVE)

GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'spuScheduleCache')
DROP PROCEDURE spuScheduleCache
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
	[STATUS] = @STATUS,
	[ACTIVE] = @ACTIVE
WHERE [dbo].[ScheduleCache].[Schedule_Cache_Id] = @SCHEDULE_CACHE_ID

GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'spdScheduleCache')
DROP PROCEDURE spdScheduleCache
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


--* ==================================================
--*  Procedure ScheduleExchange Get
--* ==================================================

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'spgScheduleExchange')
DROP PROCEDURE spgScheduleExchange
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
	[STATUS] AS [STATUS],
	[ACTIVE] AS [ACTIVE]
FROM [dbo].[ScheduleExchange] WITH (NoLock)
WHERE
	[dbo].[ScheduleExchange].[Schedule_Exchange_Id] = @Schedule_Exchange_Id

GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'spiScheduleExchange')
DROP PROCEDURE spiScheduleExchange
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
	@STATUS,
	@ACTIVE)
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'spuScheduleExchange')
DROP PROCEDURE spuScheduleExchange
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
	[STATUS] = @STATUS,
	[ACTIVE] = @ACTIVE
WHERE [dbo].[ScheduleExchange].[SCHEDULE_EXCHANGE_ID] = @SCHEDULE_EXCHANGE_ID

GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'spdScheduleExchange')
DROP PROCEDURE spdScheduleExchange
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


GO

