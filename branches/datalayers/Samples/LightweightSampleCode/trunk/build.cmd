@echo off
setlocal
set basedir=%~dp0
set "PATH=%SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319;%PATH%"
msbuild build.xml /t:All /p:Configuration="Release" /p:Platform="Any CPU" /p:BUILD_NUMBER="6"
pause
