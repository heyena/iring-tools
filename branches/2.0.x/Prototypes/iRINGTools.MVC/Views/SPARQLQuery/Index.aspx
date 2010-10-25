<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Index.aspx.cs" Inherits="dotnetRDFInterfaceService._Default"
  ValidateRequest="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <meta http-equiv="cache-control" content="no-cache" />
  <link href="../../Content/css/interface.css" rel="stylesheet" type="text/css" />
  <title>iRINGTools SPAQRL Query</title>
</head>
<body>
  <div class="banner">
    <h1>
      <img src="../../Content/img/iRINGTools_logo.png" />
      SPARQL Query</h1>
  </div>
  <form id="form1" runat="server">
  <div style="width: 734px; height: 516px">
    <asp:Label ID="Label5" runat="server" Font-Names="Arial Black" Font-Size="Small"
      Text="SPARQL Endpoint URI:"></asp:Label>
    <asp:TextBox ID="txtDefaultGraph0" Columns="100" runat="server" Width="534px" >http://localhost:54321/InterfaceService/query</asp:TextBox>
    <br />
    <asp:TextBox ID="txtQuery" runat="server" Rows="15" Columns="100" TextMode="MultiLine"
      Height="257px" Width="688px">PREFIX rdl: &lt;http://rdl.rdlfacade.org/data#&gt;
PREFIX tpl: &lt;http://tpl.rdlfacade.org/data#&gt;
PREFIX rdl: &lt;http://rdl.rdlfacade.org/data#&gt;
PREFIX owl: &lt;http://www.w3.org/2002/07/owl#&gt;
PREFIX rdf: &lt;http://www.w3.org/1999/02/22-rdf-syntax-ns#&gt;
PREFIX rdfs: &lt;http://www.w3.org/2000/01/rdf-schema#&gt;
PREFIX xsd: &lt;http://www.w3.org/2001/XMLSchema#&gt;
PREFIX ex: &lt;http://www.example.com/data#&gt;  
SELECT *
WHERE
 {?s ?p ?o}
    </asp:TextBox>
    <br />
    <asp:Label ID="Label2" runat="server" Font-Names="Arial Black" Font-Size="Small"
      Text="Default Graph URI:"></asp:Label>
    &nbsp;<asp:TextBox ID="txtDefaultGraph" Columns="100" runat="server" Width="534px" />
    <br />
    <br />
    <asp:Label ID="Label1" runat="server" Font-Names="Arial Black" Font-Size="Small"
      Text="Timeout"></asp:Label>
    &nbsp;
    <asp:TextBox ID="txtTimeout" Text="5000" runat="server" />
    <asp:Label ID="Label4" runat="server" Font-Names="Arial Black" Font-Size="Small"
      Text="Milliseconds"></asp:Label>
    <br />
    <br />
    <asp:CheckBox ID="chkPartialResults" Checked="true" runat="server" Text="Partial Results on Timeout?"
      Font-Names="Arial Black" Font-Size="Small" />
    <br />
    <br />
    <asp:Button ID="btnQuery" Text="Execute Query" runat="server" OnClick="btnQuery_Click" />
  </div>
  </form>
</body>
</html>
