@echo off
setlocal
rem set basedir=%~dp0
rem set javabasedir=%basedir%iRINGTools.ESBServices\
rem set deploymentdir=%basedir%Deployment\
set "PATH=%SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319;%PATH%"

rem echo Updating build script ...
rem cd %basedir%
rem svn update build.xml
rem pause

echo Building C# projects ...
msbuild bamboo.xml /t:All
pause

rem echo Building Java projects ...
rem cd %javabasedir%
rem call ant
rem if %ERRORLEVEL% equ 0 copy /y %javabasedir%dist\*.* %deploymentdir%
rem pause