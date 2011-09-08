<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<html>
<head id="Head1" runat="server">
    <title>Adapter Manager</title>
  <link href="../../Content/css/adaptermanager-gray.css" rel="stylesheet" type="text/css" />
<%-- <link href="../../Content/css/adaptermanager.css" rel="stylesheet" type="text/css" />--%>
<%--  <link href="../../Scripts/extjs40/resources/css/ext-all.css" rel="stylesheet" type="text/css" />--%>
  <link href="../../Scripts/extjs40/resources/css/ext-all-gray.css" rel="stylesheet"
    type="text/css" />
  <script src="<%: Url.Content("~/Scripts/extjs40/ext-all-debug.js") %>" type="text/javascript"></script>
  <script src="<%: Url.Content("~/Scripts/adaptermanager/ux/PagingToolbar.js") %>" type="text/javascript"></script>
  <script src="<%: Url.Content("~/Scripts/adaptermanager/sppidConfigWizard.js") %>" type="text/javascript"></script>
  <script src="<%: Url.Content("~/Scripts/adaptermanager/ValueListPanel.js") %>" type="text/javascript"></script>
  <script src="<%: Url.Content("~/Scripts/adaptermanager/ValueListMapPanel.js") %>" type="text/javascript"></script>
  <script src="<%: Url.Content("~/Scripts/adaptermanager/SpreadsheetLibrary.js") %>" type="text/javascript"></script>
  <script src="<%: Url.Content("~/Scripts/adaptermanager/SearchPanel.js") %>" type="text/javascript"></script>
  <script src="<%: Url.Content("~/Scripts/adaptermanager/ScopePanel.js") %>" type="text/javascript"></script>
  <script src="<%: Url.Content("~/Scripts/adaptermanager/MappingPanel.js") %>" type="text/javascript"></script>
  <script src="<%: Url.Content("~/Scripts/adaptermanager/GraphPanel.js") %>" type="text/javascript"></script>
  <script src="<%: Url.Content("~/Scripts/adaptermanager/ApplicationPanel.js") %>" type="text/javascript"></script>
  <script src="<%: Url.Content("~/Scripts/adaptermanager/DirectoryPanel.js") %>" type="text/javascript"></script>
  <script src="<%: Url.Content("~/Scripts/adaptermanager/DataGridPanel.js") %>" type="text/javascript"></script>
  <script src="<%: Url.Content("~/Scripts/adaptermanager/App.js") %>" type="text/javascript"></script>

  
</head>
<body>    
    <div id="header" class="exchangeBanner">
    <span style="float:left">
      <img alt="q" src="<%: Url.Content("~/Content/img/iRINGTools_logo.png") %>" 
         style="margin:0 0 0 11px; vertical-align:-20%"/>         
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
