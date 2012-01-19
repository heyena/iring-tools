﻿Ext.define('AM.view.nhibernate.ConnectDatabase', {
    extend: 'Ext.form.Panel',
    alias: 'widget.connectDatabase',
    labelWidth: 150,
    frame: false,
    border: false,
    autoScroll: true,
    scopeName: null,
    appName: null,
    bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
    monitorValid: true,
    defaults: { anchor: '100%', xtype: 'textfield', allowBlank: false },

    initComponent: function () {
        var scopeName = this.scopeName;
        var appName = this.appName;

        Ext.define('AM.model.provider', {
            extend: 'Ext.data.Model',
            fields: ['name']
        });

        Ext.define('AM.store.providersStore', {
            extend: 'Ext.data.Store',
            model: 'AM.model.provider',
            autoLoad: true,
            autoDestroy: true,

            proxy: {
                type: 'ajax',
                timeout: 600000,
                url: 'AdapterManager/DBProviders',
                reader: {
                    type: 'json',
                    root: 'items',
                    idProperty: 'Provider',
                    successProperty: 'success'
                }
            }
        });

        this.items = [
            {
                xtype: 'label',
                fieldLabel: 'Configure Data Source',
                labelSeparator: '',
                itemCls: 'form-title'
            }, {
                xtype: 'combo',
                fieldLabel: 'Database Provider',
                hiddenName: 'dbProvider',
                allowBlank: false,
                store: 'AM.store.providersStore',
                mode: 'local',
                editable: false,
                value: 'MsSql2008',
                triggerAction: 'all',
                displayField: 'Provider',
                valueField: 'Provider',
                listeners: { 'select': function (combo, record, index) {
                    var dbProvider = record.data.Provider.toUpperCase();
                    var dsConfigPane = this.up('form').getForm();
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
            }];

        this.tbar = new Ext.Toolbar({
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
                            dbTableNames = Ext.JSON.decode(a.response.responseText);
                            var tab = Ext.getCmp('content-panel');
                            var rp = tab.items.map[scopeName + '.' + appName + '.-nh-config'];
                            var dataObjectsPane = rp.items.map[scopeName + '.' + appName + '.dataObjectsPane'];
                            var editPane = dataObjectsPane.items.map[scopeName + '.' + appName + '.editor-panel'];
                            var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];
                            dbObjectsTree.disable();
                            setTablesSelectorPane(editPane, dbInfo, dbDict, scopeName, appName);
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
        });
    }


});