CREATE DATABASE Camelot_Test
GO

CREATE LOGIN [camelot] WITH PASSWORD = 'camelot', CHECK_POLICY = OFF
GO

USE [Camelot_Test]
GO

CREATE USER [camelot] FOR LOGIN [camelot] WITH DEFAULT_SCHEMA=[dbo]
GO

EXEC sp_addrolemember 'db_owner', N'camelot'
