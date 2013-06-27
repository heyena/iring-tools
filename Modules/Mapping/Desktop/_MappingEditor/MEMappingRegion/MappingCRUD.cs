using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using org.iringtools.informationmodel.events;
using PrismContrib.Errors;

using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Unity;

using org.iringtools.modulelibrary.events;
using org.iringtools.modulelibrary.extensions;
using org.iringtools.modulelibrary.entities;
using org.iringtools.modulelibrary.layerdal;
using org.iringtools.modulelibrary.types;
using org.iringtools.modulelibrary.usercontrols;

using org.iringtools.ontologyservice.presentation;
using org.iringtools.ontologyservice.presentation.presentationmodels;

using org.ids_adi.qmxf;

using org.iringtools.informationmodel.usercontrols;
using org.iringtools.library;
using org.iringtools.library.configuration;
using org.iringtools.utility;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Windows.Media;
using org.iringtools.mapping;
using org.iringtools.dxfr.manifest;

namespace org.iringtools.modules.memappingregion
{
  public class MappingCRUD
  {
    private IIMPresentationModel model = null;
    private IMappingView view = null;
    private IAdapter adapterProxy = null;
    private IReferenceData referenceProxy = null;
    private IEventAggregator aggregator = null;
    private string projectName = null;
    private string applicationName = null;
    public MappingPresenter Presenter { get; set; }
    public TreeView tvwMapping { get; set; }
    public TreeView tvwValues { get; set; }
    public Mapping mapping { get; set; }
    public bool saveActive { get; set; }
    private bool popupFlag { get; set; }

    [Dependency]
    public IError Error { get; set; }

    [Dependency]
    public ILoggerFacade Logger { get; set; }

    public MappingCRUD(IMappingView view, IIMPresentationModel model,
        IAdapter adapterProxy,
        IReferenceData referenceProxy,
        IEventAggregator aggregator)
    {
      this.model = model;
      this.view = view;
      this.adapterProxy = adapterProxy;
      this.referenceProxy = referenceProxy;
      this.aggregator = aggregator;
      aggregator.GetEvent<SpinnerEvent>().Subscribe(SpinnerEventHandler);
      aggregator.GetEvent<SelectionEvent>().Subscribe(SelectionEventHandler);
    }

    public void SelectionEventHandler(SelectionEventArgs e)
    {
      this.projectName = e.SelectedProject;
      this.applicationName = e.SelectedApplication;
    }

    public void SpinnerEventHandler(SpinnerEventArgs e)
    {
      switch (e.Active)
      {
        case SpinnerEventType.Started:
          this.tvwMapping.IsEnabled = false;
          this.tvwValues.IsEnabled = false;

          break;

        case SpinnerEventType.Stopped:
          this.tvwMapping.IsEnabled = true;
          this.tvwValues.IsEnabled = true;

          break;

        default:
          break;
      }
    }

    public void btnMoveDown_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        TextBox txtLabel = sender as TextBox;
        string valueListName = txtLabel.Text;

        if (mapping != null)
        {
          if (model.SelectedMappingItem == null || model.SelectedMappingItem.NodeType != NodeType.ValueMap)
          {
            MessageBox.Show("Please select a ValueMap", "ADD VALUEMAP", MessageBoxButton.OK);
          }
          else
          {
            MappingItem itemMap = model.SelectedMappingItem;
            MappingItem parentMap = (MappingItem)itemMap.Parent;
            ValueMap valueMap = (ValueMap)itemMap.Tag;
            ValueListMap valueListMap = (ValueListMap)parentMap.Tag;

            int idx = parentMap.Items.IndexOf(itemMap);
            if (idx != parentMap.Items.Count - 1)
            {
              parentMap.Items.Remove(itemMap);
              parentMap.Items.Insert(idx + 1, itemMap);
              itemMap.Focus();
            }

            idx = valueListMap.valueMaps.IndexOf(valueMap);
            if (idx != valueListMap.valueMaps.Count - 1)
            {
              valueListMap.valueMaps.Remove(valueMap);
              valueListMap.valueMaps.Insert(idx + 1, valueMap);
            }
            Presenter.ButtonCtrl("btnSave").IsEnabled = true;
            Presenter.ButtonCtrl("btnSave").Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
          }
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Failed to Move Down.  The error has been copied to the clipboard.", "MOVE DOWN", MessageBoxButton.OK);
        Logger.Log(ex.ToString(), Category.Exception, Priority.Low);
      }
    }

    public void btnMoveUp_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        TextBox txtLabel = sender as TextBox;
        string valueListName = txtLabel.Text;

        if (mapping != null)
        {
          if (model.SelectedMappingItem == null || model.SelectedMappingItem.NodeType != NodeType.ValueMap)
          {
            MessageBox.Show("Please select a ValueMap", "ADD VALUEMAP", MessageBoxButton.OK);
          }
          else
          {
            MappingItem itemMap = model.SelectedMappingItem;
            MappingItem parentMap = (MappingItem)itemMap.Parent;
            ValueMap valueMap = (ValueMap)itemMap.Tag;
            ValueListMap valueListMap = (ValueListMap)parentMap.Tag;

            int idx = parentMap.Items.IndexOf(itemMap);
            if (idx > 0)
            {
              parentMap.Items.Remove(itemMap);
              parentMap.Items.Insert(idx - 1, itemMap);
              itemMap.Focus();
            }

            idx = valueListMap.valueMaps.IndexOf(valueMap);
            if (idx > 0)
            {
              valueListMap.valueMaps.Remove(valueMap);
              valueListMap.valueMaps.Insert(idx - 1, valueMap);
            }
            Presenter.ButtonCtrl("btnSave").IsEnabled = true;
            Presenter.ButtonCtrl("btnSave").Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
          }
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Failed to Move Up", "MOVE UP", MessageBoxButton.OK);
        Logger.Log(ex.ToString(), Category.Exception, Priority.Low);
      }
    }

    public void btnMakePossessor_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (model.SelectedMappingItem == null || model.SelectedNodeType != NodeType.RoleMap)
        {
          MessageBox.Show("Please Select a Role", "MAKE POSSESSOR", MessageBoxButton.OK);
        }
        else
        {
          MappingItem item = model.SelectedMappingItem;

          item.SetTextBlockText(item.itemTextBlock.Text.Replace(Presenter.unmappedToken, ""));
          RoleMap roleMap = (RoleMap)model.SelectedMappingItem.Tag;
          roleMap.type = RoleType.Possessor;
          roleMap.value = string.Empty;
          roleMap.dataType = string.Empty;
          Presenter.ButtonCtrl("btnSave").IsEnabled = true;
          Presenter.ButtonCtrl("btnSave").Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
          Presenter.RefreshRoleMap(roleMap);
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Failed to Make Possessor", "MAKE POSSESSOR", MessageBoxButton.OK);
        Logger.Log(ex.ToString(), Category.Exception, Priority.Low);
      }
    }

    public void btnEditValueMap_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        TextBox txtLabel = sender as TextBox;
        string internalValue = txtLabel.Text;

        if (model.SelectedMappingItem == null || model.SelectedMappingItem.NodeType != NodeType.ValueMap)
        {
          MessageBox.Show("Please select a ValueMap", "EDIT VALUEMAP", MessageBoxButton.OK);
        }
        else if (String.IsNullOrEmpty(internalValue))
        {
          MessageBox.Show("ValueMap internal value is invalid", "EDIT VALUEMAP", MessageBoxButton.OK);
        }
        else
        {

          MappingItem item = model.SelectedMappingItem;
          ValueMap valueMap = (ValueMap)model.SelectedMappingItem.Tag;
          string id = Utility.GetIdFromURI(valueMap.uri);
          valueMap.internalValue = internalValue;
          item.SetTextBlockText(model.IdLabelDictionary[id] + "[" + internalValue + "]");
          Presenter.ButtonCtrl("btnSave").IsEnabled = true;
          Presenter.ButtonCtrl("btnSave").Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
          Presenter.RefreshValueMap(valueMap, item);
          saveActive = true;
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Failed to Edit Value Map", "EDIT VALUE MAP", MessageBoxButton.OK);
        Logger.Log(ex.ToString(), Category.Exception, Priority.Low);
      }
    }

    public void btnAddValueList_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        TextBox txtLabel = sender as TextBox;
        string valueListName = txtLabel.Text;

        if (mapping != null)
        {
          if (String.IsNullOrEmpty(valueListName))
          {
            MessageBox.Show("ValueList value is invalid", "ADD VALUELIST", MessageBoxButton.OK);
          }

          else
          {
            ValueListMap valueList = new ValueListMap { name = valueListName, valueMaps = new ValueMaps() };
            mapping.valueListMaps.Add(valueList);
            tvwValues.Items.Add(Presenter.AddNode(valueListName, valueList, null));
            Presenter.ButtonCtrl("btnSave").IsEnabled = true;
            Presenter.ButtonCtrl("btnSave").Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));

          }
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Failed to Add Value List", "ADD VALUELIST", MessageBoxButton.OK);
        Logger.Log(ex.ToString(), Category.Exception, Priority.Low);
      }

    }

    public void btnAddValueMap_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        TextBox txtLabel = sender as TextBox;
        string internalValue = txtLabel.Text;

        if (mapping != null)
        {
          if (String.IsNullOrEmpty(internalValue))
          {
            MessageBox.Show("ValueMap internal value is invalid", "ADD VALUEMAP", MessageBoxButton.OK);
          }
          else if (model.SelectedIMUri == null ||
          SPARQLExtensions.GetObjectTypeFromUri(model.SelectedIMUri) != SPARQLPrefix.ObjectType.Class)
          {
            MessageBox.Show("Please select a valid class", "ADD VALUEMAP", MessageBoxButton.OK);
          }
          else if (model.SelectedMappingItem == null || model.SelectedMappingItem.NodeType != NodeType.ValueList)
          {
            MessageBox.Show("Please select a ValueList", "ADD VALUEMAP", MessageBoxButton.OK);
          }
          else
          {
            ValueListMap valueList = (ValueListMap)model.SelectedMappingItem.Tag;
            String uri = Regex.Replace(model.SelectedIMUri, ".*#", "rdl:");

            ValueMap valueMap = new ValueMap { internalValue = internalValue, uri = uri };
            valueList.valueMaps.Add(valueMap);
            model.SelectedMappingItem.Items.Add(Presenter.AddNode(uri, valueMap, model.SelectedMappingItem));
            Presenter.RefreshValueMap(valueMap, model.SelectedMappingItem);
            Presenter.ButtonCtrl("btnSave").IsEnabled = true;
            Presenter.ButtonCtrl("btnSave").Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
          }
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Failed to Add Value Map", "ADD VALUE MAP", MessageBoxButton.OK);
        Logger.Log(ex.ToString(), Category.Exception, Priority.Low);
      }

    }

    public void btnAddGraph_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        TextBox txtLabel = sender as TextBox;
        string graphName = txtLabel.Text;

        if (String.IsNullOrEmpty(graphName))
        {
          MessageBox.Show("Graph name is invalid", "ADD GRAPH", MessageBoxButton.OK);
        }
        else if (model.SelectedIMUri == null || SPARQLExtensions.GetObjectTypeFromUri(model.SelectedIMUri) != SPARQLPrefix.ObjectType.Class)
        {
          MessageBox.Show("Please select a valid class from Information Model", "ADD GRAPH", MessageBoxButton.OK);
        }
        else if (model.SelectedDataObject == null || model.SelectedDataObject.DataProperty == null)
        {
          MessageBox.Show("Please select an identifier from Data Dictionary", "ADD GRAPH", MessageBoxButton.OK);
        }
        else
        {
          GraphMap graphMap = new GraphMap();
          graphMap.name = graphName;

          string dataObjectName = model.SelectedDataObject.DataObject.objectName;
          ClassMap classMap = new ClassMap
          {
            name = model.SelectedIMLabel,
            id = SPARQLExtensions.GetIdWithAliasFromUri(model.SelectedIMUri)
          };

          classMap.identifiers.Add(string.Format("{0}.{1}", dataObjectName, model.SelectedDataObject.DataProperty.propertyName));

          graphMap.AddClassMap(null, classMap);

          if (String.IsNullOrEmpty(graphMap.dataObjectName))
            graphMap.dataObjectName = dataObjectName;

          mapping.graphMaps.Add(graphMap);
          tvwMapping.Items.Add(Presenter.AddNode(graphMap.name, graphMap, null));
          Presenter.ButtonCtrl("btnSave").IsEnabled = true;
          Presenter.ButtonCtrl("btnSave").Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));

          saveActive = true;
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Failed to Add Graph", "ADD GRAPH", MessageBoxButton.OK);
        Logger.Log(ex.ToString(), Category.Exception, Priority.Low);
      }
    }

    private string AdjustTemplateName(MappingItem mappingItem, string templateName)
    {
      try
      {
        string tplName = utility.Utility.NameSafe(templateName);


        //if (mappingItem.classMap != null && mappingItem.TemplateMap != null)
        //{
        //  int lastSeqNum = 0;

        //  // find last sequence number of template names that match the new template name
        //  TemplateMap templateMap = mappingItem.TemplateMap;
        //  {
        //    if (templateMap.name.StartsWith(tplName))
        //    {
        //      if (templateMap.name.Length == tplName.Length)
        //      {
        //        lastSeqNum = 1;
        //      }
        //      else
        //      {
        //        int seqNum;
        //        if (int.TryParse(templateMap.name.Substring(tplName.Length), out seqNum))
        //        {
        //          if (seqNum >= lastSeqNum)
        //            lastSeqNum = seqNum + 1;
        //        }
        //      }
        //    }
        //  }

        //  if (lastSeqNum != 0)
        //    tplName += lastSeqNum;
        //}

        return tplName;
      }
      catch (Exception ex)
      {
        MessageBox.Show("Failed to Add Graph", "ADD GRAPH", MessageBoxButton.OK);
        Logger.Log(ex.ToString(), Category.Exception, Priority.Low);
        return "";
      }
    }

    private string GetTemplateName(MappingItem mappingItem, string templateName)
    {
      try
      {
        string tplName = utility.Utility.NameSafe(templateName);

        return tplName;
      }
      catch (Exception ex)
      {
        MessageBox.Show("Failed to Add Graph", "ADD GRAPH", MessageBoxButton.OK);
        Logger.Log(ex.ToString(), Category.Exception, Priority.Low);
        return "";
      }
    }

    public void btnAddTemplate_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        TemplateType templateType;

        ClassMap selectedClassMap = null;
        if (model.SelectedIMUri == null ||
            SPARQLExtensions.GetObjectTypeFromUri(model.SelectedIMUri) != SPARQLPrefix.ObjectType.Template)
        {
          MessageBox.Show("Please select a valid template", "ADD TEMPLATE", MessageBoxButton.OK);
          return;
        }
        else if (model.SelectedMappingItem == null || (
            model.SelectedMappingItem.NodeType != NodeType.GraphMap &&
            model.SelectedMappingItem.NodeType != NodeType.ClassMap))
        {
          MessageBox.Show("Please select a graph map or class map", "ADD TEMPLATE", MessageBoxButton.OK);
          return;
        }
        else
        {
          if (model.SelectedMappingItem.NodeType == NodeType.GraphMap)
          {
            selectedClassMap = model.SelectedGraphMap.classTemplateMaps.FirstOrDefault().classMap;
          }
          else if (model.SelectedMappingItem.NodeType == NodeType.ClassMap)
          {
            selectedClassMap = model.SelectedClassMap;
          }

          MappingItem mappingItem = model.SelectedMappingItem;

          TextBox txtLabel = sender as TextBox;
          TemplateMap templateMap = new TemplateMap();

          //templateMap.name = AdjustTemplateName(mappingItem, model.SelectedIMLabel);
          templateMap.name = GetTemplateName(mappingItem, model.SelectedIMLabel);
          templateMap.id = SPARQLExtensions.GetIdWithAliasFromUri(model.SelectedIMUri);
          TemplateTreeItem node = null;
          if (model.SelectedTreeItem is TemplateTreeItem)
          {
            node = (TemplateTreeItem)model.SelectedTreeItem;
          }
          else
          {
            node = (TemplateTreeItem)model.SelectedTreeItem.Parent;
          }
          TemplateDefinition templateDefinition = node.TemplateDefinition;
          TemplateQualification templateQualification = node.TemplateQualification;

          object template = null;
          if (templateDefinition != null)
          {
            template = templateDefinition;
            templateType = TemplateType.Definition;
          }
          else
          {
            template = templateQualification;
            templateType = TemplateType.Qualification;
          }
          templateMap.type = templateType;

          GetRoleMaps(selectedClassMap.id, template, templateMap);

          if (mappingItem.TemplateMap == null)
            mappingItem.TemplateMap = new TemplateMap();
          mappingItem.TemplateMap = templateMap;

          model.SelectedGraphMap.AddTemplateMap(selectedClassMap, templateMap);
          Presenter.PopulateTemplateMap(mappingItem, templateMap);
          Presenter.RefreshTemplateMap(templateMap);
          Presenter.ButtonCtrl("btnSave").IsEnabled = true;
          Presenter.ButtonCtrl("btnSave").Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));

          saveActive = true;
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Failed to Add Template", "ADD TEMPLATE", MessageBoxButton.OK);
        Logger.Log(ex.ToString(), Category.Exception, Priority.Low);
      }
    }


    private string HandleSpecialCharacters(string inputString)
    {
      string pattern = "[^a-zA-Z]";
      string outputString =
      System.Text.RegularExpressions.Regex.Replace(inputString, pattern, "");
      return outputString;
    }

    private void GetRoleMaps(string classId, object template, TemplateMap currentTemplateMap)
    {
      if (currentTemplateMap.roleMaps == null)
        currentTemplateMap.roleMaps = new RoleMaps();

      if (template is TemplateDefinition)
      {
        TemplateDefinition templateDefinition = (TemplateDefinition)template;
        List<RoleDefinition> roleDefinitions = templateDefinition.roleDefinition;

        foreach (RoleDefinition roleDefinition in roleDefinitions)
        {
          string range = roleDefinition.range.GetIdWithAliasFromUri();
          RoleMap roleMap = new RoleMap();
          if (range != classId && range.StartsWith("xsd:"))
          {
            roleMap.type = RoleType.DataProperty;
            roleMap.name = utility.Utility.NameSafe(roleDefinition.name.FirstOrDefault().value);
            roleMap.dataType = range;
            roleMap.propertyName = string.Empty;
            roleMap.id = roleDefinition.identifier.GetIdWithAliasFromUri();

            currentTemplateMap.roleMaps.Add(roleMap);
          }
          else if (range != classId && !range.StartsWith("xsd:"))
          {

            roleMap.type = RoleType.ObjectProperty;
            roleMap.name = utility.Utility.NameSafe(roleDefinition.name.FirstOrDefault().value);

            roleMap.dataType = range;
            roleMap.propertyName = string.Empty;
            roleMap.id = roleDefinition.identifier.GetIdWithAliasFromUri();
            currentTemplateMap.roleMaps.Add(roleMap);
          }
          else if (range == classId)
          {
            roleMap.type = RoleType.Possessor;
            roleMap.name = utility.Utility.NameSafe(roleDefinition.name.FirstOrDefault().value);
            roleMap.dataType = range;
            roleMap.propertyName = string.Empty;
            roleMap.id = roleDefinition.identifier.GetIdWithAliasFromUri();
            currentTemplateMap.roleMaps.Add(roleMap);
          }
        }
      }
      if (template is TemplateQualification)
      {
        TemplateQualification templateQualification = (TemplateQualification)template;
        List<RoleQualification> roleQualifications = templateQualification.roleQualification;

        foreach (RoleQualification roleQualification in roleQualifications)
        {
          string range = roleQualification.range.GetIdWithAliasFromUri();
          RoleMap roleMap = new RoleMap();

          roleMap.name = utility.Utility.NameSafe(roleQualification.name.FirstOrDefault().value);
          roleMap.id = roleQualification.qualifies.GetIdWithAliasFromUri();

          if (roleQualification.value != null)  // fixed role
          {
            if (!String.IsNullOrEmpty(roleQualification.value.reference))
            {
              roleMap.type = RoleType.Reference;
              roleMap.value = roleQualification.value.reference.GetIdWithAliasFromUri();
            }
            else if (!String.IsNullOrEmpty(roleQualification.value.text))  // fixed role is a literal
            {
              roleMap.type = RoleType.FixedValue;
              roleMap.value = roleQualification.value.text;
              roleMap.dataType = roleQualification.value.As;
            }

            currentTemplateMap.roleMaps.Add(roleMap);
          }
          else if (range != classId && range.StartsWith("xsd:")) // property role
          {
            roleMap.type = RoleType.DataProperty;
            roleMap.dataType = range;
            roleMap.propertyName = String.Empty;
            currentTemplateMap.roleMaps.Add(roleMap);
          }
          else if (range != classId && !range.StartsWith("xsd:"))
          {
            roleMap.type = RoleType.ObjectProperty;

            roleMap.dataType = range;
            roleMap.propertyName = String.Empty;
            currentTemplateMap.roleMaps.Add(roleMap);
          }

          else if (range == classId)    // class role
          {
            roleMap.type = RoleType.Possessor;

            roleMap.dataType = range;
            currentTemplateMap.roleMaps.Add(roleMap);
          }
        }
      }
    }


    public void btnMap_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (model.SelectedRoleMap == null)
        {
          MessageBox.Show("Please select a role map", "MAP ROLE", MessageBoxButton.OK);
          return;
        }

        MappingItem mappingItem = model.SelectedMappingItem;
        MappingItem parent = (MappingItem)mappingItem.Parent;

        TemplateMap templateMap = (TemplateMap)parent.Tag;

        Presenter.ButtonCtrl("btnSave").IsEnabled = true;
        Presenter.ButtonCtrl("btnSave").Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
        saveActive = true;

        RoleMap roleMap = model.SelectedRoleMap;
        if (
          roleMap.type == RoleType.Property ||
          roleMap.type == RoleType.DataProperty)
        {
          if (model.SelectedDataObject == null || model.SelectedDataObject.DataProperty == null)
          {
            MessageBox.Show("Please select a data property to map", "MAP ROLE", MessageBoxButton.OK);
            return;
          }

          if (roleMap.dataType == null && !roleMap.dataType.StartsWith("xsd:"))
          {
            roleMap.propertyName =
                string.Format("{0}.{1}", model.SelectedDataObject.DataObject.objectName, model.SelectedDataObject.DataProperty.propertyName);
            roleMap.valueListName = String.Empty;
          }
          else if (roleMap.dataType != null || roleMap.dataType.StartsWith("xsd:"))
          {
            DataObjectItem parentObject = (DataObjectItem)model.SelectedDataObject.Parent;

            if (!string.IsNullOrEmpty(parentObject.ParentObjectName))
            {
              roleMap.propertyName = string.Format("{0}.{1}.{2}",
                parentObject.ParentObjectName,
                parentObject.RelationshipName,
                //              model.SelectedDataObject.DataObject.objectName,
                model.SelectedDataObject.DataProperty.propertyName);
            }
            else
            {
              roleMap.propertyName =
                  string.Format("{0}.{1}", model.SelectedDataObject.DataObject.objectName, model.SelectedDataObject.DataProperty.propertyName);
            }
          }
          else
          {
            ClassDefinition selectedClass = ((ClassTreeItem)model.SelectedTreeItem).ClassDefinition;
            //roleMap.dataType = String.Empty;
            roleMap.propertyName = String.Empty;
            roleMap.dataType = selectedClass.identifier.GetIdWithAliasFromUri();
            roleMap.valueListName = String.Empty;

          }


          model.SelectedMappingItem.itemTextBlock.Text = model.SelectedMappingItem.itemTextBlock.Text.Replace(Presenter.unmappedToken, "");
          model.DetailProperties.Clear();
          Presenter.RefreshRoleMap(roleMap);
        }
        else if (roleMap.type == RoleType.ObjectProperty)
        {
          if (model.SelectedTreeItem == null || !(model.SelectedTreeItem is ClassTreeItem))
          {
            MessageBox.Show("Please select a class to map", "MAP ROLE", MessageBoxButton.OK);
            return;
          }
          ClassDefinition selectedClass = ((ClassTreeItem)model.SelectedTreeItem).ClassDefinition;
          roleMap.type = RoleType.Reference;
          roleMap.value = selectedClass.identifier.GetIdWithAliasFromUri();
          model.SelectedMappingItem.itemTextBlock.Text = model.SelectedMappingItem.itemTextBlock.Text.Replace(Presenter.unmappedToken, "");
          model.DetailProperties.Clear();
          Presenter.RefreshRoleMap(roleMap);
        }

        aggregator.GetEvent<NavigationEvent>().Publish(new NavigationEventArgs
        {
          DetailProcess = DetailType.Mapping,
          SelectedNode = mappingItem,
          Sender = this
        });
      }
      catch (Exception ex)
      {
        MessageBox.Show("Failed to Map", "MAP", MessageBoxButton.OK);
        Logger.Log(ex.ToString(), Category.Exception, Priority.Low);
      }
    }

    public void btnMapClass_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        if (model.SelectedRoleMap == null)
        {
          MessageBox.Show("Please select a role map", "MAP ROLE", MessageBoxButton.OK);
          return;
        }

        MappingItem mappingItem = model.SelectedMappingItem;
        MappingItem parent = (MappingItem)mappingItem.Parent;

        TemplateMap templateMap = (TemplateMap)parent.Tag;

        Presenter.ButtonCtrl("btnSave").IsEnabled = true;
        Presenter.ButtonCtrl("btnSave").Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
        saveActive = true;

        DataObjectItem selectedDataObject = (DataObjectItem)model.SelectedDataObject;
        if (selectedDataObject == null || selectedDataObject.Parent.GetType() == typeof(TreeView))
        {
          MessageBox.Show("Please select a data property to map", "MAP ROLE", MessageBoxButton.OK);
          return;
        }
        DataObjectItem parentObject = (DataObjectItem)selectedDataObject.Parent;
        RoleMap roleMap = model.SelectedRoleMap;
        if (roleMap.type == RoleType.ObjectProperty)
        {
          if (model.SelectedTreeItem == null || !(model.SelectedTreeItem is ClassTreeItem))
          {
            MessageBox.Show("Please select a class to map", "MAP ROLE", MessageBoxButton.OK);
            return;
          }
          else if (model.SelectedDataSourcePropertyName == null && model.SelectedTreeItem is ClassTreeItem)
          {
            MessageBox.Show("Please select a property to map", "MAP ROLE", MessageBoxButton.OK);
            return;
          }

          ClassDefinition classDefinition = ((ClassTreeItem)model.SelectedTreeItem).ClassDefinition;
          string Id = classDefinition.identifier.GetIdWithAliasFromUri();

          roleMap = model.SelectedRoleMap;
          TextBox txtLabel = sender as TextBox;

          roleMap.type = RoleType.Reference;
          roleMap.value = Id;
          ClassMap classMap = new ClassMap
          {
            name = utility.Utility.NameSafe(txtLabel.Text),
            id = Id
          };
          classMap.identifiers = new Identifiers();
          classMap.identifiers
              .Add(string.Format("{0}.{1}", model.SelectedDataObject.DataObject.objectName, model.SelectedDataObject.DataProperty.propertyName));

          model.SelectedGraphMap.AddClassMap(roleMap, classMap);

          roleMap.classMap = classMap;

          model.SelectedMappingItem.itemTextBlock.Text = model.SelectedMappingItem.itemTextBlock.Text.Replace(Presenter.unmappedToken, "");

          Presenter.PopulateRoleNode(mappingItem, roleMap);
          model.DetailProperties.Clear();
          Presenter.RefreshRoleMap(roleMap);
        }

        aggregator.GetEvent<NavigationEvent>().Publish(new NavigationEventArgs
        {
          DetailProcess = DetailType.Mapping,
          SelectedNode = mappingItem,
          Sender = this
        });
      }
      catch (Exception ex)
      {
        MessageBox.Show("Failed to Map", "MAP", MessageBoxButton.OK);
        Logger.Log(ex.ToString(), Category.Exception, Priority.Low);
      }
    }

    public void btnAddValueList(object sender, RoutedEventArgs e)
    {
      try
      {
        MappingItem mappingItem = model.SelectedMappingItem;
        MappingItem valueList = (MappingItem)tvwValues.SelectedItem;
        if (mappingItem == null) return;

        if (valueList == null || valueList.Tag is ValueMap)
        {
          MessageBox.Show("Please select a ValueList to map", "MAP ROLE", MessageBoxButton.OK);
          return;
        }
        string valuelistName = ((MappingItem)tvwValues.SelectedItem).itemTextBlock.Text;

        if (mappingItem.NodeType == NodeType.RoleMap)
        {
          if (model.SelectedDataObject == null || model.SelectedDataObject.DataProperty == null)
          {
            MessageBox.Show("Please select a property to map", "MAP ROLE", MessageBoxButton.OK);
          }
          else if (string.IsNullOrEmpty(valuelistName))
          {
            MessageBox.Show("Please select a value list", "MAP ROLE", MessageBoxButton.OK);
          }
          else
          {
            RoleMap roleMap = mappingItem.RoleMap;

            if (roleMap.dataType != null || roleMap.dataType != "")
            {
              roleMap.valueListName = valuelistName;
              if (valuelistName == "")
              {
                //roleMap.type = RoleType.DataProperty;
                roleMap.propertyName = null;
                model.SelectedMappingItem.itemTextBlock.Text += Presenter.unmappedToken;
              }
              else
              {
                //roleMap.type = RoleType.ObjectProperty;
                roleMap.propertyName =
                string.Format("{0}.{1}",
                    model.SelectedDataObject.DataObject.objectName,
                    model.SelectedDataObject.DataProperty.propertyName);

                model.SelectedMappingItem.itemTextBlock.Text =
                    model.SelectedMappingItem.itemTextBlock.Text.Replace(Presenter.unmappedToken, "");
              }
            }
            Presenter.RefreshRoleMap(roleMap);
          }

          Presenter.ButtonCtrl("btnSave").IsEnabled = true;
          Presenter.ButtonCtrl("btnSave").Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
          saveActive = true;
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Failed to Add Value List", "ADD VALUE LIST", MessageBoxButton.OK);
        Logger.Log(ex.ToString(), Category.Exception, Priority.Low);
      }
    }

    public void btnDelete_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        MappingItem mappingItem = model.SelectedMappingItem;

        if (mappingItem == null)
          return;

        Presenter.ButtonCtrl("btnSave").IsEnabled = true;
        Presenter.ButtonCtrl("btnSave").Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));

        saveActive = true;
        switch (mappingItem.NodeType)
        {
          case NodeType.GraphMap:
            {
              TreeView parent = (TreeView)mappingItem.Parent;

              mapping.graphMaps.Remove(mappingItem.GraphMap);
              tvwMapping.Items.Remove(mappingItem);
              break;
            }
          case NodeType.TemplateMap:
            {
              MappingItem parent = (MappingItem)mappingItem.Parent;
              TemplateMap templateMap = (TemplateMap)mappingItem.Tag;

              model.SelectedGraphMap.DeleteTemplateMap(parent.ClassMap.id, templateMap);

              parent.Items.Remove(mappingItem);

              if (parent.Items.Count == 0)
                parent.IsExpanded = false;
              break;
            }
          case NodeType.RoleMap:
            {
              MappingItem parent = (MappingItem)mappingItem.Parent;
              RoleMap roleMap = (RoleMap)mappingItem.Tag;
              if (roleMap.classMap != null)
                mappingItem.GraphMap.DeleteRoleMap(mappingItem.TemplateMap, roleMap.id);

              if(!string.IsNullOrEmpty(roleMap.propertyName))
                roleMap.type = RoleType.DataProperty;
              else 
                roleMap.type = RoleType.ObjectProperty;

              roleMap.propertyName = string.Empty;
              roleMap.valueListName = string.Empty;
              roleMap.value = string.Empty;
              mappingItem.Items.Clear();
              mappingItem.IsExpanded = false;

              if (!mappingItem.itemTextBlock.Text.EndsWith(Presenter.unmappedToken) && roleMap.IsMapped() == false)
              {
                mappingItem.itemTextBlock.Text += Presenter.unmappedToken;
              }
             
              Presenter.RefreshRoleMap(roleMap);
              break;
            }
          case NodeType.ClassMap:
            {
              MappingItem parent = (MappingItem)mappingItem.Parent;
              RoleMap roleMap = (RoleMap)parent.Tag;
              mappingItem.GraphMap.DeleteRoleMap(mappingItem.TemplateMap, roleMap.id);

              parent.Items.Clear();
              parent.IsExpanded = false;
              mappingItem.ClassMap = null;

              if (!parent.itemTextBlock.Text.EndsWith(Presenter.unmappedToken) && roleMap.value == string.Empty)
              {
                parent.itemTextBlock.Text += Presenter.unmappedToken;
              }
              break;
            }
          case NodeType.ValueList:
            {
              ValueListMap valueListMap = (ValueListMap)mappingItem.Tag;
              mapping.valueListMaps.Remove(valueListMap);
              tvwValues.Items.Remove(mappingItem);
              break;
            }
          case NodeType.ValueMap:
            {
              MappingItem parentMap = (MappingItem)mappingItem.Parent;

              ValueMap valueMap = (ValueMap)mappingItem.Tag;
              ValueListMap valueListMap = (ValueListMap)parentMap.Tag;

              valueListMap.valueMaps.Remove(valueMap);
              parentMap.Items.Remove(mappingItem);
              break;
            }

        }

        aggregator.GetEvent<NavigationEvent>().Publish(new NavigationEventArgs
        {
          DetailProcess = DetailType.Mapping,
          SelectedNode = mappingItem,
          Sender = this
        });
      }
      catch (Exception ex)
      {
        MessageBox.Show("Failed to Delete", "DELETE", MessageBoxButton.OK);
        Logger.Log(ex.ToString(), Category.Exception, Priority.Low);
      }
    }

    public void btnSave_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        Response response = adapterProxy.UpdateMapping(projectName, applicationName, mapping);
        Presenter.ButtonCtrl("btnSave").IsEnabled = false;
        Presenter.ButtonCtrl("btnSave").Background = new SolidColorBrush(Color.FromArgb(255, 31, 59, 83));

        saveActive = false;
      }
      catch (Exception ex)
      {
        MessageBox.Show("Failed to Save", "SAVE", MessageBoxButton.OK);
        Logger.Log(ex.ToString(), Category.Exception, Priority.Low);
      }
    }
  }
}


