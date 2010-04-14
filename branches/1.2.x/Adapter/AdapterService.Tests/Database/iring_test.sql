CREATE DATABASE iring_test
GO

CREATE LOGIN [iringtools] WITH PASSWORD = 'iringtools', CHECK_POLICY = OFF
GO

USE [iring_test]
GO

CREATE USER [iringtools] FOR LOGIN [iringtools] WITH DEFAULT_SCHEMA=[dbo]
GO

EXEC sp_addrolemember 'db_owner', N'iringtools'