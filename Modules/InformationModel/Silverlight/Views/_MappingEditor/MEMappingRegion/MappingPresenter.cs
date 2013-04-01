using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using InformationModel.Events;
using InformationModel.Views._MappingEditor.MEMappingRegion;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using ModuleLibrary.Entities;
using ModuleLibrary.Events;
using ModuleLibrary.Extensions;
using ModuleLibrary.Types;
using OntologyService.Interface.PresentationModels;
using org.ids_adi.iring;
using PrismContrib.Base;



namespace InformationModel.Views.MEDataDetailRegion
{
  /// <summary>
  /// 
  /// </summary>
  public class MappingPresenter : PresenterBase<IMappingView>
  {
    private IEventAggregator aggregator = null;
    private IAdapter adapterProxy = null;
    IIMPresentationModel model = null;

    private TreeView tvwMapping { get { return GetControl<TreeView>("tvwMapping"); } }

    private Button btnData { get { return ButtonCtrl("btnData"); } }
    private Button btnAddTemplate { get { return ButtonCtrl("btnAddTemplate"); } }
    private Button btnAddGraph { get { return ButtonCtrl("btnAddGraph"); } }
    private Button btnMap { get { return ButtonCtrl("btnMap"); } }
    private Button btnSave { get { return ButtonCtrl("btnSave"); } }
    private Button btnDelete { get { return ButtonCtrl("btnDelete"); } }
    private TextBox txtLabel { get { return TextCtrl("txtLabel"); } }

    // Create, Update, Delete code for Mapping Editor treeview
    private MappingCRUD mappingCRUD = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="MappingPresenter"/> class.
    /// </summary>
    /// <param name="view">The view.</param>
    /// <param name="model">The model.</param>
    /// <param name="adapterProxy">The adapter proxy.</param>
    public MappingPresenter(IMappingView view, IIMPresentationModel model,
      IEventAggregator aggregator,
      IAdapter adapterProxy,
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
        btnData.Click += (object sender, RoutedEventArgs e) => { mappingCRUD.btnData_Click(txtLabel, e); };
        btnAddTemplate.Click += (object sender, RoutedEventArgs e) => { mappingCRUD.btnAddTemplate_Click(txtLabel, e); };
        btnAddGraph.Click += (object sender, RoutedEventArgs e) => { mappingCRUD.btnAddGraph_Click(txtLabel, e); };
        btnMap.Click += (object sender, RoutedEventArgs e) => { mappingCRUD.btnMap_Click(txtLabel, e); };
        btnSave.Click += (object sender, RoutedEventArgs e) => { mappingCRUD.btnSave_Click(txtLabel, e); };
        btnDelete.Click += (object sender, RoutedEventArgs e) => { mappingCRUD.btnDelete_Click(txtLabel, e); };

        this.aggregator = aggregator;
        this.model = model;
        this.model.MappingTree = tvwMapping;


        // For class use
        this.adapterProxy = adapterProxy;
        adapterProxy.OnDataArrived += OnDataArrivedHandler;

        adapterProxy.GetMapping();
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

      // Handle the GetMapping() event 
      if (args.CheckForType(CompletedEventType.GetMapping))
        GetMappingHandler(args);

    }

    /// <summary>
    /// mapping handler.
    /// </summary>
    /// <param name="e">The <see cref="ModuleLibrary.Events.CompletedEventArgs"/> instance containing the event data.</param>
    void GetMappingHandler(CompletedEventArgs e)
    {
      // Ensure we have a valid parameter
      Mapping mapping = e.Data as Mapping;
      if (mapping == null)
        return;
      mappingCRUD.mapping = mapping;
      // Note that we only load first level nodes
      foreach (GraphMap graphMap in mapping.graphMaps)
      {
        tvwMapping.Items.Add(AddNode(graphMap.name, graphMap, null));
      }
    }

    /// <summary>
    /// Nodes the selected handler.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    void nodeSelectedHandler(object sender, System.Windows.RoutedEventArgs e)
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

      aggregator.GetEvent<NavigationEvent>().Publish(new NavigationEventArgs
      {
        DetailProcess = DetailType.Mapping,
        SelectedNode = selectedNode,
        Sender = this
      });
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
      MappingItem node = new MappingItem { Header = header, Tag = tag };

      // Subscribe to it's events
      node.Selected += nodeSelectedHandler;
      node.Expanded += nodeExpandedHandler;

      bool isProcessed = false;

      // Now populate it as applicable
      if (tag is GraphMap)
      {
        node.NodeType = NodeType.GraphMap;
        node.GraphMap = (GraphMap)tag;
        node.ClassMap = (ClassMap)tag;
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
        isProcessed = PopulateTemplateNode(node, (TemplateMap)tag);
      }
      if (tag is RoleMap)
      {
        node.NodeType = NodeType.RoleMap;
        node.GraphMap = parent.GraphMap;
        node.ClassMap = parent.ClassMap;
        node.TemplateMap = (TemplateMap)parent.Tag;
        node.RoleMap = (RoleMap)tag;
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
      if (selectedNode == null)
        return;

      MappingItem childNode = selectedNode.Items[0] as MappingItem;

      // If expanded then the tree has nodes so we'll grab the
      // first node and see if it is a stub.  If it isn't then
      // we have nothing to do
      if (childNode.Header.ToString() != "Stub")
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
        node.Items.Add(new MappingItem { Header = "Stub", Tag = graphMap.templateMaps });
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
        node.Items.Add(new MappingItem { Header = "Stub", Tag = templateMap.roleMaps });
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
        node.Items.Add(new MappingItem { Header = "Stub", Tag = roleMap.classMap });
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
        node.Items.Add(new MappingItem { Header = "Stub", Tag = classMap.templateMaps });
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
   public  bool PopulateTemplateMap(MappingItem node, TemplateMap templateMap)
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
      string isMapped = string.Empty;
      if (!roleMap.isMapped)
      {
        isMapped = " [UnMapped]";
      }
      MappingItem newNode = AddNode(roleMap.name + isMapped, roleMap, node);
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

  }
}
