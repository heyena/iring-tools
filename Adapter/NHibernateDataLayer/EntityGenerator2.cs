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
  public class EntityGenerator2
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
    private Dictionary<string, string> _entityNames = null;
    private DataDictionary _dataDictionary = null;
    private IndentedTextWriter _entityWriter = null;
    private StringBuilder _entityBuilder = null;
    private ILog _logger = null;

    private string _namespace = String.Empty;

    public EntityGenerator2(AdapterSettings settings)
    {
      _settings = settings;
      _logger = LogManager.GetLogger(typeof(EntityGenerator2));
    }

    public Response Generate(DatabaseDictionary dbDictionary, string projectName, string applicationName)
    {
      Response response = new Response();

      if (dbDictionary.tables != null)
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

          _entityNames = new Dictionary<string, string>();
          _dataDictionary = new DataDictionary();
          _dataDictionary.dataObjects = new List<DataObject>();

          foreach (Table table in dbDictionary.tables)
          {
            string entityName = String.IsNullOrEmpty(table.entityName) ? table.tableName : table.entityName;

            _entityNames.Add(table.tableName, entityName);
          }

          _entityBuilder = new StringBuilder();
          _entityWriter = new IndentedTextWriter(new StringWriter(_entityBuilder), "  ");

          _entityWriter.WriteLine(Utility.GeneratedCodeProlog);
          _entityWriter.WriteLine("using System;");
          _entityWriter.WriteLine("using System.Collections.Generic;");
          _entityWriter.WriteLine("using Iesi.Collections.Generic;");
          _entityWriter.WriteLine("using org.iringtools.library;");
          _entityWriter.WriteLine();
          _entityWriter.WriteLine("namespace {0}", _namespace);
          _entityWriter.Write("{"); // begin namespace block
          _entityWriter.Indent++;

          #region Create entities
          foreach (Table table in dbDictionary.tables)
          {
            CreateEntity(table);

            // Create data object for data dictionary
            DataObject dataObject = new DataObject();

            dataObject.objectName = _entityNames[table.tableName];
            dataObject.objectNamespace = _namespace;
            dataObject.dataProperties = new List<DataProperty>();

            #region Add key properties to data property
            foreach (Key key in table.keys)
            {
              DataProperty dataProperty = new DataProperty()
              {
                propertyName = String.IsNullOrEmpty(key.propertyName) ? key.columnName : key.propertyName,
                dataType = key.columnType.ToString(),
                // for backward compatibility
                dataLength = (key.dataLength == null) ? "255" : Convert.ToString(key.dataLength),
                //dataLength = Convert.ToString(key.dataLength),
                isPropertyKey = true,
                isRequired = !(key.isNullable == null || key.isNullable == true),
              };

              dataObject.dataProperties.Add(dataProperty);
            }
            #endregion

            #region Add column properties to data property
            foreach (Column column in table.columns)
            {
              DataProperty dataProperty = new DataProperty()
              {
                propertyName = String.IsNullOrEmpty(column.propertyName) ? column.columnName : column.propertyName,
                dataType = column.columnType.ToString(),
                // for backward compatibility, default data length to 255
                dataLength = (column.dataLength == null) ? "255" : Convert.ToString(column.dataLength),
                //dataLength = Convert.ToString(column.dataLength),
                isRequired = !(column.isNullable == null || column.isNullable == true),
              };

              try
              {
                dataObject.dataProperties.Add(dataProperty);
              }
              catch (Exception duplicatePropertyException)
              {
                _logger.Warn(duplicatePropertyException.ToString());
              }
            }
            #endregion

            #region Process associations
            if (table.associations != null)
            {
              dataObject.dataRelationships = new List<DataRelationship>();

              foreach (Association association in table.associations)
              {
                DataRelationship dataRelationship = new DataRelationship();
                string associatedEntityName = _entityNames[association.associatedTableName];

                dataRelationship.relatedObject = associatedEntityName;

                switch (association.GetType().Name)
                {
                  case "OneToOneAssociation":
                    dataRelationship.cardinality = Cardinality.OneToOne;
                    break;

                  case "OneToManyAssociation":
                    dataRelationship.cardinality = Cardinality.OneToMany;
                    break;

                  case "ManyToOneAssociation":
                    dataRelationship.cardinality = Cardinality.ManyToOne;
                    break;
                }

                dataObject.dataRelationships.Add(dataRelationship);
              }
            }
            #endregion

            _dataDictionary.dataObjects.Add(dataObject);
          }
          #endregion

          _entityWriter.Indent--;
          _entityWriter.WriteLine("}"); // end namespace block                

          _mappingWriter.WriteEndElement(); // end hibernate-mapping element
          _mappingWriter.Close();

          string mappingXml = _mappingBuilder.ToString();
          string entitiesSourceCode = _entityBuilder.ToString();

          #region Compile entities
          Dictionary<string, string> compilerOptions = new Dictionary<string, string>();
          compilerOptions.Add("CompilerVersion", COMPILER_VERSION);

          CompilerParameters parameters = new CompilerParameters();
          parameters.GenerateExecutable = false;
          parameters.ReferencedAssemblies.Add("System.dll");
          parameters.ReferencedAssemblies.Add(_settings.BinaryPath + "Iesi.Collections.dll");
          parameters.ReferencedAssemblies.Add(_settings.BinaryPath + "iRINGLibrary.dll");
          NHIBERNATE_ASSEMBLIES.ForEach(assembly => parameters.ReferencedAssemblies.Add(_settings.BinaryPath + assembly));

          Utility.Compile(compilerOptions, parameters, new string[] { entitiesSourceCode });
          #endregion Compile entities

          #region Writing memory data to disk
          string hibernateConfig = CreateConfiguration(dbDictionary.provider, dbDictionary.connectionString);
          Utility.WriteString(hibernateConfig, _settings.XmlPath + "nh-configuration." + projectName + "." + applicationName + ".xml", Encoding.UTF8);
          Utility.WriteString(mappingXml, _settings.XmlPath + "nh-mapping." + projectName + "." + applicationName + ".xml", Encoding.UTF8);
          Utility.WriteString(entitiesSourceCode, _settings.CodePath + "Model." + projectName + "." + applicationName + ".cs", Encoding.ASCII);
          Utility.Write<DataDictionary>(_dataDictionary, _settings.XmlPath + "DataDictionary." + projectName + "." + applicationName + ".xml");
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

    private void RemoveDups(Table table)
    {
      for (int i = 0; i < table.keys.Count; i++)
      {
        for (int j = 0; j < table.columns.Count; j++)
        {
          // remove columns that are already in keys
          if (table.columns[j].columnName.ToLower() == table.keys[i].columnName.ToLower())
          {
            table.columns.Remove(table.columns[j--]);
            continue;
          }

          // remove duplicate columns
          for (int jj = j + 1; jj < table.columns.Count; jj++)
          {
            if (table.columns[jj].columnName.ToLower() == table.columns[j].columnName.ToLower())
            {
              table.columns.Remove(table.columns[jj--]);
            }
          }
        }

        // remove duplicate keys (in order of foreign - assigned - identity/sequence)
        for (int ii = i + 1; ii < table.keys.Count; ii++)
        {
          if (table.keys[ii].columnName.ToLower() == table.keys[i].columnName.ToLower())
          {
            if (table.keys[ii].keyType != KeyType.foreign)
            {
              if (((table.keys[ii].keyType == KeyType.identity || table.keys[ii].keyType == KeyType.sequence) && table.keys[i].keyType == KeyType.assigned) ||
                    table.keys[ii].keyType == KeyType.assigned && table.keys[i].keyType == KeyType.foreign)
              {
                table.keys[i].keyType = table.keys[ii].keyType;
              }
            }

            table.keys.Remove(table.keys[ii--]);
          }
        }
      }
    }

    private void CreateEntity(Table table)
    {
      string entityName = _entityNames[table.tableName];
      string keyClassName = entityName + "Id";

      _mappingWriter.WriteStartElement("class");
      _mappingWriter.WriteAttributeString("name", _namespace + "." + entityName + ", " + ASSEMBLY_NAME);
      _mappingWriter.WriteAttributeString("table", table.tableName);

      RemoveDups(table);

      #region Create composite key
      if (table.keys.Count > 1)
      {
        _entityWriter.WriteLine();
        _entityWriter.WriteLine("[Serializable]");
        _entityWriter.WriteLine("public class {0}", keyClassName);
        _entityWriter.WriteLine("{"); // begin composite key class
        _entityWriter.Indent++;

        _mappingWriter.WriteStartElement("composite-id");
        _mappingWriter.WriteAttributeString("name", "Id");
        _mappingWriter.WriteAttributeString("class", _namespace + "." + keyClassName + ", " + ASSEMBLY_NAME);

        foreach (Key key in table.keys)
        {
          // for backward compatibility
          bool isKeyNullable = (key.isNullable == null || key.isNullable == true);
          string dataType = (key.columnType != ColumnType.String && isKeyNullable) ? (key.columnType.ToString() + "?") : (key.columnType.ToString());
          string keyName = String.IsNullOrEmpty(key.propertyName) ? key.columnName : key.propertyName;

          _entityWriter.WriteLine("public {0} {1} {{ get; set; }}", dataType, keyName);

          _mappingWriter.WriteStartElement("key-property");
          _mappingWriter.WriteAttributeString("name", keyName);
          _mappingWriter.WriteAttributeString("column", key.columnName);
          _mappingWriter.WriteEndElement(); // end key-property
        }

        _entityWriter.WriteLine("public override bool Equals(object obj)"); // start Equals method
        _entityWriter.WriteLine("{");

        _entityWriter.Indent++;
        _entityWriter.WriteLine("bool equals = false;");
        _entityWriter.WriteLine("if (obj != null)");
        _entityWriter.WriteLine("{");

        for (int i = 0; i < table.keys.Count; i++)
        {
          string keyName = String.IsNullOrEmpty(table.keys[i].propertyName) ? table.keys[i].columnName : table.keys[i].propertyName;

          if (i == 0)
          {
            _entityWriter.Indent++;
            _entityWriter.Write("equals = (");
          }
          else
          {
            _entityWriter.Write(" && ");
          }

          _entityWriter.Write("this.{0} == (({1})obj).{0}", keyName, keyClassName);
        }

        _entityWriter.WriteLine(");");
        _entityWriter.Indent--;
        _entityWriter.WriteLine("}");
        _entityWriter.WriteLine("return equals;");
        _entityWriter.Indent--;
        _entityWriter.WriteLine("}"); // end Equals method

        _entityWriter.WriteLine("public override int GetHashCode()"); // start GetHashCode method
        _entityWriter.WriteLine("{");
        _entityWriter.Indent++;
        _entityWriter.WriteLine("int _hashCode = 0;");

        for (int i = 0; i < table.keys.Count; i++)
        {
          string keyName = String.IsNullOrEmpty(table.keys[i].propertyName) ? table.keys[i].columnName : table.keys[i].propertyName;

          _entityWriter.WriteLine("_hashCode += {0}.GetHashCode();", keyName);
        }

        _entityWriter.WriteLine("return _hashCode;");
        _entityWriter.Indent--;
        _entityWriter.WriteLine("}"); // end GetHashCode method

        _entityWriter.WriteLine("public override string ToString()"); // start ToString method
        _entityWriter.WriteLine("{");
        _entityWriter.Indent++;
        _entityWriter.WriteLine("string _idString = String.Empty;");

        for (int i = 0; i < table.keys.Count; i++)
        {
          string keyName = String.IsNullOrEmpty(table.keys[i].propertyName) ? table.keys[i].columnName : table.keys[i].propertyName;

          if (i == 0)
          {
            _entityWriter.WriteLine("_idString += {0}.ToString();", keyName);
          }
          else
          {
            _entityWriter.WriteLine("_idString += \"_\" + {0}.ToString();", keyName);
          }
        }

        _entityWriter.WriteLine("return _idString;");
        _entityWriter.Indent--;
        _entityWriter.WriteLine("}"); // end ToString method

        _entityWriter.Indent--;
        _entityWriter.WriteLine("}"); // end composite key class

        _mappingWriter.WriteEndElement(); // end composite-id class element
      }
      #endregion Create composite key

      _entityWriter.WriteLine();
      _entityWriter.WriteLine("public class {0} : IDataObject", entityName);
      _entityWriter.WriteLine("{"); // begin class block
      _entityWriter.Indent++;

      if (table.keys.Count > 1)
      {
        _entityWriter.WriteLine("public {0}()", entityName);
        _entityWriter.WriteLine("{");
        _entityWriter.Indent++;
        _entityWriter.WriteLine("Id = new {0}Id();", entityName);
        _entityWriter.Indent--;
        _entityWriter.WriteLine("}");
        _entityWriter.WriteLine("public virtual {0} Id {{ get; set; }}", keyClassName);

        foreach (Key key in table.keys)
        {
          // for backward compatibility
          bool isKeyNullable = (key.isNullable == null || key.isNullable == true);
          string dataType = (key.columnType != ColumnType.String && isKeyNullable) ? (key.columnType.ToString() + "?") : (key.columnType.ToString());

          _entityWriter.WriteLine("public virtual {0} {1}", dataType, key.propertyName);
          _entityWriter.WriteLine("{");
          _entityWriter.Indent++;
          _entityWriter.WriteLine("get {{ return Id.{0}; }}", key.propertyName);
          _entityWriter.WriteLine("set {{ Id.{0} = value; }}", key.propertyName);
          _entityWriter.Indent--;
          _entityWriter.WriteLine("}");

          _mappingWriter.WriteStartElement("property");
          _mappingWriter.WriteAttributeString("name", key.propertyName);
          _mappingWriter.WriteAttributeString("column", key.columnName);
          _mappingWriter.WriteAttributeString("update", "false");
          _mappingWriter.WriteAttributeString("insert", "false");
          _mappingWriter.WriteEndElement();
        }
      }
      else if (table.keys.Count == 1 && table.keys[0].keyType != KeyType.foreign)
      {
        // for backward compatibility
        bool isKeyNullable = (table.keys[0].isNullable == null || table.keys[0].isNullable == true);
        string dataType = (table.keys[0].columnType != ColumnType.String && isKeyNullable) ? (table.keys[0].columnType.ToString() + "?") : (table.keys[0].columnType.ToString());

        _entityWriter.WriteLine("public virtual {0} Id {{ get; set; }}", dataType);

        _mappingWriter.WriteStartElement("id");
        _mappingWriter.WriteAttributeString("name", "Id");
        _mappingWriter.WriteAttributeString("column", table.keys[0].columnName);
        _mappingWriter.WriteStartElement("generator");
        _mappingWriter.WriteAttributeString("class", table.keys[0].keyType.ToString());
        _mappingWriter.WriteEndElement(); // end generator element
        _mappingWriter.WriteEndElement(); // end id element

        if (table.keys[0].keyType == KeyType.assigned)
        {
          _entityWriter.WriteLine("public virtual {0} {1}", dataType, table.keys[0].propertyName);
          _entityWriter.WriteLine("{");
          _entityWriter.Indent++;
          _entityWriter.WriteLine("get { return Id; }");
          _entityWriter.WriteLine("set { Id = value; }");
          _entityWriter.Indent--;
          _entityWriter.WriteLine("}");

          _mappingWriter.WriteStartElement("property");
          _mappingWriter.WriteAttributeString("name", table.keys[0].propertyName);
          _mappingWriter.WriteAttributeString("column", table.keys[0].columnName);
          _mappingWriter.WriteAttributeString("update", "false");
          _mappingWriter.WriteAttributeString("insert", "false");
          _mappingWriter.WriteEndElement(); // end property element
        }
      }

      #region Process associations
      if (table.associations != null)
      {
        foreach (Association association in table.associations)
        {
          string associatedEntityName = _entityNames[association.associatedTableName];

          switch (association.GetType().Name)
          {
            case "OneToOneAssociation":
              OneToOneAssociation oneToOneAssociation = (OneToOneAssociation)association;

              if (table.keys[0].keyType == KeyType.foreign)
              {
                // for backward compatibility
                bool isKeyNullable = (table.keys[0].isNullable == null || table.keys[0].isNullable == true);
                string dataType = (table.keys[0].columnType != ColumnType.String && isKeyNullable) ? (table.keys[0].columnType.ToString() + "?") : (table.keys[0].columnType.ToString());

                _entityWriter.WriteLine("public virtual {0} Id {{ get; set; }}", dataType);

                _mappingWriter.WriteStartElement("id");
                _mappingWriter.WriteAttributeString("name", "Id");
                _mappingWriter.WriteAttributeString("column", table.keys[0].columnName);
                _mappingWriter.WriteStartElement("generator");
                _mappingWriter.WriteAttributeString("class", table.keys[0].keyType.ToString());
                _mappingWriter.WriteStartElement("param");
                _mappingWriter.WriteAttributeString("name", "property");
                _mappingWriter.WriteString(associatedEntityName);
                _mappingWriter.WriteEndElement(); // end param element
                _mappingWriter.WriteEndElement(); // end generator element
                _mappingWriter.WriteEndElement(); // end id element
              }

              _mappingWriter.WriteStartElement("one-to-one");
              _mappingWriter.WriteAttributeString("name", associatedEntityName);
              _mappingWriter.WriteAttributeString("class", _namespace + "." + associatedEntityName + ", " + ASSEMBLY_NAME);

              if (oneToOneAssociation.constrained)
              {
                _mappingWriter.WriteAttributeString("constrained", "true");
              }
              else
              {
                _mappingWriter.WriteAttributeString("cascade", "save-update");
              }

              _entityWriter.WriteLine("public virtual {0} {0} {{ get; set; }}", associatedEntityName);
              _mappingWriter.WriteEndElement(); // end one-to-one element
              break;

            case "OneToManyAssociation":
              OneToManyAssociation oneToManyAssociation = (OneToManyAssociation)association;

              _entityWriter.WriteLine("public virtual ISet<{0}> {0}List {{ get; set; }}", associatedEntityName);

              _mappingWriter.WriteStartElement("set");
              _mappingWriter.WriteAttributeString("name", associatedEntityName + "List");
              _mappingWriter.WriteAttributeString("inverse", "true");
              _mappingWriter.WriteAttributeString("cascade", "all-delete-orphan");
              _mappingWriter.WriteStartElement("key");
              _mappingWriter.WriteAttributeString("column", oneToManyAssociation.associatedColumnName);
              _mappingWriter.WriteEndElement(); // end one-to-many
              _mappingWriter.WriteStartElement("one-to-many");
              _mappingWriter.WriteAttributeString("class", _namespace + "." + associatedEntityName + ", " + ASSEMBLY_NAME);
              _mappingWriter.WriteEndElement(); // end key element
              _mappingWriter.WriteEndElement(); // end set element
              break;

            case "ManyToOneAssociation":
              ManyToOneAssociation manyToOneAssociation = (ManyToOneAssociation)association;

              _entityWriter.WriteLine("public virtual {0} {0} {{ get; set; }}", associatedEntityName);

              _mappingWriter.WriteStartElement("many-to-one");
              _mappingWriter.WriteAttributeString("name", associatedEntityName);
              _mappingWriter.WriteAttributeString("column", manyToOneAssociation.columnName);

              if (IsAssociationInKeys(table.keys, manyToOneAssociation))
              {
                _mappingWriter.WriteAttributeString("update", "false");
                _mappingWriter.WriteAttributeString("insert", "false");
              }

              _mappingWriter.WriteEndElement(); // end many-to-one element
              break;
          }
        }
      }
      #endregion Process associations

      #region Process columns
      if (table.columns != null)
      {
        foreach (Column column in table.columns)
        {
          // for backward compatibility
          bool isColumnNullable = (column.isNullable == null || column.isNullable == true);
          string dataType = (column.columnType != ColumnType.String && isColumnNullable) ? (column.columnType.ToString() + "?") : (column.columnType.ToString());
          string propertyName = String.IsNullOrEmpty(column.propertyName) ? column.columnName : column.propertyName;

          _entityWriter.WriteLine("public virtual {0} {1} {{ get; set; }}", dataType, propertyName);

          _mappingWriter.WriteStartElement("property");
          _mappingWriter.WriteAttributeString("name", propertyName);
          _mappingWriter.WriteAttributeString("column", column.columnName);
          _mappingWriter.WriteEndElement(); // end property element
        }

        // implements GetPropertyValue from IDataObject
        _entityWriter.WriteLine("public virtual object GetPropertyValue(string propertyName)");
        _entityWriter.WriteLine("{");
        _entityWriter.Indent++; _entityWriter.WriteLine("switch (propertyName)");
        _entityWriter.WriteLine("{");
        _entityWriter.Indent++;

        _entityWriter.WriteLine("case \"Id\": return Id;");

        foreach (Key key in table.keys)
        {
          _entityWriter.WriteLine("case \"{0}\": return {0};", key.propertyName);
        }

        foreach (Column column in table.columns)
        {
          _entityWriter.WriteLine("case \"{0}\": return {0};", column.propertyName);
        }

        _entityWriter.WriteLine("default: throw new Exception(\"Property [\" + propertyName + \"] does not exist.\");");
        _entityWriter.Indent--;
        _entityWriter.WriteLine("}");
        _entityWriter.Indent--;
        _entityWriter.WriteLine("}");


        // implements SetPropertyValue from IDataObject
        _entityWriter.WriteLine("public virtual void SetPropertyValue(string propertyName, object value)");
        _entityWriter.WriteLine("{");
        _entityWriter.Indent++;
        _entityWriter.WriteLine("switch (propertyName)");
        _entityWriter.Write("{");
        _entityWriter.Indent++;

        _entityWriter.WriteLine(@"
        case ""Id"":
          Id = Convert.ToString(value);
          if (Id == String.Empty) throw new Exception(""Id can not be null or empty."");
          break;");

        foreach (Key key in table.keys)
        {
          _entityWriter.WriteLine("case \"{0}\":", key.propertyName);
          _entityWriter.Indent++;

          bool isColumnNullable = (key.isNullable == null || key.columnType == ColumnType.String || key.isNullable == true);
          if (isColumnNullable)
          {
            _entityWriter.WriteLine("if (value != null) {0} = Convert.To{1}(value);", key.propertyName, key.columnType);
          }
          else
          {
            _entityWriter.WriteLine("{0} = (value != null) ? Convert.To{1}(value) : default({1});", key.propertyName, key.columnType);
          }
          _entityWriter.WriteLine("break;");
          _entityWriter.Indent--;
        }

        foreach (Column column in table.columns)
        {
          _entityWriter.WriteLine("case \"{0}\":", column.propertyName);
          _entityWriter.Indent++;

          bool isColumnNullable = (column.isNullable == null || column.columnType == ColumnType.String || column.isNullable == true);
          if (isColumnNullable)
          {
            _entityWriter.WriteLine("if (value != null) {0} = Convert.To{1}(value);", column.propertyName, column.columnType);
          }
          else
          {
            _entityWriter.WriteLine("{0} = (value != null) ? Convert.To{1}(value) : default({1});", column.propertyName, column.columnType);
          }
          _entityWriter.WriteLine("break;");
          _entityWriter.Indent--;
        }

        _entityWriter.WriteLine("default:");
        _entityWriter.Indent++;
        _entityWriter.WriteLine("throw new Exception(\"Property [\" + propertyName + \"] does not exist.\");");
        _entityWriter.Indent--;
        _entityWriter.WriteLine("}");
        _entityWriter.Indent--;
        _entityWriter.WriteLine("}");
      #endregion Process columns

        _entityWriter.Indent--;
        _entityWriter.WriteLine("}"); // end class block
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

    private bool IsAssociationInKeys(List<Key> keys, ManyToOneAssociation association)
    {
      foreach (Key key in keys)
      {
        if (association.columnName == key.columnName)
        {
          return true;
        }
      }

      return false;
    }
  }
}