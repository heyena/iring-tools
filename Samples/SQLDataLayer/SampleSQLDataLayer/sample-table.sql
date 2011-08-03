USE [master]
GO

IF EXISTS(SELECT name FROM sys.databases WHERE name = 'ABC')
	DROP DATABASE [ABC]
GO

CREATE DATABASE [ABC] 
GO

IF EXISTS(SELECT * FROM sys.syslogins WHERE name = N'abc')
	DROP LOGIN [abc]
GO

CREATE LOGIN [abc] WITH PASSWORD = 'abc', CHECK_POLICY = OFF
GO

USE [ABC]
GO

IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'abc') 
	DROP USER [abc]
GO

CREATE USER [abc] FOR LOGIN [abc] WITH DEFAULT_SCHEMA=[dbo]
GO

EXEC sp_addrolemember 'db_owner', N'abc'
GO

IF EXISTS (SELECT * FROM sys.all_objects WHERE name = N'LINES')
	DROP TABLE [dbo].[LINES]
GO

CREATE TABLE [dbo].[LINES](
	[TAG] [varchar](100) NOT NULL,
	[ID] [varchar](10) NULL,
	[AREA] [varchar](10) NULL,
	[TRAINNUMBER] [varchar](5) NULL,
	[SPEC] [varchar](10) NULL,
	[SYSTEM] [varchar](20) NULL,
	[LINENO] [varchar](15) NULL,
	[NOMDIAMETER] [float] NULL,
	[INSULATIONTYPE] [varchar](12) NULL,
	[HTRACED] [varchar](8) NULL,
	[CONSTTYPE] [varchar](10) NULL,
	[DESPRESSURE] [varchar](50) NULL,
	[TESTPRESSURE] [varchar](8) NULL,
	[PWHT] [varchar](20) NULL,
	[TESTMEDIA] [varchar](20) NULL,
	[MATLTYPE] [varchar](20) NULL,
	[NDT] [varchar](10) NULL,
	[NDE] [varchar](30) NULL,
	[PIPECLASS] [varchar](20) NULL,
	[PIDNUMBER] [varchar](50) NULL,
	[DESTEMPERATURE] [varchar](50) NULL,
	[PAINTSYSTEM] [varchar](12) NULL,
	[DESIGNCODE] [varchar](10) NULL,
	[COLOURCODE] [varchar](25) NULL,
	[EWP] [varchar](26) NULL,
	[USER1] [varchar](50) NULL,
	[TAGSTATUS] [varchar](16) NULL,
	[FULLLINE] [varchar](140) NULL,
	[UOM_NOMDIAMETER] [varchar](20) NULL,
	[UOM_DESPRESSURE] [varchar](20) NULL,
	[UOM_DESTEMPERATURE] [varchar](20) NULL,
 CONSTRAINT [PK_LINES] PRIMARY KEY CLUSTERED 
(
	[TAG] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

