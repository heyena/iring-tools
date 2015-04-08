Ext.define('AM.controller.SQLConfig', {
    extend: 'Ext.app.Controller',

    stores: ['DBProviderStore'],


    views: [
        'directory.DirectoryPanel',
        'common.CenterPanel',
        'common.ContentPanel',
        'common.MessagePanel',
        'sqlconfig.SqlMainConfigPanel',
        'sqlconfig.SqlObjectsTreePanel',
        'sqlconfig.SqlConnectionPanel',
        'sqlconfig.SqlTableSelectionPanel',
        'sqlconfig.SqlObjectConfigPanel',
        'sqlconfig.SqlKeySelectionPanel',
        'sqlconfig.SqlKeyConfigPanel',
        'sqlconfig.SqlPropertySelectionPanel',
        'sqlconfig.SqlPropertyConfigPanel',
        'sqlconfig.SqlRelationshipsPanel',
        'sqlconfig.SqlRelationshipConfigPanel',
        'sqlconfig.SqlExtenConfigPanel',
         'sqlconfig.SqlExtensionPanel'

    ],

    refs: [{
        ref: 'directoryTree',
        selector: 'viewport > directorypanel > directorytree'
    }, {
        ref: 'contentPanel',
        selector: 'viewport > centerpanel > contentpanel'
    }, {
        ref: 'searchPanel',
        selector: 'viewport > centerpanel > searchpanel'
    }],

    init: function (application) {
        var me = this;
        me.application.addEvents('sqlconfig');
        var store = Ext.create('Ext.data.Store', {
            fields: ['name'],
            data: [{
                name: ''
            }]
        });
        this.control({
            "sqlobjectstreepanel": {
                itemclick: me.onTreeItemClick
                // itemcontextmenu: me.showContextMenu
            },
            "sqlobjectstreepanel button[action=editconnection]": {
                click: me.onEditConnection
            },
            "sqlobjectstreepanel button[action=save]": {
                click: me.onSave
            },
            "sqlconnectionpanel button[action=connect]": {
                click: me.onConnect
            },
            "sqltableselectionpanel button[action=apply]": {
                click: me.onApplyTableSelection
            },
            "sqlobjectconfigpanel button[action=apply]": {
                click: me.onApplyObjectConfig
            },
            "sqlkeyselectionpanel button[action=apply]": {
                click: me.onApplyKeySelection
            },
            "sqlkeyconfigpanel button[action=apply]": {
                click: me.onApplyKeyConfig
            },
            "sqlpropertyselectionpanel button[action=apply]": {
                click: me.onApplyPropertySelection
            },
            "sqlpropertyconfigpanel button[action=apply]": {
                click: me.onApplyPropertyConfig
            },
            "sqlrelationshipspanel button[action=apply]": {
                click: me.onApplyRelationships
            },
            "sqlrelationshipconfigpanel button[action=apply]": {
                click: me.onApplyRelationshipConfig
            },

            "sqlextensionpanel button[action=apply]": {
                click: me.onApplyExtension
            },
            "sqlextenconfigpanel button[action=apply]": {
                click: me.onApplyExtensionConfig
            }

        });

        application.on({
            sqlconfig: {
                fn: this.onSQLConfig,
                scope: this
            }
        });
    },

    dirNode: null,
    scope: '',
    app: '',
    dataview: null,
    applicationId:'',

    onSQLConfig: function (dirNode) {
        var me = this;
        me.dirNode = dirNode;
        var applicationInternalName = Ext.decode(dirNode.raw.record).internalName;
        var scopeInternalName = Ext.decode(dirNode.parentNode.raw.record).internalName;
        me.applicationId = Ext.decode(dirNode.raw.record).applicationId;
       // me.scope = dirNode.parentNode.data.property['Internal Name'];
        //me.app = dirNode.data.property['Internal Name'];


        me.scope = scopeInternalName;
        me.app = applicationInternalName;

        var contentPanel = me.getContentPanel();
        var title = 'SQLConfig.' + me.scope + '.' + me.app;
      //  var title = 'SQLConfig.' + scopeInternalName + '.' + applicationInternalName;
        var configPanel = contentPanel.down('sqlmainconfigpanel[title=' + title + ']');

        if (!configPanel) {
            var configPanel = Ext.widget('sqlmainconfigpanel', { title: title });
            contentPanel.add(configPanel);
            contentPanel.setActiveTab(configPanel);
            me.reload(configPanel);
        }
        else {
            contentPanel.setActiveTab(configPanel);
        }

        me.getSearchPanel().collapse();
    },

    reload: function (configPanel) {
        var me = this;
        var treePanel = configPanel.down('sqlobjectstreepanel');
        var params = treePanel.getStore().proxy.extraParams;
        params.scope = me.scope;
        params.app = me.app;
        params.applicationId = me.applicationId;
        configPanel.setLoading();
        treePanel.store.load({
            callback: function (records, operation, success) {
                configPanel.setLoading(false);
                if (success) {
                    var container = configPanel.down('#configcontainer');
                    var connInfo = treePanel.getRootNode().firstChild.raw.properties.connectionInfo;
                    var connPanel = container.down('sqlconnectionpanel');
                    connPanel.setRecord(connInfo);
                    treePanel.getRootNode().expand();
                }
                else {
                    //var resp = Ext.decode(request.response.responseText);
                    var userMsg = operation.request.scope.reader.jsonData.message;
                    var detailMsg = operation.request.scope.reader.jsonData.stackTraceDescription;
                    var expPanel = Ext.widget('exceptionpanel', { title: 'Error Notification' });
                    Ext.ComponentQuery.query('#expValue', expPanel)[0].setValue(userMsg);
                    Ext.ComponentQuery.query('#expValue2', expPanel)[0].setValue(detailMsg);
                }
            }
        });
    },

    onEditConnection: function (button, e) {
        var me = this;

        var treePanel = button.up('sqlobjectstreepanel');
        var container = treePanel.up('sqlmainconfigpanel').down('#configcontainer');
        var connInfo = treePanel.getRootNode().firstChild.raw.properties.connectionInfo;
        var connPanel = container.down('sqlconnectionpanel');

        connPanel.setRecord(connInfo);
        container.getLayout().setActiveItem(connPanel);
    },

    onTreeItemClick: function (dataview, record, item, index, e, eOpts) {
        var me = this;
        this.dataview = dataview;
        var treePanel = dataview.up('sqlobjectstreepanel');
        var container = treePanel.up('sqlmainconfigpanel').down('#configcontainer');
        var nodeType = record.raw.type.toLowerCase();
        //SqlObjectConfigPanel
        switch (nodeType) {
            case 'dataobjects':
                var tableSelPanel = container.down('sqltableselectionpanel');
                tableSelPanel.setRecord(record);
                container.getLayout().setActiveItem(tableSelPanel);
                break;
            case 'dataobject':
                var objectConfigPanel = container.down('sqlobjectconfigpanel');
                objectConfigPanel.setRecord(record);
                container.getLayout().setActiveItem(objectConfigPanel);
                break;
            case 'keys':
                var keySelPanel = container.down('sqlkeyselectionpanel');
                keySelPanel.setRecord(record, dataview);
                container.getLayout().setActiveItem(keySelPanel);
                break;
            case 'keyproperty':
                var keyConfigPanel = container.down('sqlkeyconfigpanel');
                keyConfigPanel.setRecord(record);
                container.getLayout().setActiveItem(keyConfigPanel);
                //this.reload(container);
                break;
            case 'properties':
                var propSelPanel = container.down('sqlpropertyselectionpanel');
                propSelPanel.setRecord(record);
                container.getLayout().setActiveItem(propSelPanel);
                break;

            case 'extension':
                var data = [];
                Ext.each(record.childNodes, function (node) {
                    data.push({
                        propertyName: node.data.text
                    });
                });
                var extensionPanel = container.down('sqlextensionpanel');
                extensionPanel.setRecord(data);
                container.getLayout().setActiveItem(extensionPanel);
                break;
            case 'dataproperty':
                var propConfigPanel = container.down('sqlpropertyconfigpanel');
                propConfigPanel.setRecord(record);
                container.getLayout().setActiveItem(propConfigPanel);
                break;

            case 'extensionproperty':

                var extenConfigPanel1 = container.down('sqlextenconfigpanel');
                extenConfigPanel1.setRecord(record);
                container.getLayout().setActiveItem(extenConfigPanel1);
                // extenConfigPanel1.form.reset();
                break;

            case 'relationships':
                var data = [];
                Ext.each(record.childNodes, function (node) {
                    data.push({ name: node.data.text });
                });

                var relsPanel = container.down('sqlrelationshipspanel');
                relsPanel.setRecord(data);
                container.getLayout().setActiveItem(relsPanel);
                break;
            case 'relationship':
                var relConfigPanel = container.down('sqlrelationshipconfigpanel');
                relConfigPanel.setRecord(record.raw.properties, record.parentNode.parentNode);
                container.getLayout().setActiveItem(relConfigPanel);
                break;
        }
    },



    onConnect: function (button, e) {
        var me = this;
        var configPanel = button.up('sqlmainconfigpanel');
        var objectsTree = configPanel.down('sqlobjectstreepanel');
        var form = configPanel.down('sqlconnectionpanel').getForm();

        form.findField('scope').setValue(me.scope);
        form.findField('app').setValue(me.app);
        form.findField('applicationId').setValue(me.applicationId);

        if (form.isValid()) {
            configPanel.setLoading();
            form.submit({
                success: function (form, action) {
                    configPanel.setLoading(false);
                    var tableNames = action.result.data;
                    var objectsNode = objectsTree.getRootNode().firstChild;
                    objectsNode.raw.properties['tableNames'] = tableNames;
                    objectsTree.fireEvent('itemclick', objectsTree.getView(), objectsNode);
                },
                failure: function (form, action) {
                    configPanel.setLoading(false);
                    var resp = Ext.decode(action.response.responseText);
                    var userMsg = resp['message'];
                    var detailMsg = resp['stackTraceDescription'];
                    var expPanel = Ext.widget('exceptionpanel', { title: 'Error Notification' });
                    Ext.ComponentQuery.query('#expValue', expPanel)[0].setValue(userMsg);
                    Ext.ComponentQuery.query('#expValue2', expPanel)[0].setValue(detailMsg);
                }
            });
        }
    },

    onApplyTableSelection: function (button, e) {
        var panel = button.up('sqltableselectionpanel');
        var configPanel = panel.up('sqlmainconfigpanel');
        var treePanel = configPanel.down('sqlobjectstreepanel');
        var params = treePanel.getStore().proxy.extraParams;
        var values = configPanel.down('sqlconnectionpanel').getForm().getValues();

        for (var field in values) {
            params[field] = values[field];
        }

        var tables = panel.getForm().findField('selectedTables').getValue();
        params.selectedTables = tables.join(',');

        this.reload(configPanel);
    },

    onApplyObjectConfig: function (button, e) {
        var panel = button.up('sqlobjectconfigpanel');
        var treePanel = panel.up('sqlmainconfigpanel').down('sqlobjectstreepanel');


        var values = panel.getForm().getValues();
        var props = treePanel.getSelectionModel().getLastSelected().raw.properties;

        for (var field in values) {
            props[field] = values[field];
        }
    },

    onApplyKeySelection: function (button, e) {
        var panel = button.up('sqlkeyselectionpanel');
        var treePanel = panel.up('sqlmainconfigpanel').down('sqlobjectstreepanel');

        var keysNode = treePanel.getSelectionModel().getLastSelected();
        var propsNode = keysNode.parentNode.findChild('text', 'Properties');

        var dataProps = keysNode.parentNode.raw.properties.dataProperties;
        var keys = panel.getForm().findField('selectedKeys').getValue();

        keysNode.removeAll();

        //hg
        var extensionProperties = [];
        var objectsNode = treePanel.getRootNode().firstChild;
        Ext.each(objectsNode.childNodes, function (objectNode) {
            var extNodes = objectNode.findChild('text', 'Extension').childNodes;

            Ext.each(extNodes, function (extNode, index) {
                var extVal = extNode.raw.properties;
                extensionProperties.push({
                    columnName: extVal.columnName,
                    propertyName: extVal.propertyName,
                    dataType: extVal.dataType,
                    dataLength: 1000,
                    isNullable: true,
                    keyType: 0,
                    precision: 0,
                    scale: 0,
                    definition: extVal.definition
                });
            });
        });

        Ext.each(keys, function (key) {
            Ext.each(dataProps, function (dataProp) {
                if (dataProp.propertyName === key) {
                    // add to keys node
                    keysNode.appendChild({
                        text: key,
                        type: 'keyProperty',
                        iconCls: 'treeKey',
                        leaf: true,
                        properties: dataProp
                    });

                    // update keytype in data record
                    dataProp.keyType = 'assigned';

                    // remove from selected properties node
                    var propNode = propsNode.findChild('text', key);
                    if (propNode != null) {
                        propsNode.removeChild(propNode);
                    }

                    return;
                }
            });

            //hg
            Ext.each(extensionProperties, function (extProp) {
                if (extProp.columnName === key) {
                    // add to keys node
                    keysNode.appendChild({
                        text: key,
                        type: 'keyProperty',
                        iconCls: 'treeKey',
                        leaf: true,
                        properties: extProp
                    });

                    // update keytype in data record
                    extProp.keyType = 'assigned';

                    // remove from selected properties node
                    var propNode = propsNode.findChild('text', key);
                    if (propNode != null) {
                        propsNode.removeChild(propNode);
                    }

                    return;
                }
            });
            //
        });
    },

    onApplyKeyConfig: function (button, e) {
        var panel = button.up('sqlkeyconfigpanel');
        var treePanel = panel.up('sqlmainconfigpanel').down('sqlobjectstreepanel');

        var values = panel.getForm().getValues();
        var props = treePanel.getSelectionModel().getLastSelected().raw.properties;

        if (values.propertyName == undefined || values.propertyName == "") {
            Ext.Msg.alert('Error', 'Please enter all values');
            return;
        }

        var keysNode = treePanel.getSelectionModel().getLastSelected();

        for (var field in values) {
            if (props['propertyName'] != values['propertyName']) {
                if (keysNode != null) {
                    keysNode.set("text", values['propertyName']);
                }
            }

            props[field] = values[field];
        }
    },

    onApplyExtension: function (button, e) {
        var panel = button.up('sqlextensionpanel');
        var treePanel = panel.up('sqlmainconfigpanel').down('sqlobjectstreepanel');
        var extnNode = treePanel.getSelectionModel().getLastSelected();
        var extnValues = panel.getForm().getValues();
        var grid = panel.down('grid');
        var datPropTemp;

        var exts = grid.store.getRange();



        // add new extensions
        Ext.each(exts, function (extn) {
            var extName = extn.data.propertyName;
            var extNewNode = extnNode.findChild('text', extName);

            if (extNewNode == null) {
                extnNode.appendChild({
                    text: extName,
                    type: "extensionProperty",
                    iconCls: "treeExtension",

                    leaf: true,
                    properties: {
                        columnName: extName,
                        propertyName: extName,
                        dataType: 11,
                        definition: '\'Give some meaningful definition here\'',
                        type: 'extension'

                    }
                });
            }
        });


        // remove deleted extensions
        for (var i = 0; i < extnNode.childNodes.length; i++) {
            var node = extnNode.childNodes[i];
            var found = false;

            for (var j = 0; j < exts.length; j++) {
                if (node.data.text === exts[j].data.propertyName) {
                    found = true;
                    break;
                }
            }

            if (!found) {
                extnNode.removeChild(node);
                i--;
            }
        }


    },


    onApplyExtensionConfig: function (button, e) {
        var panel = button.up('sqlextenconfigpanel');
        var treePanel = panel.up('sqlmainconfigpanel').down('sqlobjectstreepanel');

        var extnConfingValues = panel.getForm().getValues();

        if (extnConfingValues.propertyName == undefined || extnConfingValues.propertyName == "" || extnConfingValues.definition == undefined || extnConfingValues.definition == "") {
            Ext.Msg.alert('Error', 'Please enter all values');
            return;
        }

        var extenProps = treePanel.getSelectionModel().getLastSelected().raw.properties;
        var keysNode = treePanel.getSelectionModel().getLastSelected();

        for (var field in extnConfingValues) {
            if (extenProps['propertyName'] != extnConfingValues['propertyName']) {
                if (keysNode != null) {
                    keysNode.set("text", extnConfingValues['propertyName']);
                }
            }

            extenProps[field] = extnConfingValues[field];
        }
    },

    onApplyPropertySelection: function (button, e) {
        var panel = button.up('sqlpropertyselectionpanel');
        var treePanel = panel.up('sqlmainconfigpanel').down('sqlobjectstreepanel');
        var propsNode = treePanel.getSelectionModel().getLastSelected();
        var props = panel.getForm().findField('selectedProperties').getValue();
        var dataProps = propsNode.parentNode.raw.properties.dataProperties;
        propsNode.removeAll();

        Ext.each(props, function (prop) {
            Ext.each(dataProps, function (dataProp) {
                if (dataProp.propertyName === prop) {
                    propsNode.appendChild({
                    // add to properties node
                        text: prop,
                        type: 'dataProperty',
                        iconCls: 'treeProperty',
                        leaf: true,
                        properties: dataProp
                    });
                    return;
                }
            });
        });
    },

    onApplyPropertyConfig: function (button, e) {
        var panel = button.up('sqlpropertyconfigpanel');
        var treePanel = panel.up('sqlmainconfigpanel').down('sqlobjectstreepanel');

        var values = panel.getForm().getValues();

        if (values.propertyName == undefined || values.propertyName == "") {
            Ext.Msg.alert('Error', 'Please enter all values');
            return;
        }

        if (values.isNullable == "on") {
            values.isNullable = true;
        } else {
            values.isNullable = false;
        }

        var props = treePanel.getSelectionModel().getLastSelected().raw.properties;
        var keysNode = treePanel.getSelectionModel().getLastSelected();

        for (var field in values) {
            if (props['propertyName'] != values['propertyName']) {
                if (keysNode != null) {
                    keysNode.set("text", values['propertyName']);
                }
            }

            props[field] = values[field];
        }
    },

    onApplyRelationships: function (button, e) {
        var panel = button.up('sqlrelationshipspanel');
        var grid = panel.down('grid');

        var treePanel = panel.up('sqlmainconfigpanel').down('sqlobjectstreepanel');
        var relsNode = treePanel.getSelectionModel().getLastSelected();
        var rels = grid.store.getRange();

        // remove deleted relationships
        for (var i = 0; i < relsNode.childNodes.length; i++) {
            var node = relsNode.childNodes[i];
            var found = false;

            for (var j = 0; j < rels.length; j++) {
                if (node.data.text === rels[j].data.name) {
                    found = true;
                    break;
                }
            }

            if (!found) {
                relsNode.removeChild(node);
                i--;
            }
        }

        // add new relationships
        Ext.each(rels, function (rel) {
            var relName = rel.data.name;
            var relNode = relsNode.findChild('text', relName);

            if (relNode == null) {
                relsNode.appendChild({
                    text: relName,
                    type: "relationship",
                    iconCls: "relationship",
                    leaf: true,
                    properties: {
                        name: relName,
                        type: 'OneToOne',
                        sourceObject: relsNode.parentNode.data.text,
                        relatedObject: '',
                        propertyMaps: {}
                    }
                });
            }
        });
    },

    onApplyRelationshipConfig: function (button, e) {
        var panel = button.up('sqlrelationshipconfigpanel');
        var treePanel = panel.up('sqlmainconfigpanel').down('sqlobjectstreepanel');
        var relProps = treePanel.getSelectionModel().getLastSelected().raw.properties;

        var values = panel.getForm().getValues();
        delete values.sourceProperties;
        delete values.relatedProperties;

        for (var field in values) {
            relProps[field] = values[field];
        }

        var propertyMaps = panel.down('grid').getStore().getRange();
        relProps.propertyMaps = [];

        Ext.each(propertyMaps, function (map) {
            relProps.propertyMaps.push({
                dataPropertyName: map.data.dataPropertyName,
                relatedPropertyName: map.data.relatedPropertyName
            });
        });
    },

    onSave: function (button, e) {
        var me = this;
        var treePanel = button.up('sqlobjectstreepanel');
        var configPanel = treePanel.up('sqlmainconfigpanel');
        var objectsNode = treePanel.getRootNode().firstChild;
        configPanel.setLoading();
        var connInfo = configPanel.down('sqlconnectionpanel').getForm().getValues();
        var connStr = (connInfo.dbProvider.toLowerCase().indexOf('mssql') != -1)
            ? 'Data Source=' + connInfo.dbServer + '\\' + connInfo.dbInstance + ';' + 'Initial Catalog=' +
               connInfo.dbName + ';User ID=' + connInfo.dbUserName + ';Password=' + connInfo.dbPassword
            : 'Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=' + connInfo.dbServer + ')(PORT=' +
               connInfo.portNumber + '))(CONNECT_DATA=(' + connInfo.serName + '=' + connInfo.dbInstance +
              ')));User Id=' + connInfo.dbUserName + ';Password=' + connInfo.dbPassword;

        var dbDictionary = {
            provider: connInfo.dbProvider,
            connectionString: connStr,
            schemaName: connInfo.dbSchema,
            IdentityConfiguration: null,
            dataObjects: [],
            picklists: null,
            enableSearch: false,
            enableSummary: false,
            dataVersion: null,
            description: null,
            applicationId:me.applicationId

        };

        var valid = true;

        Ext.each(objectsNode.childNodes, function (objectNode) {
            var objProps = objectNode.raw.properties;
            var keyNodes = objectNode.findChild('text', 'Keys').childNodes;
            var propNodes = objectNode.findChild('text', 'Properties').childNodes;
            var relNodes = objectNode.findChild('text', 'Relationships').childNodes;
            var extNodes = objectNode.findChild('text', 'Extension').childNodes;


            var valName;

            var adVal = objProps.aliasDictionary instanceof Array;
            if (adVal) {
                if (objProps.aliasDictionary[0] != "" && objProps.aliasDictionary[0] != null && objProps.aliasDictionary[0] != undefined) {
                    var addVal = objProps.aliasDictionary[0].value instanceof Array;
                    if (addVal) {
                        valName = objProps.aliasDictionary[0];
                    } else {
                        valName = objProps.aliasDictionary[0];
                    }
                }
            } else {
                valName = objProps.aliasDictionary;
            }

            var aliasDictionary;
            if (valName != null && objProps.aliasDictionary != "" && objProps.aliasDictionary != null) {
                aliasDictionary = [{
                    "Key": "TABLE_NAME_IN",
                    "Value": valName
                }]
            }

            var dataObject = {
                tableName: objProps.tableName,
                aliasDictionary: aliasDictionary,
                objectNamespace: objProps.objectNamespace,
                objectName: objProps.objectName,
                keyDelimeter: objProps.keyDelimiter,
                keyProperties: [],
                dataProperties: [],
                extensionProperties: [],
                dataRelationships: [],
                description: objProps.description
            };

            if (keyNodes.length == 0) {
                valid = false;
                var message = 'Object ' + objProps.objectName + ' does not have any keys configured.';
                Ext.widget('messagepanel', { title: 'Configuration Error', msg: message });
                return;
            }

            Ext.each(keyNodes, function (keyNode) {
                var props = keyNode.raw.properties;

                dataObject.keyProperties.push({
                    keyPropertyName: props.propertyName
                });
            });

            Ext.each(keyNodes.concat(propNodes), function (node) {
                var props = node.raw.properties;

                var keyType = 0;
                if (node.raw.type === 'keyProperty') {
                    keyType = 1;
                }
                var valName;

                var adVal = props.aliasDictionary instanceof Array;
                if (adVal) {
                    if (props.aliasDictionary[0] != "" && props.aliasDictionary[0] != null && props.aliasDictionary[0] != undefined) {
                        var addVal = props.aliasDictionary[0].value instanceof Array;
                        if (addVal) {
                            valName = props.aliasDictionary[0].value[0].value;
                        } else {
                            valName = props.aliasDictionary[0].value;
                        }
                    }
                } else {
                    valName = props.aliasDictionary;
                }
                var aliasDictionary;

                if (valName != null && props.aliasDictionary != "" && props.aliasDictionary != null) {
                    aliasDictionary = [{
                        "Key": "COLUMN_NAME_IN",
                        "Value": valName
                    }]
                }
                if (props.isNullable == "on") {
                    props.isNullable = true;
                }
                else {
                    props.isNullable = false;

                }
                dataObject.dataProperties.push({
                    columnName: props.columnName,
                    propertyName: props.propertyName,
                    aliasDictionary: aliasDictionary,
                    dataType: props.dataType,
                    dataLength: props.dataLength,
                    isNullable: props.isNullable,
                    keyType: keyType,
                    showOnIndex: props.showOnIndex,
                    numberOfDecimals: props.numberOfDecimals,
                    isReadOnly: props.isReadOnly,
                    showOnSearch: props.showOnSearch,
                    isHidden: props.isHidden,
                    isVirtual: props.isVirtual,
                    precision: props.precision,
                    scale: props.scale
                });
            });


            Ext.each(relNodes, function (relNode) {
                var props = relNode.raw.properties;

                var relationship = {
                    propertyMaps: [],
                    relatedObjectName: props.relatedObject,
                    relationshipName: props.name,
                    relationshipType: (props.type === 'OneToOne') ? 0 : 1
                };

                Ext.each(relNode.raw.properties.propertyMaps, function (map) {
                    relationship.propertyMaps.push({
                        dataPropertyName: map.dataPropertyName,
                        relatedPropertyName: map.relatedPropertyName
                    });
                });

                dataObject.dataRelationships.push(relationship);
            });
            //extention save
            Ext.each(extNodes, function (extNode, index) {
                var extVal = extNode.raw.properties;


                dataObject.extensionProperties.push({
                    columnName: extVal.columnName,
                    propertyName: extVal.propertyName,
                    dataType: extVal.dataType,
                    dataLength: 1000,
                    isNullable: true,
                    keyType: 0,
                    precision: 0,
                    scale: 0,
                    definition: extVal.definition
                    //parameters: []
                });



            });

            //extention save
            dbDictionary.dataObjects.push(dataObject);
        });



        if (!valid) {
            configPanel.setLoading(false);
        }
        else {
            Ext.Ajax.request({
                url: 'NHibernate/SaveDBDictionary',
                method: 'POST',
                timeout: 300000,  // 5 min
                params: {
                    scope: me.scope,
                    app: me.app,
                    applicationId:me.applicationId
                },
                jsonData: dbDictionary,
                success: function (response, request) {
                    configPanel.setLoading(false);
                    var result = Ext.decode(response.responseText);
                    if (!result.success) {
                        var userMsg = result['message'];
                        var detailMsg = result['stackTraceDescription'];
                        var expPanel = Ext.widget('exceptionpanel', { title: 'Error Notification' });
                        Ext.ComponentQuery.query('#expValue', expPanel)[0].setValue(userMsg);
                        Ext.ComponentQuery.query('#expValue2', expPanel)[0].setValue(detailMsg);
                        return;
                    }
                    var dirTree = me.getDirectoryTree();
                    dirTree.store.proxy.extraParams.type = 'DataObjectsNode';
                    dirTree.setLoading();

                    dirTree.store.load({
                        node: me.dirNode,
                        callback: function (records, operation, success) {
                            dirTree.setLoading(false);

                            if (success) {
                                Ext.example.msg('Notification', 'Configuration saved successfully!');
                                me.dirNode.expand();
                                dirTree.view.refresh();
                            }
                            else {
                                var msg = operation.request.scope.reader.jsonData.message;
                                Ext.widget('messagepanel', { title: 'Refresh Error', msg: msg });
                            }
                        }
                    });
                },
                failure: function (response, request) {
                    configPanel.setLoading(false);
                    var msg = operation.request.scope.reader.jsonData.message;
                    Ext.widget('messagepanel', { title: 'Save Error', msg: response.responseText });
                }
            });
        }
    }
});
