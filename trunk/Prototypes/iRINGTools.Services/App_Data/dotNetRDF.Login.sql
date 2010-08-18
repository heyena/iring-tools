USE [master]
GO

IF EXISTS(SELECT * FROM sys.syslogins WHERE name = N'dotNetRDF')
	DROP LOGIN [dotNetRDF]
GO

CREATE LOGIN [dotNetRDF] WITH PASSWORD = 'dotNetRDF', CHECK_POLICY = OFF
GO