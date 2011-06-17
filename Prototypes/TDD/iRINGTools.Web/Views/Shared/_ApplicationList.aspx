<%@ Page Language="C#" AutoEventWireup="true" Inherits="ViewPage<IList<Application>>" %>
<ul class="application-list">
  <%foreach (Application application in Model) { %>
  <li>
    <h3><%=Html.ActionLink<ApplicationController>(x => x.Show(application.Id), application.Name) %></h3>
    <ul>
      <li><%=Html.ActionLink<ConfigurationController>(x => x.Show(application.Configuration.Id), "Configuration") %></li>
      <li><%=Html.ActionLink<DictionaryController>(x => x.Show(application.Dictionary.Id), "Dictionary") %></li>
      <li><%=Html.ActionLink<MappingController>(x => x.Show(application.Mapping.Id), "Mapping") %></li>
    </ul>
  </li>
  <%} %>
</ul>
