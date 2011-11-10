@echo off

setlocal
set java_dist="c:\bamboo-home\xml-data\build-dir\IRT-IRT22-JAVABUILD\dist\"
set tomcat_home=c:\Program Files\Apache Software Foundation\Tomcat 6.0\"

cd %~dp0%
svn update
copy %java_dist%iringtools-apps.war %tomcat_home%iipapps.war
copy %java_dist%iringtools-services.war %tomcat_home%iipservices.war
xcopy tomcat\*.* %tomcat_home% /s /y

xcopy iis\*.* ..\ /s /y /d