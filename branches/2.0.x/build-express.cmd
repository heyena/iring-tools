@echo off
cd %~dp0%
svn update build-express.xml
taskkill /IM WebDev.WebServer40.exe
start /B WebDev.WebServer40 /port:65432 /path:%~dp0%iRINGTools.Services
msbuild build-express.xml /v:n /t:all /fileLogger /flp:errorsonly;logfile=msbuild.error.log /fileLogger /flp1:warningsonly;logfile=msbuild.warning.log
taskkill /IM WebDev.WebServer40.exe
pause