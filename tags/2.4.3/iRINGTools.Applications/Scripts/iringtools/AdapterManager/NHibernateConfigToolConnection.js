Ext.ns('AdapterManager');

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
        //new application setting default value
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

function setDsConfigPane(editPane, dbInfo, dbDict, scopeName, appName, dataObjectsPane) {
	if (editPane) {
		if (editPane.items.map[scopeName + '.' + appName + '.dsconfigPane']) {
			var dsConfigPanel = editPane.items.map[scopeName + '.' + appName + '.dsconfigPane'];
			
      if (dsConfigPanel) {				
				var panelIndex = editPane.items.indexOf(dsConfigPanel);
				editPane.getLayout().setActiveItem(panelIndex);
				return;
			}
		}

		var providersStore = new Ext.data.JsonStore({
			autoLoad: true,
			autoDestroy: true,
			url: 'AdapterManager/DBProviders',
			timeout: 600000,
			root: 'items',
			idProperty: 'Provider',
			fields: [{
				name: 'Provider'
			}]
        });

        var msgbx = Ext.MessageBox.show({
            title: 'Please wait',
            msg: 'Loading items...',
            progressText: 'Initializing...',
            width: 200,
            progress: true,
            closable: false
        });
        providersStore.on('beforeload', function (treeLoader, node) {
            Runner.run(msgbx);
            //editPane.body.mask('Loading...', 'x-mask-loading');
        }, this);

        providersStore.on('load', function (treeLoader, node) {
            msgbx.hide();
            //editPane.body.unmask();
        }, this);

        var dsConfigPane = new Ext.FormPanel({
            labelWidth: 150,
            id: scopeName + '.' + appName + '.dsconfigPane',
            frame: false,
            border: false,
            autoScroll: true,
            bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
            monitorValid: true,
            defaults: { anchor: '100%', xtype: 'textfield', allowBlank: false },
            items: [{
                xtype: 'label',
                fieldLabel: 'Configure Data Source',
                labelSeparator: '',
                itemCls: 'form-title'
            }, {
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
                valueField: 'Provider',
                listeners: { 'select': function (combo, record, index) {
                    var dbProvider = record.data.Provider.toUpperCase();
                    var dbName = dsConfigPane.getForm().findField('dbName');
                    var portNumber = dsConfigPane.getForm().findField('portNumber');
                    var host = dsConfigPane.getForm().findField('host');
                    var dbServer = dsConfigPane.getForm().findField('dbServer');
                    var dbInstance = dsConfigPane.getForm().findField('dbInstance');
                    var serviceName = dsConfigPane.items.items[10];
                    var dbSchema = dsConfigPane.getForm().findField('dbSchema');
                    var userName = dsConfigPane.getForm().findField('dbUserName');
                    var password = dsConfigPane.getForm().findField('dbPassword');

                    if (dbProvider.indexOf('ORACLE') > -1) {
                        if (dbName.hidden == false) {
                            dbName.hide();
                            dbServer.hide();
                            dbInstance.hide();
                        }

                        if (host.hidden == true) {
                            if (dbDict.Provider) {
                                if (dbDict.Provider.toUpperCase().indexOf('ORACLE') > -1) {
                                    host.setValue(dbInfo.dbServer);
                                    serviceName.show();
                                    creatRadioField(serviceName, serviceName.id, dbInfo.dbInstance, dbInfo.serName);
                                    host.show();
                                    userName.setValue(dbInfo.dbUserName);
                                    password.setValue(dbInfo.dbPassword);
                                    dbSchema.setValue(dbDict.SchemaName);
                                }
                                else
                                    changeConfigOracle(host, dbSchema, userName, password, serviceName);
                            }
                            else
                                changeConfigOracle(host, dbSchema, userName, password, serviceName);

                            portNumber.setValue('1521');
                            portNumber.show();
                        }
                    }
                    else if (dbProvider.indexOf('MSSQL') > -1) {
                        if (host.hidden == false) {
                            portNumber.hide();
                            host.hide();
                            serviceName.hide();
                        }

                        if (dbName.hidden == true) {
                            if (dbDict.Provider) {
                                if (dbDict.Provider.toUpperCase().indexOf('MSSQL') > -1) {
                                    dbName.setValue(dbInfo.dbName);
                                    dbServer.setValue(dbInfo.dbServer);
                                    dbInstance.setValue(dbInfo.dbInstance);
                                    dbName.show();
                                    dbServer.show();
                                    dbInstance.show();
                                    dbSchema.setValue(dbDict.SchemaName);
                                    userName.setValue(dbInfo.dbUserName);
                                    password.setValue(dbInfo.dbPassword);
                                }
                                else
                                    changeConfig(dbName, dbServer, dbInstance, dbSchema, userName, password);
                            }
                            else
                                changeConfig(dbName, dbServer, dbInstance, dbSchema, userName, password);
                        }

                        portNumber.setValue('1433');
                    }
                    else if (dbProvider.indexOf('MYSQL') > -1) {
                        if (dbServer.hidden == true) {
                            dbServer.setValue('');
                            dbServer.clearInvalid();
                            dbServer.show();
                        }

                        if (host.hidden == false) {
                            portNumber.hide();
                            host.hide();
                            serviceName.hide();
                            portNumber.setValue('3306');
                        }
                    }
                }
                }
            }, {
                xtype: 'textfield',
                name: 'dbServer',
                fieldLabel: 'Database Server',
                value: 'localhost',
                allowBlank: false
            }, {
                xtype: 'textfield',
                name: 'host',
                fieldLabel: 'Host Name',
                hidden: true,
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
                fieldLabel: 'Database Instance',
                value: 'default',
                allowBlank: true
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
            }, {
                xtype: 'textfield',
                name: 'dbSchema',
                fieldLabel: 'Schema Name',
                value: 'dbo',
                allowBlank: false
            }, {
                xtype: 'panel',
                id: scopeName + '.' + appName + '.servicename',
                name: 'serviceName',
                layout: 'fit',
                anchor: '100% - 1',
                border: false,
                frame: false
            }],
            tbar: new Ext.Toolbar({
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
                        var host = dsConfigPane.getForm().findField('host');
                        var dbServer = dsConfigPane.getForm().findField('dbServer');
                        var dbInstance = dsConfigPane.getForm().findField('dbInstance');
                        var serviceNamePane = dsConfigPane.items.items[10];
                        var dbSchema = dsConfigPane.getForm().findField('dbSchema');
                        var servieName = '';
                        var serName = '';
                        if (dbProvider.indexOf('ORACLE') > -1) {
                            dbServer.setValue(host.getValue());
                            dbName.setValue(dbSchema.getValue());
                            servieName = serviceNamePane.items.items[0].value;
                            serName = serviceNamePane.items.items[0].serName;
                            dbInstance.setValue(servieName);
                        }
                        else if (dbProvider.indexOf('MSSQL') > -1) {
                            host.setValue(dbServer.getValue());
                            serviceName = dbInstance.getValue();
                        }
                        else if (dbProvider.indexOf('MYSQL') > -1) {
                            dbName.setValue(dbSchema.getValue());
                            dbInstance.setValue(dbSchema.getValue());
                        }

                        dsConfigPane.getForm().submit({
                            url: 'AdapterManager/TableNames',
                            timeout: 600000,
                            params: {
                                scope: scopeName,
                                app: appName,
                                serName: serName
                            },
                            success: function (f, a) {
                                if (!dbInfo)
                                    dbInfo = {};
                                dbInfo.dbTableNames = Ext.util.JSON.decode(a.response.responseText);
                                //var tab = Ext.getCmp('content-panel');
                                //var rp = tab.items.map[scopeName + '.' + appName + '.-nh-config'];
                                //var dataObjectsPane = rp.items.map[scopeName + '.' + appName + '.dataObjectsPane'];
                                var editPane = dataObjectsPane.items.map[scopeName + '.' + appName + '.editor-panel'];
                                var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];
                                dbObjectsTree.disable();
                                setTablesSelectorPane(editPane, dbInfo, dbDict, scopeName, appName, dataObjectsPane);
                                return dbInfo.dbTableNames;
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
                        setDsConfigFields(dsConfigPane, dbInfo, dbDict);
                    }
                }]
            })
        });

		if (dbInfo) {
			setDsConfigFields(dsConfigPane, dbInfo, dbDict);
		}
		editPane.add(dsConfigPane);
		var panelIndex = editPane.items.indexOf(dsConfigPane);
		editPane.getLayout().setActiveItem(panelIndex);
	}
};

function setAvailTables(dbObjectsTree, dbTableNames) {
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
					if (rootNode.childNodes[i].attributes.properties.tableName.toLowerCase() == availTableName[j].toLowerCase()) {
						found = true;
						availTableName.splice(j, 1);
						j--;
						break;
					}
				}
		}
	}
	return availTableName;
}

function setSelectTables (dbObjectsTree) {
	var selectTableNames = new Array();

	if (!dbObjectsTree.disabled) {
		var rootNode = dbObjectsTree.getRootNode();
		for (var i = 0; i < rootNode.childNodes.length; i++) {
			var nodeText = rootNode.childNodes[i].attributes.properties.tableName;
			selectTableNames.push([nodeText, nodeText]);
		}
	}

	return selectTableNames;
}

function setTablesSelectorPane(editPane, dbInfo, dbDict, scopeName, appName, dataObjectsPane) {
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

    var availItems = setAvailTables(dbObjectsTree, dbInfo.dbTableNames);
    var selectItems = setSelectTables(dbObjectsTree);

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
        }, {
            xtype: 'checkbox',
            name: 'enableSummary',
            fieldLabel: 'Enable Summary'
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

                    for (var i = 0; i < dbInfo.dbTableNames.items.length; i++) {
                        availTableName.push(dbInfo.dbTableNames.items[i]);
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
                    tablesSelectorPane.getForm().findField('enableSummary').setValue(dbDict.enableSummary);
                }
            }]
        })
    });
        tablesSelectorPane.getForm().findField('enableSummary').setValue(dbDict.enableSummary);
		editPane.add(tablesSelectorPane);
		var panelIndex = editPane.items.indexOf(tablesSelectorPane);
		editPane.getLayout().setActiveItem(panelIndex);

	}
};

function loadTree(rootNode, dbDict) {  
    var relationTypeStr = ['OneToOne', 'OneToMany'];

	// sync data object tree with data dictionary
	for (var i = 0; i < rootNode.childNodes.length; i++) {
	  var dataObjectNode = rootNode.childNodes[i];	  
		dataObjectNode.attributes.properties.tableName = dataObjectNode.text;
		for (var ijk = 0; ijk < dbDict.dataObjects.length; ijk++) {
		  var dataObject = dbDict.dataObjects[ijk];		  

		  if (dataObjectNode.text.toUpperCase() != dataObject.tableName.toUpperCase())
				continue;

			// sync data object
			dataObjectNode.attributes.properties.objectNamespace = dataObject.objectNamespace;
			dataObjectNode.attributes.properties.objectName = dataObject.objectName;
			dataObjectNode.attributes.properties.keyDelimiter = dataObject.keyDelimeter;
			dataObjectNode.attributes.properties.description = dataObject.description;
			dataObjectNode.text = dataObject.objectName;
			dataObjectNode.attributes.text = dataObject.objectName;
			dataObjectNode.setText(dataObject.objectName);

			if (dataObject.objectName.toLowerCase() == dataObjectNode.text.toLowerCase()) {
			    var shownProperty = new Array();	
                var keysNode = dataObjectNode.attributes.children[0];
				var propertiesNode = dataObjectNode.attributes.children[1];
				var relationshipsNode = dataObjectNode.attributes.children[2];

				// sync data properties
				for (var j = 0; j < propertiesNode.children.length; j++) {
					for (var jj = 0; jj < dataObject.dataProperties.length; jj++) {
						if (propertiesNode.children[j].text.toLowerCase() == dataObject.dataProperties[jj].columnName.toLowerCase()) {

						    if (!hasShown(shownProperty, propertiesNode.children[j].text.toLowerCase())) {
						        shownProperty.push(propertiesNode.children[j].text.toLowerCase());
						        propertiesNode.children[j].hidden = false;
						    }

							propertiesNode.children[j].text = dataObject.dataProperties[jj].propertyName;
							propertiesNode.children[j].properties.propertyName = dataObject.dataProperties[jj].propertyName;
							propertiesNode.children[j].properties.isHidden = dataObject.dataProperties[jj].isHidden;
						}
					}
				}

				// sync key properties
				for (var ij = 0; ij < dataObject.keyProperties.length; ij++) {
					for (var k = 0; k < keysNode.children.length; k++) {
						for (var ikk = 0; ikk < dataObject.dataProperties.length; ikk++) {
							if (dataObject.keyProperties[ij].keyPropertyName.toLowerCase() == dataObject.dataProperties[ikk].propertyName.toLowerCase()) {
								if (keysNode.children[k].text.toLowerCase() == dataObject.dataProperties[ikk].columnName.toLowerCase()) {
								    keysNode.children[k].text = dataObject.keyProperties[ij].keyPropertyName;
								    keysNode.children[k].properties.propertyName = dataObject.keyProperties[ij].keyPropertyName;
								    keysNode.children[k].properties.isHidden = dataObject.keyProperties[ij].isHidden;
									ij++;
									break;
								}
							}
						}
						break;
					}
					if (ij < dataObject.keyProperties.length) {
						for (var ijj = 0; ijj < propertiesNode.children.length; ijj++) {
							var nodeText = dataObject.keyProperties[ij].keyPropertyName;
							if (propertiesNode.children[ijj].text.toLowerCase() == nodeText.toLowerCase()) {
								var properties = propertiesNode.children[ijj].properties;
								properties.propertyName = nodeText;
								//properties.keyType = 'assigned';
								//properties.nullable = false;

								newKeyNode = new Ext.tree.TreeNode({
									text: nodeText,
									type: "keyProperty",
									leaf: true,
									iconCls: 'treeKey',
									hidden: false,
									properties: properties
								});
							  newKeyNode.iconCls = 'treeKey';
								propertiesNode.children.splice(ijj, 1);
								ijj--;

								if (newKeyNode)
									keysNode.children.push(newKeyNode);

								break;
							}
						}
					}
				}

				// sync relationships
				for (var kj = 0; kj < dataObject.dataRelationships.length; kj++) {
					var newNode = new Ext.tree.TreeNode({
						text: dataObject.dataRelationships[kj].relationshipName,
						type: 'relationship',
						leaf: true,
            iconCls: 'treeRelation',
						relatedObjMap: [],
						objectName: dataObjectNode.text,
						relatedObjectName: dataObject.dataRelationships[kj].relatedObjectName,
						relationshipType: relationTypeStr[dataObject.dataRelationships[kj].relationshipType],
						relationshipTypeIndex: dataObject.dataRelationships[kj].relationshipType,
						propertyMap: []
					});
					var mapArray = new Array();
					for (var kjj = 0; kjj < dataObject.dataRelationships[kj].propertyMaps.length; kjj++) {
						var mapItem = new Array();
						mapItem['dataPropertyName'] = dataObject.dataRelationships[kj].propertyMaps[kjj].dataPropertyName;
						mapItem['relatedPropertyName'] = dataObject.dataRelationships[kj].propertyMaps[kjj].relatedPropertyName;
						mapArray.push(mapItem);
		      }
		      newNode.iconCls = 'treeRelation';
					newNode.attributes.propertyMap = mapArray;
					relationshipsNode.expanded = true;
					relationshipsNode.children.push(newNode);
				}
			}
		}
		ijk++;
    }

    if (rootNode.childNodes.length == 1)
        if (rootNode.childNodes[0].text == "")
            rootNode.removeChild(rootNode.childNodes[0], true);
};		

function setTableNames (dbDict) {
	// populate selected tables			
	var selectTableNames = new Array();

	for (var i = 0; i < dbDict.dataObjects.length; i++) {
		var tableName = (dbDict.dataObjects[i].tableName ? dbDict.dataObjects[i].tableName : dbDict.dataObjects[i]);
		selectTableNames.push(tableName);
	}

	return selectTableNames;
};

function showTree(dbObjectsTree, dbInfo, dbDict, scopeName, appName, dataObjectsPane) {
    //loadProgressBar();
	var selectTableNames = setTableNames(dbDict);
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

	var treeLoader = dbObjectsTree.getLoader();
	var rootNode = dbObjectsTree.getRootNode();

	treeLoader.dataUrl = 'AdapterManager/DBObjects';
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
		serName: dbInfo.serName
    };
	var mySize = dataObjectsPane.getSize();
	var tempHeight = mySize.height;
	var tempWidth = mySize.width;
    //var m = dataObjectsPane.body.mask('<div id="pbar"></div>');
	var msgbx = Ext.MessageBox.show({
	    title: 'Please wait',
	    msg: 'Loading items...',
	    progressText: 'Initializing...',
	    width: 200,
	    progress: true,
	    closable: false
	});

	treeLoader.on('beforeload', function (treeLoader, node) {
	    Runner.run(msgbx);
	}, this);

	treeLoader.on('load', function (treeLoader, node) {
	    msgbx.hide();
	    //dataObjectsPane.body.unmask();
	    // pbar1.destroy();
	}, this);

    treeLoader.on('loadexception', function (treeLoader, node) {
        dataObjectsPane.body.unmask();
    }, this);

	rootNode.reload(
      function (rootNode) {
        loadTree(rootNode, dbDict);
      });

      Ext.Ajax.request({
          url: 'AdapterManager/TableNames',
          timeout: 600000,
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
              dbInfo.dbTableNames = Ext.util.JSON.decode(response.responseText);              
          },
          failure: function (f, a) {
              if (a.response)
                  showDialog(500, 400, 'Error', a.response.responseText, Ext.Msg.OK, null);
          }
      });
	return dbInfo;
}