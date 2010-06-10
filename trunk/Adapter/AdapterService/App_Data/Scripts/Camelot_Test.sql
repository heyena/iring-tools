USE [master]
GO

IF EXISTS(SELECT name FROM sys.databases WHERE name = 'Camelot_Test')
	DROP DATABASE [Camelot_Test]
GO

CREATE DATABASE [Camelot_Test] 
GO

IF EXISTS(SELECT * FROM sys.syslogins WHERE name = N'camelot')
	DROP LOGIN [camelot]
GO

CREATE LOGIN [camelot] WITH PASSWORD = 'camelot', CHECK_POLICY = OFF
GO

USE [Camelot_Test]
GO

IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'camelot') 
	DROP USER [camelot]
GO

CREATE USER [camelot] FOR LOGIN [camelot] WITH DEFAULT_SCHEMA=[dbo]
GO

EXEC sp_addrolemember 'db_owner', N'camelot'
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
	[OperatingTemperature] [float] NULL,
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

