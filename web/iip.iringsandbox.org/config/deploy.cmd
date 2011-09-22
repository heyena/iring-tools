cd %~dp0%
svn update
xcopy iis\*.* ..\ /s /y /d
xcopy tomcat\*.* "C:\Program Files\Apache Software Foundation\Tomcat 6.0" /s /y