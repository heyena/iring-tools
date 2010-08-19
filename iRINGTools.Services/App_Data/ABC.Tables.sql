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

CREATE TABLE [dbo].[EQUIPMENT](
	[TAG] [varchar](126) NOT NULL,
	[INTERNAL_TAG] [varchar](100) NULL,
	[ID] [varchar](10) NULL,
	[AREA] [varchar](10) NULL,
	[TRAINNUMBER] [varchar](5) NULL,
	[EQTYPE] [varchar](50) NULL,
	[EQPPREFIX] [varchar](20) NULL,
	[EQSEQNO] [varchar](62) NULL,
	[EQPSUFF] [varchar](10) NULL,
	[EQUIPDESC1] [varchar](200) NULL,
	[EQUIPDESC2] [varchar](120) NULL,
	[CONSTTYPE] [varchar](25) NULL,
	[EWP] [varchar](26) NULL,
	[USER1] [varchar](50) NULL,
	[USER2] [varchar](50) NULL,
	[USER3] [varchar](50) NULL,
	[TAGSTATUS] [varchar](16) NULL,
	[COMMODITY] [varchar](50) NULL,
 CONSTRAINT [PK_EQUIPMENT] PRIMARY KEY CLUSTERED 
(
	[TAG] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[INSTRUMENTS](
	[KEYTAG] [varchar](10) NULL,
	[TAG] [varchar](100) NOT NULL,
	[TAG_NO] [varchar](100) NULL,
	[TAG_CODE] [varchar](16) NULL,
	[ASSOC_EQ] [varchar](30) NULL,
	[IAREA] [varchar](10) NULL,
	[ITRAIN] [varchar](5) NULL,
	[ITYP] [varchar](10) NULL,
	[INUM] [varchar](10) NULL,
	[ISUFFIX] [varchar](10) NULL,
	[MODIFIER1] [varchar](20) NULL,
	[MODIFIER2] [varchar](20) NULL,
	[MODIFIER3] [varchar](20) NULL,
	[MODIFIER4] [varchar](20) NULL,
	[STD_DETAIL] [varchar](50) NULL,
	[DESCRIPT] [varchar](200) NULL,
	[TAG_TYPE] [varchar](16) NULL,
	[CONST_TYPE] [varchar](25) NULL,
	[COMP_ID] [varchar](50) NULL,
	[PROJ_STAT] [varchar](16) NULL,
	[PID_NO] [varchar](20) NULL,
	[LINE_NO] [varchar](20) NULL,
 CONSTRAINT [PK_INSTRUMENTS1] PRIMARY KEY CLUSTERED 
(
	[TAG] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[LINES](
	[TAG] [varchar](100) NOT NULL,
	[ID] [varchar](10) NULL,
	[AREA] [varchar](10) NULL,
	[TRAINNUMBER] [varchar](5) NULL,
	[SPEC] [varchar](10) NULL,
	[SYSTEM] [varchar](20) NULL,
	[LINENO] [varchar](15) NULL,
	[NOMDIAMETER] [varchar](15) NULL,
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

CREATE TABLE [dbo].[VALVES](
	[KEYTAG] [varchar](10) NULL,
	[TAG_NO] [varchar](100) NOT NULL,
	[VAREA] [varchar](10) NULL,
	[VTYP] [varchar](40) NULL,
	[VTRAIN] [varchar](5) NULL,
	[VNUM] [varchar](10) NULL,
	[VSUFFIX] [varchar](2) NULL,
	[TAG_TYPE] [varchar](20) NULL,
	[CONST_TYPE] [varchar](15) NULL,
	[COMP_ID] [varchar](50) NULL,
	[VSIZE] [varchar](10) NULL,
	[UOM_VSIZE] [varchar](2) NULL,
	[VSPEC_TYPE] [varchar](20) NULL,
	[VSPEC_NUM] [varchar](20) NULL,
	[VPRESRATE] [varchar](50) NULL,
	[VCONDITION] [varchar](10) NULL,
	[PID_NO] [varchar](50) NULL,
	[PROJ_STAT] [varchar](16) NULL,
 CONSTRAINT [PK_VALVES] PRIMARY KEY CLUSTERED 
(
	[TAG_NO] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

