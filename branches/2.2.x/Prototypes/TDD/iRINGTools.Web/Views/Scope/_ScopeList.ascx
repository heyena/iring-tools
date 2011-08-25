<%@ Control Language="C#" Inherits="ViewUserControl<IList<iRINGTools.Data.Scope>>" %>
<ul class="scope-list">
    <%foreach (Scope scope in ViewData.Model) { %>
    <li>
    <h3><%=Html.ActionLink<ScopeController>(x => x.Show(scope.Id), scope.Name) %></h3>
    </li>
    <h3><%Html.RenderAction<ScopeController>(x => x.ApplicationList(scope.Id)); %></h3>
    <%} %>
</ul>
