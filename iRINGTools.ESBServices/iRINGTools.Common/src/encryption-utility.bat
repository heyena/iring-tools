@echo off
cls
cd %~dp0
cmd /k  "java -cp \".;iringtools-common.jar;commons-codec-1.4.jar\" org.iringtools.utility.EncryptionUtils"
