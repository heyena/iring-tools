using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.library;
using org.iringtools.common.mapping;
using System.Xml.Linq;
using Ninject;
using log4net;
using System.Text.RegularExpressions;
using VDS.RDF;
using VDS.RDF.Storage;
using org.iringtools.utility;
using Microsoft.ServiceModel.Web;
using System.Xml.Serialization;
using org.iringtools.protocol.manifest;

namespace org.iringtools.adapter.projection
{
  public class DtoProjectionEngine : IProjectionLayer
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(DtoProjectionEngine));
    private static readonly XNamespace RDL_NS = "http://rdl.rdlfacade.org/data#";
    
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

    public XElement ToXml(string graphName, ref IList<IDataObject> dataObjects)
    {
      XElement xml = null;

      try
      {
        _graphMap = _mapping.FindGraphMap(graphName);
        _dataObjects = dataObjects;

        if (_graphMap != null && _graphMap.ClassTemplateMaps.Count > 0 &&
          _dataObjects != null && _dataObjects.Count > 0)
        {
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

          xml = SerializationExtensions.ToXml<DataTransferObjects>(dtoList);
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }

      return xml;
    }
    
    public IList<IDataObject> ToDataObjects(string graphName, ref XElement xml)
    {
      IList<IDataObject> dataObjects = null;

      try
      {
        _graphMap = _mapping.FindGraphMap(graphName);
        _dataTransferObjects = SerializationExtensions.ToObject<DataTransferObjects>(xml);

        if (_graphMap != null && _graphMap.ClassTemplateMaps.Count > 0 &&
          _dataTransferObjects != null && _dataTransferObjects.Count > 0)
        {
          ClassMap classMap = _graphMap.ClassTemplateMaps.First().ClassMap;
          List<string> identifiers = new List<string>();
          for (int i = 0; i < _dataTransferObjects.Count; i++)
          {
            DataTransferObject dataTransferObject = _dataTransferObjects[i];
            ClassObject classObject = dataTransferObject.GetClassObject(classMap.ClassId);

            if (classObject != null)
            {
              identifiers.Add(classObject.identifier);
            }
          }

          dataObjects = _dataLayer.Create(_graphMap.DataObjectName, identifiers);
          for (int dataTransferObjectIndex = 0; dataTransferObjectIndex < _dataTransferObjects.Count; dataTransferObjectIndex++)
          {
            IDataObject dataObject = dataObjects[dataTransferObjectIndex];
            PopulateDataObjects(ref dataObject, classMap.ClassId, dataTransferObjectIndex);
          }

          return dataObjects;
        }
      }
      catch (Exception ex)
      {
        throw ex;
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
      ClassTemplateMap classTemplateListMap = _graphMap.GetClassTemplateMap(classId);
      List<TemplateMap> templateMaps = classTemplateListMap.TemplateMaps;
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
