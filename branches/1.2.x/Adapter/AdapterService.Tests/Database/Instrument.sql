USE [iring_test]
GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__INSTRUMEN__PRESS__108B795B]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[INSTRUMENT] DROP CONSTRAINT [DF__INSTRUMEN__PRESS__108B795B]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF__INSTRUMEN__TEMP___117F9D94]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[INSTRUMENT] DROP CONSTRAINT [DF__INSTRUMEN__TEMP___117F9D94]
END

GO

USE [iring_test]
GO

/****** Object:  Table [dbo].[INSTRUMENT]    Script Date: 03/22/2010 16:35:50 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[INSTRUMENT]') AND type in (N'U'))
DROP TABLE [dbo].[INSTRUMENT]
GO

USE [iring_test]
GO

/****** Object:  Table [dbo].[INSTRUMENT]    Script Date: 03/22/2010 16:35:50 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[INSTRUMENT](
	[TAGNO] [varchar](30) NOT NULL,
	[STATUS] [varchar](20) NULL,
	[PLANT] [varchar](100) NULL,
	[PLANT_AREA] [varchar](100) NULL,
	[SYSTEM] [varchar](20) NULL,
	[DESCRIPTION] [varchar](100) NULL,
	[PID] [varchar](30) NULL,
	[PID_REV] [varchar](3) NULL,
	[INST_TYPE] [varchar](50) NULL,
	[IOTYPE] [varchar](10) NULL,
	[MANUFACTURER] [varchar](100) NULL,
	[MODEL_NO] [varchar](100) NULL,
	[OP_PRESS] [decimal](6, 2) NULL,
	[DGN_PRESS] [decimal](6, 2) NULL,
	[PRESS_UOM] [varchar](16) NULL,
	[OP_TEMP] [decimal](6, 2) NULL,
	[DGN_TEMP] [decimal](6, 2) NULL,
	[TEMP_UOM] [varchar](16) NULL,
	[CONTROL_SETPOINT] [varchar](20) NULL,
	[LOWER_RANGE_VALUE] [varchar](20) NULL,
	[UPPER_RANGE_VALUE] [varchar](20) NULL,
	[SCALE_UNITS] [varchar](20) NULL,
	[INPUT_SIGNAL] [varchar](50) NULL,
	[OUTPUT_SIGNAL] [varchar](50) NULL,
	[MIN_MEAS_SPAN] [varchar](20) NULL,
	[MAX_MEAS_SPAN] [varchar](20) NULL,
	[MEAS_SPAN_UOM] [varchar](20) NULL,
	[INST_LOC] [varchar](30) NULL,
	[FAILMODE] [varchar](20) NULL,
	[DATA_SHEET] [varchar](30) NULL,
	[ELEC_SCHEM] [varchar](30) NULL,
	[INSTALL_DTL] [varchar](30) NULL,
	[ISO_DWG] [varchar](30) NULL,
	[LEVEL_DIAG] [varchar](30) NULL,
	[LOC_DWG] [varchar](30) NULL,
	[LOGIC_DIAG] [varchar](30) NULL,
	[LOOP_DIAG] [varchar](30) NULL,
	[MR] [varchar](30) NULL,
	[VENDOR_DWG] [varchar](30) NULL,
	[REMARKS] [varchar](50) NULL,
	[DELETED] [varchar](1) NULL,
	[PREPARED_BY] [varchar](30) NULL,
	[DESIGN_STATE] [varchar](20) NULL,
	[UPDATED_BY] [varchar](30) NULL,
	[TIME_STAMP] [datetime] NULL,
	[IMPORT_SRC] [varchar](30) NULL,
	[CUSER] [varchar](30) NULL,
	CONSTRAINT "INSTRUMENT_PK" PRIMARY KEY ("TAGNO")
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[INSTRUMENT] ADD  DEFAULT ('bar') FOR [PRESS_UOM]
GO

ALTER TABLE [dbo].[INSTRUMENT] ADD  DEFAULT ('degC') FOR [TEMP_UOM]
GO

