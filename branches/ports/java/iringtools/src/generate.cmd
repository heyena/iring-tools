setlocal
set xsdFile=%1
rem java -jar C:\Development\java\libraries\jaxb-ri-20100511\lib\jaxb-xjc.jar %xsdFile%
C:\Development\java\libraries\jaxb-ri-20100511\bin\xjc -cp C:\Development\java\libraries\jaxb-ri-20100511\lib\collection-setter-injector.jar -Xcollection-setter-injector %xsdFile%