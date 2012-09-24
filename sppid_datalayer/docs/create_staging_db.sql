USE [master]
GO

IF EXISTS(SELECT name FROM sys.databases WHERE name = 'SPPIDStaging')
	DROP DATABASE [SPPIDStaging]
GO

CREATE DATABASE [SPPIDStaging] 
GO

IF EXISTS(SELECT * FROM sys.syslogins WHERE name = N'SPPIDStaging')
	DROP LOGIN [SPPIDStaging]
GO

CREATE LOGIN [SPPIDStaging] WITH PASSWORD = 'SPPIDStaging', CHECK_POLICY = OFF
GO

USE [SPPIDStaging]
GO

IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'SPPIDStaging') 
	DROP USER [SPPIDStaging]
GO

CREATE USER [SPPIDStaging] FOR LOGIN [SPPIDStaging] WITH DEFAULT_SCHEMA=[dbo]
GO

EXEC sp_addrolemember 'db_owner', N'SPPIDStaging'
GO