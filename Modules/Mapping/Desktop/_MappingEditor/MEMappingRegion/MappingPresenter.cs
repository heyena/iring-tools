﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;

using org.iringtools.modulelibrary.entities;
using org.iringtools.modulelibrary.events;
using org.iringtools.modulelibrary.extensions;
using org.iringtools.modulelibrary.types;
using org.iringtools.modulelibrary.layerdal;

using org.iringtools.ontologyservice.presentation.presentationmodels;
using org.iringtools.informationmodel.events;
using PrismContrib.Base;

using InformationModel.Events;
using org.iringtools.library;

#if SILVERLIGHT
using org.iringtools.modulelibrary.behaviors;
using System.Windows.Interactivity;
using System.Windows.Input;
#else
#endif


namespace org.iringtools.modules.memappingregion
{
  /// <summary>
  /// 
  /// </summary>
  public class MappingPresenter : PresenterBase<IMappingView>
  {
    private IEventAggregator aggregator = null;
    private IAdapter adapterProxy = null;
    private IReferenceData referenceDataService = null;
    IIMPresentationModel model = null;

    private TreeView tvwMapping { get { return GetControl<TreeView>("tvwMapping"); } }

    private Button btnAddTemplate { get { return ButtonCtrl("btnAddTemplate"); } }
    private Button btnAddGraph { get { return ButtonCtrl("btnAddGraph"); } }
    private Button btnMap { get { return ButtonCtrl("btnMap"); } }
    private Button btnMakeClassRole { get { return ButtonCtrl("btnMakeClassRole"); } }
    private ComboBox cbValueList { get { return ComboBoxCtrl("cbValueList"); } }
    private Button btnAddValueList { get { return ButtonCtrl("btnAddValueList"); } }
    private Button btnDelete { get { return ButtonCtrl("btnDelete"); } }
    private Button btnSave { get { return ButtonCtrl("btnSave"); } }
    private TextBox txtLabel { get { return TextCtrl("txtLabel"); } }

    public string selectedValueList { get { return ((ComboBoxItem)(cbValueList.SelectedItem)).Content.ToString(); } }
    
    // Create, Update, Delete code for Mapping Editor treeview
    private MappingCRUD mappingCRUD = null;

    public string unmappedToken { get { return " [UnMapped]"; }}
  
    /// <summary>
    /// Initializes a new instance of the <see cref="MappingPresenter"/> class.
    /// </summary>
    /// <param name="view">The view.</param>
    /// <param name="model">The model.</param>
    /// <param name="adapterProxy">The adapter proxy.</param>
    public MappingPresenter(IMappingView view, IIMPresentationModel model,
      IEventAggregator aggregator,
      IAdapter adapterProxy,
      IReferenceData referenceDataService,
      IUnityContainer container)
      : base(view, model)
    {
      try
      {
        // Create CRUD Class and configure as required
        mappingCRUD = container.Resolve<MappingCRUD>();
        mappingCRUD.Presenter = this;
        mappingCRUD.tvwMapping = tvwMapping;

        // Subcribe to button click events on mappingCRUD
        // note that we're sending in the txtLabel object as sender
        btnAddTemplate.Click += (object sender, RoutedEventArgs e) => { mappingCRUD.btnAddTemplate_Click(txtLabel, e); };
        btnAddGraph.Click += (object sender, RoutedEventArgs e) => { mappingCRUD.btnAddGraph_Click(txtLabel, e); };
        btnMap.Click += (object sender, RoutedEventArgs e) => { mappingCRUD.btnMap_Click(txtLabel, e); };
        btnMakeClassRole.Click += (object sender, RoutedEventArgs e) => { mappingCRUD.btnMakeClassRole_Click(txtLabel, e); };
        btnAddValueList.Click += (object sender, RoutedEventArgs e) => { mappingCRUD.btnAddValueList(txtLabel, e); };
        btnSave.Click += (object sender, RoutedEventArgs e) => { mappingCRUD.btnSave_Click(txtLabel, e); };
        btnDelete.Click += (object sender, RoutedEventArgs e) => { mappingCRUD.btnDelete_Click(txtLabel, e); };

#if SILVERLIGHT
        MouseScrollBehavior mouseScrollBehavior = new MouseScrollBehavior();
        Interaction.GetBehaviors(tvwMapping).Add(mouseScrollBehavior);
#else
        //TODO
#endif

        this.aggregator = aggregator;
        this.model = model;
        this.model.MappingTree = tvwMapping;
        this.adapterProxy = adapterProxy;
        this.referenceDataService = referenceDataService;

        adapterProxy.OnDataArrived += OnDataArrivedHandler;
        referenceDataService.OnDataArrived += OnDataArrivedHandler;
        aggregator.GetEvent<SpinnerEvent>().Subscribe(SpinnerEventHandler);
      }
      catch (Exception ex)
      {
        Error.SetError(ex);
      }
    }

    /// <summary>
    /// Called when [data arrived handler].
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    void OnDataArrivedHandler(object sender, System.EventArgs e)
    {
      CompletedEventArgs args = e as CompletedEventArgs;
      
      if (args == null)
        return;

      if (args.CheckForType(CompletedEventType.GetMapping))
      {
        GetMappingHandler(args);
      }
      else if (args.CheckForType(CompletedEventType.GetClassLabel))
      {
        GetClassLabelHandler(args);
      }
    }

    /// <summary>
    /// mapping handler.
    /// </summary>
    /// <param name="e">The <see cref="org.iringtools.modulelibrary.events.CompletedEventArgs"/> instance containing the event data.</param>
    void GetMappingHandler(CompletedEventArgs e)
    {
      // Ensure we have a valid parameter
      Mapping mapping = e.Data as Mapping;
      if (mapping == null)
        return;

      mappingCRUD.mapping = mapping;
      tvwMapping.Items.Clear();

      // Note that we only load first level nodes
      foreach (GraphMap graphMap in mapping.graphMaps)
      {
        tvwMapping.Items.Add(AddNode(graphMap.name, graphMap, null));
      }

      // Add value maps to value list drop list
      List<ValueMap> valueMaps = mapping.valueMaps;

      if (valueMaps.Count > 0)
      {
        string prevValueList = String.Empty;

        foreach (ValueMap valueMap in valueMaps)
        {
          string currValueList = valueMap.valueList;

          if (currValueList != prevValueList)
          {
            ComboBoxItem cbItem = new ComboBoxItem();
            cbItem.Content = currValueList;
            cbValueList.Items.Add(cbItem);
            prevValueList = currValueList;
          }
        }
      }

      ChangeControlsState(true);
    }

    /// <summary>
    /// getclasslabel handler.
    /// </summary>
    /// <param name="e">The <see cref="org.iringtools.modulelibrary.events.CompletedEventArgs"/> instance containing the event data.</param>
    void GetClassLabelHandler(CompletedEventArgs e)
    {
      string[] data = (string[])e.Data;
      string tag = data[0];
      string id = data[1];
      string label = data[2];

      KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>(tag, label);
      model.DetailProperties.Add(keyValuePair);
      model.IdLabelDictionary[id] = label;
    }

    /// <summary>
    /// Handles MouseLeftButtonUp Event
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void nodeMouseLeftButtonUpHandler(object sender, MouseButtonEventArgs e)
    {
      MappingItem selectedNode = sender as MappingItem;

      if (selectedNode == null)
        return;

      model.SelectedMappingItem = selectedNode;
      model.SelectedGraphMap = selectedNode.GraphMap;
      model.SelectedClassMap = selectedNode.ClassMap;
      model.SelectedTemplateMap = selectedNode.TemplateMap;
      model.SelectedRoleMap = selectedNode.RoleMap;
      model.SelectedNodeType = selectedNode.NodeType;

      model.DetailProperties.Clear();

      if (selectedNode.Tag is GraphMap)
      {
        GraphMap graph = (GraphMap)selectedNode.Tag;
        KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>("Graph Name", graph.name);
        model.DetailProperties.Add(keyValuePair);

        for (int i = 0; i < graph.dataObjectMaps.Count; i++)
        {
          keyValuePair = new KeyValuePair<string, string>("DataObject Name", graph.dataObjectMaps[i].name);
          model.DetailProperties.Add(keyValuePair);
        }
      }

      if (selectedNode.Tag is ClassMap)
      {
        ClassMap classMap = (ClassMap)selectedNode.Tag;
        RefreshClassMap(classMap);
      }
      else if (selectedNode.Tag is TemplateMap)
      {
        TemplateMap templateMap = (TemplateMap)selectedNode.Tag;
        RefreshTemplateMap(templateMap);
      }
      else if (selectedNode.Tag is RoleMap)
      {
        RoleMap roleMap = (RoleMap)selectedNode.Tag;
        RefreshRoleMap(roleMap);
      }

      e.Handled = true;
    }

    public void RefreshClassMap(ClassMap classMap)
    {
      KeyValuePair<string, string> keyValuePair;
      
      keyValuePair = new KeyValuePair<string, string>("Class Id", classMap.classId);
      model.DetailProperties.Add(keyValuePair);
      keyValuePair = new KeyValuePair<string, string>("Identifier", classMap.identifier);
      model.DetailProperties.Add(keyValuePair);

      string id = classMap.classId;
      if (id.Contains("#"))
      {
        id = id.Substring(id.LastIndexOf("#") + 1);
      }
      else if (id.Contains(":"))
      {
        id = id.Substring(id.LastIndexOf(":") + 1);
      }

      if (model.IdLabelDictionary.ContainsKey(id))
      {
        model.DetailProperties.Add(new KeyValuePair<string, string>("Class Name", model.IdLabelDictionary[id]));
      }
      else
      {
        referenceDataService.GetClassLabel("Class Name", classMap.classId, this);
      }
    }

    public void RefreshTemplateMap(TemplateMap templateMap)
    {
      KeyValuePair<string, string> keyValuePair;
      
      keyValuePair = new KeyValuePair<string, string>("Template Name", templateMap.name);
      model.DetailProperties.Add(keyValuePair);
      keyValuePair = new KeyValuePair<string, string>("Template Id", templateMap.templateId);
      model.DetailProperties.Add(keyValuePair);
      keyValuePair = new KeyValuePair<string, string>("Class Role", templateMap.classRole);
      model.DetailProperties.Add(keyValuePair);
      keyValuePair = new KeyValuePair<string, string>("Type", templateMap.type.ToString());
      model.DetailProperties.Add(keyValuePair);
    }

    public void RefreshRoleMap(RoleMap roleMap)
    {
      KeyValuePair<string, string> keyValuePair;
      
      keyValuePair = new KeyValuePair<string, string>("Role Name", roleMap.name);
      model.DetailProperties.Add(keyValuePair);
      keyValuePair = new KeyValuePair<string, string>("Role Id", roleMap.roleId);
      model.DetailProperties.Add(keyValuePair);
      keyValuePair = new KeyValuePair<string, string>("Property Name", roleMap.propertyName);
      model.DetailProperties.Add(keyValuePair);
      keyValuePair = new KeyValuePair<string, string>("Datatype", roleMap.dataType);
      model.DetailProperties.Add(keyValuePair);
      keyValuePair = new KeyValuePair<string, string>("ValueList", roleMap.valueList);
      model.DetailProperties.Add(keyValuePair);

      if (!string.IsNullOrEmpty(roleMap.reference))
      {
        keyValuePair = new KeyValuePair<string, string>("Reference Id", (roleMap.reference != null ? roleMap.reference : string.Empty));
        model.DetailProperties.Add(keyValuePair);
      }
    }

    // INFORMATION MODEL TREEVIEW CODE FOLLOWS

    #region METHOD: AddNode(string header, object tag)
    /// <summary>
    /// Adds the node in a manner that will let us hook into the Selected event
    /// </summary>
    /// <param name="header">The header.</param>
    /// <param name="graphMap">The graph map.</param>
    /// <returns></returns>
    public MappingItem AddNode(string header, object tag, MappingItem parent)
    {
      // First let's create a node reference
      MappingItem node = new MappingItem();
      node.itemTextBlock.Text = header;
      node.Tag = tag;

      // Subscribe to it's events
      //node.Selected += nodeSelectedHandler;
      node.MouseLeftButtonUp += nodeMouseLeftButtonUpHandler;
      node.Expanded += nodeExpandedHandler;

      bool isProcessed = false;

      // Now populate it as applicable
      if (tag is GraphMap)
      {
        node.NodeType = NodeType.GraphMap;
        node.GraphMap = (GraphMap)tag;
        node.ClassMap = (ClassMap)tag;
        node.SetImageSource("graph.png");
        node.SetTooltipText("Graph : " + node.GraphMap.name);
        isProcessed = PopulateGraphNode(node, (GraphMap)tag);
      }

      if (tag is TemplateMap)
      {
        node.NodeType = NodeType.TemplateMap;
        if (parent.NodeType == NodeType.GraphMap)
        {
          node.GraphMap = (GraphMap)parent.Tag;
          node.ClassMap = (ClassMap)parent.Tag;
        }
        else
        {
          node.GraphMap = parent.GraphMap;
          node.ClassMap = (ClassMap)parent.Tag;
        }
        node.TemplateMap = (TemplateMap)tag;
        node.SetImageSource("template.png");
        node.SetTooltipText("Template : " + node.TemplateMap.name);
        isProcessed = PopulateTemplateNode(node, (TemplateMap)tag);
      }
      if (tag is RoleMap)
      {
        node.NodeType = NodeType.RoleMap;
        node.GraphMap = parent.GraphMap;
        node.ClassMap = parent.ClassMap;
        node.TemplateMap = (TemplateMap)parent.Tag;
        node.RoleMap = (RoleMap)tag;
        node.SetImageSource("role.png");
        node.SetTooltipText("Role : " + node.RoleMap.name);
        isProcessed = PopulateRoleNode(node, (RoleMap)tag);
      }
      // ClassMap is the base for the above so we'll
      // need to ensure we only process if none of the
      // above already processed
      if (tag is ClassMap && !isProcessed)
      {
        node.NodeType = NodeType.ClassMap;
        node.GraphMap = parent.GraphMap;
        node.ClassMap = (ClassMap)tag;
        node.TemplateMap = parent.TemplateMap;
        node.RoleMap = (RoleMap)parent.Tag;
        node.SetImageSource("class.png");
        node.SetTooltipText("Class : " + node.ClassMap.name);
        PopulateClassNode(node, (ClassMap)tag);
      }

      return node;
    }
    #endregion

    #region EVENTHANDLER: nodeExpandedHandler(object sender, System.Windows.RoutedEventArgs e)
    /// <summary>
    /// Handler when a node is expanded - used to populate
    /// nodes that are a "Stub" (so we can have an arrow)
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    void nodeExpandedHandler(object sender, System.Windows.RoutedEventArgs e)
    {
      // If the sender is not a valid node return
      MappingItem selectedNode = sender as MappingItem;
      if (selectedNode == null || selectedNode.Items.Count == 0)
        return;

      MappingItem childNode = selectedNode.Items[0] as MappingItem;

      // If expanded then the tree has nodes so we'll grab the
      // first node and see if it is a stub.  If it isn't then
      // we have nothing to do
      // if (childNode.Header.ToString() != "Stub")
      if (childNode.itemTextBlock.Text != "Stub")
        return;

      // Remove the stub
      selectedNode.Items.Clear();

      bool isProcessed = false;

      // Add the child nodes
      if (childNode.Tag is ClassMap)
        isProcessed = PopulateClassMap(selectedNode, (ClassMap)childNode.Tag);

      if (childNode.Tag is List<TemplateMap>)
        foreach (TemplateMap templateMap in ((List<TemplateMap>)childNode.Tag))
          isProcessed = PopulateTemplateMap(selectedNode, templateMap);

      if (childNode.Tag is List<RoleMap>)
        foreach (RoleMap roleMap in ((List<RoleMap>)childNode.Tag))
          isProcessed = PopulateRoleMap(selectedNode, roleMap);
    }
    #endregion

    #region InformationModel Treeview Populate Methods

    /// <summary>
    /// Populates the graph node.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="graphMap">The graph map.</param>
    /// <returns></returns>
    bool PopulateGraphNode(MappingItem node, GraphMap graphMap)
    {
      if (graphMap.templateMaps != null && graphMap.templateMaps.Count > 0)
      {
        MappingItem map = new MappingItem();
        map.Tag = graphMap.templateMaps;
        map.SetTextBlockText("Stub");
        node.Items.Add(map);
      }
      return true;
    }


    /// <summary>
    /// Populates the list template map.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="templateMapList">The template map list.</param>
    /// <returns></returns>
    bool PopulateTemplateNode(MappingItem node, TemplateMap templateMap)
    {
      if (templateMap.roleMaps != null && templateMap.roleMaps.Count > 0)
      {
        MappingItem map = new MappingItem();
        map.Tag = templateMap.roleMaps;
        map.SetTextBlockText("Stub");
        node.Items.Add(map);
      }
      //node.Items.Add(new MappingItem { Header = "Stub", Tag = templateMap.roleMaps });
      return true;
    }

    /// <summary>
    /// Populates the list role map.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="roleMapList">The role map list.</param>
    /// <returns></returns>
    public bool PopulateRoleNode(MappingItem node, RoleMap roleMap)
    {
      if (roleMap.classMap != null)
      {
        MappingItem map = new MappingItem();
        map.Tag = roleMap.classMap;
        map.SetTextBlockText("Stub");
        node.Items.Add(map);
      }
      // node.Items.Add(new MappingItem { Header = "Stub", Tag = roleMap.classMap });
      return true;
    }

    /// <summary>
    /// Populates the node class map.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="classMap">The class map.</param>
    /// <returns></returns>
    public bool PopulateClassNode(MappingItem node, ClassMap classMap)
    {
      //if (node.Items.Count == 0)
      if (classMap.templateMaps != null && classMap.templateMaps.Count > 0)
      {
        MappingItem map = new MappingItem();
        map.Tag = classMap.templateMaps;
        map.SetTextBlockText("Stub");
        node.Items.Add(map);
      }
      //        node.Items.Add(new MappingItem { Header = "Stub", Tag = classMap.templateMaps });
      //else
      //  PopulateTemplateMap(node, classMap.templateMaps[0]);
      return true;
    }

    /// <summary>
    /// Populates the template map.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="templateMap">The template map.</param>
    /// <returns></returns>
    public bool PopulateTemplateMap(MappingItem node, TemplateMap templateMap)
    {
      MappingItem newNode = AddNode(templateMap.name, templateMap, node);
      node.Items.Add(newNode);
      return true;
    }

    /// <summary>
    /// Populates the role map.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="roleMap">The role map.</param>
    /// <returns></returns>
    bool PopulateRoleMap(MappingItem node, RoleMap roleMap)
    {
      string roleName = roleMap.name;
      
      if (!roleMap.isMapped)
      {
        roleName += unmappedToken;
      }

      MappingItem newNode = AddNode(roleName, roleMap, node);
      node.Items.Add(newNode);

      return true;
    }

    /// <summary>
    /// Populates the class map.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="classMap">The class map.</param>
    /// <returns></returns>
    bool PopulateClassMap(MappingItem node, ClassMap classMap)
    {
      MappingItem newNode = AddNode(classMap.name, classMap, node);
      node.Items.Add(newNode);
      return true;
    }

    #endregion
    public void SpinnerEventHandler(SpinnerEventArgs e)
    {
      switch (e.Active)
      {
        case SpinnerEventType.Started:
          ChangeControlsState(false);
          break;

        case SpinnerEventType.Stopped:
          ChangeControlsState(true);
          break;

        default:
          break;
      }
    }

    private void ChangeControlsState(bool enabled)
    {
      if (mappingCRUD.mapping != null && mappingCRUD.mapping.graphMaps.Count > 0)
      {
        tvwMapping.IsEnabled = enabled;
        txtLabel.IsEnabled = true;
        btnAddGraph.IsEnabled = enabled;
        btnAddTemplate.IsEnabled = enabled;
        btnMap.IsEnabled = enabled;
        btnMakeClassRole.IsEnabled = enabled;

        if (enabled && cbValueList.Items.Count > 0)
        {
          cbValueList.IsEnabled = enabled;
          btnAddValueList.IsEnabled = enabled;
        }
        else
        {
          cbValueList.IsEnabled = false;
          btnAddValueList.IsEnabled = false;
        }

        btnDelete.IsEnabled = enabled;
        btnSave.IsEnabled = enabled;
      }
    }
  }
}
