@echo off
setlocal
set basedir=%~dp0%
set javabasedir=%basedir%iRINGTools.ESBServices\iRINGTools.Services\
set phpbasedir=%basedir%iRINGTools.ESBServices\ExchangeManager\
set deploymentdir=%basedir%deployment\

echo Getting most recent updates ...
cd %basedir%
svn update

echo Building CSharp solution ...
call "%VS100COMNTOOLS%\vsvars32.bat"
taskkill /IM WebDev.WebServer40.exe
start /B WebDev.WebServer40 /port:54321 /path:%basedir%iRINGTools.Services
msbuild build.xml /v:n /t:all /fileLogger /flp:errorsonly;logfile=msbuild.error.log /fileLogger /flp1:warningsonly;logfile=msbuild.warning.log
taskkill /IM WebDev.WebServer40.exe

echo Building java project ...
cd %javabasedir%
call ant
copy %javabasedir%*.war %deploymentdir%

echo Building exchange mananger project ...
cd %phpbasedir%
call ant
copy %phpbasedir%*.zip %deploymentdir%

pause