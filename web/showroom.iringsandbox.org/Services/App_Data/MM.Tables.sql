USE [master]
GO

IF EXISTS(SELECT name FROM sys.databases WHERE name = 'MM')
	DROP DATABASE [MM]
GO

CREATE DATABASE [MM] 
GO

IF EXISTS(SELECT * FROM sys.syslogins WHERE name = N'mm')
	DROP LOGIN [mm]
GO

CREATE LOGIN [mm] WITH PASSWORD = 'mm', CHECK_POLICY = OFF
GO

USE [MM]
GO

IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'mm') 
	DROP USER [mm]
GO

CREATE USER [mm] FOR LOGIN [mm] WITH DEFAULT_SCHEMA=[dbo]
GO

EXEC sp_addrolemember 'db_owner', N'mm'
GO

IF EXISTS (SELECT * FROM sys.all_objects WHERE name = N'VALVE')
	DROP TABLE [dbo].[VALVE]
GO

CREATE TABLE [dbo].[VALVE](
	[ITEM] [varchar](64) NULL,
	[JOB_NO] [varchar](5) NULL,
	[CIN] [varchar](26) NULL,
	[TAGNO] [varchar](20) NULL,
	[UNIT_SUBDIV] [varchar](3) NULL,
	[GROUPCODE] [varchar](2) NULL,
	[B_SYS_LOC] [varchar](5) NULL,
	[SEQCODE] [varchar](12) NULL,
	[CLIENTSYSLOC] [varchar](4) NULL,
	[VALVESEQNO] [varchar](7) NULL,
	[SUFFIX] [varchar](2) NULL,
	[QUANTITY] [int] NULL,
	[CHAIN_OP] [varchar](1) NULL,
	[LINESEQNO] [varchar](5) NULL,
	[REL_FIELD] [varchar](1) NULL,
	[MPAGSPECIALTY] [varchar](12) NULL,
	[MPAGTAKEOFF] [varchar](2) NULL,
	[PWDESCRIPTION] [varchar](88) NULL,
	[STELLITE] [varchar](1) NULL,
	[SCHEDULE] [varchar](8) NULL,
	[CONST_REMARKS] [varchar](24) NULL,
	[DIAMETER] [float] NULL,
	[CODE] [varchar](32) NULL,
	[END_PREP] [varchar](2) NULL,
	[OP_ORIEN] [varchar](1) NULL,
	[ACTUATOR] [varchar](2) NULL,
	[STOCK_CODE] [varchar](12) NULL,
	[TYPE] [varchar](3) NULL,
	[LENGTH] [float] NULL,
	[LINE_CLASS] [varchar](3) NULL,
	[ABV_BEL_RACK] [varchar](1) NULL,
	[HEAT_TRACE] [varchar](2) NULL,
	[INSULATED_YN] [varchar](1) NULL,
	[INSL_MATL] [varchar](10) NULL,
	[INS_THK] [float] NULL,
	[LOCATION] [varchar](5) NULL,
	[PID] [varchar](20) NULL,
	[PIDREV] [varchar](3) NULL,
	[ENGR_REL] [datetime] NULL,
	[HOLDDESC] [varchar](10) NULL,
	[PID_LOC] [varchar](4) NULL,
	[REV] [varchar](3) NULL,
	[ISO_DWG] [varchar](25) NULL,
	[SPOOLSEQNO] [varchar](5) NULL,
	[SPOOLSUFFIX] [varchar](1) NULL,
	[SPOOLSEQCOD] [varchar](12) NULL,
	[MR_NO] [varchar](10) NULL,
	[LOCK_DEVICE] [varchar](4) NULL,
	[VENDSUPL] [varchar](1) NULL,
	[STARTUP] [varchar](5) NULL,
	[VENDOR] [varchar](12) NULL,
	[SCN] [varchar](15) NULL,
	[STOR_LOC] [varchar](6) NULL,
	[MWR] [varchar](10) NULL,
	[BSAP] [varchar](6) NULL,
	[DELETED] [varchar](1) NULL,
	[DEL_BY] [varchar](10) NULL,
	[UPDATED] [varchar](1) NULL,
	[UPDATED_BY] [varchar](10) NULL,
	[DATED] [datetime] NULL,
	[LOCKFLAG] [varchar](1) NULL,
	[LOCKLEVEL] [numeric](18, 0) NULL,
	[LOCKREASON] [varchar](128) NULL,
	[SHOP_REQ] [datetime] NULL,
	[LENGTH_UOM] [varchar](10) NULL
) ON [PRIMARY]
GO

IF EXISTS (SELECT * FROM sys.all_objects WHERE name = N'LINE_DWG')
	DROP TABLE [dbo].[LINE_DWG]
GO

CREATE TABLE [dbo].[LINE_DWG](
	[ATTACHMENT_GROUP_ID] [varchar](32) NULL,
	[ATTACHMENT_ID] [varchar](32) NULL,
	[ATTACHMENT_OBJECT_TYPE] [varchar](64) NULL,
	[ATTACHMENT_CHILD_ID] [varchar](64) NULL,
	[ATTACHMENT_ORDER] [int] NULL,
	[ATTACHMENT_REVISION_CHARACTER] [varchar](16) NULL,
	[ATTACHMENT_RELATION_TYPE] [varchar](64) NULL,
	[ATTACHMENT_SIZE] [bigint] NULL,
	[ATTACHMENT_MIME_TYPE] [varchar](64) NULL,
	[ATTACHMENT_FILE_EXTENSION] [varchar](64) NULL,
	[ATTACHMENT_FILE_NAME] [varchar](64) NULL,
	[ATTACHMENT_REVISION_TYPE] [varchar](64) NULL,
	[ATTACHMENT_ROLE_FLAG] [varchar](1) NULL,
	[ATTACHMENT_CHECK_SUM] [varchar](64) NULL,
	[ATTACHMENT_DIFFERENCING_TOKEN] [varchar](64) NULL,
	[ATTACHMENT_LAST_UPDATED] [datetime] NULL,
	[ATTACHMENT_COMPRESSION_FLAG] [varchar](1) NULL,
	[ATTACHMENT_COMPRESSION_TYPE] [varchar](64) NULL
) ON [PRIMARY]
GO

IF EXISTS (SELECT * FROM sys.all_objects WHERE name = N'LINE')
	DROP TABLE [dbo].[LINE]
GO

CREATE TABLE [dbo].[LINE](
	[ITEM] [varchar](64) NULL,
	[JOB_NO] [varchar](5) NULL,
	[CIN] [varchar](26) NULL,
	[TAGNO] [varchar](20) NULL,
	[UNIT_SUBDIV] [varchar](3) NULL,
	[GROUPCODE] [varchar](2) NULL,
	[B_SYS_LOC] [varchar](5) NULL,
	[SEQCODE] [varchar](12) NULL,
	[LINESEQNO] [varchar](5) NULL,
	[CLIENTSYSLOC] [varchar](4) NULL,
	[DIAMETER] [float] NULL,
	[CLASS] [varchar](3) NULL,
	[DSGNCODE] [varchar](20) NULL,
	[LENGTH] [float] NULL,
	[ABV_BEL_RACK] [varchar](1) NULL,
	[HEAT_TRACE] [varchar](2) NULL,
	[INSULATED_YN] [varchar](1) NULL,
	[INSL_MATL] [varchar](10) NULL,
	[INS_PURP] [varchar](5) NULL,
	[INS_THK] [float] NULL,
	[PAINTCODE] [varchar](5) NULL,
	[COLORCODE] [varchar](10) NULL,
	[CRITICAL_PIPE] [varchar](1) NULL,
	[LOCATION] [varchar](5) NULL,
	[PID] [varchar](20) NULL,
	[PIDREV] [varchar](3) NULL,
	[HOLDDESC] [varchar](10) NULL,
	[REV] [varchar](3) NULL,
	[HYDRO] [varchar](8) NULL,
	[INSTALLED] [datetime] NULL,
	[BSAP] [varchar](6) NULL,
	[DELETED] [varchar](1) NULL,
	[DEL_BY] [varchar](10) NULL,
	[UPDATED] [varchar](1) NULL,
	[UPDATED_BY] [varchar](10) NULL,
	[DATED] [datetime] NULL,
	[LOCKFLAG] [varchar](1) NULL,
	[LOCKLEVEL] [numeric](18, 0) NULL,
	[LOCKREASON] [varchar](128) NULL,
	[LENGTH_UOM] [varchar](10) NULL,
	[ATTACHMENT_GROUP_ID] [varchar](32) NULL
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[HISTORY](
	[DESCRIPTION] [varchar](1000) NULL
) ON [PRIMARY]
GO

