<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<html>
<head runat="server">
    <title>Adapter Manager</title>
    <link rel="stylesheet" type="text/css" href="../../Scripts/ext-3.2.1/resources/css/ext-all.css"/>
    <link rel="stylesheet" type="text/css" href="../../Content/css/AdapterManager.css"/>    

    <!-- ExtJS library: base/adapter -->
    <script src="../../Scripts/ext-3.2.1/adapter/ext/ext-base.js" type="text/javascript"></script>
    
    <!-- ExtJS library: all widgets -->
    <script src="../../Scripts/ext-3.2.1/ext-all.js" type="text/javascript"></script>
    
    <!-- extensions -->    
    <script src="../../Scripts/iringtools/iRINGTools.js" type="text/javascript"></script>    
    <script src="../../Scripts/iringtools/ActionPanel.js" type="text/javascript"></script>
    <script src="../../Scripts/iringtools/NavigationPanel.js" type="text/javascript"></script>
    <script src="../../Scripts/iringtools/MappingPanel.js" type="text/javascript"></script>
    <script src="../../Scripts/iringtools/SearchPanel.js" type="text/javascript"></script>
    <script src="../../Scripts/iringtools/ScopePanel.js" type="text/javascript"></script>
    <script src="../../Scripts/iringtools/ExchangePanel.js" type="text/javascript"></script>          
    <script src="../../Scripts/ext-3.2.1/ux/ux-all.js" type="text/javascript"></script>

    <!-- page specific -->    
    <script src="../../Scripts/iringtools/AdapterManager.js" type="text/javascript"></script>    
</head>
<body>    
    <div id="header" class="banner">
        <h1><img src="../../Content/img/iRINGTools_logo.png" />&nbsp; Adapter Manager</h1>
    </div>
    <ul id="scope-actions" class="x-hidden"> 
	<li id="new-scope"> 
		<img src="content/img/s.gif" class="icon-add"/>
		<a id="action-new-scope" href="#">Add a scope</a> 
	</li>
    <li id="remove-scope"> 
		<img src="content/img/s.gif" class="icon-add"/>
		<a id="action-remove-scope" href="#">Remove selected scope</a> 
	</li> 	
    <li id="new-application">
		<img src="content/img/s.gif" class="icon-add"/> 
		<a id="action-new-application" href="#">Add an application</a> 
	</li>
    <li id="remove-application"> 
		<img src="content/img/s.gif" class="icon-add"/>
		<a id="action-remove-application" href="#">Remove selected application</a> 
	</li> 	
    <li id="edit-application">
		<img src="content/img/s.gif" class="icon-add"/> 
		<a id="action-configure-application" href="#">Configure selected application</a> 
	</li>    
    <li id="Li3">
		<img src="content/img/s.gif" class="icon-add"/>
		<a id="action-map-application" href="#">Map selected application</a>
	</li>
</ul>
</body>
</html>
