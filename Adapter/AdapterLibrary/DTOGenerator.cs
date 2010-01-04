using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Reflection;
using org.iringtools.library;
using org.iringtools.utility;

namespace org.iringtools.adapter
{
  public class ExtendedDataProperty : DataProperty
  {
    public string propertyPath { get; set; }
    public string dtoPropertyPath { get; set; }
    public bool isDataMember { get; set; }
  }

  public class DTOGenerator
  {
    private const string COMPILER_VERSION = "v3.5";
    private const string IRING_LIBRARY_NAMESPACE = "org.iringtools.library";
    private const string ADAPTER_NAMESPACE = "org.iringtools.adapter";

    private IndentedTextWriter _dtoWriter = null;
    private StringBuilder _dtoBuilder = null;
    private Mapping _mapping = null;
    private DataDictionary _dataDictionary = null;

    private string _currentDirectory = String.Empty;
    private string _dtoNamespace = String.Empty;
    private string _classPath = string.Empty;
    private string _dataContractPath = string.Empty;
    private string _templatePath = string.Empty;
    private string _dtoTemplatePath = string.Empty;
    private List<ExtendedDataProperty> _extendedDataProperties = null;
    private List<string> _initStatements = null;

    public DTOGenerator()
    {
      _currentDirectory = Directory.GetCurrentDirectory();
      _extendedDataProperties = new List<ExtendedDataProperty>();
      _initStatements = new List<string>();
    }

    public void Generate(string projectName, string applicationName)
    {
      try
      {
        string mappingPath = _currentDirectory + "\\XML\\Mapping." + projectName + "." + applicationName + ".xml";
        _mapping = Utility.Read<Mapping>(mappingPath, false);

        string dataDictionaryPath = _currentDirectory + "\\XML\\DataDictionary." + projectName + "." + applicationName + ".xml";
        _dataDictionary = Utility.Read<DataDictionary>(dataDictionaryPath, true);

        string modelNamespace = ADAPTER_NAMESPACE + ".proj_" + projectName + "." + applicationName;
        _dtoNamespace = "http://" + applicationName + ".bechtel.com/" + projectName + "/data#";

        _dtoBuilder = new StringBuilder();
        _dtoWriter = new IndentedTextWriter(new StringWriter(_dtoBuilder), "  ");

        _dtoWriter.WriteLine("using System;");
        _dtoWriter.WriteLine("using System.Collections.Generic;");
        _dtoWriter.WriteLine("using System.Runtime.Serialization;");
        _dtoWriter.WriteLine("using System.Xml.Serialization;");
        _dtoWriter.WriteLine("using System.Xml.Xsl;");
        _dtoWriter.WriteLine("using org.iringtools.library;");
        _dtoWriter.WriteLine("using org.iringtools.utility;");

        _dtoWriter.WriteLine();
        _dtoWriter.WriteLine("namespace {0}", modelNamespace);
        _dtoWriter.WriteLine("{");
        _dtoWriter.Indent++;

        foreach (GraphMap graphMap in _mapping.graphMaps)
        {
          graphMap.name = NameSafe(graphMap.name);
          graphMap.classId = graphMap.classId.Replace("rdl:", "http://rdl.rdlfacade.org/data#");

          _dtoWriter.WriteLine();
          _dtoWriter.WriteLine("[DataContract(Name = \"{0}\", Namespace = \"{1}\" )]", graphMap.name, _dtoNamespace);
          _dtoWriter.WriteLine("[XmlRoot(Namespace = \"{0}\")]", _dtoNamespace);
          _dtoWriter.WriteLine("public class {0} : DataTransferObject", graphMap.name);
          _dtoWriter.WriteLine("{");
          _dtoWriter.Indent++;

          ProcessGraphMap(graphMap);

          _dtoWriter.WriteLine();
          _dtoWriter.WriteLine("public {0}(string classId, string graphName, string identifier) : base(classId, graphName)", graphMap.name);
          _dtoWriter.WriteLine("{");
          _dtoWriter.Indent++;

          foreach (ExtendedDataProperty extendedDataProperty in _extendedDataProperties)
          {
            _dtoWriter.WriteLine("_properties.Add(new DTOProperty(@\"{0}\", @\"{1}\", null, typeof({2}), {3}, {4}));",
              extendedDataProperty.propertyName, extendedDataProperty.dtoPropertyPath, extendedDataProperty.dataType,
              Convert.ToString(extendedDataProperty.isPropertyKey).ToLower(),
              Convert.ToString(extendedDataProperty.isRequired).ToLower());
          }

          _dtoWriter.WriteLine("Identifier = identifier;");
          _dtoWriter.WriteLine("ClassId = classId;");
          _dtoWriter.Indent--;
          _dtoWriter.WriteLine("}");

          foreach (DataObjectMap dataObjectMap in graphMap.dataObjectMaps)
          {
            string qualifiedDataObjectName = GetQualifiedDataObjectName(dataObjectMap.name);

            _dtoWriter.WriteLine();
            _dtoWriter.WriteLine("public {0}({1} dataObject) : this(\"{2}\", \"{0}\", null, dataObject) {{}}", graphMap.name, qualifiedDataObjectName, graphMap.classId, graphMap.name);

            _dtoWriter.WriteLine();
            _dtoWriter.WriteLine("public {0}(string classId, string graphName, string identifier, {1} dataObject) : this(classId, graphName, identifier)", graphMap.name, qualifiedDataObjectName);
            _dtoWriter.WriteLine("{");
            _dtoWriter.Indent++;
            _dtoWriter.WriteLine("if (dataObject != null)");
            _dtoWriter.WriteLine("{");
            _dtoWriter.Indent++;

            foreach (ExtendedDataProperty extendedDataProperty in _extendedDataProperties)
            {
              if (extendedDataProperty.isPropertyKey)
              {
                _dtoWriter.WriteLine("{0} = ({1})dataObject.Id;", extendedDataProperty.propertyPath, extendedDataProperty.dataType);
              }
              else
              {
                _dtoWriter.WriteLine("{0} = ({1})dataObject.{2};", extendedDataProperty.propertyPath, extendedDataProperty.dataType, extendedDataProperty.propertyName);
              }
            }

            _dtoWriter.Indent--;
            _dtoWriter.WriteLine("}");

            foreach (string initStatement in _initStatements)
            {
              _dtoWriter.WriteLine(initStatement);
            }

            _dtoWriter.WriteLine("_dataObject = dataObject;");
            _dtoWriter.Indent--;
            _dtoWriter.WriteLine("}");
          }

          _dtoWriter.WriteLine();
          _dtoWriter.WriteLine("public {0}() : this(\"{1}\", \"{0}\", null) {{}}", graphMap.name, graphMap.classId);

          // Generate data contract member methods
          foreach (ExtendedDataProperty extendedDataProperty in _extendedDataProperties)
          {
            String nullableType = extendedDataProperty.dataType;

            // Convert to nullable type for some data types
            if (extendedDataProperty.dataType == "DateTime" ||
              extendedDataProperty.dataType == "Double" ||
              extendedDataProperty.dataType == "Float" ||
              extendedDataProperty.dataType == "Decimal" ||
              extendedDataProperty.dataType.StartsWith("Int"))
            {
              nullableType = "global::System.Nullable<" + extendedDataProperty.dataType + ">";
            }

            if (extendedDataProperty.isDataMember)
            {
              _dtoWriter.WriteLine();
              _dtoWriter.WriteLine("[DataMember(Name = \"{0}\", EmitDefaultValue = false)]", extendedDataProperty.propertyPath);
            }

            _dtoWriter.WriteLine();
            _dtoWriter.WriteLine("[XmlIgnore]");
            _dtoWriter.WriteLine("public {0} {1}", nullableType, extendedDataProperty.propertyPath);
            _dtoWriter.WriteLine("{");
            _dtoWriter.Indent++;
            _dtoWriter.WriteLine("get");
            _dtoWriter.WriteLine("{");
            _dtoWriter.Indent++;
            _dtoWriter.WriteLine("return ({0})GetPropertyValue(\"{1}\");", extendedDataProperty.dataType, extendedDataProperty.dtoPropertyPath);
            _dtoWriter.Indent--;
            _dtoWriter.WriteLine("}");
            _dtoWriter.WriteLine("set");
            _dtoWriter.WriteLine("{");
            _dtoWriter.Indent++;
            _dtoWriter.WriteLine("SetPropertyValue(@\"{0}\", value);", extendedDataProperty.dtoPropertyPath);
            _dtoWriter.Indent--;
            _dtoWriter.WriteLine("}");
            _dtoWriter.Indent--;
            _dtoWriter.WriteLine("}");
          }

          _dtoWriter.WriteLine();
          _dtoWriter.WriteLine("public override object GetDataObject()");
          _dtoWriter.WriteLine("{");
          _dtoWriter.Indent++;

          int dataObjectMapCount = 0;

          foreach (DataObjectMap dataObjectMap in graphMap.dataObjectMaps)
          {
            string qualifiedDataObjectName = GetQualifiedDataObjectName(dataObjectMap.name);

            if (!String.IsNullOrEmpty(dataObjectMap.inFilter))
            {
              // Determine whether "if" or "else if" to use
              if (++dataObjectMapCount == 1)
              {
                _dtoWriter.WriteLine("if ({0}) // inFilter", dataObjectMap.inFilter);
                _dtoWriter.WriteLine("{");
                _dtoWriter.Indent++;
              }
              else
              {
                _dtoWriter.WriteLine("else if ({0}) // inFilter", dataObjectMap.inFilter);
                _dtoWriter.WriteLine("{");
                _dtoWriter.Indent++;
              }
            }

            _dtoWriter.WriteLine("if (_dataObject == null)");
            _dtoWriter.WriteLine("{");
            _dtoWriter.Indent++;
            _dtoWriter.WriteLine("_dataObject = new {0}();", qualifiedDataObjectName);

            foreach (ExtendedDataProperty extendedDataProperty in _extendedDataProperties)
            {
              if (extendedDataProperty.isPropertyKey)
              {
                _dtoWriter.WriteLine("(({0})_dataObject).Id = ({1})this._identifier;", qualifiedDataObjectName, extendedDataProperty.dataType);
              }
            }

            _dtoWriter.Indent--;
            _dtoWriter.WriteLine("}");

            foreach (ExtendedDataProperty extendedDataProperty in _extendedDataProperties)
            {
              if (!extendedDataProperty.isPropertyKey)
              {
                _dtoWriter.WriteLine("(({0})_dataObject).{1} = ({2})this.{3};",
                  qualifiedDataObjectName, extendedDataProperty.propertyName, extendedDataProperty.dataType, extendedDataProperty.propertyPath);
              }
            }

            if (!String.IsNullOrEmpty(dataObjectMap.inFilter))
            {
              _dtoWriter.Indent--;
              _dtoWriter.WriteLine("}");
            }
          }

          _dtoWriter.WriteLine("return _dataObject;");
          _dtoWriter.Indent--;
          _dtoWriter.WriteLine("}");

          _dtoWriter.WriteLine();
          _dtoWriter.WriteLine("public override string Serialize()");
          _dtoWriter.WriteLine("{");
          _dtoWriter.Indent++;
          _dtoWriter.WriteLine("return Utility.SerializeDataContract<{0}>(this);", graphMap.name);
          _dtoWriter.Indent--;
          _dtoWriter.WriteLine("}");

          _dtoWriter.WriteLine();
          _dtoWriter.WriteLine("public override void Write(string path)");
          _dtoWriter.WriteLine("{");
          _dtoWriter.Indent++;
          _dtoWriter.WriteLine("Utility.Write<{0}>(this, path);", graphMap.name);
          _dtoWriter.Indent--;
          _dtoWriter.WriteLine("}");

          _dtoWriter.WriteLine();
          _dtoWriter.WriteLine("public override T Transform<T>(string xmlPath, string stylesheetUri, string mappingUri, bool useDataContractDeserializer)");
          _dtoWriter.WriteLine("{");
          _dtoWriter.Indent++;
          _dtoWriter.WriteLine("string dtoPath = xmlPath + this.GraphName + \".xml\";");
          _dtoWriter.WriteLine("Mapping mapping = Utility.Read<Mapping>(mappingUri, false);");
          _dtoWriter.WriteLine("List<{0}> list = new List<{0}> {{ this }};", graphMap.name);
          _dtoWriter.WriteLine("Utility.Write<List<{0}>>(list, dtoPath);", graphMap.name);
          _dtoWriter.WriteLine("XsltArgumentList xsltArgumentList = new XsltArgumentList();");
          _dtoWriter.WriteLine("xsltArgumentList.AddParam(\"dtoFilename\", String.Empty, dtoPath);");
          _dtoWriter.WriteLine("return Utility.Transform<Mapping, T>(mapping, stylesheetUri, xsltArgumentList, false, useDataContractDeserializer);");
          _dtoWriter.Indent--;
          _dtoWriter.WriteLine("}");
          _dtoWriter.Indent--;
          _dtoWriter.WriteLine("}");
        }

        _dtoWriter.Indent--;
        _dtoWriter.WriteLine("}");
        _dtoWriter.Close();

        string sourceCode = _dtoBuilder.ToString();

        #region Compile source code
        Dictionary<string, string> compilerOptions = new Dictionary<string, string>();
        compilerOptions.Add("CompilerVersion", COMPILER_VERSION);

        CompilerParameters parameters = new CompilerParameters();
        parameters.GenerateExecutable = false;
        parameters.ReferencedAssemblies.Add(@"C:\Program Files\Reference Assemblies\Microsoft\Framework\v3.0\System.Runtime.Serialization.dll");
        parameters.ReferencedAssemblies.Add(@"C:\Windows\Microsoft.NET\Framework\v2.0.50727\System.Xml.dll");
        parameters.ReferencedAssemblies.Add(_currentDirectory + @"\bin\iRINGLibrary.dll");
        parameters.ReferencedAssemblies.Add(_currentDirectory + @"\bin\UtilityLibrary.dll");
        parameters.ReferencedAssemblies.Add(_currentDirectory + @"\bin\AdapterLibrary.dll");
        parameters.ReferencedAssemblies.Add(_currentDirectory + @"\bin\AdapterService.dll");

        Compile(compilerOptions, parameters, sourceCode);
        #endregion Compile source code

        // Write source code to disk
        Utility.WriteString(sourceCode, _currentDirectory + @"\App_Code\DTOModel." + projectName + "." + applicationName + ".cs", Encoding.ASCII);
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    private void Compile(Dictionary<string, string> compilerOptions, CompilerParameters compilerParameters, string sourceCode)
    {
      try
      {
        CSharpCodeProvider codeProvider = new CSharpCodeProvider(compilerOptions);
        CompilerResults results = codeProvider.CompileAssemblyFromSource(compilerParameters, sourceCode);

        if (results.Errors.Count > 0)
        {
          StringBuilder errors = new StringBuilder();

          foreach (CompilerError error in results.Errors)
          {
            errors.AppendLine(error.ErrorNumber + ": " + error.ErrorText);
          }

          throw new Exception(errors.ToString());
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    private string NameSafe(string name)
    {
      return Regex.Replace(name, @"^\d*|\W", "");
    }

    private DataObject GetDataObject(string dataObjectMapName)
    {
      foreach (DataObject dataObject in _dataDictionary.dataObjects)
      {
        if (dataObject.objectName == dataObjectMapName)
        {
          return dataObject;
        }
      }

      return null;
    }

    private string GetQualifiedDataObjectName(string dataObjectMapName)
    {
      DataObject dataObject = GetDataObject(dataObjectMapName);
      return (dataObject.objectNamespace != String.Empty) ? (dataObject.objectNamespace + "." + dataObject.objectName) : dataObject.objectName;
    }

    private List<string> GetKnownTypes()
    {
      List<string> knownTypes = new List<string>();
      string[] mappingFiles = Directory.GetFiles(_currentDirectory + "\\XML", "Mapping*.xml");

      foreach (string mappingFile in mappingFiles)
      {
        Mapping mappingObj = Utility.Read<Mapping>(mappingFile, false);
        string[] mappingFileFields = mappingFile.Split('.');
        string projName = mappingFileFields[mappingFileFields.Length - 3];
        string appName = mappingFileFields[mappingFileFields.Length - 2];
        string ns = ADAPTER_NAMESPACE + ".proj_" + projName + "." + appName;

        foreach (GraphMap graphMap in mappingObj.graphMaps)
        {
          string graphName = NameSafe(graphMap.name);
          string className = ns + "." + graphName;

          //[ServiceKnownType(typeof(<#= className #>))]
          knownTypes.Add(className);
        }
      }

      return knownTypes;
    }

    private bool ContainsDataProperty(string propertyPath)
    {
      foreach (ExtendedDataProperty extendedDataProperty in _extendedDataProperties)
      {
        if (extendedDataProperty.propertyPath == propertyPath)
        {
          return true;
        }
      }

      return false;
    }

    private void ProcessGraphMap(GraphMap graphMap)
    {
      _extendedDataProperties.Clear();
      _initStatements.Clear();

      foreach (TemplateMap templateMap in graphMap.templateMaps)
      {
        _classPath = string.Empty;
        _dataContractPath = string.Empty;
        _templatePath = string.Empty;
        _dtoTemplatePath = string.Empty;

        ProcessTemplateMap(templateMap, graphMap.dataObjectMaps, true);
      }
    }

    private void ProcessTemplateMap(TemplateMap templateMap, List<DataObjectMap> dataObjectMaps, bool isDataMember)
    {
      foreach (RoleMap roleMap in templateMap.roleMaps)
      {
        templateMap.name = NameSafe(templateMap.name);
        roleMap.name = NameSafe(roleMap.name);

        if (templateMap.type == TemplateType.Property)
        {
          ProcessRoleMap(templateMap.name, roleMap, dataObjectMaps, isDataMember);
        }
        else if (templateMap.type == TemplateType.Relationship)
        {
          roleMap.classMap.name = NameSafe(roleMap.classMap.name);

          if (_classPath == string.Empty)  // classMap is graphMap
          {
            _classPath = "Template" + templateMap.name;
            _dataContractPath = "tpl_" + templateMap.name;
            _templatePath = "tpl_" + templateMap.name;
            _dtoTemplatePath = "tpl:" + templateMap.name;
            _initStatements.Add(_dataContractPath + " = new " + _classPath + "();");
          }
          else
          {
            _classPath += ".Template" + templateMap.name;
            _dataContractPath += ".tpl_" + templateMap.name;
            _templatePath += "_tpl_" + templateMap.name;
            _dtoTemplatePath += ".tpl:" + templateMap.name;
            _initStatements.Add(_dataContractPath + " = new " + _classPath + "();");
          }

          _dtoWriter.WriteLine();
          _dtoWriter.WriteLine("[DataContract(Namespace = \"{0}\")]", _dtoNamespace);
          _dtoWriter.WriteLine("[XmlRoot(Namespace = \"{0}\")]", _dtoNamespace);
          _dtoWriter.WriteLine("public class Template{0}", templateMap.name);
          _dtoWriter.WriteLine("{");
          _dtoWriter.Indent++;
          _dtoWriter.WriteLine();
          _dtoWriter.WriteLine("[DataContract(Namespace = \"{0}\")]", _dtoNamespace);
          _dtoWriter.WriteLine("[XmlRoot(Namespace = \"{0}\")]", _dtoNamespace);
          _dtoWriter.WriteLine("public class Class{0}", roleMap.classMap.name);
          _dtoWriter.WriteLine("{");
          _dtoWriter.Indent++;
          _dtoWriter.WriteLine();
          _dtoWriter.WriteLine("[DataMember(EmitDefaultValue=false)]");
          _dtoWriter.WriteLine("[XmlIgnore]");
          _dtoWriter.WriteLine("public string Identifier { get; set; }");

          ProcessClassMap(roleMap, dataObjectMaps);

          _dtoWriter.Indent--;
          _dtoWriter.WriteLine("}");
          _dtoWriter.WriteLine();
          _dtoWriter.WriteLine("[DataMember(Name = \"tpl_{0}_rdl_{1}\", EmitDefaultValue = false)]", roleMap.name, roleMap.classMap.name);
          _dtoWriter.WriteLine("[XmlIgnore]");
          _dtoWriter.WriteLine("public Class{1} tpl_{0}_rdl_{1} {{ get; set; }}", roleMap.name, roleMap.classMap.name);
          _dtoWriter.Indent--;
          _dtoWriter.WriteLine("}");

          _dtoWriter.WriteLine();
          _dtoWriter.WriteLine("[DataMember(EmitDefaultValue = false)]");
          _dtoWriter.WriteLine("[XmlIgnore]");
          _dtoWriter.WriteLine("public Template{0} tpl_{0} {{ get; set; }}", templateMap.name);
        }
      }
    }

    private void ProcessRoleMap(String templateName, RoleMap roleMap, List<DataObjectMap> dataObjectMaps, bool isDataMember)
    {
      foreach (DataObjectMap dataObjectMap in dataObjectMaps)
      {
        DataObject dataObject = GetDataObject(dataObjectMap.name);

        if (dataObject != null)
        {
          foreach (DataProperty dataProperty in dataObject.dataProperties)
          {
            if (roleMap.propertyName == dataProperty.propertyName)
            {
              ExtendedDataProperty extendedDataProperty = new ExtendedDataProperty();

              extendedDataProperty.propertyName = dataProperty.propertyName;
              extendedDataProperty.dataType = dataProperty.dataType;
              extendedDataProperty.isPropertyKey = dataProperty.isPropertyKey;
              extendedDataProperty.isRequired = dataProperty.isRequired;

              if (_templatePath == String.Empty)
              {
                extendedDataProperty.propertyPath = "tpl_" + templateName + "_tpl_" + roleMap.name;
                extendedDataProperty.dtoPropertyPath = "tpl:" + templateName + ".tpl:" + roleMap.name;
              }
              else
              {
                extendedDataProperty.propertyPath = _templatePath + "_tpl_" + templateName + "_tpl_" + roleMap.name;
                extendedDataProperty.dtoPropertyPath = _dtoTemplatePath + ".tpl:" + templateName + ".tpl:" + roleMap.name;
              }

              if (ContainsDataProperty(extendedDataProperty.propertyPath))
              {
                extendedDataProperty.propertyPath += _extendedDataProperties.Count;
                extendedDataProperty.dtoPropertyPath += _extendedDataProperties.Count;
              }

              extendedDataProperty.isDataMember = isDataMember;
              _extendedDataProperties.Add(extendedDataProperty);

              if (!isDataMember)
              {
                _initStatements.Add(_dataContractPath + ".tpl_" + templateName + "_tpl_" + roleMap.name + " = " + extendedDataProperty.propertyPath + ";");
                _dtoWriter.WriteLine();
                _dtoWriter.WriteLine("[DataMember(EmitDefaultValue = false)]");
                _dtoWriter.WriteLine("public {0} tpl_{1}_tpl_{2} {{ get; set; }}", dataProperty.dataType, templateName, roleMap.name);
              }

              break;
            }
          }

          break;
        }
        else
        {
          throw new Exception("Data object map \"" + dataObjectMap.name + "\" does not exist in data dictionary.");
        }
      }
    }

    private void ProcessClassMap(RoleMap roleMap, List<DataObjectMap> dataObjectMaps)
    {
      _classPath += ".Class" + roleMap.classMap.name;
      _dataContractPath += ".tpl_" + roleMap.name + "_rdl_" + roleMap.classMap.name;
      _templatePath += "_tpl_" + roleMap.name + "_rdl_" + roleMap.classMap.name;
      _dtoTemplatePath += ".tpl:" + roleMap.name + ".rdl:" + roleMap.classMap.name;
      _initStatements.Add(_dataContractPath + " = new " + _classPath + "();");

      string combinedClassId = String.Empty;
      string[] classIds = roleMap.classMap.identifier.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

      foreach (string classId in classIds)
      {
        if (combinedClassId != String.Empty)
        {
          combinedClassId += " + ";
        }

        combinedClassId += "((GetPropertyValueByInternalName(\"" + classId.Trim() + "\") != null) ? GetPropertyValueByInternalName(\"" + classId.Trim() + "\").ToString() : \"\")";
      }

      _initStatements.Add(_dataContractPath + ".Identifier = " + combinedClassId + ";");

      string lastClassMapPath = _classPath;
      string lastDataContractPath = _dataContractPath;
      string lastTemplateMapPath = _templatePath;
      string lastDtoTemplateMapPath = _dtoTemplatePath;

      foreach (TemplateMap templateMap in roleMap.classMap.templateMaps)
      {
        _classPath = lastClassMapPath;
        _dataContractPath = lastDataContractPath;
        _templatePath = lastTemplateMapPath;
        _dtoTemplatePath = lastDtoTemplateMapPath;
        ProcessTemplateMap(templateMap, dataObjectMaps, false);
      }
    }
  }
}
