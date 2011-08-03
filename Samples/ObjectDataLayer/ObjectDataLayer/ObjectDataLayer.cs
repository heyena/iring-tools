using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;
using Ciloci.Flee;
using log4net;
using Ninject;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.sdk.objects.widgets;

namespace org.iringtools.sdk.objects
{
  public class ObjectDataLayer : BaseDataLayer, IDataLayer2
  {
    WidgetLibrary _widgetProvider = null;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(ObjectDataLayer));

    //NOTE: This is required to deliver settings to constructor.
    //NOTE: Other objects could be requested on an as needed basis.
    [Inject]
    public ObjectDataLayer(AdapterSettings settings)
      : base(settings)
    {
      _widgetProvider = new WidgetLibrary();

      _settings = settings;
    }

    public override DataDictionary GetDictionary()
    {
      DataDictionary dataDictionary = new DataDictionary();

      List<DataObject> dataObjects = new List<DataObject>();

      DataObject widget = new DataObject
      {
        objectName = "Widget",
        keyDelimeter = "_",
      };

      List<KeyProperty> keyProperties = new List<KeyProperty>
      {
        new KeyProperty
        {
          keyPropertyName = "Id",
        },
      };

      widget.keyProperties = keyProperties;

      List<DataProperty> dataProperties = new List<DataProperty>
      {
        new DataProperty
        {
          propertyName = "Id",
          keyType = KeyType.unassigned,
          dataLength = 32,
          numberOfDecimals = 0,
          dataType = DataType.Int32,
        },
        new DataProperty
        {
          propertyName = "Name",
          dataLength = 32,
          dataType = DataType.String,
          showOnIndex = true,
        },
        new DataProperty
        {
          propertyName = "Description",
          dataLength = 256,
          dataType = DataType.String,
        },
        new DataProperty
        {
          propertyName = "Length",
          dataLength = 32,
          numberOfDecimals = 2,
          dataType = DataType.Double,
        },
        new DataProperty
        {
          propertyName = "Width",
          dataLength = 32,
          numberOfDecimals = 2,
          dataType = DataType.Double,
        },
        new DataProperty
        {
          propertyName = "Height",
          dataLength = 32,
          numberOfDecimals = 2,
          dataType = DataType.Double,
        },
        new DataProperty
        {
          propertyName = "Weight",
          dataLength = 32,
          numberOfDecimals = 2,
          dataType = DataType.Double,
        },
        new DataProperty
        {
          propertyName = "LengthUOM",
          dataLength = 32,
          dataType = DataType.String,
        },
        new DataProperty
        {
          propertyName = "WeightUOM",
          dataLength = 32,
          dataType = DataType.String,
        },
        new DataProperty
        {
          propertyName = "Material",
          dataLength = 128,
          dataType = DataType.String,
        },
        new DataProperty
        {
          propertyName = "Color",
          dataLength = 32,
          dataType = DataType.String,
        },
      };

      widget.dataProperties = dataProperties;

      dataObjects.Add(widget);

      dataDictionary.dataObjects = dataObjects;

      return dataDictionary;
    }

    public override IList<IDataObject> Get(string objectType, IList<string> identifiers)
    {
      _dataObjects = new List<IDataObject>();

      try
      {
        switch (objectType.ToUpper())
        {
          case "WIDGET":

            foreach (string identifier in identifiers)
            {
              int id = 0;
              Int32.TryParse(identifier, out id);

              Widget widget = _widgetProvider.ReadWidget(id);

              IDataObject dataObject = FormDataObject(widget);

              _dataObjects.Add(dataObject);
            }
            break;

          default:
            throw new Exception("Invalid object type provided");
        }

        return _dataObjects;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetList: " + ex);
        throw new Exception("Error while getting a list of data objects of type [" + objectType + "].", ex);
      }
    }

    public override IList<IDataObject> Get(string objectType, DataFilter filter, int pageSize, int startIndex)
    {
      throw new NotImplementedException();
    }

    public override IList<string> GetIdentifiers(string objectType, DataFilter filter)
    {
      try
      {
        List<string> identifiers = new List<string>();

        //NOTE: pageSize of 0 indicates that all rows should be returned.
        IList<IDataObject> dataObjects = Get(objectType, filter, 0, 0);

        foreach (IDataObject dataObject in dataObjects)
        {
          identifiers.Add((string)dataObject.GetPropertyValue("Id"));
        }

        return identifiers;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetIdentifiers: " + ex);

        throw new Exception(
          "Error while getting a list of identifiers of type [" + objectType + "].",
          ex
        );
      }
    }

    public override IList<IDataObject> GetRelatedObjects(IDataObject dataObject, string relatedObjectType)
    {
      throw new NotImplementedException();
    }

    public override Response Post(IList<IDataObject> dataObjects)
    {
      Response response = new Response();
      string objectType = String.Empty;

      if (dataObjects == null || dataObjects.Count == 0)
      {
        Status status = new Status();
        status.Level = StatusLevel.Warning;
        status.Messages.Add("Nothing to update.");
        response.Append(status);
        return response;
      }

      try
      {
        objectType = ((GenericDataObject)dataObjects.FirstOrDefault()).ObjectType;

        switch (objectType.ToUpper())
        {
          case "WIDGET":
            foreach (IDataObject dataObject in dataObjects)
            {
              Status status = new Status();
              
              Widget widget = FormWidget(dataObject);
              string identifier = widget.Id.ToString();
              status.Identifier = identifier;

              int result = _widgetProvider.UpdateWidgets(new List<Widget>{ widget });

              string message = String.Empty;
              if (result == 0)
              {
                message = String.Format(
                  "DataObject [{0}] posted successfully.",
                  identifier
                );
              }
              else
              {
                message = String.Format(
                  "Error while posting DataObject [{0}].",
                  identifier
                );
              }

              status.Messages.Add(message);

              response.Append(status);
            }
            break;

          default:
            throw new Exception("Invalid object type provided");
        }

        return response;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Post: " + ex);

        throw new Exception(
          "Error while posting dataObjects of type [" + objectType + "].",
          ex
        );
      }
    }

    public override Response Delete(string objectType, IList<string> identifiers)
    {
      Response response = new Response();

      if (identifiers == null || identifiers.Count == 0)
      {
        Status status = new Status();
        status.Level = StatusLevel.Warning;
        status.Messages.Add("Nothing to delete.");
        response.Append(status);
        return response;
      }

      foreach (string identifier in identifiers)
      {
        Status status = new Status();
        status.Identifier = identifier;
        
        try
        {
          int id = 0;
          Int32.TryParse(identifier, out id);

          int result = _widgetProvider.DeleteWidgets(id);

          string message = String.Empty;
          if (result == 0)
          {
            message = String.Format(
              "DataObject [{0}] deleted successfully.",
              identifier
            );
          }
          else
          {
            message = String.Format(
              "Error while deleting dataObject [{0}].",
              identifier
            );
          }

          status.Messages.Add(message);
        }
        catch (Exception ex)
        {
          _logger.Error("Error in Delete: " + ex);

          status.Level = StatusLevel.Error;

          string message = String.Format(
            "Error while deleting dataObject [{0}]. {1}",
            identifier,
            ex
          );

          status.Messages.Add(message);
        }

        response.Append(status);
      }

      return response;
    }

    public override Response Delete(string objectType, DataFilter filter)
    {
      throw new NotImplementedException();
    }

    private IDataObject FormDataObject(Widget widget)
    {
      try
      {
        IDataObject dataObject = new GenericDataObject();

        dataObject.SetPropertyValue("Id", widget.Id);
        dataObject.SetPropertyValue("Name", widget.Name);
        dataObject.SetPropertyValue("Description", widget.Description);
        dataObject.SetPropertyValue("Length", widget.Length);
        dataObject.SetPropertyValue("Width", widget.Width);
        dataObject.SetPropertyValue("Height", widget.Height);
        dataObject.SetPropertyValue("Weight", widget.Weight);
        dataObject.SetPropertyValue("LengthUOM", widget.LengthUOM);
        dataObject.SetPropertyValue("WeightUOM", widget.WeightUOM);
        dataObject.SetPropertyValue("Material", widget.Material);
        dataObject.SetPropertyValue("Color", widget.Color);

        return dataObject;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in FormDataObject: " + ex);

        throw new Exception(
          "Error while forming a dataObject from a widget.",
          ex
        );
      }
    }

    private Widget FormWidget(IDataObject dataObject)
    {
      try
      {
        Widget widget = new Widget();

        if (dataObject.GetPropertyValue("Id") != null)
        {
          string identifier = dataObject.GetPropertyValue("Id").ToString();
          int id = 0;
          Int32.TryParse(identifier, out id);
          widget.Id = id;
        }

        if (dataObject.GetPropertyValue("Name") != null)
        {
          widget.Name = dataObject.GetPropertyValue("Name").ToString();
        }

        if (dataObject.GetPropertyValue("Description") != null)
        {
          widget.Description = dataObject.GetPropertyValue("Description").ToString();
        }

        if (dataObject.GetPropertyValue("Material") != null)
        {
          widget.Material = dataObject.GetPropertyValue("Material").ToString();
        }

        if (dataObject.GetPropertyValue("Length") != null)
        {
          string lengthValue = dataObject.GetPropertyValue("Length").ToString();
          double length = 0;
          Double.TryParse(lengthValue, out length);
          widget.Length = length;
        }

        if (dataObject.GetPropertyValue("Width") != null)
        {
          string widthValue = dataObject.GetPropertyValue("Width").ToString();
          double width = 0;
          Double.TryParse(widthValue, out width);
          widget.Width = width;
        }

        if (dataObject.GetPropertyValue("Height") != null)
        {
          string heightValue = dataObject.GetPropertyValue("Height").ToString();
          double height = 0;
          Double.TryParse(heightValue, out height);
          widget.Height = height;
        }

        if (dataObject.GetPropertyValue("Weight") != null)
        {
          string weightValue = dataObject.GetPropertyValue("Weight").ToString();
          double weight = 0;
          Double.TryParse(weightValue, out weight);
          widget.Weight = weight;
        }

        if (dataObject.GetPropertyValue("LengthUOM") != null)
        {
          string lengthUOMValue = dataObject.GetPropertyValue("LengthUOM").ToString();
          LengthUOM lengthUOM = LengthUOM.feet;
          Enum.TryParse<LengthUOM>(lengthUOMValue, out lengthUOM);
          widget.LengthUOM = lengthUOM;
        }

        if (dataObject.GetPropertyValue("WeightUOM") != null)
        {
          string weightUOMValue = dataObject.GetPropertyValue("WeightUOM").ToString();
          WeightUOM weightUOM = WeightUOM.grams;
          Enum.TryParse<WeightUOM>(weightUOMValue, out weightUOM);
          widget.WeightUOM = weightUOM;
        }

        if (dataObject.GetPropertyValue("Color") != null)
        {
          string colorValue = dataObject.GetPropertyValue("Color").ToString();
          Color color = Color.Black;
          Enum.TryParse<Color>(colorValue, out color);
          widget.Color = color;
        }
        return widget;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in FormWidget: " + ex);

        throw new Exception(
          "Error while forming a Widget from a DataObject.",
          ex
        );
      }
    }
  }
}
