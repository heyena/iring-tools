@echo off
call "%VS90COMNTOOLS%\vsvars32.bat"
cd %~dp0%
rem "C:\Program Files\TortoiseSVN\bin\TortoiseProc.exe" /notempfile /command:update /path:build.xml /closeonend:1
svn update
msbuild build.x64.xml /t:all /fileLogger /flp:errorsonly;logfile=msbuild.error.log /fileLogger /flp1:warningsonly;logfile=msbuild.warning.log
pause