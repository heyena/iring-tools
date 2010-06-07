using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Composite.Logging;

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
using org.iringtools.utility;
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
        private TreeView tvwValues { get { return GetControl<TreeView>("tvwValues"); } }

        private Button btnAddTemplate { get { return ButtonCtrl("btnAddTemplate"); } }
        private Button btnAddGraph { get { return ButtonCtrl("btnAddGraph"); } }
        private Button btnMap { get { return ButtonCtrl("btnMap"); } }
        //private Button btnEditFixedRole { get { return ButtonCtrl("btnEditFixedRole"); } }
        private ComboBox cbValueList { get { return ComboBoxCtrl("cbValueList"); } }
        private Button btnAddValueList { get { return ButtonCtrl("btnAddValueList"); } }
        private Button btnDelete { get { return ButtonCtrl("btnDelete"); } }
        private Button btnSave { get { return ButtonCtrl("btnSave"); } }
        private TextBox txtLabel { get { return TextCtrl("txtLabel"); } }

        public string selectedValueList { get { return ((ComboBoxItem)(cbValueList.SelectedItem)).Content.ToString(); } }

        // Create, Update, Delete code for Mapping Editor treeview
        private MappingCRUD mappingCRUD = null;

        public string unmappedToken { get { return " [UnMapped]"; } }

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
               // btnEditFixedRole.Click += (object sender, RoutedEventArgs e) => { mappingCRUD.btnEditFixedRole_Click(txtLabel, e); };
                btnAddValueList.Click += (object sender, RoutedEventArgs e) => { mappingCRUD.btnAddValueList(txtLabel, e); };
                btnSave.Click += (object sender, RoutedEventArgs e) => { mappingCRUD.btnSave_Click(txtLabel, e); };
                btnDelete.Click += (object sender, RoutedEventArgs e) => { mappingCRUD.btnDelete_Click(txtLabel, e); };
                cbValueList.SelectionChanged += new SelectionChangedEventHandler(cbValueList_SelectionChanged);

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

        void cbValueList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnAddValueList.IsEnabled = true;
        }

        /// <summary>
        /// Called when [data arrived handler].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void OnDataArrivedHandler(object sender, System.EventArgs e)
        {
            try
            {
                CompletedEventArgs args = e as CompletedEventArgs;

                if (args == null)
                    return;

                if (args.CheckForType(CompletedEventType.GetMapping))
                {
                    if (args.Error != null)
                    {
                        MessageBox.Show(args.FriendlyErrorMessage, "Get Data Dictionary Error", MessageBoxButton.OK);
                        return;
                    }
                    GetMappingHandler(args);
                }
                else if (args.CheckForType(CompletedEventType.GetClassLabel))
                {
                    if (args.Error != null)
                    {
                        MessageBox.Show(args.FriendlyErrorMessage, "Get Class Label Error", MessageBoxButton.OK);
                        return;
                    }
                    GetClassLabelHandler(args);
                }
            }
            catch (Exception ex)
            {
                 Error.SetError(ex, "Error occurred... \r\n" + ex.Message + ex.StackTrace,
                    Category.Exception, Priority.High);
            }
        }

        /// <summary>
        /// mapping handler.
        /// </summary>
        /// <param name="e">The <see cref="org.iringtools.modulelibrary.events.CompletedEventArgs"/> instance containing the event data.</param>
        void GetMappingHandler(CompletedEventArgs e)
        {
            try
            {
                // Ensure we have a valid parameter
                Mapping mapping = e.Data as Mapping;
                if (mapping == null)
                    return;

                mappingCRUD.mapping = mapping;
                tvwMapping.Items.Clear();
                tvwValues.Items.Clear();

                // Note that we only load first level nodes
                foreach (GraphMap graphMap in mapping.graphMaps)
                {
                    tvwMapping.Items.Add(AddNode(graphMap.name, graphMap, null));
                }

                //Add an empty value list item so we can use that value to clear erroneous entries
                cbValueList.Items.Add(new ComboBoxItem { Content = "<No ValueList>", IsSelected = true });

                // Add value maps to value list drop list

                List<ValueMap> valueMaps = mapping.valueMaps;
                MappingItem valueList = null;

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
                            valueList = AddNode(valueMap.valueList, (String)valueMap.valueList, null);
                            tvwValues.Items.Add(valueList);
                        }

                        if (valueList != null)
                        {
                            MappingItem internalValue = AddNode(valueMap.internalValue, valueMap, valueList);
                            valueList.Items.Add(internalValue);
                        }
                    }
                }

                //if (cbValueList.SelectedItem == null)
                //    btnAddValueList.IsEnabled = false;
                //else
                //    btnAddValueList.IsEnabled = true;

                ChangeControlsState(true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// getclasslabel handler.
        /// </summary>
        /// <param name="e">The <see cref="org.iringtools.modulelibrary.events.CompletedEventArgs"/> instance containing the event data.</param>
        void GetClassLabelHandler(CompletedEventArgs e)
        {
            try
            {
                string[] data = (string[])e.Data;
                string tag = data[0];
                string id = data[1];
                string label = data[2];

                KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>(tag, label);
                model.DetailProperties.Add(keyValuePair);
                model.IdLabelDictionary[id] = label;
            }
            catch (Exception ex)
            {
               throw ex;
            }
        }

        /// <summary>
        /// Handles MouseLeftButtonUp Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void nodeMouseLeftButtonUpHandler(object sender, MouseButtonEventArgs e)
        {
            try
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
                    string id = Utility.GetIdFromURI(graph.baseUri);
                    KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>("Graph Name", graph.name);
                    model.DetailProperties.Add(keyValuePair);

                    keyValuePair = new KeyValuePair<string, string>("Graph Uri", graph.baseUri);
                    model.DetailProperties.Add(keyValuePair);
                    if (model.IdLabelDictionary.ContainsKey(id))
                    {
                        keyValuePair = new KeyValuePair<string, string>("Class Name", model.IdLabelDictionary[id]);
                        model.DetailProperties.Add(keyValuePair);
                    }
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
            catch (Exception ex)
            {
                Error.SetError(ex, "Error occurred... \r\n" + ex.Message + ex.StackTrace,
                    Category.Exception, Priority.High);
            }
        }

        public void RefreshClassMap(ClassMap classMap)
        {
            try
            {
                KeyValuePair<string, string> keyValuePair;

                keyValuePair = new KeyValuePair<string, string>("Class Id", classMap.classId);
                model.DetailProperties.Add(keyValuePair);
                foreach (string identifier in classMap.identifiers)
                {
                    keyValuePair = new KeyValuePair<string, string>("Identifier", identifier);
                    model.DetailProperties.Add(keyValuePair);
                }
                string id = Utility.GetIdFromURI(classMap.classId);
                if (model.IdLabelDictionary.ContainsKey(id))
                {
                    model.DetailProperties.Add(new KeyValuePair<string, string>("Class Name", model.IdLabelDictionary[id]));
                }
                else if (!String.IsNullOrEmpty(id))
                {
                    referenceDataService.GetClassLabel("Class Name", id, this);
                }
            }
            catch (Exception ex)
            {
                Error.SetError(ex, "Error occurred... \r\n" + ex.Message + ex.StackTrace,
                    Category.Exception, Priority.High);
            }
        }

        public void RefreshTemplateMap(TemplateMap templateMap)
        {
            try
            {
                KeyValuePair<string, string> keyValuePair;

                keyValuePair = new KeyValuePair<string, string>("Template Name", templateMap.name);
                model.DetailProperties.Add(keyValuePair);
                keyValuePair = new KeyValuePair<string, string>("Template Id", templateMap.templateId);
                model.DetailProperties.Add(keyValuePair);
                //keyValuePair = new KeyValuePair<string, string>("Class Role Id", templateMap.classRole);
                //model.DetailProperties.Add(keyValuePair);
                //keyValuePair = new KeyValuePair<string, string>("Type", templateMap.type.ToString());
                //model.DetailProperties.Add(keyValuePair);

                //string id = Utility.GetIdFromURI(templateMap.templateId);
                //if (model.IdLabelDictionary.ContainsKey(id))
                //{
                //  model.DetailProperties.Add(new KeyValuePair<string, string>("Class Role Name", model.IdLabelDictionary[id]));
                //}
                //else if (!String.IsNullOrEmpty(id))
                //{
                //  referenceDataService.GetTemplateLabel("Class Role Name", id, this);
                //}
            }
            catch (Exception ex)
            {
                Error.SetError(ex, "Error occurred... \r\n" + ex.Message + ex.StackTrace,
                    Category.Exception, Priority.High);
            }
        }

        public void RefreshRoleMap(RoleMap roleMap)
        {
            try
            {
                model.DetailProperties.Clear();

                KeyValuePair<string, string> keyValuePair;
                keyValuePair = new KeyValuePair<string, string>("Role Type", roleMap.type.ToString());
                model.DetailProperties.Add(keyValuePair);
                keyValuePair = new KeyValuePair<string, string>("Role Name", roleMap.name);
                model.DetailProperties.Add(keyValuePair);
                keyValuePair = new KeyValuePair<string, string>("Role Id", roleMap.roleId);
                model.DetailProperties.Add(keyValuePair);

                string referenceId = (roleMap.value != null ? roleMap.value : string.Empty);
                keyValuePair = new KeyValuePair<string, string>("Reference Id", referenceId);
                model.DetailProperties.Add(keyValuePair);
                if (roleMap.type == RoleType.Reference)
                {
                    
                    string id = Utility.GetIdFromURI(referenceId);
                    if (model.IdLabelDictionary.ContainsKey(id))
                    {
                        model.DetailProperties.Add(new KeyValuePair<string, string>("Reference Class", model.IdLabelDictionary[id]));
                    }
                    else if (!String.IsNullOrEmpty(id))
                    {
                        referenceDataService.GetClassLabel("Reference Name", id, this);
                    }
                }
                keyValuePair = new KeyValuePair<string, string>("Property Name", roleMap.propertyName);
                model.DetailProperties.Add(keyValuePair);
                keyValuePair = new KeyValuePair<string, string>("Datatype", roleMap.dataType);
                model.DetailProperties.Add(keyValuePair);
                keyValuePair = new KeyValuePair<string, string>("ValueList", roleMap.valueList);
                model.DetailProperties.Add(keyValuePair);

            }
            catch (Exception ex)
            {
                Error.SetError(ex, "Error occurred... \r\n" + ex.Message + ex.StackTrace,
                    Category.Exception, Priority.High);
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
            try
            {
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
                    GraphMap graphMap = (GraphMap)tag;
                    node.NodeType = NodeType.GraphMap;
                    node.GraphMap = graphMap;
                    foreach (var pair in graphMap.classTemplateListMaps)
                    {
                        node.ClassMap = pair.Key;
                        break;
                    }
                    node.SetImageSource("graph.png");
                    node.SetTooltipText("Graph : " + node.GraphMap.name);
                    isProcessed = PopulateGraphNode(node, graphMap);
                }
                if (tag is TemplateMap)
                {
                    node.NodeType = NodeType.TemplateMap;
                    if (parent.NodeType == NodeType.GraphMap)
                    {
                        node.GraphMap = (GraphMap)parent.Tag;
                        // node.ClassMap = (ClassMap)parent.Tag;
                    }
                    else
                    {
                        node.GraphMap = parent.GraphMap;
                        // node.ClassMap = (ClassMap)parent.Tag;
                    }
                    node.TemplateMap = (TemplateMap)tag;
                    node.SetImageSource("template.png");
                    node.SetTooltipText("Template : " + node.TemplateMap.name);
                    isProcessed = PopulateTemplateNode(node, (TemplateMap)tag);
                }
                if (tag is RoleMap)
                {
                    RoleMap roleMap = (RoleMap)tag;
                    node.NodeType = NodeType.RoleMap;
                    node.GraphMap = parent.GraphMap;
                    // node.ClassMap = parent.ClassMap;
                    node.TemplateMap = (TemplateMap)parent.Tag;
                    node.RoleMap = roleMap;
                    node.SetImageSource("role.png");
                    node.SetTooltipText("Role : " + node.RoleMap.name);
                    isProcessed = PopulateRoleNode(node, roleMap);
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
                if (tag is ValueMap)
                {
                    node.NodeType = NodeType.ValueMap;
                    //node.ClassMap = (ClassMap)tag;
                    node.SetImageSource("value.png");
                    node.SetTooltipText("Value : " + ((ValueMap)tag).internalValue);
                    isProcessed = PopulateValueNode(node, (ValueMap)tag);
                }
            }
            catch (Exception ex)
            {
                 Error.SetError(ex, "Error occurred... \r\n" + ex.Message + ex.StackTrace,
                    Category.Exception, Priority.High);
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
            try
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


                if (selectedNode.Tag is ClassMap)
                {
                    {
                        ClassMap selectedClassMap = (ClassMap)selectedNode.ClassMap;
                        KeyValuePair<ClassMap, List<TemplateMap>> classTemplates =
                            selectedNode.GraphMap.GetClassTemplateListMap(selectedClassMap.classId);

                        foreach (TemplateMap templateMap in classTemplates.Value)
                        {
                            isProcessed = PopulateTemplateMap(selectedNode, templateMap);
                        }
                    }
                }
                // Add the child nodes


                if (childNode.Tag is Dictionary<ClassMap, List<TemplateMap>>)
                {

                    if (selectedNode.Tag is GraphMap)
                    {
                        GraphMap selectedGraphMap = (GraphMap)selectedNode.Tag;

                        KeyValuePair<ClassMap, List<TemplateMap>> classTemplates =
                            selectedGraphMap.GetClassTemplateListMap(selectedNode.ClassMap.classId);

                        foreach (TemplateMap templateMap in classTemplates.Value)
                        {
                            isProcessed = PopulateTemplateMap(selectedNode, templateMap);
                        }
                    }
                }

                if (childNode.Tag is ClassMap)
                    isProcessed = PopulateClassMap(selectedNode, (ClassMap)childNode.Tag);

                if (childNode.Tag is List<RoleMap>)
                    foreach (RoleMap roleMap in ((List<RoleMap>)childNode.Tag))
                        isProcessed = PopulateRoleMap(selectedNode, roleMap);
            }
            catch (Exception ex)
            {
                 Error.SetError(ex, "Error occurred... \r\n" + ex.Message + ex.StackTrace,
                    Category.Exception, Priority.High);
            }
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
            try
            {
                if (graphMap.classTemplateListMaps != null && graphMap.classTemplateListMaps.Count > 0)
                {
                    MappingItem map = new MappingItem();
                    map.Tag = graphMap.classTemplateListMaps;
                    map.SetTextBlockText("Stub");
                    node.Items.Add(map);
                }
                return true;
            }
            catch (Exception ex)
            {
                Error.SetError(ex, "Error occurred... \r\n" + ex.Message + ex.StackTrace,
                    Category.Exception, Priority.High);
                return false;
            }
        }


        /// <summary>
        /// Populates the list template map.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="templateMapList">The template map list.</param>
        /// <returns></returns>
        bool PopulateTemplateNode(MappingItem node, TemplateMap templateMap)
        {
            try
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
            catch (Exception ex)
            {
                Error.SetError(ex, "Error occurred... \r\n" + ex.Message + ex.StackTrace,
                    Category.Exception, Priority.High);
                return false;
            }
        }

        /// <summary>
        /// Populates the list role map.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="roleMapList">The role map list.</param>
        /// <returns></returns>
        public bool PopulateRoleNode(MappingItem node, RoleMap roleMap)
        {
            try
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
            catch (Exception ex)
            {
                Error.SetError(ex, "Error occurred... \r\n" + ex.Message + ex.StackTrace,
                    Category.Exception, Priority.High);
                return false;
            }
        }

        /// <summary>
        /// Populates the node class map.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="classMap">The class map.</param>
        /// <returns></returns>
        public bool PopulateClassNode(MappingItem node, ClassMap classMap)
        {
            try
            {
                //if (node.Items.Count == 0)
                if (classMap.identifiers != null && classMap.identifiers.Count > 0)
                {
                    MappingItem map = new MappingItem();
                    map.Tag = classMap.identifiers;
                    map.SetTextBlockText("Stub");
                    node.Items.Add(map);
                }
                //        node.Items.Add(new MappingItem { Header = "Stub", Tag = classMap.templateMaps });
                //else
                //  PopulateTemplateMap(node, classMap.templateMaps[0]);
                return true;
            }
            catch (Exception ex)
            {
                Error.SetError(ex, "Error occurred... \r\n" + ex.Message + ex.StackTrace,
                    Category.Exception, Priority.High);
                return false;
            }
        }

        /// <summary>
        /// Populates the template map.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="templateMap">The template map.</param>
        /// <returns></returns>
        public bool PopulateTemplateMap(MappingItem node, TemplateMap templateMap)
        {
            try
            {
                MappingItem newNode = AddNode(templateMap.name, templateMap, node);
                node.Items.Add(newNode);
                return true;
            }
            catch (Exception ex)
            {
                Error.SetError(ex, "Error occurred... \r\n" + ex.Message + ex.StackTrace,
                    Category.Exception, Priority.High);
                return false;
            }
        }

        /// <summary>
        /// Populates the role map.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="roleMap">The role map.</param>
        /// <returns></returns>
        bool PopulateRoleMap(MappingItem node, RoleMap roleMap)
        {
            try
            {
                string roleName = roleMap.name;

                if (!roleMap.isMapped && (roleMap.type == RoleType.Property || roleMap.type == RoleType.FixedValue))
                {
                    roleName += unmappedToken;
                }

                MappingItem newNode = AddNode(roleName, roleMap, node);
                node.Items.Add(newNode);

                return true;
            }
            catch (Exception ex)
            {
                Error.SetError(ex, "Error occurred... \r\n" + ex.Message + ex.StackTrace,
                    Category.Exception, Priority.High);
                return false;
            }
        }

        /// <summary>
        /// Populates the value map.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="classMap">The class map.</param>
        /// <returns></returns>
        private bool PopulateValueNode(MappingItem node, ValueMap valueMap)
        {
            //if (valueMap.internalValue.Equals(String.Empty))
            //{
            //    node.Header += unmappedToken;
            //}

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
            try
            {
                MappingItem newNode = AddNode(classMap.name, classMap, node);
                node.Items.Add(newNode);
                return true;
            }
            catch (Exception ex)
            {
               throw ex;
            }
        }

        #endregion
        public void SpinnerEventHandler(SpinnerEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
               throw ex;
            }
        }

        private void ChangeControlsState(bool enabled)
        {
            try
            {
                if (mappingCRUD.mapping != null)
                {
                    txtLabel.IsEnabled = enabled;
                    btnAddGraph.IsEnabled = enabled;

                    if (mappingCRUD.mapping.graphMaps.Count > 0)
                    {
                        tvwMapping.IsEnabled = enabled;
                        btnAddTemplate.IsEnabled = enabled;
                        btnMap.IsEnabled = enabled;
                       // btnEditFixedRole.IsEnabled = enabled;

                        if (enabled && cbValueList.Items.Count > 0)
                        {
                            cbValueList.IsEnabled = enabled;
                            btnAddValueList.IsEnabled = enabled;
                        }
                        else if (enabled && cbValueList.SelectedItem != null)
                        {
                            cbValueList.IsEnabled = false;
                            btnAddValueList.IsEnabled = false;
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
            catch (Exception ex)
            {
                Error.SetError(ex, "Error occurred... \r\n" + ex.Message + ex.StackTrace,
                    Category.Exception, Priority.High);
            }
        }
    }
}
