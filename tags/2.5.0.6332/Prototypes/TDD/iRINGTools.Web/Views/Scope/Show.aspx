<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="ViewPage<Scope>" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<div id="scope-details">
  <%Html.RenderPartial("ScopeDisplay", Model, ViewData);%>
  <div>
  <%foreach (Application application in Model.Applications) {%>
    <h3><%=application.Name%></h3>
    <p>
      <%=application.Description %>
    </p>
  <%}%>
  </div>
</div>
</asp:Content>
