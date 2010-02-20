using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Microsoft.Practices.Unity;
using PrismContrib.Errors;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Composite.Events;

using org.iringtools.modulelibrary.events;
using org.iringtools.modulelibrary.types;
using org.iringtools.modulelibrary.extensions;
using org.iringtools.modulelibrary.layerdal;

using org.iringtools.informationmodel.events;
using org.iringtools.informationmodel.types;

using org.ids_adi.iring.referenceData;
using org.iringtools.utility;
using org.ids_adi.qmxf;
using System.Linq;

using org.iringtools.ontologyservice.presentation.presentationmodels;
using org.iringtools.library;

namespace org.iringtools.informationmodel.usercontrols
{
  public class TemplateTreeItem : CustomTreeItem
  {

    public TemplateDefinition TemplateDefinition { get; set; }
    public TemplateQualification TemplateQualification { get; set; }

    public override bool CanExecute(object parameter)
    {
      return true;
    }

    public override void Execute(object parameter)
    {
      CompletedEventArgs = (CompletedEventArgs)parameter;

      if (CompletedEventArgs.CheckForType(CompletedEventType.GetTemplate))
      {
        QMXF qmxf = (QMXF)CompletedEventArgs.Data;

        TemplateDefinition = qmxf.templateDefinitions.FirstOrDefault();
        TemplateQualification = qmxf.templateQualifications.FirstOrDefault();

        if (TemplateDefinition != null)
        {
          Tag = TemplateDefinition;
          List<RoleDefinition> roleDefinitions = TemplateDefinition.roleDefinition;
          LoadRoleDefinitions(this, roleDefinitions);
        }

        if (TemplateQualification != null)
        {
          Tag = TemplateQualification;
          List<RoleQualification> roleQualifications = TemplateQualification.roleQualification;
          LoadRoleQualifications(this, roleQualifications);
        }         

        this.IsExpanded = false;
        this.isProcessed = true;
        
        //if (this.IsSelectionActive)
        //{
          UpdateModel(qmxf);
        //}
        FillTemplateDetailView();
        
      }
      else if (CompletedEventArgs.CheckForType(CompletedEventType.GetClassLabel))
      {
        DisplayAndSaveLabel(CompletedEventArgs.Data);
      }
    }

    public override void nodeMouseLeftButtonUpHandler(object sender, MouseButtonEventArgs e)
    {
      if (!isProcessed)
      {
        id = Entity.uri.GetIdFromUri();

        ReferenceDataService.GetTemplate(id, this);

        Logger.Log(string.Format("node_selected in {0} executing GetTemplate for {1}", GetType().FullName, id),
            Category.Debug, Priority.None);
      }

      UpdateModel();
      FillTemplateDetailView();
      e.Handled = true;
    }

    private void FillTemplateDetailView()
    {
        PresentationModel.SelectedTreeItem = this;
        PresentationModel.DetailProperties.Clear();

      if (Tag == null)
        return;

      if (Tag is Entity)
      {
        Entity entity = (Entity)Tag;
        KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>("Label", entity.label);
        PresentationModel.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Uri", entity.uri);
        PresentationModel.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Repository", entity.repository);
        PresentationModel.DetailProperties.Add(keyValuePair);
      }
      else if (Tag is TemplateDefinition)
      {
        TemplateDefinition templateDefinition = (TemplateDefinition)Tag;

        KeyValuePair<string, string> list = new KeyValuePair<string, string>("QMXF Type", "Template Definition");
        PresentationModel.DetailProperties.Add(list);

        list = new KeyValuePair<string, string>("Name", (templateDefinition.name.FirstOrDefault() != null ? templateDefinition.name.FirstOrDefault().value : string.Empty));
        PresentationModel.DetailProperties.Add(list);

        //GetClassLabel("Identifier", templateDefinition.identifier);

        list = new KeyValuePair<string, string>("Identifier", (templateDefinition.identifier != null ? templateDefinition.identifier.ToString() : string.Empty));
        PresentationModel.DetailProperties.Add(list);

        list = new KeyValuePair<string, string>("Description", (templateDefinition.description.FirstOrDefault() != null ? templateDefinition.description.FirstOrDefault().value : string.Empty));
        PresentationModel.DetailProperties.Add(list);

        string statusClass = templateDefinition.status.FirstOrDefault() != null ? templateDefinition.status.FirstOrDefault().Class : string.Empty;
        PresentationModel.DetailProperties.Add(new KeyValuePair<string, string>("Status Class", statusClass));

        string statusAuthority = templateDefinition.status.FirstOrDefault() != null ? templateDefinition.status.FirstOrDefault().authority : string.Empty;
        PresentationModel.DetailProperties.Add(new KeyValuePair<string, string>("Status Authority", statusAuthority));

        string statusFrom = templateDefinition.status.FirstOrDefault() != null ? templateDefinition.status.FirstOrDefault().from : string.Empty;
        PresentationModel.DetailProperties.Add(new KeyValuePair<string, string>("Status From", statusFrom));

        string statusTo = templateDefinition.status.FirstOrDefault() != null ? templateDefinition.status.FirstOrDefault().to : string.Empty;
        PresentationModel.DetailProperties.Add(new KeyValuePair<string, string>("Status To", statusTo));

      }
      else if (Tag is TemplateQualification)
      {
        TemplateQualification templateQualification = (TemplateQualification)Tag;

        KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>("QMXF Type", "Template Qualification");
        PresentationModel.DetailProperties.Add(keyValuePair);

        keyValuePair = new KeyValuePair<string, string>("Name", (templateQualification.name.FirstOrDefault() != null ? templateQualification.name.FirstOrDefault().value : string.Empty));
        PresentationModel.DetailProperties.Add(keyValuePair);

        GetClassLabel("Identifier", templateQualification.identifier);

        keyValuePair = new KeyValuePair<string, string>("Description", (templateQualification.description.FirstOrDefault() != null ? templateQualification.description.FirstOrDefault().value : string.Empty));
        PresentationModel.DetailProperties.Add(keyValuePair);

        GetClassLabel("Qualifies", templateQualification.qualifies);

        string statusClass = templateQualification.status.FirstOrDefault() != null ? templateQualification.status.FirstOrDefault().Class : string.Empty;
        GetClassLabel("Status Class", statusClass);

        string statusAuthority = templateQualification.status.FirstOrDefault() != null ? templateQualification.status.FirstOrDefault().authority : string.Empty;
        GetClassLabel("Status Authority", statusAuthority);

        string statusFrom = templateQualification.status.FirstOrDefault() != null ? templateQualification.status.FirstOrDefault().from : string.Empty;
        GetClassLabel("Status From", statusFrom);

        string statusTo = templateQualification.status.FirstOrDefault() != null ? templateQualification.status.FirstOrDefault().to : string.Empty;
        GetClassLabel("Status To", statusTo);
      }
    }

    //Search results node process
    #region METHOD: LoadRoleDefinitions(TemplateTreeItem templateNode, List<RoleDefinitions> roles)
    /// <summary>
    /// Loads the search results into the applicable treeview .
    /// </summary>
    /// <param name="templateNode">The Template Node.</param>
    /// <param name="roles">The roles.</param>
    private void LoadRoleDefinitions(TemplateTreeItem templateNode, List<RoleDefinition> roles)
    {
      try
      {
        // Add first level Search result nodes
        foreach (RoleDefinition roleDefinitions in roles)
        {
          Entity entity = new Entity
          {
            label = roleDefinitions.name[0].value ?? "[null]",
            repository = "UnKnown",
            uri = ""//roleDefinitions.qualifies
          };

          templateNode.Items.Add(AddRoleTreeItem(entity.label, entity, roleDefinitions));
        }
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
    }
    #endregion
    #region METHOD: LoadRoleQualifications(TemplateTreeItem templateNode, List<RoleQualification> roles)
    /// <summary>
    /// Loads the search results into the applicable treeview .
    /// </summary>
    /// <param name="templateNode">The Template Node.</param>
    /// <param name="roles">The roles.</param>
    private void LoadRoleQualifications(TemplateTreeItem templateNode, List<RoleQualification> roles)
    {
      try
      {
        // Add first level Search result nodes
        foreach (RoleQualification roleQualification in roles)
        {
          Entity entity = new Entity
          {
            label = roleQualification.name[0].value ?? "[null]",
            repository = "UnKnown",
            uri = ""//roleQualification.qualifies
          };

          templateNode.Items.Add(AddRoleTreeItem(entity.label, entity, roleQualification));          
        }
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
    }
    #endregion

  }
}


