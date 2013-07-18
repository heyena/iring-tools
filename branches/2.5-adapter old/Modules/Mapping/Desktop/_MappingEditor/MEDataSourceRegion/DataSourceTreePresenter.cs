using System.Collections.Generic;
using System.Linq;
using System.Windows;

using System.Windows.Controls;
using org.iringtools.informationmodel.events;
using PrismContrib.Base;

using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Composite.Logging;

using org.iringtools.modulelibrary.usercontrols;
using org.iringtools.modulelibrary.events;
using org.iringtools.modulelibrary.extensions;
using org.iringtools.modulelibrary.types;

using org.iringtools.ontologyservice.presentation.presentationmodels;

using org.iringtools.library;

#if SILVERLIGHT
using System;
using System.Windows.Input;
#else
#endif


namespace org.iringtools.modules.medatasourceregion
{
  public class DataSourceTreePresenter : PresenterBase<IDataSourceTreeView>
  {
    private IEventAggregator aggregator = null;
    private IIMPresentationModel model = null;

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

    }

    public void SpinnerEventHandler(SpinnerEventArgs e)
    {
      try
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
      catch (Exception ex)
      {
        throw ex;
      }
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

        // handle the GetDataDictionary event 
        if (args.CheckForType(CompletedEventType.GetDataDictionary))
        {
          if (args.Error != null)
          {
            MessageBox.Show(args.FriendlyErrorMessage, "Get Data Dictionary Error", MessageBoxButton.OK);
            return;
          }

          GetDictionaryHandler(args);
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    /// <summary>
    /// Loads the data source tree.
    /// </summary>
    /// <param name="dataDictionary">The data dictionary.</param>
    void GetDictionaryHandler(CompletedEventArgs e)
    {
      try
      {
        // Ensure we have a valid parameter
        DataDictionary dictionary = e.Data as DataDictionary;
        if (dictionary == null)
          return;

        tvwDataDictionary.Items.Clear();
        tvwDataDictionary.Tag = dictionary;
        // Note that we only load first level nodes
        foreach (org.iringtools.library.DataObject dataObject in dictionary.dataObjects)
        {
          tvwDataDictionary.Items.Add(AddNode(dataObject.objectName, dataObject, tvwDataDictionary));
        }
      }
      catch (Exception ex)
      {
        Error.SetError(ex, "Error occurred... \r\n" + ex.Message + ex.StackTrace,
            Category.Exception, Priority.High);
      }
    }

    private DataObjectItem AddNode(string header, object tag, object parent)
    {
      try
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
          if (parent is DataObjectItem)
          {
            node.SetImageSource("relation.png");
            node.SetTextBlockText(header);
            node.RelationshipName = header;
            node.SetTooltipText("DataRelationship : " + header);
          }
          else
          {
            node.SetImageSource("object.png");
            node.SetTextBlockText(header);
            node.SetTooltipText("Object : " + header);
          }
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
          if (((DataProperty)tag).keyType != KeyType.unassigned)
          {
            node.SetImageSource("key.png");
            node.SetTooltipText("Key Property : " + header);
          }
          else
          {
            node.SetImageSource("property.png");
            node.SetTooltipText("Property : " + header);
          }
          node.SetTextBlockText(header);
        }


        // Subscribe to it's events

        node.MouseLeftButtonUp += nodeMouseLeftButtonUpHandler;
        node.Expanded += nodeExpandedHandler;

        bool isProcessed = false;

        // Now populate it as applicable
        if (tag is org.iringtools.library.DataObject)
          isProcessed = PopulateDataObjectNode(node, (org.iringtools.library.DataObject)tag);

        return node;
      }
      catch (Exception ex)
      {
        Error.SetError(ex, "Error occurred... \r\n" + ex.Message + ex.StackTrace,
            Category.Exception, Priority.High);
        return null;
      }
    }

    /// <summary>
    /// Populates the graph node.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="graphMap">The graph map.</param>
    /// <returns></returns>
    bool PopulateDataObjectNode(DataObjectItem node, org.iringtools.library.DataObject dataObject)
    {
      try
      {
        DataObjectItem item = null;

        if (dataObject == null)
          return false;
        if (dataObject.keyProperties != null && dataObject.keyProperties.Count > 0)
        {
          item = new DataObjectItem();
          item.SetTextBlockText("Stub");
          item.Tag = dataObject.keyProperties;
          item.SetTooltipText("Keys : " + dataObject.keyProperties.ToString());
          node.Items.Add(item);
        }

        if (dataObject.dataProperties != null && dataObject.dataProperties.Count > 0)
        {
          item = new DataObjectItem();
          item.SetTextBlockText("Stub");
          item.Tag = dataObject.dataProperties;
          item.SetTooltipText("Object : " + dataObject.dataProperties.ToString());
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
    bool PopulateDataObject(DataObjectItem node, org.iringtools.library.DataObject dataObject)
    {
      try
      {
        DataObjectItem newNode = AddNode(dataObject.objectName, dataObject, node);
        node.Items.Add(newNode);
        return true;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    /// <summary>
    /// Populates the template map.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="keyProperty">Key property</param>
    /// <returns></returns>
    private bool PopulateKeyProperty(DataObjectItem node, DataProperty keyProperty)
    {
      try
      {
        DataObjectItem newNode = AddNode(keyProperty.propertyName, keyProperty, node);
        node.Items.Add(newNode);
        return true;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    /// <summary>
    /// Populates the role map.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="roleMap">The role map.</param>
    /// <returns></returns>
    bool PopulateDataProperty(DataObjectItem node, DataProperty dataProperty)
    {
      try
      {
        DataObjectItem newNode = AddNode(dataProperty.propertyName, dataProperty, node);
        node.Items.Add(newNode);
        return true;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    /// <summary>
    /// Populates the class map.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="classMap">The class map.</param>
    /// <returns></returns>
    bool PopulateDataRelationship(DataObjectItem node, org.iringtools.library.DataObject dataObject, string relationshipName)
    {
      try
      {
        DataObjectItem newNode = AddNode(relationshipName, dataObject, node);
        newNode.ParentObjectName = node.DataObject.objectName;
        node.Items.Add(newNode);
        return true;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    void nodeMouseLeftButtonUpHandler(object sender, MouseButtonEventArgs e)
    {
      try
      {
        DataObjectItem selectedNode = sender as DataObjectItem;

        if (selectedNode == null)
          return;

        model.DetailProperties.Clear();
        model.SelectedDataSourcePropertyName = selectedNode.Header.ToString();
        model.SelectedDataObject = selectedNode;

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
          keyValuePair = new KeyValuePair<string, string>("Datatype", dataProperty.dataType.ToString());
          model.DetailProperties.Add(keyValuePair);
        }

        if (selectedNode.Tag is DataRelationship)
        {
          DataRelationship dataRelationship = (DataRelationship)selectedNode.Tag;
          KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>("Related Object", dataRelationship.relatedObjectName);
          model.DetailProperties.Add(keyValuePair);
        }

        if (selectedNode.Tag is PropertyMap)
        {
          PropertyMap propertyMap = (PropertyMap)selectedNode.Tag;
          KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>("Data Property", propertyMap.dataPropertyName);
          model.DetailProperties.Add(keyValuePair);
          keyValuePair = new KeyValuePair<string, string>("Related Property", propertyMap.relatedPropertyName);
          model.DetailProperties.Add(keyValuePair);
        }

        e.Handled = true;
      }
      catch (Exception ex)
      {
        Error.SetError(ex, "Error occurred... \r\n" + ex.Message + ex.StackTrace,
            Category.Exception, Priority.High);
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
      try
      {
        // If the sender is not a valid node return
        DataObjectItem selectedNode = sender as DataObjectItem;
        if (selectedNode == null)
          return;
        DataObjectItem dataPropertyNode = null;
        DataObjectItem dataRelationNode = null;
        // DataObjectItem keyPropertyNode = null;

        foreach (DataObjectItem dataObjectItem in selectedNode.Items)
        {
          if (dataObjectItem.Tag is List<DataProperty> || dataObjectItem.Tag is org.iringtools.library.DataObject)
          {
            dataPropertyNode = dataObjectItem as DataObjectItem;
          }
          else if (dataObjectItem.Tag is List<DataRelationship>)
          {
            dataRelationNode = dataObjectItem as DataObjectItem;
          }
        }


        if (dataPropertyNode == null)
          return;

        // If expanded then the tree has nodes so we'll grab the
        // first node and see if it is a stub.  If it isn't then
        // we have nothing to do
        if (dataPropertyNode.itemTextBlock.Text != "Stub")
          return;

        // Remove the stub
        selectedNode.Items.Clear();

        bool isProcessed = false;

        // Add the key nodes
        //int a = 0;
        //if (keyPropertyNode.Tag is List<KeyProperty>)
        //    foreach (KeyProperty keyProperty in ((List<KeyProperty>)keyPropertyNode.Tag))
        //        a++;
        //isProcessed = PopulateKeyProperty(selectedNode, keyProperty);

        if (dataPropertyNode.Tag is org.iringtools.library.DataObject)
          isProcessed = PopulateDataObject(selectedNode, (org.iringtools.library.DataObject)dataPropertyNode.Tag);


        //add property nodes
        if (dataPropertyNode.Tag is List<DataProperty>)
          foreach (DataProperty dataProperty in ((List<DataProperty>)dataPropertyNode.Tag))
            isProcessed = PopulateDataProperty(selectedNode, dataProperty);

        if (dataRelationNode == null) return;

        if (dataRelationNode.Tag is List<DataRelationship>)
          foreach (DataRelationship dataRelationship in ((List<DataRelationship>)dataRelationNode.Tag))
          {
            org.iringtools.library.DataObject dataObject = ((DataDictionary)tvwDataDictionary.Tag).dataObjects.First(c => c.objectName == dataRelationship.relatedObjectName);
            if (dataObject != null)
              isProcessed = PopulateDataRelationship(selectedNode, dataObject, dataRelationship.relationshipName);
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