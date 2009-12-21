@echo off
cd %~dp0%
setlocal

rem Get properties from setup.conf
for /f "tokens=1,2,3 delims== " %%i in (setup.conf) do (
  if "%%i" equ "sql_server" set dbServer=%%j
  if "%%i" equ "sql_catalog" set dbCatalog=%%j
  if "%%i" equ "sql_username" set dbUser=%%j
  if "%%i" equ "sql_password" set dbPassword=%%j
)

if %dbUser% equ "" if %dbPassword% equ "" (@echo Windows authentication SQLCMD -S %dbServer% -i  Scripts\iRING.sql) 
if %dbUser% neq "" if %dbPassword% neq "" (@echo SqlServer authentication SQLCMD -U %dbUser% -P %dbPassword% -S %dbServer% -i  .\Scripts\iRING.sql)

rem Initialize RDF storage
.\Bin\rdfstorage.exe -out "sqlserver:rdf:Database=rdf;data source=%dbServer%; Initial Catalog=%dbCatalog%; User Id=%dbUser%; Password=%dbPassword%;"

rem Update web.config file
set old_key=".*key=\"/InterfaceService/sparql\".*"
set new_key="    <add key=\"/InterfaceService/sparql\" value=\"noreuse,rdfs+sqlserver:rdf:Database=rdf;data source=%dbServer%; Initial Catalog=%dbCatalog%; User Id=%dbUser%; Password=%dbPassword%;\"/>"
.\Bin\replace web.config %old_key% %new_key% > web.config.tmp
if %errorlevel% equ 0 (
  move /y web.config.tmp web.config >nul
)

pause