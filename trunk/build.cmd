@echo off
call "%VS100COMNTOOLS%\vsvars32.bat"
cd %~dp0%
rem "C:\Program Files\TortoiseSVN\bin\TortoiseProc.exe" /notempfile /command:update /path:build.xml /closeonend:1
svn update build.xml
taskkill /IM WebDev.WebServer40.exe
start /B WebDev.WebServer40 /port:54321 /path:C:\iring-tools\iRINGTools.Services
start /B WebDev.WebServer40 /port:12345 /path:C:\iring-tools\iRINGTools.Web
msbuild build.xml /v:n /t:all /fileLogger /flp:errorsonly;logfile=msbuild.error.log /fileLogger /flp1:warningsonly;logfile=msbuild.warning.log
taskkill /IM WebDev.WebServer40.exe
pause