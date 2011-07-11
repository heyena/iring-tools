USE [master]
GO

IF EXISTS(SELECT name FROM sys.databases WHERE name = 'SPPID')
	DROP DATABASE [SPPID]
GO

CREATE DATABASE [SPPID] 
GO

IF EXISTS(SELECT * FROM sys.syslogins WHERE name = N'SPPID')
	DROP LOGIN [SPPID]
GO

CREATE LOGIN [SPPID] WITH PASSWORD = 'sppid', CHECK_POLICY = OFF
GO

USE [SPPID]
GO

IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'SPPID') 
	DROP USER [SPPID]
GO

CREATE USER [SPPID] FOR LOGIN [abc] WITH DEFAULT_SCHEMA=[dbo]
GO

EXEC sp_addrolemember 'db_owner', N'SPPID'
GO

IF EXISTS (SELECT * FROM sys.all_objects WHERE name = N'tblEquipment')
	DROP TABLE [dbo].[tblEquipment]
GO

USE [SPPID]
GO

/****** Object:  Table [dbo].[tblEquipment]    Script Date: 07/11/2011 18:29:56 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[tblEquipment](
	[SP_ID] [nvarchar](100) NULL,
	[Adapter_ParentTag] [nvarchar](100) NULL,
	[Drawing_DateCreated] [datetime] NULL,
	[Drawing_Description] [varchar](240) NULL,
	[Drawing_DocumentCategory] [varchar](1000) NULL,
	[Drawing_DocumentType] [varchar](1000) NULL,
	[Drawing_DrawingNumber] [nvarchar](100) NULL,
	[Drawing_ItemStatus] [nvarchar](100) NULL,
	[Drawing_Name] [nvarchar](100) NULL,
	[Drawing_Path] [nvarchar](100) NULL,
	[Drawing_Revision] [nvarchar](100) NULL,
	[Drawing_Template] [nvarchar](100) NULL,
	[Drawing_Title] [nvarchar](100) NULL,
	[Drawing_Version] [nvarchar](100) NULL,
	[Representation_InStockpile] [nvarchar](100) NULL,
	[Symbol_FileName] [nvarchar](100) NULL,
	[Symbol_XCoordinate] [nvarchar](100) NULL,
	[Symbol_YCoordinate] [nvarchar](100) NULL,
	[EquipmentOther_AbsorbedDuty] [nvarchar](100) NULL,
	[EquipmentOther_Area] [nvarchar](100) NULL,
	[EquipmentOther_EquipmentOrientation] [nvarchar](100) NULL,
	[EquipmentOther_RatedDuty] [nvarchar](100) NULL,
	[EquipmentOther_Revision_ApprovalTimestamp] [datetime] NULL,
	[EquipmentOther_Revision_ApprovedBy] [nvarchar](100) NULL,
	[EquipmentOther_Revision_Responsibility] [nvarchar](100) NULL,
	[EquipmentOther_Revision_RevisionNumber] [nvarchar](100) NULL,
	[EquipmentOther_Revision_StatusTimestamp] [datetime] NULL,
	[EquipmentOther_Revision_StatusType] [nvarchar](100) NULL,
	[EquipmentOther_Revision_StatusValue] [nvarchar](100) NULL,
	[EquipmentOther_Revision_Text] [nvarchar](100) NULL,
	[Exchanger_AbsorbedDuty] [nvarchar](100) NULL,
	[Exchanger_AbsorbedDutyUOM] [nvarchar](100) NULL,
	[Exchanger_CleaningReqmtsTube] [nvarchar](100) NULL,
	[Exchanger_CoatingReqmtsTube] [nvarchar](100) NULL,
	[Exchanger_CorrosionAllowanceTube] [nvarchar](100) NULL,
	[Exchanger_DraftType] [nvarchar](100) NULL,
	[Exchanger_EquipmentOrientation] [nvarchar](100) NULL,
	[Exchanger_HeatTransferAreaPerUnit] [nvarchar](100) NULL,
	[Exchanger_HeatTransferRating] [nvarchar](100) NULL,
	[Exchanger_InsulationSpecTube] [nvarchar](100) NULL,
	[Exchanger_MaterialOfConstClassTube] [nvarchar](100) NULL,
	[Exchanger_MotorPowerPerFan] [nvarchar](100) NULL,
	[Exchanger_NumberofBays] [nvarchar](100) NULL,
	[Exchanger_NumberofBundles] [nvarchar](100) NULL,
	[Exchanger_NumberofFans] [nvarchar](100) NULL,
	[Exchanger_NumberofTubes] [nvarchar](100) NULL,
	[Exchanger_PipingMaterialsClassTube] [nvarchar](100) NULL,
	[Exchanger_PowerAbsorbedPerFan] [nvarchar](100) NULL,
	[Exchanger_RatedDuty] [nvarchar](100) NULL,
	[Exchanger_RatedDutyUOM] [nvarchar](100) NULL,
	[Exchanger_Revision_ApprovalTimestamp] [datetime] NULL,
	[Exchanger_Revision_ApprovedBy] [nvarchar](100) NULL,
	[Exchanger_Revision_Responsibility] [nvarchar](100) NULL,
	[Exchanger_Revision_RevisionNumber] [nvarchar](100) NULL,
	[Exchanger_Revision_StatusTimestamp] [nvarchar](100) NULL,
	[Exchanger_Revision_StatusType] [nvarchar](100) NULL,
	[Exchanger_Revision_StatusValue] [nvarchar](100) NULL,
	[Exchanger_Revision_Text] [nvarchar](100) NULL,
	[Exchanger_ShellDiameter] [nvarchar](100) NULL,
	[Exchanger_ShellDiameterUOM] [nvarchar](100) NULL,
	[Exchanger_TEMA_Designation] [nvarchar](100) NULL,
	[Exchanger_TubeLength] [nvarchar](100) NULL,
	[Exchanger_TypeOfLouvers] [nvarchar](100) NULL,
	[Exchanger_UnitWidth] [nvarchar](100) NULL,
	[Mechanical_CWPipingPlan] [nvarchar](100) NULL,
	[Mechanical_DifferentialPressure] [nvarchar](100) NULL,
	[Mechanical_DifferentialPressureUOM] [nvarchar](100) NULL,
	[Mechanical_DriverRatedPower] [nvarchar](100) NULL,
	[Mechanical_DriverRatedPowerUOM] [nvarchar](100) NULL,
	[Mechanical_ElectricalReqmt] [nvarchar](100) NULL,
	[Mechanical_MaterialOfConstClassInternal] [nvarchar](100) NULL,
	[Mechanical_MechRating] [nvarchar](100) NULL,
	[Mechanical_PowerAbsorbed] [nvarchar](100) NULL,
	[Mechanical_PowerConsumption] [nvarchar](100) NULL,
	[Mechanical_RatedCapacity] [nvarchar](100) NULL,
	[Mechanical_RatedCapacityUOM] [nvarchar](100) NULL,
	[Mechanical_RatedDischargePressure] [nvarchar](100) NULL,
	[Mechanical_RatedSuctionPressure] [nvarchar](100) NULL,
	[Mechanical_Revision_ApprovalTimestamp] [nvarchar](100) NULL,
	[Mechanical_Revision_ApprovedBy] [nvarchar](100) NULL,
	[Mechanical_Revision_Responsibility] [nvarchar](100) NULL,
	[Mechanical_Revision_RevisionNumber] [nvarchar](100) NULL,
	[Mechanical_Revision_StatusTimestamp] [datetime] NULL,
	[Mechanical_Revision_StatusType] [nvarchar](100) NULL,
	[Mechanical_Revision_StatusValue] [nvarchar](100) NULL,
	[Mechanical_Revision_Text] [nvarchar](100) NULL,
	[Mechanical_SealPipingPlan] [nvarchar](100) NULL,
	[Mechanical_TypeOfDriver] [nvarchar](100) NULL,
	[Vessel_DiameterInternal] [nvarchar](100) NULL,
	[Vessel_EquipmentOrientation] [nvarchar](100) NULL,
	[Vessel_LengthTanToTan] [nvarchar](100) NULL,
	[Vessel_LengthTanToTanUOM] [nvarchar](100) NULL,
	[Vessel_LevelReference] [nvarchar](100) NULL,
	[Vessel_LevelReferenceUOM] [nvarchar](100) NULL,
	[Vessel_LiquidLevelHigh] [nvarchar](100) NULL,
	[Vessel_LiquidLevelHighUOM] [nvarchar](100) NULL,
	[Vessel_LiquidLevelHighHigh] [nvarchar](100) NULL,
	[Vessel_LiquidLevelHighHighUOM] [nvarchar](100) NULL,
	[Vessel_LiquidLevelLow] [nvarchar](100) NULL,
	[Vessel_LiquidLevelLowUOM] [nvarchar](100) NULL,
	[Vessel_LiquidLevelLowLow] [nvarchar](100) NULL,
	[Vessel_LiquidLevelLowLowUOM] [nvarchar](100) NULL,
	[Vessel_LiquidLevelNormal] [nvarchar](100) NULL,
	[Vessel_LiquidLevelNormalUOM] [nvarchar](100) NULL,
	[Vessel_LiquidLevelOverflow] [nvarchar](100) NULL,
	[Vessel_LiquidLevelOverflowUOM] [nvarchar](100) NULL,
	[Vessel_MaterialOfConstClassInternal] [nvarchar](100) NULL,
	[Vessel_Revision_ApprovalTimestamp] [datetime] NULL,
	[Vessel_Revision_ApprovedBy] [nvarchar](100) NULL,
	[Vessel_Revision_Responsibility] [nvarchar](100) NULL,
	[Vessel_Revision_RevisionNumber] [nvarchar](100) NULL,
	[Vessel_Revision_StatusTimestamp] [datetime] NULL,
	[Vessel_Revision_StatusType] [nvarchar](100) NULL,
	[Vessel_Revision_StatusValue] [nvarchar](100) NULL,
	[Vessel_Revision_Text] [nvarchar](100) NULL,
	[Vessel_VolumeRating] [nvarchar](100) NULL,
	[Vessel_VolumeRatingUOM] [nvarchar](100) NULL,
	[Class] [nvarchar](100) NULL,
	[CleaningReqmts] [nvarchar](100) NULL,
	[CoatingReqmts] [nvarchar](100) NULL,
	[ConstructionBy] [nvarchar](100) NULL,
	[ConstructionStatus] [nvarchar](100) NULL,
	[CorrosionAllowance] [nvarchar](100) NULL,
	[Description] [nvarchar](100) NULL,
	[DesignBy] [nvarchar](100) NULL,
	[ERPAssetNo] [nvarchar](100) NULL,
	[EquipmentSubclass] [nvarchar](100) NULL,
	[EquipmentType] [nvarchar](100) NULL,
	[FabricationCategory] [nvarchar](100) NULL,
	[HTraceMedium] [nvarchar](100) NULL,
	[HTraceMediumTemp] [nvarchar](100) NULL,
	[HTraceReqmt] [nvarchar](100) NULL,
	[Height] [nvarchar](100) NULL,
	[HeightUOM] [nvarchar](100) NULL,
	[HoldStatus_ApprovalTimestamp] [datetime] NULL,
	[HoldStatus_ApprovedBy] [nvarchar](100) NULL,
	[HoldStatus_Responsibility] [nvarchar](100) NULL,
	[HoldStatus_RevisionNumber] [nvarchar](100) NULL,
	[HoldStatus_StatusTimestamp] [nvarchar](100) NULL,
	[HoldStatus_StatusType] [nvarchar](100) NULL,
	[HoldStatus_StatusValue] [nvarchar](100) NULL,
	[HoldStatus_Text] [nvarchar](100) NULL,
	[HoldStatus_UpdateCount] [nvarchar](100) NULL,
	[InsulDensity] [nvarchar](100) NULL,
	[InsulPurpose] [nvarchar](100) NULL,
	[InsulTemp] [nvarchar](100) NULL,
	[InsulThick] [nvarchar](100) NULL,
	[InsulThickUOM] [nvarchar](100) NULL,
	[InsulType] [nvarchar](100) NULL,
	[InsulationSpec] [nvarchar](100) NULL,
	[InsulationThkSource] [nvarchar](100) NULL,
	[InventoryTag] [nvarchar](100) NULL,
	[IsBulkItem] [nvarchar](100) NULL,
	[IsUnchecked] [nvarchar](100) NULL,
	[ItemStatus] [nvarchar](100) NULL,
	[ItemTag] [nvarchar](100) NULL,
	[ItemTypeName] [nvarchar](100) NULL,
	[MaterialOfConstClass] [nvarchar](100) NULL,
	[ModelItemType] [nvarchar](100) NULL,
	[Name] [nvarchar](100) NULL,
	[PartOfType] [nvarchar](100) NULL,
	[PipingMaterialsClass] [nvarchar](100) NULL,
	[PlantItemType] [nvarchar](100) NULL,
	[RequisitionBy] [nvarchar](100) NULL,
	[RequisitionNo] [nvarchar](100) NULL,
	[Slope] [nvarchar](100) NULL,
	[SlopeRise] [nvarchar](100) NULL,
	[SlopeRun] [nvarchar](100) NULL,
	[SteamOutPressure] [nvarchar](100) NULL,
	[SteamOutReqmt] [nvarchar](100) NULL,
	[SteamOutTemperature] [nvarchar](100) NULL,
	[StressReliefReqmt] [nvarchar](100) NULL,
	[SupplyBy] [nvarchar](100) NULL,
	[TagPrefix] [nvarchar](100) NULL,
	[TagSequenceNo] [nvarchar](100) NULL,
	[TagSuffix] [nvarchar](100) NULL,
	[TagReqdFlag] [nvarchar](100) NULL,
	[TrimSpec] [nvarchar](100) NULL,
	[UpdateCount] [nvarchar](100) NULL,
	[aabbcc_code] [nvarchar](100) NULL
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


