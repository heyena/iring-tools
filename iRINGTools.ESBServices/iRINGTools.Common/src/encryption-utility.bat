@echo off
cls
cd %~dp0 & java -cp ".;.\lib\*" org.iringtools.utility.EncryptionUtils
pause