<%@ Page Language="C#" AutoEventWireup="true" %>

<%@ Register Assembly="System.Web.Silverlight" Namespace="System.Web.UI.SilverlightControls"
    TagPrefix="asp" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" style="height:100%;">
<head id="Head1" runat="server">
    <title>ControlPanel</title>
</head>
<body style="height:100%;margin:0;" bgcolor="#CCFFFF">
    <form id="form1" runat="server" style="height:100%;">
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <div  style="height:100%;">
            <asp:Silverlight ID="Xaml1" runat="server" Source="~/ClientBin/ControlPanel.xap" MinimumVersion="2.0.31005.0" Width="100%" Height="100%" 
            InitParams="BasePath=http://localhost:59572/ControlPanel.Web/" />
        </div>
    </form>
        <form id="generateFileForm" action="DownloadFile.aspx" method="post">
        <input runat="server" type="hidden" id="downloadData" />
        <input runat="server" type="hidden" id="fileName" />
    </form>
</body>
</html>