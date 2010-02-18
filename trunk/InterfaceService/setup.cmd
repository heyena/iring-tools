@echo off
cd %~dp0%
setlocal

set token=iring

rem Get properties from setup.conf
for /f "tokens=1,2 delims== " %%i in (setup.conf) do (
  if "%%i" equ "sql_instance" set dbInstance=%%j
  if "%%i" equ "sql_username" set dbUser=%%j
  if "%%i" equ "sql_password" set dbPassword=%%j
  if "%%i" equ "project_name" set projectName=%%j
  if "%%i" equ "app_name" set appName=%%j
)

if %projectName% neq "" if %appName% neq "" set token=%projectName%_%appName%

rem Update Setup.sql file
@echo Updating SQL file ...
set old_key="<<TOKEN>>"
set new_key="%token%"
.\Bin\replace .\Scripts\Setup.sql %old_key% %new_key% > .\Scripts\Setup.sql.tmp
if %errorlevel% equ 0 (
  move /y .\Scripts\Setup.sql.tmp .\Scripts\Setup_%token%.sql >nul
)

rem Set up database
@echo Setting up interface service database ...
if %dbUser% equ "" if %dbPassword% equ "" (
  SQLCMD -S %dbInstance% -i  .\Scripts\Setup_%token%.sql  
  if %errorlevel% equ 0 (
    goto InitRDF
  )  
)

SQLCMD -U %dbUser% -P %dbPassword% -S %dbInstance% -i  .\Scripts\Setup_%token%.sql
if %errorlevel% equ 0 (
  goto InitRDF
) 

@echo ERROR Invalid database access information.
goto End

:InitRDF
rem Initialize RDF storage
@echo Initializing RDF storage ...
.\Bin\rdfstorage.exe -out "sqlserver:rdf:Database=rdf;data source=%dbInstance%; Initial Catalog=%token%; User Id=%token%; Password=%token%;"

rem Update web.config file
@echo Updating web configuration file ...
set old_key=".*key=\"/InterfaceService/sparql\".*"
set new_key="    <add key=\"/InterfaceService/sparql\" value=\"noreuse,rdfs+sqlserver:rdf:Database=rdf;data source=%dbInstance%; Initial Catalog=%token%; User Id=%token%; Password=%token%;\"/>"
.\Bin\replace web.config %old_key% %new_key% > web.config.tmp
if %errorlevel% equ 0 (
  move /y web.config.tmp web.config >nul
)

:End
endlocal
pause