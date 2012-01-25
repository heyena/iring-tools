
Ext.define('AM.view.nhibernate.DataObjectPanel', {
    extend: 'Ext.panel.Panel',
    alias: 'widget.dataobjectpane',
    layout: 'border',
    frame: false,
    border: false,
    contextName: null,
    endpoint: null,
    items: [
                { xtype: 'nhibernatetreepanel',  region: 'west' },
                { xtype: 'editorpanel', region: 'center' }
            ],

    initComponent: function () {
        var wizard = this;
        var scopeName = this.contextName;
        var appName = this.endpoint;
        var userTableNames;

        if (scopeName) {            

            var confEditor = {
                id: scopeName + '.' + appName + '.editor-panel'
            };

            var editorPanel = Ext.widget('editorpanel', confEditor);            

            Ext.Ajax.request({
                url: 'AdapterManager/DataType',
                method: 'GET',
                timeout: 6000000,
                success: function (response, request) {
                    var dataTypeName = Ext.JSON.decode(response.responseText);
                    AM.view.nhibernate.dataTypes.value = new Array();
                    var i = 0;
                    while (!dataTypeName[i])
                        i++;
                    while (dataTypeName[i]) {
                        AM.view.nhibernate.dataTypes.value.push([i, dataTypeName[i]]);
                        i++;
                    }
                },
                failure: function (f, a) {
                    if (a.response)
                        showDialog(500, 400, 'Error', a.response.responseText, Ext.Msg.OK, null);
                }
            });

            Ext.EventManager.onWindowResize(this.doLayout, this);

            Ext.Ajax.request({
                url: 'AdapterManager/DBDictionary',
                method: 'POST',
                timeout: 6000000,
                params: {
                    scope: scopeName,
                    app: appName
                },
                success: function (response, request) {
                    AM.view.nhibernate.dbDict.value = Ext.JSON.decode(response.responseText);
                    var dbDict = AM.view.nhibernate.dbDict.value;

                    if (dbDict.ConnectionString) {
                        var base64 = AM.view.nhibernate.Utility;
                        AM.view.nhibernate.dbDict.value.ConnectionString = base64.decode(dbDict.ConnectionString);
                    }

                    var dbObjectsTree = Ext.widget('nhibernatetree');

                    if (dbDict.dataObjects.length > 0) {
                        // populate data source form
                        showTree(scopeName, appName);                        
                    }
                    else {
                        dbObjectsTree.disable();
                        setDsConfigPane(scopeName, appName);
                    }
                },
                failure: function (response, request) {
                    setDsConfigPane(scopeName, appName);
                }
            });


        }
        this.callParent(arguments);

    }
});

function showTree(scopeName, appName) {
    //dbObjectsTree, dbInfo, dbDict, 

    var conf = {
        id: scopeName + '.' + appName + '.nhtree',
        contextName: scopeName,
        endpoint: appName
    };
   
    var dbObjectsTree = Ext.widget('nhibernatetree', conf);
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
    treeModel.proxy.timeout = 6000000;
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

    dbObjectsTree.show();

    Ext.Ajax.request({
        url: 'AdapterManager/TableNames',
        method: 'POST',
        timeout: 6000000,
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

function setAvailTables(dbObjectsTree) {
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

function loadTree(rootNode) {
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