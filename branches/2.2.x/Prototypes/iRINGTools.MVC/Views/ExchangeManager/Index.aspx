<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<html>
<head id="Head1" runat="server">
    <title>Exchange</title>
    <link rel="stylesheet" type="text/css" href="../../Scripts/ext-3.2.1/resources/css/ext-all.css"/>
    <link rel="stylesheet" type="text/css" href="../../Content/css/AdapterManager.css"/>    

    <!-- ExtJS library: base/adapter -->
    <script src="../../Scripts/ext-3.2.1/adapter/ext/ext-base.js" type="text/javascript"></script>
    
    <!-- ExtJS library: all widgets -->
    <script src="../../Scripts/ext-3.2.1/ext-all.js" type="text/javascript"></script>
    
    <!-- extensions -->    
    <script src="../../Scripts/iringtools/iRINGTools.js" type="text/javascript"></script>
    <script src="../../Scripts/iringtools/ExchangePanel.js" type="text/javascript"></script>          
    

    <!-- page specific -->    
    <script src="../../Scripts/iringtools/ExchangeManager.js" type="text/javascript"></script>    
</head>
<body>    
    <div id="header" class="banner">
        <h1><img src="../../Content/img/iRINGTools_logo.png" />&nbsp; Exchange Manager</h1>
    </div>    
</body>
</html>

