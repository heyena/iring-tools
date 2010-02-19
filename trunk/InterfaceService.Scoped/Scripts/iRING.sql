IF  EXISTS (SELECT name FROM sys.databases WHERE name = N'iring')
DROP DATABASE iring
GO

IF EXISTS (SELECT * FROM sys.syslogins WHERE name = N'iring')
DROP LOGIN iring
GO

CREATE DATABASE iring
GO

USE iring
GO

CREATE LOGIN iring WITH PASSWORD = 'iring', CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF
GO

CREATE USER iring FOR LOGIN iring
GO

EXEC sp_addrolemember db_owner, iring

