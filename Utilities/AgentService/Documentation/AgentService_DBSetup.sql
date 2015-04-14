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

/****** Object:  Table [dbo].[GlobalSettings]    Script Date: 10/29/2014 10:12:58 AM ******/
DROP TABLE [dbo].[GlobalSettings]
GO
/****** Object:  Table [dbo].[GlobalSettings]    Script Date: 10/29/2014 10:12:58 AM ******/
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
	[Active] [nvarchar](250) NULL
 CONSTRAINT [PKGlobalSettings] PRIMARY KEY CLUSTERED 
(
	[SiteId],[PlatformId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[GlobalSettings]  WITH CHECK ADD  CONSTRAINT [FKGS_SiteId] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
ALTER TABLE [dbo].[GlobalSettings]  WITH CHECK ADD  CONSTRAINT [FKGS_PlatformId] FOREIGN KEY([PlatformId])
REFERENCES [dbo].[Platforms] ([PlatformId])
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
	[Active] [tinyint] NOT NULL
 CONSTRAINT [PKJob] PRIMARY KEY CLUSTERED 
(
	[JobId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Job_client_Info]    Script Date: 10/29/2014 10:12:58 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[JobClientInfo](
	[SSo_Url] [nvarchar](250) NOT NULL,
	[Client_id] [nvarchar](250) NOT NULL,
	[Client_Secret] [nvarchar](250) NOT NULL,
	[Grant_Type] [nvarchar](50) NULL,
	[Request_Timeout] [int] NULL
) ON [PRIMARY]

GO


GO
/****** Object:  Table [dbo].[Schedule]    Script Date: 10/29/2014 10:12:58 AM ******/
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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Job]    Script Date: 01/08/2015 10:07:30 ******/
INSERT [dbo].[Job] ([JobId],[ScheduleId],[DataObjectId], [Is_Exchange], [Xid], [Cache_Page_size], [PlatformId], [SiteId], [Next_Start_DateTime], [Last_Start_DateTime], [Active] ) 
VALUES (N'5ddc49b3-1599-47e2-9f9a-ad37413d0987', N'5ddc49b3-1599-47e2-9f9a-ad3741355987', N'08FB6EE8-492C-4A58-9D7E-ABBF4E0B07F6', 0, '',1000, 2, 3, CAST(0x0000A41B009F1F8C AS DateTime), CAST(0x0000A41A009D29FC AS DateTime), 1)
GO
/****** Object:  Table [dbo].[Schedule]    Script Date: 01/08/2015 10:07:30 ******/
INSERT [dbo].[Schedule] ([ScheduleId], [Created_DateTime], [Created_By], [Occurance], [Weekday], [Start_DateTime], [End_DateTime], [Status]) VALUES (N'5ddc49b3-1599-47e2-9f9a-ad3741355987', CAST(0x0000A3800062E080 AS DateTime), N'Hemant', N'Daily', '', CAST(0x0000A3800062E080 AS DateTime), CAST(0x0000A4ED006B1DE0 AS DateTime), N'Ready')
GO

/****** Object:  Table [dbo].[JobClientInfo]    Script Date: 01/08/2015 10:07:30 ******/
INSERT [dbo].[JobClientInfo] ([SSo_Url], [Client_id], [Client_Secret], [Grant_Type], [Request_Timeout]) VALUES ( N'https://sso.mypsn.com/as/token.oauth2', N'iRingTools', N'0Lvnvat5T5OJk5n6VwD4optFJoq7/0POq++NfYkIgHYtmy6Pluix3aGy7EAN1Jxp', N'client_credentials', 300000)
GO


ALTER TABLE [dbo].[Job]  WITH CHECK ADD  CONSTRAINT [FKSchedule_ScheduleId] FOREIGN KEY([ScheduleId])
REFERENCES [dbo].[Schedule] ([ScheduleId])
GO
ALTER TABLE [dbo].[JobSchedule] CHECK CONSTRAINT [FK_JobSchedule_Schedule]
GO

USE [iRingAgentSchedule]
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

--ALTER TABLE Job_client_Info DROP COLUMN [Access_Token]
--ALTER TABLE Job_client_Info DROP COLUMN [App_Key]
--ALTER TABLE [dbo].[JobSchedule] ADD PlatformId int not null
--ALTER TABLE [dbo].[JobSchedule] ADD SiteId int not null
--ALTER TABLE [dbo].[JobSchedule] ADD TotalRecords int  null
--ALTER TABLE [dbo].[JobSchedule] ADD CachedRecords int  null
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
		where j.JobId = @JobId and Active = 1
		for xml PATH('job'), type, ELEMENTS XSINIL
   END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;

GO

--* ==================================================
--* Author:		<Hemant Gakhar>
--* Create date: <19-Feb-2015>
--* Description: Selecting Job
--* ==================================================

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'spgAllJobs')
DROP PROCEDURE spgAllJobs
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
		
				Declare @JobId uniqueidentifier = NewID() 
				Declare @ScheduleId uniqueidentifier = NewID()
				
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
				
				-- insert into job table
				Declare @Next_Start_DateTime datetime
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
END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO


SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- ===========================================================
-- Author:		<Hemant Gakhar>
-- Create date: <3-March-2015>
-- Description:	<updating Job>
-- ===========================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'spuJob')
DROP PROCEDURE spuJob
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

--* ==================================================
--* Author:		<Hemant Gakhar>
--* Create date: <19-Feb-2015>
--* Description: Delete Job
--* ==================================================

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'spdJob')
DROP PROCEDURE spdJob
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


-- ===========================================================
-- Author:		<Hemant Gakhar>
-- Create date: <3-March-2015>
-- Description:	<Get Schedule>
-- ===========================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'spgSchedule')
DROP PROCEDURE spgSchedule
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

-- =============================================
-- Author:	<Hemant Gakhar>
-- Create date: <19-Feb-2015>
-- Description:	<Insert Schedule> 

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'spiSchedule')
DROP PROCEDURE spiSchedule
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
-- ===========================================================
-- Author:		<Hemant Gakhar>
-- Create date: <3-March-2015>
-- Description:	<updating Schedule>
-- ===========================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'spuSchedule')
DROP PROCEDURE spuSchedule
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

-- ===========================================================
-- Author:		<Hemant Gakhar>
-- Create date: <3-March-2015>
-- Description:	<Delete Schedule>
-- ===========================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'spdSchedule')
DROP PROCEDURE spdSchedule
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



