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
  public class DtoProjectionEngine : IProjectionLayer
  {
    private static readonly XNamespace DTO_NS = "http://iringtools.org/adapter/library/dto";
    private static readonly XNamespace RDL_NS = "http://rdl.rdlfacade.org/data#";
    
    private static readonly ILog _logger = LogManager.GetLogger(typeof(DtoProjectionEngine));

    private IDataLayer _dataLayer = null;
    private Mapping _mapping = null;
    private GraphMap _graphMap = null;
    private IList<IDataObject> _dataObjects = null;
    private Dictionary<string, List<string>> _classIdentifiers = null; // dictionary of class ids and list of identifiers
    private DataTransferObjects _dataTransferObjects;

    [Inject]
    public DtoProjectionEngine(AdapterSettings settings, IDataLayer dataLayer, Mapping mapping)
    {
      _dataObjects = new List<IDataObject>();
      _classIdentifiers = new Dictionary<string, List<string>>();

      _dataLayer = dataLayer;
      _mapping = mapping;
    }

    public XElement GetXml(string graphName, ref IList<IDataObject> dataObjects)
    {
      _graphMap = _mapping.FindGraphMap(graphName);
      _dataObjects = dataObjects;

      PopulateClassIdentifiers();

      XElement dtoList = new XElement(DTO_NS + "dataTransferObjects");

      for (int i = 0; i < _dataObjects.Count; i++)
      {
        XElement dto = new XElement(DTO_NS + "dataTransferObject");
        dtoList.Add(dto);

        XElement classObjects = new XElement(DTO_NS + "classObjects");
        dto.Add(classObjects);

        foreach (var pair in _graphMap.classTemplateListMaps)
        {
          ClassMap classMap = pair.Key;
          List<TemplateMap> templateMaps = pair.Value;

          XElement classObject = new XElement(DTO_NS + "classObject");
          classObjects.Add(classObject);

          classObject.Add(new XElement(DTO_NS + "classId", classMap.classId));
          classObject.Add(new XElement(DTO_NS + "name", classMap.name));
          classObject.Add(new XElement(DTO_NS + "identifier", _classIdentifiers[classMap.classId][i]));

          XElement templateObjects = new XElement(DTO_NS + "templateObjects");
          classObject.Add(templateObjects);

          foreach (TemplateMap templateMap in templateMaps)
          {
            XElement templateObject = new XElement(DTO_NS + "templateObject");
            templateObjects.Add(templateObject);

            templateObject.Add(new XElement(DTO_NS + "templateId", templateMap.templateId));
            templateObject.Add(new XElement(DTO_NS + "name", templateMap.name));

            XElement roleObjects = new XElement(DTO_NS + "roleObjects");
            templateObject.Add(roleObjects);

            foreach (RoleMap roleMap in templateMap.roleMaps)
            {
              XElement roleObject = new XElement(DTO_NS + "roleObject");
              roleObjects.Add(roleObject);

              roleObject.Add(new XElement(DTO_NS + "type", roleMap.type));
              roleObject.Add(new XElement(DTO_NS + "roleId", roleMap.roleId));
              roleObject.Add(new XElement(DTO_NS + "name", roleMap.name));

              switch (roleMap.type)
              {
                case RoleType.Property:
                  string propertyName = roleMap.propertyName.Substring(_graphMap.dataObjectMap.Length + 1);
                  string value = Convert.ToString(_dataObjects[i].GetPropertyValue(propertyName));

                  if (!String.IsNullOrEmpty(roleMap.valueList))
                  {
                    value = _mapping.ResolveValueList(roleMap.valueList, value);
                    value = value.Replace(RDL_NS.NamespaceName, "rdl:");
                  }

                  roleObject.Add(new XElement(DTO_NS + "value", value));
                  break;

                case RoleType.FixedValue:
                  roleObject.Add(new XElement(DTO_NS + "value", roleMap.value));
                  break;

                case RoleType.Reference:
                  if (roleMap.classMap != null)
                  {
                    roleObject.Add(new XElement(DTO_NS + "value", "#" + _classIdentifiers[roleMap.classMap.classId][i]));
                  }
                  else
                  {
                    roleObject.Add(new XElement(DTO_NS + "value", roleMap.value));
                  }
                  break;
              }
            }
          }
        }
      }

      return dtoList;
    }

    public IList<IDataObject> GetDataObjects(string graphName, ref XElement xml)
    {
      _graphMap = _mapping.FindGraphMap(graphName);
      _dataTransferObjects = SerializationExtensions.ToObject<DataTransferObjects>(xml);

      ClassMap classMap = _graphMap.classTemplateListMaps.First().Key;
      List<string> identifiers = new List<string>();
      for (int i = 0; i < _dataTransferObjects.Count; i++)
      {
        DataTransferObject dataTransferObject = _dataTransferObjects[i];
        ClassObject classObject = dataTransferObject.GetClassObject(classMap.classId);

        if (classObject != null)
        {
          identifiers.Add(classObject.identifier);
        }
      }

      IList<IDataObject> dataObjects = _dataLayer.Create(_graphMap.dataObjectMap, identifiers);
      for (int dataTransferObjectIndex = 0; dataTransferObjectIndex < _dataTransferObjects.Count; dataTransferObjectIndex++)
      {
        IDataObject dataObject = dataObjects[dataTransferObjectIndex];
        PopulateDataObjects(ref dataObject, classMap.classId, dataTransferObjectIndex);
      }

      return dataObjects;
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
                  dataObject.SetPropertyValue(propertyName, roleObject.value);
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
