<%@ page language="java" contentType="text/html; charset=ISO-8859-1" pageEncoding="ISO-8859-1"%>
<%@ taglib prefix="s" uri="/struts-tags"%>
<%@ taglib prefix="sx" uri="/struts-dojo-tags" %> 
<!DOCTYPE html PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.org/TR/html4/loose.dtd">
<html>
<head>
  <sx:head />
  <meta http-equiv="Content-Type" content="text/html; charset=ISO-8859-1">
  <title>Struts 2 Demo</title>
</head>
<body>
<h1>Struts 2 Demo</h1><br/>

<!-- URL presentation http://localhost:8080/struts2demo/dti?serviceURL=http://localhost:54321/dto/12345_000/ABC/Lines -->
<s:url id="dtiUrl" action="dti">
  <s:param name="serviceURL">http://localhost:54321/dto/12345_000/ABC/Lines</s:param>
</s:url>
<sx:div id="div1" theme="ajax" href="%{dtiUrl}"></sx:div>

</body>
</html>