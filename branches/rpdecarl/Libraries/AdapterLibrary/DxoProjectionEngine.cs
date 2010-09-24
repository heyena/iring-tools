using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.library;
using System.Xml.Linq;
using Ninject;
using log4net;
using System.Text.RegularExpressions;
using VDS.RDF;
using VDS.RDF.Storage;
using org.iringtools.utility;
using Microsoft.ServiceModel.Web;
using System.Xml.Serialization;
using org.iringtools.common.mapping;
using org.iringtools.protocol.manifest;

namespace org.iringtools.adapter.projection
{
  public class DxoProjectionEngine : IProjectionLayer
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(DtoProjectionEngine));    
    private static readonly XNamespace RDL_NS = "http://rdl.rdlfacade.org/data#";
    
    private IDataLayer _dataLayer = null;
    private Mapping _mapping = null;
    private GraphMap _graphMap = null;
    private IList<IDataObject> _dataObjects = null;
    private Dictionary<string, List<string>> _classIdentifiers = null;
    private XNamespace _graphNs = String.Empty;
    private DataTransferObjects _dataTransferObjects;

    [Inject]
    public DxoProjectionEngine(AdapterSettings settings, IDataLayer dataLayer, Mapping mapping)
    {
      _dataObjects = new List<IDataObject>();
      _classIdentifiers = new Dictionary<string, List<string>>();
      
      _dataLayer = dataLayer;
      _mapping = mapping;

      _graphNs = String.Format("{0}{1}/{2}",
        settings["GraphBaseUri"],
        settings["ProjectName"],
        settings["ApplicationName"]
      );
    }

    public XElement ToXml(string graphName, ref IList<IDataObject> dataObjects)
    {
      _graphMap = _mapping.FindGraphMap(graphName);
      _dataObjects = dataObjects;

      PopulateClassIdentifiers();
      DataTransferObjects dtoList = new DataTransferObjects();

      for (int i = 0; i < _dataObjects.Count; i++)
      {
        DataTransferObject dto = new DataTransferObject();
        dtoList.Add(dto);

        foreach (ClassTemplateMap classTemplateMap in _graphMap.ClassTemplateMaps)
        {
          ClassMap classMap = classTemplateMap.ClassMap;
          List<TemplateMap> templateMaps = classTemplateMap.TemplateMaps;

          ClassObject classObject = new ClassObject
          {
            classId = classMap.ClassId,
            name = classMap.Name,
            identifier = _classIdentifiers[classMap.ClassId][i],
          };

          dto.classObjects.Add(classObject);

          foreach (TemplateMap templateMap in templateMaps)
          {
            TemplateObject templateObject = new TemplateObject
            {
              templateId = templateMap.TemplateId,
              name = templateMap.Name,
            };

            classObject.templateObjects.Add(templateObject);

            foreach (RoleMap roleMap in templateMap.RoleMaps)
            {
              RoleObject roleObject = new RoleObject
              {
                type = roleMap.Type,
                roleId = roleMap.RoleId,
                name = roleMap.Name,
              };

              templateObject.roleObjects.Add(roleObject);
              string value = roleMap.Value;

              if (roleMap.Type == RoleType.Property)
              {
                string propertyName = roleMap.PropertyName.Substring(_graphMap.DataObjectName.Length + 1);
                value = Convert.ToString(_dataObjects[i].GetPropertyValue(propertyName));

                if (!String.IsNullOrEmpty(roleMap.ValueListName))
                {
                  value = _mapping.ResolveValueList(roleMap.ValueListName, value);
                  value = value.Replace(RDL_NS.NamespaceName, "rdl:");
                }
              }
              else if (roleMap.ClassMap != null)
              {
                value = "#" + _classIdentifiers[roleMap.ClassMap.ClassId][i];
              }

              roleObject.value = value;
            }
          }
        }
      }

      XElement xml = SerializationExtensions.ToXml<DataTransferObjects>(dtoList);
      return xml;
    }
    
    public IList<IDataObject> ToDataObjects(string graphName, ref XElement xml)
    {
      _graphMap = _mapping.FindGraphMap(graphName);
      _dataTransferObjects = SerializationExtensions.ToObject<DataTransferObjects>(xml);

      #region get class identifers and add/change DTO indexes
      ClassMap classMap = _graphMap.ClassTemplateMaps.First().ClassMap;
      List<string> identifiers = new List<string>();
      List<int> addChangeDtoIndexes = new List<int>();
      
      for (int i = 0; i < _dataTransferObjects.Count; i++)
      {
        DataTransferObject dataTransferObject = _dataTransferObjects[i];

        if (dataTransferObject.transferType != TransferType.Sync && dataTransferObject.transferType != TransferType.Delete)
        {
          ClassObject classObject = dataTransferObject.GetClassObject(classMap.ClassId);

          if (classObject != null)
          {
            identifiers.Add(classObject.identifier);
            addChangeDtoIndexes.Add(i);
          }
        }
      }
      #endregion

      IList<IDataObject> dataObjects = _dataLayer.Create(_graphMap.DataObjectName, identifiers);
      int dataObjectIndex = 0;

      foreach (int i in addChangeDtoIndexes)
      {
        IDataObject dataObject = dataObjects[dataObjectIndex++];
        PopulateDataObjects(ref dataObject, classMap.ClassId, i);
      }

      return dataObjects;
    }

    public IList<string> GetDeletingIdentifiers(string graphName, ref XElement xml)
    {
      _graphMap = _mapping.FindGraphMap(graphName);
      _dataTransferObjects = SerializationExtensions.ToObject<DataTransferObjects>(xml);

      List<string> identifiers = new List<string>();

      foreach (DataTransferObject dataTransferObject in _dataTransferObjects)
      {
        if (dataTransferObject.transferType == TransferType.Delete)
        {
          identifiers.Add(dataTransferObject.classObjects[0].identifier);
        }
      }

      return identifiers;
    }

    #region helper methods
    private string ExtractId(string qualifiedId)
    {
      if (String.IsNullOrEmpty(qualifiedId) || !qualifiedId.Contains(":"))
        return qualifiedId;

      return qualifiedId.Substring(qualifiedId.IndexOf(":") + 1);
    }

    private string TitleCase(string value)
    {
      string returnValue = String.Empty;
      string[] words = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

      foreach (string word in words)
      {
        returnValue += word.Substring(0, 1).ToUpper();

        if (word.Length > 1)
          returnValue += word.Substring(1).ToLower();
      }

      return returnValue;
    }

    private void PopulateClassIdentifiers()
    {
      _classIdentifiers.Clear();

      foreach (ClassTemplateMap classTemplateMap in _graphMap.ClassTemplateMaps)
      {
        ClassMap classMap = classTemplateMap.ClassMap;

        List<string> classIdentifiers = new List<string>();

        foreach (string identifier in classMap.Identifiers)
        {
          // identifier is a fixed value
          if (identifier.StartsWith("#") && identifier.EndsWith("#"))
          {
            string value = identifier.Substring(1, identifier.Length - 2);

            for (int i = 0; i < _dataObjects.Count; i++)
            {
              if (classIdentifiers.Count == i)
              {
                classIdentifiers.Add(value);
              }
              else
              {
                classIdentifiers[i] += classMap.IdentifierDelimeter + value;
              }
            }
          }
          else  // identifier comes from a property
          {
            string[] property = identifier.Split('.');
            string objectName = property[0].Trim();
            string propertyName = property[1].Trim();

            if (_dataObjects != null)
            {
              for (int i = 0; i < _dataObjects.Count; i++)
              {
                string value = Convert.ToString(_dataObjects[i].GetPropertyValue(propertyName));

                if (classIdentifiers.Count == i)
                {
                  classIdentifiers.Add(value);
                }
                else
                {
                  classIdentifiers[i] += classMap.IdentifierDelimeter + value;
                }
              }
            }
          }
        }

        _classIdentifiers[classMap.ClassId] = classIdentifiers;
      }
    }

    private void PopulateDataObjects(ref IDataObject dataObject, string classId, int dataTransferObjectIndex)
    {
      ClassTemplateMap classTemplateMap = _graphMap.GetClassTemplateMap(classId);
      List<TemplateMap> templateMaps = classTemplateMap.TemplateMaps;
      ClassObject classObject = _dataTransferObjects[dataTransferObjectIndex].GetClassObject(classId);

      foreach (TemplateMap templateMap in templateMaps)
      {
        TemplateObject templateObject = classObject.GetTemplateObject(templateMap);

        if (templateObject != null)
        {
          foreach (RoleMap roleMap in templateMap.RoleMaps)
          {
            if (roleMap.Type == RoleType.Property)
            {
              string propertyName = roleMap.PropertyName.Substring(_graphMap.DataObjectName.Length + 1);

              foreach (RoleObject roleObject in templateObject.roleObjects)
              {
                if (roleObject.roleId == roleMap.RoleId)
                {
                  string value = roleObject.value;

                  if (!String.IsNullOrEmpty(roleMap.ValueListName))
                  {
                    value = _mapping.ResolveValueMap(roleMap.ValueListName, value);
                  }

                  dataObject.SetPropertyValue(propertyName, value);
                }
              }
            }

            if (roleMap.ClassMap != null)
            {
              PopulateDataObjects(ref dataObject, roleMap.ClassMap.ClassId, dataTransferObjectIndex);
            }
          }
        }
      }
    }

    #endregion
  }
}
