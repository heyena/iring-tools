USE [master]
GO

IF EXISTS(SELECT name FROM sys.databases WHERE name = 'iRingAgentSchedule')
	DROP DATABASE [iRingAgentSchedule]
GO

CREATE DATABASE [iRingAgentSchedule] 
GO

IF EXISTS(SELECT * FROM sys.syslogins WHERE name = N'iRingAgentSchedule')
	DROP LOGIN [iRingAgentSchedule]
GO

CREATE LOGIN [iRingAgentSchedule] WITH PASSWORD = 'iRingAgentSchedule', CHECK_POLICY = OFF
GO

USE [iRingAgentSchedule]
GO

IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'iRingAgentSchedule') 
	DROP USER [iRingAgentSchedule]
GO

CREATE USER [iRingAgentSchedule] FOR LOGIN [iRingAgentSchedule] WITH DEFAULT_SCHEMA=[dbo]
GO

EXEC sp_addrolemember 'db_owner', N'iRingAgentSchedule'
GO

USE [iRingAgentSchedule]
GO
ALTER TABLE [dbo].[JobSchedule] DROP CONSTRAINT [FK_JobSchedule_Schedule]
GO
ALTER TABLE [dbo].[JobSchedule] DROP CONSTRAINT [FK_JobSchedule_Job]
GO
ALTER TABLE [dbo].[Job_client_Info] DROP CONSTRAINT [FK_Job_client_Info_Job]
GO
/****** Object:  Table [dbo].[Schedule]    Script Date: 10/29/2014 10:12:57 AM ******/
DROP TABLE [dbo].[Schedule]
GO
/****** Object:  Table [dbo].[JobSchedule]    Script Date: 10/29/2014 10:12:58 AM ******/
DROP TABLE [dbo].[JobSchedule]
GO
/****** Object:  Table [dbo].[Job_client_Info]    Script Date: 10/29/2014 10:12:58 AM ******/
DROP TABLE [dbo].[Job_client_Info]
GO
/****** Object:  Table [dbo].[Job]    Script Date: 10/29/2014 10:12:58 AM ******/
DROP TABLE [dbo].[Job]
GO
/****** Object:  Table [dbo].[Job]    Script Date: 10/29/2014 10:12:58 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Job](
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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Job_client_Info]    Script Date: 10/29/2014 10:12:58 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Job_client_Info](
	[Job_Id] [uniqueidentifier] NOT NULL,
	[SSo_Url] [nvarchar](250) NOT NULL,
	[Client_id] [nvarchar](250) NOT NULL,
	[Client_Secret] [nvarchar](250) NOT NULL,
	[Access_Token] [nvarchar](250) NOT NULL,
	[App_Key] [nvarchar](250) NOT NULL,
	[Grant_Type] [nvarchar](50) NULL,
	[Request_Timeout] [int] NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[JobSchedule]    Script Date: 10/29/2014 10:12:58 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[JobSchedule](
	[Schedule_Id] [uniqueidentifier] NOT NULL,
	[Job_Id] [uniqueidentifier] NOT NULL,
	[Next_Start_DateTime] [datetime] NULL,
	[Last_Start_DateTime] [datetime] NULL,
	[Active] [tinyint] NOT NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Schedule]    Script Date: 10/29/2014 10:12:58 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Schedule](
	[Schedule_Id] [uniqueidentifier] NOT NULL,
	[Created_DateTime] [datetime] NOT NULL,
	[Created_By] [nvarchar](250) NOT NULL,
	[Occurance] [nvarchar](50) NULL,
	[Start_DateTime] [datetime] NULL,
	[End_DateTime] [datetime] NULL,
	[Status] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Schedule] PRIMARY KEY CLUSTERED 
(
	[Schedule_Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Job]    Script Date: 01/08/2015 10:07:30 ******/
INSERT [dbo].[Job] ([Job_id], [Is_Exchange], [Scope], [App], [DataObject], [Xid], [Exchange_Url], [Cache_Page_size]) VALUES (N'5ddc49b3-1599-47e2-9f9a-ad37413d0987', 0, N'99999_000', N'iw', N'classes', NULL, NULL, N'2000')
GO
/****** Object:  Table [dbo].[Schedule]    Script Date: 01/08/2015 10:07:30 ******/
INSERT [dbo].[Schedule] ([Schedule_Id], [Created_DateTime], [Created_By], [Occurance], [Start_DateTime], [End_DateTime], [Status]) VALUES (N'5ddc49b3-1599-47e2-9f9a-ad3741355987', CAST(0x0000A3800062E080 AS DateTime), N'Hemant', N'Daily', CAST(0x0000A3800062E080 AS DateTime), CAST(0x0000A4ED006B1DE0 AS DateTime), N'Ready')
GO
/****** Object:  Table [dbo].[JobSchedule]    Script Date: 01/08/2015 10:07:30 ******/
INSERT [dbo].[JobSchedule] ([Schedule_Id], [Job_Id], [Next_Start_DateTime], [Last_Start_DateTime], [Active]) VALUES (N'5ddc49b3-1599-47e2-9f9a-ad3741355987', N'5ddc49b3-1599-47e2-9f9a-ad37413d0987', CAST(0x0000A41B009F1F8C AS DateTime), CAST(0x0000A41A009D29FC AS DateTime), 1)
GO
/****** Object:  Table [dbo].[Job_client_Info]    Script Date: 01/08/2015 10:07:30 ******/
INSERT [dbo].[Job_client_Info] ([Job_Id], [SSo_Url], [Client_id], [Client_Secret], [Access_Token], [App_Key], [Grant_Type], [Request_Timeout]) VALUES (N'5ddc49b3-1599-47e2-9f9a-ad37413d0987', N'https://sso.mypsn.com/as/token.oauth2', N'iRingTools', N'0Lvnvat5T5OJk5n6VwD4optFJoq7/0POq++NfYkIgHYtmy6Pluix3aGy7EAN1Jxp', N'', N'', N'client_credentials', 300000)

GO
ALTER TABLE [dbo].[Job_client_Info]  WITH CHECK ADD  CONSTRAINT [FK_Job_client_Info_Job] FOREIGN KEY([Job_Id])
REFERENCES [dbo].[Job] ([Job_id])
GO
ALTER TABLE [dbo].[Job_client_Info] CHECK CONSTRAINT [FK_Job_client_Info_Job]
GO
ALTER TABLE [dbo].[JobSchedule]  WITH CHECK ADD  CONSTRAINT [FK_JobSchedule_Job] FOREIGN KEY([Job_Id])
REFERENCES [dbo].[Job] ([Job_id])
GO
ALTER TABLE [dbo].[JobSchedule] CHECK CONSTRAINT [FK_JobSchedule_Job]
GO
ALTER TABLE [dbo].[JobSchedule]  WITH CHECK ADD  CONSTRAINT [FK_JobSchedule_Schedule] FOREIGN KEY([Schedule_Id])
REFERENCES [dbo].[Schedule] ([Schedule_Id])
GO
ALTER TABLE [dbo].[JobSchedule] CHECK CONSTRAINT [FK_JobSchedule_Schedule]
GO

USE [iRingAgentSchedule]
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO
--* ==================================================
--* Author:		<Hemant Gakhar>
--* Create date: <19-Feb-2015>
--* Description: Selecting Job
--* ==================================================

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'spgJob')
DROP PROCEDURE spgJob
GO


CREATE  PROCEDURE [dbo].[spgJob] 
(
	@ScopeName NVARCHAR(255),
	@AppName NVARCHAR(255)
)
AS
BEGIN TRY
	BEGIN
	  SELECT [Job_id],[is_exchange],[Scope],[App],[DataObject],[Xid],[Exchange_Url],[cache_Page_Size] FROM Job
		  WHERE Scope=@ScopeName  AND App=@AppName 			
	END
		
END TRY
BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;

GO

-- =============================================
-- Author:	<Hemant Gakhar>
-- Create date: <19-Feb-2015>
-- Description:	<Insert Job> 

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'spiJob')
DROP PROCEDURE spiJob
GO

CREATE PROCEDURE [dbo].[spiJob] 
(
	@SiteId INT,
	@GroupName NVARCHAR(100),
	@GroupDesc NVARCHAR(100)
)
	 
AS
BEGIN TRY
	BEGIN
						
		If not Exists(Select top 1 * from Groups Where GroupName = @GroupName and Active = 1)
		Begin
			INSERt INTO  Groups (SiteId,GroupName,GroupDesc )
			VALUES (@SiteId,@GroupName,@GroupDesc)
			
			Select '1'--'Job added successfully!'
		End
		Else
			Select '0'--'Job with this name already exists!'
	END
END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO





