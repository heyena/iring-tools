USE [iring_test]
GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__VALVE__DIAMETER___03317E3D]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[VALVE] DROP CONSTRAINT [DF__VALVE__DIAMETER___03317E3D]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__VALVE__PRESS_UOM__0425A276]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[VALVE] DROP CONSTRAINT [DF__VALVE__PRESS_UOM__0425A276]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__VALVE__TEMP_UOM__0519C6AF]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[VALVE] DROP CONSTRAINT [DF__VALVE__TEMP_UOM__0519C6AF]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__VALVE__INSULATIO__060DEAE8]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[VALVE] DROP CONSTRAINT [DF__VALVE__INSULATIO__060DEAE8]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__VALVE__INSULATIO__07020F21]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[VALVE] DROP CONSTRAINT [DF__VALVE__INSULATIO__07020F21]
END

GO

USE [iring_test]
GO

/****** Object:  Table [dbo].[VALVE]    Script Date: 03/22/2010 16:36:20 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[VALVE]') AND type in (N'U'))
DROP TABLE [dbo].[VALVE]
GO

USE [iring_test]
GO

/****** Object:  Table [dbo].[VALVE]    Script Date: 03/22/2010 16:36:20 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[VALVE](
	[TAGNO] [varchar](30) NOT NULL,
	[STATUS] [varchar](20) NULL,
	[PLANT] [varchar](100) NULL,
	[PLANT_AREA] [varchar](100) NULL,
	[SYSTEM] [varchar](20) NULL,
	[DESCRIPTION] [varchar](100) NULL,
	[PID] [varchar](30) NULL,
	[PID_REV] [varchar](3) NULL,
	[END_PREP] [varchar](100) NULL,
	[RATING] [varchar](100) NULL,
	[MANUFACTURER] [varchar](100) NULL,
	[MODEL_NO] [varchar](100) NULL,
	[FLOWRATE] [varchar](50) NULL,
	[DIAMETER] [decimal](6, 2) NULL,
	[DIAMETER_UOM] [varchar](16) NULL,
	[SPEC] [varchar](20) NULL,
	[FLUID] [varchar](20) NULL,
	[OP_PRESS] [decimal](6, 2) NULL,
	[DGN_PRESS] [decimal](6, 2) NULL,
	[PRESS_UOM] [varchar](16) NULL,
	[OP_TEMP] [decimal](6, 2) NULL,
	[DGN_TEMP] [decimal](6, 2) NULL,
	[TEMP_UOM] [varchar](16) NULL,
	[INSULATION_YN] [varchar](1) NULL,
	[INSULATION_THICKNESS] [decimal](6, 2) NULL,
	[INSULATION_THICKNESS_UOM] [varchar](16) NULL,
	[REMARKS] [varchar](50) NULL,
	[DELETED] [varchar](1) NULL,
	[PREPARED_BY] [varchar](30) NULL,
	[DESIGN_STATE] [varchar](20) NULL,
	[UPDATED_BY] [varchar](30) NULL,
	[TIME_STAMP] [datetime] NULL,
	[IMPORT_SRC] [varchar](30) NULL,
	[CUSER] [varchar](30) NULL,
	CONSTRAINT "VALVE_PK" PRIMARY KEY ("TAGNO")
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[VALVE] ADD  DEFAULT ('mm') FOR [DIAMETER_UOM]
GO

ALTER TABLE [dbo].[VALVE] ADD  DEFAULT ('bar') FOR [PRESS_UOM]
GO

ALTER TABLE [dbo].[VALVE] ADD  DEFAULT ('degC') FOR [TEMP_UOM]
GO

ALTER TABLE [dbo].[VALVE] ADD  DEFAULT ('N') FOR [INSULATION_YN]
GO

ALTER TABLE [dbo].[VALVE] ADD  DEFAULT ('mm') FOR [INSULATION_THICKNESS_UOM]
GO

