<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="ViewPage<Application>" %>
<asp:Content ID="indexContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="contentTextDisplay">
        <h1>Welcome to the iRINGTools Adapter</h1>
        <%=Html.ActionLink<ApplicationController>(x => x.Index(null), "View the applications")%>
     </div>
</asp:Content>