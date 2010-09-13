@echo off
call "%VS100COMNTOOLS%\vsvars32.bat"
cd %~dp0%
svn update
msbuild build.xml /t:CreatePackages /fileLogger /flp:errorsonly;logfile=msbuild.error.log /fileLogger /flp1:warningsonly;logfile=msbuild.warning.log
pause