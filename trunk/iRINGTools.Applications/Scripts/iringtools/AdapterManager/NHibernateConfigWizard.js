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

    var dsConfigPanel = new Ext.FormPanel({
      labelWidth: 160,
      bodyStyle: 'padding:15px',
      monitorValid: true,
      defaults: {anchor: '40%'},
      items: [{
        xtype: 'label',
        fieldLabel: 'Configure Data Source',
        labelSeparator: '',
        anchor: '100% -100'
      }, {
        xtype: 'combo',
        fieldLabel: 'Database Provider',
        hiddenName: 'dbProvider',
        allowBlank: false,
        mode: 'local',
        triggerAction: 'all',
        editable: false,
        store: providersStore,
        displayField: 'Provider',
        valueField: 'Provider'
      },{
        xtype: 'textfield',
        name: 'dbServer',
        fieldLabel: 'Database Server',
        allowBlank: false
      },{
        xtype: 'textfield',
        name: 'dbInstance',
        fieldLabel: 'Database Instance',
        allowBlank: false
      },{
        xtype: 'textfield',
        name: 'dbName',
        fieldLabel: 'Database Name',
        allowBlank: false
      },{
        xtype: 'textfield',
        name: 'dbSchema',
        fieldLabel: 'Schema Name',
        allowBlank: false
      },{
        xtype: 'textfield',
        name: 'dbUserName',
        fieldLabel: 'User Name',
        allowBlank: false
      },{
        xtype: 'textfield',
        inputType: 'password',
        name: 'dbPassword',
        fieldLabel: 'Password',
        allowBlank: false
      }],
      buttons: [{
        text: 'Next',
        formBind: true,
        handler: function(button) {
          var form = wizard.getLayout().activeItem;
          var formIndex = wizard.items.indexOf(form);
          var dsConfigForm = dsConfigPanel.getForm();
          
          form.getForm().submit({
            url: 'AdapterManager/SchemaObjects',
            timeout: 600000,
            params: {
              scope: scopeName,
              app: appName
            },
            success: function(f, a) {
              var schemaObjects = Ext.util.JSON.decode(a.response.responseText);
              
              if (schemaObjects.items.length > 0) {
                // populate available tables  
                var tableSelector = tablesConfigPanel.getForm().findField('tableSelector');      
                var availItems = new Array();
                
                for (var i = 0; i < schemaObjects.items.length; i++) {
                  var schemaObject = schemaObjects.items[i];
                  var selected = false;
                  
                  for (var j = 0; j < tableSelector.toData.length; j++) {
                    if (schemaObject == tableSelector.toData[j][1]) {
                      selected = true;
                      break;
                    }
                  }
                  
                  if (!selected) {
                    availItems[i] = [schemaObject, schemaObject];   
                  }
                }
                  
                tableSelector.fromData = availItems;            
                wizard.getLayout().setActiveItem(formIndex + 1);
              }
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

    var tablesConfigPanel = new Ext.FormPanel({
      labelWidth: 200,
      bodyStyle: 'padding:15px',
      monitorValid: true,
      items: [{
        xtype: 'itemselector',
        name: 'tableSelector',
        fieldLabel: 'Configure Tables',
        imagePath: 'scripts/ext-3.3.1/ux/multiselect/',
        fromLegend: 'Available',
        toLegend: 'Selected',
        msWidth: 250,
        msHeight: 300,
        dataFields: ['tableName', 'tableValue'],
        displayField: 'tableName',
        valueField: 'tableValue'
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

    var tablesEditingPanel = new Ext.FormPanel({
      labelWidth: 160,
      bodyStyle: 'padding:15px',
      items: [{
        xtype: 'label',
        fieldLabel: 'Configure Database Dictionary',
        labelSeparator: '',
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
      items: [dsConfigPanel, tablesConfigPanel, tablesEditingPanel]
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
          // populate data source form
          var dsConfigForm = dsConfigPanel.getForm();
          var connStr = dbDict.ConnectionString;
          var connStrParts = connStr.split(';');

          for ( var i = 0; i < connStrParts.length; i++) {
            var pair = connStrParts[i].split('=');

            switch (pair[0].toUpperCase()) {
            case 'DATA SOURCE':
              var dsValue = pair[1].split('\\');
              var dbServer = (dsValue[0] == '.' ? 'localhost' : dsValue[0]);
              dsConfigForm.findField('dbServer').setValue(dbServer);
              dsConfigForm.findField('dbInstance').setValue(dsValue[1]);
              break;
            case 'INITIAL CATALOG':
              dsConfigForm.findField('dbName').setValue(pair[1]);
              break;
            case 'USER ID':
              dsConfigForm.findField('dbUserName').setValue(pair[1]);
              break;
            case 'PASSWORD':
              dsConfigForm.findField('dbPassword').setValue(pair[1]);
              break;
            }
          }

          dsConfigForm.findField('dbProvider').setValue(dbDict.Provider);
          dsConfigForm.findField('dbSchema').setValue(dbDict.SchemaName);

          // populate selected tables
          var tableSelector = tablesConfigPanel.getForm().findField('tableSelector');      
          var selectedItems = new Array();
          
          for (var i = 0; i < dbDict.dataObjects.length; i++) {
            var dataObject = dbDict.dataObjects[i];
            selectedItems[i] = [dataObject.tableName, dataObject.tableName];
          }
            
          tableSelector.toData = selectedItems;
        }
      },
      failure: function(response, request) {
        Ext.Msg.alert('error');
      }
    });

    AdapterManager.NHibernateConfigWizard.superclass.constructor.apply(this, arguments);
  }
});
