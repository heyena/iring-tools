using System.Collections.Generic;
using System.Linq;
using System.Windows;

using System.Windows.Controls;
using org.iringtools.informationmodel.events;
using PrismContrib.Base;

using InformationModel.Events;

using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Logging;

using org.iringtools.modulelibrary.usercontrols;
using org.iringtools.modulelibrary.events;
using org.iringtools.modulelibrary.extensions;
using org.iringtools.modulelibrary.types;

using org.iringtools.ontologyservice.presentation.presentationmodels;

using org.iringtools.library;

#if SILVERLIGHT
using org.iringtools.modulelibrary.behaviors;
using System.Windows.Interactivity;
#else
#endif


namespace org.iringtools.modules.medatasourceregion
{
    public class DataSourceTreePresenter : PresenterBase<IDataSourceTreeView>
    {
        private IEventAggregator aggregator = null;
        //private IAdapter adapterProxy = null;
        IIMPresentationModel model = null;

        //private ItemsControl itcSpinner { get { return GetControl<ItemsControl>("itcSpinner"); } }
        
        /// <summary>
        /// Gets tvwDataDictionary reference from View
        /// </summary>
        /// <value>The TVW data dictionary.</value>
        private TreeView tvwDataDictionary { get { return GetControl<TreeView>("tvwDataDictionary"); } }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceTreePresenter"/> class.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="model">The model.</param>
        /// <param name="adapterService">The adapter service.</param>
        public DataSourceTreePresenter(IDataSourceTreeView view, IIMPresentationModel model,
            //IWorkingSpinner spinner,
          IEventAggregator aggregator,
          IAdapter adapterProxy)
            : base(view, model)
        {
            this.aggregator = aggregator;
            this.model = model;
           
            adapterProxy.OnDataArrived += OnDataArrivedHandler;    
            aggregator.GetEvent<SpinnerEvent>().Subscribe(SpinnerEventHandler);

#if SILVERLIGHT
            MouseScrollBehavior mouseScrollBehavior = new MouseScrollBehavior();
            Interaction.GetBehaviors(tvwDataDictionary).Add(mouseScrollBehavior);
#endif
        }

        public void SpinnerEventHandler(SpinnerEventArgs e)
        {
          if (!e.ActiveService.Equals("GetMapping"))
          {
            switch (e.Active)
            {
              case SpinnerEventType.Started:
                this.tvwDataDictionary.IsEnabled = false;
                break;

              case SpinnerEventType.Stopped:
                this.tvwDataDictionary.IsEnabled = true;
                break;

              default:
                break;
            }
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

            // handle the GetDataDictionary event 
            if (args.CheckForType(CompletedEventType.GetDataDictionary))
                GetDictionaryHandler(args);

        }

        /// <summary>
        /// Loads the data source tree.
        /// </summary>
        /// <param name="dataDictionary">The data dictionary.</param>
        void GetDictionaryHandler(CompletedEventArgs e)
        {
            // Ensure we have a valid parameter
            DataDictionary dictionary = e.Data as DataDictionary;
            if (dictionary == null)
                return;

            // Note that we only load first level nodes
            foreach (org.iringtools.library.DataObject dataObject in dictionary.dataObjects)
            {
                tvwDataDictionary.Items.Add(AddNode(dataObject.objectName, dataObject, tvwDataDictionary));
            }

        }

        private DataObjectItem AddNode(string header, object tag, object parent)
        {
            DataObjectItem node = null;

            // Setup node
            if (tag is org.iringtools.library.DataObject)
            {
                node = new DataObjectItem
                {
                    Parent = parent,
                    DataObject = (org.iringtools.library.DataObject)tag,
                    DataProperty = null,
                    Tag = tag
                };
                node.SetImageSource("object.png");
                node.SetTextBlockText(header);
                node.SetTooltipText("Object : " + header);
            }
            else if (tag is DataProperty)
            {
                node = new DataObjectItem
                {
                    Parent = parent,
                    DataObject = ((DataObjectItem)parent).DataObject,
                    DataProperty = (DataProperty)tag,
                    Tag = tag,
                };
                node.SetImageSource("property.png");
                node.SetTextBlockText(header);
                node.SetTooltipText("Property : " + header);
            }

            // Subscribe to it's events
            node.Selected += nodeSelectedHandler;
            node.Expanded += nodeExpandedHandler;

            bool isProcessed = false;

            // Now populate it as applicable
            if (tag is org.iringtools.library.DataObject)
              isProcessed = PopulateDataObjectNode(node, (org.iringtools.library.DataObject)tag);


            return node;
        }


        /// <summary>
        /// Populates the graph node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="graphMap">The graph map.</param>
        /// <returns></returns>
        bool PopulateDataObjectNode(DataObjectItem node, org.iringtools.library.DataObject dataObject)
        {
          DataObjectItem item = null;

          if (dataObject == null)
            return false;

          if (dataObject.dataProperties != null && dataObject.dataProperties.Count > 0)
          {
            item = new DataObjectItem();
            item.SetTextBlockText("Stub");
            item.Tag = dataObject.dataProperties;
            item.SetTooltipText("Object : "+dataObject.dataProperties.ToString());
            node.Items.Add(item);
          }

          if (dataObject.dataRelationships != null && dataObject.dataRelationships.Count > 0)
          {
            item = new DataObjectItem();
            item.SetTextBlockText("Stub");
            item.Tag = dataObject.dataRelationships;
            item.SetTooltipText(dataObject.dataRelationships.ToString());
            node.Items.Add(item);
          }
          
          return true;
        }



        /// <summary>
        /// Populates the template map.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="templateMap">The template map.</param>
        /// <returns></returns>
        bool PopulateDataObject(DataObjectItem node, org.iringtools.library.DataObject dataObject)
        {
            DataObjectItem newNode = AddNode(dataObject.objectName, dataObject, node);
            node.Items.Add(newNode);
            return true;
        }

        /// <summary>
        /// Populates the role map.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="roleMap">The role map.</param>
        /// <returns></returns>
        bool PopulateDataProperty(DataObjectItem node, DataProperty dataProperty)
        {
            DataObjectItem newNode = AddNode(dataProperty.propertyName, dataProperty, node);
            node.Items.Add(newNode);
            return true;
        }

        /// <summary>
        /// Populates the class map.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="classMap">The class map.</param>
        /// <returns></returns>
        bool PopulateDataRelationship(DataObjectItem node, DataRelationship dataRelationship)
        {
            DataObjectItem newNode = AddNode(dataRelationship.graphProperty, dataRelationship, node);
            node.Items.Add(newNode);
            return true;
        }

        private void nodeSelectedHandler(object sender, RoutedEventArgs e)
        {
            DataObjectItem selectedNode = sender as DataObjectItem;

            if (selectedNode == null)
                return;

            model.DetailProperties.Clear();
            model.SelectedDataSourcePropertyName = selectedNode.Header.ToString();
            model.SelectedDataObject = selectedNode;

            aggregator.GetEvent<NavigationEvent>().Publish(new NavigationEventArgs
            {
                DetailProcess = DetailType.DataSource,
                SelectedNode = selectedNode,
                Sender = this
            });

            if (selectedNode.Tag is org.iringtools.library.DataObject)
            {
              org.iringtools.library.DataObject dataObject = (org.iringtools.library.DataObject)selectedNode.Tag;
                KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>("DataObject Name", dataObject.objectName);
                model.DetailProperties.Add(keyValuePair);

            }

            if (selectedNode.Tag is DataProperty)
            {
                DataProperty dataProperty = (DataProperty)selectedNode.Tag;
                KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>("Property Name", dataProperty.propertyName);
                model.DetailProperties.Add(keyValuePair);
                keyValuePair = new KeyValuePair<string, string>("Datatype", dataProperty.dataType);
                model.DetailProperties.Add(keyValuePair);
                keyValuePair = new KeyValuePair<string, string>("Is Required", dataProperty.isRequired.ToString());
                model.DetailProperties.Add(keyValuePair);
                keyValuePair = new KeyValuePair<string, string>("Is Key", dataProperty.isPropertyKey.ToString());
                model.DetailProperties.Add(keyValuePair);
            }

            if (selectedNode.Tag is DataRelationship)
            {
                DataRelationship dataRelationship = (DataRelationship)selectedNode.Tag;
                KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>("Related Graph", dataRelationship.relatedObject);
                model.DetailProperties.Add(keyValuePair);
                keyValuePair = new KeyValuePair<string, string>("Graph Property", dataRelationship.graphProperty);
                model.DetailProperties.Add(keyValuePair);
                keyValuePair = new KeyValuePair<string, string>("Cardinality", dataRelationship.cardinality.ToString());
                model.DetailProperties.Add(keyValuePair);
            }
        }

        /// <summary>
        /// Handler when a node is expanded - used to populate
        /// nodes that are a "Stub" (so we can have an arrow)
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        void nodeExpandedHandler(object sender, System.Windows.RoutedEventArgs e)
        {
            // If the sender is not a valid node return
            DataObjectItem selectedNode = sender as DataObjectItem;
            if (selectedNode == null)
                return;
            DataObjectItem childNode = null;

          if(selectedNode.Items.Count > 0)
            childNode = selectedNode.Items[0] as DataObjectItem;

            if (childNode == null)
                return;

            // If expanded then the tree has nodes so we'll grab the
            // first node and see if it is a stub.  If it isn't then
            // we have nothing to do
            if (childNode.itemTextBlock.Text != "Stub")
                return;

            // Remove the stub
            selectedNode.Items.Clear();

            bool isProcessed = false;

            // Add the child nodes
            if (childNode.Tag is org.iringtools.library.DataObject)
              isProcessed = PopulateDataObject(selectedNode, (org.iringtools.library.DataObject)childNode.Tag);

            if (childNode.Tag is List<DataProperty>)
                foreach (DataProperty dataProperty in ((List<DataProperty>)childNode.Tag))
                    isProcessed = PopulateDataProperty(selectedNode, dataProperty);

            if (childNode.Tag is List<DataRelationship>)
                foreach (DataRelationship dataRelationship in ((List<DataRelationship>)childNode.Tag))
                    isProcessed = PopulateDataRelationship(selectedNode, dataRelationship);
        }



    }
}