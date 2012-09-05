Ext.define('AM.view.nhibernate.ConnectDatabase', {
  extend: 'Ext.form.Panel',
  alias: 'widget.connectdatabase',
  frame: false,
  border: false,
  dirNode: null,
  autoScroll: true,
  contextName: null,
  endpoint: null,
  baseUrl: null,
  bodyStyle: 'background:#eee;padding:10px 0px 0px 10px',
  monitorValid: true,

  initComponent: function () {
    var me = this;
    var contextName = this.contextName;
    var endpoint = this.endpoint;
    var baseUrl = this.baseUrl;
    var dirNode = this.dirNode;
    var dbInfo = dirNode.data.record.dbInfo;
    var dbDict = dirNode.data.record.dbDict;

    this.items = [
        {
          xtype: 'label',
          text: 'Configure Data Source',
          anchor: '100%',
          cls: 'x-form-item',
          style: 'font-weight:bold;'
        },
        {
          xtype: 'combobox',
          labelWidth: 150,
          fieldLabel: 'Database Provider',
          anchor: '100%',
          hiddenName: 'dbProvider',
          name: 'dbProvider',
          editable: false,
          mode: 'local',
          value: 'MsSql2008',
          triggerAction: 'all',
          displayField: 'Provider',
          valueField: 'Provider',
          store: Ext.create('Ext.data.Store', {
            model: 'AM.model.ProviderModel',
            listeners: {
              beforeLoad: function (store, action) {
                store.proxy.extraParams.baseUrl = baseUrl;
              }
            }
          }),
          listeners: { 'select': function (combo, record) {
            var dbProvider = record[0].data.Provider.toUpperCase();
            var dbName = me.getForm().findField('dbName');
            var portNumber = me.getForm().findField('portNumber');
            var host = me.getForm().findField('host');
            var dbServer = me.getForm().findField('dbServer');
            var dbInstance = me.getForm().findField('dbInstance');
            var serviceName = me.items.items[10];
            var dbSchema = me.getForm().findField('dbSchema');
            var userName = me.getForm().findField('dbUserName');
            var password = me.getForm().findField('dbPassword');

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
                    creatRadioField(serviceName, dbInfo.dbInstance, dbInfo.serName, contextName, endpoint);
                    host.show();
                    userName.setValue(dbInfo.dbUserName);
                    password.setValue(dbInfo.dbPassword);
                    dbSchema.setValue(dbDict.SchemaName);
                  }
                  else
                    changeConfigOracle(host, dbSchema, userName, password, serviceName, contextName, endpoint);
                }
                else
                  changeConfigOracle(host, dbSchema, userName, password, serviceName, contextName, endpoint);

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
        },
        {
          xtype: 'textfield',
          anchor: '100%',
          labelWidth: 150,
          name: 'dbServer',
          fieldLabel: 'Database Server',
          value: 'localhost',
          allowBlank: false
        },
        {
          xtype: 'textfield',
          anchor: '100%',
          labelWidth: 150,
          name: 'host',
          fieldLabel: 'Host Name',
          hidden: true,
          allowBlank: false
        },
        {
          xtype: 'textfield',
          anchor: '100%',
          labelWidth: 150,
          name: 'portNumber',
          fieldLabel: 'Port Number',
          hidden: true,
          value: '1433',
          allowBlank: false
        },
        {
          xtype: 'textfield',
          name: 'dbInstance',
          anchor: '100%',
          labelWidth: 150,
          fieldLabel: 'Database Instance',
          value: 'default',
          allowBlank: true
        },
        {
          xtype: 'textfield',
          anchor: '100%',
          labelWidth: 150,
          name: 'dbName',
          fieldLabel: 'Database Name',
          allowBlank: false
        },
        {
          xtype: 'textfield',
          anchor: '100%',
          labelWidth: 150,
          name: 'dbUserName',
          fieldLabel: 'User Name',
          allowBlank: false,
          listeners: { 'change': function (field, newValue, oldValue) {
            var dbProvider = me.getForm().findField('dbProvider').getValue().toUpperCase();
            if (dbProvider.indexOf('ORACLE') > -1) {
              var dbSchema = me.getForm().findField('dbSchema');
              dbSchema.setValue(newValue);
              dbSchema.show();
            }
          }
          }
        },
        {
          xtype: 'textfield',
          anchor: '100%',
          labelWidth: 150,
          inputType: 'password',
          name: 'dbPassword',
          fieldLabel: 'Password',
          allowBlank: false
        },
        {
          xtype: 'textfield',
          anchor: '100%',
          labelWidth: 150,
          name: 'dbSchema',
          fieldLabel: 'Schema Name',
          value: 'dbo',
          allowBlank: false
        },
        {
          xtype: 'panel',
          labelWidth: 150,
          layout: 'fit',
          id: contextName + '.' + endpoint + '.servicename',
          bodyStyle: 'background:#eee',
          name: 'serviceName',
          anchor: '100%',
          border: false,
          items: [],
          frame: false
        }
        ];
    this.tbar = new Ext.Toolbar({
      items: [
          {
            xtype: 'tbspacer',
            width: 4
          },
          {
            xtype: 'button',
            icon: 'Content/img/16x16/document-properties.png',
            text: 'Connect',
            tooltip: 'Connect',
            action: 'connecttodatabase'

          },
          {
            xtype: 'tbspacer',
            width: 4
          },
          {
            xtype: 'button',
            icon: 'Content/img/16x16/edit-clear.png',
            text: 'Reset',
            tooltip: 'Reset to the latest applied changes',
            handler: function (f) {
              setDsConfigFields(me, dbInfo, dbDict);
            }
          }
        ]
    });

    this.callParent(arguments);

    if (dbDict.Provider) {
      setDsConfigFields(me, dbInfo, dbDict)
    }
  },

  setActiveRecord: function (record) {
    if (record) {
      this.getForm().setValues(record);
    } else {
      this.getForm().reset();
    }
  }
});

function changeConfigOracle(host, dbSchema, userName, password, serviceName, contextName, endpoint) {
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
  creatRadioField(serviceName, '', '', 1, contextName, endpoint);  
};

function changeConfig(dbName, dbServer, dbInstance, dbSchema, userName, password) {
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
};

function setDsConfigFields(dsConfigPane, dbInfo, dbDict) {
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
      if (Provider.indexOf('ORACLE') > -1)
        setDSOracle(portNumber, host, serviceName, dbServer, dbInstance, dbName, dbProvider, userName, password, dbSchema, dbInfo, dbDict);
      else if (Provider.indexOf('MSSQL') > -1)
        setDSMSSql(portNumber, host, serviceName, dbServer, dbInstance, dbName, dbProvider, userName, password, dbSchema, dbInfo, dbDict);
    }
    else
    //new application setting default value
      setDSDefault(portNumber, host, serviceName, dbServer, dbInstance, dbName, dbProvider, userName, password, dbSchema);
  }
  else {
    //new application setting default value
    setDSDefault(portNumber, host, serviceName, dbServer, dbInstance, dbName, dbProvider, userName, password, dbSchema);
  }
};


function setDSDefault(portNumber, host, serviceName, dbServer, dbInstance, dbName, dbProvider, userName, password, dbSchema) {
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
};

function setDSMSSql(portNumber, host, serviceName, dbServer, dbInstance, dbName, dbProvider, userName, password, dbSchema, dbInfo, dbDict) {
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
};

function setDSOracle(portNumber, host, serviceName, dbServer, dbInstance, dbName, dbProvider, userName, password, dbSchema, dbInfo, dbDict) {
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
  creatRadioField(serviceName, dbInfo.dbInstance, dbInfo.serName);

  portNumber.setValue(dbInfo.portNumber);
  portNumber.show();
};

