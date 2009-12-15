using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;

using ModuleLibrary.Entities;
using ModuleLibrary.Events;
using ModuleLibrary.Extensions;
using ModuleLibrary.Types;

using OntologyService.Interface.PresentationModels;

using PrismContrib.Base;

using InformationModel.Events;
using org.iringtools.library;

#if SILVERLIGHT
using ModuleLibrary.Behaviors;
using System.Windows.Interactivity;
#else
#endif


namespace Modules.MappingEditor.MEMappingRegion
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

#if SILVERLIGHT
        MouseScrollBehavior mouseScrollBehavior = new MouseScrollBehavior();
        Interaction.GetBehaviors(tvwMapping).Add(mouseScrollBehavior);
#else
        //TODO
#endif

        this.aggregator = aggregator;
        this.model = model;
        this.model.MappingTree = tvwMapping;


        // For class use
        this.adapterProxy = adapterProxy;
        adapterProxy.OnDataArrived += OnDataArrivedHandler;

        adapterProxy.GetMapping();
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

      model.DetailProperties.Clear();
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



      if (selectedNode.Tag is ClassMap)
      {
        ClassMap classMap = (ClassMap)selectedNode.Tag;

        KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>("Class Name", classMap.name);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Class Id", classMap.classId);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Identifier", classMap.identifier);
        model.DetailProperties.Add(keyValuePair);

        
      }

      if (selectedNode.Tag is GraphMap)
      {
        GraphMap graph = (GraphMap)selectedNode.Tag;
        KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>("Graph Name", graph.name);

        for (int i = 0; i < graph.dataObjectMaps.Count; i++)
        {
          keyValuePair = new KeyValuePair<string, string>("DataObject Name", graph.dataObjectMaps[i].name);
          model.DetailProperties.Add(keyValuePair);
        }
      }

      if (selectedNode.Tag is TemplateMap)
      {
        TemplateMap templateMap = (TemplateMap)selectedNode.Tag;
        KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>("Template Name", templateMap.name);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Template Id", templateMap.templateId);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Class Role", templateMap.classRole);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Type", templateMap.type.ToString());
        model.DetailProperties.Add(keyValuePair);
      }
      if (selectedNode.Tag is RoleMap)
      {
        RoleMap roleMap = (RoleMap)selectedNode.Tag;
        KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>("Role Name", roleMap.name);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Role Id", roleMap.roleId);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Property Name", roleMap.propertyName);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Datatype", roleMap.dataType);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("ValueList", roleMap.valueList);
        model.DetailProperties.Add(keyValuePair);
        keyValuePair = new KeyValuePair<string, string>("Class", (roleMap.classMap != null ? roleMap.classMap.name : string.Empty));
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
      MappingItem node = new MappingItem ();
      node.itemTextBlock.Text = header;
      node.Tag = tag;

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
        node.SetTooltipText("Template : "+ node.TemplateMap.name);
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
      if (selectedNode == null)
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
    public void SpinnerEventHandler(SpinnerEventArgs e)
    {
      switch (e.Active)
      {
        case SpinnerEventType.Started:
          this.tvwMapping.IsEnabled = false;
          btnAddGraph.IsEnabled = false;
          btnAddTemplate.IsEnabled = false;
          btnData.IsEnabled = false;
          btnDelete.IsEnabled = false;
          btnMap.IsEnabled = false;
          btnSave.IsEnabled = false;
            
          break;

        case SpinnerEventType.Stopped:
          this.tvwMapping.IsEnabled = true;
          this.tvwMapping.IsEnabled = true;
          btnAddGraph.IsEnabled = true;
          btnAddTemplate.IsEnabled = true;
          btnData.IsEnabled = true;
          btnDelete.IsEnabled = true;
          btnMap.IsEnabled = true;
          btnSave.IsEnabled = true;

           break;

        default:
          break;
      }
    }
  }
}
