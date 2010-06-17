IF  EXISTS (SELECT name FROM sys.databases WHERE name = N'sandbox')
DROP DATABASE sandbox
GO

IF EXISTS (SELECT * FROM sys.syslogins WHERE name = N'sandbox')
DROP LOGIN sandbox
GO

CREATE DATABASE sandbox
GO

USE sandbox
GO

CREATE LOGIN sandbox WITH PASSWORD = 'sandbox', CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF
GO

CREATE USER sandbox FOR LOGIN sandbox
GO

EXEC sp_addrolemember db_owner, sandbox


