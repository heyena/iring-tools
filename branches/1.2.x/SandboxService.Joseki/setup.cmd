@echo off
cd %~dp0%
setlocal

:main
rem Get properties from setup.conf
for /f "tokens=1,2 delims==" %%i in (setup.conf) do (
  if "%%i" equ "mySQLPath" set mySQLPath=%%j
  if "%%i" equ "dbPort" set dbPort=%%j
  if "%%i" equ "dbPassword" set dbPassword=%%j
)

rem Set mysql command
dir "%mySQLPath%\bin\mysql.exe" >nul
if %errorlevel% gtr 0 (
  @echo ERROR: MySQL path not found.
  goto :end
)
set mySQLCmd=%mySQLPath%\bin\mysql.exe

rem Check MySQL Server version
"%mySQLCmd%" -V >temp.txt
for /f "tokens=8 delims=, " %%i in (temp.txt) do (
  for /f "usebackq tokens=1 delims=." %%j in (`echo %%i`) do (
    if %%j lss 5 (
      @echo ERROR: MySQL Server 5 or higher is required.
      goto :end
    )
    @echo Detect MySQL Server version: "%%i"
  )
)

rem Check java runtime environment
java -version 2>temp.txt
if %errorlevel% gtr 0 (
  @echo ERROR: Java 6 or higher is required.
  goto :end
)
for /f "usebackq tokens=3 delims=, " %%i in (`findstr /i "version" temp.txt`) do (
  if %%i lss "1.6" (
    @echo ERROR: Java 6 or higher is required.
    goto :end
    rem @echo Installing java 6 runtime environment ...
    rem jre-6u16-windows-i586.exe /s ADDLOCAL=jrecore 
  )
  @echo Detect java version: %%i
)

rem Create sandbox database and user
"%mySQLCmd%" --user=root --password=%dbPassword% <sandbox.sql 2>temp.txt
findstr /i /c:"Access denied" temp.txt >nul
if %errorlevel% equ 0 (
  @echo ERROR: Invalid MySQL login.
  goto :end
)

rem Create default graph
java -cp lib/arq-2.8.0.jar;lib/icu4j-3.4.4.jar;lib/iri-0.7.jar;lib/jena-2.6.0.jar;lib/jetty-6.1.10.jar;lib/jetty-util-6.1.10.jar;lib/joseki-3.4.0.jar;lib/junit-4.5.jar;lib/log4j-1.2.12.jar;lib/lucene-core-2.3.1.jar;lib/mysql-connector-java-5.1.7-bin.jar;lib/slf4j-api-1.5.6.jar;lib/slf4j-log4j12-1.5.6.jar;lib/stax-api-1.0.1.jar;lib/wstx-asl-3.2.9.jar;lib/xercesImpl-2.7.1.jar jena.dbcreate --db jdbc:mysql://localhost:%dbPort%/sandbox --dbUser root --dbPassword %dbPassword% --dbType mysql --model sandbox

rem Update web.xml to point to correct location of joseki-config.ttl
copy /y nul web.xml.tmp >nul
for /f "delims=" %%i in (webapps\joseki\WEB-INF\web.xml) do (
  if "%%i" equ "    <param-value>joseki-config.ttl#system</param-value>" (
    @echo     ^<param-value^>file:///%~dp0%joseki-config.ttl#system^</param-value^>>>web.xml.tmp
  )
  
  if "%%i" neq "    <param-value>joseki-config.ttl#system</param-value>" (
    @echo %%i>>web.xml.tmp
  )
)
move /y web.xml.tmp webapps\joseki\WEB-INF\web.xml >nul

rem Create iring sandbox root environment variable
setx IRING_SANDBOX_ROOT %~dp0% >nul 2>nul
if %errorlevel% neq 0 (
  rem Require logout to take in effect
  reg add HKCU\Environment /v IRING_SANDBOX_ROOT /d %~dp0% /f >nul
)

rem Create iRING Sandbox Service
@echo Creating iRING Sandbox Service ...
sc query sandboxsvc | findstr /i "service_name" >nul
if %errorlevel% equ 0 (
  rem Delete old service
  sc stop sandboxsvc >nul
  :wait
  ping -n 1 127.0.0.1 >nul
  tasklist | find "isbwrapper.exe" >nul
  if not %errorlevel% equ 1 goto :wait
  sc delete sandboxsvc >nul
)

isbwrapper.exe -i joseki-service.conf
sc start sandboxsvc >nul
@echo iRING Sandbox Service started.

@echo Setup completed successfully.

:end 
if exist temp.txt del temp.txt
if exist web.xml.tmp del web.xml.tmp
endlocal
pause
