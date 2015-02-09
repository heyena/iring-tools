USE [master]
GO
/****** Object:  Database [iRINGConfig]    Script Date: 02/09/2015 11:49:38 ******/
CREATE DATABASE [iRINGConfig] ON  PRIMARY 
( NAME = N'iRINGConfig', FILENAME = N'E:\Microsoft SQL Server\MSSQL10_50.MULTAPP\MSSQL\DATA\iRINGConfig.mdf' , SIZE = 4096KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
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
/****** Object:  User [iRINGConfig]    Script Date: 02/09/2015 11:49:38 ******/
CREATE USER [iRINGConfig] FOR LOGIN [iRINGConfig] WITH DEFAULT_SCHEMA=[dbo]
GO
/****** Object:  Table [dbo].[Sites]    Script Date: 02/09/2015 11:49:45 ******/
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
/****** Object:  Table [dbo].[Settings]    Script Date: 02/09/2015 11:49:45 ******/
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
/****** Object:  Table [dbo].[ScheduleExchange]    Script Date: 02/09/2015 11:49:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ScheduleExchange](
	[Schedule_Exchange_Id] [uniqueidentifier] NOT NULL,
	[Task_Name] [nvarchar](100) NOT NULL,
	[Scope] [nvarchar](64) NULL,
	[Base_Url] [nvarchar](256) NULL,
	[Exchange_Id] [nvarchar](64) NULL,
	[Sso_Url] [nvarchar](256) NULL,
	[Client_Id] [nvarchar](64) NULL,
	[Client_Secret] [nvarchar](64) NULL,
	[Grant_Type] [nvarchar](64) NULL,
	[Request_Timeout] [int] NULL,
	[Start_Time] [datetime] NOT NULL,
	[End_Time] [datetime] NULL,
	[Created_Date] [datetime] NOT NULL,
	[Created_By] [nvarchar](100) NOT NULL,
	[Occurance] [nvarchar](24) NOT NULL,
	[NextStart_Date_Time] [datetime] NULL,
	[End_Date_Time] [datetime] NOT NULL,
	[Status] [nvarchar](64) NOT NULL,
	[Active] [tinyint] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Schedule_Exchange_Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ScheduleCache]    Script Date: 02/09/2015 11:49:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ScheduleCache](
	[Schedule_Cache_Id] [uniqueidentifier] NOT NULL,
	[Task_Name] [nvarchar](100) NOT NULL,
	[Project] [nvarchar](64) NOT NULL,
	[App] [nvarchar](64) NOT NULL,
	[Cache_Page_Size] [int] NULL,
	[Sso_Url] [nvarchar](256) NULL,
	[Client_Id] [nvarchar](64) NULL,
	[Client_Secret] [nvarchar](64) NULL,
	[Grant_Type] [nvarchar](64) NULL,
	[App_Key] [nvarchar](64) NULL,
	[Access_Token] [nvarchar](64) NULL,
	[Request_Timeout] [int] NULL,
	[Start_Time] [datetime] NOT NULL,
	[End_Time] [datetime] NULL,
	[Created_Date] [datetime] NOT NULL,
	[Created_By] [nvarchar](100) NOT NULL,
	[Occurance] [nvarchar](24) NOT NULL,
	[NextStart_Date_Time] [datetime] NULL,
	[End_Date_Time] [datetime] NOT NULL,
	[Status] [nvarchar](64) NOT NULL,
	[Active] [tinyint] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Schedule_Cache_Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [dbo].[ScheduleCache] ([Schedule_Cache_Id], [Task_Name], [Project], [App], [Cache_Page_Size], [Sso_Url], [Client_Id], [Client_Secret], [Grant_Type], [App_Key], [Access_Token], [Request_Timeout], [Start_Time], [End_Time], [Created_Date], [Created_By], [Occurance], [NextStart_Date_Time], [End_Date_Time], [Status], [Active]) VALUES (N'e789c9b5-e153-e411-87e4-0050568c450e', N'agent.source', N'agent', N'source', 2000, N'https://sso.mypsn.com/as/token.oauth2', N'iRingTools', N'0Lvnvat5T5OJk5n6VwD4optFJoq7/0POq++NfYkIgHYtmy6Pluix3aGy7EAN1Jxp', N'client_credentials', N'wHKxvUyEqrLTNSvsVTPX1GJs02nAo5IF', N'TmMopozebXnR8ky6YgRnAV22ICOz', 300000, CAST(0x0000A3800062E080 AS DateTime), CAST(0x0000000000000000 AS DateTime), CAST(0x0000A38000735B40 AS DateTime), N'Gakhar Hemant', N'Daily', CAST(0x0000000000000000 AS DateTime), CAST(0x0000A3C60062E080 AS DateTime), N'Ready', 1)
/****** Object:  Table [dbo].[MimeType]    Script Date: 02/09/2015 11:49:45 ******/
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
/****** Object:  Table [dbo].[ResourceType]    Script Date: 02/09/2015 11:49:45 ******/
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
/****** Object:  Table [dbo].[DataLayers]    Script Date: 02/09/2015 11:49:45 ******/
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
/****** Object:  Table [dbo].[DataFilterValues]    Script Date: 02/09/2015 11:49:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DataFilterValues](
	[Sno] [int] IDENTITY(1,1) NOT NULL,
	[DataFilterId] [uniqueidentifier] NULL,
	[Value] [nvarchar](200) NULL,
 CONSTRAINT [PK_DataFilterValues] PRIMARY KEY CLUSTERED 
(
	[Sno] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[DataFilterValues] ON
INSERT [dbo].[DataFilterValues] ([Sno], [DataFilterId], [Value]) VALUES (23, N'edbaaedc-8652-4400-b47d-eb7caeb7e049', N'2')
INSERT [dbo].[DataFilterValues] ([Sno], [DataFilterId], [Value]) VALUES (24, N'edbaaedc-8652-4400-b47d-eb7caeb7e049', N'3')
INSERT [dbo].[DataFilterValues] ([Sno], [DataFilterId], [Value]) VALUES (30, N'03af9461-ffbc-4f06-b718-bdc0bf8a882e', N'1')
INSERT [dbo].[DataFilterValues] ([Sno], [DataFilterId], [Value]) VALUES (31, N'a1b31410-c1f1-4cbd-98c6-481d6805cdee', N'2Deepak')
INSERT [dbo].[DataFilterValues] ([Sno], [DataFilterId], [Value]) VALUES (32, N'a5c95abd-e31d-4804-930d-e13f55a17547', N'1')
INSERT [dbo].[DataFilterValues] ([Sno], [DataFilterId], [Value]) VALUES (33, N'80af39c6-85aa-4f97-a3a9-c0b952b87afe', N'2')
INSERT [dbo].[DataFilterValues] ([Sno], [DataFilterId], [Value]) VALUES (34, N'bfcb743b-bed8-4a0a-ac66-a6997f0f9c2a', N'1')
INSERT [dbo].[DataFilterValues] ([Sno], [DataFilterId], [Value]) VALUES (35, N'8141f1b3-5f23-458e-8e3a-410b246ac9da', N'2')
INSERT [dbo].[DataFilterValues] ([Sno], [DataFilterId], [Value]) VALUES (53, N'a56626db-0c8a-493f-a775-84941ab01c29', N'33')
INSERT [dbo].[DataFilterValues] ([Sno], [DataFilterId], [Value]) VALUES (54, N'e6930401-3490-4360-b8ba-f5d51a741b3b', N'12')
INSERT [dbo].[DataFilterValues] ([Sno], [DataFilterId], [Value]) VALUES (55, N'134ac89f-6f0e-4fe6-8de3-584b2a5bb5b2', N'12')
INSERT [dbo].[DataFilterValues] ([Sno], [DataFilterId], [Value]) VALUES (56, N'a8e2dafe-4efe-4309-b1cb-993f771fcecc', N'2')
INSERT [dbo].[DataFilterValues] ([Sno], [DataFilterId], [Value]) VALUES (57, N'a8e2dafe-4efe-4309-b1cb-993f771fcecc', N'3')
INSERT [dbo].[DataFilterValues] ([Sno], [DataFilterId], [Value]) VALUES (58, N'6ed0eb7a-940e-4596-8ef6-229396f5f82f', N'3')
INSERT [dbo].[DataFilterValues] ([Sno], [DataFilterId], [Value]) VALUES (59, N'459f4836-0529-4a74-a827-19d6cf9589a3', N'09')
INSERT [dbo].[DataFilterValues] ([Sno], [DataFilterId], [Value]) VALUES (62, N'5fd12251-e4b0-4ca8-b76e-b812cd485d19', N'44')
INSERT [dbo].[DataFilterValues] ([Sno], [DataFilterId], [Value]) VALUES (72, N'aebe39d6-33e0-4b0d-8c58-35de0865e8ec', N'66')
INSERT [dbo].[DataFilterValues] ([Sno], [DataFilterId], [Value]) VALUES (73, N'070eb004-d137-4da6-94fe-4606b1df3e4e', N'12')
SET IDENTITY_INSERT [dbo].[DataFilterValues] OFF
/****** Object:  Table [dbo].[DataFiltersType]    Script Date: 02/09/2015 11:49:45 ******/
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
/****** Object:  Table [dbo].[DataFilters]    Script Date: 02/09/2015 11:49:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DataFilters](
	[DataFilterId] [uniqueidentifier] NOT NULL,
	[ResourceId] [uniqueidentifier] NOT NULL,
	[DataFilterTypeId] [int] NOT NULL,
	[IsExpression] [int] NOT NULL,
	[DFOrder] [int] NULL,
	[OpenGroupCount] [int] NULL,
	[LogicalOperator] [nvarchar](50) NULL,
	[PropertyName] [nvarchar](350) NULL,
	[RelationalOperator] [nvarchar](150) NULL,
	[CloseGroupCount] [int] NULL,
	[SortOrder] [nvarchar](25) NULL,
	[SiteId] [int] NOT NULL,
	[Active] [tinyint] NOT NULL,
	[IsAdmin] [bit] NULL,
	[IsCaseSensitive] [bit] NULL,
 CONSTRAINT [PK_DataFilters] PRIMARY KEY CLUSTERED 
(
	[DataFilterId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [dbo].[DataFilters] ([DataFilterId], [ResourceId], [DataFilterTypeId], [IsExpression], [DFOrder], [OpenGroupCount], [LogicalOperator], [PropertyName], [RelationalOperator], [CloseGroupCount], [SortOrder], [SiteId], [Active], [IsAdmin], [IsCaseSensitive]) VALUES (N'bfc0930f-e06f-4f4c-87bc-085c0d5e583a', N'ee565602-ba52-422c-a01f-932490794343', 3, 0, NULL, NULL, NULL, N'PipingNetworkSystem.IdentificationByTag.valIdentifier', NULL, NULL, N'Desc', 1, 1, 0, NULL)
INSERT [dbo].[DataFilters] ([DataFilterId], [ResourceId], [DataFilterTypeId], [IsExpression], [DFOrder], [OpenGroupCount], [LogicalOperator], [PropertyName], [RelationalOperator], [CloseGroupCount], [SortOrder], [SiteId], [Active], [IsAdmin], [IsCaseSensitive]) VALUES (N'8b9bb39e-a642-4435-b717-1e3619409c7a', N'd208b9b8-0372-4e2c-9b34-78b9bb453b61', 2, 0, 1, NULL, NULL, N'xyz.abc.valueIdentifier', NULL, NULL, N'Asc', 1, 1, 1, 0)
INSERT [dbo].[DataFilters] ([DataFilterId], [ResourceId], [DataFilterTypeId], [IsExpression], [DFOrder], [OpenGroupCount], [LogicalOperator], [PropertyName], [RelationalOperator], [CloseGroupCount], [SortOrder], [SiteId], [Active], [IsAdmin], [IsCaseSensitive]) VALUES (N'6ed0eb7a-940e-4596-8ef6-229396f5f82f', N'd208b9b8-0372-4e2c-9b34-78b9bb453b61', 2, 1, 2, 1, N'Or', N'xyz.abc.valuwIdentifier', N'NotEqualTo', 2, NULL, 1, 1, 1, 1)
INSERT [dbo].[DataFilters] ([DataFilterId], [ResourceId], [DataFilterTypeId], [IsExpression], [DFOrder], [OpenGroupCount], [LogicalOperator], [PropertyName], [RelationalOperator], [CloseGroupCount], [SortOrder], [SiteId], [Active], [IsAdmin], [IsCaseSensitive]) VALUES (N'aebe39d6-33e0-4b0d-8c58-35de0865e8ec', N'ee565602-ba52-422c-a01f-932490794343', 3, 1, 1, 0, NULL, N'PipingNetworkSystem.IdentificationByTag.valIdentifier', N'EqualTo', 0, NULL, 1, 1, 0, 0)
INSERT [dbo].[DataFilters] ([DataFilterId], [ResourceId], [DataFilterTypeId], [IsExpression], [DFOrder], [OpenGroupCount], [LogicalOperator], [PropertyName], [RelationalOperator], [CloseGroupCount], [SortOrder], [SiteId], [Active], [IsAdmin], [IsCaseSensitive]) VALUES (N'8141f1b3-5f23-458e-8e3a-410b246ac9da', N'bcbae4fd-7beb-4695-9d6b-d96a863eac9e', 2, 1, NULL, NULL, N'Or', N'IAREA', N'NotEqualTo', NULL, NULL, 1, 1, 0, 0)
INSERT [dbo].[DataFilters] ([DataFilterId], [ResourceId], [DataFilterTypeId], [IsExpression], [DFOrder], [OpenGroupCount], [LogicalOperator], [PropertyName], [RelationalOperator], [CloseGroupCount], [SortOrder], [SiteId], [Active], [IsAdmin], [IsCaseSensitive]) VALUES (N'070eb004-d137-4da6-94fe-4606b1df3e4e', N'ee565602-ba52-422c-a01f-932490794343', 3, 1, 2, 0, N'And', N'PipingNetworkSystem.NominalDiameter.hasScale', N'EqualTo', 0, NULL, 1, 1, 0, 0)
INSERT [dbo].[DataFilters] ([DataFilterId], [ResourceId], [DataFilterTypeId], [IsExpression], [DFOrder], [OpenGroupCount], [LogicalOperator], [PropertyName], [RelationalOperator], [CloseGroupCount], [SortOrder], [SiteId], [Active], [IsAdmin], [IsCaseSensitive]) VALUES (N'a1b31410-c1f1-4cbd-98c6-481d6805cdee', N'3915d9dd-55bc-47a7-a081-2f4a252cf809', 2, 1, NULL, NULL, NULL, N'tbl2_col2', N'NotEqualTo', NULL, NULL, 1, 1, 0, 0)
INSERT [dbo].[DataFilters] ([DataFilterId], [ResourceId], [DataFilterTypeId], [IsExpression], [DFOrder], [OpenGroupCount], [LogicalOperator], [PropertyName], [RelationalOperator], [CloseGroupCount], [SortOrder], [SiteId], [Active], [IsAdmin], [IsCaseSensitive]) VALUES (N'b9736715-ecb4-412f-a7fb-48bff03d826b', N'3915d9dd-55bc-47a7-a081-2f4a252cf809', 2, 0, NULL, NULL, NULL, N'tbl2_col2', NULL, NULL, NULL, 1, 1, 0, 0)
INSERT [dbo].[DataFilters] ([DataFilterId], [ResourceId], [DataFilterTypeId], [IsExpression], [DFOrder], [OpenGroupCount], [LogicalOperator], [PropertyName], [RelationalOperator], [CloseGroupCount], [SortOrder], [SiteId], [Active], [IsAdmin], [IsCaseSensitive]) VALUES (N'55785314-6871-474d-9570-51b85e2903d0', N'8d1e8209-d806-436e-a36d-b9ce1315c59d', 2, 0, NULL, NULL, NULL, N'ID', NULL, NULL, NULL, 1, 1, 0, 0)
INSERT [dbo].[DataFilters] ([DataFilterId], [ResourceId], [DataFilterTypeId], [IsExpression], [DFOrder], [OpenGroupCount], [LogicalOperator], [PropertyName], [RelationalOperator], [CloseGroupCount], [SortOrder], [SiteId], [Active], [IsAdmin], [IsCaseSensitive]) VALUES (N'134ac89f-6f0e-4fe6-8de3-584b2a5bb5b2', N'ee565602-ba52-422c-a01f-932490796666', 3, 1, NULL, 0, NULL, N'PipingNetworkSystem.IdentificationByTag.valIdentifier', N'EqualTo', 0, NULL, 1, 1, 0, 0)
INSERT [dbo].[DataFilters] ([DataFilterId], [ResourceId], [DataFilterTypeId], [IsExpression], [DFOrder], [OpenGroupCount], [LogicalOperator], [PropertyName], [RelationalOperator], [CloseGroupCount], [SortOrder], [SiteId], [Active], [IsAdmin], [IsCaseSensitive]) VALUES (N'5f194ca9-0e89-477d-a666-5a6ce6882110', N'951f5dd1-e796-4f5b-bc04-07a111df3733', 2, 0, 1, NULL, NULL, N'xyz.abc.valueIdentifier', NULL, NULL, N'ASC', 1, 1, 0, 0)
INSERT [dbo].[DataFilters] ([DataFilterId], [ResourceId], [DataFilterTypeId], [IsExpression], [DFOrder], [OpenGroupCount], [LogicalOperator], [PropertyName], [RelationalOperator], [CloseGroupCount], [SortOrder], [SiteId], [Active], [IsAdmin], [IsCaseSensitive]) VALUES (N'a56626db-0c8a-493f-a775-84941ab01c29', N'ee565602-ba52-422c-a01f-932490796666', 3, 1, NULL, 0, NULL, N'PipingNetworkSystem.IdentificationByTag.valIdentifier', N'EqualTo', 0, NULL, 1, 1, 0, 0)
INSERT [dbo].[DataFilters] ([DataFilterId], [ResourceId], [DataFilterTypeId], [IsExpression], [DFOrder], [OpenGroupCount], [LogicalOperator], [PropertyName], [RelationalOperator], [CloseGroupCount], [SortOrder], [SiteId], [Active], [IsAdmin], [IsCaseSensitive]) VALUES (N'a8e2dafe-4efe-4309-b1cb-993f771fcecc', N'd208b9b8-0372-4e2c-9b34-78b9bb453b61', 2, 1, 1, 10, N'Or', N'xyz.abc.valuwIdentifier', N'NotEqualTo', 23, NULL, 1, 1, 1, 0)
INSERT [dbo].[DataFilters] ([DataFilterId], [ResourceId], [DataFilterTypeId], [IsExpression], [DFOrder], [OpenGroupCount], [LogicalOperator], [PropertyName], [RelationalOperator], [CloseGroupCount], [SortOrder], [SiteId], [Active], [IsAdmin], [IsCaseSensitive]) VALUES (N'bfcb743b-bed8-4a0a-ac66-a6997f0f9c2a', N'bcbae4fd-7beb-4695-9d6b-d96a863eac9e', 2, 1, NULL, NULL, NULL, N'INUM', N'NotEqualTo', NULL, NULL, 1, 1, 0, 0)
INSERT [dbo].[DataFilters] ([DataFilterId], [ResourceId], [DataFilterTypeId], [IsExpression], [DFOrder], [OpenGroupCount], [LogicalOperator], [PropertyName], [RelationalOperator], [CloseGroupCount], [SortOrder], [SiteId], [Active], [IsAdmin], [IsCaseSensitive]) VALUES (N'03af9461-ffbc-4f06-b718-bdc0bf8a882e', N'3b0def40-a81e-443c-ad64-c74a2406ae6e', 2, 1, NULL, NULL, NULL, N'tbl1_col1', N'NotEqualTo', NULL, NULL, 1, 1, 0, 0)
INSERT [dbo].[DataFilters] ([DataFilterId], [ResourceId], [DataFilterTypeId], [IsExpression], [DFOrder], [OpenGroupCount], [LogicalOperator], [PropertyName], [RelationalOperator], [CloseGroupCount], [SortOrder], [SiteId], [Active], [IsAdmin], [IsCaseSensitive]) VALUES (N'80af39c6-85aa-4f97-a3a9-c0b952b87afe', N'8d1e8209-d806-436e-a36d-b9ce1315c59d', 2, 1, NULL, NULL, N'Or', N'AREA', N'NotEqualTo', NULL, NULL, 1, 1, 0, 0)
INSERT [dbo].[DataFilters] ([DataFilterId], [ResourceId], [DataFilterTypeId], [IsExpression], [DFOrder], [OpenGroupCount], [LogicalOperator], [PropertyName], [RelationalOperator], [CloseGroupCount], [SortOrder], [SiteId], [Active], [IsAdmin], [IsCaseSensitive]) VALUES (N'762e0f02-dcd7-486f-9208-dbae63256ec4', N'bcbae4fd-7beb-4695-9d6b-d96a863eac9e', 2, 0, NULL, NULL, NULL, N'INUM', NULL, NULL, NULL, 1, 1, 0, 0)
INSERT [dbo].[DataFilters] ([DataFilterId], [ResourceId], [DataFilterTypeId], [IsExpression], [DFOrder], [OpenGroupCount], [LogicalOperator], [PropertyName], [RelationalOperator], [CloseGroupCount], [SortOrder], [SiteId], [Active], [IsAdmin], [IsCaseSensitive]) VALUES (N'a5c95abd-e31d-4804-930d-e13f55a17547', N'8d1e8209-d806-436e-a36d-b9ce1315c59d', 2, 1, NULL, NULL, NULL, N'ID', N'NotEqualTo', NULL, NULL, 1, 1, 0, 0)
INSERT [dbo].[DataFilters] ([DataFilterId], [ResourceId], [DataFilterTypeId], [IsExpression], [DFOrder], [OpenGroupCount], [LogicalOperator], [PropertyName], [RelationalOperator], [CloseGroupCount], [SortOrder], [SiteId], [Active], [IsAdmin], [IsCaseSensitive]) VALUES (N'edbaaedc-8652-4400-b47d-eb7caeb7e049', N'951f5dd1-e796-4f5b-bc04-07a111df3733', 2, 1, 1, 10, NULL, N'xyz.abc.valuwIdentifier', N'contains', 10, NULL, 1, 1, 0, 0)
/****** Object:  Table [dbo].[Contexts]    Script Date: 02/09/2015 11:49:45 ******/
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
INSERT [dbo].[Contexts] ([ContextId], [DisplayName], [InternalName], [Description], [CacheConnStr], [SiteId], [Active], [FolderId]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', N'Test5u', N'IN', N'descriptionu', N'cacheu', 1, 1, N'6914f8d4-636f-4d9f-aa43-45f865de56b6')
INSERT [dbo].[Contexts] ([ContextId], [DisplayName], [InternalName], [Description], [CacheConnStr], [SiteId], [Active], [FolderId]) VALUES (N'54dc03c6-b1ed-4a4e-b4d9-773d4c2651f8', N'test App Display Name3', N'iTest3', N'test description3', N'abc2', 3, 1, N'6914f8d4-636f-4d9f-aa43-45f865de56b6')
INSERT [dbo].[Contexts] ([ContextId], [DisplayName], [InternalName], [Description], [CacheConnStr], [SiteId], [Active], [FolderId]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'iTest44', N'12345_000', N'test description put', N'abc', 1, 1, N'6914f8d4-636f-4d9f-aa43-45f865de56b6')
INSERT [dbo].[Contexts] ([ContextId], [DisplayName], [InternalName], [Description], [CacheConnStr], [SiteId], [Active], [FolderId]) VALUES (N'5ddc49b3-1599-47e2-9f9a-ad37413d28a7', N'iTest', N'iTest', N'test description', N'abc', 1, 1, N'6914f8d4-636f-4d9f-aa43-45f865de56b6')
INSERT [dbo].[Contexts] ([ContextId], [DisplayName], [InternalName], [Description], [CacheConnStr], [SiteId], [Active], [FolderId]) VALUES (N'ed90e5f5-a716-49a3-bbb5-ca2ee57e140e', N'Scope2DN', N'Scope2IN', N'Scope2Desc', N'CacheConnStr', 1, 1, N'6914f8d4-636f-4d9f-aa43-45f865de56b6')
INSERT [dbo].[Contexts] ([ContextId], [DisplayName], [InternalName], [Description], [CacheConnStr], [SiteId], [Active], [FolderId]) VALUES (N'e61137eb-fd18-4c31-93a1-d0c39bd1001a', N'iTest2', N'iTest2', N'test description', N'abc2', 1, 1, N'6914f8d4-636f-4d9f-aa43-45f865de56b6')
INSERT [dbo].[Contexts] ([ContextId], [DisplayName], [InternalName], [Description], [CacheConnStr], [SiteId], [Active], [FolderId]) VALUES (N'd3c26e6c-8018-4d74-916a-f5c65f67b66d', N'Scope1DN', N'Scope1iN', N'Scope1Desc', N'CacheConnStr', 1, 1, N'6914f8d4-636f-4d9f-aa43-45f865de56b6')
INSERT [dbo].[Contexts] ([ContextId], [DisplayName], [InternalName], [Description], [CacheConnStr], [SiteId], [Active], [FolderId]) VALUES (N'e78d29e3-fdcd-4636-b464-ff265701e528', N'test App Display Name4', N'iTest4', N'temp', N'abc2', 3, 1, N'6914f8d4-636f-4d9f-aa43-45f865de56b6')
/****** Object:  Table [dbo].[Extensions]    Script Date: 02/09/2015 11:49:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Extensions](
	[ExtensionId] [uniqueidentifier] NOT NULL,
	[DataObjectId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](250) NOT NULL,
	[ColumnName] [nvarchar](250) NULL,
	[PropertyName] [nvarchar](250) NULL,
	[DataType] [nvarchar](50) NULL,
	[DataLength] [int] NULL,
	[Nullable] [smallint] NULL,
	[NumberOfDecimals] [int] NULL,
	[KeyType] [nvarchar](50) NULL,
	[Precision] [int] NULL,
	[Scale] [int] NULL,
	[Definition] [nvarchar](1000) NULL
) ON [PRIMARY]
GO
/****** Object:  UserDefinedFunction [dbo].[Split]    Script Date: 02/09/2015 11:49:49 ******/
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
/****** Object:  StoredProcedure [dbo].[spuDataFilter]    Script Date: 02/09/2015 11:49:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Deepak Kumar>
-- Create date: <17-Sep-2014>
-- Description:	<updating datafilter>
-- ===========================================================
CREATE PROCEDURE [dbo].[spuDataFilter] 
(
	@ResourceId uniqueidentifier,
	@DataFilterTypeId int,
	@SiteId int,
	@RawXml xml
)	 
AS

	BEGIN 
	
	
--Set @RawXml = '<dataFilters>
--	<dataFilter>
--		<dataFilterId>00000000-0000-0000-0000-000000000000</dataFilterId>
--		<resourceId>d208b9b8-0372-4e2c-9b34-78b9bb453b61</resourceId>
--		<dataFilterTypeId>2</dataFilterTypeId>
--		<siteId>1</siteId>
--		<active>1</active>
--		<expressions>
--			<expression>
--				<dfOrder>1</dfOrder>
--				<openGroupCount>10</openGroupCount>
--				<propertyName>xyz.abc.valuwIdentifier</propertyName>
--				<relationalOperator>contains</relationalOperator>
--				<values>
--					<value>2</value>
--					<value>3</value>
--				</values>
--				<logicalOperator>Or</logicalOperator>
--				<closeGroupCount>23</closeGroupCount>
--				<isCaseSensitive>false</isCaseSensitive>
--			</expression>
--			<expression>
--				<dfOrder>2</dfOrder>
--				<openGroupCount>1</openGroupCount>
--				<propertyName>xyz.abc.valuwIdentifier</propertyName>
--				<relationalOperator>contains</relationalOperator>
--				<values>
--					<value>3</value>
--				</values>
--				<logicalOperator>AND</logicalOperator>
--				<closeGroupCount>2</closeGroupCount>
--				<isCaseSensitive>true</isCaseSensitive>
--			</expression>
--		</expressions>
--		<orderExpressions>
--			<orderExpression>
--				<dfOrder>1</dfOrder>
--				<propertyName>xyz.abc.valueIdentifier</propertyName>
--				<sortOrder>ASC</sortOrder>
--			</orderExpression>
--		</orderExpressions>
--		<isAdmin>true</isAdmin>
--	</dataFilter>
--</dataFilters>'

--This is temporary solution, need to find some workaround
Set @RawXml = REPLACE(Cast(@RawXml as varchar(max)),'<value>','<value>,')

	
	--[[deleting the exitsting stuff [had a discussion with vidisha for the same, date 23-Sep-2014] 
	Delete d from DataFilterValues d inner join  DataFilters df on d.DataFilterId = df.DataFilterId and df.ResourceId = @ResourceId
	
	Delete from DataFilters where ResourceId = @ResourceId
	--]]
	
	Create table #Expression
	(
		DataFilterId	uniqueidentifier default newid(),
		ResourceId	uniqueidentifier,
		DataFilterTypeId	int,
		IsExpression	int default 1,
		DFOrder	int,
		OpenGroupCount	int default null,
		LogicalOperator	nvarchar(50) default null,
		PropertyName	nvarchar(350) default null,
		RelationalOperator	nvarchar(150) default null,
		[Values] varchar(1000),
		CloseGroupCount	int default null,
		SiteId	int,
		IsCaseSensitive bit,
		IsAdmin bit
		
	)	

	Create table #OrderExpression
	(
		DataFilterId	uniqueidentifier default newid(),
		ResourceId	uniqueidentifier,
		DataFilterTypeId	int,
		IsExpression	int default 0,
		DFOrder	int,
		PropertyName	nvarchar(350) default null,
		SortOrder	nvarchar(25) default null,
		SiteId	int,
		IsAdmin bit
	)


SELECT       
 @ResourceId as ResourceId,      
 @SiteId as SiteId,     
 @DataFilterTypeId as DataFilterTypeId,
 nref.value('isAdmin[1]', 'bit') IsAdmin           
into #Parent FROM   @rawXML.nodes('//dataFilter') AS R(nref)
	

	INSERT INTO #Expression
			   (
				[ResourceId]
			   ,[DataFilterTypeId]
			   ,[DFOrder]
			   ,[OpenGroupCount]
			   ,[LogicalOperator]
			   ,[PropertyName]
			   ,[RelationalOperator]
			   ,[Values]
			   ,[CloseGroupCount]
			   ,[SiteId]
			   ,[IsCaseSensitive]
			   ,[IsAdmin]  
			   )
	SELECT
	 ResourceId,
	 DataFilterTypeId,
	 nref.value('dfOrder[1]', 'int') DFOrder,      
	 nref.value('openGroupCount[1]', 'int') OpenGroupCount,      
	 nref.value('logicalOperator[1]', 'nvarchar(50)') LogicalOperator,
	 nref.value('propertyName[1]', 'nvarchar(350)') PropertyName,
	 nref.value('relationalOperator[1]', 'nvarchar(150)') RelationalOperator,
	 nref.value('values[1]','varchar(500)') [values],
	 nref.value('closeGroupCount[1]', 'int') CloseGroupCount,
	 SiteId,
	 nref.value('isCaseSensitive[1]', 'bit') IsCaseSensitive,
	 IsAdmin
	FROM  @rawXML.nodes('//expression') AS R(nref),#Parent 

	

	Insert Into #OrderExpression
	(
		ResourceId,
		DataFilterTypeId,
		DFOrder,
		PropertyName,
		SortOrder,
		SiteId,
		IsAdmin
	)
	SELECT
	 ResourceId,
	 DataFilterTypeId,
	 nref.value('dfOrder[1]', 'int') DFOrder,      
	 nref.value('propertyName[1]', 'nvarchar(350)') PropertyName,
	 nref.value('sortOrder[1]', 'nvarchar(25)') SortOrder,
	 SiteId,
	 IsAdmin
	FROM  @rawXML.nodes('//orderExpression') AS R(nref),#Parent 	
   
   
   

	
	INSERT INTO [DataFilters]
           ([DataFilterId]
           ,[ResourceId]
           ,[DataFilterTypeId]
           ,[IsExpression]
           ,[DFOrder]
           ,[OpenGroupCount]
           ,[LogicalOperator]
           ,[PropertyName]
           ,[RelationalOperator]
           ,[CloseGroupCount]
           ,[SortOrder]
           ,[SiteId]
           ,[IsCaseSensitive]
           ,[IsAdmin]  
           )
	select 
		DataFilterId,
		ResourceId,
		DataFilterTypeId,
		IsExpression,
		DFOrder,
		OpenGroupCount,
		LogicalOperator,
		PropertyName,
		RelationalOperator,
		CloseGroupCount,
		Null as SortOrder,
		SiteId,
		IsCaseSensitive,
		IsAdmin
	 from #Expression
	 union all
	select 
		DataFilterId,
		ResourceId,
		DataFilterTypeId,
		IsExpression,
		DFOrder,
		Null as OpenGroupCount,
		Null as LogicalOperator,
		PropertyName,
		Null as RelationalOperator,
		Null as CloseGroupCount,
		SortOrder,
		SiteId,
		Null as IsCaseSensitive,
		IsAdmin
	 from #OrderExpression


Insert into  DataFilterValues 
Select DataFilterId,Items as value from #Expression E Cross Apply dbo.Split(E.[Values],',') S 
where S.Items <> ''

--drop table #Parent
--drop table #Expression
--drop table #OrderExpression
							

END
GO
/****** Object:  StoredProcedure [dbo].[spuSites]    Script Date: 02/09/2015 11:49:57 ******/
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
/****** Object:  StoredProcedure [dbo].[spuScheduleExchange]    Script Date: 02/09/2015 11:49:57 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
--* ==================================================
--*  Procedure ScheduleExchange Update
--* ==================================================
CREATE PROCEDURE [dbo].[spuScheduleExchange]
	@SCHEDULE_EXCHANGE_ID	nvarchar(64),
	@TASK_NAME				nvarchar(100),
	@SCOPE					nvarchar(64),
	@SSO_URL				nvarchar(256),
	@CLIENT_ID				nvarchar(64),
	@CLIENT_SECRET			nvarchar(64), 
	@GRANT_TYPE				nvarchar(64),
	@REQUEST_TIMEOUT		int,
	@START_TIME				DateTime,
	@END_TIME				DateTime,
	@CREATED_DATE			DateTime,
	@CREATED_BY				nvarchar(100),
	@OCCURANCE				nvarchar(24),
	@NEXTSTART_DATE_TIME 	DateTime,
	@END_DATE_TIME			DateTime,
	@STATUS					nvarchar(64),
	@ACTIVE					tinyint
AS
UPDATE [dbo].[ScheduleExchange] SET
	[TASK_NAME] = @TASK_NAME,
	[SCOPE] = @SCOPE,
	[SSO_URL] = @SSO_URL,
	[CLIENT_ID] = @CLIENT_ID,
	[CLIENT_SECRET] = @CLIENT_SECRET,
	[GRANT_TYPE] = @GRANT_TYPE,
	[REQUEST_TIMEOUT] = @REQUEST_TIMEOUT,
	[START_TIME] = @START_TIME,
	[END_TIME] = @END_TIME,
	[CREATED_DATE] = @CREATED_DATE,
	[CREATED_BY] = @CREATED_BY,
	[OCCURANCE] = @OCCURANCE,
	[NEXTSTART_DATE_TIME] = @NEXTSTART_DATE_TIME,
	[END_DATE_TIME] = @END_DATE_TIME,
	[STATUS] = @STATUS,
	[ACTIVE] = @ACTIVE
WHERE [dbo].[ScheduleExchange].[SCHEDULE_EXCHANGE_ID] = @SCHEDULE_EXCHANGE_ID
GO
/****** Object:  StoredProcedure [dbo].[spuScheduleCache]    Script Date: 02/09/2015 11:49:58 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
--* ==================================================
--*  Procedure ScheduleCache Update
--* ==================================================
CREATE PROCEDURE [dbo].[spuScheduleCache]
	@SCHEDULE_CACHE_ID	nvarchar(64),
	@TASK_NAME			nvarchar(100),
	@PROJECT			nvarchar(64),
	@APP				nvarchar(64), 
	@CACHE_PAGE_SIZE	int,
	@SSO_URL			nvarchar(256),
	@CLIENT_ID			nvarchar(64),
	@CLIENT_SECRET		nvarchar(64), 
	@GRANT_TYPE			nvarchar(64),
	@APP_KEY			nvarchar(64),
	@ACCESS_TOKEN		nvarchar(64),
	@REQUEST_TIMEOUT	int,
	@START_TIME			DateTime,
	@END_TIME			DateTime,
	@CREATED_DATE		DateTime,
	@CREATED_BY			nvarchar(100),
	@OCCURANCE			nvarchar(24),
	@NEXTSTART_DATE_TIME DateTime,
	@END_DATE_TIME		DateTime,
	@STATUS				nvarchar(64),
	@ACTIVE				tinyint
AS
UPDATE [dbo].[ScheduleCache] SET
	[TASK_NAME] = @TASK_NAME,
	[PROJECT] = @PROJECT,
	[APP] = @APP,
	[CACHE_PAGE_SIZE] = @CACHE_PAGE_SIZE,
	[SSO_URL] = @SSO_URL,
	[CLIENT_ID] = @CLIENT_ID,
	[CLIENT_SECRET] = @CLIENT_SECRET,
	[GRANT_TYPE] = @GRANT_TYPE,
	[APP_KEY] = @APP_KEY,
	[ACCESS_TOKEN] = @ACCESS_TOKEN,
	[REQUEST_TIMEOUT] = @REQUEST_TIMEOUT,
	[START_TIME] = @START_TIME,
	[END_TIME] = @END_TIME,
	[CREATED_DATE] = @CREATED_DATE,
	[CREATED_BY] = @CREATED_BY,
	[OCCURANCE] = @OCCURANCE,
	[NEXTSTART_DATE_TIME] = @NEXTSTART_DATE_TIME,
	[END_DATE_TIME] = @END_DATE_TIME,
	[STATUS] = @STATUS,
	[ACTIVE] = @ACTIVE
WHERE [dbo].[ScheduleCache].[Schedule_Cache_Id] = @SCHEDULE_CACHE_ID
GO
/****** Object:  StoredProcedure [dbo].[spiSites]    Script Date: 02/09/2015 11:49:58 ******/
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
/****** Object:  StoredProcedure [dbo].[spiScheduleExchange]    Script Date: 02/09/2015 11:49:59 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
--* ==================================================
--*  Procedure ScheduleExchange Insert
--* ==================================================
CREATE PROCEDURE [dbo].[spiScheduleExchange]
	@TASK_NAME				nvarchar(100),
	@SCOPE					nvarchar(64),
	@SSO_URL				nvarchar(256),
	@CLIENT_ID				nvarchar(64),
	@CLIENT_SECRET			nvarchar(64), 
	@GRANT_TYPE				nvarchar(64),
	@REQUEST_TIMEOUT		int,
	@START_TIME				DateTime,
	@END_TIME				DateTime,
	@CREATED_DATE			DateTime,
	@CREATED_BY				nvarchar(100),
	@OCCURANCE				nvarchar(24),
	@NEXTSTART_DATE_TIME 	DateTime,
	@END_DATE_TIME			DateTime,
	@STATUS					nvarchar(64),
	@ACTIVE					tinyint
AS
INSERT INTO [dbo].[ScheduleExchange] (
	[TASK_NAME],
	[SCOPE],
	[SSO_URL],
	[CLIENT_ID],
	[CLIENT_SECRET],
	[GRANT_TYPE],
	[REQUEST_TIMEOUT],
	[START_TIME],
	[END_TIME],
	[CREATED_DATE],
	[CREATED_BY],
	[OCCURANCE],
	[NEXTSTART_DATE_TIME],
	[END_DATE_TIME],
	[STATUS],
	[ACTIVE]
	)
VALUES (
	@TASK_NAME,
	@SCOPE,
	@SSO_URL,
	@CLIENT_ID,
	@CLIENT_SECRET,
	@GRANT_TYPE,
	@REQUEST_TIMEOUT,
	@START_TIME,
	@END_TIME,
	@CREATED_DATE,
	@CREATED_BY,
	@OCCURANCE,
	@NEXTSTART_DATE_TIME,
	@END_DATE_TIME,
	@STATUS,
	@ACTIVE)
GO
/****** Object:  StoredProcedure [dbo].[spiScheduleCache]    Script Date: 02/09/2015 11:49:59 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
--* ==================================================
--*  Procedure ScheduleCache Insert
--* ==================================================
CREATE PROCEDURE [dbo].[spiScheduleCache]
	@SCHEDULE_CACHE_ID	nvarchar(64),
	@TASK_NAME			nvarchar(100),
	@PROJECT			nvarchar(64),
	@APP				nvarchar(64), 
	@CACHE_PAGE_SIZE	int,
	@SSO_URL			nvarchar(256),
	@CLIENT_ID			nvarchar(64),
	@CLIENT_SECRET		nvarchar(64), 
	@GRANT_TYPE			nvarchar(64),
	@APP_KEY			nvarchar(64),
	@ACCESS_TOKEN		nvarchar(64),
	@REQUEST_TIMEOUT	int,
	@START_TIME			DateTime,
	@END_TIME			DateTime,
	@CREATED_DATE		DateTime,
	@CREATED_BY			nvarchar(100),
	@OCCURANCE			nvarchar(24),
	@NEXTSTART_DATE_TIME DateTime,
	@END_DATE_TIME		DateTime,
	@STATUS				nvarchar(64),
	@ACTIVE				tinyint
AS
INSERT INTO [dbo].[ScheduleCache] (
	[SCHEDULE_CACHE_ID],
	[TASK_NAME],
	[PROJECT],
	[APP],
	[CACHE_PAGE_SIZE],
	[SSO_URL],
	[CLIENT_ID],
	[CLIENT_SECRET],
	[GRANT_TYPE],
	[APP_KEY],
	[ACCESS_TOKEN],
	[REQUEST_TIMEOUT],
	[START_TIME],
	[END_TIME],
	[CREATED_DATE],
	[CREATED_BY],
	[OCCURANCE],
	[NEXTSTART_DATE_TIME],
	[END_DATE_TIME],
	[STATUS],
	[ACTIVE]
	)
VALUES (
	@SCHEDULE_CACHE_ID,
	@TASK_NAME,
	@PROJECT,
	@APP,
	@CACHE_PAGE_SIZE,
	@SSO_URL,
	@CLIENT_ID,
	@CLIENT_SECRET,
	@GRANT_TYPE,
	@APP_KEY,
	@ACCESS_TOKEN,
	@REQUEST_TIMEOUT,
	@START_TIME,
	@END_TIME,
	@CREATED_DATE,
	@CREATED_BY,
	@OCCURANCE,
	@NEXTSTART_DATE_TIME,
	@END_DATE_TIME,
	@STATUS,
	@ACTIVE)
GO
/****** Object:  Table [dbo].[Users]    Script Date: 02/09/2015 11:49:59 ******/
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
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[Users] ON
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (1, N'test_user1', 1, N'test1 first name', N'test1 last name', N't@t.com', N'111-111-111-11', N'test desc1', 1)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (2, N'test_user2', 1, N'test2 first name', N'test2 last name', N't@t.com', N'111-111-111-11', N'test desc2', 1)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (3, N'test_user6', 1, N'test11 first nameu', N'test11 last nameu', N't@t.com', N'111-111-111-11', N'test desc11', 1)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (4, N'test_put4', 1, N'first name post', N'last name post', N't@t.com', N'111-111-111-11', N'test post des', 0)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (5, N'test_user3', 1, N'test3 first name', N'test3 last name', N't@t.com', N'111-111-111-11', N'test desc3', 1)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (6, N'test_user333', 1, N'test33 first name', N'test33 last name', N't@t.com', NULL, N'test desc33', 0)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (7, N'test_user334', 1, N'test33 first name', N'test33 last name', N't@t.com', NULL, N'test desc33', 1)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (8, N'test_user335', 1, N'test33 first name', N'test33 last name', N't@t.com', NULL, N'test desc33', 1)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (32, N'vmallire', 1, N'Vidisha', N'Mallireddy', N'vmallire@bechtel.com', NULL, N'kbkesf', 1)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (38, N'1', 1, N'1', N'1', N'1@1.in', N'1', N'1', 0)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (39, N'2', 1, N'2uuu', N'2u', N'2@gmail.com', N'2222222222222', N'2', 0)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (40, N'ss', 1, N'ssfuy', N'sslu', N'sseu@gmail.com', N'9999999999', N'test', 1)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (41, N'rsingh6', 1, N'rohit', N'singh', N'rsingh6@bechtel.com', N'9958558803', N'55555', 1)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (42, N'fkargarm', 1, N'Farshad', N'Kargar', N'fkargarm@bechtel.com', N'5713926590', N'Admin user', 1)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (43, N'ER', 1, N'ER', N'WER', N'WER@DD.COM', N'WER', N'SDF', 1)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (44, N'rrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrr', 1, N'rru', N'fffu', N'ff@gmail.com', N'eeeeeeeee', N'eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee', 0)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (45, N'test77', 1, N'test7783', N'test888', N'abcd@testmail.com', N'88888886', N'test user', 1)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (46, N'deepak', 1, N'fName', N'lName', N'test@testmail.com', N'1234', N'abcd', 0)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (47, N'testuser', 1, N'fName', N'lName', N'test@testmail.com', N'1234', N'abcd', 1)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (48, N'ramesh_user', 1, N'ramesh', N'k', N'ramesh@gmail.com', N'999999999999999999', N'ramesh user desc', 0)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (50, N'1', 1, N'1u', N'1u', N'1@abc.cm', N'132', N'sdf', 0)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (51, N'22', 1, N'2', N'2', N'2@gmail.com', N'2345', N'sdf', 0)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (52, N'22', 1, N'2', N'2', N'2@gmai.com', N'2', N'2', 0)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (66, N'u', 1, N'u', N'u', N't@ham.com', N'34', N'df', 0)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (67, N'u1', 1, N'u1_firstName', N'u1', N'u@gmai.com', N'9898', N'sdf', 0)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (68, N'u2', 1, N'u2_firstname', N'sdf', N'sd@gc.om', N'4', N'df', 1)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (69, N'u3', 1, N'u3_firstName1', N'faa', N'f@g.com', N'7-675555', N'f', 1)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (70, N'u4', 1, N'u4_firstName', N'f', N'f@gm.com', N'6', N'6', 1)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (71, N'u5', 1, N'u5_firstname', N'df', N'df@gmai.com', N'455', N'df', 1)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (72, N'G2015_5_1', 1, N'', N'', N'', N'', N'', 0)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (73, N'U2015_1', 1, N'U2015_1_FNaa', N'U2015_1_LN', N'U2015_1_E@g.com', N'011256985555', N'U2015_1_Desc', 0)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (74, N'U2015_2', 1, N'U2015_2_FN', N'U2015_2_LN', N'U2015_2_E@becthel.com', N'0112569322222', N'U2015_2_desc', 0)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (75, N'U2015_3', 1, N'U2015_3_FN', N'U2015_3_LN', N'rohit@gmail.com', N'', N'', 0)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (76, N'xcdcv', 1, N'dcvvvf', N'', N'', N'', N'', 0)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (77, N'dd', 1, N'dd', N'', N'', N'', N'', 0)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (78, N'dcdcc', 1, N'ddddddddddddddddddddddvvvvvvvvvvvvvvv', N'', N'', N'', N'', 0)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (79, N'U2015_4', 1, N'U2015_4_FN', N'U2015_4_LN', N'', N'', N'', 0)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (80, N'U2015_5', 1, N'U2015_5_FN', N'U2015_5_LN', N'', N'', N'', 0)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (81, N'u1', 1, N'u1-Firstname', N'l', N'abc@ad.com', N'9999', N'desc', 1)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (82, N'u6', 1, N'u6-firstname', N'u6', N'u6@ac.com', N'66', N'df', 1)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (83, N'test_user4', 1, N'test_user4_fnam', N'test_user4lanm', N'test_user4@ad.com', N'878787', N'desc', 1)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (84, N'test_user5', 1, N'test_user4_fn', N'test_user4-ln', N'test_user4@ad.com', N'988', N'dwsed', 1)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (85, N'222', 1, N'', N'', N'', N'', N'', 0)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (86, N'fdf', 1, N'', N'', N'', N'', N'', 0)
INSERT [dbo].[Users] ([UserId], [UserName], [SiteId], [UserFirstName], [UserLastName], [UserEmail], [UserPhone], [UserDesc], [Active]) VALUES (87, N'aa', 1, N'', N'', N'', N'', N'', 1)
SET IDENTITY_INSERT [dbo].[Users] OFF
/****** Object:  Table [dbo].[Applications]    Script Date: 02/09/2015 11:49:59 ******/
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
INSERT [dbo].[Applications] ([ContextId], [ApplicationId], [DisplayName], [InternalName], [Description], [DXFRUrl], [SiteId], [Active], [Assembly]) VALUES (N'ed90e5f5-a716-49a3-bbb5-ca2ee57e140e', N'54d581ef-64bc-45c2-9df5-0255f7454e0e', N'App2DN', N'App2IN', N'App2Desc', N'www.DXFRUrl.com', 1, 1, N'org.iringtools.adapter.datalayer.NHibernateDataLayer, NHibernateLibrary')
INSERT [dbo].[Applications] ([ContextId], [ApplicationId], [DisplayName], [InternalName], [Description], [DXFRUrl], [SiteId], [Active], [Assembly]) VALUES (N'5ddc49b3-1599-47e2-9f9a-ad37413d28a7', N'3e1e418d-fa51-403b-bb7b-0cade34283a7', N'test App display name1', N'iApp2', N'test desc2 edit', N'abcdef', 1, 1, NULL)
INSERT [dbo].[Applications] ([ContextId], [ApplicationId], [DisplayName], [InternalName], [Description], [DXFRUrl], [SiteId], [Active], [Assembly]) VALUES (N'5ddc49b3-1599-47e2-9f9a-ad37413d28a7', N'c274b3fb-1794-4bc2-bcab-3615fea7f11f', N'test App display name1', N'iApp2', N'test desc2 edit', N'abcdef', 1, 1, NULL)
INSERT [dbo].[Applications] ([ContextId], [ApplicationId], [DisplayName], [InternalName], [Description], [DXFRUrl], [SiteId], [Active], [Assembly]) VALUES (N'5ddc49b3-1599-47e2-9f9a-ad37413d28a7', N'708ed08c-3077-4960-bf95-39e0027e086d', N'DN 29-Oct-14u', N'IN 29-Oct-14', N'desu', N'dxfu', 1, 1, N'test.dllu')
INSERT [dbo].[Applications] ([ContextId], [ApplicationId], [DisplayName], [InternalName], [Description], [DXFRUrl], [SiteId], [Active], [Assembly]) VALUES (N'5ddc49b3-1599-47e2-9f9a-ad37413d28a7', N'f0638d95-a1b2-4675-927d-7a83c28465c6', N'test App display name1', N'iApp2', N'test desc2 edit', N'abcdef', 1, 1, NULL)
INSERT [dbo].[Applications] ([ContextId], [ApplicationId], [DisplayName], [InternalName], [Description], [DXFRUrl], [SiteId], [Active], [Assembly]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'7877c504-133d-41b9-8680-7bfd73840435', N'DEF', N'DEF', N'wr', N'http://localhost:54321/dxfr', 1, 1, NULL)
INSERT [dbo].[Applications] ([ContextId], [ApplicationId], [DisplayName], [InternalName], [Description], [DXFRUrl], [SiteId], [Active], [Assembly]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'3456c504-133d-41b9-8780-7bfd73840444', N'ABC', N'ABC', N'trr', N'http://localhost:54321/dxfr', 1, 1, NULL)
INSERT [dbo].[Applications] ([ContextId], [ApplicationId], [DisplayName], [InternalName], [Description], [DXFRUrl], [SiteId], [Active], [Assembly]) VALUES (N'5ddc49b3-1599-47e2-9f9a-ad37413d28a7', N'8ebf712a-6838-4851-ae0f-8251d5153db4', N'test App display name1', N'iApp2', N'test desc2 edit', N'abcdef', 1, 1, NULL)
INSERT [dbo].[Applications] ([ContextId], [ApplicationId], [DisplayName], [InternalName], [Description], [DXFRUrl], [SiteId], [Active], [Assembly]) VALUES (N'd3c26e6c-8018-4d74-916a-f5c65f67b66d', N'fee8b7c8-2a09-42f2-a9de-8f414df8ed8a', N'App1DN', N'App1iN', N'App1Desc', N'NotMatched witg xml', 1, 1, N'org.iringtools.adapter.datalayer.NHibernateDataLayer, NHibernateLibrary')
INSERT [dbo].[Applications] ([ContextId], [ApplicationId], [DisplayName], [InternalName], [Description], [DXFRUrl], [SiteId], [Active], [Assembly]) VALUES (N'5ddc49b3-1599-47e2-9f9a-ad37413d28a7', N'2783c17d-2daf-4f01-b92b-fbe82c9f2128', N'test App display name1', N'iApp3', N'test desc3', N'abcdef', 1, 0, NULL)
INSERT [dbo].[Applications] ([ContextId], [ApplicationId], [DisplayName], [InternalName], [Description], [DXFRUrl], [SiteId], [Active], [Assembly]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'83e6e521-1a27-4e2b-8dc0-fe3f021a26de', N'test App display name', N'iApp', N'test desc', N'abcd', 3, 1, NULL)
/****** Object:  Table [dbo].[Folders]    Script Date: 02/09/2015 11:49:59 ******/
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
INSERT [dbo].[Folders] ([FolderId], [ParentFolderId], [FolderName], [SiteId], [Active]) VALUES (N'8d2bccd8-8210-45e6-b39e-1009e7f7dd31', N'f2bb2bdc-22c8-4e1d-82da-04a5334ec2f0', N'Testupdateduu', 1, 1)
INSERT [dbo].[Folders] ([FolderId], [ParentFolderId], [FolderName], [SiteId], [Active]) VALUES (N'8dff23d9-f7b7-409f-9bff-361d22c29765', N'a4986708-144a-4422-8803-93c0c0acf974', N'9Feb15Child_Child', 1, 1)
INSERT [dbo].[Folders] ([FolderId], [ParentFolderId], [FolderName], [SiteId], [Active]) VALUES (N'6914f8d4-636f-4d9f-aa43-45f865de56b6', N'504ff982-740d-45a6-95d8-6f6ba425fd40', N'Engineering', 1, 1)
INSERT [dbo].[Folders] ([FolderId], [ParentFolderId], [FolderName], [SiteId], [Active]) VALUES (N'9614f8d4-636f-4d9f-aa43-45f865de56b7', N'00000000-1111-2222-3333-444444444444', N'BsII', 1, 1)
INSERT [dbo].[Folders] ([FolderId], [ParentFolderId], [FolderName], [SiteId], [Active]) VALUES (N'4020f94e-9349-4376-b66f-548604d243d9', N'd3d6eecb-dae7-4b10-9d7e-f6389d00a552', N'9Feb15Child1', 1, 1)
INSERT [dbo].[Folders] ([FolderId], [ParentFolderId], [FolderName], [SiteId], [Active]) VALUES (N'504ff982-740d-45a6-95d8-6f6ba425fd40', N'00000000-1111-2222-3333-444444444444', N'Construction', 1, 1)
INSERT [dbo].[Folders] ([FolderId], [ParentFolderId], [FolderName], [SiteId], [Active]) VALUES (N'a4986708-144a-4422-8803-93c0c0acf974', N'd3d6eecb-dae7-4b10-9d7e-f6389d00a552', N'9Feb15Child', 1, 1)
INSERT [dbo].[Folders] ([FolderId], [ParentFolderId], [FolderName], [SiteId], [Active]) VALUES (N'8c321a48-5f9a-4d2b-a7bd-ad046f14b448', N'f2bb2bdc-22c8-4e1d-82da-04a5334ec2f0', N'Test', 1, 1)
INSERT [dbo].[Folders] ([FolderId], [ParentFolderId], [FolderName], [SiteId], [Active]) VALUES (N'd3d6eecb-dae7-4b10-9d7e-f6389d00a552', N'00000000-1111-2222-3333-444444444444', N'9Feb15', 1, 1)
/****** Object:  Table [dbo].[Commodity]    Script Date: 02/09/2015 11:49:59 ******/
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
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'266ea8c1-e081-464c-88bb-0137d7e78424', N'utjty', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'5b721a3a-ed92-4f1e-bb6e-01da52333072', N'deepak1', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'1c74884a-73ba-49d0-ac66-0545f0159173', N'ccc', 1, 0)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', N'd0dbf106-65d0-447e-a526-059a59edfc68', N'uyuyuyu', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'8c5e0be6-8625-41fb-89d4-0a0976a32d47', N'vif', 1, 0)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'3dbf0cef-891e-44ac-992a-161e2431bcdd', N'vidioio', 1, 0)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', N'59586564-e649-4d98-af36-1d14f32165f7', N'5t5t', 1, 0)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'c5b15a98-47b7-4a47-b254-20ba9abdfb6a', N'rrr', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', N'2652dfb0-db4f-4d6d-a41e-2234364a80e4', N'5t5t', 1, 0)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'6c0f7ef8-fba2-4637-89bd-2396e7281bdb', N'yyy', 1, 0)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'a740c34e-df62-4a58-9554-23ae4a1a3c6a', N'deepak3', 1, 0)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'f2716436-06bf-4fec-911a-254d0e32dc39', N'vidioio', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'b172deca-c002-4775-8fec-286ca1c9f76a', N'yujyutfh', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'a6ac8ca5-662c-4249-9d1f-2b7cb962f2cb', N'dee1ii', 1, 0)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', N'f3fd50ba-d1fe-4a68-b30f-2edd18497058', N'uiuiiu', 1, 0)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', N'a40b9b99-7f2e-4450-8883-35dfdcf0cd1a', N'uyuy', 1, 0)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'f284377d-2c04-4697-8a1d-36808d062875', N'Lines', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'c3dffaee-80fb-4f8f-9ca9-39c1c41dd926', N'iyit5r', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', N'42583349-2679-4729-b3af-3a3f42954bf9', N'tyty', 1, 0)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', N'73a53324-3981-4982-a974-45051a7cebed', N'o9o9o9', 1, 0)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'a78d2c39-7a81-4873-b736-45455bf4fb66', N'ooo', 1, 0)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'86a1c10e-7fde-41fa-9dbc-49568d1efaa5', N'Succ', 1, 0)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', N'5e143c9e-0d5c-44cc-aa9e-4a3f1e3d9a17', N'gtbtbty', 1, 0)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'0921054e-e336-4f06-a123-4a4855f16a8a', N'uuuu', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'899c44d4-8b27-4fd1-b4de-4cfb6bc8653f', N'deepak6', 1, 0)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', N'67a27f18-1b49-41eb-9f48-4dd190934d25', N'4e4e4e', 1, 0)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', N'fcbd140b-af75-45ab-a81c-4f74f157345a', N'ppp', 1, 0)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', N'4fef96cc-6425-4e26-b2fd-57a75380bc32', N'uiuiiu', 1, 0)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', N'9772e692-ee6e-4030-810f-64280b0eb2cd', N'78768', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', N'111234f9-876d-45c8-a4c1-69cbf2932d6d', N'uiui899', 1, 0)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', N'd74363f6-9413-4c1f-bc8f-69d8d9bbbfc8', N'vifrty', 1, 0)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'5cc1d872-db94-43fe-88b0-6b33fec291bb', N'yyy', 1, 0)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', N'99a89a2a-85e9-468d-98f8-6c680baeb77c', N'gtbtbty', 1, 0)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', N'fd3ff44e-92ee-4606-9246-6eee2646e7e8', N'abcdefg', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'83d6c735-df42-44b7-b21b-70c37c8d3a8c', N'deepak7updated', 1, 0)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'b26df1b8-b185-49bb-ba18-72f935f97cf0', N'Teamworks', 1, 0)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', N'8c2669bc-5ea4-466c-8115-7c18c85dc64a', N'yyyyy', 1, 0)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'21b7e812-419b-4c10-990c-7c476ce335a9', N'uiuiu', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', N'05ab813a-f365-4d07-9f5d-7e79dd82bd80', N'o9o9o9', 1, 0)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', N'950490c6-a881-42f9-8dca-7efd124c2be0', N'xxx', 1, 0)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', N'aa2d4e3a-f8f6-4a50-b282-88f2095d9c09', N'124wx', 1, 0)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', N'522f3963-f2f6-4d4d-9c1f-8be9a8c51452', N'12ws', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'ee565602-ba52-422c-a01f-932490796666', N'Lines', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'9efd6494-d611-47b4-a9a3-9450219a583d', N'122', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', N'4f7b4449-5884-4c92-a8fd-96081b3bcb2b', N'66yhy', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'd45851cd-7662-44d4-8339-97ab64d47771', N'Linesdeep', 1, 0)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'1a849581-849d-41b7-9a1d-9a9ba57ef8b8', N'deepak2', 1, 0)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', N'22a4f857-6064-46f8-aa39-9b3f842075db', N'iii', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'75c73313-d275-4874-aaa6-9c0f74466d15', N'ttt', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', N'acfd2e4f-e598-47a7-af3a-a82a74b9a04f', N'543290', 1, 0)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', N'41122303-f175-4381-9a09-a85620955760', N'yyyyy', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', N'a6ec6af2-cdaf-4832-a3e6-a945845ef118', N'o9o9o9', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'448d603b-6770-48ab-b572-af218b043fbf', N'PipingNetworkSystem', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', N'b7e2d9d1-d2b4-40c8-a290-b1dafb3aa03f', N'ujnji', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'0ece41f0-a9a0-4df4-81c7-b6652a7e5486', N'deepak4', 1, 0)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'22b5e090-7374-4314-ba5c-bd778452598e', N'dede', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', N'e6231816-0abb-47b2-acf4-be1787b25731', N'iii', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', N'fd0ce2bb-be7f-4739-aa5f-ca54da05d128', N'o9o9o9', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', N'3f462909-4de2-4940-a386-cd01182efd9f', N'0o0o0o0o', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', N'8c4eb21f-fdb6-46dc-a180-cd8146ae2df6', N'543290', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'ee0fad0d-01d4-490e-88b6-d28f9e20821a', N'deepak5', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'0768db9b-2a40-47bb-9043-d46a920c4485', N'Teamworks', 1, 0)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', N'6364aef1-cf9d-48da-8fc4-d48983659ca8', N'9090a', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'5798beed-e4cf-4f05-87b4-d48a8837d293', N'uiui', 1, 0)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', N'3720fb5d-ebfe-4c31-8dc6-d5d23a331dc7', N'opop', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'7fa67256-7113-412f-8079-de04ea85ae05', N'uiui9', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'71de4cca-7e79-429c-8c4e-dea0acb59368', N'yyyy', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'70f55273-60f5-4aba-8de4-def0d79584ab', N'dede', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'c7a1ff10-c2f9-486e-93e1-e720721e58a4', N'ccc', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', N'b58cd872-3c34-4d60-ac5d-ea7938f2695e', N'yyttt', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', N'103ed432-f36b-476c-823a-ebea3712768e', N'uuu', 1, 0)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'e8964e62-2b13-485e-97b3-f61c003ea155', N'uiui', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', N'982596bd-557e-4d8b-9740-f767b175f1b3', N'6t6t6t', 1, 1)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'b52ec7ce-fc70-4a78-896d-f9492d141883', N'122', 1, 0)
INSERT [dbo].[Commodity] ([ContextId], [CommodityId], [CommodityName], [SiteId], [Active]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', N'2b9c2271-3e3f-48b8-a190-fd725742d306', N'utjty', 1, 1)
/****** Object:  Table [dbo].[Groups]    Script Date: 02/09/2015 11:49:59 ******/
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
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (1, 1, N'test group1', N'test group description1', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (3, 1, N'test group3', N'desc 123457898', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (4, 1, N'abc4', N'xyz1', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (5, 1, N'abc5', N'xyz2', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (6, 1, N'abc6', N'xyz2', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (7, 1, N'abc7', N'xyz', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (8, 1, N'abc8', N'xyz3', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (9, 1, N'abc9', N'xyz1a', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (10, 1, N'abc10', N'xyz33', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (11, 1, N'test group', N'test group desc', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (12, 1, N'testing put', N'testing put', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (16, 1, N'1111', N'sadfsa', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (17, 1, N'123u', N'123uu', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (18, 1, N'fghf', N'gfhfgh', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (19, 1, N'999', N'123456', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (20, 1, N'test p', N'test p', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (21, 1, N'test p1', N'test t1', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (22, 1, N'test group2', N'desc 12345', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (23, 1, N'abc23', N'1234', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (24, 1, N'abc24', N'111111111', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (37, 1, N'ssu', N'ss', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (38, 1, N'dd', N'dd', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (42, 1, N'wwwww', N'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (43, 1, N'111111', N'111111', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (44, 1, N'qqqqqqqqqqq', N'qqqqqqqqqqqqq', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (45, 1, N'testing123', N'123', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (46, 1, N'11223344', N'11223344', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (47, 1, N'Administrator', N'Admin Group', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (48, 1, N'rrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrr', N'rrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrr', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (49, 1, N'test776', N'test553', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (50, 1, N'ng44', N'ngh', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (51, 1, N'testGroup2345', N'test description678', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (52, 1, N'testGroupNew', N'description New', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (53, 1, N'TestTest', N'Test', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (54, 1, N'ramesh group', N'ramesh desc grp', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (55, 1, N'abc4_1', N'new group', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (58, 1, N'asda', N'asdas', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (59, 1, N'sdfsad', N'asdfa', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (60, 1, N'sdfas', N'sdfadf', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (61, 1, N'sdfa', N'sadf', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (62, 1, N'asdfsad', N'asdsada', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (63, 1, N'one', N'tetwa', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (90, 1, N'g', N'g', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (91, 1, N'g1', N'g', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (92, 1, N'g2', N'gaa', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (93, 1, N'g3', N'gg', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (94, 1, N'g4', N'gg', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (95, 1, N'g5', N'dfua', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (96, 1, N'----', N'desc', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (97, 1, N'G2015_1', N'G2015_1_desc', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (98, 1, N'G2015_2', N'G2015_2_desc', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (99, 1, N'G2015_3', N'G2015_3_Desc', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (100, 1, N'G2015_4', N'G2015_3_desc', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (101, 1, N'bfxdgbhgfh', N'gfhgfhbgfh', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (102, 1, N'G2015_5', N'G2015_5_descddd', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (103, 1, N'G2015_7', N'wwwcccc', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (104, 1, N'g1', N'gtu1', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (105, 1, N'fvffffffffffffffffffffffffff', N'fffffffffffffffx', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (106, 1, N'xsssssssssssssssss', N'222222222222222222', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (107, 1, N'11111111111111111', N'ssss', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (108, 1, N'G2015_6', N'G2015_6', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (109, 1, N'G2015_7', N'G2015_7', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (110, 1, N'g1', N'gua', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (111, 1, N'g1-1', N'ku', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (112, 1, N'g2-2', N'aua', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (113, 1, N'g1-1-1', N'Enter Description', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (114, 1, N'g3-3', N'aq', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (115, 1, N'g4-4', N'a', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (116, 1, N'g5-5', N'da', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (117, 1, N'g2-2-2', N'au', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (118, 1, N'g1-1-1', N'aa', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (119, 1, N'g2-2-2-2', N'fu', 1)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (120, 1, N'G2015_1', N'G2015_1_desctgtvvvvvvvvvvvvvvvvvvvvvvv', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (121, 1, N'G2015_2', N'G2015_2ssssssssssssssssssssss5444444444444', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (122, 1, N'G2015_2015_555dddddddddddddd', N'G2015_55555555', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (123, 1, N'cfsdfvsdfvsdfv', N'G2015', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (124, 1, N'G2015_115', N'G2015_115_565555vvvvvvvvvvvvv', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (125, 1, N'a', N'au', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (126, 1, N'wwwwwwwwwwwwwwwwwww', N'wwwwwwwwwwwwwwwww', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (127, 1, N'aaaaaaaaaaa', N'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaa', 0)
INSERT [dbo].[Groups] ([GroupId], [SiteId], [GroupName], [GroupDesc], [Active]) VALUES (128, 1, N'8888888888888888', N'fffffffffffffff', 0)
SET IDENTITY_INSERT [dbo].[Groups] OFF
/****** Object:  StoredProcedure [dbo].[spiDataFilter]    Script Date: 02/09/2015 11:50:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Deepak Kumar>
-- Create date: <17-Sep-2014>
-- Description:	<Inserting datafilter>
-- ===========================================================
CREATE PROCEDURE [dbo].[spiDataFilter] 
(
	@ResourceId uniqueidentifier,
	@DataFilterTypeId int,
	@SiteId int,
	@RawXml xml
)	 
AS

	BEGIN 
	
	
--Set @RawXml = '<dataFilters>
--	<dataFilter>
--		<dataFilterId>00000000-0000-0000-0000-000000000000</dataFilterId>
--		<resourceId>d208b9b8-0372-4e2c-9b34-78b9bb453b61</resourceId>
--		<dataFilterTypeId>2</dataFilterTypeId>
--		<siteId>1</siteId>
--		<active>1</active>
--		<expressions>
--			<expression>
--				<dfOrder>1</dfOrder>
--				<openGroupCount>10</openGroupCount>
--				<propertyName>xyz.abc.valuwIdentifier</propertyName>
--				<relationalOperator>contains</relationalOperator>
--				<values>
--					<value>2</value>
--					<value>3</value>
--				</values>
--				<logicalOperator>Or</logicalOperator>
--				<closeGroupCount>23</closeGroupCount>
--				<isCaseSensitive>false</isCaseSensitive>
--			</expression>
--			<expression>
--				<dfOrder>2</dfOrder>
--				<openGroupCount>1</openGroupCount>
--				<propertyName>xyz.abc.valuwIdentifier</propertyName>
--				<relationalOperator>contains</relationalOperator>
--				<values>
--					<value>3</value>
--				</values>
--				<logicalOperator>AND</logicalOperator>
--				<closeGroupCount>2</closeGroupCount>
--				<isCaseSensitive>true</isCaseSensitive>
--			</expression>
--		</expressions>
--		<orderExpressions>
--			<orderExpression>
--				<dfOrder>1</dfOrder>
--				<propertyName>xyz.abc.valueIdentifier</propertyName>
--				<sortOrder>ASC</sortOrder>
--			</orderExpression>
--		</orderExpressions>
--		<isAdmin>true</isAdmin>
--	</dataFilter>
--</dataFilters>'

--This is temporary solution, need to find some workaround
Set @RawXml = REPLACE(Cast(@RawXml as varchar(max)),'<value>','<value>,')

	
	Create table #Expression
	(
		DataFilterId	uniqueidentifier default newid(),
		ResourceId	uniqueidentifier,
		DataFilterTypeId	int,
		IsExpression	int default 1,
		DFOrder	int,
		OpenGroupCount	int default null,
		LogicalOperator	nvarchar(50) default null,
		PropertyName	nvarchar(350) default null,
		RelationalOperator	nvarchar(150) default null,
		[Values] varchar(1000),
		CloseGroupCount	int default null,
		SiteId	int,
		IsCaseSensitive bit,
		IsAdmin bit
		
	)	

	Create table #OrderExpression
	(
		DataFilterId	uniqueidentifier default newid(),
		ResourceId	uniqueidentifier,
		DataFilterTypeId	int,
		IsExpression	int default 0,
		DFOrder	int,
		PropertyName	nvarchar(350) default null,
		SortOrder	nvarchar(25) default null,
		SiteId	int,
		IsAdmin bit
	)


SELECT       
 @ResourceId as ResourceId,      
 @SiteId as SiteId,     
 @DataFilterTypeId as DataFilterTypeId,
 nref.value('isAdmin[1]', 'bit') IsAdmin           
into #Parent FROM   @rawXML.nodes('//dataFilter') AS R(nref)
	

	INSERT INTO #Expression
			   (
				[ResourceId]
			   ,[DataFilterTypeId]
			   ,[DFOrder]
			   ,[OpenGroupCount]
			   ,[LogicalOperator]
			   ,[PropertyName]
			   ,[RelationalOperator]
			   ,[Values]
			   ,[CloseGroupCount]
			   ,[SiteId]
			   ,[IsCaseSensitive]
			   ,[IsAdmin]  
			   )
	SELECT
	 ResourceId,
	 DataFilterTypeId,
	 nref.value('dfOrder[1]', 'int') DFOrder,      
	 nref.value('openGroupCount[1]', 'int') OpenGroupCount,      
	 nref.value('logicalOperator[1]', 'nvarchar(50)') LogicalOperator,
	 nref.value('propertyName[1]', 'nvarchar(350)') PropertyName,
	 nref.value('relationalOperator[1]', 'nvarchar(150)') RelationalOperator,
	 nref.value('values[1]','varchar(500)') [values],
	 nref.value('closeGroupCount[1]', 'int') CloseGroupCount,
	 SiteId,
	 nref.value('isCaseSensitive[1]', 'bit') IsCaseSensitive,
	 IsAdmin
	FROM  @rawXML.nodes('//expression') AS R(nref),#Parent 

	

	Insert Into #OrderExpression
	(
		ResourceId,
		DataFilterTypeId,
		DFOrder,
		PropertyName,
		SortOrder,
		SiteId,
		IsAdmin
	)
	SELECT
	 ResourceId,
	 DataFilterTypeId,
	 nref.value('dfOrder[1]', 'int') DFOrder,      
	 nref.value('propertyName[1]', 'nvarchar(350)') PropertyName,
	 nref.value('sortOrder[1]', 'nvarchar(25)') SortOrder,
	 SiteId,
	 IsAdmin
	FROM  @rawXML.nodes('//orderExpression') AS R(nref),#Parent 	
   
   
   

	
	INSERT INTO [DataFilters]
           ([DataFilterId]
           ,[ResourceId]
           ,[DataFilterTypeId]
           ,[IsExpression]
           ,[DFOrder]
           ,[OpenGroupCount]
           ,[LogicalOperator]
           ,[PropertyName]
           ,[RelationalOperator]
           ,[CloseGroupCount]
           ,[SortOrder]
           ,[SiteId]
           ,[IsCaseSensitive]
           ,[IsAdmin]  
           )
	select 
		DataFilterId,
		ResourceId,
		DataFilterTypeId,
		IsExpression,
		DFOrder,
		OpenGroupCount,
		LogicalOperator,
		PropertyName,
		RelationalOperator,
		CloseGroupCount,
		Null as SortOrder,
		SiteId,
		IsCaseSensitive,
		IsAdmin
	 from #Expression
	 union all
	select 
		DataFilterId,
		ResourceId,
		DataFilterTypeId,
		IsExpression,
		DFOrder,
		Null as OpenGroupCount,
		Null as LogicalOperator,
		PropertyName,
		Null as RelationalOperator,
		Null as CloseGroupCount,
		SortOrder,
		SiteId,
		Null as IsCaseSensitive,
		IsAdmin
	 from #OrderExpression


Insert into  DataFilterValues 
Select DataFilterId,Items as value from #Expression E Cross Apply dbo.Split(E.[Values],',') S 
where S.Items <> ''

--drop table #Parent
--drop table #Expression
--drop table #OrderExpression
							

END
GO
/****** Object:  StoredProcedure [dbo].[spgContext]    Script Date: 02/09/2015 11:50:00 ******/
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
/****** Object:  StoredProcedure [dbo].[spgScheduleExchange]    Script Date: 02/09/2015 11:50:01 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [dbo].[spgScheduleExchange]
	@Schedule_Exchange_Id	[varchar](126)
AS

SELECT
	[Schedule_Exchange_Id] AS [Schedule_Exchange_Id],
	[Task_Name] AS [Task_Name],
	[SCOPE] AS [SCOPE],
	[Base_Url] AS [Base_Url],
	[Exchange_Id] AS [Exchange_Id],
	[SSO_URL] AS [SSO_URL],
	[CLIENT_ID] AS [CLIENT_ID],
	[CLIENT_SECRET] AS [CLIENT_SECRET],
	[GRANT_TYPE] AS [GRANT_TYPE],
	[REQUEST_TIMEOUT] AS [REQUEST_TIMEOUT],
	[Start_Time] AS [Start_Time],
	[End_Time] AS [End_Time],
	[Created_Date] AS [Created_Date] ,
	[Created_By] AS [Created_By],
	[Occurance] AS [Occurance],
	[NextStart_Date_Time] AS [NextStart_Date_Time],
	[End_Date_Time] AS [End_Date_Time],
	[STATUS] AS [STATUS],
	[ACTIVE] AS [ACTIVE]
FROM [dbo].[ScheduleExchange] WITH (NoLock)
WHERE
	[dbo].[ScheduleExchange].[Schedule_Exchange_Id] = @Schedule_Exchange_Id
GO
/****** Object:  StoredProcedure [dbo].[spgScheduleCache]    Script Date: 02/09/2015 11:50:01 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [dbo].[spgScheduleCache]
	@Schedule_Cache_Id	[varchar](126)
AS

SELECT
	[Schedule_Cache_Id] AS [Schedule_Cache_Id],
	[Task_Name] AS [Task_Name],
	[PROJECT] AS [PROJECT],
	[APP] AS [APP],
	[CACHE_PAGE_SIZE] AS [CACHE_PAGE_SIZE],
	[SSO_URL] AS [SSO_URL],
	[CLIENT_ID] AS [CLIENT_ID],
	[CLIENT_SECRET] AS [CLIENT_SECRET],
	[GRANT_TYPE] AS [GRANT_TYPE],
	[APP_KEY] AS [APP_KEY],
	[ACCESS_TOKEN] AS [ACCESS_TOKEN],
	[REQUEST_TIMEOUT] AS [REQUEST_TIMEOUT],
	[Start_Time] AS [Start_Time],
	[End_Time] AS [End_Time],
	[Created_Date] AS [Created_Date],
	[Created_By] AS [Created_By],
	[Occurance] AS [Occurance],
	[NextStart_Date_Time] AS [NextStart_Date_Time],
	[End_Date_Time] AS [End_Date_Time],
	[STATUS] AS [STATUS],
	[ACTIVE] AS [ACTIVE]
FROM [dbo].[ScheduleCache] WITH (NoLock)
WHERE
	[dbo].[ScheduleCache].[Schedule_Cache_Id] = @Schedule_Cache_Id
GO
/****** Object:  StoredProcedure [dbo].[spgSitesById]    Script Date: 02/09/2015 11:50:02 ******/
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
/****** Object:  StoredProcedure [dbo].[spgSites]    Script Date: 02/09/2015 11:50:02 ******/
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
/****** Object:  Table [dbo].[Permissions]    Script Date: 02/09/2015 11:50:02 ******/
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
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (5, 1, N'Read', N'permission desc', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (6, 1, N'Update', N'testing put', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (7, 1, N'Excute', N'excute', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (8, 1, N'ConfigureExchange', N'create exchange', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (9, 1, N'dd1', N'dd1', 0)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (10, 1, N'test1', N'testdesc', 0)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (11, 1, N'1', N'1', 0)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (12, 1, N'2', N'2', 0)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (13, 1, N'222222222222222222', N'2222444444444', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (14, 1, N'pname', N'pdesc', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (15, 1, N't1', N'qq', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (16, 1, N'qq', N'ss', 0)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (17, 1, N'qqss17', N'ss', 0)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (18, 1, N'test123', N'123', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (19, 1, N'block', N'block permission', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (20, 1, N'permission', N'permission des', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (21, 1, N'rrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrr', N'rrr', 0)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (22, 1, N'nishantnishantnishantnishantnishantnishantnishantnishantnishantnishantnishantnishantnishantnishantni', N'nishantu', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (23, 1, N'abcd22', N'abcd234', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (24, 1, N'new permission555', N'new permission description', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (25, 1, N'reamesh permission', N'ramesh perm desc', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (26, 1, N'Edit_1', N'asdf', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (27, 1, N'1', N'11u', 0)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (28, 1, N'r1', N'r1rr', 0)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (29, 1, N'pa', N'pa', 0)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (30, 1, N'p1', N'1', 0)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (31, 1, N'pp', N'pp', 0)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (32, 1, N'pp', N'pp', 0)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (33, 1, N'p', N'p', 0)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (34, 1, N'p', N'p', 0)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (35, 1, N'p', N'p', 0)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (36, 1, N'p', N'p', 0)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (37, 1, N'Read', N'Read resources', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (38, 1, N'Execute', N'Execute', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (39, 1, N'p1', N'p1u', 0)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (40, 1, N'p2', N'p2', 0)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (41, 1, N'P2015_1', N'P2015_1_Desc', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (42, 1, N'P2015_2', N'P2015_2_desc12', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (43, 1, N'P2015_3', N'P2015_3_Desc', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (44, 1, N'P2015_4', N'P2015_4_descccccccccccccccccccccccccc', 0)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (45, 1, N'p1', N'h', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (46, 1, N'p2', N'ft', 0)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (47, 1, N'p3', N'f', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (48, 1, N'p4', N'g', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (49, 1, N'p5', N'g', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (50, 1, N'p1-1', N'c', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (51, 1, N'p2-2', N'Enter Permission Description', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (52, 1, N'p3-3', N'a', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (53, 1, N'p4-4', N'a', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (54, 1, N'p5-5', N's', 1)
INSERT [dbo].[Permissions] ([PermissionId], [SiteId], [PermissionName], [PermissionDesc], [Active]) VALUES (55, 1, N'aa', N'aa', 1)
SET IDENTITY_INSERT [dbo].[Permissions] OFF
/****** Object:  StoredProcedure [dbo].[spdSites]    Script Date: 02/09/2015 11:50:03 ******/
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
/****** Object:  StoredProcedure [dbo].[spdScheduleExchange]    Script Date: 02/09/2015 11:50:03 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
--* ==================================================
--*  Procedure ScheduleExchange Delete
--* ==================================================
CREATE PROCEDURE [dbo].[spdScheduleExchange]
	@SCHEDULE_EXCHANGE_ID	[varchar](126)
AS

UPDATE [dbo].[ScheduleExchange] 
SET 
	[ACTIVE] = 0
WHERE [dbo].[ScheduleExchange].[SCHEDULE_EXCHANGE_ID] = @SCHEDULE_EXCHANGE_ID
GO
/****** Object:  StoredProcedure [dbo].[spdScheduleCache]    Script Date: 02/09/2015 11:50:04 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
--* ==================================================
--*  Procedure ScheduleCache Delete
--* ==================================================
CREATE PROCEDURE [dbo].[spdScheduleCache]
	@SCHEDULE_CACHE_ID	[varchar](126)
AS

UPDATE [dbo].[ScheduleCache] 
SET 
	[ACTIVE] = 0
WHERE [SCHEDULE_CACHE_ID] = @SCHEDULE_CACHE_ID
GO
/****** Object:  StoredProcedure [dbo].[spdDataFilter]    Script Date: 02/09/2015 11:50:05 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Deepak Kumar>
-- Create date: <17-Sep-2014>
-- Description:	<deleting datafilter>
-- ===========================================================
CREATE PROCEDURE [dbo].[spdDataFilter] 
(
	@ResourceId	uniqueidentifier
)	 
AS

	BEGIN
	
	
		UPDATE [DataFilters]
		   SET 
			Active = 0			  
	 WHERE ResourceId = @ResourceId


--As this is child table, and totaly dependent on DataFilters table, hence deleting the records permanantly
--and inserting new one
		delete
		from DataFilterValues where 
		DataFilterId in 
		(select DataFilterId from DataFilters where ResourceId = @ResourceId)



	END
GO
/****** Object:  Table [dbo].[Roles]    Script Date: 02/09/2015 11:50:05 ******/
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
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (1, 1, N'Admin', N'role1 desc', 1)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (2, 1, N'SecurityAdmin', N'0000000', 1)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (3, 1, N'ContextAdmin', N'role3 desc', 1)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (4, 1, N'ApplicationAdmin', N'test desc333 updated', 1)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (5, 1, N'GraphAdmin', N'role23 desc', 1)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (6, 1, N'ExchangeAdmin', N'role34 desc', 1)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (7, 1, N'GraphViewer', N'BB', 1)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (8, 1, N'GraphUpdater', N'BB', 1)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (9, 1, N'ExchangeViewer', N'BBB', 1)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (10, 1, N'ExchangeExcuter', N'Inserting Role from aPI', 1)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (11, 1, N'ff', N'ff', 1)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (12, 1, N'ffff12', N'ff', 0)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (13, 1, N'dee', N'pak', 1)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (14, 1, N'qqq', N'qqq', 1)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (15, 1, N'hh', N'hh', 0)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (16, 1, N'jjtt', N'jjttu', 1)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (18, 1, N'role', N'ddd', 1)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (19, 1, N'rrrr19', N'rr', 0)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (20, 1, N'Admin', N'Admin Role', 1)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (21, 1, N'rrrr21', N'rr', 0)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (22, 1, N'testRole44', N'testRole desc', 1)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (23, 1, N'ramesh role', N'ramesh role desc', 1)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (24, 1, N'Role_d', N'Enter Description', 0)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (25, 1, N'aa1', N'dfu', 0)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (26, 1, N'r1', N'rr1u', 0)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (27, 1, N'testrole', N'desc', 0)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (28, 1, N'test1', N'desc1', 1)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (29, 1, N'SuperAdmin', N'SA test', 0)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (30, 1, N'sa', N'sa', 0)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (31, 1, N'sa', N'sa', 0)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (32, 1, N'sa', N'sa', 0)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (33, 1, N'sa', N'sa', 0)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (34, 1, N'sa', N'sa', 0)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (35, 1, N'rl', N'rl', 0)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (36, 1, N'rr', N'rr', 0)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (37, 1, N'rr', N'rr', 0)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (38, 1, N'r', N'r', 0)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (39, 1, N'rr', N'rr', 0)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (40, 1, N'rr', N'r', 0)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (41, 1, N'r', N'r', 0)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (42, 1, N'r1', N'r1', 0)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (43, 1, N'r1', N'ru', 0)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (44, 1, N'r2', N'r2u', 1)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (45, 1, N'R2015_1', N'R2015_1_0', 0)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (46, 1, N'R2015_2', N'R2015_1', 0)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (47, 1, N'R2015_3', N'R2015_3_Desc', 0)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (48, 1, N'R2015_3_4', N'R2015_4_Desc', 0)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (49, 1, N'dcdcdcd', N'dcdcdcdfffffffffffffffffffffffffffff', 0)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (50, 1, N'r3', N'r3', 1)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (51, 1, N'r4', N'r4', 1)
INSERT [dbo].[Roles] ([RoleId], [SiteId], [RoleName], [RoleDesc], [Active]) VALUES (52, 1, N'r5', N'r5', 1)
SET IDENTITY_INSERT [dbo].[Roles] OFF
/****** Object:  Table [dbo].[RolePermissions]    Script Date: 02/09/2015 11:50:05 ******/
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
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (9, 1, 2, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (10, 1, 2, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (11, 1, 2, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (20, 1, 2, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (43, 1, 2, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (1, 1, 3, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (2, 1, 3, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (3, 1, 3, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (5, 1, 3, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (6, 1, 3, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (9, 1, 3, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (10, 1, 3, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (11, 1, 3, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (20, 1, 3, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (43, 1, 3, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (1, 1, 4, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (2, 1, 4, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (3, 1, 4, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (5, 1, 4, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (9, 1, 4, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (20, 1, 4, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (1, 1, 7, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (2, 1, 7, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (3, 1, 7, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (5, 1, 7, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (20, 1, 7, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (1, 1, 8, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (2, 1, 8, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (5, 1, 8, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (6, 1, 8, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (20, 1, 8, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (2, 1, 9, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (1, 1, 13, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (2, 1, 13, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (1, 1, 14, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (2, 1, 14, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (1, 1, 15, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (2, 1, 15, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (43, 1, 15, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (2, 1, 16, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (5, 1, 16, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (2, 1, 18, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (6, 1, 18, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (2, 1, 19, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (6, 1, 19, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (18, 1, 19, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (2, 1, 20, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (6, 1, 20, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (18, 1, 20, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (2, 1, 21, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (2, 1, 22, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (20, 1, 37, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (20, 1, 38, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (46, 1, 38, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (43, 1, 39, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (45, 1, 39, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (45, 1, 40, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (45, 1, 41, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (46, 1, 41, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (45, 1, 42, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (46, 1, 42, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (45, 1, 43, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (46, 1, 43, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (43, 1, 45, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (44, 1, 46, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (50, 1, 47, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (51, 1, 48, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (52, 1, 49, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (43, 1, 50, 0)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (44, 1, 51, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (50, 1, 52, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (51, 1, 53, 1)
INSERT [dbo].[RolePermissions] ([RoleId], [SiteId], [PermissionId], [Active]) VALUES (52, 1, 54, 1)
/****** Object:  StoredProcedure [dbo].[spgApplication]    Script Date: 02/09/2015 11:50:05 ******/
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
/****** Object:  StoredProcedure [dbo].[spgAllUser]    Script Date: 02/09/2015 11:50:06 ******/
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
		where active = 1
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spgAllGroups ]    Script Date: 02/09/2015 11:50:06 ******/
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
/****** Object:  StoredProcedure [dbo].[spgGroups]    Script Date: 02/09/2015 11:50:07 ******/
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
/****** Object:  StoredProcedure [dbo].[spgPermissions]    Script Date: 02/09/2015 11:50:07 ******/
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
/****** Object:  StoredProcedure [dbo].[spgPermissionRoles]    Script Date: 02/09/2015 11:50:08 ******/
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
/****** Object:  StoredProcedure [dbo].[spgRolesById]    Script Date: 02/09/2015 11:50:08 ******/
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
/****** Object:  StoredProcedure [dbo].[spgRoles]    Script Date: 02/09/2015 11:50:09 ******/
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
/****** Object:  StoredProcedure [dbo].[spgSiteRoles]    Script Date: 02/09/2015 11:50:09 ******/
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
/****** Object:  StoredProcedure [dbo].[spgSiteGroups]    Script Date: 02/09/2015 11:50:10 ******/
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
/****** Object:  StoredProcedure [dbo].[spiGraph_ATUL]    Script Date: 02/09/2015 11:50:11 ******/
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
/****** Object:  Table [dbo].[GroupRoles]    Script Date: 02/09/2015 11:50:11 ******/
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
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (1, 1, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (1, 2, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (1, 3, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (1, 5, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (1, 6, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (1, 7, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (1, 8, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (1, 9, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (1, 10, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (1, 11, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (1, 14, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (1, 16, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (1, 43, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (3, 1, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (3, 2, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (3, 5, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (3, 6, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (3, 8, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (3, 10, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (4, 1, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (4, 2, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (4, 3, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (4, 5, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (4, 6, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (5, 2, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (5, 5, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (5, 6, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (5, 46, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (6, 2, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (6, 5, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (7, 1, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (7, 2, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (7, 3, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (7, 5, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (8, 2, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (8, 7, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (9, 1, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (9, 2, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (9, 18, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (10, 1, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (10, 2, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (11, 1, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (11, 2, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (11, 3, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (11, 46, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (16, 2, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (17, 1, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (17, 2, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (17, 3, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (18, 2, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (19, 1, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (19, 2, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (19, 3, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (19, 5, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (19, 6, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (19, 7, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (19, 8, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (19, 9, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (19, 10, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (19, 11, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (19, 13, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (19, 14, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (19, 16, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (19, 18, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (19, 19, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (19, 20, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (19, 21, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (20, 2, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (21, 2, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (22, 2, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (23, 2, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (37, 2, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (38, 2, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (42, 2, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (43, 2, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (44, 2, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (45, 2, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (46, 2, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (47, 2, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (47, 20, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (48, 2, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (92, 5, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (92, 43, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (92, 44, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (93, 43, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (93, 50, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (94, 51, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (95, 43, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (95, 52, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (97, 45, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (97, 46, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (97, 48, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (98, 45, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (99, 45, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (100, 45, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (102, 45, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (102, 46, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (103, 45, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (103, 46, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (109, 46, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (110, 43, 1, 0)
GO
print 'Processed 100 total records'
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (111, 43, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (112, 44, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (113, 43, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (114, 50, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (115, 51, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (116, 52, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (117, 43, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (117, 44, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (117, 50, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (117, 51, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (117, 52, 1, 0)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (119, 44, 1, 1)
INSERT [dbo].[GroupRoles] ([GroupId], [RoleId], [SiteId], [Active]) VALUES (123, 14, 1, 0)
/****** Object:  Table [dbo].[Graphs]    Script Date: 02/09/2015 11:50:11 ******/
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
INSERT [dbo].[Graphs] ([ApplicationId], [GraphId], [GraphName], [Graph], [SiteId], [Active], [ExchangeVisible]) VALUES (N'54d581ef-64bc-45c2-9df5-0255f7454e0e', N'1fac1b09-6275-4079-af57-01fe46bd22ba', N'G3u', 0x3C6D617070696E6720786D6C6E733D22687474703A2F2F7777772E6972696E67746F6F6C732E6F72672F6D617070696E672220786D6C6E733A693D22687474703A2F2F7777772E77332E6F72672F323030312F584D4C536368656D612D696E7374616E6365223E0D0A20203C67726170684D6170733E0D0A202020203C67726170684D61703E0D0A2020202020203C6E616D653E47313C2F6E616D653E0D0A2020202020203C636C61737354656D706C6174654D6170733E0D0A20202020202020203C636C61737354656D706C6174654D61703E0D0A202020202020202020203C636C6173734D61703E0D0A2020202020202020202020203C69643E72646C3A5238393436433841343943384134374445413343323030453239323337343632353C2F69643E0D0A2020202020202020202020203C6E616D653E414354494F4E20524551554952454420425920434C49454E543C2F6E616D653E0D0A2020202020202020202020203C6964656E74696669657244656C696D697465723E5F3C2F6964656E74696669657244656C696D697465723E0D0A2020202020202020202020203C6964656E746966696572733E0D0A20202020202020202020202020203C6964656E7469666965723E45515549504D454E542E49443C2F6964656E7469666965723E0D0A2020202020202020202020203C2F6964656E746966696572733E0D0A2020202020202020202020203C706174682F3E0D0A202020202020202020203C2F636C6173734D61703E0D0A202020202020202020203C74656D706C6174654D6170733E0D0A2020202020202020202020203C74656D706C6174654D61703E0D0A20202020202020202020202020203C69643E74706C3A5246413033333235464642354334434230413831413239333044454237433730443C2F69643E0D0A20202020202020202020202020203C6E616D653E41677265656D656E74526571756972656D656E744E756D6265723C2F6E616D653E0D0A20202020202020202020202020203C726F6C654D6170733E0D0A202020202020202020202020202020203C726F6C654D61703E0D0A2020202020202020202020202020202020203C747970653E5265666572656E63653C2F747970653E0D0A2020202020202020202020202020202020203C69643E74706C3A5237413137353130384542413434323732413034353538343137353539313643313C2F69643E0D0A2020202020202020202020202020202020203C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A2020202020202020202020202020202020203C64617461547970653E72646C3A5238363043343144363234343234443246424433313632313143433744323944443C2F64617461547970653E0D0A2020202020202020202020202020202020203C76616C75653E41475245454D454E54205245515549524D454E54204E554D4245523C2F76616C75653E0D0A202020202020202020202020202020203C2F726F6C654D61703E0D0A202020202020202020202020202020203C726F6C654D61703E0D0A2020202020202020202020202020202020203C747970653E5265666572656E63653C2F747970653E0D0A2020202020202020202020202020202020203C69643E74706C3A5242393146444135323333464534334645394441304333333237443142383937383C2F69643E0D0A2020202020202020202020202020202020203C6E616D653E6861734F626A6563743C2F6E616D653E0D0A2020202020202020202020202020202020203C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A2020202020202020202020202020202020203C76616C75653E41475245454D454E54205245515549524D454E54204E554D4245523C2F76616C75653E0D0A202020202020202020202020202020203C2F726F6C654D61703E0D0A202020202020202020202020202020203C726F6C654D61703E0D0A2020202020202020202020202020202020203C747970653E5265666572656E63653C2F747970653E0D0A2020202020202020202020202020202020203C69643E74706C3A5234463442434541383733453534363332423446463835353831394643364245383C2F69643E0D0A2020202020202020202020202020202020203C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A2020202020202020202020202020202020203C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A2020202020202020202020202020202020203C76616C75653E4143544956452046495245204649474854494E472045515549504D454E5420434C4153533C2F76616C75653E0D0A202020202020202020202020202020203C2F726F6C654D61703E0D0A20202020202020202020202020203C2F726F6C654D6170733E0D0A2020202020202020202020203C2F74656D706C6174654D61703E0D0A202020202020202020203C2F74656D706C6174654D6170733E0D0A20202020202020203C2F636C61737354656D706C6174654D61703E0D0A2020202020203C2F636C61737354656D706C6174654D6170733E0D0A2020202020203C646174614F626A6563744E616D653E45515549504D454E543C2F646174614F626A6563744E616D653E0D0A202020203C2F67726170684D61703E0D0A202020203C67726170684D61703E0D0A2020202020203C6E616D653E47323C2F6E616D653E0D0A2020202020203C636C61737354656D706C6174654D6170733E0D0A20202020202020203C636C61737354656D706C6174654D61703E0D0A202020202020202020203C636C6173734D61703E0D0A2020202020202020202020203C69643E72646C3A5238343030383632303730353C2F69643E0D0A2020202020202020202020203C6E616D653E414C49474E4D454E5420494E535452554D454E543C2F6E616D653E0D0A2020202020202020202020203C6964656E74696669657244656C696D697465723E5F3C2F6964656E74696669657244656C696D697465723E0D0A2020202020202020202020203C6964656E746966696572733E0D0A20202020202020202020202020203C6964656E7469666965723E494E535452554D454E54532E494E554D3C2F6964656E7469666965723E0D0A2020202020202020202020203C2F6964656E746966696572733E0D0A2020202020202020202020203C706174682F3E0D0A202020202020202020203C2F636C6173734D61703E0D0A202020202020202020203C74656D706C6174654D6170733E0D0A2020202020202020202020203C74656D706C6174654D61703E0D0A20202020202020202020202020203C69643E74706C3A5234363637333537323130393C2F69643E0D0A20202020202020202020202020203C6E616D653E44617465496E737472756D656E74496E7374616C6C65643C2F6E616D653E0D0A20202020202020202020202020203C726F6C654D6170733E0D0A202020202020202020202020202020203C726F6C654D61703E0D0A2020202020202020202020202020202020203C747970653E5265666572656E63653C2F747970653E0D0A2020202020202020202020202020202020203C69643E74706C3A5233343939414241463130393034353042413737354544364246323435453341413C2F69643E0D0A2020202020202020202020202020202020203C6E616D653E6861734576656E74547970653C2F6E616D653E0D0A2020202020202020202020202020202020203C64617461547970653E72646C3A5236343533363631383639323C2F64617461547970653E0D0A2020202020202020202020202020202020203C76616C75653E444946464552454E5449414C204D4541535552494E4720494E535452554D454E543C2F76616C75653E0D0A202020202020202020202020202020203C2F726F6C654D61703E0D0A202020202020202020202020202020203C726F6C654D61703E0D0A2020202020202020202020202020202020203C747970653E5265666572656E63653C2F747970653E0D0A2020202020202020202020202020202020203C69643E74706C3A5239314146413031433041443034354532383234363143343732414634393341313C2F69643E0D0A2020202020202020202020202020202020203C6E616D653E6861734F626A6563743C2F6E616D653E0D0A2020202020202020202020202020202020203C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A2020202020202020202020202020202020203C76616C75653E444554454354494E4720494E535452554D454E543C2F76616C75653E0D0A202020202020202020202020202020203C2F726F6C654D61703E0D0A202020202020202020202020202020203C726F6C654D61703E0D0A2020202020202020202020202020202020203C747970653E5265666572656E63653C2F747970653E0D0A2020202020202020202020202020202020203C69643E74706C3A5243443130424441433336313634303141413037443738324141323338414141463C2F69643E0D0A2020202020202020202020202020202020203C6E616D653E76616C56616C75653C2F6E616D653E0D0A2020202020202020202020202020202020203C64617461547970653E7873643A6461746554696D653C2F64617461547970653E0D0A2020202020202020202020202020202020203C76616C75653E4445494E5354414C4C494E473C2F76616C75653E0D0A202020202020202020202020202020203C2F726F6C654D61703E0D0A20202020202020202020202020203C2F726F6C654D6170733E0D0A2020202020202020202020203C2F74656D706C6174654D61703E0D0A202020202020202020203C2F74656D706C6174654D6170733E0D0A20202020202020203C2F636C61737354656D706C6174654D61703E0D0A2020202020203C2F636C61737354656D706C6174654D6170733E0D0A2020202020203C646174614F626A6563744E616D653E494E535452554D454E54533C2F646174614F626A6563744E616D653E0D0A202020203C2F67726170684D61703E0D0A20203C2F67726170684D6170733E0D0A20203C76616C75654C6973744D6170733E0D0A202020203C76616C75654C6973744D61703E0D0A2020202020203C6E616D653E56313C2F6E616D653E0D0A2020202020203C76616C75654D6170733E0D0A20202020202020203C76616C75654D61703E0D0A202020202020202020203C696E7465726E616C56616C75653E564D313C2F696E7465726E616C56616C75653E0D0A202020202020202020203C7572693E72646C3A5237323837373032353935323C2F7572693E0D0A202020202020202020203C6C6162656C3E444E56204649584544204F464653484F524520494E5354414C4C4154494F4E53202D204E5620443436303C2F6C6162656C3E0D0A20202020202020203C2F76616C75654D61703E0D0A20202020202020203C76616C75654D61703E0D0A202020202020202020203C696E7465726E616C56616C75653E56323C2F696E7465726E616C56616C75653E0D0A202020202020202020203C7572693E72646C3A5235373639333735383530393C2F7572693E0D0A202020202020202020203C6C6162656C3E444E56204649584544204F464653484F524520494E5354414C4C4154494F4E53202D204E5620443530303C2F6C6162656C3E0D0A20202020202020203C2F76616C75654D61703E0D0A2020202020203C2F76616C75654D6170733E0D0A202020203C2F76616C75654C6973744D61703E0D0A202020203C76616C75654C6973744D61703E0D0A2020202020203C6E616D653E56323C2F6E616D653E0D0A2020202020203C76616C75654D6170733E0D0A20202020202020203C76616C75654D61703E0D0A202020202020202020203C696E7465726E616C56616C75653E56333C2F696E7465726E616C56616C75653E0D0A202020202020202020203C7572693E72646C3A5237343230333335343336363C2F7572693E0D0A202020202020202020203C6C6162656C3E444E56204649584544204F464653484F524520494E5354414C4C4154494F4E53202D204E56204533363C2F6C6162656C3E0D0A20202020202020203C2F76616C75654D61703E0D0A20202020202020203C76616C75654D61703E0D0A202020202020202020203C696E7465726E616C56616C75653E56343C2F696E7465726E616C56616C75653E0D0A202020202020202020203C7572693E72646C3A5238393134353334313837383C2F7572693E0D0A202020202020202020203C6C6162656C3E444E56204649584544204F464653484F524520494E5354414C4C4154494F4E53202D204E56204534303C2F6C6162656C3E0D0A20202020202020203C2F76616C75654D61703E0D0A2020202020203C2F76616C75654D6170733E0D0A202020203C2F76616C75654C6973744D61703E0D0A20203C2F76616C75654C6973744D6170733E0D0A3C2F6D617070696E673E, 1, 1, 1)
INSERT [dbo].[Graphs] ([ApplicationId], [GraphId], [GraphName], [Graph], [SiteId], [Active], [ExchangeVisible]) VALUES (N'fee8b7c8-2a09-42f2-a9de-8f414df8ed8a', N'fe081c16-3dd1-42d2-86c1-0808af1af8b9', N'GraphName2', 0x3C6D617070696E6720786D6C6E733D22687474703A2F2F7777772E6972696E67746F6F6C732E6F72672F6D617070696E672220786D6C6E733A693D22687474703A2F2F7777772E77332E6F72672F323030312F584D4C536368656D612D696E7374616E6365223E0D0A20203C67726170684D6170733E0D0A202020203C67726170684D61703E0D0A2020202020203C6E616D653E47726170684E616D65313C2F6E616D653E0D0A2020202020203C636C61737354656D706C6174654D6170733E0D0A20202020202020203C636C61737354656D706C6174654D61703E0D0A202020202020202020203C636C6173734D61703E0D0A2020202020202020202020203C69643E72646C3A5238313134313130343938383C2F69643E0D0A2020202020202020202020203C6E616D653E444953504C4143454D454E54205049434B2D55503C2F6E616D653E0D0A2020202020202020202020203C6964656E74696669657244656C696D697465723E23233C2F6964656E74696669657244656C696D697465723E0D0A2020202020202020202020203C6964656E746966696572733E0D0A20202020202020202020202020203C6964656E7469666965723E74626C312E74626C315F636F6C313C2F6964656E7469666965723E0D0A2020202020202020202020203C2F6964656E746966696572733E0D0A2020202020202020202020203C706174682F3E0D0A202020202020202020203C2F636C6173734D61703E0D0A202020202020202020203C74656D706C6174654D6170732F3E0D0A20202020202020203C2F636C61737354656D706C6174654D61703E0D0A2020202020203C2F636C61737354656D706C6174654D6170733E0D0A2020202020203C646174614F626A6563744E616D653E74626C313C2F646174614F626A6563744E616D653E0D0A202020203C2F67726170684D61703E0D0A202020203C67726170684D61703E0D0A2020202020203C6E616D653E47726170684E616D65323C2F6E616D653E0D0A2020202020203C636C61737354656D706C6174654D6170733E0D0A20202020202020203C636C61737354656D706C6174654D61703E0D0A202020202020202020203C636C6173734D61703E0D0A2020202020202020202020203C69643E72646C3A5232323030323730303737313C2F69643E0D0A2020202020202020202020203C6E616D653E42494359434C45205049434B55503C2F6E616D653E0D0A2020202020202020202020203C6964656E74696669657244656C696D697465723E7E3C2F6964656E74696669657244656C696D697465723E0D0A2020202020202020202020203C6964656E746966696572733E0D0A20202020202020202020202020203C6964656E7469666965723E74626C322E74626C325F636F6C313C2F6964656E7469666965723E0D0A2020202020202020202020203C2F6964656E746966696572733E0D0A2020202020202020202020203C706174682F3E0D0A202020202020202020203C2F636C6173734D61703E0D0A202020202020202020203C74656D706C6174654D6170733E0D0A2020202020202020202020203C74656D706C6174654D61703E0D0A20202020202020202020202020203C69643E74706C3A5239383935393232353538353C2F69643E0D0A20202020202020202020202020203C6E616D653E506970696E674E6574776F726B5365676D656E744861734F626A6563743C2F6E616D653E0D0A20202020202020202020202020203C726F6C654D6170733E0D0A202020202020202020202020202020203C726F6C654D61703E0D0A2020202020202020202020202020202020203C747970653E5265666572656E63653C2F747970653E0D0A2020202020202020202020202020202020203C69643E74706C3A5239373543413138354630324334393531424539463931314534454136363339363C2F69643E0D0A2020202020202020202020202020202020203C6E616D653E686173417373656D626C79547970653C2F6E616D653E0D0A2020202020202020202020202020202020203C64617461547970653E72646C3A5236313331373835313134383C2F64617461547970653E0D0A2020202020202020202020202020202020203C76616C75653E504950494E47204E4554574F524B20534547454D454E5420484153204F424A4543543C2F76616C75653E0D0A202020202020202020202020202020203C2F726F6C654D61703E0D0A202020202020202020202020202020203C726F6C654D61703E0D0A2020202020202020202020202020202020203C747970653E5265666572656E63653C2F747970653E0D0A2020202020202020202020202020202020203C69643E74706C3A5234393344444634394230424634463546413545353837453134423235334633343C2F69643E0D0A2020202020202020202020202020202020203C6E616D653E686173506172743C2F6E616D653E0D0A2020202020202020202020202020202020203C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A2020202020202020202020202020202020203C76616C75653E4E4554574F524B20434F4E56455253494F4E3C2F76616C75653E0D0A202020202020202020202020202020203C2F726F6C654D61703E0D0A202020202020202020202020202020203C726F6C654D61703E0D0A2020202020202020202020202020202020203C747970653E5265666572656E63653C2F747970653E0D0A2020202020202020202020202020202020203C69643E74706C3A5246343437324643383745324134443736413542373830433031363933423438383C2F69643E0D0A2020202020202020202020202020202020203C6E616D653E68617357686F6C653C2F6E616D653E0D0A2020202020202020202020202020202020203C64617461547970653E72646C3A5235383730313935383433363C2F64617461547970653E0D0A2020202020202020202020202020202020203C76616C75653E4E4554574F524B2043414C43554C4154494F4E3C2F76616C75653E0D0A202020202020202020202020202020203C2F726F6C654D61703E0D0A20202020202020202020202020203C2F726F6C654D6170733E0D0A2020202020202020202020203C2F74656D706C6174654D61703E0D0A202020202020202020203C2F74656D706C6174654D6170733E0D0A20202020202020203C2F636C61737354656D706C6174654D61703E0D0A2020202020203C2F636C61737354656D706C6174654D6170733E0D0A2020202020203C646174614F626A6563744E616D653E74626C323C2F646174614F626A6563744E616D653E0D0A202020203C2F67726170684D61703E0D0A20203C2F67726170684D6170733E0D0A20203C76616C75654C6973744D6170733E0D0A202020203C76616C75654C6973744D61703E0D0A2020202020203C6E616D653E56616C75654C497374313C2F6E616D653E0D0A2020202020203C76616C75654D6170733E0D0A20202020202020203C76616C75654D61703E0D0A202020202020202020203C696E7465726E616C56616C75653E56616C75654D6170494E313C2F696E7465726E616C56616C75653E0D0A202020202020202020203C7572693E72646C3A5232323030323730303737313C2F7572693E0D0A202020202020202020203C6C6162656C3E42494359434C45205049434B5550203C2F6C6162656C3E0D0A20202020202020203C2F76616C75654D61703E0D0A20202020202020203C76616C75654D61703E0D0A202020202020202020203C696E7465726E616C56616C75653E56616C75654D6170494E323C2F696E7465726E616C56616C75653E0D0A202020202020202020203C7572693E72646C3A5233363132383831333231363C2F7572693E0D0A202020202020202020203C6C6162656C3E414343454C45524F4D455445522F414343454C45524154494F4E205049434B2D55503C2F6C6162656C3E0D0A20202020202020203C2F76616C75654D61703E0D0A2020202020203C2F76616C75654D6170733E0D0A202020203C2F76616C75654C6973744D61703E0D0A202020203C76616C75654C6973744D61703E0D0A2020202020203C6E616D653E56616C75654C697374323C2F6E616D653E0D0A2020202020203C76616C75654D6170733E0D0A20202020202020203C76616C75654D61703E0D0A202020202020202020203C696E7465726E616C56616C75653E564D313C2F696E7465726E616C56616C75653E0D0A202020202020202020203C7572693E72646C3A5232323030323730303737313C2F7572693E0D0A202020202020202020203C6C6162656C3E42494359434C45205049434B55503C2F6C6162656C3E0D0A20202020202020203C2F76616C75654D61703E0D0A20202020202020203C76616C75654D61703E0D0A202020202020202020203C696E7465726E616C56616C75653E564D323C2F696E7465726E616C56616C75653E0D0A202020202020202020203C7572693E72646C3A5233363132383831333231363C2F7572693E0D0A202020202020202020203C6C6162656C3E414343454C45524F4D455445522F414343454C45524154494F4E205049434B2D55503C2F6C6162656C3E0D0A20202020202020203C2F76616C75654D61703E0D0A2020202020203C2F76616C75654D6170733E0D0A202020203C2F76616C75654C6973744D61703E0D0A20203C2F76616C75654C6973744D6170733E0D0A3C2F6D617070696E673E, 1, 1, 1)
INSERT [dbo].[Graphs] ([ApplicationId], [GraphId], [GraphName], [Graph], [SiteId], [Active], [ExchangeVisible]) VALUES (N'3456c504-133d-41b9-8780-7bfd73840444', N'47787dd6-0f59-417d-98d8-3eb59545d239', N'Gagan', 0x3C67726170687320786D6C6E733A693D22687474703A2F2F7777772E77332E6F72672F323030312F584D4C536368656D612D696E7374616E63652220786D6C6E733D22687474703A2F2F7777772E6972696E67746F6F6C732E6F72672F647866722F6D616E6966657374223E0D0A09093C67726170683E0D0A0909093C6E616D653E4C696E65733C2F6E616D653E0D0A0909093C636C61737354656D706C617465734C6973743E0D0A090909093C636C61737354656D706C617465733E0D0A09090909093C636C6173733E0D0A0909090909093C69643E72646C3A5231393139323436323535303C2F69643E0D0A0909090909093C6E616D653E504950494E47204E4554574F524B2053595354454D3C2F6E616D653E0D0A0909090909093C6B6579733E0D0A090909090909093C6B65793E0D0A09090909090909093C636C61737349643E72646C3A5231393139323436323535303C2F636C61737349643E0D0A09090909090909093C74656D706C61746549643E74706C3A5236363932313130313738333C2F74656D706C61746549643E0D0A09090909090909093C726F6C6549643E74706C3A5232323637343734393638383C2F726F6C6549643E0D0A090909090909093C2F6B65793E0D0A0909090909093C2F6B6579733E0D0A0909090909093C696E6465783E303C2F696E6465783E0D0A0909090909093C7061746820693A6E696C3D227472756522202F3E0D0A09090909093C2F636C6173733E0D0A09090909093C74656D706C617465733E0D0A0909090909093C74656D706C6174653E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C65733E0D0A09090909090909093C726F6C653E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C653E0D0A09090909090909093C726F6C653E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C76616C75653E72646C3A5234303437313034313735343C2F76616C75653E0D0A09090909090909093C2F726F6C653E0D0A09090909090909093C726F6C653E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C646174614C656E6774683E3130303C2F646174614C656E6774683E0D0A09090909090909093C2F726F6C653E0D0A090909090909093C2F726F6C65733E0D0A090909090909093C7472616E736665724F7074696F6E3E52657175697265643C2F7472616E736665724F7074696F6E3E0D0A0909090909093C2F74656D706C6174653E0D0A0909090909093C74656D706C6174653E0D0A090909090909093C69643E74706C3A5234313234383736373834363C2F69643E0D0A090909090909093C6E616D653E506C616E7441726561486173506C616E744F626A6563743C2F6E616D653E0D0A090909090909093C726F6C65733E0D0A09090909090909093C726F6C653E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5237373538353332353537363C2F69643E0D0A0909090909090909093C6E616D653E706172743C2F6E616D653E0D0A09090909090909093C2F726F6C653E0D0A09090909090909093C726F6C653E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5236323135373439383031323C2F69643E0D0A0909090909090909093C6E616D653E617373656D626C79547970653C2F6E616D653E0D0A0909090909090909093C76616C75653E72646C3A5233303934323337383439323C2F76616C75653E0D0A09090909090909093C2F726F6C653E0D0A09090909090909093C726F6C653E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5236323334383032303230313C2F69643E0D0A0909090909090909093C6E616D653E77686F6C653C2F6E616D653E0D0A0909090909090909093C76616C75653E72646C3A5234393635383331393833333C2F76616C75653E0D0A0909090909090909093C636C6173733E0D0A090909090909090909093C69643E72646C3A5234393635383331393833333C2F69643E0D0A090909090909090909093C6E616D653E504C414E5420415245413C2F6E616D653E0D0A090909090909090909093C6B65797320693A6E696C3D227472756522202F3E0D0A090909090909090909093C696E6465783E303C2F696E6465783E0D0A090909090909090909093C7061746820693A6E696C3D227472756522202F3E0D0A0909090909090909093C2F636C6173733E0D0A0909090909090909093C63617264696E616C6974793E4F6E65546F4F6E653C2F63617264696E616C6974793E0D0A09090909090909093C2F726F6C653E0D0A090909090909093C2F726F6C65733E0D0A090909090909093C7472616E736665724F7074696F6E3E446573697265643C2F7472616E736665724F7074696F6E3E0D0A0909090909093C2F74656D706C6174653E0D0A0909090909093C74656D706C6174653E0D0A090909090909093C69643E74706C3A5231313332373238313438353C2F69643E0D0A090909090909093C6E616D653E50416E644944526570726573656E746174696F6E3C2F6E616D653E0D0A090909090909093C726F6C65733E0D0A09090909090909093C726F6C653E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323731353031333837373C2F69643E0D0A0909090909090909093C6E616D653E6F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C653E0D0A09090909090909093C726F6C653E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233353239303932393137353C2F69643E0D0A0909090909090909093C6E616D653E726570726573656E746174696F6E547970653C2F6E616D653E0D0A0909090909090909093C76616C75653E72646C3A5239393438363933313937353C2F76616C75653E0D0A09090909090909093C2F726F6C653E0D0A09090909090909093C726F6C653E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5237343332383831353731303C2F69643E0D0A0909090909090909093C6E616D653E726570726573656E746174696F6E3C2F6E616D653E0D0A0909090909090909093C76616C75653E72646C3A5239393438363933313937353C2F76616C75653E0D0A0909090909090909093C636C6173733E0D0A090909090909090909093C69643E72646C3A5239393438363933313937353C2F69643E0D0A090909090909090909093C6E616D653E5020414E442049204449414752414D3C2F6E616D653E0D0A090909090909090909093C6B65797320693A6E696C3D227472756522202F3E0D0A090909090909090909093C696E6465783E303C2F696E6465783E0D0A090909090909090909093C7061746820693A6E696C3D227472756522202F3E0D0A0909090909090909093C2F636C6173733E0D0A0909090909090909093C63617264696E616C6974793E4F6E65546F4F6E653C2F63617264696E616C6974793E0D0A09090909090909093C2F726F6C653E0D0A090909090909093C2F726F6C65733E0D0A090909090909093C7472616E736665724F7074696F6E3E446573697265643C2F7472616E736665724F7074696F6E3E0D0A0909090909093C2F74656D706C6174653E0D0A0909090909093C74656D706C6174653E0D0A090909090909093C69643E74706C3A5237373434333335383831383C2F69643E0D0A090909090909093C6E616D653E4E6F6D696E616C4469616D657465723C2F6E616D653E0D0A090909090909093C726F6C65733E0D0A09090909090909093C726F6C653E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5231363331343132373832353C2F69643E0D0A0909090909090909093C6E616D653E686173506F73736573736F723C2F6E616D653E0D0A09090909090909093C2F726F6C653E0D0A09090909090909093C726F6C653E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5239333838343139333639323C2F69643E0D0A0909090909090909093C6E616D653E6861735363616C653C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C646174614C656E6774683E32303C2F646174614C656E6774683E0D0A09090909090909093C2F726F6C653E0D0A09090909090909093C726F6C653E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5235393839363835333233393C2F69643E0D0A0909090909090909093C6E616D653E686173547970653C2F6E616D653E0D0A0909090909090909093C76616C75653E72646C3A5231373632323134383034333C2F76616C75653E0D0A09090909090909093C2F726F6C653E0D0A09090909090909093C726F6C653E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5239303638383036333634383C2F69643E0D0A0909090909090909093C6E616D653E76616C56616C75653C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A73696E676C653C2F64617461547970653E0D0A0909090909090909093C646174614C656E6774683E383C2F646174614C656E6774683E0D0A09090909090909093C2F726F6C653E0D0A090909090909093C2F726F6C65733E0D0A090909090909093C7472616E736665724F7074696F6E3E446573697265643C2F7472616E736665724F7074696F6E3E0D0A0909090909093C2F74656D706C6174653E0D0A09090909093C2F74656D706C617465733E0D0A090909093C2F636C61737354656D706C617465733E0D0A090909093C636C61737354656D706C617465733E0D0A09090909093C636C6173733E0D0A0909090909093C69643E72646C3A5234393635383331393833333C2F69643E0D0A0909090909093C6E616D653E504C414E5420415245413C2F6E616D653E0D0A0909090909093C6B6579733E0D0A090909090909093C6B65793E0D0A09090909090909093C636C61737349643E72646C3A5234393635383331393833333C2F636C61737349643E0D0A09090909090909093C74656D706C61746549643E74706C3A5236363932313130313738333C2F74656D706C61746549643E0D0A09090909090909093C726F6C6549643E74706C3A5232323637343734393638383C2F726F6C6549643E0D0A090909090909093C2F6B65793E0D0A0909090909093C2F6B6579733E0D0A0909090909093C696E6465783E303C2F696E6465783E0D0A0909090909093C7061746820693A6E696C3D227472756522202F3E0D0A09090909093C2F636C6173733E0D0A09090909093C74656D706C617465733E0D0A0909090909093C74656D706C6174653E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C65733E0D0A09090909090909093C726F6C653E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C653E0D0A09090909090909093C726F6C653E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C76616C75653E72646C3A5234303437313034313735343C2F76616C75653E0D0A09090909090909093C2F726F6C653E0D0A09090909090909093C726F6C653E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C646174614C656E6774683E31303C2F646174614C656E6774683E0D0A09090909090909093C2F726F6C653E0D0A090909090909093C2F726F6C65733E0D0A090909090909093C7472616E736665724F7074696F6E3E446573697265643C2F7472616E736665724F7074696F6E3E0D0A0909090909093C2F74656D706C6174653E0D0A09090909093C2F74656D706C617465733E0D0A090909093C2F636C61737354656D706C617465733E0D0A090909093C636C61737354656D706C617465733E0D0A09090909093C636C6173733E0D0A0909090909093C69643E72646C3A5239393438363933313937353C2F69643E0D0A0909090909093C6E616D653E5020414E442049204449414752414D3C2F6E616D653E0D0A0909090909093C6B6579733E0D0A090909090909093C6B65793E0D0A09090909090909093C636C61737349643E72646C3A5239393438363933313937353C2F636C61737349643E0D0A09090909090909093C74656D706C61746549643E74706C3A5233303139333338363237333C2F74656D706C61746549643E0D0A09090909090909093C726F6C6549643E74706C3A5232323637343734393638383C2F726F6C6549643E0D0A090909090909093C2F6B65793E0D0A0909090909093C2F6B6579733E0D0A0909090909093C696E6465783E303C2F696E6465783E0D0A0909090909093C7061746820693A6E696C3D227472756522202F3E0D0A09090909093C2F636C6173733E0D0A09090909093C74656D706C617465733E0D0A0909090909093C74656D706C6174653E0D0A090909090909093C69643E74706C3A5233303139333338363237333C2F69643E0D0A090909090909093C6E616D653E436C61737369666965644964656E74696669636174696F6E3C2F6E616D653E0D0A090909090909093C726F6C65733E0D0A09090909090909093C726F6C653E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C653E0D0A09090909090909093C726F6C653E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C76616C75653E72646C3A5231363839333238333035303C2F76616C75653E0D0A09090909090909093C2F726F6C653E0D0A09090909090909093C726F6C653E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C646174614C656E6774683E35303C2F646174614C656E6774683E0D0A09090909090909093C2F726F6C653E0D0A090909090909093C2F726F6C65733E0D0A090909090909093C7472616E736665724F7074696F6E3E446573697265643C2F7472616E736665724F7074696F6E3E0D0A0909090909093C2F74656D706C6174653E0D0A09090909093C2F74656D706C617465733E0D0A090909093C2F636C61737354656D706C617465733E0D0A0909093C2F636C61737354656D706C617465734C6973743E0D0A09093C2F67726170683E0D0A093C2F6772617068733E, 1, 1, 1)
INSERT [dbo].[Graphs] ([ApplicationId], [GraphId], [GraphName], [Graph], [SiteId], [Active], [ExchangeVisible]) VALUES (N'3e1e418d-fa51-403b-bb7b-0cade34283a7', N'6f9f8981-55f3-433c-a86c-4b1262ff2efc', N'abc', 0x3C6D617070696E6720786D6C6E733D22687474703A2F2F7777772E6972696E67746F6F6C732E6F72672F6D617070696E672220786D6C6E733A693D22687474703A2F2F7777772E77332E6F72672F323030312F584D4C536368656D612D696E7374616E6365223E0D0A093C67726170684D6170733E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E4C696E65733C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5231393139323436323535303C2F69643E0D0A0909090909093C6E616D653E504950494E47204E4554574F524B2053595354454D3C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E5441473C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234303437313034313735343C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204E414D45202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E5441473C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5234313234383736373834363C2F69643E0D0A090909090909093C6E616D653E506C616E7441726561486173506C616E744F626A6563743C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5237373538353332353537363C2F69643E0D0A0909090909090909093C6E616D653E706172743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5236323135373439383031323C2F69643E0D0A0909090909090909093C6E616D653E617373656D626C79547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5233303934323337383439323C2F64617461547970653E0D0A0909090909090909093C76616C75653E504C414E5420415245412048415320504C414E54204F424A4543543C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5236323334383032303230313C2F69643E0D0A0909090909090909093C6E616D653E77686F6C653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234393635383331393833333C2F64617461547970653E0D0A0909090909090909093C76616C75653E504C414E5420415245413C2F76616C75653E0D0A0909090909090909093C636C6173734D61703E0D0A090909090909090909093C69643E72646C3A5234393635383331393833333C2F69643E0D0A090909090909090909093C6E616D653E504C414E5420415245413C2F6E616D653E0D0A090909090909090909093C6964656E746966696572733E0D0A09090909090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A090909090909090909093C2F6964656E746966696572733E0D0A090909090909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5234313234383736373834362830292F74706C3A5236323334383032303230313C2F706174683E0D0A0909090909090909093C2F636C6173734D61703E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5231313332373238313438353C2F69643E0D0A090909090909093C6E616D653E50416E644944526570726573656E746174696F6E3C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323731353031333837373C2F69643E0D0A0909090909090909093C6E616D653E6F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233353239303932393137353C2F69643E0D0A0909090909090909093C6E616D653E726570726573656E746174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393438363933313937353C2F64617461547970653E0D0A0909090909090909093C76616C75653E5020414E442049204449414752414D3C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5237343332383831353731303C2F69643E0D0A0909090909090909093C6E616D653E726570726573656E746174696F6E3C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393438363933313937353C2F64617461547970653E0D0A0909090909090909093C76616C75653E5020414E442049204449414752414D3C2F76616C75653E0D0A0909090909090909093C636C6173734D61703E0D0A090909090909090909093C69643E72646C3A5239393438363933313937353C2F69643E0D0A090909090909090909093C6E616D653E5020414E442049204449414752414D3C2F6E616D653E0D0A090909090909090909093C6964656E746966696572733E0D0A09090909090909090909093C6964656E7469666965723E4C494E45532E5049444E554D4245523C2F6964656E7469666965723E0D0A090909090909090909093C2F6964656E746966696572733E0D0A090909090909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5231313332373238313438352830292F74706C3A5237343332383831353731303C2F706174683E0D0A0909090909090909093C2F636C6173734D61703E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5237373434333335383831383C2F69643E0D0A090909090909093C6E616D653E4E6F6D696E616C4469616D657465723C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5231363331343132373832353C2F69643E0D0A0909090909090909093C6E616D653E686173506F73736573736F723C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5239333838343139333639323C2F69643E0D0A0909090909090909093C6E616D653E6861735363616C653C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E554F4D5F4E4F4D4449414D455445523C2F70726F70657274794E616D653E0D0A0909090909090909093C76616C75654C6973744E616D653E4C656E6774683C2F76616C75654C6973744E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5235393839363835333233393C2F69643E0D0A0909090909090909093C6E616D653E686173547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5231373632323134383034333C2F64617461547970653E0D0A0909090909090909093C76616C75653E4E4F4D494E414C204449414D455445523C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5239303638383036333634383C2F69643E0D0A0909090909090909093C6E616D653E76616C56616C75653C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A73696E676C653C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E4E4F4D4449414D455445523C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5234393635383331393833333C2F69643E0D0A0909090909093C6E616D653E504C414E5420415245413C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5234313234383736373834362830292F74706C3A5236323334383032303230313C2F706174683E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234303437313034313735343C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204E414D45202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E415245413C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5239393438363933313937353C2F69643E0D0A0909090909093C6E616D653E5020414E442049204449414752414D3C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E5049444E554D4245523C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5231313332373238313438352830292F74706C3A5237343332383831353731303C2F706174683E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5233303139333338363237333C2F69643E0D0A090909090909093C6E616D653E436C61737369666965644964656E74696669636174696F6E3C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5231363839333238333035303C2F64617461547970653E0D0A0909090909090909093C76616C75653E44524157494E47204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E5049444E554D4245523C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E4C494E45533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E4C313C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5231393139323436323535303C2F69643E0D0A0909090909093C6E616D653E504950494E47204E4554574F524B2053595354454D3C2F6E616D653E0D0A0909090909093C6964656E74696669657244656C696D697465723E5F3C2F6964656E74696669657244656C696D697465723E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E415245413C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E4C494E45533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E546573743C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5239383530353931383430343C2F69643E0D0A0909090909093C6E616D653E494E535452554D454E543C2F6E616D653E0D0A0909090909093C6964656E74696669657244656C696D697465723E5F3C2F6964656E74696669657244656C696D697465723E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E494E535452554D454E54532E5441473C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E494E535452554D454E54532E5441473C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E494E535452554D454E54532E4153534F435F45513C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A090909090909093C696E6465783E313C2F696E6465783E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E494E535452554D454E54533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A093C2F67726170684D6170733E0D0A093C76616C75654C6973744D6170733E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E4C656E6774683C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E6D6D3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5235323035343237353337343C2F7572693E0D0A09090909093C6C6162656C3E4D494C4C494D45545245203C2F6C6162656C3E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E54656D70657261747572653C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E646567433C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5237343837373939323730333C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E6465674B3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5235333130393636323430353C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E50726573737572653C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E50415343414C3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5239383738373735343236373C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E4241523C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5238333439303530363535313C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E6C313C2F6E616D653E0D0A0909093C76616C75654D6170732F3E0D0A09093C2F76616C75654C6973744D61703E0D0A093C2F76616C75654C6973744D6170733E0D0A3C2F6D617070696E673E, 1, 1, 1)
INSERT [dbo].[Graphs] ([ApplicationId], [GraphId], [GraphName], [Graph], [SiteId], [Active], [ExchangeVisible]) VALUES (N'54d581ef-64bc-45c2-9df5-0255f7454e0e', N'f0783082-c385-466b-9b98-60b03289171c', N'G2', 0x3C6D617070696E6720786D6C6E733D22687474703A2F2F7777772E6972696E67746F6F6C732E6F72672F6D617070696E672220786D6C6E733A693D22687474703A2F2F7777772E77332E6F72672F323030312F584D4C536368656D612D696E7374616E6365223E0D0A20203C67726170684D6170733E0D0A202020203C67726170684D61703E0D0A2020202020203C6E616D653E47313C2F6E616D653E0D0A2020202020203C636C61737354656D706C6174654D6170733E0D0A20202020202020203C636C61737354656D706C6174654D61703E0D0A202020202020202020203C636C6173734D61703E0D0A2020202020202020202020203C69643E72646C3A5238393436433841343943384134374445413343323030453239323337343632353C2F69643E0D0A2020202020202020202020203C6E616D653E414354494F4E20524551554952454420425920434C49454E543C2F6E616D653E0D0A2020202020202020202020203C6964656E74696669657244656C696D697465723E5F3C2F6964656E74696669657244656C696D697465723E0D0A2020202020202020202020203C6964656E746966696572733E0D0A20202020202020202020202020203C6964656E7469666965723E45515549504D454E542E49443C2F6964656E7469666965723E0D0A2020202020202020202020203C2F6964656E746966696572733E0D0A2020202020202020202020203C706174682F3E0D0A202020202020202020203C2F636C6173734D61703E0D0A202020202020202020203C74656D706C6174654D6170733E0D0A2020202020202020202020203C74656D706C6174654D61703E0D0A20202020202020202020202020203C69643E74706C3A5246413033333235464642354334434230413831413239333044454237433730443C2F69643E0D0A20202020202020202020202020203C6E616D653E41677265656D656E74526571756972656D656E744E756D6265723C2F6E616D653E0D0A20202020202020202020202020203C726F6C654D6170733E0D0A202020202020202020202020202020203C726F6C654D61703E0D0A2020202020202020202020202020202020203C747970653E5265666572656E63653C2F747970653E0D0A2020202020202020202020202020202020203C69643E74706C3A5237413137353130384542413434323732413034353538343137353539313643313C2F69643E0D0A2020202020202020202020202020202020203C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A2020202020202020202020202020202020203C64617461547970653E72646C3A5238363043343144363234343234443246424433313632313143433744323944443C2F64617461547970653E0D0A2020202020202020202020202020202020203C76616C75653E41475245454D454E54205245515549524D454E54204E554D4245523C2F76616C75653E0D0A202020202020202020202020202020203C2F726F6C654D61703E0D0A202020202020202020202020202020203C726F6C654D61703E0D0A2020202020202020202020202020202020203C747970653E5265666572656E63653C2F747970653E0D0A2020202020202020202020202020202020203C69643E74706C3A5242393146444135323333464534334645394441304333333237443142383937383C2F69643E0D0A2020202020202020202020202020202020203C6E616D653E6861734F626A6563743C2F6E616D653E0D0A2020202020202020202020202020202020203C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A2020202020202020202020202020202020203C76616C75653E41475245454D454E54205245515549524D454E54204E554D4245523C2F76616C75653E0D0A202020202020202020202020202020203C2F726F6C654D61703E0D0A202020202020202020202020202020203C726F6C654D61703E0D0A2020202020202020202020202020202020203C747970653E5265666572656E63653C2F747970653E0D0A2020202020202020202020202020202020203C69643E74706C3A5234463442434541383733453534363332423446463835353831394643364245383C2F69643E0D0A2020202020202020202020202020202020203C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A2020202020202020202020202020202020203C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A2020202020202020202020202020202020203C76616C75653E4143544956452046495245204649474854494E472045515549504D454E5420434C4153533C2F76616C75653E0D0A202020202020202020202020202020203C2F726F6C654D61703E0D0A20202020202020202020202020203C2F726F6C654D6170733E0D0A2020202020202020202020203C2F74656D706C6174654D61703E0D0A202020202020202020203C2F74656D706C6174654D6170733E0D0A20202020202020203C2F636C61737354656D706C6174654D61703E0D0A2020202020203C2F636C61737354656D706C6174654D6170733E0D0A2020202020203C646174614F626A6563744E616D653E45515549504D454E543C2F646174614F626A6563744E616D653E0D0A202020203C2F67726170684D61703E0D0A202020203C67726170684D61703E0D0A2020202020203C6E616D653E47323C2F6E616D653E0D0A2020202020203C636C61737354656D706C6174654D6170733E0D0A20202020202020203C636C61737354656D706C6174654D61703E0D0A202020202020202020203C636C6173734D61703E0D0A2020202020202020202020203C69643E72646C3A5238343030383632303730353C2F69643E0D0A2020202020202020202020203C6E616D653E414C49474E4D454E5420494E535452554D454E543C2F6E616D653E0D0A2020202020202020202020203C6964656E74696669657244656C696D697465723E5F3C2F6964656E74696669657244656C696D697465723E0D0A2020202020202020202020203C6964656E746966696572733E0D0A20202020202020202020202020203C6964656E7469666965723E494E535452554D454E54532E494E554D3C2F6964656E7469666965723E0D0A2020202020202020202020203C2F6964656E746966696572733E0D0A2020202020202020202020203C706174682F3E0D0A202020202020202020203C2F636C6173734D61703E0D0A202020202020202020203C74656D706C6174654D6170733E0D0A2020202020202020202020203C74656D706C6174654D61703E0D0A20202020202020202020202020203C69643E74706C3A5234363637333537323130393C2F69643E0D0A20202020202020202020202020203C6E616D653E44617465496E737472756D656E74496E7374616C6C65643C2F6E616D653E0D0A20202020202020202020202020203C726F6C654D6170733E0D0A202020202020202020202020202020203C726F6C654D61703E0D0A2020202020202020202020202020202020203C747970653E5265666572656E63653C2F747970653E0D0A2020202020202020202020202020202020203C69643E74706C3A5233343939414241463130393034353042413737354544364246323435453341413C2F69643E0D0A2020202020202020202020202020202020203C6E616D653E6861734576656E74547970653C2F6E616D653E0D0A2020202020202020202020202020202020203C64617461547970653E72646C3A5236343533363631383639323C2F64617461547970653E0D0A2020202020202020202020202020202020203C76616C75653E444946464552454E5449414C204D4541535552494E4720494E535452554D454E543C2F76616C75653E0D0A202020202020202020202020202020203C2F726F6C654D61703E0D0A202020202020202020202020202020203C726F6C654D61703E0D0A2020202020202020202020202020202020203C747970653E5265666572656E63653C2F747970653E0D0A2020202020202020202020202020202020203C69643E74706C3A5239314146413031433041443034354532383234363143343732414634393341313C2F69643E0D0A2020202020202020202020202020202020203C6E616D653E6861734F626A6563743C2F6E616D653E0D0A2020202020202020202020202020202020203C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A2020202020202020202020202020202020203C76616C75653E444554454354494E4720494E535452554D454E543C2F76616C75653E0D0A202020202020202020202020202020203C2F726F6C654D61703E0D0A202020202020202020202020202020203C726F6C654D61703E0D0A2020202020202020202020202020202020203C747970653E5265666572656E63653C2F747970653E0D0A2020202020202020202020202020202020203C69643E74706C3A5243443130424441433336313634303141413037443738324141323338414141463C2F69643E0D0A2020202020202020202020202020202020203C6E616D653E76616C56616C75653C2F6E616D653E0D0A2020202020202020202020202020202020203C64617461547970653E7873643A6461746554696D653C2F64617461547970653E0D0A2020202020202020202020202020202020203C76616C75653E4445494E5354414C4C494E473C2F76616C75653E0D0A202020202020202020202020202020203C2F726F6C654D61703E0D0A20202020202020202020202020203C2F726F6C654D6170733E0D0A2020202020202020202020203C2F74656D706C6174654D61703E0D0A202020202020202020203C2F74656D706C6174654D6170733E0D0A20202020202020203C2F636C61737354656D706C6174654D61703E0D0A2020202020203C2F636C61737354656D706C6174654D6170733E0D0A2020202020203C646174614F626A6563744E616D653E494E535452554D454E54533C2F646174614F626A6563744E616D653E0D0A202020203C2F67726170684D61703E0D0A20203C2F67726170684D6170733E0D0A20203C76616C75654C6973744D6170733E0D0A202020203C76616C75654C6973744D61703E0D0A2020202020203C6E616D653E56313C2F6E616D653E0D0A2020202020203C76616C75654D6170733E0D0A20202020202020203C76616C75654D61703E0D0A202020202020202020203C696E7465726E616C56616C75653E564D313C2F696E7465726E616C56616C75653E0D0A202020202020202020203C7572693E72646C3A5237323837373032353935323C2F7572693E0D0A202020202020202020203C6C6162656C3E444E56204649584544204F464653484F524520494E5354414C4C4154494F4E53202D204E5620443436303C2F6C6162656C3E0D0A20202020202020203C2F76616C75654D61703E0D0A20202020202020203C76616C75654D61703E0D0A202020202020202020203C696E7465726E616C56616C75653E56323C2F696E7465726E616C56616C75653E0D0A202020202020202020203C7572693E72646C3A5235373639333735383530393C2F7572693E0D0A202020202020202020203C6C6162656C3E444E56204649584544204F464653484F524520494E5354414C4C4154494F4E53202D204E5620443530303C2F6C6162656C3E0D0A20202020202020203C2F76616C75654D61703E0D0A2020202020203C2F76616C75654D6170733E0D0A202020203C2F76616C75654C6973744D61703E0D0A202020203C76616C75654C6973744D61703E0D0A2020202020203C6E616D653E56323C2F6E616D653E0D0A2020202020203C76616C75654D6170733E0D0A20202020202020203C76616C75654D61703E0D0A202020202020202020203C696E7465726E616C56616C75653E56333C2F696E7465726E616C56616C75653E0D0A202020202020202020203C7572693E72646C3A5237343230333335343336363C2F7572693E0D0A202020202020202020203C6C6162656C3E444E56204649584544204F464653484F524520494E5354414C4C4154494F4E53202D204E56204533363C2F6C6162656C3E0D0A20202020202020203C2F76616C75654D61703E0D0A20202020202020203C76616C75654D61703E0D0A202020202020202020203C696E7465726E616C56616C75653E56343C2F696E7465726E616C56616C75653E0D0A202020202020202020203C7572693E72646C3A5238393134353334313837383C2F7572693E0D0A202020202020202020203C6C6162656C3E444E56204649584544204F464653484F524520494E5354414C4C4154494F4E53202D204E56204534303C2F6C6162656C3E0D0A20202020202020203C2F76616C75654D61703E0D0A2020202020203C2F76616C75654D6170733E0D0A202020203C2F76616C75654C6973744D61703E0D0A20203C2F76616C75654C6973744D6170733E0D0A3C2F6D617070696E673E, 1, 1, 1)
INSERT [dbo].[Graphs] ([ApplicationId], [GraphId], [GraphName], [Graph], [SiteId], [Active], [ExchangeVisible]) VALUES (N'3e1e418d-fa51-403b-bb7b-0cade34283a7', N'87a895d1-dac2-4083-b900-640ecba914e8', N'TEST', 0x3C6D617070696E6720786D6C6E733D22687474703A2F2F7777772E6972696E67746F6F6C732E6F72672F6D617070696E672220786D6C6E733A693D22687474703A2F2F7777772E77332E6F72672F323030312F584D4C536368656D612D696E7374616E6365223E0D0A093C67726170684D6170733E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E4C696E65733C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5231393139323436323535303C2F69643E0D0A0909090909093C6E616D653E504950494E47204E4554574F524B2053595354454D3C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E5441473C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234303437313034313735343C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204E414D45202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E5441473C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5234313234383736373834363C2F69643E0D0A090909090909093C6E616D653E506C616E7441726561486173506C616E744F626A6563743C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5237373538353332353537363C2F69643E0D0A0909090909090909093C6E616D653E706172743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5236323135373439383031323C2F69643E0D0A0909090909090909093C6E616D653E617373656D626C79547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5233303934323337383439323C2F64617461547970653E0D0A0909090909090909093C76616C75653E504C414E5420415245412048415320504C414E54204F424A4543543C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5236323334383032303230313C2F69643E0D0A0909090909090909093C6E616D653E77686F6C653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234393635383331393833333C2F64617461547970653E0D0A0909090909090909093C76616C75653E504C414E5420415245413C2F76616C75653E0D0A0909090909090909093C636C6173734D61703E0D0A090909090909090909093C69643E72646C3A5234393635383331393833333C2F69643E0D0A090909090909090909093C6E616D653E504C414E5420415245413C2F6E616D653E0D0A090909090909090909093C6964656E746966696572733E0D0A09090909090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A090909090909090909093C2F6964656E746966696572733E0D0A090909090909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5234313234383736373834362830292F74706C3A5236323334383032303230313C2F706174683E0D0A0909090909090909093C2F636C6173734D61703E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5231313332373238313438353C2F69643E0D0A090909090909093C6E616D653E50416E644944526570726573656E746174696F6E3C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323731353031333837373C2F69643E0D0A0909090909090909093C6E616D653E6F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233353239303932393137353C2F69643E0D0A0909090909090909093C6E616D653E726570726573656E746174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393438363933313937353C2F64617461547970653E0D0A0909090909090909093C76616C75653E5020414E442049204449414752414D3C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5237343332383831353731303C2F69643E0D0A0909090909090909093C6E616D653E726570726573656E746174696F6E3C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393438363933313937353C2F64617461547970653E0D0A0909090909090909093C76616C75653E5020414E442049204449414752414D3C2F76616C75653E0D0A0909090909090909093C636C6173734D61703E0D0A090909090909090909093C69643E72646C3A5239393438363933313937353C2F69643E0D0A090909090909090909093C6E616D653E5020414E442049204449414752414D3C2F6E616D653E0D0A090909090909090909093C6964656E746966696572733E0D0A09090909090909090909093C6964656E7469666965723E4C494E45532E5049444E554D4245523C2F6964656E7469666965723E0D0A090909090909090909093C2F6964656E746966696572733E0D0A090909090909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5231313332373238313438352830292F74706C3A5237343332383831353731303C2F706174683E0D0A0909090909090909093C2F636C6173734D61703E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5237373434333335383831383C2F69643E0D0A090909090909093C6E616D653E4E6F6D696E616C4469616D657465723C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5231363331343132373832353C2F69643E0D0A0909090909090909093C6E616D653E686173506F73736573736F723C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5239333838343139333639323C2F69643E0D0A0909090909090909093C6E616D653E6861735363616C653C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E554F4D5F4E4F4D4449414D455445523C2F70726F70657274794E616D653E0D0A0909090909090909093C76616C75654C6973744E616D653E4C656E6774683C2F76616C75654C6973744E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5235393839363835333233393C2F69643E0D0A0909090909090909093C6E616D653E686173547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5231373632323134383034333C2F64617461547970653E0D0A0909090909090909093C76616C75653E4E4F4D494E414C204449414D455445523C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5239303638383036333634383C2F69643E0D0A0909090909090909093C6E616D653E76616C56616C75653C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A73696E676C653C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E4E4F4D4449414D455445523C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5234393635383331393833333C2F69643E0D0A0909090909093C6E616D653E504C414E5420415245413C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5234313234383736373834362830292F74706C3A5236323334383032303230313C2F706174683E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234303437313034313735343C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204E414D45202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E415245413C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5239393438363933313937353C2F69643E0D0A0909090909093C6E616D653E5020414E442049204449414752414D3C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E5049444E554D4245523C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5231313332373238313438352830292F74706C3A5237343332383831353731303C2F706174683E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5233303139333338363237333C2F69643E0D0A090909090909093C6E616D653E436C61737369666965644964656E74696669636174696F6E3C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5231363839333238333035303C2F64617461547970653E0D0A0909090909090909093C76616C75653E44524157494E47204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E5049444E554D4245523C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E4C494E45533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E4C313C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5231393139323436323535303C2F69643E0D0A0909090909093C6E616D653E504950494E47204E4554574F524B2053595354454D3C2F6E616D653E0D0A0909090909093C6964656E74696669657244656C696D697465723E5F3C2F6964656E74696669657244656C696D697465723E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E415245413C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E4C494E45533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E546573743C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5239383530353931383430343C2F69643E0D0A0909090909093C6E616D653E494E535452554D454E543C2F6E616D653E0D0A0909090909093C6964656E74696669657244656C696D697465723E5F3C2F6964656E74696669657244656C696D697465723E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E494E535452554D454E54532E5441473C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E494E535452554D454E54532E5441473C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E494E535452554D454E54532E4153534F435F45513C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A090909090909093C696E6465783E313C2F696E6465783E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E494E535452554D454E54533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A093C2F67726170684D6170733E0D0A093C76616C75654C6973744D6170733E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E4C656E6774683C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E6D6D3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5235323035343237353337343C2F7572693E0D0A09090909093C6C6162656C3E4D494C4C494D45545245203C2F6C6162656C3E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E54656D70657261747572653C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E646567433C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5237343837373939323730333C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E6465674B3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5235333130393636323430353C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E50726573737572653C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E50415343414C3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5239383738373735343236373C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E4241523C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5238333439303530363535313C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E6C313C2F6E616D653E0D0A0909093C76616C75654D6170732F3E0D0A09093C2F76616C75654C6973744D61703E0D0A093C2F76616C75654C6973744D6170733E0D0A3C2F6D617070696E673E, 1, 1, 1)
INSERT [dbo].[Graphs] ([ApplicationId], [GraphId], [GraphName], [Graph], [SiteId], [Active], [ExchangeVisible]) VALUES (N'3e1e418d-fa51-403b-bb7b-0cade34283a7', N'14fe717e-db88-406e-8a67-6e5cbb4ac731', N'TEST', 0x3C6D617070696E6720786D6C6E733D22687474703A2F2F7777772E6972696E67746F6F6C732E6F72672F6D617070696E672220786D6C6E733A693D22687474703A2F2F7777772E77332E6F72672F323030312F584D4C536368656D612D696E7374616E6365223E0D0A093C67726170684D6170733E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E4C696E65733C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5231393139323436323535303C2F69643E0D0A0909090909093C6E616D653E504950494E47204E4554574F524B2053595354454D3C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E5441473C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234303437313034313735343C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204E414D45202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E5441473C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5234313234383736373834363C2F69643E0D0A090909090909093C6E616D653E506C616E7441726561486173506C616E744F626A6563743C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5237373538353332353537363C2F69643E0D0A0909090909090909093C6E616D653E706172743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5236323135373439383031323C2F69643E0D0A0909090909090909093C6E616D653E617373656D626C79547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5233303934323337383439323C2F64617461547970653E0D0A0909090909090909093C76616C75653E504C414E5420415245412048415320504C414E54204F424A4543543C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5236323334383032303230313C2F69643E0D0A0909090909090909093C6E616D653E77686F6C653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234393635383331393833333C2F64617461547970653E0D0A0909090909090909093C76616C75653E504C414E5420415245413C2F76616C75653E0D0A0909090909090909093C636C6173734D61703E0D0A090909090909090909093C69643E72646C3A5234393635383331393833333C2F69643E0D0A090909090909090909093C6E616D653E504C414E5420415245413C2F6E616D653E0D0A090909090909090909093C6964656E746966696572733E0D0A09090909090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A090909090909090909093C2F6964656E746966696572733E0D0A090909090909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5234313234383736373834362830292F74706C3A5236323334383032303230313C2F706174683E0D0A0909090909090909093C2F636C6173734D61703E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5231313332373238313438353C2F69643E0D0A090909090909093C6E616D653E50416E644944526570726573656E746174696F6E3C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323731353031333837373C2F69643E0D0A0909090909090909093C6E616D653E6F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233353239303932393137353C2F69643E0D0A0909090909090909093C6E616D653E726570726573656E746174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393438363933313937353C2F64617461547970653E0D0A0909090909090909093C76616C75653E5020414E442049204449414752414D3C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5237343332383831353731303C2F69643E0D0A0909090909090909093C6E616D653E726570726573656E746174696F6E3C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393438363933313937353C2F64617461547970653E0D0A0909090909090909093C76616C75653E5020414E442049204449414752414D3C2F76616C75653E0D0A0909090909090909093C636C6173734D61703E0D0A090909090909090909093C69643E72646C3A5239393438363933313937353C2F69643E0D0A090909090909090909093C6E616D653E5020414E442049204449414752414D3C2F6E616D653E0D0A090909090909090909093C6964656E746966696572733E0D0A09090909090909090909093C6964656E7469666965723E4C494E45532E5049444E554D4245523C2F6964656E7469666965723E0D0A090909090909090909093C2F6964656E746966696572733E0D0A090909090909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5231313332373238313438352830292F74706C3A5237343332383831353731303C2F706174683E0D0A0909090909090909093C2F636C6173734D61703E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5237373434333335383831383C2F69643E0D0A090909090909093C6E616D653E4E6F6D696E616C4469616D657465723C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5231363331343132373832353C2F69643E0D0A0909090909090909093C6E616D653E686173506F73736573736F723C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5239333838343139333639323C2F69643E0D0A0909090909090909093C6E616D653E6861735363616C653C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E554F4D5F4E4F4D4449414D455445523C2F70726F70657274794E616D653E0D0A0909090909090909093C76616C75654C6973744E616D653E4C656E6774683C2F76616C75654C6973744E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5235393839363835333233393C2F69643E0D0A0909090909090909093C6E616D653E686173547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5231373632323134383034333C2F64617461547970653E0D0A0909090909090909093C76616C75653E4E4F4D494E414C204449414D455445523C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5239303638383036333634383C2F69643E0D0A0909090909090909093C6E616D653E76616C56616C75653C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A73696E676C653C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E4E4F4D4449414D455445523C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5234393635383331393833333C2F69643E0D0A0909090909093C6E616D653E504C414E5420415245413C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5234313234383736373834362830292F74706C3A5236323334383032303230313C2F706174683E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234303437313034313735343C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204E414D45202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E415245413C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5239393438363933313937353C2F69643E0D0A0909090909093C6E616D653E5020414E442049204449414752414D3C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E5049444E554D4245523C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5231313332373238313438352830292F74706C3A5237343332383831353731303C2F706174683E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5233303139333338363237333C2F69643E0D0A090909090909093C6E616D653E436C61737369666965644964656E74696669636174696F6E3C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5231363839333238333035303C2F64617461547970653E0D0A0909090909090909093C76616C75653E44524157494E47204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E5049444E554D4245523C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E4C494E45533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E4C313C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5231393139323436323535303C2F69643E0D0A0909090909093C6E616D653E504950494E47204E4554574F524B2053595354454D3C2F6E616D653E0D0A0909090909093C6964656E74696669657244656C696D697465723E5F3C2F6964656E74696669657244656C696D697465723E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E415245413C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E4C494E45533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E546573743C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5239383530353931383430343C2F69643E0D0A0909090909093C6E616D653E494E535452554D454E543C2F6E616D653E0D0A0909090909093C6964656E74696669657244656C696D697465723E5F3C2F6964656E74696669657244656C696D697465723E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E494E535452554D454E54532E5441473C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E494E535452554D454E54532E5441473C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E494E535452554D454E54532E4153534F435F45513C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A090909090909093C696E6465783E313C2F696E6465783E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E494E535452554D454E54533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A093C2F67726170684D6170733E0D0A093C76616C75654C6973744D6170733E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E4C656E6774683C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E6D6D3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5235323035343237353337343C2F7572693E0D0A09090909093C6C6162656C3E4D494C4C494D45545245203C2F6C6162656C3E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E54656D70657261747572653C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E646567433C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5237343837373939323730333C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E6465674B3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5235333130393636323430353C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E50726573737572653C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E50415343414C3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5239383738373735343236373C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E4241523C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5238333439303530363535313C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E6C313C2F6E616D653E0D0A0909093C76616C75654D6170732F3E0D0A09093C2F76616C75654C6973744D61703E0D0A093C2F76616C75654C6973744D6170733E0D0A3C2F6D617070696E673E, 1, 1, 1)
INSERT [dbo].[Graphs] ([ApplicationId], [GraphId], [GraphName], [Graph], [SiteId], [Active], [ExchangeVisible]) VALUES (N'3456c504-133d-41b9-8780-7bfd73840444', N'071bce1a-9396-43c9-98f3-73359a4b2e0d', N'TEST', 0x3C6D617070696E6720786D6C6E733D22687474703A2F2F7777772E6972696E67746F6F6C732E6F72672F6D617070696E672220786D6C6E733A693D22687474703A2F2F7777772E77332E6F72672F323030312F584D4C536368656D612D696E7374616E6365223E0D0A093C67726170684D6170733E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E4C696E65733C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5231393139323436323535303C2F69643E0D0A0909090909093C6E616D653E504950494E47204E4554574F524B2053595354454D3C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E5441473C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234303437313034313735343C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204E414D45202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E5441473C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5234313234383736373834363C2F69643E0D0A090909090909093C6E616D653E506C616E7441726561486173506C616E744F626A6563743C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5237373538353332353537363C2F69643E0D0A0909090909090909093C6E616D653E706172743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5236323135373439383031323C2F69643E0D0A0909090909090909093C6E616D653E617373656D626C79547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5233303934323337383439323C2F64617461547970653E0D0A0909090909090909093C76616C75653E504C414E5420415245412048415320504C414E54204F424A4543543C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5236323334383032303230313C2F69643E0D0A0909090909090909093C6E616D653E77686F6C653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234393635383331393833333C2F64617461547970653E0D0A0909090909090909093C76616C75653E504C414E5420415245413C2F76616C75653E0D0A0909090909090909093C636C6173734D61703E0D0A090909090909090909093C69643E72646C3A5234393635383331393833333C2F69643E0D0A090909090909090909093C6E616D653E504C414E5420415245413C2F6E616D653E0D0A090909090909090909093C6964656E746966696572733E0D0A09090909090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A090909090909090909093C2F6964656E746966696572733E0D0A090909090909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5234313234383736373834362830292F74706C3A5236323334383032303230313C2F706174683E0D0A0909090909090909093C2F636C6173734D61703E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5231313332373238313438353C2F69643E0D0A090909090909093C6E616D653E50416E644944526570726573656E746174696F6E3C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323731353031333837373C2F69643E0D0A0909090909090909093C6E616D653E6F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233353239303932393137353C2F69643E0D0A0909090909090909093C6E616D653E726570726573656E746174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393438363933313937353C2F64617461547970653E0D0A0909090909090909093C76616C75653E5020414E442049204449414752414D3C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5237343332383831353731303C2F69643E0D0A0909090909090909093C6E616D653E726570726573656E746174696F6E3C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393438363933313937353C2F64617461547970653E0D0A0909090909090909093C76616C75653E5020414E442049204449414752414D3C2F76616C75653E0D0A0909090909090909093C636C6173734D61703E0D0A090909090909090909093C69643E72646C3A5239393438363933313937353C2F69643E0D0A090909090909090909093C6E616D653E5020414E442049204449414752414D3C2F6E616D653E0D0A090909090909090909093C6964656E746966696572733E0D0A09090909090909090909093C6964656E7469666965723E4C494E45532E5049444E554D4245523C2F6964656E7469666965723E0D0A090909090909090909093C2F6964656E746966696572733E0D0A090909090909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5231313332373238313438352830292F74706C3A5237343332383831353731303C2F706174683E0D0A0909090909090909093C2F636C6173734D61703E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5237373434333335383831383C2F69643E0D0A090909090909093C6E616D653E4E6F6D696E616C4469616D657465723C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5231363331343132373832353C2F69643E0D0A0909090909090909093C6E616D653E686173506F73736573736F723C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5239333838343139333639323C2F69643E0D0A0909090909090909093C6E616D653E6861735363616C653C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E554F4D5F4E4F4D4449414D455445523C2F70726F70657274794E616D653E0D0A0909090909090909093C76616C75654C6973744E616D653E4C656E6774683C2F76616C75654C6973744E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5235393839363835333233393C2F69643E0D0A0909090909090909093C6E616D653E686173547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5231373632323134383034333C2F64617461547970653E0D0A0909090909090909093C76616C75653E4E4F4D494E414C204449414D455445523C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5239303638383036333634383C2F69643E0D0A0909090909090909093C6E616D653E76616C56616C75653C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A73696E676C653C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E4E4F4D4449414D455445523C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5234393635383331393833333C2F69643E0D0A0909090909093C6E616D653E504C414E5420415245413C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5234313234383736373834362830292F74706C3A5236323334383032303230313C2F706174683E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234303437313034313735343C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204E414D45202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E415245413C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5239393438363933313937353C2F69643E0D0A0909090909093C6E616D653E5020414E442049204449414752414D3C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E5049444E554D4245523C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5231313332373238313438352830292F74706C3A5237343332383831353731303C2F706174683E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5233303139333338363237333C2F69643E0D0A090909090909093C6E616D653E436C61737369666965644964656E74696669636174696F6E3C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5231363839333238333035303C2F64617461547970653E0D0A0909090909090909093C76616C75653E44524157494E47204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E5049444E554D4245523C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E4C494E45533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E4C313C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5231393139323436323535303C2F69643E0D0A0909090909093C6E616D653E504950494E47204E4554574F524B2053595354454D3C2F6E616D653E0D0A0909090909093C6964656E74696669657244656C696D697465723E5F3C2F6964656E74696669657244656C696D697465723E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E415245413C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E4C494E45533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E546573743C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5239383530353931383430343C2F69643E0D0A0909090909093C6E616D653E494E535452554D454E543C2F6E616D653E0D0A0909090909093C6964656E74696669657244656C696D697465723E5F3C2F6964656E74696669657244656C696D697465723E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E494E535452554D454E54532E5441473C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E494E535452554D454E54532E5441473C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E494E535452554D454E54532E4153534F435F45513C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A090909090909093C696E6465783E313C2F696E6465783E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E494E535452554D454E54533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A093C2F67726170684D6170733E0D0A093C76616C75654C6973744D6170733E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E4C656E6774683C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E6D6D3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5235323035343237353337343C2F7572693E0D0A09090909093C6C6162656C3E4D494C4C494D45545245203C2F6C6162656C3E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E54656D70657261747572653C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E646567433C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5237343837373939323730333C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E6465674B3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5235333130393636323430353C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E50726573737572653C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E50415343414C3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5239383738373735343236373C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E4241523C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5238333439303530363535313C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E6C313C2F6E616D653E0D0A0909093C76616C75654D6170732F3E0D0A09093C2F76616C75654C6973744D61703E0D0A093C2F76616C75654C6973744D6170733E0D0A3C2F6D617070696E673E, 1, 1, 1)
INSERT [dbo].[Graphs] ([ApplicationId], [GraphId], [GraphName], [Graph], [SiteId], [Active], [ExchangeVisible]) VALUES (N'7877c504-133d-41b9-8680-7bfd73840435', N'd208b9b8-0372-4e2c-9b34-78b9bb453b61', N'Lines', 0x3C6D617070696E6720786D6C6E733D22687474703A2F2F7777772E6972696E67746F6F6C732E6F72672F6D617070696E672220786D6C6E733A693D22687474703A2F2F7777772E77332E6F72672F323030312F584D4C536368656D612D696E7374616E6365223E0D0A093C67726170684D6170733E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E4C696E65733C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5231393139323436323535303C2F69643E0D0A0909090909093C6E616D653E504950494E47204E4554574F524B2053595354454D3C2F6E616D653E0D0A0909090909093C6964656E74696669657244656C696D697465722F3E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E5441473C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234303437313034313735343C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204E414D45202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E5441473C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5234313234383736373834363C2F69643E0D0A090909090909093C6E616D653E506C616E7441726561486173506C616E744F626A6563743C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5237373538353332353537363C2F69643E0D0A0909090909090909093C6E616D653E706172743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5236323135373439383031323C2F69643E0D0A0909090909090909093C6E616D653E617373656D626C79547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5233303934323337383439323C2F64617461547970653E0D0A0909090909090909093C76616C75653E504C414E5420415245412048415320504C414E54204F424A4543543C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5236323334383032303230313C2F69643E0D0A0909090909090909093C6E616D653E77686F6C653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234393635383331393833333C2F64617461547970653E0D0A0909090909090909093C76616C75653E504C414E5420415245413C2F76616C75653E0D0A0909090909090909093C636C6173734D61703E0D0A090909090909090909093C69643E72646C3A5234393635383331393833333C2F69643E0D0A090909090909090909093C6E616D653E504C414E5420415245413C2F6E616D653E0D0A090909090909090909093C6964656E746966696572733E0D0A09090909090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A090909090909090909093C2F6964656E746966696572733E0D0A090909090909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5234313234383736373834362830292F74706C3A5236323334383032303230313C2F706174683E0D0A0909090909090909093C2F636C6173734D61703E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5231313332373238313438353C2F69643E0D0A090909090909093C6E616D653E50416E644944526570726573656E746174696F6E3C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323731353031333837373C2F69643E0D0A0909090909090909093C6E616D653E6F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233353239303932393137353C2F69643E0D0A0909090909090909093C6E616D653E726570726573656E746174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393438363933313937353C2F64617461547970653E0D0A0909090909090909093C76616C75653E5020414E442049204449414752414D3C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5237343332383831353731303C2F69643E0D0A0909090909090909093C6E616D653E726570726573656E746174696F6E3C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393438363933313937353C2F64617461547970653E0D0A0909090909090909093C76616C75653E5020414E442049204449414752414D3C2F76616C75653E0D0A0909090909090909093C636C6173734D61703E0D0A090909090909090909093C69643E72646C3A5239393438363933313937353C2F69643E0D0A090909090909090909093C6E616D653E5020414E442049204449414752414D3C2F6E616D653E0D0A090909090909090909093C6964656E746966696572733E0D0A09090909090909090909093C6964656E7469666965723E4C494E45532E5049444E554D4245523C2F6964656E7469666965723E0D0A090909090909090909093C2F6964656E746966696572733E0D0A090909090909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5231313332373238313438352830292F74706C3A5237343332383831353731303C2F706174683E0D0A0909090909090909093C2F636C6173734D61703E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5237373434333335383831383C2F69643E0D0A090909090909093C6E616D653E4E6F6D696E616C4469616D657465723C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5231363331343132373832353C2F69643E0D0A0909090909090909093C6E616D653E686173506F73736573736F723C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5239333838343139333639323C2F69643E0D0A0909090909090909093C6E616D653E6861735363616C653C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E554F4D5F4E4F4D4449414D455445523C2F70726F70657274794E616D653E0D0A0909090909090909093C76616C75654C6973744E616D653E4C656E6774683C2F76616C75654C6973744E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5235393839363835333233393C2F69643E0D0A0909090909090909093C6E616D653E686173547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5231373632323134383034333C2F64617461547970653E0D0A0909090909090909093C76616C75653E4E4F4D494E414C204449414D455445523C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5239303638383036333634383C2F69643E0D0A0909090909090909093C6E616D653E76616C56616C75653C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A73696E676C653C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E4E4F4D4449414D455445523C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5241443030324630363645393634393938424344444441424137353137324635303C2F69643E0D0A090909090909093C6E616D653E41677265656D656E744964656E74696669636174696F6E436F64653C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5237394332433130323432463134383831383843383839333242304539323341463C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239443845374345453332374634334644384142433644433343464642323333303C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5235373543433343303734353734374237413331453842343844453739373445333C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5237313246363746334632393834434245423836363633453939344532363845443C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E44455350524553535552453C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5234393635383331393833333C2F69643E0D0A0909090909093C6E616D653E504C414E5420415245413C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5234313234383736373834362830292F74706C3A5236323334383032303230313C2F706174683E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234303437313034313735343C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204E414D45202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E415245413C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5239393438363933313937353C2F69643E0D0A0909090909093C6E616D653E5020414E442049204449414752414D3C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E5049444E554D4245523C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5231313332373238313438352830292F74706C3A5237343332383831353731303C2F706174683E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5233303139333338363237333C2F69643E0D0A090909090909093C6E616D653E436C61737369666965644964656E74696669636174696F6E3C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5231363839333238333035303C2F64617461547970653E0D0A0909090909090909093C76616C75653E44524157494E47204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E5049444E554D4245523C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E4C494E45533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A090D0A093C2F67726170684D6170733E0D0A093C76616C75654C6973744D6170733E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E4C656E6774683C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E6D6D3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5235323035343237353337343C2F7572693E0D0A09090909093C6C6162656C3E4D494C4C494D455452452020203C2F6C6162656C3E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E54656D70657261747572653C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E646567433C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5237343837373939323730333C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E6465674B3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5235333130393636323430353C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E50726573737572653C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E50415343414C3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5239383738373735343236373C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E4241523C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5238333439303530363535313C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A093C2F76616C75654C6973744D6170733E0D0A3C2F6D617070696E673E, 1, 1, 1)
INSERT [dbo].[Graphs] ([ApplicationId], [GraphId], [GraphName], [Graph], [SiteId], [Active], [ExchangeVisible]) VALUES (N'54d581ef-64bc-45c2-9df5-0255f7454e0e', N'47f7418a-e970-426d-8522-89c76613a8e8', N'G3u', 0x3C6D617070696E6720786D6C6E733D22687474703A2F2F7777772E6972696E67746F6F6C732E6F72672F6D617070696E672220786D6C6E733A693D22687474703A2F2F7777772E77332E6F72672F323030312F584D4C536368656D612D696E7374616E6365223E0D0A20203C67726170684D6170733E0D0A202020203C67726170684D61703E0D0A2020202020203C6E616D653E47313C2F6E616D653E0D0A2020202020203C636C61737354656D706C6174654D6170733E0D0A20202020202020203C636C61737354656D706C6174654D61703E0D0A202020202020202020203C636C6173734D61703E0D0A2020202020202020202020203C69643E72646C3A5238393436433841343943384134374445413343323030453239323337343632353C2F69643E0D0A2020202020202020202020203C6E616D653E414354494F4E20524551554952454420425920434C49454E543C2F6E616D653E0D0A2020202020202020202020203C6964656E74696669657244656C696D697465723E5F3C2F6964656E74696669657244656C696D697465723E0D0A2020202020202020202020203C6964656E746966696572733E0D0A20202020202020202020202020203C6964656E7469666965723E45515549504D454E542E49443C2F6964656E7469666965723E0D0A2020202020202020202020203C2F6964656E746966696572733E0D0A2020202020202020202020203C706174682F3E0D0A202020202020202020203C2F636C6173734D61703E0D0A202020202020202020203C74656D706C6174654D6170733E0D0A2020202020202020202020203C74656D706C6174654D61703E0D0A20202020202020202020202020203C69643E74706C3A5246413033333235464642354334434230413831413239333044454237433730443C2F69643E0D0A20202020202020202020202020203C6E616D653E41677265656D656E74526571756972656D656E744E756D6265723C2F6E616D653E0D0A20202020202020202020202020203C726F6C654D6170733E0D0A202020202020202020202020202020203C726F6C654D61703E0D0A2020202020202020202020202020202020203C747970653E5265666572656E63653C2F747970653E0D0A2020202020202020202020202020202020203C69643E74706C3A5237413137353130384542413434323732413034353538343137353539313643313C2F69643E0D0A2020202020202020202020202020202020203C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A2020202020202020202020202020202020203C64617461547970653E72646C3A5238363043343144363234343234443246424433313632313143433744323944443C2F64617461547970653E0D0A2020202020202020202020202020202020203C76616C75653E41475245454D454E54205245515549524D454E54204E554D4245523C2F76616C75653E0D0A202020202020202020202020202020203C2F726F6C654D61703E0D0A202020202020202020202020202020203C726F6C654D61703E0D0A2020202020202020202020202020202020203C747970653E5265666572656E63653C2F747970653E0D0A2020202020202020202020202020202020203C69643E74706C3A5242393146444135323333464534334645394441304333333237443142383937383C2F69643E0D0A2020202020202020202020202020202020203C6E616D653E6861734F626A6563743C2F6E616D653E0D0A2020202020202020202020202020202020203C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A2020202020202020202020202020202020203C76616C75653E41475245454D454E54205245515549524D454E54204E554D4245523C2F76616C75653E0D0A202020202020202020202020202020203C2F726F6C654D61703E0D0A202020202020202020202020202020203C726F6C654D61703E0D0A2020202020202020202020202020202020203C747970653E5265666572656E63653C2F747970653E0D0A2020202020202020202020202020202020203C69643E74706C3A5234463442434541383733453534363332423446463835353831394643364245383C2F69643E0D0A2020202020202020202020202020202020203C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A2020202020202020202020202020202020203C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A2020202020202020202020202020202020203C76616C75653E4143544956452046495245204649474854494E472045515549504D454E5420434C4153533C2F76616C75653E0D0A202020202020202020202020202020203C2F726F6C654D61703E0D0A20202020202020202020202020203C2F726F6C654D6170733E0D0A2020202020202020202020203C2F74656D706C6174654D61703E0D0A202020202020202020203C2F74656D706C6174654D6170733E0D0A20202020202020203C2F636C61737354656D706C6174654D61703E0D0A2020202020203C2F636C61737354656D706C6174654D6170733E0D0A2020202020203C646174614F626A6563744E616D653E45515549504D454E543C2F646174614F626A6563744E616D653E0D0A202020203C2F67726170684D61703E0D0A202020203C67726170684D61703E0D0A2020202020203C6E616D653E47323C2F6E616D653E0D0A2020202020203C636C61737354656D706C6174654D6170733E0D0A20202020202020203C636C61737354656D706C6174654D61703E0D0A202020202020202020203C636C6173734D61703E0D0A2020202020202020202020203C69643E72646C3A5238343030383632303730353C2F69643E0D0A2020202020202020202020203C6E616D653E414C49474E4D454E5420494E535452554D454E543C2F6E616D653E0D0A2020202020202020202020203C6964656E74696669657244656C696D697465723E5F3C2F6964656E74696669657244656C696D697465723E0D0A2020202020202020202020203C6964656E746966696572733E0D0A20202020202020202020202020203C6964656E7469666965723E494E535452554D454E54532E494E554D3C2F6964656E7469666965723E0D0A2020202020202020202020203C2F6964656E746966696572733E0D0A2020202020202020202020203C706174682F3E0D0A202020202020202020203C2F636C6173734D61703E0D0A202020202020202020203C74656D706C6174654D6170733E0D0A2020202020202020202020203C74656D706C6174654D61703E0D0A20202020202020202020202020203C69643E74706C3A5234363637333537323130393C2F69643E0D0A20202020202020202020202020203C6E616D653E44617465496E737472756D656E74496E7374616C6C65643C2F6E616D653E0D0A20202020202020202020202020203C726F6C654D6170733E0D0A202020202020202020202020202020203C726F6C654D61703E0D0A2020202020202020202020202020202020203C747970653E5265666572656E63653C2F747970653E0D0A2020202020202020202020202020202020203C69643E74706C3A5233343939414241463130393034353042413737354544364246323435453341413C2F69643E0D0A2020202020202020202020202020202020203C6E616D653E6861734576656E74547970653C2F6E616D653E0D0A2020202020202020202020202020202020203C64617461547970653E72646C3A5236343533363631383639323C2F64617461547970653E0D0A2020202020202020202020202020202020203C76616C75653E444946464552454E5449414C204D4541535552494E4720494E535452554D454E543C2F76616C75653E0D0A202020202020202020202020202020203C2F726F6C654D61703E0D0A202020202020202020202020202020203C726F6C654D61703E0D0A2020202020202020202020202020202020203C747970653E5265666572656E63653C2F747970653E0D0A2020202020202020202020202020202020203C69643E74706C3A5239314146413031433041443034354532383234363143343732414634393341313C2F69643E0D0A2020202020202020202020202020202020203C6E616D653E6861734F626A6563743C2F6E616D653E0D0A2020202020202020202020202020202020203C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A2020202020202020202020202020202020203C76616C75653E444554454354494E4720494E535452554D454E543C2F76616C75653E0D0A202020202020202020202020202020203C2F726F6C654D61703E0D0A202020202020202020202020202020203C726F6C654D61703E0D0A2020202020202020202020202020202020203C747970653E5265666572656E63653C2F747970653E0D0A2020202020202020202020202020202020203C69643E74706C3A5243443130424441433336313634303141413037443738324141323338414141463C2F69643E0D0A2020202020202020202020202020202020203C6E616D653E76616C56616C75653C2F6E616D653E0D0A2020202020202020202020202020202020203C64617461547970653E7873643A6461746554696D653C2F64617461547970653E0D0A2020202020202020202020202020202020203C76616C75653E4445494E5354414C4C494E473C2F76616C75653E0D0A202020202020202020202020202020203C2F726F6C654D61703E0D0A20202020202020202020202020203C2F726F6C654D6170733E0D0A2020202020202020202020203C2F74656D706C6174654D61703E0D0A202020202020202020203C2F74656D706C6174654D6170733E0D0A20202020202020203C2F636C61737354656D706C6174654D61703E0D0A2020202020203C2F636C61737354656D706C6174654D6170733E0D0A2020202020203C646174614F626A6563744E616D653E494E535452554D454E54533C2F646174614F626A6563744E616D653E0D0A202020203C2F67726170684D61703E0D0A20203C2F67726170684D6170733E0D0A20203C76616C75654C6973744D6170733E0D0A202020203C76616C75654C6973744D61703E0D0A2020202020203C6E616D653E56313C2F6E616D653E0D0A2020202020203C76616C75654D6170733E0D0A20202020202020203C76616C75654D61703E0D0A202020202020202020203C696E7465726E616C56616C75653E564D313C2F696E7465726E616C56616C75653E0D0A202020202020202020203C7572693E72646C3A5237323837373032353935323C2F7572693E0D0A202020202020202020203C6C6162656C3E444E56204649584544204F464653484F524520494E5354414C4C4154494F4E53202D204E5620443436303C2F6C6162656C3E0D0A20202020202020203C2F76616C75654D61703E0D0A20202020202020203C76616C75654D61703E0D0A202020202020202020203C696E7465726E616C56616C75653E56323C2F696E7465726E616C56616C75653E0D0A202020202020202020203C7572693E72646C3A5235373639333735383530393C2F7572693E0D0A202020202020202020203C6C6162656C3E444E56204649584544204F464653484F524520494E5354414C4C4154494F4E53202D204E5620443530303C2F6C6162656C3E0D0A20202020202020203C2F76616C75654D61703E0D0A2020202020203C2F76616C75654D6170733E0D0A202020203C2F76616C75654C6973744D61703E0D0A202020203C76616C75654C6973744D61703E0D0A2020202020203C6E616D653E56323C2F6E616D653E0D0A2020202020203C76616C75654D6170733E0D0A20202020202020203C76616C75654D61703E0D0A202020202020202020203C696E7465726E616C56616C75653E56333C2F696E7465726E616C56616C75653E0D0A202020202020202020203C7572693E72646C3A5237343230333335343336363C2F7572693E0D0A202020202020202020203C6C6162656C3E444E56204649584544204F464653484F524520494E5354414C4C4154494F4E53202D204E56204533363C2F6C6162656C3E0D0A20202020202020203C2F76616C75654D61703E0D0A20202020202020203C76616C75654D61703E0D0A202020202020202020203C696E7465726E616C56616C75653E56343C2F696E7465726E616C56616C75653E0D0A202020202020202020203C7572693E72646C3A5238393134353334313837383C2F7572693E0D0A202020202020202020203C6C6162656C3E444E56204649584544204F464653484F524520494E5354414C4C4154494F4E53202D204E56204534303C2F6C6162656C3E0D0A20202020202020203C2F76616C75654D61703E0D0A2020202020203C2F76616C75654D6170733E0D0A202020203C2F76616C75654C6973744D61703E0D0A20203C2F76616C75654C6973744D6170733E0D0A3C2F6D617070696E673E, 1, 0, 1)
INSERT [dbo].[Graphs] ([ApplicationId], [GraphId], [GraphName], [Graph], [SiteId], [Active], [ExchangeVisible]) VALUES (N'3456c504-133d-41b9-8780-7bfd73840444', N'ee904602-ba52-422c-a01f-932490798853', N'Lines', 0x3C6D617070696E6720786D6C6E733D22687474703A2F2F7777772E6972696E67746F6F6C732E6F72672F6D617070696E672220786D6C6E733A693D22687474703A2F2F7777772E77332E6F72672F323030312F584D4C536368656D612D696E7374616E6365223E0D0A093C67726170684D6170733E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E4C696E65733C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5231393139323436323535303C2F69643E0D0A0909090909093C6E616D653E504950494E47204E4554574F524B2053595354454D3C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E5441473C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234303437313034313735343C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204E414D45202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E5441473C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5234313234383736373834363C2F69643E0D0A090909090909093C6E616D653E506C616E7441726561486173506C616E744F626A6563743C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5237373538353332353537363C2F69643E0D0A0909090909090909093C6E616D653E706172743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5236323135373439383031323C2F69643E0D0A0909090909090909093C6E616D653E617373656D626C79547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5233303934323337383439323C2F64617461547970653E0D0A0909090909090909093C76616C75653E504C414E5420415245412048415320504C414E54204F424A4543543C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5236323334383032303230313C2F69643E0D0A0909090909090909093C6E616D653E77686F6C653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234393635383331393833333C2F64617461547970653E0D0A0909090909090909093C76616C75653E504C414E5420415245413C2F76616C75653E0D0A0909090909090909093C636C6173734D61703E0D0A090909090909090909093C69643E72646C3A5234393635383331393833333C2F69643E0D0A090909090909090909093C6E616D653E504C414E5420415245413C2F6E616D653E0D0A090909090909090909093C6964656E746966696572733E0D0A09090909090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A090909090909090909093C2F6964656E746966696572733E0D0A090909090909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5234313234383736373834362830292F74706C3A5236323334383032303230313C2F706174683E0D0A0909090909090909093C2F636C6173734D61703E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5231313332373238313438353C2F69643E0D0A090909090909093C6E616D653E50416E644944526570726573656E746174696F6E3C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323731353031333837373C2F69643E0D0A0909090909090909093C6E616D653E6F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233353239303932393137353C2F69643E0D0A0909090909090909093C6E616D653E726570726573656E746174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393438363933313937353C2F64617461547970653E0D0A0909090909090909093C76616C75653E5020414E442049204449414752414D3C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5237343332383831353731303C2F69643E0D0A0909090909090909093C6E616D653E726570726573656E746174696F6E3C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393438363933313937353C2F64617461547970653E0D0A0909090909090909093C76616C75653E5020414E442049204449414752414D3C2F76616C75653E0D0A0909090909090909093C636C6173734D61703E0D0A090909090909090909093C69643E72646C3A5239393438363933313937353C2F69643E0D0A090909090909090909093C6E616D653E5020414E442049204449414752414D3C2F6E616D653E0D0A090909090909090909093C6964656E746966696572733E0D0A09090909090909090909093C6964656E7469666965723E4C494E45532E5049444E554D4245523C2F6964656E7469666965723E0D0A090909090909090909093C2F6964656E746966696572733E0D0A090909090909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5231313332373238313438352830292F74706C3A5237343332383831353731303C2F706174683E0D0A0909090909090909093C2F636C6173734D61703E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5237373434333335383831383C2F69643E0D0A090909090909093C6E616D653E4E6F6D696E616C4469616D657465723C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5231363331343132373832353C2F69643E0D0A0909090909090909093C6E616D653E686173506F73736573736F723C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5239333838343139333639323C2F69643E0D0A0909090909090909093C6E616D653E6861735363616C653C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E554F4D5F4E4F4D4449414D455445523C2F70726F70657274794E616D653E0D0A0909090909090909093C76616C75654C6973744E616D653E4C656E6774683C2F76616C75654C6973744E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5235393839363835333233393C2F69643E0D0A0909090909090909093C6E616D653E686173547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5231373632323134383034333C2F64617461547970653E0D0A0909090909090909093C76616C75653E4E4F4D494E414C204449414D455445523C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5239303638383036333634383C2F69643E0D0A0909090909090909093C6E616D653E76616C56616C75653C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A73696E676C653C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E4E4F4D4449414D455445523C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5234393635383331393833333C2F69643E0D0A0909090909093C6E616D653E504C414E5420415245413C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5234313234383736373834362830292F74706C3A5236323334383032303230313C2F706174683E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234303437313034313735343C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204E414D45202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E415245413C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5239393438363933313937353C2F69643E0D0A0909090909093C6E616D653E5020414E442049204449414752414D3C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E5049444E554D4245523C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5231313332373238313438352830292F74706C3A5237343332383831353731303C2F706174683E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5233303139333338363237333C2F69643E0D0A090909090909093C6E616D653E436C61737369666965644964656E74696669636174696F6E3C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5231363839333238333035303C2F64617461547970653E0D0A0909090909090909093C76616C75653E44524157494E47204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E5049444E554D4245523C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E4C494E45533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E4C313C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5231393139323436323535303C2F69643E0D0A0909090909093C6E616D653E504950494E47204E4554574F524B2053595354454D3C2F6E616D653E0D0A0909090909093C6964656E74696669657244656C696D697465723E5F3C2F6964656E74696669657244656C696D697465723E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E415245413C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E4C494E45533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E546573743C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5239383530353931383430343C2F69643E0D0A0909090909093C6E616D653E494E535452554D454E543C2F6E616D653E0D0A0909090909093C6964656E74696669657244656C696D697465723E5F3C2F6964656E74696669657244656C696D697465723E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E494E535452554D454E54532E5441473C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E494E535452554D454E54532E5441473C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E494E535452554D454E54532E4153534F435F45513C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A090909090909093C696E6465783E313C2F696E6465783E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E494E535452554D454E54533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A093C2F67726170684D6170733E0D0A093C76616C75654C6973744D6170733E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E4C656E6774683C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E6D6D3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5235323035343237353337343C2F7572693E0D0A09090909093C6C6162656C3E4D494C4C494D45545245203C2F6C6162656C3E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E54656D70657261747572653C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E646567433C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5237343837373939323730333C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E6465674B3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5235333130393636323430353C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E50726573737572653C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E50415343414C3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5239383738373735343236373C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E4241523C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5238333439303530363535313C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E6C313C2F6E616D653E0D0A0909093C76616C75654D6170732F3E0D0A09093C2F76616C75654C6973744D61703E0D0A093C2F76616C75654C6973744D6170733E0D0A3C2F6D617070696E673E, 1, 1, 1)
INSERT [dbo].[Graphs] ([ApplicationId], [GraphId], [GraphName], [Graph], [SiteId], [Active], [ExchangeVisible]) VALUES (N'3456c504-133d-41b9-8780-7bfd73840444', N'a6058594-ac45-4f12-9830-b34c4ff14bb7', N'TEST', 0x3C6D617070696E6720786D6C6E733D22687474703A2F2F7777772E6972696E67746F6F6C732E6F72672F6D617070696E672220786D6C6E733A693D22687474703A2F2F7777772E77332E6F72672F323030312F584D4C536368656D612D696E7374616E6365223E0D0A093C67726170684D6170733E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E4C696E65733C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5231393139323436323535303C2F69643E0D0A0909090909093C6E616D653E504950494E47204E4554574F524B2053595354454D3C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E5441473C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234303437313034313735343C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204E414D45202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E5441473C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5234313234383736373834363C2F69643E0D0A090909090909093C6E616D653E506C616E7441726561486173506C616E744F626A6563743C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5237373538353332353537363C2F69643E0D0A0909090909090909093C6E616D653E706172743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5236323135373439383031323C2F69643E0D0A0909090909090909093C6E616D653E617373656D626C79547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5233303934323337383439323C2F64617461547970653E0D0A0909090909090909093C76616C75653E504C414E5420415245412048415320504C414E54204F424A4543543C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5236323334383032303230313C2F69643E0D0A0909090909090909093C6E616D653E77686F6C653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234393635383331393833333C2F64617461547970653E0D0A0909090909090909093C76616C75653E504C414E5420415245413C2F76616C75653E0D0A0909090909090909093C636C6173734D61703E0D0A090909090909090909093C69643E72646C3A5234393635383331393833333C2F69643E0D0A090909090909090909093C6E616D653E504C414E5420415245413C2F6E616D653E0D0A090909090909090909093C6964656E746966696572733E0D0A09090909090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A090909090909090909093C2F6964656E746966696572733E0D0A090909090909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5234313234383736373834362830292F74706C3A5236323334383032303230313C2F706174683E0D0A0909090909090909093C2F636C6173734D61703E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5231313332373238313438353C2F69643E0D0A090909090909093C6E616D653E50416E644944526570726573656E746174696F6E3C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323731353031333837373C2F69643E0D0A0909090909090909093C6E616D653E6F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233353239303932393137353C2F69643E0D0A0909090909090909093C6E616D653E726570726573656E746174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393438363933313937353C2F64617461547970653E0D0A0909090909090909093C76616C75653E5020414E442049204449414752414D3C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5237343332383831353731303C2F69643E0D0A0909090909090909093C6E616D653E726570726573656E746174696F6E3C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393438363933313937353C2F64617461547970653E0D0A0909090909090909093C76616C75653E5020414E442049204449414752414D3C2F76616C75653E0D0A0909090909090909093C636C6173734D61703E0D0A090909090909090909093C69643E72646C3A5239393438363933313937353C2F69643E0D0A090909090909090909093C6E616D653E5020414E442049204449414752414D3C2F6E616D653E0D0A090909090909090909093C6964656E746966696572733E0D0A09090909090909090909093C6964656E7469666965723E4C494E45532E5049444E554D4245523C2F6964656E7469666965723E0D0A090909090909090909093C2F6964656E746966696572733E0D0A090909090909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5231313332373238313438352830292F74706C3A5237343332383831353731303C2F706174683E0D0A0909090909090909093C2F636C6173734D61703E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5237373434333335383831383C2F69643E0D0A090909090909093C6E616D653E4E6F6D696E616C4469616D657465723C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5231363331343132373832353C2F69643E0D0A0909090909090909093C6E616D653E686173506F73736573736F723C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5239333838343139333639323C2F69643E0D0A0909090909090909093C6E616D653E6861735363616C653C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E554F4D5F4E4F4D4449414D455445523C2F70726F70657274794E616D653E0D0A0909090909090909093C76616C75654C6973744E616D653E4C656E6774683C2F76616C75654C6973744E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5235393839363835333233393C2F69643E0D0A0909090909090909093C6E616D653E686173547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5231373632323134383034333C2F64617461547970653E0D0A0909090909090909093C76616C75653E4E4F4D494E414C204449414D455445523C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5239303638383036333634383C2F69643E0D0A0909090909090909093C6E616D653E76616C56616C75653C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A73696E676C653C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E4E4F4D4449414D455445523C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5234393635383331393833333C2F69643E0D0A0909090909093C6E616D653E504C414E5420415245413C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5234313234383736373834362830292F74706C3A5236323334383032303230313C2F706174683E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5234303437313034313735343C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204E414D45202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E415245413C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5239393438363933313937353C2F69643E0D0A0909090909093C6E616D653E5020414E442049204449414752414D3C2F6E616D653E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E5049444E554D4245523C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174683E72646C3A5231393139323436323535302F74706C3A5231313332373238313438352830292F74706C3A5237343332383831353731303C2F706174683E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5233303139333338363237333C2F69643E0D0A090909090909093C6E616D653E436C61737369666965644964656E74696669636174696F6E3C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5234343533373530343037303C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233303739303130383031363C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5231363839333238333035303C2F64617461547970653E0D0A0909090909090909093C76616C75653E44524157494E47204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E50726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5232323637343734393638383C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E5049444E554D4245523C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E4C494E45533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E4C313C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5231393139323436323535303C2F69643E0D0A0909090909093C6E616D653E504950494E47204E4554574F524B2053595354454D3C2F6E616D653E0D0A0909090909093C6964656E74696669657244656C696D697465723E5F3C2F6964656E74696669657244656C696D697465723E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E4C494E45532E415245413C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E4C494E45532E415245413C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E4C494E45533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A09093C67726170684D61703E0D0A0909093C6E616D653E546573743C2F6E616D653E0D0A0909093C636C61737354656D706C6174654D6170733E0D0A090909093C636C61737354656D706C6174654D61703E0D0A09090909093C636C6173734D61703E0D0A0909090909093C69643E72646C3A5239383530353931383430343C2F69643E0D0A0909090909093C6E616D653E494E535452554D454E543C2F6E616D653E0D0A0909090909093C6964656E74696669657244656C696D697465723E5F3C2F6964656E74696669657244656C696D697465723E0D0A0909090909093C6964656E746966696572733E0D0A090909090909093C6964656E7469666965723E494E535452554D454E54532E5441473C2F6964656E7469666965723E0D0A0909090909093C2F6964656E746966696572733E0D0A0909090909093C706174682F3E0D0A09090909093C2F636C6173734D61703E0D0A09090909093C74656D706C6174654D6170733E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E494E535452554D454E54532E5441473C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A0909090909093C2F74656D706C6174654D61703E0D0A0909090909093C74656D706C6174654D61703E0D0A090909090909093C69643E74706C3A5236363932313130313738333C2F69643E0D0A090909090909093C6E616D653E4964656E74696669636174696F6E42795461673C2F6E616D653E0D0A090909090909093C726F6C654D6170733E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E5265666572656E63653C2F747970653E0D0A0909090909090909093C69643E74706C3A5233363344343241394533363534324435393335393931303139323842383531373C2F69643E0D0A0909090909090909093C6E616D653E6861734964656E74696669636174696F6E547970653C2F6E616D653E0D0A0909090909090909093C64617461547970653E72646C3A5239393034373032373530333C2F64617461547970653E0D0A0909090909090909093C76616C75653E544147204944454E54494649434154494F4E20434F4445202852455449524544293C2F76616C75653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E506F73736573736F723C2F747970653E0D0A0909090909090909093C69643E74706C3A5242443432423934344331363934314234384137323732323532383444454333353C2F69643E0D0A0909090909090909093C6E616D653E6861734F626A6563743C2F6E616D653E0D0A0909090909090909093C64617461547970653E646D3A506F737369626C65496E646976696475616C3C2F64617461547970653E0D0A09090909090909093C2F726F6C654D61703E0D0A09090909090909093C726F6C654D61703E0D0A0909090909090909093C747970653E4461746150726F70657274793C2F747970653E0D0A0909090909090909093C69643E74706C3A5244344344333739383246423534304131393442314135454538434641353535343C2F69643E0D0A0909090909090909093C6E616D653E76616C4964656E7469666965723C2F6E616D653E0D0A0909090909090909093C64617461547970653E7873643A737472696E673C2F64617461547970653E0D0A0909090909090909093C70726F70657274794E616D653E494E535452554D454E54532E4153534F435F45513C2F70726F70657274794E616D653E0D0A09090909090909093C2F726F6C654D61703E0D0A090909090909093C2F726F6C654D6170733E0D0A090909090909093C696E6465783E313C2F696E6465783E0D0A0909090909093C2F74656D706C6174654D61703E0D0A09090909093C2F74656D706C6174654D6170733E0D0A090909093C2F636C61737354656D706C6174654D61703E0D0A0909093C2F636C61737354656D706C6174654D6170733E0D0A0909093C646174614F626A6563744E616D653E494E535452554D454E54533C2F646174614F626A6563744E616D653E0D0A09093C2F67726170684D61703E0D0A093C2F67726170684D6170733E0D0A093C76616C75654C6973744D6170733E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E4C656E6774683C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E6D6D3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5235323035343237353337343C2F7572693E0D0A09090909093C6C6162656C3E4D494C4C494D45545245203C2F6C6162656C3E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E54656D70657261747572653C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E646567433C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5237343837373939323730333C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E6465674B3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5235333130393636323430353C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E50726573737572653C2F6E616D653E0D0A0909093C76616C75654D6170733E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E50415343414C3C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5239383738373735343236373C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A090909093C76616C75654D61703E0D0A09090909093C696E7465726E616C56616C75653E4241523C2F696E7465726E616C56616C75653E0D0A09090909093C7572693E72646C3A5238333439303530363535313C2F7572693E0D0A090909093C2F76616C75654D61703E0D0A0909093C2F76616C75654D6170733E0D0A09093C2F76616C75654C6973744D61703E0D0A09093C76616C75654C6973744D61703E0D0A0909093C6E616D653E6C313C2F6E616D653E0D0A0909093C76616C75654D6170732F3E0D0A09093C2F76616C75654C6973744D61703E0D0A093C2F76616C75654C6973744D6170733E0D0A3C2F6D617070696E673E, 1, 1, 1)
/****** Object:  Table [dbo].[ResourceGroups]    Script Date: 02/09/2015 11:50:11 ******/
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
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'00000000-0000-0000-0000-000000000000', 1, 1, 1, 2)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'00000000-0000-0000-0000-000000000000', 3, 1, 1, 2)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'266ea8c1-e081-464c-88bb-0137d7e78424', 1, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'266ea8c1-e081-464c-88bb-0137d7e78424', 3, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'266ea8c1-e081-464c-88bb-0137d7e78424', 5, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'5b721a3a-ed92-4f1e-bb6e-01da52333072', 1, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'5b721a3a-ed92-4f1e-bb6e-01da52333072', 3, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'1fac1b09-6275-4079-af57-01fe46bd22ba', 1, 1, 1, 4)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'1fac1b09-6275-4079-af57-01fe46bd22ba', 3, 1, 1, 4)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'1c74884a-73ba-49d0-ac66-0545f0159173', 3, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'1c74884a-73ba-49d0-ac66-0545f0159173', 5, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'd0dbf106-65d0-447e-a526-059a59edfc68', 8, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'd0dbf106-65d0-447e-a526-059a59edfc68', 9, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'8c5e0be6-8625-41fb-89d4-0a0976a32d47', 1, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'8c5e0be6-8625-41fb-89d4-0a0976a32d47', 3, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'8c5e0be6-8625-41fb-89d4-0a0976a32d47', 9, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'3e1e418d-fa51-403b-bb7b-0cade34283a7', 1, 1, 1, 1)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'8d2bccd8-8210-45e6-b39e-1009e7f7dd31', 1, 1, 0, 2)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'8d2bccd8-8210-45e6-b39e-1009e7f7dd31', 3, 1, 0, 2)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'8d2bccd8-8210-45e6-b39e-1009e7f7dd31', 5, 1, 1, 2)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'8d2bccd8-8210-45e6-b39e-1009e7f7dd31', 6, 1, 1, 2)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'8d2bccd8-8210-45e6-b39e-1009e7f7dd31', 7, 1, 0, 2)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'3dbf0cef-891e-44ac-992a-161e2431bcdd', 1, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'59586564-e649-4d98-af36-1d14f32165f7', 9, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'c5b15a98-47b7-4a47-b254-20ba9abdfb6a', 1, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'c5b15a98-47b7-4a47-b254-20ba9abdfb6a', 6, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'c5b15a98-47b7-4a47-b254-20ba9abdfb6a', 8, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'2652dfb0-db4f-4d6d-a41e-2234364a80e4', 9, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'6c0f7ef8-fba2-4637-89bd-2396e7281bdb', 1, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'6c0f7ef8-fba2-4637-89bd-2396e7281bdb', 7, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'6c0f7ef8-fba2-4637-89bd-2396e7281bdb', 8, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'a740c34e-df62-4a58-9554-23ae4a1a3c6a', 1, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'a740c34e-df62-4a58-9554-23ae4a1a3c6a', 3, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'f2716436-06bf-4fec-911a-254d0e32dc39', 1, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'b172deca-c002-4775-8fec-286ca1c9f76a', 1, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'b172deca-c002-4775-8fec-286ca1c9f76a', 6, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'a6ac8ca5-662c-4249-9d1f-2b7cb962f2cb', 1, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'a6ac8ca5-662c-4249-9d1f-2b7cb962f2cb', 3, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'a6ac8ca5-662c-4249-9d1f-2b7cb962f2cb', 4, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'a6ac8ca5-662c-4249-9d1f-2b7cb962f2cb', 5, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'a6ac8ca5-662c-4249-9d1f-2b7cb962f2cb', 6, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'f3fd50ba-d1fe-4a68-b30f-2edd18497058', 1, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'a40b9b99-7f2e-4450-8883-35dfdcf0cd1a', 8, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'c274b3fb-1794-4bc2-bcab-3615fea7f11f', 1, 3, 1, 4)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'8dff23d9-f7b7-409f-9bff-361d22c29765', 1, 1, 1, 2)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'8dff23d9-f7b7-409f-9bff-361d22c29765', 3, 1, 1, 2)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'8dff23d9-f7b7-409f-9bff-361d22c29765', 5, 1, 1, 2)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'f284377d-2c04-4697-8a1d-36808d062875', 1, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'f284377d-2c04-4697-8a1d-36808d062875', 3, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'c3dffaee-80fb-4f8f-9ca9-39c1c41dd926', 5, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'c3dffaee-80fb-4f8f-9ca9-39c1c41dd926', 6, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'c3dffaee-80fb-4f8f-9ca9-39c1c41dd926', 7, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'708ed08c-3077-4960-bf95-39e0027e086d', 1, 1, 1, 3)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'708ed08c-3077-4960-bf95-39e0027e086d', 3, 1, 1, 3)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'708ed08c-3077-4960-bf95-39e0027e086d', 5, 1, 1, 3)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'708ed08c-3077-4960-bf95-39e0027e086d', 7, 1, 1, 3)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'42583349-2679-4729-b3af-3a3f42954bf9', 8, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'7bb276c7-61d4-4ce4-aed8-3c18e5a154fd', 1, 1, 1, 4)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'7bb276c7-61d4-4ce4-aed8-3c18e5a154fd', 3, 1, 1, 4)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'7bb276c7-61d4-4ce4-aed8-3c18e5a154fd', 5, 1, 1, 4)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'9839261a-038d-426c-b145-443d16f900d4', 1, 1, 0, 6)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'9839261a-038d-426c-b145-443d16f900d4', 3, 1, 0, 6)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'9839261a-038d-426c-b145-443d16f900d4', 5, 1, 0, 6)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'73a53324-3981-4982-a974-45051a7cebed', 7, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'a78d2c39-7a81-4873-b736-45455bf4fb66', 1, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'a78d2c39-7a81-4873-b736-45455bf4fb66', 5, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'6914f8d4-636f-4d9f-aa43-45f865de56b6', 1, 1, 1, 2)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'9614f8d4-636f-4d9f-aa43-45f865de56b7', 3, 1, 1, 2)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'86a1c10e-7fde-41fa-9dbc-49568d1efaa5', 1, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'5e143c9e-0d5c-44cc-aa9e-4a3f1e3d9a17', 1, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'5e143c9e-0d5c-44cc-aa9e-4a3f1e3d9a17', 5, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'0921054e-e336-4f06-a123-4a4855f16a8a', 6, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'0921054e-e336-4f06-a123-4a4855f16a8a', 7, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'6f9f8981-55f3-433c-a86c-4b1262ff2efc', 1, 1, 1, 1)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'899c44d4-8b27-4fd1-b4de-4cfb6bc8653f', 7, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'899c44d4-8b27-4fd1-b4de-4cfb6bc8653f', 8, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'899c44d4-8b27-4fd1-b4de-4cfb6bc8653f', 9, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'67a27f18-1b49-41eb-9f48-4dd190934d25', 5, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'fcbd140b-af75-45ab-a81c-4f74f157345a', 1, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'4020f94e-9349-4376-b66f-548604d243d9', 1, 1, 1, 2)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'4020f94e-9349-4376-b66f-548604d243d9', 3, 1, 1, 2)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'4020f94e-9349-4376-b66f-548604d243d9', 5, 1, 1, 2)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'4fef96cc-6425-4e26-b2fd-57a75380bc32', 1, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'9772e692-ee6e-4030-810f-64280b0eb2cd', 8, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'9772e692-ee6e-4030-810f-64280b0eb2cd', 9, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'111234f9-876d-45c8-a4c1-69cbf2932d6d', 4, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'111234f9-876d-45c8-a4c1-69cbf2932d6d', 5, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'd74363f6-9413-4c1f-bc8f-69d8d9bbbfc8', 6, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'd74363f6-9413-4c1f-bc8f-69d8d9bbbfc8', 9, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'5cc1d872-db94-43fe-88b0-6b33fec291bb', 1, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'5cc1d872-db94-43fe-88b0-6b33fec291bb', 7, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'5cc1d872-db94-43fe-88b0-6b33fec291bb', 8, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', 1, 1, 1, 1)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', 3, 1, 0, 1)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', 5, 1, 0, 1)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'e2f9b77c-0f56-4f2d-b45b-6bee80d92c32', 6, 1, 0, 1)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'99a89a2a-85e9-468d-98f8-6c680baeb77c', 1, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'99a89a2a-85e9-468d-98f8-6c680baeb77c', 5, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'fd3ff44e-92ee-4606-9246-6eee2646e7e8', 7, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'fd3ff44e-92ee-4606-9246-6eee2646e7e8', 9, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'504ff982-740d-45a6-95d8-6f6ba425fd40', 1, 1, 1, 2)
GO
print 'Processed 100 total records'
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'83d6c735-df42-44b7-b21b-70c37c8d3a8c', 1, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'83d6c735-df42-44b7-b21b-70c37c8d3a8c', 3, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'83d6c735-df42-44b7-b21b-70c37c8d3a8c', 5, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'83d6c735-df42-44b7-b21b-70c37c8d3a8c', 8, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'b26df1b8-b185-49bb-ba18-72f935f97cf0', 1, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'b26df1b8-b185-49bb-ba18-72f935f97cf0', 9, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'54dc03c6-b1ed-4a4e-b4d9-773d4c2651f8', 1, 1, 1, 1)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'd208b9b8-0372-4e2c-9b34-78b9bb453b61', 3, 1, 1, 4)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'7877c504-133d-41b9-8680-7bfd73840435', 1, 1, 1, 3)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'3456c504-133d-41b9-8780-7bfd73840444', 3, 1, 1, 3)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', 3, 1, 1, 1)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'2567c504-133d-41b9-8780-7bfd73840ed3', 4, 1, 1, 1)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'8c2669bc-5ea4-466c-8115-7c18c85dc64a', 1, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'21b7e812-419b-4c10-990c-7c476ce335a9', 6, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'21b7e812-419b-4c10-990c-7c476ce335a9', 7, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'05ab813a-f365-4d07-9f5d-7e79dd82bd80', 7, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'950490c6-a881-42f9-8dca-7efd124c2be0', 1, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'aa2d4e3a-f8f6-4a50-b282-88f2095d9c09', 6, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'47f7418a-e970-426d-8522-89c76613a8e8', 1, 1, 0, 4)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'47f7418a-e970-426d-8522-89c76613a8e8', 3, 1, 0, 4)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'47f7418a-e970-426d-8522-89c76613a8e8', 7, 1, 0, 4)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'5cf72c68-6c14-4c4f-8b2c-8a57d81bc225', 1, 1, 1, 4)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'5cf72c68-6c14-4c4f-8b2c-8a57d81bc225', 3, 1, 1, 4)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'5cf72c68-6c14-4c4f-8b2c-8a57d81bc225', 5, 1, 1, 4)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'522f3963-f2f6-4d4d-9c1f-8be9a8c51452', 7, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'ee565602-ba52-422c-a01f-932490794343', 1, 1, 1, 6)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'ee565602-ba52-422c-a01f-932490796666', 3, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'ee904602-ba52-422c-a01f-932490798853', 1, 1, 1, 4)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'a4986708-144a-4422-8803-93c0c0acf974', 1, 1, 1, 2)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'a4986708-144a-4422-8803-93c0c0acf974', 3, 1, 1, 2)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'a4986708-144a-4422-8803-93c0c0acf974', 5, 1, 1, 2)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'9efd6494-d611-47b4-a9a3-9450219a583d', 1, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'9efd6494-d611-47b4-a9a3-9450219a583d', 3, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'9efd6494-d611-47b4-a9a3-9450219a583d', 7, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'4f7b4449-5884-4c92-a8fd-96081b3bcb2b', 7, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'4f7b4449-5884-4c92-a8fd-96081b3bcb2b', 9, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'4f7b4449-5884-4c92-a8fd-96081b3bcb2b', 23, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'd45851cd-7662-44d4-8339-97ab64d47771', 1, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'd45851cd-7662-44d4-8339-97ab64d47771', 3, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'd45851cd-7662-44d4-8339-97ab64d47771', 4, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'd45851cd-7662-44d4-8339-97ab64d47771', 5, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'1a849581-849d-41b7-9a1d-9a9ba57ef8b8', 1, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'1a849581-849d-41b7-9a1d-9a9ba57ef8b8', 3, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'22a4f857-6064-46f8-aa39-9b3f842075db', 1, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'22a4f857-6064-46f8-aa39-9b3f842075db', 4, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'22a4f857-6064-46f8-aa39-9b3f842075db', 5, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'22a4f857-6064-46f8-aa39-9b3f842075db', 6, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'22a4f857-6064-46f8-aa39-9b3f842075db', 7, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'22a4f857-6064-46f8-aa39-9b3f842075db', 8, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'22a4f857-6064-46f8-aa39-9b3f842075db', 23, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'75c73313-d275-4874-aaa6-9c0f74466d15', 1, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'75c73313-d275-4874-aaa6-9c0f74466d15', 6, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'acfd2e4f-e598-47a7-af3a-a82a74b9a04f', 9, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'41122303-f175-4381-9a09-a85620955760', 1, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'41122303-f175-4381-9a09-a85620955760', 7, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'41122303-f175-4381-9a09-a85620955760', 9, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'a6ec6af2-cdaf-4832-a3e6-a945845ef118', 7, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'8c321a48-5f9a-4d2b-a7bd-ad046f14b448', 1, 1, 1, 2)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'8c321a48-5f9a-4d2b-a7bd-ad046f14b448', 3, 1, 1, 2)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'8c321a48-5f9a-4d2b-a7bd-ad046f14b448', 5, 1, 0, 2)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'5ddc49b3-1599-47e2-9f9a-ad37413d28a7', 3, 1, 1, 1)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'448d603b-6770-48ab-b572-af218b043fbf', 3, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'448d603b-6770-48ab-b572-af218b043fbf', 9, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'b7e2d9d1-d2b4-40c8-a290-b1dafb3aa03f', 9, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'0ece41f0-a9a0-4df4-81c7-b6652a7e5486', 1, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'0ece41f0-a9a0-4df4-81c7-b6652a7e5486', 3, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'22b5e090-7374-4314-ba5c-bd778452598e', 5, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'22b5e090-7374-4314-ba5c-bd778452598e', 6, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'e6231816-0abb-47b2-acf4-be1787b25731', 1, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'fd0ce2bb-be7f-4739-aa5f-ca54da05d128', 7, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'3f462909-4de2-4940-a386-cd01182efd9f', 9, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'8c4eb21f-fdb6-46dc-a180-cd8146ae2df6', 9, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'e61137eb-fd18-4c31-93a1-d0c39bd1001a', 3, 1, 1, 1)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'ee0fad0d-01d4-490e-88b6-d28f9e20821a', 8, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'ee0fad0d-01d4-490e-88b6-d28f9e20821a', 9, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'0768db9b-2a40-47bb-9043-d46a920c4485', 1, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'0768db9b-2a40-47bb-9043-d46a920c4485', 9, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'6364aef1-cf9d-48da-8fc4-d48983659ca8', 10, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'5798beed-e4cf-4f05-87b4-d48a8837d293', 3, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'5798beed-e4cf-4f05-87b4-d48a8837d293', 5, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'3720fb5d-ebfe-4c31-8dc6-d5d23a331dc7', 1, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'7fa67256-7113-412f-8079-de04ea85ae05', 1, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'71de4cca-7e79-429c-8c4e-dea0acb59368', 1, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'71de4cca-7e79-429c-8c4e-dea0acb59368', 5, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'70f55273-60f5-4aba-8de4-def0d79584ab', 5, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'70f55273-60f5-4aba-8de4-def0d79584ab', 6, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'c7a1ff10-c2f9-486e-93e1-e720721e58a4', 1, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'c7a1ff10-c2f9-486e-93e1-e720721e58a4', 6, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'b58cd872-3c34-4d60-ac5d-ea7938f2695e', 9, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'103ed432-f36b-476c-823a-ebea3712768e', 1, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'e8964e62-2b13-485e-97b3-f61c003ea155', 6, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'e8964e62-2b13-485e-97b3-f61c003ea155', 7, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'd3d6eecb-dae7-4b10-9d7e-f6389d00a552', 1, 1, 1, 2)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'd3d6eecb-dae7-4b10-9d7e-f6389d00a552', 3, 1, 1, 2)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'd3d6eecb-dae7-4b10-9d7e-f6389d00a552', 5, 1, 1, 2)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'982596bd-557e-4d8b-9740-f767b175f1b3', 9, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'b52ec7ce-fc70-4a78-896d-f9492d141883', 1, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'b52ec7ce-fc70-4a78-896d-f9492d141883', 3, 1, 0, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'2b9c2271-3e3f-48b8-a190-fd725742d306', 1, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'2b9c2271-3e3f-48b8-a190-fd725742d306', 5, 1, 1, 5)
INSERT [dbo].[ResourceGroups] ([ResourceId], [GroupId], [SiteId], [Active], [ResourceTypeId]) VALUES (N'83e6e521-1a27-4e2b-8dc0-fe3f021a26de', 1, 1, 1, 3)
GO
print 'Processed 200 total records'
/****** Object:  Table [dbo].[BindingConfig]    Script Date: 02/09/2015 11:50:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BindingConfig](
	[ApplicationId] [uniqueidentifier] NOT NULL,
	[ModuleName] [nvarchar](250) NOT NULL,
	[BindName] [nvarchar](250) NOT NULL,
	[Service] [nvarchar](650) NOT NULL,
	[To] [nvarchar](650) NOT NULL,
	[SiteId] [int] NOT NULL,
	[Active] [tinyint] NOT NULL,
 CONSTRAINT [PK_BindingConfig] PRIMARY KEY CLUSTERED 
(
	[ApplicationId] ASC,
	[ModuleName] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [dbo].[BindingConfig] ([ApplicationId], [ModuleName], [BindName], [Service], [To], [SiteId], [Active]) VALUES (N'fee8b7c8-2a09-42f2-a9de-8f414df8ed8a', N'DataLayerBinding.Scope1iN.App1iN', N'DataLayer', N'org.iringtools.library.IDataLayer, iRINGLibrary', N'org.iringtools.adapter.datalayer.NHibernateDataLayer, NHibernateLibrary', 1, 1)
/****** Object:  Table [dbo].[ApplicationSettings]    Script Date: 02/09/2015 11:50:11 ******/
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
/****** Object:  Table [dbo].[Exchanges]    Script Date: 02/09/2015 11:50:11 ******/
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
INSERT [dbo].[Exchanges] ([ExchangeId], [CommodityId], [SourceGraphId], [DestinationGraphId], [Name], [Description], [PoolSize], [XTypeAdd], [XTypeChange], [XTypeSync], [XTypeDelete], [XTypeSetNull], [SiteId], [Active]) VALUES (N'9839261a-038d-426c-b145-443d16f900d4', N'd45851cd-7662-44d4-8339-97ab64d47771', N'd208b9b8-0372-4e2c-9b34-78b9bb453b61', N'ee904602-ba52-422c-a01f-932490798853', N'testnameu', N'descriptionu', 1111, N'testXtypeAddu', N'testXtypeChangeu', N'testXtypeSyncu', N'testXypeDeleteu', N'testxtypeSetNullu', 1, 0)
INSERT [dbo].[Exchanges] ([ExchangeId], [CommodityId], [SourceGraphId], [DestinationGraphId], [Name], [Description], [PoolSize], [XTypeAdd], [XTypeChange], [XTypeSync], [XTypeDelete], [XTypeSetNull], [SiteId], [Active]) VALUES (N'ee565602-ba52-422c-a01f-932490794343', N'ee565602-ba52-422c-a01f-932490796666', N'd208b9b8-0372-4e2c-9b34-78b9bb453b61', N'ee904602-ba52-422c-a01f-932490798853', N'ABC -> DEF', NULL, 10, N'add', N'change', N'sync', N'delete', NULL, 1, 1)
/****** Object:  Table [dbo].[Dictionary]    Script Date: 02/09/2015 11:50:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Dictionary](
	[Dictionaryid] [uniqueidentifier] NOT NULL,
	[ApplicationID] [uniqueidentifier] NOT NULL,
	[IsDBDictionary] [bit] NOT NULL,
	[PickListId] [int] NULL,
	[EnableSearch] [nvarchar](50) NULL,
	[EnableSummary] [nvarchar](50) NULL,
	[DataVersion] [nvarchar](50) NULL,
	[Provider] [nvarchar](50) NULL,
	[ConnectionString] [nvarchar](1000) NULL,
	[SchemaName] [nvarchar](50) NULL,
 CONSTRAINT [PK_Dictionary] PRIMARY KEY CLUSTERED 
(
	[Dictionaryid] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [dbo].[Dictionary] ([Dictionaryid], [ApplicationID], [IsDBDictionary], [PickListId], [EnableSearch], [EnableSummary], [DataVersion], [Provider], [ConnectionString], [SchemaName]) VALUES (N'71b0ef8b-b425-4986-aeb1-6f5d053b5420', N'7877c504-133d-41b9-8680-7bfd73840435', 1, NULL, N'false', N'false', NULL, N'mssql2008', N'connectionstring', N'dbo')
INSERT [dbo].[Dictionary] ([Dictionaryid], [ApplicationID], [IsDBDictionary], [PickListId], [EnableSearch], [EnableSummary], [DataVersion], [Provider], [ConnectionString], [SchemaName]) VALUES (N'1a09b59d-ec94-4765-8998-9e2f413cd61e', N'fee8b7c8-2a09-42f2-a9de-8f414df8ed8a', 0, NULL, N'false', N'false', NULL, N'MsSql2008', N'U/0CMsal+QOwOAtVk713zwgdwkCEiNxlzkjt90Ap19vj/M10piCIXHiUBGcwOtwXG42Rpx/ry5FrW9+t1y7MQaysiTW4Yq3hVp68jYrt4J0=', N'dbo')
INSERT [dbo].[Dictionary] ([Dictionaryid], [ApplicationID], [IsDBDictionary], [PickListId], [EnableSearch], [EnableSummary], [DataVersion], [Provider], [ConnectionString], [SchemaName]) VALUES (N'09b806c9-fc8e-49bf-a290-b1939b772266', N'54d581ef-64bc-45c2-9df5-0255f7454e0e', 0, NULL, N'false', N'false', NULL, NULL, NULL, NULL)
INSERT [dbo].[Dictionary] ([Dictionaryid], [ApplicationID], [IsDBDictionary], [PickListId], [EnableSearch], [EnableSummary], [DataVersion], [Provider], [ConnectionString], [SchemaName]) VALUES (N'54291e73-07d7-4270-abbf-fc8a4b1b3f02', N'3456c504-133d-41b9-8780-7bfd73840444', 1, NULL, N'false', N'false', NULL, N'MsSql2008', N'oGYugaP2i9vrJtm/ksepvd0XI0x9NY6y/Hi55Ou0jhbMVSgoghrjMee8wL2arE4qfYgS9FbwcA6GGjjOXCaAW0i3U3hjJMWz3hxV6skFR3A=', N'dbo')
/****** Object:  Table [dbo].[UserGroups]    Script Date: 02/09/2015 11:50:11 ******/
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
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (6, 5, 39, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (7, 6, 39, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (8, 1, 32, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (9, 3, 32, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (10, 4, 6, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (11, 5, 6, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (12, 6, 6, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (13, 7, 6, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (14, 9, 2, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (15, 9, 3, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (16, 9, 4, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (17, 9, 5, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (18, 3, 2, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (19, 3, 4, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (20, 4, 4, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (21, 5, 4, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (22, 4, 7, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (23, 5, 7, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (24, 6, 7, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (25, 5, 3, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (26, 5, 5, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (27, 3, 6, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (28, 10, 2, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (29, 10, 4, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (30, 3, 5, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (31, 4, 1, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (32, 4, 3, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (33, 4, 5, 1, NULL, 1)
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
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (45, 6, 1, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (46, 3, 38, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (47, 3, 39, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (48, 3, 8, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (49, 9, 1, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (50, 7, 1, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (51, 3, 40, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (52, 5, 32, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (53, 6, 32, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (54, 7, 32, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (55, 8, 32, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (56, 9, 32, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (57, 1, 41, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (58, 3, 41, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (59, 1, 8, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (60, 1, 40, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (61, 1, 42, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (62, 1, 7, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (63, 6, 3, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (64, 23, 1, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (65, 23, 3, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (66, 23, 6, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (67, 23, 8, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (68, 23, 32, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (69, 22, 2, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (70, 22, 1, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (71, 22, 5, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (72, 22, 7, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (73, 22, 6, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (74, 22, 8, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (75, 23, 5, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (76, 23, 7, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (77, 16, 1, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (78, 16, 3, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (79, 20, 1, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (80, 20, 3, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (81, 20, 4, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (82, 20, 5, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (83, 37, 2, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (84, 37, 4, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (85, 37, 6, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (86, 37, 8, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (87, 37, 3, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (88, 37, 1, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (89, 20, 2, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (90, 4, 38, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (91, 4, 2, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (92, 10, 1, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (93, 10, 3, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (94, 17, 1, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (95, 17, 2, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (96, 17, 3, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (97, 6, 8, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (98, 6, 2, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (99, 6, 38, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (100, 6, 41, 1, NULL, 1)
GO
print 'Processed 100 total records'
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (101, 6, 40, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (102, 6, 44, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (103, 6, 42, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (104, 6, 43, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (105, 6, 4, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (106, 6, 5, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (107, 8, 2, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (108, 10, 5, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (109, 10, 6, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (110, 10, 7, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (111, 10, 8, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (112, 10, 32, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (113, 10, 38, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (114, 10, 39, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (115, 10, 40, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (116, 10, 41, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (117, 10, 42, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (118, 10, 43, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (119, 10, 44, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (120, 4, 8, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (121, 4, 40, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (122, 4, 41, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (123, 4, 42, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (124, 4, 43, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (125, 4, 44, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (126, 5, 1, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (127, 5, 8, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (128, 5, 38, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (129, 5, 40, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (130, 5, 41, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (131, 5, 42, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (132, 5, 43, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (133, 5, 44, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (134, 8, 3, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (135, 8, 4, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (136, 8, 5, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (137, 8, 6, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (138, 8, 7, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (139, 8, 8, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (140, 8, 38, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (141, 8, 39, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (142, 8, 40, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (143, 8, 41, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (144, 8, 42, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (145, 8, 43, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (146, 8, 44, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (147, 8, 1, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (148, 19, 1, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (149, 19, 2, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (150, 7, 2, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (151, 7, 3, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (152, 7, 5, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (153, 7, 7, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (154, 7, 8, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (155, 7, 38, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (156, 7, 39, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (157, 7, 40, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (158, 7, 41, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (159, 7, 42, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (160, 7, 43, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (161, 7, 44, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (162, 18, 1, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (163, 18, 7, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (164, 1, 43, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (165, 9, 7, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (166, 16, 32, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (168, 91, 68, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (169, 92, 68, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (170, 91, 67, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (171, 92, 67, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (172, 93, 67, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (173, 94, 67, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (174, 92, 69, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (175, 94, 70, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (176, 91, 70, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (177, 92, 70, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (178, 93, 70, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (179, 95, 71, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (180, 94, 71, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (181, 92, 71, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (182, 95, 67, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (183, 95, 68, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (184, 95, 69, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (185, 95, 70, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (186, 11, 68, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (187, 97, 73, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (188, 97, 74, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (189, 97, 71, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (190, 97, 75, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (191, 97, 78, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (192, 97, 77, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (193, 97, 3, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (194, 98, 73, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (195, 99, 73, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (196, 100, 73, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (197, 102, 73, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (198, 103, 73, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (199, 113, 81, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (200, 113, 1, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (201, 112, 68, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (202, 112, 2, 1, NULL, 1)
GO
print 'Processed 200 total records'
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (203, 110, 81, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (204, 110, 1, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (205, 63, 1, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (206, 119, 68, 1, NULL, 1)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (207, 92, 2, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (208, 92, 39, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (209, 120, 2, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (210, 120, 7, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (211, 120, 1, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (212, 120, 3, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (213, 120, 5, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (214, 121, 5, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (215, 121, 3, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (216, 121, 8, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (217, 117, 68, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (218, 117, 69, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (219, 117, 70, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (220, 117, 71, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (221, 117, 81, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (222, 117, 82, 1, NULL, 0)
INSERT [dbo].[UserGroups] ([UserGroupId], [GroupId], [UserId], [SiteId], [UserGroupsDesc], [Active]) VALUES (223, 93, 69, 1, NULL, 1)
SET IDENTITY_INSERT [dbo].[UserGroups] OFF
/****** Object:  Table [dbo].[ValueListMap]    Script Date: 02/09/2015 11:50:11 ******/
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
/****** Object:  StoredProcedure [dbo].[spgUserById]    Script Date: 02/09/2015 11:50:11 ******/
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
/****** Object:  StoredProcedure [dbo].[spgSiteUsers]    Script Date: 02/09/2015 11:50:12 ******/
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
/****** Object:  StoredProcedure [dbo].[spiGroups]    Script Date: 02/09/2015 11:50:13 ******/
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

		--declare @aa int
		--set @aa = 10 / 0
						
		If not Exists(Select top 1 * from Groups Where GroupName = @GroupName and Active = 1)
		Begin
			INSERt INTO  Groups (SiteId,GroupName,GroupDesc )
			VALUES (@SiteId,@GroupName,@GroupDesc)
			
			Select '1'--'Group added successfully!'
		End
		Else
			Select '0'--'Group with this name already exists!'
	END
END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spiRole]    Script Date: 02/09/2015 11:50:13 ******/
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
	
		--declare @aa int
		--set @aa = 10 / 0
		
		If not Exists(Select top 1 * from Roles Where RoleName = @RoleName and Active = 1)
		Begin
		
		INSERT INTO Roles (SiteId,RoleName,RoleDesc)
		VALUES(@SiteId,@RoleName,@RoleDesc)
			
			Select '1'--'Role added successfully!'
		End
		Else
			Select '0'--'Role with this name already exists!'

	END
END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spiPermissions]    Script Date: 02/09/2015 11:50:14 ******/
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
		
		--declare @aa int
		--set @aa = 10 / 0
		
		If not Exists(Select top 1 * from [Permissions] Where PermissionName = @PermissionName and Active = 1)
		Begin
			INSERt INTO  [Permissions] (SiteId,PermissionName,PermissionDesc)
			VALUES (@SiteId,@PermissionName,@PermissionDesc)
						
			Select '1'--'Permission added successfully!'
		End
		Else
			Select '0'--'Permission with this name already exists!'
		
	END
END TRY

BEGIN CATCH
      SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spiUser]    Script Date: 02/09/2015 11:50:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <26-June-2014>
-- Description:	<Inserting user details>

-- =============================================
CREATE PROCEDURE [dbo].[spiUser]  --1,'test_user1','df','df','emil',34,'df'
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
	
		--declare @aa int
		--set @aa = 10 / 0
		
		If not Exists(Select top 1 * from users Where UserName = @UserName and Active = 1)
		Begin
			
			INSERT INTO Users (SiteId,UserName,UserFirstName,UserLastName,UserEmail,UserPhone,UserDesc)
			VALUES(@SiteId,@UserName,@UserFirstName,@UserLastName,@UserEmail,@UserPhone,@UserDesc)			
			
			Select  '1'--'User added successfully!'
		End
		Else
			Select  '0'--'User with this name already exists!'
				
	END
END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spuRoles]    Script Date: 02/09/2015 11:50:15 ******/
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
	
	--declare @aa int
 --   set @aa = 10 / 0
	
		UPDATE Roles 
			SET SiteId = @SiteId,
				RoleName =@RoleName,
				RoleDesc = 	@RoleDesc
		WHERE RoleId= @RoleId AND SiteId = @SiteId
		
		Select '1'--'Role updated successfully!'
		
	END
END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spuPermissions]    Script Date: 02/09/2015 11:50:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Gagan Dhamija>
-- Create date: <08-Jul-2014>
-- Description:	<Update Permissions>

-- =============================================
CREATE PROCEDURE [dbo].[spuPermissions] 
(
	@SiteId INT,
	@PermissionId INT,
	@PermissionName NVARCHAR(100),
	@PermissionDesc NVARCHAR(100)
)
	 
AS
BEGIN TRY
	BEGIN
	
	--declare @aa int
 --   set @aa = 10 / 0
    	
		UPDATE Permissions 
			SET PermissionName = @PermissionName,
				PermissionDesc = @PermissionDesc
		  WHERE	SiteId = @SiteId and PermissionId = @PermissionId
		  
		Select '1'--'Permission updated successfully!'
	END
END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spuGroups]    Script Date: 02/09/2015 11:50:16 ******/
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
	
	--declare @aa int
 --   set @aa = 10 / 0
    
		UPDATE Groups 
			SET GroupName = @GroupName,
				GroupDesc = @GroupDesc
		  WHERE	SiteId = @SiteId and GroupId = @GroupId
		  
		Select '1'--'Group updated successfully!'
	END
END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spuUser]    Script Date: 02/09/2015 11:50:16 ******/
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
 	--declare @aa int
		--set @aa = 10 / 0

	  UPDATE Users SET  
	  SiteId =@SiteId,  
	  UserName = @UserName,  
	  UserFirstName = @UserFirstName,  
	  UserLastName =@UserLastName,  
	  UserEmail = @UserEmail,  
	  UserPhone = @UserPhone,  
	  UserDesc = @UserDesc WHERE UserName = @UserName AND Active =1 AND SiteId = @SiteId  
   
	 Select  '1'--'User updated successfully!'	
 END  
END TRY  
  
BEGIN CATCH  
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spuContext]    Script Date: 02/09/2015 11:50:17 ******/
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
	@UserName varchar(100),--'vmallire'
	@DisplayName NVARCHAR(255),
	@Description NVARCHAR(255),
	@CacheConnStr NVARCHAR(255),
	@SiteId INT,
	@ContextId uniqueidentifier,
	@GroupList xml
)	
	 
AS
BEGIN

	UPDATE Contexts
		SET DisplayName=@DisplayName,
			[Description]=@Description,
			CacheConnStr=@CacheConnStr,
			SiteId=@SiteId
		WHERE ContextID = @ContextId
		
		
	SELECT       
	nref.value('groupId[1]', 'int') GroupId      
	INTO #Groups FROM   @GroupList.nodes('//group') AS R(nref) 

	/*Below query can not be included in merge statement
	as it will inactive all the non matching rows, hence
	separate query has written*/
	Update ResourceGroups
	Set   Active = 0
	Where ResourceId = @ContextId 
	
	Declare @ResourceTypeId int = 1 --See ResourceType for detail
	
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
							inner join #Groups tg on ug.GroupId = tg.GroupId
	
	)AS S  (GroupId)    
	ON (T.GroupId = S.GroupId AND T.SiteId = @SiteId AND T.ResourceId = @ContextId)       
	WHEN NOT MATCHED BY TARGET       
		THEN INSERT(ResourceId,GroupId,SiteId,ResourceTypeId) VALUES(@ContextId,S.GroupId,@SiteId,@ResourceTypeId)
	WHEN MATCHED       
		THEN UPDATE SET T.SiteId = @SiteId,  
		T.Active = 1;  	
				
END
GO
/****** Object:  StoredProcedure [dbo].[spuCommodity]    Script Date: 02/09/2015 11:50:17 ******/
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
	@GroupList xml
	
)	 
AS

	BEGIN
	
	
	
	Update Commodity
		Set CommodityName = @CommodityName
	Where CommodityId = @CommodityId 
			And Active = 1
	
	SELECT       
	nref.value('groupId[1]', 'int') GroupId      
	INTO #Groups FROM   @GroupList.nodes('//group') AS R(nref) 

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
							inner join #Groups tg on ug.GroupId = tg.GroupId
	
	)AS S  (GroupId)    
	ON (T.GroupId = S.GroupId AND T.SiteId = @SiteId AND T.ResourceId = @CommodityId)       
	WHEN NOT MATCHED BY TARGET       
		THEN INSERT(ResourceId,GroupId,SiteId,ResourceTypeId) VALUES(@CommodityId,S.GroupId,@SiteId,@ResourceTypeId)
	WHEN MATCHED       
		THEN UPDATE SET T.SiteId = @SiteId,  
		T.Active = 1;    

		 
	END
GO
/****** Object:  StoredProcedure [dbo].[spuApplication]    Script Date: 02/09/2015 11:50:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <10-June-2014>
-- Description:	<updating Application details>

-- =============================================
CREATE PROCEDURE [dbo].[spuApplication] 
(
	@UserName varchar(100),--'vmallire'
	--@ContextId	uniqueidentifier,
	@ApplicationId	uniqueidentifier,
	@DisplayName	nvarchar(255),
	--@InternalName	nvarchar(255),
	@Description	nvarchar(255),
	@DXFRUrl	nvarchar(255),
	@SiteId	int,
	@Assembly nvarchar(255),
	@GroupList xml  
)	
	 
AS
BEGIN 
			
	UPDATE [Applications]
	SET 
	 [DisplayName] = @DisplayName
	,[Description] = @Description
	,[DXFRUrl] = @DXFRUrl
	,[SiteId] = @SiteId
	,[Assembly] = @Assembly
	WHERE [ApplicationId] = @ApplicationId


	SELECT       
	nref.value('groupId[1]', 'int') GroupId      
	INTO #Groups FROM   @GroupList.nodes('//group') AS R(nref) 

	/*Below query can not be included in merge statement
	as it will inactive all the non matching rows, hence
	separate query has written*/
	Update ResourceGroups
	Set   Active = 0
	Where ResourceId = @ApplicationId 
	
	Declare @ResourceTypeId int = 3 --See ResourceType for detail
	
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
							inner join #Groups tg on ug.GroupId = tg.GroupId
	
	)AS S  (GroupId)    
	ON (T.GroupId = S.GroupId AND T.SiteId = @SiteId AND T.ResourceId = @ApplicationId)       
	WHEN NOT MATCHED BY TARGET       
		THEN INSERT(ResourceId,GroupId,SiteId,ResourceTypeId) VALUES(@ApplicationId,S.GroupId,@SiteId,@ResourceTypeId)
	WHEN MATCHED       
		THEN UPDATE SET T.SiteId = @SiteId,  
		T.Active = 1;  

		
END
GO
/****** Object:  StoredProcedure [dbo].[spuGraph]    Script Date: 02/09/2015 11:50:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Gagan Dhamija>
-- Create date: <16-Jul-2014>
-- Description:	<updating  Graphs> 
-- ===========================================================
create PROCEDURE [dbo].[spuGraph] 
(
	@UserName varchar(100),--'vmallire'
	@GraphId uniqueidentifier,
	@GraphName nvarchar(255),
	@Graph varbinary(MAX),
	@SiteId int,
	@GroupList xml
)	 
AS

	BEGIN
	
	UPDATE [Graphs]
	   SET 
		   [GraphName] = @GraphName
		  ,[Graph] = @Graph
		  ,[SiteId] = @SiteId
	WHERE  GraphId = @GraphId


	SELECT       
	nref.value('groupId[1]', 'int') GroupId      
	INTO #Groups FROM   @GroupList.nodes('//group') AS R(nref) 

	/*Below query can not be included in merge statement
	as it will inactive all the non matching rows, hence
	separate query has written*/
	Update ResourceGroups
	Set   Active = 0
	Where ResourceId = @GraphId 
	
	Declare @ResourceTypeId int = 4 --See ResourceType for detail
	
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
							inner join #Groups tg on ug.GroupId = tg.GroupId
	
	)AS S  (GroupId)    
	ON (T.GroupId = S.GroupId AND T.SiteId = @SiteId AND T.ResourceId = @GraphId)       
	WHEN NOT MATCHED BY TARGET       
		THEN INSERT(ResourceId,GroupId,SiteId,ResourceTypeId) VALUES(@GraphId,S.GroupId,@SiteId,@ResourceTypeId)
	WHEN MATCHED       
		THEN UPDATE SET T.SiteId = @SiteId,  
		T.Active = 1;  

 END
GO
/****** Object:  StoredProcedure [dbo].[spuFolder]    Script Date: 02/09/2015 11:50:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Deepak Kumar>
-- Create date: <25-Sep-2014>
-- Description:	<Inserting folder>
-- ===========================================================
CREATE PROCEDURE [dbo].[spuFolder] 
(
	@UserName varchar(100),--'vmallire'
	@SiteId INT,--1
	@FolderId uniqueidentifier,
	@ParentFolderId uniqueidentifier,
    @FolderName nvarchar(50),
	@GroupList xml
)	 
AS

	BEGIN
	
	
	
	Update Folders
		Set FolderName = @FolderName,
			ParentFolderId = @ParentFolderId
	Where FolderId = @FolderId 
			And Active = 1

	SELECT       
	nref.value('groupId[1]', 'int') GroupId      
	INTO #Groups FROM   @GroupList.nodes('//group') AS R(nref) 

	/*Below query can not be included in merge statement
	as it will inactive all the non matching rows, hence
	separate query has written*/
	Update ResourceGroups
	Set   Active = 0
	Where ResourceId = @FolderId
	
	Declare @ResourceTypeId int = 2 --See ResourceType for detail
	
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
							inner join #Groups tg on ug.GroupId = tg.GroupId
	
	)AS S  (GroupId)    
	ON (T.GroupId = S.GroupId AND T.SiteId = @SiteId AND T.ResourceId = @FolderId)       
	WHEN NOT MATCHED BY TARGET       
		THEN INSERT(ResourceId,GroupId,SiteId,ResourceTypeId) VALUES(@FolderId,S.GroupId,@SiteId,@ResourceTypeId)
	WHEN MATCHED       
		THEN UPDATE SET T.SiteId = @SiteId,  
		T.Active = 1;    
							

	END
GO
/****** Object:  StoredProcedure [dbo].[spuExchange]    Script Date: 02/09/2015 11:50:19 ******/
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
	/*Note: All the nullable Fields in [Exchanges] table
	  needs to have default value to null as done below.
	  so that if value is not supplied in the parameter
	  then null will be assigned in corresspoing field
	*/
	@UserName varchar(100),--'vmallire'
	@ExchangeId uniqueidentifier,
	@SourceGraphId	uniqueidentifier,
	@DestinationGraphId	uniqueidentifier,
	@Name	nvarchar(255),
	@Description	nvarchar(255) = null,
	@PoolSize	int,
	@XTypeAdd	nvarchar(25) = null,
	@XTypeChange	nvarchar(25) = null,
	@XTypeSync	nvarchar(25) = null,
	@XTypeDelete	nvarchar(25) = null,
	@XTypeSetNull	nvarchar(25) = null,
	@SiteId	int,
	@GroupList xml
	
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

	SELECT       
	nref.value('groupId[1]', 'int') GroupId      
	INTO #Groups FROM   @GroupList.nodes('//group') AS R(nref)

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
							inner join #Groups tg on ug.GroupId = tg.GroupId
	
	)AS S  (GroupId)    
	ON (T.GroupId = S.GroupId AND T.SiteId = @SiteId AND T.ResourceId = @ExchangeId)       
	WHEN NOT MATCHED BY TARGET       
		THEN INSERT(ResourceId,GroupId,SiteId,ResourceTypeId) VALUES(@ExchangeId,S.GroupId,@SiteId,@ResourceTypeId)
	WHEN MATCHED       
		THEN UPDATE SET T.SiteId = @SiteId,  
		T.Active = 1;    
							

	END
GO
/****** Object:  StoredProcedure [dbo].[spiUserGroups]    Script Date: 02/09/2015 11:50:20 ******/
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
BEGIN TRY
	BEGIN
	    
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

Declare @IsGroupExists int    
Select top 1 @IsGroupExists = GroupId from #Source

	if (@IsGroupExists <> -1)      
	Begin      
		MERGE [UserGroups] AS T      
		USING #Source AS S      
		ON (T.GroupId = S.GroupId AND T.UserId = S.UserId)       
		WHEN NOT MATCHED BY TARGET       
			THEN INSERT([GroupId],[UserId],[SiteId]) VALUES(S.[GroupId],S.[UserId],@SiteId)      
		WHEN MATCHED       
			THEN UPDATE SET T.SiteId = @SiteId,  
			--T.UserGroupsDesc = S.UserGroupsDesc,   
			T.Active = 1; 
			
	 Select '1' --'Groups mapped with the user successfully!' 		    
	End
	Else
	 Select '0' --'All groups unmapped with the user successfully!'
	    
END
END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spgContextByUser]    Script Date: 02/09/2015 11:50:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Gagan Dhamija>
-- Create date: <22-July-2014>
-- Description:	<Selecting Contexts based on users>
-- ===========================================================
CREATE  PROCEDURE [dbo].[spgContextByUser]
(
	@UserName varchar(100),
	@SiteId INT,
	@FolderId uniqueidentifier
)	 
AS
BEGIN TRY
	BEGIN
	 
	  WITH XMLNAMESPACES ( DEFAULT 'http://www.iringtools.org/library')
      select  
		c.ContextId as contextId ,
		c.DisplayName as displayName,
		c.InternalName as internalName,
		c.Description as description,
		c.CacheConnStr as cacheConnStr,
		c.SiteId as siteId,
		c.Active as active,
		c.FolderId as folderId,
		(
		  select 
				distinct 
				p.PermissionId as permissionId,
				p.SiteId as siteId, 
				p.PermissionName as permissionName,
				p.PermissionDesc as permissionDesc,
				p.Active as active 
		  from permissions p
		  inner join ResourceGroups rg on rg.siteid = c.siteid and rg.active = c.active and rg.resourceid = c.contextid 
		  inner join Groups g on g.groupId = rg.groupId and g.siteid = c.siteid and g.active = c.active
		  inner join grouproles gr on  gr.groupid = rg.groupid and gr.siteid = c.siteid and gr.active = c.active    
		  inner join roles r on r.roleid = gr.roleid and r.siteid = c.siteid and r.active = c.active     
		  inner join rolepermissions rp on rp.permissionid = p.permissionid and rp.roleid = r.roleid and rp.siteid = c.siteid and rp.active = c.active          
		  inner join usergroups ug on ug.groupid = rg.groupid  and ug.siteid = c.siteid and ug.active = c.active
		  inner join users u on u.userid = ug.userid and u.username = @UserName and u.siteid = c.siteid and u.active = c.active
		where c.folderid = @FolderId and  c.siteid = @SiteId and c.active = 1   for xml PATH('permission'), type 
		) as 'permissions',
		(
			Select gg.groupId,gg.groupName from ResourceGroups rr inner join Groups gg on rr.GroupId = gg.GroupId 
			where rr.ResourceId = c.contextid 
			and rr.Active = 1
			and gg.Active = 1 
			for xml PATH('group'), type
		) as 'groups'
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
/****** Object:  StoredProcedure [dbo].[spiGroupUsers]    Script Date: 02/09/2015 11:50:21 ******/
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

BEGIN TRY
	BEGIN
 
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


Declare @IsUserExists int    
Select top 1 @IsUserExists = UserId from #Source

if (@IsUserExists <> -1)
Begin    
	MERGE [UserGroups] AS T    
	USING #Source AS S    
	ON (T.GroupId = S.GroupId AND T.UserId = S.UserId)     
	WHEN NOT MATCHED BY TARGET     
		THEN INSERT([GroupId],[UserId],[SiteId]) VALUES(S.[GroupId],S.[UserId],@SiteId)    
	WHEN MATCHED     
		THEN UPDATE SET T.SiteId = @SiteId,
		--T.UserGroupsDesc = S.UserGroupsDesc, 
		T.Active = 1;
	    
	Select '1' --'Users mapped with the group successfully!' 		    
End
Else
	 Select '0' --'All users unmapped with the group successfully!'   
	    
END
END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spiRolePermissions]    Script Date: 02/09/2015 11:50:22 ******/
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
BEGIN TRY
	BEGIN
	   
				SET NOCOUNT ON;    
				    
				--Set @rawXML = ''    
				
				--declare @aa int
				--set @aa = 10 / 0    
				      
				SELECT       
				 nref.value('roleId[1]', 'int') RoleId,      
				 nref.value('permissionId[1]', 'int') PermissionId--,      
				INTO #Source FROM   @rawXML.nodes('//rolePermission') AS R(nref)      
				      
				UPDATE t    
				SET t.Active = 0    
				FROM RolePermissions t INNER JOIN #Source s ON t.RoleId = s.RoleId

				Declare @IsPermissionExists int    
				Select top 1 @IsPermissionExists = PermissionId from #Source      



					if (@IsPermissionExists <> -1)      
					Begin 
						MERGE [RolePermissions] AS T      
						USING #Source AS S      
						ON (T.RoleId = S.RoleId AND T.PermissionId = S.PermissionId )       
						WHEN NOT MATCHED BY TARGET       
							THEN INSERT([RoleId],[PermissionID],[SiteId]) VALUES(S.[RoleId],S.[PermissionID],@SiteId)      
						WHEN MATCHED       
							THEN UPDATE SET T.SiteId = @SiteId,  
							T.Active = 1;     
						Select '1' --'Permissions mapped with the role successfully!' 		    
					End
					Else
						 Select '0' --'All permissions unmapped with the role successfully!'


END
END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spiRoleGroups]    Script Date: 02/09/2015 11:50:22 ******/
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
BEGIN TRY
	BEGIN
	    
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
		    
				--declare @aa int
				--set @aa = 10 / 0    
						      
		SELECT       
		 nref.value('groupId[1]', 'int') GroupId,      
		 nref.value('roleId[1]', 'int') RoleId--,      
		INTO #Source FROM   @rawXML.nodes('//groupRole') AS R(nref)      
		      
		UPDATE t    
		SET t.Active = 0    
		FROM GroupRoles t INNER JOIN #Source s ON t.RoleId = s.RoleId


		Declare @IsGroupExists int    
		Select top 1 @IsGroupExists = GroupId from #Source

			if (@IsGroupExists <> -1)      
			Begin      
				MERGE [GroupRoles] AS T      
				USING #Source AS S      
				ON (T.RoleId = S.RoleId AND T.GroupId = S.GroupId )       
				WHEN NOT MATCHED BY TARGET       
					THEN INSERT([GroupId],[RoleId],[SiteId]) VALUES(S.[GroupId],S.[RoleId],@SiteId)      
				WHEN MATCHED       
					THEN UPDATE SET T.SiteId = @SiteId,  
					--T.UserGroupsDesc = S.UserGroupsDesc,   
					T.Active = 1;     
				Select '1' --'Groups mapped with the role successfully!' 		    
			End
			Else
				 Select '0' --'All groups unmapped with the role successfully!'


END
END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spiGroupRoles]    Script Date: 02/09/2015 11:50:23 ******/
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


BEGIN TRY
	BEGIN
    
			SET NOCOUNT ON;    
			    
			--Set @rawXML = ''    
		  --declare @aa int
		  --set @aa = 10 / 0   
			      
			SELECT       
			 nref.value('groupId[1]', 'int') GroupId,      
			 nref.value('roleId[1]', 'int') RoleId--,      
			INTO #Source FROM   @rawXML.nodes('//groupRole') AS R(nref)      
			      
			UPDATE t    
			SET t.Active = 0    
			FROM GroupRoles t INNER JOIN #Source s ON t.GroupId = s.GroupId
			     
			Declare @IsRoleExists int    
			Select top 1 @IsRoleExists = RoleId from #Source      

				if (@IsRoleExists <> -1)      
				Begin         
					MERGE [GroupRoles] AS T      
					USING #Source AS S      
					ON (T.RoleId = S.RoleId AND T.GroupId = S.GroupId )       
					WHEN NOT MATCHED BY TARGET       
						THEN INSERT([GroupId],[RoleId],[SiteId]) VALUES(S.[GroupId],S.[RoleId],@SiteId)      
					WHEN MATCHED       
						THEN UPDATE SET T.SiteId = @SiteId,  
						T.Active = 1;     
					Select '1' --'Roles mapped with the group successfully!' 		    
				End
				Else
					 Select '0' --'All roles unmapped with the group successfully!'

	 END
END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH;
GO
/****** Object:  Table [dbo].[DataObjects]    Script Date: 02/09/2015 11:50:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DataObjects](
	[DataObjectId] [uniqueidentifier] NOT NULL,
	[DictionaryId] [uniqueidentifier] NOT NULL,
	[TableName] [nvarchar](50) NOT NULL,
	[ObjectNameSpace] [nvarchar](100) NULL,
	[ObjectName] [nvarchar](50) NULL,
	[KeyDelimeter] [nvarchar](50) NULL,
	[Description] [nvarchar](200) NULL,
 CONSTRAINT [PK_DataObjects] PRIMARY KEY CLUSTERED 
(
	[DataObjectId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [dbo].[DataObjects] ([DataObjectId], [DictionaryId], [TableName], [ObjectNameSpace], [ObjectName], [KeyDelimeter], [Description]) VALUES (N'951f5dd1-e796-4f5b-bc04-07a111df3733', N'54291e73-07d7-4270-abbf-fc8a4b1b3f02', N'LINES', N'org.iringtools.adapter.datalayer.proj_12345_000.abc', N'LINES', NULL, NULL)
INSERT [dbo].[DataObjects] ([DataObjectId], [DictionaryId], [TableName], [ObjectNameSpace], [ObjectName], [KeyDelimeter], [Description]) VALUES (N'3915d9dd-55bc-47a7-a081-2f4a252cf809', N'1a09b59d-ec94-4765-8998-9e2f413cd61e', N'tbl2', N'org.iringtools.adapter.datalayer.proj_Scope1iN.App1iN', N'tbl2', NULL, NULL)
INSERT [dbo].[DataObjects] ([DataObjectId], [DictionaryId], [TableName], [ObjectNameSpace], [ObjectName], [KeyDelimeter], [Description]) VALUES (N'8d1e8209-d806-436e-a36d-b9ce1315c59d', N'09b806c9-fc8e-49bf-a290-b1939b772266', N'EQUIPMENT', N'org.iringtools.adapter.datalayer.proj_Scope2IN.App2IN', N'EQUIPMENT', NULL, NULL)
INSERT [dbo].[DataObjects] ([DataObjectId], [DictionaryId], [TableName], [ObjectNameSpace], [ObjectName], [KeyDelimeter], [Description]) VALUES (N'3b0def40-a81e-443c-ad64-c74a2406ae6e', N'1a09b59d-ec94-4765-8998-9e2f413cd61e', N'tbl1', N'org.iringtools.adapter.datalayer.proj_Scope1iN.App1iN', N'tbl1', NULL, NULL)
INSERT [dbo].[DataObjects] ([DataObjectId], [DictionaryId], [TableName], [ObjectNameSpace], [ObjectName], [KeyDelimeter], [Description]) VALUES (N'723cb279-9961-46ba-ae46-d4f765590dcf', N'71b0ef8b-b425-4986-aeb1-6f5d053b5420', N'LINES', N'org.iringtools.adapter.datalayer.proj_12345_000.DEF', N'LINES', N' key delimiter', N'desc')
INSERT [dbo].[DataObjects] ([DataObjectId], [DictionaryId], [TableName], [ObjectNameSpace], [ObjectName], [KeyDelimeter], [Description]) VALUES (N'bcbae4fd-7beb-4695-9d6b-d96a863eac9e', N'09b806c9-fc8e-49bf-a290-b1939b772266', N'INSTRUMENTS', N'org.iringtools.adapter.datalayer.proj_Scope2IN.App2IN', N'INSTRUMENTS', NULL, NULL)
INSERT [dbo].[DataObjects] ([DataObjectId], [DictionaryId], [TableName], [ObjectNameSpace], [ObjectName], [KeyDelimeter], [Description]) VALUES (N'b8d3d2ef-3b52-4ec5-b611-e46f05973702', N'71b0ef8b-b425-4986-aeb1-6f5d053b5420', N'tbl1', N'org.iringtools.adapter.datalayer.proj_scope1IN.app1IN', N'tbl1', NULL, NULL)
/****** Object:  StoredProcedure [dbo].[spiGraph]    Script Date: 02/09/2015 11:50:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Gagan Dhamija>
-- Create date: <16-Jul-2014>
-- Description:	<Inserting  Graphs> 
-- ===========================================================
create PROCEDURE [dbo].[spiGraph] 
(
	@UserName varchar(100),--'vmallire'
	@ApplicationId uniqueidentifier,
	@GraphName nvarchar(255),
	@Graph varbinary(MAX),
	@SiteId int,
	@GroupList xml
)	 
AS

	BEGIN
	
	
	
	Declare @GraphId uniqueidentifier = NewID()  
	Declare @ResourceTypeId int = 4 --See ResourceType for detail
	
	
	INSERT INTO [Graphs]
           ([ApplicationId]
           ,[GraphId]
           ,[GraphName]
           ,[Graph]
           ,[SiteId]
            )
     VALUES
           (
           @ApplicationId
           ,@GraphId
           ,@GraphName
           ,@Graph
           ,@SiteId
           )

	
	SELECT       
	nref.value('groupId[1]', 'int') GroupId      
	INTO #Groups FROM   @GroupList.nodes('//group') AS R(nref)
	
	INSERT INTO [ResourceGroups]
           ([ResourceId]
           ,[GroupId]
           ,[SiteId]
           ,[ResourceTypeId]
           )
	Select 
		@GraphId,
		tg.GroupId,
		@SiteId,
		@ResourceTypeId
	from  UserGroups ug 
			inner join Users u on ug.UserId = u.UserId
				And u.UserName = @UserName
				And u.SiteId = @SiteId
				And u.Active = 1
				And ug.Active = 1
				And ug.SiteId = @SiteId
			inner join #Groups tg on ug.GroupId = tg.GroupId
							

	END
GO
/****** Object:  StoredProcedure [dbo].[spiFolder]    Script Date: 02/09/2015 11:50:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Deepak Kumar>
-- Create date: <25-Sep-2014>
-- Description:	<Inserting folder>
-- ===========================================================
CREATE PROCEDURE [dbo].[spiFolder] 
(
	@UserName varchar(100),--'vmallire'
	@SiteId INT,--1
	@ParentFolderId uniqueidentifier,
    @FolderName nvarchar(50),
	@GroupList xml
)	 
AS

	BEGIN
	
	
	
	Declare @FolderId uniqueidentifier = NewID()  
	Declare @ResourceTypeId int = 2 --See ResourceType for detail
	
	
	INSERT INTO [Folders]
			   ([FolderId]
			   ,[ParentFolderId]
			   ,[FolderName]
			   ,[SiteId]
			   )
	Select @FolderId, @ParentFolderId,@FolderName,@SiteId
	
	
	SELECT       
	nref.value('groupId[1]', 'int') GroupId      
	INTO #Groups FROM   @GroupList.nodes('//group') AS R(nref)
	
	INSERT INTO [ResourceGroups]
           ([ResourceId]
           ,[GroupId]
           ,[SiteId]
           ,[ResourceTypeId]
           )
	Select 
		@FolderId,
		tg.GroupId,
		@SiteId,
		@ResourceTypeId
	from  UserGroups ug 
			inner join Users u on ug.UserId = u.UserId
				And u.UserName = @UserName
				And u.SiteId = @SiteId
				And u.Active = 1
				And ug.Active = 1
				And ug.SiteId = @SiteId
			inner join #Groups tg on ug.GroupId = tg.GroupId
							

	END
GO
/****** Object:  StoredProcedure [dbo].[spiExchange]    Script Date: 02/09/2015 11:50:24 ******/
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
	/*Note: All the nullable Fields in [Exchanges] table
	  needs to have default value to null as done below.
	  so that if value is not supplied in the parameter
	  then null will be assigned in corresspoing field
	*/
	@UserName varchar(100),--'vmallire'
	@CommodityId	uniqueidentifier,
	@SourceGraphId	uniqueidentifier,
	@DestinationGraphId	uniqueidentifier,
	@Name	nvarchar(255),
	@Description	nvarchar(255) = null,
	@PoolSize	int,
	@XTypeAdd	nvarchar(25) = null,
	@XTypeChange	nvarchar(25)  = null,
	@XTypeSync	nvarchar(25) = null,
	@XTypeDelete	nvarchar(25) = null,
	@XTypeSetNull	nvarchar(25) = null,
	@SiteId	int,
	@GroupList xml
	
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


	SELECT       
			nref.value('groupId[1]', 'int') GroupId      
			INTO #Groups FROM   @GroupList.nodes('//group') AS R(nref)
			
	
	INSERT INTO [ResourceGroups]
           ([ResourceId]
           ,[GroupId]
           ,[SiteId]
           ,[ResourceTypeId]
           )
	Select 
		@ExchangeId,
		tg.GroupId,
		@SiteId,
		@ResourceTypeId
	from  UserGroups ug 
			inner join Users u on ug.UserId = u.UserId
				And u.UserName = @UserName
				And u.SiteId = @SiteId
				And u.Active = 1
				And ug.Active = 1
				And ug.SiteId = @SiteId
			inner join #Groups tg on ug.GroupId = tg.GroupId
							

	END
GO
/****** Object:  StoredProcedure [dbo].[spiContext]    Script Date: 02/09/2015 11:50:25 ******/
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
    @UserName varchar(100),--'vmallire'
	@DisplayName NVARCHAR(255),
	@InternalName NVARCHAR(255),
	@Description NVARCHAR(255),
	@CacheConnStr NVARCHAR(255),
	@SiteId INT,
	@FolderId UNIQUEIDENTIFIER,
	@GroupList xml   
)	
	 
AS

BEGIN
		DECLARE @ContextCount INT
		
		SELECT @ContextCount=COUNT(*) FROM Contexts WHERE InternalName=@InternalName AND SiteId=@SiteId
		IF @ContextCount=0
		BEGIN
			
			Declare @ContextId uniqueidentifier = NewID()  
	        Declare @ResourceTypeId int = 1 --See ResourceType for detail
			
			INSERT INTO Contexts(ContextId,DisplayName,InternalName, Description,CacheConnStr,SiteId,FolderId)
			VALUES(@ContextId,@DisplayName,@InternalName,@Description,@CacheConnStr,@SiteId,@FolderId)
			
			SELECT       
			nref.value('groupId[1]', 'int') GroupId      
			INTO #Groups FROM   @GroupList.nodes('//group') AS R(nref) 

			INSERT INTO [ResourceGroups]
           ([ResourceId]
           ,[GroupId]
           ,[SiteId]
           ,[ResourceTypeId]
           )
			Select 
				@ContextId,
				tg.GroupId,
				@SiteId,
				@ResourceTypeId
			from  UserGroups ug 
					inner join Users u on ug.UserId = u.UserId
						And u.UserName = @UserName
						And u.SiteId = @SiteId
						And u.Active = 1
						And ug.Active = 1
						And ug.SiteId = @SiteId
					inner join #Groups tg on ug.GroupId = tg.GroupId
		
			END
		
END
GO
/****** Object:  StoredProcedure [dbo].[spiCommodity]    Script Date: 02/09/2015 11:50:25 ******/
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
	@GroupList xml
	
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

	SELECT       
	nref.value('groupId[1]', 'int') GroupId      
	INTO #Groups FROM   @GroupList.nodes('//group') AS R(nref) 

	
	INSERT INTO [ResourceGroups]
           ([ResourceId]
           ,[GroupId]
           ,[SiteId]
           ,[ResourceTypeId]
           )
	Select 
		@CommodityId,
		tg.GroupId,
		@SiteId,
		@ResourceTypeId
	from  UserGroups ug 
			inner join Users u on ug.UserId = u.UserId
				And u.UserName = @UserName
				And u.SiteId = @SiteId
				And u.Active = 1
				And ug.Active = 1
				And ug.SiteId = @SiteId
			inner join #Groups tg on ug.GroupId = tg.GroupId
							

	END
GO
/****** Object:  StoredProcedure [dbo].[spiApplication]    Script Date: 02/09/2015 11:50:26 ******/
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
	@UserName varchar(100),--'vmallire'
	@ContextId	uniqueidentifier,
	--@ApplicationId	uniqueidentifier,
	@DisplayName	nvarchar(255),
	@InternalName	nvarchar(255),
	@Description	nvarchar(255),
	@DXFRUrl	nvarchar(255),
	@SiteId	int,
	@Assembly nvarchar(255),
	@GroupList xml  
)	
	 
AS
BEGIN 
			
		IF Not Exists (Select top 1 * from Applications where InternalName = @InternalName And SiteId = @SiteId) 
		BEGIN
			
			Declare @ApplicationId uniqueidentifier = NewID() 
			Declare @ResourceTypeId int = 3 --See ResourceType for detail
			 
			INSERT INTO [Applications]
           ([ContextId]
           ,[ApplicationId]
           ,[DisplayName]
           ,[InternalName]
           ,[Description]
           ,[DXFRUrl]
           ,[SiteId]
           ,[Assembly])
		     VALUES
           (@ContextId
           ,@ApplicationId
           ,@DisplayName
           ,@InternalName
           ,@Description
           ,@DXFRUrl
           ,@SiteId
           ,@Assembly)

			SELECT       
			nref.value('groupId[1]', 'int') GroupId      
			INTO #Groups FROM   @GroupList.nodes('//group') AS R(nref) 
			
			INSERT INTO [ResourceGroups]
           ([ResourceId]
           ,[GroupId]
           ,[SiteId]
           ,[ResourceTypeId]
           )
			Select 
				@ApplicationId,
				tg.GroupId,
				@SiteId,
				@ResourceTypeId
			from  UserGroups ug 
					inner join Users u on ug.UserId = u.UserId
						And u.UserName = @UserName
						And u.SiteId = @SiteId
						And u.Active = 1
						And ug.Active = 1
						And ug.SiteId = @SiteId
					inner join #Groups tg on ug.GroupId = tg.GroupId

			
		END
		
	END
GO
/****** Object:  StoredProcedure [dbo].[spgValuelistforManifest]    Script Date: 02/09/2015 11:50:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Gagan Dhamija>
-- Create date: <17-Sep-2014>
-- Description:	<Selecting Valuelist based on Application Id>
-- ===========================================================
CREATE PROCEDURE [dbo].[spgValuelistforManifest]
(
	@UserName varchar(100),
	@SiteId int,
	@ApplicationId uniqueidentifier
)	 
AS
BEGIN TRY
	BEGIN
	 
      WITH XMLNAMESPACES ( DEFAULT 'http://www.iringtools.org/mapping')
      select  v.Name as name, 
      (select distinct  vm.internalvalue as internalValue, vm.Uri as uri, vm.Label as label from ValueListMap vm
      inner join ResourceGroups crg on crg.siteid = @SiteId and crg.active = 1 and crg.resourceid = vm.ApplicationId 
      inner join usergroups cug on cug.groupid = crg.groupid and cug.siteid = crg.siteid and cug.active = crg.active
      inner join users cu on cu.userid = cug.userid and cu.username = @UserName and cu.siteid = crg.siteid and cu.active = crg.active
      inner Join  Applications a  on  a.ApplicationId = vm.ApplicationId  
      where UPPER(vm.ApplicationId) = UPPER(@ApplicationId)  and vm.Name = v.Name
      Group BY   vm.Label, vm.internalvalue ,vm.Uri 
      for xml PATH('valueMap'), type ) as 'valueMaps'
      from ValueListMap v
      inner join ResourceGroups crg on crg.siteid = @SiteId and crg.active = 1 and crg.resourceid = v.ApplicationId      
      inner join usergroups cug on cug.groupid = crg.groupid and cug.siteid = crg.siteid and cug.active = crg.active
      inner join users cu on cu.userid = cug.userid and cu.username = @UserName and cu.siteid = crg.siteid and cu.active = crg.active
      inner Join  Applications a  on  a.ApplicationId = v.ApplicationId 
      where UPPER(v.ApplicationId) = UPPER(@ApplicationId)  
      Group BY v.ApplicationId, v.Name
      for xml PATH('valueListMap'), ROOT('valueListMaps'), type, ELEMENTS XSINIL

	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spgValuelist]    Script Date: 02/09/2015 11:50:27 ******/
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
/****** Object:  StoredProcedure [dbo].[spgUserGroups]    Script Date: 02/09/2015 11:50:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Gagan Dhamija>
-- Create date: <28-July-2014>
-- Description:	<Getting User Groups by specific user>
-- =============================================
CREATE PROCEDURE [dbo].[spgUserGroups] --'test user6',1
(
	@userName Varchar(50),
	@siteId INT
)
	 
AS
BEGIN TRY
	BEGIN
         SELECT ug.UserGroupId, ug.GroupId, ug.UserId, ug.UserGroupsDesc FROM UserGroups ug
         JOIN Users ON ug.UserId=Users.UserId 
         where ug.SiteId = @siteId and ug.Active = 1 and Users.UserName = @userName and Users.Active = 1;
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spgRolesGroup]    Script Date: 02/09/2015 11:50:28 ******/
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
/****** Object:  StoredProcedure [dbo].[spgCommoditiesByUser]    Script Date: 02/09/2015 11:50:29 ******/
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
		) as 'permissions',
		(
			Select gg.groupId,gg.groupName from ResourceGroups rr inner join Groups gg on rr.GroupId = gg.GroupId 
			where rr.ResourceId = c.CommodityId 
			and rr.Active = 1
			and gg.Active = 1 
			for xml PATH('group'), type
		) as 'groups'
		
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
/****** Object:  StoredProcedure [dbo].[spgApplicationByUser]    Script Date: 02/09/2015 11:50:29 ******/
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
	
	 WITH XMLNAMESPACES (default 'http://www.iringtools.org/library') 
     select  
		 a.ContextId as contextId , 
		 a.ApplicationId as applicationId , 
		 a.DisplayName as displayName,
		 a.InternalName as internalName,
		 a.Description as description,
		 a.DXFRUrl as dxfrUrl,
		 a.SiteId as siteId,
		 a.Active as active,
		 a.Assembly as assembly,
		 (
		 select 
			 distinct  
			 p.PermissionId as permissionId, 
			 p.SiteId as siteId, 
			 p.PermissionName as permissionName, 
			 p.PermissionDesc as permissionDesc, 
			 p.Active as active 
		 from permissions p
		 inner join ResourceGroups rg on rg.siteid = a.siteid and rg.active = a.active and rg.resourceid = a.ApplicationId
		 inner join Groups g on g.groupId = rg.groupId and g.siteid = a.siteid and g.active = a.active
		 inner join grouproles gr on  gr.groupid = rg.groupid and gr.siteid = a.siteid and gr.active = a.active    
		 inner join roles r on r.roleid = gr.roleid and r.siteid = a.siteid and r.active = a.active     
		 inner join rolepermissions rp on rp.permissionid = p.permissionid and rp.roleid = r.roleid and rp.siteid = a.siteid and rp.active = a.active          
		 inner join usergroups ug on ug.groupid = rg.groupid  and ug.siteid = a.siteid and ug.active = a.active
		 inner join users u on u.userid = ug.userid and u.username = @UserName and u.siteid = a.siteid and u.active = a.active
		 where a.ContextId = @ContextId and  a.siteid = @SiteId and a.active = 1   for xml PATH('permission'), type 
		 ) as 'permissions',
		 (
		 select 
			 distinct 
			 s.Name as name,
			 appset.SettingValue as value 
		 from ApplicationSettings appSet 
		 inner join ResourceGroups appSetRG on appSetRG.siteid = a.siteid and appSetRG.active = a.active and appSetRG.resourceid = a.ApplicationId and  appSet.ApplicationId = appSetRG.resourceid
		 inner join Groups appSeG on appSeG.groupId = appSetRG.groupId and appSeG.siteid = a.siteid and appSeG.active = a.active
		 inner join grouproles appSeGR on  appSeGR.groupid = appSetRG.groupid and appSeGR.siteid = a.siteid and appSeGR.active = a.active    
		 inner join roles appSeR on appSeR.roleid = appSeGR.roleid and appSeR.siteid = a.siteid and appSeR.active = a.active         
		 inner join usergroups appSeUG on appSeUG.groupid = appSetRG.groupid and appSeUG.siteid = a.siteid and appSeUG.active = a.active
		 inner join users appSeU on appSeU.userid = appSeUG.userid and appSeU.username = @UserName and appSeU.siteid = a.siteid and appSeU.active = a.active
		 inner join Settings s  on s.SettingId = appset.SettingID 
		 where a.ContextId = @ContextId and  a.siteid = @SiteId and a.active = 1   for xml PATH('applicationSetting'), type 
		 ) as 'applicationSettings',
		 		(
			Select gg.groupId,gg.groupName from ResourceGroups rr inner join Groups gg on rr.GroupId = gg.GroupId 
			where rr.ResourceId = a.ApplicationId
			and rr.Active = 1
			and gg.Active = 1 
			for xml PATH('group'), type
		) as 'groups' 
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
/****** Object:  StoredProcedure [dbo].[spgRolePermissions]    Script Date: 02/09/2015 11:50:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <02-Jul-2014>
-- Description:	<Return permission based on role>

-- =============================================
CREATE PROCEDURE [dbo].[spgRolePermissions] --1,1
(
	@RoleId INT,
	@Siteid INT
)	
	 
AS
BEGIN TRY
	BEGIN
		select p.PermissionId,p.PermissionName,p.PermissionDesc from RolePermissions rp inner join [Permissions] p on rp.PermissionId = p.PermissionId
											and rp.Active = 1 and p.Active = 1 And rp.SiteId = @Siteid And p.SiteId = @Siteid
										 inner join Roles r on r.RoleId = rp.RoleId and r.Active = 1 and r.SiteId = @Siteid 	
											where rp.RoleId = @RoleId
		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage
END CATCH

		
		
		--select * from RolePermissions where RoleId = 1 and SiteId =1
GO
/****** Object:  StoredProcedure [dbo].[spgRoleGroups]    Script Date: 02/09/2015 11:50:30 ******/
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
/****** Object:  StoredProcedure [dbo].[spgNames]    Script Date: 02/09/2015 11:50:31 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Gagan Dhamija>
-- Create date: <19-Sep-2014>
-- Description:	<Getting Names>

-- =============================================
CREATE  PROCEDURE [dbo].[spgNames] 
 (
 	@GraphId uniqueidentifier
 )
AS
BEGIN TRY
	BEGIN

	   select GraphName, Applications.ApplicationId as AppId, Applications.InternalName as AppName, Contexts.InternalName as ScopeName from Graphs
       join Applications on Applications.ApplicationId = Graphs.ApplicationId
       join Contexts on Contexts.ContextId = Applications.ContextId
       where UPPER(GraphId) = UPPER(@GraphId)
  		
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH;
GO
/****** Object:  StoredProcedure [dbo].[spgGroupUsers]    Script Date: 02/09/2015 11:50:31 ******/
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
BEGIN
		
	SELECT  
		U.UserId,
		U.UserFirstName ,
		U.UserLastName,
		U.UserEmail,
		U.UserPhone,
		U.UserDesc 
	FROM Users U INNER JOIN UserGroups UG ON UG.UserId = U.UserId
				 INNER JOIN Groups G ON G.GroupId = UG.GroupId
	WHERE UG.GroupId = @GroupId
		AND UG.Active=1 
		AND U.Active=1 
		AND G.Active =1   
		
END
GO
/****** Object:  StoredProcedure [dbo].[spgGroupUserIdGroupId]    Script Date: 02/09/2015 11:50:32 ******/
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
/****** Object:  StoredProcedure [dbo].[spgGroupUser]    Script Date: 02/09/2015 11:50:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================  
-- Author:  <Atul Srivastava>  
-- Create date: <01-JuL-2014>  
-- Description: <return all group that the user belongs to>  
-- =============================================  
CREATE PROCEDURE [dbo].[spgGroupUser]  
(  
 @UserName NVARCHAR(100),  
 @SiteId INT  
)  
    
AS  
 BEGIN  
    
  SELECT G.GroupId, G.GroupName,G.GroupDesc FROM Groups G  
   INNER JOIN UserGroups UG ON UG.GroupId = G.GroupId  
   INNER JOIN Users U ON U.UserId = UG.UserId   
  WHERE U.UserName = @UserName 
		AND UG.SiteId=@SiteId
		AND G.SiteId = @SiteId
		AND U.SiteId = @SiteId
		AND U.Active=1 
		AND G.Active=1 
		AND UG.Active=1 
 END
GO
/****** Object:  StoredProcedure [dbo].[spgGroupRoles]    Script Date: 02/09/2015 11:50:33 ******/
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
/****** Object:  StoredProcedure [dbo].[spgGraphMappingByUser]    Script Date: 02/09/2015 11:50:33 ******/
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
/****** Object:  StoredProcedure [dbo].[spgGraphByUser]    Script Date: 02/09/2015 11:50:34 ******/
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
BEGIN
	    
    WITH XMLNAMESPACES (DEFAULT 'http://www.iringtools.org/library') 
    select 
		g.ApplicationId as applicationId, 
		g.GraphId as graphId, 
		g.graphName as graphName, 
		g.SiteId as siteId, 
		g.Active as active,
		(
			select 
			distinct  
			p.PermissionId as permissionId, 
			p.SiteId as siteId, 
			p.PermissionName as permissionName, 
			p.PermissionDesc as permissionDesc, 
			p.Active as active from 
			permissions p
			inner join ResourceGroups rg on rg.siteid = g.siteid and rg.active = g.active and rg.resourceid = g.graphId 
			inner join Groups g on g.groupId = rg.groupId and g.siteid = g.siteid and g.active = g.active
			inner join grouproles gr on  gr.groupid = rg.groupid and gr.siteid = g.siteid and gr.active = g.active    
			inner join roles r on r.roleid = gr.roleid and r.siteid = g.siteid and r.active = g.active     
			inner join rolepermissions rp on rp.permissionid = p.permissionid and rp.roleid = r.roleid and rp.siteid = g.siteid and rp.active = g.active          
			inner join usergroups ug on ug.groupid = rg.groupid  and ug.siteid = g.siteid and ug.active = g.active
			inner join users u on u.userid = ug.userid and u.username = @UserName and u.siteid = g.siteid and u.active = g.active
			where  g.siteid = 1 and g.active = 1   for xml PATH('permission'), type 
		) as 'permissions',
		(
			Select gg.groupId,gg.groupName from ResourceGroups rr inner join Groups gg on rr.GroupId = gg.GroupId 
			where rr.ResourceId = g.graphId 
			and rr.Active = 1
			and gg.Active = 1 
			for xml PATH('group'), type
		)  as 'groups'
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
GO
/****** Object:  StoredProcedure [dbo].[spgGraphBinary]    Script Date: 02/09/2015 11:50:34 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Gagan Dhamija>
-- Create date: <18-Sep-2014>
-- Description:	<Selecting Graphs based on users>
-- ===========================================================
CREATE PROCEDURE [dbo].[spgGraphBinary] 
(
	@UserName varchar(100),
	@SiteId INT,
	@GraphId uniqueidentifier
)	 
AS
BEGIN TRY
	BEGIN
	    
    select g.graph as graphObject from Graphs g
    inner join ResourceGroups crg on crg.siteid = g.siteid and crg.active = g.active 
    inner join Groups cg on cg.groupId = crg.groupId and cg.siteid = g.siteid and cg.active = g.active
    inner join grouproles cgr on  cgr.groupid = crg.groupid and cgr.siteid = g.siteid and cgr.active = g.active    
    inner join roles cr on cr.roleid = cgr.roleid and cr.siteid = g.siteid and cr.active = g.active         
    inner join usergroups cug on cug.groupid = crg.groupid and cug.siteid = g.siteid and cug.active = g.active
    inner join users cu on cu.userid = cug.userid and cu.username = @UserName and cu.siteid = g.siteid and cu.active = g.active
    where UPPER(g.graphId) = UPPER(@GraphId) and g.siteid = @SiteId and g.active = 1
    Group BY g.GraphId, g.graphName, g.graph  
	
	END
END TRY

BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber,ERROR_MESSAGE() AS ErrorMessage;
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spgGraph]    Script Date: 02/09/2015 11:50:35 ******/
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
/****** Object:  StoredProcedure [dbo].[spgFolderByUser]    Script Date: 02/09/2015 11:50:36 ******/
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
      select 
      f.FolderId as folderId, 
      f.ParentFolderId as parentFolderId, 
      f.FolderName as folderName, 
      f.SiteId as siteId, 
      f.Active as active, 
      (
      select 
		  distinct  
		  p.PermissionId as permissionId, 
		  p.SiteId as siteId, 
		  p.PermissionName as permissionName, 
		  p.PermissionDesc as permissionDesc, 
		  p.Active as active 
      from permissions p
      inner join ResourceGroups rg on rg.siteid = f.siteid and rg.active = f.active and rg.resourceid = f.folderId 
      inner join Groups g on g.groupId = rg.groupId and g.siteid = f.siteid and g.active = f.active
      inner join grouproles gr on  gr.groupid = rg.groupid and gr.siteid = f.siteid and gr.active = f.active    
      inner join roles r on r.roleid = gr.roleid and r.siteid = f.siteid and r.active = f.active     
      inner join rolepermissions rp on rp.permissionid = p.permissionid and rp.roleid = r.roleid and rp.siteid = f.siteid and rp.active = f.active          
      inner join usergroups ug on ug.groupid = rg.groupid  and ug.siteid = f.siteid and ug.active = f.active
      inner join users u on u.userid = ug.userid and u.username = @UserName and u.siteid = f.siteid and u.active = f.active
      where f.parentFolderId = @FolderId and  f.siteid = @SiteId and f.active = 1   for xml PATH('permission'), type 
      ) as 'permissions',
	  (
		Select gg.groupId,gg.groupName from ResourceGroups rr inner join Groups gg on rr.GroupId = gg.GroupId 
		where rr.ResourceId = f.folderId 
		and rr.Active = 1
		and gg.Active = 1 
		for xml PATH('group'), type
	  )  as 'groups'
      
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
/****** Object:  StoredProcedure [dbo].[spgExchangesByUser]    Script Date: 02/09/2015 11:50:36 ******/
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
		) as 'permissions',
		(
		Select gg.groupId,gg.groupName from ResourceGroups rr inner join Groups gg on rr.GroupId = gg.GroupId 
		where rr.ResourceId = e.ExchangeId 
		and rr.Active = 1
		and gg.Active = 1 
		for xml PATH('group'), type
		)  as 'groups'
		
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
/****** Object:  StoredProcedure [dbo].[spgDataFilterByUser]    Script Date: 02/09/2015 11:50:37 ******/
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
	 
		WITH XMLNAMESPACES ( DEFAULT 'http://www.iringtools.org/data/filter' ) 
		select 
		df.ResourceId as resourceId, 
		df.SiteId as siteId, 
		df.Active as active,
		(
			select  
			efd.DFOrder as dfOrder,
			efd.OpenGroupCount as openGroupCount, 
			efd.PropertyName as propertyName, 
			efd.RelationalOperator as relationalOperator,
			(
			select value from DataFilterValues where DataFilterId = efd.DataFilterId for XML path(''),root('values'),type
			),
			efd.LogicalOperator as logicalOperator, 
			efd.CloseGroupCount as closeGroupCount,
			efd.IsCaseSensitive as isCaseSensitive 
			from DataFilters efd
			where  efd.IsExpression =  1
			and ResourceId = @ResourceId  Order By efd.DFOrder for xml PATH('expression'), type 
		) as 'expressions',

		(
			select 
			oedf.DFOrder as dfOrder,
			oedf.PropertyName as propertyName, 
			oedf.SortOrder as sortOrder
			from DataFilters oedf  where  oedf.IsExpression =  0
			and ResourceId = @ResourceId  Order By oedf.DFOrder  for xml PATH('orderExpression'), type 
		) as 'orderExpressions'
		,df.IsAdmin as isAdmin
		from  DataFilters df
		inner join  DataFiltersType dft on dft.DataFilterTypeId = df.DataFilterTypeId
		inner join ResourceGroups arg on arg.siteid = df.siteid and arg.active = df.active and arg.resourceid =  df.ResourceId
		inner join Groups ag on ag.groupId = arg.groupId and ag.siteid = df.siteid and ag.active = df.active
		inner join grouproles agr on  agr.groupid = arg.groupid and agr.siteid = df.siteid and agr.active = df.active    
		inner join roles ar on ar.roleid = agr.roleid and ar.siteid = df.siteid and ar.active = df.active         
		inner join usergroups aug on aug.groupid = arg.groupid and aug.siteid = df.siteid and aug.active = df.active
		inner join users au on au.userid = aug.userid and au.username = @UserName and au.siteid = df.siteid and au.active = df.active
		where df.ResourceId = @ResourceId and  
		--dft.DataFilterTypeName = 'AppData'  and 
		df.siteid = @SiteId and df.active = 1
		Group BY df.SiteId,df.Active, df.ResourceId,df.IsAdmin 
		for xml PATH('dataFilter'), ROOT('dataFilters'),type, ELEMENTS XSINIL 

	END
GO
/****** Object:  StoredProcedure [dbo].[spdUser]    Script Date: 02/09/2015 11:50:37 ******/
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
		
		--declare @aa int
		--set @aa = 10 / 0
		
		Declare @UserId Int 
		
		Select @UserId = UserId from users WHERE UserName = @UserName And Active = 1
		
		UPDATE Users SET Active = 0 WHERE UserId = @UserId
		
		UPDATE UserGroups SET Active = 0 WHERE UserId = @UserId
		
		Select '1'--'User deleted successfully!'
		
	END
END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spdContext]    Script Date: 02/09/2015 11:50:38 ******/
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
		@ContextId uniqueidentifier
)	
	 
AS
	BEGIN
	
	Update Contexts
		Set Active = 0
	Where ContextId = @ContextId 


	Update ResourceGroups
	Set   Active = 0
	Where ResourceId = @ContextId 
		 
	END
GO
/****** Object:  StoredProcedure [dbo].[spdCommodity]    Script Date: 02/09/2015 11:50:38 ******/
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
/****** Object:  StoredProcedure [dbo].[spdApplication]    Script Date: 02/09/2015 11:50:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Atul Srivastava>
-- Create date: <09-June-2014>
-- Description:	<Deleting Context >

-- =============================================
 CREATE PROCEDURE [dbo].[spdApplication] 
(
		@ApplicationId uniqueidentifier
)	
	 
AS
	BEGIN
	
	Update Applications
		Set Active = 0
	Where ApplicationId = @ApplicationId 


	Update ResourceGroups
	Set   Active = 0
	Where ResourceId = @ApplicationId
		 
	END
GO
/****** Object:  StoredProcedure [dbo].[spdRoles]    Script Date: 02/09/2015 11:50:39 ******/
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
	
	--declare @aa int
	--	set @aa = 10 / 0
		
		UPDATE Roles SET Active = 0 WHERE RoleId = @RoleId
		
		UPDATE GroupRoles SET Active = 0 WHERE RoleId = @RoleId
		
		UPDATE RolePermissions SET Active = 0 WHERE RoleId = @RoleId
		
		Select '1'--'Role deleted successfully!'
		
	END
END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spdPermissions]    Script Date: 02/09/2015 11:50:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Gagan Dhamija>
-- Create date: <09-Jul-2014>
-- Description:	<Deleting Permissions>

-- =============================================
CREATE PROCEDURE [dbo].[spdPermissions] 
(
	@PermissionId INT,
	@SiteId INT
)	
	 
AS
BEGIN TRY
	BEGIN
	
	 --   declare @aa int
		--set @aa = 10 / 0
		
		UPDATE [Permissions] SET Active = 0 WHERE PermissionId = @PermissionId
		
		UPDATE RolePermissions SET Active = 0 WHERE PermissionId = @PermissionId
		
		Select '1'--'Role deleted successfully!'
		
	END
END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spdGroups]    Script Date: 02/09/2015 11:50:40 ******/
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
		
		--declare @aa int
		--set @aa = 10 / 0
		
		UPDATE Groups SET Active = 0 WHERE GroupId= @GroupId
		
		UPDATE UserGroups SET Active = 0 WHERE GroupId = @GroupId
		
		UPDATE GroupRoles SET Active = 0 WHERE GroupId = @GroupId
 
		Select '1'--'Group deleted successfully!'
		
	END
END TRY

BEGIN CATCH
    SELECT 'Error occured at database: ' + ERROR_MESSAGE() 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[spdGraph]    Script Date: 02/09/2015 11:50:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Gagan Dhamija>
-- Create date: <17-Jul-2014>
-- Description:	<Deleting Graphs>
-- ===========================================================
CREATE PROCEDURE [dbo].[spdGraph] 
(
	@GraphId uniqueidentifier
)	 
AS

	BEGIN
	
	
	
	Update Graphs
		Set Active = 0
	Where GraphId = @GraphId 


	Update ResourceGroups
	Set   Active = 0
	Where ResourceId = @GraphId
							

	END
GO
/****** Object:  StoredProcedure [dbo].[spdFolder]    Script Date: 02/09/2015 11:50:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ===========================================================
-- Author:		<Deepak kumar>
-- Create date: <2014>
-- Description:	<Deleting folder>
-- ===========================================================
CREATE PROCEDURE [dbo].[spdFolder] 
(
	@FolderId uniqueidentifier
)	 
AS

	BEGIN
	
	
	
	Update Folders
		Set Active = 0
	Where FolderId = @FolderId


	Update ResourceGroups
	Set   Active = 0
	Where ResourceId = @FolderId
							

	END
GO
/****** Object:  StoredProcedure [dbo].[spdExchange]    Script Date: 02/09/2015 11:50:42 ******/
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
/****** Object:  Table [dbo].[PickList]    Script Date: 02/09/2015 11:50:42 ******/
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
/****** Object:  Table [dbo].[KeyProperty]    Script Date: 02/09/2015 11:50:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[KeyProperty](
	[DataObjectId] [uniqueidentifier] NOT NULL,
	[KeyPropertyName] [nvarchar](250) NULL
) ON [PRIMARY]
GO
INSERT [dbo].[KeyProperty] ([DataObjectId], [KeyPropertyName]) VALUES (N'b8d3d2ef-3b52-4ec5-b611-e46f05973702', N'TAG')
INSERT [dbo].[KeyProperty] ([DataObjectId], [KeyPropertyName]) VALUES (N'723cb279-9961-46ba-ae46-d4f765590dcf', N'tbl2_col1')
INSERT [dbo].[KeyProperty] ([DataObjectId], [KeyPropertyName]) VALUES (N'723cb279-9961-46ba-ae46-d4f765590dcf', N'tbl2_col2')
INSERT [dbo].[KeyProperty] ([DataObjectId], [KeyPropertyName]) VALUES (N'951f5dd1-e796-4f5b-bc04-07a111df3733', N'TAG')
INSERT [dbo].[KeyProperty] ([DataObjectId], [KeyPropertyName]) VALUES (N'3b0def40-a81e-443c-ad64-c74a2406ae6e', N'tbl1_col1')
INSERT [dbo].[KeyProperty] ([DataObjectId], [KeyPropertyName]) VALUES (N'3915d9dd-55bc-47a7-a081-2f4a252cf809', N'tbl2_col1')
INSERT [dbo].[KeyProperty] ([DataObjectId], [KeyPropertyName]) VALUES (N'3915d9dd-55bc-47a7-a081-2f4a252cf809', N'tbl2_col2')
INSERT [dbo].[KeyProperty] ([DataObjectId], [KeyPropertyName]) VALUES (N'8d1e8209-d806-436e-a36d-b9ce1315c59d', N'ID')
INSERT [dbo].[KeyProperty] ([DataObjectId], [KeyPropertyName]) VALUES (N'bcbae4fd-7beb-4695-9d6b-d96a863eac9e', N'INUM')
/****** Object:  Table [dbo].[DataRelationships]    Script Date: 02/09/2015 11:50:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DataRelationships](
	[RelationshipId] [uniqueidentifier] NOT NULL,
	[DataObjectID] [uniqueidentifier] NOT NULL,
	[RelationShipName] [nvarchar](50) NULL,
	[RelatedObjectName] [nvarchar](50) NULL,
	[RelationShipTYpe] [nvarchar](50) NULL,
 CONSTRAINT [PK_DataRelationships] PRIMARY KEY CLUSTERED 
(
	[RelationshipId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [dbo].[DataRelationships] ([RelationshipId], [DataObjectID], [RelationShipName], [RelatedObjectName], [RelationShipTYpe]) VALUES (N'f18b8b64-2200-4ada-8ba5-2d103d78e362', N'723cb279-9961-46ba-ae46-d4f765590dcf', N'tbl2_new', N'tbl1', N'OneToOne')
INSERT [dbo].[DataRelationships] ([RelationshipId], [DataObjectID], [RelationShipName], [RelatedObjectName], [RelationShipTYpe]) VALUES (N'638b0b8f-e443-45a3-a384-3bf76e9c43d0', N'723cb279-9961-46ba-ae46-d4f765590dcf', N'tbl2', N'tbl1', N'OneToOne')
/****** Object:  Table [dbo].[DataProperties]    Script Date: 02/09/2015 11:50:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DataProperties](
	[DataObjectId] [uniqueidentifier] NULL,
	[PickListId] [int] NULL,
	[ColumnName] [nvarchar](50) NULL,
	[PropertyName] [nvarchar](50) NULL,
	[DataType] [nvarchar](50) NULL,
	[DataLength] [int] NULL,
	[IsNullable] [nvarchar](10) NULL,
	[KeyType] [nvarchar](50) NULL,
	[ShowOnIndex] [nvarchar](50) NULL,
	[NumberOfDecimals] [int] NULL,
	[IsReadOnly] [nvarchar](10) NULL,
	[ShowOnSearch] [nvarchar](50) NULL,
	[IsHidden] [nvarchar](10) NULL,
	[Description] [nvarchar](250) NULL,
	[AliasDictionary] [nvarchar](50) NULL,
	[ReferenceType] [nvarchar](50) NULL,
	[IsVirtual] [nvarchar](10) NULL
) ON [PRIMARY]
GO
INSERT [dbo].[DataProperties] ([DataObjectId], [PickListId], [ColumnName], [PropertyName], [DataType], [DataLength], [IsNullable], [KeyType], [ShowOnIndex], [NumberOfDecimals], [IsReadOnly], [ShowOnSearch], [IsHidden], [Description], [AliasDictionary], [ReferenceType], [IsVirtual]) VALUES (N'b8d3d2ef-3b52-4ec5-b611-e46f05973702', NULL, N'tbl1_col1', N'tbl1_col1', N'Int32', 4, N'false', N'assigned', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[DataProperties] ([DataObjectId], [PickListId], [ColumnName], [PropertyName], [DataType], [DataLength], [IsNullable], [KeyType], [ShowOnIndex], [NumberOfDecimals], [IsReadOnly], [ShowOnSearch], [IsHidden], [Description], [AliasDictionary], [ReferenceType], [IsVirtual]) VALUES (N'b8d3d2ef-3b52-4ec5-b611-e46f05973702', NULL, N'tbl1_col2', N'tbl1_col2', N'String', 50, N'true', N'unassigned', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[DataProperties] ([DataObjectId], [PickListId], [ColumnName], [PropertyName], [DataType], [DataLength], [IsNullable], [KeyType], [ShowOnIndex], [NumberOfDecimals], [IsReadOnly], [ShowOnSearch], [IsHidden], [Description], [AliasDictionary], [ReferenceType], [IsVirtual]) VALUES (N'723cb279-9961-46ba-ae46-d4f765590dcf', NULL, N'TAG', N'TAG', N'String', 100, N'false', N'assigned', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[DataProperties] ([DataObjectId], [PickListId], [ColumnName], [PropertyName], [DataType], [DataLength], [IsNullable], [KeyType], [ShowOnIndex], [NumberOfDecimals], [IsReadOnly], [ShowOnSearch], [IsHidden], [Description], [AliasDictionary], [ReferenceType], [IsVirtual]) VALUES (N'723cb279-9961-46ba-ae46-d4f765590dcf', NULL, N'ID', N'ID', N'String', 10, N'true', N'unassigned', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[DataProperties] ([DataObjectId], [PickListId], [ColumnName], [PropertyName], [DataType], [DataLength], [IsNullable], [KeyType], [ShowOnIndex], [NumberOfDecimals], [IsReadOnly], [ShowOnSearch], [IsHidden], [Description], [AliasDictionary], [ReferenceType], [IsVirtual]) VALUES (N'723cb279-9961-46ba-ae46-d4f765590dcf', NULL, N'AREA', N'AREA', N'String', 10, N'true', N'unassigned', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[DataProperties] ([DataObjectId], [PickListId], [ColumnName], [PropertyName], [DataType], [DataLength], [IsNullable], [KeyType], [ShowOnIndex], [NumberOfDecimals], [IsReadOnly], [ShowOnSearch], [IsHidden], [Description], [AliasDictionary], [ReferenceType], [IsVirtual]) VALUES (N'723cb279-9961-46ba-ae46-d4f765590dcf', NULL, N'TRAINNUMBER', N'TRAINNUMBER', N'String', 5, N'true', N'unassigned', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[DataProperties] ([DataObjectId], [PickListId], [ColumnName], [PropertyName], [DataType], [DataLength], [IsNullable], [KeyType], [ShowOnIndex], [NumberOfDecimals], [IsReadOnly], [ShowOnSearch], [IsHidden], [Description], [AliasDictionary], [ReferenceType], [IsVirtual]) VALUES (N'723cb279-9961-46ba-ae46-d4f765590dcf', NULL, N'SPEC', N'SPEC', N'String', 10, N'true', N'unassigned', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[DataProperties] ([DataObjectId], [PickListId], [ColumnName], [PropertyName], [DataType], [DataLength], [IsNullable], [KeyType], [ShowOnIndex], [NumberOfDecimals], [IsReadOnly], [ShowOnSearch], [IsHidden], [Description], [AliasDictionary], [ReferenceType], [IsVirtual]) VALUES (N'723cb279-9961-46ba-ae46-d4f765590dcf', NULL, N'SYSTEM', N'SYSTEM', N'String', 20, N'true', N'unassigned', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[DataProperties] ([DataObjectId], [PickListId], [ColumnName], [PropertyName], [DataType], [DataLength], [IsNullable], [KeyType], [ShowOnIndex], [NumberOfDecimals], [IsReadOnly], [ShowOnSearch], [IsHidden], [Description], [AliasDictionary], [ReferenceType], [IsVirtual]) VALUES (N'723cb279-9961-46ba-ae46-d4f765590dcf', NULL, N'tbl2_col3', N'tbl2_col3', N'String', 50, N'true', N'unassigned', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[DataProperties] ([DataObjectId], [PickListId], [ColumnName], [PropertyName], [DataType], [DataLength], [IsNullable], [KeyType], [ShowOnIndex], [NumberOfDecimals], [IsReadOnly], [ShowOnSearch], [IsHidden], [Description], [AliasDictionary], [ReferenceType], [IsVirtual]) VALUES (N'723cb279-9961-46ba-ae46-d4f765590dcf', NULL, N'tbl2_col4', N'tbl2_col4', N'String', 50, N'true', N'unassigned', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[DataProperties] ([DataObjectId], [PickListId], [ColumnName], [PropertyName], [DataType], [DataLength], [IsNullable], [KeyType], [ShowOnIndex], [NumberOfDecimals], [IsReadOnly], [ShowOnSearch], [IsHidden], [Description], [AliasDictionary], [ReferenceType], [IsVirtual]) VALUES (N'b8d3d2ef-3b52-4ec5-b611-e46f05973702', NULL, N'tbl2_col4', N'tbl2_col4', N'String', 50, N'true', N'unassigned', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[DataProperties] ([DataObjectId], [PickListId], [ColumnName], [PropertyName], [DataType], [DataLength], [IsNullable], [KeyType], [ShowOnIndex], [NumberOfDecimals], [IsReadOnly], [ShowOnSearch], [IsHidden], [Description], [AliasDictionary], [ReferenceType], [IsVirtual]) VALUES (N'b8d3d2ef-3b52-4ec5-b611-e46f05973702', NULL, N'tbl2_col2', N'tbl2_col2', N'String', 50, N'true', N'assigned', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[DataProperties] ([DataObjectId], [PickListId], [ColumnName], [PropertyName], [DataType], [DataLength], [IsNullable], [KeyType], [ShowOnIndex], [NumberOfDecimals], [IsReadOnly], [ShowOnSearch], [IsHidden], [Description], [AliasDictionary], [ReferenceType], [IsVirtual]) VALUES (N'b8d3d2ef-3b52-4ec5-b611-e46f05973702', NULL, N'tbl2_col3', N'tbl2_col3', N'String', 50, N'true', N'unassigned', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[DataProperties] ([DataObjectId], [PickListId], [ColumnName], [PropertyName], [DataType], [DataLength], [IsNullable], [KeyType], [ShowOnIndex], [NumberOfDecimals], [IsReadOnly], [ShowOnSearch], [IsHidden], [Description], [AliasDictionary], [ReferenceType], [IsVirtual]) VALUES (N'b8d3d2ef-3b52-4ec5-b611-e46f05973702', NULL, N'tbl2_col4', N'tbl2_col4', N'String', 50, N'true', N'unassigned', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[DataProperties] ([DataObjectId], [PickListId], [ColumnName], [PropertyName], [DataType], [DataLength], [IsNullable], [KeyType], [ShowOnIndex], [NumberOfDecimals], [IsReadOnly], [ShowOnSearch], [IsHidden], [Description], [AliasDictionary], [ReferenceType], [IsVirtual]) VALUES (N'951f5dd1-e796-4f5b-bc04-07a111df3733', NULL, N'TAG', N'TAG', N'String', 100, N'false', N'assigned', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[DataProperties] ([DataObjectId], [PickListId], [ColumnName], [PropertyName], [DataType], [DataLength], [IsNullable], [KeyType], [ShowOnIndex], [NumberOfDecimals], [IsReadOnly], [ShowOnSearch], [IsHidden], [Description], [AliasDictionary], [ReferenceType], [IsVirtual]) VALUES (N'951f5dd1-e796-4f5b-bc04-07a111df3733', NULL, N'ID', N'ID', N'String', 10, N'true', N'unassigned', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[DataProperties] ([DataObjectId], [PickListId], [ColumnName], [PropertyName], [DataType], [DataLength], [IsNullable], [KeyType], [ShowOnIndex], [NumberOfDecimals], [IsReadOnly], [ShowOnSearch], [IsHidden], [Description], [AliasDictionary], [ReferenceType], [IsVirtual]) VALUES (N'951f5dd1-e796-4f5b-bc04-07a111df3733', NULL, N'AREA', N'AREA', N'String', 10, N'true', N'unassigned', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[DataProperties] ([DataObjectId], [PickListId], [ColumnName], [PropertyName], [DataType], [DataLength], [IsNullable], [KeyType], [ShowOnIndex], [NumberOfDecimals], [IsReadOnly], [ShowOnSearch], [IsHidden], [Description], [AliasDictionary], [ReferenceType], [IsVirtual]) VALUES (N'951f5dd1-e796-4f5b-bc04-07a111df3733', NULL, N'TRAINNUMBER', N'TRAINNUMBER', N'String', 5, N'true', N'unassigned', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[DataProperties] ([DataObjectId], [PickListId], [ColumnName], [PropertyName], [DataType], [DataLength], [IsNullable], [KeyType], [ShowOnIndex], [NumberOfDecimals], [IsReadOnly], [ShowOnSearch], [IsHidden], [Description], [AliasDictionary], [ReferenceType], [IsVirtual]) VALUES (N'951f5dd1-e796-4f5b-bc04-07a111df3733', NULL, N'SPEC', N'SPEC', N'String', 10, N'true', N'unassigned', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[DataProperties] ([DataObjectId], [PickListId], [ColumnName], [PropertyName], [DataType], [DataLength], [IsNullable], [KeyType], [ShowOnIndex], [NumberOfDecimals], [IsReadOnly], [ShowOnSearch], [IsHidden], [Description], [AliasDictionary], [ReferenceType], [IsVirtual]) VALUES (N'951f5dd1-e796-4f5b-bc04-07a111df3733', NULL, N'SYSTEM', N'SYSTEM', N'String', 20, N'true', N'unassigned', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[DataProperties] ([DataObjectId], [PickListId], [ColumnName], [PropertyName], [DataType], [DataLength], [IsNullable], [KeyType], [ShowOnIndex], [NumberOfDecimals], [IsReadOnly], [ShowOnSearch], [IsHidden], [Description], [AliasDictionary], [ReferenceType], [IsVirtual]) VALUES (N'3b0def40-a81e-443c-ad64-c74a2406ae6e', NULL, N'tbl1_col1', N'tbl1_col1', N'Int32', 4, N'false', N'assigned', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[DataProperties] ([DataObjectId], [PickListId], [ColumnName], [PropertyName], [DataType], [DataLength], [IsNullable], [KeyType], [ShowOnIndex], [NumberOfDecimals], [IsReadOnly], [ShowOnSearch], [IsHidden], [Description], [AliasDictionary], [ReferenceType], [IsVirtual]) VALUES (N'3b0def40-a81e-443c-ad64-c74a2406ae6e', NULL, N'tbl1_col2', N'tbl1_col2', N'String', 50, N'true', N'unassigned', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[DataProperties] ([DataObjectId], [PickListId], [ColumnName], [PropertyName], [DataType], [DataLength], [IsNullable], [KeyType], [ShowOnIndex], [NumberOfDecimals], [IsReadOnly], [ShowOnSearch], [IsHidden], [Description], [AliasDictionary], [ReferenceType], [IsVirtual]) VALUES (N'3915d9dd-55bc-47a7-a081-2f4a252cf809', NULL, N'tbl2_col1', N'tbl2_col1', N'Int32', 4, N'false', N'assigned', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[DataProperties] ([DataObjectId], [PickListId], [ColumnName], [PropertyName], [DataType], [DataLength], [IsNullable], [KeyType], [ShowOnIndex], [NumberOfDecimals], [IsReadOnly], [ShowOnSearch], [IsHidden], [Description], [AliasDictionary], [ReferenceType], [IsVirtual]) VALUES (N'3915d9dd-55bc-47a7-a081-2f4a252cf809', NULL, N'tbl2_col2', N'tbl2_col2', N'String', 50, N'true', N'assigned', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[DataProperties] ([DataObjectId], [PickListId], [ColumnName], [PropertyName], [DataType], [DataLength], [IsNullable], [KeyType], [ShowOnIndex], [NumberOfDecimals], [IsReadOnly], [ShowOnSearch], [IsHidden], [Description], [AliasDictionary], [ReferenceType], [IsVirtual]) VALUES (N'3915d9dd-55bc-47a7-a081-2f4a252cf809', NULL, N'tbl2_col3', N'tbl2_col3', N'String', 50, N'true', N'unassigned', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[DataProperties] ([DataObjectId], [PickListId], [ColumnName], [PropertyName], [DataType], [DataLength], [IsNullable], [KeyType], [ShowOnIndex], [NumberOfDecimals], [IsReadOnly], [ShowOnSearch], [IsHidden], [Description], [AliasDictionary], [ReferenceType], [IsVirtual]) VALUES (N'8d1e8209-d806-436e-a36d-b9ce1315c59d', NULL, N'ID', N'ID', N'String', 10, N'true', N'assigned', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[DataProperties] ([DataObjectId], [PickListId], [ColumnName], [PropertyName], [DataType], [DataLength], [IsNullable], [KeyType], [ShowOnIndex], [NumberOfDecimals], [IsReadOnly], [ShowOnSearch], [IsHidden], [Description], [AliasDictionary], [ReferenceType], [IsVirtual]) VALUES (N'8d1e8209-d806-436e-a36d-b9ce1315c59d', NULL, N'AREA', N'AREA', N'String', 10, N'true', N'unassigned', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[DataProperties] ([DataObjectId], [PickListId], [ColumnName], [PropertyName], [DataType], [DataLength], [IsNullable], [KeyType], [ShowOnIndex], [NumberOfDecimals], [IsReadOnly], [ShowOnSearch], [IsHidden], [Description], [AliasDictionary], [ReferenceType], [IsVirtual]) VALUES (N'bcbae4fd-7beb-4695-9d6b-d96a863eac9e', NULL, N'INUM', N'INUM', N'String', 10, N'true', N'assigned', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[DataProperties] ([DataObjectId], [PickListId], [ColumnName], [PropertyName], [DataType], [DataLength], [IsNullable], [KeyType], [ShowOnIndex], [NumberOfDecimals], [IsReadOnly], [ShowOnSearch], [IsHidden], [Description], [AliasDictionary], [ReferenceType], [IsVirtual]) VALUES (N'bcbae4fd-7beb-4695-9d6b-d96a863eac9e', NULL, N'IAREA', N'IAREA', N'String', 10, N'true', N'unassigned', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
/****** Object:  Table [dbo].[PropertyMap]    Script Date: 02/09/2015 11:50:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PropertyMap](
	[RelationshipID] [uniqueidentifier] NULL,
	[DataPropertyName] [nvarchar](50) NULL,
	[RelatedPropertyName] [nvarchar](50) NULL
) ON [PRIMARY]
GO
INSERT [dbo].[PropertyMap] ([RelationshipID], [DataPropertyName], [RelatedPropertyName]) VALUES (N'638b0b8f-e443-45a3-a384-3bf76e9c43d0', N'tbl2_col1', N'tbl1_col2')
INSERT [dbo].[PropertyMap] ([RelationshipID], [DataPropertyName], [RelatedPropertyName]) VALUES (N'f18b8b64-2200-4ada-8ba5-2d103d78e362', N'tbl2_col2', N'tbl1_col2')
INSERT [dbo].[PropertyMap] ([RelationshipID], [DataPropertyName], [RelatedPropertyName]) VALUES (N'f18b8b64-2200-4ada-8ba5-2d103d78e362', N'tbl2_col3', N'tbl1_col1')
/****** Object:  StoredProcedure [dbo].[spgDataDictionary]    Script Date: 02/09/2015 11:50:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Deepak Kumar>
-- Create date: <23-Sep-2014>
-- Description:	<gettting database dictionary>
-- =============================================
CREATE PROCEDURE [dbo].[spgDataDictionary] --'3E1E418D-FA51-403B-BB7B-0CADE34283A7' 
(
	@ApplicationID uniqueidentifier
)	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
			--WITH XMLNAMESPACES (DEFAULT 'http://www.iringtools.org/library' ) 	
			SELECT
			(
				SELECT
				   [tableName]
				  ,[objectNamespace]
				  ,[objectName]
				  ,[keyDelimeter]
				  ,(
					 SELECT 
					   KP.[keyPropertyName]
					   FROM [KeyProperty] KP Where KP.DataObjectId = DO.DataObjectId for xml path('keyProperty'),type
				   ) as 'keyProperties'
				  ,(
					  SELECT 
					   DP.[columnName]
					  ,DP.[propertyName]
					  ,DP.[dataType]
					  ,DP.[dataLength]
					  ,DP.isNullable
					  ,DP.[keyType]
					  --,DP.[showOnIndex]
					  --,DP.[numberOfDecimals]
					  --,DP.[isReadOnly]
					  --,DP.[showOnSearch]
					  --,DP.[isHidden]
					  --,DP.[description]
					  --,DP.[aliasDictionary]
					  --,DP.[referenceType]
					  --,DP.[isVirtual]
					FROM [DataProperties] DP Where DP.DataObjectId = DO.DataObjectId For xml path('dataProperty'),type
				   ) as 'dataProperties'
				  ,(
						Select
						(
							SELECT 
							PM.[dataPropertyName]
							,PM.[relatedPropertyName]
							FROM [PropertyMap] PM Where PM.RelationshipID = DR.RelationshipId
													--order by PM.RelationshipID desc
													For xml path('propertyMap'),type
						)as 'propertyMaps' 
						,DR.[relatedObjectName]
						,DR.[relationshipName]
						,DR.[relationshipType]
						FROM [DataRelationships] DR Where DR.DataObjectId = DO.DataObjectId For xml path('dataRelationship'),type
				   ) as 'dataRelationships'
			      
				  ,[description]
				  ,(
					
						select  
						efd.DFOrder as dfOrder,
						efd.OpenCount as openCount, 
						efd.LogicalOperator as logicalOperator, 
						efd.PropertyName as propertyName, 
						efd.RelationalOperator as relationalOperator,
						(
						select value from DataFilterValues where DataFilterId = efd.DataFilterId for XML path(''),root('values'),type
						),
						efd.CloseCount as closeCount 
						from DataFilters efd 
						where 
						efd.IsExpression = 1 
						and efd.ResourceId = DO.DataObjectId  Order By efd.DFOrder for xml PATH('expression'),root('expressions'),type

						
				   )as 'dataFilter'
				   ,(
					
     				    select 
						oedf.DFOrder as dfOrder,
						oedf.PropertyName as propertyName, 
						oedf.Sort as sort
						from DataFilters oedf  where  oedf.IsExpression =  0 
						and oedf.ResourceId = DO.DataObjectId  Order By oedf.DFOrder  for xml PATH('orderExpression'),root('orderExpressions'), type 
						
				   )as 'dataFilter'
			   FROM [DataObjects] DO Where DO.DictionaryId = D.Dictionaryid
									 --Order by DO.ObjectName 
									 for xml path('dataObject'),type

			 ) as 'dataObjects'
			,(  
				--This needs to confirm
				SELECT 
				 PL.[pickListName]
				,PL.[description]
				,PL.[valueProperyIndex]
				,PL.[tableName]
				FROM [PickList] PL Where PL.DictionaryId = D.Dictionaryid for xml path('picklist'),type
			 ) as 'picklists'
			,D.[enableSearch]
			,D.[enableSummary]
			,D.[dataVersion]
			 	 
			FROM [Dictionary] D
			where ApplicationID = @ApplicationID--'3E1E418D-FA51-403B-BB7B-0CADE34283A7' 
			and [IsDBDictionary] = 0
			for xml path('dataDictionary'),type, ELEMENTS XSINIL 
			
--Just for testing (below is the actual xml which is generated and stored in xml file in app_data folder) 			
/*declare @xml xml =  '<databaseDictionary xmlns="http://www.iringtools.org/library">
  <dataObjects>
    <dataObject>
      <tableName>tbl1</tableName>
      <objectNamespace>org.iringtools.adapter.datalayer.proj_scope1IN.app1IN</objectNamespace>
      <objectName>tbl1</objectName>
      <keyProperties>
        <keyProperty>
          <keyPropertyName>tbl1_col1</keyPropertyName>
        </keyProperty>
      </keyProperties>
      <dataProperties>
        <dataProperty>
          <columnName>tbl1_col1</columnName>
          <propertyName>tbl1_col1</propertyName>
          <dataType>Int32</dataType>
          <dataLength>4</dataLength>
          <isNullable>false</isNullable>
          <keyType>assigned</keyType>
        </dataProperty>
        <dataProperty>
          <columnName>tbl1_col2</columnName>
          <propertyName>tbl1_col2</propertyName>
          <dataType>String</dataType>
          <dataLength>50</dataLength>
          <isNullable>true</isNullable>
          <keyType>unassigned</keyType>
        </dataProperty>
      </dataProperties>
      
    </dataObject>
    <dataObject>
      <tableName>tbl2</tableName>
      <objectNamespace>org.iringtools.adapter.datalayer.proj_scope1IN.app1IN</objectNamespace>
      <objectName>tbl2_ojbName</objectName>
      <keyDelimeter>Deep key delimiter</keyDelimeter>
      <keyProperties>
        <keyProperty>
          <keyPropertyName>tbl2_col1</keyPropertyName>
        </keyProperty>
        <keyProperty>
          <keyPropertyName>tbl2_col2</keyPropertyName>
        </keyProperty>
      </keyProperties>
      <dataProperties>
        <dataProperty>
          <columnName>tbl2_col1</columnName>
          <propertyName>tbl2_col1</propertyName>
          <dataType>Int32</dataType>
          <dataLength>4</dataLength>
          <isNullable>false</isNullable>
          <keyType>assigned</keyType>
        </dataProperty>
        <dataProperty>
          <columnName>tbl2_col2</columnName>
          <propertyName>tbl2_col2</propertyName>
          <dataType>String</dataType>
          <dataLength>50</dataLength>
          <isNullable>true</isNullable>
          <keyType>assigned</keyType>
        </dataProperty>
        <dataProperty>
          <columnName>tbl2_col3</columnName>
          <propertyName>tbl2_col3</propertyName>
          <dataType>String</dataType>
          <dataLength>50</dataLength>
          <isNullable>true</isNullable>
          <keyType>unassigned</keyType>
        </dataProperty>
        <dataProperty>
          <columnName>tbl2_col4</columnName>
          <propertyName>tbl2_col4</propertyName>
          <dataType>String</dataType>
          <dataLength>50</dataLength>
          <isNullable>true</isNullable>
          <keyType>unassigned</keyType>
        </dataProperty>
      </dataProperties>
      <dataRelationships>
        <dataRelationship>
          <propertyMaps>
            <propertyMap>
              <dataPropertyName>tbl2_col1</dataPropertyName>
              <relatedPropertyName>tbl1_col2</relatedPropertyName>
            </propertyMap>
          </propertyMaps>
          <relatedObjectName>tbl1</relatedObjectName>
          <relationshipName>tbl2</relationshipName>
          <relationshipType>OneToOne</relationshipType>
        </dataRelationship>
        <dataRelationship>
          <propertyMaps>
            <propertyMap>
              <dataPropertyName>tbl2_col2</dataPropertyName>
              <relatedPropertyName>tbl1_col2</relatedPropertyName>
            </propertyMap>
            <propertyMap>
              <dataPropertyName>tbl2_col3</dataPropertyName>
              <relatedPropertyName>tbl1_col1</relatedPropertyName>
            </propertyMap>
          </propertyMaps>
          <relatedObjectName>tbl1</relatedObjectName>
          <relationshipName>tbl2_new</relationshipName>
          <relationshipType>OneToOne</relationshipType>
        </dataRelationship>
      </dataRelationships>
      <description>desc</description>
    </dataObject>
  </dataObjects>
 
  <enableSearch>false</enableSearch>
  <enableSummary>false</enableSummary>
 
  <provider>MsSql2008ss</provider>
  <connectionString>U/0CMsal+QOwOAtVk713zwgdwkCEiNxlzkjt90Ap19vj/M10piCIXHiUBGcwOtwXCXkyjqs9VdiCvagat/TWfQqhv8QXniyih8hBgInMAuaLTiwzrLPyoso+pv9zLlVF</connectionString>
  <schemaName>dbo</schemaName>
</databaseDictionary>'

Select @xml
*/

END
GO
/****** Object:  StoredProcedure [dbo].[spgDataBaseDictionary]    Script Date: 02/09/2015 11:50:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Deepak Kumar>
-- Create date: <23-Sep-2014>
-- Description:	<gettting database dictionary>
-- =============================================
CREATE PROCEDURE [dbo].[spgDataBaseDictionary] --'3E1E418D-FA51-403B-BB7B-0CADE34283A7' 
(
	@ApplicationID uniqueidentifier
)	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
			WITH XMLNAMESPACES (DEFAULT 'http://www.iringtools.org/library' ) 	
			SELECT
			(
				SELECT
				   [tableName]
				  ,[objectNamespace]
				  ,[objectName]
				  ,[keyDelimeter]
				  ,(
					 SELECT 
					   KP.[keyPropertyName]
					   FROM [KeyProperty] KP Where KP.DataObjectId = DO.DataObjectId for xml path('keyProperty'),type
				   ) as 'keyProperties'
				  ,(
					  SELECT 
					   DP.[columnName]
					  ,DP.[propertyName]
					  ,DP.[dataType]
					  ,DP.[dataLength]
					  ,DP.isNullable
					  ,DP.[keyType]
					  --,DP.[showOnIndex]
					  --,DP.[numberOfDecimals]
					  --,DP.[isReadOnly]
					  --,DP.[showOnSearch]
					  --,DP.[isHidden]
					  --,DP.[description]
					  --,DP.[aliasDictionary]
					  --,DP.[referenceType]
					  --,DP.[isVirtual]
					FROM [DataProperties] DP Where DP.DataObjectId = DO.DataObjectId For xml path('dataProperty'),type
				   ) as 'dataProperties'
				  ,(
						Select
						(
							SELECT 
							PM.[dataPropertyName]
							,PM.[relatedPropertyName]
							FROM [PropertyMap] PM Where PM.RelationshipID = DR.RelationshipId
													--order by PM.RelationshipID desc
													For xml path('propertyMap'),type
						)as 'propertyMaps' 
						,DR.[relatedObjectName]
						,DR.[relationshipName]
						,DR.[relationshipType]
						FROM [DataRelationships] DR Where DR.DataObjectId = DO.DataObjectId For xml path('dataRelationship'),type
				   ) as 'dataRelationships'
			      
				  ,[description]
			   FROM [DataObjects] DO Where DO.DictionaryId = D.Dictionaryid
									 --Order by DO.ObjectName 
									 for xml path('dataObject'),type

			 ) as 'dataObjects'
			,(  
				--This needs to confirm
				SELECT 
				 PL.[pickListName]
				,PL.[description]
				,PL.[valueProperyIndex]
				,PL.[tableName]
				FROM [PickList] PL Where PL.DictionaryId = D.Dictionaryid for xml path('picklist'),type
			 ) as 'picklists'
			,D.[enableSearch]
			,D.[enableSummary]
			,D.[dataVersion]
			,D.[provider]
			,D.[connectionString]
			,D.[schemaName]
			 	 
			FROM [Dictionary] D
			where ApplicationID = @ApplicationID--'3E1E418D-FA51-403B-BB7B-0CADE34283A7' 
			and [IsDBDictionary] = 1
			for xml path('databaseDictionary'),type, ELEMENTS XSINIL 
			
--Just for testing (below is the actual xml which is generated and stored in xml file in app_data folder) 			
/*declare @xml xml =  '<databaseDictionary xmlns="http://www.iringtools.org/library">
  <dataObjects>
    <dataObject>
      <tableName>tbl1</tableName>
      <objectNamespace>org.iringtools.adapter.datalayer.proj_scope1IN.app1IN</objectNamespace>
      <objectName>tbl1</objectName>
      <keyProperties>
        <keyProperty>
          <keyPropertyName>tbl1_col1</keyPropertyName>
        </keyProperty>
      </keyProperties>
      <dataProperties>
        <dataProperty>
          <columnName>tbl1_col1</columnName>
          <propertyName>tbl1_col1</propertyName>
          <dataType>Int32</dataType>
          <dataLength>4</dataLength>
          <isNullable>false</isNullable>
          <keyType>assigned</keyType>
        </dataProperty>
        <dataProperty>
          <columnName>tbl1_col2</columnName>
          <propertyName>tbl1_col2</propertyName>
          <dataType>String</dataType>
          <dataLength>50</dataLength>
          <isNullable>true</isNullable>
          <keyType>unassigned</keyType>
        </dataProperty>
      </dataProperties>
      
    </dataObject>
    <dataObject>
      <tableName>tbl2</tableName>
      <objectNamespace>org.iringtools.adapter.datalayer.proj_scope1IN.app1IN</objectNamespace>
      <objectName>tbl2_ojbName</objectName>
      <keyDelimeter>Deep key delimiter</keyDelimeter>
      <keyProperties>
        <keyProperty>
          <keyPropertyName>tbl2_col1</keyPropertyName>
        </keyProperty>
        <keyProperty>
          <keyPropertyName>tbl2_col2</keyPropertyName>
        </keyProperty>
      </keyProperties>
      <dataProperties>
        <dataProperty>
          <columnName>tbl2_col1</columnName>
          <propertyName>tbl2_col1</propertyName>
          <dataType>Int32</dataType>
          <dataLength>4</dataLength>
          <isNullable>false</isNullable>
          <keyType>assigned</keyType>
        </dataProperty>
        <dataProperty>
          <columnName>tbl2_col2</columnName>
          <propertyName>tbl2_col2</propertyName>
          <dataType>String</dataType>
          <dataLength>50</dataLength>
          <isNullable>true</isNullable>
          <keyType>assigned</keyType>
        </dataProperty>
        <dataProperty>
          <columnName>tbl2_col3</columnName>
          <propertyName>tbl2_col3</propertyName>
          <dataType>String</dataType>
          <dataLength>50</dataLength>
          <isNullable>true</isNullable>
          <keyType>unassigned</keyType>
        </dataProperty>
        <dataProperty>
          <columnName>tbl2_col4</columnName>
          <propertyName>tbl2_col4</propertyName>
          <dataType>String</dataType>
          <dataLength>50</dataLength>
          <isNullable>true</isNullable>
          <keyType>unassigned</keyType>
        </dataProperty>
      </dataProperties>
      <dataRelationships>
        <dataRelationship>
          <propertyMaps>
            <propertyMap>
              <dataPropertyName>tbl2_col1</dataPropertyName>
              <relatedPropertyName>tbl1_col2</relatedPropertyName>
            </propertyMap>
          </propertyMaps>
          <relatedObjectName>tbl1</relatedObjectName>
          <relationshipName>tbl2</relationshipName>
          <relationshipType>OneToOne</relationshipType>
        </dataRelationship>
        <dataRelationship>
          <propertyMaps>
            <propertyMap>
              <dataPropertyName>tbl2_col2</dataPropertyName>
              <relatedPropertyName>tbl1_col2</relatedPropertyName>
            </propertyMap>
            <propertyMap>
              <dataPropertyName>tbl2_col3</dataPropertyName>
              <relatedPropertyName>tbl1_col1</relatedPropertyName>
            </propertyMap>
          </propertyMaps>
          <relatedObjectName>tbl1</relatedObjectName>
          <relationshipName>tbl2_new</relationshipName>
          <relationshipType>OneToOne</relationshipType>
        </dataRelationship>
      </dataRelationships>
      <description>desc</description>
    </dataObject>
  </dataObjects>
 
  <enableSearch>false</enableSearch>
  <enableSummary>false</enableSummary>
 
  <provider>MsSql2008ss</provider>
  <connectionString>U/0CMsal+QOwOAtVk713zwgdwkCEiNxlzkjt90Ap19vj/M10piCIXHiUBGcwOtwXCXkyjqs9VdiCvagat/TWfQqhv8QXniyih8hBgInMAuaLTiwzrLPyoso+pv9zLlVF</connectionString>
  <schemaName>dbo</schemaName>
</databaseDictionary>'

Select @xml
*/

END
GO
/****** Object:  Default [DF_Sites_Active]    Script Date: 02/09/2015 11:49:45 ******/
ALTER TABLE [dbo].[Sites] ADD  CONSTRAINT [DF_Sites_Active]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF__ScheduleE__Sched__28B808A7]    Script Date: 02/09/2015 11:49:45 ******/
ALTER TABLE [dbo].[ScheduleExchange] ADD  DEFAULT (newsequentialid()) FOR [Schedule_Exchange_Id]
GO
/****** Object:  Default [DF__ScheduleC__Sched__23F3538A]    Script Date: 02/09/2015 11:49:45 ******/
ALTER TABLE [dbo].[ScheduleCache] ADD  DEFAULT (newsequentialid()) FOR [Schedule_Cache_Id]
GO
/****** Object:  Default [DF_MimeType_Active]    Script Date: 02/09/2015 11:49:45 ******/
ALTER TABLE [dbo].[MimeType] ADD  CONSTRAINT [DF_MimeType_Active]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF_DataFilters_DataFilterId]    Script Date: 02/09/2015 11:49:45 ******/
ALTER TABLE [dbo].[DataFilters] ADD  CONSTRAINT [DF_DataFilters_DataFilterId]  DEFAULT (newid()) FOR [DataFilterId]
GO
/****** Object:  Default [DF_DataFilters_Active]    Script Date: 02/09/2015 11:49:45 ******/
ALTER TABLE [dbo].[DataFilters] ADD  CONSTRAINT [DF_DataFilters_Active]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF_DataFilters_IsAdmin]    Script Date: 02/09/2015 11:49:45 ******/
ALTER TABLE [dbo].[DataFilters] ADD  CONSTRAINT [DF_DataFilters_IsAdmin]  DEFAULT ((0)) FOR [IsAdmin]
GO
/****** Object:  Default [DF_DataFilters_IsCaseSensitive]    Script Date: 02/09/2015 11:49:45 ******/
ALTER TABLE [dbo].[DataFilters] ADD  CONSTRAINT [DF_DataFilters_IsCaseSensitive]  DEFAULT ((0)) FOR [IsCaseSensitive]
GO
/****** Object:  Default [DF_Contexts_ContextId]    Script Date: 02/09/2015 11:49:45 ******/
ALTER TABLE [dbo].[Contexts] ADD  CONSTRAINT [DF_Contexts_ContextId]  DEFAULT (newid()) FOR [ContextId]
GO
/****** Object:  Default [DF__Contexts__Active__46E78A0C]    Script Date: 02/09/2015 11:49:45 ******/
ALTER TABLE [dbo].[Contexts] ADD  CONSTRAINT [DF__Contexts__Active__46E78A0C]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF__Users__Active__0EA330E9]    Script Date: 02/09/2015 11:49:59 ******/
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF__Users__Active__0EA330E9]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF_Applications_ApplicationId]    Script Date: 02/09/2015 11:49:59 ******/
ALTER TABLE [dbo].[Applications] ADD  CONSTRAINT [DF_Applications_ApplicationId]  DEFAULT (newid()) FOR [ApplicationId]
GO
/****** Object:  Default [DF__Applicati__Activ__4316F928]    Script Date: 02/09/2015 11:49:59 ******/
ALTER TABLE [dbo].[Applications] ADD  CONSTRAINT [DF__Applicati__Activ__4316F928]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF_Folders_FolderId]    Script Date: 02/09/2015 11:49:59 ******/
ALTER TABLE [dbo].[Folders] ADD  CONSTRAINT [DF_Folders_FolderId]  DEFAULT (newid()) FOR [FolderId]
GO
/****** Object:  Default [DF__Folders__Active__49C3F6B7]    Script Date: 02/09/2015 11:49:59 ******/
ALTER TABLE [dbo].[Folders] ADD  CONSTRAINT [DF__Folders__Active__49C3F6B7]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF__Commodity__Activ__42E1EEFE]    Script Date: 02/09/2015 11:49:59 ******/
ALTER TABLE [dbo].[Commodity] ADD  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF_Groups_Active]    Script Date: 02/09/2015 11:49:59 ******/
ALTER TABLE [dbo].[Groups] ADD  CONSTRAINT [DF_Groups_Active]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF__Permissio__Activ__1B0907CE]    Script Date: 02/09/2015 11:50:02 ******/
ALTER TABLE [dbo].[Permissions] ADD  CONSTRAINT [DF__Permissio__Activ__1B0907CE]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF__Roles__Active__15502E78]    Script Date: 02/09/2015 11:50:05 ******/
ALTER TABLE [dbo].[Roles] ADD  CONSTRAINT [DF__Roles__Active__15502E78]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF__RolePermi__Activ__14270015]    Script Date: 02/09/2015 11:50:05 ******/
ALTER TABLE [dbo].[RolePermissions] ADD  CONSTRAINT [DF__RolePermi__Activ__14270015]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF__GroupRole__Activ__4CA06362]    Script Date: 02/09/2015 11:50:11 ******/
ALTER TABLE [dbo].[GroupRoles] ADD  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF_Graphs_GraphId]    Script Date: 02/09/2015 11:50:11 ******/
ALTER TABLE [dbo].[Graphs] ADD  CONSTRAINT [DF_Graphs_GraphId]  DEFAULT (newid()) FOR [GraphId]
GO
/****** Object:  Default [DF__Graphs__Active__753864A1]    Script Date: 02/09/2015 11:50:11 ******/
ALTER TABLE [dbo].[Graphs] ADD  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF_Graphs_ExchangeVisible]    Script Date: 02/09/2015 11:50:11 ******/
ALTER TABLE [dbo].[Graphs] ADD  CONSTRAINT [DF_Graphs_ExchangeVisible]  DEFAULT ((1)) FOR [ExchangeVisible]
GO
/****** Object:  Default [DF__ResourceG__Activ__4F7CD00D]    Script Date: 02/09/2015 11:50:11 ******/
ALTER TABLE [dbo].[ResourceGroups] ADD  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF_ResourceGroups_ResourceTypeId]    Script Date: 02/09/2015 11:50:11 ******/
ALTER TABLE [dbo].[ResourceGroups] ADD  CONSTRAINT [DF_ResourceGroups_ResourceTypeId]  DEFAULT ((1)) FOR [ResourceTypeId]
GO
/****** Object:  Default [DF__Exchanges__Activ__60A75C0F]    Script Date: 02/09/2015 11:50:11 ******/
ALTER TABLE [dbo].[Exchanges] ADD  CONSTRAINT [DF__Exchanges__Activ__60A75C0F]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF_Dictionary_Dictionaryid]    Script Date: 02/09/2015 11:50:11 ******/
ALTER TABLE [dbo].[Dictionary] ADD  CONSTRAINT [DF_Dictionary_Dictionaryid]  DEFAULT (newid()) FOR [Dictionaryid]
GO
/****** Object:  Default [DF_Dictionary_IsDBDictionary]    Script Date: 02/09/2015 11:50:11 ******/
ALTER TABLE [dbo].[Dictionary] ADD  CONSTRAINT [DF_Dictionary_IsDBDictionary]  DEFAULT ((1)) FOR [IsDBDictionary]
GO
/****** Object:  Default [DF__UserGroup__Activ__267ABA7A]    Script Date: 02/09/2015 11:50:11 ******/
ALTER TABLE [dbo].[UserGroups] ADD  CONSTRAINT [DF__UserGroup__Activ__267ABA7A]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF_DataObjects_DataObjectId]    Script Date: 02/09/2015 11:50:23 ******/
ALTER TABLE [dbo].[DataObjects] ADD  CONSTRAINT [DF_DataObjects_DataObjectId]  DEFAULT (newid()) FOR [DataObjectId]
GO
/****** Object:  Default [DF_DataRelationships_RelationshipId]    Script Date: 02/09/2015 11:50:42 ******/
ALTER TABLE [dbo].[DataRelationships] ADD  CONSTRAINT [DF_DataRelationships_RelationshipId]  DEFAULT (newid()) FOR [RelationshipId]
GO
/****** Object:  ForeignKey [FK__Users__SiteId__6754599E]    Script Date: 02/09/2015 11:49:59 ******/
ALTER TABLE [dbo].[Users]  WITH CHECK ADD FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
/****** Object:  ForeignKey [FK__Applicati__Conte__4222D4EF]    Script Date: 02/09/2015 11:49:59 ******/
ALTER TABLE [dbo].[Applications]  WITH CHECK ADD  CONSTRAINT [FK__Applicati__Conte__4222D4EF] FOREIGN KEY([ContextId])
REFERENCES [dbo].[Contexts] ([ContextId])
GO
ALTER TABLE [dbo].[Applications] CHECK CONSTRAINT [FK__Applicati__Conte__4222D4EF]
GO
/****** Object:  ForeignKey [FK__Applicati__SiteI__46E78A0C]    Script Date: 02/09/2015 11:49:59 ******/
ALTER TABLE [dbo].[Applications]  WITH CHECK ADD  CONSTRAINT [FK__Applicati__SiteI__46E78A0C] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
ALTER TABLE [dbo].[Applications] CHECK CONSTRAINT [FK__Applicati__SiteI__46E78A0C]
GO
/****** Object:  ForeignKey [FK__Folders__SiteId__59FA5E80]    Script Date: 02/09/2015 11:49:59 ******/
ALTER TABLE [dbo].[Folders]  WITH CHECK ADD  CONSTRAINT [FK__Folders__SiteId__59FA5E80] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
ALTER TABLE [dbo].[Folders] CHECK CONSTRAINT [FK__Folders__SiteId__59FA5E80]
GO
/****** Object:  ForeignKey [FK__Commodity__Conte__40F9A68C]    Script Date: 02/09/2015 11:49:59 ******/
ALTER TABLE [dbo].[Commodity]  WITH CHECK ADD  CONSTRAINT [FK__Commodity__Conte__40F9A68C] FOREIGN KEY([ContextId])
REFERENCES [dbo].[Contexts] ([ContextId])
GO
ALTER TABLE [dbo].[Commodity] CHECK CONSTRAINT [FK__Commodity__Conte__40F9A68C]
GO
/****** Object:  ForeignKey [FK__Commodity__SiteI__41EDCAC5]    Script Date: 02/09/2015 11:49:59 ******/
ALTER TABLE [dbo].[Commodity]  WITH CHECK ADD FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
/****** Object:  ForeignKey [FK__Groups__SiteId__0519C6AF]    Script Date: 02/09/2015 11:49:59 ******/
ALTER TABLE [dbo].[Groups]  WITH CHECK ADD  CONSTRAINT [FK__Groups__SiteId__0519C6AF] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
ALTER TABLE [dbo].[Groups] CHECK CONSTRAINT [FK__Groups__SiteId__0519C6AF]
GO
/****** Object:  ForeignKey [FK__Permissio__SiteI__60A75C0F]    Script Date: 02/09/2015 11:50:02 ******/
ALTER TABLE [dbo].[Permissions]  WITH CHECK ADD FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
/****** Object:  ForeignKey [FK__Roles__SiteId__145C0A3F]    Script Date: 02/09/2015 11:50:05 ******/
ALTER TABLE [dbo].[Roles]  WITH CHECK ADD  CONSTRAINT [FK__Roles__SiteId__145C0A3F] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
ALTER TABLE [dbo].[Roles] CHECK CONSTRAINT [FK__Roles__SiteId__145C0A3F]
GO
/****** Object:  ForeignKey [FK__RolePermi__Permi__1332DBDC]    Script Date: 02/09/2015 11:50:05 ******/
ALTER TABLE [dbo].[RolePermissions]  WITH CHECK ADD  CONSTRAINT [FK__RolePermi__Permi__1332DBDC] FOREIGN KEY([PermissionId])
REFERENCES [dbo].[Permissions] ([PermissionId])
GO
ALTER TABLE [dbo].[RolePermissions] CHECK CONSTRAINT [FK__RolePermi__Permi__1332DBDC]
GO
/****** Object:  ForeignKey [FK_Role_RoleId]    Script Date: 02/09/2015 11:50:05 ******/
ALTER TABLE [dbo].[RolePermissions]  WITH CHECK ADD  CONSTRAINT [FK_Role_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[Roles] ([RoleId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[RolePermissions] CHECK CONSTRAINT [FK_Role_RoleId]
GO
/****** Object:  ForeignKey [FK_Site_SiteId]    Script Date: 02/09/2015 11:50:05 ******/
ALTER TABLE [dbo].[RolePermissions]  WITH CHECK ADD  CONSTRAINT [FK_Site_SiteId] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[RolePermissions] CHECK CONSTRAINT [FK_Site_SiteId]
GO
/****** Object:  ForeignKey [FK__GroupRole__Group__5CD6CB2B]    Script Date: 02/09/2015 11:50:11 ******/
ALTER TABLE [dbo].[GroupRoles]  WITH CHECK ADD FOREIGN KEY([GroupId])
REFERENCES [dbo].[Groups] ([GroupId])
GO
/****** Object:  ForeignKey [FK__GroupRole__RoleI__5DCAEF64]    Script Date: 02/09/2015 11:50:11 ******/
ALTER TABLE [dbo].[GroupRoles]  WITH CHECK ADD FOREIGN KEY([RoleId])
REFERENCES [dbo].[Roles] ([RoleId])
GO
/****** Object:  ForeignKey [FK__GroupRole__SiteI__5EBF139D]    Script Date: 02/09/2015 11:50:11 ******/
ALTER TABLE [dbo].[GroupRoles]  WITH CHECK ADD FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
/****** Object:  ForeignKey [FK__Graphs__Applicat__4D94879B]    Script Date: 02/09/2015 11:50:11 ******/
ALTER TABLE [dbo].[Graphs]  WITH CHECK ADD  CONSTRAINT [FK__Graphs__Applicat__4D94879B] FOREIGN KEY([ApplicationId])
REFERENCES [dbo].[Applications] ([ApplicationId])
GO
ALTER TABLE [dbo].[Graphs] CHECK CONSTRAINT [FK__Graphs__Applicat__4D94879B]
GO
/****** Object:  ForeignKey [FK__Graphs__SiteId__7720AD13]    Script Date: 02/09/2015 11:50:11 ******/
ALTER TABLE [dbo].[Graphs]  WITH CHECK ADD FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
/****** Object:  ForeignKey [FK__ResourceG__Group__619B8048]    Script Date: 02/09/2015 11:50:11 ******/
ALTER TABLE [dbo].[ResourceGroups]  WITH CHECK ADD FOREIGN KEY([GroupId])
REFERENCES [dbo].[Groups] ([GroupId])
GO
/****** Object:  ForeignKey [FK__ResourceG__Resou__7C1A6C5A]    Script Date: 02/09/2015 11:50:11 ******/
ALTER TABLE [dbo].[ResourceGroups]  WITH CHECK ADD FOREIGN KEY([ResourceTypeId])
REFERENCES [dbo].[ResourceType] ([ResourceTypeId])
GO
/****** Object:  ForeignKey [FK__ResourceG__SiteI__628FA481]    Script Date: 02/09/2015 11:50:11 ******/
ALTER TABLE [dbo].[ResourceGroups]  WITH CHECK ADD FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
/****** Object:  ForeignKey [FK_BindingConfig_Applications]    Script Date: 02/09/2015 11:50:11 ******/
ALTER TABLE [dbo].[BindingConfig]  WITH CHECK ADD  CONSTRAINT [FK_BindingConfig_Applications] FOREIGN KEY([ApplicationId])
REFERENCES [dbo].[Applications] ([ApplicationId])
GO
ALTER TABLE [dbo].[BindingConfig] CHECK CONSTRAINT [FK_BindingConfig_Applications]
GO
/****** Object:  ForeignKey [FK_ApplicationSettings_Applications]    Script Date: 02/09/2015 11:50:11 ******/
ALTER TABLE [dbo].[ApplicationSettings]  WITH CHECK ADD  CONSTRAINT [FK_ApplicationSettings_Applications] FOREIGN KEY([ApplicationId])
REFERENCES [dbo].[Applications] ([ApplicationId])
GO
ALTER TABLE [dbo].[ApplicationSettings] CHECK CONSTRAINT [FK_ApplicationSettings_Applications]
GO
/****** Object:  ForeignKey [FK_ApplicationSettings_Settings]    Script Date: 02/09/2015 11:50:11 ******/
ALTER TABLE [dbo].[ApplicationSettings]  WITH CHECK ADD  CONSTRAINT [FK_ApplicationSettings_Settings] FOREIGN KEY([settingId])
REFERENCES [dbo].[Settings] ([SettingId])
GO
ALTER TABLE [dbo].[ApplicationSettings] CHECK CONSTRAINT [FK_ApplicationSettings_Settings]
GO
/****** Object:  ForeignKey [FK__Exchanges__Commo__47A6A41B]    Script Date: 02/09/2015 11:50:11 ******/
ALTER TABLE [dbo].[Exchanges]  WITH CHECK ADD  CONSTRAINT [FK__Exchanges__Commo__47A6A41B] FOREIGN KEY([CommodityId])
REFERENCES [dbo].[Commodity] ([CommodityId])
GO
ALTER TABLE [dbo].[Exchanges] CHECK CONSTRAINT [FK__Exchanges__Commo__47A6A41B]
GO
/****** Object:  ForeignKey [FK_Dictionary_Applications]    Script Date: 02/09/2015 11:50:11 ******/
ALTER TABLE [dbo].[Dictionary]  WITH CHECK ADD  CONSTRAINT [FK_Dictionary_Applications] FOREIGN KEY([ApplicationID])
REFERENCES [dbo].[Applications] ([ApplicationId])
GO
ALTER TABLE [dbo].[Dictionary] CHECK CONSTRAINT [FK_Dictionary_Applications]
GO
/****** Object:  ForeignKey [FK__UserGroup__Group__6477ECF3]    Script Date: 02/09/2015 11:50:11 ******/
ALTER TABLE [dbo].[UserGroups]  WITH CHECK ADD FOREIGN KEY([GroupId])
REFERENCES [dbo].[Groups] ([GroupId])
GO
/****** Object:  ForeignKey [FK__UserGroup__SiteI__656C112C]    Script Date: 02/09/2015 11:50:11 ******/
ALTER TABLE [dbo].[UserGroups]  WITH CHECK ADD FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
/****** Object:  ForeignKey [FK__UserGroup__UserI__66603565]    Script Date: 02/09/2015 11:50:11 ******/
ALTER TABLE [dbo].[UserGroups]  WITH CHECK ADD FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([UserId])
GO
/****** Object:  ForeignKey [FK_ValueListMap_Applications]    Script Date: 02/09/2015 11:50:11 ******/
ALTER TABLE [dbo].[ValueListMap]  WITH CHECK ADD  CONSTRAINT [FK_ValueListMap_Applications] FOREIGN KEY([ApplicationId])
REFERENCES [dbo].[Applications] ([ApplicationId])
GO
ALTER TABLE [dbo].[ValueListMap] CHECK CONSTRAINT [FK_ValueListMap_Applications]
GO
/****** Object:  ForeignKey [FK_DataObjects_Dictionary]    Script Date: 02/09/2015 11:50:23 ******/
ALTER TABLE [dbo].[DataObjects]  WITH CHECK ADD  CONSTRAINT [FK_DataObjects_Dictionary] FOREIGN KEY([DictionaryId])
REFERENCES [dbo].[Dictionary] ([Dictionaryid])
GO
ALTER TABLE [dbo].[DataObjects] CHECK CONSTRAINT [FK_DataObjects_Dictionary]
GO
/****** Object:  ForeignKey [FK_PickList_Dictionary]    Script Date: 02/09/2015 11:50:42 ******/
ALTER TABLE [dbo].[PickList]  WITH CHECK ADD  CONSTRAINT [FK_PickList_Dictionary] FOREIGN KEY([DictionaryId])
REFERENCES [dbo].[Dictionary] ([Dictionaryid])
GO
ALTER TABLE [dbo].[PickList] CHECK CONSTRAINT [FK_PickList_Dictionary]
GO
/****** Object:  ForeignKey [FK_KeyProperty_DataObjects]    Script Date: 02/09/2015 11:50:42 ******/
ALTER TABLE [dbo].[KeyProperty]  WITH CHECK ADD  CONSTRAINT [FK_KeyProperty_DataObjects] FOREIGN KEY([DataObjectId])
REFERENCES [dbo].[DataObjects] ([DataObjectId])
GO
ALTER TABLE [dbo].[KeyProperty] CHECK CONSTRAINT [FK_KeyProperty_DataObjects]
GO
/****** Object:  ForeignKey [FK_DataRelationships_DataObjects]    Script Date: 02/09/2015 11:50:42 ******/
ALTER TABLE [dbo].[DataRelationships]  WITH CHECK ADD  CONSTRAINT [FK_DataRelationships_DataObjects] FOREIGN KEY([DataObjectID])
REFERENCES [dbo].[DataObjects] ([DataObjectId])
GO
ALTER TABLE [dbo].[DataRelationships] CHECK CONSTRAINT [FK_DataRelationships_DataObjects]
GO
/****** Object:  ForeignKey [FK_DataProperties_DataObjects]    Script Date: 02/09/2015 11:50:42 ******/
ALTER TABLE [dbo].[DataProperties]  WITH CHECK ADD  CONSTRAINT [FK_DataProperties_DataObjects] FOREIGN KEY([DataObjectId])
REFERENCES [dbo].[DataObjects] ([DataObjectId])
GO
ALTER TABLE [dbo].[DataProperties] CHECK CONSTRAINT [FK_DataProperties_DataObjects]
GO
/****** Object:  ForeignKey [FK_DataProperties_PickList]    Script Date: 02/09/2015 11:50:42 ******/
ALTER TABLE [dbo].[DataProperties]  WITH CHECK ADD  CONSTRAINT [FK_DataProperties_PickList] FOREIGN KEY([PickListId])
REFERENCES [dbo].[PickList] ([PickListId])
GO
ALTER TABLE [dbo].[DataProperties] CHECK CONSTRAINT [FK_DataProperties_PickList]
GO
/****** Object:  ForeignKey [FK_PropertyMap_DataRelationships]    Script Date: 02/09/2015 11:50:42 ******/
ALTER TABLE [dbo].[PropertyMap]  WITH CHECK ADD  CONSTRAINT [FK_PropertyMap_DataRelationships] FOREIGN KEY([RelationshipID])
REFERENCES [dbo].[DataRelationships] ([RelationshipId])
GO
ALTER TABLE [dbo].[PropertyMap] CHECK CONSTRAINT [FK_PropertyMap_DataRelationships]
GO
