<%@ Control Language="C#" Inherits="ViewUserControl<Scope>" %>
<div class="scope-wrap">
  <div class="scope-info">
    <div class="name">
      <label><%=Html.LabelFor(x => x.Name) %></label>
      <%=Html.DisplayFor(x => x.Name) %>
    </div>
    <div class="description">
      <label><%=Html.LabelFor(x => x.Description) %></label>
      <%=Html.DisplayFor(x => x.Description) %>
    </div>
    <%if (this.UserIsAdmin()){ %>
        <%=Html.ActionLink<ScopeController>(x=>x.Edit(ViewData.Model.Id),"Manage") %>
    <%} %>
  </div>
</div>
