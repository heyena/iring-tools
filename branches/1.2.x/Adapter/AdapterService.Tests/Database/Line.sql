USE [iring_test]
GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__LINE__DIAMETER_U__7C8480AE]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[LINE] DROP CONSTRAINT [DF__LINE__DIAMETER_U__7C8480AE]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__LINE__PRESS_UOM__7D78A4E7]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[LINE] DROP CONSTRAINT [DF__LINE__PRESS_UOM__7D78A4E7]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__LINE__TEMP_UOM__7E6CC920]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[LINE] DROP CONSTRAINT [DF__LINE__TEMP_UOM__7E6CC920]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__LINE__LENGTH_UOM__7F60ED59]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[LINE] DROP CONSTRAINT [DF__LINE__LENGTH_UOM__7F60ED59]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__LINE__INSULATION__00551192]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[LINE] DROP CONSTRAINT [DF__LINE__INSULATION__00551192]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__LINE__INSULATION__014935CB]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[LINE] DROP CONSTRAINT [DF__LINE__INSULATION__014935CB]
END

GO

USE [iring_test]
GO

/****** Object:  Table [dbo].[LINE]    Script Date: 03/22/2010 16:36:11 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LINE]') AND type in (N'U'))
DROP TABLE [dbo].[LINE]
GO

USE [iring_test]
GO

/****** Object:  Table [dbo].[LINE]    Script Date: 03/22/2010 16:36:11 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[LINE](
	[TAGNO] [varchar](30) NOT NULL,
	[STATUS] [varchar](20) NULL,
	[PLANT] [varchar](100) NULL,
	[PLANT_AREA] [varchar](100) NULL,
	[SYSTEM] [varchar](20) NULL,
	[DESCRIPTION] [varchar](100) NULL,
	[PID] [varchar](30) NULL,
	[PID_REV] [varchar](3) NULL,
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
	[LENGTH] [decimal](6, 2) NULL,
	[LENGTH_UOM] [varchar](16) NULL,
	[INSULATION_YN] [varchar](1) NULL,
	[INSULATION_THICKNESS] [decimal](6, 2) NULL,
	[INSULATION_THICKNESS_UOM] [varchar](16) NULL,
	[VISIBLE_TAG] [varchar](30) NULL,
	[HYDRO_TEST] [varchar](20) NULL,
	[INSTALLATION_DATE] [datetime] NULL,
	[FLOW] [varchar](50) NULL,
	[REMARKS] [varchar](50) NULL,
	[DELETED] [varchar](1) NULL,
	[PREPARED_BY] [varchar](30) NULL,
	[DESIGN_STATE] [varchar](20) NULL,
	[UPDATED_BY] [varchar](30) NULL,
	[TIME_STAMP] [datetime] NULL,
	[IMPORT_SRC] [varchar](30) NULL,
	[CUSER] [varchar](30) NULL,
CONSTRAINT "LINE_PK" PRIMARY KEY ("TAGNO")

)  ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[LINE] ADD  DEFAULT ('mm') FOR [DIAMETER_UOM]
GO

ALTER TABLE [dbo].[LINE] ADD  DEFAULT ('bar') FOR [PRESS_UOM]
GO

ALTER TABLE [dbo].[LINE] ADD  DEFAULT ('degC') FOR [TEMP_UOM]
GO

ALTER TABLE [dbo].[LINE] ADD  DEFAULT ('mm') FOR [LENGTH_UOM]
GO

ALTER TABLE [dbo].[LINE] ADD  DEFAULT ('N') FOR [INSULATION_YN]
GO

ALTER TABLE [dbo].[LINE] ADD  DEFAULT ('mm') FOR [INSULATION_THICKNESS_UOM]
GO

