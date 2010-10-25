@echo off
@REM Script to build the Joseki classpath
@REM $Id: joseki_path.bat,v 1.5 2004/01/09 18:21:34 andy_seaborne Exp $

@REM Must be windows format \ not /
if "%IRING_INTERFACE_ROOT%" == ""  goto noRoot

set CP=""
REM Do this to put the developement .class files first
REM NB no space before the ")"
if EXIST %IRING_INTERFACE_ROOT%\classes (
  if "%CP%" == "" (set CP=%IRING_INTERFACE_ROOT%\classes) ELSE (set CP=%CP%;%IRING_INTERFACE_ROOT%\classes)
)

pushd %IRING_INTERFACE_ROOT%
for %%f in (lib\*.jar) do call :oneStep %%f
popd
goto noMore

:oneStep
if "%CP%" == "" (set CP=%IRING_INTERFACE_ROOT%\%1) ELSE (set CP=%CP%;%IRING_INTERFACE_ROOT%\%1)
exit /B

:noRoot
echo Environment variable IRING_INTERFACE_ROOT needs to be set
REM set IRING_INTERFACE_ROOT=%CD:~0,-4%

:noMore

