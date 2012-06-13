using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using System.Web;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;
using System.Text;
using System.Collections;
using System.Net;
using System.IO;

namespace org.iringtools.datadictionarysample
{
  class Program
  {
    public DataObject CreateFolder()
    {
      DataObject folder = new DataObject();
      folder.tableName = "folder";
      folder.objectName = "folder";
      folder.objectNamespace = "iringtools.folder";
      folder.keyDelimeter = "/";
      folder.isReadOnly = false;
      folder.hasContent = true;
      folder.isListOnly = false;
      folder.defaultProjectionFormat = "json";
      folder.defaultListProjectionFormat = "json";
      folder.description = "folder";
      folder.isRelatedOnly = false;
      folder.groupName = "directory";
      folder.version = "2.4";

      folder.dataProperties = new List<DataProperty>();
      DataProperty id = new DataProperty();
      folder.dataProperties.Add(id);
      id.columnName = "id";
      id.propertyName = "id";
      id.dataType = DataType.String;
      id.dataLength = 5;
      id.isNullable = false;
      id.keyType = KeyType.assigned;
      id.showOnIndex = true;
      id.numberOfDecimals = 0;
      id.isReadOnly = true;
      id.showOnSearch = true;
      id.isHidden = false;
      id.description = "data property for id";
      id.referenceType = "RDL";
      id.isVirtual = false;

      DataProperty name = new DataProperty();
      folder.dataProperties.Add(name);
      name.columnName = "name";
      name.propertyName = "name";
      name.dataType = DataType.String;
      name.dataLength = 20;
      name.isNullable = false;
      name.keyType = KeyType.unassigned;
      name.showOnIndex = true;
      name.numberOfDecimals = 0;
      name.isReadOnly = false;
      name.showOnSearch = true;
      name.isHidden = false;
      name.description = "data property for folder name";
      name.referenceType = "RDL";
      name.isVirtual = false;

      folder.keyProperties = new List<KeyProperty>();
      KeyProperty key = new KeyProperty();
      folder.keyProperties.Add(key);
      key.keyPropertyName = "id";

      return folder;
    }

    public DataObject CreateSubFolder()
    {
      DataObject subFolder = new DataObject();
      subFolder.tableName = "subFolder";
      subFolder.objectName = "subFolder";
      subFolder.objectNamespace = "iringtools.subFolder";
      subFolder.keyDelimeter = "/";
      subFolder.isReadOnly = false;
      subFolder.hasContent = true;
      subFolder.isListOnly = false;
      subFolder.defaultProjectionFormat = "json";
      subFolder.defaultListProjectionFormat = "json";
      subFolder.description = "subFolder";
      subFolder.isRelatedOnly = false;
      subFolder.groupName = "folder";
      subFolder.version = "2.4";

      subFolder.dataProperties = new List<DataProperty>();
      DataProperty subFolderId = new DataProperty();
      subFolder.dataProperties.Add(subFolderId);
      subFolderId.columnName = "id";
      subFolderId.propertyName = "id";
      subFolderId.dataType = DataType.String;
      subFolderId.dataLength = 5;
      subFolderId.isNullable = false;
      subFolderId.keyType = KeyType.assigned;
      subFolderId.showOnIndex = true;
      subFolderId.numberOfDecimals = 0;
      subFolderId.isReadOnly = true;
      subFolderId.showOnSearch = true;
      subFolderId.isHidden = false;
      subFolderId.description = "data property for id";
      subFolderId.referenceType = "RDL";
      subFolderId.isVirtual = false;

      DataProperty subFolderName = new DataProperty();
      subFolder.dataProperties.Add(subFolderName);
      subFolderName.columnName = "name";
      subFolderName.propertyName = "name";
      subFolderName.dataType = DataType.String;
      subFolderName.dataLength = 20;
      subFolderName.isNullable = false;
      subFolderName.keyType = KeyType.unassigned;
      subFolderName.showOnIndex = true;
      subFolderName.numberOfDecimals = 0;
      subFolderName.isReadOnly = false;
      subFolderName.showOnSearch = true;
      subFolderName.isHidden = false;
      subFolderName.description = "data property for subFolder name";
      subFolderName.referenceType = "RDL";
      subFolderName.isVirtual = false;

      subFolder.keyProperties = new List<KeyProperty>();
      KeyProperty subFolderKey = new KeyProperty();
      subFolder.keyProperties.Add(subFolderKey);
      subFolderKey.keyPropertyName = "id";

      return subFolder;
    }

    static void Main(string[] args)
    {
      Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
      Program pg = new Program();      
      DataDictionary dataDictionary = new DataDictionary();
      DataObject folder = pg.CreateFolder();
      DataObject subFolder = pg.CreateSubFolder();
      dataDictionary.dataObjects = new List<DataObject>();
      dataDictionary.dataObjects.Add(folder);
      dataDictionary.dataObjects.Add(subFolder);

      folder.dataRelationships = new List<DataRelationship>();
      DataRelationship relation = new DataRelationship();
      folder.dataRelationships.Add(relation);
      relation.relatedObjectName = "subFolder";
      relation.relationshipName = "idRelation";
      relation.relationshipType = RelationshipType.OneToOne;
      relation.propertyMaps = new List<PropertyMap>();
      PropertyMap map = new PropertyMap();
      map.dataPropertyName = "id";
      map.relatedPropertyName = "id";
      relation.propertyMaps.Add(map);

      string path = Directory.GetCurrentDirectory();
      path = path.Substring(0, path.LastIndexOf("\\bin"));
      path = path + "\\DatadictionarySample.xml";
      
      Utility.Write<DataDictionary>(dataDictionary, path);
    }
  }
}
