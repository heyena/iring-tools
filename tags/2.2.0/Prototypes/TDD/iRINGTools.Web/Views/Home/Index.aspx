<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>
<asp:Content ID="indexContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="contentTextDisplay">
        <h1>Welcome to the iRINGTools Adapter</h1>
        <%=Html.ActionLink<ScopeController>(x => x.Index(null), "View the scopes") %>
     </div>
</asp:Content>
