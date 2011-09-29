USE [master]
GO

IF EXISTS(SELECT name FROM sys.databases WHERE name = 'sppid')
	DROP DATABASE [SPPID]
GO

CREATE DATABASE [SPPID] 
GO

IF EXISTS(SELECT * FROM sys.syslogins WHERE name = N'sppid')
	DROP LOGIN [sppid]
GO

CREATE LOGIN [sppid] WITH PASSWORD = 'sppid', CHECK_POLICY = OFF
GO

USE [SPPID]
GO

IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'sppid') 
	DROP USER [sppid]
GO

CREATE USER [sppid] FOR LOGIN [sppid] WITH DEFAULT_SCHEMA=[dbo]
GO

EXEC sp_addrolemember 'db_owner', N'sppid'
GO

CREATE TABLE [dbo].[Equipment](
	[ItemTag] [nvarchar](80) NULL,
	[SupplyBy] [nvarchar](100) NULL,
	[SP_ID] [nvarchar](32) NOT NULL,
	[SP_PlantGroupID] [nvarchar](32) NULL,
	[SP_PartOfID] [nvarchar](32) NULL,
	[aabbcc_code] [nvarchar](40) NULL,
	[ConstructionStatus] [nvarchar](100) NULL,
	[PlantItemName] [nvarchar](80) NULL,
	[PidUnitName] [nvarchar](80) NULL,
	[PidUnitDescription] [nvarchar](240) NULL,
	[DrawingNumber] [nvarchar](80) NULL,
	[DrawingName] [nvarchar](80) NULL,
	[Title] [nvarchar](80) NULL,
	[DrawingDescription] [nvarchar](240) NULL,
	[TagPrefix] [nvarchar](40) NULL,
	[TagSequenceNo] [nvarchar](40) NULL,
	[TagSuffix] [nvarchar](40) NULL,
	[Class] [int] NULL,
	[EquipmentSubclass] [int] NULL,
	[EquipmentType] [int] NULL,
	[CoatingRequirements] [int] NULL,
	[EnumerationName] [nvarchar](255) NULL,
	[EnumerationDescription] [nvarchar](255) NULL,
	[InStockpile] [int] NULL,
	[PidUnitCode] [nvarchar](40) NULL,
 CONSTRAINT [PK_Equipment] PRIMARY KEY CLUSTERED 
(
	[SP_ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO





