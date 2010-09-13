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
    private Dictionary<string, List<string>> _classIdentifiers = null; // dictionary of class ids and list of identifiers
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

        foreach (var pair in _graphMap.classTemplateListMaps)
        {
          ClassMap classMap = pair.Key;
          List<TemplateMap> templateMaps = pair.Value;

          ClassObject classObject = new ClassObject
          {
            classId = classMap.classId,
            name = classMap.name,
            identifier = _classIdentifiers[classMap.classId][i],
          };

          dto.classObjects.Add(classObject);

          foreach (TemplateMap templateMap in templateMaps)
          {
            TemplateObject templateObject = new TemplateObject
            {
              templateId = templateMap.templateId,
              name = templateMap.name,
            };

            classObject.templateObjects.Add(templateObject);

            foreach (RoleMap roleMap in templateMap.roleMaps)
            {
              RoleObject roleObject = new RoleObject
              {
                type = roleMap.type,
                roleId = roleMap.roleId,
                name = roleMap.name,
              };

              templateObject.roleObjects.Add(roleObject);
              string value = roleMap.value;

              if (roleMap.type == RoleType.Property)
              {
                string propertyName = roleMap.propertyName.Substring(_graphMap.dataObjectMap.Length + 1);
                value = Convert.ToString(_dataObjects[i].GetPropertyValue(propertyName));

                if (!String.IsNullOrEmpty(roleMap.valueList))
                {
                  value = _mapping.ResolveValueList(roleMap.valueList, value);
                  value = value.Replace(RDL_NS.NamespaceName, "rdl:");
                }
              }
              else if (roleMap.classMap != null)
              {
                value = "#" + _classIdentifiers[roleMap.classMap.classId][i];
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
      ClassMap classMap = _graphMap.classTemplateListMaps.First().Key;
      List<string> identifiers = new List<string>();
      List<int> addChangeDtoIndexes = new List<int>();
      
      for (int i = 0; i < _dataTransferObjects.Count; i++)
      {
        DataTransferObject dataTransferObject = _dataTransferObjects[i];

        if (dataTransferObject.transferType != TransferType.Sync && dataTransferObject.transferType != TransferType.Delete)
        {
          ClassObject classObject = dataTransferObject.GetClassObject(classMap.classId);

          if (classObject != null)
          {
            identifiers.Add(classObject.identifier);
            addChangeDtoIndexes.Add(i);
          }
        }
      }
      #endregion

      IList<IDataObject> dataObjects = _dataLayer.Create(_graphMap.dataObjectMap, identifiers);
      int dataObjectIndex = 0;

      foreach (int i in addChangeDtoIndexes)
      {
        IDataObject dataObject = dataObjects[dataObjectIndex++];
        PopulateDataObjects(ref dataObject, classMap.classId, i);
      }

      return dataObjects;
    }

    public IList<string> GetDeletingDataObjects(string graphName, ref XElement xml)
    {
      _graphMap = _mapping.FindGraphMap(graphName);
      _dataTransferObjects = SerializationExtensions.ToObject<DataTransferObjects>(xml);

      ClassMap classMap = _graphMap.classTemplateListMaps.First().Key;
      List<string> identifiers = new List<string>();

      foreach (DataTransferObject dataTransferObject in _dataTransferObjects)
      {
        if (dataTransferObject.transferType == TransferType.Delete)
        {
          ClassObject classObject = dataTransferObject.GetClassObject(classMap.classId);
          identifiers.Add(classObject.identifier);
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

      foreach (ClassMap classMap in _graphMap.classTemplateListMaps.Keys)
      {
        List<string> classIdentifiers = new List<string>();

        foreach (string identifier in classMap.identifiers)
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
                classIdentifiers[i] += classMap.identifierDelimeter + value;
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
                  classIdentifiers[i] += classMap.identifierDelimeter + value;
                }
              }
            }
          }
        }

        _classIdentifiers[classMap.classId] = classIdentifiers;
      }
    }

    private void PopulateDataObjects(ref IDataObject dataObject, string classId, int dataTransferObjectIndex)
    {
      KeyValuePair<ClassMap, List<TemplateMap>> classTemplateListMap = _graphMap.GetClassTemplateListMap(classId);
      List<TemplateMap> templateMaps = classTemplateListMap.Value;
      ClassObject classObject = _dataTransferObjects[dataTransferObjectIndex].GetClassObject(classId);

      foreach (TemplateMap templateMap in templateMaps)
      {
        TemplateObject templateObject = classObject.GetTemplateObject(templateMap);

        if (templateObject != null)
        {
          foreach (RoleMap roleMap in templateMap.roleMaps)
          {
            if (roleMap.type == RoleType.Property)
            {
              string propertyName = roleMap.propertyName.Substring(_graphMap.dataObjectMap.Length + 1);

              foreach (RoleObject roleObject in templateObject.roleObjects)
              {
                if (roleObject.roleId == roleMap.roleId)
                {
                  string value = roleObject.value;

                  if (!String.IsNullOrEmpty(roleMap.valueList))
                  {
                    value = _mapping.ResolveValueMap(roleMap.valueList, value);
                  }

                  dataObject.SetPropertyValue(propertyName, value);
                }
              }
            }

            if (roleMap.classMap != null)
            {
              PopulateDataObjects(ref dataObject, roleMap.classMap.classId, dataTransferObjectIndex);
            }
          }
        }
      }
    }

    #endregion
  }
}
