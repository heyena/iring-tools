<%@ page language="java" contentType="text/html; charset=ISO-8859-1" pageEncoding="ISO-8859-1"%>
<!DOCTYPE html PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.org/TR/html4/loose.dtd">
<html>
<head>
  <meta http-equiv="Content-Type" content="text/html; charset=ISO-8859-1">
  <meta http-equiv="Cache-Control" content="no-cache">
  <meta http-equiv="Pragma" content="no-cache">
  <meta http-equiv="Expires" content="0">
  
  <title>iRINGTools: Exchange Manager</title>
  <link rel="shortcut icon" href="resources/images/favicon.ico">
  
  <link rel="stylesheet" type="text/css" href="resources/ext-3.4.0/resources/css/ext-all.css" />  
  <link rel="stylesheet" type="text/css" href="resources/ext-3.4.0/resources/css/xtheme-gray.css" /> 
  <link rel="stylesheet" type="text/css" href="resources/css/exchange-manager-gray.css"/>
  <script type="text/javascript" src="resources/ext-3.4.0/adapter/ext/ext-base.js"></script> 
  <script type="text/javascript" src="resources/ext-3.4.0/ext-all.js"></script>
  
  <link rel="stylesheet" type="text/css" href="resources/ext-3.4.0/ux/gridfilters/css/GridFilters.css" />
  <link rel="stylesheet" type="text/css" href="resources/ext-3.4.0/ux/gridfilters/css/RangeMenu.css" />
  <script type="text/javascript" src="resources/ext-3.4.0/ux/gridfilters/menu/RangeMenu.js"></script>
  <script type="text/javascript" src="resources/ext-3.4.0/ux/gridfilters/menu/ListMenu.js"></script>
  <script type="text/javascript" src="resources/ext-3.4.0/ux/gridfilters/GridFilters.js"></script>
  <script type="text/javascript" src="resources/ext-3.4.0/ux/gridfilters/filter/Filter.js"></script>
  <script type="text/javascript" src="resources/ext-3.4.0/ux/gridfilters/filter/StringFilter.js"></script>
  <script type="text/javascript" src="resources/ext-3.4.0/ux/gridfilters/filter/DateFilter.js"></script>
  <script type="text/javascript" src="resources/ext-3.4.0/ux/gridfilters/filter/ListFilter.js"></script>
  <script type="text/javascript" src="resources/ext-3.4.0/ux/gridfilters/filter/NumericFilter.js"></script>
  <script type="text/javascript" src="resources/ext-3.4.0/ux/gridfilters/filter/BooleanFilter.js"></script>
  <script type="text/javascript" src="resources/ext-3.4.0/ux/paging-toolbar-resizer.js"></script>
  <script type="text/javascript" src="resources/ext-3.4.0/ux/Ext.ux.form.ReadonlyField.txt"></script>
  
  <!--
  <link rel="stylesheet" type="text/css" href="http://localhost:8081/docs/ext-3.4.0/resources/css/ext-all.css" />  
  <link rel="stylesheet" type="text/css" href="http://localhost:8081/docs/ext-3.4.0/resources/css/xtheme-gray.css" /> 
  <link rel="stylesheet" type="text/css" href="resources/css/exchange-manager-gray.css"/>
  <script type="text/javascript" src="http://localhost:8081/docs/ext-3.4.0/adapter/ext/ext-base.js"></script> 
  <script type="text/javascript" src="http://localhost:8081/docs/ext-3.4.0/ext-all.js"></script>
  
  <link rel="stylesheet" type="text/css" href="http://localhost:8081/docs/ext-3.4.0/ux/gridfilters/css/GridFilters.css" />
  <link rel="stylesheet" type="text/css" href="http://localhost:8081/docs/ext-3.4.0/ux/gridfilters/css/RangeMenu.css" />
  <script type="text/javascript" src="http://localhost:8081/docs/ext-3.4.0/ux/gridfilters/menu/RangeMenu.js"></script>
  <script type="text/javascript" src="http://localhost:8081/docs/ext-3.4.0/ux/gridfilters/menu/ListMenu.js"></script>
  <script type="text/javascript" src="http://localhost:8081/docs/ext-3.4.0/ux/gridfilters/GridFilters.js"></script>
  <script type="text/javascript" src="http://localhost:8081/docs/ext-3.4.0/ux/gridfilters/filter/Filter.js"></script>
  <script type="text/javascript" src="http://localhost:8081/docs/ext-3.4.0/ux/gridfilters/filter/StringFilter.js"></script>
  <script type="text/javascript" src="http://localhost:8081/docs/ext-3.4.0/ux/gridfilters/filter/DateFilter.js"></script>
  <script type="text/javascript" src="http://localhost:8081/docs/ext-3.4.0/ux/gridfilters/filter/ListFilter.js"></script>
  <script type="text/javascript" src="http://localhost:8081/docs/ext-3.4.0/ux/gridfilters/filter/NumericFilter.js"></script>
  <script type="text/javascript" src="http://localhost:8081/docs/ext-3.4.0/ux/gridfilters/filter/BooleanFilter.js"></script>
  <script type="text/javascript" src="http://localhost:8081/docs/ext-3.4.0/ux/paging-toolbar-resizer.js"></script>
  <script type="text/javascript" src="http://localhost:8081/docs/ext-3.4.0/ux/Ext.ux.form.ReadonlyField.txt"></script>
  -->
  
  <script type="text/javascript" src="resources/scripts/iringtools-commons.js"></script>
  <script type="text/javascript" src="resources/scripts/exchange-manager.js"></script>
</head>

<body>
  <div id="header" class="exchangeBanner">
    <span style="float:left">
      <img src="resources/images/iringlogo-big.png" 
         style="margin:0 0 0 11px; vertical-align:-20%">         
            <span style="margin:0 0 0 6px;"><font size="5px"
        style="font-family: Arial, Helvetica, Sans-Serif">Exchange Manager</font></span>
    </span>
    <span style="float:right;margin:18px 36px 1px 0"><a
        href="http://iringug.org/wiki/index.php?title=IRINGTools" target="_blank"  class="headerLnkBlack">Help</a>&nbsp;&nbsp;&nbsp;&nbsp;<a 
        id="about-link" href="#" class="headerLnkBlack">About</a>
    </span>
  </div>
  </body>
</html>