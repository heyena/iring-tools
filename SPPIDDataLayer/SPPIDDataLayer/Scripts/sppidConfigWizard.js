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
        var SPPIDdbInfo;
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
                                            SPPIDdbInfo = showTree(dbObjectsTree, SPPIDdbInfo, dbDict, scopeName, appName, dataObjectsPane);
                                        }
                                        else {
                                            dbObjectsTree.disable();
                                            editPane = dataObjectsPane.items.items[1];
                                            if (!editPane) {
                                                var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
                                            }
                                            setDsConfigPane_SPPID(editPane, SPPIDdbInfo, dbDict, scopeName, appName, dataObjectsPane, datalayer);
                                        }
                                    },
                                    failure: function (response, request) {
                                        editPane = dataObjectsPane.items.items[1];
                                        if (!editPane) {
                                            var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
                                        }
                                        setDsConfigPane_SPPID(editPane, SPPIDdbInfo, dbDict, scopeName, appName, dataObjectsPane, datalayer);
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
                                dbTableNames = setDsConfigPane_SPPID(editPane, SPPIDdbInfo, dbDict, scopeName, appName, dataObjectsPane, datalayer);
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

                                var dsConfigPane_SPPID = editPane.items.map[scopeName + '.' + appName + '.dsConfigPane_SPPID'];
                                var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];
                                var rootNode = dbObjectsTree.getRootNode();
                                var treeProperty = getTreeJson(dsConfigPane_SPPID, rootNode, SPPIDdbInfo, dbDict, dataTypes);

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

                                setTablesSelectorPane(editPane, SPPIDdbInfo, dbDict, scopeName, appName, dataObjectsPane, dbTableNames);
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
            url: 'SPPID/GetConfiguration',
            method: 'POST',
            params: {
                scope: scopeName,
                app: appName
            },
            success: function (response, request) {
                dbDict = Ext.util.JSON.decode(response.responseText);
                //				if (dbDict.PlantConnectionString)
                //				    dbDict.PlantConnectionString = Base64.decode(dbDict.PlantConnectionString);

                //				if (dbDict.SiteConnectionString)
                //				    dbDict.SiteConnectionString = Base64.decode(dbDict.SiteConnectionString);

                //				if (dbDict.StagingConnectionString)
                //				    dbDict.StagingConnectionString = Base64.decode(dbDict.StagingConnectionString);

                var tab = Ext.getCmp('content-panel');
                var rp = tab.items.map[scopeName + '.' + appName + '.-sppid-config'];
                var dataObjectsPane = rp.items.map[scopeName + '.' + appName + '.dataObjectsPane'];
                var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];

                //if (dbDict.dataObjects.length > 0) {
                if (dbDict.total > 0) {

                    // populate data source form
                    SPPIDdbInfo = showTree(dbObjectsTree, SPPIDdbInfo, dbDict, scopeName, appName, dataObjectsPane);
                    var abcdd = 5;
                }
                else {
                    dbObjectsTree.disable();
                    editPane = dataObjectsPane.items.items[1];
                    if (!editPane) {
                        var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
                    }
                    setDsConfigPane_SPPID(editPane, SPPIDdbInfo, dbDict, scopeName, appName, dataObjectsPane, datalayer);
                }
            },
            failure: function (response, request) {
                editPane = dataObjectsPane.items.items[1];
                if (!editPane) {
                    var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
                }
                editPane.add(dsConfigPane_SPPID);
                editPane.getLayout().setActiveItem(editPane.items.length - 1);
            }
        });

        AdapterManager.sppidConfigWizard.superclass.constructor.apply(this, arguments);
    }
});


//*************************************
function setDsConfigFields(dsConfigPane_SPPID, SPPIDdbInfo, dbDict) {
    var dsConfigForm_SPPId = dsConfigPane_SPPID.getForm();
    var Provider = null;

    if (dbDict.Provider)
        Provider = dbDict.Provider.toUpperCase();

    var dbName = dsConfigForm.findField('dbName');
    var portNumber = dsConfigForm.findField('portNumber');
    var host = dsConfigForm.findField('host');
    var dbServer = dsConfigForm.findField('dbServer');
    var dbInstance = dsConfigForm.findField('dbInstance');
    var serviceName = dsConfigPane_SPPID.items.items[0].items.items[9];
    var dbSchema = dsConfigForm.findField('dbSchema');
    var userName = dsConfigForm.findField('dbUserName');
    var password = dsConfigForm.findField('dbPassword');
    var dbProvider = dsConfigForm.findField('dbProvider');

    if (SPPIDdbInfo) {
        if (Provider) {
            if (Provider.indexOf('ORACLE') > -1) {
                dbName.hide();
                dbServer.hide();
                dbInstance.hide();

                dbServer.setValue(SPPIDdbInfo.dbServer);
                dbInstance.setValue(SPPIDdbInfo.dbInstance);
                dbName.setValue(SPPIDdbInfo.dbName);

                userName.setValue(SPPIDdbInfo.dbUserName);
                password.setValue(SPPIDdbInfo.dbPassword);
                dbProvider.setValue(dbDict.Provider);
                dbSchema.setValue(dbDict.SchemaName);

                host.setValue(SPPIDdbInfo.dbServer);
                host.show();

                serviceName.show();
                creatRadioField(serviceName, serviceName.id, SPPIDdbInfo.dbInstance, SPPIDdbInfo.serName);

                portNumber.setValue(SPPIDdbInfo.portNumber);
                portNumber.show();
            }
            else if (Provider.indexOf('MSSQL') > -1) {
                portNumber.hide();
                host.hide();
                serviceName.hide();

                dbServer.setValue(SPPIDdbInfo.dbServer);
                dbServer.show();
                dbInstance.setValue(SPPIDdbInfo.dbInstance);
                dbInstance.show();
                dbName.setValue(SPPIDdbInfo.dbName);
                dbName.show();
                dbProvider.setValue(dbDict.Provider);
                host.setValue(SPPIDdbInfo.dbServer);
                portNumber.setValue(SPPIDdbInfo.portNumber);
                userName.setValue(SPPIDdbInfo.dbUserName);
                password.setValue(SPPIDdbInfo.dbPassword);
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

function changeConfigOracle(host, dbSchema, userName, password, serviceName, OraclePane, plantDatabase) {
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
    for (var i = 0; i < OraclePane.items.length; i++) {
        Ext.getCmp(OraclePane.items.items[i].id).show();
    }
    OraclePane.show();
    for (var i = 0; i < plantDatabase.items.length; i++) {
        if (Ext.getCmp(plantDatabase.items.items[i].id).getValue() == '') {
            Ext.getCmp(plantDatabase.items.items[i].id).setValue(' ');
        }
        Ext.getCmp(plantDatabase.items.items[i].id).hide();
    }

    plantDatabase.hide();

}

function changeConfig(dbName, dbServer, dbInstance, dbSchema, userName, password, plantDatabase, OraclePane) {
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

    for (var i = 0; i < plantDatabase.items.length; i++) {
        Ext.getCmp(plantDatabase.items.items[i].id).show();
    }
    plantDatabase.show();

    for (var i = 0; i < OraclePane.items.length; i++) {
        Ext.getCmp(OraclePane.items.items[i].id).setValue(' ');
        Ext.getCmp(OraclePane.items.items[i].id).hide();
    }
    OraclePane.hide();
}

function setDsConfigPane_SPPID(editPane, SPPIDdbInfo, dbDict, scopeName, appName, dataObjectsPane, datalayer) {
    if (editPane) {
        if (editPane.items.map[scopeName + '.' + appName + '.dsConfigPane_SPPID']) {
            var dsConfigPanel = editPane.items.map[scopeName + '.' + appName + '.dsConfigPane_SPPID'];

            if (dsConfigPanel) {
                var panelIndex = editPane.items.indexOf(dsConfigPanel);
                editPane.getLayout().setActiveItem(panelIndex);
                return;
            }
        }



        var dsConfigPane_SPPID = new Ext.FormPanel({
            labelWidth: 150,
            id: scopeName + '.' + appName + '.dsConfigPane_SPPID',
            frame: false,
            border: false,
            autoScroll: true,
            title: "Configure SP & ID Data Source",
            bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
            monitorValid: true,
            defaults: { anchor: '100%', xtype: 'textfield', allowBlank: false },
            items: [{
                xtype: 'fieldset',
                id: 'siteDatabase',
                title: "SP & ID Site Database Details",
                defaults: { anchor: '100%', xtype: 'textfield', allowBlank: false, width: 300 },
                items: [{
                    xtype: 'combo',
                    fieldLabel: 'Database Provider',
                    hiddenName: 'dbProvider',
                    allowBlank: false,
                    store: new Ext.data.SimpleStore({
                        fields: ['Provider'],
                        data: [
                      ["MSSQL2000", "MSSQL2000"], ["MSSQL2005", "MSSQL2005"], ["MSSQL2008", "MSSQL2008"], ["ORACLE8i", "ORACLE8i"], ["ORACLE9i", "ORACLE9i"], ["ORACLE10g", "ORACLE10g"]
                          ]
                    }),
                    selectOnFocus: true,
                    mode: 'local',
                    editable: false,
                    value: 'MsSql2008',
                    triggerAction: 'all',
                    displayField: 'Provider',
                    valueField: 'Provider',
                    listeners: { 'select': function (combo, record, index) {
                        var dbProvider = record.data.Provider.toUpperCase();
                        var dbName = dsConfigPane_SPPID.getForm().findField('dbName');
                        var portNumber = dsConfigPane_SPPID.getForm().findField('portNumber');
                        var host = dsConfigPane_SPPID.getForm().findField('dbhost');
                        var dbServer = dsConfigPane_SPPID.getForm().findField('dbServer');
                        var dbInstance = dsConfigPane_SPPID.getForm().findField('dbInstance');
                        var serviceName = dsConfigPane_SPPID.items.items[0].items.items[9];
                        var dbSchema = dsConfigPane_SPPID.getForm().findField('dbSchema');
                        var userName = dsConfigPane_SPPID.getForm().findField('dbUserName');
                        var password = dsConfigPane_SPPID.getForm().findField('dbPassword');
                        var OraclePane = dsConfigPane_SPPID.items.items[2];
                        var plantDatabase = dsConfigPane_SPPID.items.items[1];


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
                                        changeConfigOracle(host, dbSchema, userName, password, serviceName, OraclePane, plantDatabase);


                                }
                                else {
                                    changeConfigOracle(host, dbSchema, userName, password, serviceName, OraclePane, plantDatabase);
                                }

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
                                        changeConfig(dbName, dbServer, dbInstance, dbSchema, userName, password, plantDatabase, OraclePane);
                                }
                                else
                                    changeConfig(dbName, dbServer, dbInstance, dbSchema, userName, password, plantDatabase, OraclePane);
                            }

                            portNumber.setValue('1433');
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
                    name: 'dbhost',
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
                        var dbProvider = dsConfigPane_SPPID.getForm().findField('dbProvider').getValue().toUpperCase();
                        if (dbProvider.indexOf('ORACLE') > -1) {
                            var dbSchema = dsConfigPane_SPPID.getForm().findField('dbSchema');
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
                    xtype: 'textfield',
                    fieldLabel: 'Database Provider',
                    name: 'dbplantProvider',
                    allowBlank: false,
                    readOnly: true,
                    value: 'MsSql2008'


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
                    allowBlank: false
                },
                {
                    xtype: 'textfield',
                    inputType: 'password',
                    name: 'dbplantPassword',
                    fieldLabel: 'Password',
                    allowBlank: false
                }]
            },

            { xtype: 'fieldset',
                id: 'OracleDatabase',
                title: "Oracle Schema Details",
                layout: "column",
                defaults: { anchor: '100%', xtype: 'textfield', allowBlank: false, width: 250, style: 'margin-left: 5px; margin-bottom: 5px;' },
                hidden: true,
                items: [{
                    xtype: 'fieldset',
                    columnWidth: .5,
                    title: "PID Schema",
                    padding: 3,
                    items: [{
                        xtype: 'textfield',
                        fieldLabel: 'User Name',
                        name: 'dbPIDUserName',
                        value: 'RUSSELCITY_PILOTPID'
                    },
                   {
                       xtype: 'textfield',
                       inputType: 'password',
                       fieldLabel: 'Password',
                       name: 'dbPIDPassword',
                       value: 'RUSSELCITY_PILOTPID'
                   }]
                },
                 {
                     xtype: 'fieldset',
                     columnWidth: .5,
                     padding: 3,
                     title: "PID Datadictionary",

                     items: [{
                         xtype: 'textfield',
                         fieldLabel: 'User Name',
                         name: 'dbPIDDataDicUserName',
                         value: 'RUSSELCITY_PILOTPIDD'
                     },
                        {
                            xtype: 'textfield',
                            inputType: 'password',
                            fieldLabel: 'Password',
                            name: 'dbPIDDataDicPassword',
                            value: 'RUSSELCITY_PILOTPIDD'
                        }]
                 },
                    {
                        xtype: 'fieldset',
                        columnWidth: .5,
                        padding: 5,
                        title: "Plant Schema",
                        items: [{
                            xtype: 'textfield',
                            fieldLabel: 'User Name',
                            name: 'dbOraPlantUserName',
                            value: 'RUSSELCITY_PILOT'
                        },
                        {
                            xtype: 'textfield',
                            inputType: 'password',
                            fieldLabel: 'Password',
                            name: 'dbOraPlantPassword',
                            value: 'RUSSELCITY_PILOT'
                        }]
                    },
                    {
                        xtype: 'fieldset',
                        columnWidth: .5,
                        padding: 5,
                        title: "Plant Datadictionary",
                        items: [{
                            xtype: 'textfield',
                            fieldLabel: 'User Name',
                            name: 'dbPlantDataDicUserName',
                            value: 'RUSSELCITY_PILOTD'
                        },
                        {
                            xtype: 'textfield',
                            inputType: 'password',
                            fieldLabel: 'Password',
                            name: 'dbPlantDataDicPassword',
                            value: 'RUSSELCITY_PILOTD'
                        }]
                    }]
            },
                {
                    xtype: 'fieldset',
                    id: 'staggingDatabase',
                    title: " Stagging Database Details",
                    defaults: { anchor: '100%', xtype: 'textfield', allowBlank: false, width: 300 },
                    items: [{
                        xtype: 'textfield',
                        fieldLabel: 'Database Provider',
                        name: 'dbstageProvider',
                        allowBlank: false,
                        readOnly: true,
                        value: 'MsSql2008'


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
                        allowBlank: false

                    }, {
                        xtype: 'textfield',
                        inputType: 'password',
                        name: 'dbstagePassword',
                        fieldLabel: 'Password',
                        allowBlank: false
                    }]
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
                        var dbProvider = dsConfigPane_SPPID.getForm().findField('dbProvider').getValue().toUpperCase();
                        var dbName = dsConfigPane_SPPID.getForm().findField('dbName');
                        var host = dsConfigPane_SPPID.getForm().findField('dbhost');
                        var portNumber = dsConfigPane_SPPID.getForm().findField('dbportNumber');
                        var dbServer = dsConfigPane_SPPID.getForm().findField('dbServer');
                        var dbInstance = dsConfigPane_SPPID.getForm().findField('dbInstance');
                        var serviceNamePane = dsConfigPane_SPPID.items.items[0].items.items[9];
                        var dbSchema = dsConfigPane_SPPID.getForm().findField('dbSchema');

                        var servieName = '';
                        var serName = '';
                        var _datalayer = '';

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

                        dsConfigPane_SPPID.getForm().submit({
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
                                setTablesSelectorPane(editPane, SPPIDdbInfo, dbDict, scopeName, appName, dataObjectsPane, dbTableNames);
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
                        setDsConfigFields(dsConfigPane_SPPID);
                    }
                }]
            })
        });

        if (SPPIDdbInfo) {
            setDsConfigFields(dsConfigPane_SPPID);
        }
        editPane.add(dsConfigPane_SPPID);
        var panelIndex = editPane.items.indexOf(dsConfigPane_SPPID);
        editPane.getLayout().setActiveItem(panelIndex);
    }
};

function setAvailTables_SPPID(dbObjectsTree, dbTableNames) {
    var availTableName = new Array();

    if (dbObjectsTree.disabled) {
        for (var i = 0; i < dbTableNames.success.length; i++) {
            var tableName = dbTableNames.success[i];
            availTableName.push(tableName);
        }
    }
    else {
        var rootNode = dbObjectsTree.getRootNode();
        if (dbTableNames.items) {
            for (var i = 0; i < dbTableNames.success.length; i++) {
                availTableName.push(dbTableNames.success[i]);
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

function setSelectTables_SPPID(dbObjectsTree, dbTableNames) {
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


function setTablesSelectorPane(editPane, SPPIDdbInfo, dbDict, scopeName, appName, dataObjectsPane, dbTableNames) {
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

        var availItems = setAvailTables_SPPID(dbObjectsTree, dbTableNames);
        var selectItems = setSelectTables_SPPID(dbObjectsTree, dbTableNames);

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
                        var dsConfigPane_SPPID = editPane.items.map[scopeName + '.' + appName + '.dsConfigPane_SPPID'];
                        var tablesSelectorPane = editPane.items.map[scopeName + '.' + appName + '.tablesSelectorPane'];
                        var tablesSelForm = tablesSelectorPane.getForm();
                        var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];
                        var serName = '';
                        var serviceName = '';

                        if (dbObjectsTree.disabled) {
                            dbObjectsTree.enable();
                        }

                        if (dsConfigPane_SPPID) {
                            var serviceNamePane = dsConfigPane_SPPID.items.items[0].items.items[9];
                            if (serviceNamePane.items.items[0])
                                serName = serviceNamePane.items.items[0].serName;
                        }
                        else {
                            if (SPPIDdbInfo.serName)
                                serName = SPPIDdbInfo.serName;
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

                        treeLoader.dataUrl = 'SPPID/DBObjects';
                        if (dsConfigPane_SPPID) {
                            var dsConfigForm = dsConfigPane_SPPID.getForm();
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
                                dbServer: SPPIDdbInfo.dbServer,
                                dbInstance: SPPIDdbInfo.dbInstance,
                                dbName: SPPIDdbInfo.dbName,
                                dbSchema: dbDict.SchemaName,
                                dbUserName: SPPIDdbInfo.dbUserName,
                                dbPassword: SPPIDdbInfo.dbPassword,
                                portNumber: SPPIDdbInfo.portNumber,
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


