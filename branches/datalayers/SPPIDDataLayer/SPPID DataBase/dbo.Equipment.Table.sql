USE [PW_iRing_Staging]
GO
/****** Object:  Table [dbo].[Equipment]    Script Date: 06/07/2012 19:04:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Equipment](
	[SP_ID] [nvarchar](255) NULL,
	[ItemTag] [nvarchar](255) NULL,
	[SupplyByID] [int] NULL,
	[SP_PlantGroupID] [nvarchar](255) NULL,
	[SP_PartOfID] [nvarchar](255) NULL,
	[aabbcc_code] [nvarchar](255) NULL,
	[ConstructionStatusID] [int] NULL,
	[PlantItemName] [nvarchar](255) NULL,
	[PidUnitName] [nvarchar](255) NULL,
	[PidUnitDescription] [nvarchar](255) NULL,
	[Drawingint] [nvarchar](255) NULL,
	[DrawingName] [nvarchar](255) NULL,
	[Title] [nvarchar](max) NULL,
	[DrawingDescription] [nvarchar](255) NULL,
	[TagPrefix] [nvarchar](255) NULL,
	[TagSequenceNo] [nvarchar](255) NULL,
	[TagSuffix] [nvarchar](255) NULL,
	[EquipmentClassID] [int] NULL,
	[EquipmentSubClassID] [int] NULL,
	[EquipmentTypeID] [int] NULL,
	[CoatingRequirementsID] [int] NULL,
	[SupplyBy] [nvarchar](255) NULL,
	[ConstructionStatus] [nvarchar](255) NULL,
	[EquipmentClass] [nvarchar](255) NULL,
	[EquipmentSubClass] [nvarchar](255) NULL,
	[EquipmentType] [nvarchar](255) NULL,
	[CoatingRequirements] [nvarchar](255) NULL,
	[InStockPile] [int] NULL
) ON [PRIMARY]
GO