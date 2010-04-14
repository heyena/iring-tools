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
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.CodeDom.Compiler;
using System.Text;
using Microsoft.VisualStudio.TextTemplating;
using org.iringtools.adapter.rules;
using org.iringtools.adapter.semantic;
using org.iringtools.utility;
using org.ids_adi.qxf;
using org.iringtools.library;

namespace org.iringtools.adapter
{
  public partial class AdapterService : IAdapterService
    {
        private AdapterServiceProvider _adapterServiceProvider = null;


        /// <summary>
        /// Adapter Service Constructor
        /// </summary>
        public AdapterService()
        {
          ConfigSettings configSettings = new ConfigSettings();
          configSettings.BaseDirectoryPath = System.AppDomain.CurrentDomain.BaseDirectory;
          configSettings.XmlPath = System.Configuration.ConfigurationManager.AppSettings["XmlPath"];
          configSettings.TripleStoreConnectionString = System.Configuration.ConfigurationManager.AppSettings["TripleStoreConnectionString"];
          configSettings.ModelDTOPath = System.Configuration.ConfigurationManager.AppSettings["ModelDTOPath"];
          configSettings.IDataServicePath = System.Configuration.ConfigurationManager.AppSettings["IDataServicePath"];
          configSettings.InterfaceServer = System.Configuration.ConfigurationManager.AppSettings["InterfaceService"];
          configSettings.TrimData = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["TrimData"]);
          configSettings.UseSemweb = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["UseSemweb"]);
          configSettings.EncryptedToken = System.Configuration.ConfigurationManager.AppSettings["TargetCredentialToken"];
          configSettings.EncryptedProxyToken = System.Configuration.ConfigurationManager.AppSettings["ProxyCredentialToken"];
          configSettings.ProxyHost = System.Configuration.ConfigurationManager.AppSettings["ProxyHost"];
          configSettings.ProxyPort = System.Configuration.ConfigurationManager.AppSettings["ProxyPort"];
          configSettings.TransformPath = System.Configuration.ConfigurationManager.AppSettings["TransformPath"];
          configSettings.DataLayerConfigPath = System.Configuration.ConfigurationManager.AppSettings["DataLayerConfigPath"];
         _adapterServiceProvider = new AdapterServiceProvider(configSettings);           
        }

        /// <summary>
        /// Gets the mapping by reading Mapping.xml.
        /// </summary>
        /// <returns>Returns mapping object.</returns>
        public Mapping GetMapping()
        {
          return _adapterServiceProvider.GetMapping();
        }

        /// <summary>
        /// Gets the Data Dictionary by reading DataDictionary.xml
        /// </summary>
        /// <returns>Returns Data Dictionary object.</returns>
        public DataDictionary GetDictionary()
        {
          return _adapterServiceProvider.GetDictionary();
        }

        /// <summary>
        /// Updates mapping.
        /// </summary>
        /// <param name="mapping">The new mapping object with which the mapping file is to be updated.</param>
        /// <returns>Returns the response as success/failure.</returns>
        public Response UpdateMapping(Mapping mapping)
        {
          return _adapterServiceProvider.UpdateMapping(mapping);
        }

        /// <summary>
        /// Refreshes Data Dictionary by generating a new DataDictionary.xml from csdl.
        /// </summary>
        /// <returns>Returns the response as success/failure.</returns>
        public Response RefreshDictionary()
        {
          return _adapterServiceProvider.RefreshDictionary();
        }

        /// <summary>
        /// Gets the data for a graphname and identifier in a QXF format.
        /// </summary>
        /// <param name="graphName">The name of graph for which data is to be fetched.</param>
        /// <param name="identifier">The unique identifier used as filter to return single row's data.</param>
        /// <returns>Returns the data in QXF format.</returns>
        public Envelope Get(string graphName, string identifier)
        {
          return _adapterServiceProvider.Get(graphName, identifier);
        }

        /// <summary>
        /// Gets all the data for the graphname.
        /// </summary>
        /// <param name="graphName">The name of graph for which data is to be fetched.</param>
        /// <returns>Returns the data in QXF format.</returns>
        public Envelope GetList(string graphName)
        {
          return _adapterServiceProvider.GetList(graphName);
        }

        /// <summary>
        /// Refreshes the triple store for all the graphmaps.
        /// </summary>
        /// <returns>Returns the response as success/failure.</returns>
        public Response RefreshAll()
        {
          return _adapterServiceProvider.RefreshAll();
        }

        /// <summary>
        /// Refreshes the triple store for the graphmap passed.
        /// </summary>
        /// <param name="graphName">The name of graph for which triple store will be refreshed.</param>
        /// <returns>Returns the response as success/failure.</returns>
        public Response RefreshGraph(string graphName)
        {
          return _adapterServiceProvider.RefreshGraph(graphName);
        }

        /// <summary>
        /// Pulls the data from a triple store into the database.
        /// </summary>
        /// <param name="request">The request parameter containing targetUri, targetCredentials, graphName, filter will be passed.</param>
        /// <returns>Returns the response as success/failure.</returns>
        public Response Pull(Request request)
        {
          return _adapterServiceProvider.Pull(request);
        }        

        /// <summary>
        /// Generated the IAdapterService (partial) IDataService and ModelDTO using T4.
        /// </summary>
        /// <returns>Returns the response as success/failure.</returns>
        public Response Generate()
        {
          return _adapterServiceProvider.Generate();
        }

        /// <summary>
      /// Clears the triple store.
      /// </summary>
        /// <returns>Returns the response as success/failure.</returns>
        public Response ClearStore()
        {
          return _adapterServiceProvider.ClearStore();
        }

    }
}