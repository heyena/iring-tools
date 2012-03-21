Ext.define('AM.view.nhibernate.ConnectDatabase', {
  extend: 'Ext.form.Panel',
  alias: 'widget.connectdatabase',
  frame: false,
  border: false,
  dbDict: null,
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
    var dbDict = this.dbDict;
    var baseUrl = this.baseUrl;
    
    this.items = [
        {
          xtype: 'label',
          text: 'Configure Data Source',
          anchor: '100%',
          cls: 'form-title'
        },
        {
          xtype: 'combo',
          labelWidth: 150,
          fieldLabel: 'Database Provider',
          anchor: '100%',
          hiddenName: 'dbProvider',
          name: 'dbProvider',
          allowBlank: false,
          store: Ext.create('Ext.data.Store', {
            model: 'AM.model.ProviderModel',
            listeners: {              
              beforeLoad: function (store, action) {
                store.proxy.extraParams.baseUrl = baseUrl;
              }
            }
          }),
          mode: 'local',
          editable: false,
          value: 'MsSql2008',
          triggerAction: 'all',
          displayField: 'Provider',
          valueField: 'Provider',
          listeners: { 'select': function (combo, record, index) {
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
          value: '1521',
          allowBlank: false
        },
        {
        	xtype: 'textfield',
          name: 'dbInstance',
          anchor: '100%',
          labelWidth: 150,
          fieldLabel: 'Database Instance',
          value: 'default',
          allowBlank: false
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
            action: 'resettables'

          }
        ]
    });
    this.callParent(arguments);
  },

  setActiveRecord: function (record) {
    if (record) {
      this.getForm().setValues(record);
    } else {
      this.getForm().reset();
    }
  }
});

function changeConfigOracle(host, dbSchema, userName, password, serviceName) {
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

