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
using System.Linq;
using System.Reflection;
using org.iringtools.utility;
using org.w3.sparql_results;
using Ninject;
using org.iringtools.library;


namespace org.iringtools.adapter.projection
{
  public class SPARQLEngine : IProjectionEngine
    {
        private WebProxyCredentials _proxyCredentials = null;
        private WebCredentials _targetCredentials = null;
        private string _targetUri = String.Empty;
        private Mapping _mapping = null;
        private IDTOService _dtoService = null;
        private bool _trimData;
        
        private string _identifierClassName = string.Empty;
        private Dictionary<string, DataTransferObject> _dtoList = null;
        private Dictionary<string, Dictionary<string, string>> _pullValueLists = null;
        private Dictionary<string, Dictionary<string, string>> _refreshValueLists = null;
        private int _instanceCounter = 0;
        
        [Inject]
        public SPARQLEngine(AdapterSettings settings, IDTOService dtoService)
        {
          _mapping = settings.Mapping;
          _proxyCredentials = settings.ProxyCredentials;
          _targetCredentials = settings.TargetCredentials;
          _targetUri = settings.InterfaceServer;
          _trimData = settings.TrimData;  
          _dtoService = dtoService;
        }
        
        public List<string> GetIdentifiers(string graphName)
        {
            try
            {
                GraphMap graphMap = new GraphMap();
                List<string> identifiers = new List<string>();
                bool isIdentifierMapped = false;
                TemplateMap identifierTemplateMap = null;
                RoleMap identifierRoleMap = null;
                foreach (GraphMap mappingGraphMap in _mapping.graphMaps)
                {
                    if (mappingGraphMap.name == graphName)
                    {
                        graphMap = mappingGraphMap;
                    }
                }

                foreach (TemplateMap templateMap in graphMap.templateMaps)
                {
                    foreach (RoleMap roleMap in templateMap.roleMaps)
                    {
                        if (roleMap.propertyName == graphMap.identifier)
                        {
                            identifierTemplateMap = templateMap;
                            identifierRoleMap = roleMap;
                            isIdentifierMapped = true;
                            break;
                        }
                    }
                    if (isIdentifierMapped) break;
                }

                if (isIdentifierMapped)
                {
                    string identifier = String.Empty;
                    string identifierUri = String.Empty;

                    SPARQLQuery identifierQuery = new SPARQLQuery(SPARQLQueryType.SELECT);

                    identifierQuery.addVariable("?" + identifierRoleMap.propertyName);
                    identifierQuery.addVariable("?i");

                    SPARQLClassification classification = identifierQuery.addClassification(graphMap.classId, "?i");
                    identifierQuery.addTemplate(identifierTemplateMap.templateId, identifierTemplateMap.classRole, "?i", identifierRoleMap.roleId, "?" + identifierRoleMap.propertyName);
                    identifierQuery.addTemplate(identifierTemplateMap.templateId, identifierTemplateMap.classRole, "?i", "tpl:endDateTime", "?endDateTime");
                    SPARQLResults sparqlResults = SPARQLClient.PostQuery(_targetUri, identifierQuery.getSPARQL(), _targetCredentials, _proxyCredentials);

                    foreach (SPARQLResult result in sparqlResults.resultsElement.results)
                    {
                        foreach (SPARQLBinding binding in result.bindings)
                        {
                            if (binding.name == identifierRoleMap.propertyName)
                            {
                                identifier = binding.literal.value;
                                identifiers.Add(identifier);
                                break;
                            }
                        }
                    }
                    return identifiers;
                }
                else
                {
                    throw new Exception(String.Format("Identifier is not mapped for graph {0}", graphMap.name));
                }
            }
            catch (Exception exception)
            {
                throw new Exception(String.Format("GetIdentifiersFromTripleStore[{0}]", graphName), exception);
            }
        }

        public List<DataTransferObject> GetList(string graphName)
        {
          try
          {
            _dtoList = new Dictionary<string, DataTransferObject>();
            foreach (GraphMap graphMap in _mapping.graphMaps)
            {
              if (graphMap.name == graphName)
              {
                QueryGraphMap(graphMap);
              }
            }
            return _dtoList.Values.ToList<DataTransferObject>();
          }
          catch (Exception exception)
          {
            throw new Exception(String.Format("GetList[{0}]", graphName), exception);
          }
        }

        public void Post(DataTransferObject dto)
        {
          try
          {
            foreach (GraphMap graphMap in _mapping.graphMaps)
            {
              if (graphMap.name == dto.GraphName)
              {
                RefreshGraphMap(graphMap, dto);
              }
            }
          }
          catch (Exception exception)
          {
            throw new Exception(String.Format("Post[{0}][{1}]", dto.GraphName, dto.Identifier), exception);
          }
        }

        public void PostList(List<DataTransferObject> dtos)
        {
          try
          {
            foreach (GraphMap graphMap in _mapping.graphMaps)
            {
              foreach (DataTransferObject dto in dtos)
              {
                if (graphMap.name == dto.GraphName)
                {
                  RefreshGraphMap(graphMap, dto);
                }
              }
            }
          }
          catch (Exception exception)
          {
            throw new Exception("PostList: " + exception);
          }
        }

        public void Delete(string graphName, string identifier)
        {
          try
          {
            foreach (GraphMap graphMap in _mapping.graphMaps)
            {
              if (graphMap.name == graphName)
              {
                RefreshDeleteGraphMap(graphMap, identifier);
              }
            }
          }
          catch (Exception exception)
          {
            throw new Exception("Delete: " + exception);
          }
        }

        public void DeleteList(string graphName, List<string> identifiers)
        {
          try
          {
            foreach (GraphMap graphMap in _mapping.graphMaps)
            {
              if (graphMap.name == graphName)
              {
                foreach (string identifier in identifiers)
                {
                  RefreshDeleteGraphMap(graphMap, identifier);
                }
              }
            }
          }
          catch (Exception exception)
          {
            throw new Exception("DeleteList: " + exception);
          }
        }

        public void DeleteAll()
        {
          try
          {
            SPARQLClient.PostQueryAsMultipartMessage(_targetUri, "CLEAR", _targetCredentials, _proxyCredentials);
          }
          catch (Exception exception)
          {
            throw new Exception("DeleteAll: " + exception);
          }
        }

        private void QueryGraphMap(GraphMap graphMap)
        {
            try
            {
                List<string> identifiers = GetIdentifiers(graphMap.name);
                TemplateMap identifierTemplateMap = null;
                RoleMap identifierRoleMap = null;
                foreach (TemplateMap templateMap in graphMap.templateMaps)
                {
                    foreach (RoleMap roleMap in templateMap.roleMaps)
                    {
                        if (roleMap.propertyName == graphMap.identifier)
                        {
                            identifierTemplateMap = templateMap;
                            identifierRoleMap = roleMap;
                            break;
                        }
                    }
                }

                SPARQLQuery identifierQuery = new SPARQLQuery(SPARQLQueryType.SELECT);

                identifierQuery.addVariable("?" + identifierRoleMap.propertyName);
                identifierQuery.addVariable("?i");

                SPARQLClassification classification = identifierQuery.addClassification(graphMap.classId, "?i");
                identifierQuery.addTemplate(classification.TemplateName, classification.ClassRole, classification.ClassId, "tpl:endDateTime", "?endDateTime");
                identifierQuery.addTemplate(identifierTemplateMap.templateId, identifierTemplateMap.classRole, "?i", identifierRoleMap.roleId, "?" + identifierRoleMap.propertyName);

                foreach (String identifier in identifiers)
                {
                    DataTransferObject dto = _dtoService.GetDTO(graphMap.name, identifier);
                    if (dto == null) dto = _dtoService.Create(graphMap.name, identifier);
                    _dtoList.Add(identifier, dto);
                }

                foreach (TemplateMap templateMap in graphMap.templateMaps)
                {
                    QueryTemplateMap(templateMap, (ClassMap)graphMap, identifierQuery);
                }
            }
            catch (Exception exception)
            {
                throw new Exception(String.Format("QueryGraphMap[{0}]", graphMap.name), exception);
            }
        }

        private void QueryTemplateMap(TemplateMap templateMap, ClassMap classMap, SPARQLQuery previousQuery)
        {
            try
            {
                SPARQLQuery query = new SPARQLQuery(SPARQLQueryType.SELECT);
                query.Merge(previousQuery);

                string graphIdentifierVariable = query.Variables.First<string>();
                string parentIdentifierVariable = query.Variables.Last<string>();

                if (templateMap.roleMaps[0].classMap == null)
                {
                    foreach (RoleMap roleMap in templateMap.roleMaps)
                    {
                        query.addVariable("?" + roleMap.propertyName);
                        if (roleMap.reference != null)
                        {
                          query.addTemplate(templateMap.templateId, templateMap.classRole, parentIdentifierVariable, roleMap.roleId, roleMap.reference);
                        }
                        else if (roleMap.value != null)
                        {
                          string value = query.getLITERAL_SPARQL(roleMap.value, roleMap.dataType);
                          query.addTemplate(templateMap.templateId, templateMap.classRole, parentIdentifierVariable, roleMap.roleId,  value);
                        }
                        else
                        {
                          query.addTemplate(templateMap.templateId, templateMap.classRole, parentIdentifierVariable, roleMap.roleId, "?" + roleMap.propertyName);
                        }
                    }
                    query.addTemplate(templateMap.templateId, templateMap.classRole, parentIdentifierVariable, "tpl:endDateTime", "?endDateTime");
                    SPARQLResults sparqlResults = SPARQLClient.PostQuery(_targetUri, query.getSPARQL(), _targetCredentials, _proxyCredentials);

                    foreach (SPARQLResult result in sparqlResults.resultsElement.results)
                    {
                        DataTransferObject dto = _dtoList[result.bindings[0].literal.value];

                        foreach (SPARQLBinding binding in result.bindings)
                        {
                            string graphIdentifierVariableName = graphIdentifierVariable.Replace("?", "");
                            string propertyName = binding.name;

                            if (propertyName != graphIdentifierVariableName)
                            {
                                object propertyValue = null;

                                RoleMap roleMap = FindRoleMap(templateMap, propertyName);

                                if (binding.literal != null)
                                {
                                    propertyValue = binding.literal.value;
                                    dto.SetPropertyValue(propertyName, propertyValue);
                                }
                                else if (roleMap != null && (roleMap.valueList != "" || roleMap.valueList != null))
                                {
                                    Dictionary<string, string> valueList = GetPullValueMap(roleMap.valueList);

                                    string propertyUri = query.getPREFIX_URI(binding.uri);
                                    propertyValue = valueList[propertyUri];
                                    dto.SetPropertyValue(propertyName, propertyValue);
                                }
                            }
                        }
                    }
                }
                else
                {
                    RoleMap roleMap = templateMap.roleMaps[0];

                    _instanceCounter++;

                    string instanceVariable = "?i" + _instanceCounter.ToString();

                    if (roleMap.reference != null)
                    {
                      query.addTemplate(templateMap.templateId, templateMap.classRole, parentIdentifierVariable, roleMap.roleId, roleMap.reference);
                    }
                    else if (roleMap.value != null)
                    {
                      string value = query.getLITERAL_SPARQL(roleMap.value, roleMap.dataType);
                      query.addTemplate(templateMap.templateId, templateMap.classRole, parentIdentifierVariable, roleMap.roleId, value);
                    }
                    else
                    {
                      query.addTemplate(templateMap.templateId, templateMap.classRole, parentIdentifierVariable, roleMap.roleId, instanceVariable);
                    }

                    QueryClassMap(roleMap.classMap, roleMap, query, instanceVariable);

                    _instanceCounter--;
                }
            }
            catch (Exception exception)
            {
                throw new Exception(String.Format("QueryTemplateMap[{0}]", templateMap.name), exception);
            }
        }

        private void QueryClassMap(ClassMap classMap, RoleMap roleMap, SPARQLQuery previousQuery, string instanceVariable)
        {
            try
            {
                SPARQLQuery query = new SPARQLQuery(SPARQLQueryType.SELECT);
                query.Merge(previousQuery);

                query.addVariable(instanceVariable);
                SPARQLClassification classification = query.addClassification(classMap.classId, instanceVariable);
                query.addTemplate(classification.TemplateName, classification.ClassRole, classification.ClassId, "tpl:endDateTime", "?endDateTime");
                foreach (TemplateMap templateMap in classMap.templateMaps)
                {
                    QueryTemplateMap(templateMap, classMap, query);
                }

            }
            catch (Exception exception)
            {
                throw new Exception(String.Format("QueryClassMap[{0}][{1}]", classMap.name, roleMap.name), exception);
            }
        }

        private Dictionary<string, string> GetPullValueMap(string valueListName)
        {
            try
            {
                Dictionary<string, string> valueList = null;

                if (_pullValueLists == null)
                {
                    _pullValueLists = new Dictionary<string, Dictionary<string, string>>();
                }

                if (_pullValueLists.ContainsKey(valueListName))
                {
                    valueList = _pullValueLists[valueListName];
                }
                else
                {
                    valueList = new Dictionary<string, string>();
                    foreach (ValueMap valueMap in _mapping.valueMaps)
                    {
                        if (valueMap.valueList == valueListName)
                        {
                            string uri = valueMap.modelURI;
                            if (!valueList.ContainsKey(uri))
                            {
                                valueList.Add(uri, valueMap.internalValue); //First one is the default
                            }
                        }
                    }
                    _pullValueLists.Add(valueListName, valueList);
                }

                return valueList;
            }
            catch (Exception exception)
            {
                throw new Exception("Error while getting or building ValueList " + valueListName + ".", exception);
            }
        }

        private Dictionary<string, string> GetRefreshValueMap(string valueListName)
        {
            try
            {
                Dictionary<string, string> valueList = null;

                if (_refreshValueLists == null)
                {
                    _refreshValueLists = new Dictionary<string, Dictionary<string, string>>();
                }

                if (_refreshValueLists.ContainsKey(valueListName))
                {
                    valueList = _refreshValueLists[valueListName];
                }
                else
                {
                    valueList = new Dictionary<string, string>();
                    foreach (ValueMap valueMap in _mapping.valueMaps)
                    {
                        if (valueMap.valueList == valueListName)
                        {
                            string key = valueMap.internalValue;
                            if (!valueList.ContainsKey(key))
                            {
                                valueList.Add(key, valueMap.modelURI); //First one is the default
                            }
                        }
                    }
                    _refreshValueLists.Add(valueListName, valueList);
                }

                return valueList;
            }
            catch (Exception exception)
            {
                throw new Exception("Error while getting or building ValueList " + valueListName + ".", exception);
            }
        }

        private T FindCollectionItem<T>(List<T> collection, string propertyName, string propertyValue)
        {
          try
          {
            Type[] types = { };
            ConstructorInfo constructor = typeof(T).GetConstructor(types);
            object[] parameters = { };

            T requestedItem = (T)constructor.Invoke(parameters);

            PropertyInfo property = typeof(T).GetProperty(propertyName);
            MethodInfo getMethod = property.GetGetMethod();
            foreach (T item in collection)
            {
              object value = getMethod.Invoke(item, parameters);

              if (value.ToString() == propertyValue)
              {
                requestedItem = item;
                break;
              }
            }

            return requestedItem;
          }
          catch (Exception exception)
          {
            throw new Exception(String.Format("FindCollectionItem[{0}][{1}]", propertyName, propertyValue), exception);
          }
        }

        private RoleMap FindRoleMap(TemplateMap templateMap, string propertyName)
        {
          try
          {
            RoleMap roleMap = null;

            var queryResults = from map in templateMap.roleMaps
                               where map.propertyName == propertyName
                               select map;

            if (queryResults.Count<RoleMap>() > 0)
            {
              roleMap = queryResults.First<RoleMap>();
            }

            return roleMap;
          }
          catch (Exception exception)
          {
            throw new Exception(String.Format("FindRoleMap[{0}][{1}]", templateMap.name, propertyName), exception);
          }
        }
    
        private void RefreshDeleteGraphMap(GraphMap graphMap, string identifier)
        {
          try
          {
            //PREFIX dm: <http://dm.rdswip.org/data#>
            //PREFIX rdl: <http://rdl.rdswip.org/data#>
            //PREFIX tpl: <http://tpl.rdswip.org/data#>
            //PREFIX sb: <http://sandbox.ids-adi.org/data#>
            //PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
            //PREFIX eg: <http://www.example.com/data#>
            //SELECT
            //?s ?p
            //WHERE 
            //{
            // ?s ?p eg:R99486931975__1-M6-AB-001 . 
            // FILTER (?p != dm:instance)
            // OPTIONAL { ?s tpl:endDateTime ?endDateTime }
            // FILTER (!bound(?endDateTime))
            //}

            //check if class is referenced by others

            identifier = "eg:id__" + identifier;

            foreach (TemplateMap templateMap in graphMap.templateMaps)
            {
              RefreshDeleteTemplateMap(templateMap, (ClassMap)graphMap, identifier);
            }

            SPARQLQuery query = new SPARQLQuery(SPARQLQueryType.SELECTFORDELETE);
            query.addVariable("?subject");
            query.addVariable("?predicate");
            SPARQLClassification classificationForQuery = query.addClassification(graphMap.classId, query.getPREFIX_URI(identifier));

            SPARQLResults sparqlResults = SPARQLClient.PostQuery(_targetUri, query.getSPARQL(), _targetCredentials, _proxyCredentials);
            //if not terminate it
            //check the results above, if 0 then terminate it else don't
            if (sparqlResults.resultsElement.results.Count() == 0)
            {
              SPARQLQuery insertTemporalQuery = new SPARQLQuery(SPARQLQueryType.INSERTTEMPORAL);
              SPARQLClassification classification = insertTemporalQuery.addClassification(graphMap.classId, query.getPREFIX_URI(identifier));
              string endTimeValue = query.getLITERAL_SPARQL(DateTime.UtcNow.ToString(), "datetime");
              insertTemporalQuery.addTemplate(classification.TemplateName, classification.ClassRole, identifier, "tpl:endDateTime", "?endDateTime", endTimeValue);
              SPARQLBuilder.ExecuteUpdateQuery(_targetUri, _targetCredentials, _proxyCredentials, insertTemporalQuery.getSPARQL());

            }
          }
          catch (Exception exception)
          {
            throw new Exception(String.Format("RefreshDeleteGraphMap[{0}][{1}]", graphMap.name, identifier), exception);
          }
        }

        private void RefreshDeleteTemplateMap(TemplateMap templateMap, ClassMap classMap, string parentIdentifierVariable)
        {
          try
          {
            if (templateMap.type == TemplateType.Property)
            {
              //terminate the template
              SPARQLQuery query = new SPARQLQuery(SPARQLQueryType.INSERTTEMPORAL);
              foreach (RoleMap roleMap in templateMap.roleMaps)
              {
                query.addTemplate(templateMap.templateId, templateMap.classRole, parentIdentifierVariable, roleMap.roleId, "?" + roleMap.propertyName);
              }
              string endTimeValue = query.getLITERAL_SPARQL(DateTime.UtcNow.ToString(), "datetime");
              query.addTemplate(templateMap.templateId, templateMap.classRole, parentIdentifierVariable, "tpl:endDateTime", "?endDateTime", endTimeValue);
              SPARQLBuilder.ExecuteUpdateQuery(_targetUri, _targetCredentials, _proxyCredentials, query.getSPARQL());
            }
            else
            {
              RoleMap roleMap = templateMap.roleMaps[0];
              //Get the instance variable                

              string instanceVariable = SPARQLBuilder.GetRelatedClassInstance(_targetUri, _targetCredentials, _proxyCredentials, templateMap, roleMap, parentIdentifierVariable)[0];

              SPARQLQuery query = new SPARQLQuery(SPARQLQueryType.INSERTTEMPORAL);
              SPARQLClassification classification = query.addClassification(classMap.classId, query.getPREFIX_URI(parentIdentifierVariable));
              string endTimeValue = query.getLITERAL_SPARQL(DateTime.UtcNow.ToString(), "datetime");
              query.addTemplate(templateMap.templateId, templateMap.classRole, parentIdentifierVariable, roleMap.roleId, query.getPREFIX_URI(instanceVariable));
              query.addTemplate(templateMap.templateId, templateMap.classRole, parentIdentifierVariable, "tpl:endDateTime", "?endDateTime", endTimeValue);
              SPARQLBuilder.ExecuteUpdateQuery(_targetUri, _targetCredentials, _proxyCredentials, query.getSPARQL());

              RefreshDeleteClassMap(roleMap.classMap, roleMap, instanceVariable);              

            }
          }
          catch (Exception exception)
          {
            throw new Exception(String.Format("RefreshDeleteGraphMap[{0}][{1}][{2}]", templateMap.name, classMap.name, parentIdentifierVariable), exception);
          }
        }

        private void RefreshDeleteClassMap(ClassMap classMap, RoleMap currentRoleMap, string parentIdentifierVariable)
        {
          try
          {
            //check that the class is not referenced by others
            SPARQLQuery query = new SPARQLQuery(SPARQLQueryType.SELECTFORDELETE);
            query.addVariable("?subject");
            query.addVariable("?predicate");
            SPARQLClassification classificationForQuery = query.addClassification(classMap.classId, query.getPREFIX_URI(parentIdentifierVariable));

            SPARQLResults sparqlResults = SPARQLClient.PostQuery(_targetUri, query.getSPARQL(), _targetCredentials, _proxyCredentials);
            //if not terminate it           
            if (sparqlResults.resultsElement.results.Count() == 1)
            {
              SPARQLQuery insertTemporalQuery = new SPARQLQuery(SPARQLQueryType.INSERTTEMPORAL);
              SPARQLClassification classification = insertTemporalQuery.addClassification(classMap.classId, query.getPREFIX_URI(parentIdentifierVariable));
              string endTimeValue = query.getLITERAL_SPARQL(DateTime.UtcNow.ToString(), "datetime");
              insertTemporalQuery.addTemplate(classification.TemplateName, classification.ClassRole, parentIdentifierVariable, "tpl:endDateTime", "?endDateTime", endTimeValue);
              SPARQLBuilder.ExecuteUpdateQuery(_targetUri, _targetCredentials, _proxyCredentials, insertTemporalQuery.getSPARQL());

              foreach (TemplateMap templateMap in classMap.templateMaps)
              {
                RefreshDeleteTemplateMap(templateMap, classMap, parentIdentifierVariable);
              }
            }
            
            
          }
          catch (Exception exception)
          {
            throw new Exception(String.Format("RefreshDeleteClassMap[{0}][{1}][{2}]", classMap.name, currentRoleMap.name, parentIdentifierVariable), exception);
          }
        }

        private void RefreshGraphMap(GraphMap graphMap, DataTransferObject dto)
        {
          try
          {
            string identifier = dto.Identifier;

            identifier = "eg:id__" + identifier;
            SPARQLBuilder.RefreshGraphClassName(_targetUri, _targetCredentials, _proxyCredentials, graphMap.classId, identifier);

            foreach (TemplateMap templateMap in graphMap.templateMaps)
            {
              RefreshTemplateMap(templateMap, (ClassMap)graphMap, dto, identifier);
            }
          }
          catch (Exception exception)
          {
            throw new Exception(String.Format("RefreshGraphMap[{0}][{1}]", graphMap.name, dto.Identifier), exception);
          }
        }       

        private void RefreshTemplateMap(TemplateMap templateMap, ClassMap classMap, DataTransferObject dto, string parentIdentifierVariable)
        {
            try
            {
                if (templateMap.type == TemplateType.Property)
                {
                  SPARQLResults sparqlResults = SPARQLBuilder.GetTemplateValues(_targetUri, _targetCredentials, _proxyCredentials, templateMap, parentIdentifierVariable); 
                  #region If property exists already
                  if (sparqlResults.resultsElement.results.Count > 0)
                    {
                        foreach (SPARQLResult result in sparqlResults.resultsElement.results)
                        {

                            SPARQLQuery insertTemporalQuery = new SPARQLQuery(SPARQLQueryType.INSERTTEMPORAL);

                            SPARQLQuery insertQuery = new SPARQLQuery(SPARQLQueryType.INSERT);

                            bool isPropertyValueDifferent = false;

                            #region Check if Current property Value differs from New Value
                            foreach (SPARQLBinding binding in result.bindings)
                            {
                                string propertyName = binding.name;
                                string propertyValue = string.Empty;
                                string propertyType = string.Empty;
                                string dtoPropertyValue = string.Empty;
                                string curPropertyValue = string.Empty;

                                if (binding.literal != null && binding.literal.value != null)
                                {
                                    curPropertyValue = binding.literal.value;
                                    propertyType = "literal";
                                }
                                else if (binding.uri != null)
                                {
                                    curPropertyValue = insertQuery.getPREFIX_URI(binding.uri);
                                    propertyType = "uri";
                                }
                                else
                                {
                                    curPropertyValue = string.Empty;
                                    propertyType = "literal";
                                }

                                object obj = dto.GetPropertyValue(propertyName);
                                if (obj != null)
                                  if (_trimData)
                                    dtoPropertyValue = obj.ToString().Trim();
                                  else
                                    dtoPropertyValue = obj.ToString();
                                else
                                    dtoPropertyValue = "";

                                RoleMap roleMap = FindRoleMap(templateMap, propertyName);

                                if (roleMap.reference != null && roleMap.reference != String.Empty)
                                {
                                  if (roleMap.valueList != null && roleMap.valueList != String.Empty)
                                  {
                                    Dictionary<string, string> valueList = GetRefreshValueMap(roleMap.valueList);
                                    if (valueList.ContainsKey(dtoPropertyValue))
                                    {
                                      propertyValue = valueList[dtoPropertyValue];
                                    }
                                    else
                                    {
                                      throw (new Exception(String.Format("valueList[{0}] value[{1}] isn't defined", roleMap.valueList, dtoPropertyValue)));
                                    }
                                  }
                                  else
                                  {
                                    propertyValue = roleMap.reference;
                                  }
                                }
                                else if (roleMap.value != null && roleMap.value != String.Empty)
                                {
                                  propertyValue = roleMap.value;
                                }
                                else
                                {
                                  propertyValue = dtoPropertyValue;
                                }
                                //if (roleMap.valueList == null || roleMap.valueList == string.Empty)
                                //{
                                //    propertyValue = dtoPropertyValue;
                                //}
                                //else
                                //{
                                //    Dictionary<string, string> valueList = GetRefreshValueMap(roleMap.valueList);
                                //    if (valueList.ContainsKey(dtoPropertyValue))
                                //    {
                                //        propertyValue = valueList[dtoPropertyValue];
                                //    }
                                //    else
                                //    {
                                //        throw (new Exception(String.Format("valueList[{0}] value[{1}] isn't defined", roleMap.valueList, dtoPropertyValue)));
                                //    }
                                //}

                                if (!curPropertyValue.Equals(propertyValue))
                                {
                                    isPropertyValueDifferent = true;
                                }

                            }
                            #endregion

                            if (isPropertyValueDifferent)
                            {
                                foreach (SPARQLBinding binding in result.bindings)
                                {
                                    string propertyName = binding.name;
                                    string propertyValue = string.Empty;
                                    string propertyType = string.Empty;
                                    string dtoPropertyValue = string.Empty;

                                    if (binding.literal != null && binding.literal.value != null)
                                    {
                                        propertyType = "literal";
                                    }
                                    else if (binding.uri != null)
                                    {
                                        propertyType = "uri";
                                    }
                                    else
                                    {
                                        propertyType = "literal";
                                    }

                                    object obj = dto.GetPropertyValue(propertyName);
                                    if (obj != null)
                                      if (_trimData)
                                        dtoPropertyValue = obj.ToString().Trim();
                                      else
                                        dtoPropertyValue = obj.ToString();
                                    else
                                        dtoPropertyValue = "";

                                    RoleMap roleMap = FindRoleMap(templateMap, propertyName);

                                    if (roleMap.reference != null && roleMap.reference != String.Empty)
                                    {
                                      if (roleMap.valueList != null && roleMap.valueList != String.Empty)
                                      {
                                        Dictionary<string, string> valueList = GetRefreshValueMap(roleMap.valueList);
                                        if (valueList.ContainsKey(dtoPropertyValue))
                                        {
                                          propertyValue = valueList[dtoPropertyValue];
                                        }
                                        else
                                        {
                                          throw (new Exception(String.Format("valueList[{0}] value[{1}] isn't defined", roleMap.valueList, dtoPropertyValue)));
                                        }
                                      }
                                      else
                                      {
                                        propertyValue = roleMap.reference;
                                      }
                                    }
                                    else if (roleMap.value != null && roleMap.value != String.Empty)
                                    {
                                      propertyValue = roleMap.value;
                                    }
                                    else
                                    {
                                      propertyValue = dtoPropertyValue;
                                    }
                                    //if (roleMap.valueList == null || roleMap.valueList == string.Empty)
                                    //{
                                    //    propertyValue = dtoPropertyValue;
                                    //}
                                    //else
                                    //{
                                    //    Dictionary<string, string> valueList = GetRefreshValueMap(roleMap.valueList);
                                    //    if (valueList.ContainsKey(dtoPropertyValue))
                                    //    {
                                    //        propertyValue = valueList[dtoPropertyValue];
                                    //    }
                                    //    else
                                    //    {
                                    //        throw (new Exception(String.Format("valueList[{0}] value[{1}] isn't defined", roleMap.valueList, dtoPropertyValue)));
                                    //    }
                                    //}
                                    if (propertyType == "literal")
                                        propertyValue = insertQuery.getLITERAL_SPARQL(propertyValue, roleMap.dataType);

                                    insertTemporalQuery.addTemplate(templateMap.templateId, templateMap.classRole, parentIdentifierVariable, roleMap.roleId, "?" + roleMap.propertyName, propertyValue);
                                    insertQuery.addTemplate(templateMap.templateId, templateMap.classRole, parentIdentifierVariable, roleMap.roleId, "?" + roleMap.propertyName, propertyValue);
                                }
                                string endTimeValue = insertTemporalQuery.getLITERAL_SPARQL(DateTime.UtcNow.ToString(), "datetime");
                                insertTemporalQuery.addTemplate(templateMap.templateId, templateMap.classRole, parentIdentifierVariable, "tpl:endDateTime", "?endDateTime", endTimeValue);
                                SPARQLBuilder.ExecuteUpdateQuery(_targetUri, _targetCredentials, _proxyCredentials, insertTemporalQuery.getSPARQL());

                                string startTimeValue = insertQuery.getLITERAL_SPARQL(DateTime.UtcNow.ToString(), "datetime");
                                insertQuery.addTemplate(templateMap.templateId, templateMap.classRole, parentIdentifierVariable, "tpl:startDateTime", "?startDateTime", startTimeValue);
                                SPARQLBuilder.ExecuteUpdateQuery(_targetUri, _targetCredentials, _proxyCredentials, insertQuery.getSPARQL());
                            }

                        }
                    }
                    #endregion
                  #region Else if property doesn't exist
                    else
                    {

                        SPARQLQuery insertQuery = new SPARQLQuery(SPARQLQueryType.INSERT);

                        foreach (RoleMap roleMap in templateMap.roleMaps)
                        {
                            string propertyValue = string.Empty;
                            string propertyType = "literal";
                            string dtoPropertyValue = string.Empty;
                            string curPropertyValue = string.Empty;

                            object obj = dto.GetPropertyValueByInternalName(roleMap.propertyName);
                            if (obj != null)
                              if (_trimData)
                                dtoPropertyValue = obj.ToString().Trim();
                              else
                                dtoPropertyValue = obj.ToString();
                            else
                                dtoPropertyValue = string.Empty;

                            if (roleMap.reference != null && roleMap.reference != String.Empty)
                            {
                              if (roleMap.valueList != null && roleMap.valueList != String.Empty)
                              {
                                Dictionary<string, string> valueList = GetRefreshValueMap(roleMap.valueList);
                                if (valueList.ContainsKey(dtoPropertyValue))
                                {
                                  propertyValue = valueList[dtoPropertyValue];
                                  propertyType = "uri";
                                }
                                else
                                {
                                  throw (new Exception(String.Format("valueList[{0}] value[{1}] isn't defined", roleMap.valueList, dtoPropertyValue)));
                                }
                              }
                              else
                              {
                                propertyValue = roleMap.reference;
                                propertyType = "uri";
                              }
                            }
                            else if (roleMap.value != null && roleMap.value != String.Empty)
                            {
                              propertyValue = roleMap.value;
                              propertyType = "literal";
                            }
                            else
                            {
                              propertyValue = dtoPropertyValue;
                              propertyType = "literal";
                            }
                            //if (roleMap.valueList == null || roleMap.valueList == string.Empty)
                            //{
                            //    propertyValue = dtoPropertyValue;
                            //    propertyType = "literal";
                            //}
                            //else
                            //{
                            //    Dictionary<string, string> valueList = GetRefreshValueMap(roleMap.valueList);
                            //    if (valueList.ContainsKey(dtoPropertyValue))
                            //    {
                            //        propertyValue = valueList[dtoPropertyValue];
                            //        propertyType = "uri";
                            //    }
                            //    else
                            //    {
                            //        throw (new Exception(String.Format("valueList[{0}] value[{1}] isn't defined", roleMap.valueList, dtoPropertyValue)));
                            //    }
                            //}

                            {
                                if (propertyType == "literal")
                                    propertyValue = insertQuery.getLITERAL_SPARQL(propertyValue, roleMap.dataType);

                                insertQuery.addTemplate(templateMap.templateId, templateMap.classRole, parentIdentifierVariable, roleMap.roleId, "?" + roleMap.propertyName, propertyValue);
                            }

                        }
                        string startTimeValue = insertQuery.getLITERAL_SPARQL(DateTime.UtcNow.ToString(), "datetime");
                        insertQuery.addTemplate(templateMap.templateId, templateMap.classRole, parentIdentifierVariable, "tpl:startDateTime", "?startDateTime", startTimeValue);

                        SPARQLBuilder.ExecuteUpdateQuery(_targetUri, _targetCredentials, _proxyCredentials, insertQuery.getSPARQL());
                    }
                    #endregion
                }
                else
                {
                    RoleMap roleMap = templateMap.roleMaps[0];

                    string instanceVariable = SPARQLBuilder.RefreshRelatedClass(_targetUri, _targetCredentials, _proxyCredentials, templateMap, roleMap, roleMap.classMap, parentIdentifierVariable, dto);
                    RefreshClassMap(roleMap.classMap, roleMap, dto, instanceVariable);
                }
            }
            catch (Exception exception)
            {
              throw new Exception(String.Format("RefreshTemplateMap[{0}][{1}][{2}]", templateMap.name, classMap.name, parentIdentifierVariable), exception);
            }
        }
              
        private void RefreshClassMap(ClassMap classMap, RoleMap currentRoleMap, DataTransferObject dto, string parentIdentifierVariable)
        {
          try
          {
            foreach (TemplateMap templateMap in classMap.templateMaps)
            {
              RefreshTemplateMap(templateMap, classMap, dto, parentIdentifierVariable);
            }
          }
          catch (Exception exception)
          {
            throw new Exception(String.Format("RefreshClassMap[{0}][{1}][{2}]", classMap.name, currentRoleMap.name, parentIdentifierVariable), exception);
          }
        }              
    }

  public class SPARQLBuilder
    {
        #region Constants
        public static string propertyType = "Property";
        public static string relationshipType = "Relationship";        
        #endregion

        public static void RefreshGraphClassName(string uri, WebCredentials targetCredentials, WebProxyCredentials proxyCredentials, string classId, string identifier)
        {
            try
            {

                if (!TestForClassInstance(uri, targetCredentials, proxyCredentials, classId, identifier))
                {
                    SPARQLQuery query = new SPARQLQuery(SPARQLQueryType.INSERT);
                    SPARQLClassification classification = query.addClassification(classId, identifier);
                    string startTimeValue = query.getLITERAL_SPARQL(DateTime.UtcNow.ToString(), "datetime");
                    query.addTemplate(classification.TemplateName, classification.ClassRole, classification.ClassId, "tpl:startDateTime", startTimeValue);

                    ExecuteUpdateQuery(uri, targetCredentials, proxyCredentials, query.getSPARQL());
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
            
        public static string RefreshRelatedClass(string uri, WebCredentials targetCredentials, WebProxyCredentials proxyCredentials, TemplateMap templateMap, RoleMap roleMap, ClassMap classMap, string className, DataTransferObject dto)
        {
            try
            {
                string unqualifiedClassId = classMap.classId.Split(':')[1];
                string identifier = "eg:" + unqualifiedClassId;

                if (classMap.identifier != string.Empty)
                {
                    string[] identifiers = classMap.identifier.Split(',');

                    
                    foreach (string identifierPart in identifiers)
                    {                      
                        object identifierPartValue = dto.GetPropertyValue(identifierPart.Trim());

                        if (identifierPartValue != null)
                        {
                            string safeValue = identifierPartValue.ToString().Replace(' ', '_');
                            safeValue = safeValue.Replace("?", "");
                            identifier += "__" + safeValue;
                        }
                        else
                        {
                            identifier += "__";
                        }
                    }
                }

                SPARQLBuilder.RefreshGraphClassName(uri, targetCredentials, proxyCredentials, classMap.classId, identifier);

                string relatedClassName = GetRelatedClassInstance(uri, targetCredentials, proxyCredentials, templateMap, roleMap, className)[0];
                if (relatedClassName == string.Empty)
                {
                    SPARQLQuery query = new SPARQLQuery(SPARQLQueryType.INSERT);
                    query.addTemplate(templateMap.templateId, templateMap.classRole, className, roleMap.roleId, identifier);
                    string startTimeValue = query.getLITERAL_SPARQL(DateTime.UtcNow.ToString(), "datetime");
                    query.addTemplate(templateMap.templateId, templateMap.classRole, className, "tpl:startDateTime", "?startDateTime", startTimeValue);
                    /*
                    string query = prefixes + insertQuery + @"
                              {
                              _:c     a           dm:classification                      .
                              _:c     dm:class    " + classMap.classId + @"              .
                              _:c     dm:instance  eg:" + identifier + @"                .
                              _:t     a        " + templateMap.templateId + @"           .   
                              _:t  " + templateMap.classRole + @" <" + className + @">  .
                              _:t  " + roleMap.roleId + "      eg:" + identifier + @"  .
          
                                              }";
                    */

                    ExecuteUpdateQuery(uri, targetCredentials, proxyCredentials, query.getSPARQL());
                    relatedClassName = identifier;
                }
                return relatedClassName;
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }

        public static bool TestForClassInstance(string uri, WebCredentials targetCredentials, WebProxyCredentials proxyCredentials, string classId, string identifier)
        {
            try
            {
                SPARQLQuery query = new SPARQLQuery(SPARQLQueryType.SELECT);

                SPARQLClassification classification = query.addClassification(classId, identifier);
                query.addVariable(classification.getNode());
                query.addTemplate(classification.TemplateName, classification.ClassRole, classification.ClassId, "tpl:endDateTime", "?endDateTime");

                string blankNodeID = ExecuteScalarSelectQuery(uri, targetCredentials, proxyCredentials, query.getSPARQL(), relationshipType);
                return blankNodeID != String.Empty;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public static SPARQLResults GetTemplateValues(string uri, WebCredentials targetCredentials, WebProxyCredentials proxyCredentials, TemplateMap templateMap, string parentIdentifierVariable)
        {
          try
          {
            SPARQLQuery query = new SPARQLQuery(SPARQLQueryType.SELECT);
            foreach (RoleMap roleMap in templateMap.roleMaps)
            {

              if (roleMap.reference != null && roleMap.reference != String.Empty)
              {
                query.addTemplate(templateMap.templateId, templateMap.classRole, parentIdentifierVariable, roleMap.roleId, roleMap.reference);                
              }
              else if (roleMap.value != null && roleMap.value != String.Empty)
              {
                string value = query.getLITERAL_SPARQL(roleMap.value, roleMap.dataType);
                query.addTemplate(templateMap.templateId, templateMap.classRole, parentIdentifierVariable, roleMap.roleId, value);                
              }
              else
              {
                query.addVariable("?" + roleMap.propertyName);
                query.addTemplate(templateMap.templateId, templateMap.classRole, parentIdentifierVariable, roleMap.roleId, "?" + roleMap.propertyName);
              }              
            }
            query.addTemplate(templateMap.templateId, templateMap.classRole, parentIdentifierVariable, "tpl:endDateTime", "?endDateTime");

            SPARQLResults sparqlResults = SPARQLClient.PostQuery(uri, query.getSPARQL(), targetCredentials, proxyCredentials);
            return sparqlResults;
          }
          catch (Exception exception)
          {
            throw exception;
          }
        }

        public static List<string> GetRelatedClassInstance(string uri, WebCredentials targetCredentials, WebProxyCredentials proxyCredentials, TemplateMap templateMap, RoleMap roleMap, string className)
        {
            List<string> results;
            try
            {
                results = new List<string>();
                SPARQLQuery query = new SPARQLQuery(SPARQLQueryType.SELECT);

                query.addVariable("?i2");
                SPARQLClassification classification = query.addClassification(roleMap.classMap.classId, "?i2");
                query.addTemplate(templateMap.templateId, templateMap.classRole, className, roleMap.roleId, "?i2");
                query.addTemplate(templateMap.templateId, templateMap.classRole, className, "tpl:endDateTime", "?endDateTime");

                /*       
                string relatedQueryClass = prefixes + selectQuery + @"
                                          ?i2 
                                          WHERE 
                                            { 
                                              ?c2  a                       dm:classification                                . 
                                              ?c2  dm:class                " + roleMap.classMap.classId + @"                .
                                              ?c2  dm:instance             ?i2                                              .
                                      
                                              ?t2  a                            " + templateMap.templateId + @"             .
                                              ?t2  " + templateMap.classRole + @" <" + className + @">                     .
                                              ?t2  " + roleMap.roleId + @"   ?i2                                            .
                                            }";
                */

                string result = ExecuteScalarSelectQuery(uri, targetCredentials, proxyCredentials, query.getSPARQL(), relationshipType);
                results.Add(result);
                return results;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
              
        public static string GetPropertyValue(string uri, WebCredentials targetCredentials, WebProxyCredentials proxyCredentials, TemplateMap templateMap, RoleMap roleMap, string className)
        {
            try
            {
                SPARQLQuery query = new SPARQLQuery(SPARQLQueryType.SELECT);

                query.addVariable("?" + roleMap.propertyName);
                query.addTemplate(templateMap.templateId, templateMap.classRole, className, roleMap.roleId, "?" + roleMap.propertyName);

                /*
                  string query = prefixes + selectQuery + @"
                                   ?" + roleMap.propertyName + @"
                                    WHERE 
                                      { 
                                        ?t1  a                         " + templateMap.templateId + @"                  .
                                        ?t1  " + templateMap.classRole + @"         <" + className + @">               . 
                                        ?t1  " + roleMap.roleId + @"                ?" + roleMap.propertyName + @"              .
                                       }";
                 */

                return ExecuteScalarSelectQuery(uri, targetCredentials, proxyCredentials, query.getSPARQL(), propertyType);
            }
            catch (Exception exception)
            {
                throw exception;
            }

        }

        public static string ExecuteScalarSelectQuery(string uri, WebCredentials targetCredentials, WebProxyCredentials proxyCredentials, string query, string relationshipOrProperty)
        {
            try
            {
                string result = String.Empty;

                SPARQLResults sparqlResults = SPARQLClient.PostQuery(uri, query, targetCredentials, proxyCredentials);

                if (sparqlResults.resultsElement.results.Count > 0)
                {
                    if (relationshipOrProperty == propertyType)
                    {
                        result = sparqlResults.resultsElement.results[0].bindings[0].literal.value;
                    }
                    else
                    {
                      //Problem lies here, if system is inserted as 120 for another row then also it returns 1st and terminates it and then insert another value
                        result = sparqlResults.resultsElement.results[0].bindings[0].uri;
                    }
                }
                return result;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public static void ExecuteUpdateQuery(string uri, WebCredentials targetCredentials, WebProxyCredentials proxyCredentials, string query)
        {
            try
            {
                string result = string.Empty;
                MultiPartMessage requestMessage = new MultiPartMessage
                {
                    name = "update",
                    type = MultipartMessageType.FormData,
                    message = query,
                };

                List<MultiPartMessage> requestMessages = new List<MultiPartMessage>
                {
                  requestMessage
                };
                //TODO: Create an overload in SparqlClient.cs and call its method.
                WebHttpClient webHttpClient = new WebHttpClient(uri, targetCredentials.GetNetworkCredential(), proxyCredentials.GetWebProxy());
                webHttpClient.PostMultipartMessage("", requestMessages);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
    }
}
