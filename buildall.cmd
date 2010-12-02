@echo off
setlocal
set basedir=%~dp0
set javabasedir=%basedir%iRINGTools.ESBServices\
set deploymentdir=%basedir%deployment\

echo Getting most recent updates ...
cd %basedir%
svn update

echo Cleaning up deployment directory ...
if exist %deploymentdir% del /q %deploymentdir%*.*
if not exist %deploymentdir% mkdir %deploymentdir%

echo Building CSharp solution ...
call "%VS100COMNTOOLS%\vsvars32.bat"
taskkill /IM WebDev.WebServer40.exe
start /B WebDev.WebServer40 /port:54321 /path:%basedir%iRINGTools.Services
msbuild build.xml /v:n /t:all /fileLogger /flp:errorsonly;logfile=msbuild.error.log /fileLogger /flp1:warningsonly;logfile=msbuild.warning.log
taskkill /IM WebDev.WebServer40.exe

pause

echo Building java project ...
cd %javabasedir%
call ant
if %ERRORLEVEL% equ 0 copy /y %javabasedir%dist\*.* %deploymentdir%

pause