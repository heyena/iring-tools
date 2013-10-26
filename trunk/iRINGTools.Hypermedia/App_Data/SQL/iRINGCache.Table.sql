USE [master]
GO

IF EXISTS(SELECT name FROM sys.databases WHERE name = 'iRINGCache')
	DROP DATABASE [iRINGCache]
GO

CREATE DATABASE [iRINGCache] 
GO

IF EXISTS(SELECT * FROM sys.syslogins WHERE name = N'iRINGCache')
	DROP LOGIN [abc]
GO

CREATE LOGIN [iRINGCache] WITH PASSWORD = 'iRINGCache', CHECK_POLICY = OFF
GO

USE [iRINGCache] 
GO

IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'iRINGCache') 
	DROP USER iRINGCache
GO

CREATE USER iRINGCache FOR LOGIN iRINGCache WITH DEFAULT_SCHEMA=[dbo]
GO

EXEC sp_addrolemember 'db_owner', N'iRINGCache'
GO

CREATE TABLE [dbo].[Caches](
	[CacheId] [nvarchar](64) NOT NULL,
	[Context] [nvarchar](256) NOT NULL,
	[Application] [nvarchar](256) NOT NULL,
	[State] [nvarchar](32) NULL,
	[Timestamp] [datetime] NOT NULL,
 CONSTRAINT [PK_Caches] PRIMARY KEY CLUSTERED 
(
	[CacheId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO