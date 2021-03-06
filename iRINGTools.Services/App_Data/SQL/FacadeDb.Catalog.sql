USE [master]
GO

IF EXISTS(SELECT name FROM sys.databases WHERE name = 'InterfaceDb')
	DROP DATABASE [InterfaceDb]
GO

IF EXISTS(SELECT name FROM sys.databases WHERE name = 'FacadeDb')
	DROP DATABASE [FacadeDb]
GO

CREATE DATABASE [FacadeDb] 
GO

USE [FacadeDb]
GO

IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'dotNetRDF') 
	DROP USER [dotNetRDF]
GO

CREATE LOGIN dotNetRDF WITH PASSWORD = 'dotNetRDF', CHECK_POLICY = OFF
GO

CREATE USER [dotNetRDF] FOR LOGIN [dotNetRDF] WITH DEFAULT_SCHEMA=[dotNetRDF]
GO

EXEC sp_addrolemember 'db_owner', N'dotNetRDF'
GO

 --TABLE CREATION IS HANDLED BY dotnetRDF Library Should not be done here
 --Tables will be created to the latest standard if they do not exist
CREATE TABLE [GRAPH_TRIPLES](
	[graphID] [int] NOT NULL,
	[tripleID] [int] NOT NULL,
 CONSTRAINT [GraphTriplesPKey] PRIMARY KEY CLUSTERED 
 (
	[graphID] ASC,
	[tripleID] ASC
 )
)
GO

CREATE TABLE [GRAPHS](
	[graphID] [int] IDENTITY(1,1) NOT NULL,
	[graphURI] [nvarchar](max) NULL,
	[graphHash] [int] NOT NULL,
 CONSTRAINT [GraphPKey] PRIMARY KEY CLUSTERED 
 (
	[graphID] ASC
 )
)
GO

CREATE TABLE [NAMESPACES](
	[nsID] [int] IDENTITY(1,1) NOT NULL,
	[graphID] [int] NOT NULL,
	[nsPrefixID] [int] NOT NULL,
	[nsUriID] [int] NOT NULL,
 CONSTRAINT [NSPKey] PRIMARY KEY CLUSTERED 
 (
	[nsID] ASC
 )
)
GO

CREATE TABLE [NODES](
	[nodeID] [int] NOT NULL,
	[nodeType] [tinyint] NOT NULL,
	[nodeValue] [nvarchar](max) NULL,
	[nodeHash] [int] NOT NULL,
 CONSTRAINT [NodePKey] PRIMARY KEY CLUSTERED 
 (
	[nodeID] ASC
 )
)
GO

CREATE TABLE [NS_PREFIXES](
	[nsPrefixID] [int] IDENTITY(1,1) NOT NULL,
	[nsPrefix] [nvarchar](50) NULL,
 CONSTRAINT [NSPrefixPKey] PRIMARY KEY CLUSTERED 
 (
	[nsPrefixID] ASC
 )
)
GO

CREATE TABLE [NS_URIS](
	[nsUriID] [int] IDENTITY(1,1) NOT NULL,
	[nsUri] [nvarchar](max) NULL,
	[nsUriHash] [int] NOT NULL,
 CONSTRAINT [NSUriPKey] PRIMARY KEY CLUSTERED 
 (
	[nsUriID] ASC
 )
)
GO

CREATE TABLE [TRIPLES](
	[tripleID] [int] NOT NULL,
	[tripleSubject] [int] NOT NULL,
	[triplePredicate] [int] NOT NULL,
	[tripleObject] [int] NOT NULL,
	[tripleHash] [int] NOT NULL,
 CONSTRAINT [TriplePKey] PRIMARY KEY CLUSTERED 
 (
    [tripleID] ASC
 )
)
GO


