@echo off
setlocal
set basedir=%~dp0
set "PATH=%SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319;C:\Program Files\TortoiseSVN\bin;%PATH%"
msbuild build.xml /t:All /p:Configuration="Release" /p:Platform="Any CPU"
pause