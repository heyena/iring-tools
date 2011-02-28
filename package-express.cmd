@echo off
cd %~dp0%
svn update
msbuild build-express.xml /t:CreatePackages /fileLogger /flp:errorsonly;logfile=msbuild.error.log /fileLogger /flp1:warningsonly;logfile=msbuild.warning.log
pause