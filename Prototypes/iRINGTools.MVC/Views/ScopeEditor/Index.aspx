<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html>
<head runat="server">
    <title>Scope Editor</title>
    <link rel="stylesheet" type="text/css" href="~/Scripts/ext-3.2.1/resources/css/ext-all.css"/>

    <!-- ExtJS library: base/adapter -->
    <script src="../../Scripts/ext-3.2.1/adapter/ext/ext-base.js" type="text/javascript"></script>
    
    <!-- ExtJS library: all widgets -->
    <script src="../../Scripts/ext-3.2.1/ext-all.js" type="text/javascript"></script>
    
    <!-- extensions -->
    <script src="../../Scripts/iringtools/iRINGTools.js" type="text/javascript"></script>
    <script src="../../Scripts/iringtools/ScopeDetails.js" type="text/javascript"></script>    
    <script src="../../Scripts/iringtools/ScopeMapping.js" type="text/javascript"></script>
    <script src="../../Scripts/ext-3.2.1/ux/SearchField.js" type="text/javascript"></script>

    <!-- page specific -->    
    <script src="../../Scripts/iringtools/ScopeEditor.js" type="text/javascript"></script>    
</head>
<body>
    <div id="header"><h1>Script Editor</h1></div>
    <div id="layout"></div>
</body>
</html>
