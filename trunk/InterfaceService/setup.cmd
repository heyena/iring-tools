@echo off
cd %~dp0%
setlocal

rem Get properties from setup.conf
for /f "tokens=1,2 delims== " %%i in (setup.conf) do (
  if "%%i" equ "sql_instance" set dbInstance=%%j
  if "%%i" equ "sql_username" set dbUser=%%j
  if "%%i" equ "sql_password" set dbPassword=%%j
)

rem Set up database
@echo Setting up interface service database ...
if %dbUser% equ "" if %dbPassword% equ "" (
  SQLCMD -S %dbInstance% -i  .\Scripts\Setup.sql  
  if %errorlevel% equ 0 (
    goto End
  )  
)

SQLCMD -U %dbUser% -P %dbPassword% -S %dbInstance% -i  .\Scripts\Setup.sql
if %errorlevel% equ 0 (
  goto End
) 

@echo ERROR Invalid database access information.
goto End

:End
endlocal
pause