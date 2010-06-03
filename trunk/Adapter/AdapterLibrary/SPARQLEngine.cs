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


namespace org.iringtools.adapter.semantic
{
  public class SPARQLEngine : ISemanticLayer
    {
        protected WebProxyCredentials _proxyCredentials = null;
        protected WebCredentials _targetCredentials = null;
        protected string _targetUri = String.Empty;
        protected Mapping _mapping = null;
        protected IDTOLayer _dtoService = null;
        protected bool _trimData;
        protected string _graphName = string.Empty;
        protected string _identifierClassName = string.Empty;
        protected Dictionary<string, DataTransferObject> _dtoList = null;
        protected Dictionary<string, Dictionary<string, string>> _pullValueLists = null;
        protected Dictionary<string, Dictionary<string, string>> _refreshValueLists = null;
        protected int _instanceCounter = 0;
        

        [Inject]
        public SPARQLEngine(AdapterSettings settings, IDTOLayer dtoService)
        {
          _mapping = settings.Mapping;
          _proxyCredentials = settings.ProxyCredentials;
          _targetCredentials = settings.TargetCredentials;
          _targetUri = settings.InterfaceServer;
          _trimData = settings.TrimData;  
          _dtoService = dtoService;

        }

        public virtual void Initialize()
        {
          //nothing to do here.
        }

        public virtual List<string> GetIdentifiers(string graphName)
        {
            try
            {
                this._graphName = graphName;
                GraphMap graphMap = new GraphMap();
                List<string> identifiers = new List<string>();
                bool isIdentifierMapped = false;
                TemplateMap identifierTemplateMap = null;
                RoleMap identifierRoleMap = null;
                ClassMap identifierClass = null;
                string classIdentifier = string.Empty;

                foreach (GraphMap mappingGraphMap in _mapping.graphMaps)
                {
                    if (mappingGraphMap.name == graphName)
                    {
                        graphMap = mappingGraphMap;
                    }
                }

                foreach (var keyValuePair in graphMap.classTemplateListMaps)
                {
                    foreach (TemplateMap templateMap in keyValuePair.Value)
                    {
                        foreach (RoleMap roleMap in templateMap.roleMaps)
                        {
                            if (keyValuePair.Key.identifiers.Contains(roleMap.propertyName))
                            {
                                classIdentifier = keyValuePair.Key.classId;
                                identifierTemplateMap = templateMap;
                                identifierRoleMap = roleMap;
                                isIdentifierMapped = true;
                                break;
                            }
                        }
                        if (isIdentifierMapped) break;
                    }
                }

                if (isIdentifierMapped)
                {
                    string identifier = String.Empty;
                    string identifierUri = String.Empty;
                    string identifierVariable = String.Empty;

                    SPARQLQuery identifierQuery = new SPARQLQuery(SPARQLQueryType.SELECTDISTINCT);

                    identifierQuery.addVariable("?" + identifierRoleMap.propertyName);
                    //identifierQuery.addVariable("?i");

                    SPARQLClassification classification = identifierQuery.addClassification(classIdentifier, "?i");
                    //identifierQuery.addTemplate(identifierTemplateMap.templateId, identifierTemplateMap.classRole, "?i", identifierRoleMap.roleId, "?" + identifierRoleMap.propertyName);

                    SPARQLTemplate identifierTemplate = new SPARQLTemplate();
                    identifierTemplate.TemplateName = identifierTemplateMap.templateId;
                    identifierTemplate.ClassRole = identifierTemplateMap.roleMaps.Select(c => c.type == RoleType.ClassRole).ToString();
                    identifierTemplate.ClassId = "?i";

                    foreach (RoleMap roleMap in identifierTemplateMap.roleMaps)
                    {
                      if (roleMap.type == RoleType.Reference)
                      {
                        identifierTemplate.addRole(roleMap.roleId, roleMap.dataType);
                      }
                      else if (roleMap.value != null && roleMap.value != String.Empty)
                      {
                        string value = identifierQuery.getLITERAL_SPARQL(roleMap.value, roleMap.dataType);
                        identifierTemplate.addRole(roleMap.roleId, value);
                      }
                      else
                      {
                        identifierVariable = roleMap.propertyName;
                        identifierQuery.addVariable("?" + identifierVariable);
                        identifierTemplate.addRole(roleMap.roleId, "?" + identifierVariable);
                      }
                    }
                    identifierTemplate.addRole("p7tpl:valEndTime", "?endDateTime");
                    identifierQuery.addTemplate(identifierTemplate);

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

        public virtual List<DataTransferObject> Get(string graphName)
        {
          try
          {
              this._graphName = graphName;
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

        public virtual Response Post(string graphName, List<DataTransferObject> dtoList)
        {
          string identifier = String.Empty;
          Response response = new Response();

          try
          {
              List<string> tripleStoreIdentifiers = this.GetIdentifiers(graphName);
              List<string> identifiersToBeDeleted = tripleStoreIdentifiers;
              foreach (DataTransferObject commonDTO in dtoList)
              {
                  if (tripleStoreIdentifiers.Contains(commonDTO.Identifier))
                  {
                      identifiersToBeDeleted.Remove(commonDTO.Identifier);
                  }
              }
              if (identifiersToBeDeleted.Count > 0)
                  response.Append(this.Delete(graphName, identifiersToBeDeleted));

            var graphMaps = from map in _mapping.graphMaps
                            where map.name == graphName
                            select map;

            foreach (GraphMap graphMap in graphMaps)
            {
              foreach (DataTransferObject dto in dtoList)
              {
                graphName = dto.GraphName;
                identifier = dto.Identifier;

                DateTime b = DateTime.Now;

                RefreshGraphMap(graphMap, dto);

                DateTime e = DateTime.Now;
                TimeSpan d = e.Subtract(b);

                response.Add(String.Format("Post({0},{1}) Execution Time [{2}:{3}.{4}] Minutes", graphName, identifier, d.Minutes, d.Seconds, d.Milliseconds));
              }
            }
          }
          catch (Exception exception)
          {
            response.Level = StatusLevel.Error;
            response.Add("Error in Post[" + graphName + "][" + identifier + "].");
            response.Add(exception.ToString());
          }

          return response;
        }

        public virtual Response Delete(string graphName, List<string> identifiers)
        {
          Response response = new Response();

          try
          {
            var graphMaps = from map in _mapping.graphMaps
                            where map.name == graphName
                            select map;

            foreach (GraphMap graphMap in graphMaps)
            {
              DateTime b = DateTime.Now;

              foreach (string identifier in identifiers)
              {
                RefreshDeleteGraphMap(graphMap, identifier);
              }

              DateTime e = DateTime.Now;
              TimeSpan d = e.Subtract(b);

              response.Add(String.Format("Delete({0}) Execution Time [{1}:{2}.{3}] Minutes", graphName, d.Minutes, d.Seconds, d.Milliseconds));
            }
          }
          catch (Exception exception)
          {
            response.Level = StatusLevel.Error;
            response.Add("Error in Delete[" + graphName + "].");
            response.Add(exception.ToString());
          }

          return response;
        }

        public virtual Response Post(string graph)
        {
          Response response = new Response();  
          
          //Nothing todo here?

          return response;
        }

        public virtual Response Clear(string graphName)
        {
          Response response = new Response();

          try
          {
            DateTime b = DateTime.Now;

            SPARQLClient.PostQueryAsMultipartMessage(_targetUri, "CLEAR", _targetCredentials, _proxyCredentials);

            DateTime e = DateTime.Now;
            TimeSpan d = e.Subtract(b);

            response.Add(String.Format("Clear() Execution Time [{0}:{1}.{2}] Minutes", d.Minutes, d.Seconds, d.Milliseconds));
          }
          catch (Exception exception)
          {
            response.Level = StatusLevel.Error;
            response.Add("Error in Clear[].");
            response.Add(exception.ToString());
          }

          return response;
        }

        private void QueryGraphMap(GraphMap graphMap)
        {
            try
            {
                List<string> identifiers = GetIdentifiers(graphMap.name);
                TemplateMap identifierTemplateMap = null;
                RoleMap identifierRoleMap = null;
                ClassMap classMap = null;
                string classIdentifier = string.Empty;
 
                foreach (var keyValuePair in graphMap.classTemplateListMaps)
                {
                    foreach (TemplateMap templateMap in keyValuePair.Value)
                    {
                        foreach (RoleMap roleMap in templateMap.roleMaps)
                        {
                            if (keyValuePair.Key.identifiers.Contains(roleMap.propertyName))
                            {
                                classMap = keyValuePair.Key;
                                classIdentifier = keyValuePair.Key.identifiers.Select(c => c == roleMap.propertyName).ToString();
                                identifierTemplateMap = templateMap;
                                identifierRoleMap = roleMap;
                                break;
                            }
                        }

                    }
                }
                SPARQLQuery identifierQuery = new SPARQLQuery(SPARQLQueryType.SELECTDISTINCT);

                identifierQuery.addVariable("?" + identifierRoleMap.propertyName);
                identifierQuery.addVariable("?i");

                //SPARQLClassification classification = identifierQuery.addClassification(graphMap.classId, "?i");
                //identifierQuery.addTemplate(classification.TemplateName, classification.ClassRole, classification.ClassId, "p7tpl:valEndTime", "?endDateTime");
               

                SPARQLClassification classification = new SPARQLClassification();
                classification.ClassId = classMap.classId;
                classification.addRole("p7tpl:R99011248051", "?i");
                classification.addRole("p7tpl:valEndTime", "?endDateTime");

                identifierQuery.addTemplate(classification);

                SPARQLTemplate identifierTemplate = new SPARQLTemplate();
                identifierTemplate.TemplateName = identifierTemplateMap.templateId;
                identifierTemplate.ClassRole = identifierTemplateMap.roleMaps.Select(c => c.type == RoleType.ClassRole).ToString();
                identifierTemplate.ClassId = "?i";
                identifierTemplate.addRole(identifierRoleMap.roleId, "?" + identifierRoleMap.propertyName);
                foreach (RoleMap roleMap in identifierTemplateMap.roleMaps)
                {
                  if (roleMap != identifierRoleMap)
                  {
                    if (roleMap.type == RoleType.Reference)
                    {
                      identifierTemplate.addRole(roleMap.roleId, roleMap.dataType);
                    }
                  }
                }
                identifierTemplate.addRole("p7tpl:valEndTime", "?endDateTime");
                identifierQuery.addTemplate(identifierTemplate);

                foreach (String identifier in identifiers)
                {
                  DataTransferObject dto = _dtoService.GetDTO(graphMap.name, identifier);
                    if (dto == null) dto = _dtoService.Create(graphMap.name, identifier);
                    _dtoList.Add(identifier, dto);
                }

                foreach (var keyValuePair in graphMap.classTemplateListMaps)
                {
                    foreach(TemplateMap templateMap in keyValuePair.Value)
                    QueryTemplateMap(templateMap, keyValuePair.Key, identifierQuery);
                }
            }
            catch (Exception exception)
            {
                throw new Exception(String.Format("QueryGraphMap[{0}]", graphMap.name), exception);
            }
        }



        protected virtual void QueryTemplateMap(TemplateMap templateMap, ClassMap classMap, SPARQLQuery previousQuery)
        {
            try
            {
                SPARQLQuery query = new SPARQLQuery(SPARQLQueryType.SELECTDISTINCT);
                query.Merge(previousQuery);

                string graphIdentifierVariable = query.Variables.First<string>();
                string graphIdentifierVariableName = graphIdentifierVariable.Replace("?", "");
                string parentIdentifierVariable = query.Variables.Last<string>();
                string identifierVariable = String.Empty;

                RoleMap classRoleMap =
                         (from roleMap in templateMap.roleMaps
                          where roleMap.classMap != null
                          select roleMap).FirstOrDefault();

                if (classRoleMap == null)
                {
                    SPARQLTemplate sparqlTemplate = new SPARQLTemplate();
                    sparqlTemplate.TemplateName = templateMap.templateId;
                    sparqlTemplate.ClassRole = templateMap.roleMaps.Select(c => c.type == RoleType.ClassRole).ToString();
                    sparqlTemplate.ClassId = parentIdentifierVariable;

                    foreach (RoleMap roleMap in templateMap.roleMaps)
                    {

                      if (roleMap.type == RoleType.Reference)
                        {
                          sparqlTemplate.addRole(roleMap.roleId, roleMap.dataType);
                        }
                        else if (roleMap.value != null && roleMap.value != String.Empty)
                        {
                          string value = query.getLITERAL_SPARQL(roleMap.value, roleMap.dataType);
                          sparqlTemplate.addRole(roleMap.roleId,  value);
                        }
                        else
                        {
                          identifierVariable = roleMap.propertyName;
                          query.addVariable("?" + identifierVariable);
                          sparqlTemplate.addRole(roleMap.roleId, "?" + identifierVariable);
                        }
                    }

                    sparqlTemplate.addRole("p7tpl:valEndTime", "?endDateTime");
                    query.addTemplate(sparqlTemplate);
                  
                    SPARQLResults sparqlResults = SPARQLClient.PostQuery(_targetUri, query.getSPARQL(), _targetCredentials, _proxyCredentials);

                    foreach (SPARQLResult result in sparqlResults.resultsElement.results)
                    {
                      SPARQLBinding identifierBinding = 
                         (from binding in result.bindings
                          where binding.name == graphIdentifierVariableName
                          select binding).FirstOrDefault();

                      DataTransferObject dto = _dtoList[identifierBinding.literal.value];

                        foreach (SPARQLBinding binding in result.bindings)
                        {
                            
                            string propertyName = binding.name;

                            if (propertyName != graphIdentifierVariableName)
                            {
                                object propertyValue = null;

                                RoleMap roleMap = FindRoleMap(templateMap, propertyName);

                                if (binding.literal != null)
                                {
                                    propertyValue = binding.literal.value;
                                    dto.SetPropertyValueByInternalName(propertyName, propertyValue);
                                }
                                else if (roleMap != null && (roleMap.valueList != "" || roleMap.valueList != null))
                                {
                                    Dictionary<string, string> valueList = GetPullValueMap(roleMap.valueList);

                                    string propertyUri = query.getPREFIX_URI(binding.uri);
                                    propertyValue = valueList[propertyUri];
                                    dto.SetPropertyValueByInternalName(propertyName, propertyValue);
                                }
                            }
                        }
                    }
                }
                else
                {
                    _instanceCounter++;

                    SPARQLTemplate sparqlTemplate = new SPARQLTemplate();
                    sparqlTemplate.TemplateName = templateMap.templateId;
                    sparqlTemplate.ClassRole = templateMap.roleMaps.Select(c => c.type == RoleType.ClassRole).ToString();
                    sparqlTemplate.ClassId = parentIdentifierVariable;

                    string instanceVariable = "?i" + _instanceCounter.ToString();

                    if (classRoleMap.type == RoleType.Reference)
                    {
                      sparqlTemplate.addRole(classRoleMap.roleId, classRoleMap.dataType);
                    }
                    else if (classRoleMap.value != null && classRoleMap.value != String.Empty)
                    {
                      string value = query.getLITERAL_SPARQL(classRoleMap.value, classRoleMap.dataType);
                      sparqlTemplate.addRole(classRoleMap.roleId, value);
                    }
                    else
                    {
                      sparqlTemplate.addRole(classRoleMap.roleId, instanceVariable);
                    }
                    sparqlTemplate.addRole("p7tpl:valEndTime", "?endDateTime");
                    query.addTemplate(sparqlTemplate);

                 //   QueryClassMap(classRoleMap.classMap, classRoleMap, query, instanceVariable);

                    _instanceCounter--;
                }
            }
            catch (Exception exception)
            {
                throw new Exception(String.Format("QueryTemplateMap[{0}]", templateMap.name), exception);
            }
        }

        //internal void QueryClassMap(ClassMap classMap, RoleMap roleMap, SPARQLQuery previousQuery, string instanceVariable)
        //{
        //    try
        //    {
        //        SPARQLQuery query = new SPARQLQuery(SPARQLQueryType.SELECTDISTINCT);
        //        query.Merge(previousQuery);

        //        query.addVariable(instanceVariable);
                
        //        SPARQLClassification classification = new SPARQLClassification();
        //        classification.ClassId = classMap.classId;
        //        classification.addRole("p7tpl:R99011248051", instanceVariable);
        //        classification.addRole("p7tpl:valEndTime", "?endDateTime");

        //        query.addTemplate(classification);

        //        foreach (var keyValuePair in classMap.TemplateMaps)
        //        {
        //            QueryTemplateMap(templateMap, classMap, query);
        //        }

        //    }
        //    catch (Exception exception)
        //    {
        //        throw new Exception(String.Format("QueryClassMap[{0}][{1}]", classMap.name, roleMap.name), exception);
        //    }
        //}

        internal Dictionary<string, string> GetPullValueMap(string valueListName)
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
                            string uri = valueMap.uri;
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
                                valueList.Add(key, valueMap.uri); //First one is the default
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

        internal RoleMap FindRoleMap(TemplateMap templateMap, string propertyName)
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
            ClassMap classMap = null;
            identifier = "eg:id__" + identifier;
            foreach (var keyValuePair in graphMap.classTemplateListMaps)
            {
                foreach (TemplateMap templateMap in keyValuePair.Value)
                {
                    classMap = keyValuePair.Key;
                    RefreshDeleteTemplateMap(templateMap, keyValuePair.Key, identifier);
                }
            }
            SPARQLQuery query = new SPARQLQuery(SPARQLQueryType.SELECTFORDELETE);
            query.addVariable("?subject");
            query.addVariable("?predicate");
            SPARQLClassification classificationForQuery = query.addClassification(classMap.classId, query.getPREFIX_URI(identifier));

            SPARQLResults sparqlResults = SPARQLClient.PostQuery(_targetUri, query.getSPARQL(), _targetCredentials, _proxyCredentials);
            //if not terminate it
            //check the results above, if 0 then terminate it else don't
            if (sparqlResults.resultsElement.results.Count() == 0)
            {
              SPARQLQuery insertTemporalQuery = new SPARQLQuery(SPARQLQueryType.INSERTTEMPORAL);
              SPARQLClassification classification = insertTemporalQuery.addClassification(classMap.classId, query.getPREFIX_URI(identifier));
              string endTimeValue = query.getLITERAL_SPARQL(DateTime.UtcNow.ToString(), "datetime");
              insertTemporalQuery.addTemplate(classification.TemplateName, classification.ClassRole, identifier, "p7tpl:valEndTime", "?endDateTime", endTimeValue);
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
            //if (templateMap.type == TemplateType.Property)
            //{
            //  //terminate the template
            //  SPARQLQuery query = new SPARQLQuery(SPARQLQueryType.INSERTTEMPORAL);
            //  foreach (RoleMap roleMap in templateMap.roleMaps)
            //  {
            //    query.addTemplate(templateMap.templateId, templateMap.classRole, parentIdentifierVariable, roleMap.roleId, "?" + roleMap.propertyName);
            //  }
            //  string endTimeValue = query.getLITERAL_SPARQL(DateTime.UtcNow.ToString(), "datetime");
            //  query.addTemplate(templateMap.templateId, templateMap.classRole, parentIdentifierVariable, "p7tpl:valEndTime", "?endDateTime", endTimeValue);
            //  SPARQLBuilder.ExecuteUpdateQuery(_targetUri, _targetCredentials, _proxyCredentials, query.getSPARQL());
            //}
            //else
            //{
              foreach (RoleMap roleMap in templateMap.roleMaps)
              {
                if (roleMap.classMap != null)
                {
                  string instanceVariable = SPARQLBuilder.GetRelatedClassInstance(_targetUri, _targetCredentials, _proxyCredentials, templateMap, roleMap, parentIdentifierVariable)[0];
                  SPARQLQuery query = new SPARQLQuery(SPARQLQueryType.INSERTTEMPORAL);
                  SPARQLClassification classification = query.addClassification(classMap.classId, query.getPREFIX_URI(parentIdentifierVariable));
                  string endTimeValue = query.getLITERAL_SPARQL(DateTime.UtcNow.ToString(), "datetime");

                //  query.addTemplate(templateMap.templateId, templateMap.classRole, parentIdentifierVariable, roleMap.roleId, query.getPREFIX_URI(instanceVariable));
                //  query.addTemplate(templateMap.templateId, templateMap.classRole, parentIdentifierVariable, "p7tpl:valEndTime", "?endDateTime", endTimeValue);

                  SPARQLBuilder.ExecuteUpdateQuery(_targetUri, _targetCredentials, _proxyCredentials, query.getSPARQL());
                  RefreshDeleteClassMap(roleMap.classMap, roleMap, instanceVariable);

                  break;
                }
              }
            //}
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
              insertTemporalQuery.addTemplate(classification.TemplateName, classification.ClassRole, parentIdentifierVariable, "p7tpl:valEndTime", "?endDateTime", endTimeValue);
              SPARQLBuilder.ExecuteUpdateQuery(_targetUri, _targetCredentials, _proxyCredentials, insertTemporalQuery.getSPARQL());

              //foreach (TemplateMap templateMap in classMap.templateMaps)
              //{
              //  RefreshDeleteTemplateMap(templateMap, classMap, parentIdentifierVariable);
              //}
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
            SPARQLBuilder.RefreshGraphClassName(_targetUri, _targetCredentials, _proxyCredentials, graphMap.classTemplateListMaps.Select(c => c.Key).First().classId , identifier);
            foreach (var keyValuePair in graphMap.classTemplateListMaps)
            {
                foreach (TemplateMap templateMap in keyValuePair.Value)
                {
                    RefreshTemplateMap(templateMap, keyValuePair.Key, dto, identifier);
                }
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
                //if (templateMap.type == TemplateType.Property)
                //{
                //  SPARQLResults sparqlResults = SPARQLBuilder.GetTemplateValues(_targetUri, _targetCredentials, _proxyCredentials, templateMap, parentIdentifierVariable); 
                //  #region If property exists already
                //  if (sparqlResults.resultsElement.results.Count > 0)
                //    {
                //        foreach (SPARQLResult result in sparqlResults.resultsElement.results)
                //        {

                //            SPARQLQuery insertTemporalQuery = new SPARQLQuery(SPARQLQueryType.INSERTTEMPORAL);

                //            SPARQLQuery insertQuery = new SPARQLQuery(SPARQLQueryType.INSERT);

                //            bool isPropertyValueDifferent = false;

                //            #region Check if Current property Value differs from New Value
                //            foreach (SPARQLBinding binding in result.bindings)
                //            {
                //                string propertyName = binding.name;
                //                string propertyValue = string.Empty;
                //                string propertyType = string.Empty;
                //                string dtoPropertyValue = string.Empty;
                //                string curPropertyValue = string.Empty;

                //                if (binding.literal != null && binding.literal.value != null)
                //                {
                //                    curPropertyValue = binding.literal.value;
                //                    propertyType = "literal";
                //                }
                //                else if (binding.uri != null)
                //                {
                //                    curPropertyValue = insertQuery.getPREFIX_URI(binding.uri);
                //                    propertyType = "uri";
                //                }
                //                else
                //                {
                //                    curPropertyValue = string.Empty;
                //                    propertyType = "literal";
                //                }

                //                object obj = dto.GetPropertyValueByInternalName(propertyName);
                //                if (obj != null)
                //                  if (_trimData)
                //                    dtoPropertyValue = obj.ToString().Trim();
                //                  else
                //                    dtoPropertyValue = obj.ToString();
                //                else
                //                    dtoPropertyValue = "";

                //                RoleMap roleMap = FindRoleMap(templateMap, propertyName);

                //                if (roleMap.reference != null && roleMap.reference != String.Empty)
                //                {
                //                  if (roleMap.valueList != null && roleMap.valueList != String.Empty)
                //                  {
                //                    Dictionary<string, string> valueList = GetRefreshValueMap(roleMap.valueList);
                //                    if (valueList.ContainsKey(dtoPropertyValue))
                //                    {
                //                      propertyValue = valueList[dtoPropertyValue];
                //                    }
                //                    else
                //                    {
                //                      throw (new Exception(String.Format("valueList[{0}] value[{1}] isn't defined", roleMap.valueList, dtoPropertyValue)));
                //                    }
                //                  }
                //                  else
                //                  {
                //                    propertyValue = roleMap.reference;
                //                  }
                //                }
                //                else if (roleMap.value != null && roleMap.value != String.Empty)
                //                {
                //                  propertyValue = roleMap.value;
                //                }
                //                else
                //                {
                //                  propertyValue = dtoPropertyValue;
                //                }
                               
                //                if (!curPropertyValue.Equals(propertyValue))
                //                {
                //                    isPropertyValueDifferent = true;
                //                }

                //            }
                //            #endregion

                //            if (isPropertyValueDifferent)
                //            {
                //                foreach (SPARQLBinding binding in result.bindings)
                //                {
                //                    string propertyName = binding.name;
                //                    string propertyValue = string.Empty;
                //                    string propertyType = string.Empty;
                //                    string dtoPropertyValue = string.Empty;

                //                    if (binding.literal != null && binding.literal.value != null)
                //                    {
                //                        propertyType = "literal";
                //                    }
                //                    else if (binding.uri != null)
                //                    {
                //                        propertyType = "uri";
                //                    }
                //                    else
                //                    {
                //                        propertyType = "literal";
                //                    }

                //                    object obj = dto.GetPropertyValueByInternalName(propertyName);
                //                    if (obj != null)
                //                      if (_trimData)
                //                        dtoPropertyValue = obj.ToString().Trim();
                //                      else
                //                        dtoPropertyValue = obj.ToString();
                //                    else
                //                        dtoPropertyValue = "";

                //                    RoleMap roleMap = FindRoleMap(templateMap, propertyName);

                //                    if (roleMap.reference != null && roleMap.reference != String.Empty)
                //                    {
                //                      if (roleMap.valueList != null && roleMap.valueList != String.Empty)
                //                      {
                //                        Dictionary<string, string> valueList = GetRefreshValueMap(roleMap.valueList);
                //                        if (valueList.ContainsKey(dtoPropertyValue))
                //                        {
                //                          propertyValue = valueList[dtoPropertyValue];
                //                        }
                //                        else
                //                        {
                //                          throw (new Exception(String.Format("valueList[{0}] value[{1}] isn't defined", roleMap.valueList, dtoPropertyValue)));
                //                        }
                //                      }
                //                      else
                //                      {
                //                        propertyValue = roleMap.reference;
                //                      }
                //                    }
                //                    else if (roleMap.value != null && roleMap.value != String.Empty)
                //                    {
                //                      propertyValue = roleMap.value;
                //                    }
                //                    else
                //                    {
                //                      propertyValue = dtoPropertyValue;
                //                    }                                    
                //                    if (propertyType == "literal")
                //                        propertyValue = insertQuery.getLITERAL_SPARQL(propertyValue, roleMap.dataType);

                //                    insertTemporalQuery.addTemplate(templateMap.templateId, templateMap.classRole, parentIdentifierVariable, roleMap.roleId, "?" + roleMap.propertyName, propertyValue);
                //                    insertQuery.addTemplate(templateMap.templateId, templateMap.classRole, parentIdentifierVariable, roleMap.roleId, "?" + roleMap.propertyName, propertyValue);
                //                }
                //                string endTimeValue = insertTemporalQuery.getLITERAL_SPARQL(DateTime.UtcNow.ToString(), "datetime");
                //                insertTemporalQuery.addTemplate(templateMap.templateId, templateMap.classRole, parentIdentifierVariable, "p7tpl:valEndTime", "?endDateTime", endTimeValue);
                //                SPARQLBuilder.ExecuteUpdateQuery(_targetUri, _targetCredentials, _proxyCredentials, insertTemporalQuery.getSPARQL());

                //                string startTimeValue = insertQuery.getLITERAL_SPARQL(DateTime.UtcNow.ToString(), "datetime");
                //                insertQuery.addTemplate(templateMap.templateId, templateMap.classRole, parentIdentifierVariable, "p7tpl:valStartTime", "?startDateTime", startTimeValue);
                //                SPARQLBuilder.ExecuteUpdateQuery(_targetUri, _targetCredentials, _proxyCredentials, insertQuery.getSPARQL());
                //            }

                //        }
                //    }
                //    #endregion
                //  #region Else if property doesn't exist
                //    else
                //    {

                //        SPARQLQuery insertQuery = new SPARQLQuery(SPARQLQueryType.INSERT);

                //        foreach (RoleMap roleMap in templateMap.roleMaps)
                //        {
                //            string propertyValue = string.Empty;
                //            string propertyType = "literal";
                //            string dtoPropertyValue = string.Empty;
                //            string curPropertyValue = string.Empty;

                //            object obj = dto.GetPropertyValueByInternalName(roleMap.propertyName);
                //            if (obj != null)
                //              if (_trimData)
                //                dtoPropertyValue = obj.ToString().Trim();
                //              else
                //                dtoPropertyValue = obj.ToString();
                //            else
                //                dtoPropertyValue = string.Empty;

                //            if (roleMap.reference != null && roleMap.reference != String.Empty)
                //            {
                //              if (roleMap.valueList != null && roleMap.valueList != String.Empty)
                //              {
                //                Dictionary<string, string> valueList = GetRefreshValueMap(roleMap.valueList);
                //                if (valueList.ContainsKey(dtoPropertyValue))
                //                {
                //                  propertyValue = valueList[dtoPropertyValue];
                //                  propertyType = "uri";
                //                }
                //                else
                //                {
                //                  throw (new Exception(String.Format("valueList[{0}] value[{1}] isn't defined", roleMap.valueList, dtoPropertyValue)));
                //                }
                //              }
                //              else
                //              {
                //                propertyValue = roleMap.reference;
                //                propertyType = "uri";
                //              }
                //            }
                //            else if (roleMap.value != null && roleMap.value != String.Empty)
                //            {
                //              propertyValue = roleMap.value;
                //              propertyType = "literal";
                //            }
                //            else
                //            {
                //              propertyValue = dtoPropertyValue;
                //              propertyType = "literal";
                //            }
                //            {
                //                if (propertyType == "literal")
                //                    propertyValue = insertQuery.getLITERAL_SPARQL(propertyValue, roleMap.dataType);

                //                insertQuery.addTemplate(templateMap.templateId, templateMap.classRole, parentIdentifierVariable, roleMap.roleId, "?" + roleMap.propertyName, propertyValue);
                //            }

                //        }
                //        string startTimeValue = insertQuery.getLITERAL_SPARQL(DateTime.UtcNow.ToString(), "datetime");
                //        insertQuery.addTemplate(templateMap.templateId, templateMap.classRole, parentIdentifierVariable, "p7tpl:valStartTime", "?startDateTime", startTimeValue);

                //        SPARQLBuilder.ExecuteUpdateQuery(_targetUri, _targetCredentials, _proxyCredentials, insertQuery.getSPARQL());
                //    }
                //    #endregion
                //}
                //else
                //{
                    foreach (RoleMap roleMap in templateMap.roleMaps)
                    {
                        if (roleMap.classMap != null)
                        {
                            string instanceVariable = SPARQLBuilder.RefreshRelatedClass(_targetUri, _targetCredentials, _proxyCredentials, templateMap, roleMap, roleMap.classMap, parentIdentifierVariable, dto);
                            RefreshClassMap(roleMap.classMap, roleMap, dto, instanceVariable);
                        }
                    }
                }
            //}
            catch (Exception exception)
            {
              throw new Exception(String.Format("RefreshTemplateMap[{0}][{1}][{2}]", templateMap.name, classMap.name, parentIdentifierVariable), exception);
            }
        }
              
        private void RefreshClassMap(ClassMap classMap, RoleMap currentRoleMap, DataTransferObject dto, string parentIdentifierVariable)
        {
          try
          {
          //  foreach (TemplateMap templateMap in classMap.templateMaps)
          //  {
           //   RefreshTemplateMap(templateMap, classMap, dto, parentIdentifierVariable);
          //  }
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
                    query.addTemplate(classification.TemplateName, classification.ClassRole, classification.ClassId, "p7tpl:valStartTime", startTimeValue);

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

                if (classMap.identifiers != null)
                {
                    //string[] identifiers = classMap.identifier.Split(',');

                    
                    foreach (string identifierPart in classMap.identifiers)
                    {                      
                        object identifierPartValue = dto.GetPropertyValueByInternalName(identifierPart.Trim());

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
                    query.addTemplate(templateMap.templateId, templateMap.roleMaps.Select(c => c.type == RoleType.ClassRole).ToString(), className, roleMap.roleId, identifier);
                    string startTimeValue = query.getLITERAL_SPARQL(DateTime.UtcNow.ToString(), "datetime");
                    query.addTemplate(templateMap.templateId, templateMap.roleMaps.Select(c => c.type == RoleType.ClassRole).ToString(), className, "p7tpl:valStartTime", "?startDateTime", startTimeValue);
                    
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
                SPARQLQuery query = new SPARQLQuery(SPARQLQueryType.SELECTDISTINCT);

                SPARQLClassification classification = query.addClassification(classId, identifier);
                query.addVariable(classification.getNode());
                query.addTemplate(classification.TemplateName, classification.ClassRole, classification.ClassId, "p7tpl:valEndTime", "?endDateTime");

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
            SPARQLQuery query = new SPARQLQuery(SPARQLQueryType.SELECTDISTINCT);
            foreach (RoleMap roleMap in templateMap.roleMaps)
            {

              if (roleMap.type == RoleType.Reference)
              {
                query.addTemplate(templateMap.templateId, templateMap.roleMaps.Select(c => c.type == RoleType.ClassRole).ToString(), parentIdentifierVariable, roleMap.roleId, roleMap.dataType);                
              }
              else if (roleMap.value != null && roleMap.value != String.Empty)
              {
                string value = query.getLITERAL_SPARQL(roleMap.value, roleMap.dataType);
                query.addTemplate(templateMap.templateId, templateMap.roleMaps.Select(c => c.type == RoleType.ClassRole).ToString(), parentIdentifierVariable, roleMap.roleId, value);                
              }
              else
              {
                query.addVariable("?" + roleMap.propertyName);
                query.addTemplate(templateMap.templateId, templateMap.roleMaps.Select(c => c.type == RoleType.ClassRole).ToString(), parentIdentifierVariable, roleMap.roleId, "?" + roleMap.propertyName);
              }              
            }
            query.addTemplate(templateMap.templateId, templateMap.roleMaps.Select(c => c.type == RoleType.ClassRole).ToString(), parentIdentifierVariable, "p7tpl:valEndTime", "?endDateTime");

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
                SPARQLQuery query = new SPARQLQuery(SPARQLQueryType.SELECTDISTINCT);

                query.addVariable("?i2");
                SPARQLClassification classification = query.addClassification(roleMap.classMap.classId, "?i2");
                query.addTemplate(templateMap.templateId, templateMap.roleMaps.Select(c => c.type == RoleType.ClassRole).ToString(), className, roleMap.roleId, "?i2");
                query.addTemplate(templateMap.templateId, templateMap.roleMaps.Select(c => c.type == RoleType.ClassRole).ToString(), className, "p7tpl:valEndTime", "?endDateTime");

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
                SPARQLQuery query = new SPARQLQuery(SPARQLQueryType.SELECTDISTINCT);

                query.addVariable("?" + roleMap.propertyName);
                query.addTemplate(templateMap.templateId, templateMap.roleMaps.Select(c => c.type == RoleType.ClassRole).ToString(), className, roleMap.roleId, "?" + roleMap.propertyName);

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
