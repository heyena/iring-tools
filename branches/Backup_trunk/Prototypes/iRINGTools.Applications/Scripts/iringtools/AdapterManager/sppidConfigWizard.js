Ext.ns('AdapterManager');

AdapterManager.sppidConfigWizard = Ext.extend(Ext.Container, {
    scope: null,
    app: null,
    iconCls: 'tabsSPPID',
    border: false,
    frame: false,


    constructor: function (config) {
        config = config || {};

        var wizard = this;
        var scopeName = config.scope;
        var appName = config.app;
        var dbDict;
        var dbInfo;
        var dbTableNames;
        var userTableNames;


        var myStore = new Ext.data.SimpleStore({
            fields: ['abbr', 'state'],
            data: [
        ['Drawing', 'Drawing'],
        ['EquipmentOther', 'EquipmentOther'],
        ['Exchanger', 'Exchanger'],
        ['Mechanical', 'Mechanical'],
        ['Vessel', 'Vessel'],
        ['Class', 'Class'],
        ['FabricationCategory', 'FabricationCategory'],
        ['InsulDensity', 'InsulDensity'],
        ['Name', 'Name']]
        });


        var setDsConfigPane = function (editPane) {
            if (editPane) {
                if (editPane.items.map[scopeName + '.' + appName + '.dsconfigPane']) {
                    var dsConfigPanel = editPane.items.map[scopeName + '.' + appName + '.dsconfigPane'];
                    if (dsConfigPanel) {
                        //						var panelIndex = editPane.items.indexOf(dsConfigPanel);
                        //						editPane.getLayout().setActiveItem(panelIndex);
                        //						dsConfigPanel.show();
                        var panelIndex = editPane.items.indexOf(dsConfigPanel);
                        editPane.getLayout().setActiveItem(panelIndex);
                        return;
                    }
                }

                var providersStore = new Ext.data.JsonStore({
                    autoLoad: true,
                    autoDestroy: true,
                    url: null,
                    root: 'items',
                    idProperty: 'Provider',
                    fields: [{
                        name: 'Provider'
                    }]
                });

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
                        fieldLabel: 'Configure SP & ID Data Source',
                        labelSeparator: '',
                        itemCls: 'form-title',
                        labelWidth: 350
                    },
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
                                    else {
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
                                }
                                else {
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
                                    else {
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
                                }
                                else {
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
                                    else {
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
                                }
                                else {
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
                                    else {
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
                                }
                                else {
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
                    name: 'dbplantServer',
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
                }, {
                    xtype: 'textfield',
                    name: 'dbplantSchema',
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
                }]
            },
            tbar = new Ext.Toolbar({
                items: [{
                    xtype: 'tbspacer',
                    width: 4
                }, {
                    xtype: 'tbbutton',
                    icon: 'Content/img/16x16/document-properties.png',
                    text: 'Connect',
                    tooltip: 'Connect'
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
            })]
                });

                if (dbInfo) {
                    setDsConfigFields(dsConfigPane);
                }
                editPane.add(dsConfigPane);
                var panelIndex = editPane.items.indexOf(dsConfigPane);
                editPane.getLayout().setActiveItem(panelIndex);
            }
        };

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
                autoScroll: true,
                bodyStyle: 'background:#fff',
                items: [{
                    xtype: 'treepanel',
                    border: false,
                    autoScroll: true,
                    animate: true,
                    lines: true,
                    frame: false,
                    enableDD: false,
                    containerScroll: true,
                    rootVisible: true,
                    root: {
                        text: 'Commodities',
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
                            tooltip: 'Reload Commodities',
                            handler: null
                        }, {
                            xtype: 'tbspacer',
                            width: 4
                        }, {
                            xtype: 'tbspacer',
                            width: 4
                        }, {
                            xtype: 'button',
                            icon: 'Content/img/16x16/document-save.png',
                            text: 'Save',
                            tooltip: 'Save the commodities tree to the back-end server',
                            formBind: true,
                            handler: null
                        }]
                    })
                }]
            },
                             {
                                 xtype: 'panel',
                                 name: 'editor-panel',
                                 border: 1,
                                 frame: false,
                                 id: scopeName + '.' + appName + '.editor-panel',
                                 region: 'center',
                                 layout: 'card'
                             }]
        });



        var showTree = function (dbObjectsTree) {
            var treeLoader = dbObjectsTree.getLoader();
            var rootNode = dbObjectsTree.getRootNode();

            treeLoader.dataUrl = null;
            treeLoader.baseParams = null

            rootNode.reload(
          function (rootNode) {
              loadTree(rootNode);
          });
        }

        Ext.apply(this, {
            id: scopeName + '.' + appName + '.-nh-config',
            title: 'P & ID Configuration - ' + scopeName + '.' + appName,
            closable: true,
            border: false,
            frame: true,
            layout: 'fit',
            items: [dataObjectsPane]
        });

        Ext.EventManager.onWindowResize(this.doLayout, this);

        Ext.Ajax.request({
            url: 'AdapterManager/DBDictionary',
            method: 'POST',
            params: {
                scope: scopeName,
                app: appName
            },
            success: function (response, request) {
                dbDict = Ext.util.JSON.decode(response.responseText);

                var tab = Ext.getCmp('content-panel');
                var rp = tab.items.map[scopeName + '.' + appName + '.-nh-config'];
                var dataObjectsPane = rp.items.map[scopeName + '.' + appName + '.dataObjectsPane'];
                var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];

                if (dbDict.dataObjects.length > 0) {
                    // populate data source form
                    showTree(dbObjectsTree);
                }
                else {
                    dbObjectsTree.disable();
                    editPane = dataObjectsPane.items.items[1];
                    if (!editPane) {
                        var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
                    }
                    setDsConfigPane(editPane);
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



