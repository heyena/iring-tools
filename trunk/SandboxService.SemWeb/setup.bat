@echo off
cd %~dp0%
setlocal

rem Get properties from setup.conf
for /f "tokens=1,2,3 delims== " %%i in (setup.conf) do (
  if "%%i" equ "sql_server" set dbServer=%%j
  if "%%i" equ "sql_username" set dbUser=%%j
  if "%%i" equ "sql_password" set dbPassword=%%j
)

if %dbUser% equ "" (
  if %dbPassword% equ "" (
    @echo Using Windows authentication ...
    SQLCMD -S %dbServer% -i  .\Scripts\sandbox.sql
    
    if %errorlevel% neq 0 (
      goto end
    )
  ) 
)
if %dbUser% neq "" (
  if %dbPassword% neq "" (
    @echo Using SQLServer authentication ...
    SQLCMD -U %dbUser% -P %dbPassword% -S %dbServer% -i  .\Scripts\sandbox.sql
    
    if %errorlevel% neq 0 (
      goto end
    )
  )
)

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

:end
endlocal
pause