setlocal
set xsdFile=%1
..\..\libraries\jaxb-ri-20100511\bin\xjc -d ..\src\ -cp ..\..\libraries\jaxb-ri-20100511\lib\collection-setter-injector.jar -Xcollection-setter-injector %xsdFile%