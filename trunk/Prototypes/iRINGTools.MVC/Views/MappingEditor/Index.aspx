<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Index.aspx.cs" Inherits="MappingEditor1" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" style="height:100%;">
<head id="Head1" runat="server">
    <meta http-equiv="cache-control" content="no-cache"/>
    <title>MappingEditor</title>
    <script type="text/javascript">
    function onBeforeUnloadAction() {
         return "Did you save your changes!!!........";
    }
    window.onbeforeunload = function () {
        if ((window.event.clientX < 0) || (window.event.clientY < 0)) {
             return onBeforeUnloadAction();
        }
    } 
    </script>
</head>
<body style="height:100%;margin:0;">
    <form id="form1" runat="server" style="height:100%;">
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <div id="silverlightControlHost">
        <object id="Xaml1" data="data:application/x-silverlight-2," type="application/x-silverlight-2" width="100%" height="100%">
		  <param name="source" value="ClientBin/MappingEditor.xap"/>
		  <param name="onError" value="onSilverlightError" />
		  <param name="background" value="white" />
		  <param name="minRuntimeVersion" value="4.0.50401.0" />
		  <param name="autoUpgrade" value="true" />          
		  <a href="http://go.microsoft.com/fwlink/?LinkID=149156&v=4.0.50401.0" style="text-decoration:none">
 			  <img src="http://go.microsoft.com/fwlink/?LinkId=161376" alt="Get Microsoft Silverlight" style="border-style:none"/>
		  </a>
	    </object>
        <iframe id="_sl_historyFrame" style="visibility:hidden;height:0px;width:0px;border:0px"></iframe>
        </div>         
    </form>
</body>
</html>
