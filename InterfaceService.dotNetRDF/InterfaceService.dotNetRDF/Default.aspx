<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SPARQLEndpointDemo._Default" ValidateRequest="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <meta http-equiv="cache-control" content="no-cache">
    <link rel="stylesheet" type="text/css" href="interface.css">
    <title>iRING Interface</title>
</head>
<body>
    <div class="banner">
      <h1><img src="iRINGTools_logo.png" alt="iRING Logo"/> Interface Service</h1>
    </div>
    <form id="form1" runat="server">
    <div style="width: 734px; height: 423px">
        <asp:TextBox ID="txtQuery" runat="server" Rows="15" Columns="100" 
            TextMode="MultiLine" Height="257px" Width="688px">PREFIX rdl: &lt;http://rdl.rdlfacade.org/data#&gt;
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
        Default Graph URI: <asp:TextBox ID="txtDefaultGraph" 
            Text="http://iring.hatch.com.au/12345_000/PSPID/Lines" Columns="100" 
            runat="server" />
        <br />
        Timeout: <asp:TextBox ID="txtTimeout" Text="5000" runat="server" /> Milliseconds
        <br />
        <asp:CheckBox ID="chkPartialResults" Checked="true" runat="server" Text="Partial Results on Timeout?" />
        <br />
        <br />
        <asp:Button ID="btnQuery" Text="Execute Query" runat="server" 
            onclick="btnQuery_Click" />
    </div>
    </form>
</body>
</html>
