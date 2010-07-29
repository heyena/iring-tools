USE [master]
GO

IF EXISTS(SELECT name FROM sys.databases WHERE name = 'dotNetRDF')
	DROP DATABASE [dotNetRDF]
GO

CREATE DATABASE [dotNetRDF] 
GO

IF EXISTS(SELECT * FROM sys.syslogins WHERE name = N'dotNetRDF')
	DROP LOGIN [dotNetRDF]
GO

CREATE LOGIN [dotNetRDF] WITH PASSWORD = 'dotNetRDF', CHECK_POLICY = OFF
GO

USE [dotNetRDF]
GO

IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'dotNetRDF') 
	DROP USER [dotNetRDF]
GO

CREATE USER [dotNetRDF] FOR LOGIN [dotNetRDF] WITH DEFAULT_SCHEMA=[dotNetRDF]
GO

EXEC sp_addrolemember 'db_owner', N'dotNetRDF'
GO