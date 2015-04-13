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
	[PlatformId] [int] NOT NULL,
	[SiteId] [int] NOT NULL,
	[Next_Start_DateTime] [datetime] NULL,
	[Last_Start_DateTime] [datetime] NULL,
	[TotalRecords] [int] NULL,
	[CachedRecords] [int] NULL,
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
	[Weekday] [nvarchar](50) NULL,
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
INSERT [dbo].[Schedule] ([Schedule_Id], [Created_DateTime], [Created_By], [Occurance], [Weekday], [Start_DateTime], [End_DateTime], [Status]) VALUES (N'5ddc49b3-1599-47e2-9f9a-ad3741355987', CAST(0x0000A3800062E080 AS DateTime), N'Hemant', N'Daily', '', CAST(0x0000A3800062E080 AS DateTime), CAST(0x0000A4ED006B1DE0 AS DateTime), N'Ready')
GO
/****** Object:  Table [dbo].[JobSchedule]    Script Date: 01/08/2015 10:07:30 ******/
INSERT [dbo].[JobSchedule] ([Schedule_Id], [Job_Id], [PlatformId], [SiteId], [Next_Start_DateTime], [Last_Start_DateTime], [Active]) VALUES (N'5ddc49b3-1599-47e2-9f9a-ad3741355987', N'5ddc49b3-1599-47e2-9f9a-ad37413d0987', 2,3,CAST(0x0000A41B009F1F8C AS DateTime), CAST(0x0000A41A009D29FC AS DateTime), 1)
GO
/****** Object:  Table [dbo].[Job_client_Info]    Script Date: 01/08/2015 10:07:30 ******/
INSERT [dbo].[Job_client_Info] ([Job_Id], [SSo_Url], [Client_id], [Client_Secret], [Grant_Type], [Request_Timeout]) VALUES (N'5ddc49b3-1599-47e2-9f9a-ad37413d0987', N'https://sso.mypsn.com/as/token.oauth2', N'iRingTools', N'0Lvnvat5T5OJk5n6VwD4optFJoq7/0POq++NfYkIgHYtmy6Pluix3aGy7EAN1Jxp', N'client_credentials', 300000)



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
	@PlatformId int,
	@SiteId int,
	@ScopeName NVARCHAR(255),
	@AppName NVARCHAR(255)
)
AS
BEGIN TRY
   BEGIN
	 
    WITH XMLNAMESPACES ( DEFAULT 'http://www.iringtools.org/library')

	SELECT 
		j.Job_id as job_Id,
		j.is_exchange as isExchange,
		j.Scope as scope,
		j.App as app,
		j.DataObject as dataObject,
		j.Xid as xid,
		j.Exchange_Url as exchange_Url,
		j.cache_Page_Size as cache_page_size,
		(
		  Select
			distinct
			jci.job_id as Job_Id,
			jci.sso_url as SSo_Url,
			jci.client_id as Client_id,
			jci.client_secret as Client_Secret,
			jci.grant_type as Grant_Type,
			jci.request_timeout as Request_Timeout
			from Job_client_info jci
			where j.job_id = jci.job_id for xml PATH('jobclientinfo'), type 
		) as 'jobclientinfos',
		(
		  select 
		    s.schedule_id as Schedule_Id,
		    s.created_datetime as Created_DateTime, 
		    s.Created_By as Created_By, 
		    s.occurance as Occurance, 
		    s.[weekday] as Weekday, 
		    s.start_datetime as Start_DateTime, 
		    s.end_datetime as End_DateTime, 
		    s.[status] as Status
		    from schedule s, jobschedule js
		    where j.job_id = js.job_id and js.schedule_id = s.schedule_id and js.PlatformId = @PlatformId and js.SiteId = @SiteId for xml PATH('schedule'), type
		) as 'schedules',
		(
		  select
		    js.Schedule_Id as Schedule_Id,
			js.Job_Id as Job_Id,
			js.PlatformId as PlatformId,
			js.SiteId as SiteId,
			js.Next_Start_DateTime as Next_Start_DateTime,
			js.Last_Start_DateTime as Last_Start_DateTime,
			js.TotalRecords as TotalRecords,
			js.CachedRecords as CachedRecords,
			js.Active as Active
			from schedule s, jobschedule js
			where j.job_id = js.job_id and js.schedule_id = s.schedule_id and js.PlatformId = @PlatformId and js.SiteId = @SiteId for xml PATH('jobschedule'), type
		) as 'jobschedules'
		from job j
		where j.Scope=@ScopeName  AND j.App=@AppName 
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
	@PlatformId int,
	@SiteId int
)
AS
BEGIN TRY
	BEGIN
	  --SELECT [Job_id],[is_exchange],[Scope],[App],[DataObject],[Xid],[Exchange_Url],[cache_Page_Size] FROM Job
	  --SELECT  j.Job_id,j.is_exchange,j.Scope,j.App,j.DataObject,j.Xid,j.Exchange_Url,j.cache_Page_Size,
      --jci.sso_url, jci.client_id, jci.client_secret, jci.grant_type, jci.request_timeout,
      --js.next_start_datetime, js.last_start_datetime, js.active,
      --s.created_datetime, s.Created_By, s.occurance, s.[weekday], s.start_datetime, s.end_datetime, s.[status]
      --FROM Job j, Job_client_info jci, Schedule s, jobschedule js
      --WHERE j.job_id = jci.job_id 
      --and j.job_id = js.job_id
      --and js.schedule_id = s.schedule_id
	  WITH XMLNAMESPACES ( DEFAULT 'http://www.iringtools.org/library')
	  
	  select distinct (SELECT 
		j.Job_id as job_Id,
		j.is_exchange as isExchange,
		j.Scope as scope,
		j.App as app,
		j.DataObject as dataObject,
		j.Xid as xid,
		j.Exchange_Url as exchange_Url,
		j.cache_Page_Size as cache_page_size,
		(
		  Select
			distinct
			jci.job_id as Job_Id,
			jci.sso_url as SSo_Url,
			jci.client_id as Client_id,
			jci.client_secret as Client_Secret,
			jci.grant_type as Grant_Type,
			jci.request_timeout as Request_Timeout
			from Job_client_info jci
			where j.job_id = jci.job_id for xml PATH('jobclientinfo'), type 
		) as 'jobclientinfos',
		(
		  select 
		    s.schedule_id as Schedule_Id,
		    s.created_datetime as Created_DateTime, 
		    s.Created_By as Created_By, 
		    s.occurance as Occurance, 
		    s.[weekday] as Weekday, 
		    s.start_datetime as Start_DateTime, 
		    s.end_datetime as End_DateTime, 
		    s.[status] as Status
		    from schedule s, jobschedule js
		    where j.job_id = js.job_id and js.schedule_id = s.schedule_id and js.PlatformId = @PlatformId and js.SiteId = @SiteId for xml PATH('schedule'), type
		) as 'schedules',
		(
		  select
		    js.Schedule_Id as Schedule_Id,
			js.Job_Id as Job_Id,
			js.PlatformId as PlatformId,
			js.SiteId as SiteId,
			js.Next_Start_DateTime as Next_Start_DateTime,
			js.Last_Start_DateTime as Last_Start_DateTime,
			js.TotalRecords as TotalRecords,
			js.CachedRecords as CachedRecords,
			js.Active as Active
			from schedule s, jobschedule js
			where j.job_id = js.job_id and js.schedule_id = s.schedule_id and js.PlatformId = @PlatformId and js.SiteId = @SiteId for xml PATH('jobschedule'), type
		) as 'jobschedules'
		from job j for xml PATH('job')) from job  for xml PATH('jobs') , type, ELEMENTS XSINIL
		 			
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
	@Is_Exchange INT,
	@Scope NVARCHAR(50),
	@App NVARCHAR(50),
	@DataObject NVARCHAR(50),
	@Xid NVARCHAR(50),
	@Exchange_Url NVARCHAR(250),
	@Cache_Page_Size NVARCHAR(50),
	@PlatformId int,
	@SiteId int,
	@Schedules xml 
)
	 
AS
BEGIN TRY
	BEGIN
						
		If not Exists(Select top 1 * from Job Where Scope = @Scope and App = @App and 
		    Is_Exchange = @Is_Exchange)
		  Begin
			Declare @Job_Id uniqueidentifier = NewID() 
			
			INSERT INTO  Job (Job_Id,Is_Exchange,Scope,App,DataObject,Xid,Exchange_Url,Cache_Page_Size )
			VALUES (@Job_Id,@Is_Exchange,@Scope,@App,@DataObject,@Xid,@Exchange_Url,@Cache_Page_Size)
			
			--insert into schedule table
			Declare @Schedule_id uniqueidentifier = NewID() 
			Insert into schedule (Schedule_Id, Created_DateTime, Created_By, Occurance, Weekday, Start_DateTime, End_DateTime, Status)
			select 
			@Schedule_id,
			convert(datetime, T.N.value('(Created_DateTime)[1]', 'NVARCHAR(100)'), 121),
			T.N.value('(Created_By)[1]', 'NVARCHAR(250)') as Created_By,
			T.N.value('(Occurance)[1]', 'NVARCHAR(50)') as Occurance,
			T.N.value('(Weekday)[1]', 'NVARCHAR(50)') as Weekday,
			convert(datetime, T.N.value('(Start_DateTime)[1]', 'NVARCHAR(100)'), 121) ,
			convert(datetime, T.N.value('(End_DateTime)[1]', 'NVARCHAR(100)'), 121),
			T.N.value('(Status)[1]', 'NVARCHAR(50)') as Status
			from @Schedules.nodes('schedules/schedule') as T(N)
			
			--insert into job schedule
			Insert into jobschedule([Schedule_id],[Job_Id],[PlatformId],[SiteId],[Next_Start_DateTime],[Active])
			select
			  @Schedule_id ,
			  @Job_Id,
			  @PlatformId,
			  @SiteId,
			  convert(datetime, T.N.value('(Start_DateTime)[1]', 'NVARCHAR(100)'), 121) ,
			  1
			  from @Schedules.nodes('schedules/schedule') as T(N)
			  
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
	@Job_Id uniqueidentifier,
	@Is_Exchange INT,
	@Scope NVARCHAR(50),
	@App NVARCHAR(50),
	@DataObject NVARCHAR(50),
	@Xid NVARCHAR(50),
	@Exchange_Url NVARCHAR(250),
	@Cache_Page_Size NVARCHAR(50),
	@Schedules xml,
	@JobSchedules xml
)	 
AS
BEGIN TRY
	BEGIN
	
	UPDATE [Job]
	   SET 
		  [Is_Exchange] = @Is_Exchange,
		  [Scope] = @Scope,
		  [App] = @App,
		  [DataObject] = @DataObject,
		  [Xid] = @Xid,
		  [Exchange_Url] = @Exchange_Url,
		  [Cache_Page_Size] = @Cache_Page_Size
	WHERE Job_Id = @Job_Id 
	 
	Declare @Schedule_Id uniqueidentifier
	SET @Schedule_Id =  (SELECT  T.N.value('(Schedule_Id)[1]', 'NVARCHAR(250)') as Schedule_Id from @Schedules.nodes('schedules/schedule') as T(N))
	
	 --update table schedule 
	 UPDATE [schedule]
		SET 
		Created_DateTime = convert(datetime, T.N.value('(Created_DateTime)[1]', 'NVARCHAR(100)'), 121),
		Created_By = T.N.value('(Created_By)[1]', 'NVARCHAR(250)'),
		Occurance = T.N.value('(Occurance)[1]', 'NVARCHAR(50)'),
		Weekday = T.N.value('(Weekday)[1]', 'NVARCHAR(50)'),
		Start_DateTime = convert(datetime, T.N.value('(Start_DateTime)[1]', 'NVARCHAR(100)'), 121), 
		End_DateTime = convert(datetime, T.N.value('(End_DateTime)[1]', 'NVARCHAR(100)'), 121),
		Status = T.N.value('(Status)[1]', 'NVARCHAR(50)') 
		from @Schedules.nodes('schedules/schedule') as T(N)	
		WHERE Schedule_Id = @Schedule_Id 
		
	-- update job schedule
	UPDATE [jobschedule]
		SET
		Next_Start_DateTime = convert(datetime, JS.N.value('(Next_Start_DateTime)[1]', 'NVARCHAR(100)'), 121),
		Last_Start_DateTime = convert(datetime, JS.N.value('(Last_Start_DateTime)[1]', 'NVARCHAR(100)'), 121),
		Active = JS.N.value('(Active)[1]', 'tinyint'),
		PlatformId = JS.N.value('(PlatformId)[1]', 'int'),
		SiteId = JS.N.value('(SiteId)[1]', 'int'),
		TotalRecords = JS.N.value('(TotalRecords)[1]', 'int'),
		CachedRecords = JS.N.value('(CachedRecords)[1]', 'int')
		from @JobSchedules.nodes('jobschedules/jobschedule') as JS(N)	
		WHERE Job_Id = @Job_Id and Schedule_Id = @Schedule_Id 
		
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
	  -- delete from child tables
	  declare @ScheduleId uniqueidentifier
	  SET @ScheduleId = (select Schedule_Id from JobSchedule where Job_Id = @JobId)
	  
	  
	  DELETE FROM Job_Client_Info where Job_Id = @JobId

	  DELETE from JobSchedule where job_id = @JobId and schedule_id = @ScheduleId 
	  
	  DELETE From Schedule where schedule_id = @ScheduleId
	  
	  DELETE from Job where Job_Id = @JobId
	  
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
-- Description:	<Get Job_Client_Info>
-- ===========================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'spgJob_Client_Info')
DROP PROCEDURE spgJob_Client_Info
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
-- =============================================
-- Author:	<Hemant Gakhar>
-- Create date: <19-Feb-2015>
-- Description:	<Insert Job_Client_Info> 

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'spiJob_client_Info')
DROP PROCEDURE spiJob_client_Info
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
-- ===========================================================
-- Author:		<Hemant Gakhar>
-- Create date: <3-March-2015>
-- Description:	<updating Job Client Info>
-- ===========================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'spuJob_client_Info')
DROP PROCEDURE spuJob_client_Info
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

-- ===========================================================
-- Author:		<Hemant Gakhar>
-- Create date: <3-March-2015>
-- Description:	<Delete Job Client Info>
-- ===========================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'spdJobClientInfo')
DROP PROCEDURE spdJobClientInfo
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
	@Schedule_Id uniqueidentifier
)
AS
BEGIN TRY
	BEGIN
	  SELECT [Schedule_id],[Created_DateTime],[Created_By],[Occurance],[Weekday],[Start_DateTime],[End_DateTime],[Status] FROM Schedule
		  WHERE Schedule_Id=@Schedule_Id			
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
						
		--If not Exists(Select top 1 * from Schedule Where Schedule_Id = @Schedule_Id )
		  Begin
			Declare @Schedule_id uniqueidentifier = NewID() 
			
			if @Status = ''
			  Begin
				Set @Status = 'Ready'
			  End
			
			INSERT INTO  Schedule (Schedule_Id, Created_DateTime, Created_By, Occurance, Weekday, Start_DateTime, End_DateTime, Status)
			VALUES (@Schedule_Id, convert(datetime, @Created_DateTime, 121), @Created_By, @Occurance, @Weekday, convert(datetime, @Start_DateTime, 121), convert(datetime, @End_DateTime, 121), @Status )
			
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
	@Schedule_Id uniqueidentifier,
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
		  [Schedule_Id] = @Schedule_Id,
		  [Created_DateTime] = convert(datetime, @Created_DateTime, 121),
		  [Created_By] = @Created_By,
		  [Occurance] = @Occurance,
		  [Weekday] = @Weekday,
		  [Start_DateTime] = convert(datetime, @Start_DateTime, 121),
		  [End_DateTime] = convert(datetime, @End_DateTime, 121),
		  [Status] = @Status
	 WHERE Schedule_Id = @Schedule_Id 

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
	  DELETE FROM JobSchedule where Schedule_Id = @ScheduleId
	  
	  DELETE FROM Schedule where Schedule_Id = @ScheduleId
	  Select  '1'--'Schedule Deleted successfully!'
	END
		
END TRY
BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;

GO


-- ===========================================================
-- Author:		<Hemant Gakhar>
-- Create date: <3-March-2015>
-- Description:	<Get JobSchedule>
-- ===========================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'spgJobSchedule')
DROP PROCEDURE spgJobSchedule
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
-- =============================================
-- Author:	<Hemant Gakhar>
-- Create date: <19-Feb-2015>
-- Description:	<Insert JobSchedule> 

-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'spiJobSchedule')
DROP PROCEDURE spiJobSchedule
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

-- ===========================================================
-- Author:		<Hemant Gakhar>
-- Create date: <3-March-2015>
-- Description:	<updating JobSchedule>
-- ===========================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'spuJobSchedule')
DROP PROCEDURE spuJobSchedule
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

-- ===========================================================
-- Author:		<Hemant Gakhar>
-- Create date: <3-March-2015>
-- Description:	<Delete Schedule>
-- ===========================================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'spdJobSchedule')
DROP PROCEDURE spdJobSchedule
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

