USE [Camelot_Test]
GO
/****** Object:  Table [dbo].[VacuumTower]    Script Date: 08/14/2009 15:56:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[VacuumTower](
	[tag] [varchar](255) NOT NULL,
	[description] [varchar](255) NULL,
 CONSTRAINT [PK_VacuumTower] PRIMARY KEY CLUSTERED 
(
	[tag] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
INSERT [dbo].[VacuumTower] ([tag], [description]) VALUES (N'1-AB-VT-001', N'Vacuum Vessel')
INSERT [dbo].[VacuumTower] ([tag], [description]) VALUES (N'1-AB-VT-002', N'Vacuum Vessel')
INSERT [dbo].[VacuumTower] ([tag], [description]) VALUES (N'1-AB-VT-003', N'Vacuum Vessel')
INSERT [dbo].[VacuumTower] ([tag], [description]) VALUES (N'1-AB-VT-004', N'Vacuum Vessel')
