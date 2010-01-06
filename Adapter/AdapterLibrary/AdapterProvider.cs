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
using System.Reflection;
using System.Web.Compilation; 
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TextTemplating;
using Ninject;
using Ninject.Parameters;
using Ninject.Contrib.Dynamic;
using org.iringtools.adapter.rules;
using org.iringtools.adapter.semantic;
using org.iringtools.library;
using org.iringtools.utility;
using System.Collections.Specialized;
using Ninject.Modules;
using org.iringtools.adapter.dataLayer;

namespace org.iringtools.adapter
{
  public partial class AdapterProvider //: IAdapter
  {
    private ISemanticEngine _semanticEngine = null;
    private IDTOService _dtoService = null;
    private IKernel _kernel = null;
    private AdapterSettings _settings = null;
    private string _mappingPath = String.Empty;
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
      ApplicationSettings applicationSettings = _kernel.Get<ApplicationSettings>(
        new ConstructorArgument("projectName", projectName),
        new ConstructorArgument("applicationName", applicationName)
      );

      string bindingConfigurationPath = _settings.XmlPath + applicationSettings.BindingConfigurationPath;
      BindingConfiguration bindingConfiguration = Utility.Read<BindingConfiguration>(bindingConfigurationPath, false);
      _kernel.Load(new DynamicModule(bindingConfiguration));      
      _settings.Mapping = GetMapping(projectName, applicationName);
      _dtoService = _kernel.Get<IDTOService>("DTOService." + projectName + "." + applicationName);

      if (_settings.UseSemweb)
      {
        _semanticEngine = _kernel.Get<ISemanticEngine>("SemWeb");
      }
      else
      {
        _semanticEngine = _kernel.Get<ISemanticEngine>("Sparql");
      }
    }

    public void UninitializeApplication()
    {
      //_kernel.Unload("Ninject.Contrib.Dynamic.DynamicModule, Ninject.Contrib.Dynamic");
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
        return Utility.Read<Mapping>(path, false);
      }
      catch (Exception exception)
      {
        throw new Exception("Error while getting Mapping from " + path + ". " + exception.ToString(), exception);
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
        throw new Exception("Error while getting Dictionary. " + exception.ToString(), exception);
      }
      finally
      {
        UninitializeApplication();
      }
    }

    /// <summary>
    /// Updates mapping.
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
        response.Add("Error while updating Mapping.");
        response.Add(exception.ToString());
      }
      finally
      {
        UninitializeApplication();
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
        response.Add("Error while refreshing Dictionary.");
        response.Add(exception.ToString());
      }
      finally
      {
        UninitializeApplication();
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
        throw new Exception("Error while getting " + graphName + " data with identifier " + identifier + ". " + exception.ToString(), exception);
      }
      finally
      {
        UninitializeApplication();
      }
    }

    /// <summary>
    /// Gets the data for a graphname and identifier in a QXF format.
    /// </summary>
    /// <param name="graphName">The name of graph for which data is to be fetched.</param>
    /// <param name="identifier">The unique identifier used as filter to return single row's data.</param>
    /// <returns>Returns the data in QXF format.</returns>
    public Envelope Get(string projectName, string applicationName, string graphName, string identifier)
    {
      try
      {
        InitializeApplication(projectName, applicationName);

        Envelope envelope = new Envelope();

        DataTransferObject dto = _dtoService.GetDTO(graphName, identifier);

        envelope.Payload.Add(dto);

        return envelope;
      }
      catch (Exception exception)
      {
        throw new Exception("Error while getting " + graphName + " data with identifier " + identifier + ". " + exception.ToString(), exception);
      }
      finally
      {
        UninitializeApplication();
      }
    }

    /// <summary>
    /// Gets all the data for the graphname.
    /// </summary>
    /// <param name="graphName">The name of graph for which data is to be fetched.</param>
    /// <returns>Returns the data in QXF format.</returns>
    public Envelope GetList(string projectName, string applicationName, string graphName)
    {
      try
      {
        InitializeApplication(projectName, applicationName);

        Envelope envelope = new Envelope();

        List<DataTransferObject> dtoList = _dtoService.GetList(graphName);

        envelope.Payload = dtoList;

        return envelope;
      }
      catch (Exception exception)
      {
        throw new Exception("Error while getting " + graphName + " data. " + exception.ToString(), exception);
      }
      finally
      {
        UninitializeApplication();
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
        throw new Exception("Error while getting " + graphName + " data. " + exception.ToString(), exception);
      }
      finally
      {
        UninitializeApplication();
      }
    }

    /// <summary>
    /// Gets all the data for the graphname.
    /// </summary>
    /// <param name="graphName">The name of graph for which data is to be fetched.</param>
    /// <returns>Returns the data in QXF format.</returns>
    public Dictionary<string, string> GetDTOListREST(string projectName, string applicationName, string graphName)
    {
      try
      {
        InitializeApplication(projectName, applicationName);

        Dictionary<string, string> dtoList = _dtoService.GetListREST(graphName);

        return dtoList;
      }
      catch (Exception exception)
      {
        throw new Exception("Error while getting " + graphName + " data. " + exception.ToString(), exception);
      }
      finally
      {
        UninitializeApplication();
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
        _isAppInitialized = true;
        DateTime b = DateTime.Now;

        foreach (GraphMap graphMap in _settings.Mapping.graphMaps)
        {
          response.Append(RefreshGraph(projectName, applicationName, graphMap.name));
        }

        DateTime e = DateTime.Now;
        TimeSpan d = e.Subtract(b);

        response.Add(String.Format("RefreshAll() Execution Time [{0}:{1}.{2}] Seconds ", d.Minutes, d.Seconds, d.Milliseconds));
      }
      catch (Exception exception)
      {
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
        if(!_isAppInitialized)
        InitializeApplication(projectName, applicationName);

        DateTime b = DateTime.Now;

        List<DataTransferObject> commonDTOList = _dtoService.GetList(graphName);       

        List<string> tripleStoreIdentifiers = _semanticEngine.GetIdentifiersFromTripleStore(graphName);
        List<string> identifiersToBeDeleted = tripleStoreIdentifiers;
        foreach (DataTransferObject commonDTO in commonDTOList)
        {
          if (tripleStoreIdentifiers.Contains(commonDTO.Identifier))
          {
            identifiersToBeDeleted.Remove(commonDTO.Identifier);
          }
        }
        foreach (String identifier in identifiersToBeDeleted)
        {
          _semanticEngine.RefreshDelete(graphName, identifier);
        }

        RuleEngine ruleEngine = new RuleEngine();
        if (File.Exists(_settings.XmlPath + "Refresh" + graphName + ".rules"))
        {
          commonDTOList = ruleEngine.RuleSetForCollection(commonDTOList, _settings.XmlPath + "Refresh" + graphName + ".rules");
        }
        foreach (DataTransferObject commonDTO in commonDTOList)
        {
          try
          {
            response.Append(RefreshDTO(commonDTO));
          }
          catch (Exception exception)
          {
            response.Add(exception.ToString());
          }
        }

        DateTime e = DateTime.Now;
        TimeSpan d = e.Subtract(b);

        response.Add(String.Format("RefreshGraph({0}) Execution Time [{1}:{2}.{3}] Seconds ", graphName, d.Minutes, d.Seconds, d.Milliseconds));

        if (_settings.UseSemweb)
          _semanticEngine.DumpStoreData(_settings.XmlPath);
      }
      catch (Exception exception)
      {
        response.Add("Error while Refreshing TripleStore for GraphMap[" + graphName + "].");
        response.Add(exception.ToString());
      }
      finally
      {
        UninitializeApplication();
      }
      return response;
    }

    /// <summary>
    /// This is the private method for refreshing the triple store for this dto.
    /// </summary>
    /// <param name="dto">The triple store will be refreshed with this dto passes.</param>
    /// <returns>Returns the response as success/failure.</returns>
    private Response RefreshDTO(DataTransferObject dto)
    {
      Response response = new Response();
      try
      {
        DateTime b = DateTime.Now;

        if (!_settings.UseSemweb)
        {
          _semanticEngine.RefreshQuery(dto);
        }
        else
        {
          _semanticEngine.RefreshQuery(dto);
        }

        DateTime e = DateTime.Now;
        TimeSpan d = e.Subtract(b);

        response.Add(String.Format("RefreshDTO({0},{1}) Execution Time [{2}:{3}.{4}] Seconds", dto.GraphName, dto.Identifier, d.Minutes, d.Seconds, d.Milliseconds));
      }
      catch (Exception exception)
      {
        response.Add("Error while RefreshDTO[" + dto.GraphName + "][" + dto.Identifier + "] data.");
        response.Add(exception.ToString());
      }
      finally
      {
        UninitializeApplication();
      }
      return response;
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

        targetUri = request["targetUri"];
        targetCredentialsXML = request["targetCredentials"];
        graphName = request["graphName"];
        filter = request["filter"];

        WebCredentials targetCredentials = Utility.Deserialize<WebCredentials>(targetCredentialsXML, true);
        if (targetCredentials.isEncrypted) targetCredentials.Decrypt();

        //SPARQLEngine engine = new SPARQLEngine(_mapping, targetUri, targetCredentials, _proxyCredentials, _trimData);

        DateTime b = DateTime.Now;
        DateTime e;
        TimeSpan d;
        SPARQLEngine sparqlEngine = (SPARQLEngine)_semanticEngine;
        List<DataTransferObject> dtoList = sparqlEngine.PullQuery(graphName);

        RuleEngine ruleEngine = new RuleEngine();
        if (File.Exists(_settings.XmlPath + "Pull" + graphName + ".rules"))
        {
          dtoList = ruleEngine.RuleSetForCollection(dtoList, _settings.XmlPath + "Pull" + graphName + ".rules");
        }

        e = DateTime.Now;
        d = e.Subtract(b);
        response.Add(String.Format("PullQuery[{0}] Execution Time [{1}:{2}.{3}] Seconds ", graphName, d.Minutes, d.Seconds, d.Milliseconds));
        b = e;

        response.Append(_dtoService.PostList(graphName, dtoList));

        e = DateTime.Now;
        d = e.Subtract(b);
        response.Add(String.Format("Pull[{0},{1}] Execution Time [{2}:{3}.{4}] Seconds ", targetUri, graphName, d.Minutes, d.Seconds, d.Milliseconds));
      }
      catch (Exception exception)
      {
        response.Add("Error while pulling " + graphName + " data from " + targetUri + " as " + targetUri + " data with filter " + filter + ".\r\n");
        response.Add(exception.ToString());
      }
      finally
      {
        UninitializeApplication();
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
        response.Add("Error while pulling " + graphName + " data from " + targetUri + " as " + targetUri + " data with filter " + filter + ".\r\n");
        response.Add(exception.ToString());
      }
      finally
      {
        UninitializeApplication();
      }
      return response;
    }

    public Response ClearStore(string projectName, string applicationName)
    {
      Response response = new Response();
      try
      {
        InitializeApplication(projectName, applicationName);

        if (_settings.UseSemweb)
        {
          _semanticEngine.ClearStore();
          response.Add("Store cleared successfully.");
        }
        else
        {
          _semanticEngine.ClearStore();
          response.Add("Store cleared successfully.");
        }
      }
      catch (Exception exception)
      {
        response.Add("Error while clearing TripleStore.");
        response.Add(exception.ToString());
      }
      finally
      {
        UninitializeApplication();
      }
      return response;
    }

    /// <summary>
    /// Generating code to a temporary file. If successful, update old code with the new generated content.
    /// </summary>
    private string TransformText(string templateFileName, string outputFileName)
    {
      CustomTextTemplateHost host = null;
      Engine engine = null;

      try
      {
        host = new CustomTextTemplateHost();
        engine = new Engine();

        string input = File.ReadAllText(templateFileName);
        host.TemplateFileValue = templateFileName;
        string output = engine.ProcessTemplate(input, host);

        File.WriteAllText(outputFileName, output, host.FileEncoding);

        if (host.Errors.HasErrors)
        {
          string errors = string.Empty;

          foreach (CompilerError error in host.Errors)
          {
            errors += error.ToString();
          }

          throw new Exception(errors);
        }

        return output;
      }
      catch (Exception ex)
      {
        throw ex;
      }
      finally
      {
        engine = null;
        host = null;
      }
    }

    /// <summary>
    /// Generated DTO Model and Service.
    /// </summary>
    /// <returns>Returns the response as success/failure.</returns>
    public Response Generate(string projectName, string applicationName)
    {
      Response response = new Response();

      try
      {
        DTOGenerator dtoGenerator = new DTOGenerator();
        dtoGenerator.Generate(projectName, applicationName);

        UpdateBindingConfiguration(projectName, applicationName);

        response.Add("DTO Model generated successfully.");
      }
      catch (Exception ex)
      {
        response.Add("Error generating DTO Model.");
        response.Add(ex.ToString());
      }

      return response;
    }

    /// <summary>
    /// Update NInject binding configuration file.
    /// </summary>
    /// <returns></returns>
    private void UpdateBindingConfiguration(string projectName, string applicationName)
    {
      string bindingConfigurationPath = _settings.XmlPath + "BindingConfiguration." + projectName + "." + applicationName + ".xml";
      string dtoServiceBindingName = "DTOService." + projectName + "." + applicationName;
      Binding dtoServiceBinding = new Binding()
      {
        Name = dtoServiceBindingName,
        Interface = "org.iringtools.adapter.IDTOService, AdapterLibrary",
        Implementation = "org.iringtools.adapter.proj_" + projectName + "." + applicationName + ".DTOService, AdapterService"
      };

      if (File.Exists(bindingConfigurationPath))
      {
        BindingConfiguration bindingConfiguration = Utility.Read<BindingConfiguration>(bindingConfigurationPath, false);
        bool bindingExists = false;

        foreach (Binding binding in bindingConfiguration.Bindings)
        {
          if (binding.Name == dtoServiceBindingName)
          {
            bindingExists = true;
            break;
          }
        }

        // DTOService binding does not exist, add it to binding configuration
        if (!bindingExists)
        {
          bindingConfiguration.Bindings.Add(dtoServiceBinding);
          Utility.Write<BindingConfiguration>(bindingConfiguration, bindingConfigurationPath, false);
        }
      }
      else
      {
        BindingConfiguration bindingConfiguration = new BindingConfiguration();
        bindingConfiguration.Bindings = new List<Binding>();
        bindingConfiguration.Bindings.Add(dtoServiceBinding);
        Utility.Write<BindingConfiguration>(bindingConfiguration, bindingConfigurationPath, false);
      }
    }

    /// <summary>
    /// Generated DTO Model and Service using T4.
    /// </summary>
    /// <returns>Returns the response as success/failure.</returns>
    public Response GenerateOLD(string projectName, string applicationName)
    {
      Response response = new Response();

      string currentDirectory = Directory.GetCurrentDirectory();
      string appCodeDirectory = currentDirectory + "\\App_Code";
      string suffix = "." + projectName + "." + applicationName;

      string dtoModelName = "DTOModel" + suffix;
      string dtoModelTemplateFileName = currentDirectory + "\\Templates\\DTOModel.tt";
      string dtoModelFileNameTmp = appCodeDirectory + "\\" + dtoModelName + ".tmp";
      string dtoModelFileName = appCodeDirectory + "\\" + dtoModelName + ".cs";

      string dtoServiceName = "DTOService" + suffix;
      string dtoServiceTemplateFileName = currentDirectory + "\\Templates\\DTOService.tt";
      string dtoServiceFileNameTmp = appCodeDirectory + "\\" + dtoServiceName + ".tmp";
      string dtoServiceFileName = appCodeDirectory + "\\" + dtoServiceName + ".cs";

      string iServiceTemplateFileName = currentDirectory + "\\Templates\\IService.tt";
      string iServiceFileNameTmp = appCodeDirectory + "\\IService.Generated.tmp";
      string iServiceFileName = appCodeDirectory + "\\IService.Generated.cs";

      string iDataServiceTemplateFileName = currentDirectory + "\\Templates\\IDataService.tt";
      string iDataServiceFileNameTmp = appCodeDirectory + "\\IDataService.Generated.tmp";
      string iDataServiceFileName = appCodeDirectory + "\\IDataService.Generated.cs";

      System.Environment.SetEnvironmentVariable("projectName", projectName);
      System.Environment.SetEnvironmentVariable("applicationName", applicationName);

      try
      {
        // Generate DTOModel.cs
        string dtoModelContent = TransformText(dtoModelTemplateFileName, dtoModelFileNameTmp);

        // Generate DTOService.cs
        string dtoServiceContent = TransformText(dtoServiceTemplateFileName, dtoServiceFileNameTmp);

        // Refresh iService.cs
        string iServiceContent = TransformText(iServiceTemplateFileName, iServiceFileNameTmp);

        // Refresh iDataService.cs
        string iDataServiceContent = TransformText(iDataServiceTemplateFileName, iDataServiceFileNameTmp);

        // Write generated C# code to disk
        File.WriteAllText(dtoModelFileName, dtoModelContent);
        File.WriteAllText(dtoServiceFileName, dtoServiceContent);
        File.WriteAllText(iServiceFileName, iServiceContent);
        File.WriteAllText(iDataServiceFileName, iDataServiceContent);

        UpdateBindingConfiguration(projectName, applicationName);

        response.Add("DTO Model generated successfully.");
      }
      catch (Exception exception)
      {
        response.Add("Error generating DTO Model.");
        response.Add(exception.ToString());
      }
      finally
      {
        File.Delete(dtoModelFileNameTmp);
        File.Delete(dtoServiceFileNameTmp);
        File.Delete(iServiceFileNameTmp);
        File.Delete(iDataServiceFileNameTmp);
      }

      return response;
    }

    /// <summary>
    /// Generate entities, hibernate-mapping, hibernate-configuration, and data dictionary for the application
    /// </summary>
    /// <returns>Returns the response as success/failure.</returns>
    public Response UpdateDatabaseDictionary(DatabaseDictionary dbDictionary, string projectName, string applicationName)
    {
      Response response = new Response();

      if (String.IsNullOrEmpty(projectName) || String.IsNullOrEmpty(applicationName))
      {
        response.Add("Error project name and application name can not be null");
      }
      else
      {
        EntityGenerator generator = _kernel.Get<EntityGenerator>();

        #region Update binding configuration
        string bindingConfigurationPath = _settings.XmlPath + "BindingConfiguration." + projectName + "." + applicationName + ".xml";
        string dataLayerBindingName = "DataLayer." + projectName + "." + applicationName;
        Binding dataLayerBinding = new Binding()
        {
          Name = dataLayerBindingName,
          Interface = "org.iringtools.library.IDataLayer, iRINGLibrary",
          Implementation = "org.iringtools.adapter.dataLayer.NHibernateDataLayer, NHibernateDataLayer"
        };

        if (File.Exists(bindingConfigurationPath))
        {
          BindingConfiguration bindingConfiguration = Utility.Read<BindingConfiguration>(bindingConfigurationPath, false);
          bool bindingExists = false;

          foreach (Binding binding in bindingConfiguration.Bindings)
          {
            if (binding.Name == dataLayerBindingName)
            {
              bindingExists = true;
              break;
            }
          }

          // DataLayer binding does not exist, add it to binding configuration
          if (!bindingExists)
          {
            bindingConfiguration.Bindings.Add(dataLayerBinding);
            Utility.Write<BindingConfiguration>(bindingConfiguration, bindingConfigurationPath, false);
          }
        }
        else
        {
          BindingConfiguration bindingConfiguration = new BindingConfiguration();

          bindingConfiguration.Bindings = new List<Binding>();
          bindingConfiguration.Bindings.Add(dataLayerBinding);
          Utility.Write<BindingConfiguration>(bindingConfiguration, bindingConfigurationPath, false);
        }
        #endregion

        response = generator.Generate(dbDictionary, projectName, applicationName);
      }

      return response;
    }
  }
}