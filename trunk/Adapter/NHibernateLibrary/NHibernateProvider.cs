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
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using log4net;
using Ninject;
using org.ids_adi.qmxf;
using org.iringtools.library;
using org.iringtools.utility;
using NHibernate;
using org.iringtools.adapter.datalayer;

namespace org.iringtools.nhibernate
{
  public class NHibernateProvider
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(NHibernateProvider));

    private IKernel _kernel = null;
    private NHibernateSettings _settings = null;

    //Projection specific stuff
    private IList<IDataObject> _dataObjects = new List<IDataObject>(); // dictionary of object names and list of data objects
    private Dictionary<string, List<string>> _classIdentifiers = new Dictionary<string, List<string>>(); // dictionary of class ids and list of identifiers

    [Inject]
    public NHibernateProvider(NameValueCollection settings)
    {
      _kernel = new StandardKernel(); //new NHibernateModule());
      _settings = _kernel.Get<NHibernateSettings>();
      _settings.AppendSettings(settings);

      Directory.SetCurrentDirectory(_settings["BaseDirectoryPath"]);
    }

    #region public methods
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
          //Binding dataLayerBinding = new Binding()
          //{
          //  Name = "DataLayer",
          //  Interface = "org.iringtools.library.IDataLayer, iRINGLibrary",
          //  Implementation = "org.iringtools.adapter.datalayer.NHibernateDataLayer, NHibernateDataLayer"
          //};
          //UpdateBindingConfiguration(projectName, applicationName, dataLayerBinding);

          //Binding semanticLayerBinding = new Binding()
          //{
          //  Name = "SemanticLayer",
          //  Interface = "org.iringtools.adapter.ISemanticLayer, AdapterLibrary",
          //  Implementation = "org.iringtools.adapter.semantic.dotNetRdfEngine, AdapterLibrary"
          //};
          //UpdateBindingConfiguration(projectName, applicationName, semanticLayerBinding);

          //Binding projectionLayerBinding = new Binding()
          //{
          //  Name = "ProjectionLayer",
          //  Interface = "org.iringtools.adapter.IProjectionLayer, AdapterLibrary",
          //  Implementation = "org.iringtools.adapter.projection.RdfProjectionEngine, AdapterLibrary"
          //};
          //UpdateBindingConfiguration(projectName, applicationName, projectionLayerBinding);

          //UpdateScopes(projectName, applicationName);

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
    private void RemoveDups(DataObject dataObject)
    {
      try
      {
        /* GvR
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
        } */
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
          throw new Exception(string.Format("Table \"{0}\" has no key.", dataObject.tableName));
        }
      }

      return true;
    }
    #endregion
  }
}