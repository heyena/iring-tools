USE [master]
GO
/****** Object:  Database [iRINGConfig]    Script Date: 09/15/2014 12:09:24 ******/
CREATE DATABASE [iRINGConfig] ON  PRIMARY 
( NAME = N'iRINGConfig', FILENAME = N'E:\Microsoft SQL Server\MSSQL10_50.MULTAPP\MSSQL\DATA\iRINGConfig.mdf' , SIZE = 3072KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'iRINGConfig_log', FILENAME = N'E:\Microsoft SQL Server\MSSQL10_50.MULTAPP\MSSQL\DATA\iRINGConfig_log.ldf' , SIZE = 1024KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
ALTER DATABASE [iRINGConfig] SET COMPATIBILITY_LEVEL = 100
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [iRINGConfig].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [iRINGConfig] SET ANSI_NULL_DEFAULT OFF
GO
ALTER DATABASE [iRINGConfig] SET ANSI_NULLS OFF
GO
ALTER DATABASE [iRINGConfig] SET ANSI_PADDING OFF
GO
ALTER DATABASE [iRINGConfig] SET ANSI_WARNINGS OFF
GO
ALTER DATABASE [iRINGConfig] SET ARITHABORT OFF
GO
ALTER DATABASE [iRINGConfig] SET AUTO_CLOSE OFF
GO
ALTER DATABASE [iRINGConfig] SET AUTO_CREATE_STATISTICS ON
GO
ALTER DATABASE [iRINGConfig] SET AUTO_SHRINK OFF
GO
ALTER DATABASE [iRINGConfig] SET AUTO_UPDATE_STATISTICS ON
GO
ALTER DATABASE [iRINGConfig] SET CURSOR_CLOSE_ON_COMMIT OFF
GO
ALTER DATABASE [iRINGConfig] SET CURSOR_DEFAULT  GLOBAL
GO
ALTER DATABASE [iRINGConfig] SET CONCAT_NULL_YIELDS_NULL OFF
GO
ALTER DATABASE [iRINGConfig] SET NUMERIC_ROUNDABORT OFF
GO
ALTER DATABASE [iRINGConfig] SET QUOTED_IDENTIFIER OFF
GO
ALTER DATABASE [iRINGConfig] SET RECURSIVE_TRIGGERS OFF
GO
ALTER DATABASE [iRINGConfig] SET  DISABLE_BROKER
GO
ALTER DATABASE [iRINGConfig] SET AUTO_UPDATE_STATISTICS_ASYNC OFF
GO
ALTER DATABASE [iRINGConfig] SET DATE_CORRELATION_OPTIMIZATION OFF
GO
ALTER DATABASE [iRINGConfig] SET TRUSTWORTHY OFF
GO
ALTER DATABASE [iRINGConfig] SET ALLOW_SNAPSHOT_ISOLATION OFF
GO
ALTER DATABASE [iRINGConfig] SET PARAMETERIZATION SIMPLE
GO
ALTER DATABASE [iRINGConfig] SET READ_COMMITTED_SNAPSHOT OFF
GO
ALTER DATABASE [iRINGConfig] SET HONOR_BROKER_PRIORITY OFF
GO
ALTER DATABASE [iRINGConfig] SET  READ_WRITE
GO
ALTER DATABASE [iRINGConfig] SET RECOVERY SIMPLE
GO
ALTER DATABASE [iRINGConfig] SET  MULTI_USER
GO
ALTER DATABASE [iRINGConfig] SET PAGE_VERIFY CHECKSUM
GO
ALTER DATABASE [iRINGConfig] SET DB_CHAINING OFF
GO
USE [iRINGConfig]
GO
/****** Object:  User [iRINGConfig]    Script Date: 09/15/2014 12:09:25 ******/
CREATE USER [iRINGConfig] FOR LOGIN [iRINGConfig] WITH DEFAULT_SCHEMA=[dbo]
GO
/****** Object:  Table [dbo].[DataLayers]    Script Date: 09/15/2014 12:09:35 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DataLayers](
	[Assembly] [nvarchar](650) NOT NULL,
	[Name] [nvarchar](250) NOT NULL,
	[Configurable] [nvarchar](50) NULL,
	[IsLigthWeight] [nvarchar](50) NULL,
	[SiteId] [int] NOT NULL,
	[Active] [tinyint] NOT NULL,
 CONSTRAINT [PK_DataLayers] PRIMARY KEY CLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DataFiltersType]    Script Date: 09/15/2014 12:09:35 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DataFiltersType](
	[DataFilterTypeId] [int] NOT NULL,
	[DataFilterTypeName] [nvarchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[DataFilterTypeId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [dbo].[DataFiltersType] ([DataFilterTypeId], [DataFilterTypeName]) VALUES (1, N'DataObject')
INSERT [dbo].[DataFiltersType] ([DataFilterTypeId], [DataFilterTypeName]) VALUES (2, N'AppData')
INSERT [dbo].[DataFiltersType] ([DataFilterTypeId], [DataFilterTypeName]) VALUES (3, N'ExchangeData')
/****** Object:  Table [dbo].[DataFilters]    Script Date: 09/15/2014 12:09:35 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DataFilters](
	[DataFilterId] [uniqueidentifier] NOT NULL,
	[ResourceId] [uniqueidentifier] NOT NULL,
	[DataFilterTypeId] [int] NOT NULL,
	[IsExpression] [int] NOT NULL,
	[DFOrder] [int] NOT NULL,
	[OpenCount] [int] NULL,
	[LogicalOperator] [nvarchar](50) NULL,
	[PropertyName] [nvarchar](350) NULL,
	[RelationalOperator] [nvarchar](150) NULL,
	[Value] [nvarchar](250) NULL,
	[CloseCount] [int] NULL,
	[Sort] [nvarchar](25) NULL,
	[SiteId] [int] NOT NULL,
	[Active] [tinyint] NOT NULL
) ON [PRIMARY]
GO
INSERT [dbo].[DataFilters] ([DataFilterId], [ResourceId], [DataFilterTypeId], [IsExpression], [DFOrder], [OpenCount], [LogicalOperator], [PropertyName], [RelationalOperator], [Value], [CloseCount], [Sort], [SiteId], [Active]) VALUES (N'd208b9b8-0372-4e2c-9b34-78b9bb455555', N'd208b9b8-0372-4e2c-9b34-78b9bb453b61', 2, 0, 1, 0, NULL, N'xyz.abc.valueIdentifier', NULL, NULL, NULL, N'ASC', 1, 1)
INSERT [dbo].[DataFilters] ([DataFilterId], [ResourceId], [DataFilterTypeId], [IsExpression], [DFOrder], [OpenCount], [LogicalOperator], [PropertyName], [RelationalOperator], [Value], [CloseCount], [Sort], [SiteId], [Active]) VALUES (N'd208b9b8-0372-4e2c-9b34-78b9bb453030', N'd208b9b8-0372-4e2c-9b34-78b9bb453b61', 2, 1, 1, 0, NULL, N'xyz.abc.valuwIdentifier', N'contains', N'05', 0, NULL, 1, 1)
INSERT [dbo].[DataFilters] ([DataFilterId], [ResourceId], [DataFilterTypeId], [IsExpression], [DFOrder], [OpenCount], [LogicalOperator], [PropertyName], [RelationalOperator], [Value], [CloseCount], [Sort], [SiteId], [Active]) VALUES (N'd208b9b8-0372-4e2c-9b34-78b9bb453040', N'd208b9b8-0372-4e2c-9b34-78b9bb453b61', 2, 1, 2, 0, N'And', N'xys.abc.piningSystem', N'endswith', N'90', 0, NULL, 1, 1)
/****** Object:  Table [dbo].[Contexts]    Script Date: 09/15/2014 12:09:35 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Contexts](
	[ContextId] [uniqueidentifier] NOT NULL,
	[DisplayName] [nvarchar](255) NOT NULL,
	[InternalName] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](255) NOT NULL,
	[CacheConnStr] [nvarchar](255) NOT NULL,
	[SiteId] [int] NOT NULL,
	[Active] [tinyint] NOT NULL,
	[FolderId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK__Contexts__23A237491BFD2C07] PRIMARY KEY CLUSTERED 
(
	[ContextId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [UK_DisplayName] UNIQUE NONCLUSTERED 
(
	[DisplayName] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [UK_InternalName] UNIQUE NONCLUSTERED 
(
	[InternalName] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [dbo].[Contexts] ([ContextId], [DisplayName], [InternalName], [Description], [CacheConnStr], [SiteId], [Active], [FolderId]) VALUES (N'54dc03c6-b1ed-4a4e-b4d9-773d4c2651f8', N'test App Display Name3', N'iTest3', N'test description3', N'abc2', 3, 1, N'6914f8d4-636f-4d9f-aa43-45f865de56b6')
INSERT [dbo].[Contexts] ([ContextId], [DisplayName], [InternalName], [Description], [CacheConnStr], [SiteId], [Active], [FolderId]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'iTest44', N'iTest44', N'test description put', N'abc', 1, 1, N'6914f8d4-636f-4d9f-aa43-45f865de56b6')
INSERT [dbo].[Contexts] ([ContextId], [DisplayName], [InternalName], [Description], [CacheConnStr], [SiteId], [Active], [FolderId]) VALUES (N'5ddc49b3-1599-47e2-9f9a-ad37413d28a7', N'iTest', N'iTest', N'test description', N'abc', 1, 1, N'6914f8d4-636f-4d9f-aa43-45f865de56b6')
INSERT [dbo].[Contexts] ([ContextId], [DisplayName], [InternalName], [Description], [CacheConnStr], [SiteId], [Active], [FolderId]) VALUES (N'e61137eb-fd18-4c31-93a1-d0c39bd1001a', N'iTest2', N'iTest2', N'test description', N'abc2', 1, 1, N'6914f8d4-636f-4d9f-aa43-45f865de56b6')
INSERT [dbo].[Contexts] ([ContextId], [DisplayName], [InternalName], [Description], [CacheConnStr], [SiteId], [Active], [FolderId]) VALUES (N'e78d29e3-fdcd-4636-b464-ff265701e528', N'test App Display Name4', N'iTest4', N'temp', N'abc2', 3, 1, N'6914f8d4-636f-4d9f-aa43-45f865de56b6')
/****** Object:  Table [dbo].[Sites]    Script Date: 09/15/2014 12:09:35 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Sites](
	[SiteId] [int] IDENTITY(1,1) NOT NULL,
	[SiteName] [nvarchar](10) NOT NULL,
	[SiteDesc] [nchar](255) NULL,
	[Active] [tinyint] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[SiteId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[Sites] ON
INSERT [dbo].[Sites] ([SiteId], [SiteName], [SiteDesc], [Active]) VALUES (1, N'test site', N'test site description - testing update                                                                                                                                                                                                                         ', 1)
INSERT [dbo].[Sites] ([SiteId], [SiteName], [SiteDesc], [Active]) VALUES (2, N'test site2', N'test site description2                                                                                                                                                                                                                                         ', 1)
INSERT [dbo].[Sites] ([SiteId], [SiteName], [SiteDesc], [Active]) VALUES (3, N'test site3', N'test site description3                                                                                                                                                                                                                                         ', 1)
INSERT [dbo].[Sites] ([SiteId], [SiteName], [SiteDesc], [Active]) VALUES (4, N'test site', N'test site description                                                                                                                                                                                                                                          ', 1)
SET IDENTITY_INSERT [dbo].[Sites] OFF
/****** Object:  Table [dbo].[Settings]    Script Date: 09/15/2014 12:09:35 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Settings](
	[SettingId] [int] NOT NULL,
	[Name] [nvarchar](250) NOT NULL,
 CONSTRAINT [PK_Settings] PRIMARY KEY CLUSTERED 
(
	[SettingId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [dbo].[Settings] ([SettingId], [Name]) VALUES (1, N'MaxTherad')
INSERT [dbo].[Settings] ([SettingId], [Name]) VALUES (2, N'MinThread')
/****** Object:  Table [dbo].[ResourceType]    Script Date: 09/15/2014 12:09:35 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ResourceType](
	[ResourceTypeId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[ResourceTypeId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[ResourceType] ON
INSERT [dbo].[ResourceType] ([ResourceTypeId], [Name]) VALUES (1, N'Context')
INSERT [dbo].[ResourceType] ([ResourceTypeId], [Name]) VALUES (2, N'Folders')
INSERT [dbo].[ResourceType] ([ResourceTypeId], [Name]) VALUES (3, N'Application')
INSERT [dbo].[ResourceType] ([ResourceTypeId], [Name]) VALUES (4, N'Graphs')
INSERT [dbo].[ResourceType] ([ResourceTypeId], [Name]) VALUES (5, N'Commodity')
INSERT [dbo].[ResourceType] ([ResourceTypeId], [Name]) VALUES (6, N'Exchange')
SET IDENTITY_INSERT [dbo].[ResourceType] OFF
/****** Object:  Table [dbo].[MimeType]    Script Date: 09/15/2014 12:09:35 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MimeType](
	[FileId] [int] IDENTITY(1,1) NOT NULL,
	[FileType] [nvarchar](10) NOT NULL,
	[Description] [nvarchar](100) NULL,
	[Active] [int] NOT NULL
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[MimeType] ON
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (1, N'.ai', N'application/postscript', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (2, N'.aif', N'audio/x-aiff', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (3, N'.aifc', N'audio/x-aiff', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (4, N'.aiff', N'audio/x-aiff', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (5, N'.asc', N'text/plain', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (6, N'.atom', N'application/atom+xml', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (7, N'.au', N'audio/basic', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (8, N'.avi', N'video/x-msvideo', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (9, N'.bcpio', N'application/x-bcpio', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (10, N'.bin', N'application/octet-stream', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (11, N'.bmp', N'image/bmp', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (12, N'.cdf', N'application/x-netcdf', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (13, N'.cgm', N'image/cgm', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (14, N'.class', N'application/octet-stream', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (15, N'.cpio', N'application/x-cpio', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (16, N'.cpt', N'application/mac-compactpro', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (17, N'.csh', N'application/x-csh', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (18, N'.css', N'text/css', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (19, N'.dcr', N'application/x-director', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (20, N'.dif', N'video/x-dv', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (21, N'.dir', N'application/x-director', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (22, N'.djv', N'image/vnd.djvu', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (23, N'.djvu', N'image/vnd.djvu', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (24, N'.dll', N'application/octet-stream', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (25, N'.dmg', N'application/octet-stream', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (26, N'.dms', N'application/octet-stream', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (27, N'.doc', N'application/msword', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (28, N'.dtd', N'application/xml-dtd', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (29, N'.dv', N'video/x-dv', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (30, N'.dvi', N'application/x-dvi', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (31, N'.dxr', N'application/x-director', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (32, N'.eps', N'application/postscript', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (33, N'.etx', N'text/x-setext', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (34, N'.exe', N'application/octet-stream', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (35, N'.ez', N'application/andrew-inset', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (36, N'.gif', N'image/gif', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (37, N'.gram', N'application/srgs', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (38, N'.grxml', N'application/srgs+xml', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (39, N'.gtar', N'application/x-gtar', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (40, N'.hdf', N'application/x-hdf', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (41, N'.hqx', N'application/mac-binhex40', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (42, N'.htm', N'text/html', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (43, N'.html', N'text/html', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (44, N'.ice', N'x-conference/x-cooltalk', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (45, N'.ico', N'image/x-icon', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (46, N'.ics', N'text/calendar', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (47, N'.ief', N'image/ief', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (48, N'.ifb', N'text/calendar', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (49, N'.iges', N'model/iges', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (50, N'.igs', N'model/iges', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (51, N'.jnlp', N'application/x-java-jnlp-file', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (52, N'.jp2', N'image/jp2', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (53, N'.jpe', N'image/jpeg', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (54, N'.jpeg', N'image/jpeg', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (55, N'.jpg', N'image/jpeg', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (56, N'.js', N'application/x-javascript', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (57, N'.kar', N'audio/midi', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (58, N'.latex', N'application/x-latex', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (59, N'.lha', N'application/octet-stream', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (60, N'.lzh', N'application/octet-stream', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (61, N'.m3u', N'audio/x-mpegurl', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (62, N'.m4a', N'audio/mp4a-latm', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (63, N'.m4b', N'audio/mp4a-latm', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (64, N'.m4p', N'audio/mp4a-latm', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (65, N'.m4u', N'video/vnd.mpegurl', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (66, N'.m4v', N'video/x-m4v', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (67, N'.mac', N'image/x-macpaint', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (68, N'.man', N'application/x-troff-man', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (69, N'.mathml', N'application/mathml+xml', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (70, N'.me', N'application/x-troff-me', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (71, N'.mesh', N'model/mesh', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (72, N'.mid', N'audio/midi', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (73, N'.midi', N'audio/midi', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (74, N'.mif', N'application/vnd.mif', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (75, N'.mov', N'video/quicktime', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (76, N'.movie', N'video/x-sgi-movie', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (77, N'.mp2', N'audio/mpeg', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (78, N'.mp3', N'audio/mpeg', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (79, N'.mp4', N'video/mp4', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (80, N'.mpe', N'video/mpeg', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (81, N'.mpeg', N'video/mpeg', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (82, N'.mpg', N'video/mpeg', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (83, N'.mpga', N'audio/mpeg', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (84, N'.ms', N'application/x-troff-ms', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (85, N'.msh', N'model/mesh', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (86, N'.mxu', N'video/vnd.mpegurl', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (87, N'.nc', N'application/x-netcdf', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (88, N'.oda', N'application/oda', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (89, N'.ogg', N'application/ogg', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (90, N'.pbm', N'image/x-portable-bitmap', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (91, N'.pct', N'image/pict', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (92, N'.pdb', N'chemical/x-pdb', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (93, N'.pdf', N'application/pdf', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (94, N'.pgm', N'image/x-portable-graymap', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (95, N'.pgn', N'application/x-chess-pgn', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (96, N'.pic', N'image/pict', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (97, N'.pict', N'image/pict', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (98, N'.png', N'image/png', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (99, N'.pnm', N'image/x-portable-anymap', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (100, N'.pnt', N'image/x-macpaint', 1)
GO
print 'Processed 100 total records'
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (101, N'.pntg', N'image/x-macpaint', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (102, N'.ppm', N'image/x-portable-pixmap', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (103, N'.ppt', N'application/vnd.ms-powerpoint', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (104, N'.ps', N'application/postscript', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (105, N'.qt', N'video/quicktime', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (106, N'.qti', N'image/x-quicktime', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (107, N'.qtif', N'image/x-quicktime', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (108, N'.ra', N'audio/x-pn-realaudio', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (109, N'.ram', N'audio/x-pn-realaudio', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (110, N'.ras', N'image/x-cmu-raster', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (111, N'.rdf', N'application/rdf+xml', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (112, N'.reg', N'text/plain', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (113, N'.rgb', N'image/x-rgb', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (114, N'.rm', N'application/vnd.rn-realmedia', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (115, N'.roff', N'application/x-troff', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (116, N'.rtf', N'application/rtf', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (117, N'.rtx', N'text/richtext', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (118, N'.sgm', N'text/sgml', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (119, N'.sgml', N'text/sgml', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (120, N'.sh', N'application/x-sh', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (121, N'.shar', N'application/x-shar', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (122, N'.silo', N'model/mesh', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (123, N'.sit', N'application/x-stuffit', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (124, N'.skd', N'application/x-koan', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (125, N'.skm', N'application/x-koan', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (126, N'.skp', N'application/x-koan', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (127, N'.skt', N'application/x-koan', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (128, N'.smi', N'application/smil', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (129, N'.smil', N'application/smil', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (130, N'.snd', N'audio/basic', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (131, N'.so', N'application/octet-stream', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (132, N'.spl', N'application/x-futuresplash', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (133, N'.src', N'application/x-wais-source', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (134, N'.sv4cpio', N'application/x-sv4cpio', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (135, N'.sv4crc', N'application/x-sv4crc', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (136, N'.svg', N'image/svg+xml', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (137, N'.swf', N'application/x-shockwave-flash', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (138, N'.t', N'application/x-troff', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (139, N'.tar', N'application/x-tar', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (140, N'.tcl', N'application/x-tcl', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (141, N'.tex', N'application/x-tex', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (142, N'.texi', N'application/x-texinfo', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (143, N'.texinfo', N'application/x-texinfo', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (144, N'.tif', N'image/tiff', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (145, N'.tiff', N'image/tiff', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (146, N'.tr', N'application/x-troff', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (147, N'.tsv', N'text/tab-separated-values', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (148, N'.txt', N'text/plain', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (149, N'.ustar', N'application/x-ustar', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (150, N'.vcd', N'application/x-cdlink', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (151, N'.vrml', N'model/vrml', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (152, N'.vxml', N'application/voicexml+xml', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (153, N'.wav', N'audio/x-wav', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (154, N'.wbmp', N'image/vnd.wap.wbmp', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (155, N'.wbmxl', N'application/vnd.wap.wbxml', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (156, N'.wml', N'text/vnd.wap.wml', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (157, N'.wmlc', N'application/vnd.wap.wmlc', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (158, N'.wmls', N'text/vnd.wap.wmlscript', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (159, N'.wmlsc', N'application/vnd.wap.wmlscriptc', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (160, N'.wrl', N'model/vrml', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (161, N'.xbm', N'image/x-xbitmap', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (162, N'.xht', N'application/xhtml+xml', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (163, N'.xhtml', N'application/xhtml+xml', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (164, N'.xls', N'application/vnd.ms-excel', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (165, N'.xml', N'application/xml', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (166, N'.xpm', N'image/x-xpixmap', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (167, N'.xsl', N'application/xml', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (168, N'.xslt', N'application/xslt+xml', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (169, N'.xul', N'application/vnd.mozilla.xul+xml', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (170, N'.xwd', N'image/x-xwindowdump', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (171, N'.xyz', N'chemical/x-xyz', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (172, N'.xlsx', N'application/vnd.ms-excel', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (173, N'.docx', N'application/vnd.ms-word', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (174, N'.mpp', N'application/vnd.ms-project', 1)
INSERT [dbo].[MimeType] ([FileId], [FileType], [Description], [Active]) VALUES (175, N'.vsd', N'application/vnd.ms-visio', 1)
SET IDENTITY_INSERT [dbo].[MimeType] OFF
/****** Object:  UserDefinedFunction [dbo].[Split]    Script Date: 09/15/2014 12:09:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[Split](@String nvarchar(4000), @Delimiter char(1))
RETURNS @Results TABLE (Id Int identity(1,1), Items nvarchar(4000))
AS
BEGIN
DECLARE @INDEX INT
DECLARE @SLICE nvarchar(4000)
-- HAVE TO SET TO 1 SO IT DOESNT EQUAL Z
--     ERO FIRST TIME IN LOOP
SELECT @INDEX = 1
WHILE @INDEX !=0
BEGIN
-- GET THE INDEX OF THE FIRST OCCURENCE OF THE SPLIT CHARACTER
SELECT @INDEX = CHARINDEX(@Delimiter,@STRING)
-- NOW PUSH EVERYTHING TO THE LEFT OF IT INTO THE SLICE VARIABLE
IF @INDEX !=0
SELECT @SLICE = LEFT(@STRING,@INDEX - 1)
ELSE
SELECT @SLICE = @STRING
-- PUT THE ITEM INTO THE RESULTS SET
INSERT INTO @Results(Items) VALUES(@SLICE)
-- CHOP THE ITEM REMOVED OFF THE MAIN STRING
SELECT @STRING = RIGHT(@STRING,LEN(@STRING) - @INDEX)
-- BREAK OUT IF WE ARE DONE
IF LEN(@STRING) = 0 BREAK
END
RETURN
END
GO
/****** Object:  StoredProcedure [dbo].[spuContext]    Script Date: 09/15/2014 12:09:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================================================
-- Author:		<Atul Srivastava>
-- Create date: <06-June-2014>
-- Modified date: <18-Jul-2014> Added Folder Id parameter
-- Description:	<Updating Context details>

-- =============================================================================
 CREATE PROCEDURE [dbo].[spuContext] 
(
	@DisplayName NVARCHAR(255),
	@InternalName NVARCHAR(255),
	@Description NVARCHAR(255),
	@CacheConnStr NVARCHAR(255),
	@SiteId INT,
	@FolderId uniqueidentifier
)	
	 
AS
BEGIN TRY
	BEGIN
		BEGIN
			UPDATE Contexts
				SET DisplayName=@DisplayName,
					[Description]=@Description,
					CacheConnStr=@CacheConnStr,
					SiteId=@SiteId,
					Folderid = @FolderId
				WHERE InternalName = @InternalName AND SiteId=@SiteId
				
		END
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spuSites]    Script Date: 09/15/2014 12:09:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <06-June-2014>
-- Description:	<Updating Sites details,,>

-- =============================================
CREATE PROCEDURE [dbo].[spuSites] 

(
	@SiteId INT,
	@SiteName NVARCHAR(10),
	@SiteDesc NVARCHAR(255)
	
)	
	 
AS
BEGIN TRY
	BEGIN
		UPDATE Sites 
			SET SiteName = @SiteName,SiteDesc= @SiteDesc
			WHERE SiteId = @SiteId
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spiSites]    Script Date: 09/15/2014 12:09:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <06-June-2014>
-- Description:	<Inserting Site details,,>
-- =============================================
CREATE PROCEDURE [dbo].[spiSites]
(
	@SiteName NVARCHAR(10),
	@SiteDesc NVARCHAR(255)

)	
	 
AS
BEGIN TRY
	BEGIN
		INSERT INTO Sites(SiteName,SiteDesc) VALUES (@SiteName,@SiteDesc)
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spiContext]    Script Date: 09/15/2014 12:09:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <06-June-2014>
-- Description:	<Inserting Context details>

-- =============================================
CREATE PROCEDURE [dbo].[spiContext] 
(
	@DisplayName NVARCHAR(255),
	@InternalName NVARCHAR(255),
	@Description NVARCHAR(255),
	@CacheConnStr NVARCHAR(255),
	@SiteId INT,
	@FolderId UNIQUEIDENTIFIER   
)	
	 
AS
BEGIN TRY
	BEGIN
		DECLARE @ContextCount INT
		
		SELECT @ContextCount=COUNT(*) FROM Contexts WHERE InternalName=@InternalName AND SiteId=@SiteId
		IF @ContextCount=0
		BEGIN
			INSERT INTO Contexts(DisplayName,InternalName, Description,CacheConnStr,SiteId,FolderId)
				VALUES(@DisplayName,@InternalName,@Description,@CacheConnStr,@SiteId,@FolderId)
		END
		ELSE 
			SELECT 'Scope already created'
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber;
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spgSitesById]    Script Date: 09/15/2014 12:09:52 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <13-June-2014>
-- Description:	<Getting Site by specific Id,,>
-- =============================================
CREATE PROCEDURE [dbo].[spgSitesById] 
(
	@SiteId INT
)
	 
AS
BEGIN TRY
	BEGIN
		SELECT SiteName,SiteDesc FROM Sites WHERE Active=1 AND SiteId = @SiteId
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber, ERROR_MESSAGE() AS ErrorMessage;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spgSites]    Script Date: 09/15/2014 12:09:52 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <12-June-2014>
-- Description:	<Getting Site details,,>
-- =============================================
CREATE PROCEDURE [dbo].[spgSites]

	 
AS
BEGIN TRY
	BEGIN
		SELECT SiteId,SiteName,SiteDesc,Active FROM Sites WHERE Active=1
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spdContext]    Script Date: 09/15/2014 12:09:54 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <09-June-2014>
-- Description:	<Deleting Context >

-- =============================================
 CREATE PROCEDURE [dbo].[spdContext] 
(
	@InternalName NVARCHAR(255),
	@SiteId INT
)	
	 
AS
BEGIN TRY
	BEGIN
		BEGIN
			UPDATE Contexts
				SET Active=0
				WHERE InternalName = @InternalName AND SiteId= @SiteId
		END
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spgContext]    Script Date: 09/15/2014 12:09:54 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Atul Srivastava>
-- Create date: <11-June-2014>
-- Modified Date <18-Jul-2014> Added folder id in select
-- Description:	<Selecting Context details>

-- ===========================================================
CREATE PROCEDURE [dbo].[spgContext] 
(
	@SiteId INT
)	 
AS
BEGIN TRY
	BEGIN
		SELECT ContextId, DisplayName, InternalName, Description, CacheConnStr, SiteId,FolderId, Active 
		FROM Contexts WHERE Active =1 AND SiteId=@SiteId

	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spdSites]    Script Date: 09/15/2014 12:09:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <06-June-2014>
-- Description:	<Deleting/Deactivating Sites>

-- =============================================
CREATE PROCEDURE [dbo].[spdSites] 

(
	@SiteId INT
)	
	 
AS
BEGIN TRY
	BEGIN
		UPDATE Sites 
			SET Active =0
			WHERE SiteId = @SiteId
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber;
END CATCH;
GO
/****** Object:  Table [dbo].[Users]    Script Date: 09/15/2014 12:09:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[UserId] [int] IDENTITY(1,1) NOT NULL,
	[UserName] [nvarchar](100) NOT NULL,
	[SiteId] [int] NOT NULL,
	[UserFirstName] [nvarchar](50) NULL,
	[UserLastName] [nvarchar](50) NULL,
	[UserEmail] [nvarchar](50) NULL,
	[UserPhone] [nvarchar](50) NULL,
	[UserDesc] [nvarchar](255) NULL,
	[Active] [tinyint] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [UK_Username] UNIQUE NONCLUSTERED 
(
	[UserName] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[Users] ON
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (1, N'test user1', 1, N'test1 first name', N'test1 last name', N't@t.com', N'111-111-111-11', N'test desc1', 1)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (2, N'test user2', 2, N'test2 first name', N'test2 last name', N't@t.com', N'111-111-111-11', N'test desc2', 1)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (3, N'test user6', 1, N'test11 first name', N'test11 last name', N't@t.com', N'111-111-111-11', N'test desc11', 1)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (4, N'test put4', 1, N'first name post', N'last name post', N't@t.com', N'111-111-111-11', N'test post des', 0)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (5, N'test user3', 3, N'test3 first name', N'test3 last name', N't@t.com', N'111-111-111-11', N'test desc3', 1)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (6, N'test user333', 3, N'test33 first name', N'test33 last name', N't@t.com', NULL, N'test desc33', 0)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (7, N'test user334', 3, N'test33 first name', N'test33 last name', N't@t.com', NULL, N'test desc33', 1)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (8, N'test user335', 3, N'test33 first name', N'test33 last name', N't@t.com', NULL, N'test desc33', 1)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (32, N'vmallire', 1, N'Vidisha', N'Mallireddy', N'vmallire@bechtel.com', NULL, N'kbkesf', 1)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (38, N'1', 1, N'1', N'1', N'1@1.in', N'1', N'1', 1)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (39, N'2', 1, N'2', N'2', N'2@gmail.com', N'2222222222222', N'2', 1)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (40, N'ss', 1, N'ss', N'ss', N'ss@gmail.com', N'989155589', N'', 1)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (41, N'rsingh', 1, N'rohit', N'singh', N'rsingh6@bechtel.com', N'9958558803', N'55555', 1)
SET IDENTITY_INSERT [dbo].[Users] OFF
/****** Object:  Table [dbo].[Permissions]    Script Date: 09/15/2014 12:09:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Permissions](
	[PermissionId] [int] IDENTITY(1,1) NOT NULL,
	[SiteId] [int] NOT NULL,
	[PermissionName] [nvarchar](100) NOT NULL,
	[PermissionDesc] [nvarchar](100) NULL,
	[Active] [tinyint] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[PermissionId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[Permissions] ON
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (2, 1, N'Edit', N'Edit', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (3, 1, N'Add', N'Add', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (4, 1, N'Delete', N'Delete', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (5, 3, N'test 33', N'permission desc', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (6, 1, N'testing put', N'testing put', 0)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (7, 1, N'Invoke exchange', N'excute', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (8, 1, N'ConfigureExchange', N'create exchange', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (9, 1, N'dd1', N'dd1', 0)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (10, 1, N'test1', N'testdesc', 0)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (11, 1, N'1', N'1', 0)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (12, 1, N'2', N'2', 0)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (13, 1, N'222222222222222222', N'2222444444444444444444444444444', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (14, 1, N'pname', N'pdesc', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (15, 1, N't1', N'qq', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (16, 1, N'qq', N'ss', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (17, 1, N'qq', N'ss', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (18, 1, N'test123', N'123', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (19, 1, N'block', N'block permission', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (20, 1, N'permission', N'permission des', 1)
SET IDENTITY_INSERT [dbo].[Permissions] OFF
/****** Object:  Table [dbo].[Roles]    Script Date: 09/15/2014 12:09:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Roles](
	[RoleId] [int] IDENTITY(1,1) NOT NULL,
	[SiteId] [int] NOT NULL,
	[RoleName] [nvarchar](100) NOT NULL,
	[RoleDesc] [nvarchar](100) NULL,
	[Active] [tinyint] NULL,
 CONSTRAINT [PK__Roles__8AFACE1A1273C1CD] PRIMARY KEY CLUSTERED 
(
	[RoleId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[Roles] ON
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (1, 1, N'Role1', N'role1 desc', 1)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (2, 1, N'test333', N'0000000', 1)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (3, 3, N'Role3', N'role3 desc', 1)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (4, 1, N'test333', N'test desc333 updated', 0)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (5, 2, N'Role2', N'role23 desc', 1)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (6, 3, N'Role3', N'role34 desc', 1)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (7, 1, N'AA', N'BB', 1)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (8, 1, N'AA', N'BB', 1)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (9, 3, N'AAA', N'BBB', 1)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (10, 1, N'Inserting Role', N'Inserting Role from aPI', 1)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (11, 1, N'ff', N'ff', 1)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (12, 1, N'ff', N'ff', 0)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (13, 1, N'dee', N'pak', 1)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (14, 1, N'qqq', N'qqq', 1)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (15, 1, N'hh', N'hh', 0)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (16, 1, N'jjtt', N'jjtt', 1)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (18, 1, N'role', N'ddd', 1)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (19, 1, N'rr', N'rr', 1)
SET IDENTITY_INSERT [dbo].[Roles] OFF
/****** Object:  Table [dbo].[Applications]    Script Date: 09/15/2014 12:09:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Applications](
	[ContextId] [uniqueidentifier] NOT NULL,
	[ApplicationId] [uniqueidentifier] NOT NULL,
	[DisplayName] [nvarchar](255) NOT NULL,
	[InternalName] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](255) NOT NULL,
	[DXFRUrl] [nvarchar](255) NOT NULL,
	[SiteId] [int] NOT NULL,
	[Active] [tinyint] NOT NULL,
	[Assembly] [nvarchar](550) NULL,
 CONSTRAINT [PK__Applicat__C93A4C994AB81AF0] PRIMARY KEY CLUSTERED 
(
	[ApplicationId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [dbo].[Applications] ([ContextId], [ApplicationId], [DisplayName], [InternalName], [Description], [DXFRUrl], [SiteId], [Active], [Assembly]) VALUES (N'5ddc49b3-1599-47e2-9f9a-ad37413d28a7', N'3e1e418d-fa51-403b-bb7b-0cade34283a7', N'test App display name1', N'iApp2', N'test desc2 edit', N'abcdef', 1, 1, NULL)
INSERT [dbo].[Applications] ([ContextId], [ApplicationId], [DisplayName], [InternalName], [Description], [DXFRUrl], [SiteId], [Active], [Assembly]) VALUES (N'5ddc49b3-1599-47e2-9f9a-ad37413d28a7', N'c274b3fb-1794-4bc2-bcab-3615fea7f11f', N'test App display name1', N'iApp2', N'test desc2 edit', N'abcdef', 1, 1, NULL)
INSERT [dbo].[Applications] ([ContextId], [ApplicationId], [DisplayName], [InternalName], [Description], [DXFRUrl], [SiteId], [Active], [Assembly]) VALUES (N'5ddc49b3-1599-47e2-9f9a-ad37413d28a7', N'f0638d95-a1b2-4675-927d-7a83c28465c6', N'test App display name1', N'iApp2', N'test desc2 edit', N'abcdef', 1, 1, NULL)
INSERT [dbo].[Applications] ([ContextId], [ApplicationId], [DisplayName], [InternalName], [Description], [DXFRUrl], [SiteId], [Active], [Assembly]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'7877c504-133d-41b9-8680-7bfd73840435', N'DEF', N'DEF', N'wr', N'http://localhost:54321/dxfr', 1, 1, NULL)
INSERT [dbo].[Applications] ([ContextId], [ApplicationId], [DisplayName], [InternalName], [Description], [DXFRUrl], [SiteId], [Active], [Assembly]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'3456c504-133d-41b9-8780-7bfd73840444', N'ABC', N'ABC', N'trr', N'http://localhost:53421/dxfr', 1, 1, NULL)
INSERT [dbo].[Applications] ([ContextId], [ApplicationId], [DisplayName], [InternalName], [Description], [DXFRUrl], [SiteId], [Active], [Assembly]) VALUES (N'5ddc49b3-1599-47e2-9f9a-ad37413d28a7', N'8ebf712a-6838-4851-ae0f-8251d5153db4', N'test App display name1', N'iApp2', N'test desc2 edit', N'abcdef', 1, 1, NULL)
INSERT [dbo].[Applications] ([ContextId], [ApplicationId], [DisplayName], [InternalName], [Description], [DXFRUrl], [SiteId], [Active], [Assembly]) VALUES (N'5ddc49b3-1599-47e2-9f9a-ad37413d28a7', N'2783c17d-2daf-4f01-b92b-fbe82c9f2128', N'test App display name1', N'iApp3', N'test desc3', N'abcdef', 1, 0, NULL)
INSERT [dbo].[Applications] ([ContextId], [ApplicationId], [DisplayName], [InternalName], [Description], [DXFRUrl], [SiteId], [Active], [Assembly]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'83e6e521-1a27-4e2b-8dc0-fe3f021a26de', N'test App display name', N'iApp', N'test desc', N'abcd', 3, 1, NULL)
/****** Object:  Table [dbo].[Folders]    Script Date: 09/15/2014 12:09:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Folders](
	[FolderId] [uniqueidentifier] NOT NULL,
	[ParentFolderId] [uniqueidentifier] NULL,
	[FolderName] [nvarchar](50) NOT NULL,
	[SiteId] [int] NOT NULL,
	[Active] [tinyint] NOT NULL,
 CONSTRAINT [PK__Folders__ACD7107F2B3F6F97] PRIMARY KEY CLUSTERED 
(
	[FolderId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [dbo].[Folders] ([FolderId], [ParentFolderId], [FolderName], [SiteId], [Active]) VALUES (N'f2bb2bdc-22c8-4e1d-82da-04a5334ec2f0', N'00000000-1111-2222-3333-444444444444', N'Civil', 1, 1)
INSERT [dbo].[Folders] ([FolderId], [ParentFolderId], [FolderName], [SiteId], [Active]) VALUES (N'6914f8d4-636f-4d9f-aa43-45f865de56b6', N'504ff982-740d-45a6-95d8-6f6ba425fd40', N'Engineering', 1, 1)
INSERT [dbo].[Folders] ([FolderId], [ParentFolderId], [FolderName], [SiteId], [Active]) VALUES (N'9614f8d4-636f-4d9f-aa43-45f865de56b7', N'00000000-1111-2222-3333-444444444444', N'BsII', 1, 1)
INSERT [dbo].[Folders] ([FolderId], [ParentFolderId], [FolderName], [SiteId], [Active]) VALUES (N'504ff982-740d-45a6-95d8-6f6ba425fd40', N'00000000-1111-2222-3333-444444444444', N'Construction', 1, 1)
/****** Object:  Table [dbo].[Commodity]    Script Date: 09/15/2014 12:09:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Commodity](
	[ContextId] [uniqueidentifier] NOT NULL,
	[CommodityId] [uniqueidentifier] NOT NULL,
	[CommodityName] [nvarchar](255) NULL,
	[SiteId] [int] NOT NULL,
	[Active] [tinyint] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[CommodityId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'a6ac8ca5-662c-4249-9d1f-2b7cb962f2cb', N'dee1ii', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'ee565602-ba52-422c-a01f-932490796666', N'Lines', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'd45851cd-7662-44d4-8339-97ab64d47771', N'Linesdeep', 1, 0)
/****** Object:  Table [dbo].[Groups]    Script Date: 09/15/2014 12:09:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Groups](
	[GroupId] [int] IDENTITY(1,1) NOT NULL,
	[SiteId] [int] NOT NULL,
	[GroupName] [nvarchar](100) NOT NULL,
	[GroupDesc] [nvarchar](255) NULL,
	[Active] [tinyint] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[GroupId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[Groups] ON
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (1, 1, N'test group1', N'test group description', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (3, 1, N'test group3', N'desc 123457898', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (4, 2, N'abc4', N'xyz', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (5, 2, N'abc5', N'xyz2', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (6, 2, N'abc6', N'xyz2', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (7, 2, N'abc7', N'xyz2', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (8, 3, N'abc8', N'xyz3', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (9, 1, N'abc9', N'xyz1', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (10, 3, N'abc10', N'xyz33', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (11, 2, N'test group', N'test group desc', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (12, 1, N'testing put', N'testing put', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (16, 1, N'1111', N'11111', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (17, 1, N'123', N'123', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (18, 1, N'fghf', N'gfhfgh', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (19, 1, N'999', N'123456', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (20, 1, N'test p', N'test p', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (21, 1, N'test p', N'test t', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (22, 1, N'test group2', N'desc 12345', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (23, 1, N'abc23', N'1234', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (24, 1, N'abc24', N'111111111', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (37, 1, N'ss', N'ss', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (38, 1, N'dd', N'dd', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (42, 1, N'wwwww', N'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (43, 1, N'111111', N'111111', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (44, 1, N'qqqqqqqqqqq', N'qqqqqqqqqqqqq', 1)
SET IDENTITY_INSERT [dbo].[Groups] OFF
/****** Object:  Table [dbo].[GroupRoles]    Script Date: 09/15/2014 12:09:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GroupRoles](
	[GroupId] [int] NOT NULL,
	[RoleId] [int] NOT NULL,
	[SiteId] [int] NOT NULL,
	[Active] [tinyint] NOT NULL,
 CONSTRAINT [PK_GroupRoles] PRIMARY KEY CLUSTERED 
(
	[GroupId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (1, 1, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (1, 2, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (1, 3, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (1, 5, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (1, 6, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (1, 7, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (1, 8, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (1, 9, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (1, 10, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (1, 11, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (1, 14, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (1, 16, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (3, 1, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (3, 5, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (3, 6, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (3, 8, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (3, 10, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (4, 1, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (4, 2, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (4, 3, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (4, 6, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (5, 2, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (5, 5, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (5, 6, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (7, 1, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (7, 3, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (7, 5, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (8, 7, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (9, 1, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (10, 1, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (10, 2, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (11, 1, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (11, 3, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (37, 2, 1, 1)
/****** Object:  Table [dbo].[Graphs]    Script Date: 09/15/2014 12:09:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Graphs](
	[ApplicationId] [uniqueidentifier] NOT NULL,
	[GraphId] [uniqueidentifier] NOT NULL,
	[GraphName] [nvarchar](255) NOT NULL,
	[Graph] [varbinary](max) NOT NULL,
	[SiteId] [int] NOT NULL,
	[Active] [tinyint] NOT NULL,
	[ExchangeVisible] [tinyint] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[GraphId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
INSERT [dbo].[Graphs] ([ApplicationId], [GraphId], [GraphName], [Graph], [SiteId], [Active], [ExchangeVisible]) VALUES (N'3e1e418d-fa51-403b-bb7b-0cade34283a7', N'6f9f8981-55f3-433c-a86c-4b1262ff2efc', N'abc', 0x3C6D617070696E6720786D6C6E733D22687474703A2F2F7777772E6972696E67746F6F6C732E6F72672F6D617070696E672220786D6C6E733A693D22687474703A2F2F7777772E77332E6F72672F323030312F584D4C536368656D612D696E7374616E6365223E0D0A093C67726170684D6170733E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E4C696E65733C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5231393139323436323535303C2F69643E0D0A0909090909093C6E616D653E504950494E47204E4554574F524B2053595354454D3C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E5441473C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234303437313034313735343C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204E414D45202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E5441473C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5234313234383736373834363C2F69643E0D0A090909090909093C6E616D653E506C616E7441726561486173506C616E744F626A6563743C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5237373538353332353537363C2F69643E0D0A0909090909090909093C6E616D653E706172743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5236323135373439383031323C2F69643E0D0A0909090909090909093C6E616D653E617373656D626C79547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5233303934323337383439323C2F64617461547970653E0D0A0909090909090909093C76616C75653E504C414E5420415245412048415320504C414E54204F424A4543543C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5236323334383032303230313C2F69643E0D0A0909090909090909093C6E616D653E77686F6C653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234393635383331393833333C2F64617461547970653E0D0A0909090909090909093C76616C75653E504C414E5420415245413C2F76616C75653E0D0A0909090909090909093C636C6173734D61703E0D0A090909090909090909093C69643E72646C3A5234393635383331393833333C2F69643E0D0A090909090909090909093C6E616D653E504C414E5420415245413C2F6E616D653E0D0A090909090909090909093C6964656E746966696572733E0D0A09090909090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A090909090909090909093C2F6964656E746966696572733E0D0A090909090909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5234313234383736373834362830292F74706C3A5236323334383032303230313C2F706174683E0D0A0909090909090909093C2F636C6173734D61703E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5231313332373238313438353C2F69643E0D0A090909090909093C6E616D653E50416E644944526570726573656E746174696F6E3C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323731353031333837373C2F69643E0D0A0909090909090909093C6E616D653E6F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233353239303932393137353C2F69643E0D0A0909090909090909093C6E616D653E726570726573656E746174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393438363933313937353C2F64617461547970653E0D0A0909090909090909093C76616C75653E5020414E442049204449414752414D3C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5237343332383831353731303C2F69643E0D0A0909090909090909093C6E616D653E726570726573656E746174696F6E3C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393438363933313937353C2F64617461547970653E0D0A0909090909090909093C76616C75653E5020414E442049204449414752414D3C2F76616C75653E0D0A0909090909090909093C636C6173734D61703E0D0A090909090909090909093C69643E72646C3A5239393438363933313937353C2F69643E0D0A090909090909090909093C6E616D653E5020414E442049204449414752414D3C2F6E616D653E0D0A090909090909090909093C6964656E746966696572733E0D0A09090909090909090909093C6964656E7469666965723E4C494E45532E5049444E554D4245523C2F6964656E7469666965723E0D0A090909090909090909093C2F6964656E746966696572733E0D0A090909090909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5231313332373238313438352830292F74706C3A5237343332383831353731303C2F706174683E0D0A0909090909090909093C2F636C6173734D61703E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5237373434333335383831383C2F69643E0D0A090909090909093C6E616D653E4E6F6D696E616C4469616D657465723C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5231363331343132373832353C2F69643E0D0A0909090909090909093C6E616D653E686173506F73736573736F723C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5239333838343139333639323C2F69643E0D0A0909090909090909093C6E616D653E6861735363616C653C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E554F4D5F4E4F4D4449414D455445523C2F70726F70657274794E616D653E0D0A0909090909090909093C76616C75654C6973744E616D653E4C656E6774683C2F76616C75654C6973744E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5235393839363835333233393C2F69643E0D0A0909090909090909093C6E616D653E686173547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5231373632323134383034333C2F64617461547970653E0D0A0909090909090909093C76616C75653E4E4F4D494E414C204449414D455445523C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5239303638383036333634383C2F69643E0D0A0909090909090909093C6E616D653E76616C56616C75653C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A73696E676C653C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E4E4F4D4449414D455445523C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5234393635383331393833333C2F69643E0D0A0909090909093C6E616D653E504C414E5420415245413C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5234313234383736373834362830292F74706C3A5236323334383032303230313C2F706174683E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234303437313034313735343C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204E414D45202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E415245413C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5239393438363933313937353C2F69643E0D0A0909090909093C6E616D653E5020414E442049204449414752414D3C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E5049444E554D4245523C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5231313332373238313438352830292F74706C3A5237343332383831353731303C2F706174683E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5233303139333338363237333C2F69643E0D0A090909090909093C6E616D653E436C61737369666965644964656E74696669636174696F6E3C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5231363839333238333035303C2F64617461547970653E0D0A0909090909090909093C76616C75653E44524157494E47204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E5049444E554D4245523C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E4C494E45533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E4C313C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5231393139323436323535303C2F69643E0D0A0909090909093C6E616D653E504950494E47204E4554574F524B2053595354454D3C2F6E616D653E0D0A0909090909093C6964656E74696669657244656C696D697465723E5F3C2F6964656E74696669657244656C696D697465723E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E415245413C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E4C494E45533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E546573743C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5239383530353931383430343C2F69643E0D0A0909090909093C6E616D653E494E535452554D454E543C2F6E616D653E0D0A0909090909093C6964656E74696669657244656C696D697465723E5F3C2F6964656E74696669657244656C696D697465723E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E494E535452554D454E54532E5441473C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E494E535452554D454E54532E5441473C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E494E535452554D454E54532E4153534F435F45513C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A090909090909093C696E6465783E313C2F696E6465783E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E494E535452554D454E54533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A093C2F67726170684D6170733E0D0A093C76616C75654C6973744D6170733E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E4C656E6774683C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E6D6D3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5235323035343237353337343C2F7572693E0D0A09090909093C6C6162656C3E4D494C4C494D45545245203C2F6C6162656C3E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E54656D70657261747572653C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E646567433C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5237343837373939323730333C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E6465674B3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5235333130393636323430353C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E50726573737572653C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E50415343414C3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5239383738373735343236373C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E4241523C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5238333439303530363535313C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E6C313C2F6E616D653E0D0A0909093C76616C75654D6170732F3E0D0A09093C2F76616C75654C6973744D61703E0D0A093C2F76616C75654C6973744D6170733E0D0A3C2F6D617070696E673E, 1, 1, 1)
INSERT [dbo].[Graphs] ([ApplicationId], [GraphId], [GraphName], [Graph], [SiteId], [Active], [ExchangeVisible]) VALUES (N'3e1e418d-fa51-403b-bb7b-0cade34283a7', N'87a895d1-dac2-4083-b900-640ecba914e8', N'TEST', 0x3C6D617070696E6720786D6C6E733D22687474703A2F2F7777772E6972696E67746F6F6C732E6F72672F6D617070696E672220786D6C6E733A693D22687474703A2F2F7777772E77332E6F72672F323030312F584D4C536368656D612D696E7374616E6365223E0D0A093C67726170684D6170733E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E4C696E65733C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5231393139323436323535303C2F69643E0D0A0909090909093C6E616D653E504950494E47204E4554574F524B2053595354454D3C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E5441473C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234303437313034313735343C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204E414D45202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E5441473C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5234313234383736373834363C2F69643E0D0A090909090909093C6E616D653E506C616E7441726561486173506C616E744F626A6563743C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5237373538353332353537363C2F69643E0D0A0909090909090909093C6E616D653E706172743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5236323135373439383031323C2F69643E0D0A0909090909090909093C6E616D653E617373656D626C79547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5233303934323337383439323C2F64617461547970653E0D0A0909090909090909093C76616C75653E504C414E5420415245412048415320504C414E54204F424A4543543C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5236323334383032303230313C2F69643E0D0A0909090909090909093C6E616D653E77686F6C653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234393635383331393833333C2F64617461547970653E0D0A0909090909090909093C76616C75653E504C414E5420415245413C2F76616C75653E0D0A0909090909090909093C636C6173734D61703E0D0A090909090909090909093C69643E72646C3A5234393635383331393833333C2F69643E0D0A090909090909090909093C6E616D653E504C414E5420415245413C2F6E616D653E0D0A090909090909090909093C6964656E746966696572733E0D0A09090909090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A090909090909090909093C2F6964656E746966696572733E0D0A090909090909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5234313234383736373834362830292F74706C3A5236323334383032303230313C2F706174683E0D0A0909090909090909093C2F636C6173734D61703E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5231313332373238313438353C2F69643E0D0A090909090909093C6E616D653E50416E644944526570726573656E746174696F6E3C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323731353031333837373C2F69643E0D0A0909090909090909093C6E616D653E6F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233353239303932393137353C2F69643E0D0A0909090909090909093C6E616D653E726570726573656E746174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393438363933313937353C2F64617461547970653E0D0A0909090909090909093C76616C75653E5020414E442049204449414752414D3C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5237343332383831353731303C2F69643E0D0A0909090909090909093C6E616D653E726570726573656E746174696F6E3C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393438363933313937353C2F64617461547970653E0D0A0909090909090909093C76616C75653E5020414E442049204449414752414D3C2F76616C75653E0D0A0909090909090909093C636C6173734D61703E0D0A090909090909090909093C69643E72646C3A5239393438363933313937353C2F69643E0D0A090909090909090909093C6E616D653E5020414E442049204449414752414D3C2F6E616D653E0D0A090909090909090909093C6964656E746966696572733E0D0A09090909090909090909093C6964656E7469666965723E4C494E45532E5049444E554D4245523C2F6964656E7469666965723E0D0A090909090909090909093C2F6964656E746966696572733E0D0A090909090909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5231313332373238313438352830292F74706C3A5237343332383831353731303C2F706174683E0D0A0909090909090909093C2F636C6173734D61703E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5237373434333335383831383C2F69643E0D0A090909090909093C6E616D653E4E6F6D696E616C4469616D657465723C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5231363331343132373832353C2F69643E0D0A0909090909090909093C6E616D653E686173506F73736573736F723C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5239333838343139333639323C2F69643E0D0A0909090909090909093C6E616D653E6861735363616C653C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E554F4D5F4E4F4D4449414D455445523C2F70726F70657274794E616D653E0D0A0909090909090909093C76616C75654C6973744E616D653E4C656E6774683C2F76616C75654C6973744E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5235393839363835333233393C2F69643E0D0A0909090909090909093C6E616D653E686173547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5231373632323134383034333C2F64617461547970653E0D0A0909090909090909093C76616C75653E4E4F4D494E414C204449414D455445523C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5239303638383036333634383C2F69643E0D0A0909090909090909093C6E616D653E76616C56616C75653C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A73696E676C653C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E4E4F4D4449414D455445523C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5234393635383331393833333C2F69643E0D0A0909090909093C6E616D653E504C414E5420415245413C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5234313234383736373834362830292F74706C3A5236323334383032303230313C2F706174683E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234303437313034313735343C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204E414D45202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E415245413C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5239393438363933313937353C2F69643E0D0A0909090909093C6E616D653E5020414E442049204449414752414D3C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E5049444E554D4245523C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5231313332373238313438352830292F74706C3A5237343332383831353731303C2F706174683E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5233303139333338363237333C2F69643E0D0A090909090909093C6E616D653E436C61737369666965644964656E74696669636174696F6E3C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5231363839333238333035303C2F64617461547970653E0D0A0909090909090909093C76616C75653E44524157494E47204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E5049444E554D4245523C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E4C494E45533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E4C313C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5231393139323436323535303C2F69643E0D0A0909090909093C6E616D653E504950494E47204E4554574F524B2053595354454D3C2F6E616D653E0D0A0909090909093C6964656E74696669657244656C696D697465723E5F3C2F6964656E74696669657244656C696D697465723E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E415245413C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E4C494E45533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E546573743C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5239383530353931383430343C2F69643E0D0A0909090909093C6E616D653E494E535452554D454E543C2F6E616D653E0D0A0909090909093C6964656E74696669657244656C696D697465723E5F3C2F6964656E74696669657244656C696D697465723E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E494E535452554D454E54532E5441473C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E494E535452554D454E54532E5441473C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E494E535452554D454E54532E4153534F435F45513C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A090909090909093C696E6465783E313C2F696E6465783E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E494E535452554D454E54533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A093C2F67726170684D6170733E0D0A093C76616C75654C6973744D6170733E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E4C656E6774683C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E6D6D3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5235323035343237353337343C2F7572693E0D0A09090909093C6C6162656C3E4D494C4C494D45545245203C2F6C6162656C3E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E54656D70657261747572653C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E646567433C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5237343837373939323730333C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E6465674B3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5235333130393636323430353C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E50726573737572653C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E50415343414C3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5239383738373735343236373C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E4241523C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5238333439303530363535313C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E6C313C2F6E616D653E0D0A0909093C76616C75654D6170732F3E0D0A09093C2F76616C75654C6973744D61703E0D0A093C2F76616C75654C6973744D6170733E0D0A3C2F6D617070696E673E, 1, 1, 1)
INSERT [dbo].[Graphs] ([ApplicationId], [GraphId], [GraphName], [Graph], [SiteId], [Active], [ExchangeVisible]) VALUES (N'3e1e418d-fa51-403b-bb7b-0cade34283a7', N'14fe717e-db88-406e-8a67-6e5cbb4ac731', N'TEST', 0x3C6D617070696E6720786D6C6E733D22687474703A2F2F7777772E6972696E67746F6F6C732E6F72672F6D617070696E672220786D6C6E733A693D22687474703A2F2F7777772E77332E6F72672F323030312F584D4C536368656D612D696E7374616E6365223E0D0A093C67726170684D6170733E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E4C696E65733C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5231393139323436323535303C2F69643E0D0A0909090909093C6E616D653E504950494E47204E4554574F524B2053595354454D3C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E5441473C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234303437313034313735343C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204E414D45202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E5441473C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5234313234383736373834363C2F69643E0D0A090909090909093C6E616D653E506C616E7441726561486173506C616E744F626A6563743C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5237373538353332353537363C2F69643E0D0A0909090909090909093C6E616D653E706172743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5236323135373439383031323C2F69643E0D0A0909090909090909093C6E616D653E617373656D626C79547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5233303934323337383439323C2F64617461547970653E0D0A0909090909090909093C76616C75653E504C414E5420415245412048415320504C414E54204F424A4543543C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5236323334383032303230313C2F69643E0D0A0909090909090909093C6E616D653E77686F6C653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234393635383331393833333C2F64617461547970653E0D0A0909090909090909093C76616C75653E504C414E5420415245413C2F76616C75653E0D0A0909090909090909093C636C6173734D61703E0D0A090909090909090909093C69643E72646C3A5234393635383331393833333C2F69643E0D0A090909090909090909093C6E616D653E504C414E5420415245413C2F6E616D653E0D0A090909090909090909093C6964656E746966696572733E0D0A09090909090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A090909090909090909093C2F6964656E746966696572733E0D0A090909090909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5234313234383736373834362830292F74706C3A5236323334383032303230313C2F706174683E0D0A0909090909090909093C2F636C6173734D61703E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5231313332373238313438353C2F69643E0D0A090909090909093C6E616D653E50416E644944526570726573656E746174696F6E3C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323731353031333837373C2F69643E0D0A0909090909090909093C6E616D653E6F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233353239303932393137353C2F69643E0D0A0909090909090909093C6E616D653E726570726573656E746174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393438363933313937353C2F64617461547970653E0D0A0909090909090909093C76616C75653E5020414E442049204449414752414D3C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5237343332383831353731303C2F69643E0D0A0909090909090909093C6E616D653E726570726573656E746174696F6E3C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393438363933313937353C2F64617461547970653E0D0A0909090909090909093C76616C75653E5020414E442049204449414752414D3C2F76616C75653E0D0A0909090909090909093C636C6173734D61703E0D0A090909090909090909093C69643E72646C3A5239393438363933313937353C2F69643E0D0A090909090909090909093C6E616D653E5020414E442049204449414752414D3C2F6E616D653E0D0A090909090909090909093C6964656E746966696572733E0D0A09090909090909090909093C6964656E7469666965723E4C494E45532E5049444E554D4245523C2F6964656E7469666965723E0D0A090909090909090909093C2F6964656E746966696572733E0D0A090909090909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5231313332373238313438352830292F74706C3A5237343332383831353731303C2F706174683E0D0A0909090909090909093C2F636C6173734D61703E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5237373434333335383831383C2F69643E0D0A090909090909093C6E616D653E4E6F6D696E616C4469616D657465723C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5231363331343132373832353C2F69643E0D0A0909090909090909093C6E616D653E686173506F73736573736F723C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5239333838343139333639323C2F69643E0D0A0909090909090909093C6E616D653E6861735363616C653C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E554F4D5F4E4F4D4449414D455445523C2F70726F70657274794E616D653E0D0A0909090909090909093C76616C75654C6973744E616D653E4C656E6774683C2F76616C75654C6973744E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5235393839363835333233393C2F69643E0D0A0909090909090909093C6E616D653E686173547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5231373632323134383034333C2F64617461547970653E0D0A0909090909090909093C76616C75653E4E4F4D494E414C204449414D455445523C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5239303638383036333634383C2F69643E0D0A0909090909090909093C6E616D653E76616C56616C75653C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A73696E676C653C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E4E4F4D4449414D455445523C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5234393635383331393833333C2F69643E0D0A0909090909093C6E616D653E504C414E5420415245413C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5234313234383736373834362830292F74706C3A5236323334383032303230313C2F706174683E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234303437313034313735343C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204E414D45202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E415245413C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5239393438363933313937353C2F69643E0D0A0909090909093C6E616D653E5020414E442049204449414752414D3C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E5049444E554D4245523C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5231313332373238313438352830292F74706C3A5237343332383831353731303C2F706174683E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5233303139333338363237333C2F69643E0D0A090909090909093C6E616D653E436C61737369666965644964656E74696669636174696F6E3C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5231363839333238333035303C2F64617461547970653E0D0A0909090909090909093C76616C75653E44524157494E47204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E5049444E554D4245523C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E4C494E45533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E4C313C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5231393139323436323535303C2F69643E0D0A0909090909093C6E616D653E504950494E47204E4554574F524B2053595354454D3C2F6E616D653E0D0A0909090909093C6964656E74696669657244656C696D697465723E5F3C2F6964656E74696669657244656C696D697465723E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E415245413C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E4C494E45533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E546573743C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5239383530353931383430343C2F69643E0D0A0909090909093C6E616D653E494E535452554D454E543C2F6E616D653E0D0A0909090909093C6964656E74696669657244656C696D697465723E5F3C2F6964656E74696669657244656C696D697465723E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E494E535452554D454E54532E5441473C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E494E535452554D454E54532E5441473C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E494E535452554D454E54532E4153534F435F45513C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A090909090909093C696E6465783E313C2F696E6465783E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E494E535452554D454E54533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A093C2F67726170684D6170733E0D0A093C76616C75654C6973744D6170733E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E4C656E6774683C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E6D6D3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5235323035343237353337343C2F7572693E0D0A09090909093C6C6162656C3E4D494C4C494D45545245203C2F6C6162656C3E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E54656D70657261747572653C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E646567433C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5237343837373939323730333C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E6465674B3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5235333130393636323430353C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E50726573737572653C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E50415343414C3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5239383738373735343236373C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E4241523C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5238333439303530363535313C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E6C313C2F6E616D653E0D0A0909093C76616C75654D6170732F3E0D0A09093C2F76616C75654C6973744D61703E0D0A093C2F76616C75654C6973744D6170733E0D0A3C2F6D617070696E673E, 1, 1, 1)
INSERT [dbo].[Graphs] ([ApplicationId], [GraphId], [GraphName], [Graph], [SiteId], [Active], [ExchangeVisible]) VALUES (N'3456c504-133d-41b9-8780-7bfd73840444', N'071bce1a-9396-43c9-98f3-73359a4b2e0d', N'TEST', 0x3C6D617070696E6720786D6C6E733D22687474703A2F2F7777772E6972696E67746F6F6C732E6F72672F6D617070696E672220786D6C6E733A693D22687474703A2F2F7777772E77332E6F72672F323030312F584D4C536368656D612D696E7374616E6365223E0D0A093C67726170684D6170733E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E4C696E65733C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5231393139323436323535303C2F69643E0D0A0909090909093C6E616D653E504950494E47204E4554574F524B2053595354454D3C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E5441473C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234303437313034313735343C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204E414D45202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E5441473C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5234313234383736373834363C2F69643E0D0A090909090909093C6E616D653E506C616E7441726561486173506C616E744F626A6563743C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5237373538353332353537363C2F69643E0D0A0909090909090909093C6E616D653E706172743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5236323135373439383031323C2F69643E0D0A0909090909090909093C6E616D653E617373656D626C79547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5233303934323337383439323C2F64617461547970653E0D0A0909090909090909093C76616C75653E504C414E5420415245412048415320504C414E54204F424A4543543C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5236323334383032303230313C2F69643E0D0A0909090909090909093C6E616D653E77686F6C653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234393635383331393833333C2F64617461547970653E0D0A0909090909090909093C76616C75653E504C414E5420415245413C2F76616C75653E0D0A0909090909090909093C636C6173734D61703E0D0A090909090909090909093C69643E72646C3A5234393635383331393833333C2F69643E0D0A090909090909090909093C6E616D653E504C414E5420415245413C2F6E616D653E0D0A090909090909090909093C6964656E746966696572733E0D0A09090909090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A090909090909090909093C2F6964656E746966696572733E0D0A090909090909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5234313234383736373834362830292F74706C3A5236323334383032303230313C2F706174683E0D0A0909090909090909093C2F636C6173734D61703E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5231313332373238313438353C2F69643E0D0A090909090909093C6E616D653E50416E644944526570726573656E746174696F6E3C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323731353031333837373C2F69643E0D0A0909090909090909093C6E616D653E6F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233353239303932393137353C2F69643E0D0A0909090909090909093C6E616D653E726570726573656E746174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393438363933313937353C2F64617461547970653E0D0A0909090909090909093C76616C75653E5020414E442049204449414752414D3C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5237343332383831353731303C2F69643E0D0A0909090909090909093C6E616D653E726570726573656E746174696F6E3C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393438363933313937353C2F64617461547970653E0D0A0909090909090909093C76616C75653E5020414E442049204449414752414D3C2F76616C75653E0D0A0909090909090909093C636C6173734D61703E0D0A090909090909090909093C69643E72646C3A5239393438363933313937353C2F69643E0D0A090909090909090909093C6E616D653E5020414E442049204449414752414D3C2F6E616D653E0D0A090909090909090909093C6964656E746966696572733E0D0A09090909090909090909093C6964656E7469666965723E4C494E45532E5049444E554D4245523C2F6964656E7469666965723E0D0A090909090909090909093C2F6964656E746966696572733E0D0A090909090909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5231313332373238313438352830292F74706C3A5237343332383831353731303C2F706174683E0D0A0909090909090909093C2F636C6173734D61703E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5237373434333335383831383C2F69643E0D0A090909090909093C6E616D653E4E6F6D696E616C4469616D657465723C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5231363331343132373832353C2F69643E0D0A0909090909090909093C6E616D653E686173506F73736573736F723C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5239333838343139333639323C2F69643E0D0A0909090909090909093C6E616D653E6861735363616C653C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E554F4D5F4E4F4D4449414D455445523C2F70726F70657274794E616D653E0D0A0909090909090909093C76616C75654C6973744E616D653E4C656E6774683C2F76616C75654C6973744E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5235393839363835333233393C2F69643E0D0A0909090909090909093C6E616D653E686173547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5231373632323134383034333C2F64617461547970653E0D0A0909090909090909093C76616C75653E4E4F4D494E414C204449414D455445523C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5239303638383036333634383C2F69643E0D0A0909090909090909093C6E616D653E76616C56616C75653C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A73696E676C653C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E4E4F4D4449414D455445523C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5234393635383331393833333C2F69643E0D0A0909090909093C6E616D653E504C414E5420415245413C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5234313234383736373834362830292F74706C3A5236323334383032303230313C2F706174683E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234303437313034313735343C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204E414D45202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E415245413C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5239393438363933313937353C2F69643E0D0A0909090909093C6E616D653E5020414E442049204449414752414D3C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E5049444E554D4245523C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5231313332373238313438352830292F74706C3A5237343332383831353731303C2F706174683E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5233303139333338363237333C2F69643E0D0A090909090909093C6E616D653E436C61737369666965644964656E74696669636174696F6E3C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5231363839333238333035303C2F64617461547970653E0D0A0909090909090909093C76616C75653E44524157494E47204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E5049444E554D4245523C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E4C494E45533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E4C313C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5231393139323436323535303C2F69643E0D0A0909090909093C6E616D653E504950494E47204E4554574F524B2053595354454D3C2F6E616D653E0D0A0909090909093C6964656E74696669657244656C696D697465723E5F3C2F6964656E74696669657244656C696D697465723E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E415245413C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E4C494E45533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E546573743C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5239383530353931383430343C2F69643E0D0A0909090909093C6E616D653E494E535452554D454E543C2F6E616D653E0D0A0909090909093C6964656E74696669657244656C696D697465723E5F3C2F6964656E74696669657244656C696D697465723E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E494E535452554D454E54532E5441473C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E494E535452554D454E54532E5441473C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E494E535452554D454E54532E4153534F435F45513C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A090909090909093C696E6465783E313C2F696E6465783E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E494E535452554D454E54533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A093C2F67726170684D6170733E0D0A093C76616C75654C6973744D6170733E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E4C656E6774683C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E6D6D3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5235323035343237353337343C2F7572693E0D0A09090909093C6C6162656C3E4D494C4C494D45545245203C2F6C6162656C3E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E54656D70657261747572653C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E646567433C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5237343837373939323730333C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E6465674B3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5235333130393636323430353C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E50726573737572653C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E50415343414C3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5239383738373735343236373C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E4241523C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5238333439303530363535313C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E6C313C2F6E616D653E0D0A0909093C76616C75654D6170732F3E0D0A09093C2F76616C75654C6973744D61703E0D0A093C2F76616C75654C6973744D6170733E0D0A3C2F6D617070696E673E, 1, 1, 1)
INSERT [dbo].[Graphs] ([ApplicationId], [GraphId], [GraphName], [Graph], [SiteId], [Active], [ExchangeVisible]) VALUES (N'3456c504-133d-41b9-8780-7bfd73840444', N'd208b9b8-0372-4e2c-9b34-78b9bb453b61', N'abc', 0x3C6D617070696E6720786D6C6E733D22687474703A2F2F7777772E6972696E67746F6F6C732E6F72672F6D617070696E672220786D6C6E733A693D22687474703A2F2F7777772E77332E6F72672F323030312F584D4C536368656D612D696E7374616E6365223E0D0A093C67726170684D6170733E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E4C696E65733C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5231393139323436323535303C2F69643E0D0A0909090909093C6E616D653E504950494E47204E4554574F524B2053595354454D3C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E5441473C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234303437313034313735343C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204E414D45202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E5441473C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5234313234383736373834363C2F69643E0D0A090909090909093C6E616D653E506C616E7441726561486173506C616E744F626A6563743C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5237373538353332353537363C2F69643E0D0A0909090909090909093C6E616D653E706172743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5236323135373439383031323C2F69643E0D0A0909090909090909093C6E616D653E617373656D626C79547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5233303934323337383439323C2F64617461547970653E0D0A0909090909090909093C76616C75653E504C414E5420415245412048415320504C414E54204F424A4543543C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5236323334383032303230313C2F69643E0D0A0909090909090909093C6E616D653E77686F6C653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234393635383331393833333C2F64617461547970653E0D0A0909090909090909093C76616C75653E504C414E5420415245413C2F76616C75653E0D0A0909090909090909093C636C6173734D61703E0D0A090909090909090909093C69643E72646C3A5234393635383331393833333C2F69643E0D0A090909090909090909093C6E616D653E504C414E5420415245413C2F6E616D653E0D0A090909090909090909093C6964656E746966696572733E0D0A09090909090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A090909090909090909093C2F6964656E746966696572733E0D0A090909090909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5234313234383736373834362830292F74706C3A5236323334383032303230313C2F706174683E0D0A0909090909090909093C2F636C6173734D61703E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5231313332373238313438353C2F69643E0D0A090909090909093C6E616D653E50416E644944526570726573656E746174696F6E3C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323731353031333837373C2F69643E0D0A0909090909090909093C6E616D653E6F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233353239303932393137353C2F69643E0D0A0909090909090909093C6E616D653E726570726573656E746174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393438363933313937353C2F64617461547970653E0D0A0909090909090909093C76616C75653E5020414E442049204449414752414D3C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5237343332383831353731303C2F69643E0D0A0909090909090909093C6E616D653E726570726573656E746174696F6E3C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393438363933313937353C2F64617461547970653E0D0A0909090909090909093C76616C75653E5020414E442049204449414752414D3C2F76616C75653E0D0A0909090909090909093C636C6173734D61703E0D0A090909090909090909093C69643E72646C3A5239393438363933313937353C2F69643E0D0A090909090909090909093C6E616D653E5020414E442049204449414752414D3C2F6E616D653E0D0A090909090909090909093C6964656E746966696572733E0D0A09090909090909090909093C6964656E7469666965723E4C494E45532E5049444E554D4245523C2F6964656E7469666965723E0D0A090909090909090909093C2F6964656E746966696572733E0D0A090909090909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5231313332373238313438352830292F74706C3A5237343332383831353731303C2F706174683E0D0A0909090909090909093C2F636C6173734D61703E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5237373434333335383831383C2F69643E0D0A090909090909093C6E616D653E4E6F6D696E616C4469616D657465723C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5231363331343132373832353C2F69643E0D0A0909090909090909093C6E616D653E686173506F73736573736F723C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5239333838343139333639323C2F69643E0D0A0909090909090909093C6E616D653E6861735363616C653C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E554F4D5F4E4F4D4449414D455445523C2F70726F70657274794E616D653E0D0A0909090909090909093C76616C75654C6973744E616D653E4C656E6774683C2F76616C75654C6973744E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5235393839363835333233393C2F69643E0D0A0909090909090909093C6E616D653E686173547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5231373632323134383034333C2F64617461547970653E0D0A0909090909090909093C76616C75653E4E4F4D494E414C204449414D455445523C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5239303638383036333634383C2F69643E0D0A0909090909090909093C6E616D653E76616C56616C75653C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A73696E676C653C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E4E4F4D4449414D455445523C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5234393635383331393833333C2F69643E0D0A0909090909093C6E616D653E504C414E5420415245413C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5234313234383736373834362830292F74706C3A5236323334383032303230313C2F706174683E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234303437313034313735343C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204E414D45202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E415245413C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5239393438363933313937353C2F69643E0D0A0909090909093C6E616D653E5020414E442049204449414752414D3C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E5049444E554D4245523C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5231313332373238313438352830292F74706C3A5237343332383831353731303C2F706174683E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5233303139333338363237333C2F69643E0D0A090909090909093C6E616D653E436C61737369666965644964656E74696669636174696F6E3C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5231363839333238333035303C2F64617461547970653E0D0A0909090909090909093C76616C75653E44524157494E47204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E5049444E554D4245523C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E4C494E45533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E4C313C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5231393139323436323535303C2F69643E0D0A0909090909093C6E616D653E504950494E47204E4554574F524B2053595354454D3C2F6E616D653E0D0A0909090909093C6964656E74696669657244656C696D697465723E5F3C2F6964656E74696669657244656C696D697465723E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E415245413C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E4C494E45533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E546573743C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5239383530353931383430343C2F69643E0D0A0909090909093C6E616D653E494E535452554D454E543C2F6E616D653E0D0A0909090909093C6964656E74696669657244656C696D697465723E5F3C2F6964656E74696669657244656C696D697465723E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E494E535452554D454E54532E5441473C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E494E535452554D454E54532E5441473C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E494E535452554D454E54532E4153534F435F45513C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A090909090909093C696E6465783E313C2F696E6465783E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E494E535452554D454E54533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A093C2F67726170684D6170733E0D0A093C76616C75654C6973744D6170733E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E4C656E6774683C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E6D6D3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5235323035343237353337343C2F7572693E0D0A09090909093C6C6162656C3E4D494C4C494D45545245203C2F6C6162656C3E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E54656D70657261747572653C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E646567433C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5237343837373939323730333C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E6465674B3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5235333130393636323430353C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E50726573737572653C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E50415343414C3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5239383738373735343236373C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E4241523C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5238333439303530363535313C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E6C313C2F6E616D653E0D0A0909093C76616C75654D6170732F3E0D0A09093C2F76616C75654C6973744D61703E0D0A093C2F76616C75654C6973744D6170733E0D0A3C2F6D617070696E673E, 1, 1, 1)
INSERT [dbo].[Graphs] ([ApplicationId], [GraphId], [GraphName], [Graph], [SiteId], [Active], [ExchangeVisible]) VALUES (N'3456c504-133d-41b9-8780-7bfd73840444', N'ee904602-ba52-422c-a01f-932490798853', N'Lines', 0x3C6D617070696E6720786D6C6E733D22687474703A2F2F7777772E6972696E67746F6F6C732E6F72672F6D617070696E672220786D6C6E733A693D22687474703A2F2F7777772E77332E6F72672F323030312F584D4C536368656D612D696E7374616E6365223E0D0A093C67726170684D6170733E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E4C696E65733C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5231393139323436323535303C2F69643E0D0A0909090909093C6E616D653E504950494E47204E4554574F524B2053595354454D3C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E5441473C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234303437313034313735343C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204E414D45202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E5441473C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5234313234383736373834363C2F69643E0D0A090909090909093C6E616D653E506C616E7441726561486173506C616E744F626A6563743C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5237373538353332353537363C2F69643E0D0A0909090909090909093C6E616D653E706172743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5236323135373439383031323C2F69643E0D0A0909090909090909093C6E616D653E617373656D626C79547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5233303934323337383439323C2F64617461547970653E0D0A0909090909090909093C76616C75653E504C414E5420415245412048415320504C414E54204F424A4543543C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5236323334383032303230313C2F69643E0D0A0909090909090909093C6E616D653E77686F6C653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234393635383331393833333C2F64617461547970653E0D0A0909090909090909093C76616C75653E504C414E5420415245413C2F76616C75653E0D0A0909090909090909093C636C6173734D61703E0D0A090909090909090909093C69643E72646C3A5234393635383331393833333C2F69643E0D0A090909090909090909093C6E616D653E504C414E5420415245413C2F6E616D653E0D0A090909090909090909093C6964656E746966696572733E0D0A09090909090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A090909090909090909093C2F6964656E746966696572733E0D0A090909090909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5234313234383736373834362830292F74706C3A5236323334383032303230313C2F706174683E0D0A0909090909090909093C2F636C6173734D61703E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5231313332373238313438353C2F69643E0D0A090909090909093C6E616D653E50416E644944526570726573656E746174696F6E3C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323731353031333837373C2F69643E0D0A0909090909090909093C6E616D653E6F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233353239303932393137353C2F69643E0D0A0909090909090909093C6E616D653E726570726573656E746174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393438363933313937353C2F64617461547970653E0D0A0909090909090909093C76616C75653E5020414E442049204449414752414D3C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5237343332383831353731303C2F69643E0D0A0909090909090909093C6E616D653E726570726573656E746174696F6E3C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393438363933313937353C2F64617461547970653E0D0A0909090909090909093C76616C75653E5020414E442049204449414752414D3C2F76616C75653E0D0A0909090909090909093C636C6173734D61703E0D0A090909090909090909093C69643E72646C3A5239393438363933313937353C2F69643E0D0A090909090909090909093C6E616D653E5020414E442049204449414752414D3C2F6E616D653E0D0A090909090909090909093C6964656E746966696572733E0D0A09090909090909090909093C6964656E7469666965723E4C494E45532E5049444E554D4245523C2F6964656E7469666965723E0D0A090909090909090909093C2F6964656E746966696572733E0D0A090909090909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5231313332373238313438352830292F74706C3A5237343332383831353731303C2F706174683E0D0A0909090909090909093C2F636C6173734D61703E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5237373434333335383831383C2F69643E0D0A090909090909093C6E616D653E4E6F6D696E616C4469616D657465723C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5231363331343132373832353C2F69643E0D0A0909090909090909093C6E616D653E686173506F73736573736F723C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5239333838343139333639323C2F69643E0D0A0909090909090909093C6E616D653E6861735363616C653C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E554F4D5F4E4F4D4449414D455445523C2F70726F70657274794E616D653E0D0A0909090909090909093C76616C75654C6973744E616D653E4C656E6774683C2F76616C75654C6973744E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5235393839363835333233393C2F69643E0D0A0909090909090909093C6E616D653E686173547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5231373632323134383034333C2F64617461547970653E0D0A0909090909090909093C76616C75653E4E4F4D494E414C204449414D455445523C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5239303638383036333634383C2F69643E0D0A0909090909090909093C6E616D653E76616C56616C75653C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A73696E676C653C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E4E4F4D4449414D455445523C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5234393635383331393833333C2F69643E0D0A0909090909093C6E616D653E504C414E5420415245413C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5234313234383736373834362830292F74706C3A5236323334383032303230313C2F706174683E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234303437313034313735343C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204E414D45202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E415245413C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5239393438363933313937353C2F69643E0D0A0909090909093C6E616D653E5020414E442049204449414752414D3C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E5049444E554D4245523C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5231313332373238313438352830292F74706C3A5237343332383831353731303C2F706174683E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5233303139333338363237333C2F69643E0D0A090909090909093C6E616D653E436C61737369666965644964656E74696669636174696F6E3C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5231363839333238333035303C2F64617461547970653E0D0A0909090909090909093C76616C75653E44524157494E47204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E5049444E554D4245523C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E4C494E45533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E4C313C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5231393139323436323535303C2F69643E0D0A0909090909093C6E616D653E504950494E47204E4554574F524B2053595354454D3C2F6E616D653E0D0A0909090909093C6964656E74696669657244656C696D697465723E5F3C2F6964656E74696669657244656C696D697465723E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E415245413C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E4C494E45533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E546573743C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5239383530353931383430343C2F69643E0D0A0909090909093C6E616D653E494E535452554D454E543C2F6E616D653E0D0A0909090909093C6964656E74696669657244656C696D697465723E5F3C2F6964656E74696669657244656C696D697465723E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E494E535452554D454E54532E5441473C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E494E535452554D454E54532E5441473C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E494E535452554D454E54532E4153534F435F45513C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A090909090909093C696E6465783E313C2F696E6465783E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E494E535452554D454E54533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A093C2F67726170684D6170733E0D0A093C76616C75654C6973744D6170733E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E4C656E6774683C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E6D6D3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5235323035343237353337343C2F7572693E0D0A09090909093C6C6162656C3E4D494C4C494D45545245203C2F6C6162656C3E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E54656D70657261747572653C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E646567433C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5237343837373939323730333C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E6465674B3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5235333130393636323430353C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E50726573737572653C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E50415343414C3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5239383738373735343236373C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E4241523C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5238333439303530363535313C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E6C313C2F6E616D653E0D0A0909093C76616C75654D6170732F3E0D0A09093C2F76616C75654C6973744D61703E0D0A093C2F76616C75654C6973744D6170733E0D0A3C2F6D617070696E673E, 1, 1, 1)
INSERT [dbo].[Graphs] ([ApplicationId], [GraphId], [GraphName], [Graph], [SiteId], [Active], [ExchangeVisible]) VALUES (N'3456c504-133d-41b9-8780-7bfd73840444', N'a6058594-ac45-4f12-9830-b34c4ff14bb7', N'TEST', 0x3C6D617070696E6720786D6C6E733D22687474703A2F2F7777772E6972696E67746F6F6C732E6F72672F6D617070696E672220786D6C6E733A693D22687474703A2F2F7777772E77332E6F72672F323030312F584D4C536368656D612D696E7374616E6365223E0D0A093C67726170684D6170733E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E4C696E65733C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5231393139323436323535303C2F69643E0D0A0909090909093C6E616D653E504950494E47204E4554574F524B2053595354454D3C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E5441473C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234303437313034313735343C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204E414D45202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E5441473C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5234313234383736373834363C2F69643E0D0A090909090909093C6E616D653E506C616E7441726561486173506C616E744F626A6563743C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5237373538353332353537363C2F69643E0D0A0909090909090909093C6E616D653E706172743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5236323135373439383031323C2F69643E0D0A0909090909090909093C6E616D653E617373656D626C79547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5233303934323337383439323C2F64617461547970653E0D0A0909090909090909093C76616C75653E504C414E5420415245412048415320504C414E54204F424A4543543C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5236323334383032303230313C2F69643E0D0A0909090909090909093C6E616D653E77686F6C653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234393635383331393833333C2F64617461547970653E0D0A0909090909090909093C76616C75653E504C414E5420415245413C2F76616C75653E0D0A0909090909090909093C636C6173734D61703E0D0A090909090909090909093C69643E72646C3A5234393635383331393833333C2F69643E0D0A090909090909090909093C6E616D653E504C414E5420415245413C2F6E616D653E0D0A090909090909090909093C6964656E746966696572733E0D0A09090909090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A090909090909090909093C2F6964656E746966696572733E0D0A090909090909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5234313234383736373834362830292F74706C3A5236323334383032303230313C2F706174683E0D0A0909090909090909093C2F636C6173734D61703E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5231313332373238313438353C2F69643E0D0A090909090909093C6E616D653E50416E644944526570726573656E746174696F6E3C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323731353031333837373C2F69643E0D0A0909090909090909093C6E616D653E6F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233353239303932393137353C2F69643E0D0A0909090909090909093C6E616D653E726570726573656E746174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393438363933313937353C2F64617461547970653E0D0A0909090909090909093C76616C75653E5020414E442049204449414752414D3C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5237343332383831353731303C2F69643E0D0A0909090909090909093C6E616D653E726570726573656E746174696F6E3C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393438363933313937353C2F64617461547970653E0D0A0909090909090909093C76616C75653E5020414E442049204449414752414D3C2F76616C75653E0D0A0909090909090909093C636C6173734D61703E0D0A090909090909090909093C69643E72646C3A5239393438363933313937353C2F69643E0D0A090909090909090909093C6E616D653E5020414E442049204449414752414D3C2F6E616D653E0D0A090909090909090909093C6964656E746966696572733E0D0A09090909090909090909093C6964656E7469666965723E4C494E45532E5049444E554D4245523C2F6964656E7469666965723E0D0A090909090909090909093C2F6964656E746966696572733E0D0A090909090909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5231313332373238313438352830292F74706C3A5237343332383831353731303C2F706174683E0D0A0909090909090909093C2F636C6173734D61703E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5237373434333335383831383C2F69643E0D0A090909090909093C6E616D653E4E6F6D696E616C4469616D657465723C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5231363331343132373832353C2F69643E0D0A0909090909090909093C6E616D653E686173506F73736573736F723C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5239333838343139333639323C2F69643E0D0A0909090909090909093C6E616D653E6861735363616C653C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E554F4D5F4E4F4D4449414D455445523C2F70726F70657274794E616D653E0D0A0909090909090909093C76616C75654C6973744E616D653E4C656E6774683C2F76616C75654C6973744E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5235393839363835333233393C2F69643E0D0A0909090909090909093C6E616D653E686173547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5231373632323134383034333C2F64617461547970653E0D0A0909090909090909093C76616C75653E4E4F4D494E414C204449414D455445523C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5239303638383036333634383C2F69643E0D0A0909090909090909093C6E616D653E76616C56616C75653C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A73696E676C653C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E4E4F4D4449414D455445523C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5234393635383331393833333C2F69643E0D0A0909090909093C6E616D653E504C414E5420415245413C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5234313234383736373834362830292F74706C3A5236323334383032303230313C2F706174683E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234303437313034313735343C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204E414D45202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E415245413C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5239393438363933313937353C2F69643E0D0A0909090909093C6E616D653E5020414E442049204449414752414D3C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E5049444E554D4245523C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5231313332373238313438352830292F74706C3A5237343332383831353731303C2F706174683E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5233303139333338363237333C2F69643E0D0A090909090909093C6E616D653E436C61737369666965644964656E74696669636174696F6E3C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5231363839333238333035303C2F64617461547970653E0D0A0909090909090909093C76616C75653E44524157494E47204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E5049444E554D4245523C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E4C494E45533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E4C313C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5231393139323436323535303C2F69643E0D0A0909090909093C6E616D653E504950494E47204E4554574F524B2053595354454D3C2F6E616D653E0D0A0909090909093C6964656E74696669657244656C696D697465723E5F3C2F6964656E74696669657244656C696D697465723E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E415245413C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E4C494E45533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E546573743C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5239383530353931383430343C2F69643E0D0A0909090909093C6E616D653E494E535452554D454E543C2F6E616D653E0D0A0909090909093C6964656E74696669657244656C696D697465723E5F3C2F6964656E74696669657244656C696D697465723E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E494E535452554D454E54532E5441473C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E494E535452554D454E54532E5441473C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E494E535452554D454E54532E4153534F435F45513C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A090909090909093C696E6465783E313C2F696E6465783E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E494E535452554D454E54533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A093C2F67726170684D6170733E0D0A093C76616C75654C6973744D6170733E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E4C656E6774683C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E6D6D3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5235323035343237353337343C2F7572693E0D0A09090909093C6C6162656C3E4D494C4C494D45545245203C2F6C6162656C3E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E54656D70657261747572653C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E646567433C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5237343837373939323730333C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E6465674B3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5235333130393636323430353C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E50726573737572653C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E50415343414C3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5239383738373735343236373C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E4241523C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5238333439303530363535313C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E6C313C2F6E616D653E0D0A0909093C76616C75654D6170732F3E0D0A09093C2F76616C75654C6973744D61703E0D0A093C2F76616C75654C6973744D6170733E0D0A3C2F6D617070696E673E, 1, 1, 1)
/****** Object:  Table [dbo].[BindingConfig]    Script Date: 09/15/2014 12:09:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BindingConfig](
	[ApplicationId] [uniqueidentifier] NOT NULL,
	[ModuleName] [nvarchar](250) NOT NULL,
	[BindName] [nvarchar](250) NOT NULL,
	[Service] [nvarchar](650) NOT NULL,
	[Too] [nvarchar](650) NOT NULL,
	[SiteId] [int] NOT NULL,
	[Active] [tinyint] NOT NULL,
 CONSTRAINT [PK_BindingConfig] PRIMARY KEY CLUSTERED 
(
	[ApplicationId] ASC,
	[ModuleName] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ApplicationSettings]    Script Date: 09/15/2014 12:09:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ApplicationSettings](
	[ApplicationId] [uniqueidentifier] NOT NULL,
	[settingId] [int] NOT NULL,
	[settingValue] [nvarchar](550) NULL,
 CONSTRAINT [PK_ApplicationSettings] PRIMARY KEY CLUSTERED 
(
	[ApplicationId] ASC,
	[settingId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [dbo].[ApplicationSettings] ([ApplicationId], [settingId], [settingValue]) VALUES (N'3456c504-133d-41b9-8780-7bfd73840444', 1, N'10')
INSERT [dbo].[ApplicationSettings] ([ApplicationId], [settingId], [settingValue]) VALUES (N'3456c504-133d-41b9-8780-7bfd73840444', 2, N'1')
/****** Object:  Table [dbo].[Exchanges]    Script Date: 09/15/2014 12:09:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Exchanges](
	[ExchangeId] [uniqueidentifier] NOT NULL,
	[CommodityId] [uniqueidentifier] NOT NULL,
	[SourceGraphId] [uniqueidentifier] NOT NULL,
	[DestinationGraphId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](255) NULL,
	[PoolSize] [int] NOT NULL,
	[XTypeAdd] [nvarchar](25) NULL,
	[XTypeChange] [nvarchar](25) NULL,
	[XTypeSync] [nvarchar](25) NULL,
	[XTypeDelete] [nvarchar](25) NULL,
	[XTypeSetNull] [nvarchar](25) NULL,
	[SiteId] [int] NOT NULL,
	[Active] [tinyint] NOT NULL,
 CONSTRAINT [PK__Exchange__72E6008B0F975522] PRIMARY KEY CLUSTERED 
(
	[ExchangeId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [dbo].[Exchanges] ([ExchangeId], [CommodityId], [SourceGraphId], [DestinationGraphId], [Name], [Description], [PoolSize], [XTypeAdd], [XTypeChange], [XTypeSync], [XTypeDelete], [XTypeSetNull], [SiteId], [Active]) VALUES (N'ee565602-ba52-422c-a01f-932490794343', N'ee565602-ba52-422c-a01f-932490796666', N'd208b9b8-0372-4e2c-9b34-78b9bb453b61', N'ee904602-ba52-422c-a01f-932490798853', N'ABC -> DEF', NULL, 10, N'add', N'change', N'sync', N'delete', NULL, 1, 1)
/****** Object:  Table [dbo].[Dictionary]    Script Date: 09/15/2014 12:09:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Dictionary](
	[Dictionaryid] [uniqueidentifier] NOT NULL,
	[ApplicationID] [uniqueidentifier] NOT NULL,
	[IsDBDictionary] [nvarchar](50) NOT NULL,
	[PickListId] [int] NULL,
	[EnableSearch] [nvarchar](50) NULL,
	[EnableSummary] [nvarchar](50) NULL,
	[DataVersion] [nvarchar](50) NULL,
	[Provider] [nvarchar](50) NULL,
	[ConnectionString] [nvarchar](50) NULL,
	[SchemaName] [nvarchar](50) NULL,
 CONSTRAINT [PK_Dictionary] PRIMARY KEY CLUSTERED 
(
	[Dictionaryid] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RolePermissions]    Script Date: 09/15/2014 12:09:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RolePermissions](
	[RoleId] [int] NOT NULL,
	[SiteId] [int] NOT NULL,
	[PermissionId] [int] NOT NULL,
	[Active] [tinyint] NOT NULL,
 CONSTRAINT [PK_RolePermissions] PRIMARY KEY CLUSTERED 
(
	[PermissionId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (1, 1, 2, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (2, 1, 2, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (3, 1, 2, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (5, 1, 2, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (6, 1, 2, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (10, 1, 2, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (1, 1, 3, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (2, 1, 3, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (3, 1, 3, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (5, 1, 3, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (6, 1, 3, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (10, 1, 3, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (1, 1, 4, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (3, 1, 4, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (5, 1, 4, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (1, 1, 7, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (2, 1, 7, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (5, 1, 7, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (1, 1, 8, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (2, 1, 8, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (5, 1, 8, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (1, 1, 13, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (1, 1, 14, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (1, 1, 15, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (6, 1, 17, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (6, 1, 18, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (6, 1, 19, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (6, 1, 20, 1)
/****** Object:  Table [dbo].[ResourceGroups]    Script Date: 09/15/2014 12:09:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ResourceGroups](
	[ResourceId] [uniqueidentifier] NOT NULL,
	[GroupId] [int] NOT NULL,
	[SiteId] [int] NOT NULL,
	[Active] [tinyint] NOT NULL,
	[ResourceTypeId] [int] NOT NULL,
 CONSTRAINT [PK_ResourceGroups_1] PRIMARY KEY CLUSTERED 
(
	[ResourceId] ASC,
	[GroupId] ASC,
	[SiteId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'3e1e418d-fa51-403b-bb7b-0cade34283a7', 1, 1, 1, 1)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'a6ac8ca5-662c-4249-9d1f-2b7cb962f2cb', 1, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'a6ac8ca5-662c-4249-9d1f-2b7cb962f2cb', 3, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'a6ac8ca5-662c-4249-9d1f-2b7cb962f2cb', 4, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'a6ac8ca5-662c-4249-9d1f-2b7cb962f2cb', 5, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'a6ac8ca5-662c-4249-9d1f-2b7cb962f2cb', 6, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'c274b3fb-1794-4bc2-bcab-3615fea7f11f', 1, 3, 1, 4)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'6914f8d4-636f-4d9f-aa43-45f865de56b6', 1, 1, 1, 2)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'9614f8d4-636f-4d9f-aa43-45f865de56b7', 3, 1, 1, 2)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'6f9f8981-55f3-433c-a86c-4b1262ff2efc', 1, 1, 1, 1)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'504ff982-740d-45a6-95d8-6f6ba425fd40', 1, 1, 1, 2)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'54dc03c6-b1ed-4a4e-b4d9-773d4c2651f8', 1, 1, 1, 1)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'd208b9b8-0372-4e2c-9b34-78b9bb453b61', 3, 1, 1, 4)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'7877c504-133d-41b9-8680-7bfd73840435', 1, 1, 1, 3)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'3456c504-133d-41b9-8780-7bfd73840444', 3, 1, 1, 3)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', 1, 1, 1, 1)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', 3, 1, 1, 1)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'ee565602-ba52-422c-a01f-932490794343', 1, 1, 1, 6)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'ee565602-ba52-422c-a01f-932490796666', 1, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'ee904602-ba52-422c-a01f-932490798853', 1, 1, 1, 4)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'd45851cd-7662-44d4-8339-97ab64d47771', 1, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'd45851cd-7662-44d4-8339-97ab64d47771', 3, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'd45851cd-7662-44d4-8339-97ab64d47771', 4, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'd45851cd-7662-44d4-8339-97ab64d47771', 5, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'5ddc49b3-1599-47e2-9f9a-ad37413d28a7', 1, 1, 1, 1)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'e61137eb-fd18-4c31-93a1-d0c39bd1001a', 3, 1, 1, 1)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'83e6e521-1a27-4e2b-8dc0-fe3f021a26de', 1, 1, 1, 3)
/****** Object:  Table [dbo].[UserGroups]    Script Date: 09/15/2014 12:09:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserGroups](
	[UserGroupId] [int] IDENTITY(1,1) NOT NULL,
	[GroupId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[SiteId] [int] NOT NULL,
	[UserGroupsDesc] [nvarchar](255) NULL,
	[Active] [tinyint] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[UserGroupId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[UserGroups] ON
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (1, 1, 1, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (2, 1, 3, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (3, 1, 4, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (4, 1, 38, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (5, 4, 39, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (6, 5, 39, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (7, 6, 39, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (8, 1, 32, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (9, 3, 32, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (10, 4, 6, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (11, 5, 6, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (12, 6, 6, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (13, 7, 6, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (14, 9, 2, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (15, 9, 3, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (16, 9, 4, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (17, 9, 5, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (18, 3, 2, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (19, 3, 4, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (20, 4, 4, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (21, 5, 4, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (22, 4, 7, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (23, 5, 7, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (24, 6, 7, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (25, 5, 3, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (26, 5, 5, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (27, 3, 6, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (28, 10, 2, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (29, 10, 4, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (30, 3, 5, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (31, 4, 1, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (32, 4, 3, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (33, 4, 5, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (34, 1, 6, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (35, 1, 39, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (36, 1, 5, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (37, 5, 2, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (38, 11, 1, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (39, 11, 2, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (40, 11, 3, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (41, 1, 2, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (42, 3, 3, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (43, 4, 32, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (44, 3, 1, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (45, 6, 1, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (46, 3, 38, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (47, 3, 39, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (48, 3, 8, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (49, 9, 1, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (50, 7, 1, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (51, 3, 40, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (52, 5, 32, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (53, 6, 32, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (54, 7, 32, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (55, 8, 32, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (56, 9, 32, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (57, 1, 41, 1, NULL, 1)
SET IDENTITY_INSERT [dbo].[UserGroups] OFF
/****** Object:  StoredProcedure [dbo].[spgApplication]    Script Date: 09/15/2014 12:09:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <11-June-2014>
-- Description:	<Selecting Application>

-- =============================================
CREATE  PROCEDURE [dbo].[spgApplication] 
(
	@ScopeInternalName NVARCHAR(255),
	@SiteId INT
)
 
AS
BEGIN TRY
	BEGIN
		
		DECLARE @ContextId uniqueidentifier
		
		SELECT @ContextId=Contextid FROM Contexts WHERE InternalName=@ScopeInternalName AND SiteId=@Siteid
		
		IF @ContextId IS NOT NULL
			BEGIN
				SELECT InternalName,DisplayName,[Description],DXFRUrl FROM Applications
					WHERE ContextId=@ContextId AND SiteId=@Siteid AND Active=1
					
			END
		ELSE
			BEGIN
				SELECT 'Scope Not Found'
			END
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spgAllUser]    Script Date: 09/15/2014 12:09:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <24-June-2014>
-- Description:	<Getting All User >
-- =============================================
CREATE PROCEDURE [dbo].[spgAllUser]

	 
AS
BEGIN TRY
	BEGIN
		SELECT UserId, UserName, SiteId, UserFirstName, UserLastName,UserLastName +','+ UserFirstName AS UserFullName, UserEmail, UserPhone, UserDesc, Active FROM [Users]
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spgAllGroups ]    Script Date: 09/15/2014 12:09:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <17-June-2014>
-- Description:	<Getting All Groups>

-- =============================================
create  PROCEDURE [dbo].[spgAllGroups ] 
 
AS
BEGIN TRY
	BEGIN
		SELECT GroupId,SiteId,GroupName,GroupDesc FROM Groups WHERE Active=1
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spdUser]    Script Date: 09/15/2014 12:09:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <07-Jul-2014>
-- Description:	<Deleting user>

-- =============================================
CREATE PROCEDURE [dbo].[spdUser] 
(
	@UserName NVARCHAR(100),
	@SiteId INT
)	
	 
AS
BEGIN TRY
	BEGIN
		
		UPDATE Users SET Active = 0 WHERE UserName = @UserName AND SiteId = @SiteId
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spdRoles]    Script Date: 09/15/2014 12:09:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <07-Jul-2014>
-- Description:	<Deleting Roles>

-- =============================================
CREATE PROCEDURE [dbo].[spdRoles] 
(
	@RoleId INT,
	@SiteId INT
)	
	 
AS
BEGIN TRY
	BEGIN
		
		UPDATE Roles SET Active = 0 WHERE RoleId= @RoleId AND SiteId = @SiteId
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spdPermissions]    Script Date: 09/15/2014 12:09:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Gagan Dhamija>
-- Create date: <09-Jul-2014>
-- Description:	<Deleting Permissions>

-- =============================================
Create PROCEDURE [dbo].[spdPermissions] 
(
	@PermissionId INT,
	@SiteId INT
)	
	 
AS
BEGIN TRY
	BEGIN
		
		UPDATE [Permissions] SET Active = 0 WHERE PermissionId= @PermissionId AND SiteId = @SiteId
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spdGroups]    Script Date: 09/15/2014 12:09:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Gagan Dhamija>
-- Create date: <10-Jul-2014>
-- Description:	<Deleting Groups>

-- =============================================
CREATE PROCEDURE [dbo].[spdGroups] 
(
	@GroupId INT,
	@SiteId INT
)	
	 
AS
BEGIN TRY
	BEGIN
		
		UPDATE Groups SET Active = 0 WHERE GroupId= @GroupId AND SiteId = @SiteId
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spgGroups]    Script Date: 09/15/2014 12:09:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <17-June-2014>
-- Description:	<Getting All Groups>

-- =============================================
CREATE  PROCEDURE [dbo].[spgGroups] 
(
	@GroupId INT
)
 
AS
BEGIN TRY
	BEGIN
		SELECT GroupId,SiteId,GroupName,GroupDesc FROM Groups WHERE Active=1 AND GroupId = @GroupId
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spgPermissions]    Script Date: 09/15/2014 12:09:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Prashant Dubey>
--Modified by Atul Srivastava - Adding Site id paramenter
-- Create date: <27-June-2014>
-- Description:	<Getting Permissions>
-- =============================================
CREATE PROCEDURE [dbo].[spgPermissions]
(
	@SiteId INT
)
	 
AS
BEGIN TRY
	BEGIN
		SELECT PermissionId,PermissionName,PermissionDesc FROM Permissions WHERE Active=1 AND SiteId= @SiteId
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spgPermissionRoles]    Script Date: 09/15/2014 12:10:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <02-Jul-2014>
-- Description:	<Return permission based on role id and permission id>

-- =============================================
CREATE PROCEDURE [dbo].[spgPermissionRoles] 
(
	@RoleId INT,
	@PermissionId INT,
	@Siteid INT
)	
	 
AS
BEGIN TRY
	BEGIN
		SELECT PermissionId,PermissionName,PermissionDesc FROM [Permissions] P
			INNER JOIN Roles R ON R.SiteId = P.SiteId
		WHERE R.RoleId = @RoleId AND P.Active =1 AND R.Active=1 AND P.PermissionId = @PermissionId
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spgRolesById]    Script Date: 09/15/2014 12:10:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <17-June-2014>
-- Description:	<Getting Role by specific id details,,>
-- =============================================
CREATE PROCEDURE [dbo].[spgRolesById]
(
	@RoleId INT
)
	 
AS
BEGIN TRY
	BEGIN
		SELECT RoleId,SiteId, RoleName,RoleDesc FROM Roles WHERE Active=1 AND RoleId =@RoleId
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spgRoles]    Script Date: 09/15/2014 12:10:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <17-June-2014>
-- Description:	<Getting Site details,,>
-- =============================================
CREATE PROCEDURE [dbo].[spgRoles]
	 
AS
BEGIN TRY
	BEGIN
		SELECT RoleId,SiteId, RoleName,RoleDesc   FROM Roles WHERE Active=1
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spgRolePermissions]    Script Date: 09/15/2014 12:10:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <02-Jul-2014>
-- Description:	<Return permission based on role>

-- =============================================
CREATE PROCEDURE [dbo].[spgRolePermissions] 
(
	@RoleId INT,
	@Siteid INT
)	
	 
AS
BEGIN TRY
	BEGIN
		SELECT PermissionId,PermissionName,PermissionDesc FROM [Permissions] P
			INNER JOIN Roles R ON R.SiteId = P.SiteId
		WHERE R.RoleId = @RoleId AND P.Active =1 AND R.Active=1
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spgSiteRoles]    Script Date: 09/15/2014 12:10:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <18-June-2014>
-- Description:	<Getting Site Roles,,>
-- =============================================
CREATE PROCEDURE [dbo].[spgSiteRoles]
(
	@SiteId INT
)
	 
AS
BEGIN TRY
	BEGIN
		SELECT RoleName,RoleDesc FROM Roles WHERE Active=1 AND SiteId=@SiteId
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber, ERROR_MESSAGE() as ErrorMessage
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spgSiteGroups]    Script Date: 09/15/2014 12:10:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =================================================================
-- Author:		<Atul Srivastava>
-- Create date: <27-June-2014>
-- Description:	<Getting site groups based on site id

-- ==================================================================
CREATE PROCEDURE [dbo].[spgSiteGroups] 
(
	@SiteId INT
)	
	 
AS
BEGIN TRY
	BEGIN
		SELECT GroupId,SiteId,GroupName,GroupDesc FROM Groups WHERE Active =1 AND SiteId =@SiteId
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spuApplication]    Script Date: 09/15/2014 12:10:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <11-June-2014>
-- Description:	<Updating Application details>

-- =============================================
CREATE PROCEDURE [dbo].[spuApplication] 
(
	@ScopeInternalName NVARCHAR(255),
	@AppDisplayName NVARCHAR(255),
	@AppInternalName NVARCHAR(255),
	@Description NVARCHAR(255),
	@DXFRUrl NVARCHAR(255),
	@SiteId INT
)	
	 
AS
BEGIN TRY
	BEGIN
		DECLARE @ContextId uniqueidentifier
			
		SELECT @ContextId=Contextid FROM Contexts WHERE InternalName=@ScopeInternalName AND SiteId=@SiteId
		
		IF @ContextId IS NOT NULL
		BEGIN
			UPDATE Applications
				SET DisplayName =@AppDisplayName,
				InternalName=@AppInternalName, 
				Description=@Description,
				DXFRUrl=@DXFRUrl,
				SiteId=@SiteId
			WHERE InternalName = @AppInternalName AND SiteId=@SiteId
				
		END
		ELSE
			SELECT 'Scope Not Found'
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spgUserById]    Script Date: 09/15/2014 12:10:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <18-June-2014>
-- Description:	<Getting User by specific user id >
-- =============================================
CREATE PROCEDURE [dbo].[spgUserById]
(
	@UserId INT
)
	 
AS
BEGIN TRY
	BEGIN
		SELECT UserId,UserName,SiteId,UserFirstName,UserLastName,UserLastName,UserEmail,UserPhone,UserDesc  FROM Users WHERE Active=1 AND UserId =@UserId
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spgSiteUsers]    Script Date: 09/15/2014 12:10:04 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <30-June-2014>
-- Description:	<Getting Users based on site id>
-- =============================================
CREATE PROCEDURE [dbo].[spgSiteUsers]
(
	@SiteId INT
)
	 
AS
BEGIN TRY
	BEGIN
		
		SELECT  U.UserId,U.UserFirstName ,U.UserLastName,U.UserEmail,U.UserPhone,U.UserDesc FROM Users U
		WHERE U.Active=1 AND U.SiteId = @SiteId
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spiApplication]    Script Date: 09/15/2014 12:10:04 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <10-June-2014>
-- Description:	<Inserting Application details>

-- =============================================
CREATE PROCEDURE [dbo].[spiApplication] 
(
	@ScopeInternalName NVARCHAR(255),
	@AppDisplayName NVARCHAR(255),
	@AppInternalName NVARCHAR(255),
	@Description NVARCHAR(255),
	@DXFRUrl NVARCHAR(255),
	@SiteId INT
)	
	 
AS
BEGIN TRY
	BEGIN
		DECLARE @ContextId uniqueidentifier
			
		SELECT @ContextId=Contextid FROM Contexts WHERE InternalName=@ScopeInternalName AND SiteId=@SiteId AND Active=1
		
		IF @ContextId IS NOT NULL
		BEGIN
			INSERT INTO Applications(ContextId,DisplayName,InternalName, Description,DXFRUrl,SiteId)
				VALUES(@ContextId,@AppDisplayName,@AppInternalName,@Description,@DXFRUrl,@SiteId)
		END
		ELSE
			SELECT 'Scope Not Found'
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spiGraph_ATUL]    Script Date: 09/15/2014 12:10:05 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <16-June-2014>
-- Description:	<Inserting Graph details>

-- =============================================
CREATE PROCEDURE [dbo].[spiGraph_ATUL] 
(
	@ScopeInternalName NVARCHAR(255),
	@AppInternalName NVARCHAR(255),
	@GraphName NVARCHAR(255),
	@Graph VARBINARY(MAX),
	@SiteId INT,
	@MappingFilePath NVARCHAR(255)
	
)	
	 
AS
BEGIN TRY
	BEGIN
		DECLARE @ContexId uniqueidentifier
		DECLARE @ApplicationId uniqueidentifier
		SELECT @ContexId = ContextId FROM Contexts WHERE InternalName = @ScopeInternalName AND SiteId=@SiteId
			
		SELECT @ApplicationId=ApplicationId  FROM Applications WHERE ContextId=@ContexId AND InternalName=@AppInternalName AND SiteId=@SiteId
		
		IF @ApplicationId IS NOT NULL
		BEGIN
			
		
		DECLARE @SQL NVARCHAR(MAX)
		SET @SQL =''
		SET @SQL='INSERT INTO Graphs
				(ApplicationId,GraphName,SiteId,Graph)' +
				'SELECT ' + ''''+ CONVERT(NVARCHAR(MAX),@ApplicationId) +''''+ ',' + ''''+ CONVERT(NVARCHAR(MAX),@GraphName) +'''' + ','+ ''''+CONVERT(NVARCHAR(MAX),@SiteId ) +'''' +
				 ',BulkColumn FROM OPENROWSET(Bulk' + '''' + CONVERT(NVARCHAR(MAX),@MappingFilePath )+'''' + ', SINGLE_BLOB)AS Blob'
						
		print @SQL
		EXEC(@SQL) 
		-----EXEC [spiGraph] 'iName','testApp Iname','TEST',1,'C:\3.0\iRINGTools.Services\App_Data\iabc.config'
		--INSERT INTO Graphs
		--		(ApplicationId,GraphName,SiteId,Graph)
		--		SELECT @AppInternalName, 'test graph',1,
		--			BulkColumn FROM OPENROWSET(
		--				Bulk @MappingFilePath, SINGLE_BLOB)AS Blob
				
		END
		ELSE
			SELECT 'Application Not Found'
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spiGroups]    Script Date: 09/15/2014 12:10:05 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <07-Jul-2014>
-- Description:	<Deleting Roles> 

-- =============================================
CREATE PROCEDURE [dbo].[spiGroups] 
(
	@SiteId INT,
	@GroupName NVARCHAR(100),
	@GroupDesc NVARCHAR(100)
)
	 
AS
BEGIN TRY
	BEGIN
	
		INSERt INTO  Groups (SiteId,GroupName,GroupDesc )
			VALUES (@SiteId,@GroupName,@GroupDesc)
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spiRole]    Script Date: 09/15/2014 12:10:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <03-Jul-2014>
-- Description:	<Inserting Roles>

-- =============================================
CREATE PROCEDURE [dbo].[spiRole] 
(
	@SiteId INT,
	@RoleName NVARCHAR(100),
	@RoleDesc NVARCHAR(100)
	
)	
	 
AS
BEGIN TRY
	BEGIN
		INSERT INTO Roles (SiteId,RoleName,RoleDesc)
		VALUES(@SiteId,@RoleName,@RoleDesc)
	
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spiPermissions]    Script Date: 09/15/2014 12:10:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <07-Jul-2014>
-- Description:	<Deleting Roles> 

-- =============================================
CREATE PROCEDURE [dbo].[spiPermissions] 
(
	@SiteId INT,
	@PermissionName NVARCHAR(100),
	@PermissionDesc NVARCHAR(100)
)
	 
AS
BEGIN TRY
	BEGIN
	
		INSERt INTO  [Permissions] (SiteId,PermissionName,PermissionDesc)
			VALUES (@SiteId,@PermissionName,@PermissionDesc)
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spiUser]    Script Date: 09/15/2014 12:10:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <26-June-2014>
-- Description:	<Inserting user details>

-- =============================================
CREATE PROCEDURE [dbo].[spiUser] 
(
	@SiteId INT,
	@UserName NVARCHAR(100),
	@UserFirstName  NVARCHAR(50),
	@UserLastName NVARCHAR(50),
	@UserEmail NVARCHAR(50),
	@UserPhone NVARCHAR(50),
	@UserDesc  NVARCHAR(255)
)	
	 
AS
BEGIN TRY
	BEGIN
		INSERT INTO Users (SiteId,UserName,UserFirstName,UserLastName,UserEmail,UserPhone,UserDesc)
		VALUES(@SiteId,@UserName,@UserFirstName,@UserLastName,@UserEmail,@UserPhone,@UserDesc)
	
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spdApplication]    Script Date: 09/15/2014 12:10:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <11-June-2014>
-- Description:	<Updating Application details>

-- =============================================
CREATE PROCEDURE [dbo].[spdApplication] 
(
	@ScopeInternalName NVARCHAR(255),
	@AppInternalName NVARCHAR(255),
	@Siteid INT
)	
	 
AS
BEGIN TRY
	BEGIN
		DECLARE @ContextId uniqueidentifier
		SELECT @ContextId=Contextid FROM Contexts WHERE InternalName=@ScopeInternalName AND SiteId=@Siteid
		IF @ContextId IS NOT NULL
		BEGIN
			UPDATE Applications
				SET Active=0
			WHERE ContextId=@ContextId AND InternalName=@AppInternalName AND SiteId=@Siteid
				
		END
		ELSE
			SELECT 'Scope Not Found'
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spuRoles]    Script Date: 09/15/2014 12:10:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <07-Jul-2014>
-- Description:	<Deleting Roles>

-- =============================================
CREATE PROCEDURE [dbo].[spuRoles] 
(
	@RoleId INT,
	@SiteId INT,
	@RoleName NVARCHAR(100),
	@RoleDesc NVARCHAR(100)
)
	 
AS
BEGIN TRY
	BEGIN
	
		UPDATE Roles 
			SET SiteId = @SiteId,
				RoleName =@RoleName,
				RoleDesc = 	@RoleDesc
		WHERE RoleId= @RoleId AND SiteId = @SiteId
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spuPermissions]    Script Date: 09/15/2014 12:10:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Gagan Dhamija>
-- Create date: <08-Jul-2014>
-- Description:	<Update Permissions>

-- =============================================
Create PROCEDURE [dbo].[spuPermissions] 
(
	@SiteId INT,
	@PermissionId INT,
	@PermissionName NVARCHAR(100),
	@PermissionDesc NVARCHAR(100)
)
	 
AS
BEGIN TRY
	BEGIN
	
		UPDATE Permissions 
			SET PermissionName = @PermissionName,
				PermissionDesc = @PermissionDesc
		  WHERE	SiteId = @SiteId and PermissionId = @PermissionId
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spuGroups]    Script Date: 09/15/2014 12:10:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Gagan Dhamija>
-- Create date: <10-Jul-2014>
-- Description:	<Update Groups>

-- =============================================
CREATE PROCEDURE [dbo].[spuGroups] 
(
	@SiteId INT,
	@GroupId INT,
	@GroupName NVARCHAR(100),
	@GroupDesc NVARCHAR(100)
)
	 
AS
BEGIN TRY
	BEGIN
	
		UPDATE Groups 
			SET GroupName = @GroupName,
				GroupDesc = @GroupDesc
		  WHERE	SiteId = @SiteId and GroupId = @GroupId
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spuUser]    Script Date: 09/15/2014 12:10:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================  
-- Author:  <Atul Srivastava>  
-- Create date: <07-Jul-2014>  
-- Description: <Inserting user details>  
  
-- =============================================  
CREATE PROCEDURE [dbo].[spuUser]   
(  
 @UserId INT,  --[[Deepak kumar uncommented userid although not used here but sent from service]]
 @SiteId INT,  
 @UserName NVARCHAR(100),  
 @UserFirstName  NVARCHAR(50),  
 @UserLastName NVARCHAR(50),  
 @UserEmail NVARCHAR(50),  
 @UserPhone NVARCHAR(50),  
 @UserDesc  NVARCHAR(255)  
)   
    
AS  
BEGIN TRY  
 BEGIN  
  UPDATE Users SET  
  SiteId =@SiteId,  
  UserName = @UserName,  
  UserFirstName = @UserFirstName,  
  UserLastName =@UserLastName,  
  UserEmail = @UserEmail,  
  UserPhone = @UserPhone,  
  UserDesc = @UserDesc WHERE UserName = @UserName AND Active =1 AND SiteId = @SiteId  
   
 END  
END TRY  
  
BEGIN CATCH  
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage  
END CATCH
GO
/****** Object:  Table [dbo].[ValueListMap]    Script Date: 09/15/2014 12:10:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ValueListMap](
	[ApplicationId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](250) NOT NULL,
	[Label] [nvarchar](250) NOT NULL,
	[internalValue] [nvarchar](250) NOT NULL,
	[Uri] [nvarchar](550) NULL,
PRIMARY KEY CLUSTERED 
(
	[ApplicationId] ASC,
	[Name] ASC,
	[Label] ASC,
	[internalValue] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [dbo].[ValueListMap] ([ApplicationId], [Name], [Label], [internalValue], [Uri]) VALUES (N'3456c504-133d-41b9-8780-7bfd73840444', N'Length', N'KiloMeter', N'KM', N'rdl:R52054275654')
INSERT [dbo].[ValueListMap] ([ApplicationId], [Name], [Label], [internalValue], [Uri]) VALUES (N'3456c504-133d-41b9-8780-7bfd73840444', N'Length', N'Meter', N'm', N'rdl:R52054275344')
INSERT [dbo].[ValueListMap] ([ApplicationId], [Name], [Label], [internalValue], [Uri]) VALUES (N'3456c504-133d-41b9-8780-7bfd73840444', N'Length', N'MILLIMETER', N'mm', N'rdl:R52054275374')
INSERT [dbo].[ValueListMap] ([ApplicationId], [Name], [Label], [internalValue], [Uri]) VALUES (N'3456c504-133d-41b9-8780-7bfd73840444', N'Temparature', N'degree', N'deg', N'rdl:R520542757610')
INSERT [dbo].[ValueListMap] ([ApplicationId], [Name], [Label], [internalValue], [Uri]) VALUES (N'3456c504-133d-41b9-8780-7bfd73840444', N'Temparature', N'Faren heat', N'f', N'rdl:R52054275777')
/****** Object:  StoredProcedure [dbo].[spuCommodity]    Script Date: 09/15/2014 12:10:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ================================
-- Author:		<Deepak Kumar>
-- Create date: <11-Sep-2014>
-- Description:	<update commodity>
-- ================================
CREATE PROCEDURE [dbo].[spuCommodity] --'vmallire',1,'A6AC8CA5-662C-4249-9D1F-2B7CB962F2CB','dee1ii','4,5,1,6'
(
	@UserName varchar(100),--'vmallire'
	@SiteId INT,--1
	@CommodityId uniqueidentifier,
	@CommodityName nvarchar(255),
	@GroupList varchar(1000)
	
)	 
AS

	BEGIN
	
	
	
	Update Commodity
		Set CommodityName = @CommodityName
	Where CommodityId = @CommodityId 
			And Active = 1


	/*Below query can not be included in merge statement
	as it will inactive all the non matching rows, hence
	separate query has written*/
	Update ResourceGroups
	Set   Active = 0
	Where ResourceId = @CommodityId 
	
	Declare @ResourceTypeId int = 5 --See ResourceType for detail
	
	MERGE ResourceGroups AS T      
	USING 
	(
		--this query is making sure that only those groups will be considered which are
		--assigned to this user
		Select ug.GroupId from Users u inner join UserGroups ug on u.UserId = ug.UserId
								And u.UserName = @UserName
								And u.SiteId = @SiteId
								And u.Active = 1
								And ug.Active = 1
								And ug.SiteId = @SiteId  
							inner join dbo.Split(@GroupList,',') tg on ug.GroupId = tg.Items
	
	)AS S  (GroupId)    
	ON (T.GroupId = S.GroupId AND T.SiteId = @SiteId AND T.ResourceId = @CommodityId)       
	WHEN NOT MATCHED BY TARGET       
		THEN INSERT(ResourceId,GroupId,SiteId,ResourceTypeId) VALUES(@CommodityId,S.GroupId,@SiteId,@ResourceTypeId)
	WHEN MATCHED       
		THEN UPDATE SET T.SiteId = @SiteId,  
		T.Active = 1;    

		 
	END
GO
/****** Object:  StoredProcedure [dbo].[spuGraphs]    Script Date: 09/15/2014 12:10:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Gagan Dhamija>
-- Create date: <21-July-2014>
-- Description:	<Updating Graphs details>

-- =============================================
CREATE PROCEDURE [dbo].[spuGraphs] 
(
	@GraphId uniqueidentifier,
	@GraphName nvarchar(255),
	@Graph varbinary(MAX),
	@SiteId int
)	
	 
AS
BEGIN TRY
	BEGIN
		UPDATE Graphs
			SET GraphName = @GraphName,
				Graph = @Graph 
			WHERE GraphId = @GraphId AND SiteId=@SiteId
				
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spuExchange]    Script Date: 09/15/2014 12:10:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Deepak Kumar>
-- Create date: <10-Sep-2014>
-- Description:	<updating Exchange>
-- ===========================================================
CREATE PROCEDURE [dbo].[spuExchange] 
(

	@UserName varchar(100),--'vmallire'
	@ExchangeId uniqueidentifier,
	@SourceGraphId	uniqueidentifier,
	@DestinationGraphId	uniqueidentifier,
	@Name	nvarchar(255),
	@Description	nvarchar(255),
	@PoolSize	int,
	@XTypeAdd	nvarchar(25),
	@XTypeChange	nvarchar(25),
	@XTypeSync	nvarchar(25),
	@XTypeDelete	nvarchar(25),
	@XTypeSetNull	nvarchar(25),
	@SiteId	int,
	@GroupList varchar(1000)
	
)	 
AS

	BEGIN
	
	--select * from ResourceType
	
	UPDATE [Exchanges]
	   SET [ExchangeId] = @ExchangeId
		  ,[SourceGraphId] = @SourceGraphId
		  ,[DestinationGraphId] = @DestinationGraphId
		  ,[Name] = @Name
		  ,[Description] = @Description
		  ,[PoolSize] = @PoolSize
		  ,[XTypeAdd] = @XTypeAdd
		  ,[XTypeChange] = @XTypeChange
		  ,[XTypeSync] = @XTypeSync
		  ,[XTypeDelete] = @XTypeDelete
		  ,[XTypeSetNull] = @XTypeSetNull
		  ,[SiteId] = @SiteId
	 WHERE ExchangeId = @ExchangeId and Active = 1



	/*Below query can not be included in merge statement
	as it will inactive all the non matching rows, hence
	separate query has written*/
	Update ResourceGroups
	Set   Active = 0
	Where ResourceId = @ExchangeId 

	
	Declare @ResourceTypeId int = 6 --See ResourceType for detail
	

	
	MERGE ResourceGroups AS T      
	USING 
	(
		--this query is making sure that only those groups will be considered which are
		--assigned to this user
		Select ug.GroupId from Users u inner join UserGroups ug on u.UserId = ug.UserId
								And u.UserName = @UserName
								And u.SiteId = @SiteId
								And u.Active = 1
								And ug.Active = 1
								And ug.SiteId = @SiteId  
							inner join dbo.Split(@GroupList,',') tg on ug.GroupId = tg.Items
	
	)AS S  (GroupId)    
	ON (T.GroupId = S.GroupId AND T.SiteId = @SiteId AND T.ResourceId = @ExchangeId)       
	WHEN NOT MATCHED BY TARGET       
		THEN INSERT(ResourceId,GroupId,SiteId,ResourceTypeId) VALUES(@ExchangeId,S.GroupId,@SiteId,@ResourceTypeId)
	WHEN MATCHED       
		THEN UPDATE SET T.SiteId = @SiteId,  
		T.Active = 1;    
							

	END
GO
/****** Object:  StoredProcedure [dbo].[spiUserGroups]    Script Date: 09/15/2014 12:10:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================    
-- Author:  <Deepak Kumar>    
-- Create date: <21-Aug-2014>    
-- Description: <Inserting Groups for user, as 
-- one user can lie in multiple groups>    
-- =============================================    
CREATE PROCEDURE [dbo].[spiUserGroups]-- ''    
(    
 @rawXML xml,    
 @SiteId int    
)     
      
AS    
Begin    
SET NOCOUNT ON;    
    
--Set @rawXML = '<userGroups>    
--  <userGroup>    
--    <userGroupId>20</userGroupId>    
--    <groupId>5</groupId>    
--    <userId>39</userId>    
--    <siteId>1</siteId>    
--    <userGroupsDesc>dfssaa</userGroupsDesc>    
--    <active>1</active>    
--  </userGroup>    
--</userGroups>'    
    
      
SELECT       
 nref.value('groupId[1]', 'int') GroupId,      
 nref.value('userId[1]', 'int') UserId--,      
 --nref.value('userGroupsDesc[1]', 'nvarchar(255)') UserGroupsDesc      
INTO #Source FROM   @rawXML.nodes('//userGroup') AS R(nref)      
      
UPDATE t    
SET t.Active = 0    
FROM UserGroups t INNER JOIN #Source s ON t.UserId = s.UserId    
      
      
MERGE [UserGroups] AS T      
USING #Source AS S      
ON (T.GroupId = S.GroupId AND T.UserId = S.UserId)       
WHEN NOT MATCHED BY TARGET       
    THEN INSERT([GroupId],[UserId],[SiteId]) VALUES(S.[GroupId],S.[UserId],@SiteId)      
WHEN MATCHED       
    THEN UPDATE SET T.SiteId = @SiteId,  
    --T.UserGroupsDesc = S.UserGroupsDesc,   
    T.Active = 1;     
End
GO
/****** Object:  StoredProcedure [dbo].[spiRolePermissions]    Script Date: 09/15/2014 12:10:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================    
-- Author:  <Deepak Kumar>    
-- Create date: <26-Aug-2014>    
-- Description: <mapping role with permissions>    
-- =============================================    
CREATE PROCEDURE [dbo].[spiRolePermissions]-- ''    
(    
 @rawXML xml,    
 @SiteId int    
)     
      
AS    
Begin    
SET NOCOUNT ON;    
    
--Set @rawXML = ''    
    
      
SELECT       
 nref.value('roleId[1]', 'int') RoleId,      
 nref.value('permissionId[1]', 'int') PermissionId--,      
INTO #Source FROM   @rawXML.nodes('//rolePermission') AS R(nref)      
      
UPDATE t    
SET t.Active = 0    
FROM RolePermissions t INNER JOIN #Source s ON t.RoleId = s.RoleId
      
      
MERGE [RolePermissions] AS T      
USING #Source AS S      
ON (T.RoleId = S.RoleId AND T.PermissionId = S.PermissionId )       
WHEN NOT MATCHED BY TARGET       
    THEN INSERT([RoleId],[PermissionID],[SiteId]) VALUES(S.[RoleId],S.[PermissionID],@SiteId)      
WHEN MATCHED       
    THEN UPDATE SET T.SiteId = @SiteId,  
    T.Active = 1;     
End
GO
/****** Object:  StoredProcedure [dbo].[spiRoleGroups]    Script Date: 09/15/2014 12:10:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================    
-- Author:  <Deepak Kumar>    
-- Create date: <22-Aug-2014>    
-- Description: <Inserting Groups in a role>    
-- =============================================    
CREATE PROCEDURE [dbo].[spiRoleGroups]-- ''    
(    
 @rawXML xml,    
 @SiteId int    
)     
      
AS    
Begin    
SET NOCOUNT ON;    
    
--Set @rawXML = '<groupRoles xmlns1="http://www.iringtools.org/library" xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
--  <groupRole>
--    <groupId>1</groupId>
--    <roleId>1</roleId>
--  </groupRole>
--  <groupRole>
--    <groupId>3</groupId>
--    <roleId>1</roleId>
--  </groupRole>
--  <groupRole>
--    <groupId>4</groupId>
--    <roleId>1</roleId>
--  </groupRole>
--  <groupRole>
--    <groupId>5</groupId>
--    <roleId>1</roleId>
--  </groupRole>
--</groupRoles>'    
    
      
SELECT       
 nref.value('groupId[1]', 'int') GroupId,      
 nref.value('roleId[1]', 'int') RoleId--,      
INTO #Source FROM   @rawXML.nodes('//groupRole') AS R(nref)      
      
UPDATE t    
SET t.Active = 0    
FROM GroupRoles t INNER JOIN #Source s ON t.RoleId = s.RoleId
      
      
MERGE [GroupRoles] AS T      
USING #Source AS S      
ON (T.RoleId = S.RoleId AND T.GroupId = S.GroupId )       
WHEN NOT MATCHED BY TARGET       
    THEN INSERT([GroupId],[RoleId],[SiteId]) VALUES(S.[GroupId],S.[RoleId],@SiteId)      
WHEN MATCHED       
    THEN UPDATE SET T.SiteId = @SiteId,  
    --T.UserGroupsDesc = S.UserGroupsDesc,   
    T.Active = 1;     
End
GO
/****** Object:  StoredProcedure [dbo].[spiGroupUsers]    Script Date: 09/15/2014 12:10:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================  
-- Author:  <Deepak Kumar>  
-- Create date: <19-Aug-2014>  
-- Description: <Inserting Group users>  
-- =============================================  
CREATE PROCEDURE [dbo].[spiGroupUsers]-- ''  
(  
 @rawXML xml,  
 @SiteId int  
)   
    
AS  
Begin  
SET NOCOUNT ON;  
  
--Set @rawXML = '<userGroups>  
--  <userGroup>  
--    <userGroupId>20</userGroupId>  
--    <groupId>5</groupId>  
--    <userId>39</userId>  
--    <siteId>1</siteId>  
--    <userGroupsDesc>dfssaa</userGroupsDesc>  
--    <active>1</active>  
--  </userGroup>  
--</userGroups>'  
  
    
SELECT     
 nref.value('groupId[1]', 'int') GroupId,    
 nref.value('userId[1]', 'int') UserId--,    
 --nref.value('userGroupsDesc[1]', 'nvarchar(255)') UserGroupsDesc    
INTO #Source FROM   @rawXML.nodes('//userGroup') AS R(nref)    
    
UPDATE t  
SET t.Active = 0  
FROM UserGroups t INNER JOIN #Source s ON t.GroupId = s.GroupId  
    
    
MERGE [UserGroups] AS T    
USING #Source AS S    
ON (T.GroupId = S.GroupId AND T.UserId = S.UserId)     
WHEN NOT MATCHED BY TARGET     
    THEN INSERT([GroupId],[UserId],[SiteId]) VALUES(S.[GroupId],S.[UserId],@SiteId)    
WHEN MATCHED     
    THEN UPDATE SET T.SiteId = @SiteId,
    --T.UserGroupsDesc = S.UserGroupsDesc, 
    T.Active = 1;   
End
GO
/****** Object:  StoredProcedure [dbo].[spiGroupRoles]    Script Date: 09/15/2014 12:10:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================    
-- Author:  <Deepak Kumar>    
-- Create date: <25-Aug-2014>    
-- Description: <mapping role with groups>    
-- =============================================    
CREATE PROCEDURE [dbo].[spiGroupRoles]-- ''    
(    
 @rawXML xml,    
 @SiteId int    
)     
      
AS    
Begin    
SET NOCOUNT ON;    
    
--Set @rawXML = ''    
    
      
SELECT       
 nref.value('groupId[1]', 'int') GroupId,      
 nref.value('roleId[1]', 'int') RoleId--,      
INTO #Source FROM   @rawXML.nodes('//groupRole') AS R(nref)      
      
UPDATE t    
SET t.Active = 0    
FROM GroupRoles t INNER JOIN #Source s ON t.GroupId = s.GroupId
      
      
MERGE [GroupRoles] AS T      
USING #Source AS S      
ON (T.RoleId = S.RoleId AND T.GroupId = S.GroupId )       
WHEN NOT MATCHED BY TARGET       
    THEN INSERT([GroupId],[RoleId],[SiteId]) VALUES(S.[GroupId],S.[RoleId],@SiteId)      
WHEN MATCHED       
    THEN UPDATE SET T.SiteId = @SiteId,  
    T.Active = 1;     
End
GO
/****** Object:  StoredProcedure [dbo].[spiGraphs]    Script Date: 09/15/2014 12:10:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Gagan Dhamija>
-- Create date: <16-Jul-2014>
-- Description:	<Inserting  Graphs> 

-- =============================================
CREATE PROCEDURE [dbo].[spiGraphs] 
(
	@ApplicationId uniqueidentifier,
	@GraphName nvarchar(255),
	@Graph varbinary(MAX),
	@SiteId int
)
	 
AS
BEGIN TRY
	BEGIN
	
		INSERT INTO Graphs ([ApplicationId],[GraphName],[Graph],[SiteId]) 
		VALUES (@ApplicationId,@GraphName,CONVERT(varbinary(MAX),@Graph),@SiteId )
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spiExchange]    Script Date: 09/15/2014 12:10:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Deepak Kumar>
-- Create date: <10-Sep-2014>
-- Description:	<Inserting Exchange>
-- ===========================================================
CREATE PROCEDURE [dbo].[spiExchange] 
(

	@UserName varchar(100),--'vmallire'
	@CommodityId	uniqueidentifier,
	@SourceGraphId	uniqueidentifier,
	@DestinationGraphId	uniqueidentifier,
	@Name	nvarchar(255),
	@Description	nvarchar(255),
	@PoolSize	int,
	@XTypeAdd	nvarchar(25),
	@XTypeChange	nvarchar(25),
	@XTypeSync	nvarchar(25),
	@XTypeDelete	nvarchar(25),
	@XTypeSetNull	nvarchar(25),
	@SiteId	int,
	@GroupList varchar(1000)
	
)	 
AS

	BEGIN
	
	--select * from ResourceType
	
	Declare @ExchangeId uniqueidentifier = NewID()  
	Declare @ResourceTypeId int = 6 --See ResourceType for detail
	
	INSERT INTO [Exchanges]
           ([ExchangeId]
           ,[CommodityId]
           ,[SourceGraphId]
           ,[DestinationGraphId]
           ,[Name]
           ,[Description]
           ,[PoolSize]
           ,[XTypeAdd]
           ,[XTypeChange]
           ,[XTypeSync]
           ,[XTypeDelete]
           ,[XTypeSetNull]
           ,[SiteId]
           )
	Select
			@ExchangeId
           ,@CommodityId
           ,@SourceGraphId
           ,@DestinationGraphId
           ,@Name
           ,@Description
           ,@PoolSize
           ,@XTypeAdd
           ,@XTypeChange
           ,@XTypeSync
           ,@XTypeDelete
           ,@XTypeSetNull
           ,@SiteId

	
	INSERT INTO [ResourceGroups]
           ([ResourceId]
           ,[GroupId]
           ,[SiteId]
           ,[ResourceTypeId]
           )
	Select 
		@ExchangeId,
		tg.Items,
		@SiteId,
		@ResourceTypeId
	from  UserGroups ug 
			inner join Users u on ug.UserId = u.UserId
				And u.UserName = @UserName
				And u.SiteId = @SiteId
				And u.Active = 1
				And ug.Active = 1
				And ug.SiteId = @SiteId
			inner join dbo.Split(@GroupList,',') tg on ug.GroupId = tg.Items
							

	END
GO
/****** Object:  StoredProcedure [dbo].[spiCommodity]    Script Date: 09/15/2014 12:10:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Deepak Kumar>
-- Create date: <10-Sep-2014>
-- Description:	<Inserting commodity>
-- ===========================================================
CREATE PROCEDURE [dbo].[spiCommodity] 
(
	@UserName varchar(100),--'vmallire'
	@SiteId INT,--1
	@ContextId uniqueidentifier,
	@CommodityName nvarchar(255),
	@GroupList varchar(1000)
	
)	 
AS

	BEGIN
	
	
	
	Declare @CommodityId uniqueidentifier = NewID()  
	Declare @ResourceTypeId int = 5 --See ResourceType for detail
	
	INSERT INTO [Commodity]
           ([ContextId]
           ,[CommodityId]
           ,[CommodityName]
           ,[SiteId]
           )
	Select 
		@ContextId,
		@CommodityId,
		@CommodityName,
		@SiteId

	
	INSERT INTO [ResourceGroups]
           ([ResourceId]
           ,[GroupId]
           ,[SiteId]
           ,[ResourceTypeId]
           )
	Select 
		@CommodityId,
		tg.Items,
		@SiteId,
		@ResourceTypeId
	from  UserGroups ug 
			inner join Users u on ug.UserId = u.UserId
				And u.UserName = @UserName
				And u.SiteId = @SiteId
				And u.Active = 1
				And ug.Active = 1
				And ug.SiteId = @SiteId
			inner join dbo.Split(@GroupList,',') tg on ug.GroupId = tg.Items
							

	END
GO
/****** Object:  StoredProcedure [dbo].[spgValuelist]    Script Date: 09/15/2014 12:10:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Gagan Dhamija>
-- Create date: <1-Sep-2014>
-- Description:	<Selecting Valuelist based on Application Id>
-- ===========================================================
CREATE PROCEDURE [dbo].[spgValuelist]
(
	@UserName varchar(100),
	@SiteId int,
	@ApplicationId uniqueidentifier
)	 
AS
BEGIN TRY
	BEGIN
	 
	  WITH XMLNAMESPACES ( DEFAULT 'http://www.iringtools.org/library')
      select  v.Name as name, 
      (select distinct  vm.Label as label, vm.internalvalue as internalValue, vm.Uri as uri   from ValueListMap vm
      inner join ResourceGroups crg on crg.siteid = @SiteId and crg.active = 1 and crg.resourceid = vm.ApplicationId 
      inner join usergroups cug on cug.groupid = crg.groupid and cug.siteid = crg.siteid and cug.active = crg.active
      inner join users cu on cu.userid = cug.userid and cu.username = @UserName and cu.siteid = crg.siteid and cu.active = crg.active
      inner Join  Applications a  on  a.ApplicationId = vm.ApplicationId  
      where vm.ApplicationId = @ApplicationId  and vm.Name = v.Name
      Group BY   vm.Label, vm.internalvalue ,vm.Uri 
      for xml PATH('valueMap'), type ) as 'valueMaps'
      from ValueListMap v
      inner join ResourceGroups crg on crg.siteid = @SiteId and crg.active = 1 and crg.resourceid = v.ApplicationId      
      inner join usergroups cug on cug.groupid = crg.groupid and cug.siteid = crg.siteid and cug.active = crg.active
      inner join users cu on cu.userid = cug.userid and cu.username = @UserName and cu.siteid = crg.siteid and cu.active = crg.active
      inner Join  Applications a  on  a.ApplicationId = v.ApplicationId 
      where v.ApplicationId = @ApplicationId  
      Group BY v.ApplicationId, v.Name
      for xml PATH('valueListMap'), ROOT('valueListMaps'), type, ELEMENTS XSINIL

	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spgUserGroups]    Script Date: 09/15/2014 12:10:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Gagan Dhamija>
-- Create date: <28-July-2014>
-- Description:	<Getting User Groups by specific user>
-- =============================================
CREATE PROCEDURE [dbo].[spgUserGroups]
(
	@userName Varchar(50),
	@siteId INT
)
	 
AS
BEGIN TRY
	BEGIN
         SELECT ug.UserGroupId, ug.GroupId, ug.UserId, ug.UserGroupsDesc FROM UserGroups ug
         JOIN Users ON ug.UserId=Users.UserId 
         where ug.SiteId = @siteId and ug.Active = 1 and Users.UserName = @userName;
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spgRolesGroup]    Script Date: 09/15/2014 12:10:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <01-JuL-2014>
-- Description:	<Return role based on group >
-- =============================================
CREATE PROCEDURE [dbo].[spgRolesGroup] 
(
	@GroupId INT,
	@SiteId INT
)
	 
AS
BEGIN TRY
	BEGIN
		SELECT DISTINCT R.RoleId, R.RoleName,R.RoleDesc FROM Roles R
			INNER JOIN GroupRoles GR ON GR.RoleId = R.RoleId WHERE GR.Active =1 AND R.Active =1 AND GR.GroupId = @GroupId AND GR.SiteId = @SiteId
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spgRoleGroups]    Script Date: 09/15/2014 12:10:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Deepak Kumar>
-- Create date: <22-Aug-2014>
-- Description:	<Return all group belongs 
-- to role>
-- =============================================
CREATE PROCEDURE [dbo].[spgRoleGroups]
(
	@RoleId INT,
	@SiteId INT
)
	 
AS
BEGIN
		
		SELECT G.GroupId, G.GroupName,G.GroupDesc FROM Groups G
			INNER JOIN GroupRoles GR ON GR.GroupId = G.GroupId
			INNER JOIN Roles R ON R.RoleId = GR.RoleId 
		WHERE GR.Active=1 AND R.Active=1 AND G.Active=1 AND R.RoleId = @RoleId AND GR.SiteId=@SiteId
END
GO
/****** Object:  StoredProcedure [dbo].[spgGroupUsers]    Script Date: 09/15/2014 12:10:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <27-June-2014>
-- Description:	<Getting Group Users>
-- =============================================
CREATE PROCEDURE [dbo].[spgGroupUsers]
(
	@GroupId INT
)
	 
AS
BEGIN TRY
	BEGIN
		
		SELECT  U.UserId,U.UserFirstName ,U.UserLastName,U.UserEmail,U.UserPhone,U.UserDesc FROM Users U
			INNER JOIN UserGroups UG ON UG.UserId = U.UserId
			INNER JOIN Groups G ON G.GroupId = UG.GroupId
		WHERE UG.Active=1 AND U.Active=1 AND G.Active =1 AND  UG.GroupId = @GroupId
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spgGroupUserIdGroupId]    Script Date: 09/15/2014 12:10:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <01-JuL-2014>
-- Description:	<return user based on user id and grop id>
-- =============================================
CREATE PROCEDURE [dbo].[spgGroupUserIdGroupId]
(
	@UserId INT,
	@GroupId INT,
	@SiteId INT
)
	 
AS
BEGIN TRY
	BEGIN
		
		SELECT  U.UserId,U.UserFirstName ,U.UserLastName,U.UserEmail,U.UserPhone,U.UserDesc FROM Users U
			INNER JOIN UserGroups UG ON UG.UserId = U.UserId
			INNER JOIN Groups G ON G.GroupId = UG.GroupId
		WHERE UG.Active=1 AND U.Active=1 AND G.Active =1 AND  UG.GroupId = @GroupId AND U.UserId =@UserId
	
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spgGroupUser]    Script Date: 09/15/2014 12:10:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <01-JuL-2014>
-- Description:	<return all group that the user belongs to>
-- =============================================
CREATE PROCEDURE [dbo].[spgGroupUser]
(
	@UserId INT,
	@SiteId INT
)
	 
AS
BEGIN TRY
	BEGIN
		
		SELECT G.GroupId, G.GroupName,G.GroupDesc FROM Groups G
			INNER JOIN UserGroups UG ON UG.GroupId = G.GroupId
			INNER JOIN Users U ON U.UserId = UG.UserId 
		WHERE UG.Active=1 AND U.Active=1 AND G.Active=1 AND U.UserId = @UserId AND UG.SiteId=@SiteId
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spgCommoditiesByUser]    Script Date: 09/15/2014 12:10:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Vidisha>
-- Create date: <01-Sep-2014>
-- Description:	<Selecting commodities based on users>
-- ===========================================================
CREATE PROCEDURE [dbo].[spgCommoditiesByUser]  --'vmallire',1,'2567c504-133d-41b9-8780-7bfd73840ed3'  
(
	@UserName varchar(100),--'vmallire'
	@SiteId INT,--1
	@ContextId uniqueidentifier--'2567c504-133d-41b9-8780-7bfd73840ed3'
)	 
AS

	BEGIN
	 
		WITH XMLNAMESPACES ( DEFAULT 'http://www.iringtools.org/library' ) 
		select 
		c.ContextId as contextId, 
		c.CommodityId as commodityId, 
		c.CommodityName as commodityName, 
		c.SiteId as siteId, 
		c.Active as active,
		(
		select distinct  p.PermissionId as permissionId, p.SiteId as siteId, p.PermissionName as permissionName, p.PermissionDesc as permissionDesc, p.Active as active from permissions p
		inner join ResourceGroups rg on rg.siteid = c.siteid and rg.active = c.active and rg.resourceid = c.CommodityId
		inner join Groups g on g.groupId = rg.groupId and g.siteid = g.siteid and g.active = g.active
		inner join grouproles gr on  gr.groupid = rg.groupid and gr.siteid = g.siteid and gr.active = g.active    
		inner join roles r on r.roleid = gr.roleid and r.siteid = g.siteid and r.active = g.active     
		inner join rolepermissions rp on rp.permissionid = p.permissionid and rp.roleid = r.roleid and rp.siteid = g.siteid and rp.active = g.active          
		inner join usergroups ug on ug.groupid = rg.groupid  and ug.siteid = g.siteid and ug.active = g.active
		inner join users u on u.userid = ug.userid and u.username = @UserName and u.siteid = g.siteid and u.active = g.active
		where  g.siteid = @SiteId and g.active = 1   for xml PATH('permission'), type 
		) as 'permissions'
		
		from Commodity c
			inner join ResourceGroups crg on crg.siteid = c.siteid and crg.active = c.active and crg.resourceid = c.commodityId
			inner join Groups cg on cg.groupId = crg.groupId and cg.siteid = c.siteid and cg.active = c.active
			inner join grouproles cgr on  cgr.groupid = crg.groupid and cgr.siteid = c.siteid and cgr.active = c.active    
			inner join roles cr on cr.roleid = cgr.roleid and cr.siteid = c.siteid and cr.active = c.active         
			inner join usergroups cug on cug.groupid = crg.groupid and cug.siteid = c.siteid and cug.active = c.active
			inner join users cu on cu.userid = cug.userid and cu.username = @UserName and cu.siteid = c.siteid and cu.active = c.active
		where c.contextId = @ContextId and c.siteid = @SiteId and c.active = 1
		Group BY c.ContextId,c.CommodityId, c.CommodityName, c.SiteId, c.Active
		for xml PATH('commodity'), ROOT('commodities'), type, ELEMENTS XSINIL
 

	END
GO
/****** Object:  StoredProcedure [dbo].[spgApplicationByUser]    Script Date: 09/15/2014 12:10:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Gagan Dhamija>
-- Create date: <24-July-2014>
-- Description:	<Selecting Applications based on users>
-- ===========================================================
CREATE PROCEDURE [dbo].[spgApplicationByUser]
(
	@UserName varchar(100),
	@SiteId INT,
	@ContextId uniqueidentifier
)	 
AS
BEGIN TRY
	BEGIN
	
	 WITH XMLNAMESPACES ( default 'http://www.iringtools.org/library') 
     select  a.ContextId as contextId , a.ApplicationId as applicationId , a.DisplayName as displayName,a.InternalName as internalName,a.Description as description,
     a.DXFRUrl as dxfrUrl,a.SiteId as siteId,a.Active as active,a.Assembly as assembly,
     (select distinct  p.PermissionId as permissionId, p.SiteId as siteId, p.PermissionName as permissionName, p.PermissionDesc as permissionDesc, p.Active as active from permissions p
     inner join ResourceGroups rg on rg.siteid = a.siteid and rg.active = a.active and rg.resourceid = a.ApplicationId
     inner join Groups g on g.groupId = rg.groupId and g.siteid = a.siteid and g.active = a.active
     inner join grouproles gr on  gr.groupid = rg.groupid and gr.siteid = a.siteid and gr.active = a.active    
     inner join roles r on r.roleid = gr.roleid and r.siteid = a.siteid and r.active = a.active     
     inner join rolepermissions rp on rp.permissionid = p.permissionid and rp.roleid = r.roleid and rp.siteid = a.siteid and rp.active = a.active          
     inner join usergroups ug on ug.groupid = rg.groupid  and ug.siteid = a.siteid and ug.active = a.active
     inner join users u on u.userid = ug.userid and u.username = @UserName and u.siteid = a.siteid and u.active = a.active
     where a.ContextId = @ContextId and  a.siteid = @SiteId and a.active = 1   for xml PATH('permission'), type ) as 'permissions',
     (select distinct s.Name as name,appset.SettingValue as value from ApplicationSettings appSet 
     inner join ResourceGroups appSetRG on appSetRG.siteid = a.siteid and appSetRG.active = a.active and appSetRG.resourceid = a.ApplicationId and  appSet.ApplicationId = appSetRG.resourceid
     inner join Groups appSeG on appSeG.groupId = appSetRG.groupId and appSeG.siteid = a.siteid and appSeG.active = a.active
     inner join grouproles appSeGR on  appSeGR.groupid = appSetRG.groupid and appSeGR.siteid = a.siteid and appSeGR.active = a.active    
     inner join roles appSeR on appSeR.roleid = appSeGR.roleid and appSeR.siteid = a.siteid and appSeR.active = a.active         
     inner join usergroups appSeUG on appSeUG.groupid = appSetRG.groupid and appSeUG.siteid = a.siteid and appSeUG.active = a.active
     inner join users appSeU on appSeU.userid = appSeUG.userid and appSeU.username = @UserName and appSeU.siteid = a.siteid and appSeU.active = a.active
     inner join Settings s  on s.SettingId = appset.SettingID 
     where a.ContextId = @ContextId and  a.siteid = @SiteId and a.active = 1   for xml PATH('applicationSetting'), type ) as 'applicationSettings' 
     from  Applications a
     inner join ResourceGroups arg on arg.siteid = a.siteid and arg.active = a.active and arg.resourceid = a.ApplicationId
     inner join Groups ag on ag.groupId = arg.groupId and ag.siteid = a.siteid and ag.active = a.active
     inner join grouproles agr on  agr.groupid = arg.groupid and agr.siteid = a.siteid and agr.active = a.active    
     inner join roles ar on ar.roleid = agr.roleid and ar.siteid = a.siteid and ar.active = a.active         
     inner join usergroups aug on aug.groupid = arg.groupid and aug.siteid = a.siteid and aug.active = a.active
     inner join users au on au.userid = aug.userid and au.username = @UserName and au.siteid = a.siteid and au.active = a.active
     where a.ContextId = @ContextId and  a.siteid = @SiteId and a.active = 1	 
Group BY  a.ContextId , a.ApplicationId , a.DisplayName,a.InternalName,a.Description,
	   a.DXFRUrl,a.SiteId,a.Active,a.Assembly
	  for xml PATH('application'), ROOT('applications'), type, ELEMENTS XSINIL
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spdCommodity]    Script Date: 09/15/2014 12:10:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ================================
-- Author:		<Deepak Kumar>
-- Create date: <11-Sep-2014>
-- Description:	<delete commodity>
-- ================================
create PROCEDURE [dbo].[spdCommodity] 
(
	@CommodityId uniqueidentifier
)	 
AS

	BEGIN
	
	Update Commodity
		Set Active = 0
	Where CommodityId = @CommodityId 


	Update ResourceGroups
	Set   Active = 0
	Where ResourceId = @CommodityId 
		 
	END
GO
/****** Object:  StoredProcedure [dbo].[spgGroupRoles]    Script Date: 09/15/2014 12:10:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <19-June-2014>
-- Description:	<Getting group roles>

-- =============================================
CREATE  PROCEDURE [dbo].[spgGroupRoles]
(
	@GroupId INT,
	@RoleId INT,
	@SiteId INT
)

 
AS
BEGIN TRY
	BEGIN
		SELECT GroupRoleId,GroupId,RoleId,SiteId FROM GroupRoles WHERE Active=1 AND GroupId=@GroupId AND RoleId=@RoleId AND siteid= @SiteId
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spgGraphMappingByUser]    Script Date: 09/15/2014 12:10:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Gagan Dhamija>
-- Create date: <27-Aug-2014>
-- Description:	<Selecting Graphs based on users>
-- ===========================================================
CREATE PROCEDURE [dbo].[spgGraphMappingByUser] 
(
	@UserName varchar(100),
	@SiteId INT,
	@GraphId uniqueidentifier
)	 
AS
BEGIN TRY
	BEGIN
	    
    WITH XMLNAMESPACES (DEFAULT 'http://www.iringtools.org/library') 
    select g.GraphId as graphId, g.graphName as graphName,g.graph as graphObject from Graphs g
    inner join ResourceGroups crg on crg.siteid = g.siteid and crg.active = g.active 
    inner join Groups cg on cg.groupId = crg.groupId and cg.siteid = g.siteid and cg.active = g.active
    inner join grouproles cgr on  cgr.groupid = crg.groupid and cgr.siteid = g.siteid and cgr.active = g.active    
    inner join roles cr on cr.roleid = cgr.roleid and cr.siteid = g.siteid and cr.active = g.active         
    inner join usergroups cug on cug.groupid = crg.groupid and cug.siteid = g.siteid and cug.active = g.active
    inner join users cu on cu.userid = cug.userid and cu.username = @UserName and cu.siteid = g.siteid and cu.active = g.active
    where g.graphId = @GraphId and g.siteid = @SiteId and g.active = 1
    Group BY g.GraphId, g.graphName, g.graph  
    for xml PATH('graph'), ROOT('graphs'), type, ELEMENTS XSINIL
	
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spgGraphByUser]    Script Date: 09/15/2014 12:10:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Gagan Dhamija>
-- Create date: <27-Aug-2014>
-- Description:	<Selecting Graphs based on users>
-- ===========================================================
CREATE PROCEDURE [dbo].[spgGraphByUser] 
(
	@UserName varchar(100),
	@SiteId INT,
	@ApplicationId uniqueidentifier
)	 
AS
BEGIN TRY
	BEGIN
	    
    WITH XMLNAMESPACES (DEFAULT 'http://www.iringtools.org/library') 
    select g.ApplicationId as applicationId, g.GraphId as graphId, g.graphName as graphName, g.SiteId as siteId, g.Active as active,
    (select distinct  p.PermissionId as permissionId, p.SiteId as siteId, p.PermissionName as permissionName, p.PermissionDesc as permissionDesc, p.Active as active from permissions p
    inner join ResourceGroups rg on rg.siteid = g.siteid and rg.active = g.active and rg.resourceid = g.graphId 
    inner join Groups g on g.groupId = rg.groupId and g.siteid = g.siteid and g.active = g.active
    inner join grouproles gr on  gr.groupid = rg.groupid and gr.siteid = g.siteid and gr.active = g.active    
    inner join roles r on r.roleid = gr.roleid and r.siteid = g.siteid and r.active = g.active     
    inner join rolepermissions rp on rp.permissionid = p.permissionid and rp.roleid = r.roleid and rp.siteid = g.siteid and rp.active = g.active          
    inner join usergroups ug on ug.groupid = rg.groupid  and ug.siteid = g.siteid and ug.active = g.active
    inner join users u on u.userid = ug.userid and u.username = @UserName and u.siteid = g.siteid and u.active = g.active
    where  g.siteid = 1 and g.active = 1   for xml PATH('permission'), type ) as 'permissions'
    from Graphs g
    inner join ResourceGroups crg on crg.siteid = g.siteid and crg.active = g.active and crg.resourceid = g.graphId
    inner join Groups cg on cg.groupId = crg.groupId and cg.siteid = g.siteid and cg.active = g.active
    inner join grouproles cgr on  cgr.groupid = crg.groupid and cgr.siteid = g.siteid and cgr.active = g.active    
    inner join roles cr on cr.roleid = cgr.roleid and cr.siteid = g.siteid and cr.active = g.active         
    inner join usergroups cug on cug.groupid = crg.groupid and cug.siteid = g.siteid and cug.active = g.active
    inner join users cu on cu.userid = cug.userid and cu.username = @UserName and cu.siteid = g.siteid and cu.active = g.active
    where g.applicationId = @ApplicationId and g.siteid = @SiteId and g.active = 1
    Group BY g.ApplicationId, g.GraphId, g.graphName, g.SiteId, g.Active
    for xml PATH('graph'), ROOT('graphs'), type, ELEMENTS XSINIL
	
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spgGraph]    Script Date: 09/15/2014 12:10:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Gagan Dhamija>
-- Create date: <15-July-2014>
-- Description:	<Getting All Graphs>

-- =============================================
CREATE  PROCEDURE [dbo].[spgGraph] 
 
AS
BEGIN TRY
	BEGIN
		SELECT ApplicationId,GraphId,GraphName,Graph,SiteId FROM Graphs WHERE Active=1
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spgFolderByUser]    Script Date: 09/15/2014 12:10:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Gagan Dhamija>
-- Create date: <22-Aug-2014>
-- Description:	<Selecting Group based on users>
-- ===========================================================
CREATE PROCEDURE [dbo].[spgFolderByUser]
(
	@UserName varchar(100),
	@SiteId INT,
	@FolderId uniqueidentifier
)	 
AS
BEGIN TRY
	BEGIN
	 
	  WITH XMLNAMESPACES ( DEFAULT 'http://www.iringtools.org/library')
      select f.FolderId as folderId, f.ParentFolderId as parentFolderId, f.FolderName as folderName, f.SiteId as siteId, f.Active as active, (select distinct  p.PermissionId as permissionId, p.SiteId as siteId, p.PermissionName as permissionName, p.PermissionDesc as permissionDesc, p.Active as active from permissions p
      inner join ResourceGroups rg on rg.siteid = f.siteid and rg.active = f.active and rg.resourceid = f.folderId 
      inner join Groups g on g.groupId = rg.groupId and g.siteid = f.siteid and g.active = f.active
      inner join grouproles gr on  gr.groupid = rg.groupid and gr.siteid = f.siteid and gr.active = f.active    
      inner join roles r on r.roleid = gr.roleid and r.siteid = f.siteid and r.active = f.active     
      inner join rolepermissions rp on rp.permissionid = p.permissionid and rp.roleid = r.roleid and rp.siteid = f.siteid and rp.active = f.active          
      inner join usergroups ug on ug.groupid = rg.groupid  and ug.siteid = f.siteid and ug.active = f.active
      inner join users u on u.userid = ug.userid and u.username = @UserName and u.siteid = f.siteid and u.active = f.active
      where f.parentFolderId = @FolderId and  f.siteid = @SiteId and f.active = 1   for xml PATH('permission'), type ) as 'permissions'
      from Folders f
      inner join ResourceGroups crg on crg.siteid = f.siteid and crg.active = f.active and crg.resourceid = f.folderId
      inner join Groups cg on cg.groupId = crg.groupId and cg.siteid = f.siteid and cg.active = f.active
      inner join grouproles cgr on  cgr.groupid = crg.groupid and cgr.siteid = f.siteid and cgr.active = f.active    
      inner join roles cr on cr.roleid = cgr.roleid and cr.siteid = f.siteid and cr.active = f.active         
      inner join usergroups cug on cug.groupid = crg.groupid and cug.siteid = f.siteid and cug.active = f.active
      inner join users cu on cu.userid = cug.userid and cu.username = @UserName and cu.siteid = f.siteid and cu.active = f.active
      where f.parentFolderId = @FolderId and f.siteid = @SiteId and f.active = 1 
      Group BY f.FolderId, f.ParentFolderId, f.FolderName, f.SiteId, f.Active
      for xml PATH('folder'), ROOT('folders'), type, ELEMENTS XSINIL

	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spgExchangesByUser]    Script Date: 09/15/2014 12:10:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Vidisha>
-- Create date: <27-Aug-2014>
-- Description:	<Selecting exchanges based on users>
-- ===========================================================
CREATE PROCEDURE [dbo].[spgExchangesByUser] --'vmallire',1,'ee565602-ba52-422c-a01f-932490796666'
(
	@UserName varchar(100),--'vmallire'
	@SiteId INT,--1
	@CommodityId uniqueidentifier--'ee565602-ba52-422c-a01f-932490796666'
)	 
AS

	BEGIN
	 
		WITH XMLNAMESPACES ( DEFAULT 'http://www.iringtools.org/library' ) 
		select  
		e.ExchangeId as exchangeId,
		e.CommodityId as commodityId, 
		e.SourceGraphId as sourceGraphId, 
		e.DestinationGraphId as destinationGraphId,
		e.Name as name,
		e.Description as description, 
		e.PoolSize as poolSize, 
		e.XTypeAdd as xtypeAdd ,
		e.XTypeChange as xtypeChange ,
		e.XTypeSync as xtypeSync ,
		e.XTypeDelete as xtypeDelete ,
		e.XTypeSetNull as xtypeSetNull,
		e.SiteId as siteId,
		e.Active as active,
		(
			select distinct  p.PermissionId as permissionId, p.SiteId as siteId, p.PermissionName as permissionName, p.PermissionDesc as permissionDesc, p.Active as active from permissions p
			  inner join ResourceGroups rg on rg.siteid = e.siteid and rg.active = e.active and rg.resourceid = e.ExchangeId
			  inner join Groups g on g.groupId = rg.groupId and g.siteid = e.siteid and g.active = e.active
			  inner join grouproles gr on  gr.groupid = rg.groupid and gr.siteid = e.siteid and gr.active = e.active    
			  inner join roles r on r.roleid = gr.roleid and r.siteid = e.siteid and r.active = e.active     
			  inner join rolepermissions rp on rp.permissionid = p.permissionid and rp.roleid = r.roleid and rp.siteid = e.siteid and rp.active = e.active          
			  inner join usergroups ug on ug.groupid = rg.groupid  and ug.siteid = e.siteid and ug.active = e.active
			  inner join users u on u.userid = ug.userid and u.username = @UserName and u.siteid = e.siteid and u.active = e.active
			  where e.CommodityId = @CommodityId and  e.siteid = @SiteId and e.active = 1   for xml PATH('permission'), type 
		) as 'permissions'
		
		from  Exchanges e
			inner join ResourceGroups arg on arg.siteid = e.siteid and arg.active = e.active and arg.resourceid =  e.ExchangeId
			inner join Groups ag on ag.groupId = arg.groupId and ag.siteid = e.siteid and ag.active = e.active
			inner join grouproles agr on  agr.groupid = arg.groupid and agr.siteid = e.siteid and agr.active = e.active    
			inner join roles ar on ar.roleid = agr.roleid and ar.siteid = e.siteid and ar.active = e.active         
			inner join usergroups aug on aug.groupid = arg.groupid and aug.siteid = e.siteid and aug.active = e.active
			inner join users au on au.userid = aug.userid and au.username = @UserName and au.siteid = e.siteid and au.active = e.active
		where e.CommodityId = @CommodityId and  e.siteid = @SiteId and e.active = 1
		Group BY e.CommodityId, e.ExchangeId , e.Name,e.Description, e.PoolSize, e.SourceGraphId, e.DestinationGraphId,e.SiteId,e.Active, e.xtypeAdd, e.xtypeChange, e.xtypeSync, e.xtypeDelete, e.xtypeSetNull
		for xml PATH('exchange'), ROOT('exchanges'), type, ELEMENTS XSINIL


	END
GO
/****** Object:  StoredProcedure [dbo].[spgDataFilterByUser]    Script Date: 09/15/2014 12:10:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Vidisha>
-- Create date: <27-Aug-2014>
-- Description:	<Selecting datafilters based on users>
-- ===========================================================
CREATE PROCEDURE [dbo].[spgDataFilterByUser]
(
	@UserName varchar(100),--'vmallire'
	@SiteId INT,--1
	@ResourceId uniqueidentifier--'d208b9b8-0372-4e2c-9b34-78b9bb453b61'
)	 
AS

	BEGIN
	 
		WITH XMLNAMESPACES ( DEFAULT 'http://www.iringtools.org/library' ) 
		select 
		df.ResourceId as resourceId, 
		df.SiteId as siteId, 
		df.Active as active,
		(
			select distinct 
			efd.DFOrder as dfOrder,
			efd.OpenCount as openCount, 
			efd.LogicalOperator as logicalOperator, 
			efd.PropertyName as propertyName, 
			efd.RelationalOperator as relationalOperator,
			efd.Value as value, 
			efd.CloseCount as closeCount 
			from DataFilters efd
			where  efd.IsExpression =  1  Order By efd.DFOrder for xml PATH('expression'), type 
		) as 'expressions',

		(
			select distinct 
			oedf.DFOrder as dfOrder,
			oedf.PropertyName as propertyName, 
			oedf.Sort as sort
			from DataFilters oedf  where  oedf.IsExpression =  0  Order By oedf.DFOrder  for xml PATH('orderExpression'), type 
		) as 'orderExpressions'

		from  DataFilters df
		inner join  DataFiltersType dft on dft.DataFilterTypeId = df.DataFilterTypeId
		inner join ResourceGroups arg on arg.siteid = df.siteid and arg.active = df.active and arg.resourceid =  df.ResourceId
		inner join Groups ag on ag.groupId = arg.groupId and ag.siteid = df.siteid and ag.active = df.active
		inner join grouproles agr on  agr.groupid = arg.groupid and agr.siteid = df.siteid and agr.active = df.active    
		inner join roles ar on ar.roleid = agr.roleid and ar.siteid = df.siteid and ar.active = df.active         
		inner join usergroups aug on aug.groupid = arg.groupid and aug.siteid = df.siteid and aug.active = df.active
		inner join users au on au.userid = aug.userid and au.username = @UserName and au.siteid = df.siteid and au.active = df.active
		where df.ResourceId = @ResourceId and  dft.DataFilterTypeName = 'AppData'  and  df.siteid = @SiteId and df.active = 1
		Group BY df.SiteId,df.Active, df.ResourceId 
		for xml PATH('dataFilter'), ROOT('dataFilters'), type, ELEMENTS XSINIL 

	END
GO
/****** Object:  StoredProcedure [dbo].[spgContextByUser]    Script Date: 09/15/2014 12:10:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Gagan Dhamija>
-- Create date: <22-July-2014>
-- Description:	<Selecting Contexts based on users>
-- ===========================================================
CREATE PROCEDURE [dbo].[spgContextByUser]
(
	@UserName varchar(100),
	@SiteId INT,
	@FolderId uniqueidentifier
)	 
AS
BEGIN TRY
	BEGIN
	 
	  WITH XMLNAMESPACES ( DEFAULT 'http://www.iringtools.org/library')
      select  c.ContextId as contextId ,c.DisplayName as displayName,c.InternalName as internalName,c.Description as description,
      c.CacheConnStr as cacheConnStr,c.SiteId as siteId,c.Active as active,c.FolderId as folderId,
     (select distinct p.PermissionId as permissionId,p.SiteId as siteId, p.PermissionName as permissionName,p.PermissionDesc as permissionDesc,
      p.Active as active from permissions p
      inner join ResourceGroups rg on rg.siteid = c.siteid and rg.active = c.active and rg.resourceid = c.contextid 
      inner join Groups g on g.groupId = rg.groupId and g.siteid = c.siteid and g.active = c.active
      inner join grouproles gr on  gr.groupid = rg.groupid and gr.siteid = c.siteid and gr.active = c.active    
      inner join roles r on r.roleid = gr.roleid and r.siteid = c.siteid and r.active = c.active     
      inner join rolepermissions rp on rp.permissionid = p.permissionid and rp.roleid = r.roleid and rp.siteid = c.siteid and rp.active = c.active          
      inner join usergroups ug on ug.groupid = rg.groupid  and ug.siteid = c.siteid and ug.active = c.active
      inner join users u on u.userid = ug.userid and u.username = @UserName and u.siteid = c.siteid and u.active = c.active
      where c.folderid = @FolderId and  c.siteid = @SiteId and c.active = 1   for xml PATH('permission'), type ) as 'permissions'
      from contexts c
      inner join ResourceGroups crg on crg.siteid = c.siteid and crg.active = c.active and crg.resourceid = c.contextid
      inner join Groups cg on cg.groupId = crg.groupId and cg.siteid = c.siteid and cg.active = c.active
      inner join grouproles cgr on  cgr.groupid = crg.groupid and cgr.siteid = c.siteid and cgr.active = c.active    
      inner join roles cr on cr.roleid = cgr.roleid and cr.siteid = c.siteid and cr.active = c.active         
      inner join usergroups cug on cug.groupid = crg.groupid and cug.siteid = c.siteid and cug.active = c.active
      inner join users cu on cu.userid = cug.userid and cu.username = @UserName and cu.siteid = c.siteid and cu.active = c.active
      where c.folderid = @FolderId and  c.siteid = @SiteId and c.active = 1
      GROUP BY  c.ContextId, c.DisplayName, c.InternalName,  c.Description, c.CacheConnStr,c.SiteId,c.Active,c.FolderId
      for xml PATH('context'), ROOT('contexts'), type, ELEMENTS XSINIL
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spdGraph]    Script Date: 09/15/2014 12:10:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Gagan Dhamija>
-- Create date: <17-Jul-2014>
-- Description:	<Deleting Graphs>

-- =============================================
CREATE PROCEDURE [dbo].[spdGraph] 
(
	@GraphId uniqueidentifier,
	@SiteId INT
)	
	 
AS
BEGIN TRY
	BEGIN
		
		UPDATE Graphs SET Active = 0 WHERE GraphId= @GraphId AND SiteId = @SiteId
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spdExchange]    Script Date: 09/15/2014 12:10:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Deepak Kumar>
-- Create date: <10-Sep-2014>
-- Description:	<delete Exchange>
-- ===========================================================
CREATE PROCEDURE [dbo].[spdExchange] 
(
	@ExchangeId uniqueidentifier
)	 
AS
BEGIN

	Update Exchanges
		Set Active = 0
	Where ExchangeId = @ExchangeId 


	Update ResourceGroups
		Set   Active = 0
	Where ResourceId = @ExchangeId   
						

END
GO
/****** Object:  Table [dbo].[DataObjects]    Script Date: 09/15/2014 12:10:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DataObjects](
	[DataObjectId] [uniqueidentifier] NOT NULL,
	[DictionaryId] [uniqueidentifier] NOT NULL,
	[TableName] [nvarchar](50) NOT NULL,
	[ObjectNameSpace] [nvarchar](50) NULL,
	[ObjectName] [nvarchar](50) NULL,
	[KeyDelimeter] [nvarchar](50) NULL,
 CONSTRAINT [PK_DataObjects] PRIMARY KEY CLUSTERED 
(
	[DataObjectId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PickList]    Script Date: 09/15/2014 12:10:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PickList](
	[DictionaryId] [uniqueidentifier] NOT NULL,
	[PickListId] [int] NOT NULL,
	[pickListName] [nvarchar](50) NULL,
	[Description] [nvarchar](250) NULL,
	[ValueProperyIndex] [nvarchar](50) NULL,
	[TableName] [nvarchar](50) NULL,
 CONSTRAINT [PK_PickList] PRIMARY KEY CLUSTERED 
(
	[PickListId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DataRelationships]    Script Date: 09/15/2014 12:10:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DataRelationships](
	[RelationshipId] [uniqueidentifier] NOT NULL,
	[DataObjectID] [uniqueidentifier] NOT NULL,
	[RelationShipName] [nvarchar](50) NULL,
	[relationObjectName] [nvarchar](50) NULL,
	[RelationShipTYpe] [nvarchar](50) NULL,
 CONSTRAINT [PK_DataRelationships] PRIMARY KEY CLUSTERED 
(
	[RelationshipId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DataProperties]    Script Date: 09/15/2014 12:10:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DataProperties](
	[DataObjectId] [uniqueidentifier] NULL,
	[PickListId] [int] NULL,
	[ColumnName] [nvarchar](50) NULL,
	[PropertyName] [nvarchar](50) NULL,
	[DataType] [int] NULL,
	[DataLength] [nvarchar](50) NULL,
	[IsNullable] [nchar](10) NULL,
	[KeyType] [nvarchar](50) NULL,
	[ShowOnIndex] [nvarchar](50) NULL,
	[NumberOfDecimals] [int] NULL,
	[IsReadOnly] [nchar](10) NULL,
	[ShowOnSearch] [nvarchar](50) NULL,
	[IsHidden] [nchar](10) NULL,
	[Description] [nvarchar](250) NULL,
	[AliasDictionary] [nvarchar](50) NULL,
	[ReferenceType] [nvarchar](50) NULL,
	[IsVirtual] [nchar](10) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[KeyProperty]    Script Date: 09/15/2014 12:10:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[KeyProperty](
	[DataObjectId] [uniqueidentifier] NOT NULL,
	[KeyPropertyName] [nvarchar](250) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PropertyMap]    Script Date: 09/15/2014 12:10:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PropertyMap](
	[RelationshipID] [uniqueidentifier] NULL,
	[DataPropertyName] [nvarchar](50) NULL,
	[RelationPropertyName] [nvarchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  Default [DF_Contexts_ContextId]    Script Date: 09/15/2014 12:09:35 ******/
ALTER TABLE [dbo].[Contexts] ADD  CONSTRAINT [DF_Contexts_ContextId]  DEFAULT (newid()) FOR [ContextId]
GO
/****** Object:  Default [DF__Contexts__Active__46E78A0C]    Script Date: 09/15/2014 12:09:35 ******/
ALTER TABLE [dbo].[Contexts] ADD  CONSTRAINT [DF__Contexts__Active__46E78A0C]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF_Sites_Active]    Script Date: 09/15/2014 12:09:35 ******/
ALTER TABLE [dbo].[Sites] ADD  CONSTRAINT [DF_Sites_Active]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF_MimeType_Active]    Script Date: 09/15/2014 12:09:35 ******/
ALTER TABLE [dbo].[MimeType] ADD  CONSTRAINT [DF_MimeType_Active]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF__Users__Active__0EA330E9]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF__Users__Active__0EA330E9]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF__Permissio__Activ__1B0907CE]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[Permissions] ADD  CONSTRAINT [DF__Permissio__Activ__1B0907CE]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF__Roles__Active__15502E78]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[Roles] ADD  CONSTRAINT [DF__Roles__Active__15502E78]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF_Applications_ContextId]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[Applications] ADD  CONSTRAINT [DF_Applications_ContextId]  DEFAULT (newid()) FOR [ContextId]
GO
/****** Object:  Default [DF_Applications_ApplicationId]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[Applications] ADD  CONSTRAINT [DF_Applications_ApplicationId]  DEFAULT (newid()) FOR [ApplicationId]
GO
/****** Object:  Default [DF_Applications_DisplayName]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[Applications] ADD  CONSTRAINT [DF_Applications_DisplayName]  DEFAULT (N'NEW GUID') FOR [DisplayName]
GO
/****** Object:  Default [DF__Applicati__Activ__4316F928]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[Applications] ADD  CONSTRAINT [DF__Applicati__Activ__4316F928]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF_Folders_FolderId]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[Folders] ADD  CONSTRAINT [DF_Folders_FolderId]  DEFAULT (newid()) FOR [FolderId]
GO
/****** Object:  Default [DF_Folders_ParentFolderId]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[Folders] ADD  CONSTRAINT [DF_Folders_ParentFolderId]  DEFAULT (newid()) FOR [ParentFolderId]
GO
/****** Object:  Default [DF__Folders__Active__49C3F6B7]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[Folders] ADD  CONSTRAINT [DF__Folders__Active__49C3F6B7]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF__Commodity__Activ__42E1EEFE]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[Commodity] ADD  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF_Groups_Active]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[Groups] ADD  CONSTRAINT [DF_Groups_Active]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF__GroupRole__Activ__4CA06362]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[GroupRoles] ADD  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF_Graphs_GraphId]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[Graphs] ADD  CONSTRAINT [DF_Graphs_GraphId]  DEFAULT (newid()) FOR [GraphId]
GO
/****** Object:  Default [DF__Graphs__Active__753864A1]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[Graphs] ADD  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF__Exchanges__Activ__60A75C0F]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[Exchanges] ADD  CONSTRAINT [DF__Exchanges__Activ__60A75C0F]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF__RolePermi__Activ__14270015]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[RolePermissions] ADD  CONSTRAINT [DF__RolePermi__Activ__14270015]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF__ResourceG__Activ__4F7CD00D]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[ResourceGroups] ADD  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF_ResourceGroups_ResourceTypeId]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[ResourceGroups] ADD  CONSTRAINT [DF_ResourceGroups_ResourceTypeId]  DEFAULT ((1)) FOR [ResourceTypeId]
GO
/****** Object:  Default [DF__UserGroup__Activ__267ABA7A]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[UserGroups] ADD  CONSTRAINT [DF__UserGroup__Activ__267ABA7A]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  ForeignKey [FK__Users__SiteId__6754599E]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[Users]  WITH CHECK ADD FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
/****** Object:  ForeignKey [FK__Permissio__SiteI__60A75C0F]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[Permissions]  WITH CHECK ADD FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
/****** Object:  ForeignKey [FK__Roles__SiteId__145C0A3F]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[Roles]  WITH CHECK ADD  CONSTRAINT [FK__Roles__SiteId__145C0A3F] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
ALTER TABLE [dbo].[Roles] CHECK CONSTRAINT [FK__Roles__SiteId__145C0A3F]
GO
/****** Object:  ForeignKey [FK__Applicati__Conte__4222D4EF]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[Applications]  WITH CHECK ADD  CONSTRAINT [FK__Applicati__Conte__4222D4EF] FOREIGN KEY([ContextId])
REFERENCES [dbo].[Contexts] ([ContextId])
GO
ALTER TABLE [dbo].[Applications] CHECK CONSTRAINT [FK__Applicati__Conte__4222D4EF]
GO
/****** Object:  ForeignKey [FK__Applicati__SiteI__46E78A0C]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[Applications]  WITH CHECK ADD  CONSTRAINT [FK__Applicati__SiteI__46E78A0C] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
ALTER TABLE [dbo].[Applications] CHECK CONSTRAINT [FK__Applicati__SiteI__46E78A0C]
GO
/****** Object:  ForeignKey [FK__Folders__SiteId__59FA5E80]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[Folders]  WITH CHECK ADD  CONSTRAINT [FK__Folders__SiteId__59FA5E80] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
ALTER TABLE [dbo].[Folders] CHECK CONSTRAINT [FK__Folders__SiteId__59FA5E80]
GO
/****** Object:  ForeignKey [FK__Commodity__Conte__40F9A68C]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[Commodity]  WITH CHECK ADD  CONSTRAINT [FK__Commodity__Conte__40F9A68C] FOREIGN KEY([ContextId])
REFERENCES [dbo].[Contexts] ([ContextId])
GO
ALTER TABLE [dbo].[Commodity] CHECK CONSTRAINT [FK__Commodity__Conte__40F9A68C]
GO
/****** Object:  ForeignKey [FK__Commodity__SiteI__41EDCAC5]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[Commodity]  WITH CHECK ADD FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
/****** Object:  ForeignKey [FK__Groups__SiteId__0519C6AF]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[Groups]  WITH CHECK ADD  CONSTRAINT [FK__Groups__SiteId__0519C6AF] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
ALTER TABLE [dbo].[Groups] CHECK CONSTRAINT [FK__Groups__SiteId__0519C6AF]
GO
/****** Object:  ForeignKey [FK__GroupRole__Group__5CD6CB2B]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[GroupRoles]  WITH CHECK ADD FOREIGN KEY([GroupId])
REFERENCES [dbo].[Groups] ([GroupId])
GO
/****** Object:  ForeignKey [FK__GroupRole__RoleI__5DCAEF64]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[GroupRoles]  WITH CHECK ADD FOREIGN KEY([RoleId])
REFERENCES [dbo].[Roles] ([RoleId])
GO
/****** Object:  ForeignKey [FK__GroupRole__SiteI__5EBF139D]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[GroupRoles]  WITH CHECK ADD FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
/****** Object:  ForeignKey [FK__Graphs__Applicat__4D94879B]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[Graphs]  WITH CHECK ADD  CONSTRAINT [FK__Graphs__Applicat__4D94879B] FOREIGN KEY([ApplicationId])
REFERENCES [dbo].[Applications] ([ApplicationId])
GO
ALTER TABLE [dbo].[Graphs] CHECK CONSTRAINT [FK__Graphs__Applicat__4D94879B]
GO
/****** Object:  ForeignKey [FK__Graphs__SiteId__7720AD13]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[Graphs]  WITH CHECK ADD FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
/****** Object:  ForeignKey [FK_BindingConfig_Applications]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[BindingConfig]  WITH CHECK ADD  CONSTRAINT [FK_BindingConfig_Applications] FOREIGN KEY([ApplicationId])
REFERENCES [dbo].[Applications] ([ApplicationId])
GO
ALTER TABLE [dbo].[BindingConfig] CHECK CONSTRAINT [FK_BindingConfig_Applications]
GO
/****** Object:  ForeignKey [FK_ApplicationSettings_Applications]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[ApplicationSettings]  WITH CHECK ADD  CONSTRAINT [FK_ApplicationSettings_Applications] FOREIGN KEY([ApplicationId])
REFERENCES [dbo].[Applications] ([ApplicationId])
GO
ALTER TABLE [dbo].[ApplicationSettings] CHECK CONSTRAINT [FK_ApplicationSettings_Applications]
GO
/****** Object:  ForeignKey [FK_ApplicationSettings_Settings]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[ApplicationSettings]  WITH CHECK ADD  CONSTRAINT [FK_ApplicationSettings_Settings] FOREIGN KEY([settingId])
REFERENCES [dbo].[Settings] ([SettingId])
GO
ALTER TABLE [dbo].[ApplicationSettings] CHECK CONSTRAINT [FK_ApplicationSettings_Settings]
GO
/****** Object:  ForeignKey [FK__Exchanges__Commo__47A6A41B]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[Exchanges]  WITH CHECK ADD  CONSTRAINT [FK__Exchanges__Commo__47A6A41B] FOREIGN KEY([CommodityId])
REFERENCES [dbo].[Commodity] ([CommodityId])
GO
ALTER TABLE [dbo].[Exchanges] CHECK CONSTRAINT [FK__Exchanges__Commo__47A6A41B]
GO
/****** Object:  ForeignKey [FK_Dictionary_Applications]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[Dictionary]  WITH CHECK ADD  CONSTRAINT [FK_Dictionary_Applications] FOREIGN KEY([ApplicationID])
REFERENCES [dbo].[Applications] ([ApplicationId])
GO
ALTER TABLE [dbo].[Dictionary] CHECK CONSTRAINT [FK_Dictionary_Applications]
GO
/****** Object:  ForeignKey [FK__RolePermi__Permi__1332DBDC]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[RolePermissions]  WITH CHECK ADD  CONSTRAINT [FK__RolePermi__Permi__1332DBDC] FOREIGN KEY([PermissionId])
REFERENCES [dbo].[Permissions] ([PermissionId])
GO
ALTER TABLE [dbo].[RolePermissions] CHECK CONSTRAINT [FK__RolePermi__Permi__1332DBDC]
GO
/****** Object:  ForeignKey [FK_Role_RoleId]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[RolePermissions]  WITH CHECK ADD  CONSTRAINT [FK_Role_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[Roles] ([RoleId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[RolePermissions] CHECK CONSTRAINT [FK_Role_RoleId]
GO
/****** Object:  ForeignKey [FK_Site_SiteId]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[RolePermissions]  WITH CHECK ADD  CONSTRAINT [FK_Site_SiteId] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[RolePermissions] CHECK CONSTRAINT [FK_Site_SiteId]
GO
/****** Object:  ForeignKey [FK__ResourceG__Group__619B8048]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[ResourceGroups]  WITH CHECK ADD FOREIGN KEY([GroupId])
REFERENCES [dbo].[Groups] ([GroupId])
GO
/****** Object:  ForeignKey [FK__ResourceG__Resou__7C1A6C5A]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[ResourceGroups]  WITH CHECK ADD FOREIGN KEY([ResourceTypeId])
REFERENCES [dbo].[ResourceType] ([ResourceTypeId])
GO
/****** Object:  ForeignKey [FK__ResourceG__SiteI__628FA481]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[ResourceGroups]  WITH CHECK ADD FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
/****** Object:  ForeignKey [FK__UserGroup__Group__6477ECF3]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[UserGroups]  WITH CHECK ADD FOREIGN KEY([GroupId])
REFERENCES [dbo].[Groups] ([GroupId])
GO
/****** Object:  ForeignKey [FK__UserGroup__SiteI__656C112C]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[UserGroups]  WITH CHECK ADD FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
/****** Object:  ForeignKey [FK__UserGroup__UserI__66603565]    Script Date: 09/15/2014 12:09:55 ******/
ALTER TABLE [dbo].[UserGroups]  WITH CHECK ADD FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([UserId])
GO
/****** Object:  ForeignKey [FK_ValueListMap_Applications]    Script Date: 09/15/2014 12:10:09 ******/
ALTER TABLE [dbo].[ValueListMap]  WITH CHECK ADD  CONSTRAINT [FK_ValueListMap_Applications] FOREIGN KEY([ApplicationId])
REFERENCES [dbo].[Applications] ([ApplicationId])
GO
ALTER TABLE [dbo].[ValueListMap] CHECK CONSTRAINT [FK_ValueListMap_Applications]
GO
/****** Object:  ForeignKey [FK_DataObjects_Dictionary]    Script Date: 09/15/2014 12:10:25 ******/
ALTER TABLE [dbo].[DataObjects]  WITH CHECK ADD  CONSTRAINT [FK_DataObjects_Dictionary] FOREIGN KEY([DictionaryId])
REFERENCES [dbo].[Dictionary] ([Dictionaryid])
GO
ALTER TABLE [dbo].[DataObjects] CHECK CONSTRAINT [FK_DataObjects_Dictionary]
GO
/****** Object:  ForeignKey [FK_PickList_Dictionary]    Script Date: 09/15/2014 12:10:25 ******/
ALTER TABLE [dbo].[PickList]  WITH CHECK ADD  CONSTRAINT [FK_PickList_Dictionary] FOREIGN KEY([DictionaryId])
REFERENCES [dbo].[Dictionary] ([Dictionaryid])
GO
ALTER TABLE [dbo].[PickList] CHECK CONSTRAINT [FK_PickList_Dictionary]
GO
/****** Object:  ForeignKey [FK_DataRelationships_DataObjects]    Script Date: 09/15/2014 12:10:25 ******/
ALTER TABLE [dbo].[DataRelationships]  WITH CHECK ADD  CONSTRAINT [FK_DataRelationships_DataObjects] FOREIGN KEY([DataObjectID])
REFERENCES [dbo].[DataObjects] ([DataObjectId])
GO
ALTER TABLE [dbo].[DataRelationships] CHECK CONSTRAINT [FK_DataRelationships_DataObjects]
GO
/****** Object:  ForeignKey [FK_DataProperties_DataObjects]    Script Date: 09/15/2014 12:10:25 ******/
ALTER TABLE [dbo].[DataProperties]  WITH CHECK ADD  CONSTRAINT [FK_DataProperties_DataObjects] FOREIGN KEY([DataObjectId])
REFERENCES [dbo].[DataObjects] ([DataObjectId])
GO
ALTER TABLE [dbo].[DataProperties] CHECK CONSTRAINT [FK_DataProperties_DataObjects]
GO
/****** Object:  ForeignKey [FK_DataProperties_PickList]    Script Date: 09/15/2014 12:10:25 ******/
ALTER TABLE [dbo].[DataProperties]  WITH CHECK ADD  CONSTRAINT [FK_DataProperties_PickList] FOREIGN KEY([PickListId])
REFERENCES [dbo].[PickList] ([PickListId])
GO
ALTER TABLE [dbo].[DataProperties] CHECK CONSTRAINT [FK_DataProperties_PickList]
GO
/****** Object:  ForeignKey [FK_KeyProperty_DataObjects]    Script Date: 09/15/2014 12:10:25 ******/
ALTER TABLE [dbo].[KeyProperty]  WITH CHECK ADD  CONSTRAINT [FK_KeyProperty_DataObjects] FOREIGN KEY([DataObjectId])
REFERENCES [dbo].[DataObjects] ([DataObjectId])
GO
ALTER TABLE [dbo].[KeyProperty] CHECK CONSTRAINT [FK_KeyProperty_DataObjects]
GO
/****** Object:  ForeignKey [FK_PropertyMap_DataRelationships]    Script Date: 09/15/2014 12:10:25 ******/
ALTER TABLE [dbo].[PropertyMap]  WITH CHECK ADD  CONSTRAINT [FK_PropertyMap_DataRelationships] FOREIGN KEY([RelationshipID])
REFERENCES [dbo].[DataRelationships] ([RelationshipId])
GO
ALTER TABLE [dbo].[PropertyMap] CHECK CONSTRAINT [FK_PropertyMap_DataRelationships]
GO
