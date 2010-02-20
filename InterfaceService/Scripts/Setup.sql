IF EXISTS (SELECT * FROM sys.syslogins WHERE name = N'iring')
DROP LOGIN [iring]
GO

CREATE LOGIN [iring] WITH PASSWORD = 'iring', CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF
GO

EXEC sp_addsrvrolemember [iring], dbcreator
