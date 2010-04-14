@echo off
::This will clear your interface of all data.
cd %JOSEKIROOT%
call setclasspath
call dbremove data
call dbcreate data