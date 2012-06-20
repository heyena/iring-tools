USE [PW_iRing_Staging]
GO
/****** Object:  Table [dbo].[PipingComponents]    Script Date: 06/07/2012 19:04:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PipingComponents](
	[SP_ID] [nvarchar](255) NULL,
	[TagSequenceNo] [nvarchar](255) NULL,
	[B_TagSuffix] [nvarchar](255) NULL,
	[PipingCompTypeID] [int] NULL,
	[PipingCompClassID] [int] NULL,
	[PipingCompSubclassID] [int] NULL,
	[IsInline] [int] NULL,
	[StressReliefReqmt] [int] NULL,
	[ItemTag] [nvarchar](255) NULL,
	[SupplyByID] [int] NULL,
	[SP_PlantGroupID] [nvarchar](255) NULL,
	[SP_PartOfID] [nvarchar](255) NULL,
	[aabbcc_code] [nvarchar](255) NULL,
	[ConstructionStatusID] [int] NULL,
	[PlantItemName] [nvarchar](255) NULL,
	[NominalDiameterID] [int] NULL,
	[CoatingRequirementsID] [int] NULL,
	[PipeRunID] [nvarchar](255) NULL,
	[PidUnitName] [nvarchar](255) NULL,
	[PidUnitDescription] [nvarchar](255) NULL,
	[InStockPile] [int] NULL,
	[Drawingint] [nvarchar](255) NULL,
	[DrawingName] [nvarchar](255) NULL,
	[Title] [nvarchar](max) NULL,
	[DrawingDescription] [nvarchar](255) NULL,
	[ModelItemDescription] [nvarchar](255) NULL,
	[SupplyBy] [nvarchar](255) NULL,
	[ConstructionStatus] [nvarchar](255) NULL,
	[NominalDiameter] [nvarchar](255) NULL,
	[OperatingFluid] [nvarchar](255) NULL,
	[PipingCompType] [nvarchar](255) NULL,
	[PipingCompClass] [nvarchar](255) NULL,
	[PipingCompSubclass] [nvarchar](255) NULL,
	[CoatingRequirements] [nvarchar](255) NULL
) ON [PRIMARY]
GO