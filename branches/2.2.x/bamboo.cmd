@echo off
setlocal
set "PATH=%SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319;%PATH%"

echo Building C# projects ...
msbuild bamboo.xml /t:All
pause