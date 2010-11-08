using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.adapter;
using System.Xml.Linq;
using org.iringtools.library;
using VDS.RDF.Query;
using VDS.RDF;
using System.Text.RegularExpressions;
using org.iringtools.common.mapping;
using org.iringtools.protocol.manifest;

namespace org.iringtools.adapter.projection
{
  public abstract class BaseProjectionEngine : IProjectionLayer
  {
    protected static readonly XNamespace RDF_NS = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    protected static readonly XNamespace OWL_NS = "http://www.w3.org/2002/07/owl#";
    protected static readonly XNamespace XSD_NS = "http://www.w3.org/2001/XMLSchema#";
    protected static readonly XNamespace XSI_NS = "http://www.w3.org/2001/XMLSchema-instance#";
    protected static readonly XNamespace TPL_NS = "http://tpl.rdlfacade.org/data#";
    protected static readonly XNamespace RDL_NS = "http://rdl.rdlfacade.org/data#";

    protected static readonly XName OWL_THING = OWL_NS + "Thing";
    protected static readonly XName RDF_ABOUT = RDF_NS + "about";
    protected static readonly XName RDF_DESCRIPTION = RDF_NS + "Description";
    protected static readonly XName RDF_TYPE = RDF_NS + "type";
    protected static readonly XName RDF_RESOURCE = RDF_NS + "resource";
    protected static readonly XName RDF_DATATYPE = RDF_NS + "datatype";

    protected static readonly string XSD_PREFIX = "xsd:";
    protected static readonly string RDF_PREFIX = "rdf:";
    protected static readonly string RDL_PREFIX = "rdl:";
    protected static readonly string TPL_PREFIX = "tpl:";
    protected static readonly string RDF_NIL = RDF_PREFIX + "nil";

    protected static readonly string CLASS_INSTANCE_QUERY_TEMPLATE = String.Format(@"
      PREFIX rdf: <{0}>
      PREFIX rdl: <{1}> 
      SELECT ?class
      WHERE {{{{ 
        ?class rdf:type {{0}} . 
      }}}}", RDF_NS.NamespaceName, RDL_NS.NamespaceName);

    protected static readonly string SUBCLASS_INSTANCE_QUERY_TEMPLATE = String.Format(@"
      PREFIX rdf: <{0}>
      PREFIX rdl: <{1}> 
      PREFIX tpl: <{2}> 
      PREFIX g:   <{{0}}>
      SELECT ?class 
      WHERE {{{{
	      ?bnode {{1}} g:{{2}} . 
	      ?bnode rdf:type {{3}} . 
	      ?bnode {{4}} {{5}} . 
	      ?bnode {{6}} ?class 
      }}}}", RDF_NS.NamespaceName, RDL_NS.NamespaceName, TPL_NS.NamespaceName);

    protected static readonly string LITERAL_QUERY_TEMPLATE = String.Format(@"
      PREFIX rdf: <{0}>
      PREFIX rdl: <{1}> 
      PREFIX tpl: <{2}> 
      PREFIX i:   <{{0}}>
      SELECT ?literals 
      WHERE {{{{
	      ?bnode {{1}} i:{{2}} . 
	      ?bnode rdf:type {{3}} . 
	      ?bnode {{4}} {{5}} . 
	      ?bnode {{6}} ?literals 
      }}}}", RDF_NS.NamespaceName, RDL_NS.NamespaceName, TPL_NS.NamespaceName);

    protected AdapterSettings _settings = null;
    protected Mapping _mapping = null;
    protected GraphMap _graphMap = null;
    protected string _graphBaseUri = null;
    protected IDataLayer _dataLayer = null;
    protected IList<IDataObject> _dataObjects = null;
    protected Dictionary<string, List<string>> _classIdentifiers = null;
    protected List<string> _relatedObjectPaths = null;
    protected Dictionary<string, IList<IDataObject>>[] _relatedObjects = null;
    protected TripleStore _memoryStore = null;

    public BaseProjectionEngine()
    {
      _dataObjects = new List<IDataObject>();
      _classIdentifiers = new Dictionary<string, List<string>>();
    }

    public abstract XDocument ToXml(string graphName, ref IList<IDataObject> dataObjects);
    public abstract IList<IDataObject> ToDataObjects(string graphName, ref XDocument xDocument);

    //propertyPath = "Instrument.LineItems.Tag";
    protected List<IDataObject> GetRelatedObjects(string propertyPath, IDataObject dataObject)
    {
      List<IDataObject> parentObjects = new List<IDataObject>();
      string[] objectPath = propertyPath.Split('.');

      parentObjects.Add(dataObject);

      for (int i = 0; i < objectPath.Length - 1; i++)
      {
        foreach (IDataObject parentObj in parentObjects)
        {
          if (parentObj.GetType().Name != objectPath[i])
          {
            List<IDataObject> relatedObjects = new List<IDataObject>();

            foreach (IDataObject relatedObj in _dataLayer.GetRelatedObjects(parentObj, objectPath[i]))
            {
              if (!relatedObjects.Contains(relatedObj))
              {
                relatedObjects.Add(relatedObj);
              }
            }

            parentObjects = relatedObjects;
          }
        }
      }

      return parentObjects;
    }

    // senario (assume no circular relationships - should be handled by AppEditor): 
    //  dataObject1.L1RelatedDataObjects.L2RelatedDataObjects.LnRelatedDataObjects.property1
    //  dataObject1.L1RelatedDataObjects.L2RelatedDataObjects.LnRelatedDataObjects.property2
    protected void SetObjects(int dataObjectIndex, string propertyPath, List<string> relatedValues)
    {
      Dictionary<string, IList<IDataObject>> relatedObjectDictionary = _relatedObjects[dataObjectIndex];
      int lastDotPosition = propertyPath.LastIndexOf('.');
      string property = propertyPath.Substring(lastDotPosition + 1);
      string objectPathString = propertyPath.Substring(0, lastDotPosition);  // exclude property
      string[] objectPath = objectPathString.Split('.');

      if (!_relatedObjectPaths.Contains(objectPathString))
        _relatedObjectPaths.Add(objectPathString);

      // top level data objects are processed separately, so start with 1
      for (int i = 1; i < objectPath.Length; i++)
      {
        string relatedObjectType = objectPath[i];
        IList<IDataObject> relatedObjects = null;

        if (relatedObjectDictionary.ContainsKey(relatedObjectType))
        {
          relatedObjects = relatedObjectDictionary[relatedObjectType];
        }
        else
        {
          if (i == objectPath.Length - 1)  // last related object in the chain
          {
            relatedObjects = _dataLayer.Create(relatedObjectType, new string[relatedValues.Count]);
          }
          else // intermediate related object
          {
            relatedObjects = _dataLayer.Create(relatedObjectType, null);
          }

          relatedObjectDictionary.Add(relatedObjectType, relatedObjects);
        }

        // only fill last related object values now; values of intermediate related objects' parent might not be available yet.
        if (i == objectPath.Length - 1)
        {
          for (int j = 0; j < relatedValues.Count; j++)
          {
            relatedObjects[j].SetPropertyValue(property, relatedValues[j]);
          }
        }
      }
    }

    // senario:
    //  dataObject1.L1RelatedDataObjects.property1
    //  dataObject1.L1RelatedDataObjects.property2
    //  dataObject1.L1RelatedDataObjects.L2RelatedDataObjects.property3.value1
    //  dataObject1.L1RelatedDataObjects.L2RelatedDataObjects.property4.value2
    //
    // L2RelatedDataObjects result:
    //  dataObject1.L1RelatedDataObjects[1].L2RelatedDataObjects[1].property3.value1
    //  dataObject1.L1RelatedDataObjects[1].L2RelatedDataObjects[2].property4.value2
    //  dataObject1.L1RelatedDataObjects[2].L2RelatedDataObjects[1].property3.value1
    //  dataObject1.L1RelatedDataObjects[2].L2RelatedDataObjects[2].property4.value2
    protected void SetRelatedObjects()
    {
      DataDictionary dictionary = _dataLayer.GetDictionary();

      for (int i = 0; i < _dataObjects.Count; i++)
      {
        Dictionary<string, IList<IDataObject>> relatedObjectDictionary = _relatedObjects[i];

        foreach (string relatedObjectPath in _relatedObjectPaths)
        {
          string[] relatedObjectPathElements = relatedObjectPath.Split('.');

          for (int j = 0; j < relatedObjectPathElements.Length - 1; j++)
          {
            string parentObjectType = relatedObjectPathElements[j];
            string relatedObjectType = relatedObjectPathElements[j + 1];

            if (relatedObjectDictionary.ContainsKey(relatedObjectType))
            {
              IList<IDataObject> parentObjects = null;

              if (j == 0)
                parentObjects = new List<IDataObject> { _dataObjects[i] };
              else
                parentObjects = relatedObjectDictionary[parentObjectType];

              IList<IDataObject> relatedObjects = relatedObjectDictionary[relatedObjectType];

              foreach (IDataObject parentObject in parentObjects)
              {
                DataObject dataObject = dictionary.DataObjects.First(c => c.ObjectName == parentObjectType);
                DataRelationship dataRelationship = dataObject.DataRelationships.First(c => c.RelationshipName == relatedObjectType);

                foreach (IDataObject relatedObject in relatedObjects)
                {
                  foreach (PropertyMap map in dataRelationship.PropertyMaps)
                  {
                    relatedObject.SetPropertyValue(map.RelatedPropertyName, parentObject.GetPropertyValue(map.DataPropertyName));
                  }
                }
              }
            }
          }
        }
      }
    }

    protected void SetClassIdentifiers(DataDirection direction)
    {
      switch (direction)
      {
        case DataDirection.Outbound:
          SetOutboundClassIdentifiers();
          break;

        case DataDirection.InboundSparql:
          SetInboundSparqlClassIdentifiers();
          break;

        case DataDirection.InboundDto:
          SetInboundDtoClassIdentifiers();
          break;
      }
    }

    private void SetOutboundClassIdentifiers()
    {
      _classIdentifiers.Clear();

      foreach (ClassTemplateMap classTemplateMap in _graphMap.ClassTemplateMaps)
      {
        ClassMap classMap = classTemplateMap.ClassMap;
        List<string> identifiers = new List<string>();

        foreach (string identifier in classMap.Identifiers)
        {
          // identifier is a fixed value
          if (identifier.StartsWith("#") && identifier.EndsWith("#"))
          {
            string value = identifier.Substring(1, identifier.Length - 2);

            for (int i = 0; i < _dataObjects.Count; i++)
            {
              if (identifiers.Count == i)
              {
                identifiers.Add(value);
              }
              else
              {
                identifiers[i] += classMap.IdentifierDelimiter + value;
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

                if (identifiers.Count == i)
                {
                  identifiers.Add(value);
                }
                else
                {
                  identifiers[i] += classMap.IdentifierDelimiter + value;
                }
              }
            }
          }
        }

        _classIdentifiers[classMap.ClassId] = identifiers;
      }
    }

    private void SetInboundSparqlClassIdentifiers()
    {
      _classIdentifiers.Clear();

      if (_memoryStore != null)
      {
        ClassTemplateMap classTemplateMap = _graphMap.ClassTemplateMaps.First();
        string classId = classTemplateMap.ClassMap.ClassId;

        string query = String.Format(CLASS_INSTANCE_QUERY_TEMPLATE, classId);
        object results = _memoryStore.ExecuteQuery(query);

        if (results != null)
        {
          SparqlResultSet resultSet = (SparqlResultSet)results;

          foreach (SparqlResult result in resultSet)
          {
            string classInstance = result.ToString().Remove(0, ("?class = " + _graphBaseUri).Length);

            if (!String.IsNullOrEmpty(classInstance))
            {
              if (!_classIdentifiers.ContainsKey(classId))
              {
                _classIdentifiers[classId] = new List<string> { classInstance };
              }
              else if (!_classIdentifiers[classId].Contains(classInstance))
              {
                _classIdentifiers[classId].Add(classInstance);
              }
            }
            else
            {
              throw new Exception("Class identifier of [" + classId + "] is null");
            }
          }
        }
      }
    }

    protected void SetInboundDtoClassIdentifiers()
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
                classIdentifiers[i] += classMap.IdentifierDelimiter + value;
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
                  classIdentifiers[i] += classMap.IdentifierDelimiter + value;
                }
              }
            }
          }
        }

        _classIdentifiers[classMap.ClassId] = classIdentifiers;
      }
    }
  }

  public enum DataDirection
  {
    InboundSparql,
    InboundDto,
    Outbound,
  }
}
