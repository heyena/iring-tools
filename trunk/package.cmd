@echo off
setlocal
set basedir=%~dp0
set "PATH=%SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319;%PATH%"

echo Creating Deployment Packages ...
msbuild bamboo.xml /t:CreatePackages
pause