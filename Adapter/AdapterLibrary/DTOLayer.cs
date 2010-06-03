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
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Ninject;
using org.iringtools.adapter.datalayer;
using org.iringtools.library;
using org.iringtools.utility;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage;

namespace org.iringtools.adapter
{
  public class DTOLayer // : IDTOLayer
  {
    private static readonly string DATALAYER_NS = "org.iringtools.adapter.datalayer";

    private static readonly XNamespace RDF_NS = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    private static readonly XNamespace OWL_NS = "http://www.w3.org/2002/07/owl#";
    private static readonly XNamespace XSD_NS = "http://www.w3.org/2001/XMLSchema#";
    private static readonly XNamespace XSI_NS = "http://www.w3.org/2001/XMLSchema-instance#";
    private static readonly XNamespace TPL_NS = "http://tpl.rdlfacade.org/data#";
    private static readonly XNamespace RDL_NS = "http://rdl.rdlfacade.org/data#";

    private static readonly XName OWL_THING = OWL_NS + "Thing";
    private static readonly XName RDF_ABOUT = RDF_NS + "about";
    private static readonly XName RDF_TYPE = RDF_NS + "type";
    private static readonly XName RDF_RESOURCE = RDF_NS + "resource";
    private static readonly XName RDF_DATATYPE = RDF_NS + "datatype";

    private static readonly string XSD_PREFIX = "xsd:";
    private static readonly string RDF_PREFIX = "rdf:";
    private static readonly string RDL_PREFIX = "rdl:";
    private static readonly string TPL_PREFIX = "tpl:";
    
    private static readonly string RDF_NIL = RDF_PREFIX + "nil";

    private static readonly string CLASS_INSTANCE_QUERY_TEMPLATE = String.Format(@"
      PREFIX rdf: <{0}>
      PREFIX rdl: <{1}> 
      SELECT ?_instance
      WHERE {{{{ 
        ?_instance rdf:type {{0}} . 
      }}}}", RDF_NS.NamespaceName, RDL_NS.NamespaceName);

    private static readonly string LITERAL_QUERY_TEMPLATE = String.Format(@"
      PREFIX rdf: <{0}>
      PREFIX rdl: <{1}> 
      PREFIX tpl: <{2}> 
      SELECT ?_values 
      WHERE {{{{
	      ?_instance rdf:type {{0}} . 
	      ?_bnode {{1}} ?_instance . 
	      ?_bnode rdf:type {{2}} . 
	      ?_bnode {{3}} ?_values 
      }}}}", RDF_NS.NamespaceName, RDL_NS.NamespaceName, TPL_NS.NamespaceName);

    private NHibernateDataLayer _dataLayer = null;
    private Mapping _mapping = null;
    private GraphMap _graphMap = null;
    private Dictionary<string, IList<IDataObject>> _dataObjectSet = null; // dictionary of object names and list of data objects
    private Dictionary<string, List<string>> _classIdentifiers = null; // dictionary of class ids and list of identifiers
    private List<Dictionary<string, string>> _dtoList = null;  // dictionary of property xpath and value pairs
    private MicrosoftSqlStoreManager _tripleStore = null;
    private TripleStore _memoryStore = null;    
    private XNamespace _graphNs = String.Empty;
    private string _dataLayerAssemblyName = String.Empty;
    private string _dataObjectNs = String.Empty;

    [Inject]
    public DTOLayer(AdapterSettings adapterSettings, ApplicationSettings appSettings)
    {
      string scope = appSettings.ProjectName + "." + appSettings.ApplicationName;

      _dataLayer = new NHibernateDataLayer(adapterSettings, appSettings);
      _mapping = Utility.Read<Mapping>(adapterSettings.XmlPath + "Mapping." + scope + ".xml");
      _graphNs = adapterSettings.GraphBaseUri + appSettings.ProjectName + "/" + appSettings.ApplicationName + "#";
      _dataObjectNs = DATALAYER_NS + ".proj_" + scope;
      //todo: get dataObjectAssembly from datalayer binding configuration
      _dataLayerAssemblyName = "MappingTest";
      _tripleStore = new MicrosoftSqlStoreManager(adapterSettings.DBServer, adapterSettings.DBname, adapterSettings.DBUser, adapterSettings.DBPassword);
    }

    #region public methods
    public List<Dictionary<string, string>> GetDTOList(string graphName)
    {
      try
      {
        FindGraphMap(graphName);
        LoadDataObjects();

        _dtoList = new List<Dictionary<string, string>>();
        int maxDataObjectsCount = MaxDataObjectsCount();

        for (int i = 0; i < maxDataObjectsCount; i++)
        {
          _dtoList.Add(new Dictionary<string, string>());
        }

        ClassMap classMap = _graphMap.classTemplateListMaps.First().Key;
        FillDTOList(classMap.classId, "rdl:" + classMap.name);

        return _dtoList;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public XElement GetHierarchicalDTOList(string graphName)
    {
      try
      {
        FindGraphMap(graphName);
        LoadDataObjects();

        XElement graphElement = new XElement(_graphNs + graphName, new XAttribute(XNamespace.Xmlns + "i", XSI_NS));
        ClassMap classMap = _graphMap.classTemplateListMaps.First().Key;

        int maxDataObjectsCount = MaxDataObjectsCount();
        for (int i = 0; i < maxDataObjectsCount; i++)
        {
          XElement classElement = new XElement(_graphNs + TitleCase(classMap.name));
          graphElement.Add(classElement);
          FillHierarchicalDTOList(classElement, classMap.classId, i);
        }

        return graphElement;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public XElement GetGraphRdf(string graphName)
    {
      try
      {
        FindGraphMap(graphName);
        LoadDataObjects();
        return GetGraphRdf();
      }
      catch (Exception ex)
      {
        throw ex;
      }      
    }

    public Response DeleteGraph(string graphName)
    {
      Response response = new Response();

      try
      {
        FindGraphMap(graphName);
        DeleteGraph(new Uri(_graphMap.baseUri));

        response.Level = StatusLevel.Success;
        response.Add("Graph [" + graphName + "] has been deleted successfully.");
      }
      catch (Exception ex)
      {
        response.Level = StatusLevel.Error;
        response.Add("Error deleting graph [" + graphName + "]. " + ex);
      }

      return response;
    }

    public Response Refresh(string graphName)
    {
      Response response = new Response();
      
      try
      {
        DateTime startTime = DateTime.Now;

        // create RDF from graphName
        XElement rdf = GetGraphRdf(graphName);

        #region load RDF to graph then save it to triple store
        Uri graphUri = new Uri(_graphMap.baseUri);
        XmlDocument xdoc = new XmlDocument();        
        xdoc.LoadXml(rdf.ToString());
        rdf.RemoveAll();

        RdfXmlParser parser = new RdfXmlParser();
        Graph graph = new Graph();
        graph.BaseUri = graphUri;
        parser.Load(graph, xdoc);
        xdoc.RemoveAll();

        DeleteGraph(graphUri);
        _tripleStore.SaveGraph(graph);
        #endregion

        #region report status
        DateTime endTime = DateTime.Now;
        TimeSpan duration = endTime.Subtract(startTime);

        response.Level = StatusLevel.Success;
        response.Add("Graph [" + graphName + "] has been refreshed in triple store successfully.");

        response.Add(String.Format("Execution time [{0}:{1}.{2}] minutes.",
          duration.Minutes, duration.Seconds, duration.Milliseconds));
        #endregion
      }
      catch (Exception ex)
      {
        response.Level = StatusLevel.Error;
        response.Add("Error refreshing graph [" + graphName + "]. " + ex);
      }

      return response;
    }

    public Response Pull(string graphName)
    {
      Response response = new Response();

      try
      {
        DateTime startTime = DateTime.Now;
        FindGraphMap(graphName);

        #region load graph from triple store
        Graph graph = new Graph();
        graph.BaseUri = new Uri(_graphMap.baseUri);
        _tripleStore.LoadGraph(graph, _graphMap.baseUri);
        #endregion

        #region create in-memory store
        _memoryStore = new TripleStore();
        _memoryStore.Add(graph);
        graph.Dispose();
        #endregion

        #region query in-memory store and build data objects from query results
        int classInstanceCount = ClassInstanceCount();
        _dataObjectSet = new Dictionary<string, IList<IDataObject>>();

        foreach (var pair in _graphMap.classTemplateListMaps)
        {
          ClassMap classMap = pair.Key;
          List<TemplateMap> templateMaps = pair.Value;
          int dupTemplatePos = 0;

          foreach (TemplateMap templateMap in templateMaps)
          {
            List<RoleMap> propertyMapRoles = new List<RoleMap>();
            string classRoleId = String.Empty;

            #region find propertyMapRoles and classRoleId
            foreach (RoleMap roleMap in templateMap.roleMaps)
            {
              if (roleMap.type == RoleType.ClassRole)
              {
                classRoleId = roleMap.roleId;
              }
              else if (roleMap.type == RoleType.Property)
              {
                propertyMapRoles.Add(roleMap);
              }
            }
            #endregion

            #region query for property values and save them into dataObjects
            foreach (RoleMap roleMap in propertyMapRoles)
            {
              string query = String.Format(LITERAL_QUERY_TEMPLATE, classMap.classId, classRoleId, templateMap.templateId, roleMap.roleId);
              object results = _memoryStore.ExecuteQuery(query);

              if (results is SparqlResultSet)
              {
                string[] property = roleMap.propertyName.Split('.');
                string objectName = property[0].Trim();
                string propertyName = property[1].Trim();

                if (!_dataObjectSet.ContainsKey(objectName))
                {
                  _dataObjectSet.Add(objectName, new List<IDataObject>());
                }

                IList<IDataObject> dataObjects = _dataObjectSet[objectName];
                if (dataObjects.Count == 0)
                {
                  string objectType = _dataObjectNs + "." + objectName + "," + _dataLayerAssemblyName;
                  dataObjects = _dataLayer.Create(objectType, new string[classInstanceCount]);
                }

                SparqlResultSet resultSet = (SparqlResultSet)results;
                if (resultSet.Count > classInstanceCount)
                {
                  dupTemplatePos++;
                }

                int objectIndex = 0;
                int resultSetIndex = (dupTemplatePos == 0) ? 0 : dupTemplatePos - 1;

                while (resultSetIndex < resultSet.Count)
                {
                  string value = Regex.Replace(resultSet[resultSetIndex].ToString(), @".*= ", String.Empty);

                  if (value == RDF_NIL)
                  {
                    value = String.Empty;
                  }
                  else if (value.Contains("^^"))
                  {
                    value = value.Substring(0, value.IndexOf("^^"));
                  }
                  else if (!String.IsNullOrEmpty(roleMap.valueList))
                  {
                    value = ResolveValueMap(roleMap.valueList, value);
                  }

                  dataObjects[objectIndex++].SetPropertyValue(propertyName, value);

                  if (dupTemplatePos == 0)
                    resultSetIndex++;
                  else if (dupTemplatePos < 3)
                    resultSetIndex += 2;
                  else
                    resultSetIndex += dupTemplatePos;
                }

                _dataObjectSet[objectName] = dataObjects;
              }
              else
              {
                throw new Exception("Error querying in-memory triple store.");
              }
            }
            #endregion
          }
          #endregion
        }

        // post data objects to data layer
        foreach (var pair in _dataObjectSet)
        {
          response.Append(_dataLayer.Post(pair.Value));
        }

        #region report status
        DateTime endTime = DateTime.Now;
        TimeSpan duration = endTime.Subtract(startTime);
        
        response.Level = StatusLevel.Success;
        response.Add("Graph [" + graphName + "] has been posted to legacy system successfully.");

        response.Add(String.Format("Execution time [{0}:{1}.{2}] minutes.",
          duration.Minutes, duration.Seconds, duration.Milliseconds));
        #endregion
      }
      catch (Exception ex)
      {
        response.Level = StatusLevel.Error;
        response.Add("Error pulling graph [" + graphName + "]. " + ex);
      }

      return response;
    }
    #endregion

    #region utility methods
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

    public void SaveRdf(XElement rdf, string fileName)
    {
      XDocument doc = new XDocument(rdf);
      doc.Save(fileName);
    }
    #endregion

    #region private methods
    private void FindGraphMap(string graphName)
    {
      foreach (GraphMap graphMap in _mapping.graphMaps)
      {
        if (graphMap.name.ToLower() == graphName.ToLower())
        {
          _graphMap = graphMap;

          if (_graphMap.classTemplateListMaps.Count == 0)
            throw new Exception("Graph [" + graphName + "] is empty.");

          return;
        }
      }

      throw new Exception("Graph [" + graphName + "] does not exist.");
    }

    private void LoadDataObjects()
    {
      _dataObjectSet = new Dictionary<string, IList<IDataObject>>();

      foreach (DataObjectMap dataObjectMap in _graphMap.dataObjectMaps)
      {
        _dataObjectSet.Add(dataObjectMap.name, _dataLayer.Get(dataObjectMap.name, null));
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
            return valueMap.uri;
          }
        }
      }

      return RDF_NIL;
    }

    private string ResolveValueMap(string valueList, string uri)
    {
      if (_mapping != null && _mapping.valueMaps.Count > 0)
      {
        foreach (ValueMap valueMap in _mapping.valueMaps)
        {
          if (valueMap.valueList == valueList && valueMap.uri == uri)
          {
            return valueMap.internalValue;
          }
        }
      }

      return String.Empty;
    }

    private void PopulateClassIdentifiers()
    {
      _classIdentifiers = new Dictionary<string, List<string>>();

      foreach (ClassMap classMap in _graphMap.classTemplateListMaps.Keys)
      {
        List<string> classIdentifiers = new List<string>();

        foreach (string identifier in classMap.identifiers)
        {
          string[] property = identifier.Split('.');
          string objectName = property[0].Trim();
          string propertyName = property[1].Trim();

          IList<IDataObject> dataObjects = _dataObjectSet[objectName];
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

    // get max # of data records from all data objects
    private int MaxDataObjectsCount()
    {
      int maxCount = 0;

      foreach (var pair in _dataObjectSet)
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
        new XAttribute(XNamespace.Xmlns + "owl", OWL_NS),
        new XAttribute(XNamespace.Xmlns + "xsd", XSD_NS),
        new XAttribute(XNamespace.Xmlns + "tpl", TPL_NS));

      foreach (var pair in _graphMap.classTemplateListMaps)
      {
        ClassMap classMap = pair.Key;
        int maxDataObjectsCount = MaxDataObjectsCount();

        for (int i = 0; i < maxDataObjectsCount; i++)
        {
          string classId = classMap.classId.Substring(classMap.classId.IndexOf(":") + 1);
          string classInstance = _graphNs.NamespaceName + _classIdentifiers[classMap.classId][i];

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
      return new XElement(OWL_THING, new XAttribute(RDF_ABOUT, classInstance),
      new XElement(RDF_TYPE, new XAttribute(RDF_RESOURCE, RDL_NS.NamespaceName + classId))
      );
    }

    private XElement CreateRdfTemplateElement(TemplateMap templateMap, string classInstance, int dataObjectIndex)
    {
      string templateId = templateMap.templateId.Replace(TPL_PREFIX, TPL_NS.NamespaceName);

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
                string[] property = identifier.Split('.');
                string objectName = property[0].Trim();
                string propertyName = property[1].Trim();

                IDataObject dataObject = _dataObjectSet[objectName].ElementAt(dataObjectIndex);

                if (dataObject != null)
                {
                  string value = Convert.ToString(dataObject.GetPropertyValue(propertyName));

                  if (identifierValue != String.Empty)
                    identifierValue += roleMap.classMap.identifierDelimeter;

                  identifierValue += value;
                }
              }

              roleElement.Add(new XAttribute(RDF_RESOURCE, _graphNs.NamespaceName + identifierValue));
            }
            else
            {
              roleElement.Add(new XAttribute(RDF_RESOURCE, roleMap.value.Replace(RDL_PREFIX, RDL_NS.NamespaceName)));
            }
            break;
          }
          case RoleType.FixedValue:
          {
            dataType = roleMap.dataType.Replace(XSD_PREFIX, XSD_NS.NamespaceName);
            roleElement.Add(new XAttribute(RDF_DATATYPE, dataType));
            roleElement.Add(new XText(roleMap.value));
            break;
          }
          case RoleType.Property:
          {
            string[] property = roleMap.propertyName.Split('.');
            string objectName = property[0].Trim();
            string propertyName = property[1].Trim();

            IDataObject dataObject = _dataObjectSet[objectName].ElementAt(dataObjectIndex);
            string value = Convert.ToString(dataObject.GetPropertyValue(propertyName));

            if (String.IsNullOrEmpty(roleMap.valueList))
            {
              if (String.IsNullOrEmpty(value))
              {
                roleElement.Add(new XAttribute(RDF_RESOURCE, RDF_NIL));
              }
              else
              {
                dataType = roleMap.dataType.Replace(XSD_PREFIX, XSD_NS.NamespaceName);
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
      KeyValuePair<ClassMap, List<TemplateMap>> classTemplateListMap = _graphMap.GetClassTemplateListMap(classId);
      string classPath = xPath;

      foreach (TemplateMap templateMap in classTemplateListMap.Value)
      {
        xPath = classPath + "/tpl:" + templateMap.name;
        string templatePath = xPath;

        foreach (RoleMap roleMap in templateMap.roleMaps)
        {
          if (roleMap.type == RoleType.Property)
          {
            xPath += "/tpl:" + roleMap.name;

            string[] property = roleMap.propertyName.Split('.');
            string objectName = property[0].Trim();
            string propertyName = property[1].Trim();
            string value = String.Empty;

            IList<IDataObject> dataObjects = _dataObjectSet[objectName];
            for (int i = 0; i < dataObjects.Count; i++)
            {              
              value = Convert.ToString(dataObjects[i].GetPropertyValue(propertyName));

              if (!String.IsNullOrEmpty(roleMap.valueList))
              {
                value = ResolveValueList(roleMap.valueList, value);
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

    //todo: use reference if element already created somewhere in the doc
    private void FillHierarchicalDTOList(XElement classElement, string classId, int dataObjectIndex)
    {
      KeyValuePair<ClassMap, List<TemplateMap>> classTemplateListMap = _graphMap.GetClassTemplateListMap(classId);
      ClassMap classMap = classTemplateListMap.Key;
      List<TemplateMap> templateMaps = classTemplateListMap.Value;

      classElement.Add(new XAttribute("rdluri", classMap.classId));
      classElement.Add(new XAttribute("id", _classIdentifiers[classMap.classId][dataObjectIndex]));

      foreach (TemplateMap templateMap in templateMaps)
      {
        XElement templateElement = new XElement(_graphNs + templateMap.name);
        templateElement.Add(new XAttribute("rdluri", templateMap.templateId));
        classElement.Add(templateElement);

        foreach (RoleMap roleMap in templateMap.roleMaps)
        {
          XElement roleElement = new XElement(_graphNs + roleMap.name);

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
                XElement element = new XElement(_graphNs + TitleCase(roleMap.classMap.name));
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
              string[] property = roleMap.propertyName.Split('.');
              string objectName = property[0].Trim();
              string propertyName = property[1].Trim();
              IDataObject dataObject = _dataObjectSet[objectName][dataObjectIndex];
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

    private void DeleteGraph(Uri graphUri)
    {
      string graphId = _tripleStore.GetGraphID(graphUri);

      if (!String.IsNullOrEmpty(graphId))
      {
        _tripleStore.RemoveGraph(graphId);
      }
    }

    private int ClassInstanceCount()
    {
      ClassMap classMap = _graphMap.classTemplateListMaps.First().Key;
      string query = String.Format(CLASS_INSTANCE_QUERY_TEMPLATE, classMap.classId);
      object results = _memoryStore.ExecuteQuery(query);

      if (results is SparqlResultSet)
      {                
        SparqlResultSet resultSet = (SparqlResultSet)results;
        return resultSet.Count;
      }

      throw new Exception("Error querying instances of class [" + classMap.name + "].");
    }
    #endregion
  }
}
