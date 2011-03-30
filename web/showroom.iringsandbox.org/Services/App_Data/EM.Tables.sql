USE [EM]
GO
/****** Object:  Table [dbo].[VALVE]    Script Date: 03/30/2011 11:50:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[VALVE](
	[DWG_NO] [varchar](24) NULL,
	[ERROR_STATUS] [varchar](64) NULL,
	[SENDER_LOCK] [varchar](1) NULL,
	[SENDER_LOCK_REASON] [varchar](128) NULL,
	[STARTUP] [varchar](10) NULL,
	[RATING] [varchar](20) NULL,
	[PID_REV] [varchar](3) NULL,
	[DES_RESP] [varchar](40) NULL,
	[PID] [varchar](24) NULL,
	[HOLD] [varchar](10) NULL,
	[HEAT_TRACE] [varchar](5) NULL,
	[PROJ_NO] [varchar](5) NULL,
	[LINE_NUMBER] [varchar](10) NULL,
	[MATERIAL_SPEC_LINE] [varchar](6) NULL,
	[LINE_TAG] [varchar](30) NULL,
	[QUANTITY] [numeric](5, 0) NULL,
	[DIAMETER] [float] NULL,
	[DIAMETER_UOM] [varchar](5) NULL,
	[VALVE_LOCK_DEVICE] [varchar](10) NULL,
	[VALVE_NUMBER] [varchar](10) NULL,
	[SEQ_NO] [varchar](10) NULL,
	[CLIENT_SYSTEM] [varchar](5) NULL,
	[WERE_CLONED_FROM] [varchar](3) NULL,
	[WERE_CLONED_TO] [varchar](40) NULL,
	[COM_GRP] [varchar](2) NULL,
	[CIN] [varchar](26) NULL,
	[SYSTEM] [varchar](4) NULL,
	[IS_CLONED] [varchar](1) NULL,
	[PID_LOCATION] [varchar](4) NULL,
	[VALVE_TYPE] [varchar](50) NULL,
	[SEQUENCE] [varchar](24) NULL,
	[SUFFIX] [varchar](6) NULL,
	[UNIT] [varchar](3) NULL,
	[WAS_CLONED] [varchar](1) NULL,
	[TAG] [varchar](20) NOT NULL,
	[CLIENT_UNIT] [varchar](2) NULL,
 CONSTRAINT [PK_VALVE] PRIMARY KEY CLUSTERED 
(
	[TAG] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[LINE]    Script Date: 03/30/2011 11:50:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[LINE](
	[DWG_NO] [varchar](24) NULL,
	[ERROR_STATUS] [varchar](64) NULL,
	[SENDER_LOCK_FLAG] [varchar](1) NULL,
	[SENDER_LOCK_REASON] [varchar](128) NULL,
	[DESIGN] [varchar](20) NULL,
	[CLIENT_SYSTEM] [varchar](10) NULL,
	[RATING] [varchar](20) NULL,
	[REV] [varchar](3) NULL,
	[PID] [varchar](24) NULL,
	[LOCATOR] [varchar](3) NULL,
	[HOLD] [varchar](10) NULL,
	[HEAT_TRACE] [varchar](5) NULL,
	[PROJ_NO] [varchar](5) NULL,
	[DWG_TAG] [varchar](30) NULL,
	[LINE_NUMBER] [varchar](5) NULL,
	[SPEC] [varchar](6) NULL,
	[DIAMETER] [float] NULL,
	[DIAMETER_UOM] [varchar](6) NULL,
	[REV_STATUS] [varchar](40) NULL,
	[SEQ_NO] [varchar](10) NULL,
	[WERE_CLONED_FROM] [varchar](3) NULL,
	[WERE_CLONED_TO] [varchar](40) NULL,
	[COM_GRP] [varchar](2) NULL,
	[CIN] [varchar](26) NULL,
	[SYSTEM] [varchar](3) NULL,
	[IS_CLONE] [varchar](1) NULL,
	[SEQUENCE_CODE] [varchar](24) NULL,
	[UNIT] [varchar](3) NULL,
	[WAS_CLONED] [varchar](1) NULL,
	[TAG] [varchar](20) NOT NULL,
	[CLIENT_UNIT] [varchar](2) NULL,
	[INS_THK] [numeric](38, 0) NULL,
	[PUB_FLAG] [varchar](1) NULL,
	[BSAP] [varchar](6) NULL,
	[ATTACHMENT_OBJECT_TYPE] [varchar](64) NULL,
	[ATTACHMENT_CHILD_ID] [varchar](64) NULL,
	[ATTACHMENT_ORDER] [numeric](12, 0) NULL,
	[ATTACHMENT_RELATION_TYPE] [varchar](64) NULL,
	[ATTACHMENT_SIZE] [numeric](38, 0) NULL,
	[ATTACHMENT_FILE_EXTENSION] [varchar](64) NULL,
	[ATTACHMENT_MIME_TYPE] [varchar](64) NULL,
	[ATTACHMENT_FILE_NAME] [varchar](256) NULL,
	[ATTACHMENT_REVISION_TYPE] [varchar](64) NULL,
	[ATTACHMENT_ROLE_FLAG] [varchar](1) NULL,
	[ATTACHMENT_CHECK_SUM] [varchar](64) NULL,
	[ATTACHMENT_DIFFERENCING_TOKEN] [varchar](64) NULL,
	[ATTACHMENT_LAST_UPDATED] [date] NULL,
	[ATTACHMENT_COMPRESSION_FLAG] [varchar](1) NULL,
	[ATTACHMENT_COMPRESSION_TYPE] [varchar](64) NULL,
 CONSTRAINT [PK_LINE] PRIMARY KEY CLUSTERED 
(
	[TAG] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
