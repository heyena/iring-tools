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
            ValueList valueList = (ValueList)parentMap.Tag;

            int idx = parentMap.Items.IndexOf(itemMap);
            if (idx != parentMap.Items.Count - 1)
            {
              parentMap.Items.Remove(itemMap);
              parentMap.Items.Insert(idx + 1, itemMap);
              itemMap.Focus();
            }

            idx = valueList.valueMaps.IndexOf(valueMap);
            if (idx != valueList.valueMaps.Count - 1)
            {
              valueList.valueMaps.Remove(valueMap);
              valueList.valueMaps.Insert(idx + 1, valueMap);
            }
            Presenter.ButtonCtrl("btnSave").IsEnabled = true;
            Presenter.ButtonCtrl("btnSave").Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
          }
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Failed to Move Down", "MOVE DOWN", MessageBoxButton.OK);
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
            ValueList valueList = (ValueList)parentMap.Tag;

            int idx = parentMap.Items.IndexOf(itemMap);
            if (idx > 0)
            {
              parentMap.Items.Remove(itemMap);
              parentMap.Items.Insert(idx - 1, itemMap);
              itemMap.Focus();
            }

            idx = valueList.valueMaps.IndexOf(valueMap);
            if (idx > 0)
            {
              valueList.valueMaps.Remove(valueMap);
              valueList.valueMaps.Insert(idx - 1, valueMap);
            }
            Presenter.ButtonCtrl("btnSave").IsEnabled = true;
            Presenter.ButtonCtrl("btnSave").Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
          }
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Failed to Move Up", "MOVE UP", MessageBoxButton.OK);
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
        else if (!Regex.IsMatch(internalValue, @"^[A-Za-z_]+\w*$"))
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
          if (!Regex.IsMatch(valueListName, @"^[A-Za-z_]+\w*$"))
          {
            MessageBox.Show("ValueList value is invalid", "ADD VALUELIST", MessageBoxButton.OK);
          }

          else
          {
            ValueList valueList = new ValueList { name = valueListName, valueMaps = new List<ValueMap>() };
            mapping.valueLists.Add(valueList);
            tvwValues.Items.Add(Presenter.AddNode(valueListName, valueList, null));
            Presenter.ButtonCtrl("btnSave").IsEnabled = true;
            Presenter.ButtonCtrl("btnSave").Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));

          }
        }
      }
      catch (Exception ex) 
      {
        MessageBox.Show("Failed to Add Value List", "ADD VALUELIST", MessageBoxButton.OK);
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
          if (!Regex.IsMatch(internalValue, @"^[A-Za-z_]+\w*$"))
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
            ValueList valueList = (ValueList)model.SelectedMappingItem.Tag;
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
      }

    }

    public void btnAddGraph_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        TextBox txtLabel = sender as TextBox;
        string graphName = txtLabel.Text;

        if (!Regex.IsMatch(graphName, @"^[A-Za-z_]+\w*$"))
        {
          MessageBox.Show("Graph name is invalid", "ADD GRAPH", MessageBoxButton.OK);
        }
        else if (model.SelectedIMUri == null ||
            SPARQLExtensions.GetObjectTypeFromUri(model.SelectedIMUri) != SPARQLPrefix.ObjectType.Class)
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

          string dataObjectMap = model.SelectedDataObject.DataObject.objectName;
          ClassMap classMap = new ClassMap
              {
                name = model.SelectedIMLabel,
                classId = SPARQLExtensions.GetIdWithAliasFromUri(model.SelectedIMUri)
              };

          classMap.identifiers.Add(string.Format("{0}.{1}", dataObjectMap, model.SelectedDataObject.DataProperty.propertyName));

          graphMap.AddClassMap(null, classMap);

          if (String.IsNullOrEmpty(graphMap.dataObjectMap))
            graphMap.dataObjectMap = dataObjectMap;

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
      }
    }

    private string AdjustTemplateName(MappingItem mappingItem, string templateName)
    {
      try
      {
        string tplName = utility.Utility.NameSafe(templateName);


        if (mappingItem.ClassMap != null && mappingItem.TemplateMap != null)
        {
          int lastSeqNum = 0;

          // find last sequence number of template names that match the new template name
          TemplateMap templateMap = mappingItem.TemplateMap;
          {
            if (templateMap.name.StartsWith(tplName))
            {
              if (templateMap.name.Length == tplName.Length)
              {
                lastSeqNum = 1;
              }
              else
              {
                int seqNum;
                if (int.TryParse(templateMap.name.Substring(tplName.Length), out seqNum))
                {
                  if (seqNum >= lastSeqNum)
                    lastSeqNum = seqNum + 1;
                }
              }
            }
          }

          if (lastSeqNum != 0)
            tplName += lastSeqNum;
        }

        return tplName;
      }
      catch (Exception ex)
      {
        MessageBox.Show("Failed to Add Graph", "ADD GRAPH", MessageBoxButton.OK);
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
            selectedClassMap = model.SelectedGraphMap.classTemplateListMaps.Keys.FirstOrDefault();

          else if (model.SelectedMappingItem.NodeType == NodeType.ClassMap)
            selectedClassMap = model.SelectedClassMap;

          MappingItem mappingItem = model.SelectedMappingItem;

          TextBox txtLabel = sender as TextBox;
          TemplateMap templateMap = new TemplateMap();

          templateMap.name = AdjustTemplateName(mappingItem, model.SelectedIMLabel);
          templateMap.templateId = SPARQLExtensions.GetIdWithAliasFromUri(model.SelectedIMUri);
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
          templateMap.templateType = templateType;

          GetRoleMaps(selectedClassMap.classId, template, templateMap);

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
        currentTemplateMap.roleMaps = new List<RoleMap>();

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
            roleMap.type = RoleType.Property;
            roleMap.name = utility.Utility.NameSafe(roleDefinition.name.FirstOrDefault().value);
            roleMap.dataType = range;
            roleMap.propertyName = string.Empty;
            roleMap.roleId = roleDefinition.identifier.GetIdWithAliasFromUri();

            currentTemplateMap.roleMaps.Add(roleMap);
          }
          else if (range != classId && !range.StartsWith("xsd:"))
          {

            roleMap.type = RoleType.Reference;
            //roleMap.isMapped = true;
            roleMap.name = utility.Utility.NameSafe(roleDefinition.name.FirstOrDefault().value);

            roleMap.value = range;
            roleMap.propertyName = string.Empty;
            roleMap.roleId = roleDefinition.identifier.GetIdWithAliasFromUri();
            currentTemplateMap.roleMaps.Add(roleMap);
          }
          else if (range == classId)
          {
            roleMap.type = RoleType.Possessor;
            roleMap.name = utility.Utility.NameSafe(roleDefinition.name.FirstOrDefault().value);
            roleMap.dataType = range;
            roleMap.propertyName = string.Empty;
            roleMap.roleId = roleDefinition.identifier.GetIdWithAliasFromUri();
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
          roleMap.roleId = roleQualification.qualifies.GetIdWithAliasFromUri();

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
            roleMap.type = RoleType.Property;
            roleMap.dataType = range;
            roleMap.propertyName = String.Empty;
            currentTemplateMap.roleMaps.Add(roleMap);
          }
          else if (range != classId && !range.StartsWith("xsd:"))
          {
            roleMap.type = RoleType.Reference;

            roleMap.dataType = range;
            roleMap.propertyName = String.Empty;
            currentTemplateMap.roleMaps.Add(roleMap);
          }

          else if (range == classId)    // class role ??
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

        DataObjectItem parentObject = (DataObjectItem)model.SelectedDataObject.Parent;
        RoleMap roleMap = model.SelectedRoleMap;
        if (roleMap.type == RoleType.Property)
        {
          if (roleMap.dataType == null && !roleMap.dataType.StartsWith("xsd:"))
          {
            if (model.SelectedDataObject == null || model.SelectedDataObject.DataProperty == null)
            {
              MessageBox.Show("Please select a property to map", "MAP ROLE", MessageBoxButton.OK);
              return;
            }

            roleMap.propertyName =
                string.Format("{0}.{1}", model.SelectedDataObject.DataObject.objectName, model.SelectedDataObject.DataProperty.propertyName);
            roleMap.valueList = String.Empty;
          }
          else if (roleMap.dataType != null || roleMap.dataType.StartsWith("xsd:"))
          {

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
            roleMap.valueList = String.Empty;

          }


          model.SelectedMappingItem.itemTextBlock.Text = model.SelectedMappingItem.itemTextBlock.Text.Replace(Presenter.unmappedToken, "");
          model.DetailProperties.Clear();
          Presenter.RefreshRoleMap(roleMap);
        }
        else if (roleMap.type == RoleType.Reference && templateMap.templateType == TemplateType.Definition)
        {
          if (model.SelectedTreeItem == null || !(model.SelectedTreeItem is ClassTreeItem))
          {
            MessageBox.Show("Please select a class to map", "MAP ROLE", MessageBoxButton.OK);
            return;
          }
          ClassDefinition selectedClass = ((ClassTreeItem)model.SelectedTreeItem).ClassDefinition;
          roleMap.value = selectedClass.identifier.GetIdWithAliasFromUri();
          model.SelectedMappingItem.itemTextBlock.Text = model.SelectedMappingItem.itemTextBlock.Text.Replace(Presenter.unmappedToken, "");
          model.DetailProperties.Clear();
          Presenter.RefreshRoleMap(roleMap);
        }
        else if (roleMap.type == RoleType.Reference && templateMap.templateType == TemplateType.Qualification)
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
          roleMap.value = Id;
          ClassMap classMap = new ClassMap
          {
            name = utility.Utility.NameSafe(txtLabel.Text),
            classId = Id
          };
          classMap.identifiers = new List<string>();
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
        string valuelist = ((MappingItem)tvwValues.SelectedItem).itemTextBlock.Text;

        if (mappingItem.NodeType == NodeType.RoleMap)
        {
          if (model.SelectedDataObject == null || model.SelectedDataObject.DataProperty == null)
          {
            MessageBox.Show("Please select a property to map", "MAP ROLE", MessageBoxButton.OK);
          }
          else if (string.IsNullOrEmpty(valuelist))
          {
            MessageBox.Show("Please select a value list", "MAP ROLE", MessageBoxButton.OK);
          }
          else
          {
            RoleMap roleMap = mappingItem.RoleMap;

            if (roleMap.dataType != null || roleMap.dataType != "")
            {
              roleMap.type = RoleType.Property;
              roleMap.valueList = valuelist;
              if (valuelist == "")
              {
                roleMap.propertyName = null;
                model.SelectedMappingItem.itemTextBlock.Text += Presenter.unmappedToken;
              }
              else
              {
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

              model.SelectedGraphMap.DeleteTemplateMap(parent.ClassMap.classId, templateMap.templateId);

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
                mappingItem.GraphMap.DeleteRoleMap(mappingItem.TemplateMap, roleMap.classMap.classId);

              roleMap.propertyName = string.Empty;
              roleMap.valueList = string.Empty;
              roleMap.value = string.Empty;
              mappingItem.Items.Clear();
              mappingItem.IsExpanded = false;


              if (!mappingItem.itemTextBlock.Text.EndsWith(Presenter.unmappedToken) && roleMap.isMapped == false)
              {
                mappingItem.itemTextBlock.Text += Presenter.unmappedToken;
              }

              break;
            }
          case NodeType.ClassMap:
            {
              MappingItem parent = (MappingItem)mappingItem.Parent;
              RoleMap roleMap = (RoleMap)parent.Tag;
              mappingItem.GraphMap.DeleteRoleMap(mappingItem.TemplateMap, roleMap.roleId);

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
              ValueList valueList = (ValueList)mappingItem.Tag;
              mapping.valueLists.Remove(valueList);
              tvwValues.Items.Remove(mappingItem);
              break;
            }
          case NodeType.ValueMap:
            {
              MappingItem parentMap = (MappingItem)mappingItem.Parent;

              ValueMap valueMap = (ValueMap)mappingItem.Tag;
              ValueList valueList = (ValueList)parentMap.Tag;

              valueList.valueMaps.Remove(valueMap);
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
      }
    }
  }
}


