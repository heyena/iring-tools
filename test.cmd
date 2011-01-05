@echo off
call "%VS100COMNTOOLS%\vsvars32.bat"
cd %~dp0%
svn update build.xml
taskkill /IM WebDev.WebServer40.exe >> null
start /B WebDev.WebServer40 /port:65432 /path:%~dp0%iRINGTools.Services
msbuild build.xml /t:Test /fileLogger /flp:errorsonly;logfile=msbuild.error.log /fileLogger /flp1:warningsonly;logfile=msbuild.warning.log
taskkill /IM WebDev.WebServer40.exe >> null
pause