Ext.ns('AdapterManager');

AdapterManager.sppidConfigWizard = Ext.extend(Ext.Container, {
    scope: null,
    app: null,
    datalayer: null,
    iconCls: 'tabsSPPID',
    border: false,
    frame: false,


    constructor: function (config) {
        config = config || {};

        var wizard = this;
        var scopeName = config.scope;
        var appName = config.app;
        var datalayer = config.datalayer;
        var dbDict;
        var dbInfo;
        var dbTableNames;
        var userTableNames;
        var dataObjectsPane = new Ext.Panel({
            layout: 'border',
            id: scopeName + '.' + appName + '.dataObjectsPane',
            frame: false,
            border: false,
            items: [{
                xtype: 'panel',
                name: 'data-objects-pane',
                region: 'west',
                minWidth: 240,
                width: 300,
                split: true,
                layout: 'border',
                bodyStyle: 'background:#fff',
                items: [{
                    xtype: 'treepanel',
                    border: false,
                    autoScroll: true,
                    animate: true,
                    region: 'center',
                    lines: true,
                    frame: false,
                    enableDD: false,
                    containerScroll: true,
                    rootVisible: true,
                    root: {
                        text: 'Commodites',
                        nodeType: 'async',
                        iconCls: 'folder'
                    },
                    loader: new Ext.tree.TreeLoader(),
                    tbar: new Ext.Toolbar({
                        items: [{
                            xtype: 'tbspacer',
                            width: 4
                        }, {
                            xtype: 'button',
                            icon: 'Content/img/16x16/view-refresh.png',
                            text: 'Reload',
                            tooltip: 'Reload Data Objects',
                            handler: function () {
                                var editPane = dataObjectsPane.items.items[1];
                                var items = editPane.items.items;

                                for (var i = 0; i < items.length; i++) {
                                    items[i].destroy();
                                    i--;
                                }

                                Ext.Ajax.request({
                                    url: 'SPPID/DBDictionary',
                                    method: 'POST',
                                    params: {
                                        scope: scopeName,
                                        app: appName
                                    },
                                    success: function (response, request) {
                                        dbDict = Ext.util.JSON.decode(response.responseText);
                                        if (dbDict.ConnectionString)
                                            dbDict.ConnectionString = Base64.decode(dbDict.ConnectionString);

                                        var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];

                                        if (dbDict.dataObjects.length > 0) {
                                            // populate data source form
                                            dbInfo = showTree(dbObjectsTree, dbInfo, dbDict, scopeName, appName, dataObjectsPane);
                                        }
                                        else {
                                            dbObjectsTree.disable();
                                            editPane = dataObjectsPane.items.items[1];
                                            if (!editPane) {
                                                var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
                                            }
                                            setDsConfigPane_SPPID(editPane, dbInfo, dbDict, scopeName, appName, dataObjectsPane,datalayer);
                                        }
                                    },
                                    failure: function (response, request) {
                                        editPane = dataObjectsPane.items.items[1];
                                        if (!editPane) {
                                            var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
                                        }
                                        setDsConfigPane_SPPID(editPane, dbInfo, dbDict, scopeName, appName, dataObjectsPane, datalayer);
                                        editPane.getLayout().setActiveItem(editPane.items.length - 1);
                                    }
                                });
                            }
                        }, {
                            xtype: 'tbspacer',
                            width: 4
                        }, {
                            xtype: 'button',
                            icon: 'Content/img/16x16/document-properties.png',
                            text: 'Edit Connection',
                            tooltip: 'Edit database connection',
                            handler: function () {
                                editPane = dataObjectsPane.items.items[1];
                                if (!editPane) {
                                    var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
                                }
                                dbTableNames = setDsConfigPane_SPPID(editPane, dbInfo, dbDict, scopeName, appName, dataObjectsPane, datalayer);
                            }
                        }, {
                            xtype: 'tbspacer',
                            width: 4
                        }, {
                            xtype: 'button',
                            icon: 'Content/img/16x16/document-save.png',
                            text: 'Save',
                            tooltip: 'Save the data objects tree to the back-end server',
                            formBind: true,
                            handler: function (button) {
                                editPane = dataObjectsPane.items.items[1];

                                if (!editPane) {
                                    var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
                                }

                                var dsConfigPane = editPane.items.map[scopeName + '.' + appName + '.dsconfigPane'];
                                var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];
                                var rootNode = dbObjectsTree.getRootNode();
                                var treeProperty = getTreeJson(dsConfigPane, rootNode, dbInfo, dbDict, dataTypes);

                                Ext.Ajax.request({
                                    url: null,
                                    method: 'POST',
                                    params: {
                                        scope: scopeName,
                                        app: appName,
                                        tree: JSON.stringify(treeProperty)
                                    },
                                    success: function (response, request) {
                                        var rtext = response.responseText;
                                        var error = 'SUCCESS = FALSE';
                                        var index = rtext.toUpperCase().indexOf(error);
                                        if (index == -1) {
                                            showDialog(400, 100, 'Saving Result', 'Configuration has been saved successfully.', Ext.Msg.OK, null);
                                            var navpanel = Ext.getCmp('nav-panel');
                                            navpanel.onReload();
                                        }
                                        else {
                                            var msg = rtext.substring(index + error.length + 2, rtext.length - 1);
                                            showDialog(400, 100, 'Saving Result - Error', msg, Ext.Msg.OK, null);
                                        }
                                    },
                                    failure: function (response, request) {
                                        showDialog(660, 300, 'Saving Result', 'An error has occurred while saving the configuration.', Ext.Msg.OK, null);
                                    }
                                });
                            }
                        }]
                    }),
                    listeners: {
                        click: function (node, e) {
                            if (node.isRoot) {
                                editPane = dataObjectsPane.items.items[1];
                                if (!editPane) {
                                    var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
                                }

                                setTablesSelectorPane(editPane, dbInfo, dbDict, scopeName, appName, dataObjectsPane, dbTableNames);
                                return;
                            }
                            else if (!node)
                                return;

                            var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
                            if (!editPane)
                                editPane = dataObjectsPane.items.items[1];

                            var nodeType = node.attributes.type;


                            if (!nodeType && node.attributes.attributes)
                                nodeType = node.attributes.attributes.type;


                            if (nodeType) {
                                editPane.show();
                                var editPaneLayout = editPane.getLayout();

                                switch (nodeType.toUpperCase()) {
                                    case 'DATAOBJECT':
                                        setDataObject(editPane, node, dbDict, dataObjectsPane, scopeName, appName);
                                        break;

                                    case 'KEYS':
                                        setKeysFolder(editPane, node, scopeName, appName);
                                        break;

                                    case 'KEYPROPERTY':
                                        setKeyProperty(editPane, node, scopeName, appName, dataTypes);
                                        break;

                                    case 'PROPERTIES':
                                        setPropertiesFolder(editPane, node, scopeName, appName);
                                        break;

                                    case 'DATAPROPERTY':
                                        setDataProperty(editPane, node, scopeName, appName, dataTypes);
                                        break;

                                    case 'RELATIONSHIPS':
                                        setRelations(editPane, node, scopeName, appName);
                                        break;

                                    case 'RELATIONSHIP':
                                        setRelationFields(editPane, node, scopeName, appName);
                                        break;
                                }
                            }
                            else {
                                editPane.hide();
                            }
                        }
                    }
                }]
            }, {
                xtype: 'panel',
                name: 'editor-panel',
                border: 1,
                frame: false,
                id: scopeName + '.' + appName + '.editor-panel',
                region: 'center',
                layout: 'card'
            }]
        });


        Ext.apply(this, {
            id: scopeName + '.' + appName + '.-sppid-config',
            title: 'P & ID Configuration - ' + scopeName + '.' + appName,
            closable: true,
            border: false,
            frame: true,
            layout: 'fit',
            items: [dataObjectsPane]
        });

        Ext.EventManager.onWindowResize(this.doLayout, this);

        Ext.Ajax.request({
            url: 'SPPID/GetConfig',
			method: 'POST',
			params: {
				scope: scopeName,
				app: appName
			},
			success: function (response, request) {
				dbDict = Ext.util.JSON.decode(response.responseText);
				if (dbDict.ConnectionString)
					dbDict.ConnectionString = Base64.decode(dbDict.ConnectionString);

				var tab = Ext.getCmp('content-panel');
				var rp = tab.items.map[scopeName + '.' + appName + '.-sppid-config'];
				var dataObjectsPane = rp.items.map[scopeName + '.' + appName + '.dataObjectsPane'];
				var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];

				//if (dbDict.dataObjects.length > 0) {
				if (dbDict.total > 0) {
				   
					// populate data source form
				    dbInfo = showTree(dbObjectsTree, dbInfo, dbDict, scopeName, appName, dataObjectsPane);
					var abcdd = 5;
				}
				else {
					dbObjectsTree.disable();
					editPane = dataObjectsPane.items.items[1];
					if (!editPane) {
						var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
					}
		setDsConfigPane_SPPID(editPane, dbInfo, dbDict, scopeName, appName, dataObjectsPane, datalayer);
				}
			},
			failure: function (response, request) {
				editPane = dataObjectsPane.items.items[1];
				if (!editPane) {
					var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
				}
				editPane.add(dsConfigPane);
				editPane.getLayout().setActiveItem(editPane.items.length - 1);
			}
		});

AdapterManager.sppidConfigWizard.superclass.constructor.apply(this, arguments);
	}
});


//*************************************
function setDsConfigFields (dsConfigPane, dbInfo, dbDict) {
	var dsConfigForm = dsConfigPane.getForm();
	var Provider = null;

	if (dbDict.Provider)
		Provider = dbDict.Provider.toUpperCase();

	var dbName = dsConfigForm.findField('dbName');
	var portNumber = dsConfigForm.findField('portNumber');
	var host = dsConfigForm.findField('host');
	var dbServer = dsConfigForm.findField('dbServer');
	var dbInstance = dsConfigForm.findField('dbInstance');
	var serviceName = dsConfigPane.items.items[10];
	var dbSchema = dsConfigForm.findField('dbSchema');
	var userName = dsConfigForm.findField('dbUserName');
	var password = dsConfigForm.findField('dbPassword');
	var dbProvider = dsConfigForm.findField('dbProvider');

	if (dbInfo) {
		if (Provider) {
			if (Provider.indexOf('ORACLE') > -1) {
				dbName.hide();
				dbServer.hide();
				dbInstance.hide();

				dbServer.setValue(dbInfo.dbServer);
				dbInstance.setValue(dbInfo.dbInstance);
				dbName.setValue(dbInfo.dbName);

				userName.setValue(dbInfo.dbUserName);
				password.setValue(dbInfo.dbPassword);
				dbProvider.setValue(dbDict.Provider);
				dbSchema.setValue(dbDict.SchemaName);

				host.setValue(dbInfo.dbServer);
				host.show();

				serviceName.show();
				creatRadioField(serviceName, serviceName.id, dbInfo.dbInstance, dbInfo.serName);

				portNumber.setValue(dbInfo.portNumber);
				portNumber.show();
			}
			else if (Provider.indexOf('MSSQL') > -1) {
				portNumber.hide();
				host.hide();
				serviceName.hide();

				dbServer.setValue(dbInfo.dbServer);
				dbServer.show();
				dbInstance.setValue(dbInfo.dbInstance);
				dbInstance.show();
				dbName.setValue(dbInfo.dbName);
				dbName.show();
				dbProvider.setValue(dbDict.Provider);
				host.setValue(dbInfo.dbServer);
				portNumber.setValue(dbInfo.portNumber);
				userName.setValue(dbInfo.dbUserName);
				password.setValue(dbInfo.dbPassword);
				dbSchema.setValue(dbDict.SchemaName);
			}
		}
	}
	else {
		dbServer.setValue('localhost');
		dbServer.show();
		dbInstance.setValue('default');
		dbInstance.show();
		dbSchema.setValue('dbo');
		portNumber.setValue('1433');
		portNumber.hide();

		dbName.setValue('');
		dbName.clearInvalid();
		dbName.show();
		userName.setValue('');
		password.setValue('');
		dbProvider.setValue('MsSql2008');
		host.setValue('');
		host.hide();
		serviceName.hide();

		userName.clearInvalid();
		password.clearInvalid();
	}
};

function changeConfigOracle (host, dbSchema, userName, password, serviceName) {
	host.setValue('');
	host.clearInvalid();

	host.show();

	dbSchema.setValue('');
	dbSchema.clearInvalid();

	userName.setValue('');
	userName.clearInvalid();

	password.setValue('');
	password.clearInvalid();
	serviceName.show();
	creatRadioField(serviceName, serviceName.id, '', '', 1);
}

function changeConfig (dbName, dbServer, dbInstance, dbSchema, userName, password) {
	dbName.setValue('');
	dbName.clearInvalid();
	dbName.show();

	dbServer.setValue('localhost');
	dbServer.show();

	dbInstance.setValue('default');
	dbInstance.show();

	dbSchema.setValue('dbo');

	userName.setValue('');
	userName.clearInvalid();

	password.setValue('');
	password.clearInvalid();
}

function setDsConfigPane_SPPID(editPane, dbInfo, dbDict, scopeName, appName, dataObjectsPane, datalayer) {
	if (editPane) {
		if (editPane.items.map[scopeName + '.' + appName + '.dsconfigPane']) {
			var dsConfigPanel = editPane.items.map[scopeName + '.' + appName + '.dsconfigPane'];
			
      if (dsConfigPanel) {				
				var panelIndex = editPane.items.indexOf(dsConfigPanel);
				editPane.getLayout().setActiveItem(panelIndex);
				return;
			}
		}

//		var providersStore = new Ext.data.JsonStore({
//			autoLoad: true,
//			autoDestroy: true,
//			url: 'AdapterManager/DBProviders',
//			root: 'items',
//			idProperty: 'Provider',
//			fields: [{
//				name: 'Provider'
//			}]
//});

var providersStore = new Ext.data.JsonStore({
    fields: ['Provider'],
    data: []
});


        providersStore.on('beforeload', function (treeLoader, node) {
            editPane.body.mask('Loading...', 'x-mask-loading');
        }, this);

        providersStore.on('load', function (treeLoader, node) {
            editPane.body.unmask();
        }, this);

         var dsConfigPane = new Ext.FormPanel({
                    labelWidth: 150,
                    id: scopeName + '.' + appName + '.dsconfigPane',
                    frame: false,
                    border: false,
                    autoScroll: true,
                    title: "Configure SP & ID Data Source",
                    bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
                    monitorValid: true,
                    defaults: { anchor: '100%', xtype: 'textfield', allowBlank: false },
                    items: [

            {
                xtype: 'fieldset',
                id: 'siteDatabase',
                title: "SP & ID Site Database Details",
                defaults: { anchor: '100%', xtype: 'textfield', allowBlank: false, width: 300 },
                items: [{
                    xtype: 'combo',
                    fieldLabel: 'Database Provider',
                    hiddenName: 'dbProvider',
                    allowBlank: false,
                    store: providersStore,
                    mode: 'local',
                    editable: false,
                    value: 'MsSql2008',
                    triggerAction: 'all',
                    displayField: 'Provider',
                    valueField: 'Provider'

                }, {
                    xtype: 'textfield',
                    name: 'dbServer',
                    fieldLabel: 'Database Server',
                    value: 'localhost',
                    allowBlank: false
                }, {
                    xtype: 'textfield',
                    name: 'portNumber',
                    fieldLabel: 'Port Number',
                    hidden: true,
                    value: '1521',
                    allowBlank: false
                }, {
                    name: 'dbInstance',
                    xtype: 'textfield',
                    fieldLabel: 'Database Instance',
                    value: 'default',
                    allowBlank: false
                }, {
                    xtype: 'textfield',
                    name: 'dbName',
                    fieldLabel: 'Database Name',
                    allowBlank: false
                }, {
                    xtype: 'textfield',
                    name: 'dbUserName',
                    fieldLabel: 'User Name',
                    allowBlank: false,
                    listeners: { 'change': function (field, newValue, oldValue) {
                        var dbProvider = dsConfigPane.getForm().findField('dbProvider').getValue().toUpperCase();
                        if (dbProvider.indexOf('ORACLE') > -1) {
                            var dbSchema = dsConfigPane.getForm().findField('dbSchema');
                            dbSchema.setValue(newValue);
                            dbSchema.show();
                        }
                    }
                    }
                }, {
                    xtype: 'textfield',
                    inputType: 'password',
                    name: 'dbPassword',
                    fieldLabel: 'Password',
                    allowBlank: false
                },  
           {
               xtype: 'checkbox',
               name: 'isPlantSchemaSame',
               fieldLabel: 'Site and Plant Databases are same',
               listeners: {
                   check: function (checkbox, checked) {
                       var siteDatabase = Ext.getCmp('siteDatabase').items
                       var plantDatabase = Ext.getCmp('plantDatabase').items
                       if (checked == true) {
                           for (var i = 0; i < siteDatabase.length - 1; i++) {
                               Ext.getCmp(plantDatabase.items[i].id).setValue(Ext.getCmp(siteDatabase.items[i].id).getValue());
                               Ext.getCmp(plantDatabase.items[i].id).disable(true);
                               //Ext.getCmp('dfd').items.items[i]Ext.getCmp(siteDatabase.items[5].id).getValue()
                           }
                       }
                       else {
                           for (var i = 0; i < siteDatabase.length - 1; i++) {
                               Ext.getCmp(plantDatabase.items[i].id).enable(true);
                               // Ext.getCmp(plantDatabase.items[i].id).setValue(Ext.getCmp(siteDatabase.items[i].id).originalValue);
                           }
                       }
                   }
               }


           }]
            },

            {
                xtype: 'fieldset',
                id: 'plantDatabase',
                title: " SP & ID Plant Database Details",
                defaults: { anchor: '100%', xtype: 'textfield', allowBlank: false, width: 300 },
                items: [{
                    xtype: 'combo',
                    fieldLabel: 'Database Provider',
                    hiddenName: 'dbplantProvider',
                    allowBlank: false,
                    store: providersStore,
                    mode: 'local',
                    editable: false,
                    value: 'MsSql2008',
                    triggerAction: 'all',
                    displayField: 'Provider',
                    valueField: 'Provider'

                }, {
                    xtype: 'textfield',
                    name: 'dbplantServer',
                    fieldLabel: 'Database Server',
                    value: 'localhost',
                    allowBlank: false
                }, {
                    xtype: 'textfield',
                    name: 'plantportNumber',
                    fieldLabel: 'Port Number',
                    hidden: true,
                    value: '1521',
                    allowBlank: false
                }, {
                    name: 'dbplantInstance',
                    xtype: 'textfield',
                    fieldLabel: 'Database Instance',
                    value: 'default',
                    allowBlank: false
                }, {
                    xtype: 'textfield',
                    name: 'dbplantName',
                    fieldLabel: 'Database Name',
                    allowBlank: false
                }, {
                    xtype: 'textfield',
                    name: 'dbplantUserName',
                    fieldLabel: 'User Name',
                    allowBlank: false,
                    listeners: { 'change': function (field, newValue, oldValue) {
                        var dbProvider = dsConfigPane.getForm().findField('dbProvider').getValue().toUpperCase();
                        if (dbProvider.indexOf('ORACLE') > -1) {
                            var dbSchema = dsConfigPane.getForm().findField('dbSchema');
                            dbSchema.setValue(newValue);
                            dbSchema.show();
                        }
                    }
                    }
                }, {
                    xtype: 'textfield',
                    inputType: 'password',
                    name: 'dbplantPassword',
                    fieldLabel: 'Password',
                    allowBlank: false
                }]
            },

                {
                xtype: 'fieldset',
                id: 'staggingDatabase',
                title: " Stagging Database Details",
                defaults: { anchor: '100%', xtype: 'textfield', allowBlank: false, width: 300 },
                items: [{
                    xtype: 'combo',
                    fieldLabel: 'Database Provider',
                    hiddenName: 'dbstageProvider',
                    allowBlank: false,
                    store: providersStore,
                    mode: 'local',
                    editable: false,
                    value: 'MsSql2008',
                    triggerAction: 'all',
                    displayField: 'Provider',
                    valueField: 'Provider'

                }, {
                    xtype: 'textfield',
                    name: 'dbstageServer',
                    fieldLabel: 'Database Server',
                    value: 'localhost',
                    allowBlank: false
                }, {
                    xtype: 'textfield',
                    name: 'stageportNumber',
                    fieldLabel: 'Port Number',
                    hidden: true,
                    value: '1521',
                    allowBlank: false
                }, {
                    name: 'dbstageInstance',
                    xtype: 'textfield',
                    fieldLabel: 'Database Instance',
                    value: 'default',
                    allowBlank: false
                }, {
                    xtype: 'textfield',
                    name: 'dbstageName',
                    fieldLabel: 'Database Name',
                    allowBlank: false
                }, {
                    xtype: 'textfield',
                    name: 'dbstageUserName',
                    fieldLabel: 'User Name',
                    allowBlank: false,
                    listeners: { 'change': function (field, newValue, oldValue) {
                        var dbProvider = dsConfigPane.getForm().findField('dbProvider').getValue().toUpperCase();
                        if (dbProvider.indexOf('ORACLE') > -1) {
                            var dbSchema = dsConfigPane.getForm().findField('dbSchema');
                            dbSchema.setValue(newValue);
                            dbSchema.show();
                        }
                    }
                    }
                }, {
                    xtype: 'textfield',
                    inputType: 'password',
                    name: 'dbstagePassword',
                    fieldLabel: 'Password',
                    allowBlank: false
                }]
            }],
            tbar : new Ext.Toolbar({
                items: [{
                    xtype: 'tbspacer',
                    width: 4
                }, {
                    xtype: 'tbbutton',
                    icon: 'Content/img/16x16/document-properties.png',
                    text: 'Connect',
                    tooltip: 'Connect',
                    handler: function (f) {
                        var dbProvider = dsConfigPane.getForm().findField('dbProvider').getValue().toUpperCase();
                        var dbName = dsConfigPane.getForm().findField('dbName');
                        var portNumber = dsConfigPane.getForm().findField('portNumber');
                        var dbServer = dsConfigPane.getForm().findField('dbServer');
                        var dbInstance = dsConfigPane.getForm().findField('dbInstance');
                        
                        

                        var dbplantProvider = dsConfigPane.getForm().findField('dbplantProvider').getValue().toUpperCase();
                        var dbplantName = dsConfigPane.getForm().findField('dbplantName').getValue();
                        var portplantNumber = dsConfigPane.getForm().findField('plantportNumber');
                        var dbplantServer = dsConfigPane.getForm().findField('dbplantServer').getValue();
                        var dbplantInstance = dsConfigPane.getForm().findField('dbplantInstance').getValue();
                        
                        
                        var dbplantPassword = dsConfigPane.getForm().findField('dbplantPassword').getValue();
                        var dbplantUsername = dsConfigPane.getForm().findField('dbplantUserName').getValue();

                        var servieName = '';
                        var serName = '';
                        var _datalayer = '';

                        dsConfigPane.getForm().submit({
                            url: 'SPPID/UpdateConfig',
                            timeout: 600000,
                            params: {
                                scope: scopeName,
                                app: appName,
                                serName: serName,
                                //                                dbplantUserName: dbplantUsername,
                                //                               dbplantPassword: dbplantPassword,
                                //                               dbplantServer: dbplantServer,
                                //                               dbplantInstance: dbplantInstance,
                                //                               dbplantName: dbplantName,
                                _datalayer: datalayer
                            },
                            success: function (f, a) {
                                dbTableNames = Ext.util.JSON.decode(a.response.responseText);
                                //var tab = Ext.getCmp('content-panel');
                                //var rp = tab.items.map[scopeName + '.' + appName + '.-nh-config'];
                                //var dataObjectsPane = rp.items.map[scopeName + '.' + appName + '.dataObjectsPane'];
                                var editPane = dataObjectsPane.items.map[scopeName + '.' + appName + '.editor-panel'];
                                var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];
                                dbObjectsTree.disable();
                                setTablesSelectorPane(editPane, dbInfo, dbDict, scopeName, appName, dataObjectsPane);
                            },
                            failure: function (f, a) {
                                if (a.response)
                                    showDialog(500, 400, 'Error', a.response.responseText, Ext.Msg.OK, null);
                                else {
                                    showDialog(400, 100, 'Warning', 'Please fill in every field in this form.', Ext.Msg.OK, null);
                                }
                            },
                            waitMsg: 'Loading ...'
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
                    handler: function (f) {
                        setDsConfigFields(dsConfigPane);
                    }
                }]
            })
                });

                if (dbInfo) {
                    setDsConfigFields(dsConfigPane);
                }
                editPane.add(dsConfigPane);
                var panelIndex = editPane.items.indexOf(dsConfigPane);
                editPane.getLayout().setActiveItem(panelIndex);
            }
        };

function setAvailTables(dbObjectsTree) {
    var availTableName = new Array();

   
    return availTableName;
}

function setSelectTables(dbObjectsTree) {
    var selectTableNames = new Array();

  

    return selectTableNames;
}
//***************************************************
     //------------------------------------------

//        var setDsConfigPane = function (editPane) {
//            if (editPane) {
//                if (editPane.items.map[scopeName + '.' + appName + '.dsconfigPane']) {
//                    var dsConfigPanel = editPane.items.map[scopeName + '.' + appName + '.dsconfigPane'];
//                    if (dsConfigPanel) {
//                        //						var panelIndex = editPane.items.indexOf(dsConfigPanel);
//                        //						editPane.getLayout().setActiveItem(panelIndex);
//                        //						dsConfigPanel.show();
//                        var panelIndex = editPane.items.indexOf(dsConfigPanel);
//                        editPane.getLayout().setActiveItem(panelIndex);
//                        return;
//                    }
//                }

//                var providersStore = new Ext.data.JsonStore({
//                    autoLoad: true,
//                    autoDestroy: true,
//                    url: null,
//                    root: 'items',
//                    idProperty: 'Provider',
//                    fields: [{
//                        name: 'Provider'
//                    }]
//                });

//                var dsConfigPane = new Ext.FormPanel({
//                    labelWidth: 150,
//                    id: scopeName + '.' + appName + '.dsconfigPane',
//                    frame: false,
//                    border: false,
//                    autoScroll: true,
//                    title: "Configure SP & ID Data Source",
//                    bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
//                    monitorValid: true,
//                    defaults: { anchor: '100%', xtype: 'textfield', allowBlank: false },
//                    items: [

////                    {
////                        xtype: 'fieldset',
////                        title: 'Configure SP & ID Data Source',
////                        labelSeparator: '',
////                        itemCls: 'form-title',
////                        labelWidth: 350,
////                        border: 0
////                    },
//            {
//                xtype: 'fieldset',
//                id: 'siteDatabase',
//                title: "SP & ID Site Database Details",
//                defaults: { anchor: '100%', xtype: 'textfield', allowBlank: false, width: 300 },
//                items: [{
//                    xtype: 'combo',
//                    fieldLabel: 'Database Provider',
//                    hiddenName: 'dbProvider',
//                    allowBlank: false,
//                    store: providersStore,
//                    mode: 'local',
//                    editable: false,
//                    value: 'MsSql2008',
//                    triggerAction: 'all',
//                    displayField: 'Provider',
//                    valueField: 'Provider'

//                }, {
//                    xtype: 'textfield',
//                    name: 'dbServer',
//                    fieldLabel: 'Database Server',
//                    value: 'localhost',
//                    allowBlank: false
//                }, {
//                    xtype: 'textfield',
//                    name: 'portNumber',
//                    fieldLabel: 'Port Number',
//                    hidden: true,
//                    value: '1521',
//                    allowBlank: false
//                }, {
//                    name: 'dbInstance',
//                    xtype: 'textfield',
//                    fieldLabel: 'Database Instance',
//                    value: 'default',
//                    allowBlank: false
//                }, {
//                    xtype: 'textfield',
//                    name: 'dbName',
//                    fieldLabel: 'Database Name',
//                    allowBlank: false
//                }, {
//                    xtype: 'textfield',
//                    name: 'dbUserName',
//                    fieldLabel: 'User Name',
//                    allowBlank: false,
//                    listeners: { 'change': function (field, newValue, oldValue) {
//                        var dbProvider = dsConfigPane.getForm().findField('dbProvider').getValue().toUpperCase();
//                        if (dbProvider.indexOf('ORACLE') > -1) {
//                            var dbSchema = dsConfigPane.getForm().findField('dbSchema');
//                            dbSchema.setValue(newValue);
//                            dbSchema.show();
//                        }
//                    }
//                    }
//                }, {
//                    xtype: 'textfield',
//                    inputType: 'password',
//                    name: 'dbPassword',
//                    fieldLabel: 'Password',
//                    allowBlank: false
//                },  
//           {
//               xtype: 'checkbox',
//               name: 'isPlantSchemaSame',
//               fieldLabel: 'Site and Plant Databases are same',
//               listeners: {
//                   check: function (checkbox, checked) {
//                       var siteDatabase = Ext.getCmp('siteDatabase').items
//                       var plantDatabase = Ext.getCmp('plantDatabase').items
//                       if (checked == true) {
//                           for (var i = 0; i < siteDatabase.length - 1; i++) {
//                               Ext.getCmp(plantDatabase.items[i].id).setValue(Ext.getCmp(siteDatabase.items[i].id).getValue());
//                               Ext.getCmp(plantDatabase.items[i].id).disable(true);
//                               //Ext.getCmp('dfd').items.items[i]Ext.getCmp(siteDatabase.items[5].id).getValue()
//                           }
//                       }
//                       else {
//                           for (var i = 0; i < siteDatabase.length - 1; i++) {
//                               Ext.getCmp(plantDatabase.items[i].id).enable(true);
//                               // Ext.getCmp(plantDatabase.items[i].id).setValue(Ext.getCmp(siteDatabase.items[i].id).originalValue);
//                           }
//                       }
//                   }
//               }


//           }]
//            },

//            {
//                xtype: 'fieldset',
//                id: 'plantDatabase',
//                title: " SP & ID Plant Database Details",
//                defaults: { anchor: '100%', xtype: 'textfield', allowBlank: false, width: 300 },
//                items: [{
//                    xtype: 'combo',
//                    fieldLabel: 'Database Provider',
//                    hiddenName: 'dbplantProvider',
//                    allowBlank: false,
//                    store: providersStore,
//                    mode: 'local',
//                    editable: false,
//                    value: 'MsSql2008',
//                    triggerAction: 'all',
//                    displayField: 'Provider',
//                    valueField: 'Provider'

//                }, {
//                    xtype: 'textfield',
//                    name: 'dbplantServer',
//                    fieldLabel: 'Database Server',
//                    value: 'localhost',
//                    allowBlank: false
//                }, {
//                    xtype: 'textfield',
//                    name: 'plantportNumber',
//                    fieldLabel: 'Port Number',
//                    hidden: true,
//                    value: '1521',
//                    allowBlank: false
//                }, {
//                    name: 'dbplantInstance',
//                    xtype: 'textfield',
//                    fieldLabel: 'Database Instance',
//                    value: 'default',
//                    allowBlank: false
//                }, {
//                    xtype: 'textfield',
//                    name: 'dbplantName',
//                    fieldLabel: 'Database Name',
//                    allowBlank: false
//                }, {
//                    xtype: 'textfield',
//                    name: 'dbplantUserName',
//                    fieldLabel: 'User Name',
//                    allowBlank: false,
//                    listeners: { 'change': function (field, newValue, oldValue) {
//                        var dbProvider = dsConfigPane.getForm().findField('dbProvider').getValue().toUpperCase();
//                        if (dbProvider.indexOf('ORACLE') > -1) {
//                            var dbSchema = dsConfigPane.getForm().findField('dbSchema');
//                            dbSchema.setValue(newValue);
//                            dbSchema.show();
//                        }
//                    }
//                    }
//                }, {
//                    xtype: 'textfield',
//                    inputType: 'password',
//                    name: 'dbplantPassword',
//                    fieldLabel: 'Password',
//                    allowBlank: false
//                }]
//            },

//                {
//                xtype: 'fieldset',
//                id: 'staggingDatabase',
//                title: " Stagging Database Details",
//                defaults: { anchor: '100%', xtype: 'textfield', allowBlank: false, width: 300 },
//                items: [{
//                    xtype: 'combo',
//                    fieldLabel: 'Database Provider',
//                    hiddenName: 'dbstageProvider',
//                    allowBlank: false,
//                    store: providersStore,
//                    mode: 'local',
//                    editable: false,
//                    value: 'MsSql2008',
//                    triggerAction: 'all',
//                    displayField: 'Provider',
//                    valueField: 'Provider'

//                }, {
//                    xtype: 'textfield',
//                    name: 'dbstageServer',
//                    fieldLabel: 'Database Server',
//                    value: 'localhost',
//                    allowBlank: false
//                }, {
//                    xtype: 'textfield',
//                    name: 'stageportNumber',
//                    fieldLabel: 'Port Number',
//                    hidden: true,
//                    value: '1521',
//                    allowBlank: false
//                }, {
//                    name: 'dbstageInstance',
//                    xtype: 'textfield',
//                    fieldLabel: 'Database Instance',
//                    value: 'default',
//                    allowBlank: false
//                }, {
//                    xtype: 'textfield',
//                    name: 'dbstageName',
//                    fieldLabel: 'Database Name',
//                    allowBlank: false
//                }, {
//                    xtype: 'textfield',
//                    name: 'dbstageUserName',
//                    fieldLabel: 'User Name',
//                    allowBlank: false,
//                    listeners: { 'change': function (field, newValue, oldValue) {
//                        var dbProvider = dsConfigPane.getForm().findField('dbProvider').getValue().toUpperCase();
//                        if (dbProvider.indexOf('ORACLE') > -1) {
//                            var dbSchema = dsConfigPane.getForm().findField('dbSchema');
//                            dbSchema.setValue(newValue);
//                            dbSchema.show();
//                        }
//                    }
//                    }
//                }, {
//                    xtype: 'textfield',
//                    inputType: 'password',
//                    name: 'dbstagePassword',
//                    fieldLabel: 'Password',
//                    allowBlank: false
//                }]
//            },
//            tbar = new Ext.Toolbar({
//                items: [{
//                    xtype: 'tbspacer',
//                    width: 4
//                }, {
//                    xtype: 'tbbutton',
//                    icon: 'Content/img/16x16/document-properties.png',
//                    text: 'Connect',
//                    tooltip: 'Connect',
//                    handler: function (f) {
//                        var dbProvider = dsConfigPane.getForm().findField('dbProvider').getValue().toUpperCase();
//                        var dbName = dsConfigPane.getForm().findField('dbName');
//                        var portNumber = dsConfigPane.getForm().findField('portNumber');
//                        var dbServer = dsConfigPane.getForm().findField('dbServer');
//                        var dbInstance = dsConfigPane.getForm().findField('dbInstance');
//                        
//                        

//                        var dbplantProvider = dsConfigPane.getForm().findField('dbplantProvider').getValue().toUpperCase();
//                        var dbplantName = dsConfigPane.getForm().findField('dbplantName').getValue();
//                        var portplantNumber = dsConfigPane.getForm().findField('plantportNumber');
//                        var dbplantServer = dsConfigPane.getForm().findField('dbplantServer').getValue();
//                        var dbplantInstance = dsConfigPane.getForm().findField('dbplantInstance').getValue();
//                        
//                        
//                        var dbplantPassword = dsConfigPane.getForm().findField('dbplantPassword').getValue();
//                        var dbplantUsername = dsConfigPane.getForm().findField('dbplantUserName').getValue();

//                        var servieName = '';
//                        var serName = '';
//                        var _datalayer = '';

//                        dsConfigPane.getForm().submit({
//                            url: 'SPPID/UpdateConfig',
//                            timeout: 600000,
//                            params: {
//                                scope: scopeName,
//                                app: appName,
//                                serName: serName,
//                                //                                dbplantUserName: dbplantUsername,
//                                //                               dbplantPassword: dbplantPassword,
//                                //                               dbplantServer: dbplantServer,
//                                //                               dbplantInstance: dbplantInstance,
//                                //                               dbplantName: dbplantName,
//                                _datalayer: datalayer
//                            },
//                            success: function (f, a) {
//                                dbTableNames = Ext.util.JSON.decode(a.response.responseText);
//                                //var tab = Ext.getCmp('content-panel');
//                                //var rp = tab.items.map[scopeName + '.' + appName + '.-nh-config'];
//                                //var dataObjectsPane = rp.items.map[scopeName + '.' + appName + '.dataObjectsPane'];
//                                var editPane = dataObjectsPane.items.map[scopeName + '.' + appName + '.editor-panel'];
//                                var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];
//                                dbObjectsTree.disable();
//                                setTablesSelectorPane(editPane, dbInfo, dbDict, scopeName, appName, dataObjectsPane);
//                            },
//                            failure: function (f, a) {
//                                if (a.response)
//                                    showDialog(500, 400, 'Error', a.response.responseText, Ext.Msg.OK, null);
//                                else {
//                                    showDialog(400, 100, 'Warning', 'Please fill in every field in this form.', Ext.Msg.OK, null);
//                                }
//                            },
//                            waitMsg: 'Loading ...'
//                        });
//                    }
//                }, {
//                    xtype: 'tbspacer',
//                    width: 4
//                }, {
//                    xtype: 'tbbutton',
//                    icon: 'Content/img/16x16/edit-clear.png',
//                    text: 'Reset',
//                    tooltip: 'Reset to the latest applied changes',
//                    handler: function (f) {
//                        setDsConfigFields(dsConfigPane);
//                    }
//                }]
//            })]
//                });

//                if (dbInfo) {
//                    setDsConfigFields(dsConfigPane);
//                }
//                editPane.add(dsConfigPane);
//                var panelIndex = editPane.items.indexOf(dsConfigPane);
//                editPane.getLayout().setActiveItem(panelIndex);
//            }
//        };

//        var dataObjectsPane = new Ext.Panel({
//            layout: 'border',
//            id: scopeName + '.' + appName + '.dataObjectsPane',
//            frame: false,
//            border: false,
//            items: [{
//                xtype: 'panel',
//                name: 'data-objects-pane',
//                region: 'west',
//                minWidth: 240,
//                width: 300,
//                split: true,
//                autoScroll: true,
//                bodyStyle: 'background:#fff',
//                items: [{
//                    xtype: 'treepanel',
//                    border: false,
//                    autoScroll: true,
//                    animate: true,
//                    lines: true,
//                    frame: false,
//                    enableDD: false,
//                    containerScroll: true,
//                    rootVisible: true,
//                    root: {
//                        text: 'Commodities',
//                        nodeType: 'async',
//                        iconCls: 'folder'
//                    },
//                    loader: new Ext.tree.TreeLoader(),
//                    tbar: new Ext.Toolbar({
//                        items: [{
//                            xtype: 'tbspacer',
//                            width: 4
//                        }, {
//                            xtype: 'button',
//                            icon: 'Content/img/16x16/view-refresh.png',
//                            text: 'Reload',
//                            tooltip: 'Reload Commodities',
//                            handler: null
//                        }, {
//                            xtype: 'tbspacer',
//                            width: 4
//                        }, {
//                            xtype: 'tbspacer',
//                            width: 4
//                        }, {
//                            xtype: 'button',
//                            icon: 'Content/img/16x16/document-save.png',
//                            text: 'Save',
//                            tooltip: 'Save the commodities tree to the back-end server',
//                            formBind: true,
//                            handler: null
//                        }]
//                    })
//                }]
//            },
//                             {
//                                 xtype: 'panel',
//                                 name: 'editor-panel',
//                                 border: 1,
//                                 frame: false,
//                                 id: scopeName + '.' + appName + '.editor-panel',
//                                 region: 'center',
//                                 layout: 'card'
//                             }]
//        });



//        var showTree = function (dbObjectsTree) {
//            var treeLoader = dbObjectsTree.getLoader();
//            var rootNode = dbObjectsTree.getRootNode();

//            treeLoader.dataUrl = null;
//            treeLoader.baseParams = null

//            rootNode.reload(
//          function (rootNode) {
//              loadTree(rootNode);
//          });
//        }

//        Ext.apply(this, {
//            id: scopeName + '.' + appName + '.-nh-config',
//            title: 'P & ID Configuration - ' + scopeName + '.' + appName,
//            closable: true,
//            border: false,
//            frame: true,
//            layout: 'fit',
//            items: [dataObjectsPane]
//        });

//        Ext.EventManager.onWindowResize(this.doLayout, this);

//        Ext.Ajax.request({
//            url: 'SPPID/GetConfig',
//            method: 'POST',
//            params: {
//                scope: scopeName,
//                app: appName
//            },
//            success: function (response, request) {
//                dbDict = Ext.util.JSON.decode(response.responseText);

//                var tab = Ext.getCmp('content-panel');
//                var rp = tab.items.map[scopeName + '.' + appName + '.-nh-config'];
//                var dataObjectsPane = rp.items.map[scopeName + '.' + appName + '.dataObjectsPane'];
//                var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];

//                //if (dbDict.dataObjects.length > 0) {
//                if (dbDict.total > 0) {
//                    // populate data source form
//                    showTree(dbObjectsTree);
//                }
//                else {
//                    dbObjectsTree.disable();
//                    editPane = dataObjectsPane.items.items[1];
//                    if (!editPane) {
//                        var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
//                    }
//                    setDsConfigPane(editPane);
//                }
//            },
//            failure: function (response, request) {
//                editPane = dataObjectsPane.items.items[1];
//                if (!editPane) {
//                    var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
//                }
//                editPane.add(dsConfigPane);
//                editPane.getLayout().setActiveItem(editPane.items.length - 1);
//            }
//        });

//        AdapterManager.sppidConfigWizard.superclass.constructor.apply(this, arguments);
//    }
//});

function setTablesSelectorPane(editPane, dbInfo, dbDict, scopeName, appName, dataObjectsPane, dbTableNames) {
    var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];

    if (editPane) {
        if (editPane.items.map[scopeName + '.' + appName + '.tablesSelectorPane']) {
            var tableSelectorPanel = editPane.items.map[scopeName + '.' + appName + '.tablesSelectorPane'];
            if (tableSelectorPanel) {
                if (dbObjectsTree.disabled)
                    tableSelectorPanel.destroy();
                else {
                    var panelIndex = editPane.items.indexOf(tableSelectorPanel);
                    editPane.getLayout().setActiveItem(panelIndex);
                    return;
                }
            }
        }

        var availItems = setAvailTables(dbObjectsTree, dbTableNames);
        var selectItems = setSelectTables(dbObjectsTree, dbTableNames);

        var tablesSelectorPane = new Ext.FormPanel({
            frame: false,
            border: false,
            autoScroll: true,
            id: scopeName + '.' + appName + '.tablesSelectorPane',
            bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
            labelWidth: 140,
            monitorValid: true,
            items: [{
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
            }],
            tbar: new Ext.Toolbar({
                items: [{
                    xtype: 'tbspacer',
                    width: 4
                }, {
                    xtype: 'tbbutton',
                    icon: 'Content/img/16x16/apply.png',
                    text: 'Apply',
                    tooltip: 'Apply the current changes to the data objects tree',
                    handler: function () {
                        //var tab = Ext.getCmp('content-panel');
                        //var rp = tab.items.map[scopeName + '.' + appName + '.-nh-config'];
                        //var dataObjectsPane = rp.items.map[scopeName + '.' + appName + '.dataObjectsPane'];
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

                        treeLoader.on('beforeload', function (treeLoader, node) {
                            dataObjectsPane.body.mask('Loading...', 'x-mask-loading');
                        }, this);

                        treeLoader.on('load', function (treeLoader, node) {
                            dataObjectsPane.body.unmask();
                        }, this);

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
                                if (rootNode.childNodes[i].attributes.properties.tableName.toLowerCase() == availTableName[j].toLowerCase()) {
                                    found = true;
                                    availTableName.splice(j, 1);
                                    j--;
                                    break;
                                }
                            }

                        for (var i = 0; i < rootNode.childNodes.length; i++) {
                            var nodeText = rootNode.childNodes[i].attributes.properties.tableName;
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
            })
        });
        editPane.add(tablesSelectorPane);
        var panelIndex = editPane.items.indexOf(tablesSelectorPane);
        editPane.getLayout().setActiveItem(panelIndex);

    }
};


