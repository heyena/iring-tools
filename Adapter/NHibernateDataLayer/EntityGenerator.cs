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

namespace org.iringtools.adapter.datalayer
{
  public class EntityGenerator
  {
    private string COMPILER_VERSION = "v3.5";
    private string ASSEMBLY_NAME = "App_Code";
    private List<string> NHIBERNATE_ASSEMBLIES = new List<string>() 
    {
      "NHibernate.dll",     
      "NHibernate.ByteCode.Castle.dll",
      "Iesi.Collections.dll",
    };

    private AdapterSettings _settings = null;
    private StringBuilder _mappingBuilder = null;
    private XmlTextWriter _mappingWriter = null;
    //private Dictionary<string, string> _objectNames = null;
    //private DataDictionary _dataDictionary = null;
    private IndentedTextWriter _dataObjectWriter = null;
    private StringBuilder _dataObjectBuilder = null;
    private ILog _logger = null;

    private string _namespace = String.Empty;

    public EntityGenerator(AdapterSettings settings)
    {
      _settings = settings;
      _logger = LogManager.GetLogger(typeof(EntityGenerator));
    }

    public Response Generate(DatabaseDictionary dbDictionary, string projectName, string applicationName)
    {
      Response response = new Response();

      if (dbDictionary.dataObjects != null)
      {
        _namespace = "org.iringtools.adapter.datalayer.proj_" + projectName + "." + applicationName;

        try
        {
          Directory.CreateDirectory(_settings.XmlPath);

          _mappingBuilder = new StringBuilder();
          _mappingWriter = new XmlTextWriter(new StringWriter(_mappingBuilder));
          _mappingWriter.Formatting = Formatting.Indented;

          _mappingWriter.WriteStartElement("hibernate-mapping", "urn:nhibernate-mapping-2.2");
          _mappingWriter.WriteAttributeString("default-lazy", "true");

          //_dataDictionary = new DataDictionary();
          //_dataDictionary.dataObjects = new List<DataObject>();

          //_objectNames = new Dictionary<string, string>();
          //foreach (DataObject dataObject in dbDictionary.dataObjects)
          //{
          //  string objectName = String.IsNullOrEmpty(dataObject.objectName) ? dataObject.tableName : dataObject.objectName;
          //  _objectNames.Add(dataObject.tableName, objectName);
          //}

          _dataObjectBuilder = new StringBuilder();
          _dataObjectWriter = new IndentedTextWriter(new StringWriter(_dataObjectBuilder), "  ");

          _dataObjectWriter.WriteLine(Utility.GeneratedCodeProlog);
          _dataObjectWriter.WriteLine("using System;");
          _dataObjectWriter.WriteLine("using System.Collections.Generic;");
          _dataObjectWriter.WriteLine("using Iesi.Collections.Generic;");
          _dataObjectWriter.WriteLine("using org.iringtools.library;");
          _dataObjectWriter.WriteLine();
          _dataObjectWriter.WriteLine("namespace {0}", _namespace);
          _dataObjectWriter.Write("{"); // begin namespace block
          _dataObjectWriter.Indent++;

          //#region Create entities
          foreach (DataObject dataObject in dbDictionary.dataObjects)
          {
            CreateDataObject(dataObject);

            // Create data object for data dictionary
            //DataObject dataObject = new DataObject();

            //dataObject.objectName = _objectNames[dataObject.dataObjectName];
            //dataObject.objectNamespace = _namespace;
            //dataObject.dataProperties = new List<DataProperty>();

            //  #region Add key properties
            //  foreach (KeyProperty keyProperty in dataObject.keyProperties)
            //  {
            //    dataObject.keyProperties.Add(new KeyProperty
            //    {
            //      propertyName = keyProperty.propertyName,
            //      dataType = keyProperty.dataType,
            //      dataLength = keyProperty.dataLength,
            //      isNullable = keyProperty.isNullable,
            //      keyType = keyProperty.keyType
            //    });
            //  }
            //  #endregion

            //  #region Add data properties 
            //  foreach (Column column in dataObject.dataProperties)
            //  {
            //    DataProperty dataProperty = new DataProperty()
            //    {
            //      propertyName = column.propertyName,
            //      dataLength = column.dataLength,
            //      dataType = column.dataType,
            //      isNullable = column.isNullable
            //    };

            //    try
            //    {
            //      dataObject.dataProperties.Add(dataProperty);
            //    }
            //    catch (Exception duplicatePropertyException)
            //    {
            //      _logger.Warn(duplicatePropertyException.ToString());
            //    }
            //  }
            //  #endregion

            //  #region Process relationships
            //  if (dataObject.dataRelationships != null)
            //  {
            //    dataObject.dataRelationships = new List<DataRelationship>();

            //    foreach (Relationship relationship in dataObject.dataRelationships)
            //    {
            //      DataRelationship dataRelationship = new DataRelationship();
            //      string associatedEntityName = _objectNames[relationship.associatedDataObjectName];

            //      dataRelationship.relatedObject = associatedEntityName;

            //      switch (relationship.GetType().Name)
            //      {
            //        case "OneToOneRelationship":
            //          dataRelationship.cardinality = Cardinality.OneToOne;
            //          break;

            //        case "OneToManyRelationship":
            //          dataRelationship.cardinality = Cardinality.OneToMany;
            //          break;

            //        case "ManyToOneRelationship":
            //          dataRelationship.cardinality = Cardinality.ManyToOne;
            //          break;
            //      }

            //      dataObject.dataRelationships.Add(dataRelationship);
            //    }
            //  }
            //  #endregion

            //  _dataDictionary.dataObjects.Add(dataObject);
          }
          //#endregion

          _dataObjectWriter.Indent--;
          _dataObjectWriter.WriteLine("}"); // end namespace block                

          _mappingWriter.WriteEndElement(); // end hibernate-mapping element
          _mappingWriter.Close();

          string mappingXml = _mappingBuilder.ToString();
          string sourceCode = _dataObjectBuilder.ToString();

          #region Compile entities
          Dictionary<string, string> compilerOptions = new Dictionary<string, string>();
          compilerOptions.Add("CompilerVersion", COMPILER_VERSION);

          CompilerParameters parameters = new CompilerParameters();
          parameters.GenerateExecutable = false;
          parameters.ReferencedAssemblies.Add("System.dll");
          parameters.ReferencedAssemblies.Add(_settings.BinaryPath + "Iesi.Collections.dll");
          parameters.ReferencedAssemblies.Add(_settings.BinaryPath + "iRINGLibrary.dll");
          NHIBERNATE_ASSEMBLIES.ForEach(assembly => parameters.ReferencedAssemblies.Add(_settings.BinaryPath + assembly));

          Utility.Compile(compilerOptions, parameters, new string[] { sourceCode });
          #endregion Compile entities

          #region Writing memory data to disk
          string hibernateConfig = CreateConfiguration(dbDictionary.provider, dbDictionary.connectionString);
          Utility.WriteString(hibernateConfig, _settings.XmlPath + "nh-configuration." + projectName + "." + applicationName + ".xml", Encoding.UTF8);
          Utility.WriteString(mappingXml, _settings.XmlPath + "nh-mapping." + projectName + "." + applicationName + ".xml", Encoding.UTF8);
          Utility.WriteString(sourceCode, _settings.CodePath + "Model." + projectName + "." + applicationName + ".cs", Encoding.ASCII);

          DataDictionary dataDictionary = new DataDictionary { dataObjects = dbDictionary.dataObjects };
          Utility.Write<DataDictionary>(dataDictionary, _settings.XmlPath + "DataDictionary." + projectName + "." + applicationName + ".xml");
          #endregion

          response.Add("Entities generated successfully.");
        }
        catch (Exception ex)
        {
          throw new Exception("Error generating application entities " + ex);
        }
      }

      return response;
    }

    private void RemoveDups(DataObject dataObject)
    {
      for (int i = 0; i < dataObject.keyProperties.Count; i++)
      {
        for (int j = 0; j < dataObject.dataProperties.Count; j++)
        {
          // remove columns that are already in keys
          if (dataObject.dataProperties[j].propertyName.ToLower() == dataObject.keyProperties[i].propertyName.ToLower())
          {
            dataObject.dataProperties.Remove(dataObject.dataProperties[j--]);
            continue;
          }

          // remove duplicate columns
          for (int jj = j + 1; jj < dataObject.dataProperties.Count; jj++)
          {
            if (dataObject.dataProperties[jj].propertyName.ToLower() == dataObject.dataProperties[j].propertyName.ToLower())
            {
              dataObject.dataProperties.Remove(dataObject.dataProperties[jj--]);
            }
          }
        }

        // remove duplicate keys (in order of foreign - assigned - iddataObject/sequence)
        for (int ii = i + 1; ii < dataObject.keyProperties.Count; ii++)
        {
          if (dataObject.keyProperties[ii].columnName.ToLower() == dataObject.keyProperties[i].columnName.ToLower())
          {
            if (dataObject.keyProperties[ii].keyType != KeyType.foreign)
            {
              if (((dataObject.keyProperties[ii].keyType == KeyType.identity || dataObject.keyProperties[ii].keyType == KeyType.sequence) && dataObject.keyProperties[i].keyType == KeyType.assigned) ||
                    dataObject.keyProperties[ii].keyType == KeyType.assigned && dataObject.keyProperties[i].keyType == KeyType.foreign)
              {
                dataObject.keyProperties[i].keyType = dataObject.keyProperties[ii].keyType;
              }
            }

            dataObject.keyProperties.Remove(dataObject.keyProperties[ii--]);
          }
        }
      }
    }

    private void CreateDataObject(DataObject dataObject)
    {
      string keyClassName = dataObject.objectName + "Id";
      _mappingWriter.WriteStartElement("class");
      _mappingWriter.WriteAttributeString("name", _namespace + "." + dataObject.objectName + ", " + ASSEMBLY_NAME);
      _mappingWriter.WriteAttributeString("table", "[" + dataObject.tableName + "]");

      RemoveDups(dataObject);

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
        _mappingWriter.WriteAttributeString("class", _namespace + "." + keyClassName + ", " + ASSEMBLY_NAME);

        foreach (KeyProperty keyProperty in dataObject.keyProperties)
        {
          // for backward compatibility
          //bool isKeyNullable = (keyProperty.isNullable == null || keyProperty.isNullable == true);
          //string dataType = (keyProperty.dataType != DataType.String && isKeyNullable) ? (keyProperty.dataType.ToString() + "?") : (keyProperty.dataType.ToString());
          //string keyName = String.IsNullOrEmpty(keyProperty.propertyName) ? keyProperty.columnName : keyProperty.propertyName;

          _dataObjectWriter.WriteLine("public {0} {1} {{ get; set; }}", keyProperty.dataType, keyProperty.propertyName);

          _mappingWriter.WriteStartElement("key-property");
          _mappingWriter.WriteAttributeString("name", keyProperty.propertyName);
          _mappingWriter.WriteAttributeString("column", keyProperty.columnName);
          _mappingWriter.WriteEndElement(); // end key-property
        }

        _dataObjectWriter.WriteLine("public override bool Equals(object obj)"); // start Equals method
        _dataObjectWriter.WriteLine("{");

        _dataObjectWriter.Indent++;
        _dataObjectWriter.WriteLine("bool equals = false;");
        _dataObjectWriter.WriteLine("if (obj != null)");
        _dataObjectWriter.WriteLine("{");

        for (int i = 0; i < dataObject.keyProperties.Count; i++)
        {
          string keyName = String.IsNullOrEmpty(dataObject.keyProperties[i].propertyName) ? dataObject.keyProperties[i].columnName : dataObject.keyProperties[i].propertyName;

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
          string keyName = String.IsNullOrEmpty(dataObject.keyProperties[i].propertyName) ? dataObject.keyProperties[i].columnName : dataObject.keyProperties[i].propertyName;

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
          string keyName = String.IsNullOrEmpty(dataObject.keyProperties[i].propertyName) ? dataObject.keyProperties[i].columnName : dataObject.keyProperties[i].propertyName;

          if (i == 0)
          {
            _dataObjectWriter.WriteLine("_idString += {0}.ToString();", keyName);
          }
          else
          {
            _dataObjectWriter.WriteLine("_idString += \"_\" + {0}.ToString();", keyName);
          }
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

        foreach (KeyProperty keyProperty in dataObject.keyProperties)
        {
          // for backward compatibility
          //bool isKeyNullable = (keyProperty.isNullable == null || keyProperty.isNullable == true);
          //string dataType = (keyProperty.dataType != DataType.String && isKeyNullable) ? (keyProperty.dataType.ToString() + "?") : (keyProperty.dataType.ToString());

          _dataObjectWriter.WriteLine("public virtual {0} {1}", keyProperty.dataType, keyProperty.propertyName);
          _dataObjectWriter.WriteLine("{");
          _dataObjectWriter.Indent++;
          _dataObjectWriter.WriteLine("get {{ return Id.{0}; }}", keyProperty.propertyName);
          _dataObjectWriter.WriteLine("set {{ Id.{0} = value; }}", keyProperty.propertyName);
          _dataObjectWriter.Indent--;
          _dataObjectWriter.WriteLine("}");

          _mappingWriter.WriteStartElement("property");
          _mappingWriter.WriteAttributeString("name", keyProperty.propertyName);
          _mappingWriter.WriteAttributeString("column", keyProperty.columnName);
          _mappingWriter.WriteAttributeString("update", "false");
          _mappingWriter.WriteAttributeString("insert", "false");
          _mappingWriter.WriteEndElement();
        }
      }
      else if (dataObject.keyProperties.Count == 1 && dataObject.keyProperties.First().keyType != KeyType.foreign)
      {
        // for backward compatibility
        //bool isKeyNullable = (dataObject.keyProperties.First().isNullable == null || dataObject.keyProperties.First().isNullable == true);
        //string dataType = (dataObject.keyProperties.First().dataType != DataType.String && isKeyNullable) ? (dataObject.keyProperties.First().dataType.ToString() + "?") : (dataObject.keyProperties.First().dataType.ToString());

        _dataObjectWriter.WriteLine("public virtual {0} Id {{ get; set; }}", dataObject.keyProperties.First().dataType);

        _mappingWriter.WriteStartElement("id");
        _mappingWriter.WriteAttributeString("name", "Id");
        _mappingWriter.WriteAttributeString("column", dataObject.keyProperties.First().columnName);
        _mappingWriter.WriteStartElement("generator");
        _mappingWriter.WriteAttributeString("class", dataObject.keyProperties.First().keyType.ToString());
        _mappingWriter.WriteEndElement(); // end generator element
        _mappingWriter.WriteEndElement(); // end id element

        if (dataObject.keyProperties.First().keyType == KeyType.assigned)
        {
          _dataObjectWriter.WriteLine("public virtual {0} {1}", dataObject.keyProperties.First().dataType, dataObject.keyProperties.First().propertyName);
          _dataObjectWriter.WriteLine("{");
          _dataObjectWriter.Indent++;
          _dataObjectWriter.WriteLine("get { return Id; }");
          _dataObjectWriter.WriteLine("set { Id = value; }");
          _dataObjectWriter.Indent--;
          _dataObjectWriter.WriteLine("}");

          _mappingWriter.WriteStartElement("property");
          _mappingWriter.WriteAttributeString("name", dataObject.keyProperties.First().propertyName);
          _mappingWriter.WriteAttributeString("column", dataObject.keyProperties.First().columnName);
          _mappingWriter.WriteAttributeString("update", "false");
          _mappingWriter.WriteAttributeString("insert", "false");
          _mappingWriter.WriteEndElement(); // end property element
        }
      }

      #region Process relationships
      if (dataObject.dataRelationships != null)
      {
        foreach (DataRelationship dataRelationship in dataObject.dataRelationships)
        {
          //string associatedEntityName = _objectNames[dataRelationship.relatedObjectName];
          //string relatedObjectName = dataRelationship.relatedTableName;

          switch (dataRelationship.GetType().Name)
          {
            case "OneToOneRelationship":
              OneToOneRelationship oneToOneRelationship = (OneToOneRelationship)dataRelationship;

              if (dataObject.keyProperties.First().keyType == KeyType.foreign)
              {
                // for backward compatibility
                //bool isKeyNullable = (dataObject.keyProperties.First().isNullable == null || dataObject.keyProperties.First().isNullable == true);
                //string dataType = (dataObject.keyProperties.First().dataType != DataType.String && isKeyNullable) ? (dataObject.keyProperties.First().dataType.ToString() + "?") : (dataObject.keyProperties.First().dataType.ToString());

                _dataObjectWriter.WriteLine("public virtual {0} Id {{ get; set; }}", dataObject.keyProperties.First().dataType);

                _mappingWriter.WriteStartElement("id");
                _mappingWriter.WriteAttributeString("name", "Id");
                _mappingWriter.WriteAttributeString("column", dataObject.keyProperties.First().columnName);
                _mappingWriter.WriteStartElement("generator");
                _mappingWriter.WriteAttributeString("class", dataObject.keyProperties.First().keyType.ToString());
                _mappingWriter.WriteStartElement("param");
                _mappingWriter.WriteAttributeString("name", "property");
                _mappingWriter.WriteString(dataRelationship.relatedTableName);
                _mappingWriter.WriteEndElement(); // end param element
                _mappingWriter.WriteEndElement(); // end generator element
                _mappingWriter.WriteEndElement(); // end id element
              }

              _mappingWriter.WriteStartElement("one-to-one");
              _mappingWriter.WriteAttributeString("name", dataRelationship.relatedTableName);
              _mappingWriter.WriteAttributeString("class", _namespace + "." + dataRelationship.relatedTableName + ", " + ASSEMBLY_NAME);

              if (oneToOneRelationship.isKeyConstrained)
              {
                _mappingWriter.WriteAttributeString("constrained", "true");
              }
              else
              {
                _mappingWriter.WriteAttributeString("cascade", "save-update");
              }

              _dataObjectWriter.WriteLine("public virtual {0} {0} {{ get; set; }}", dataRelationship.relatedTableName);
              _mappingWriter.WriteEndElement(); // end one-to-one element
              break;

            case "OneToManyRelationship":
              OneToManyRelationship oneToManyRelationship = (OneToManyRelationship)dataRelationship;

              _dataObjectWriter.WriteLine("public virtual ISet<{0}> {0}List {{ get; set; }}", dataRelationship.relatedTableName);

              _mappingWriter.WriteStartElement("set");
              _mappingWriter.WriteAttributeString("name", dataRelationship.relatedTableName + "List");
              _mappingWriter.WriteAttributeString("inverse", "true");
              _mappingWriter.WriteAttributeString("cascade", "all-delete-orphan");
              _mappingWriter.WriteStartElement("key");
              _mappingWriter.WriteAttributeString("column", oneToManyRelationship.relatedColumnName);
              _mappingWriter.WriteEndElement(); // end one-to-many
              _mappingWriter.WriteStartElement("one-to-many");
              _mappingWriter.WriteAttributeString("class", _namespace + "." + dataRelationship.relatedTableName + ", " + ASSEMBLY_NAME);
              _mappingWriter.WriteEndElement(); // end key element
              _mappingWriter.WriteEndElement(); // end set element
              break;

            case "ManyToOneRelationship":
              ManyToOneRelationship manyToOneRelationship = (ManyToOneRelationship)dataRelationship;

              _dataObjectWriter.WriteLine("public virtual {0} {0} {{ get; set; }}", dataRelationship.relatedTableName);

              _mappingWriter.WriteStartElement("many-to-one");
              _mappingWriter.WriteAttributeString("name", dataRelationship.relatedTableName);
              _mappingWriter.WriteAttributeString("column", manyToOneRelationship.columnName);

              if (containsRelationship(dataObject.keyProperties, manyToOneRelationship))
              {
                _mappingWriter.WriteAttributeString("update", "false");
                _mappingWriter.WriteAttributeString("insert", "false");
              }

              _mappingWriter.WriteEndElement(); // end many-to-one element
              break;
          }
        }
      }
      #endregion Process relationships

      #region Process columns
      if (dataObject.dataProperties != null)
      {
        foreach (DataProperty dataProperty in dataObject.dataProperties)
        {
          // for backward compatibility
          //bool isColumnNullable = (dataProperty.isNullable == null || dataProperty.isNullable == true);
          //string dataType = (dataProperty.dataType != DataType.String && isColumnNullable) ? (dataProperty.dataType.ToString() + "?") : (dataProperty.dataType.ToString());
          //string propertyName = String.IsNullOrEmpty(dataProperty.propertyName) ? dataProperty.columnName : dataProperty.propertyName;

          _dataObjectWriter.WriteLine("public virtual {0} {1} {{ get; set; }}", dataProperty.dataType, dataProperty.propertyName);

          _mappingWriter.WriteStartElement("property");
          _mappingWriter.WriteAttributeString("name", dataProperty.propertyName);
          _mappingWriter.WriteAttributeString("column", dataProperty.columnName);
          _mappingWriter.WriteEndElement(); // end property element
        }

        // implements GetPropertyValue from IDataObject
        _dataObjectWriter.WriteLine("public virtual object GetPropertyValue(string propertyName)");
        _dataObjectWriter.WriteLine("{");
        _dataObjectWriter.Indent++; _dataObjectWriter.WriteLine("switch (propertyName)");
        _dataObjectWriter.WriteLine("{");
        _dataObjectWriter.Indent++;

        _dataObjectWriter.WriteLine("case \"Id\": return Id;");

        foreach (KeyProperty keyProperty in dataObject.keyProperties)
        {
          _dataObjectWriter.WriteLine("case \"{0}\": return {0};", keyProperty.propertyName);
        }

        foreach (DataProperty dataProperty in dataObject.dataProperties)
        {
          _dataObjectWriter.WriteLine("case \"{0}\": return {0};", dataProperty.propertyName);
        }

        _dataObjectWriter.WriteLine("default: throw new Exception(\"Property [\" + propertyName + \"] does not exist.\");");
        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}");
        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}");


        // implements SetPropertyValue from IDataObject
        _dataObjectWriter.WriteLine("public virtual void SetPropertyValue(string propertyName, object value)");
        _dataObjectWriter.WriteLine("{");
        _dataObjectWriter.Indent++;
        _dataObjectWriter.WriteLine("switch (propertyName)");
        _dataObjectWriter.Write("{");
        _dataObjectWriter.Indent++;

        _dataObjectWriter.WriteLine(@"
        case ""Id"":
          Id = Convert.ToString(value);
          if (Id == String.Empty) throw new Exception(""Id can not be null or empty."");
          break;");

        foreach (KeyProperty keyProperty in dataObject.keyProperties)
        {
          _dataObjectWriter.WriteLine("case \"{0}\":", keyProperty.propertyName);
          _dataObjectWriter.Indent++;

          bool isColumnNullable = (keyProperty.dataType == DataType.String || keyProperty.isNullable == true);
          if (isColumnNullable)
          {
            _dataObjectWriter.WriteLine("if (value != null) {0} = Convert.To{1}(value);", keyProperty.propertyName, keyProperty.dataType);
          }
          else
          {
            _dataObjectWriter.WriteLine("{0} = (value != null) ? Convert.To{1}(value) : default({1});", keyProperty.propertyName, keyProperty.dataType);
          }
          _dataObjectWriter.WriteLine("break;");
          _dataObjectWriter.Indent--;
        }

        foreach (DataProperty dataProperty in dataObject.dataProperties)
        {
          _dataObjectWriter.WriteLine("case \"{0}\":", dataProperty.propertyName);
          _dataObjectWriter.Indent++;

          bool isColumnNullable = (dataProperty.dataType == DataType.String || dataProperty.isNullable == true);
          if (isColumnNullable)
          {
            _dataObjectWriter.WriteLine("if (value != null) {0} = Convert.To{1}(value);", dataProperty.propertyName, dataProperty.dataType);
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
        _dataObjectWriter.WriteLine("}");
        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}");
        #endregion Process columns

        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}"); // end class block
        _mappingWriter.WriteEndElement(); // end class element
      }
    }

    private string CreateConfiguration(Provider provider, string connectionString)
    {
      string driver = String.Empty;
      string dialect = String.Empty;

      switch (provider)
      {
        case Provider.MsSql2000:
          driver = "NHibernate.Driver.SqlClientDriver";
          dialect = "NHibernate.Dialect.MsSql2000Dialect";
          break;

        case Provider.MsSql2005:
          driver = "NHibernate.Driver.SqlClientDriver";
          dialect = "NHibernate.Dialect.MsSql2005Dialect";
          break;

        case Provider.MsSql2008:
          driver = "NHibernate.Driver.SqlClientDriver";
          dialect = "NHibernate.Dialect.MsSql2008Dialect";
          break;

        case Provider.MySql3:
          driver = "NHibernate.Driver.MySqlDataDriver";
          dialect = "NHibernate.Dialect.MySQLDialect";
          break;

        case Provider.MySql4:
          driver = "NHibernate.Driver.MySqlDataDriver";
          dialect = "NHibernate.Dialect.MySQLDialect";
          break;

        case Provider.MySql5:
          driver = "NHibernate.Driver.MySqlDataDriver";
          dialect = "NHibernate.Dialect.MySQL5Dialect";
          break;

        case Provider.Oracle8i:
          driver = "NHibernate.Driver.OracleClientDriver";
          dialect = "NHibernate.Dialect.Oracle8iDialect";
          break;

        case Provider.Oracle9i:
          driver = "NHibernate.Driver.OracleClientDriver";
          dialect = "NHibernate.Dialect.Oracle9iDialect";
          break;

        case Provider.Oracle10g:
          driver = "NHibernate.Driver.OracleClientDriver";
          dialect = "NHibernate.Dialect.Oracle10gDialect";
          break;

        case Provider.OracleLite:
          driver = "NHibernate.Driver.OracleLiteDataClientDriver";
          dialect = "NHibernate.Dialect.OracleLiteDialect";
          break;

        case Provider.PostgresSql81:
          driver = "NHibernate.Driver.NpgsqlDriver";
          dialect = "NHibernate.Dialect.PostgreSQL81Dialect";
          break;

        case Provider.PostgresSql82:
          driver = "NHibernate.Driver.NpgsqlDriver";
          dialect = "NHibernate.Dialect.PostgreSQL82Dialect";
          break;

        case Provider.SqLite:
          driver = "NHibernate.Driver.SQLiteDriver";
          dialect = "NHibernate.Dialect.SQLiteDialect";
          break;
      }

      try
      {
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
        configWriter.WriteString(connectionString);
        configWriter.WriteEndElement(); // end property element
        configWriter.WriteStartElement("property");
        configWriter.WriteAttributeString("name", "proxyfactory.factory_class");
        configWriter.WriteString("NHibernate.ByteCode.Castle.ProxyFactoryFactory, NHibernate.ByteCode.Castle");
        configWriter.WriteEndElement(); // end property element
        configWriter.WriteStartElement("property");
        configWriter.WriteAttributeString("name", "dialect");
        configWriter.WriteString(dialect);
        configWriter.WriteEndElement(); // end property element
        configWriter.WriteStartElement("property");
        configWriter.WriteAttributeString("name", "show_sql");
        configWriter.WriteString("false");
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

    private bool containsRelationship(List<KeyProperty> keyProperties, ManyToOneRelationship relationship)
    {
      foreach (KeyProperty keyProperty in keyProperties)
      {
        if (relationship.columnName == keyProperty.columnName)
        {
          return true;
        }
      }

      return false;
    }
  }
}