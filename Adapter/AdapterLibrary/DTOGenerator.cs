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

namespace org.iringtools.adapter
{
  public class DTOGenerator
  {
    private const string INDENTATION = "  ";
    private const string COMPILER_VERSION = "v3.5";
    private const string LIBRARY_NAMESPACE = "org.iringtools.library";
    private const string ADAPTER_NAMESPACE = "org.iringtools.adapter";

    private Mapping _mapping = null;
    private DataDictionary _dataDictionary = null;
    private List<ExtendedDataProperty> _extendedDataProperties = null;
    private List<string> _initStatements = null;
    private IndentedTextWriter _dtoModelWriter = null;
    private ILog _logger = null;
    
    private string _currentDirectory = String.Empty;
    private string _classNamespace = String.Empty;
    private string _xmlNamespace = String.Empty;
    private string _classPath = string.Empty;
    private string _dataContractPath = string.Empty;
    private string _templatePath = string.Empty;
    private string _dtoTemplatePath = string.Empty;

    public DTOGenerator()
    {
      _currentDirectory = Directory.GetCurrentDirectory();
      _extendedDataProperties = new List<ExtendedDataProperty>();
      _initStatements = new List<string>();
      _logger = LogManager.GetLogger(typeof(DTOGenerator));
    }

    public void Generate(string projectName, string applicationName)
    {
      try
      {
        string mappingPath = _currentDirectory + "\\XML\\Mapping." + projectName + "." + applicationName + ".xml";
        _mapping = Utility.Read<Mapping>(mappingPath, false);

        string dataDictionaryPath = _currentDirectory + "\\XML\\DataDictionary." + projectName + "." + applicationName + ".xml";
        _dataDictionary = Utility.Read<DataDictionary>(dataDictionaryPath, true);

        _classNamespace = ADAPTER_NAMESPACE + ".proj_" + projectName + "." + applicationName;
        _xmlNamespace = "http://" + applicationName + ".bechtel.com/" + projectName + "/data#";

        Dictionary<string, string> compilerOptions = new Dictionary<string, string>();
        compilerOptions.Add("CompilerVersion", COMPILER_VERSION);

        CompilerParameters parameters = new CompilerParameters();
        parameters.GenerateExecutable = false;
        parameters.ReferencedAssemblies.Add("System.Core.dll");
        parameters.ReferencedAssemblies.Add("System.Runtime.Serialization.dll");
        parameters.ReferencedAssemblies.Add("System.ServiceModel.dll");
        parameters.ReferencedAssemblies.Add("System.ServiceModel.Web.dll");
        parameters.ReferencedAssemblies.Add("System.Xml.dll");
        parameters.ReferencedAssemblies.Add("System.Xml.Linq.dll");
        parameters.ReferencedAssemblies.Add(_currentDirectory + @"\bin\Ninject.dll");
        parameters.ReferencedAssemblies.Add(_currentDirectory + @"\bin\iRINGLibrary.dll");
        parameters.ReferencedAssemblies.Add(_currentDirectory + @"\bin\UtilityLibrary.dll");
        parameters.ReferencedAssemblies.Add(_currentDirectory + @"\bin\AdapterLibrary.dll");
        parameters.ReferencedAssemblies.Add(_currentDirectory + @"\bin\AdapterService.dll");

        // Generate code
        string dtoModel = GenerateDTOModel(projectName, applicationName);
        string dtoService = GenerateDTOService();
        string serviceInterface = UpdateServiceInterface();        
        string dataServiceInterface = UpdateDataServiceInterface();

        // Compile generated code
        string[] sources = new string[] { dtoModel, dtoService, serviceInterface, dataServiceInterface };
        Utility.Compile(compilerOptions, parameters, sources);
        
        // Write generated code to disk
        Utility.WriteString(dtoModel, _currentDirectory + @"\App_Code\DTOModel." + projectName + "." + applicationName + ".cs", Encoding.ASCII);
        Utility.WriteString(dtoService, _currentDirectory + @"\App_Code\DTOService." + projectName + "." + applicationName + ".cs", Encoding.ASCII);
        Utility.WriteString(serviceInterface, _currentDirectory + @"\App_Code\IService.Generated.cs", Encoding.ASCII);
        Utility.WriteString(dataServiceInterface, _currentDirectory + @"\App_Code\IDataService.Generated.cs", Encoding.ASCII);
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
        throw ex;
      }
    }

    private string GenerateDTOModel(string projectName, string applicationName)
    {
      try
      {
        StringBuilder dtoModelBuilder = new StringBuilder();
        _dtoModelWriter = new IndentedTextWriter(new StringWriter(dtoModelBuilder), INDENTATION);

        _dtoModelWriter.WriteLine(Utility.GeneratedCodeProlog());
        _dtoModelWriter.WriteLine("using System;");
        _dtoModelWriter.WriteLine("using System.Collections.Generic;");
        _dtoModelWriter.WriteLine("using System.Runtime.Serialization;");
        _dtoModelWriter.WriteLine("using System.Xml.Serialization;");
        _dtoModelWriter.WriteLine("using System.Xml.Xsl;");
        _dtoModelWriter.WriteLine("using org.iringtools.library;");
        _dtoModelWriter.WriteLine("using org.iringtools.utility;");

        _dtoModelWriter.WriteLine();
        _dtoModelWriter.WriteLine("namespace {0}", _classNamespace);
        _dtoModelWriter.Write("{");
        _dtoModelWriter.Indent++;

        foreach (GraphMap graphMap in _mapping.graphMaps)
        {
          graphMap.name = NameSafe(graphMap.name);
          graphMap.classId = graphMap.classId.Replace("rdl:", "http://rdl.rdlfacade.org/data#");

          _dtoModelWriter.WriteLine();
          _dtoModelWriter.WriteLine("[DataContract(Name = \"{0}\", Namespace = \"{1}\" )]", graphMap.name, _xmlNamespace);
          _dtoModelWriter.WriteLine("[XmlRoot(Namespace = \"{0}\")]", _xmlNamespace);
          _dtoModelWriter.WriteLine("public class {0} : DataTransferObject", graphMap.name);
          _dtoModelWriter.Write("{");
          _dtoModelWriter.Indent++;

          ProcessGraphMap(graphMap);

          _dtoModelWriter.WriteLine();
          _dtoModelWriter.WriteLine("public {0}(string classId, string graphName, string identifier) : base(classId, graphName)", graphMap.name);
          _dtoModelWriter.WriteLine("{");
          _dtoModelWriter.Indent++;

          foreach (ExtendedDataProperty extendedDataProperty in _extendedDataProperties)
          {
            _dtoModelWriter.WriteLine("_properties.Add(new DTOProperty(@\"{0}\", @\"{1}\", null, typeof({2}), {3}, {4}));",
              extendedDataProperty.propertyName, extendedDataProperty.dtoPropertyPath, extendedDataProperty.dataType,
              Convert.ToString(extendedDataProperty.isPropertyKey).ToLower(),
              Convert.ToString(extendedDataProperty.isRequired).ToLower());
          }

          _dtoModelWriter.WriteLine("Identifier = identifier;");
          _dtoModelWriter.WriteLine("ClassId = classId;");
          _dtoModelWriter.Indent--;
          _dtoModelWriter.WriteLine("}");

          foreach (DataObjectMap dataObjectMap in graphMap.dataObjectMaps)
          {
            string qualifiedDataObjectName = GetQualifiedDataObjectName(dataObjectMap.name);

            _dtoModelWriter.WriteLine();
            _dtoModelWriter.WriteLine("public {0}({1} dataObject) : this(\"{2}\", \"{0}\", null, dataObject) {{}}", graphMap.name, qualifiedDataObjectName, graphMap.classId, graphMap.name);

            _dtoModelWriter.WriteLine();
            _dtoModelWriter.WriteLine("public {0}(string classId, string graphName, string identifier, {1} dataObject) : this(classId, graphName, identifier)", graphMap.name, qualifiedDataObjectName);
            _dtoModelWriter.WriteLine("{");
            _dtoModelWriter.Indent++;
            _dtoModelWriter.WriteLine("if (dataObject != null)");
            _dtoModelWriter.WriteLine("{");
            _dtoModelWriter.Indent++;

            foreach (ExtendedDataProperty extendedDataProperty in _extendedDataProperties)
            {
              if (extendedDataProperty.isPropertyKey)
              {
                _dtoModelWriter.WriteLine("{0} = ({1})dataObject.Id;", extendedDataProperty.propertyPath, extendedDataProperty.dataType);
              }
              else
              {
                _dtoModelWriter.WriteLine("{0} = ({1})dataObject.{2};", extendedDataProperty.propertyPath, extendedDataProperty.dataType, extendedDataProperty.propertyName);
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

            _dtoModelWriter.WriteLine();

            if (extendedDataProperty.isDataMember)
            {
              _dtoModelWriter.WriteLine("[DataMember(Name = \"{0}\", EmitDefaultValue = false)]", extendedDataProperty.propertyPath);
            }

            _dtoModelWriter.WriteLine("[XmlIgnore]");
            _dtoModelWriter.WriteLine("public {0} {1}", nullableType, extendedDataProperty.propertyPath);
            _dtoModelWriter.WriteLine("{");
            _dtoModelWriter.Indent++;
            _dtoModelWriter.WriteLine("get");
            _dtoModelWriter.WriteLine("{");
            _dtoModelWriter.Indent++;
            _dtoModelWriter.WriteLine("return ({0})GetPropertyValue(\"{1}\");", extendedDataProperty.dataType, extendedDataProperty.dtoPropertyPath);
            _dtoModelWriter.Indent--;
            _dtoModelWriter.WriteLine("}");
            _dtoModelWriter.WriteLine("set");
            _dtoModelWriter.WriteLine("{");
            _dtoModelWriter.Indent++;
            _dtoModelWriter.WriteLine("SetPropertyValue(@\"{0}\", value);", extendedDataProperty.dtoPropertyPath);
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

            foreach (ExtendedDataProperty extendedDataProperty in _extendedDataProperties)
            {
              if (extendedDataProperty.isPropertyKey)
              {
                _dtoModelWriter.WriteLine("(({0})_dataObject).Id = ({1})this._identifier;", qualifiedDataObjectName, extendedDataProperty.dataType);
              }
            }

            _dtoModelWriter.Indent--;
            _dtoModelWriter.WriteLine("}");

            foreach (ExtendedDataProperty extendedDataProperty in _extendedDataProperties)
            {
              if (!extendedDataProperty.isPropertyKey)
              {
                _dtoModelWriter.WriteLine("(({0})_dataObject).{1} = ({2})this.{3};",
                  qualifiedDataObjectName, extendedDataProperty.propertyName, extendedDataProperty.dataType, extendedDataProperty.propertyPath);
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

        return dtoModelBuilder.ToString();
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    private string GenerateDTOService()
    {
      try
      {
        StringBuilder dtoServiceBuilder = new StringBuilder();
        IndentedTextWriter dtoServiceWriter = new IndentedTextWriter(new StringWriter(dtoServiceBuilder), INDENTATION);

        dtoServiceWriter.WriteLine(Utility.GeneratedCodeProlog());
        dtoServiceWriter.WriteLine("using System;");
        dtoServiceWriter.WriteLine("using System.Collections.Generic;");
        dtoServiceWriter.WriteLine("using System.IO;");
        dtoServiceWriter.WriteLine("using System.Linq;");
        dtoServiceWriter.WriteLine("using System.ServiceModel;");
        dtoServiceWriter.WriteLine("using System.Xml;");
        dtoServiceWriter.WriteLine("using System.Xml.Linq;");
        dtoServiceWriter.WriteLine("using System.Xml.Xsl;");
        dtoServiceWriter.WriteLine("using Ninject;");
        dtoServiceWriter.WriteLine("using org.iringtools.library;");
        dtoServiceWriter.WriteLine("using org.iringtools.utility;");
        dtoServiceWriter.WriteLine();
        dtoServiceWriter.WriteLine("namespace {0}", _classNamespace);
        dtoServiceWriter.WriteLine("{");
        dtoServiceWriter.Indent++;
        dtoServiceWriter.WriteLine("public class DTOService : IDTOService");
        dtoServiceWriter.WriteLine("{");
        dtoServiceWriter.Indent++;
        dtoServiceWriter.WriteLine("IKernel _kernel = null;");
        dtoServiceWriter.WriteLine("IDataLayer _dataLayer = null;");
        dtoServiceWriter.WriteLine("AdapterSettings _settings = null;");

        dtoServiceWriter.WriteLine();
        dtoServiceWriter.WriteLine("[Inject]");
        dtoServiceWriter.WriteLine("public DTOService(IKernel kernel, IDataLayer dataLayer, AdapterSettings settings)");
        dtoServiceWriter.WriteLine("{");
        dtoServiceWriter.Indent++;
        dtoServiceWriter.WriteLine("_kernel = kernel;");
        dtoServiceWriter.WriteLine("_dataLayer = dataLayer;");
        dtoServiceWriter.WriteLine("_settings = settings;");
        dtoServiceWriter.Indent--;
        dtoServiceWriter.WriteLine("}");

        dtoServiceWriter.WriteLine();
        dtoServiceWriter.WriteLine("public T TransformList<T>(string graphName, List<DataTransferObject> dtoList, string xmlPath, string stylesheetUri, string mappingUri, bool useDataContractDeserializer)");
        dtoServiceWriter.WriteLine("{");
        dtoServiceWriter.Indent++;
        dtoServiceWriter.WriteLine("string dtoPath = xmlPath + graphName + \"DTO.xml\";");
        dtoServiceWriter.WriteLine("Mapping mapping = Utility.Read<Mapping>(mappingUri, false);");
        dtoServiceWriter.WriteLine();
        dtoServiceWriter.WriteLine("switch (graphName)");
        dtoServiceWriter.Write("{");
        dtoServiceWriter.Indent++;
        
        foreach (GraphMap graphMap in _mapping.graphMaps)
        {
          dtoServiceWriter.WriteLine();
          dtoServiceWriter.WriteLine("case \"{0}\":", graphMap.name);
          dtoServiceWriter.Indent++;
          dtoServiceWriter.WriteLine("List<{0}> {0}List = new List<{0}>();", graphMap.name);
          dtoServiceWriter.WriteLine();
          dtoServiceWriter.WriteLine("foreach (DataTransferObject dto in dtoList)");
          dtoServiceWriter.WriteLine("{");
          dtoServiceWriter.Indent++;
          dtoServiceWriter.WriteLine("{0}List.Add(({0})dto);", graphMap.name);
          dtoServiceWriter.Indent--;
          dtoServiceWriter.WriteLine("}");
          dtoServiceWriter.WriteLine();
          dtoServiceWriter.WriteLine("Utility.Write<List<{0}>>({0}List, dtoPath);", graphMap.name);
          dtoServiceWriter.WriteLine("break;");
          dtoServiceWriter.Indent--;
        }

        dtoServiceWriter.Indent--;
        dtoServiceWriter.WriteLine("}");
        dtoServiceWriter.WriteLine();
        dtoServiceWriter.WriteLine("XsltArgumentList xsltArgumentList = new XsltArgumentList();");
        dtoServiceWriter.WriteLine("xsltArgumentList.AddParam(\"dtoFilename\", String.Empty, dtoPath);");
        dtoServiceWriter.WriteLine();
        dtoServiceWriter.WriteLine("return Utility.Transform<Mapping, T>(mapping, stylesheetUri, xsltArgumentList, false, useDataContractDeserializer);");
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
          graphMap.classId = graphMap.classId.Replace("rdl:", "http://rdl.rdlfacade.org/data#");
          dtoServiceWriter.WriteLine();
          dtoServiceWriter.WriteLine("case \"{0}\":", graphMap.name);
          dtoServiceWriter.Indent++;
          dtoServiceWriter.WriteLine("dto = new {0}(\"{1}\", graphName, identifier);", graphMap.name, graphMap.classId);
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
          dtoServiceWriter.WriteLine();
          dtoServiceWriter.WriteLine("case \"{0}\":", graphMap.name);
          dtoServiceWriter.Indent++;

          foreach (DataObjectMap dataObjectMap in graphMap.dataObjectMaps)
          {
            string qualifiedDataObjectName = GetQualifiedDataObjectName(dataObjectMap.name);
		
            if (!String.IsNullOrEmpty(dataObjectMap.outFilter))
            {
              string outFilter = dataObjectMap.outFilter.Substring(dataObjectMap.outFilter.IndexOf("_") + 1);
              string[] idList = graphMap.identifier.Split(new string[]{","}, StringSplitOptions.RemoveEmptyEntries);
              string identifiers = dataObjectMap.name + "List.Id";

              //string identifiers = String.Empty;			
              //foreach (string id in idList)
              //{
              //	if (identifiers != String.Empty)
              //	{
              //		identifiers += " + ";
              //	}				
              //	identifiers += dataObjectMap.name + "List." + id.Trim();
              //}
			
              dtoServiceWriter.WriteLine(
          @"var {0}DO = 
            (from {1}List in _dataLayer.GetList<{2}>()
             where {3} == identifier && {1}List.{4}  // outFilter
             select {1}List).FirstOrDefault<{2}>();

          if ({0}DO != default({2}))
          {{
            dto = new {0}({0}DO);
            dto.Identifier = {0}DO.Id;
          }}", graphMap.name, dataObjectMap.name, qualifiedDataObjectName, identifiers, outFilter);
				
            }
            else
            {
              dtoServiceWriter.WriteLine(
  				@"var {0}DO = 
            (from {0}List in _dataLayer.GetList<{2}>()
             where {0}List.Id == identifier
             select {0}List).FirstOrDefault<{2}>();   
        
          if ({0}DO != default({2}))
          {{                        
            dto = new {1}({0}DO);
            dto.Identifier = {0}DO.Id;
            break; 
          }}", dataObjectMap.name, graphMap.name, qualifiedDataObjectName);
            }
          }

          dtoServiceWriter.WriteLine();
          dtoServiceWriter.WriteLine("break;");
          dtoServiceWriter.Indent--;
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
          dtoServiceWriter.WriteLine();
          dtoServiceWriter.WriteLine("case \"{0}\":", graphMap.name);
          dtoServiceWriter.Indent++;

          foreach (DataObjectMap dataObjectMap in graphMap.dataObjectMaps)
          {
            string qualifiedDataObjectName = GetQualifiedDataObjectName(dataObjectMap.name);
		
            if (!String.IsNullOrEmpty(dataObjectMap.outFilter))
            {
              String outFilter = dataObjectMap.outFilter.Substring(dataObjectMap.outFilter.IndexOf("_") + 1);
        
              dtoServiceWriter.WriteLine(
          @"var {0}DOList = 
            from {1}List in _dataLayer.GetList<{2}>()
            where {1}List.{3}  // outFilter
            select {1}List;

          foreach (var {0}DO in {0}DOList)
          {{   					
            {0} dto = new {0}({0}DO);
            dto.Identifier = {0}DO.Id;
            dtoList.Add(dto);
          }}", graphMap.name, dataObjectMap.name, qualifiedDataObjectName, outFilter); 
            }
            else
            {
              dtoServiceWriter.WriteLine(
  				@"var {0}DOList = 
            from {0}List in _dataLayer.GetList<{1}>()
            select {0}List;  
    
          foreach (var {0}DO in {0}DOList)
          {{   					
            {2} dto = new {2}({0}DO);
            dto.Identifier = {0}DO.Id;
            dtoList.Add(dto);
          }}", dataObjectMap.name, qualifiedDataObjectName, graphMap.name);
            }
          }

          dtoServiceWriter.WriteLine();
          dtoServiceWriter.WriteLine("break;");
          dtoServiceWriter.Indent--;
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
        dtoServiceWriter.WriteLine("Dictionary<string, string> identifierUriPairs = new Dictionary<string, string>();");
        dtoServiceWriter.WriteLine("String endpoint = OperationContext.Current.Channel.LocalAddress.ToString();");
			  dtoServiceWriter.WriteLine();
        dtoServiceWriter.WriteLine("switch (graphName)");
        dtoServiceWriter.Write("{");
        dtoServiceWriter.Indent++;
        
        foreach (GraphMap graphMap in _mapping.graphMaps)
        {
          dtoServiceWriter.WriteLine();
          dtoServiceWriter.WriteLine("case \"{0}\":", graphMap.name);
          dtoServiceWriter.Indent++;

          foreach (DataObjectMap dataObjectMap in graphMap.dataObjectMaps)
          {
            string qualifiedDataObjectName = GetQualifiedDataObjectName(dataObjectMap.name);
		
            if (!String.IsNullOrEmpty(dataObjectMap.outFilter))
            {
              String outFilter = dataObjectMap.outFilter.Substring(dataObjectMap.outFilter.IndexOf("_") + 1);
        
              dtoServiceWriter.WriteLine(
          @"var {0}DOList = 
            from {1}List in _dataLayer.GetList<{2}>()
            where {1}List.{3}  // outFilter
            select {1}List;
    
          foreach (var {0}DO in {0}DOList)
          {{   
            string identifier = {0}DO.Id;
            identifierUriPairs.Add(identifier, endpoint + ""/"" + graphName + ""/"" + identifier);            
          }}",
          graphMap.name, dataObjectMap.name, qualifiedDataObjectName, outFilter);
            }
            else
            {
              dtoServiceWriter.WriteLine(
  				@"var {0}DOList = 
            from {0}List in _dataLayer.GetList<{1}>()
            select {0}List;  

          foreach (var {0}DO in {0}DOList)
          {{
            string identifier = {0}DO.Id;
            identifierUriPairs.Add(identifier, endpoint + ""/"" + graphName + ""/"" + identifier);  
          }}",
          dataObjectMap.name, qualifiedDataObjectName);
            }
          }

          dtoServiceWriter.WriteLine();
          dtoServiceWriter.WriteLine("break;");
          dtoServiceWriter.Indent--;
        }

        dtoServiceWriter.Indent--;
        dtoServiceWriter.WriteLine("}");
			  dtoServiceWriter.WriteLine();
        dtoServiceWriter.WriteLine("return identifierUriPairs;");
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
          dtoServiceWriter.WriteLine();
          dtoServiceWriter.Write("case \"{0}\":", graphMap.name);
          dtoServiceWriter.Indent++;

          if (graphMap.dataObjectMaps.Count == 1)
          {
            DataObjectMap dataObjectMap = graphMap.dataObjectMaps[0];
            string qualifiedDataObjectName = GetQualifiedDataObjectName(dataObjectMap.name);
        
            dtoServiceWriter.WriteLine(@"
            {0} {1}DO = ({0})dto.GetDataObject();
            response.Append(_dataLayer.Post<{0}>({1}DO));",
            qualifiedDataObjectName, graphMap.name);
          }
          else
          {
            dtoServiceWriter.WriteLine(@"<#= graphMap.name #> <#= graphMap.name #>Obj = (<#= graphMap.name #>)dto;");
            int dataObjectMapCount = 0;
		
            foreach (DataObjectMap dataObjectMap in graphMap.dataObjectMaps)
            {
              string qualifiedDataObjectName = GetQualifiedDataObjectName(dataObjectMap.name);
			
              if (++dataObjectMapCount == 1)
              {
                dtoServiceWriter.WriteLine("if ({0}Obj.{1}) // inFilter", graphMap.name, dataObjectMap.inFilter);
              }
              else
              {
                dtoServiceWriter.WriteLine("else if ({0}Obj.{1}) // inFilter", graphMap.name, dataObjectMap.inFilter);
              }
        
              dtoServiceWriter.WriteLine("{");
              dtoServiceWriter.Indent++;
              dtoServiceWriter.WriteLine(@"
                {0} {1}DO = ({0}){2}Obj.GetDataObject();
                response.Append(_dataLayer.Post<{0}>({1}DO));", qualifiedDataObjectName, dataObjectMap.name, graphMap.name);
              dtoServiceWriter.Indent--;
              dtoServiceWriter.WriteLine("}");
            }
          }
        
          dtoServiceWriter.WriteLine("break;");
          dtoServiceWriter.Indent--;
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
          dtoServiceWriter.WriteLine();
          dtoServiceWriter.WriteLine("case \"{0}\":", graphMap.name);
          dtoServiceWriter.Indent++;

          if (graphMap.dataObjectMaps.Count == 1)
          {
            DataObjectMap dataObjectMap = graphMap.dataObjectMaps[0];
            string qualifiedDataObjectName = GetQualifiedDataObjectName(dataObjectMap.name);
            
            dtoServiceWriter.WriteLine(
        @"List<{0}> {1}DOList = new List<{0}>();

            foreach (DataTransferObject dto in dtoList)
            {{
              {1}DOList.Add(({0})dto.GetDataObject());
            }}

            response.Append(_dataLayer.PostList<{0}>({1}DOList));",
        qualifiedDataObjectName, graphMap.name);  
          }
          else
          {
            foreach (DataObjectMap dataObjectMap in graphMap.dataObjectMaps)
            {
              string qualifiedDataObjectName = GetQualifiedDataObjectName(dataObjectMap.name);
              dtoServiceWriter.WriteLine(@"List<{0}> {1}DOList = new List<{0}>();", qualifiedDataObjectName, dataObjectMap.name);
            }
        
            dtoServiceWriter.WriteLine("foreach ({0} dto in dtoList)", graphMap.name);
            dtoServiceWriter.WriteLine("{");
            dtoServiceWriter.Indent++;

            int dataObjectMapCount = 0;
		
            foreach (DataObjectMap dataObjectMap in graphMap.dataObjectMaps)
            {
              string qualifiedDataObjectName = GetQualifiedDataObjectName(dataObjectMap.name);
			
              if (++dataObjectMapCount == 1)
              {			
                dtoServiceWriter.WriteLine("if (dto.{0}) // inFilter", dataObjectMap.inFilter);
              }
              else 
              {
                dtoServiceWriter.WriteLine("else if (dto.{0}) // inFilter", dataObjectMap.inFilter);
              }
        
              dtoServiceWriter.WriteLine(@"
              {
                {0}DOList.Add(({1})dto.GetDataObject());
              }", dataObjectMap.name, qualifiedDataObjectName);	
            }
            
            dtoServiceWriter.Indent--;
            dtoServiceWriter.WriteLine("}");
					
            foreach (DataObjectMap dataObjectMap in graphMap.dataObjectMaps)
            {
              string qualifiedDataObjectName = GetQualifiedDataObjectName(dataObjectMap.name);
              dtoServiceWriter.WriteLine("response.Append(_dataLayer.PostList<{0}>({1}DOList));", qualifiedDataObjectName, dataObjectMap.name);
        		}
          }

          dtoServiceWriter.WriteLine("break;");
          dtoServiceWriter.Indent--;
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
          dtoServiceWriter.WriteLine();
          dtoServiceWriter.WriteLine(
        @"case ""{0}"":
            XmlReader {0}Reader = XmlReader.Create(new StringReader(dtoListString));
            XDocument {0}File = XDocument.Load({0}Reader);
            {0}File = Utility.RemoveNamespace({0}File);", graphMap.name);
          dtoServiceWriter.Indent++;

          if (graphMap.dataObjectMaps.Count == 1)
          {
            DataObjectMap dataObjectMap = graphMap.dataObjectMaps[0];
            string qualifiedDataObjectName = GetQualifiedDataObjectName(dataObjectMap.name);

            dtoServiceWriter.WriteLine(
            @"List<{0}> {0}List = new List<{0}>(); 
            var {0}Query = from c in {0}File.Elements(""Envelope"").Elements(""Payload"").Elements(""DataTransferObject"") select c;

            foreach (var dto in {0}Query)
            {{
              var propertyQuery = from c in dto.Elements(""Properties"").Elements(""Property"") select c;
              {0} graphObject = new {0}();

              foreach (var dtoProperty in propertyQuery)
              {{
                for (int i = 0; i < graphObject._properties.Count; i++)
                {{
                  if (dtoProperty.Attribute(""name"").Value == graphObject._properties[i].OIMProperty)
                  {{
                    graphObject._properties[i].Value = dtoProperty.Attribute(""value"").Value.ToString();
                  }}
                }}
              }}

              {0}List.Add(graphObject);
            }}

            foreach ({0} dto in {0}List)
            {{
              dtoList.Add(dto);
            }}", graphMap.name);
          }

          dtoServiceWriter.WriteLine();
          dtoServiceWriter.WriteLine("break;");
          dtoServiceWriter.Indent--;
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
      return _dataLayer.RefreshDictionary();
    }");

        dtoServiceWriter.Indent--;
        dtoServiceWriter.WriteLine("}"); 
        dtoServiceWriter.Indent--;
        dtoServiceWriter.WriteLine("}");
        dtoServiceWriter.Close();

        return dtoServiceBuilder.ToString();
      }
      catch (Exception ex)
      {
        _logger.Error(ex.ToString());
        throw ex;
      }
    }

    private string UpdateServiceInterface()
    {
      try
      {
        StringBuilder serviceBuilder = new StringBuilder();
        IndentedTextWriter serviceWriter = new IndentedTextWriter(new StringWriter(serviceBuilder), INDENTATION);

        serviceWriter.WriteLine(Utility.GeneratedCodeProlog());
        serviceWriter.WriteLine("using System.Collections.Generic;");
        serviceWriter.WriteLine("using System.ServiceModel;");
        serviceWriter.WriteLine("using System.ServiceModel.Web;");
        serviceWriter.WriteLine();
        serviceWriter.WriteLine("namespace {0}", ADAPTER_NAMESPACE);
        serviceWriter.WriteLine("{");
        serviceWriter.Indent++;

        serviceWriter.WriteLine("public partial interface IService");
        serviceWriter.WriteLine("{");
        serviceWriter.Indent++;

        serviceWriter.WriteLine("[OperationContract]");
        serviceWriter.WriteLine("[XmlSerializerFormat]");
        WriteKnownTypes(serviceWriter);
        serviceWriter.WriteLine("[WebGet(UriTemplate = \"/{projectName}/{applicationName}/{graphName}\")]");
        serviceWriter.WriteLine("Envelope GetList(string projectName, string applicationName, string graphName);");

        serviceWriter.WriteLine();
        serviceWriter.WriteLine("[OperationContract]");
        serviceWriter.WriteLine("[XmlSerializerFormat]");
        WriteKnownTypes(serviceWriter);
        serviceWriter.WriteLine("[WebGet(UriTemplate = \"/{projectName}/{applicationName}/{graphName}/{identifier}\")]");
        serviceWriter.WriteLine("Envelope Get(string projectName, string applicationName, string graphName, string identifier);");

        serviceWriter.Indent--;
        serviceWriter.WriteLine("}");

        serviceWriter.Indent--;
        serviceWriter.WriteLine("}");
        serviceWriter.Close();

        return serviceBuilder.ToString();
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    private string UpdateDataServiceInterface()
    {
      try
      {
        StringBuilder dataServiceBuilder = new StringBuilder();
        IndentedTextWriter dataServiceWriter = new IndentedTextWriter(new StringWriter(dataServiceBuilder), INDENTATION);

        dataServiceWriter.WriteLine(Utility.GeneratedCodeProlog());
        dataServiceWriter.WriteLine("using System.Collections.Generic;");
        dataServiceWriter.WriteLine("using System.ServiceModel;");
        dataServiceWriter.WriteLine("using System.ServiceModel.Web;");
        dataServiceWriter.WriteLine();
        dataServiceWriter.WriteLine("namespace {0}", ADAPTER_NAMESPACE);
        dataServiceWriter.WriteLine("{");
        dataServiceWriter.Indent++;

        dataServiceWriter.WriteLine("[ServiceContract(Namespace = \"http://ns.iringtools.org/protocol\")]");
        dataServiceWriter.WriteLine("public interface IDataService");
        dataServiceWriter.WriteLine("{");
        dataServiceWriter.Indent++;

        dataServiceWriter.WriteLine("[OperationContract]");
        WriteKnownTypes(dataServiceWriter);
        dataServiceWriter.WriteLine("DTOListResponse GetDataList(DTORequest request);");

        dataServiceWriter.WriteLine();
        dataServiceWriter.WriteLine("[OperationContract]");
        WriteKnownTypes(dataServiceWriter);
        dataServiceWriter.WriteLine("DTOResponse GetData(DTORequest request);");

        dataServiceWriter.Indent--;
        dataServiceWriter.WriteLine("}");

        dataServiceWriter.Indent--;
        dataServiceWriter.WriteLine("}");
        dataServiceWriter.Close();

        return dataServiceBuilder.ToString();
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

    private void WriteKnownTypes(IndentedTextWriter writer)
    {
      try
      {
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
            writer.WriteLine("[ServiceKnownType(typeof({0}))]", className);
          }
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
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

          _dtoModelWriter.WriteLine();
          _dtoModelWriter.WriteLine("[DataMember(EmitDefaultValue = false)]");
          _dtoModelWriter.WriteLine("[XmlIgnore]");
          _dtoModelWriter.WriteLine("public Template{0} tpl_{0} {{ get; set; }}", templateMap.name);
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
                _dtoModelWriter.WriteLine();
                _dtoModelWriter.WriteLine("[DataMember(EmitDefaultValue = false)]");
                _dtoModelWriter.WriteLine("public {0} tpl_{1}_tpl_{2} {{ get; set; }}", dataProperty.dataType, templateName, roleMap.name);
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

  public class ExtendedDataProperty : DataProperty
  {
    public string propertyPath { get; set; }
    public string dtoPropertyPath { get; set; }
    public bool isDataMember { get; set; }
  }
}
