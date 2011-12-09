Ext.ns('AdapterManager');

function setTreeProperty(dsConfigPane, dbInfo, dbDict, tablesSelectorPane) {
    var treeProperty = {};
    if (tablesSelectorPane)
        treeProperty.enableSummary = tablesSelectorPane.getForm().findField('enableSummary').getValue();
    else if (dbDict.enableSummary)
        treeProperty.enableSummary = dbDict.enableSummary;
    else
        treeProperty.enableSummary = false;

	if (dsConfigPane) {
		var dsConfigForm = dsConfigPane.getForm();
		treeProperty.provider = dsConfigForm.findField('dbProvider').getValue();
		var dbServer = dsConfigForm.findField('dbServer').getValue();
		dbServer = (dbServer.toLowerCase() == 'localhost' ? '.' : dbServer);
		var upProvider = treeProperty.provider.toUpperCase();
		var serviceNamePane = dsConfigPane.items.items[10];
		var serviceName = '';
		var serName = '';
		if (serviceNamePane.items.items[0]) {
			serviceName = serviceNamePane.items.items[0].value;
			serName = serviceNamePane.items.items[0].serName;
		}
		else if (dbInfo) {
			if (dbInfo.dbInstance)
				serviceName = dbInfo.dbInstance;
			if (dbInfo.serName)
				serName = dbInfo.serName;
		}

		if (upProvider.indexOf('MSSQL') > -1) {
			var dbInstance = dsConfigForm.findField('dbInstance').getValue();
			var dbDatabase = dsConfigForm.findField('dbName').getValue();
			if (dbInstance.toUpperCase() == "DEFAULT") {
				var dataSrc = 'Data Source=' + dbServer + ';Initial Catalog=' + dbDatabase;
			} else {
				var dataSrc = 'Data Source=' + dbServer + '\\' + dbInstance + ';Initial Catalog=' + dbDatabase;
			}
		}
		else if (upProvider.indexOf('ORACLE') > -1)
			var dataSrc = 'Data Source=' + '(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=' + dbServer + ')(PORT=' + dsConfigForm.findField('portNumber').getValue() + ')))(CONNECT_DATA=(SERVER=DEDICATED)(' + serName + '=' + serviceName + ')))';
		else if (upProvider.indexOf('MYSQL') > -1)
			var dataSrc = 'Data Source=' + dbServer;

		treeProperty.connectionString = dataSrc
                                  + ';User ID=' + dsConfigForm.findField('dbUserName').getValue()
                                  + ';Password=' + dsConfigForm.findField('dbPassword').getValue();

		treeProperty.schemaName = dsConfigForm.findField('dbSchema').getValue();
	}
	else {
		treeProperty.provider = dbDict.Provider;
		var dbServer = dbInfo.dbServer;
		var upProvider = treeProperty.provider.toUpperCase();
		dbServer = (dbServer.toLowerCase() == 'localhost' ? '.' : dbServer);

		if (upProvider.indexOf('MSSQL') > -1) {			
			if (dbInfo.dbInstance) {
				if (dbInfo.dbInstance.toUpperCase() == "DEFAULT") {
					var dataSrc = 'Data Source=' + dbServer + ';Initial Catalog=' + dbInfo.dbName;
				} else {
					var dataSrc = 'Data Source=' 
					            + dbServer + '\\' + dbInfo.dbInstance
											+ ';Initial Catalog=' + dbInfo.dbName;
				}
			}
		}
		else if (upProvider.indexOf('ORACLE') > -1)
			var dataSrc = 'Data Source=' + '(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=' + dbServer + ')(PORT=' + dbInfo.portNumber + ')))(CONNECT_DATA=(SERVER=DEDICATED)(' + dbInfo.serName + '=' + dbInfo.dbInstance + ')))';
		else if (upProvider.indexOf('MYSQL') > -1)
			var dataSrc = 'Data Source=' + dbServer;

		treeProperty.connectionString = dataSrc
                                        + ';User ID=' + dbInfo.dbUserName
                                        + ';Password=' + dbInfo.dbPassword;
		treeProperty.schemaName = dbDict.SchemaName;
	}
	return treeProperty;
};

function getDataTypeIndex(datatype, dataTypes) {
	var i = 0;

	while (!dataTypes[i])
		i++;

	for (var k = i; k < dataTypes.length; k++) {
		if (dataTypes[k][1] == datatype)
			return dataTypes[k][0];
	}
};

function getFolderFromChildNode(folderNode, dataTypes) {
	var folderNodeProp = folderNode.attributes.properties;
	var folder = {};
	var keyName = '';
	
	folder.tableName = folderNodeProp.tableName;
	folder.objectNamespace = folderNodeProp.objectNamespace;
	folder.objectName = folderNodeProp.objectName;
	folder.description = folderNodeProp.description;

	if (!folderNodeProp.keyDelimiter)
		folder.keyDelimeter = 'null';
	else
		folder.keyDelimeter = folderNodeProp.keyDelimiter;

	folder.keyProperties = new Array();
	folder.dataProperties = new Array();
	folder.dataRelationships = new Array();

	for (var j = 0; j < folderNode.attributes.children.length; j++) {
		if (folderNode.childNodes[1])
			var propertyFolderNode = folderNode.childNodes[1];
		else
			var propertyFolderNode = folderNode.attributes.children[1];

		if (folderNode.childNodes[0])
			var keyFolderNode = folderNode.childNodes[0];
		else
			var keyFolderNode = folderNode.attributes.children[0];

		if (folderNode.childNodes[2])
			var relationFolderNode = folderNode.childNodes[2];
		else
			var relationFolderNode = folderNode.attributes.children[2];

		if (folderNode.childNodes[j])
			subFolderNodeText = folderNode.childNodes[j].text;
		else
			subFolderNodeText = folderNode.attributes.children[j].text;

		switch (subFolderNodeText) {
			case 'Keys':
				if (folderNode.childNodes[1])
					var keyChildenNodes = keyFolderNode.childNodes;
				else
					var keyChildenNodes = keyFolderNode.children;

				for (var k = 0; k < keyChildenNodes.length; k++) {
					var keyNode = keyChildenNodes[k];

					if (!keyNode.hidden) {
						var keyProps = {};

						if (keyNode.properties)
							var keyNodeProf = keyNode.properties;
						else if (keyNode.attributes.attributes)
							var keyNodeProf = keyNode.attributes.attributes.properties;
						else
							var keyNodeProf = keyNode.attributes.properties;

						keyProps.keyPropertyName = keyNode.text;
						keyName = keyNode.text;
						folder.keyProperties.push(keyProps);

						var tagProps = {};
						tagProps.columnName = keyNodeProf.columnName;
						tagProps.propertyName = keyNode.text;
						if (typeof keyNodeProf.dataType == 'string')
							tagProps.dataType = getDataTypeIndex(keyNodeProf.dataType, dataTypes);
						else
							tagProps.dataType = keyNodeProf.dataType;

						tagProps.dataLength = keyNodeProf.dataLength;

						if (keyNodeProf.nullable)
							tagProps.isNullable = keyNodeProf.nullable.toString().toLowerCase();
						else
						    tagProps.isNullable = 'false';

						if (keyNodeProf.isHidden)
						    tagProps.isHidden = keyNodeProf.isHidden.toString().toLowerCase();
						else
						    tagProps.isHidden = 'false';

						if (!keyNodeProf.keyType)
							tagProps.keyType = 1;
						else 
							if (typeof keyNodeProf.keyType != 'string')
								tagProps.keyType = keyNodeProf.keyType;
							else {
								switch (keyNodeProf.keyType.toLowerCase()) {
									case 'assigned':
										tagProps.keyType = 1;
										break;
									case 'unassigned':
										tagProps.keyType = 0;
										break;
									default:
										tagProps.keyType = 1;
										break;
								}
							}

						if (keyNodeProf.showOnIndex)
							tagProps.showOnIndex = keyNodeProf.showOnIndex.toString().toLowerCase();
						else
							tagProps.showOnIndex = 'false';

						tagProps.numberOfDecimals = keyNodeProf.numberOfDecimals;
						folder.dataProperties.push(tagProps);
					}
				}
				break;
		case 'Properties':
		  if (folderNode.childNodes[1])
		    var propChildenNodes = propertyFolderNode.childNodes;
		  else
		    var propChildenNodes = propertyFolderNode.children;
		  for (var k = 0; k < propChildenNodes.length; k++) {
		    var propertyNode = propChildenNodes[k];

		    if (!propertyNode.hidden) {
		      if (propertyNode.properties)
		        var propertyNodeProf = propertyNode.properties;
		      else if (propertyNode.attributes)
		        var propertyNodeProf = propertyNode.attributes.properties;

		      var props = {};
		      props.columnName = propertyNodeProf.columnName;
		      props.propertyName = propertyNodeProf.propertyName;

		      if (typeof propertyNodeProf.dataType == 'string')
		        props.dataType = getDataTypeIndex(propertyNodeProf.dataType, dataTypes);
		      else
		        props.dataType = propertyNodeProf.dataType;

		      props.dataLength = propertyNodeProf.dataLength;

		      if (propertyNodeProf.nullable)
		        props.isNullable = propertyNodeProf.nullable.toString().toLowerCase();
		      else
		        props.isNullable = 'false';

		      if (keyName != '' ) {
		        if (props.columnName == keyName)
		          props.keyType = 1;
		        else
		          props.keyType = 0;
		      }
		      else
		        props.keyType = 0;

		      if (propertyNodeProf.showOnIndex)
		        props.showOnIndex = propertyNodeProf.showOnIndex.toString().toLowerCase();
		      else
		        props.showOnIndex = 'false';
                
		      if (propertyNodeProf.isHidden)
		          props.isHidden = propertyNodeProf.isHidden.toString().toLowerCase();
		      else
		          props.isHidden = 'false';

		      props.numberOfDecimals = propertyNodeProf.numberOfDecimals;

		      folder.dataProperties.push(props);
		    }
		  }
		  break;
			case 'Relationships':
				if (!relationFolderNode)
					break;

				if (relationFolderNode.childNodes)
					var relChildenNodes = relationFolderNode.childNodes;
				else
					var relChildenNodes = relationFolderNode.children;

				if (relChildenNodes)
					for (var k = 0; k < relChildenNodes.length; k++) {
						var relationNode = relChildenNodes[k];
						var found = false;
						for (var ik = 0; ik < folder.dataRelationships.length; ik++)
							if (relationNode.text.toLowerCase() == folder.dataRelationships[ik].relationshipName.toLowerCase()) {
								found = true;
								break;
							}

						if (found || relationNode.text == '')
							continue;

						if (relationNode.attributes) {
							if (relationNode.attributes.attributes) {
								if (relationNode.attributes.attributes.propertyMap)
									var relationNodeAttr = relationNode.attributes.attributes;
								else if (relationNode.attributes.propertyMap)
									var relationNodeAttr = relationNode.attributes;
								else
									var relationNodeAttr = relationNode.attributes.attributes;
							}
							else {
								var relationNodeAttr = relationNode.attributes;
							}
						}
						else {
							relationNodeAttr = relationNode;
						}

						var relation = {};
						relation.propertyMaps = new Array();

						for (var m = 0; m < relationNodeAttr.propertyMap.length; m++) {
							var propertyPairNode = relationNodeAttr.propertyMap[m];
							var propertyPair = {};

							propertyPair.dataPropertyName = propertyPairNode.dataPropertyName;
							propertyPair.relatedPropertyName = propertyPairNode.relatedPropertyName;
							relation.propertyMaps.push(propertyPair);
						}

						relation.relatedObjectName = relationNodeAttr.relatedObjectName;
						relation.relationshipName = relationNodeAttr.text;
						relation.relationshipType = relationNodeAttr.relationshipTypeIndex;
						folder.dataRelationships.push(relation);
					}
				break;
		}
	}
	return folder;
};

function getTreeJson(dsConfigPane, rootNode, dbInfo, dbDict, dataTypes, tablesSelectorPane) {
	var treeProperty = {};
	treeProperty.dataObjects = new Array();
	treeProperty.IdentityConfiguration = null;

	var tProp = setTreeProperty(dsConfigPane, dbInfo, dbDict, tablesSelectorPane);
	treeProperty.connectionString = tProp.connectionString;
	if (treeProperty.connectionString != null && treeProperty.connectionString.length > 0) {
		treeProperty.connectionString = Base64.encode(tProp.connectionString);
	}
	treeProperty.schemaName = tProp.schemaName;
	treeProperty.provider = tProp.provider;
	treeProperty.enableSummary = tProp.enableSummary;	

	var keyName;
	for (var i = 0; i < rootNode.childNodes.length; i++) {
		var folder = getFolderFromChildNode(rootNode.childNodes[i], dataTypes);
		treeProperty.dataObjects.push(folder);
  }

  dbDict.ConnectionString = treeProperty.connectionString;
  dbDict.SchemaName = treeProperty.schemaName;
  dbDict.Provider = treeProperty.provider;
  dbDict.dataObjects = treeProperty.dataObjects;
  
	return treeProperty;
};