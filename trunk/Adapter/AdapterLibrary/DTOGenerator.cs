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
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Reflection;
using org.iringtools.library;
using org.iringtools.utility;
using log4net;
using Ninject.Contrib.Dynamic;

namespace org.iringtools.adapter
{
  public class DTOGenerator
  {
    private const string INDENTATION = "  ";
    private const string COMPILER_VERSION = "v3.5";
    private const string LIBRARY_NAMESPACE = "org.iringtools.library";
    private const string ADAPTER_NAMESPACE = "org.iringtools.adapter";
    private const string RDL_NAMESPACE = "http://rdl.rdlfacade.org/data#";
    private const string TPL_NAMESPACE = "http://tpl.rdlfacade.org/data#";

    private static readonly ILog _logger = LogManager.GetLogger(typeof(DTOGenerator));

    private AdapterSettings _settings = null;
    private Mapping _mapping = null;
    private DataDictionary _dataDictionary = null;
    private List<MappingProperty> _mappingProperties = null;
    private List<string> _initStatements = null;
    private StringBuilder _dtoModelBuilder = null;
    private IndentedTextWriter _dtoModelWriter = null;
    private StringBuilder _dtoServiceBuilder = null;

    private string _classNamespace = String.Empty;
    private string _xmlNamespace = String.Empty;
    private string _classPath = string.Empty;
    private string _dataContractPath = string.Empty;
    private string _templatePath = string.Empty;
    private string _dtoTemplatePath = string.Empty;

    public DTOGenerator(AdapterSettings settings)
    {
      _settings = settings;
      _mappingProperties = new List<MappingProperty>();
      _initStatements = new List<string>();
    }

    public void Generate(string projectName, string applicationName)
    {
      try
      {
        string scope = projectName + "." + applicationName;
        string mappingPath = _settings.XmlPath + "Mapping." + scope + ".xml";
        _mapping = Utility.Read<Mapping>(mappingPath, false);

        string dataDictionaryPath = _settings.XmlPath + "DataDictionary." + scope + ".xml";
        _dataDictionary = Utility.Read<DataDictionary>(dataDictionaryPath, true);

        _classNamespace = ADAPTER_NAMESPACE + ".proj_" + scope;
        _xmlNamespace = "http://" + applicationName + ".iringtools.org/" + projectName + "/data#";

        Dictionary<string, string> compilerOptions = new Dictionary<string, string>();
        compilerOptions.Add("CompilerVersion", COMPILER_VERSION);

        CompilerParameters parameters = new CompilerParameters();
        parameters.GenerateExecutable = false;
        parameters.ReferencedAssemblies.Add("System.dll");
        parameters.ReferencedAssemblies.Add("System.Configuration.dll");
        parameters.ReferencedAssemblies.Add("System.Core.dll");
        parameters.ReferencedAssemblies.Add("System.Runtime.Serialization.dll");
        parameters.ReferencedAssemblies.Add("System.ServiceModel.dll");
        parameters.ReferencedAssemblies.Add("System.ServiceModel.Web.dll");
        parameters.ReferencedAssemblies.Add("System.Xml.dll");
        parameters.ReferencedAssemblies.Add("System.Xml.Linq.dll");
        parameters.ReferencedAssemblies.Add(_settings.BinaryPath + "Iesi.Collections.dll");
        parameters.ReferencedAssemblies.Add(_settings.BinaryPath + "log4net.dll");
        parameters.ReferencedAssemblies.Add(_settings.BinaryPath + "Ninject.dll");
        parameters.ReferencedAssemblies.Add(_settings.BinaryPath + "Microsoft.ServiceModel.Web.dll");
        parameters.ReferencedAssemblies.Add(_settings.BinaryPath + "iRINGLibrary.dll");
        parameters.ReferencedAssemblies.Add(_settings.BinaryPath + "UtilityLibrary.dll");
        parameters.ReferencedAssemblies.Add(_settings.BinaryPath + "AdapterLibrary.dll");

        AddCustomDataLayerAssembly(projectName, applicationName, parameters);

        // Generate code
        //List<string> serviceKnownTypes = GetServiceKnownTypes(projectName, applicationName);
        string dtoModel = GenerateDTOModel(projectName, applicationName);
        string dtoService = GenerateDTOService(projectName, applicationName);

        #region Compile code
        List<string> sources = new List<string>();

        // Add services code
        sources.Add(Utility.ReadString(_settings.CodePath + "IService.cs"));
        sources.Add(Utility.ReadString(_settings.CodePath + "Service.cs"));
        sources.Add(Utility.ReadString(_settings.CodePath + "IDataService.cs"));
        sources.Add(Utility.ReadString(_settings.CodePath + "DataService.cs"));

        // Add models and DTOmodels for other apps.
        List<KeyValuePair<string, ScopeApplication>> scopeApps = GetScopeApplications();
        foreach (KeyValuePair<string, ScopeApplication> scopeApp in scopeApps)
        {
          string modelPath = _settings.CodePath + "Model." + scopeApp.Key + "." + scopeApp.Value.Name + ".cs";
          if (File.Exists(modelPath))
          {
            sources.Add(Utility.ReadString(modelPath));
          }

          if (scopeApp.Key != projectName || scopeApp.Value.Name != applicationName)
          {
            string dtoModelPath = _settings.CodePath + "DTOModel." + scopeApp.Key + "." + scopeApp.Value.Name + ".cs";
            if (File.Exists(dtoModelPath))
            {
              sources.Add(Utility.ReadString(dtoModelPath));
            }

            AddCustomDataLayerAssembly(scopeApp.Key, scopeApp.Value.Name, parameters);
          }
        }

        // Add generated code
        sources.Add(dtoModel);
        sources.Add(dtoService);

        // Do compile
        Utility.Compile(compilerOptions, parameters, sources.ToArray());
        #endregion

        // Write generated code to disk
        Utility.WriteString(dtoModel, _settings.CodePath + "DTOModel." + scope + ".cs", Encoding.ASCII);
        Utility.WriteString(dtoService, _settings.CodePath + "DTOService." + scope + ".cs", Encoding.ASCII);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());

        if (_dtoModelBuilder != null)
        {
          _logger.Error("DTOModel.cs:");
          _logger.Error(_dtoModelBuilder.ToString());
        }

        if (_dtoServiceBuilder != null)
        {
          _logger.Error("DTOService.cs:");
          _logger.Error(_dtoServiceBuilder.ToString());
        }

        throw ex;
      }
    }

    private string GenerateDTOModel(string projectName, string applicationName)
    {
      try
      {
        _dtoModelBuilder = new StringBuilder();
        _dtoModelWriter = new IndentedTextWriter(new StringWriter(_dtoModelBuilder), INDENTATION);

        _dtoModelWriter.WriteLine(Utility.GeneratedCodeProlog);
        _dtoModelWriter.WriteLine("using System;");
        _dtoModelWriter.WriteLine("using System.Collections.Generic;");
        _dtoModelWriter.WriteLine("using System.Runtime.Serialization;");
        _dtoModelWriter.WriteLine("using System.Xml.Serialization;");
        _dtoModelWriter.WriteLine("using System.Xml.Xsl;");
        _dtoModelWriter.WriteLine("using log4net;");
        _dtoModelWriter.WriteLine("using org.iringtools.library;");
        _dtoModelWriter.WriteLine("using org.iringtools.utility;");

        _dtoModelWriter.WriteLine();
        _dtoModelWriter.WriteLine("namespace {0}", _classNamespace);
        _dtoModelWriter.Write("{");
        _dtoModelWriter.Indent++;

        foreach (GraphMap graphMap in _mapping.graphMaps)
        {
          graphMap.name = Utility.NameSafe(graphMap.name);
          graphMap.classId = graphMap.classId.Replace("rdl:", RDL_NAMESPACE);

          _dtoModelWriter.WriteLine();
          _dtoModelWriter.WriteLine("[DataContract(Name = \"{0}\", Namespace = \"{1}\" )]", graphMap.name, _xmlNamespace);
          _dtoModelWriter.WriteLine("[XmlRoot(Namespace = \"{0}\")]", _xmlNamespace);
          _dtoModelWriter.WriteLine("public class {0} : DataTransferObject", graphMap.name);
          _dtoModelWriter.Write("{");
          _dtoModelWriter.Indent++;
          _dtoModelWriter.WriteLine();
          _dtoModelWriter.WriteLine("private static readonly ILog _logger = LogManager.GetLogger(typeof({0}));", graphMap.name);

          ProcessGraphMap(graphMap);

          _dtoModelWriter.WriteLine();
          _dtoModelWriter.WriteLine("public {0}(string classId, string graphName, string identifier) : base(classId, graphName)", graphMap.name);
          _dtoModelWriter.WriteLine("{");
          _dtoModelWriter.Indent++;

          foreach (MappingProperty mappingProperty in _mappingProperties)
          {
            string value = (mappingProperty.value == null) ? "null" : "@\"" + mappingProperty.value + "\"";

            if (!String.IsNullOrEmpty(mappingProperty.propertyName) && graphMap.identifier.ToLower() == mappingProperty.propertyName.ToLower())
            {
              _dtoModelWriter.WriteLine("_properties.Add(new DTOProperty(@\"{0}\", @\"{1}\", {2}, typeof({3}), {4}, {5}));",
              mappingProperty.propertyName, mappingProperty.dtoPropertyPath, "identifier", mappingProperty.mappingDataType,
              "true", Convert.ToString(mappingProperty.isRequired).ToLower());
            }
            else
            {
              _dtoModelWriter.WriteLine("_properties.Add(new DTOProperty(@\"{0}\", @\"{1}\", {2}, typeof({3}), {4}, {5}));",
              mappingProperty.propertyName, mappingProperty.dtoPropertyPath, value, mappingProperty.mappingDataType,
              "false", Convert.ToString(mappingProperty.isRequired).ToLower());
            }
          }

          _dtoModelWriter.WriteLine("Identifier = identifier;");
          _dtoModelWriter.WriteLine("ClassId = classId;");
          _dtoModelWriter.Indent--;
          _dtoModelWriter.WriteLine("}");

          foreach (DataObjectMap dataObjectMap in graphMap.dataObjectMaps)
          {
            string qualifiedDataObjectName = GetQualifiedDataObjectName(dataObjectMap.name);

            _dtoModelWriter.WriteLine();
            _dtoModelWriter.WriteLine("public {0}(IDataObject dataObject) : this(\"{1}\", \"{0}\", null, dataObject) {{}}", graphMap.name, graphMap.classId);

            _dtoModelWriter.WriteLine();
            _dtoModelWriter.WriteLine("public {0}(string classId, string graphName, string identifier, IDataObject dataObject) : this(classId, graphName, identifier)", graphMap.name);
            _dtoModelWriter.WriteLine("{");
            _dtoModelWriter.Indent++;
            _dtoModelWriter.WriteLine("if (dataObject != null)");
            _dtoModelWriter.WriteLine("{");
            _dtoModelWriter.Indent++;

            foreach (MappingProperty mappingProperty in _mappingProperties)
            {
              if (!String.IsNullOrEmpty(mappingProperty.propertyName))
              {
                //TODO: handle multi-column key
                _dtoModelWriter.WriteLine("{0} = Convert.To{1}(dataObject.GetPropertyValue(\"{2}\"));", mappingProperty.propertyPath, mappingProperty.mappingDataType, mappingProperty.propertyName);
              }
            }

            _dtoModelWriter.Indent--;
            _dtoModelWriter.WriteLine("}");

            foreach (string initStatement in _initStatements)
            {
              _dtoModelWriter.WriteLine(initStatement);
            }

            _dtoModelWriter.WriteLine("_dataObject = dataObject;");
            _dtoModelWriter.Indent--;
            _dtoModelWriter.WriteLine("}");
          }

          _dtoModelWriter.WriteLine();
          _dtoModelWriter.WriteLine("public {0}() : this(\"{1}\", \"{0}\", null) {{}}", graphMap.name, graphMap.classId);

          // Generate data contract member methods
          foreach (MappingProperty mappingProperty in _mappingProperties)
          {
            String type = mappingProperty.mappingDataType;

            if (type.ToLower() != "string")
            {
              if (!String.IsNullOrEmpty(mappingProperty.propertyName))
              {
                if (!mappingProperty.isRequired)
                {
                  type = type + "?";
                }
              }
              else
              {
                type = type + "?";
              }
            }

            _dtoModelWriter.WriteLine();

            if (mappingProperty.isDataMember)
            {
              _dtoModelWriter.WriteLine("[DataMember(Name = \"{0}\", EmitDefaultValue = false)]", mappingProperty.propertyPath);
            }

            _dtoModelWriter.WriteLine("[XmlIgnore]");
            _dtoModelWriter.WriteLine("public {0} {1}", type, mappingProperty.propertyPath);
            _dtoModelWriter.WriteLine("{");
            _dtoModelWriter.Indent++;
            _dtoModelWriter.WriteLine("get");
            _dtoModelWriter.WriteLine("{");
            _dtoModelWriter.Indent++;
            _dtoModelWriter.WriteLine("return ({0})GetPropertyValue(\"{1}\");", type, mappingProperty.dtoPropertyPath);
            _dtoModelWriter.Indent--;
            _dtoModelWriter.WriteLine("}");
            _dtoModelWriter.WriteLine("set");
            _dtoModelWriter.WriteLine("{");
            _dtoModelWriter.Indent++;
            _dtoModelWriter.WriteLine("SetPropertyValue(@\"{0}\", value);", mappingProperty.dtoPropertyPath);
            _dtoModelWriter.Indent--;
            _dtoModelWriter.WriteLine("}");
            _dtoModelWriter.Indent--;
            _dtoModelWriter.WriteLine("}");
          }

          _dtoModelWriter.WriteLine();
          _dtoModelWriter.WriteLine("public override object GetDataObject()");
          _dtoModelWriter.WriteLine("{");
          _dtoModelWriter.Indent++;

          int dataObjectMapCount = 0;

          foreach (DataObjectMap dataObjectMap in graphMap.dataObjectMaps)
          {
            string qualifiedDataObjectName = GetQualifiedDataObjectName(dataObjectMap.name);

            if (!String.IsNullOrEmpty(dataObjectMap.inFilter))
            {
              // Determine whether "if" or "else if" to use
              if (++dataObjectMapCount == 1)
              {
                _dtoModelWriter.WriteLine("if ({0}) // inFilter", dataObjectMap.inFilter);
                _dtoModelWriter.WriteLine("{");
                _dtoModelWriter.Indent++;
              }
              else
              {
                _dtoModelWriter.WriteLine("else if ({0}) // inFilter", dataObjectMap.inFilter);
                _dtoModelWriter.WriteLine("{");
                _dtoModelWriter.Indent++;
              }
            }

            _dtoModelWriter.WriteLine("if (_dataObject == null)");
            _dtoModelWriter.WriteLine("{");
            _dtoModelWriter.Indent++;
            _dtoModelWriter.WriteLine("_dataObject = new {0}();", qualifiedDataObjectName);

            foreach (MappingProperty mappingProperty in _mappingProperties)
            {
              //TODO: handle multi-column key
              if (mappingProperty.isPropertyKey)
              {
                if (mappingProperty.dataType.ToLower() == "string" &&
                    mappingProperty.mappingDataType.ToLower() == "string" &&
                    !String.IsNullOrEmpty(mappingProperty.dataLength))
                {
                  _dtoModelWriter.WriteLine();
                  _dtoModelWriter.WriteLine("if (!String.IsNullOrEmpty(this.Identifier) && this.Identifier.Length > {0})", mappingProperty.dataLength);
                  _dtoModelWriter.WriteLine("{");
                  _dtoModelWriter.Indent++;
                  _dtoModelWriter.WriteLine("_logger.Warn(\"Truncate {0} value from ---\" + this.Identifier + \"--- to {1} characters.\");",
                    mappingProperty.propertyName, mappingProperty.dataLength);
                  _dtoModelWriter.WriteLine("this.Identifier = this.Identifier.Substring(0, {0});", mappingProperty.dataLength);
                  _dtoModelWriter.Indent--;
                  _dtoModelWriter.WriteLine("}");
                }

                _dtoModelWriter.WriteLine("(({0})_dataObject).{1} = Convert.To{2}(this.Identifier);", qualifiedDataObjectName, mappingProperty.propertyName, mappingProperty.dataType);
              }
            }

            _dtoModelWriter.Indent--;
            _dtoModelWriter.WriteLine("}");

            foreach (MappingProperty mappingProperty in _mappingProperties)
            {
              if (!mappingProperty.isPropertyKey && !String.IsNullOrEmpty(mappingProperty.propertyName))
              {
                if (mappingProperty.dataType.ToLower() == "string" &&
                    mappingProperty.mappingDataType.ToLower() == "string" &&
                    !String.IsNullOrEmpty(mappingProperty.dataLength))
                {
                  _dtoModelWriter.WriteLine();
                  _dtoModelWriter.WriteLine("if (!String.IsNullOrEmpty(this.{0}) && this.{0}.Length > {1})", mappingProperty.propertyPath, mappingProperty.dataLength);
                  _dtoModelWriter.WriteLine("{");
                  _dtoModelWriter.Indent++;
                  _dtoModelWriter.WriteLine("_logger.Warn(\"Truncate {0} value from ---\" + this.{1} + \"--- to {2} characters.\");",
                    mappingProperty.propertyName, mappingProperty.propertyPath, mappingProperty.dataLength);
                  _dtoModelWriter.WriteLine("this.{0} = this.{0}.Substring(0, {1});", mappingProperty.propertyPath, mappingProperty.dataLength);
                  _dtoModelWriter.Indent--;
                  _dtoModelWriter.WriteLine("}");
                }

                _dtoModelWriter.WriteLine("(({0})_dataObject).{1} = Convert.To{2}(this.{3});",
                  qualifiedDataObjectName, mappingProperty.propertyName, mappingProperty.dataType, mappingProperty.propertyPath);
              }
            }

            if (!String.IsNullOrEmpty(dataObjectMap.inFilter))
            {
              _dtoModelWriter.Indent--;
              _dtoModelWriter.WriteLine("}");
            }
          }

          _dtoModelWriter.WriteLine("return _dataObject;");
          _dtoModelWriter.Indent--;
          _dtoModelWriter.WriteLine("}");

          _dtoModelWriter.WriteLine();
          _dtoModelWriter.WriteLine("public override string Serialize()");
          _dtoModelWriter.WriteLine("{");
          _dtoModelWriter.Indent++;
          _dtoModelWriter.WriteLine("return Utility.SerializeDataContract<{0}>(this);", graphMap.name);
          _dtoModelWriter.Indent--;
          _dtoModelWriter.WriteLine("}");

          _dtoModelWriter.WriteLine();
          _dtoModelWriter.WriteLine("public override void Write(string path)");
          _dtoModelWriter.WriteLine("{");
          _dtoModelWriter.Indent++;
          _dtoModelWriter.WriteLine("Utility.Write<{0}>(this, path);", graphMap.name);
          _dtoModelWriter.Indent--;
          _dtoModelWriter.WriteLine("}");

          _dtoModelWriter.WriteLine();
          _dtoModelWriter.WriteLine("public override T Transform<T>(string xmlPath, string stylesheetUri, string mappingUri, bool useDataContractDeserializer)");
          _dtoModelWriter.WriteLine("{");
          _dtoModelWriter.Indent++;
          _dtoModelWriter.WriteLine("string dtoPath = xmlPath + this.GraphName + \".xml\";");
          _dtoModelWriter.WriteLine("Mapping mapping = Utility.Read<Mapping>(mappingUri, false);");
          _dtoModelWriter.WriteLine("List<{0}> list = new List<{0}> {{ this }};", graphMap.name);
          _dtoModelWriter.WriteLine("Utility.Write<List<{0}>>(list, dtoPath);", graphMap.name);
          _dtoModelWriter.WriteLine("XsltArgumentList xsltArgumentList = new XsltArgumentList();");
          _dtoModelWriter.WriteLine("xsltArgumentList.AddParam(\"dtoFilename\", String.Empty, dtoPath);");
          _dtoModelWriter.WriteLine("return Utility.Transform<Mapping, T>(mapping, stylesheetUri, xsltArgumentList, false, useDataContractDeserializer);");
          _dtoModelWriter.Indent--;
          _dtoModelWriter.WriteLine("}");
          _dtoModelWriter.Indent--;
          _dtoModelWriter.WriteLine("}");
        }

        _dtoModelWriter.Indent--;
        _dtoModelWriter.WriteLine("}");
        _dtoModelWriter.Close();

        return _dtoModelBuilder.ToString();
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    private string GenerateDTOService(string projectName, string applicationName)
    {
      string namespacePrefix = ADAPTER_NAMESPACE + ".proj_" + projectName + "." + applicationName;

      try
      {
        _dtoServiceBuilder = new StringBuilder();
        IndentedTextWriter dtoServiceWriter = new IndentedTextWriter(new StringWriter(_dtoServiceBuilder), INDENTATION);

        dtoServiceWriter.WriteLine(Utility.GeneratedCodeProlog);
        dtoServiceWriter.WriteLine("using System;");
        dtoServiceWriter.WriteLine("using System.Collections.Generic;");
        dtoServiceWriter.WriteLine("using System.IO;");
        dtoServiceWriter.WriteLine("using System.Linq;");
        dtoServiceWriter.WriteLine("using System.ServiceModel;");
        dtoServiceWriter.WriteLine("using System.Xml;");
        dtoServiceWriter.WriteLine("using System.Xml.Linq;");
        dtoServiceWriter.WriteLine("using System.Xml.Xsl;");
        dtoServiceWriter.WriteLine("using System.Xml.Serialization;");
        dtoServiceWriter.WriteLine("using Ninject;");
        dtoServiceWriter.WriteLine("using org.iringtools.library;");
        dtoServiceWriter.WriteLine("using org.iringtools.utility;");
        dtoServiceWriter.WriteLine("using Microsoft.ServiceModel.Web;");
        dtoServiceWriter.WriteLine("using org.ids_adi.qxf;");
        dtoServiceWriter.WriteLine();
        dtoServiceWriter.WriteLine("namespace {0}", _classNamespace);
        dtoServiceWriter.WriteLine("{");
        dtoServiceWriter.Indent++;
        dtoServiceWriter.WriteLine("public class DTOService : IDTOLayer");
        dtoServiceWriter.WriteLine("{");
        dtoServiceWriter.Indent++;
        dtoServiceWriter.WriteLine("IKernel _kernel = null;");
        dtoServiceWriter.WriteLine("IDataLayer _dataLayer = null;");
        dtoServiceWriter.WriteLine("AdapterSettings _adapterSettings = null;");
        dtoServiceWriter.WriteLine("ApplicationSettings _applicationSettings = null;");

        dtoServiceWriter.WriteLine();
        dtoServiceWriter.WriteLine("[Inject]");
        dtoServiceWriter.WriteLine("public DTOService(IKernel kernel, IDataLayer dataLayer, AdapterSettings adapterSettings, ApplicationSettings applicationSettings)");
        dtoServiceWriter.WriteLine("{");
        dtoServiceWriter.Indent++;
        dtoServiceWriter.WriteLine("_kernel = kernel;");
        dtoServiceWriter.WriteLine("_dataLayer = dataLayer;");
        dtoServiceWriter.WriteLine("_adapterSettings = adapterSettings;");
        dtoServiceWriter.WriteLine("_applicationSettings = applicationSettings;");
        dtoServiceWriter.Indent--;
        dtoServiceWriter.WriteLine("}");

        dtoServiceWriter.WriteLine();
        dtoServiceWriter.WriteLine("public Response CreateRDF(string graphName, List<DataTransferObject> dtoList)");
        dtoServiceWriter.WriteLine("{");
        dtoServiceWriter.Indent++;
        dtoServiceWriter.WriteLine("Response response = new Response();");
        dtoServiceWriter.WriteLine("try");
        dtoServiceWriter.Write("{");
        dtoServiceWriter.Indent++;
        dtoServiceWriter.WriteLine(@"
        string scope = _applicationSettings.ProjectName + ""."" + _applicationSettings.ApplicationName;
        string mappingPath = _adapterSettings.XmlPath + ""Mapping."" + scope + "".xml"";
        string dto2qxfPath = _adapterSettings.BaseDirectoryPath + @""Transforms\dto2qxf.xsl"";
        string qxf2rdfPath = _adapterSettings.BaseDirectoryPath + @""Transforms\qxf2rdf.xsl"";

        string dtoFilePath = _adapterSettings.XmlPath + ""DTO."" + scope + ""."" + graphName + "".xml"";
        string qxfPath = _adapterSettings.XmlPath + ""QXF."" + scope + ""."" + graphName + "".xml"";
        string rdfFileName = ""RDF."" + scope + ""."" + graphName + "".xml"";
        string rdfPath = _adapterSettings.XmlPath + rdfFileName;

        Mapping mapping = Utility.Read<Mapping>(mappingPath, false);"
        );

        dtoServiceWriter.WriteLine();
        dtoServiceWriter.WriteLine("switch (graphName)");
        dtoServiceWriter.Write("{");
        dtoServiceWriter.Indent++;

        foreach (GraphMap graphMap in _mapping.graphMaps)
        {
          string qualifiedGraphName = namespacePrefix + "." + graphMap.name;

          dtoServiceWriter.WriteLine();
          dtoServiceWriter.WriteLine("case \"{0}\":", graphMap.name);
          dtoServiceWriter.WriteLine("{");
          dtoServiceWriter.Indent++;
          dtoServiceWriter.WriteLine("List<{0}> theDTOList = new List<{0}>();", qualifiedGraphName);
          dtoServiceWriter.WriteLine();
          dtoServiceWriter.WriteLine("foreach (DataTransferObject dto in dtoList)");
          dtoServiceWriter.WriteLine("{");
          dtoServiceWriter.Indent++;
          dtoServiceWriter.WriteLine("theDTOList.Add(({0})dto);", qualifiedGraphName);
          dtoServiceWriter.Indent--;
          dtoServiceWriter.WriteLine("}");
          dtoServiceWriter.WriteLine();
          dtoServiceWriter.WriteLine("Utility.Write<List<{0}>>(theDTOList, dtoFilePath, false);", qualifiedGraphName);
          dtoServiceWriter.WriteLine("break;");
          dtoServiceWriter.Indent--;
          dtoServiceWriter.WriteLine("}");
        }

        dtoServiceWriter.Indent--;
        dtoServiceWriter.WriteLine("}");

        dtoServiceWriter.WriteLine(@"
        XsltArgumentList xsltArgumentList = new XsltArgumentList();
        xsltArgumentList.AddParam(""dtoFilePath"", String.Empty, dtoFilePath);
        xsltArgumentList.AddParam(""graphName"", String.Empty, graphName);

        // Transform mapping + dto to qxf
        QXF qxf = Utility.Transform<Mapping, QXF>(mapping, dto2qxfPath, xsltArgumentList, false);
        response.Add(""Transform DTOList to QXF successfully."");

        // Write qxf to file
        Utility.Write<QXF>(qxf, qxfPath, false);

        // Transform qxf to rdf
        Stream rdf = Utility.Transform<QXF>(qxf, qxf2rdfPath, false);
        response.Add(""Transform QXF to RDF successfully."");

        // Write rdf to file
        Utility.WriteStream(rdf, rdfPath);
        response.Add(""RDF file ["" + rdfFileName + ""] created successfully."");

        response.Level = StatusLevel.Success;
        return response;"
        );

        dtoServiceWriter.Indent--;
        dtoServiceWriter.WriteLine("}");
        dtoServiceWriter.WriteLine("catch (Exception ex)");
        dtoServiceWriter.WriteLine("{");
        dtoServiceWriter.Indent++;
        dtoServiceWriter.WriteLine("throw ex;");
        dtoServiceWriter.Indent--;
        dtoServiceWriter.WriteLine("}");
        dtoServiceWriter.Indent--;
        dtoServiceWriter.WriteLine("}");

        dtoServiceWriter.WriteLine();
        dtoServiceWriter.WriteLine("public XElement SerializeDTO(string graphName, List<DataTransferObject> dtoList)");
        dtoServiceWriter.WriteLine("{");
        dtoServiceWriter.Indent++;
        dtoServiceWriter.WriteLine("XElement element = null;");
        dtoServiceWriter.WriteLine("try");
        dtoServiceWriter.WriteLine("{");
        dtoServiceWriter.Indent++;
        dtoServiceWriter.WriteLine("switch (graphName)");
        dtoServiceWriter.WriteLine("{");
        dtoServiceWriter.Indent++;

        foreach (GraphMap graphMap in _mapping.graphMaps)
        {
          string qualifiedGraphName = namespacePrefix + "." + graphMap.name;

          dtoServiceWriter.WriteLine("case \"{0}\":", graphMap.name);
          dtoServiceWriter.WriteLine("{");
          dtoServiceWriter.Indent++;
          dtoServiceWriter.WriteLine("List<{0}> theDTOList = new List<{0}>();", qualifiedGraphName);
          dtoServiceWriter.WriteLine();
          dtoServiceWriter.WriteLine("foreach (DataTransferObject dto in dtoList)");
          dtoServiceWriter.WriteLine("{");
          dtoServiceWriter.Indent++;
          dtoServiceWriter.WriteLine("theDTOList.Add(({0})dto);", qualifiedGraphName);
          dtoServiceWriter.Indent--;
          dtoServiceWriter.WriteLine("}");
          dtoServiceWriter.WriteLine();
          dtoServiceWriter.WriteLine("XmlSerializer serializer = new XmlSerializer(typeof(List<{0}>));", qualifiedGraphName);
          dtoServiceWriter.WriteLine("element = SerializationExtensions.ToXml<List<{0}>>(theDTOList, serializer);", qualifiedGraphName);
          dtoServiceWriter.WriteLine("break;");
          dtoServiceWriter.Indent--;
          dtoServiceWriter.WriteLine("}");
        }

        dtoServiceWriter.Indent--;
        dtoServiceWriter.WriteLine("}");
        dtoServiceWriter.WriteLine("return element;");
        dtoServiceWriter.Indent--;
        dtoServiceWriter.WriteLine("}");
        dtoServiceWriter.WriteLine("catch (Exception ex)");
        dtoServiceWriter.WriteLine("{");
        dtoServiceWriter.Indent++;
        dtoServiceWriter.WriteLine("throw new Exception(\"Error while serializing DTO.\", ex);");
        dtoServiceWriter.Indent--;
        dtoServiceWriter.WriteLine("}");
        dtoServiceWriter.Indent--;
        dtoServiceWriter.WriteLine("}");

        dtoServiceWriter.WriteLine();
        dtoServiceWriter.WriteLine("public XElement SerializeXML(string graphName, List<DataTransferObject> dtoList)");
        dtoServiceWriter.WriteLine("{");
        dtoServiceWriter.Indent++;
        dtoServiceWriter.WriteLine("XElement element = null;");
        dtoServiceWriter.WriteLine("try");
        dtoServiceWriter.WriteLine("{");
        dtoServiceWriter.Indent++;
        dtoServiceWriter.WriteLine("switch (graphName)");
        dtoServiceWriter.WriteLine("{");
        dtoServiceWriter.Indent++;

        foreach (GraphMap graphMap in _mapping.graphMaps)
        {
          string qualifiedGraphName = namespacePrefix + "." + graphMap.name;

          dtoServiceWriter.WriteLine("case \"{0}\":", graphMap.name);
          dtoServiceWriter.WriteLine("{");
          dtoServiceWriter.Indent++;
          dtoServiceWriter.WriteLine("List<{0}> theDTOList = new List<{0}>();", qualifiedGraphName);
          dtoServiceWriter.WriteLine();
          dtoServiceWriter.WriteLine("foreach (DataTransferObject dto in dtoList)");
          dtoServiceWriter.WriteLine("{");
          dtoServiceWriter.Indent++;
          dtoServiceWriter.WriteLine("theDTOList.Add(({0})dto);", qualifiedGraphName);
          dtoServiceWriter.Indent--;
          dtoServiceWriter.WriteLine("}");
          dtoServiceWriter.WriteLine();
          dtoServiceWriter.WriteLine("element = SerializationExtensions.ToXml<List<{0}>>(theDTOList);", qualifiedGraphName);
          dtoServiceWriter.WriteLine("break;");
          dtoServiceWriter.Indent--;
          dtoServiceWriter.WriteLine("}");
        }

        dtoServiceWriter.Indent--;
        dtoServiceWriter.WriteLine("}");
        dtoServiceWriter.WriteLine("return element;");
        dtoServiceWriter.Indent--;
        dtoServiceWriter.WriteLine("}");
        dtoServiceWriter.WriteLine("catch (Exception ex)");
        dtoServiceWriter.WriteLine("{");
        dtoServiceWriter.Indent++;
        dtoServiceWriter.WriteLine("throw new Exception(\"Error while serializing DTOList.\", ex);");
        dtoServiceWriter.Indent--;
        dtoServiceWriter.WriteLine("}");
        dtoServiceWriter.Indent--;
        dtoServiceWriter.WriteLine("}");

        dtoServiceWriter.WriteLine();
        dtoServiceWriter.WriteLine("public DataTransferObject Create(string graphName, string identifier)");
        dtoServiceWriter.WriteLine("{");
        dtoServiceWriter.Indent++;
        dtoServiceWriter.WriteLine("DataTransferObject dto = null;");
        dtoServiceWriter.WriteLine();
        dtoServiceWriter.WriteLine("switch (graphName)");
        dtoServiceWriter.Write("{");
        dtoServiceWriter.Indent++;

        foreach (GraphMap graphMap in _mapping.graphMaps)
        {
          string qualifiedGraphName = namespacePrefix + "." + graphMap.name;

          graphMap.classId = graphMap.classId.Replace("rdl:", "http://rdl.rdlfacade.org/data#");
          dtoServiceWriter.WriteLine();
          dtoServiceWriter.WriteLine("case \"{0}\":", graphMap.name);
          dtoServiceWriter.Indent++;
          dtoServiceWriter.WriteLine("dto = new {0}(\"{1}\", graphName, identifier);", qualifiedGraphName, graphMap.classId);
          dtoServiceWriter.WriteLine("break;");
          dtoServiceWriter.Indent--;
        }

        dtoServiceWriter.Indent--;
        dtoServiceWriter.WriteLine("}");
        dtoServiceWriter.WriteLine();
        dtoServiceWriter.WriteLine("return dto;");
        dtoServiceWriter.Indent--;
        dtoServiceWriter.WriteLine("}");

        dtoServiceWriter.WriteLine(@"
    public List<DataTransferObject> CreateList(string graphName, List<string> identifiers)
    {
      List<DataTransferObject> dtoList = new List<DataTransferObject>();

      foreach (string identifier in identifiers)
      {
        dtoList.Add(Create(graphName, identifier));
      }

      return dtoList;
    }");

        dtoServiceWriter.WriteLine();
        dtoServiceWriter.WriteLine("public DataTransferObject GetDTO(string graphName, string identifier)");
        dtoServiceWriter.WriteLine("{");
        dtoServiceWriter.Indent++;
        dtoServiceWriter.WriteLine("DataTransferObject dto = null;");
        dtoServiceWriter.WriteLine();
        dtoServiceWriter.WriteLine("switch (graphName)");
        dtoServiceWriter.Write("{");
        dtoServiceWriter.Indent++;

        foreach (GraphMap graphMap in _mapping.graphMaps)
        {
          string qualifiedGraphName = namespacePrefix + "." + graphMap.name;

          dtoServiceWriter.WriteLine();
          dtoServiceWriter.WriteLine("case \"{0}\":", graphMap.name);
          dtoServiceWriter.Write("{");
          dtoServiceWriter.Indent++;

          foreach (DataObjectMap dataObjectMap in graphMap.dataObjectMaps)
          {
            string qualifiedDataObjectName = GetQualifiedDataObjectName(dataObjectMap.name);

            //TODO: handle multi-column key and outFilter
            dtoServiceWriter.WriteLine(@"
          IDataObject dataObject = _dataLayer.Get(""{0}"", new List<string> {{ identifier }}).FirstOrDefault<IDataObject>();
          if (dataObject != null)
          {{
            dto = new {1}(dataObject);
            dto.Identifier = Convert.ToString(dataObject.GetPropertyValue(""{2}""));
          }}", qualifiedDataObjectName, qualifiedGraphName, graphMap.identifier);
          }

          dtoServiceWriter.WriteLine();
          dtoServiceWriter.WriteLine("break;");
          dtoServiceWriter.Indent--;
          dtoServiceWriter.WriteLine("}");
        }

        dtoServiceWriter.Indent--;
        dtoServiceWriter.WriteLine("}");
        dtoServiceWriter.WriteLine();
        dtoServiceWriter.WriteLine("return dto;");
        dtoServiceWriter.Indent--;
        dtoServiceWriter.WriteLine("}");

        dtoServiceWriter.WriteLine();
        dtoServiceWriter.WriteLine("public List<DataTransferObject> GetList(string graphName)");
        dtoServiceWriter.WriteLine("{");
        dtoServiceWriter.Indent++;
        dtoServiceWriter.WriteLine("List<DataTransferObject> dtoList = new List<DataTransferObject>();");
        dtoServiceWriter.WriteLine();
        dtoServiceWriter.WriteLine("switch (graphName)");
        dtoServiceWriter.Write("{");
        dtoServiceWriter.Indent++;

        foreach (GraphMap graphMap in _mapping.graphMaps)
        {
          string qualifiedGraphName = namespacePrefix + "." + graphMap.name;

          dtoServiceWriter.WriteLine();
          dtoServiceWriter.WriteLine("case \"{0}\":", graphMap.name);
          dtoServiceWriter.Write("{");
          dtoServiceWriter.Indent++;

          foreach (DataObjectMap dataObjectMap in graphMap.dataObjectMaps)
          {
            string qualifiedDataObjectName = GetQualifiedDataObjectName(dataObjectMap.name);

            //TODO: handle multi-column key
            if (!String.IsNullOrEmpty(dataObjectMap.outFilter))
            {
              //TODO: apploy outFilter (must be DataFilter)
              string outFilter = dataObjectMap.outFilter.Substring(dataObjectMap.outFilter.IndexOf("_") + 1);

              dtoServiceWriter.WriteLine(@"
          IList<IDataObject> dataObjects = _dataLayer.Get(""{0}"", {1}, 0, 0);          
          foreach (IDataObject dataObject in dataObjects)
          {{
            {2} dto = new {2}(dataObject);
            dto.Identifier = Convert.ToString(dataObject.GetPropertyValue(""{3}""));
            dtoList.Add(dto);
          }}", qualifiedDataObjectName, outFilter, qualifiedGraphName, graphMap.identifier);
            }
            else
            {
              dtoServiceWriter.WriteLine(@"
          IList<IDataObject> dataObjects = _dataLayer.Get(""{0}"", null, 0, 0);          
          foreach (IDataObject dataObject in dataObjects)
          {{
            {1} dto = new {1}(dataObject);
            dto.Identifier = Convert.ToString(dataObject.GetPropertyValue(""{2}""));
            dtoList.Add(dto);
          }}", qualifiedDataObjectName, qualifiedGraphName, graphMap.identifier);
            }
          }

          dtoServiceWriter.WriteLine("break;");
          dtoServiceWriter.Indent--;
          dtoServiceWriter.WriteLine("}");
        }

        dtoServiceWriter.Indent--;
        dtoServiceWriter.WriteLine("}");
        dtoServiceWriter.WriteLine();
        dtoServiceWriter.WriteLine("return dtoList;");
        dtoServiceWriter.Indent--;
        dtoServiceWriter.WriteLine("}");

        dtoServiceWriter.WriteLine();
        dtoServiceWriter.WriteLine("public Dictionary<string, string> GetListREST(string graphName)");
        dtoServiceWriter.WriteLine("{");
        dtoServiceWriter.Indent++;
        dtoServiceWriter.WriteLine("Dictionary<string, string> dtoDictionary = new Dictionary<string, string>();");
        dtoServiceWriter.WriteLine("String endpoint = OperationContext.Current.Channel.LocalAddress.ToString();");
        dtoServiceWriter.WriteLine();
        dtoServiceWriter.WriteLine("switch (graphName)");
        dtoServiceWriter.Write("{");
        dtoServiceWriter.Indent++;

        foreach (GraphMap graphMap in _mapping.graphMaps)
        {
          string qualifiedGraphName = namespacePrefix + "." + graphMap.name;

          dtoServiceWriter.WriteLine();
          dtoServiceWriter.WriteLine("case \"{0}\":", graphMap.name);
          dtoServiceWriter.Write("{");
          dtoServiceWriter.Indent++;

          foreach (DataObjectMap dataObjectMap in graphMap.dataObjectMaps)
          {
            string qualifiedDataObjectName = GetQualifiedDataObjectName(dataObjectMap.name);

            //TODO: handle multi-column key
            if (!String.IsNullOrEmpty(dataObjectMap.outFilter))
            {
              //TODO: apply outFilter (must be DataFilter)
              String outFilter = dataObjectMap.outFilter.Substring(dataObjectMap.outFilter.IndexOf("_") + 1);

              dtoServiceWriter.WriteLine(@"
          IList<string> identifiers = _dataLayer.GetIdentifiers(""{0}"", {1});
          foreach (string identifier in identifiers)
          {{
            dtoDictionary.Add(identifier, endpoint + ""/"" + graphName + ""/"" + identifier);
          }}", qualifiedDataObjectName, outFilter);
            }
            else
            {
              dtoServiceWriter.WriteLine(@"
          IList<string> identifiers = _dataLayer.GetIdentifiers(""{0}"", null);
          foreach (string identifier in identifiers)
          {{
            dtoDictionary.Add(identifier, endpoint + ""/"" + graphName + ""/"" + identifier);
          }}", qualifiedDataObjectName);
            }
          }

          dtoServiceWriter.WriteLine();
          dtoServiceWriter.WriteLine("break;");
          dtoServiceWriter.Indent--;
          dtoServiceWriter.WriteLine("}");
        }

        dtoServiceWriter.Indent--;
        dtoServiceWriter.WriteLine("}");
        dtoServiceWriter.WriteLine();
        dtoServiceWriter.WriteLine("return dtoDictionary;");
        dtoServiceWriter.Indent--;
        dtoServiceWriter.WriteLine("}");

        dtoServiceWriter.WriteLine();
        dtoServiceWriter.WriteLine("public Response Post(string graphName, DataTransferObject dto)");
        dtoServiceWriter.WriteLine("{");
        dtoServiceWriter.Indent++;
        dtoServiceWriter.WriteLine("Response response = new Response();");
        dtoServiceWriter.WriteLine();
        dtoServiceWriter.WriteLine("if (dto != null)");
        dtoServiceWriter.WriteLine("{");
        dtoServiceWriter.Indent++;
        dtoServiceWriter.WriteLine("switch (graphName)");
        dtoServiceWriter.Write("{");
        dtoServiceWriter.Indent++;

        foreach (GraphMap graphMap in _mapping.graphMaps)
        {
          string qualifiedGraphName = namespacePrefix + "." + graphMap.name;

          dtoServiceWriter.WriteLine();
          dtoServiceWriter.WriteLine("case \"{0}\":", graphMap.name);
          dtoServiceWriter.Write("{");
          dtoServiceWriter.Indent++;
          
          //TODO: apply inFilter (must be DataFilter)
          dtoServiceWriter.WriteLine(@"
            IDataObject dataObject = (IDataObject)dto.GetDataObject();
            response.Append(_dataLayer.Post(new List<IDataObject>{dataObject}));
            break;");

          dtoServiceWriter.Indent--;
          dtoServiceWriter.WriteLine("}");
        }

        dtoServiceWriter.Indent--;
        dtoServiceWriter.WriteLine("}");
        dtoServiceWriter.Indent--;
        dtoServiceWriter.WriteLine("}");
        dtoServiceWriter.WriteLine();
        dtoServiceWriter.WriteLine("return response;");
        dtoServiceWriter.Indent--;
        dtoServiceWriter.WriteLine("}");

        dtoServiceWriter.WriteLine();
        dtoServiceWriter.WriteLine("public Response PostList(string graphName, List<DataTransferObject> dtoList)");
        dtoServiceWriter.WriteLine("{");
        dtoServiceWriter.Indent++;
        dtoServiceWriter.WriteLine("Response response = new Response();");
        dtoServiceWriter.WriteLine();
        dtoServiceWriter.WriteLine("if (dtoList != null && dtoList.Count<DataTransferObject>() > 0)");
        dtoServiceWriter.WriteLine("{");
        dtoServiceWriter.Indent++;
        dtoServiceWriter.WriteLine("switch (graphName)");
        dtoServiceWriter.Write("{");
        dtoServiceWriter.Indent++;

        foreach (GraphMap graphMap in _mapping.graphMaps)
        {
          string qualifiedGraphName = namespacePrefix + "." + graphMap.name;

          dtoServiceWriter.WriteLine();
          dtoServiceWriter.WriteLine("case \"{0}\":", graphMap.name);
          dtoServiceWriter.Write("{");
          dtoServiceWriter.Indent++;

          //TODO: apply inFilter (must be DataFilter)
          dtoServiceWriter.WriteLine(@"
            IList<IDataObject> dataObjects = new List<IDataObject>();
            foreach (DataTransferObject dto in dtoList)
            {
              dataObjects.Add((IDataObject)(dto.GetDataObject()));
            }
            response.Append(_dataLayer.Post(dataObjects));
            break;");

          dtoServiceWriter.Indent--;
          dtoServiceWriter.WriteLine("}");
        }

        dtoServiceWriter.Indent--;
        dtoServiceWriter.WriteLine("}");
        dtoServiceWriter.Indent--;
        dtoServiceWriter.WriteLine("}");
        dtoServiceWriter.WriteLine();
        dtoServiceWriter.WriteLine("return response;");
        dtoServiceWriter.Indent--;
        dtoServiceWriter.WriteLine("}");

        dtoServiceWriter.WriteLine();
        dtoServiceWriter.WriteLine("public object CreateList(string graphName, string dtoListString)");
        dtoServiceWriter.WriteLine("{");
        dtoServiceWriter.Indent++;
        dtoServiceWriter.WriteLine("List<DataTransferObject> dtoList = new List<DataTransferObject>();");
        dtoServiceWriter.WriteLine();
        dtoServiceWriter.WriteLine("if (dtoListString != null && dtoListString != String.Empty)");
        dtoServiceWriter.WriteLine("{");
        dtoServiceWriter.Indent++;
        dtoServiceWriter.WriteLine("switch (graphName)");
        dtoServiceWriter.Write("{");
        dtoServiceWriter.Indent++;

        foreach (GraphMap graphMap in _mapping.graphMaps)
        {
          string qualifiedGraphName = namespacePrefix + "." + graphMap.name;

          dtoServiceWriter.WriteLine();
          dtoServiceWriter.WriteLine(
        @"case ""{0}"":
          {{
            XmlReader reader = XmlReader.Create(new StringReader(dtoListString));
            XDocument file = XDocument.Load(reader);
            file = Utility.RemoveNamespace(file);", graphMap.name);
          dtoServiceWriter.Indent++;

          if (graphMap.dataObjectMaps.Count == 1)
          {
            DataObjectMap dataObjectMap = graphMap.dataObjectMaps[0];
            string qualifiedDataObjectName = GetQualifiedDataObjectName(dataObjectMap.name);

            dtoServiceWriter.WriteLine(
            @"List<{0}> _dtoList = new List<{0}>(); 
            var dtoResults = from c in file.Elements(""Envelope"").Elements(""Payload"").Elements(""DataTransferObject"") select c;

            foreach (var dtoResult in dtoResults)
            {{
              var dtoProperties = from c in dtoResult.Elements(""Properties"").Elements(""Property"") select c;
              {0} dto = new {0}();

              foreach (var dtoProperty in dtoProperties)
              {{
                for (int i = 0; i < dto._properties.Count; i++)
                {{
                  if (dtoProperty.Attribute(""name"").Value == dto._properties[i].OIMProperty)
                  {{
                    dto._properties[i].Value = dtoProperty.Attribute(""value"").Value.ToString();
                  }}
                }}
              }}

              _dtoList.Add(dto);
            }}

            foreach ({0} dto in _dtoList)
            {{
              dtoList.Add(dto);
            }}", qualifiedGraphName);
          }

          dtoServiceWriter.WriteLine();
          dtoServiceWriter.WriteLine("break;");
          dtoServiceWriter.Indent--;
          dtoServiceWriter.WriteLine("}");
        }

        dtoServiceWriter.Indent--;
        dtoServiceWriter.WriteLine("}");
        dtoServiceWriter.Indent--;
        dtoServiceWriter.WriteLine("}");
        dtoServiceWriter.WriteLine();
        dtoServiceWriter.WriteLine("return dtoList;");
        dtoServiceWriter.Indent--;
        dtoServiceWriter.WriteLine("}");

        dtoServiceWriter.WriteLine(@"
    public DataDictionary GetDictionary()
    {
      return _dataLayer.GetDictionary();
    }

    public Response RefreshDictionary()
    {
      throw new NotImplementedException();
    }");

        dtoServiceWriter.Indent--;
        dtoServiceWriter.WriteLine("}");
        dtoServiceWriter.Indent--;
        dtoServiceWriter.WriteLine("}");
        dtoServiceWriter.Close();

        return _dtoServiceBuilder.ToString();
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
        throw ex;
      }
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

    //private List<string> GetServiceKnownTypes(string projectName, string applicationName)
    //{
    //  try
    //  {
    //    List<string> knownTypes = new List<string>();
    //    string ns = ADAPTER_NAMESPACE + ".proj_" + projectName + "." + applicationName;

    //    string mappingFile = _settings.XmlPath + "Mapping." + projectName + "." + applicationName + ".xml";
    //    Mapping mapping = Utility.Read<Mapping>(mappingFile, false);

    //    foreach (GraphMap graphMap in mapping.graphMaps)
    //    {
    //      knownTypes.Add("[ServiceKnownType(typeof(" + ns + "." + graphMap.name + "))]");
    //    }

    //    return knownTypes;
    //  }
    //  catch (Exception ex)
    //  {
    //    throw ex;
    //  }
    //}

    private void ProcessGraphMap(GraphMap graphMap)
    {
      _mappingProperties.Clear();
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
      List<string> templateClassPropertyList = new List<string>();

      foreach (RoleMap roleMap in templateMap.roleMaps)
      {
        roleMap.name = Utility.NameSafe(roleMap.name);

        if (templateMap.type == TemplateType.Property)
        {
          ProcessRoleMap(templateMap.name, roleMap, dataObjectMaps, isDataMember);
        }
        else if (templateMap.type == TemplateType.Relationship)
        {
          if (roleMap.classMap == null)
          {
            ProcessRoleMap(templateMap.name, roleMap, dataObjectMaps, isDataMember);
          }
          else if (roleMap.classMap.templateMaps != null && roleMap.classMap.templateMaps.Count > 0)
          {
            roleMap.classMap.name = Utility.NameSafe(roleMap.classMap.name);

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

            _dtoModelWriter.WriteLine();
            _dtoModelWriter.WriteLine("[DataContract(Namespace = \"{0}\")]", _xmlNamespace);
            _dtoModelWriter.WriteLine("[XmlRoot(Namespace = \"{0}\")]", _xmlNamespace);
            _dtoModelWriter.WriteLine("public class Template{0}", templateMap.name);
            _dtoModelWriter.Write("{");
            _dtoModelWriter.Indent++;
            _dtoModelWriter.WriteLine();
            _dtoModelWriter.WriteLine("[DataContract(Namespace = \"{0}\")]", _xmlNamespace);
            _dtoModelWriter.WriteLine("[XmlRoot(Namespace = \"{0}\")]", _xmlNamespace);
            _dtoModelWriter.WriteLine("public class Class{0}", roleMap.classMap.name);
            _dtoModelWriter.Write("{");
            _dtoModelWriter.Indent++;
            _dtoModelWriter.WriteLine();
            _dtoModelWriter.WriteLine("[DataMember(EmitDefaultValue=false)]");
            _dtoModelWriter.WriteLine("[XmlIgnore]");
            _dtoModelWriter.WriteLine("public string Identifier { get; set; }");

            ProcessClassMap(roleMap, dataObjectMaps);

            _dtoModelWriter.Indent--;
            _dtoModelWriter.WriteLine("}");
            _dtoModelWriter.WriteLine();
            _dtoModelWriter.WriteLine("[DataMember(Name = \"tpl_{0}_rdl_{1}\", EmitDefaultValue = false)]", roleMap.name, roleMap.classMap.name);
            _dtoModelWriter.WriteLine("[XmlIgnore]");
            _dtoModelWriter.WriteLine("public Class{1} tpl_{0}_rdl_{1} {{ get; set; }}", roleMap.name, roleMap.classMap.name);
            _dtoModelWriter.Indent--;
            _dtoModelWriter.WriteLine("}");

            if (!templateClassPropertyList.Contains(templateMap.name))
            {
              _dtoModelWriter.WriteLine();
              _dtoModelWriter.WriteLine("[DataMember(EmitDefaultValue = false)]");
              _dtoModelWriter.WriteLine("[XmlIgnore]");
              _dtoModelWriter.WriteLine("public Template{0} tpl_{0} {{ get; set; }}", templateMap.name);

              templateClassPropertyList.Add(templateMap.name);
            }
          }
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
            if ((!String.IsNullOrEmpty(roleMap.propertyName) && roleMap.propertyName.ToUpper() == dataProperty.propertyName.ToUpper()) ||
                !String.IsNullOrEmpty(roleMap.reference) || !String.IsNullOrEmpty(roleMap.value))
            {
              MappingProperty mappingProperty = new MappingProperty();
              mappingProperty.value = null;

              if (!String.IsNullOrEmpty(roleMap.propertyName) && roleMap.propertyName.ToUpper() == dataProperty.propertyName.ToUpper())
              {
                mappingProperty.propertyName = dataProperty.propertyName;
                mappingProperty.dataType = dataProperty.dataType;
                mappingProperty.dataLength = dataProperty.dataLength;
                mappingProperty.isPropertyKey = dataProperty.isPropertyKey;
                mappingProperty.isRequired = dataProperty.isRequired;
                mappingProperty.mappingDataType = String.IsNullOrEmpty(roleMap.dataType) ? dataProperty.dataType : Utility.XsdTypeToCSharpType(roleMap.dataType);
              }
              else if (!String.IsNullOrEmpty(roleMap.reference))
              {
                string reference = roleMap.reference;

                if (roleMap.reference.StartsWith("rdl:") || roleMap.reference.StartsWith("RDL:"))
                {
                  reference = RDL_NAMESPACE + roleMap.reference.Substring(4);
                }
                else if (roleMap.reference.StartsWith("tpl:") || roleMap.reference.StartsWith("TPL:"))
                {
                  reference = TPL_NAMESPACE + roleMap.reference.Substring(4);
                }

                mappingProperty.value = "<" + reference + ">";
                mappingProperty.mappingDataType = String.IsNullOrEmpty(roleMap.dataType) ? "String" : Utility.XsdTypeToCSharpType(roleMap.dataType);
              }
              else if (!String.IsNullOrEmpty(roleMap.value))
              {
                mappingProperty.value = roleMap.value;
                mappingProperty.mappingDataType = String.IsNullOrEmpty(roleMap.dataType) ? "String" : Utility.XsdTypeToCSharpType(roleMap.dataType);
              }

              if (_templatePath == String.Empty)
              {
                mappingProperty.propertyPath = "tpl_" + templateName + "_tpl_" + roleMap.name;
                mappingProperty.dtoPropertyPath = "tpl:" + templateName + ".tpl:" + roleMap.name;
              }
              else
              {
                mappingProperty.propertyPath = _templatePath + "_tpl_" + templateName + "_tpl_" + roleMap.name;
                mappingProperty.dtoPropertyPath = _dtoTemplatePath + ".tpl:" + templateName + ".tpl:" + roleMap.name;
              }

              mappingProperty.isDataMember = isDataMember;
              _mappingProperties.Add(mappingProperty);

              if (!isDataMember)
              {
                if (String.IsNullOrEmpty(mappingProperty.mappingDataType))
                {
                  _initStatements.Add(_dataContractPath + ".tpl_" + templateName + "_tpl_" + roleMap.name + " = " + mappingProperty.propertyPath + ";");
                }
                else
                {
                  _initStatements.Add(_dataContractPath + ".tpl_" + templateName + "_tpl_" + roleMap.name + " = Convert.To" + Utility.XsdTypeToCSharpType(mappingProperty.mappingDataType) + "(" + mappingProperty.propertyPath + ");");
                }

                _dtoModelWriter.WriteLine();
                _dtoModelWriter.WriteLine("[DataMember(EmitDefaultValue = false)]");
                _dtoModelWriter.WriteLine("public {0} tpl_{1}_tpl_{2} {{ get; set; }}", mappingProperty.mappingDataType, templateName, roleMap.name);
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

    private void AddCustomDataLayerAssembly(string projectName, string applicationName, CompilerParameters parameters)
    {
      string bindingConfigurationPath = _settings.XmlPath + "BindingConfiguration." + projectName + "." + applicationName + ".xml";

      if (File.Exists(bindingConfigurationPath))
      {
        BindingConfiguration bindingConfiguration = Utility.Read<BindingConfiguration>(bindingConfigurationPath, false);

        foreach (Binding binding in bindingConfiguration.Bindings)
        {
          if (binding.Name.ToUpper() == "DATALAYER" && !binding.Implementation.ToUpper().Contains("NHIBERNATEDATALAYER"))
          {
            string[] bindingImpl = binding.Implementation.Split(',');
            string bindingAssembly = bindingImpl[1].Trim() + ".dll";
            parameters.ReferencedAssemblies.Add(_settings.BinaryPath + bindingAssembly);
            break;
          }
        }
      }
      else
      {
        string errorMessage = "Binding configuration file " + bindingConfigurationPath + " not found.";

        _logger.Error(errorMessage);
        throw new Exception(errorMessage);
      }
    }

    private List<KeyValuePair<string, ScopeApplication>> GetScopeApplications()
    {
      try
      {
        List<KeyValuePair<string, ScopeApplication>> scopeApps = new List<KeyValuePair<string, ScopeApplication>>();
        string scopesPath = _settings.XmlPath + "Scopes.xml";

        if (File.Exists(scopesPath))
        {
          List<ScopeProject> projects = Utility.Read<List<ScopeProject>>(scopesPath);

          foreach (ScopeProject project in projects)
          {
            foreach (ScopeApplication application in project.Applications)
            {
              scopeApps.Add(new KeyValuePair<string, ScopeApplication>(project.Name, application));
            }
          }
        }

        return scopeApps;
      }
      catch (Exception exception)
      {
        _logger.Error("Error in GetGeneratedApplications: " + exception);
        throw exception;
      }
    }
  }

  public class MappingProperty : DataProperty
  {
    public string propertyPath { get; set; }
    public string dtoPropertyPath { get; set; }
    public bool isDataMember { get; set; }
    public string value { get; set; }
    public string mappingDataType { get; set; }
  }
}
