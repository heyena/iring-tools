USE [Camelot_Test]
GO
/****** Object:  Table [dbo].[InLinePipingComponent]    Script Date: 08/14/2009 15:56:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
DROP TABLE [dbo].[InLinePipingComponent]
GO
CREATE TABLE [dbo].[InLinePipingComponent](
	[tag] [varchar](255) NOT NULL,
	[componentType] [varchar](255) NOT NULL,
	[diameter] [float] NULL,
	[uomDiameter] [varchar](255) NULL,
	[rating] [varchar](255) NULL,
	[system] [varchar](255) NULL,
	[unit] [varchar](255) NULL,
	[projectNumber] [varchar](255) NULL,
	[pid] [varchar](255) NULL,
	[lineTag] [varchar](255) NULL,
	[quantity] [int] NULL,
	[isCloned] [bit] NULL,
 CONSTRAINT [PK_InLinePipingComponent] PRIMARY KEY CLUSTERED 
(
	[tag] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
INSERT [dbo].[InLinePipingComponent] ([tag], [componentType], [diameter], [uomDiameter], [rating], [system], [unit], [projectNumber], [pid], [lineTag], [quantity], [isCloned]) VALUES (N'1-AB-PV-001', N'Valve', 1, N'INCH', N'150 lb', N'AB', N'1', N'24193', N'1-M1-AB-001', N'1-AB-L006', 1, 0)
INSERT [dbo].[InLinePipingComponent] ([tag], [componentType], [diameter], [uomDiameter], [rating], [system], [unit], [projectNumber], [pid], [lineTag], [quantity], [isCloned]) VALUES (N'1-AB-PV-002', N'Valve', 1, N'INCH', N'150 lb', N'AB', N'1', N'24193', N'1-M1-AB-001', N'1-AB-L006', 1, 0)
INSERT [dbo].[InLinePipingComponent] ([tag], [componentType], [diameter], [uomDiameter], [rating], [system], [unit], [projectNumber], [pid], [lineTag], [quantity], [isCloned]) VALUES (N'1-AB-PV-003', N'Valve', 1, N'INCH', N'150 lb', N'AB', N'1', N'24193', N'1-M1-AB-002', N'1-AB-L006', 1, 0)
INSERT [dbo].[InLinePipingComponent] ([tag], [componentType], [diameter], [uomDiameter], [rating], [system], [unit], [projectNumber], [pid], [lineTag], [quantity], [isCloned]) VALUES (N'1-AE-IM-001', N'Instrument', 3, N'INCH', N'50 lb', N'AE', N'1', N'24193', N'1-M2-AE-001', N'1-AE-L003', 1, 0)
INSERT [dbo].[InLinePipingComponent] ([tag], [componentType], [diameter], [uomDiameter], [rating], [system], [unit], [projectNumber], [pid], [lineTag], [quantity], [isCloned]) VALUES (N'1-CE-PV-001', N'Valve', 5, N'INCH', N'300 lb', N'CE', N'1', N'24193', N'1-M3-CE-001', N'1-CE-L007', 1, 0)
INSERT [dbo].[InLinePipingComponent] ([tag], [componentType], [diameter], [uomDiameter], [rating], [system], [unit], [projectNumber], [pid], [lineTag], [quantity], [isCloned]) VALUES (N'1-HG-IM-001', N'Instrument', 8, N'INCH', N'35 lb', N'HG', N'1', N'24193', N'1-M4-HG-001', N'1-HG-L004', 1, 0)
INSERT [dbo].[InLinePipingComponent] ([tag], [componentType], [diameter], [uomDiameter], [rating], [system], [unit], [projectNumber], [pid], [lineTag], [quantity], [isCloned]) VALUES (N'2-AB-PV-001', N'Valve', 1, N'INCH', N'150 lb', N'AB', N'2', N'24193', N'2-M1-AB-001', N'2-AB-L006', 1, 0)
INSERT [dbo].[InLinePipingComponent] ([tag], [componentType], [diameter], [uomDiameter], [rating], [system], [unit], [projectNumber], [pid], [lineTag], [quantity], [isCloned]) VALUES (N'2-AE-IM-001', N'Instrument', 3, N'INCH', N'50 lb', N'AE', N'2', N'24193', N'2-M2-AE-001', N'2-AE-L003', 1, 0)
