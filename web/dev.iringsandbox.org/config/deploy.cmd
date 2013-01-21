@echo off

setlocal
set java_dist="c:\bamboo-home\xml-data\build-dir\IRT-TRUNK-JAVA\dist\"
set tomcat_home="c:\Program Files\Apache Software Foundation\Tomcat 6.0\"

cd %~dp0%
svn update

copy %java_dist%iringtools-apps.war %tomcat_home%webapps\devapps.war
copy %java_dist%iringtools-services.war %tomcat_home%webapps\devservices.war
rem give tomcat 30 seconds to deploy war files
ping -n 30 localhost
xcopy tomcat\*.* %tomcat_home% /s /y

xcopy iis\*.* ..\ /s /y /d