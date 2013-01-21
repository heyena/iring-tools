﻿<%@ Page Language="C#" %>

<html>
  <head>
    <meta http-equiv="cache-control" content="no-cache">
    <link rel="stylesheet" type="text/css" href="css/iring-tools.css">
    <title>iRINGTools Version 2.0</title>
  </head>
  <body>
    <div class="banner">
      <h1><img src="img/iring-tools-logo.png"/>&nbsp; Version 2.0</h1>
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
      <h2>iRINGTools Services</h2>
      <ul>
		<li><a href="<%=ResolveUrl("~/SandboxService/query") %>">Sandbox Service</a></li>
		<li><a href="RefDataService/help">Reference Data Service</a></li>
		<li><a href="NHibernateService/help">NHibernate Service</a></li>
    <li><a href="AdapterService/help">Adapter Service</a></li>
    <li><a href="data/help">Data Service</a></li>
    <li><a href="dxfr/help">Data Transfer Service</a></li>
    <li><a href="<%=ResolveUrl("~/InterfaceService/query") %>">Façade Service</a></li>
	  </ul>
    </div>
  </body>
</html> 