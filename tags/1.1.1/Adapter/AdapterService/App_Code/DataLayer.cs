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
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.IO;
using System.Linq;
using System.Text;
using org.ids_adi.iring.adapter.dataLayer.Model;
using org.iringtools.utility;
using System.Data.Entity.Design;
using System.Data.Mapping;
using System.Xml.Linq;
using System.Xml;
using org.iringtools.library;

namespace org.iringtools.adapter.dataLayer
{
  /// <remarks>
  /// The DataLayer class implements Get, Post, Update and Delete methods.
  /// This class uses .net entity framework to perform operations.
  /// The methods implemented in this class are generic methods dealing with type T.
  /// </remarks>
  public class DataLayer : IDataLayer
  {   
    /// <summary>
    /// Private members of DataLayer class.
    /// </summary>
    private string _xmlPath = String.Empty;
    private string _transformPath = String.Empty;
    private string _dataLayerConfigPath = String.Empty;
    private string _csdlPath = String.Empty;
    private string _ssdlPath = String.Empty;
    private string _mslPath = String.Empty;
    private string _edmxPath = String.Empty;
    private bool _useEDMX = false;
    private string _dataDictionaryPath = String.Empty;
    private string _basePath = String.Empty;

    #region Constants
    private const string DATA_DICTIONARY_FILENAME = "DataDictionary.xml";
    private const string CSDL_TO_DATA_DICTIONARY_FILENAME = "CSDLtoDataDictionary.xsl";

    private const string csdlNamespace = "http://schemas.microsoft.com/ado/2006/04/edm";
    private const string ssdlNamespace = "http://schemas.microsoft.com/ado/2006/04/edm/ssdl";
    private const string mslNamespace = "urn:schemas-microsoft-com:windows:storage:mapping:CS";

    private const string ssdlContainerNamespace = "org.ids_adi.iring.adapter.dataLayer.Model.Store";  
    #endregion

    private Entities entities = new Entities();
    
    /// <summary>
    /// The constructor of class DataLayer.
    /// This constructor initializes the new object of Entities.
    /// </summary>
    public DataLayer(ConfigSettings configSettings) 
    {
      _basePath = configSettings.BaseDirectoryPath;
      _xmlPath = configSettings.XmlPath;
      _transformPath = configSettings.TransformPath;
      _dataDictionaryPath = _xmlPath + DATA_DICTIONARY_FILENAME;

      _dataLayerConfigPath = configSettings.DataLayerConfigPath;
      if (_dataLayerConfigPath != null || _dataLayerConfigPath != String.Empty)
      {
        _dataLayerConfigPath = ".\\App_Data\\Model.csdl";
        configSettings.DataLayerConfigPath = _dataLayerConfigPath;
      }

      string[] extensionArray = _dataLayerConfigPath.Split('.');
      string[] modelNameArray = _dataLayerConfigPath.Split('\\');
      string extension = extensionArray.Last<string>();
      string modelName = modelNameArray.Last<string>();
      string filePath = _dataLayerConfigPath.Replace(modelName, "");
      if (extension == "csdl")
      {
        modelName = modelName.Replace(".csdl", "");
        _useEDMX = false;        
        _csdlPath = filePath + modelName + ".csdl";
        _ssdlPath = filePath + modelName + ".ssdl";
        _mslPath = filePath + modelName + ".msl";
      }

      if (extension == "edmx")
      {
        modelName = modelName.Replace(".edmx", "");
        _useEDMX = true;        
        _edmxPath = filePath + modelName + ".edmx";
      }
    }

    /// <summary>
    /// The Get method gets data from database using entity framework.
    /// The method gets data for the requested entity.
    /// </summary>
    /// <typeparam name="T">This is the type of data requested from Get method.</typeparam>
    /// <param name="propertyName">The name of the property on which the filter will be applied.</param>
    /// <param name="propertyValue">The value of the property using which the filter will be prepared.</param>
    /// <returns>The method returns the entity with data in it. 
    /// So the data returned is of type T, which is any generic type as per the request.</returns>
    public T Get<T>(Dictionary<string, object> queryProperties)
    {
        ObjectQuery<T> query;
        EntitySetBase baseSet;
        EntityType entityType;
        List<string> masterList;
        List<string> includePaths;
        StringBuilder filter;
        try
        {
            //entities = new Entities();
            filter = new StringBuilder();
            masterList = new List<string>();
            ObjectParameter[] parameters = new ObjectParameter[] { };

            baseSet = GetEntitySetBase<T>();            
            entityType = entities.MetadataWorkspace.GetItem<EntityType>(typeof(T).FullName, DataSpace.CSpace);
            query = entities.CreateQuery<T>(baseSet.ToString(), parameters);            
            includePaths = GetIncludePaths(typeof(T).FullName, typeof(T).Name, masterList, typeof(T).Name);
            foreach (string includePath in includePaths)
            {
                query = query.Include(includePath);
            }  
            foreach (KeyValuePair<string, object> keyValuePair in queryProperties)
            {
               query.Parameters.Add(new ObjectParameter(keyValuePair.Key, keyValuePair.Value));
               query = query.Where("it." + keyValuePair.Key + "=@" + keyValuePair.Key);
               //filter.Append("it." + keyValuePair.Key + "=@" + keyValuePair.Key + " AND ");
            }
            //filter.Remove(filter.Length - 4, 4);
            //query = query.Where(filter.ToString());            
            return query.FirstOrDefault<T>();           
        }
        catch (Exception exception)
        {
            throw new Exception("Error while getting data of type " + typeof(T).Name + ".", exception);
        }
    }

    /// <summary>
    /// Get the complete list of data of a generic type.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <returns>Returns a list of data of a generic type.</returns>
    public IList<T> GetList<T>()
    {
        return GetList<T>(null);
    }

    /// <summary>
    /// The Get method gets data from database using entity framework.
    /// The method gets data for the requested entity.
    /// </summary>
    /// <typeparam name="T">This is the type of data requested from Get method.</typeparam>
    /// <param name="propertyName">The name of the property on which the filter will be applied.</param>
    /// <param name="propertyValue">The value of the property using which the filter will be prepared.</param>
    /// <returns></returns>
    public IList<T> GetList<T>(Dictionary<string, object> queryProperties)
    {
        ObjectQuery<T> query;
        EntitySetBase baseSet;
        EntityType entityType;
        List<string> masterList;
        List<string> includePaths;
        StringBuilder filter;
        try
        {
            //entities = new Entities();
            filter = new StringBuilder();
            masterList = new List<string>();
            ObjectParameter[] parameters = new ObjectParameter[] { };

            baseSet = GetEntitySetBase<T>();
            entityType = entities.MetadataWorkspace.GetItem<EntityType>(typeof(T).FullName, DataSpace.CSpace);
            query = entities.CreateQuery<T>(baseSet.ToString(), parameters);
            includePaths = GetIncludePaths(typeof(T).FullName, typeof(T).Name, masterList, typeof(T).Name);
            foreach (string includePath in includePaths)
            {
                query = query.Include(includePath);
            }
            if (queryProperties != null)
            {
                foreach (KeyValuePair<string, object> keyValuePair in queryProperties)
                {
                    query.Parameters.Add(new ObjectParameter(keyValuePair.Key, keyValuePair.Value));
                    query = query.Where("it." + keyValuePair.Key + "=@" + keyValuePair.Key);
                }
            }
          
            return (IList<T>)query.ToList<T>();
        }
        catch (Exception exception)
        {
            throw new Exception("Error while getting data of type " + typeof(T).Name + ".", exception);
        }
    }

    /// <summary>
    /// The Post method posts data into the database using entity framework.
    /// The method posts the data in the complete hierarchy of the entity that is passed as a graph.
    /// The method also checks that if the record already exists then it updates it else it inserts a new record.
    /// </summary>
    /// <typeparam name="T">This is the type of data that will be posted into the database.</typeparam>
    /// <param name="graph">The graph parameter is the container of the data to be posted. 
    /// The data can be present in hierarchy for the entity.</param>
    /// <returns>The method returns a response. 
    /// The response contains the number of records that have been posted successfully/unsuccessfully.</returns>
    public Response Post<T>(T graph)
    {
        Response response;
        EntitySetBase baseSet;
        bool isAdded;
        try
        {
            //entities = new Entities();
            DateTime b = DateTime.Now;

            isAdded = false;
            response = new Response();
            baseSet = GetEntitySetBase<T>();
            T internalGraph = default(T);
            //need to get Entity Set from MetaData
            EntityKey key = entities.CreateEntityKey(baseSet.Name, graph);

            try
            {
                internalGraph = (T)entities.GetObjectByKey(key);
            }
            catch (ObjectNotFoundException)
            {
                response = Add<T>(graph);
                isAdded = true;
                response.Add("Records of type " + typeof(T).Name + " have been added successfully");
            }

            if (!isAdded)
            {
                graph = Update<T>(graph);
                response.Add("Records of type " + typeof(T).Name + " have been posted successfully");
            }
            DateTime e = DateTime.Now;
            TimeSpan d = e.Subtract(b);

            response.Add(String.Format("Post<{0}> Execution Time [{1}:{2}.{3}] Seconds", typeof(T).Name, d.Minutes, d.Seconds, d.Milliseconds));

            return response;
        }
        catch (Exception exception)
        {
            throw new Exception("Error while posting data of type " + typeof(T).Name + ".", exception);
        }
    }

    /// <summary>
    /// The Post method posts data into the database using entity framework.
    /// The method posts the data in the complete hierarchy of the entity that is passed as a graph.
    /// The method also checks that if the record already exists then it updates it else it inserts a new record.
    /// </summary>
    /// <typeparam name="T">This is the type of data that will be posted into the database.</typeparam>
    /// <param name="graph">The graph parameter is the container of the data to be posted. 
    /// The data can be present in hierarchy for the entity.</param>
    /// <returns>The method returns a response. 
    /// The response contains the number of records that have been posted successfully/unsuccessfully.</returns>
    public Response PostList<T>(List<T> graphList)
    {
      Response response;
      try
      {
        response = new Response();
        
        foreach (T graph in graphList)
        {
          try
          {
            Response responseGraph = Post<T>(graph);
            response.Append(responseGraph);
          }
          catch (Exception exception)
          {
            response.Add(exception.ToString());
          }
        }
        
        return response;
      }
      catch (Exception exception)
      {
        throw new Exception("Error while posting data of type " + typeof(T).Name + ".", exception);
      }
    }

    /// <summary>
    /// The Add method adds data into the database using entity framework.
    /// The method adds the data in the complete hierarchy of the entity that is passed as a graph.
    /// </summary>
    /// <typeparam name="T">This is the type of data that will be added into the database.</typeparam>
    /// <param name="graph">The graph parameter is the container of the data to be posted. 
    /// The data can be present in hierarchy for the entity.</param>
    /// <returns>The method returns a response. 
    /// The response contains the number of records that have been added successfully/unsuccessfully.</returns>
    private Response Add<T>(T graph)
    {
      Response response;
      int recordsAffected;
      EntitySetBase baseSet;
      try
      {
          recordsAffected = 0;
          response = new Response();
          baseSet = GetEntitySetBase<T>();
          //need to get Entity Set from MetaData
          entities.AddObject(baseSet.ToString(), graph);
          recordsAffected = entities.SaveChanges();

          if (recordsAffected > 0)
          {
              response.Add(recordsAffected + " records (" + typeof(T).Name + ") have been posted succesfully");
          }
          else
          {
              response.Add("Records of type " + typeof(T).Name + " have not been posted");
          }
          return response;
      }
      catch (Exception exception)
      {
          throw new Exception("Error while posting data of type " + typeof(T).Name + ".", exception);
      }
    }

    /// <summary>
    /// The Update method updates data into the database using entity framework.
    /// The method updates the data in the complete hierarchy of the entity that is passed as a graph.
    /// If a related entity is not present, then it does not deletes that record. 
    /// it only updates the records that present in the graph and leaves others untouched.
    /// </summary>
    /// <typeparam name="T">This is the type of data that will be updated into the database.</typeparam>
    /// <param name="graph">The graph parameter is the container of the data to be updated. 
    /// The data can be present in hierarchy for the entity.
    /// The related data that is not present in the graph will not be touched.</param>
    /// <returns>The method would return the updated entity.</returns>
    private T Update<T>(T graph)
    {
      EntitySetBase baseSet;
      try
      {         
          baseSet = GetEntitySetBase<T>();
          entities.ApplyPropertyChanges(baseSet.EntityContainer.Name + "." + baseSet.Name, graph);
          entities.SaveChanges();
          return graph;
      }
      catch (Exception exception)
      {
          throw new Exception("Error while updating data of type " + typeof(T).Name + ".", exception);
      }
    }

    /// <summary>
    /// The Delete method deletes the data based on the graph passed as parameter.
    /// If Cascade delete is configured for the entity then the Delete method deletes the entity and its children.
    /// If Cascade delete is not configured and children are present for an entity then it does not delete that entity.
    /// </summary>
    /// <typeparam name="T">This is the type of data that need to be deleted from the database.</typeparam>
    /// <param name="graph">The graph parameter is the container of the data to be deleted. </param>
    /// <returns>The method returns a response. 
    /// The response contains the number of records that have been deleted successfully/unsuccessfully.</returns>
    public Response Delete<T>(T graph)
    {
      Response response;
      int recordsAffected;
      try
      {
          //entities = new Entities();
          recordsAffected = 0;
          response = new Response();
          entities.DeleteObject(graph);  //Will need to get Entity Set from MetaData
          recordsAffected = entities.SaveChanges();
          if (recordsAffected > 0)
          {
              response.Add(recordsAffected + " records (" + typeof(T).Name + ") have been deleted succesfully");
          }
          else
          {
              response.Add("Records of type " + typeof(T).Name + " have not been deleted");
          }
          return response;
      }
      catch (Exception exception)
      {
          throw new Exception("Error while deleting data of type " + typeof(T).Name + ".", exception);
      }
    }

    /// <summary>
    /// Gets the data dictionary.
    /// </summary>
    /// <returns>Returns data dictionary.</returns>
    public DataDictionary GetDictionary()
    {
      return Utility.Read<DataDictionary>(_dataDictionaryPath);
    }

    #region RefreshDictionary
    /// <summary>
    /// Refreshes the data dictionary
    /// </summary>
    /// <returns>Returns the response.</returns>
    public Response RefreshDictionary()
    {
      Response response = new Response();

      try
      {       

        IList<EdmSchemaError> classGenerationErrors = null;
        IList<EdmSchemaError> viewGenerationErrors = null;

        if (!_useEDMX)
        {
          classGenerationErrors = GenerateClass(false);

          Stream dataDictionaryStream = new FileStream(_csdlPath, FileMode.Open, FileAccess.Read);
          DataDictionary dataDictionary = Utility.Transform<DataDictionary>(dataDictionaryStream, _transformPath + CSDL_TO_DATA_DICTIONARY_FILENAME);
          Utility.Write<DataDictionary>(dataDictionary, _dataDictionaryPath);
          dataDictionaryStream.Close();
          response.Add("Data dictionary created successfully.");

          FileStream csdlStream = new FileStream(_csdlPath, FileMode.Open, FileAccess.Read);
          XmlReader[] csdlXmlReaders = { XmlReader.Create(csdlStream) };

          FileStream ssdlStream = new FileStream(_ssdlPath, FileMode.Open, FileAccess.Read);
          XmlReader[] ssdlXmlReaders = { XmlReader.Create(ssdlStream) };

          FileStream mslStream = new FileStream(_mslPath, FileMode.Open, FileAccess.Read);
          XmlReader[] mslXmlReaders = { XmlReader.Create(mslStream) };
          viewGenerationErrors = GenerateView(csdlXmlReaders, ssdlXmlReaders, mslXmlReaders);

          csdlStream.Close();
          ssdlStream.Close();
          mslStream.Close();          
        }
        else
        {
          classGenerationErrors = GenerateClass(true);

          XDocument doc = XDocument.Load(_edmxPath);
          XElement c = GetCsdlFromEdmx(doc);

          string csdl = c.ToString();
          byte[] byteArray = Encoding.ASCII.GetBytes(csdl);
          Stream dataDictionaryStream = new MemoryStream(byteArray);

          DataDictionary dataDictionary = Utility.Transform<DataDictionary>(dataDictionaryStream, _transformPath + CSDL_TO_DATA_DICTIONARY_FILENAME);
          Utility.Write<DataDictionary>(dataDictionary, _dataDictionaryPath);
          dataDictionaryStream.Close();
          response.Add("Data dictionary created successfully.");          
          
          XElement c1 = GetCsdlFromEdmx(doc);
          XElement s = GetSsdlFromEdmx(doc);
          XElement m = GetMslFromEdmx(doc);

          XmlReader[] csdlXmlReaders = { c1.CreateReader() };
          XmlReader[] ssdlXmlReaders = { s.CreateReader() };
          XmlReader[] mslXmlReaders = { m.CreateReader() };

          viewGenerationErrors = GenerateView(csdlXmlReaders, ssdlXmlReaders, mslXmlReaders);          
        }

        foreach (EdmSchemaError e in classGenerationErrors)
        {
          response.Add(e.Message);
        }
        foreach (EdmSchemaError e in viewGenerationErrors)
        {
          response.Add(e.Message);
        }
      }
      catch (Exception exception)
      {
        response.Add(exception.ToString());
      }

      return response;
    }

    private XElement GetCsdlFromEdmx(XDocument xdoc)
    {
      try
      {
      return (from item in xdoc.Descendants(
                  XName.Get("Schema", csdlNamespace))
              select item).First();
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    private XElement GetSsdlFromEdmx(XDocument xdoc)
    {
      try
      {
      return (from item in xdoc.Descendants(
                  XName.Get("Schema", ssdlNamespace))
              select item).First();
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    private XElement GetMslFromEdmx(XDocument xdoc)
    {
      try
      {
        return (from item in xdoc.Descendants(
                    XName.Get("Mapping", mslNamespace))
                select item).First();
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    private IList<EdmSchemaError> GenerateClass(bool useEDMX)
    {
      try
      {        
        IList<EdmSchemaError> errors = null;
        if (!useEDMX)
        {
          EntityClassGenerator codeGen = new EntityClassGenerator(LanguageOption.GenerateCSharpCode);
          errors = codeGen.GenerateCode(_csdlPath, _basePath + @"App_Code\Model.cs");
        }
        else
        {
          XElement c = GetCsdlFromEdmx(XDocument.Load(_edmxPath));
          StringWriter sw = new StringWriter();
          EntityClassGenerator codeGen = new EntityClassGenerator(LanguageOption.GenerateCSharpCode);
          errors = codeGen.GenerateCode(c.CreateReader(), sw);
          File.WriteAllText(_basePath + @"App_Code\Model.cs", sw.ToString());
          sw.Close();
        }
        return errors;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    private IList<EdmSchemaError> GenerateView(XmlReader[] csdlXmlReaders, XmlReader[] ssdlXmlReaders, XmlReader[] mslXmlReaders)
    {
      try
      {
        // load the csdl     
        IList<EdmSchemaError> cErrors = null;
        EdmItemCollection edmItemCollection =
            MetadataItemCollectionFactory.CreateEdmItemCollection(csdlXmlReaders, out cErrors);

        // load the ssdl 
        IList<EdmSchemaError> sErrors = null;
        StoreItemCollection storeItemCollection =
            MetadataItemCollectionFactory.CreateStoreItemCollection(ssdlXmlReaders, out sErrors);

        // load the msl
        IList<EdmSchemaError> mErrors = null;
        StorageMappingItemCollection mappingItemCollection =
            MetadataItemCollectionFactory.CreateStorageMappingItemCollection(
            edmItemCollection, storeItemCollection, mslXmlReaders, out mErrors);

        // either pre-compile views or validate the mappings
        IList<EdmSchemaError> viewGenerationErrors = null;

        // generate views & write them out to a file        
        EntityViewGenerator evg = new EntityViewGenerator(LanguageOption.GenerateCSharpCode);
        viewGenerationErrors =
            evg.GenerateViews(mappingItemCollection, _basePath + @"App_Code\Views.cs");
        return viewGenerationErrors;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }
    
    #endregion

    /// <summary>
    /// The GetEntitySetBase method have been implemented to reuse code. 
    /// Based on the type T of data, the method returns the EntitySetBase using the MetadataWorkspace.
    /// </summary>
    /// <typeparam name="T">This is the type of data for which the EntitySetBase is required.</typeparam>
    /// <returns>The method returns the EntitySetBase to be used by other methods.</returns>
    private  EntitySetBase GetEntitySetBase<T>()
    {
        try
        {
            MetadataWorkspace workspace = entities.MetadataWorkspace;
            ReadOnlyCollection<EntityContainer> containers =
            workspace.GetItems<EntityContainer>(DataSpace.CSpace);

            // Iterate through the collection to get each entity container.
            foreach (EntityContainer container in containers)
            {
                // Iterate through the collection to get each entity set base.
                foreach (EntitySetBase baseSet in container.BaseEntitySets)
                {
                    if (baseSet.ElementType.Name == typeof(T).Name)
                    {
                        return baseSet;
                    }
                }
            }
            return null;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    /// <summary>
    /// Gets the list of strings of types of data to be included.
    /// </summary>
    /// <param name="entityFullName">The entity name with namespace whose underneath types are to be retrieved.</param>
    /// <param name="entityName">The entity name without namespace whose underneath types are to be retrieved.</param>
    /// <param name="masterList">As it is a recursive method so master list is the list containing all the include paths.</param>
    /// <param name="mainEntityName">The top most entity name.</param>
    /// <returns>Returns the list of include paths.</returns>
    private List<string> GetIncludePaths(string entityFullName, string entityName, List<string> masterList, string mainEntityName)
    {
        string parentNode = string.Empty;
        List<string> includeList = new List<string>();
        if (mainEntityName != entityName)
        {
            parentNode = entityName;
        }
        List<string> childNodeList = GetChildrenNodes(entityFullName, entityName, masterList);
        if (childNodeList != null)
        {
            foreach (string childNode in childNodeList)
            {
                includeList.Add(childNode);
                List<string> tempList = GetIncludePaths(entityFullName, childNode, masterList, mainEntityName);
                foreach (string temp in tempList)
                {
                    includeList.Add(childNode + "." + temp);
                }
            }
        }
        return includeList;
    }

    /// <summary>
    /// Get the list of strings containing the children nodes of an entity.
    /// </summary>
    /// <param name="entityFullName">The entity name with namespace whose underneath types are to be retrieved.</param>
    /// <param name="entityName">The entity name without namespace whose underneath types are to be retrieved.</param>
    /// <param name="masterList">As it is a recursive method so master list is the list containing all the children nodes.</param>
    /// <returns>Returns the list of children nodes.</returns>
    private List<string> GetChildrenNodes(string entityFullName, string entityName, List<string> masterList)
    {
        masterList.Add(entityName);
        List<string> childPropertyList = GetNavigationProperties(entityFullName);
        if (childPropertyList != null)
        {
            foreach (string masterEntity in masterList)
            {
                childPropertyList.Remove(masterEntity);
            }
        }
        return childPropertyList;
    }

    /// <summary>
    /// Gets the navigation properties.
    /// </summary>
    /// <param name="entityFullName">The entity name with namespace whose underneath types are to be retrieved.</param>
    /// <returns>Returns the list of navigation properties.</returns>
    private List<string> GetNavigationProperties(string entityFullName)
    {
        EntityType entityType;
        MetadataWorkspace workSpace;
        List<string> navigationPropertyList;
        try
        {
            navigationPropertyList = new List<string>();
            workSpace = entities.MetadataWorkspace;
            entityType = workSpace.GetItem<EntityType>(entityFullName, DataSpace.CSpace);
            foreach (NavigationProperty navigationProperty in entityType.NavigationProperties)
            {
                navigationPropertyList.Add(navigationProperty.ToEndMember.Name);
            }
            return navigationPropertyList;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
  }
}
