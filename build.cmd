@echo off
setlocal

set basedir=%~dp0
set deploymentdir=%basedir%Deployment\
set "PATH=%SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319;%PATH%"

echo Updating svn ...
cd %basedir%
svn update
pause

echo Building C# projects ...
msbuild build.xml /t:ReBuild,CreatePackages
pause