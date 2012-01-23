﻿Ext.define('AM.view.nhibernate.SelectTablesPanel', {
    extend: 'Ext.form.Panel',
    alias: 'widget.selectTables',   
    frame: false,
	border: false,
	autoScroll: true,	
	bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
	labelWidth: 140,
	monitorValid: true,

	initComponent: function () {
	    this.items = [{
	        xtype: 'label',
	        fieldLabel: 'Select Tables',
	        labelSeparator: '',
	        itemCls: 'form-title'
	    }, {
	        xtype: 'itemselector',
	        hideLabel: true,
	        bodyStyle: 'background:#eee',
	        frame: true,
	        name: 'tableSelector',
	        imagePath: 'scripts/ext-3.3.1/examples/ux/images/',
	        multiselects: [{
	            width: 240,
	            height: 370,
	            store: availItems,
	            displayField: 'tableName',
	            valueField: 'tableValue',
	            border: 0
	        }, {
	            width: 240,
	            height: 370,
	            store: selectItems,
	            displayField: 'tableName',
	            valueField: 'tableValue',
	            border: 0
	        }],
	        listeners: {
	            change: function (itemSelector, selectedValuesStr) {
	                var selectTables = itemSelector.toMultiselect.store.data.items;
	                for (var i = 0; i < selectTables.length; i++) {
	                    var selectTableName = selectTables[i].data.text;
	                    if (selectTableName == '')
	                        itemSelector.toMultiselect.store.removeAt(i);
	                }

	                var availTables = itemSelector.fromMultiselect.store.data.items;
	                for (var i = 0; i < availTables.length; i++) {
	                    var availTableName = availTables[i].data.text
	                    if (availTables[i].data.text == '')
	                        itemSelector.fromMultiselect.store.removeAt(i);
	                }
	            }
	        }
	    }];

	    this.tbar = new Ext.Toolbar({
	        items: [{
	            xtype: 'tbspacer',
	            width: 4
	        }, {
	            xtype: 'tbbutton',
	            icon: 'Content/img/16x16/apply.png',
	            text: 'Apply',
	            tooltip: 'Apply the current changes to the data objects tree',
	            handler: function () {
	                var editPane = dataObjectsPane.items.items[1];
	                var dsConfigPane = editPane.items.map[scopeName + '.' + appName + '.dsconfigPane'];
	                var tablesSelectorPane = editPane.items.map[scopeName + '.' + appName + '.tablesSelectorPane'];
	                var tablesSelForm = tablesSelectorPane.getForm();
	                var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];
	                var serName = '';
	                var serviceName = '';

	                if (dbObjectsTree.disabled) {
	                    dbObjectsTree.enable();
	                }

	                if (dsConfigPane) {
	                    var serviceNamePane = dsConfigPane.items.items[10];
	                    if (serviceNamePane.items.items[0])
	                        serName = serviceNamePane.items.items[0].serName;
	                }
	                else {
	                    if (dbInfo.serName)
	                        serName = dbInfo.serName;
	                }

	                var treeLoader = dbObjectsTree.getLoader();
	                if (tablesSelForm.findField('tableSelector').getValue().indexOf('') == -1)
	                    var selectTableNames = tablesSelForm.findField('tableSelector').getValue();
	                else {
	                    var tableNames = tablesSelForm.findField('tableSelector').toMultiselect.store.data.items;
	                    var selectTableNames = new Array();
	                    for (var i = 0; i < tableNames.length; i++) {
	                        selectTableNames.push(tableNames[i].data.text);
	                    }
	                }

	                if (selectTableNames.length < 1) {
	                    var rootNode = dbObjectsTree.getRootNode();
	                    while (rootNode.firstChild) {
	                        rootNode.removeChild(rootNode.firstChild);
	                    }
	                    return;
	                }

	                userTableNames = new Array();

	                if (selectTableNames[1]) {
	                    if (selectTableNames[1].length > 1 && selectTableNames[0].length > 1) {
	                        for (var i = 0; i < selectTableNames.length; i++) {
	                            userTableNames.push(selectTableNames[i]);
	                        }
	                    }
	                    else {
	                        userTableNames.push(selectTableNames)
	                    }
	                }
	                else {
	                    userTableNames.push(selectTableNames[0]);
	                }

	                treeLoader.dataUrl = 'AdapterManager/DBObjects';
	                if (dsConfigPane) {
	                    var dsConfigForm = dsConfigPane.getForm();
	                    treeLoader.baseParams = {
	                        scope: scopeName,
	                        app: appName,
	                        dbProvider: dsConfigForm.findField('dbProvider').getValue(),
	                        dbServer: dsConfigForm.findField('dbServer').getValue(),
	                        dbInstance: dsConfigForm.findField('dbInstance').getValue(),
	                        dbName: dsConfigForm.findField('dbName').getValue(),
	                        dbSchema: dsConfigForm.findField('dbSchema').getValue(),
	                        dbUserName: dsConfigForm.findField('dbUserName').getValue(),
	                        dbPassword: dsConfigForm.findField('dbPassword').getValue(),
	                        portNumber: dsConfigForm.findField('portNumber').getValue(),
	                        tableNames: selectTableNames,
	                        serName: serName
	                    };
	                }
	                else {
	                    treeLoader.baseParams = {
	                        scope: scopeName,
	                        app: appName,
	                        dbProvider: dbDict.Provider,
	                        dbServer: dbInfo.dbServer,
	                        dbInstance: dbInfo.dbInstance,
	                        dbName: dbInfo.dbName,
	                        dbSchema: dbDict.SchemaName,
	                        dbUserName: dbInfo.dbUserName,
	                        dbPassword: dbInfo.dbPassword,
	                        portNumber: dbInfo.portNumber,
	                        tableNames: selectTableNames,
	                        serName: serName
	                    };
	                }

	                var rootNode = dbObjectsTree.getRootNode();
	                rootNode.reload(
                        function (rootNode) {
                            loadTree(rootNode, dbDict);
                        });
	            }
	        }, {
	            xtype: 'tbspacer',
	            width: 4
	        }, {
	            xtype: 'tbbutton',
	            icon: 'Content/img/16x16/edit-clear.png',
	            text: 'Reset',
	            tooltip: 'Reset to the latest applied changes',
	            handler: function () {
	                var rootNode = dataObjectsPane.items.items[0].items.items[0].getRootNode();
	                var selectTableNames = new Array();
	                var selectTableNamesSingle = new Array();
	                var firstSelectTableNames = new Array();
	                var availTableName = new Array();
	                var found = false;

	                for (var i = 0; i < dbTableNames.items.length; i++) {
	                    availTableName.push(dbTableNames.items[i]);
	                }

	                for (var j = 0; j < availTableName.length; j++)
	                    for (var i = 0; i < rootNode.childNodes.length; i++) {
	                        if (rootNode.childNodes[i].data.property.tableName.toLowerCase() == availTableName[j].toLowerCase()) {
	                            found = true;
	                            availTableName.splice(j, 1);
	                            j--;
	                            break;
	                        }
	                    }

	                for (var i = 0; i < rootNode.childNodes.length; i++) {
	                    var nodeText = rootNode.childNodes[i].data.property.tableName;
	                    selectTableNames.push([nodeText, nodeText]);
	                    selectTableNamesSingle.push(nodeText);
	                }

	                if (selectTableNames[0]) {
	                    firstSelectTableNames.push(selectTableNames[0]);
	                    var tablesSelector = tablesSelectorPane.items.items[1];

	                    if (tablesSelector.toMultiselect.store.data) {
	                        tablesSelector.toMultiselect.reset();
	                        tablesSelector.toMultiselect.store.removeAll();
	                    }

	                    tablesSelector.toMultiselect.store.loadData(firstSelectTableNames);
	                    tablesSelector.toMultiselect.store.commitChanges();

	                    var firstSelectTables = tablesSelector.toMultiselect.store.data.items;
	                    var loadSingle = false;
	                    var selectTableName = firstSelectTables[0].data.text;

	                    if (selectTableName[1])
	                        if (selectTableName[1].length > 1)
	                            var loadSingle = true;

	                    tablesSelector.toMultiselect.reset();
	                    tablesSelector.toMultiselect.store.removeAll();

	                    if (!loadSingle)
	                        tablesSelector.toMultiselect.store.loadData(selectTableNames);
	                    else
	                        tablesSelector.toMultiselect.store.loadData(selectTableNamesSingle);

	                    tablesSelector.toMultiselect.store.commitChanges();
	                }
	                else {
	                    if (!tablesSelector)
	                        var tablesSelector = tablesSelectorPane.items.items[1];
	                    if (tablesSelector.toMultiselect) {
	                        tablesSelector.toMultiselect.reset();
	                        tablesSelector.toMultiselect.store.removeAll();
	                        tablesSelector.toMultiselect.store.commitChanges();
	                    }
	                }

	                if (tablesSelector.fromMultiselect.store.data) {
	                    tablesSelector.fromMultiselect.reset();
	                    tablesSelector.fromMultiselect.store.removeAll();
	                }

	                tablesSelector.fromMultiselect.store.loadData(availTableName);
	                tablesSelector.fromMultiselect.store.commitChanges();
	            }
	        }]
	    });

	    editPane.add(tablesSelectorPane);
	    var panelIndex = editPane.items.indexOf(tablesSelectorPane);
	    editPane.getLayout().setActiveItem(panelIndex);
    }
});		

function loadTree (rootNode) {
	var relationTypeStr = ['OneToOne', 'OneToMany'];
	var dbDict = AM.view.nhibernate.dbDict.value;

	// sync data object tree with data dictionary
	for (var i = 0; i < rootNode.childNodes.length; i++) {
	    var dataObjectNode = rootNode.childNodes[i];
	    dataObjectNode.raw.nhproperty.tableName = dataObjectNode.raw.text;
	    for (var ijk = 0; ijk < dbDict.dataObjects.length; ijk++) {
	        var dataObject = dbDict.dataObjects[ijk];

	        if (dataObjectNode.raw.text.toUpperCase() != dataObject.tableName.toUpperCase())
	            continue;

	        // sync data object
	        dataObjectNode.raw.nhproperty.objectNamespace = dataObject.objectNamespace;
	        dataObjectNode.raw.nhproperty.objectName = dataObject.objectName;
	        dataObjectNode.raw.nhproperty.keyDelimiter = dataObject.keyDelimeter;
	        dataObjectNode.raw.nhproperty.description = dataObject.description;
	        dataObjectNode.raw.text = dataObject.objectName;
	        dataObjectNode.set('title', dataObject.objectName);

	        if (dataObject.objectName.toLowerCase() == dataObjectNode.raw.text.toLowerCase()) {
	            var shownProperty = new Array();
	            var keysNode = dataObjectNode.childNodes[0];
	            var propertiesNode = dataObjectNode.childNodes[1];
	            var relationshipsNode = dataObjectNode.childNodes[2];

	            // sync data properties
                if (propertiesNode)
	            for (var j = 0; j < propertiesNode.childNodes.length; j++) {
	                for (var jj = 0; jj < dataObject.dataProperties.length; jj++) {
	                    if (propertiesNode.childNodes[j].raw.text.toLowerCase() == dataObject.dataProperties[jj].columnName.toLowerCase()) {

	                        if (!hasShown(shownProperty, propertiesNode.childNodes[j].raw.text.toLowerCase())) {
	                            shownProperty.push(propertiesNode.childNodes[j].raw.text.toLowerCase());
	                            propertiesNode.childNodes[j].raw.hidden = false;
	                        }

	                        propertiesNode.childNodes[j].raw.text = dataObject.dataProperties[jj].propertyName;
	                        propertiesNode.childNodes[j].raw.nhproperty.propertyName = dataObject.dataProperties[jj].propertyName;
	                        propertiesNode.childNodes[j].raw.nhproperty.isHidden = dataObject.dataProperties[jj].isHidden;
	                    }
	                }
	            }

	            // sync key properties
                if (keysNode)
	            for (var ij = 0; ij < dataObject.keyProperties.length; ij++) {
	                for (var k = 0; k < keysNode.childNodes.length; k++) {
	                    for (var ikk = 0; ikk < dataObject.dataProperties.length; ikk++) {
	                        if (dataObject.keyProperties[ij].keyPropertyName.toLowerCase() == dataObject.dataProperties[ikk].propertyName.toLowerCase()) {
	                            if (keysNode.childNodes[k].raw.text.toLowerCase() == dataObject.dataProperties[ikk].columnName.toLowerCase()) {
	                                keysNode.childNodes[k].raw.text = dataObject.keyProperties[ij].keyPropertyName;
	                                keysNode.childNodes[k].raw.nhproperty.propertyName = dataObject.keyProperties[ij].keyPropertyName;
	                                keysNode.childNodes[k].raw.nhproperty.isHidden = dataObject.keyProperties[ij].isHidden;
	                                ij++;
	                                break;
	                            }
	                        }
	                    }
	                    break;
	                }
	                if (ij < dataObject.keyProperties.length) {
	                    for (var ijj = 0; ijj < propertiesNode.childNodes.length; ijj++) {
	                        var nodeText = dataObject.keyProperties[ij].keyPropertyName;
	                        if (propertiesNode.childNodes[ijj].raw.text.toLowerCase() == nodeText.toLowerCase()) {
	                            var properties = propertiesNode.childNodes[ijj].raw.nhproperty;
	                            properties.propertyName = nodeText;
	                            //properties.keyType = 'assigned';
	                            //properties.nullable = false;

                                keysNode.appendChild({	                            
	                                text: nodeText,
	                                type: "keyProperty",
	                                leaf: true,
	                                iconCls: 'treeKey',
	                                hidden: false,
	                                nhproperty: properties
	                            });
	                            //newKeyNode.iconCls = 'treeKey';
	                            propertiesNode.removeChild(propertiesNode.childNodes[ijj], true);
	                            ijj--;

	                            //if (newKeyNode)
	                                //keysNode.childNodes.push(newKeyNode);

	                            break;
	                        }
	                    }
	                }
	            }

	            // sync relationships
	            for (var kj = 0; kj < dataObject.dataRelationships.length; kj++) {
	                
	                var mapArray = new Array();
	                for (var kjj = 0; kjj < dataObject.dataRelationships[kj].propertyMaps.length; kjj++) {
	                    var mapItem = new Array();
	                    mapItem['dataPropertyName'] = dataObject.dataRelationships[kj].propertyMaps[kjj].dataPropertyName;
	                    mapItem['relatedPropertyName'] = dataObject.dataRelationships[kj].propertyMaps[kjj].relatedPropertyName;
	                    mapArray.push(mapItem);
	                }

	                relationshipsNode.appendChild({
	                    text: dataObject.dataRelationships[kj].relationshipName,
	                    type: 'relationship',
	                    leaf: true,
	                    iconCls: 'treeRelation',
	                    relatedObjMap: null,
	                    objectName: dataObjectNode.raw.text,
	                    relatedObjectName: dataObject.dataRelationships[kj].relatedObjectName,
	                    relationshipType: relationTypeStr[dataObject.dataRelationships[kj].relationshipType],
	                    relationshipTypeIndex: dataObject.dataRelationships[kj].relationshipType,
	                    propertyMap: mapArray
	                });
	                
                    //newNode.iconCls = 'treeRelation';
	                //newNode.data.propertyMap = mapArray;
	                relationshipsNode.expanded = true;
	                
	            }
	        }
	    }
	    ijk++;
	}
};		

function setTableNames () {
    // populate selected tables		
    var dbDict = AM.view.nhibernate.dbDict.value;	
	var selectTableNames = new Array();

	for (var i = 0; i < dbDict.dataObjects.length; i++) {
		var tableName = (dbDict.dataObjects[i].tableName ? dbDict.dataObjects[i].tableName : dbDict.dataObjects[i]);
		selectTableNames.push(tableName);
	}

	return selectTableNames;
};

function showTree(scopeName, appName) {
    //dbObjectsTree, dbInfo, dbDict, 
    var dbObjectsTree = Ext.widget('nhibernatetree');
    var dbInfo = AM.view.nhibernate.dbInfo.value;
    var dbDict = AM.view.nhibernate.dbDict.value;

	var selectTableNames = setTableNames();
	var connStr = dbDict.ConnectionString;
	if (!connStr) {
		showDialog(400, 100, 'Warning', 'Please save the Data Objects tree first.', Ext.Msg.OK, null);
	}

	var connStrParts = connStr.split(';');
	dbInfo = {};
	var provider = dbDict.Provider.toUpperCase();

	dbInfo.dbName = dbDict.SchemaName;
	if (!dbInfo.dbUserName)
		for (var i = 0; i < connStrParts.length; i++) {
			var pair = connStrParts[i].split('=');
			switch (pair[0].toUpperCase()) {
				case 'DATA SOURCE':
					if (provider.indexOf('MSSQL') > -1) {
						var dsValue = pair[1].split('\\');
						dbInfo.dbServer = (dsValue[0].toLowerCase() == '.' ? 'localhost' : dsValue[0]);
						dbInfo.dbInstance = dsValue[1];
						dbInfo.portNumber = 1433;
						dbInfo.serName = '';
					}
					else if (provider.indexOf('MYSQL') > -1) {
						dbInfo.dbServer = (pair[1].toLowerCase() == '.' ? 'localhost' : pair[1]);
						dbInfo.portNumber = 3306;
					}
					else if (provider.indexOf('ORACLE') > -1) {
						var dsStr = connStrParts[i].substring(12, connStrParts[i].length);
						var dsValue = dsStr.split('=');
						for (var j = 0; j < dsValue.length; j++) {
							dsValue[j] = dsValue[j].substring(dsValue[j].indexOf('(') + 1, dsValue[j].length);
							switch (dsValue[j].toUpperCase()) {
								case 'HOST':
									var server = dsValue[j + 1];
									var port = dsValue[j + 2];
									var index = server.indexOf(')');
									server = server.substring(0, index);
									dbInfo.portNumber = port.substring(0, 4);
									dbInfo.dbServer = (server.toLowerCase() == '.' ? 'localhost' : server);
									break;
								case 'SERVICE_NAME':
									var sername = dsValue[j + 1];
									index = sername.indexOf(')');
									dbInfo.dbInstance = sername.substring(0, index);
									dbInfo.serName = 'SERVICE_NAME';
									break;
								case 'SID':
									var sername = dsValue[j + 1];
									index = sername.indexOf(')');
									dbInfo.dbInstance = sername.substring(0, index);
									dbInfo.serName = 'SID';
									break;
							}
						}
					}
					break;
				case 'INITIAL CATALOG':
					dbInfo.dbName = pair[1];
					break;
				case 'USER ID':
					dbInfo.dbUserName = pair[1];
					break;
				case 'PASSWORD':
					dbInfo.dbPassword = pair[1];
					break;
			}
		}

    var treeModel = Ext.widget('nhTreeModel');
    treeModel.proxy.url = 'AdapterManager/DBObjects';
    treeModel.proxy.type = 'ajax';
    treeModel.proxy.timeout = 600000;
    treeModel.proxy.actionMethods.read = 'POST';
    treeModel.proxy.reader.type = 'json';
    
	var rootNode = dbObjectsTree.getRootNode();

	if (dbDict.Provider) {
	    dbObjectsTree.getStore().load({
            params: {
                scope: scopeName,
		        app: appName,
		        dbProvider: dbDict.Provider,
		        dbServer: dbInfo.dbServer,
		        dbInstance: dbInfo.dbInstance,
		        dbName: dbInfo.dbName,
		        dbSchema: dbDict.SchemaName,
		        dbUserName: dbInfo.dbUserName,
		        dbPassword: dbInfo.dbPassword,
		        portNumber: dbInfo.portNumber,
		        tableNames: selectTableNames,
		        serName: dbInfo.serName
            }
           }
        );
		loadTree(rootNode);		
	}

	dbObjectsTree.view.refresh();

	Ext.Ajax.request({
		url: 'AdapterManager/TableNames',
		method: 'POST',
		params: {
			scope: scopeName,
			app: appName,
			dbProvider: dbDict.Provider,
			dbServer: dbInfo.dbServer,
			dbInstance: dbInfo.dbInstance,
			dbName: dbInfo.dbName,
			dbSchema: dbDict.SchemaName,
			dbUserName: dbInfo.dbUserName,
			dbPassword: dbInfo.dbPassword,
			portNumber: dbInfo.portNumber,
			serName: dbInfo.serName
		},
		success: function (response, request) {
			AM.view.nhibernate.dbTableNames = Ext.JSON.decode(response.responseText);
		},
		failure: function (f, a) {
			if (a.response)
				showDialog(500, 400, 'Error', a.response.responseText, Ext.Msg.OK, null);
		}
    });

    AM.view.nhibernate.dbInfo.value = dbInfo;    

};

function setAvailTables (dbObjectsTree) {
	var availTableName = new Array();

	if (dbObjectsTree.disabled) {
		for (var i = 0; i < dbTableNames.items.length; i++) {
			var tableName = dbTableNames.items[i];
			availTableName.push(tableName);
		}
	}
	else {
		var rootNode = dbObjectsTree.getRootNode();
		if (dbTableNames.items) {
			for (var i = 0; i < dbTableNames.items.length; i++) {
				availTableName.push(dbTableNames.items[i]);
			}
		}

		if (!dbObjectsTree.disabled) {
			for (var j = 0; j < availTableName.length; j++)
				for (var i = 0; i < rootNode.childNodes.length; i++) {
					if (rootNode.childNodes[i].data.property.tableName.toLowerCase() == availTableName[j].toLowerCase()) {
						found = true;
						availTableName.splice(j, 1);
						j--;
						break;
					}
				}
		}
	}
	return availTableName;
};

function setSelectTables (dbObjectsTree) {
	var selectTableNames = new Array();

	if (!dbObjectsTree.disabled) {
		var rootNode = dbObjectsTree.getRootNode();
		for (var i = 0; i < rootNode.childNodes.length; i++) {
			var nodeText = rootNode.childNodes[i].data.property.tableName;
			selectTableNames.push([nodeText, nodeText]);
		}
	}

	return selectTableNames;
};