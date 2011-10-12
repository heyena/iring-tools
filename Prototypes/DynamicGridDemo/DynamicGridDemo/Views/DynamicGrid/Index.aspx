<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
  <title>Dynamic Grid Demo</title>
  
  <link rel="stylesheet" type="text/css" href="<%: Url.Content("~/scripts/ext-3.3.1/resources/css/ext-all.css") %>" />
  <script type="text/javascript" src="<%: Url.Content("~/scripts/ext-3.3.1/adapter/ext/ext-base.js") %>"></script>  
  <script type="text/javascript" src="<%: Url.Content("~/scripts/ext-3.3.1/ext-all.js") %>"></script>
  
  <link rel="stylesheet" type="text/css" href="<%: Url.Content("~/scripts/ext-3.3.1/ux/gridfilters/css/GridFilters.css") %>" />
  <link rel="stylesheet" type="text/css" href="<%: Url.Content("~/scripts/ext-3.3.1/ux/gridfilters/css/RangeMenu.css") %>" />
  <script type="text/javascript" src="<%: Url.Content("~/scripts/ext-3.3.1/ux/gridfilters/GridFilters.js") %>"></script>
  <script type="text/javascript" src="<%: Url.Content("~/scripts/ext-3.3.1/ux/gridfilters/menu/ListMenu.js") %>"></script>
  <script type="text/javascript" src="<%: Url.Content("~/scripts/ext-3.3.1/ux/gridfilters/menu/RangeMenu.js") %>"></script>
  <script type="text/javascript" src="<%: Url.Content("~/scripts/ext-3.3.1/ux/gridfilters/filter/BooleanFilter.js") %>"></script>
  <script type="text/javascript" src="<%: Url.Content("~/scripts/ext-3.3.1/ux/gridfilters/filter/DateFilter.js") %>"></script>
  <script type="text/javascript" src="<%: Url.Content("~/scripts/ext-3.3.1/ux/gridfilters/filter/Filter.js") %>"></script>
  <script type="text/javascript" src="<%: Url.Content("~/scripts/ext-3.3.1/ux/gridfilters/filter/ListFilter.js") %>"></script>
  <script type="text/javascript" src="<%: Url.Content("~/scripts/ext-3.3.1/ux/gridfilters/filter/NumericFilter.js") %>"></script>
  <script type="text/javascript" src="<%: Url.Content("~/scripts/ext-3.3.1/ux/gridfilters/filter/StringFilter.js") %>"></script>

  <script type="text/javascript" src="<%: Url.Content("~/scripts/ext-3.3.1/ux/paging-toolbar-resizer.js") %>"></script>

  <script type="text/javascript" src="<%: Url.Content("~/scripts/dynamic-grid-demo.js") %>"></script>
</head>

<body>
  <div id="grid-div"></div>
</body>

</html>
