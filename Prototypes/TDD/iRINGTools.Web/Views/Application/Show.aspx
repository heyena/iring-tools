<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="ViewPage<Application>" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<div id="application-details">
  <%Html.Partial("ApplicationDisplay", Model, ViewData);%>
</div>
</asp:Content>
