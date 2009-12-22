@echo off
cd %~dp0%
setlocal

rem Get properties from setup.conf
for /f "tokens=1,2,3 delims== " %%i in (setup.conf) do (
  if "%%i" equ "sql_server" set dbServer=%%j
  if "%%i" equ "sql_username" set dbUser=%%j
  if "%%i" equ "sql_password" set dbPassword=%%j
)

rem Set up database
@echo Setting up sandbox service database ...
if %dbUser% equ "" if %dbPassword% equ "" (
  SQLCMD -S %dbServer% -i  .\Scripts\sandbox.sql  
  if %errorlevel% equ 0 (
    goto InitRDF
  )  
)

SQLCMD -U %dbUser% -P %dbPassword% -S %dbServer% -i  .\Scripts\sandbox.sql
if %errorlevel% equ 0 (
  goto InitRDF
) 

@echo ERROR Invalid database access information.
goto End

:InitRDF
rem Initialize RDF storage
@echo Initializing RDF storage ...
.\Bin\rdfstorage.exe -out "sqlserver:rdf:Database=rdf;data source=%dbServer%; Initial Catalog=sandbox; User Id=sandbox; Password=sandbox;"

rem Update web.config file
@echo Updating web configuration file ...
set old_key=".*key=\"/SandboxService/sparql\".*"
set new_key="    <add key=\"/SandboxService/sparql\" value=\"noreuse,rdfs+sqlserver:rdf:Database=rdf;data source=%dbServer%; Initial Catalog=sandbox; User Id=sandbox; Password=sandbox;\"/>"
.\Bin\replace web.config %old_key% %new_key% > web.config.tmp
if %errorlevel% equ 0 (
  move /y web.config.tmp web.config >nul
)

:End
endlocal
pause