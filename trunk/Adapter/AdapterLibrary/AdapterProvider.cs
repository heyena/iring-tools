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
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL + exEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Ninject;
using org.iringtools.adapter.datalayer;
using org.iringtools.library;
using org.iringtools.utility;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using System.Collections.Specialized;
using Ninject.Parameters;
using System.IO;
using log4net;
using Ninject.Contrib.Dynamic;
using NHibernate;

namespace org.iringtools.adapter
{
  public class AdapterProvider
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterProvider));

    private IKernel _kernel = null;
    private AdapterSettings _settings = null;
    private IDataLayer _dataLayer = null;
    private ISemanticLayer _semanticEngine = null;
    private IProjectionLayer _projectionEngine = null;
    private Mapping _mapping = null;
    private bool _isInitialized = false;

    [Inject]
    public AdapterProvider(NameValueCollection settings)
    {
      _kernel = new StandardKernel(new AdapterModule());
      _settings = _kernel.Get<AdapterSettings>(new ConstructorArgument("AppSettings", settings)); 
      
      Directory.SetCurrentDirectory(_settings.BaseDirectoryPath);
    }

    #region public methods
    public List<ScopeProject> GetScopes()
    {
      string path = string.Format("{0}Scopes.xml", _settings.XmlPath);

      try
      {
        if (File.Exists(path))
        {
          return Utility.Read<List<ScopeProject>>(path);
        }

        return new List<ScopeProject>();
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetScopes: {0}", ex));
        throw new Exception(string.Format("Error getting the list of projects/applications from path [{0}]: {1}", path, ex));
      }
    }

    public Manifest GetManifest(string projectName, string applicationName)
    {
      string path = string.Format("{0}Mapping.{1}.{2}.xml",_settings.XmlPath, projectName, applicationName);

      try
      {
        Initialize(projectName, applicationName);

        Manifest manifest = new Manifest();
        manifest.Graphs = new List<ManifestGraph>();

        if (File.Exists(path))
        {
          foreach (GraphMap graphMap in _mapping.graphMaps)
          {
            ManifestGraph manifestGraph = new ManifestGraph { GraphName = graphMap.name };
            manifest.Graphs.Add(manifestGraph);
          }
        }

        return manifest;
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetManifest: {0}", ex));
        throw new Exception(string.Format("Error getting manifest from path [{0}: {1}", path, ex));
      }
    }

    public DataDictionary GetDictionary(string projectName, string applicationName)
    {
      try
      {
        Initialize(projectName, applicationName);
        return _dataLayer.GetDictionary();
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetDictionary: {0}", ex));
        throw new Exception(string.Format("Error getting data dictionary: {0}", ex));
      }
    }

    public Mapping GetMapping(string projectName, string applicationName)
    {
      try
      {
        Initialize(projectName, applicationName);
        return _mapping;
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in GetMapping: {0}", ex));
        throw new Exception(string.Format("Error getting mapping: {0}", ex));
      }
    }

    public Response UpdateMapping(string projectName, string applicationName, Mapping mapping)
    {
      Response response = new Response();
      string path = string.Format("{0}Mapping.{1}.{2}.xml", _settings.XmlPath, projectName, applicationName);

      try
      {
        Utility.Write<Mapping>(mapping, path, true);
        response.Add("Mapping file updated successfully.");
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in UpdateMapping: {0}", ex));
        response.Level = StatusLevel.Error;
        response.Add(string.Format("Error saving mapping file to path [{0}]: {1}", path, ex));
      }

      return response;
    }

    public Response RefreshAll(string projectName, string applicationName)
    {
      Response response = new Response();

      try
      {
        Initialize(projectName, applicationName);
        DateTime start = DateTime.Now;
        
        foreach (GraphMap graphMap in _mapping.graphMaps)
        {
          response.Append(Refresh(graphMap.name));
        }

        DateTime end = DateTime.Now;
        TimeSpan duration = end.Subtract(start);

        response.Add(String.Format("RefreshAll() completed in [{0}:{1}.{2}] minutes.",
          duration.Minutes, duration.Seconds, duration.Milliseconds));
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in RefreshAll: {0}", ex));

        response.Level = StatusLevel.Error;
        response.Add(string.Format("Error refreshing all graphs: {0}", ex));
      }

      return response;
    }

    public Response Refresh(string projectName, string applicationName, string graphName)
    {
      Response response = new Response();

      try
      {
        Initialize(projectName, applicationName);
        response.Append(Refresh(graphName));
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in Refresh: {0}", ex));

        response.Level = StatusLevel.Error;
        response.Add(string.Format("Error refreshing graph [{0}]: {1}", graphName, ex));
      }

      return response;
    }

    public XElement GetRdf(string projectName, string applicationName, string graphName)
    {
      try
      {
        Initialize(projectName, applicationName);
        return _projectionEngine.GetRdf(graphName);
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in Refresh: {0}", ex));
        throw ex;
      }
    }

    public List<Dictionary<string, string>> GetDTOList(string projectName, string applicationName, string graphName)
    {
      try
      {
        Initialize(projectName, applicationName);
        return _projectionEngine.GetDTOList(graphName);
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public XElement GetHierarchicalDTOList(string projectName, string applicationName, string graphName)
    {
      try
      {
        Initialize(projectName, applicationName);
        return _projectionEngine.GetHierachicalDTOList(graphName);
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public XElement GetQtxf(string projectName, string applicationName, string graphName)
    {
      try
      {
        Initialize(projectName, applicationName);
        return _projectionEngine.GetQtxf(graphName);
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }    

    public Response Pull(string projectName, string applicationName, string graphName)
    {
      Response response = new Response();

      try
      {
        Initialize(projectName, applicationName);
        Dictionary<string, IList<IDataObject>> dataObjectSet = _semanticEngine.Get(graphName);
        DateTime startTime = DateTime.Now;

        foreach (var pair in dataObjectSet)
          response.Append(_dataLayer.Post(pair.Value));

        DateTime endTime = DateTime.Now;
        TimeSpan duration = endTime.Subtract(startTime);
        
        response.Level = StatusLevel.Success;
        response.Add(string.Format("Graph [{0}] has been posted to legacy system successfully.", graphName));

        response.Add(String.Format("Execution time [{0}:{1}.{2}] minutes.",
          duration.Minutes, duration.Seconds, duration.Milliseconds));
      }
      catch (Exception ex)
      {
        response.Level = StatusLevel.Error;
        response.Add(string.Format("Error pulling graph: {0}", ex));
      }

      return response;
    }

    public Response DeleteAll(string projectName, string applicationName)
    {
      Response response = new Response();

      try
      {
        Initialize(projectName, applicationName);

        foreach (GraphMap graphMap in _mapping.graphMaps)
        {
          response.Append(_semanticEngine.Delete(graphMap.name));
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error clearing all graphs: {0}", ex));

        response.Level = StatusLevel.Error;
        response.Add(string.Format("Error clearing all graphs: {0}", ex));
      }

      return response;
    }

    public Response Delete(string projectName, string applicationName, string graphName)
    {
      Initialize(projectName, applicationName);

      return _semanticEngine.Delete(graphName);
    }
    
    public Response UpdateDatabaseDictionary(string projectName, string applicationName, DatabaseDictionary dbDictionary)
    {
      Response response = new Response();

      try
      {
        if (String.IsNullOrEmpty(projectName) || String.IsNullOrEmpty(applicationName))
        {
          response.Add("Error project name and application name can not be null");
        }
        else if (ValidateDatabaseDictionary(dbDictionary))
        {
          foreach (DataObject dataObject in dbDictionary.dataObjects)
          {
            RemoveDups(dataObject);
          }

          EntityGenerator generator = _kernel.Get<EntityGenerator>();
          response = generator.Generate(dbDictionary, projectName, applicationName);

          // Update binding configuration
          Binding dataLayerBinding = new Binding()
          {
            Name = "DataLayer",
            Interface = "org.iringtools.library.IDataLayer, iRINGLibrary",
            Implementation = "org.iringtools.adapter.datalayer.NHibernateDataLayer, NHibernateDataLayer"
          };
          UpdateBindingConfiguration(projectName, applicationName, dataLayerBinding);

          Binding semanticLayerBinding = new Binding()
          {
            Name = "SemanticLayer",
            Interface = "org.iringtools.adapter.ISemanticLayer, AdapterLibrary",
            Implementation = "org.iringtools.adapter.semantic.dotNetRdfEngine, AdapterLibrary"
          };
          UpdateBindingConfiguration(projectName, applicationName, semanticLayerBinding);

          Binding projectionLayerBinding = new Binding()
          {
            Name = "ProjectionLayer",
            Interface = "org.iringtools.adapter.IProjectionLayer, AdapterLibrary",
            Implementation = "org.iringtools.adapter.projection.DTOProjectionEngine, AdapterLibrary"
          };
          UpdateBindingConfiguration(projectName, applicationName, projectionLayerBinding);

          UpdateScopes(projectName, applicationName);

          response.Add("Database dictionary updated successfully.");
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in UpdateDatabaseDictionary: {0}", ex));

        response.Level = StatusLevel.Error;
        response.Add(string.Format("Error updating database dictionary: {0}", ex));
      }

      return response;
    }
    #endregion

    #region private methods
    private void Initialize(string projectName, string applicationName)
    {
      try
      {
        if (!_isInitialized)
        {
          string scope = string.Format("{0}.{1}",projectName, applicationName);
          
          ApplicationSettings applicationSettings = _kernel.Get<ApplicationSettings>(
            new ConstructorArgument("projectName", projectName),
            new ConstructorArgument("applicationName", applicationName)
          );

          string bindingConfigurationPath = _settings.XmlPath + applicationSettings.BindingConfigurationPath;
          BindingConfiguration bindingConfiguration = Utility.Read<BindingConfiguration>(bindingConfigurationPath, false);

          _kernel.Load(new DynamicModule(bindingConfiguration));
          _dataLayer = _kernel.Get<IDataLayer>("DataLayer");
          _semanticEngine = _kernel.Get<ISemanticLayer>("SemanticLayer");
          _projectionEngine = _kernel.Get<IProjectionLayer>("ProjectionLayer");
          _mapping = Utility.Read<Mapping>(string.Format("{0}Mapping.{1}.xml",_settings.XmlPath, scope));

          _isInitialized = true;
        }
      }
      catch (Exception exception)
      {
        _logger.Error(string.Format("Error initializing application: {0}", exception));
        throw new Exception(string.Format("Error initializing application: {0} {1)", exception.ToString(), exception));
      }
    }

    private Response Refresh(string graphName)
    {      
      XElement rdf = _projectionEngine.GetRdf(graphName);
      return _semanticEngine.Refresh(graphName, rdf);
    }

    private void RemoveDups(DataObject dataObject)
    {
      try
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
      catch (Exception ex)
      {
        throw ex;
      }
    }

    private bool ValidateDatabaseDictionary(DatabaseDictionary dbDictionary)
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
          throw new Exception(string.Format("Table \"{0}\" has no key.", dataObject.tableName ));
        }
      }

      return true;
    }
    
    private void UpdateBindingConfiguration(string projectName, string applicationName, Binding binding)
    {
      try
      {
        string bindingConfigurationPath = string.Format("{0}BindingConfiguration.{1}.{2}.xml", _settings.XmlPath, projectName, applicationName);

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
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in UpdateBindingConfiguration: {0}", ex));
        throw ex;
      }
    }

    private void UpdateScopes(string projectName, string applicationName)
    {
      try
      {
        string scopesPath = string.Format("{0}Scopes.xml", _settings.XmlPath);

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
                  break;
                }
              }

              if (!applicationExists)
              {
                project.Applications.Add(new ScopeApplication() { Name = applicationName });
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
                   Name = applicationName
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
                   Name = applicationName
                 }
               }
            }
          };

          Utility.Write<List<ScopeProject>>(projects, scopesPath, true);
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in UpdateScopes: {0}", ex));
        throw ex;
      }
    }

    #endregion
  }
}
