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
using System.Web;
using org.iringtools.utility;
using SemWeb;
using SemWeb.Util;
using SemWeb.Query;
using org.iringtools.library;

namespace org.iringtools.adapter.semantic
{
  public class SemWebEngine
  {
    private bool _trimData;
    private Store _store = null;
    private Mapping _mapping = null;
    private Dictionary<string, Dictionary<string, string>> _refreshValueLists = null;

    #region Constants
    public string propertyType = "Property";
    public string relationshipType = "Relationship";
    public string endDateTime = "endDateTime";

    public const string rdfPrefix = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    public const string dmPrefix = "http://dm.rdlfacade.org/data#";
    public const string rdlPrefix = "http://rdl.rdlfacade.org/data#";
    public const string tplPrefix = "http://tpl.rdlfacade.org/data#";
    public const string egPrefix = "http://www.example.com/data#";

    public static string rdfType = rdfPrefix + "type";
    public static SemWeb.Entity classificationTemplateType = dmPrefix + "classification";
    public static string classType = dmPrefix + "class";
    public static string instanceType = dmPrefix + "instance";
    public static SemWeb.Entity startDateTimeTemplate = tplPrefix + "startDateTime";
    public static string endDateTimeTemplate = tplPrefix + "endDateTime";
    #endregion

    public SemWebEngine(string storeConnectionString, Mapping mapping, bool trimData)
    {
      _mapping = mapping;
      _trimData = trimData;
      _store = Store.Create(storeConnectionString);      
    }

    public List<string> GetIdentifiersFromTripleStore(string graphName)
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

          GraphMatch query = new GraphMatch();

          SemWeb.Entity classIdEntity = graphMap.classId.Replace("rdl:", rdlPrefix);
          SemWeb.Entity templateIdEntity = identifierTemplateMap.templateId.Replace("tpl:", tplPrefix);
          string templateMapClassRole = identifierTemplateMap.classRole.Replace("tpl:", tplPrefix);
          string roleMapRoleId = identifierRoleMap.roleId.Replace("tpl:", tplPrefix);
          SemWeb.Variable relatedClassificationTemplate = new SemWeb.Variable("i1");
          SemWeb.Variable classificationVariable = new SemWeb.Variable("c1");
          SemWeb.Variable templateVariable = new SemWeb.Variable("t1");
          SemWeb.Variable propertyNameVariable = new SemWeb.Variable(identifierRoleMap.propertyName);

          Statement statementA = new Statement(classificationVariable, rdfType, classificationTemplateType);
          Statement statementB = new Statement(classificationVariable, classType, classIdEntity);
          Statement statementC = new Statement(classificationVariable, instanceType, relatedClassificationTemplate);
          Statement statementD = new Statement(templateVariable, rdfType, templateIdEntity);
          Statement statementE = new Statement(templateVariable, templateMapClassRole, relatedClassificationTemplate);
          Statement statementF = new Statement(templateVariable, roleMapRoleId, propertyNameVariable);

          query.AddGraphStatement(statementA);
          query.AddGraphStatement(statementB);
          query.AddGraphStatement(statementC);
          query.AddGraphStatement(statementD);
          query.AddGraphStatement(statementE);
          query.AddGraphStatement(statementF);

          QueryResultBuffer resultBuffer = GetUnterminatedTemplates(query, templateVariable);

          foreach (VariableBindings binding in resultBuffer.Bindings)
          {
            identifier = ((Literal)(binding[identifierRoleMap.propertyName])).Value;
            identifiers.Add(identifier); 
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

    public void RefreshQuery(DataTransferObject dto)
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
        throw new Exception(String.Format("RefreshQuery[{0}][{1}]", dto.GraphName, dto.Identifier), exception);
      }
    }

    public void RefreshDelete(string graphName, string identifier)
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
        throw new Exception(String.Format("RefreshDelete[{0}][{1}]", graphName, identifier), exception);
      }
    }

    private void RefreshDeleteGraphMap(GraphMap graphMap, string identifier)
        {
          try
          {
            identifier = "eg:id__" + identifier;

            foreach (TemplateMap templateMap in graphMap.templateMaps)
            {
              RefreshDeleteTemplateMap(templateMap, (ClassMap)graphMap, identifier);
            }

            GraphMatch query = new GraphMatch();

            SemWeb.Variable subjectVariable = new SemWeb.Variable("subject");
            SemWeb.Variable predicateVariable = new SemWeb.Variable("predicate");
            SemWeb.Entity classIdEntity = graphMap.classId.Replace("rdl:", rdlPrefix);
            SemWeb.Entity instanceValue = identifier.Replace("eg:", egPrefix);
            SemWeb.Entity instanceTypeEntity = instanceType;
            //LiteralFilter filter = LiteralFilter.Create(LiteralFilter.CompType.NE, instanceTypeEntity);

            Statement statementA = new Statement(subjectVariable, predicateVariable, instanceValue);
            query.AddGraphStatement(statementA);
            
            //query.AddLiteralFilter(predicateVariable, filter);
            QueryResultBuffer resultBuffer = GetUnterminatedTemplates(query, subjectVariable);
                        
              var results = from VariableBindings b in resultBuffer.Bindings
                            where (((SemWeb.Entity)(b["predicate"])).Uri == instanceType)
                            select b;

              int noOfInstancesRelated = resultBuffer.Bindings.Count - results.Count();            

            if (noOfInstancesRelated == 0)
            {
              GraphMatch queryClass = new GraphMatch();                         
              Variable classificationTemplateVariable = new Variable("c1");
              Statement statementB = new Statement(classificationTemplateVariable, rdfType, classificationTemplateType);
              Statement statementC = new Statement(classificationTemplateVariable, classType, classIdEntity);
              Statement statementD = new Statement(classificationTemplateVariable, instanceType, instanceValue);

              queryClass.AddGraphStatement(statementB);
              queryClass.AddGraphStatement(statementC);
              queryClass.AddGraphStatement(statementD);

              QueryResultBuffer resultBufferUnterminatedClass = GetUnterminatedTemplates(queryClass, classificationTemplateVariable);
              VariableBindings variableBindingsToBeTerminated = (VariableBindings)(resultBufferUnterminatedClass.Bindings[0]);
              BNode bNodeToBeTerminated = (BNode)(variableBindingsToBeTerminated["c1"]);
              Literal endTimeValue = GetPropertyValueType(DateTime.UtcNow.ToString(), "datetime");
              Statement statement1 = new Statement(bNodeToBeTerminated, endDateTimeTemplate, endTimeValue);
              _store.Add(statement1);
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
              QueryResultBuffer resultBuffer = GetTemplateValues(templateMap, parentIdentifierVariable);
              BNode bNodeToBeTerminated = null;
              foreach (VariableBindings variableBinding in resultBuffer.Bindings)
              {
                bNodeToBeTerminated = ((BNode)variableBinding["t1"]);
              }
              Literal endTimeValue = GetPropertyValueType(DateTime.UtcNow.ToString(), "datetime");
              Statement statement1 = new Statement(bNodeToBeTerminated, endDateTimeTemplate, endTimeValue);
              _store.Add(statement1);
            }
            else
            {
              RoleMap roleMap = templateMap.roleMaps[0];
              string instanceVariable = GetRelatedClassInstance(templateMap, roleMap, parentIdentifierVariable)[0];
              
              QueryResultBuffer resultBuffer = GetRelatedClass(templateMap, roleMap, parentIdentifierVariable);
              BNode bNodeToBeTerminated = null;
              foreach (VariableBindings variableBinding in resultBuffer.Bindings)
              {
                bNodeToBeTerminated = ((BNode)variableBinding["t1"]);                
              }
              Literal endTimeValue = GetPropertyValueType(DateTime.UtcNow.ToString(), "datetime");
              Statement statement1 = new Statement(bNodeToBeTerminated, endDateTimeTemplate, endTimeValue);
              _store.Add(statement1);
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
            GraphMatch query = new GraphMatch();
            Variable subjectVariable = new Variable("subject");
            Variable predicateVariable = new Variable("predicate");
            SemWeb.Entity classIdEntity = classMap.classId.Replace("rdl:", rdlPrefix);
            SemWeb.Entity instanceTypeEntity = instanceType;
            SemWeb.Entity instanceValue = parentIdentifierVariable.Replace("eg:", egPrefix);
            //LiteralFilter filter = LiteralFilter.Create(LiteralFilter.CompType.NE, instanceTypeEntity);

            Statement statementA = new Statement(subjectVariable, predicateVariable, instanceValue);
            query.AddGraphStatement(statementA);
            
            //query.AddLiteralFilter(predicateVariable, filter);
            QueryResultBuffer resultBuffer = GetUnterminatedTemplates(query, subjectVariable);
            
            var results = from VariableBindings b in resultBuffer.Bindings
                          where (((SemWeb.Entity)(b["predicate"])).Uri == instanceType)
                            select b;

            int noOfInstancesRelated = resultBuffer.Bindings.Count - results.Count();
            
            if (noOfInstancesRelated == 1)
            {
              GraphMatch queryClass = new GraphMatch();

              SemWeb.Variable classificationTemplateVariable = new Variable("c1");

              Statement statementB = new Statement(classificationTemplateVariable, rdfType, classificationTemplateType);
              Statement statementC = new Statement(classificationTemplateVariable, classType, classIdEntity);
              Statement statementD = new Statement(classificationTemplateVariable, instanceType, instanceValue);

              queryClass.AddGraphStatement(statementB);
              queryClass.AddGraphStatement(statementC);
              queryClass.AddGraphStatement(statementD);

              QueryResultBuffer resultBufferUnterminatedClass = GetUnterminatedTemplates(queryClass, classificationTemplateVariable);
              VariableBindings variableBindingsToBeTerminated = (VariableBindings)(resultBufferUnterminatedClass.Bindings[0]);
              BNode bNodeToBeTerminated = (BNode)(variableBindingsToBeTerminated["c1"]);
              Literal endTimeValue = GetPropertyValueType(DateTime.UtcNow.ToString(), "datetime");
              Statement statement1 = new Statement(bNodeToBeTerminated, endDateTimeTemplate, endTimeValue);
              _store.Add(statement1);
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
        RefreshGraphClassName(graphMap.classId, identifier);

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
          QueryResultBuffer resultBufferTemplateValues = GetTemplateValues(templateMap, parentIdentifierVariable);
          #region If property exists already
          if (resultBufferTemplateValues.Bindings.Count > 0)
          {
            foreach (VariableBindings binding in resultBufferTemplateValues.Bindings)
            {
              bool isPropertyValueDifferent = false;

              #region Check if Current property Value differs from New Value
              foreach (Variable variable in binding.Variables)
              {
                if(variable.LocalName != "t1")
                {
                string propertyName = variable.LocalName;
                string propertyValue = string.Empty;
                string propertyType = string.Empty;
                string dtoPropertyValue = string.Empty;
                string curPropertyValue = string.Empty;

                if ((binding[variable.LocalName] is Literal) && (((Literal)(binding[variable.LocalName])).Value != null))
                {
                  curPropertyValue = ((Literal)(binding[variable.LocalName])).Value;
                  propertyType = "literal";
                }
                else if (binding[variable.LocalName].Uri != null)
                {
                  curPropertyValue = binding[variable.LocalName].Uri;
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

                if (roleMap.valueList == null || roleMap.valueList == string.Empty)
                {
                  propertyValue = dtoPropertyValue;
                }
                else
                {
                  Dictionary<string, string> valueList = GetRefreshValueMap(roleMap.valueList);
                  if (valueList.ContainsKey(dtoPropertyValue))
                  {
                    propertyValue = valueList[dtoPropertyValue].Replace("rdl:", rdlPrefix);
                  }
                  else
                  {
                    throw (new Exception(String.Format("valueList[{0}] value[{1}] isn't defined", roleMap.valueList, dtoPropertyValue)));
                  }
                }

                if (!curPropertyValue.Equals(propertyValue))
                {
                  isPropertyValueDifferent = true;
                }
              }

              }
              #endregion

              if (isPropertyValueDifferent)
              {
                GraphMatch queryTemporal = new GraphMatch();
                GraphMatch query = new GraphMatch();
                Variable templateVariable = new Variable("t1");
                BNode bNodeTemplate = new BNode("t1");
                SemWeb.Entity templateTypeEntity = templateMap.templateId.Replace("tpl:", tplPrefix);
                SemWeb.Entity parentIdentifierVariableEntity = parentIdentifierVariable.Replace("eg:", egPrefix);
                string templateMapClassRole = templateMap.classRole.Replace("tpl:", tplPrefix);

                Statement statementA = new Statement(templateVariable, rdfType, templateTypeEntity);
                Statement statementB = new Statement(templateVariable, templateMapClassRole, parentIdentifierVariableEntity);
                
                queryTemporal.AddGraphStatement(statementA);
                queryTemporal.AddGraphStatement(statementB);                

                StatementList statementListToBeAddedToStore = new StatementList();
                foreach (Variable variable in binding.Variables)
                {
                  if(variable.LocalName != "t1")
                  {
                  string propertyName = variable.LocalName;
                  string propertyValue = string.Empty;
                  string propertyType = string.Empty;
                  string dtoPropertyValue = string.Empty;
               
                  if ((binding[variable.LocalName] is Literal) && (((Literal)(binding[variable.LocalName])).Value != null))
                  {                    
                    propertyType = "literal";
                  }
                  else if (binding[variable.LocalName].Uri != null)
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

                  if (roleMap.valueList == null || roleMap.valueList == string.Empty)
                  {
                    propertyValue = dtoPropertyValue;
                  }
                  else
                  {
                    Dictionary<string, string> valueList = GetRefreshValueMap(roleMap.valueList);
                    if (valueList.ContainsKey(dtoPropertyValue))
                    {
                      propertyValue = valueList[dtoPropertyValue].Replace("rdl:", rdlPrefix);
                    }
                    else
                    {
                      throw (new Exception(String.Format("valueList[{0}] value[{1}] isn't defined", roleMap.valueList, dtoPropertyValue)));
                    }
                  }
                  string roleMapRoleId = roleMap.roleId.Replace("tpl:", tplPrefix);
                  Variable propertyNameVariable = new Variable(roleMap.propertyName);
                  Statement statementC = new Statement(templateVariable, roleMapRoleId, propertyNameVariable);
                  queryTemporal.AddGraphStatement(statementC);

                  if (propertyType == "literal")
                  {
                    Literal propertyValueLiteral = GetPropertyValueType(propertyValue, roleMap.dataType);
                    Statement statement = new Statement(bNodeTemplate, roleMapRoleId, propertyValueLiteral);
                    statementListToBeAddedToStore.Add(statement);
                  }
                  else
                  {
                    SemWeb.Entity propertyValueEntity = propertyValue;
                    Statement statement = new Statement(bNodeTemplate, roleMapRoleId, propertyValueEntity);
                    statementListToBeAddedToStore.Add(statement);
                  }                  
                }
                }               

                QueryResultBuffer resultBuffer = GetUnterminatedTemplates(queryTemporal, templateVariable);

                BNode bNodeToBeTerminated = null;
                foreach (VariableBindings variableBinding in resultBuffer.Bindings)
                {
                  bNodeToBeTerminated = ((BNode)variableBinding["t1"]);
                }
                Literal endTimeValue = GetPropertyValueType(DateTime.UtcNow.ToString(), "datetime");
                Statement statement1 = new Statement(bNodeToBeTerminated, endDateTimeTemplate, endTimeValue);
                _store.Add(statement1);

                Statement statement2 = new Statement(bNodeTemplate, rdfType, templateTypeEntity);
                Statement statement3 = new Statement(bNodeTemplate, templateMapClassRole, parentIdentifierVariableEntity);
                _store.Add(statement2);
                _store.Add(statement3);

                foreach (Statement statement in statementListToBeAddedToStore)
                {
                  _store.Add(statement);
                }

                Literal startTimeValue = GetPropertyValueType(DateTime.UtcNow.ToString(), "datetime");
                Statement statement4 = new Statement(bNodeTemplate, startDateTimeTemplate, startTimeValue);
                _store.Add(statement4);
               }
            }
          }
          #endregion
          #region Else if property doesn't exist
          else
          {
            BNode bNodeTemplate = new BNode("t1");
            SemWeb.Entity templateTypeEntity = templateMap.templateId.Replace("tpl:", tplPrefix);
            SemWeb.Entity parentIdentifierVariableEntity = parentIdentifierVariable.Replace("eg:", egPrefix);
            string templateMapClassRole = templateMap.classRole.Replace("tpl:", tplPrefix);
            
            Statement statement1 = new Statement(bNodeTemplate, rdfType, templateTypeEntity);
            Statement statement2 = new Statement(bNodeTemplate, templateMapClassRole, parentIdentifierVariableEntity);

            _store.Add(statement1);
            _store.Add(statement2);

            foreach (RoleMap roleMap in templateMap.roleMaps)
            {
              string propertyValue = string.Empty;
              string propertyType = "literal";
              string dtoPropertyValue = string.Empty;
              string curPropertyValue = string.Empty;

              object obj = dto.GetPropertyValue(roleMap.propertyName);
              if (obj != null)
                if (_trimData)
                  dtoPropertyValue = obj.ToString().Trim();
                else
                  dtoPropertyValue = obj.ToString();
              else
                dtoPropertyValue = string.Empty;

              if (roleMap.valueList == null || roleMap.valueList == string.Empty)
              {
                propertyValue = dtoPropertyValue;
                propertyType = "literal";
              }
              else
              {
                Dictionary<string, string> valueList = GetRefreshValueMap(roleMap.valueList);
                if (valueList.ContainsKey(dtoPropertyValue))
                {
                  propertyValue = valueList[dtoPropertyValue].Replace("rdl:", rdlPrefix);
                  propertyType = "uri";
                }
                else
                {
                  throw (new Exception(String.Format("valueList[{0}] value[{1}] isn't defined", roleMap.valueList, dtoPropertyValue)));
                }
              }
              string roleMapRoleId = roleMap.roleId.Replace("tpl:", tplPrefix);
              if (propertyType == "literal")
              {
                Literal propertyValueLiteral = GetPropertyValueType(propertyValue, roleMap.dataType);
                Statement statement3 = new Statement(bNodeTemplate, roleMapRoleId, propertyValueLiteral);
                _store.Add(statement3);
              }
              else
              {
                SemWeb.Entity propertyValueEntity = propertyValue;
                Statement statement3 = new Statement(bNodeTemplate, roleMapRoleId, propertyValueEntity);
                _store.Add(statement3);
              }                    
            }
            Literal startTimeValue = GetPropertyValueType(DateTime.UtcNow.ToString(), "datetime");
            Statement statement4 = new Statement(bNodeTemplate, startDateTimeTemplate, startTimeValue);
            _store.Add(statement4);
          }
          #endregion
        }
        else
        {
          RoleMap roleMap = templateMap.roleMaps[0];          
          string instanceVariable = RefreshRelatedClass(templateMap, roleMap, roleMap.classMap, parentIdentifierVariable , dto);
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

    private void RefreshGraphClassName(string classId, string identifier)
    {
      try
      {
        if (!TestForClassInstance(classId, identifier))
        {
          SemWeb.Entity classIdEntity = classId.Replace("rdl:", rdlPrefix);
          SemWeb.Entity instanceValue = identifier.Replace("eg:", egPrefix);
          Literal startTimeValue = GetPropertyValueType(DateTime.UtcNow.ToString(), "datetime");

          BNode bNodeClassificationTemplate = new BNode("c1");
          Statement statement1 = new Statement(bNodeClassificationTemplate, rdfType, classificationTemplateType);
          Statement statement2 = new Statement(bNodeClassificationTemplate, classType, classIdEntity);
          Statement statement3 = new Statement(bNodeClassificationTemplate, instanceType, instanceValue);
          Statement statement4 = new Statement(bNodeClassificationTemplate, startDateTimeTemplate, startTimeValue);

          _store.Add(statement1);
          _store.Add(statement2);
          _store.Add(statement3);
          _store.Add(statement4);

        }
      }
      catch (Exception exception)
      {
        throw new Exception(String.Format("RefreshGraphClassName[{0}][{1}]", classId, identifier), exception);
      }
    }

    private string RefreshRelatedClass(TemplateMap templateMap, RoleMap roleMap, ClassMap classMap, string className, DataTransferObject dto)
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
        RefreshGraphClassName(classMap.classId, identifier);
        string relatedClassName = string.Empty;
        List<string> results = GetRelatedClassInstance(templateMap, roleMap, className);
        if (results.Count > 0)
          relatedClassName = results[0];

        if (relatedClassName != string.Empty && identifier != relatedClassName)
        {
          QueryResultBuffer resultBuffer = GetRelatedClass(templateMap, roleMap, className);

          BNode bNodeToBeTerminated = null;
          foreach (VariableBindings variableBinding in resultBuffer.Bindings)
          {
            bNodeToBeTerminated = ((BNode)variableBinding["t1"]);
          }
          Literal endTimeValue = GetPropertyValueType(DateTime.UtcNow.ToString(), "datetime");
          Statement statement1 = new Statement(bNodeToBeTerminated, endDateTimeTemplate, endTimeValue);
          _store.Add(statement1);
        }

        if (relatedClassName == string.Empty || identifier != relatedClassName)
        {
          BNode bNodeTemplate = new BNode("t1");
          SemWeb.Entity templateTypeEntity = templateMap.templateId.Replace("tpl:", tplPrefix);
          string templateMapClassRole = templateMap.classRole.Replace("tpl:", tplPrefix);
          string roleMapRoleId = roleMap.roleId.Replace("tpl:", tplPrefix);
          SemWeb.Entity classNameEntity = className.Replace("eg:", egPrefix);
          SemWeb.Entity identifierEntity = identifier.Replace("eg:", egPrefix);
          Literal startTimeValue = GetPropertyValueType(DateTime.UtcNow.ToString(), "datetime");

          Statement statement1 = new Statement(bNodeTemplate, rdfType, templateTypeEntity);          
          Statement statement2 = new Statement(bNodeTemplate, templateMapClassRole, classNameEntity);          
          Statement statement3 = new Statement(bNodeTemplate, roleMapRoleId, identifierEntity);          
          Statement statement4 = new Statement(bNodeTemplate, startDateTimeTemplate, startTimeValue);
          
          _store.Add(statement1);
          _store.Add(statement2);
          _store.Add(statement3);
          _store.Add(statement4);
          relatedClassName = identifier;
        }
        return relatedClassName;

      }
      catch (Exception exception)
      {
        throw new Exception(String.Format("RefreshRelatedClass[{0}][{1}][{2}][{3}]", templateMap.name, roleMap.name, classMap.name, className), exception);
      }
    }

    private QueryResultBuffer GetTemplateValues(TemplateMap templateMap, string parentIdentifierVariable)
    {
      try
      {
        GraphMatch query = new GraphMatch();

        SemWeb.Entity templateIdEntity = templateMap.templateId.Replace("tpl:", tplPrefix);
        SemWeb.Entity parentIdentifierVariableEntity = parentIdentifierVariable.Replace("eg:", egPrefix);
        SemWeb.Variable templateVariable = new SemWeb.Variable("t1");
        string templateMapClassRole = templateMap.classRole.Replace("tpl:", tplPrefix);

        Statement statementA = new Statement(templateVariable, rdfType, templateIdEntity);        
        Statement statementB = new Statement(templateVariable, templateMapClassRole, parentIdentifierVariableEntity);

        query.AddGraphStatement(statementA);
        query.AddGraphStatement(statementB);

        foreach (RoleMap roleMap in templateMap.roleMaps)
        {
          SemWeb.Variable propertyTemplate = new SemWeb.Variable(roleMap.propertyName);
          string roleMapRoleId = roleMap.roleId.Replace("tpl:", tplPrefix);
          Statement statementC = new Statement(templateVariable, roleMapRoleId, propertyTemplate);
          query.AddGraphStatement(statementC);
        }
       
        QueryResultBuffer resultBuffer = GetUnterminatedTemplates(query, templateVariable);
        return resultBuffer;
      }
      catch (Exception exception)
      {
        throw new Exception(String.Format("GetTemplateValues[{0}][{1}]", templateMap.name, parentIdentifierVariable), exception);
      }
    }

    private bool TestForClassInstance(string classId, string identifier)
    {
      try
      {
        GraphMatch query = new GraphMatch();

        SemWeb.Entity classIdEntity = classId.Replace("rdl:", rdlPrefix);
        SemWeb.Entity instanceValue = identifier.Replace("eg:", egPrefix);
        SemWeb.Variable classificationTemplate = new SemWeb.Variable("c1");
        
        Statement statementA = new Statement(classificationTemplate, rdfType, classificationTemplateType);        
        Statement statementB = new Statement(classificationTemplate, classType, classIdEntity);        
        Statement statementC = new Statement(classificationTemplate, instanceType, instanceValue);

        query.AddGraphStatement(statementA);
        query.AddGraphStatement(statementB);
        query.AddGraphStatement(statementC);
        
        QueryResultBuffer resultBuffer = GetUnterminatedTemplates(query, classificationTemplate);
        if (resultBuffer.Bindings.Count <= 0)
        {
          return false;
        }
        else
        {
          return true;
        }

      }
      catch (Exception exception)
      {
        throw new Exception(String.Format("TestForClassInstance[{0}][{1}]", classId, identifier), exception);
      }
    }

    private List<string> GetRelatedClassInstance(TemplateMap templateMap, RoleMap roleMap, string className)
    {
      List<string> results;
      try
      {
        results = new List<string>();
        QueryResultBuffer resultBuffer = GetRelatedClass(templateMap, roleMap, className);

        foreach (VariableBindings variableBindings in resultBuffer.Bindings)
        {
          SemWeb.Entity entity = (SemWeb.Entity)variableBindings["i1"];
          results.Add(entity.Uri.Replace(egPrefix, "eg:"));                        
        }
        return results;
      }
      catch (Exception exception)
      {
        throw new Exception(String.Format("GetRelatedClassInstance[{0}][{1}][{2}]", templateMap.name, roleMap.name, className), exception);
      }
    }

    private QueryResultBuffer GetRelatedClass(TemplateMap templateMap, RoleMap roleMap, string className)
    {
      try
      {
        GraphMatch query = new GraphMatch();

        SemWeb.Entity templateIdEntity = templateMap.templateId.Replace("tpl:", tplPrefix);
        SemWeb.Entity classNameEntity = className.Replace("eg:", egPrefix);
        string templateMapClassRole = templateMap.classRole.Replace("tpl:", tplPrefix);
        string roleMapRoleId = roleMap.roleId.Replace("tpl:", tplPrefix);
        SemWeb.Variable relatedClassificationTemplate = new SemWeb.Variable("i1");
        SemWeb.Variable templateVariable = new SemWeb.Variable("t1");

        Statement statementD = new Statement(templateVariable, rdfType, templateIdEntity);
        Statement statementE = new Statement(templateVariable, templateMapClassRole, classNameEntity);
        Statement statementF = new Statement(templateVariable, roleMapRoleId, relatedClassificationTemplate);

        query.AddGraphStatement(statementD);
        query.AddGraphStatement(statementE);
        query.AddGraphStatement(statementF);

        QueryResultBuffer resultBuffer = GetUnterminatedTemplates(query, templateVariable);
        return resultBuffer;
      }
      catch (Exception exception)
      {
        throw new Exception(String.Format("GetRelatedClass[{0}][{1}][{2}]", templateMap.name, roleMap.name, className), exception);
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

    private Literal GetPropertyValueType(Object value, String type)
    {
      try
      {
        Literal literal;
        if (type.Equals(string.Empty))
          type = "String";

        if (!type.Contains(':'))
          type = "http://www.w3.org/2001/XMLSchema#" + type;

        if (type.Contains("xsd:"))
         type = type.Replace("xsd:", "http://www.w3.org/2001/XMLSchema#");

        string str = string.Empty;
        if (value == null)
          str = "";
        else
          str = value.ToString();
       return literal = new Literal(str, null, type);
        //return String.Format("\"{0}\"^^{1}", str, type);
      }
      catch (Exception exception)
      {
        throw new Exception("Error in getting property value and type for " + value.ToString() + " and type " + type + ".", exception);
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

    private QueryResultBuffer GetUnterminatedTemplates(GraphMatch query, BNode bNodeTemplate)
    {
      try
      {
        QueryResultBuffer resultBuffer = new QueryResultBuffer();
        query.Run(_store, resultBuffer);

        SemWeb.Variable endDateTimeVariable = new SemWeb.Variable(endDateTime);
        Statement statementA = new Statement(bNodeTemplate, endDateTimeTemplate, endDateTimeVariable);
        query.AddGraphStatement(statementA);

        QueryResultBuffer resultBufferWithEndDateTime = new QueryResultBuffer();
        query.Run(_store, resultBufferWithEndDateTime);

        foreach (VariableBindings binding in resultBufferWithEndDateTime.Bindings)
        {
          var results =
            from VariableBindings b in resultBuffer.Bindings
            where (((BNode)(b[bNodeTemplate.LocalName])) == ((BNode)(binding[bNodeTemplate.LocalName])))
            select b;

          VariableBindings vb = results.FirstOrDefault();
          if (vb != null)
            resultBuffer.Bindings.Remove(vb);
        }
        return resultBuffer;
      }
      catch (Exception exception)
      {
        throw new Exception(String.Format("GetUnterminatedTemplates"), exception);
      }
    }

    public void DumpStoreData(string xmlPath)
    {
      using (RdfWriter writer = new RdfXmlWriter(xmlPath + "rdf.xml"))
      {
        writer.Write(_store);
      }

    }

    public void ClearStore()
    {
      _store.Clear();
      
    }
  }
}
