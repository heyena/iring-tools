@echo off
cd %~dp0%
svn update build-express.xml
msbuild build-express.xml /v:n /t:all /fileLogger /flp:errorsonly;logfile=msbuild.error.log /fileLogger /flp1:warningsonly;logfile=msbuild.warning.log
pause