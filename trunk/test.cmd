@echo off
setlocal
set basedir=%~dp0
set "PATH=%SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319;%PATH%"

echo Updating build script ...
cd %basedir%
svn update build.xml
pause

echo Running C# tests ...
msbuild build.xml /t:Test /fileLogger /flp:errorsonly;logfile=msbuild.error.log /fileLogger /flp1:warningsonly;logfile=msbuild.warning.log
pause