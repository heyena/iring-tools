@echo off
cd %~dp0%
@set "PATH=%SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319;%PATH%"
svn update build-express.xml
msbuild build-express.xml /t:All /fileLogger /flp:errorsonly;logfile=msbuild.error.log /fileLogger /flp1:warningsonly;logfile=msbuild.warning.log
pause