@echo off
cd %~dp0%
@set "PATH=%SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319;%PATH%"
svn update bamboo.xml
msbuild bamboo.xml /t:CreatePackages
pause