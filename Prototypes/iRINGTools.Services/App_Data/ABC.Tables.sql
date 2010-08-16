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

CREATE TABLE [dbo].[Instruments](
	[Tag] [varchar](50) NOT NULL,
	[PlantArea] [varchar](50) NULL,
	[System] [varchar](50) NULL,
	[PID] [varchar](50) NULL,
	[PIDRev] [varchar](50) NULL,
	[Plant] [varchar](50) NULL,
	[Manufacturer] [varchar](50) NULL,
	[PressUOM] [varchar](50) NULL,
	[DesignPressure] [float] NULL,
	[OperatingPressure] [float] NULL,
 CONSTRAINT [PK_Instruments] PRIMARY KEY CLUSTERED 
(
	[Tag] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[Lines](
	[Tag] [varchar](50) NOT NULL,
	[PlantArea] [varchar](50) NULL,
	[System] [varchar](50) NULL,
	[PID] [varchar](50) NULL,
	[PIDRev] [varchar](50) NULL,
	[Plant] [varchar](50) NULL,
	[LengthUOM] [varchar](50) NULL,
	[Length] [float] NULL,
	[DiameterUOM] [varchar](50) NULL,
	[Diameter] [float] NULL,
	[Fluid] [varchar](50) NULL,
	[TempUOM] [varchar](50) NULL,
	[DesignTemperature] [float] NULL,
	[OperatingTemperature] [float] NULL,
 CONSTRAINT [PK_Lines] PRIMARY KEY CLUSTERED 
(
	[Tag] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

