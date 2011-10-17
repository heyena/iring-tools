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
                    title: "Configure SP & ID Data Source",
                    bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
                    monitorValid: true,
                    defaults: { anchor: '100%', xtype: 'textfield', allowBlank: false },
                    items: [

//                    {
//                        xtype: 'fieldset',
//                        title: 'Configure SP & ID Data Source',
//                        labelSeparator: '',
//                        itemCls: 'form-title',
//                        labelWidth: 350,
//                        border: 0
//                    },
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
            },
            tbar = new Ext.Toolbar({
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



