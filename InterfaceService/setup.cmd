@echo off
cd %~dp0%
setlocal

rem Get properties from setup.conf
for /f "tokens=1,2,3 delims== " %%i in (setup.conf) do (
  if "%%i" equ "sql_instance" set dbInstance=%%j
  if "%%i" equ "sql_username" set dbUser=%%j
  if "%%i" equ "sql_password" set dbPassword=%%j
)

rem Set up database
@echo Setting up interface service database ...
if %dbUser% equ "" if %dbPassword% equ "" (
  SQLCMD -S %dbInstance% -i  .\Scripts\iRING.sql  
  if %errorlevel% equ 0 (
    goto InitRDF
  )  
)

SQLCMD -U %dbUser% -P %dbPassword% -S %dbInstance% -i  .\Scripts\iRING.sql
if %errorlevel% equ 0 (
  goto InitRDF
) 

@echo ERROR Invalid database access information.
goto End

:InitRDF
rem Initialize RDF storage
@echo Initializing RDF storage ...
.\Bin\rdfstorage.exe -out "sqlserver:rdf:Database=rdf;data source=%dbInstance%; Initial Catalog=iring; User Id=iring; Password=iring;"

rem Update web.config file
@echo Updating web configuration file ...
set old_key=".*key=\"/InterfaceService/sparql\".*"
set new_key="    <add key=\"/InterfaceService/sparql\" value=\"noreuse,rdfs+sqlserver:rdf:Database=rdf;data source=%dbInstance%; Initial Catalog=iring; User Id=iring; Password=iring;\"/>"
.\Bin\replace web.config %old_key% %new_key% > web.config.tmp
if %errorlevel% equ 0 (
  move /y web.config.tmp web.config >nul
)

:End
endlocal
pause