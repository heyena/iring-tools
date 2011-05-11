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

using org.ids_adi.qmxf;
using System.Linq;

using org.iringtools.ontologyservice.presentation.presentationmodels;
using org.iringtools.utility;
using org.iringtools.modulelibrary.usercontrols;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using org.iringtools.library;
using System.Windows.Threading;

namespace org.iringtools.informationmodel.usercontrols
{
  public class ClassTreeItem : CustomTreeItem
  {
    CustomTreeItem superclasses = null;
    CustomTreeItem subclasses = null;
    CustomTreeItem templates = null;
    CustomTreeItem members = null;
    CustomTreeItem classifications = null;

    public ClassDefinition ClassDefinition { get; set; }

    public override void nodeMouseLeftButtonUpHandler(object sender, MouseButtonEventArgs e)
    {
      try
      {
        if (!isProcessed)
        {
          Logger.Log(string.Format("node_selected in {0} executing GetQMXF for {1}", GetType().FullName, id),
              Category.Debug, Priority.None);

          ReferenceDataService.GetClass(id, this);
        }

        UpdateModel();
        FillClassDetailView();
        e.Handled = true;
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
    }

    private void FillClassDetailView()
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
        else if (Tag is ClassDefinition)
        {
          ClassDefinition classDefinition = (ClassDefinition)Tag;

          PresentationModel.SelectedNodeType = NodeType.Class;

          KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>("Repository", Entity.Repository);
          PresentationModel.DetailProperties.Add(keyValuePair);

          keyValuePair = new KeyValuePair<string, string>("URI", Entity.Uri);
          PresentationModel.DetailProperties.Add(keyValuePair);

          keyValuePair = new KeyValuePair<string, string>("QMXF Type", "Class Definition");
          PresentationModel.DetailProperties.Add(keyValuePair);

          keyValuePair = new KeyValuePair<string, string>("Name", (classDefinition.name.FirstOrDefault() != null ? classDefinition.name.FirstOrDefault().value : string.Empty));
          PresentationModel.DetailProperties.Add(keyValuePair);

          keyValuePair = new KeyValuePair<string, string>("Identifier", (classDefinition.identifier != null ? classDefinition.identifier.ToString() : string.Empty));
          PresentationModel.DetailProperties.Add(keyValuePair);

          keyValuePair = new KeyValuePair<string, string>("Entity Type", (classDefinition.entityType != null ? classDefinition.entityType.reference : string.Empty));
          PresentationModel.DetailProperties.Add(keyValuePair);

          keyValuePair = new KeyValuePair<string, string>("Description", (classDefinition.description.FirstOrDefault() != null ? classDefinition.description.FirstOrDefault().value : string.Empty));
          PresentationModel.DetailProperties.Add(keyValuePair);

          string statusClass = classDefinition.status.FirstOrDefault() != null ? classDefinition.status.FirstOrDefault().Class : string.Empty;
          PresentationModel.DetailProperties.Add(new KeyValuePair<string, string>("Status Class", statusClass));

          string statusAuthority = classDefinition.status.FirstOrDefault() != null ? classDefinition.status.FirstOrDefault().authority : string.Empty;
          PresentationModel.DetailProperties.Add(new KeyValuePair<string, string>("Status Authority", statusAuthority));

          string statusFrom = classDefinition.status.FirstOrDefault() != null ? classDefinition.status.FirstOrDefault().from : string.Empty;
          PresentationModel.DetailProperties.Add(new KeyValuePair<string, string>("Status From", statusFrom));

          string statusTo = classDefinition.status.FirstOrDefault() != null ? classDefinition.status.FirstOrDefault().to : string.Empty;
          PresentationModel.DetailProperties.Add(new KeyValuePair<string, string>("Status To", statusTo));
        }
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
    }

    /// <summary>
    /// Handles the selected event of the superclasses control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    public void superclasses_selected(object sender, System.Windows.RoutedEventArgs e)
    {
      try
      {
        if (superclasses.isProcessed)
          return;

        Logger.Log(string.Format("superclasses_selected in {0}", GetType().FullName),
            Category.Debug, Priority.None);
        if (ClassDefinition == null)
        {
          superclasses.itemTextBlock.Text = "[Super Classes] (0)";
          return;
        }
        List<Specialization> specializations = ClassDefinition.specialization;

        foreach (Specialization specialization in specializations)
        {
          string label = specialization.label;

          Entity entity = new Entity
          {
            Label = label ?? "[null]",
            Repository = "UnKnown",
            Uri = specialization.reference,
          };
          superclasses.Items.Add(AddTreeItem(label, entity));
        }

        superclasses.itemTextBlock.Text = "[Super Classes] (" + specializations.Count + ")";
        superclasses.IsExpanded = false;
        superclasses.isProcessed = true;
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }

    }

    /// <summary>
    /// Handles the selected event of the classifications control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    public void classifications_selected(object sender, System.Windows.RoutedEventArgs e)
    {

      string language = string.Empty;
      List<string> names = new List<string>();
      try
      {
        if (classifications.isProcessed)
          return;

        Logger.Log(string.Format("classifications_selected in {0}", GetType().FullName),
            Category.Debug, Priority.None);

        if (ClassDefinition == null)
        {
          classifications.itemTextBlock.Text = "[Member Of] (0)";
          return;
        }
        List<Classification> classification = ClassDefinition.classification;

        foreach (Classification classificationDetails in classification)
        {
          names = classificationDetails.label.Split('@').ToList();
          if (names.Count == 1)
            language = "en";
          else
            language = names[names.Count - 1];

          Entity entity = new Entity
          {
            Label = names[0] ?? "[null]",
            Repository = ClassDefinition.repositoryName,
            Lang = language,
            Uri = classificationDetails.reference,
          };
          classifications.Items.Add(AddTreeItem(names[0], entity));
        }

        classifications.itemTextBlock.Text = "[Member Of] (" + classification.Count + ")";
        classifications.IsExpanded = false;
        classifications.isProcessed = true;
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
    }

    /// <summary>
    /// Handles the selected event of the subclasses control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    public void subclasses_selected(object sender, System.Windows.RoutedEventArgs e)
    {
      try
      {
        if (subclasses.isProcessed)
          return;

        Logger.Log(string.Format("subclasses_selected in {0}", GetType().FullName),
            Category.Debug, Priority.None);

        ReferenceDataService.GetSubClasses(id, this);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
    }

    #region METHOD: Execute()
    /// <summary>
    /// Executes the specified parameter.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    public override void Execute(object parameter)
    {
      try
      {
        CompletedEventArgs = (CompletedEventArgs)parameter;
        Logger.Log(string.Format("Execute for type {0} in {1}",
            CompletedEventArgs.CompletedType,
            GetType().FullName), Category.Debug, Priority.None);

        if (CompletedEventArgs.CheckForType(CompletedEventType.GetClass))
        {
          if (CompletedEventArgs.Error != null)
          {
            MessageBox.Show(CompletedEventArgs.FriendlyErrorMessage, "Get Class Error", MessageBoxButton.OK);
            return;
          }

          QMXF qmxf = (QMXF)CompletedEventArgs.Data;
          qmxf.sourceRepository = this.Entity.Repository;
          ClassDefinition classDefinition = qmxf.classDefinitions.Where(c => c.repositoryName == qmxf.sourceRepository).FirstOrDefault();

          ProcessClassDefinition(classDefinition);

          //GvJ this is breaking the load of the qmxf of the selected item
          //if (this.IsSelectionActive)
          //{
          UpdateModel(qmxf);
          //}

          FillClassDetailView();
        }
        else if (CompletedEventArgs.CheckForType(CompletedEventType.GetSubClasses))
        {
          if (CompletedEventArgs.Error != null)
          {
            MessageBox.Show(CompletedEventArgs.FriendlyErrorMessage, "Get SubClasses Error", MessageBoxButton.OK);
            return;
          }

          List<Entity> entities = (List<Entity>)CompletedEventArgs.Data;

          foreach (Entity entity in entities)
            subclasses.Items.Add(AddTreeItem(entity.Label, entity));

          subclasses.itemTextBlock.Text = "[Sub Classes] (" + entities.Count + ")";
          subclasses.IsExpanded = false;
          subclasses.isProcessed = true;

        }
        else if (CompletedEventArgs.CheckForType(CompletedEventType.GetClassTemplates))
        {
          if (CompletedEventArgs.Error != null)
          {
            MessageBox.Show(CompletedEventArgs.FriendlyErrorMessage, "Get ClassTemplates Error", MessageBoxButton.OK);
            return;
          }

          List<Entity> entities = (List<Entity>)CompletedEventArgs.Data;

          foreach (Entity entity in entities)
            templates.Items.Add(AddTreeItem(entity.Label, entity));

          templates.itemTextBlock.Text = "[Templates] (" + entities.Count + ")";
          templates.IsExpanded = false;
          templates.isProcessed = true;
        }
        else if (CompletedEventArgs.CheckForType(CompletedEventType.GetClassMembers))
        {
          if (CompletedEventArgs.Error != null)
          {
            MessageBox.Show(CompletedEventArgs.FriendlyErrorMessage, "Get Members Error", MessageBoxButton.OK);
            return;
          }

          List<Entity> entities = (List<Entity>)CompletedEventArgs.Data;

          foreach (Entity entity in entities)
            members.Items.Add(AddTreeItem(entity.Label, entity));

          members.itemTextBlock.Text = "[Members] (" + entities.Count + ")";
          members.IsExpanded = false;
          members.isProcessed = true;
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
    #endregion

    /// <summary>
    /// Handles the selected event of the templates control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    public void templates_selected(object sender, System.Windows.RoutedEventArgs e)
    {
      try
      {
        if (templates.isProcessed)
          return;

        Logger.Log(string.Format("templates_selected in {0}", GetType().FullName),
                Category.Debug, Priority.None);

        ReferenceDataService.GetClassTemplates(id, this);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
    }

    public void ProcessClassDefinition(ClassDefinition classDefinition)
    {

      try
      {
        //Image image = null;
        ClassDefinition = classDefinition;
        Tag = classDefinition;

        classifications = Container.Resolve<CustomTreeItem>();

        classifications.SetImageSource("folder.png");
        classifications.SetTooltipText("Classification Collection...");
        classifications.itemTextBlock.Text = "[Member Of]";
        // classifications.Header = "Classifications";

        members = Container.Resolve<CustomTreeItem>();
        members.SetImageSource("folder.png");
        members.SetTooltipText("Class Members Collection...");
        members.itemTextBlock.Text = "[Members]";
        members.Selected += members_selected;
        this.Items.Add(members);

        classifications.Selected += classifications_selected;
        this.Items.Add(classifications);
        superclasses = Container.Resolve<CustomTreeItem>();

        superclasses.SetImageSource("folder.png");
        superclasses.SetTooltipText("Super Class Collection...");
        superclasses.SetTextBlockText("[Super Classes]");

        // superclasses.Header = "Super Classes";
        superclasses.Selected += superclasses_selected;
        this.Items.Add(superclasses);

        subclasses = Container.Resolve<CustomTreeItem>();
        subclasses.SetTooltipText("Sub Class Collection...");
        subclasses.SetImageSource("folder.png");
        subclasses.SetTextBlockText("[Sub Classes]");

        //subclasses.Header = "Sub Classes";
        subclasses.Selected += subclasses_selected;
        this.Items.Add(subclasses);

        templates = Container.Resolve<CustomTreeItem>();
        templates.SetTooltipText("Template Collection...");
        templates.SetImageSource("folder.png");
        templates.SetTextBlockText("[Templates]");

        // templates.Header = "Templates";
        templates.Selected += templates_selected;
        this.Items.Add(templates);

        isProcessed = true;
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
    }

    public void members_selected(object sender, System.Windows.RoutedEventArgs e)
    {
      try
      {
        if (members.isProcessed)
          return;

        Logger.Log(string.Format("members_selected in {0}", GetType().FullName),
            Category.Debug, Priority.None);

        ReferenceDataService.GetClassMembers(id, this);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
    }
  }
}
