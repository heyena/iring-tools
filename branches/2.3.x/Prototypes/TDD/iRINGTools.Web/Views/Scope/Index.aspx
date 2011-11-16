<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="ViewPage<Scope>" %>
<asp:Content ID="indexContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="contentTextDisplay">
        <h1>Welcome to the iRINGTools Adapter</h1>
        <%=Html.ActionLink<ScopeController>(x => x.Index(null), "View the scopes")%>
     </div>
</asp:Content>