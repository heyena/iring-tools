<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Index.aspx.cs" Inherits="MappingEditor1" %>

<%@ Register Assembly="System.Web.Silverlight" Namespace="System.Web.UI.SilverlightControls"
    TagPrefix="asp" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" style="height:100%;">
<head id="Head1" runat="server">
    <meta http-equiv="cache-control" content="no-cache"/>
    <title>MappingEditor</title>
    <script src="../../Scripts/Silverlight.js" type="text/javascript"></script>
        <script type="text/javascript">
          function onSilverlightError(sender, args) {
            var appSource = "";
            if (sender != null && sender != 0) {
              appSource = sender.getHost().Source;
            }

            var errorType = args.ErrorType;
            var iErrorCode = args.ErrorCode;

            if (errorType == "ImageError" || errorType == "MediaError") {
              return;
            }

            var errMsg = "Unhandled Error in Silverlight Application " + appSource + "\n";

            errMsg += "Code: " + iErrorCode + "    \n";
            errMsg += "Category: " + errorType + "       \n";
            errMsg += "Message: " + args.ErrorMessage + "     \n";

            if (errorType == "ParserError") {
              errMsg += "File: " + args.xamlFile + "     \n";
              errMsg += "Line: " + args.lineNumber + "     \n";
              errMsg += "Position: " + args.charPosition + "     \n";
            }
            else if (errorType == "RuntimeError") {
              if (args.lineNumber != 0) {
                errMsg += "Line: " + args.lineNumber + "     \n";
                errMsg += "Position: " + args.charPosition + "     \n";
              }
              errMsg += "MethodName: " + args.methodName + "     \n";
            }

            throw new Error(errMsg);
          }
    </script>
 <script type="text/javascript">
   function onBeforeUnloadAction() {
     return "You are about to leave this page.  Did you save your changes?";
   }
   window.onbeforeunload = function () {
     if ((window.event.clientX < 0) ||
      (window.event.clientY < 0)) {
       return onBeforeUnloadAction();
     }
   } 
 </script>

</head>
<body style="height:100%;margin:0;">
    <form id="form1" runat="server" style="height:100%;">
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <div  style="height:100%;">
            <asp:Silverlight ID="Xaml1" runat="server" Source="~/ClientBin/MappingEditor.xap" MinimumVersion="2.0.31005.0" Width="100%" Height="99%" />
        </div>
    </form>
</body>
</html>