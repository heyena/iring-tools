USE [master]
GO

IF EXISTS(SELECT name FROM sys.databases WHERE name = 'SP3DStaging')
	DROP DATABASE [SP3DStaging]
GO

CREATE DATABASE [SP3DStaging] 
GO

IF EXISTS(SELECT * FROM sys.syslogins WHERE name = N'SP3DStaging')
	DROP LOGIN [SP3DStaging]
GO

CREATE LOGIN [SP3DStaging] WITH PASSWORD = 'SP3DStaging', CHECK_POLICY = OFF
GO

USE [SP3DStaging]
GO

IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'SP3DStaging') 
	DROP USER [SP3DStaging]
GO

CREATE USER [SP3DStaging] FOR LOGIN [SP3DStaging] WITH DEFAULT_SCHEMA=[dbo]
GO

EXEC sp_addrolemember 'db_owner', N'SP3DStaging'
GO