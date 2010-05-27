﻿// Copyright (c) 2009, ids-adi.org /////////////////////////////////////////////
// All rights reserved.
//------------------------------------------------------------------------------
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the ids-adi.org nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
//------------------------------------------------------------------------------
// THIS SOFTWARE IS PROVIDED BY ids-adi.org ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL ids-adi.org BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Linq;
using org.iringtools.adapter;
using org.iringtools.adapter.datalayer;
using org.iringtools.library;
using System.Text.RegularExpressions;

namespace org.iringtools.adapter
{
  public class DTOLayer // : IDTOLayer
  {
    private static XNamespace RDF_NS = "http://www.w3.org/1999/02/22-rdf-syntax-ns";
    private static XNamespace RDFS_NS = "http://www.w3.org/2000/01/rdf-schema";
    private static XNamespace OWL_NS = "http://www.w3.org/2002/07/owl";
    private static XNamespace XSD_NS = "http://www.w3.org/2001/XMLSchema";
    private static XNamespace TPL_NS = "http://tpl.rdlfacade.org/data";

    private static XName OWL_THING = OWL_NS + "Thing";
    private static XName RDF_TYPE = RDF_NS + "type";
    private static XName RDF_RESOURCE = RDF_NS + "resource";
    private static XName RDF_DATATYPE = RDF_NS + "datatype";

    private static string RDL_NS = "http://rdl.rdlfacade.org/data#";
    private static string DOMAIN_NS = "http://www.example.com/data#";
    private static string RDF_NIL = RDF_NS.NamespaceName + "#nil";

    private static string XSD_PREFIX = "xsd:";
    private static string RDL_PREFIX = "rdl:";
    private static string TPL_PREFIX = "tpl:";

    private NHibernateDataLayer _dataLayer = null;
    private Dictionary<string, IList<IDataObject>> _dataObjects = null;
    private Dictionary<string, List<string>> _classIdentifiers = null;
    private Mapping _mapping = null;
    private Graph _graph = null;
    private List<Dictionary<string, string>> _dtoList = null;

    public DTOLayer(Mapping mapping)
    {
      #region to be injected
      AdapterSettings settings = new AdapterSettings(
        new NameValueCollection {
          {"BaseDirectoryPath", @"C:\Development\dotNet\MappingTest\MappingTest\"},
          {"XmlPath", @"C:\Development\dotNet\MappingTest\MappingTest\"},
          {"ProxyCredentialToken", ""},
          {"ProxyHost", ""},
          {"ProxyPort", ""},
          {"UseSemweb", "false"},
          {"UsedotnetRDF", "true"},
          {"TripleStoreConnectionString", ""},
          {"InterfaceService", ""},
          {"InterfaceServicePath", ""},
          {"TargetCredentialToken", ""},
          {"InterfaceServicePath", ""},
          {"TrimData", "false"},
          {"BinaryPath", ""},
          {"CodePath", ""},
          {"DBServer", ""},
          {"DBName", ""},
          {"DBUser", ""},
          {"DBPassword", ""},
          {"EndpointTimeout", "50000"},
          {"InterfaceCredentialToken", ""},
          {"GraphBaseUri", ""}
        }
      );
      ApplicationSettings appSettings = new ApplicationSettings("12345_000", "ABC");
      #endregion

      _mapping = mapping;
      _dataLayer = new NHibernateDataLayer(settings, appSettings);
    }

    #region public methods
    public List<Dictionary<string, string>> GetDTOList(string graphName)
    {
      foreach (Graph graph in _mapping.graphs)
      {
        if (graph.name.ToLower() == graphName.ToLower())
        {
          _graph = graph;
          LoadDataObjects();

          _dtoList = new List<Dictionary<string, string>>();
          int maxDataObjectsCount = MaxDataObjectsCount();
          for (int i = 0; i < maxDataObjectsCount; i++)
          {
            _dtoList.Add(new Dictionary<string, string>());
          }
          ClassMap classMap = _graph.graphMaps.First().Key;
          FillDTOList(classMap.classId, "rdl:" + classMap.name);

          return _dtoList;
        }
      }

      throw new Exception("Graph " + graphName + " does not exist.");
    }

    public XElement GetHierarchicalDTOList(string graphName)
    {
      foreach (Graph graph in _mapping.graphs)
      {
        if (graph.name.ToLower() == graphName.ToLower())
        {
          _graph = graph;
          LoadDataObjects();

          //todo: add namespace (http://localhost/12345_000/ABC) for graph
          XElement graphElement = new XElement(graphName);
          ClassMap classMap = _graph.graphMaps.First().Key;

          int maxDataObjectsCount = MaxDataObjectsCount();
          for (int i = 0; i < maxDataObjectsCount; i++)
          {
            XElement classElement = new XElement(TitleCase(classMap.name));
            graphElement.Add(classElement);
            FillHierarchicalDTOList(classElement, classMap.classId, i);
          }

          return graphElement;
        }
      }

      throw new Exception("Graph " + graphName + " does not exist.");
    }

    public XElement GetGraphRdf(string graphName)
    {
      foreach (Graph graph in _mapping.graphs)
      {
        if (graph.name.ToLower() == graphName.ToLower())
        {
          _graph = graph;
          LoadDataObjects();
          return GetGraphRdf();
        }
      }

      throw new Exception("Graph " + graphName + " does not exist.");
    }
    #endregion

    #region utility methods
    private string TitleCase(string value)
    {
      string returnValue = String.Empty;
      string[] words = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

      foreach (string word in words)
      {
        string lowerCaseWord = word.ToLower();
        returnValue += lowerCaseWord[0].ToString().ToUpper();

        if (lowerCaseWord.Length > 1)
          returnValue += lowerCaseWord.Substring(1);
      }

      return returnValue;
    }

    public void SaveRdf(XElement rdf, string fileName)
    {
      XDocument doc = new XDocument(rdf);
      doc.Save(fileName);
    }
    #endregion

    #region private methods
    private void LoadDataObjects()
    {
      _dataObjects = new Dictionary<string, IList<IDataObject>>();

      foreach (DataObjectMap dataObjectMap in _graph.dataObjectMaps)
      {
        _dataObjects.Add(dataObjectMap.name, _dataLayer.Get(dataObjectMap.name, null));
      }

      PopulateClassIdentifiers();
    }

    private string ResolveValueList(string valueList, string value)
    {
      if (_mapping != null && _mapping.valueMaps.Count > 0)
      {
        foreach (ValueMap valueMap in _mapping.valueMaps)
        {
          if (valueMap.valueList == valueList && valueMap.internalValue == value)
          {
            return DOMAIN_NS + valueMap.uri;
          }
        }
      }

      return RDF_NIL;
    }

    private void PopulateClassIdentifiers()
    {
      _classIdentifiers = new Dictionary<string, List<string>>();

      foreach (ClassMap classMap in _graph.graphMaps.Keys)
      {
        List<string> classIdentifiers = new List<string>();

        foreach (string identifier in classMap.identifiers)
        {
          string[] propertyMap = identifier.Split('.');
          string objectName = propertyMap[0].Trim();
          string propertyName = propertyMap[1].Trim();

          IList<IDataObject> dataObjects = _dataObjects[objectName];
          if (dataObjects != null)
          {
            for (int i = 0; i < dataObjects.Count; i++)
            {
              string value = Convert.ToString(dataObjects[i].GetPropertyValue(propertyName));

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

        _classIdentifiers[classMap.classId] = classIdentifiers;
      }
    }

    // maximum number of data records from all data objects
    private int MaxDataObjectsCount()
    {
      int maxCount = 0;

      foreach (var pair in _dataObjects)
      {
        if (pair.Value.Count > maxCount)
        {
          maxCount = pair.Value.Count;
        }
      }

      return maxCount;
    }

    private XElement GetGraphRdf()
    {
      XElement graphElement = new XElement(RDF_NS + "RDF",
        new XAttribute(XNamespace.Xmlns + "rdf", RDF_NS),
        new XAttribute(XNamespace.Xmlns + "rdfs", RDFS_NS),
        new XAttribute(XNamespace.Xmlns + "owl", OWL_NS),
        new XAttribute(XNamespace.Xmlns + "xsd", XSD_NS),
        new XAttribute(XNamespace.Xmlns + "tpl", TPL_NS));

      foreach (var pair in _graph.graphMaps)
      {
        ClassMap classMap = pair.Key;

        int maxDataObjectsCount = MaxDataObjectsCount();
        for (int i = 0; i < maxDataObjectsCount; i++)
        {
          string classId = classMap.classId.Substring(classMap.classId.IndexOf(":") + 1);
          string classInstance = DOMAIN_NS + _classIdentifiers[classMap.classId][i];

          graphElement.Add(CreateRdfClassElement(classId, classInstance));

          foreach (TemplateMap templateMap in pair.Value)
          {
            graphElement.Add(CreateRdfTemplateElement(templateMap, classInstance, i));
          }
        }
      }

      return graphElement;
    }

    private XElement CreateRdfClassElement(string classId, string classInstance)
    {
      return new XElement(OWL_THING,
        new XElement(RDF_TYPE, new XAttribute(RDF_RESOURCE, TPL_NS.NamespaceName + "#R63638239485")),
        new XElement(TPL_NS + "R55055340393", new XAttribute(RDF_RESOURCE, RDL_NS + classId)),
        new XElement(TPL_NS + "R99011248051", new XAttribute(RDF_RESOURCE, classInstance))
      );
    }

    private XElement CreateRdfTemplateElement(TemplateMap templateMap, string classInstance, int dataObjectIndex)
    {
      string templateId = templateMap.templateId.Replace(TPL_PREFIX, TPL_NS.NamespaceName + "#");

      XElement templateElement = new XElement(OWL_THING);
      templateElement.Add(new XElement(RDF_TYPE, new XAttribute(RDF_RESOURCE, templateId)));

      foreach (RoleMap roleMap in templateMap.roleMaps)
      {
        string roleId = roleMap.roleId.Substring(roleMap.roleId.IndexOf(":") + 1);
        string dataType = String.Empty;
        XElement roleElement = new XElement(TPL_NS + roleId);

        switch (roleMap.type)
        {
          case RoleType.ClassRole:
            {
              roleElement.Add(new XAttribute(RDF_RESOURCE, classInstance));
              break;
            }
          case RoleType.Reference:
            {
              if (roleMap.classMap != null)
              {
                string identifierValue = String.Empty;

                foreach (string identifier in roleMap.classMap.identifiers)
                {
                  string[] propertyMap = identifier.Split('.');
                  string objectName = propertyMap[0].Trim();
                  string propertyName = propertyMap[1].Trim();

                  IDataObject dataObject = _dataObjects[objectName].ElementAt(dataObjectIndex);

                  if (dataObject != null)
                  {
                    string value = Convert.ToString(dataObject.GetPropertyValue(propertyName));

                    if (identifierValue != String.Empty)
                      identifierValue += roleMap.classMap.identifierDelimeter;

                    identifierValue += value;
                  }
                }

                roleElement.Add(new XAttribute(RDF_RESOURCE, DOMAIN_NS + identifierValue));
              }
              else
              {
                roleElement.Add(new XAttribute(RDF_RESOURCE, roleMap.value.Replace(RDL_PREFIX, RDL_NS)));
              }
              break;
            }
          case RoleType.FixedValue:
            {
              dataType = roleMap.dataType.Replace(XSD_PREFIX, XSD_NS.NamespaceName + "#");
              roleElement.Add(new XAttribute(RDF_DATATYPE, dataType));
              roleElement.Add(new XText(roleMap.value));
              break;
            }
          case RoleType.Property:
            {
              string[] propertyMap = roleMap.propertyName.Split('.');
              string objectName = propertyMap[0].Trim();
              string propertyName = propertyMap[1].Trim();

              IDataObject dataObject = _dataObjects[objectName].ElementAt(dataObjectIndex);
              string value = Convert.ToString(dataObject.GetPropertyValue(propertyName));

              if (String.IsNullOrEmpty(roleMap.valueList))
              {
                if (String.IsNullOrEmpty(value))
                {
                  roleElement.Add(new XAttribute(RDF_RESOURCE, RDF_NIL));
                }
                else
                {
                  dataType = roleMap.dataType.Replace(XSD_PREFIX, XSD_NS.NamespaceName + "#");
                  roleElement.Add(new XAttribute(RDF_DATATYPE, dataType));
                  roleElement.Add(new XText(value));
                }
              }
              else // resolve value list to uri
              {
                string valueListUri = ResolveValueList(roleMap.valueList, value);
                roleElement.Add(new XAttribute(RDF_RESOURCE, valueListUri));
              }

              break;
            }
        }

        templateElement.Add(roleElement);
      }

      return templateElement;
    }

    private void FillDTOList(string classId, string xPath)
    {
      KeyValuePair<ClassMap, List<TemplateMap>> graphMap = _graph.GetGraphMap(classId);
      string classPath = xPath;

      foreach (TemplateMap templateMap in graphMap.Value)
      {
        xPath = classPath + "/tpl:" + templateMap.name;
        string templatePath = xPath;

        foreach (RoleMap roleMap in templateMap.roleMaps)
        {
          if (roleMap.type == RoleType.Property || roleMap.type == RoleType.FixedValue)
          {
            xPath += "/tpl:" + roleMap.name;

            string[] propertyMap = roleMap.propertyName.Split('.');
            string objectName = propertyMap[0].Trim();
            string propertyName = propertyMap[1].Trim();
            string value = String.Empty;

            IList<IDataObject> dataObjects = _dataObjects[objectName];
            for (int i = 0; i < dataObjects.Count; i++)
            {
              if (roleMap.type == RoleType.Property)
              {
                value = Convert.ToString(dataObjects[i].GetPropertyValue(propertyName));

                if (!String.IsNullOrEmpty(roleMap.valueList))
                {
                  value = ResolveValueList(roleMap.valueList, value);
                }
              }
              else
              {
                value = roleMap.value;
              }

              Dictionary<string, string> propertyValuePair = _dtoList[i];
              propertyValuePair[xPath] = value;
            }

            xPath = templatePath;
          }

          if (roleMap.classMap != null)
          {
            FillDTOList(roleMap.classMap.classId, xPath + "/rdl:" + roleMap.classMap.name);
          }
        }
      }
    }

    //todo: use reference if element already created in the doc
    private void FillHierarchicalDTOList(XElement classElement, string classId, int dataObjectIndex)
    {
      KeyValuePair<ClassMap, List<TemplateMap>> graphMap = _graph.GetGraphMap(classId);
      ClassMap classMap = graphMap.Key;
      List<TemplateMap> templateMaps = graphMap.Value;

      classElement.Add(new XAttribute("rdluri", classMap.classId));
      classElement.Add(new XAttribute("id", _classIdentifiers[classMap.classId][dataObjectIndex]));

      foreach (TemplateMap templateMap in templateMaps)
      {
        XElement templateElement = new XElement(templateMap.name);
        templateElement.Add(new XAttribute("rdluri", templateMap.templateId));
        classElement.Add(templateElement);

        foreach (RoleMap roleMap in templateMap.roleMaps)
        {
          XElement roleElement = new XElement(roleMap.name);

          switch (roleMap.type)
          {
            case RoleType.ClassRole:
              templateElement.Add(new XAttribute("classRole", roleMap.roleId));
              break;

            case RoleType.Reference:
              roleElement.Add(new XAttribute("rdluri", roleMap.roleId));
              roleElement.Add(new XAttribute("reference", roleMap.value));
              templateElement.Add(roleElement);

              if (roleMap.classMap != null)
              {
                XElement element = new XElement(TitleCase(roleMap.classMap.name));
                roleElement.Add(element);
                FillHierarchicalDTOList(element, roleMap.classMap.classId, dataObjectIndex);
              }

              break;

            case RoleType.FixedValue:
              roleElement.Add(new XAttribute("rdluri", roleMap.roleId));
              roleElement.Add(new XText(roleMap.value));
              templateElement.Add(roleElement);
              break;

            case RoleType.Property:
              string[] propertyMap = roleMap.propertyName.Split('.');
              string objectName = propertyMap[0].Trim();
              string propertyName = propertyMap[1].Trim();
              IDataObject dataObject = _dataObjects[objectName][dataObjectIndex];
              roleElement.Add(new XAttribute("rdluri", roleMap.roleId));

              string value = Convert.ToString(dataObject.GetPropertyValue(propertyName));
              if (!String.IsNullOrEmpty(roleMap.valueList))
              {
                value = ResolveValueList(roleMap.valueList, value);
                roleElement.Add(new XAttribute("reference", value));
              }
              else
              {
                roleElement.Add(new XText(value));
              }

              templateElement.Add(roleElement);
              break;
          }
        }
      }
    }
    #endregion
  }
}


