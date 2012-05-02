
USE [MASTER]
GO

IF EXISTS(SELECT name FROM sys.databases WHERE name = 'TIPS')
	DROP DATABASE [TIPS]
GO

CREATE DATABASE [TIPS] 
GO

USE [TIPS]
GO

CREATE TABLE [dbo].[ValueList](
	[VLID] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
  PRIMARY KEY ([VLID])
)
GO

CREATE TABLE [dbo].[ValueMap](
	[VMID] [int] NOT NULL,
	[URI] [nvarchar](255) NOT NULL,
	[Value] [nvarchar](max) NOT NULL,
	[VLID] [int] NOT NULL,
  PRIMARY KEY ([VMID]),
  FOREIGN KEY ([VLID]) REFERENCES [dbo].[ValueList](VLID)
)
GO

CREATE TABLE [dbo].[GraphMap](
	[GMID] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[DataObjectName] [nvarchar](50) NOT NULL,
	[TypicalValues] [nvarchar](50) NOT NULL,
  PRIMARY KEY ([GMID])
)
GO

CREATE TABLE [dbo].[ClassMap](
	[CMID] [int] NOT NULL,
	[RID] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Identifier] [nvarchar](50) NOT NULL,
	[Delimiter] [nvarchar](10) NULL,
	[Value] [nvarchar](255) NULL,
	[GMID] [int] NOT NULL,
  PRIMARY KEY ([CMID]),
  FOREIGN KEY ([GMID]) REFERENCES [dbo].[GraphMap](GMID)
)
GO

CREATE TABLE [dbo].[TemplateMap](
	[TMID] [int] NOT NULL,
	[RID] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Type] [nvarchar](50) NOT NULL,
	[CMID] [int] NOT NULL,
  PRIMARY KEY ([TMID]),
  FOREIGN KEY ([CMID]) REFERENCES [dbo].[ClassMap](CMID)
)
GO

CREATE TABLE [dbo].[RoleMap](
	[RMID] [int] NOT NULL,
	[RID] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Type] [nvarchar](50) NOT NULL,
	[Property] [nvarchar](50) NULL,
	[DataType] [nvarchar](50) NULL,
	[DataLength] [int] NULL,
	[NumOfDecimals] [int] NULL,
	[Value] [nvarchar](max) NULL,
	[TMID] [int] NOT NULL,
	[VLID] [int] NULL,
	[CMID] [int] NULL,
  PRIMARY KEY ([RMID]),
  FOREIGN KEY ([TMID]) REFERENCES [dbo].[TemplateMap](TMID),
  FOREIGN KEY ([VLID]) REFERENCES [dbo].[ValueList](VLID)
)
GO

CREATE TABLE [dbo].[CommodityList](
	[CMID] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
PRIMARY KEY ([CMID])
)
GO
CREATE TABLE [dbo].[CommodityMap](
	[CMPID] [int] NOT NULL,
	[GMID] [int] NOT NULL,
	[CMID] [int] NOT NULL,
	FOREIGN KEY ([CMID]) REFERENCES [dbo].[CommodityList](CMID),
	FOREIGN KEY ([GMID]) REFERENCES [dbo].[GraphMap](GMID),
  PRIMARY KEY ([GMID])
)
GO