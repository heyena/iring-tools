<%@ page language="java" contentType="text/html; charset=ISO-8859-1" pageEncoding="ISO-8859-1"%>
<%@ taglib uri="/WEB-INF/iringtools.tld" prefix="it" %> 
<html>
  <head>
    <meta http-equiv="cache-control" content="no-cache">
    <link rel="stylesheet" type="text/css" href="resources/css/iring-tools.css">
    <title>iRINGTools Core</title>
  </head>
  <body>
    <div class="banner">
	    <h1>
	      <img src="resources/images/iringlogo-huge.png" />&nbsp;Version <it:version />
	    </h1>
    </div>
    <div class="main">
      <p>
      iRINGTools is a set of free, public domain, open source (BSD 3 license) 
      software applications and utilities that implement iRING protocols. 
      iRINGTools provide users with production ready deployable solutions. 
      iRINGTools also provides technology solution providers with usage patterns 
      for the implementation of iRING protocols in their respective solutions.
      </p>
      <br/>
      <p>
      The iRINGTools open source software was created to provide users with a 
      deployable implementation of ISO 15926 services. With iRINGTools you can 
      browse and extend ISO 15926 reference data, map an application schema to the
      <br/>
      ISO 15926 reference data, and transform an application's data into an ISO 
      15926 representation. iRINGTools can perform these functions via the 
      following services:
      </p>
      <br/>
	  <table width="100%">
        <tr valign=top>
          <td>
            <h2>iRINGTools Core Tools</h2>
            <ul>
              <li>
			    <a href="xchmgr">
			    Exchange Manager</a>
			  </li>
			  <!--<li>
			    <a href="fedmgr">
			    Federation Manager</a>
			  </li>-->
            </ul>
          </td>
          <td>
            <h2>iRINGTools Core Services</h2>
            <ul>
				<li>
				  <a href="../services/dir?_wadl&_type=xml">
				  Directory Service</a>
				</li>
				<li>
				  <a href="../services/history?_wadl&_type=xml">
				  History Service</a>
				</li>
				<!--<li>
				  <a href="../services/refdata?_wadl&_type=xml">
				  RefData Service</a>
				</li>-->
				<li>
				  <a href="../services/esb?_wadl&_type=xml">
				  ESB Service (Temporary)</a>
				</li>
            </ul>
          </td>
        </tr>
      </table>
    </div>
  </body>
</html>
