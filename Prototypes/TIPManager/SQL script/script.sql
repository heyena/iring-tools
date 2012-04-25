USE [TIP]
GO
/****** Object:  Table [dbo].[T_TemplateMap]    Script Date: 04/25/2012 16:31:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[T_TemplateMap](
	[TMID] [int] NOT NULL,
	[Name] [nvarchar](max) NULL,
	[Identifier] [nvarchar](max) NULL,
	[TemplateType] [nvarchar](max) NULL,
	[RMID] [int] NULL,
 CONSTRAINT [PK_T_TemplateMap] PRIMARY KEY CLUSTERED 
(
	[TMID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[T_RoleMap]    Script Date: 04/25/2012 16:31:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[T_RoleMap](
	[RMID] [nchar](10) NULL,
	[TRMID] [nchar](10) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[T_IdentifierMap]    Script Date: 04/25/2012 16:31:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[T_IdentifierMap](
	[IMID] [int] NOT NULL,
	[IdentifierValue] [nvarchar](max) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[T_GraphMap]    Script Date: 04/25/2012 16:31:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[T_GraphMap](
	[GMID] [int] NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[DataObjectName] [nvarchar](max) NOT NULL,
	[CTID] [int] NOT NULL,
 CONSTRAINT [PK_T_GraphMap] PRIMARY KEY CLUSTERED 
(
	[GMID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[T_ClassMap]    Script Date: 04/25/2012 16:31:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[T_ClassMap](
	[CMID] [int] NOT NULL,
	[Name] [nchar](10) NULL,
	[Identifier] [nchar](10) NULL,
	[IdDelimeter] [nchar](10) NULL,
	[IdValue] [nchar](10) NULL,
	[IMID] [int] NULL,
 CONSTRAINT [PK_T_ClassMap] PRIMARY KEY CLUSTERED 
(
	[CMID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[T_ValueMap]    Script Date: 04/25/2012 16:31:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[T_ValueMap](
	[VLID] [int] NOT NULL,
	[InternalValue] [nvarchar](50) NULL,
	[URI/ID] [nvarchar](max) NULL,
	[Label] [nvarchar](max) NULL,
 CONSTRAINT [PK_T_ValueMap] PRIMARY KEY CLUSTERED 
(
	[VLID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[T_TemplateRoleMap]    Script Date: 04/25/2012 16:31:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[T_TemplateRoleMap](
	[TRMID] [int] NOT NULL,
	[Name] [nvarchar](max) NULL,
	[Type] [nvarchar](max) NULL,
	[Identifier] [nvarchar](max) NULL,
	[DataType] [nvarchar](max) NULL,
	[Value] [nvarchar](max) NULL,
	[PropertyName] [nvarchar](max) NULL,
	[VLID] [int] NULL,
	[CMID] [int] NULL,
 CONSTRAINT [PK_T_TemplateRoleMap] PRIMARY KEY CLUSTERED 
(
	[TRMID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[T_ClassTemplateMap]    Script Date: 04/25/2012 16:31:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[T_ClassTemplateMap](
	[CTID] [int] NOT NULL,
	[CMID] [int] NOT NULL,
	[TMID] [int] NOT NULL,
 CONSTRAINT [PK_T_ClassTemplateMap] PRIMARY KEY CLUSTERED 
(
	[CTID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  ForeignKey [FK_CMID]    Script Date: 04/25/2012 16:31:12 ******/
ALTER TABLE [dbo].[T_ClassTemplateMap]  WITH CHECK ADD  CONSTRAINT [FK_CMID] FOREIGN KEY([CMID])
REFERENCES [dbo].[T_ClassMap] ([CMID])
GO
ALTER TABLE [dbo].[T_ClassTemplateMap] CHECK CONSTRAINT [FK_CMID]
GO
/****** Object:  ForeignKey [FK_TMID]    Script Date: 04/25/2012 16:31:12 ******/
ALTER TABLE [dbo].[T_ClassTemplateMap]  WITH CHECK ADD  CONSTRAINT [FK_TMID] FOREIGN KEY([TMID])
REFERENCES [dbo].[T_TemplateMap] ([TMID])
GO
ALTER TABLE [dbo].[T_ClassTemplateMap] CHECK CONSTRAINT [FK_TMID]
GO
/****** Object:  ForeignKey [FK_TCMID]    Script Date: 04/25/2012 16:31:12 ******/
ALTER TABLE [dbo].[T_TemplateRoleMap]  WITH CHECK ADD  CONSTRAINT [FK_TCMID] FOREIGN KEY([CMID])
REFERENCES [dbo].[T_ClassMap] ([CMID])
GO
ALTER TABLE [dbo].[T_TemplateRoleMap] CHECK CONSTRAINT [FK_TCMID]
GO
/****** Object:  ForeignKey [FK_VLID]    Script Date: 04/25/2012 16:31:12 ******/
ALTER TABLE [dbo].[T_TemplateRoleMap]  WITH CHECK ADD  CONSTRAINT [FK_VLID] FOREIGN KEY([VLID])
REFERENCES [dbo].[T_ValueMap] ([VLID])
GO
ALTER TABLE [dbo].[T_TemplateRoleMap] CHECK CONSTRAINT [FK_VLID]
GO
