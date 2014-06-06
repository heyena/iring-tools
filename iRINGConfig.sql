
/****** Object:  Database [iRINGConfig]    Script Date: 06/06/2014 11:53:59 ******/
CREATE DATABASE [iRINGConfig]  

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
/****** Object:  Table [dbo].[Sites]    Script Date: 06/06/2014 11:53:59 ******/
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
/****** Object:  Table [dbo].[Permissions]    Script Date: 06/06/2014 11:53:59 ******/
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
/****** Object:  Table [dbo].[Groups]    Script Date: 06/06/2014 11:53:59 ******/
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
/****** Object:  Table [dbo].[Exchanges]    Script Date: 06/06/2014 11:53:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Exchanges](
	[ExchangeId] [uniqueidentifier] NOT NULL,
	[SourceGraphId] [uniqueidentifier] NOT NULL,
	[DestinationGraphId] [uniqueidentifier] NOT NULL,
	[Description] [nvarchar](255) NOT NULL,
	[PoolSize] [int] NULL,
	[Add] [tinyint] NULL,
	[Change] [tinyint] NULL,
	[Sync] [tinyint] NULL,
	[Delete] [tinyint] NULL,
	[SetNull] [tinyint] NULL,
	[SiteId] [int] NOT NULL,
	[Active] [tinyint] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ExchangeId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Contexts]    Script Date: 06/06/2014 11:53:59 ******/
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
PRIMARY KEY CLUSTERED 
(
	[ContextId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 06/06/2014 11:53:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[UserId] [int] IDENTITY(1,1) NOT NULL,
	[UserName] [nvarchar](100) NOT NULL,
	[SiteId] [int] NOT NULL,
	[UserFristName] [nvarchar](50) NULL,
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
/****** Object:  Table [dbo].[Roles]    Script Date: 06/06/2014 11:53:59 ******/
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
/****** Object:  Table [dbo].[ResourceGroups]    Script Date: 06/06/2014 11:53:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ResourceGroups](
	[ResourceId] [uniqueidentifier] NOT NULL,
	[GroupId] [int] NOT NULL,
	[SiteId] [int] NOT NULL,
	[Active] [tinyint] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ResourceId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Folders]    Script Date: 06/06/2014 11:53:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Folders](
	[FolderId] [uniqueidentifier] NOT NULL,
	[ContextId] [uniqueidentifier] NOT NULL,
	[ParentFolderId] [uniqueidentifier] NOT NULL,
	[SiteId] [int] NOT NULL,
	[Active] [tinyint] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[FolderId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserGroups]    Script Date: 06/06/2014 11:53:59 ******/
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
/****** Object:  Table [dbo].[Applications]    Script Date: 06/06/2014 11:53:59 ******/
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
 CONSTRAINT [PK__Applicat__C93A4C994AB81AF0] PRIMARY KEY CLUSTERED 
(
	[ApplicationId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[GroupRoles]    Script Date: 06/06/2014 11:53:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GroupRoles](
	[GroupRoleId] [int] IDENTITY(1,1) NOT NULL,
	[GroupId] [int] NOT NULL,
	[RoleId] [int] NOT NULL,
	[SiteId] [int] NOT NULL,
	[GroupRolesDesc] [nvarchar](255) NULL,
	[Active] [tinyint] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[GroupRoleId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Graphs]    Script Date: 06/06/2014 11:53:59 ******/
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
PRIMARY KEY CLUSTERED 
(
	[GraphId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Default [DF_Sites_Active]    Script Date: 06/06/2014 11:53:59 ******/
ALTER TABLE [dbo].[Sites] ADD  CONSTRAINT [DF_Sites_Active]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF__Permissio__Activ__1B0907CE]    Script Date: 06/06/2014 11:53:59 ******/
ALTER TABLE [dbo].[Permissions] ADD  CONSTRAINT [DF__Permissio__Activ__1B0907CE]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF_Groups_Active]    Script Date: 06/06/2014 11:53:59 ******/
ALTER TABLE [dbo].[Groups] ADD  CONSTRAINT [DF_Groups_Active]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF__Exchanges__Activ__60A75C0F]    Script Date: 06/06/2014 11:53:59 ******/
ALTER TABLE [dbo].[Exchanges] ADD  CONSTRAINT [DF__Exchanges__Activ__60A75C0F]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF__Contexts__Active__403A8C7D]    Script Date: 06/06/2014 11:53:59 ******/
ALTER TABLE [dbo].[Contexts] ADD  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF__Users__Active__0EA330E9]    Script Date: 06/06/2014 11:53:59 ******/
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF__Users__Active__0EA330E9]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF__Roles__Active__15502E78]    Script Date: 06/06/2014 11:53:59 ******/
ALTER TABLE [dbo].[Roles] ADD  CONSTRAINT [DF__Roles__Active__15502E78]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF__ResourceG__Activ__3A81B327]    Script Date: 06/06/2014 11:53:59 ******/
ALTER TABLE [dbo].[ResourceGroups] ADD  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF__Folders__Active__5DCAEF64]    Script Date: 06/06/2014 11:53:59 ******/
ALTER TABLE [dbo].[Folders] ADD  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF__UserGroup__Activ__267ABA7A]    Script Date: 06/06/2014 11:53:59 ******/
ALTER TABLE [dbo].[UserGroups] ADD  CONSTRAINT [DF__UserGroup__Activ__267ABA7A]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF__Applicati__Activ__4316F928]    Script Date: 06/06/2014 11:53:59 ******/
ALTER TABLE [dbo].[Applications] ADD  CONSTRAINT [DF__Applicati__Activ__4316F928]  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF__GroupRole__Activ__2E1BDC42]    Script Date: 06/06/2014 11:53:59 ******/
ALTER TABLE [dbo].[GroupRoles] ADD  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  Default [DF__Graphs__Active__4F7CD00D]    Script Date: 06/06/2014 11:53:59 ******/
ALTER TABLE [dbo].[Graphs] ADD  DEFAULT ((1)) FOR [Active]
GO
/****** Object:  ForeignKey [FK__Permissio__SiteI__1A14E395]    Script Date: 06/06/2014 11:53:59 ******/
ALTER TABLE [dbo].[Permissions]  WITH CHECK ADD FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
/****** Object:  ForeignKey [FK__Groups__SiteId__0519C6AF]    Script Date: 06/06/2014 11:53:59 ******/
ALTER TABLE [dbo].[Groups]  WITH CHECK ADD  CONSTRAINT [FK__Groups__SiteId__0519C6AF] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
ALTER TABLE [dbo].[Groups] CHECK CONSTRAINT [FK__Groups__SiteId__0519C6AF]
GO
/****** Object:  ForeignKey [FK__Exchanges__SiteI__5FB337D6]    Script Date: 06/06/2014 11:53:59 ******/
ALTER TABLE [dbo].[Exchanges]  WITH CHECK ADD  CONSTRAINT [FK__Exchanges__SiteI__5FB337D6] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
ALTER TABLE [dbo].[Exchanges] CHECK CONSTRAINT [FK__Exchanges__SiteI__5FB337D6]
GO
/****** Object:  ForeignKey [FK__Contexts__SiteId__3F466844]    Script Date: 06/06/2014 11:53:59 ******/
ALTER TABLE [dbo].[Contexts]  WITH CHECK ADD FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
/****** Object:  ForeignKey [FK__Users__SiteId__0F975522]    Script Date: 06/06/2014 11:53:59 ******/
ALTER TABLE [dbo].[Users]  WITH CHECK ADD FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
/****** Object:  ForeignKey [FK__Roles__SiteId__145C0A3F]    Script Date: 06/06/2014 11:53:59 ******/
ALTER TABLE [dbo].[Roles]  WITH CHECK ADD  CONSTRAINT [FK__Roles__SiteId__145C0A3F] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
ALTER TABLE [dbo].[Roles] CHECK CONSTRAINT [FK__Roles__SiteId__145C0A3F]
GO
/****** Object:  ForeignKey [FK__ResourceG__Group__38996AB5]    Script Date: 06/06/2014 11:53:59 ******/
ALTER TABLE [dbo].[ResourceGroups]  WITH CHECK ADD FOREIGN KEY([GroupId])
REFERENCES [dbo].[Groups] ([GroupId])
GO
/****** Object:  ForeignKey [FK__ResourceG__SiteI__398D8EEE]    Script Date: 06/06/2014 11:53:59 ******/
ALTER TABLE [dbo].[ResourceGroups]  WITH CHECK ADD FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
/****** Object:  ForeignKey [FK__Folders__Context__5BE2A6F2]    Script Date: 06/06/2014 11:53:59 ******/
ALTER TABLE [dbo].[Folders]  WITH CHECK ADD FOREIGN KEY([ContextId])
REFERENCES [dbo].[Contexts] ([ContextId])
GO
/****** Object:  ForeignKey [FK__Folders__SiteId__5CD6CB2B]    Script Date: 06/06/2014 11:53:59 ******/
ALTER TABLE [dbo].[Folders]  WITH CHECK ADD FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
/****** Object:  ForeignKey [FK__UserGroup__Group__239E4DCF]    Script Date: 06/06/2014 11:53:59 ******/
ALTER TABLE [dbo].[UserGroups]  WITH CHECK ADD FOREIGN KEY([GroupId])
REFERENCES [dbo].[Groups] ([GroupId])
GO
/****** Object:  ForeignKey [FK__UserGroup__SiteI__25869641]    Script Date: 06/06/2014 11:53:59 ******/
ALTER TABLE [dbo].[UserGroups]  WITH CHECK ADD FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
/****** Object:  ForeignKey [FK__UserGroup__UserI__24927208]    Script Date: 06/06/2014 11:53:59 ******/
ALTER TABLE [dbo].[UserGroups]  WITH CHECK ADD FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([UserId])
GO
/****** Object:  ForeignKey [FK__Applicati__Conte__4222D4EF]    Script Date: 06/06/2014 11:53:59 ******/
ALTER TABLE [dbo].[Applications]  WITH CHECK ADD  CONSTRAINT [FK__Applicati__Conte__4222D4EF] FOREIGN KEY([ContextId])
REFERENCES [dbo].[Contexts] ([ContextId])
GO
ALTER TABLE [dbo].[Applications] CHECK CONSTRAINT [FK__Applicati__Conte__4222D4EF]
GO
/****** Object:  ForeignKey [FK__Applicati__SiteI__46E78A0C]    Script Date: 06/06/2014 11:53:59 ******/
ALTER TABLE [dbo].[Applications]  WITH CHECK ADD  CONSTRAINT [FK__Applicati__SiteI__46E78A0C] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
ALTER TABLE [dbo].[Applications] CHECK CONSTRAINT [FK__Applicati__SiteI__46E78A0C]
GO
/****** Object:  ForeignKey [FK__GroupRole__Group__2B3F6F97]    Script Date: 06/06/2014 11:53:59 ******/
ALTER TABLE [dbo].[GroupRoles]  WITH CHECK ADD FOREIGN KEY([GroupId])
REFERENCES [dbo].[Groups] ([GroupId])
GO
/****** Object:  ForeignKey [FK__GroupRole__RoleI__2C3393D0]    Script Date: 06/06/2014 11:53:59 ******/
ALTER TABLE [dbo].[GroupRoles]  WITH CHECK ADD FOREIGN KEY([RoleId])
REFERENCES [dbo].[Roles] ([RoleId])
GO
/****** Object:  ForeignKey [FK__GroupRole__SiteI__2D27B809]    Script Date: 06/06/2014 11:53:59 ******/
ALTER TABLE [dbo].[GroupRoles]  WITH CHECK ADD FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
/****** Object:  ForeignKey [FK__Graphs__Applicat__4D94879B]    Script Date: 06/06/2014 11:53:59 ******/
ALTER TABLE [dbo].[Graphs]  WITH CHECK ADD  CONSTRAINT [FK__Graphs__Applicat__4D94879B] FOREIGN KEY([ApplicationId])
REFERENCES [dbo].[Applications] ([ApplicationId])
GO
ALTER TABLE [dbo].[Graphs] CHECK CONSTRAINT [FK__Graphs__Applicat__4D94879B]
GO
/****** Object:  ForeignKey [FK__Graphs__SiteId__4E88ABD4]    Script Date: 06/06/2014 11:53:59 ******/
ALTER TABLE [dbo].[Graphs]  WITH CHECK ADD FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([SiteId])
GO
