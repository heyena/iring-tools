@echo off

rem Get properties from setup.conf
for /f "tokens=1,2,3 delims== " %%i in (setup.conf) do (
  if "%%i" equ "sql_username" set dbUser=%%j
  if "%%i" equ "sql_password" set dbPassword=%%j
  if "%%i" equ "sql_server" set dbServer=%%j
)

if %dbUser% equ "" if %dbPassword% equ "" (@echo Windows authentication SQLCMD -S %dbServer% -i  Scripts\iRING.sql) 
if %dbUser% neq "" if %dbPassword% neq "" (@echo SqlServer authentication SQLCMD -U %dbUser% -P %dbPassword% -S %dbServer% -i  Scripts\iRING.sql)


Bin\rdfstorage.exe -out "sqlserver:rdf:Database=rdf;data source=%dbServer%; Initial Catalog=iring; User Id=iring; Password=iring;"

pause