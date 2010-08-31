﻿<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<html>
<head runat="server">
    <title>Scope Editor</title>
    <link rel="stylesheet" type="text/css" href="~/Scripts/ext-3.2.1/resources/css/ext-all.css"/>
    <link rel="stylesheet" type="text/css" href="../../Content/css/scopeeditor.css"/>

    <!-- ExtJS library: base/adapter -->
    <script src="../../Scripts/ext-3.2.1/adapter/ext/ext-base.js" type="text/javascript"></script>
    
    <!-- ExtJS library: all widgets -->
    <script src="../../Scripts/ext-3.2.1/ext-all.js" type="text/javascript"></script>
    
    <!-- extensions -->
    <script src="../../Scripts/iringtools/iRINGTools.js" type="text/javascript"></script>
    <script src="../../Scripts/iringtools/ScopeDetails.js" type="text/javascript"></script>    
    <script src="../../Scripts/iringtools/ScopeMapping.js" type="text/javascript"></script>    
    <script src="../../Scripts/ext-3.2.1/ux/ux-all.js" type="text/javascript"></script>   

    <!-- page specific -->    
    <script src="../../Scripts/iringtools/ScopeEditor.js" type="text/javascript"></script>    
</head>
<body>
    <div id="header" class="banner">
      <h1><img src="../../Content/img/iRINGTools_logo.png" />&nbsp; Scope Editor</h1>
    </div>
    </body>
</html>
