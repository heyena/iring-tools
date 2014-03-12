Ext.define('AM.controller.NHConfig', {
    extend: 'Ext.app.Controller',

    stores: [
        'DBProviderStore'
    ],

    views: [
        'directory.DirectoryPanel',
        'common.CenterPanel',
        'common.ContentPanel',
        'common.MessagePanel',
        'nhconfig.MainConfigPanel',
        'nhconfig.ObjectsTreePanel',
        'nhconfig.ConnectionPanel',
        'nhconfig.TableSelectionPanel',
        'nhconfig.ObjectConfigPanel',
        'nhconfig.KeySelectionPanel',
        'nhconfig.KeyConfigPanel',
        'nhconfig.PropertySelectionPanel',
        'nhconfig.PropertyConfigPanel',
        'nhconfig.RelationshipsPanel',
        'nhconfig.RelationshipConfigPanel'
    ],

    refs: [
    {
        ref: 'directoryTree',
        selector: 'viewport > directorypanel > directorytree'
    },
    {
        ref: 'contentPanel',
        selector: 'viewport > centerpanel > contentpanel'
    },
    {
        ref: 'searchPanel',
        selector: 'viewport > centerpanel > searchpanel'
    }],

    init: function (application) {
        var me = this;
        me.application.addEvents('nhconfig');

        this.control({
            "objectstreepanel": {
                itemclick: me.onTreeItemClick
            },
            "objectstreepanel button[action=editconnection]": {
                click: me.onEditConnection
            },
            "objectstreepanel button[action=save]": {
                click: me.onSave
            },
            "connectionpanel button[action=connect]": {
                click: me.onConnect
            },
            "tableselectionpanel button[action=apply]": {
                click: me.onApplyTableSelection
            },
            "objectconfigpanel button[action=apply]": {
                click: me.onApplyObjectConfig
            },
            "keyselectionpanel button[action=apply]": {
                click: me.onApplyKeySelection
            },
            "keyconfigpanel button[action=apply]": {
                click: me.onApplyKeyConfig
            },
            "propertyselectionpanel button[action=apply]": {
                click: me.onApplyPropertySelection
            },
            "propertyconfigpanel button[action=apply]": {
                click: me.onApplyPropertyConfig
            },
            "relationshipspanel button[action=apply]": {
                click: me.onApplyRelationships
            },
            "relationshipconfigpanel button[action=apply]": {
                click: me.onApplyRelationshipConfig
            }
        });

        application.on({
            nhconfig: {
                fn: this.onNHConfig,
                scope: this
            }
        });
    },

    dirNode: null,
    scope: '',
    app: '',

    onNHConfig: function (dirNode) {
        var me = this;

        me.dirNode = dirNode;
        me.scope = dirNode.parentNode.data.property['Internal Name'];
        me.app = dirNode.data.property['Internal Name'];

        var contentPanel = me.getContentPanel();
        var title = 'NHConfig.' + me.scope + '.' + me.app;
        var configPanel = contentPanel.down('mainconfigpanel[title=' + title + ']');

        if (!configPanel) {
            var configPanel = Ext.widget('mainconfigpanel', { title: title });
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

        var treePanel = configPanel.down('objectstreepanel');
        var params = treePanel.getStore().proxy.extraParams;
        params.scope = me.scope;
        params.app = me.app;

        configPanel.setLoading();
        treePanel.store.load({
            callback: function (records, operation, success) {
                configPanel.setLoading(false);

                if (success) {
                    var container = configPanel.down('#configcontainer');
                    var connInfo = treePanel.getRootNode().firstChild.raw.properties.connectionInfo;
                    var connPanel = container.down('connectionpanel');

                    connPanel.setRecord(connInfo);
                    treePanel.getRootNode().expand();
                }
                else {
                    //var resp = Ext.decode(request.response.responseText);
					var userMsg = operation.request.scope.reader.jsonData.message;
					var detailMsg = operation.request.scope.reader.jsonData.stackTraceDescription;
					var expPanel = Ext.widget('exceptionpanel', { title: 'Error Notification'});
					Ext.ComponentQuery.query('#expValue',expPanel)[0].setValue(userMsg);
					Ext.ComponentQuery.query('#expValue2',expPanel)[0].setValue(detailMsg);
                }
            }
        });
    },

    onEditConnection: function (button, e) {
        var me = this;

        var treePanel = button.up('objectstreepanel');
        var container = treePanel.up('mainconfigpanel').down('#configcontainer');
        var connInfo = treePanel.getRootNode().firstChild.raw.properties.connectionInfo;
        var connPanel = container.down('connectionpanel');

        connPanel.setRecord(connInfo);
        container.getLayout().setActiveItem(connPanel);
    },

    onTreeItemClick: function (dataview, record, item, index, e, eOpts) {
        var me = this;
        var treePanel = dataview.up('objectstreepanel');
        var container = treePanel.up('mainconfigpanel').down('#configcontainer');
        var nodeType = record.raw.type.toLowerCase();

        switch (nodeType) {
            case 'dataobjects':
                var tableSelPanel = container.down('tableselectionpanel');
                tableSelPanel.setRecord(record);
                container.getLayout().setActiveItem(tableSelPanel);
                break;
            case 'dataobject':
                var objectConfigPanel = container.down('objectconfigpanel');
                objectConfigPanel.setRecord(record.raw.properties);
                container.getLayout().setActiveItem(objectConfigPanel);
                break;
            case 'keys':
                var keySelPanel = container.down('keyselectionpanel');
                keySelPanel.setRecord(record);
                container.getLayout().setActiveItem(keySelPanel);
                break;
            case 'keyproperty':
                var keyConfigPanel = container.down('keyconfigpanel');
                keyConfigPanel.setRecord(record.raw.properties);
                container.getLayout().setActiveItem(keyConfigPanel);
                break;
            case 'properties':
                var propSelPanel = container.down('propertyselectionpanel');
                propSelPanel.setRecord(record);
                container.getLayout().setActiveItem(propSelPanel);
                break;
            case 'dataproperty':
                var propConfigPanel = container.down('propertyconfigpanel');
                propConfigPanel.setRecord(record.raw.properties);
                container.getLayout().setActiveItem(propConfigPanel);
                break;
            case 'relationships':
                var data = [];
                Ext.each(record.childNodes, function (node) {
                    data.push({ name: node.data.text });
                });

                var relsPanel = container.down('relationshipspanel');
                relsPanel.setRecord(data);
                container.getLayout().setActiveItem(relsPanel);
                break;
            case 'relationship':
                var relConfigPanel = container.down('relationshipconfigpanel');
                relConfigPanel.setRecord(record.raw.properties, record.parentNode.parentNode);
                container.getLayout().setActiveItem(relConfigPanel);
                break;
        }
    },

    onConnect: function (button, e) {
        var me = this;
        var configPanel = button.up('mainconfigpanel');
        var objectsTree = configPanel.down('objectstreepanel');
        var form = configPanel.down('connectionpanel').getForm();

        form.findField('scope').setValue(me.scope);
        form.findField('app').setValue(me.app);

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
					var expPanel = Ext.widget('exceptionpanel', { title: 'Error Notification'});
					Ext.ComponentQuery.query('#expValue',expPanel)[0].setValue(userMsg);
					Ext.ComponentQuery.query('#expValue2',expPanel)[0].setValue(detailMsg);
                }
            });
        }
    },

    onApplyTableSelection: function (button, e) {
        var panel = button.up('tableselectionpanel');
        var configPanel = panel.up('mainconfigpanel');
        var treePanel = configPanel.down('objectstreepanel');
        var params = treePanel.getStore().proxy.extraParams;
        var values = configPanel.down('connectionpanel').getForm().getValues();

        for (var field in values) {
            params[field] = values[field];
        }

        var tables = panel.getForm().findField('selectedTables').getValue();
        params.selectedTables = tables.join(',');

        this.reload(configPanel);
    },

    onApplyObjectConfig: function (button, e) {
        var panel = button.up('objectconfigpanel');
        var treePanel = panel.up('mainconfigpanel').down('objectstreepanel');

        var values = panel.getForm().getValues();
        var props = treePanel.getSelectionModel().getLastSelected().raw.properties;

        for (var field in values) {
            props[field] = values[field];
        }
    },

    onApplyKeySelection: function (button, e) {
        var panel = button.up('keyselectionpanel');
        var treePanel = panel.up('mainconfigpanel').down('objectstreepanel');

        var keysNode = treePanel.getSelectionModel().getLastSelected();
        var propsNode = keysNode.parentNode.findChild('text', 'Properties');

        var dataProps = keysNode.parentNode.raw.properties.dataProperties;
        var keys = panel.getForm().findField('selectedKeys').getValue();

        keysNode.removeAll();

        Ext.each(keys, function (key) {
            Ext.each(dataProps, function (dataProp) {
                if (dataProp.columnName === key) {
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
        });
    },

    onApplyKeyConfig: function (button, e) {
        var panel = button.up('keyconfigpanel');
        var treePanel = panel.up('mainconfigpanel').down('objectstreepanel');

        var values = panel.getForm().getValues();
        var props = treePanel.getSelectionModel().getLastSelected().raw.properties;

        for (var field in values) {
            props[field] = values[field];
        }
    },

    onApplyPropertySelection: function (button, e) {
        var panel = button.up('propertyselectionpanel');
        var treePanel = panel.up('mainconfigpanel').down('objectstreepanel');

        var propsNode = treePanel.getSelectionModel().getLastSelected();
        var props = panel.getForm().findField('selectedProperties').getValue();
        var dataProps = propsNode.parentNode.raw.properties.dataProperties;

        propsNode.removeAll();

        Ext.each(props, function (prop) {
            Ext.each(dataProps, function (dataProp) {
                if (dataProp.columnName === prop) {
                    // add to properties node
                    propsNode.appendChild({
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
        var panel = button.up('propertyconfigpanel');
        var treePanel = panel.up('mainconfigpanel').down('objectstreepanel');

        var values = panel.getForm().getValues();
        var props = treePanel.getSelectionModel().getLastSelected().raw.properties;

        for (var field in values) {
            props[field] = values[field];
        }
    },

    onApplyRelationships: function (button, e) {
        var panel = button.up('relationshipspanel');
        var grid = panel.down('grid');

        var treePanel = panel.up('mainconfigpanel').down('objectstreepanel');
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
        var panel = button.up('relationshipconfigpanel');
        var treePanel = panel.up('mainconfigpanel').down('objectstreepanel');
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
        var treePanel = button.up('objectstreepanel');
        var configPanel = treePanel.up('mainconfigpanel');
        var objectsNode = treePanel.getRootNode().firstChild;

        configPanel.setLoading();

        var connInfo = configPanel.down('connectionpanel').getForm().getValues();
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
            description: null
        };

        var valid = true;

        Ext.each(objectsNode.childNodes, function (objectNode) {
            var objProps = objectNode.raw.properties;
            var keyNodes = objectNode.findChild('text', 'Keys').childNodes;
            var propNodes = objectNode.findChild('text', 'Properties').childNodes;
            var relNodes = objectNode.findChild('text', 'Relationships').childNodes;

            var dataObject = {
                tableName: objProps.tableName,
                objectNamespace: objProps.objectNamespace,
                objectName: objProps.objectName,
                keyDelimeter: objProps.keyDelimiter,
                keyProperties: [],
                dataProperties: [],
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

                dataObject.dataProperties.push({
                    columnName: props.columnName,
                    propertyName: props.propertyName,
                    dataType: props.dataType,
                    dataLength: props.dataLength,
                    isNullable: props.isNullable,
                    keyType: keyType,
                    showOnIndex: props.showOnIndex,
                    numberOfDecimals: props.numberOfDecimals,
                    isReadOnly: props.isReadOnly,
                    showOnSearch: props.showOnSearch,
                    isHidden: props.isHidden,
                    isVirtual: props.isVirtual
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
                    app: me.app
                },
                jsonData: dbDictionary,
                success: function (response, request) {
                    configPanel.setLoading(false);

                    var result = Ext.decode(response.responseText);
                    if (!result.success) {
                        var userMsg = result['message'];
						var detailMsg = result['stackTraceDescription'];
						var expPanel = Ext.widget('exceptionpanel', { title: 'Error Notification'});
						Ext.ComponentQuery.query('#expValue',expPanel)[0].setValue(userMsg);
						Ext.ComponentQuery.query('#expValue2',expPanel)[0].setValue(detailMsg);
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
