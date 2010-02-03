@echo off
.\Bin\rdfstorage.exe -clear -out "sqlserver:rdf:Database=rdf;data source=.\SQLEXPRESS; Initial Catalog=sandbox; User Id=sandbox; Password=sandbox;" %1

