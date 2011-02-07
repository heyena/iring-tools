@echo off
call "%VS100COMNTOOLS%\vsvars32.bat"
cd %~dp0%
taskkill /IM WebDev.WebServer40.exe
start /B WebDev.WebServer40 /port:65432 /path:%~dp0%iRINGTools.Services
start /B WebDev.WebServer40 /port:23456 /path:%~dp0%iRINGTools.Applications