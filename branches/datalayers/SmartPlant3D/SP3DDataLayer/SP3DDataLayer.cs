﻿using System;
using System.Data;
using System.Collections;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using System.Xml.Linq;
using Ciloci.Flee;
using log4net;
using Ninject;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;
using StaticDust.Configuration;
using Ingr.SP3D.Common.Middle.Services;
using Ingr.SP3D.Common.Middle;
//using Ingr.SP3D.Structure.Middle;
//using Ingr.SP3D.ReferenceData.Middle;
//using Ingr.SP3D.Systems.Middle;
//using Ingr.SP3D.ReferenceData.Middle.Services;
using NHibernate;
using Ninject.Extensions.Xml;
using Microsoft.Win32;
using System.Reflection;

namespace iringtools.sdk.sp3ddatalayer
{
    public class MyType
    {
        public MyType()
        {
            Console.WriteLine();
            Console.WriteLine("MyType instantiated!");
        }
    }
  public class SP3DDataLayer : BaseDataLayer
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(SP3DDataLayer));
    private SP3DProvider sp3dProvider = null;

    static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
        Assembly objExecutingAssemblies = Assembly.GetExecutingAssembly();
        AssemblyName[] arrReferencedAssmbNames = objExecutingAssemblies.GetReferencedAssemblies();
        //Loop through the array of referenced assembly names.
        foreach (AssemblyName strAssmbName in arrReferencedAssmbNames)
        {
            //Look for the assembly names that have raised the "AssemblyResolve" event.
            if ((strAssmbName.Name.EndsWith("CommonMiddle") && strAssmbName.FullName.Substring(0, strAssmbName.FullName.IndexOf(",")) == "CommonMiddle"))
            {
                //We only have this handler to deal with loading of CommonMiddle. Rest everything we dont bother.
                //AppDomain.CurrentDomain.AssemblyResolve -= MyResolveEventHandler;
                //Build the path of the assembly from where it has to be loaded.
                //Check CurrentUser,LocalMachine, 64bit CurrentUser,LocalMachine
                const string ProductInstallationKey = "\\Software\\Intergraph\\SP3D\\Installation";
                const string Wow6432Path = "\\Software\\Wow6432Node\\Intergraph\\SP3D\\Installation";
                const string InstallDirKey = "INSTALLDIR";

                string sInstallPath = Convert.ToString(Registry.GetValue(Registry.CurrentUser.ToString() + ProductInstallationKey, InstallDirKey, ""));
                //try Local Machine
                if (string.IsNullOrEmpty(sInstallPath))
                {
                    sInstallPath = Convert.ToString(Registry.GetValue(Registry.LocalMachine.ToString() + ProductInstallationKey, InstallDirKey, ""));
                }
                //try 64-bit CurrentUser
                if (string.IsNullOrEmpty(sInstallPath))
                {
                    sInstallPath = Convert.ToString(Registry.GetValue(Registry.CurrentUser.ToString() + Wow6432Path, InstallDirKey, ""));
                }
                //try 64-bit LocalMachine
                if (string.IsNullOrEmpty(sInstallPath))
                {
                    sInstallPath = Convert.ToString(Registry.GetValue(Registry.LocalMachine.ToString() + Wow6432Path, InstallDirKey, ""));
                }
                if ((!string.IsNullOrEmpty((sInstallPath.Trim()))))
                {
                    if ((sInstallPath.EndsWith("\\") == false))
                        sInstallPath += "\\";
                    sInstallPath += "Core\\Container\\Bin\\Assemblies\\Release\\";
                }
                else
                {
                    throw new Exception("Error Reading SmartPlant 3D Installation Directory from Registry !!! Exiting");
                }
                //Load the assembly from the specified path and return it.
                string strTempAssmbPath = sInstallPath + "CommonMiddle" + ".dll";
                return Assembly.LoadFrom(strTempAssmbPath);
            }
        }
        return null;
    }

    
    [Inject]
    public  SP3DDataLayer(AdapterSettings settings)
      : base(settings)
    {
       
        sp3dProvider = new SP3DProvider(settings);
    }

    

    public override DataDictionary GetDictionary()
    {
      if (sp3dProvider == null)
      {
        setSP3DProviderSettings();
      }

      DataDictionary dataDictionary = sp3dProvider.GetDictionary();
      return dataDictionary;
    }

    public override Response Refresh(string objectType, DataFilter filter)
    {
      if (sp3dProvider == null)
      {
        setSP3DProviderSettings();
      }

      return sp3dProvider.RefreshCachingTable(objectType, filter);
    }

    public override Response Refresh(string objectType)
    {
      if (sp3dProvider == null)
      {
        setSP3DProviderSettings();
      }

      return sp3dProvider.RefreshCachingTables(objectType);
    }

    public override Response RefreshAll()
    {
      return Refresh(string.Empty);
    }

    public override IList<IDataObject> Get(string objectType, DataFilter filter, int pageSize, int startIndex)
    {
      if (sp3dProvider == null)
      {
        setSP3DProviderSettings();
      }

      return sp3dProvider.Get(objectType, filter, pageSize, startIndex);
    }   

    public DataObject GetObjectDefinition(string objectType)
    {
      DataDictionary dictionary = GetDictionary();
      DataObject objDef = dictionary.dataObjects.Find(x => x.objectName.ToLower() == objectType.ToLower());
      return objDef;
    }

    public override Response Post(IList<IDataObject> dataObjects)
    {
      Response response = new Response();

      if (sp3dProvider == null)
      {
        setSP3DProviderSettings();
      }

      response = sp3dProvider.PostSP3DBusinessObjects(dataObjects);
      response.Append(sp3dProvider.Post(dataObjects));

      return response;
    }

    public override IList<string> GetIdentifiers(string objectType, DataFilter filter)
    {
      if (sp3dProvider == null)
      {
        setSP3DProviderSettings();
      }

      return sp3dProvider.GetIdentifiers(objectType, filter);
    }

    public override IList<IDataObject> Get(string objectType, IList<string> identifiers)
    {
      if (sp3dProvider == null)
      {
        setSP3DProviderSettings();
      }
      return sp3dProvider.Get(objectType, identifiers);      
    }

    public override IList<IDataObject> Create(string objectType, IList<string> identifiers)
    {
      if (sp3dProvider == null)
      {
        setSP3DProviderSettings();
      }
      return sp3dProvider.Create(objectType, identifiers);
    }

    public override Response Delete(string objectType, IList<string> identifiers)
    {
      if (sp3dProvider == null)
      {
        setSP3DProviderSettings();
      }

      Response response = sp3dProvider.Delete(objectType, identifiers);
      response.Append(sp3dProvider.DeleteSP3DIdentifiers(objectType, identifiers));
      return response;
    }

    public override Response Delete(string objectType, DataFilter filter)
    {
      if (sp3dProvider == null)
      {
        setSP3DProviderSettings();
      }

      return sp3dProvider.DeleteSP3DBusinessObjects(objectType, filter);
    }

    public override IList<IDataObject> GetRelatedObjects(IDataObject parentDataObject, string relatedObjectType)
    {
      if (sp3dProvider == null)
      {
        setSP3DProviderSettings();
      }

      IList<IDataObject> relatedObjects = null;
      ISession session = null;

      try
      {
        DataObject dataObject = sp3dProvider._dataDictionary.dataObjects.Find(c => c.objectName.ToLower() == parentDataObject.GetType().Name.ToLower());
        if (dataObject == null)
        {
          throw new Exception("Parent data object [" + parentDataObject.GetType().Name + "] not found.");
        }

        DataRelationship dataRelationship = dataObject.dataRelationships.Find(c => c.relatedObjectName.ToLower() == relatedObjectType.ToLower());
        if (dataRelationship == null)
        {
          throw new Exception("Relationship between data object [" + parentDataObject.GetType().Name +
            "] and related data object [" + relatedObjectType + "] not found.");
        }

        session = NHibernateSessionManager.Instance.GetSession(_settings["AppDataPath"], _settings["Scope"]);

        StringBuilder sql = new StringBuilder();
        sql.Append("from " + dataRelationship.relatedObjectName + " where ");

        foreach (PropertyMap map in dataRelationship.propertyMaps)
        {
          DataProperty propertyMap = dataObject.dataProperties.First(c => c.propertyName == map.dataPropertyName);

          if (propertyMap.dataType == DataType.String)
          {
            sql.Append(map.relatedPropertyName + " = '" + parentDataObject.GetPropertyValue(map.dataPropertyName) + "' and ");
          }
          else
          {
            sql.Append(map.relatedPropertyName + " = " + parentDataObject.GetPropertyValue(map.dataPropertyName) + " and ");
          }
        }

        sql.Remove(sql.Length - 4, 4);  // remove the tail "and "
        IQuery query = session.CreateQuery(sql.ToString());
        relatedObjects = query.List<IDataObject>();

        if (relatedObjects != null && relatedObjects.Count > 0 && dataRelationship.relationshipType == RelationshipType.OneToOne)
        {
          return new List<IDataObject> { relatedObjects.First() };
        }

        return relatedObjects;
      }
      catch (Exception e)
      {
        string error = "Error getting related objects [" + relatedObjectType + "] " + e;
        _logger.Error(error);
        throw new Exception(error);
      }
      finally
      {
        sp3dProvider.CloseSession(session);
      }
    }

    public override long GetCount(string objectType, DataFilter filter)
    {
      if (sp3dProvider == null)
      {
        setSP3DProviderSettings();
      }
      return sp3dProvider.GetCount(objectType, filter);      
    }

    public override long GetRelatedCount(IDataObject parentDataObject, string relatedObjectType)
    {
      try
      {
        DataFilter filter = CreateDataFilter(parentDataObject, relatedObjectType);
        return GetCount(relatedObjectType, filter);
      }
      catch (Exception ex)
      {
        string error = String.Format("Error getting related object count for object {0}: {1}", relatedObjectType, ex);
        _logger.Error(error);
        throw new Exception(error);
      }
    }

    public override IList<IDataObject> GetRelatedObjects(IDataObject parentDataObject, string relatedObjectType, int pageSize, int startIndex)
    {
      try
      {
        DataFilter filter = CreateDataFilter(parentDataObject, relatedObjectType);
        return Get(relatedObjectType, filter, pageSize, startIndex);
      }
      catch (Exception ex)
      {
        string error = String.Format("Error getting related objects for object {0}: {1}", relatedObjectType, ex);
        _logger.Error(error);
        throw new Exception(error);
      }
    }

    private DataFilter FilterByIdentity(string objectType, DataFilter filter, IdentityProperties identityProperties)
    {
      DataObject dataObject = sp3dProvider._databaseDictionary.dataObjects.Find(d => d.objectName == objectType);
      DataProperty dataProperty = dataObject.dataProperties.Find(p => p.columnName == identityProperties.IdentityProperty);

      if (dataProperty != null)
      {
        if (filter == null)
        {
          filter = new DataFilter();
        }
        
        if (filter.Expressions == null)
        {
          filter.Expressions = new List<org.iringtools.library.Expression>();
        }
        else if (filter.Expressions.Count > 0)
        {
          org.iringtools.library.Expression firstExpression = filter.Expressions.First();
          org.iringtools.library.Expression lastExpression = filter.Expressions.Last();
          firstExpression.OpenGroupCount++;
          lastExpression.CloseGroupCount++;          
        }
      }

      return filter;
    }

    private void setSP3DProviderSettings()
    {
      ServiceSettings servcieSettings = new ServiceSettings();
      _settings.AppendSettings(servcieSettings);
      sp3dProvider = new SP3DProvider(_settings);
    }
  }
}







