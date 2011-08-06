﻿<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<html>
<head id="Head1" runat="server">
    <title>Adapter Manager</title>
    <link rel="stylesheet" type="text/css" href="../../Scripts/ext-3.3.1/resources/css/ext-all.css"/>
    <link rel="stylesheet" type="text/css" href="../../Scripts/ext-3.3.1/examples/ux/css/ux-all.css"/>
    <link rel="stylesheet" type="text/css" href="../../Scripts/ext-3.3.1/ux/multiselect/Multiselect.css">
    <link rel="stylesheet" type="text/css" href="../../Scripts/ext-3.3.1/resources/css/xtheme-gray.css" /> 
    <link rel="stylesheet" type="text/css" href="../../Content/css/AdapterManager-gray.css"/> 
    <!-- ExtJS library: base/adapter -->
    <script src="<%: Url.Content("~/Scripts/ext-3.3.1/adapter/ext/ext-base.js") %>" type="text/javascript"></script>
    <script src="<%: Url.Content("~/Scripts/iringtools/AdapterManager/json2.js") %>" type="text/javascript"></script>
    <!-- ExtJS library: all widgets -->
    <!--<script src="<%: Url.Content("~/Scripts/ext-3.3.1/ext-all.js") %>" type="text/javascript"></script>-->
    <script src="<%: Url.Content("~/Scripts/ext-3.3.1/ext-all-debug-w-comments.js") %>" type="text/javascript"></script>
    <script src="<%: Url.Content("~/Scripts/ext-3.3.1/examples/ux/ux-all.js") %>" type="text/javascript"></script>
    <script src="<%: Url.Content("~/Scripts/ext-3.3.1/examples/ux/ComboListAutoSizer.js") %>" type="text/javascript"></script>
    <!-- extensions -->      
    <script src="<%: Url.Content("~/Scripts/iringtools/AdapterManager/App.js") %>" type="text/javascript"></script>    
    <script src="<%: Url.Content("~/Scripts/iringtools/AdapterManager/ActionPanel.js") %>" type="text/javascript"></script>
    <script src="<%: Url.Content("~/Scripts/iringtools/AdapterManager/DirectoryPanel.js") %>" type="text/javascript"></script>
    <script src="<%: Url.Content("~/Scripts/iringtools/AdapterManager/MappingPanel.js") %>" type="text/javascript"></script>
    <script src="<%: Url.Content("~/Scripts/iringtools/AdapterManager/SearchPanel.js") %>" type="text/javascript"></script>
    <script src="<%: Url.Content("~/Scripts/iringtools/AdapterManager/ScopePanel.js") %>" type="text/javascript"></script>
    <script src="<%: Url.Content("~/Scripts/iringtools/AdapterManager/ValueListPanel.js") %>" type="text/javascript"></script>
    <script src="<%: Url.Content("~/Scripts/iringtools/AdapterManager/ValueListMapPanel.js") %>" type="text/javascript"></script> 
    <script src="<%: Url.Content("~/Scripts/iringtools/AdapterManager/GraphPanel.js") %>" type="text/javascript"></script>                 
    <script src="<%: Url.Content("~/Scripts/iringtools/AdapterManager/ApplicationPanel.js") %>" type="text/javascript"></script>
    <script src="<%: Url.Content("~/Scripts/iringtools/AdapterManager/AjaxRowExpander.js") %>" type="text/javascript"></script>    
    <script src="<%: Url.Content("~/Scripts/iringtools/AdapterManager/ExcelLibrary.js") %>" type="text/javascript"></script>
    <script src="<%: Url.Content("~/Scripts/iringtools/AdapterManager/NHibernateConfigWizard.js") %>" type="text/javascript"></script>
    <script src="<%: Url.Content("~/Scripts/iringtools/AdapterManager/HrefItem.js") %>" type="text/javascript"></script>
    <script src="<%: Url.Content("~/Scripts/iringtools/AdapterManager/Utility.js") %>" type="text/javascript"></script>
    <!-- page specific -->    
    <script src="<%: Url.Content("~/Scripts/iringtools/AdapterManager/AdapterManager.js") %>" type="text/javascript"></script>    
</head>
<body>    
    <div id="header" class="exchangeBanner">
    <span style="float:left">
      <img src="<%: Url.Content("~/Content/img/iRINGTools_logo.png") %>" 
         style="margin:0 0 0 11px; vertical-align:-20%">         
            <span style="margin:0 0 0 6px;"><font size="5px"
        style="font-family: Arial, Helvetica, Sans-Serif">Adapter Manager</font></span>
    </span>
    <span style="float:right;margin:18px 36px 1px 0"><a
        href="http://iringug.org/wiki/index.php?title=IRINGTools" target="_blank"  class="headerLnkBlack">Help</a>&nbsp;&nbsp;&nbsp;&nbsp;<a 
        id="about-link" href="#" class="headerLnkBlack">About</a>
    </span>
  </div>    
</body>
</html>
