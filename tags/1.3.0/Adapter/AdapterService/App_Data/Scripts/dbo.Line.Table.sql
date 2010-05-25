USE [Camelot_Test]
GO
/****** Object:  Table [dbo].[Line]    Script Date: 08/14/2009 15:56:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Line](
	[tag] [varchar](255) NOT NULL,
	[diameter] [float] NULL,
	[uomDiameter] [varchar](255) NULL,
	[system] [varchar](50) NULL,
 CONSTRAINT [PK_Line] PRIMARY KEY CLUSTERED 
(
	[tag] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
INSERT [dbo].[Line] ([tag], [diameter], [uomDiameter], [system]) VALUES (N'1-AB-L-001', 1, N'INCH', N'AB')
INSERT [dbo].[Line] ([tag], [diameter], [uomDiameter], [system]) VALUES (N'1-AB-L-002', 2, N'INCH', N'AB')
INSERT [dbo].[Line] ([tag], [diameter], [uomDiameter], [system]) VALUES (N'1-AB-L-003', 3, N'INCH', N'AB')
INSERT [dbo].[Line] ([tag], [diameter], [uomDiameter], [system]) VALUES (N'1-AB-L-004', 4, N'INCH', N'AB')
