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
      try
      {
        CompletedEventArgs = (CompletedEventArgs)parameter;

        if (CompletedEventArgs.CheckForType(CompletedEventType.GetTemplate))
        {
          if (CompletedEventArgs.Error != null)
          {
            MessageBox.Show(CompletedEventArgs.FriendlyErrorMessage, "Get Template Error", MessageBoxButton.OK);
            return;
          }
          QMXF qmxf = null;
          qmxf = (QMXF)CompletedEventArgs.Data;
          qmxf.sourceRepository = this.Entity.Repository;
          TemplateDefinition = qmxf.templateDefinitions.Where(c=>c.repositoryName == qmxf.sourceRepository).FirstOrDefault();
          TemplateQualification = qmxf.templateQualifications.Where(c => c.repositoryName == qmxf.sourceRepository).FirstOrDefault();

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
          if (CompletedEventArgs.Error != null)
          {
            MessageBox.Show(CompletedEventArgs.FriendlyErrorMessage, "Get Class Label Error", MessageBoxButton.OK);
            return;
          }

          DisplayAndSaveLabel(CompletedEventArgs.Data);
        }
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
    }

    public override void nodeMouseLeftButtonUpHandler(object sender, MouseButtonEventArgs e)
    {
      try
      {
        if (!isProcessed)
        {
          id = Entity.Uri.GetIdFromUri();

          ReferenceDataService.GetTemplate(id, this);

          Logger.Log(string.Format("node_selected in {0} executing GetTemplate for {1}", GetType().FullName, id),
              Category.Debug, Priority.None);
        }

        UpdateModel();
        FillTemplateDetailView();
        e.Handled = true;
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
    }

    private void FillTemplateDetailView()
    {
      try
      {
        PresentationModel.SelectedTreeItem = this;
        PresentationModel.DetailProperties.Clear();

        if (Tag == null)
          return;

        if (Tag is Entity)
        {
          Entity = (Entity)Tag;
        }
        else
        {
          KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>("Repository", Entity.Repository);
          PresentationModel.DetailProperties.Add(keyValuePair);

          keyValuePair = new KeyValuePair<string, string>("URI", Entity.Uri);
          PresentationModel.DetailProperties.Add(keyValuePair);

          if (Tag is TemplateDefinition)
          {
            TemplateDefinition templateDefinition = (TemplateDefinition)Tag;

            PresentationModel.SelectedNodeType = NodeType.TemplateDefinition;

            keyValuePair = new KeyValuePair<string, string>("QMXF Type", "Template Definition");
            PresentationModel.DetailProperties.Add(keyValuePair);

            keyValuePair = new KeyValuePair<string, string>("Name", (templateDefinition.name.FirstOrDefault() != null ? templateDefinition.name.FirstOrDefault().value : string.Empty));
            PresentationModel.DetailProperties.Add(keyValuePair);

            //GetClassLabel("Identifier", templateDefinition.identifier);

            keyValuePair = new KeyValuePair<string, string>("Identifier", (templateDefinition.identifier != null ? templateDefinition.identifier.ToString() : string.Empty));
            PresentationModel.DetailProperties.Add(keyValuePair);

            keyValuePair = new KeyValuePair<string, string>("Description", (templateDefinition.description.FirstOrDefault() != null ? templateDefinition.description.FirstOrDefault().value : string.Empty));
            PresentationModel.DetailProperties.Add(keyValuePair);

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

            PresentationModel.SelectedNodeType = NodeType.TemplateQualification;

            keyValuePair = new KeyValuePair<string, string>("QMXF Type", "Template Qualification");
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
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
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
          templateNode.Items.Add(AddRoleTreeItem(roleDefinitions.name[0].value ?? "[null]", Entity, roleDefinitions));
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
          templateNode.Items.Add(AddRoleTreeItem(roleQualification.name[0].value ?? "[null]", Entity, roleQualification));
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


