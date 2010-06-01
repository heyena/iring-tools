// Copyright (c) 2009, ids-adi.org /////////////////////////////////////////////
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
using System.Xml;
using VDS.RDF.Parsing;
using VDS.RDF;
using VDS.RDF.Storage;
using VDS.RDF.Query;
using System.Text;
using Ninject;
using org.iringtools.utility;

namespace org.iringtools.adapter
{
  public class DTOLayer // : IDTOLayer
  {
    private static readonly string DATALAYER_NS = "org.iringtools.adapter.datalayer";

    //private static XNamespace RDFS_NS = "http://www.w3.org/2000/01/rdf-schema#";
    private static readonly XNamespace RDF_NS = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    private static readonly XNamespace OWL_NS = "http://www.w3.org/2002/07/owl#";
    private static readonly XNamespace XSD_NS = "http://www.w3.org/2001/XMLSchema#";
    private static readonly XNamespace TPL_NS = "http://tpl.rdlfacade.org/data#";
    private static readonly XNamespace RDL_NS = "http://rdl.rdlfacade.org/data#";

    private static readonly XName OWL_THING = OWL_NS + "Thing";
    private static readonly XName RDF_ABOUT = RDF_NS + "about";
    private static readonly XName RDF_TYPE = RDF_NS + "type";
    private static readonly XName RDF_RESOURCE = RDF_NS + "resource";
    private static readonly XName RDF_DATATYPE = RDF_NS + "datatype";

    private static readonly string RDF_NIL = RDF_NS.NamespaceName + "nil";
    private static readonly string XSD_PREFIX = "xsd:";
    private static readonly string RDL_PREFIX = "rdl:";
    private static readonly string TPL_PREFIX = "tpl:";

    //private static readonly string IDENTIFIER_QUERY = @"
    //  PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
	  //  SELECT ?x 
	  //  WHERE {{ ?x rdf:type {0} . }}";

    private static readonly string LITERAL_QUERY = @"
	    PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
	    PREFIX rdl: <http://rdl.rdlfacade.org/data#> 
	    PREFIX tpl: <http://tpl.rdlfacade.org/data#> 
	    SELECT ?z 
	    WHERE {{
		    ?x rdf:type {0} .
		    ?y {1} ?x .
		    ?y rdf:type {2} . 
		    ?y {3} ?z .
	      }}";
    // where
    //  {0}: class id
    //  {1}: class-role id
    //  {2}: template id
    //  {3}: property-role id    

    private NHibernateDataLayer _dataLayer = null;
    private Dictionary<string, IList<IDataObject>> _dataObjects = null; // dictionary of object names and list of data records
    private Dictionary<string, List<string>> _classIdentifiers = null; // dictionary of class ids and list of identifiers
    private Mapping _mapping = null;
    private GraphMap _graphMap = null;
    private List<Dictionary<string, string>> _dtoList = null;  // dictionary of property and value pairs

    private string _domainUri = String.Empty;
    private string _dataObjectNamespace = String.Empty;
    private string _dataObjectAssemblyName = String.Empty;

    // todo: load triple store 
    [Inject]
    public DTOLayer(AdapterSettings adapterSettings, ApplicationSettings appSettings)
    {
      string scope = appSettings.ProjectName + "." + appSettings.ApplicationName;

      _dataLayer = new NHibernateDataLayer(adapterSettings, appSettings);
      _mapping = Utility.Read<Mapping>(adapterSettings.XmlPath + "Mapping." + scope + ".xml");
      //todo: create DomainUri (or replace GraphBaseUri) in adapterSettings (and always end it with /")
      _domainUri = adapterSettings.GraphBaseUri + appSettings.ProjectName + "/" + appSettings.ApplicationName;
       _dataObjectNamespace = DATALAYER_NS + ".proj_" + scope;
      //todo: get dataObjectAssembly from binding configuration
       _dataObjectAssemblyName = "MappingTest";
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

        //todo: include namespace (in format of http://localhost/12345_000/ABC) in graph
        XElement graphElement = new XElement(graphName);
        ClassMap classMap = _graphMap.classTemplateListMaps.First().Key;

        int maxDataObjectsCount = MaxDataObjectsCount();
        for (int i = 0; i < maxDataObjectsCount; i++)
        {
          XElement classElement = new XElement(TitleCase(classMap.name));
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

    public Response PostRdf(XElement rdf)
    {
      Response response = new Response();
      
      try
      {
        Uri graphUri = new Uri(_graphMap.baseUri);
        MicrosoftSqlStoreManager _msStore = new MicrosoftSqlStoreManager(@".\SQLEXPRESS", "dotNetRdf", "dotNetRdf", "dotNetRdf");

        // remove old graph in triple store if exists
        string graphId = _msStore.GetGraphID(graphUri);
        if (!String.IsNullOrEmpty(graphId))
        {
          _msStore.ClearGraph(graphId);
        }

        // load rdf to graph and save graph to triple store
        XmlDocument xdoc = new XmlDocument();
        RdfXmlParser parser = new RdfXmlParser();
        Graph graph = new Graph();
        graph.BaseUri = graphUri;
        xdoc.LoadXml(rdf.ToString());
        parser.Load(graph, xdoc);
        _msStore.SaveGraph(graph);

        // create a response and return it
        response.Level = StatusLevel.Success;
        response.Add("Graph [" + graph.BaseUri.ToString() + "] has been posted successfully");
      }
      catch (Exception ex)
      {
        response.Level = StatusLevel.Error;
        response.Add("Error posting RDF: " + ex);
      }

      return response;
    }

    public Response Pull(string graphName)
    {
      Response response = new Response();
      MicrosoftSqlStoreManager _msStore = new MicrosoftSqlStoreManager(@".\SQLEXPRESS", "dotNetRdf", "dotNetRdf", "dotNetRdf");

      try
      {
        FindGraphMap(graphName);

        #region load graph from triple store
        Graph graph = new Graph();
        graph.BaseUri = new Uri(_graphMap.baseUri);
        _msStore.LoadGraph(graph, _graphMap.baseUri);
        #endregion

        #region create in-memory store
        TripleStore store = new TripleStore();
        store.Add(graph);
        graph.Dispose();
        #endregion

        #region query in-memory store and build data objects from query results
        _dataObjects = new Dictionary<string, IList<IDataObject>>(); 
        
        foreach (var pair in _graphMap.classTemplateListMaps)
        {
          ClassMap classMap = pair.Key;
          List<TemplateMap> templateMaps = pair.Value;
          string query = String.Empty;
          object results = null;
          
          foreach (TemplateMap templateMap in templateMaps)
          {
            string classRoleId = String.Empty;
            RoleMap propertyRoleMap = null;

            foreach (RoleMap roleMap in templateMap.roleMaps)
            {
              if (roleMap.type == RoleType.ClassRole)
              {
                classRoleId = roleMap.roleId;
              }
              else if (roleMap.type == RoleType.Property)
              {                
                propertyRoleMap = roleMap;
              }
            }
  
            query = String.Format(LITERAL_QUERY, classMap.classId, classRoleId, templateMap.templateId, propertyRoleMap.roleId);
            results = store.ExecuteQuery(query);

            if (results is SparqlResultSet)
            {
              string[] propertyMap = propertyRoleMap.propertyName.Split('.');
              string objectName = propertyMap[0].Trim();
              string propertyName = propertyMap[1].Trim();

              if (!_dataObjects.ContainsKey(objectName))
              {
                _dataObjects.Add(objectName, new List<IDataObject>());
              }

              IList<IDataObject> dataObjects = _dataObjects[objectName];
              SparqlResultSet resultSet = (SparqlResultSet)results;
              
              if (dataObjects.Count == 0)
              {
                string objectType = _dataObjectNamespace + "." + objectName + "," + _dataObjectAssemblyName;
                dataObjects = _dataLayer.Create(objectType, new string[resultSet.Count]);
              }
              
              for (int i = 0; i < resultSet.Count; i++)
              { 
                //todo: need to resolve valueList
                string literal = resultSet[i].ToString();
                int valueStartPos = literal.LastIndexOf("= ");
                string value = literal.Substring(valueStartPos + 1, literal.IndexOf("^^") - valueStartPos - 1);

                dataObjects[i].SetPropertyValue(propertyName, value);
              }
            }
            else
            {
              throw new Exception("Error querying in-memory triple store.");
            }
          }
        }
        #endregion

        #region post data objects to legacy database
        foreach (var pair in _dataObjects)
        {
          response.Append(_dataLayer.Post(pair.Value));
        }
        #endregion

        response.Level = StatusLevel.Success;
        response.Add("Pull graph [" + graphName + "] successfully.");
      }
      catch (Exception ex)
      {
        response.Level = StatusLevel.Error;
        response.Add("Error pulling graph [" + graphName + "]: " + ex);
      }

      return response;
    }
    #endregion

    #region utility methods
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
          return;
        }
      }

      throw new Exception("Graph [" + graphName + "] does not exist.");
    }

    private void LoadDataObjects()
    {
      _dataObjects = new Dictionary<string, IList<IDataObject>>();

      foreach (DataObjectMap dataObjectMap in _graphMap.dataObjectMaps)
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
            return valueMap.uri;
          }
        }
      }

      return RDF_NIL;
    }

    private void PopulateClassIdentifiers()
    {
      _classIdentifiers = new Dictionary<string, List<string>>();

      foreach (ClassMap classMap in _graphMap.classTemplateListMaps.Keys)
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
          string classInstance = _domainUri + "#" + _classIdentifiers[classMap.classId][i];

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

                roleElement.Add(new XAttribute(RDF_RESOURCE, _domainUri + "#" + identifierValue));
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
      KeyValuePair<ClassMap, List<TemplateMap>> classTemplateListMap = _graphMap.GetClassTemplateListMap(classId);
      ClassMap classMap = classTemplateListMap.Key;
      List<TemplateMap> templateMaps = classTemplateListMap.Value;

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


