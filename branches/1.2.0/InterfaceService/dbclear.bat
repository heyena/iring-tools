@echo off
.\Bin\rdfstorage.exe -clear -out "sqlserver:rdf:Database=rdf;data source=.\SQLEXPRESS; Initial Catalog=%1; User Id=%1; Password=%1;"

