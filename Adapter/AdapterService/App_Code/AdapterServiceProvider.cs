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
    public class AdapterServiceProvider : IAdapterService
    {
      
        WebCredentials _targetCredentials = null;
        WebProxyCredentials _proxyCredentials = null;

        private Mapping _mapping = null;
        private string _xmlPath = String.Empty;
        private string _mappingPath = String.Empty;
        private string _dtoToQXFPath = String.Empty;
        private string _qxfToRDFPath = String.Empty;
        private string _dataDictionaryPath = String.Empty;
        private string _interfaceServer = String.Empty;
        private string _modelDTOPath = String.Empty;
        private string _iDataServicePath = String.Empty;
        private bool _trimData = false;
        private bool _useSemweb = false;
        private string _transFormPath = string.Empty;
        private SPARQLEngine _sparqlEngine = null;
        private SemWebEngine _semwebEngine = null;
        private string _tripleStoreConnectionString = String.Empty;

        #region Constants
        private const string MAPPING_FILENAME = "Mapping.xml";
        private const string DTO_TO_QXF_FILENAME = "DTOToQXF.xsl";
        private const string QXF_TO_RDF_FILENAME = "QXFToRDF.xsl";
        private const string DATA_DICTIONARY_FILENAME = "DataDictionary.xml";
        private const string MAPPING_TO_DTO_CLASSES_FILENAME = "MappingToDTOClasses.xsl";
        private const string MAPPING_TO_IDATASERVICE_FILENAME = "MappingToIDataService.xsl";
        #endregion

        /// <summary>
        /// Adapter Service Constructor
        /// </summary>
        public AdapterServiceProvider(ConfigSettings configSettings)
        {
            Directory.SetCurrentDirectory(configSettings.BaseDirectoryPath);
            DTOFactory.configSettings = configSettings;

            _xmlPath = configSettings.XmlPath;
            if (string.IsNullOrEmpty(_xmlPath))
            {
              _xmlPath = ".\\XML\\";
              configSettings.XmlPath = _xmlPath;
            }
            _mappingPath = _xmlPath + MAPPING_FILENAME;
            _dataDictionaryPath = _xmlPath + DATA_DICTIONARY_FILENAME;

            _transFormPath = configSettings.TransformPath;
            if (string.IsNullOrEmpty(_transFormPath))
            {
                _transFormPath = ".\\Transforms\\";
                configSettings.TransformPath = _transFormPath;
            }
            _dtoToQXFPath = _transFormPath + DTO_TO_QXF_FILENAME;
            _qxfToRDFPath = _transFormPath + QXF_TO_RDF_FILENAME;
           

            _modelDTOPath = configSettings.ModelDTOPath;
            if (string.IsNullOrEmpty(_modelDTOPath))
            {
              _modelDTOPath = ".\\App_Code\\ModelDTO.cs";
              configSettings.ModelDTOPath = _modelDTOPath;
            }

            _iDataServicePath = configSettings.IDataServicePath;
            if (string.IsNullOrEmpty(_iDataServicePath))
            {
              _iDataServicePath = ".\\App_Code\\IDataService.cs";
              configSettings.IDataServicePath = _iDataServicePath;
            }

            _interfaceServer = configSettings.InterfaceServer;
            if (string.IsNullOrEmpty(_interfaceServer))
            {
              _interfaceServer = "http://localhost:2222/iring";
              configSettings.InterfaceServer = _interfaceServer;
            }

            _trimData = configSettings.TrimData;
            _useSemweb = configSettings.UseSemweb;
            _tripleStoreConnectionString = configSettings.TripleStoreConnectionString;

            string encryptedToken = configSettings.EncryptedToken;
            string encryptedProxyToken = configSettings.EncryptedProxyToken;
            string proxyHost = configSettings.ProxyHost;
            string proxyPortString = configSettings.ProxyPort;

            if (encryptedToken == String.Empty)
            {
                _targetCredentials = new WebCredentials();
            }
            else
            {
                _targetCredentials = new WebCredentials(encryptedToken);
                _targetCredentials.Decrypt();
            }
            
            int proxyPort = 0;
            Int32.TryParse(proxyPortString, out proxyPort);
            if (string.IsNullOrEmpty(encryptedProxyToken))
            {
                _proxyCredentials = new WebProxyCredentials();
            }
            else
            {
                _proxyCredentials = new WebProxyCredentials(encryptedProxyToken, proxyHost, proxyPort);
                _proxyCredentials.Decrypt();
            }

            _mapping = GetMapping();

            if (!_useSemweb)
              _sparqlEngine = new SPARQLEngine(_mapping, _interfaceServer, _targetCredentials, _proxyCredentials, _trimData);
            else
              _semwebEngine = new SemWebEngine(_tripleStoreConnectionString, _mapping, _trimData);
        }

        /// <summary>
        /// Gets the mapping by reading Mapping.xml.
        /// </summary>
        /// <returns>Returns mapping object.</returns>
        public Mapping GetMapping()
        {
            try
            {
                if (_mapping == null)
                {
                    _mapping = Utility.Read<Mapping>(_mappingPath, false);
                }

                return _mapping;

            }
            catch (Exception exception)
            {
                throw new Exception("Error while getting Mapping from " + _mappingPath + ". " + exception.ToString(), exception);
            }
        }

        /// <summary>
        /// Gets the Data Dictionary by reading DataDictionary.xml
        /// </summary>
        /// <returns>Returns Data Dictionary object.</returns>
        public DataDictionary GetDictionary()
        {
            try
            {
                return DTOFactory.GetDictionary();
            }
            catch (Exception exception)
            {
                throw new Exception("Error while getting Dictionary. " + exception.ToString(), exception);
            }
        }

        /// <summary>
        /// Updates mapping.
        /// </summary>
        /// <param name="mapping">The new mapping object with which the mapping file is to be updated.</param>
        /// <returns>Returns the response as success/failure.</returns>
        public Response UpdateMapping(Mapping mapping)
        {
            Response response = new Response();

            try
            {
                Utility.Write<Mapping>(mapping, _mappingPath, false);
                response.Add("Mapping file updated successfully");
            }
            catch (Exception exception)
            {
                response.Add("Error while updating Mapping.");
                response.Add(exception.ToString());
            }

            return response;
        }

        /// <summary>
        /// Refreshes Data Dictionary by generating a new DataDictionary.xml from csdl.
        /// </summary>
        /// <returns>Returns the response as success/failure.</returns>
        public Response RefreshDictionary()
        {
            Response response = new Response();
            try
            {
                response = DTOFactory.RefreshDictionary();
            }
            catch (Exception exception)
            {
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
        public Envelope Get(string graphName, string identifier)
        {
            try
            {
              Envelope envelope = new Envelope();

              DataTransferObject dto = DTOFactory.GetDTO(graphName, identifier);

                //if (dto != null)
                //{
                //    QXF qxf = dto.Transform<QXF>(_xmlPath, _dtoToQXFPath, _mappingPath, false);
                //    Utility.Write<QXF>(qxf, _xmlPath + identifier + ".QXF.xml");
                //    Stream rdf = Utility.Transform<QXF>(qxf, _qxfToRDFPath, false);
                //    Utility.WriteStream(rdf, _xmlPath + identifier + ".RDF.xml");
                //    return qxf;
                //}

                //Utility.Write<CommonDTO>(dto, _xmlPath + identifier + ".Payload.xml", false);

              envelope.Payload.Add(dto);
              
              return envelope;
            }
            catch (Exception exception)
            {
                throw new Exception("Error while getting " + graphName + " data with identifier " + identifier + ". " + exception.ToString(), exception);
            }
        }

        /// <summary>
        /// Gets all the data for the graphname.
        /// </summary>
        /// <param name="graphName">The name of graph for which data is to be fetched.</param>
        /// <returns>Returns the data in QXF format.</returns>
        public Envelope GetList(string graphName)
        {
            try
            {
              Envelope envelope = new Envelope();
              
                List<DataTransferObject> dtoList = DTOFactory.GetList(graphName);

                //if (dtoList.Count > 0)
                //{
                //    QXF qxf = DTOFactory.TransformList<QXF>(graphName, dtoList, _xmlPath, _dtoToQXFPath, _mappingPath, false);
                //    Utility.Write<QXF>(qxf, _xmlPath + graphName + ".QXF.xml", false);
                //    Stream rdf = Utility.Transform<QXF>(qxf, _qxfToRDFPath, false);
                //    Utility.WriteStream(rdf, _xmlPath + graphName + ".RDF.xml");
                //    return qxf;
                //}

                envelope.Payload = dtoList;

                return envelope;
            }
            catch (Exception exception)
            {
                throw new Exception("Error while getting " + graphName + " data. " + exception.ToString(), exception);
            }
        }

        /// <summary>
        /// Refreshes the triple store for all the graphmaps.
        /// </summary>
        /// <returns>Returns the response as success/failure.</returns>
        public Response RefreshAll()
        {
            Response response = new Response();
            try
            {
                DateTime b = DateTime.Now;

                foreach (GraphMap graphMap in _mapping.graphMaps)
                {
                  response.Append(RefreshGraph(graphMap.name));
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
        public Response RefreshGraph(string graphName)
        {
            Response response = new Response();
            try
            {
                DateTime b = DateTime.Now;

                List<DataTransferObject> commonDTOList = DTOFactory.GetList(graphName);

                if (!_useSemweb)
                {
                  List<string> tripleStoreIdentifiers = _sparqlEngine.GetIdentifiersFromTripleStore(graphName);
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
                    _sparqlEngine.RefreshDelete(graphName, identifier);
                  }
                }
                else
                {  
                  List<string> tripleStoreIdentifiers = _semwebEngine.GetIdentifiersFromTripleStore(graphName);
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
                    _semwebEngine.RefreshDelete(graphName, identifier);
                  }
                }                

                RuleEngine ruleEngine = new RuleEngine();
                if (File.Exists(_xmlPath + "Refresh" + graphName + ".rules"))
                {
                    commonDTOList = ruleEngine.RuleSetForCollection(commonDTOList, _xmlPath + "Refresh" + graphName + ".rules");
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

                if (_useSemweb)
                  _semwebEngine.DumpStoreData(_xmlPath);
            }
            catch (Exception exception)
            {
                response.Add("Error while Refreshing TripleStore for GraphMap[" + graphName + "].");
                response.Add(exception.ToString());
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

                if (!_useSemweb)
                {
                  _sparqlEngine.RefreshQuery(dto);
                }
                else
                {
                  _semwebEngine.RefreshQuery(dto);
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
            return response;
        }

        /// <summary>
        /// Pulls the data from a triple store into the database.
        /// </summary>
        /// <param name="request">The request parameter containing targetUri, targetCredentials, graphName, filter will be passed.</param>
        /// <returns>Returns the response as success/failure.</returns>
        public Response Pull(Request request)
        {
            string targetUri = String.Empty;
            string targetCredentialsXML = String.Empty;
            string targetGraph = String.Empty;
            string graphName = String.Empty;
            string filter = String.Empty;
            Response response = new Response();
            try
            {
                targetUri = request["targetUri"];
                targetCredentialsXML = request["targetCredentials"];
                graphName = request["graphName"];
                filter = request["filter"];

                WebCredentials targetCredentials = Utility.Deserialize<WebCredentials>(targetCredentialsXML, true);
                if (targetCredentials.isEncrypted) targetCredentials.Decrypt();

                SPARQLEngine engine = new SPARQLEngine(_mapping, targetUri, targetCredentials, _proxyCredentials, _trimData);

                DateTime b = DateTime.Now;
                DateTime e;
                TimeSpan d;

                List<DataTransferObject> dtoList = engine.PullQuery(graphName);

                RuleEngine ruleEngine = new RuleEngine();
                if (File.Exists(_xmlPath + "Pull" + graphName + ".rules"))
                {
                    dtoList = ruleEngine.RuleSetForCollection(dtoList, _xmlPath + "Pull" + graphName + ".rules");
                }

                e = DateTime.Now;
                d = e.Subtract(b);
                response.Add(String.Format("PullQuery[{0}] Execution Time [{1}:{2}.{3}] Seconds ", graphName, d.Minutes, d.Seconds, d.Milliseconds));
                b = e;

                response.Append(DTOFactory.PostList(graphName, dtoList));

                e = DateTime.Now;
                d = e.Subtract(b);
                response.Add(String.Format("Pull[{0},{1}] Execution Time [{2}:{3}.{4}] Seconds ", targetUri, graphName, d.Minutes, d.Seconds, d.Milliseconds));
            }
            catch (Exception exception)
            {
                response.Add("Error while pulling " + graphName + " data from " + targetUri + " as " + targetUri + " data with filter " + filter + ".\r\n");
                response.Add(exception.ToString());
            }
            return response;
        }

        public Response ClearStore()
        {
          Response response = new Response();
          try
          {
            if (_useSemweb)
            {
              _semwebEngine.ClearStore();
              response.Add("Store cleared successfully.");
            }
            else
            {
              _sparqlEngine.ClearUpdateQuery();
              response.Add("Store cleared successfully.");
            }            
          }
          catch (Exception exception)
          {
            response.Add("Error while clearing TripleStore.");
            response.Add(exception.ToString());
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
        /// Generated the IAdapterService (partial) IDataService and ModelDTO using T4.
        /// </summary>
        /// <returns>Returns the response as success/failure.</returns>
        public Response Generate()
        {
          Response response = new Response();

          try
          {
            string outputDirectory = Directory.GetCurrentDirectory() + @"..\App_Code";

            // Generate IAdapterService.DTO.cs from IAdapterService.DTO.tt
            string iDataServiceTemplateFileName = Directory.GetCurrentDirectory() + @"\Templates\IDataService.DTO.tt";
            string tempIDataServiceFileName = Directory.GetCurrentDirectory() + @"\App_Code\IDataService.DTO.tmp";
            string iDataServiceFileName = Directory.GetCurrentDirectory() + @"\App_Code\IDataService.DTO.cs";
            string iDataService = TransformText(iDataServiceTemplateFileName, tempIDataServiceFileName);

            // Generate IDataService.cs from IDataService.tt
            string iAdapterServiceTemplateFileName = Directory.GetCurrentDirectory() + @"\Templates\IAdapterService.DTO.tt";
            string tempIAdapterServiceFileName = Directory.GetCurrentDirectory() + @"\App_Code\IAdapterService.DTO.tmp";
            string iAdapterServiceFileName = Directory.GetCurrentDirectory() + @"\App_Code\IAdapterService.DTO.cs";
            string iAdapterService = TransformText(iAdapterServiceTemplateFileName, tempIAdapterServiceFileName);

            // Generate ModelDTO.cs from ModelDTO.tt
            string modelDTOTemplateFileName = Directory.GetCurrentDirectory() + @"\Templates\ModelDTO.tt";
            string tempModelDTOFileName = Directory.GetCurrentDirectory() + @"\App_Code\ModelDTO.tmp";
            string modelDTOFileName = Directory.GetCurrentDirectory() + @"\App_Code\ModelDTO.cs";
            string modelDTO = TransformText(modelDTOTemplateFileName, tempModelDTOFileName);

            // Write .tmp files .cs files
            File.WriteAllText(iDataServiceFileName, iDataService, Encoding.UTF8);
            File.Delete(tempIDataServiceFileName);

            File.WriteAllText(iAdapterServiceFileName, iAdapterService, Encoding.UTF8);
            File.Delete(tempIAdapterServiceFileName);

            File.WriteAllText(modelDTOFileName, modelDTO, Encoding.UTF8);
            File.Delete(tempModelDTOFileName);

            response.Add("IAdapterService.DTO.cs, IDataService.DTO.cs and ModelDTO.cs generated successfully");
          }
          catch (Exception exception)
          {
            response.Add("Error while generating IAdapterService.DTO.cs, IDataService.cs or ModelDTO.cs.");
            response.Add(exception.ToString());
          }

          return response;
        }
    }
}