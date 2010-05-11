﻿// Copyright (c) 2009, ids-adi.org /////////////////////////////////////////////
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
using System.Reflection;
using System.Web.Compilation;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ninject;
using Ninject.Parameters;
using Ninject.Contrib.Dynamic;
using org.iringtools.adapter.rules;
using org.iringtools.adapter.semantic;
using org.iringtools.library;
using org.iringtools.utility;
using System.Collections.Specialized;
using Ninject.Modules;
using org.iringtools.adapter.datalayer;
using log4net;
using System.ServiceModel;
using System.Data.SqlClient;
using NHibernate;
using org.ids_adi.qxf;
using System.Xml.Xsl;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Linq;

namespace org.iringtools.adapter
{
  public partial class AdapterProvider //: IAdapter
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterProvider));
    private ISemanticLayer _semanticEngine = null;
    private IDTOLayer _dtoService = null;
    private IKernel _kernel = null;
    private AdapterSettings _settings = null;
    private bool _isAppInitialized = false;

    /// <summary>
    /// Adapter Service Constructor
    /// </summary>
    public AdapterProvider(NameValueCollection settings)
    {
      _kernel = new StandardKernel(new AdapterModule());
      _settings = _kernel.Get<AdapterSettings>(new ConstructorArgument("AppSettings", settings));
      Directory.SetCurrentDirectory(_settings.BaseDirectoryPath);
    }

    public void InitializeApplication(string projectName, string applicationName)
    {
      try
      {
        if (!_isAppInitialized)
        {
          ApplicationSettings applicationSettings = _kernel.Get<ApplicationSettings>(
            new ConstructorArgument("projectName", projectName),
            new ConstructorArgument("applicationName", applicationName)
          );

          string bindingConfigurationPath = _settings.XmlPath + applicationSettings.BindingConfigurationPath;
          BindingConfiguration bindingConfiguration = Utility.Read<BindingConfiguration>(bindingConfigurationPath, false);
          _kernel.Load(new DynamicModule(bindingConfiguration));
          _settings.Mapping = GetMapping(projectName, applicationName);
          _dtoService = _kernel.TryGet<IDTOLayer>("DTOLayer");

          if (_dtoService != null)
          {
            _semanticEngine = _kernel.Get<ISemanticLayer>("SemanticLayer");
          }

          _isAppInitialized = true;
        }
      }
      catch (Exception exception)
      {
        _logger.Error("Error in Initializing Application: " + exception);
        throw new Exception("Error in Initializing Application: " + exception.ToString(), exception);
      }
    }

    /// <summary>
    /// Gets the mapping by reading Mapping.xml.
    /// </summary>
    /// <returns>Returns mapping object.</returns>
    public Mapping GetMapping(string projectName, string applicationName)
    {
      string path = _settings.XmlPath + "Mapping." + projectName + "." + applicationName + ".xml";

      try
      {
        Mapping mapping = null;

        if (File.Exists(path))
        {
          mapping = Utility.Read<Mapping>(path, false);
        }

        return mapping;
      }
      catch (Exception exception)
      {
        _logger.Error("Error in GetMapping: " + exception);
        throw new Exception("Error while getting Mapping from " + path + ". " + exception.ToString(), exception);
      }
    }

    /// <summary>
    /// Gets all of the projects and their corresponding applications.
    /// </summary>
    /// <returns>Returns a strongly typed list of projects, each project has an (Response) Applications property detailing
    /// which applications are available</returns>
    public List<ScopeProject> GetScopes()
    {
      string path = _settings.XmlPath + "Scopes.xml";

      try
      {
        if (File.Exists(path))
        {
          List<ScopeProject> _projects = Utility.Read<List<ScopeProject>>(path);
          return _projects;
        }

        return new List<ScopeProject>();
      }
      catch (Exception exception)
      {
        _logger.Error("Error in GetScopes: " + exception);
        throw new Exception("Error while getting the list of projects/applications from " + path + "." + exception);
      }
    }

    public Manifest GetManifest(string projectName, string applicationName)
    {
      string path = _settings.XmlPath + "Mapping." + projectName + "." + applicationName + ".xml";

      try
      {
        Manifest manifest = new Manifest();

        manifest.Graphs = new List<ManifestGraph>();

        if (File.Exists(path))
        {
          Mapping mapping = Utility.Read<Mapping>(path, false);

          foreach (GraphMap graphMap in mapping.graphMaps)
          {
            ManifestGraph manifestGraph = new ManifestGraph { GraphName = graphMap.name };

            manifest.Graphs.Add(manifestGraph);
          }
        }

        return manifest;
      }
      catch (Exception exception)
      {
        _logger.Error("Error in GetManifest: " + exception);
        throw new Exception("Error while getting the manifest from " + path + "." + exception);
      }
    }
    
    /// <summary>
    /// Gets the Data Dictionary by reading DataDictionary.xml
    /// </summary>
    /// <returns>Returns Data Dictionary object.</returns>
    public DataDictionary GetDictionary(string projectName, string applicationName)
    {
      try
      {
        InitializeApplication(projectName, applicationName);

        return _dtoService.GetDictionary();
      }
      catch (Exception exception)
      {
        _logger.Error("Error in GetDictionary: " + exception);
        throw new Exception("Error while getting Dictionary. " + exception.ToString(), exception);
      }
    }

    /// <summary>
    /// Update mapping file.
    /// </summary>
    /// <param name="mapping">The new mapping object with which the mapping file is to be updated.</param>
    /// <returns>Returns the response as success/failure.</returns>
    public Response UpdateMapping(string projectName, string applicationName, Mapping mapping)
    {
      Response response = new Response();
      string path = _settings.XmlPath + "Mapping." + projectName + "." + applicationName + ".xml";

      try
      {
        Utility.Write<Mapping>(mapping, path, false);
        response.Add("Mapping file updated successfully.");
      }
      catch (Exception exception)
      {
        _logger.Error("Error in UpdateMapping: " + exception);

        response.Level = StatusLevel.Error;
        response.Add("Error while updating Mapping.");
        response.Add(exception.ToString());
      }

      return response;
    }

    /// <summary>
    /// Refreshes Data Dictionary by generating a new DataDictionary.xml from csdl.
    /// </summary>
    /// <returns>Returns the response as success/failure.</returns>
    public Response RefreshDictionary(string projectName, string applicationName)
    {
      Response response = new Response();
      try
      {
        InitializeApplication(projectName, applicationName);

        response = _dtoService.RefreshDictionary();
      }
      catch (Exception exception)
      {
        _logger.Error("Error in RefreshDictionary: " + exception);

        response.Level = StatusLevel.Error;
        response.Add("Error while refreshing Dictionary.");
        response.Add(exception.ToString());
      }

      return response;
    }

    /// <summary>
    /// Gets the data for a graphname and identifier in a QXF format.
    /// </summary>
    /// <param name="graphName">The name of graph for which data is to be fetched.</param>
    /// <param name="identifier">The unique identifier used as filter to return single row's data.</param>
    /// <returns>Returns the data in QXF format.</returns>
    public DataTransferObject GetDTO(string projectName, string applicationName, string graphName, string identifier)
    {
      try
      {
        InitializeApplication(projectName, applicationName);

        DataTransferObject dto = _dtoService.GetDTO(graphName, identifier);

        return dto;
      }
      catch (Exception exception)
      {
        _logger.Error("Error in GetDTO: " + exception);
        throw new Exception("Error while getting " + graphName + " data with identifier " + identifier + ". " + exception.ToString(), exception);
      }
    }

    /// <summary>
    /// Gets all the data for the graphname.
    /// </summary>
    /// <param name="graphName">The name of graph for which data is to be fetched.</param>
    /// <returns>Returns the data in QXF format.</returns>
    public List<DataTransferObject> GetDTOList(string projectName, string applicationName, string graphName)
    {
      try
      {
        InitializeApplication(projectName, applicationName);

        List<DataTransferObject> dtoList = _dtoService.GetList(graphName);

        return dtoList;
      }
      catch (Exception exception)
      {
        _logger.Error("Error in GetDTOList: " + exception);
        throw new Exception("Error while getting " + graphName + " data. " + exception.ToString(), exception);
      }
    }

    /// <summary>
    /// Refreshes the triple store for all the graphmaps.
    /// </summary>
    /// <returns>Returns the response as success/failure.</returns>
    public Response RefreshAll(string projectName, string applicationName)
    {
      Response response = new Response();
      try
      {
        InitializeApplication(projectName, applicationName);

        DateTime start = DateTime.Now;

        foreach (GraphMap graphMap in _settings.Mapping.graphMaps)
        {
          response.Append(RefreshGraph(projectName, applicationName, graphMap.name));
        }

        DateTime end = DateTime.Now;
        TimeSpan duration = end.Subtract(start);

        response.Add(String.Format("RefreshAll() Execution Time [{0}:{1}.{2}] minutes.", 
          duration.Minutes, duration.Seconds, duration.Milliseconds));
      }
      catch (Exception exception)
      {
        _logger.Error("Error in RefreshAll: " + exception);

        response.Level = StatusLevel.Error;
        response.Add("Error while Refreshing TripleStore.");
        response.Add(exception.ToString());
      }
      return response;
    }

    /// <summary>
    /// Refreshes the triple store for the graphmap passed.
    /// </summary>
    /// <param name="graphName">The name of graph for which triple store will be refreshed.</param>
    /// <returns>Returns the response as success/failure.</returns>
    public Response RefreshGraph(string projectName, string applicationName, string graphName)
    {
      Response response = new Response();
      try
      {
        InitializeApplication(projectName, applicationName);

        _semanticEngine.Initialize();

        DateTime start = DateTime.Now;

        List<DataTransferObject> dtoList = _dtoService.GetList(graphName);

        List<string> tripleStoreIdentifiers = _semanticEngine.GetIdentifiers(graphName);
        List<string> identifiersToBeDeleted = tripleStoreIdentifiers;
        foreach (DataTransferObject commonDTO in dtoList)
        {
          if (tripleStoreIdentifiers.Contains(commonDTO.Identifier))
          {
            identifiersToBeDeleted.Remove(commonDTO.Identifier);
          }
        }

        response.Append(_semanticEngine.Delete(graphName, identifiersToBeDeleted));

        RuleEngine ruleEngine = new RuleEngine();
        if (File.Exists(_settings.XmlPath + "Refresh" + graphName + ".rules"))
        {
          dtoList = ruleEngine.RuleSetForCollection(dtoList, _settings.XmlPath + "Refresh" + graphName + ".rules");
        }

        response.Append(_semanticEngine.Post(graphName, dtoList));
        
        DateTime end = DateTime.Now;
        TimeSpan duration = end.Subtract(start);

        response.Add(String.Format("RefreshGraph({0}) Execution Time [{1}:{2}.{3}] minutes.", 
          graphName, duration.Minutes, duration.Seconds, duration.Milliseconds));
      }
      catch (Exception exception)
      {
        _logger.Error("Error in RefreshGraph: " + exception);

        response.Level = StatusLevel.Error;
        response.Add("Error while Refreshing TripleStore for GraphMap[" + graphName + "].");
        response.Add(exception.ToString());
      }

      return response;
    }

    /// <summary>
    /// Creates RDF for a graph
    /// </summary>
    /// <param name="projectName"></param>
    /// <param name="applicationName"></param>
    /// <param name="graphName"></param>
    /// <returns>success/failed</returns>
    public XElement Get(string projectName, string applicationName, string graphName, string identifier, string format)
    {
      XElement result = null;
      try
      {
        InitializeApplication(projectName, applicationName);

        List<DataTransferObject> dtoList = new List<DataTransferObject>();

        dtoList.Add(_dtoService.GetDTO(graphName, identifier));

        XElement graph = _dtoService.SerializeDTO(graphName, dtoList);

        if (format != null)
        {
          ITransformationLayer transformEngine = _kernel.Get<ITransformationLayer>(format);
          result = transformEngine.Transform(graphName, graph);
        }
        else
        {
          result = graph;
        }
        
      }
      catch (Exception exception)
      {
        throw new Exception("Error in CreateGraphRDF: " + exception);
      }

      return result;
    }

    /// <summary>
    /// Creates RDF for a graph
    /// </summary>
    /// <param name="projectName"></param>
    /// <param name="applicationName"></param>
    /// <param name="graphName"></param>
    /// <returns>success/failed</returns>
    public XElement GetList(string projectName, string applicationName, string graphName, string format)
    {
        XElement result = null;
        try
        {
            InitializeApplication(projectName, applicationName);

            List<DataTransferObject> dtoList = _dtoService.GetList(graphName);

            XElement graph = null;
            if (format == "xml")
            {
              graph = _dtoService.SerializeXML(graphName, dtoList);
            }
            else
            {
              graph = _dtoService.SerializeDTO(graphName, dtoList);
            }

            if (format != null && format != "xml")
            {
                ITransformationLayer transformEngine = _kernel.Get<ITransformationLayer>(format);
                result = transformEngine.Transform(graphName, graph);
            }
            else
            {
                result = graph;
            }
        }
        catch (Exception exception)
        {
            throw new Exception("Error in CreateGraphRDF: " + exception);
        }

        return result;
    }

    /// <summary>
    /// Pulls the data from a triple store into the database.
    /// </summary>
    /// <param name="request">The request parameter containing targetUri, targetCredentials, graphName, filter will be passed.</param>
    /// <returns>Returns the response as success/failure.</returns>
    public Response Pull(string projectName, string applicationName, Request request)
    {
      string targetUri = String.Empty;
      string targetCredentialsXML = String.Empty;
      string graphName = String.Empty;
      string filter = String.Empty;
      Response response = new Response();

      try
      {
        InitializeApplication(projectName, applicationName);

        _settings.InterfaceServer = request["targetUri"];
        targetCredentialsXML = request["targetCredentials"];
        graphName = request["graphName"];
        filter = request["filter"];

        WebCredentials targetCredentials = Utility.Deserialize<WebCredentials>(targetCredentialsXML, true);
        if (targetCredentials.isEncrypted) targetCredentials.Decrypt();

        _settings.TargetCredentials = targetCredentials;

        DateTime b = DateTime.Now;
        DateTime e;
        TimeSpan d;

        _semanticEngine.Initialize();

        List<DataTransferObject> dtoList = _semanticEngine.Get(graphName);

        
        RuleEngine ruleEngine = new RuleEngine();
        if (File.Exists(_settings.XmlPath + "Pull" + graphName + ".rules"))
        {
          dtoList = ruleEngine.RuleSetForCollection(dtoList, _settings.XmlPath + "Pull" + graphName + ".rules");
        }

        e = DateTime.Now;
        d = e.Subtract(b);
        response.Add(String.Format("PullQuery[{0}] Execution Time [{1}:{2}.{3}] minutes.", graphName, d.Minutes, d.Seconds, d.Milliseconds));
        b = e;

        response.Append(_dtoService.PostList(graphName, dtoList));

        e = DateTime.Now;
        d = e.Subtract(b);
        response.Add(String.Format("Pull[{0},{1}] Execution Time [{2}:{3}.{4}] minutes.", targetUri, graphName, d.Minutes, d.Seconds, d.Milliseconds));
      }
      catch (Exception exception)
      {
        _logger.Error("Error in Pull: " + exception);

        response.Level = StatusLevel.Error;
        response.Add("Error while pulling " + graphName + " data from " + targetUri + " as " + targetUri + " data with filter " + filter + ".\r\n");
        response.Add(exception.ToString());
      }

      return response;
    }

    public Response PullDTO(string projectName, string applicationName, Request request)
    {
      String targetUri = String.Empty;
      String targetCredentialsXML = String.Empty;
      String graphName = String.Empty;
      String filter = String.Empty;
      String projectNameForPull = String.Empty;
      String applicationNameForPull = String.Empty;
      String dtoListString = String.Empty;
      Response response = new Response();
      try
      {
        InitializeApplication(projectName, applicationName);

        targetUri = request["targetUri"];
        targetCredentialsXML = request["targetCredentials"];
        graphName = request["graphName"];
        filter = request["filter"];
        projectNameForPull = request["projectName"];
        applicationNameForPull = request["applicationName"];

        WebCredentials targetCredentials = Utility.Deserialize<WebCredentials>(targetCredentialsXML, true);
        if (targetCredentials.isEncrypted) targetCredentials.Decrypt();

        WebHttpClient httpClient = new WebHttpClient(targetUri);
        if (filter != String.Empty)
        {
          dtoListString = httpClient.GetMessage(@"/" + projectNameForPull + "/" + applicationNameForPull + "/" + graphName + "/" + filter);
        }
        else
        {
          dtoListString = httpClient.GetMessage(@"/" + projectNameForPull + "/" + applicationNameForPull + "/" + graphName);
        }
        List<DataTransferObject> dtoList = (List<DataTransferObject>)_dtoService.CreateList(graphName, dtoListString);
        response.Append(_dtoService.PostList(graphName, dtoList));
        response.Add(String.Format("Pull is successful from " + targetUri + "for Graph " + graphName));
      }
      catch (Exception exception)
      {
        _logger.Error("Error in PullDTO: " + exception);

        response.Level = StatusLevel.Error;
        response.Add("Error while pulling " + graphName + " data from " + targetUri + " as " + targetUri + " data with filter " + filter + ".\r\n");
        response.Add(exception.ToString());
      }

      return response;
    }

    /// <summary>
    /// Refreshes the triple store for all the graphmaps.
    /// </summary>
    /// <returns>Returns the response as success/failure.</returns>
    public Response ClearAll(string projectName, string applicationName)
    {
      Response response = new Response();
      try
      {
        InitializeApplication(projectName, applicationName);

        DateTime start = DateTime.Now;

        foreach (GraphMap graphMap in _settings.Mapping.graphMaps)
        {
          response.Append(ClearGraph(projectName, applicationName, graphMap.name));
        }

        DateTime end = DateTime.Now;
        TimeSpan duration = end.Subtract(start);

        response.Add(String.Format("ClearAll() Execution Time [{0}:{1}.{2}] minutes.",
          duration.Minutes, duration.Seconds, duration.Milliseconds));
      }
      catch (Exception exception)
      {
        _logger.Error("Error in ClearAll: " + exception);

        response.Level = StatusLevel.Error;
        response.Add("Error while clearing TripleStore.");
        response.Add(exception.ToString());
      }
      return response;
    }

    public Response ClearGraph(string projectName, string applicationName, string graphName)
    {
      Response response = new Response();
      try
      {
        InitializeApplication(projectName, applicationName);

        _semanticEngine.Initialize();

        _semanticEngine.Clear(graphName);

        response.Add(graphName + " graph cleared successfully.");
      }
      catch (Exception exception)
      {
        _logger.Error("Error in Clear: " + exception);

        response.Level = StatusLevel.Error;
        response.Add("Error while clearing TripleStore graph.");
        response.Add(exception.ToString());
        response.Level = StatusLevel.Error;
      }

      return response;
    }

    /// <summary>
    /// Generated DTO Model and Service.
    /// </summary>
    /// <returns>Returns the response as success/failure.</returns>
    public Response Generate(string projectName, string applicationName)
    {
      Response response = new Response();
      DTOGenerator dtoGenerator = new DTOGenerator(_settings);

      try
      {
        // Generate DTO code
        dtoGenerator.Generate(projectName, applicationName);

        // Update NInject binding configuration
        Binding dtoServiceBinding = new Binding()
        {
          Name = "DTOLayer",
          Interface = "org.iringtools.adapter.IDTOLayer, AdapterLibrary",
          Implementation = "org.iringtools.adapter.proj_" + projectName + "." + applicationName + ".DTOService, App_Code"
        };
        UpdateBindingConfiguration(projectName, applicationName, dtoServiceBinding);

        // Set hasDTOLayer flag for the application to true
        UpdateScopes(projectName, applicationName, true);

        response.Add("DTO Model generated successfully.");
      }
      catch (Exception exception)
      {
        _logger.Error("Error in Generate: " + exception);

        response.Level = StatusLevel.Error;
        response.Add("Error generating DTO Model.");
        response.Add(exception.ToString());
      }

      return response;
    }

    /// <summary>
    /// Deletes application references from the interface service web config
    /// </summary>
    /// <param name="projectName"></param>
    /// <param name="applicationName"></param>
    private void UpdateInterfaceServiceWebConfig(string projectName, string applicationName)
    {
      try
      {
        string path = _settings.InterfaceServerPath + "Web.config";
        if (File.Exists(path))
        {
          StreamReader reader = new StreamReader(path);
          StringBuilder builder = new StringBuilder();

          #region Read and buffer all lines that does not contain the application name
          string line = String.Empty;
          while ((line = reader.ReadLine()) != null)
          {
            if (!line.ToUpper().Contains(projectName.ToUpper() + "/" + applicationName.ToUpper()))
            {
              builder.Append(line + System.Environment.NewLine);
            }
          }

          reader.Close();
          #endregion

          // Write buffer back to file
          StreamWriter writer = new StreamWriter(path);
          writer.WriteLine(builder.ToString());
          writer.Close();
        }
        else
        {
          throw new Exception("Interface Service Path not found.");
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    /// <summary>
    /// Deletes application service known types from IService.Generated.cs
    /// </summary>
    /// <param name="projectName"></param>
    /// <param name="applicationName"></param>
    private void UpdateIService(string projectName, string applicationName)
    {
      try
      {
        string path = _settings.CodePath + "IService.Generated.cs";
        if (File.Exists(path))
        {
          StreamReader reader = new StreamReader(path);
          StringBuilder builder = new StringBuilder();

          #region Read and buffer all lines that does not contain the application name
          string line = String.Empty;
          while ((line = reader.ReadLine()) != null)
          {
            if (!line.ToUpper().Contains(projectName.ToUpper() + "." + applicationName.ToUpper()))
            {
              builder.Append(line + System.Environment.NewLine);
            }
          }

          reader.Close();
          #endregion

          // Write buffer back to file
          StreamWriter writer = new StreamWriter(path);
          writer.WriteLine(builder.ToString());
          writer.Close();
        }
        else
        {
          throw new Exception("File IService.Generated.cs not found.");
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    /// <summary>
    /// Deletes application service known types from IDataService.Generated.cs
    /// </summary>
    /// <param name="projectName"></param>
    /// <param name="applicationName"></param>
    private void UpdateIDataService(string projectName, string applicationName)
    {
      try
      {
        string path = _settings.CodePath + "IDataService.Generated.cs";
        if (File.Exists(path))
        {
          StreamReader reader = new StreamReader(path);
          StringBuilder builder = new StringBuilder();

          #region Read and buffer all lines that does not contain the application name
          string line = String.Empty;
          while ((line = reader.ReadLine()) != null)
          {
            if (!line.ToUpper().Contains(projectName.ToUpper() + "." + applicationName.ToUpper()))
            {
              builder.Append(line + System.Environment.NewLine);
            }
          }

          reader.Close();
          #endregion

          // Write buffer back to file
          StreamWriter writer = new StreamWriter(path);
          writer.WriteLine(builder.ToString());
          writer.Close();
        }
        else
        {
          throw new Exception("File IDataService.Generated.cs not found.");
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    private void DropDatabase(string projectName, string applicationName, string connectionString)
    {
      try
      {
        string _sqlCheckDatabase = @"SELECT name FROM sys.databases WHERE name = N'@token'";
        string _sqlDropDatabase = @"DROP DATABASE [@token]";
        string _sqlCheckLogin = @"SELECT * FROM sys.syslogins WHERE name = N'@token'";
        string _sqlDropLogin = @"DROP LOGIN [@token]";
        string _scopeName = projectName + "_" + applicationName;

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
          string sqlCheckDatabase = _sqlCheckDatabase.Replace("@token", _scopeName);

          connection.Open();

          object databaseName = null;
          using (SqlCommand command = new SqlCommand(sqlCheckDatabase, connection))
          {
            databaseName = command.ExecuteScalar();
          }

          if (databaseName != null)
          {
            string sqlDropDatabase = _sqlDropDatabase.Replace("@token", _scopeName);

            using (SqlCommand command = new SqlCommand(sqlDropDatabase, connection))
            {
              command.ExecuteNonQuery();
            }
          }

          string sqlCheckLogin = _sqlCheckLogin.Replace("@token", _scopeName);

          object loginName = null;
          using (SqlCommand command = new SqlCommand(sqlCheckLogin, connection))
          {
            loginName = command.ExecuteScalar();
          }

          if (loginName != null)
          {
            string sqlDropLogin = _sqlDropLogin.Replace("@token", _scopeName);

            using (SqlCommand command = new SqlCommand(sqlDropLogin, connection))
            {
              command.ExecuteNonQuery();
            }
          }
        }
      }
      catch (Exception exception)
      {
        throw new Exception("Error while dropping the triplestore database ", exception);
      }
    }

    /// <summary>
    /// Delete and application by removing it from scopes.xml and its generated code/xml
    /// </summary>
    /// <param name="projectName"></param>
    /// <param name="applicationName"></param>
    /// <returns></returns>
    public Response Delete(string projectName, string applicationName)
    {
      Response response = new Response();
      
      try
      {
        DeleteApplicationScope(projectName, applicationName);
        
        UpdateInterfaceServiceWebConfig(projectName, applicationName);
        UpdateIService(projectName, applicationName);
        UpdateIDataService(projectName, applicationName);

        File.Delete(_settings.CodePath + "DTOService." + projectName + "." + applicationName + ".cs");
        File.Delete(_settings.CodePath + "DTOModel." + projectName + "." + applicationName + ".cs");
        File.Delete(_settings.CodePath + "Model." + projectName + "." + applicationName + ".cs");

        File.Delete(_settings.XmlPath + "BindingConfiguration." + projectName + "." + applicationName + ".xml");
        File.Delete(_settings.XmlPath + "DataDictionary." + projectName + "." + applicationName + ".xml");
        File.Delete(_settings.XmlPath + "nh-configuration." + projectName + "." + applicationName + ".xml");
        File.Delete(_settings.XmlPath + "nh-mapping." + projectName + "." + applicationName + ".xml");

        #region Delete application database
        try
        {
          string triplestoreConnStrPrefix = @"sqlserver:rdf:Database=rdf;";
          string masterConnStr = _settings.TripleStoreConnectionString.Remove(0, triplestoreConnStrPrefix.Length);
          DropDatabase(projectName, applicationName, masterConnStr);   
        }
        catch (Exception ex)
        {
          response.Level = StatusLevel.Warning;
          response.Add("WARNING: Delete application database failed");
          response.Add(ex.ToString());
        }
        #endregion

        response.Level = StatusLevel.Success;
        response.Add("Application deleted successfully.");
      }
      catch (Exception exception)
      {
        _logger.Error("Error in Delete: " + exception);

        response.Level = StatusLevel.Error;
        response.Add("Error deleting application.");
        response.Add(exception.ToString());
      }

      return response;
    }

    /// <summary>
    /// Update NInject binding config file.
    /// </summary>
    /// <param name="projectName"></param>
    /// <param name="applicationName"></param>
    /// <param name="binding"></param>
    private void UpdateBindingConfiguration(string projectName, string applicationName, Binding binding)
    {
      try
      {
        string bindingConfigurationPath = _settings.XmlPath + "BindingConfiguration." + projectName + "." + applicationName + ".xml";

        if (File.Exists(bindingConfigurationPath))
        {
          BindingConfiguration bindingConfiguration = Utility.Read<BindingConfiguration>(bindingConfigurationPath, false);
          bool bindingExists = false;

          // Update binding if exists
          for (int i = 0; i < bindingConfiguration.Bindings.Count; i++)
          {
            if (bindingConfiguration.Bindings[i].Name.ToUpper() == binding.Name.ToUpper())
            {
              bindingConfiguration.Bindings[i] = binding;
              bindingExists = true;
              break;
            }
          }

          // Add binding if not exist
          if (!bindingExists)
          {
            bindingConfiguration.Bindings.Add(binding);
          }

          Utility.Write<BindingConfiguration>(bindingConfiguration, bindingConfigurationPath, false);
        }
        else
        {
          BindingConfiguration bindingConfiguration = new BindingConfiguration();
          bindingConfiguration.Bindings = new List<Binding>();
          bindingConfiguration.Bindings.Add(binding);
          Utility.Write<BindingConfiguration>(bindingConfiguration, bindingConfigurationPath, false);
        }
      }
      catch (Exception exception)
      {
        _logger.Error("Error in UpdateBindingConfiguration: " + exception);
        throw exception;
      }
    }

    /// <summary>
    /// Update Scopes.xml to add project/application if it has not been added
    /// </summary>
    /// <param name="projectName"></param>
    /// <param name="applicationName"></param>
    private void UpdateScopes(string projectName, string applicationName, bool hasDTOLayer)
    {
      try
      {
        string scopesPath = _settings.XmlPath + "Scopes.xml";

        if (File.Exists(scopesPath))
        {
          List<ScopeProject> projects = Utility.Read<List<ScopeProject>>(scopesPath);
          bool projectExists = false;
          
          foreach (ScopeProject project in projects)
          {
            bool applicationExists = false;
            
            if (project.Name.ToUpper() == projectName.ToUpper())
            {
              foreach (ScopeApplication application in project.Applications)
              {
                if (application.Name.ToUpper() == applicationName.ToUpper())
                {
                  applicationExists = true;

                  if (hasDTOLayer)
                  {
                    application.hasDTOLayer = true;
                  }

                  break;
                }
              }

              if (!applicationExists)
              {
                project.Applications.Add(new ScopeApplication() { Name = applicationName, hasDTOLayer = false });
              }

              projectExists = true;
              break;
            }
          }

          // project does not exist, add it
          if (!projectExists)
          {
            ScopeProject project = new ScopeProject()
            {
               Name = projectName,
               Applications = new List<ScopeApplication>()
               {
                 new ScopeApplication()
                 {
                   Name = applicationName,
                   hasDTOLayer = false
                 }
               }
            };

            projects.Add(project);
          }

          Utility.Write<List<ScopeProject>>(projects, scopesPath, true);
        }
        else
        {
          List<ScopeProject> projects = new List<ScopeProject>()
          {
            new ScopeProject()
            {
              Name = projectName,
              Applications = new List<ScopeApplication>()
               {
                 new ScopeApplication()
                 {
                   Name = applicationName,
                   hasDTOLayer = false
                 }
               }
            }
          };

          Utility.Write<List<ScopeProject>>(projects, scopesPath, true);
        }
      }
      catch (Exception exception)
      {
        _logger.Error("Error in UpdateScopes: " + exception);
        throw exception;
      }
    }

    /// <summary>
    /// Removes an application scope
    /// </summary>
    /// <param name="projectName"></param>
    /// <param name="applicationName"></param>
    private void DeleteApplicationScope(string projectName, string applicationName)
    {
      bool isDeleted = false;
      try
      {
        string scopesPath = _settings.XmlPath + "Scopes.xml";

        if (File.Exists(scopesPath))
        {
          List<ScopeProject> projects = Utility.Read<List<ScopeProject>>(scopesPath);

          foreach (ScopeProject project in projects)
          {
            if (project.Name.ToUpper() == projectName.ToUpper())
            {
              foreach (ScopeApplication application in project.Applications)
              {
                if (application.Name.ToUpper() == applicationName.ToUpper())
                {
                  project.Applications.Remove(application);
                  isDeleted = true;
                  break;
                }
              }

              break;
            }
          }

          if (isDeleted)
          {
            Utility.Write<List<ScopeProject>>(projects, scopesPath, true);
          }
          else
          {
            throw new Exception("Scope Application not found.");
          }
        }
      }
      catch (Exception exception)
      {
        _logger.Error("Error in DeleteApplicationScope: " + exception);
        throw exception;
      }
    }

    private bool Validate(DatabaseDictionary dbDictionary)
    {
      ISession session = null;

      try
      {
        // Validate connection string
        string connectionString = dbDictionary.connectionString;
        NHibernate.Cfg.Configuration config = new NHibernate.Cfg.Configuration();
        Dictionary<string, string> properties = new Dictionary<string, string>();

        properties.Add("connection.provider", "NHibernate.Connection.DriverConnectionProvider");
        properties.Add("connection.connection_string", dbDictionary.connectionString);
        properties.Add("proxyfactory.factory_class", "NHibernate.ByteCode.Castle.ProxyFactoryFactory, NHibernate.ByteCode.Castle");
        properties.Add("dialect", "NHibernate.Dialect." + dbDictionary.provider + "Dialect");

        if (dbDictionary.provider.ToString().ToUpper().Contains("MSSQL"))
        {
          properties.Add("connection.driver_class", "NHibernate.Driver.SqlClientDriver");
        }
        else if (dbDictionary.provider.ToString().ToUpper().Contains("ORACLE"))
        {
          properties.Add("connection.driver_class", "NHibernate.Driver.OracleClientDriver");
        }
        else
        {
          throw new Exception("Database not supported.");
        }

        config.AddProperties(properties);
        ISessionFactory factory = config.BuildSessionFactory();

        session = factory.OpenSession();
      }
      catch (Exception ex)
      {
        throw new Exception("Invalid connection string: " + ex.Message);
      }
      finally
      {
        if (session != null) session.Close();
      }

      // Validate table key
      foreach (DataObject dataObject in dbDictionary.dataObjects)
      {
        if (dataObject.keyProperties == null || dataObject.keyProperties.Count == 0)
        {
          throw new Exception("Table \"" + dataObject.tableName + "\" has no key.");
        }
      }

      return true;
    }

    /// <summary>
    /// Generate entities, hibernate-mapping, hibernate-configuration, and data dictionary for the application
    /// </summary>
    /// <returns>Returns the response as success/failure.</returns>
    public Response UpdateDatabaseDictionary(DatabaseDictionary dbDictionary, string projectName, string applicationName)
    {
      Response response = new Response();

      try
      {
        if (String.IsNullOrEmpty(projectName) || String.IsNullOrEmpty(applicationName))
        {
          response.Add("Error project name and application name can not be null");
        }
        else if (Validate(dbDictionary))
        {
          EntityGenerator generator = _kernel.Get<EntityGenerator>();
          response = generator.Generate(dbDictionary, projectName, applicationName);
          
          // Update binding configuration
          Binding semanticLayerBinding = new Binding()
          {
            Name = "SemanticLayer",
            Interface = "org.iringtools.adapter.ISemanticLayer, AdapterLibrary",
            Implementation = "org.iringtools.adapter.semantic.SemWebRDFEngine, AdapterLibrary"
          };
          UpdateBindingConfiguration(projectName, applicationName, semanticLayerBinding);

          Binding dataLayerBinding = new Binding()
          {
            Name = "DataLayer",
            Interface = "org.iringtools.library.IDataLayer2, iRINGLibrary",
            Implementation = "org.iringtools.adapter.datalayer.NHibernateDataLayer2, NHibernateDataLayer"
          };
          UpdateBindingConfiguration(projectName, applicationName, dataLayerBinding);

          // Update project/application scope to add application if it does not exist
          UpdateScopes(projectName, applicationName, false);
          
          // Generate default mapping
          string mappingPath = _settings.XmlPath + "Mapping." + projectName + "." + applicationName + ".xml";
          if (!File.Exists(mappingPath))
          {
            Utility.Write<Mapping>(new Mapping(), mappingPath, false);
          }

          // Generate DTO layer if it does not exist
          string dtoModelPath = _settings.CodePath + "DTOModel." + projectName + "." + applicationName + ".cs";
          if (!File.Exists(dtoModelPath))
          {
            Generate(projectName, applicationName);
          }

          response.Add("Database dictionary updated successfully.");
        }
      }
      catch (Exception exception)
      {
        if (File.Exists(_settings.CodePath + "Model." + projectName + "." + applicationName + ".cs"))
        {
          File.Delete(_settings.CodePath + "Model." + projectName + "." + applicationName + ".cs");
        }

        _logger.Error("Error in UpdateDatabaseDictionary: " + exception);

        response.Level = StatusLevel.Error;
        response.Add("Error updating database dictionary: " + exception);
      }

      return response;
    }
  }
}