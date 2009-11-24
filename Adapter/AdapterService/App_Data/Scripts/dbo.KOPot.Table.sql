USE [Camelot_Test]
GO
/****** Object:  Table [dbo].[KOPot]    Script Date: 08/14/2009 15:56:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
DROP TABLE [dbo].[KOPot]
GO
CREATE TABLE [dbo].[KOPot](
	[tag] [varchar](255) NOT NULL,
	[description] [varchar](255) NULL,
 CONSTRAINT [PK_KOPot] PRIMARY KEY CLUSTERED 
(
	[tag] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
INSERT [dbo].[KOPot] ([tag], [description]) VALUES (N'1-AB-KO-001', N'Knock Out Vessel')
INSERT [dbo].[KOPot] ([tag], [description]) VALUES (N'1-AB-KO-002', N'Knock Out Vessel')
INSERT [dbo].[KOPot] ([tag], [description]) VALUES (N'1-AB-KO-003', N'Knock Out Vessel')
INSERT [dbo].[KOPot] ([tag], [description]) VALUES (N'1-AB-KO-004', N'Knock Out Vessel')
