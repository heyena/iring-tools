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
using System.Text;
using System.CodeDom.Compiler;
using System.Reflection;
using System.IO;
using System.Xml;
using Microsoft.CSharp;
using log4net;
using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.adapter;
using Ingr.SP3D.Common.Middle.Services;
using Ingr.SP3D.Common.Middle;

namespace iringtools.sdk.sp3ddatalayer
{
  public class EntityGenerator
  {
    private List<string> NHIBERNATE_ASSEMBLIES = new List<string>() 
    {
      "NHibernate.dll",     
      "NHibernate.ByteCode.Castle.dll",
      "Iesi.Collections.dll",
    };

    private static readonly ILog _logger = LogManager.GetLogger(typeof(EntityGenerator));

    private AdapterSettings _settings = null;
    private StringBuilder _mappingBuilder = null;
    private XmlTextWriter _mappingWriter = null;
    private List<DataObject> _dataObjects = null;
    private List<BusinessCommodity> _businessCommodities = null;
    private IndentedTextWriter _dataObjectWriter = null;
    private StringBuilder _dataObjectBuilder = null;

    public EntityGenerator(AdapterSettings settings)
    {
      _settings = settings;
    }

    public Response GenerateSP3D(string compilerVersion, BusinessObjectConfiguration dbSchema, DatabaseDictionary databaseDictionary, string projectName, string applicationName)
    {
      Utility.Write<BusinessObjectConfiguration>(dbSchema, "C:\\temp\\verified-businessConfiguration.xml");
      Response response = new Response();
      Status status = new Status();
      string commodityName = string.Empty, objectNamespace = string.Empty;
      DataObject commodityDataObject = null;

      if (dbSchema.businessCommodities != null)
      {
        _businessCommodities = dbSchema.businessCommodities;

        try
        {
          foreach (BusinessCommodity businessCommodity in dbSchema.businessCommodities)
          {
            commodityName = businessCommodity.commodityName.ToLower();
            objectNamespace = businessCommodity.objectNamespace;
            commodityDataObject = databaseDictionary.GetDataObject(commodityName);
            status.Identifier = String.Format("{0}.{1}.{2}", projectName, applicationName, commodityName);
            Directory.CreateDirectory(_settings["AppDataPath"]);

            _mappingBuilder = new StringBuilder();
            _mappingWriter = new XmlTextWriter(new StringWriter(_mappingBuilder));
            _mappingWriter.Formatting = Formatting.Indented;

            _mappingWriter.WriteStartElement("hibernate-mapping", "urn:nhibernate-mapping-2.2");
            _mappingWriter.WriteAttributeString("default-lazy", "true");

            _dataObjectBuilder = new StringBuilder();
            _dataObjectWriter = new IndentedTextWriter(new StringWriter(_dataObjectBuilder), "  ");

            _dataObjectWriter.WriteLine(Utility.GeneratedCodeProlog);
            _dataObjectWriter.WriteLine("using System;");
            _dataObjectWriter.WriteLine("using System.Globalization;");
            _dataObjectWriter.WriteLine("using System.Collections.Generic;");
            _dataObjectWriter.WriteLine("using Iesi.Collections.Generic;");
            _dataObjectWriter.WriteLine("using org.iringtools.library;");

            foreach (BusinessObject businessObject in businessCommodity.businessObjects)
            {
              prepareSP3DStartObjectNHibernateMap(businessObject);

              #region related objects

              if (businessObject.relatedObjects != null)
              {
                foreach (RelatedObject relatedObject in businessObject.relatedObjects)
                {
                  preparecreateSP3DRONHibernateMap(relatedObject);
                }
              }

              if (businessObject.relations != null)
              {
                foreach (BusinessRelation relation in businessObject.relations)
                  preparecreateSP3DRONHibernateMap(relation);
              }

              #endregion related objects
            }

            _mappingWriter.WriteEndElement(); // end hibernate-mapping element
            _mappingWriter.Close();

            string mappingXml = _mappingBuilder.ToString();
            string sourceCode = _dataObjectBuilder.ToString();

            #region Compile entities
            Dictionary<string, string> compilerOptions = new Dictionary<string, string>();
            compilerOptions.Add("CompilerVersion", compilerVersion);

            CompilerParameters parameters = new CompilerParameters();
            parameters.GenerateExecutable = false;
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add(_settings["BinaryPath"] + "Iesi.Collections.dll");
            parameters.ReferencedAssemblies.Add(_settings["BinaryPath"] + "iRINGLibrary.dll");
            NHIBERNATE_ASSEMBLIES.ForEach(assembly => parameters.ReferencedAssemblies.Add(_settings["BinaryPath"] + assembly));

            Utility.Compile(compilerOptions, parameters, new string[] { sourceCode });
            #endregion Compile entities

            #region Writing memory data to disk

            Utility.WriteString(mappingXml, _settings["AppDataPath"] + "nh-mapping." + projectName + "." + applicationName + "." + commodityName + ".xml", Encoding.UTF8);
            Utility.WriteString(sourceCode, _settings["AppCodePath"] + "Model." + projectName + "." + applicationName + "." + commodityName + ".cs", Encoding.ASCII);

            string hibernateConfig = CreateConfiguration(
              (Provider)Enum.Parse(typeof(Provider), dbSchema.Provider, true),
              dbSchema.ConnectionString, dbSchema.SchemaName);

            Utility.WriteString(hibernateConfig, _settings["AppDataPath"] + "nh-configuration." + projectName + "." + applicationName + "." + commodityName + ".xml", Encoding.UTF8);
            DataDictionary dataDictionarySP3D = CreateDataDictionarySP3D(businessCommodity);
            DatabaseDictionary databaseDictionarySP3D = CreateDatabaseDictionarySP3D(dataDictionarySP3D, databaseDictionary);
            dataDictionarySP3D.dataVersion = databaseDictionary.dataVersion;
            dataDictionarySP3D.enableSearch = databaseDictionary.enableSearch;
            dataDictionarySP3D.enableSummary = databaseDictionary.enableSummary;

            Utility.Write<DataDictionary>(dataDictionarySP3D, _settings["AppDataPath"] + "DataDictionary." + projectName + "." + applicationName + "." + commodityName + ".xml");
            Utility.Write<DatabaseDictionary>(databaseDictionarySP3D, _settings["AppDataPath"] + "DatabaseDictionary." + projectName + "." + applicationName + "." + commodityName + ".xml");
            #endregion

            status.Messages.Add("Entities of [" + projectName + "." + applicationName + "] generated successfully.");

          }
          DataDictionary dataDictionary = CreateDataDictionary(databaseDictionary.dataObjects);
          dataDictionary.dataVersion = databaseDictionary.dataVersion;
          dataDictionary.enableSearch = databaseDictionary.enableSearch;
          dataDictionary.enableSummary = databaseDictionary.enableSummary;

          Utility.Write<DataDictionary>(dataDictionary, _settings["AppDataPath"] + "DataDictionary." + projectName + "." + applicationName + ".xml");
          Utility.Write<BusinessObjectConfiguration>(dbSchema, _settings["AppDataPath"] + "BusinessObjectConfiguration." + projectName + "." + applicationName + ".xml");
        }
        catch (Exception ex)
        {
          throw new Exception("Error generating application entities " + ex);
          //no need to status, thrown exception will be statused above.
        }
      }

      response.Append(status);
      return response;
    }

    #region support functions for writing mapping files and model

    private void writeProperty(string propertyName, string columnName)
    {
      _mappingWriter.WriteStartElement("property");
      _mappingWriter.WriteAttributeString("name", propertyName);
      _mappingWriter.WriteAttributeString("column", "\"" + columnName + "\"");
      _mappingWriter.WriteAttributeString("update", "false");
      _mappingWriter.WriteAttributeString("insert", "false");
      _mappingWriter.WriteEndElement(); // end property element    
    }

    private void writeInterface(List<BusinessProperty> bpropList, BusinessInterface bintf)
    {
      /*
      * <join table="JCoatingInfo">
      <key column="&quot;oid&quot;" />
      */
      _mappingWriter.WriteStartElement("join");
      _mappingWriter.WriteAttributeString("table", bintf.tableName);
      _mappingWriter.WriteStartElement("key");
      _mappingWriter.WriteAttributeString("column", "\"" + "oid" + "\"");
      _mappingWriter.WriteEndElement(); // end key element

      foreach (BusinessProperty bprop in bintf.businessProperties)
      {
        if (!bpropList.Contains(bprop))
          bpropList.Add(bprop);

        writeProperty(bprop.propertyName, bprop.columnName);
        writeModelProperty(bprop.dataType, bprop.propertyName);
      }

      _mappingWriter.WriteEndElement(); // end join
    }

    private void writeModelProperty(DataType dataType, string propertyName)
    {
      _dataObjectWriter.WriteLine("public virtual {0} {1} {{get; set;}}", dataType, propertyName);      
    }

    private void writeClass(string nsClassName, string tableName, string key)
    {
      _mappingWriter.WriteStartElement("class");
      _mappingWriter.WriteAttributeString("name", nsClassName);
      _mappingWriter.WriteAttributeString("table", "\"" + tableName + "\"");

      _mappingWriter.WriteStartElement("id");
      _mappingWriter.WriteAttributeString("name", "Id");
      _mappingWriter.WriteAttributeString("column", "\"" + key + "\"");
      _mappingWriter.WriteStartElement("generator");
      _mappingWriter.WriteAttributeString("class", "assigned");
      _mappingWriter.WriteEndElement(); // end generator element
      _mappingWriter.WriteEndElement(); // end id element      

      writeProperty(key, key);
    }

    private void writeModelClass(string className, string key, DataType keyDataType)
    {
      _dataObjectWriter.WriteLine();
      _dataObjectWriter.Indent++;
      _dataObjectWriter.WriteLine("public class {0} : IDataObject", className);
      _dataObjectWriter.WriteLine("{"); // begin class block
      _dataObjectWriter.Indent++;

      _dataObjectWriter.WriteLine("public virtual String Id {get; set;}");
      _dataObjectWriter.WriteLine("public virtual String {0}", key);
      _dataObjectWriter.WriteLine("{");
      _dataObjectWriter.Indent++;
      _dataObjectWriter.WriteLine("get {return Id;}");
      _dataObjectWriter.WriteLine("set {Id = value;}");
      _dataObjectWriter.Indent--;
      _dataObjectWriter.WriteLine("}");
    }

    private void writeLeftClassToRightRelation(List<string> rightRelationClassNames, string objectNameSpace)
    {
      /* starting related object and middle related object
       * <many-to-one name="HasEqpAsChild" class="org.iringtools.adapter.datalayer.proj_12345_000.sp3d.equipment.HasEqpAsChild, App_Code" property-ref="oidDestination">
      <column name="oid" not-null="true" sql-type="uniqueidentifier" />
    </many-to-one>
       * */
      string rightRelationNSClassName = string.Empty;

      foreach (string rightRelationClassName in rightRelationClassNames)
      {
        rightRelationNSClassName = getNSClassName(objectNameSpace, rightRelationClassName);
        _mappingWriter.WriteStartElement("many-to-one");
        _mappingWriter.WriteAttributeString("name", rightRelationClassName);
        _mappingWriter.WriteAttributeString("class", rightRelationNSClassName);
        _mappingWriter.WriteAttributeString("property-ref", "oidDestination");
        _mappingWriter.WriteStartElement("column");
        _mappingWriter.WriteAttributeString("name", "oid");
        _mappingWriter.WriteAttributeString("not-null", "true");
        _mappingWriter.WriteAttributeString("sql-type", "uniqueidentifier");
        _mappingWriter.WriteEndElement(); // end column
        _mappingWriter.WriteEndElement(); // end many-to-one
      }
    }

    private void writeLeftRelationToRightClasses(List<string> rightClassNames, string objectNameSpace)
    {
      /* relation
       * <one-to-one name="CPMachinerySystem" class="org.iringtools.adapter.datalayer.proj_12345_000.sp3d.equipment.CPMachinerySystem, App_Code" constrained="true" /> />
      */

      string rightNSClassName = string.Empty;

      foreach (string rightClassName in rightClassNames)
      {
        rightNSClassName = getNSClassName(objectNameSpace, rightClassName);
        _mappingWriter.WriteStartElement("one-to-one");
        _mappingWriter.WriteAttributeString("name", rightClassName);
        _mappingWriter.WriteAttributeString("class", rightNSClassName);
        _mappingWriter.WriteAttributeString("constrained", "true");
        _mappingWriter.WriteEndElement(); // end one-to-one
      }
    }

    private void writeRightClassToLeftRelation(List<string> leftRelationClassNames, string objectNameSpace)
    {
      /* middle related object, end object
       * <one-to-one name="HasEqpAsChild" class="org.iringtools.adapter.datalayer.proj_12345_000.sp3d.equipment.HasEqpAsChild, App_Code" /> />
      */
      string leftRelationNSClassName = string.Empty;

      foreach (string leftRelationClassName in leftRelationClassNames)
      {
        leftRelationNSClassName = getNSClassName(objectNameSpace, leftRelationClassName);
        _mappingWriter.WriteStartElement("one-to-one");
        _mappingWriter.WriteAttributeString("name", leftRelationClassName);
        _mappingWriter.WriteAttributeString("class", leftRelationNSClassName);
        _mappingWriter.WriteEndElement(); // end one-to-one
      }
    }

    private void writeRightRelationToLeftClass(List<string> leftClassNames, string objectNameSpace)
    {
      /* relation
       * <set name="CPSmartEquipments" generic="false">
      <key>
        <column name="oid" />
      </key>
      <one-to-many class="org.iringtools.adapter.datalayer.proj_12345_000.sp3d.equipment.CPSmartEquipment, App_Code" />
    </set>
       */
      string leftNSClassName = string.Empty;

      foreach (string leftClassName in leftClassNames)
      {
        leftNSClassName = getNSClassName(objectNameSpace, leftClassName);
        _mappingWriter.WriteStartElement("set");
        _mappingWriter.WriteAttributeString("name", leftClassName + "s");
        _mappingWriter.WriteAttributeString("generic", "false");
        _mappingWriter.WriteStartElement("key");
        _mappingWriter.WriteStartElement("column");
        _mappingWriter.WriteAttributeString("name", "oid");
        _mappingWriter.WriteEndElement(); // end column
        _mappingWriter.WriteEndElement(); // end key

        _mappingWriter.WriteStartElement("one-to-many");
        _mappingWriter.WriteAttributeString("class", leftNSClassName);
        _mappingWriter.WriteEndElement(); // end one-to-many

        _mappingWriter.WriteEndElement(); // end set
      }
    }

    private void writeModelClassToRleationOrRelationToClass(string objectNameSpace, List<string> classNames)
    {
      /*
       *org.iringtools.adapter.datalayer.proj_12345_000.sp3d.equipment.HasEqpAsChild _HasEqpAsChild;

        public virtual org.iringtools.adapter.datalayer.proj_12345_000.sp3d.equipment.HasEqpAsChild HasEqpAsChild
        {
          get
          {
            return this._HasEqpAsChild;
          }
          set
          {
            this._HasEqpAsChild = (org.iringtools.adapter.datalayer.proj_12345_000.sp3d.equipment.HasEqpAsChild)value;
          }
        }
       * */
      string objectName = string.Empty;
      string classNSName = string.Empty;

      foreach (string className in classNames)
      {
        objectName = "_" + className;
        classNSName = getNSModelClassName(objectNameSpace, className);
        _dataObjectWriter.WriteLine();
        _dataObjectWriter.WriteLine("{0} {1};", classNSName, objectName);
        _dataObjectWriter.WriteLine("public virtual {0} {1}", classNSName, className);

        _dataObjectWriter.WriteLine("{"); // begin function block
        _dataObjectWriter.Indent++;
        _dataObjectWriter.WriteLine("get");
        _dataObjectWriter.WriteLine("{");
        _dataObjectWriter.Indent++;
        _dataObjectWriter.WriteLine("return this.{0};", objectName);
        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}");
        _dataObjectWriter.WriteLine("set");
        _dataObjectWriter.WriteLine("{");
        _dataObjectWriter.Indent++;
        _dataObjectWriter.WriteLine("this.{0} = ({1})value;", objectName, classNSName);
        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}"); //end set
        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}"); //end funcion
      }
    }

    private void writeClassEnd()
    {
      _mappingWriter.WriteEndElement();
      _dataObjectWriter.WriteLine("}");
      string modelXml = _dataObjectBuilder.ToString();
    }

    private void writeModelRightRelationToLeftClasses(string relationConstructor, List<string> leftClassNames)
    {
      /*
       * Iesi.Collections.ISet _CPSmartEquipments;
        public HasEqpAsChild()
        {
          this._CPSmartEquipments = new Iesi.Collections.HashedSet();
        }
        public virtual Iesi.Collections.ISet CPSmartEquipments
        {
          get
          {
            return this._CPSmartEquipments;
          }
          set
          {
            this._CPSmartEquipments = (Iesi.Collections.ISet)value;
          }
        }
       */
      string objectName = string.Empty;

      foreach (string leftClassName in leftClassNames)
      {
        objectName = "_" + leftClassName + "s";
        _dataObjectWriter.WriteLine("Iesi.Collections.ISet {0};", objectName);

        _dataObjectWriter.WriteLine();

        _dataObjectWriter.WriteLine("public virtual Iesi.Collections.ISet {0}", leftClassName + "s");
        _dataObjectWriter.WriteLine("{"); // begin function block
        _dataObjectWriter.Indent++;
        _dataObjectWriter.WriteLine("get");
        _dataObjectWriter.WriteLine("{");
        _dataObjectWriter.Indent++;
        _dataObjectWriter.WriteLine("return this.{0};", objectName);
        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}");
        _dataObjectWriter.WriteLine("set");
        _dataObjectWriter.WriteLine("{");
        _dataObjectWriter.Indent++;
        _dataObjectWriter.WriteLine("this.{0} = (Iesi.Collections.ISet)value;", objectName);
        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}"); //end set
        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}"); //end funcion
      }

      _dataObjectWriter.WriteLine("public {0}()", relationConstructor);
      _dataObjectWriter.WriteLine("{");
      _dataObjectWriter.Indent++;
      _dataObjectWriter.WriteLine("this.{0} = new Iesi.Collections.HashedSet();", objectName);
      _dataObjectWriter.Indent--;
      _dataObjectWriter.WriteLine("}"); //end contructor
    }

    private string getNSClassName(string objectNameSpace, string className)
    {
      return objectNameSpace + "." + className + ", " + _settings["ExecutingAssemblyName"];
    }

    private string getNSModelClassName(string objectNameSpace, string className)
    {
      return objectNameSpace + "." + className;
    }

    #endregion support functions for writing mapping files and model

    #region function for writing mapping files and model
    private void writeMiddleObject(List<string> leftRelationClassNames, string objectNameSpace, List<string> rightRelationClassNames)
    {
      /*
       * <one-to-one name="HasEqpAsChild" class="org.iringtools.adapter.datalayer.proj_12345_000.sp3d.equipment.HasEqpAsChild, App_Code" />
       * <many-to-one name="SystemHierarchy" class="org.iringtools.adapter.datalayer.proj_12345_000.sp3d.equipment.SystemHierarchy, App_Code" property-ref="oidDestination">
      <column name="oid" not-null="true" sql-type="uniqueidentifier" />
    </many-to-one>
       */

      writeRightClassToLeftRelation(leftRelationClassNames, objectNameSpace);
      writeLeftClassToRightRelation(rightRelationClassNames, objectNameSpace);
    }

    private void writeRelation(List<string> leftClassesNames, string objectNameSpace, List<string> rightClassNames)
    {/*
      * <set name="CPSmartEquipments" generic="false">
      <key>
        <column name="oid" />
      </key>
      <one-to-many class="org.iringtools.adapter.datalayer.proj_12345_000.sp3d.equipment.CPSmartEquipment, App_Code" />
    </set>
    <one-to-one name="CPMachinerySystem" class="org.iringtools.adapter.datalayer.proj_12345_000.sp3d.equipment.CPMachinerySystem, App_Code" constrained="true" />
      */
      writeRightRelationToLeftClass(leftClassesNames, objectNameSpace);
      writeLeftRelationToRightClasses(rightClassNames, objectNameSpace);
    }

    private void writeStartObject(List<string> rightRelationClassNames, string objectNameSpace)
    {
      writeLeftClassToRightRelation(rightRelationClassNames, objectNameSpace);
    }

    private void writeEndObject(List<string> leftRelationClassNames, string objectNameSpace)
    {
      writeRightClassToLeftRelation(leftRelationClassNames, objectNameSpace);
    }

    private void writeModelStartclass(List<string> rightRelationClassNames, string objectNameSpace)
    {
      writeModelClassToRleationOrRelationToClass(objectNameSpace, rightRelationClassNames);
    }

    private void writeModelEndclass(List<string> leftRelationClassNames, string objectNameSpace)
    {
      writeModelClassToRleationOrRelationToClass(objectNameSpace, leftRelationClassNames);
    }

    private void writeModelMiddleClass(List<string> leftRelationClassNames, string objectNameSapce, List<string> rightRelationClassNames)
    {
      writeModelClassToRleationOrRelationToClass(objectNameSapce, leftRelationClassNames);
      writeModelClassToRleationOrRelationToClass(objectNameSapce, rightRelationClassNames);
    }

    private void writeModelRelation(string relationConstructor, List<string> leftClassNames, List<string> rightClassNames, string objectNameSpace)
    {
      writeModelRightRelationToLeftClasses(relationConstructor, leftClassNames);
      writeModelClassToRleationOrRelationToClass(objectNameSpace, rightClassNames);
    }

    private void writeModelGetCodeList(BusinessProperty dataProperty)
    {
      _dataObjectWriter.WriteLine("switch {0}", dataProperty.propertyName);
      _dataObjectWriter.WriteLine("{");
      _dataObjectWriter.Indent++;

      foreach (CodelistItem codeItem in dataProperty.codeList)
      {
        _dataObjectWriter.WriteLine("case {0}:", codeItem.Value.ToString());
        _dataObjectWriter.Indent++;
        _dataObjectWriter.WriteLine("return {0};", codeItem.DisplayName);
      }

      _dataObjectWriter.Indent--;
      _dataObjectWriter.WriteLine("}");
      _dataObjectWriter.WriteLine("break;");
    }

    private void writeModelGetFunction(List<BusinessProperty> businessDataPropertyList, string objectName, List<string> relationNames, NodeType nodeType)
    {
      bool hasCodeList = false;
      _dataObjectWriter.WriteLine();
      _dataObjectWriter.WriteLine("public virtual object GetPropertyValue(string propertyName)");
      _dataObjectWriter.WriteLine("{");
      _dataObjectWriter.Indent++; _dataObjectWriter.WriteLine("switch (propertyName)");
      _dataObjectWriter.WriteLine("{");
      _dataObjectWriter.Indent++;
      _dataObjectWriter.WriteLine("case \"Id\": return Id;");

      foreach (BusinessProperty dataProperty in businessDataPropertyList)
      {
        hasCodeList = false;

        if (dataProperty.codeList != null)
          if (dataProperty.codeList.Count > 0)
            hasCodeList = true;

        if (objectName != "")
        {
          if (dataProperty.propertyName.ToLower() == "classname")
            _dataObjectWriter.WriteLine("case \"{0}\": return \"{1}\";", dataProperty.propertyName, objectName);
          else
          {
            if (hasCodeList)
              writeModelGetCodeList(dataProperty);
            else
              _dataObjectWriter.WriteLine("case \"{0}\": return {0};", dataProperty.propertyName);
          }
        }
        else
        {
          if (hasCodeList)
            writeModelGetCodeList(dataProperty);
          else
            _dataObjectWriter.WriteLine("case \"{0}\": return {0};", dataProperty.propertyName);
        }
      }

      switch (nodeType)
      {
        case NodeType.StartObject:
          foreach (string relationName in relationNames)
            _dataObjectWriter.WriteLine("case \"{0}_oidOrigin\": return {1}.oidOrigin;", relationName, "_" + relationName);
          break;
        case NodeType.MiddleObject:
          foreach (string relationName in relationNames)
            _dataObjectWriter.WriteLine("case \"oidOrigin\": return {0}.oidOrigin;", "_" + relationName);
          break;
        case NodeType.EndObject:
          _dataObjectWriter.WriteLine("case \"oidOrigin\": return null;");
          break;
      }     

      _dataObjectWriter.WriteLine("default: throw new Exception(\"Property [\" + propertyName + \"] does not exist.\");");
      _dataObjectWriter.Indent--;
      _dataObjectWriter.WriteLine("}"); //end switch
      _dataObjectWriter.Indent--;
      _dataObjectWriter.WriteLine("}"); //end funtion
    }

    private void writeModelSetCodeList(BusinessProperty dataProperty, string header)
    {
      _dataObjectWriter.WriteLine("switch ({0})", header, dataProperty.dataType);
      _dataObjectWriter.WriteLine("{");
      _dataObjectWriter.Indent++;

      foreach (CodelistItem codeItem in dataProperty.codeList)
      {
        _dataObjectWriter.WriteLine("case {0}:", codeItem.DisplayName);
        _dataObjectWriter.Indent++;
        _dataObjectWriter.WriteLine("{0} = {1};", dataProperty.propertyName, codeItem.Value.ToString());
      }

      _dataObjectWriter.Indent--;
      _dataObjectWriter.WriteLine("}");
      _dataObjectWriter.WriteLine("break;");
    }

    private void writeModelSetFunction(List<BusinessProperty> businessDataPropertyList, DataType keyDataType)
    {
      bool hasCodeList = false;
      _dataObjectWriter.WriteLine();
      _dataObjectWriter.WriteLine("public virtual void SetPropertyValue(string propertyName, object value)");
      _dataObjectWriter.WriteLine("{");
      _dataObjectWriter.Indent++;
      _dataObjectWriter.WriteLine("switch (propertyName)");
      _dataObjectWriter.Write("{");
      _dataObjectWriter.Indent++;

      if (IsNumeric(keyDataType))
      {
        _dataObjectWriter.WriteLine(@"
                case ""Id"":
                  Id = {0}.Parse((String)value, NumberStyles.Any);
                  break;", keyDataType);
      }
      else
      {
        _dataObjectWriter.WriteLine(@"
                case ""Id"":
                  Id = Convert.To{0}(value);
                  break;", keyDataType);
      }      

      foreach (BusinessProperty dataProperty in businessDataPropertyList)
      {
        hasCodeList = true;

        if (dataProperty.codeList != null)
          if (dataProperty.codeList.Count > 0)
            hasCodeList = true;

        _dataObjectWriter.WriteLine("case \"{0}\":", dataProperty.propertyName);
        _dataObjectWriter.Indent++;

        bool dataPropertyIsNullable = (dataProperty.dataType == DataType.String || dataProperty.isNullable == true);
        if (dataPropertyIsNullable)
        {
          if (IsNumeric(dataProperty.dataType))
          {
            if (hasCodeList)
              writeModelSetCodeList(dataProperty, "{0}.Parse((String)value, NumberStyles.Any)");
            else
              _dataObjectWriter.WriteLine("{0} = {1}.Parse((String)value, NumberStyles.Any);", dataProperty.propertyName, dataProperty.dataType);
          }
          else
          {
            if (hasCodeList)
              writeModelSetCodeList(dataProperty, "Convert.To{0}(value)");
            else
              _dataObjectWriter.WriteLine("{0} = Convert.To{1}(value);", dataProperty.propertyName, dataProperty.dataType);
          }
        }
        else
        {
          if (hasCodeList)
            writeModelSetCodeList(dataProperty, "(value != null) ? Convert.To{0}(value) : default({0})");
          else
            _dataObjectWriter.WriteLine("{0} = (value != null) ? Convert.To{1}(value) : default({1});", dataProperty.propertyName, dataProperty.dataType);
        }

        _dataObjectWriter.WriteLine("break;");
        _dataObjectWriter.Indent--;        
      }

      _dataObjectWriter.WriteLine("default:");
      _dataObjectWriter.Indent++;
      _dataObjectWriter.WriteLine("throw new Exception(\"Property [\" + propertyName + \"] does not exist.\");");
      _dataObjectWriter.Indent--;
      _dataObjectWriter.Indent--;
      _dataObjectWriter.WriteLine("}"); //end of switch
      _dataObjectWriter.Indent--;
      _dataObjectWriter.WriteLine("}"); //end of set function

      #region generate GetRelatedObjects method
      //_dataObjectWriter.WriteLine();
      //_dataObjectWriter.WriteLine(@"public virtual IList<IDataObject> GetRelatedObjects(string relatedObjectType)");
      //_dataObjectWriter.WriteLine("{");
      //_dataObjectWriter.Indent++;
      //_dataObjectWriter.WriteLine("switch (relatedObjectType)");
      //_dataObjectWriter.WriteLine("{");
      //_dataObjectWriter.Indent++;

      //_dataObjectWriter.WriteLine("default:");
      //_dataObjectWriter.Indent++;
      //_dataObjectWriter.WriteLine("throw new Exception(\"Related object [\" + relatedObjectType + \"] does not exist.\");");
      //_dataObjectWriter.Indent--;
      //_dataObjectWriter.Indent--;
      //_dataObjectWriter.WriteLine("}");
      //_dataObjectWriter.Indent--;
      //_dataObjectWriter.WriteLine("}");
      #endregion generate GetRelatedObjects method
      _dataObjectWriter.Indent--;
    }

    #endregion function for writing mapping files and model  

    private void preparecreateSP3DRONHibernateMap(RelatedObject businessObject)
    {
      _dataObjectWriter.WriteLine();
      _dataObjectWriter.WriteLine("namespace {0}", businessObject.objectNamespace);
      _dataObjectWriter.Write("{"); // begin namespace block
      _dataObjectWriter.Indent++;

      CreateNHibernateROMapSP3D(businessObject);

      _dataObjectWriter.Indent--;
      _dataObjectWriter.WriteLine("}"); // end namespace block    
    }

    private void prepareSP3DStartObjectNHibernateMap(BusinessObject businessObject)
    {
      _dataObjectWriter.WriteLine();
      _dataObjectWriter.WriteLine("namespace {0}", businessObject.objectNamespace);
      _dataObjectWriter.Write("{"); // begin namespace block
      _dataObjectWriter.Indent++;

      CreateNHibernateStartObjectMapSP3D(businessObject);

      _dataObjectWriter.Indent--;
      _dataObjectWriter.WriteLine("}"); // end namespace block    
    }    

    private DataDictionary CreateDataDictionarySP3D(BusinessCommodity businessCommodity)
    {
      string objectName = String.Empty;
      string propertyName = string.Empty;
      string keyPropertyName = string.Empty;
      string relatedPropertyName = string.Empty;
      string relatedObjectName = string.Empty;
      string relationshipName = string.Empty;
      string relatedInterfaceName = string.Empty;
      string startInterfaceName = string.Empty;
      string interfaceName = string.Empty;
      string commodityName = businessCommodity.commodityName.ToLower();
      DataObject dataObject = null;
      DataProperty newDataProperty = null;
      _dataObjects = new List<DataObject>();

      foreach (BusinessObject businessObject in businessCommodity.businessObjects)
      {
        dataObject = new DataObject();        
        dataObject.objectNamespace = businessCommodity.objectNamespace + "." + commodityName;

        if (businessCommodity.dataFilter != null)
          dataObject.dataFilter = businessCommodity.dataFilter;

        objectName = businessObject.objectName;
        dataObject.objectName = objectName;
        dataObject.tableName = businessObject.tableName;
        dataObject.keyProperties = new List<KeyProperty>();
        dataObject.dataProperties = new List<DataProperty>();
        
        if (businessObject.businessKeyProperties != null)
          if (businessObject.businessKeyProperties.Count > 0)
            foreach (BusinessKeyProperty businessKeyProerpty in businessObject.businessKeyProperties)
            {
              KeyProperty keyProperty = new KeyProperty();
              DataProperty dataProperty = businessKeyProerpty.convertKeyPropertyToDataProperty();

              if (!dataObject.dataProperties.Contains(dataProperty))
                dataObject.addKeyProperty(dataProperty);
            }

        if (businessObject.businessProperties != null)
          if (businessObject.businessProperties.Count > 0)
            foreach (BusinessProperty bp in businessObject.businessProperties)
            {
              newDataProperty = getNewDataProperty(bp);
              if (!dataObject.dataProperties.Contains(newDataProperty))
                dataObject.dataProperties.Add(newDataProperty);
            }        

        _dataObjects.Add(dataObject);

        if (businessObject.relatedObjects != null)
          if (businessObject.relatedObjects.Count > 0)
            foreach (RelatedObject relatedObject in businessObject.relatedObjects)
            {
              _dataObjects.Add(relatedObject.convertRelatedObjectToDataObject());
            }

        if (businessObject.relations != null)
          if (businessObject.relations.Count > 0)
            foreach (BusinessRelation relation in businessObject.relations)
              _dataObjects.Add(relation.convertRelationToDataObject());
      }

      return new DataDictionary { dataObjects = _dataObjects };
    }

    private DatabaseDictionary CreateDatabaseDictionarySP3D(DataDictionary dataDictionarySP3D, DatabaseDictionary databaseDictionary)
    {
      DatabaseDictionary databaseDictionarySP3D = new DatabaseDictionary();
      databaseDictionarySP3D.dataObjects = dataDictionarySP3D.dataObjects;
      databaseDictionarySP3D.ConnectionString = databaseDictionary.ConnectionString;
      databaseDictionarySP3D.Provider = databaseDictionary.Provider;
      databaseDictionarySP3D.SchemaName = databaseDictionary.SchemaName;
      databaseDictionarySP3D.picklists = databaseDictionary.picklists;
      databaseDictionarySP3D.IdentityConfiguration = databaseDictionary.IdentityConfiguration;

      return databaseDictionarySP3D;
    }

    private DataProperty getNewDataProperty(BusinessProperty businessProperty)
    {
      string propertyName = businessProperty.propertyName;
      DataProperty dataProperty = new DataProperty();

      if (!String.IsNullOrEmpty(businessProperty.columnName))
        dataProperty.columnName = businessProperty.columnName;
      else
        dataProperty.columnName = propertyName;
      dataProperty.propertyName = propertyName;

      dataProperty.dataType = businessProperty.dataType;
      dataProperty.isNullable = businessProperty.isNullable;
      dataProperty.isReadOnly = businessProperty.isReadOnly;

      if (!String.IsNullOrEmpty(businessProperty.description))
        dataProperty.description = businessProperty.description;

      return dataProperty;
    }

    private void CreateNHibernateROMapSP3D(RelatedObject businessObject)
    {
      BusinessKeyProperty keyObj = businessObject.businessKeyProperties.First();
      string className = string.Empty;
      string tableName = string.Empty;

      if (businessObject.nodeType == NodeType.Relation)
      {
        className = businessObject.relationName;
        tableName = businessObject.relationTableName;
      }
      else
      {
        className = businessObject.objectName;
        tableName = businessObject.tableName;
      }

      string key = keyObj.keyPropertyName;
      string nsClassName = getNSClassName(businessObject.objectNamespace, className);

      writeClass(nsClassName, tableName, key);
      writeModelClass(className, key, keyObj.dataType);

      List<BusinessProperty> businessDataPropertyList = new List<BusinessProperty>();

      BusinessProperty businessProperty = businessObject.convertKeyPropertyToProperyt(keyObj);
      businessDataPropertyList.Add(businessProperty);

      switch (businessObject.nodeType)
      {
        case NodeType.MiddleObject:
          writeMiddleObject(businessObject.leftClassNames, businessObject.objectNamespace, businessObject.rightClassNames);
          writeModelMiddleClass(businessObject.leftClassNames, businessObject.objectNamespace, businessObject.rightClassNames);
          break;
        case NodeType.EndObject:
          writeEndObject(businessObject.leftClassNames, businessObject.objectNamespace);
          writeModelEndclass(businessObject.leftClassNames, businessObject.objectNamespace);
          break;
        case NodeType.Relation:

          if (businessObject.businessProperties != null)
            if (businessObject.businessProperties.Count > 0)
              foreach (BusinessProperty businessProp in businessObject.businessProperties)
              {
                if (!businessDataPropertyList.Contains(businessProp))
                  businessDataPropertyList.Add(businessProp);

                writeProperty(businessProp.propertyName, businessProp.columnName);
                writeModelProperty(businessProp.dataType, businessProp.propertyName);
              }

          writeRelation(businessObject.leftClassNames, businessObject.objectNamespace, businessObject.rightClassNames);
          writeModelRelation(businessObject.relationName, businessObject.leftClassNames, businessObject.rightClassNames, businessObject.objectNamespace);
          break;
      }

      if (businessObject.nodeType != NodeType.Relation)
      {
        if (businessObject.businessInterfaces != null)
          if (businessObject.businessInterfaces.Count > 0)
            foreach (BusinessInterface businessInterface in businessObject.businessInterfaces)
            {
              writeInterface(businessDataPropertyList, businessInterface);
            }

        
      }

      if (businessObject.nodeType == NodeType.MiddleObject)
        writeModelGetFunction(businessDataPropertyList, "", businessObject.rightClassNames, businessObject.nodeType);
      else
        writeModelGetFunction(businessDataPropertyList, "", null, businessObject.nodeType);      

      writeModelSetFunction(businessDataPropertyList, keyObj.dataType);
      string mappingXml = _mappingBuilder.ToString();
      string modelXml = _dataObjectBuilder.ToString();
      writeClassEnd();
    }

    private void CreateNHibernateStartObjectMapSP3D(BusinessObject businessObject)
    {
      BusinessKeyProperty keyObj = businessObject.businessKeyProperties.First();
      string className = businessObject.objectName;
      string key = keyObj.keyPropertyName;
      string nsClassName = getNSClassName(businessObject.objectNamespace, className);

      writeClass(nsClassName, businessObject.tableName, key);
      writeModelClass(className, key, keyObj.dataType);

      List<BusinessProperty> businessDataPropertyList = new List<BusinessProperty>();
      BusinessProperty businessProperty = businessObject.convertKeyPropertyToProperyt(keyObj);
      businessDataPropertyList.Add(businessProperty);

      if (businessObject.businessProperties != null)
        if (businessObject.businessProperties.Count > 0)
          foreach (BusinessProperty bprop in businessObject.businessProperties)
          {
            businessDataPropertyList.Add(bprop);
            writeModelProperty(bprop.dataType, bprop.propertyName);
          }        

      if (businessObject.rightClassNames != null)
        if (businessObject.rightClassNames.Count > 0)
        {
          writeStartObject(businessObject.rightClassNames, businessObject.objectNamespace);
          writeModelStartclass(businessObject.rightClassNames, businessObject.objectNamespace);
        }

      if (businessObject.businessInterfaces != null)
        if (businessObject.businessInterfaces.Count > 0)
          foreach (BusinessInterface businessInterface in businessObject.businessInterfaces)
          {
            writeInterface(businessDataPropertyList, businessInterface);
          }

      writeModelGetFunction(businessDataPropertyList, className, businessObject.rightClassNames, businessObject.nodeType);
      writeModelSetFunction(businessDataPropertyList, keyObj.dataType);

      string modelXml = _dataObjectBuilder.ToString();
      writeClassEnd();
    }

    private string CreateConfiguration(Provider provider, string connectionString, String defaultSchema)
    {
      string driver = String.Empty;
      string dialect = String.Empty;

      try
      {
        string dbProvider = provider.ToString();

        if (dbProvider.ToUpper().Contains("MSSQL"))
        {
          driver = "NHibernate.Driver.SqlClientDriver";
        }
        else if (dbProvider.ToUpper().Contains("MYSQL"))
        {
          driver = "NHibernate.Driver.MySqlDataDriver";
        }
        else if (dbProvider.ToUpper().Contains("ORACLE"))
        {
          driver = "NHibernate.Driver.OracleClientDriver";
        }
        else
          throw new Exception(string.Format("Database provider {0} is not supported", dbProvider));

        dialect = "NHibernate.Dialect." + dbProvider + "Dialect";

        StringBuilder configBuilder = new StringBuilder();
        XmlTextWriter configWriter = new XmlTextWriter(new StringWriter(configBuilder));

        configWriter.Formatting = Formatting.Indented;
        configWriter.WriteStartElement("configuration");
        configWriter.WriteStartElement("hibernate-configuration", "urn:nhibernate-configuration-2.2");
        configWriter.WriteStartElement("session-factory");
        configWriter.WriteStartElement("property");
        configWriter.WriteAttributeString("name", "connection.provider");
        configWriter.WriteString("NHibernate.Connection.DriverConnectionProvider");
        configWriter.WriteEndElement(); // end property element
        configWriter.WriteStartElement("property");
        configWriter.WriteAttributeString("name", "connection.driver_class");
        configWriter.WriteString(driver);
        configWriter.WriteEndElement(); // end property element
        configWriter.WriteStartElement("property");
        configWriter.WriteAttributeString("name", "connection.connection_string");
        configWriter.WriteString(EncryptionUtility.Encrypt(connectionString));
        configWriter.WriteEndElement(); // end property element
        configWriter.WriteStartElement("property");
        configWriter.WriteAttributeString("name", "proxyfactory.factory_class");
        configWriter.WriteString("NHibernate.ByteCode.Castle.ProxyFactoryFactory, NHibernate.ByteCode.Castle");
        configWriter.WriteEndElement(); // end property element
        configWriter.WriteStartElement("property");
        configWriter.WriteAttributeString("name", "default_schema");
        configWriter.WriteString(defaultSchema);
        configWriter.WriteEndElement(); // end property element
        configWriter.WriteStartElement("property");
        configWriter.WriteAttributeString("name", "dialect");
        configWriter.WriteString(dialect);
        configWriter.WriteEndElement(); // end property element
        configWriter.WriteStartElement("property");
        configWriter.WriteAttributeString("name", "show_sql");
        configWriter.WriteString("true");
        configWriter.WriteEndElement(); // end property element
        configWriter.WriteEndElement(); // end session-factory element
        configWriter.WriteEndElement(); // end hibernate-configuration element
        configWriter.WriteEndElement(); // end configuration element
        configWriter.Close();

        return configBuilder.ToString();
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    private DataObject GetDataObject(string dataObjectName)
    {
      foreach (DataObject dataObject in _dataObjects)
      {
        if (dataObject.objectName.ToLower() == dataObjectName.ToLower())
          return dataObject;
      }

      return null;
    }

    private string GetColumnName(DataObject dataObject, string propertyName)
    {
      foreach (DataProperty property in dataObject.dataProperties)
      {
        if (property.propertyName.ToLower() == propertyName.ToLower())
          return property.columnName;
      }

      return String.Empty;
    }

    private bool IsNumeric(DataType dataType)
    {
      if (dataType == DataType.Int32 ||
          dataType == DataType.Decimal ||
          dataType == DataType.Double ||
          dataType == DataType.Single ||
          dataType == DataType.Int16 ||
          dataType == DataType.Int64 ||
          dataType == DataType.Byte)
      {
        return true;
      }

      return false;
    }

    public void GenerateSingleDataObject(string compilerVersion, DatabaseDictionary dbSchema, string projectName, string applicationName, DataObject dataObject)
    {
      string objectName = dataObject.objectName;

      if (dbSchema.dataObjects != null)
      {
        _dataObjects = dbSchema.dataObjects;

        try
        {
          Directory.CreateDirectory(_settings["AppDataPath"]);
          StringBuilder mappingBuilder = new StringBuilder();
          XmlTextWriter mappingWriter = new XmlTextWriter(new StringWriter(mappingBuilder));
          mappingWriter.Formatting = Formatting.Indented;
          mappingWriter.WriteStartElement("hibernate-mapping", "urn:nhibernate-mapping-2.2");
          mappingWriter.WriteAttributeString("default-lazy", "true");
          StringBuilder dataObjectBuilder = new StringBuilder();
          IndentedTextWriter dataObjectWriter = new IndentedTextWriter(new StringWriter(dataObjectBuilder), "  ");
          dataObjectWriter.WriteLine(Utility.GeneratedCodeProlog);
          dataObjectWriter.WriteLine("using System;");
          dataObjectWriter.WriteLine("using System.Globalization;");
          dataObjectWriter.WriteLine("using System.Collections.Generic;");
          dataObjectWriter.WriteLine("using Iesi.Collections.Generic;");
          dataObjectWriter.WriteLine("using org.iringtools.library;");
          dataObjectWriter.WriteLine();
          dataObjectWriter.WriteLine("namespace {0}", dataObject.objectNamespace);
          dataObjectWriter.Write("{"); // begin namespace block
          dataObjectWriter.Indent++;
          dataObject.objectNamespace = dataObject.objectNamespace;
          CreateNHibernateDataObjectMap(dataObject);
          dataObjectWriter.Indent--;
          dataObjectWriter.WriteLine("}"); // end namespace block
          mappingWriter.WriteEndElement(); // end hibernate-mapping element
          mappingWriter.Close();
          string mappingXml = mappingBuilder.ToString();
          string sourceCode = dataObjectBuilder.ToString();

          #region Compile entities
          Dictionary<string, string> compilerOptions = new Dictionary<string, string>();
          compilerOptions.Add("CompilerVersion", compilerVersion);
          CompilerParameters parameters = new CompilerParameters();
          parameters.GenerateExecutable = false;
          parameters.ReferencedAssemblies.Add("System.dll");
          parameters.ReferencedAssemblies.Add(_settings["BinaryPath"] + "Iesi.Collections.dll");
          parameters.ReferencedAssemblies.Add(_settings["BinaryPath"] + "iRINGLibrary.dll");
          NHIBERNATE_ASSEMBLIES.ForEach(assembly => parameters.ReferencedAssemblies.Add(_settings["BinaryPath"] + assembly));
          Utility.Compile(compilerOptions, parameters, new string[] { sourceCode });
          #endregion Compile entities

          #region Writing memory data to disk
          string hibernateConfig = CreateConfiguration(
            (Provider)Enum.Parse(typeof(Provider), dbSchema.Provider, true),
            dbSchema.ConnectionString, dbSchema.SchemaName);

          Utility.WriteString(hibernateConfig, _settings["AppDataPath"] + "nh-configuration." + projectName + "." + applicationName + "._" + objectName + ".xml", Encoding.UTF8);
          Utility.WriteString(mappingXml, _settings["AppDataPath"] + "nh-mapping." + projectName + "." + applicationName + "._" + objectName + ".xml", Encoding.UTF8);
          Utility.WriteString(sourceCode, _settings["AppCodePath"] + "Model." + projectName + "." + applicationName + "._" + objectName + ".cs", Encoding.ASCII);
          DataDictionary dataDictionary = CreateDataDictionary(dbSchema.dataObjects);
          dataDictionary.dataVersion = dbSchema.dataVersion;
          dataDictionary.enableSearch = dbSchema.enableSearch;
          dataDictionary.enableSummary = dbSchema.enableSummary;
          Utility.Write<DataDictionary>(dataDictionary, _settings["AppDataPath"] + "DataDictionary." + projectName + "." + applicationName + "._" + objectName + ".xml");
          #endregion          
        }
        catch (Exception ex)
        {
          throw new Exception("Error generating application entities " + ex);
          //no need to status, thrown exception will be statused above.
        }
      }      
    }

    public Response Generate(string compilerVersion, DatabaseDictionary dbSchema, string projectName, string applicationName)
    {
      Response response = new Response();
      Status status = new Status();

      if (dbSchema.dataObjects != null)
      {
        _dataObjects = dbSchema.dataObjects;

        try
        {
          status.Identifier = String.Format("{0}.{1}", projectName, applicationName);
          Directory.CreateDirectory(_settings["AppDataPath"]);
          _mappingBuilder = new StringBuilder();
          _mappingWriter = new XmlTextWriter(new StringWriter(_mappingBuilder));
          _mappingWriter.Formatting = Formatting.Indented;
          _mappingWriter.WriteStartElement("hibernate-mapping", "urn:nhibernate-mapping-2.2");
          _mappingWriter.WriteAttributeString("default-lazy", "true");
          _dataObjectBuilder = new StringBuilder();
          _dataObjectWriter = new IndentedTextWriter(new StringWriter(_dataObjectBuilder), "  ");
          _dataObjectWriter.WriteLine(Utility.GeneratedCodeProlog);
          _dataObjectWriter.WriteLine("using System;");
          _dataObjectWriter.WriteLine("using System.Globalization;");
          _dataObjectWriter.WriteLine("using System.Collections.Generic;");
          _dataObjectWriter.WriteLine("using Iesi.Collections.Generic;");
          _dataObjectWriter.WriteLine("using org.iringtools.library;");

          foreach (DataObject dataObject in dbSchema.dataObjects)
          {
            _dataObjectWriter.WriteLine();
            _dataObjectWriter.WriteLine("namespace {0}", dataObject.objectNamespace);
            _dataObjectWriter.Write("{"); // begin namespace block
            _dataObjectWriter.Indent++;
            dataObject.objectNamespace = dataObject.objectNamespace;
            CreateNHibernateDataObjectMap(dataObject);
            _dataObjectWriter.Indent--;
            _dataObjectWriter.WriteLine("}"); // end namespace block    

          }

          _mappingWriter.WriteEndElement(); // end hibernate-mapping element
          _mappingWriter.Close();
          string mappingXml = _mappingBuilder.ToString();
          string sourceCode = _dataObjectBuilder.ToString();

          #region Compile entities
          Dictionary<string, string> compilerOptions = new Dictionary<string, string>();
          compilerOptions.Add("CompilerVersion", compilerVersion);

          CompilerParameters parameters = new CompilerParameters();
          parameters.GenerateExecutable = false;
          parameters.ReferencedAssemblies.Add("System.dll");
          parameters.ReferencedAssemblies.Add(_settings["BinaryPath"] + "Iesi.Collections.dll");
          parameters.ReferencedAssemblies.Add(_settings["BinaryPath"] + "iRINGLibrary.dll");
          NHIBERNATE_ASSEMBLIES.ForEach(assembly => parameters.ReferencedAssemblies.Add(_settings["BinaryPath"] + assembly));

          Utility.Compile(compilerOptions, parameters, new string[] { sourceCode });
          #endregion Compile entities

          #region Writing memory data to disk
          string hibernateConfig = CreateConfiguration(
            (Provider)Enum.Parse(typeof(Provider), dbSchema.Provider, true),
            dbSchema.ConnectionString, dbSchema.SchemaName);

          Utility.WriteString(hibernateConfig, _settings["AppDataPath"] + "nh-configuration." + projectName + "." + applicationName + ".xml", Encoding.UTF8);
          Utility.WriteString(mappingXml, _settings["AppDataPath"] + "nh-mapping." + projectName + "." + applicationName + ".xml", Encoding.UTF8);
          Utility.WriteString(sourceCode, _settings["AppCodePath"] + "Model." + projectName + "." + applicationName + ".cs", Encoding.ASCII);

          DataDictionary dataDictionary = CreateDataDictionary(dbSchema.dataObjects);
          dataDictionary.dataVersion = dbSchema.dataVersion;
          dataDictionary.enableSearch = dbSchema.enableSearch;
          dataDictionary.enableSummary = dbSchema.enableSummary;

          Utility.Write<DataDictionary>(dataDictionary, _settings["AppDataPath"] + "DataDictionary." + projectName + "." + applicationName + ".xml");
          #endregion

          status.Messages.Add("Entities of [" + projectName + "." + applicationName + "] generated successfully.");
        }
        catch (Exception ex)
        {
          throw new Exception("Error generating application entities " + ex);

          //no need to status, thrown exception will be statused above.
        }
      }

      response.Append(status);
      return response;
    }

    // Remove table names and column names from database dictionary
    private DataDictionary CreateDataDictionary(List<DataObject> dataObjects)
    {
      /*
      foreach (DataObject dataObject in dataObjects)
      {        
        dataObject.tableName = null;

        foreach (DataProperty dataProperty in dataObject.dataProperties)
          dataProperty.columnName = null;
      }
      */

      return new DataDictionary { dataObjects = dataObjects };
    }

    private void CreateNHibernateDataObjectMap(DataObject dataObject)
    {
      string keyClassName = dataObject.objectName + "Id";

      _mappingWriter.WriteStartElement("class");
      _mappingWriter.WriteAttributeString("name", dataObject.objectNamespace + "." + dataObject.objectName + ", " + _settings["ExecutingAssemblyName"]);
      _mappingWriter.WriteAttributeString("table", "\"" + dataObject.tableName + "\"");

      #region Create composite key
      if (dataObject.keyProperties.Count > 1)
      {
        _dataObjectWriter.WriteLine();
        _dataObjectWriter.WriteLine("[Serializable]");
        _dataObjectWriter.WriteLine("public class {0}", keyClassName);
        _dataObjectWriter.WriteLine("{"); // begin composite key class
        _dataObjectWriter.Indent++;

        _mappingWriter.WriteStartElement("composite-id");
        _mappingWriter.WriteAttributeString("name", "Id");
        _mappingWriter.WriteAttributeString("class", dataObject.objectNamespace + "." + keyClassName + ", " + _settings["ExecutingAssemblyName"]);

        foreach (KeyProperty keyName in dataObject.keyProperties)
        {
          DataProperty keyProperty = dataObject.getKeyProperty(keyName.keyPropertyName);

          if (keyProperty != null)
          {
            _dataObjectWriter.WriteLine("public {0} {1} {{ get; set; }}", keyProperty.dataType, keyProperty.propertyName);

            _mappingWriter.WriteStartElement("key-property");
            _mappingWriter.WriteAttributeString("name", keyProperty.propertyName);
            _mappingWriter.WriteAttributeString("column", "\"" + keyProperty.columnName + "\"");
            _mappingWriter.WriteEndElement(); // end key-property
          }
        }

        _dataObjectWriter.WriteLine("public override bool Equals(object obj)"); // start Equals method
        _dataObjectWriter.WriteLine("{");

        _dataObjectWriter.Indent++;
        _dataObjectWriter.WriteLine("bool equals = false;");
        _dataObjectWriter.WriteLine("if (obj != null)");
        _dataObjectWriter.WriteLine("{");

        for (int i = 0; i < dataObject.keyProperties.Count; i++)
        {
          DataProperty keyProperty = dataObject.getKeyProperty(dataObject.keyProperties[i].keyPropertyName);

          string keyName = String.IsNullOrEmpty(keyProperty.propertyName) ? keyProperty.columnName : keyProperty.propertyName;

          if (i == 0)
          {
            _dataObjectWriter.Indent++;
            _dataObjectWriter.Write("equals = (");
          }
          else
          {
            _dataObjectWriter.Write(" && ");
          }

          _dataObjectWriter.Write("this.{0} == (({1})obj).{0}", keyName, keyClassName);
        }

        _dataObjectWriter.WriteLine(");");
        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}");
        _dataObjectWriter.WriteLine("return equals;");
        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}"); // end Equals method

        _dataObjectWriter.WriteLine("public override int GetHashCode()"); // start GetHashCode method
        _dataObjectWriter.WriteLine("{");
        _dataObjectWriter.Indent++;
        _dataObjectWriter.WriteLine("int _hashCode = 0;");

        for (int i = 0; i < dataObject.keyProperties.Count; i++)
        {
          DataProperty keyProperty = dataObject.getKeyProperty(dataObject.keyProperties[i].keyPropertyName);

          string keyName = String.IsNullOrEmpty(keyProperty.propertyName) ? keyProperty.columnName : keyProperty.propertyName;

          _dataObjectWriter.WriteLine("_hashCode += {0}.GetHashCode();", keyName);
        }

        _dataObjectWriter.WriteLine("return _hashCode;");
        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}"); // end GetHashCode method

        _dataObjectWriter.WriteLine("public override string ToString()"); // start ToString method
        _dataObjectWriter.WriteLine("{");
        _dataObjectWriter.Indent++;
        _dataObjectWriter.WriteLine("string _idString = String.Empty;");

        for (int i = 0; i < dataObject.keyProperties.Count; i++)
        {
          DataProperty keyProperty = dataObject.getKeyProperty(dataObject.keyProperties[i].keyPropertyName);
          string keyName = String.IsNullOrEmpty(keyProperty.propertyName) ? keyProperty.columnName : keyProperty.propertyName;

          _dataObjectWriter.WriteLine("_idString += {0}.ToString();", keyName);
        }

        _dataObjectWriter.WriteLine("return _idString;");
        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}"); // end ToString method

        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}"); // end composite key class

        _mappingWriter.WriteEndElement(); // end composite-id class element
      }
      #endregion Create composite key

      _dataObjectWriter.WriteLine();
      _dataObjectWriter.WriteLine("public class {0} : IDataObject", dataObject.objectName);
      _dataObjectWriter.WriteLine("{"); // begin class block
      _dataObjectWriter.Indent++;

      if (dataObject.keyProperties.Count > 1)
      {
        _dataObjectWriter.WriteLine("public {0}()", dataObject.objectName);
        _dataObjectWriter.WriteLine("{");
        _dataObjectWriter.Indent++;
        _dataObjectWriter.WriteLine("Id = new {0}Id();", dataObject.objectName);
        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}");
        _dataObjectWriter.WriteLine("public virtual {0} Id {{ get; set; }}", keyClassName);

        foreach (KeyProperty keyName in dataObject.keyProperties)
        {
          DataProperty keyProperty = dataObject.getKeyProperty(keyName.keyPropertyName);

          _dataObjectWriter.WriteLine("public virtual {0} {1}", keyProperty.dataType, keyProperty.propertyName);
          _dataObjectWriter.WriteLine("{");
          _dataObjectWriter.Indent++;
          _dataObjectWriter.WriteLine("get {{ return Id.{0}; }}", keyProperty.propertyName);
          _dataObjectWriter.WriteLine("set {{ Id.{0} = value; }}", keyProperty.propertyName);
          _dataObjectWriter.Indent--;
          _dataObjectWriter.WriteLine("}");

          _mappingWriter.WriteStartElement("property");
          _mappingWriter.WriteAttributeString("name", keyProperty.propertyName);
          _mappingWriter.WriteAttributeString("column", "\"" + keyProperty.columnName + "\"");
          _mappingWriter.WriteAttributeString("update", "false");
          _mappingWriter.WriteAttributeString("insert", "false");
          _mappingWriter.WriteEndElement();
        }
      }
      else if (dataObject.keyProperties.Count == 1)
      {
        DataProperty keyProperty = dataObject.getKeyProperty(dataObject.keyProperties.First().keyPropertyName);

        _dataObjectWriter.WriteLine("public virtual {0} Id {{ get; set; }}", keyProperty.dataType);

        _mappingWriter.WriteStartElement("id");
        _mappingWriter.WriteAttributeString("name", "Id");
        _mappingWriter.WriteAttributeString("column", "\"" + keyProperty.columnName + "\"");
        _mappingWriter.WriteStartElement("generator");
        _mappingWriter.WriteAttributeString("class", keyProperty.keyType.ToString());
        _mappingWriter.WriteEndElement(); // end generator element
        _mappingWriter.WriteEndElement(); // end id element

        if (keyProperty.keyType == KeyType.assigned)
        {
          _dataObjectWriter.WriteLine("public virtual {0} {1}", keyProperty.dataType, keyProperty.propertyName);
          _dataObjectWriter.WriteLine("{");
          _dataObjectWriter.Indent++;
          _dataObjectWriter.WriteLine("get { return Id; }");
          _dataObjectWriter.WriteLine("set { Id = value; }");
          _dataObjectWriter.Indent--;
          _dataObjectWriter.WriteLine("}");

          _mappingWriter.WriteStartElement("property");
          _mappingWriter.WriteAttributeString("name", keyProperty.propertyName);
          _mappingWriter.WriteAttributeString("column", "\"" + keyProperty.columnName + "\"");
          _mappingWriter.WriteAttributeString("update", "false");
          _mappingWriter.WriteAttributeString("insert", "false");
          _mappingWriter.WriteEndElement(); // end property element          
        }
      }

      #region Process relationships
      //if (dataObject.dataRelationships != null)
      //{
      //  foreach (DataRelationship dataRelationship in dataObject.dataRelationships)
      //  {
      //    DataObject relatedDataObject = GetDataObject(dataRelationship.relatedObjectName);

      //    switch (dataRelationship.relationshipType)
      //    {
      //      case RelationshipType.OneToOne:

      //DataProperty keyProperty = dataObject.getKeyProperty(dataObject.keyProperties.First().keyPropertyName);

      /*
      _dataObjectWriter.WriteLine("public virtual {0} Id {{ get; set; }}", keyProperty.dataType);

      _mappingWriter.WriteStartElement("id");
      _mappingWriter.WriteAttributeString("name", "Id");
      _mappingWriter.WriteAttributeString("column", "\"" + keyProperty.columnName + "\"");
      _mappingWriter.WriteStartElement("generator");
      _mappingWriter.WriteAttributeString("class", keyProperty.keyType.ToString());
      _mappingWriter.WriteStartElement("param");
      _mappingWriter.WriteAttributeString("name", "property");
      _mappingWriter.WriteString(dataRelationship.relatedObjectName);
      _mappingWriter.WriteEndElement(); // end param element
      _mappingWriter.WriteEndElement(); // end generator element
      _mappingWriter.WriteEndElement(); // end id element
      */

      //_mappingWriter.WriteStartElement("one-to-one");
      //_mappingWriter.WriteAttributeString("name", dataRelationship.relationshipName);
      //_mappingWriter.WriteAttributeString("class", _namespace + "." + dataRelationship.relatedObjectName + ", " + _settings["ExecutingAssemblyName"]);
      //_mappingWriter.WriteAttributeString("cascade", "save-update");

      /*
      if (oneToOneRelationship.isKeySource)
      {
        _mappingWriter.WriteAttributeString("cascade", "save-update");
      }
      else
      {
        _mappingWriter.WriteAttributeString("constrained", "true");
      }
      */

      //_dataObjectWriter.WriteLine("public virtual {0} {1} {{ get; set; }}", dataRelationship.relatedObjectName, dataRelationship.relationshipName);
      //_mappingWriter.WriteEndElement(); // end one-to-one element

      //  break;

      //case RelationshipType.OneToMany:

      //if (dataRelationship.propertyMaps.Count > 0)
      //{
      //  _dataObjectWriter.WriteLine("public virtual Iesi.Collections.Generic.ISet<{0}> {1} {{ get; set; }}", dataRelationship.relatedObjectName, dataRelationship.relationshipName);
      //  _mappingWriter.WriteStartElement("set");
      //  _mappingWriter.WriteAttributeString("name", dataRelationship.relationshipName);
      //  _mappingWriter.WriteAttributeString("table", relatedDataObject.tableName);
      //  _mappingWriter.WriteAttributeString("inverse", "true");
      //  _mappingWriter.WriteAttributeString("cascade", "all-delete-orphan");

      //  if (dataRelationship.propertyMaps.Count == 1)
      //  {
      //    _mappingWriter.WriteStartElement("key");
      //    _mappingWriter.WriteAttributeString("column", "\"" + GetColumnName(relatedDataObject, dataRelationship.propertyMaps.First().relatedPropertyName) + "\"");
      //    _mappingWriter.WriteEndElement(); // end key
      //  }
      //  else 
      //  {
      //    _mappingWriter.WriteStartElement("key");
      //    foreach (PropertyMap propertyMap in dataRelationship.propertyMaps)
      //    {
      //      _mappingWriter.WriteStartElement("column");
      //      _mappingWriter.WriteAttributeString("name", "\"" + GetColumnName(relatedDataObject, propertyMap.relatedPropertyName) + "\"");
      //      _mappingWriter.WriteEndElement(); // end column
      //    }
      //    _mappingWriter.WriteEndElement(); // end key
      //  }

      //  _mappingWriter.WriteStartElement("one-to-many");
      //  _mappingWriter.WriteAttributeString("class", _namespace + "." + dataRelationship.relatedObjectName + ", " + _settings["ExecutingAssemblyName"]);
      //  _mappingWriter.WriteEndElement(); // one-to-many
      //  _mappingWriter.WriteEndElement(); // end set element
      //}
      //        break;
      //    }
      //  }
      //}
      #endregion Process relationships

      #region Process columns
      if (dataObject.dataProperties != null)
      {
        foreach (DataProperty dataProperty in dataObject.dataProperties)
        {
          if (!dataObject.isKeyProperty(dataProperty.propertyName))
          {
            bool isNullableType = (dataProperty.dataType != DataType.String && dataProperty.isNullable == true);
            if (isNullableType)
            {
              _dataObjectWriter.WriteLine("public virtual {0}? {1} {{ get; set; }}", dataProperty.dataType, dataProperty.propertyName);
            }
            else
            {
              _dataObjectWriter.WriteLine("public virtual {0} {1} {{ get; set; }}", dataProperty.dataType, dataProperty.propertyName);
            }

            _mappingWriter.WriteStartElement("property");
            _mappingWriter.WriteAttributeString("name", dataProperty.propertyName);
            _mappingWriter.WriteAttributeString("column", "\"" + dataProperty.columnName + "\"");
            _mappingWriter.WriteEndElement(); // end property element
          }
        }

        // Implements GetPropertyValue of IDataObject
        _dataObjectWriter.WriteLine();
        _dataObjectWriter.WriteLine("public virtual object GetPropertyValue(string propertyName)");
        _dataObjectWriter.WriteLine("{");
        _dataObjectWriter.Indent++; _dataObjectWriter.WriteLine("switch (propertyName)");
        _dataObjectWriter.WriteLine("{");
        _dataObjectWriter.Indent++;
        _dataObjectWriter.WriteLine("case \"Id\": return Id;");

        foreach (DataProperty dataProperty in dataObject.dataProperties)
        {
          _dataObjectWriter.WriteLine("case \"{0}\": return {0};", dataProperty.propertyName);
        }

        _dataObjectWriter.WriteLine("default: throw new Exception(\"Property [\" + propertyName + \"] does not exist.\");");
        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}");
        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}");

        // Implements SetPropertyValue of IDataObject
        _dataObjectWriter.WriteLine();
        _dataObjectWriter.WriteLine("public virtual void SetPropertyValue(string propertyName, object value)");
        _dataObjectWriter.WriteLine("{");
        _dataObjectWriter.Indent++;
        _dataObjectWriter.WriteLine("switch (propertyName)");
        _dataObjectWriter.Write("{");
        _dataObjectWriter.Indent++;

        if (dataObject.keyProperties.Count == 1)
        {
          DataProperty keyProperty = dataObject.getKeyProperty(dataObject.keyProperties.First().keyPropertyName);
          DataType keyDataType = keyProperty.dataType;

          if (IsNumeric(keyDataType))
          {
            _dataObjectWriter.WriteLine(@"
        case ""Id"":
          Id = {0}.Parse((String)value, NumberStyles.Any);
          break;", keyDataType);
          }
          else
          {
            _dataObjectWriter.WriteLine(@"
        case ""Id"":
          Id = Convert.To{0}(value);
          break;", keyDataType);
          }
        }
        else if (dataObject.keyProperties.Count > 1)
        {
          _dataObjectWriter.WriteLine(@"
        case ""Id"":
          Id = ({0}Id)value;
          break;", dataObject.objectName);
        }

        /*
        foreach (KeyProperty keyName in dataObject.keyProperties)
        {
          DataProperty keyProperty = dataObject.getKeyProperty(keyName.keyPropertyName);

          _dataObjectWriter.WriteLine("case \"{0}\":", keyProperty.propertyName);
          _dataObjectWriter.Indent++;

          bool isDataPropertyNullable = (keyProperty.dataType == DataType.String || keyProperty.isNullable == true);
          if (isDataPropertyNullable)
          {
            _dataObjectWriter.WriteLine("if (value != null) {0} = Convert.To{1}(value);", keyProperty.propertyName, keyProperty.dataType);
          }
          else
          {
            _dataObjectWriter.WriteLine("{0} = (value != null) ? Convert.To{1}(value) : default({1});", keyProperty.propertyName, keyProperty.dataType);
          }

          _dataObjectWriter.WriteLine("break;");
          _dataObjectWriter.Indent--;
        }*/

        foreach (DataProperty dataProperty in dataObject.dataProperties)
        {
          _dataObjectWriter.WriteLine("case \"{0}\":", dataProperty.propertyName);
          _dataObjectWriter.Indent++;

          bool dataPropertyIsNullable = (dataProperty.dataType == DataType.String || dataProperty.isNullable == true);
          if (dataPropertyIsNullable)
          {
            if (IsNumeric(dataProperty.dataType))
            {
              _dataObjectWriter.WriteLine("{0} = {1}.Parse((String)value, NumberStyles.Any);", dataProperty.propertyName, dataProperty.dataType);
            }
            else
            {
              _dataObjectWriter.WriteLine("{0} = Convert.To{1}(value);", dataProperty.propertyName, dataProperty.dataType);
            }
          }
          else
          {
            _dataObjectWriter.WriteLine("{0} = (value != null) ? Convert.To{1}(value) : default({1});", dataProperty.propertyName, dataProperty.dataType);
          }

          _dataObjectWriter.WriteLine("break;");
          _dataObjectWriter.Indent--;
        }

        _dataObjectWriter.WriteLine("default:");
        _dataObjectWriter.Indent++;
        _dataObjectWriter.WriteLine("throw new Exception(\"Property [\" + propertyName + \"] does not exist.\");");
        _dataObjectWriter.Indent--;
        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}");
        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}");
      #endregion Process columns

        #region generate GetRelatedObjects method
        //_dataObjectWriter.WriteLine();
        //_dataObjectWriter.WriteLine(@"public virtual IList<IDataObject> GetRelatedObjects(string relatedObjectType)");
        //_dataObjectWriter.WriteLine("{");
        //_dataObjectWriter.Indent++;
        //_dataObjectWriter.WriteLine("switch (relatedObjectType)");
        //_dataObjectWriter.WriteLine("{");
        //_dataObjectWriter.Indent++;

        //foreach (DataRelationship dataRelationship in dataObject.dataRelationships)
        //{
        //  _dataObjectWriter.WriteLine("case \"{0}\":", dataRelationship.relationshipName);
        //  _dataObjectWriter.Indent++;

        //  if (dataRelationship.relationshipType == RelationshipType.OneToOne)
        //  {
        //    _dataObjectWriter.WriteLine(@"return new List<IDataObject>{{{0}}};", dataRelationship.relationshipName);
        //  }
        //  else if (dataRelationship.relationshipType == RelationshipType.OneToMany)
        //  {
        //    _dataObjectWriter.WriteLine(@"IList<IDataObject> relatedObjects = new List<IDataObject>();");
        //    _dataObjectWriter.WriteLine(@"foreach ({0} relatedObject in {1}) relatedObjects.Add(relatedObject);", dataRelationship.relatedObjectName, dataRelationship.relationshipName);
        //    _dataObjectWriter.WriteLine(@"return relatedObjects;");
        //  }

        //  _dataObjectWriter.Indent--;
        //}

        //_dataObjectWriter.WriteLine("default:");
        //_dataObjectWriter.Indent++;
        //_dataObjectWriter.WriteLine("throw new Exception(\"Related object [\" + relatedObjectType + \"] does not exist.\");");
        //_dataObjectWriter.Indent--;
        //_dataObjectWriter.Indent--;
        //_dataObjectWriter.WriteLine("}");
        //_dataObjectWriter.Indent--;
        //_dataObjectWriter.WriteLine("}");
        #endregion

        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}"); // end class block
        _mappingWriter.WriteEndElement(); // end class element
      }
    }
  }
}