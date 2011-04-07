Ext.ns('AdapterManager');

AdapterManager.NHibernateConfigWizard = Ext.extend(Ext.Container, {
  scope: null,
  app: null,
  
  constructor: function(config) {
    config = config || {};
    
    var wizard = this;
    var scopeName = config.scope.Name;
    var appName = config.app.Name;
    
    var providersStore = new Ext.data.JsonStore({
      autoLoad: true,
      autoDestroy: true,
      url: 'AdapterManager/DataProviders',
      root: 'items',
      idProperty: 'Provider',
      fields: [{ 
        name: 'Provider'
      }]
    });
    
    var dsForm = new Ext.FormPanel({
      id: 'ds-form',
      labelWidth: 160,
      bodyStyle: 'padding:15px',
      border: false,
      frame: true,
      monitorValid: true,
      items: [{
        xtype: 'label',
        fieldLabel: 'Configure Data Source',
        labelSeparator: '',
        style: {
          marginBottom: '50px'
        },
        anchor: '100% -100'
      },{
        xtype: 'combo',
        fieldLabel: 'Database Provider',
        hiddenName: 'provider',
        allowBlank: false,
        mode: 'local',
        triggerAction: 'all',
        editable: false,
        store: providersStore,
        displayField: 'Provider',
        valueField: 'Provider',
        anchor: '40%'
      },{
        xtype: 'textfield',
        name: 'dbInstance',
        fieldLabel: 'Database Instance',
        allowBlank: false,       
        anchor: '40%'
      },{
        xtype: 'textfield',
        name: 'dbName',
        fieldLabel: 'Database Name',
        allowBlank: false,       
        anchor: '40%'
      },{
        xtype: 'textfield',
        name: 'schema',
        fieldLabel: 'Schema Name',
        allowBlank: false,
        anchor: '40%'
      },{
        xtype: 'textfield',
        name: 'userId',
        fieldLabel: 'User ID',
        allowBlank: false,
        anchor: '40%'
      },{
        xtype: 'textfield',
        inputType: 'password',
        name: 'password',
        fieldLabel: 'Password',
        allowBlank: false,
        anchor: '40%'
      }],
      buttons: [{
        text: 'Next',
        formBind: true,
        handler: function(button) {
          var form = wizard.getLayout().activeItem;
          var formIndex = wizard.items.indexOf(form);
          
          form.getForm().submit({
            url: 'AdapterManager/DataObjects',
            success: function(f, a) {
              wizard.getLayout().setActiveItem(formIndex + 1);
            },
            failure: function(f, a) {
              Ext.Msg.show({
                title: 'Error',
                msg: a.response.responseText,
                modal: true,
                icon: Ext.Msg.ERROR,
                buttons: Ext.Msg.OK
              });
            },
            params: {
              scope: scopeName,
              app: appName
            },
            waitMsg: 'Processing ...'
          });
        }
      },{
        text: 'Cancel',
        handler: function() {
          wizard.destroy();
        }
      }]
    });

    var importForm = new Ext.FormPanel({
      id: 'import-form',
      labelWidth: 160,
      bodyStyle: 'padding:15px',
      border: false,
      frame: true,
      monitorValid: true,
      items: [{
        xtype: 'label',
        fieldLabel: 'Select tables to import',
        labelSeparator: '',
        style: {
          marginBottom: '50'
        },
        anchor: '100% -100'
      },{
        xtype: 'textfield',
        name: 'table',
        fieldLabel: 'table',
        anchor: '40%'
      }],
      buttons: [{
        text: 'Prev',
        handler: function() {
          var form = wizard.getLayout().activeItem;
          var formIndex = wizard.items.indexOf(form);
          wizard.getLayout().setActiveItem(formIndex - 1);
        }
      },{
        text: 'Next',
        formBind: true,
        handler: function() {
          var form = wizard.getLayout().activeItem;
          var formIndex = wizard.items.indexOf(form);
          wizard.getLayout().setActiveItem(formIndex + 1);
        }
      },{
        text: 'Cancel',
        handler: function() {
          wizard.destroy();
        }
      }]
    });
    
    var dictionaryForm = new Ext.FormPanel({
      id: 'dictionary-form',
      labelWidth: 160,
      bodyStyle: 'padding:15px',
      border: false,
      frame: true,
      items: [{
        xtype: 'label',
        fieldLabel: 'Configure Database Dictionary',
        labelSeparator: '',
        style: {
          marginBottom: '50'
        },
        anchor: '100% -100'
      },{
        xtype: 'textfield',
        name: 'table',
        fieldLabel: 'table',
        anchor: '40%'
      }],
      buttons: [{
        text: 'Prev',
        handler: function() {
          var form = wizard.getLayout().activeItem;
          var formIndex = wizard.items.indexOf(form);
          wizard.getLayout().setActiveItem(formIndex - 1);
        }
      },{
        text: 'Next',
        formBind: true,
        handler: function() {
          var form = wizard.getLayout().activeItem;
          var formIndex = wizard.items.indexOf(form);
          wizard.getLayout().setActiveItem(formIndex + 1);
        }
      },{
        text: 'Cancel',
        handler: function() {
          wizard.destroy();
        }
      }]
    });
    
    Ext.apply(this, {
      id: 'nh-config-wizard',
      title: 'NHibernate Configuration Wizard',
      closable: true,
      layout: 'card',
      activeItem: 0,
      items: [dsForm, importForm, dictionaryForm]
    });
    
    Ext.Ajax.request({
      url: 'AdapterManager/DatabaseDictionary',
      method: 'POST',
      params: {
        scope: scopeName,
        app: appName
      },
      success: function(response, request) {
        var dbDict = Ext.util.JSON.decode(response.responseText);
        
        if (dbDict) {
          var form = dsForm.getForm();
          var connStr = dbDict.ConnectionString;
          var connStrParts = connStr.split(';');
          
          for (var i = 0; i < connStrParts.length; i++) {
            var pair = connStrParts[i].split('=');
            
            switch (pair[0].toUpperCase()) {
              case 'DATA SOURCE':
                form.findField('dbInstance').setValue(pair[1]);
                break;              
              case 'INITIAL CATALOG':
                form.findField('dbName').setValue(pair[1]);
                break;          
              case 'USER ID':
                form.findField('userId').setValue(pair[1]);
                break;          
              case 'PASSWORD':
                form.findField('password').setValue(pair[1]);
                break;
            }
          }
          
          form.findField('provider').setValue(dbDict.Provider);          
          form.findField('schema').setValue(dbDict.SchemaName);
        }
      },
      failure: function(response, request) {
        Ext.Msg.alert('error');
      }
    });

    AdapterManager.NHibernateConfigWizard.superclass.constructor.apply(this, arguments);
  }
});
