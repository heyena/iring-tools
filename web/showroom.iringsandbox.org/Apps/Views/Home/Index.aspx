<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<html>
  <head>
    <meta http-equiv="cache-control" content="no-cache"/>
    <link rel="stylesheet" type="text/css" href="<%=ResolveUrl("~/content/css/iring-tools.css") %>"/>
    <title>iRINGTools Version 2.02.00</title>
  </head>
  <body>
    <div class="banner">
    <h1>
      <img src="<%=ResolveUrl("~/content/img/iring-tools-logo.png") %>" />&nbsp; Version 2.01.00</h1>
    </div>      
    <div class="main">
      <p>iRINGTools is a set of free, public domain, open source (BSD 3 license) software applications and utilities that 
         implement iRING protocols. iRINGTools provide users with production ready deployable solutions. iRINGTools also 
         provides technology solution providers with usage patterns for the implementation of iRING protocols in their respective 
         solutions.</p><br>
      <p>The iRINGTools open source software was created to provide users with a deployable implementation of ISO 15926 
      	 services. With iRINGTools you can browse and extend ISO 15926 reference data, map an application schema to the<br>
      	 ISO 15926 reference data, and transform an application's data into an ISO 15926 representation. iRINGTools can perform 
      	 these functions via the following services:</p><br>
      <h2>iRINGTools Applications</h2>
      <ul>
      <li><a href="services/sandbox/sparql/query">Sandbox SPARQL Query</a></li>
      <li><a href="<%=ResolveUrl("~/RefDataEditor") %>">Reference Data Editor</a></li>
      <li><a href="<%=ResolveUrl("~/AdapterManager") %>">Adapter Manager</a></li>
      <li><a href="services/facade/sparql/query">Facade SPARQL Query</a></li>
	    </ul>
    </div>
  </body>
</html>
