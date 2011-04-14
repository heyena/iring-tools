Ext.ns('AdapterManager');

AdapterManager.NHibernateConfigWizard = Ext.extend(Ext.Container, {
  scope: null,
  app: null,

  constructor: function(config) {
    config = config || {};

    var wizard = this;
    var scopeName = config.scope.Name;
    var appName = config.app.Name;
    
    var setDataPropertyFields = function(form, properties) {
      if (form && properties) {
        form.findField('columnName').setValue(properties.columnName);
        form.findField('propertyName').setValue(properties.propertyName);
        form.findField('dataType').setValue(properties.dataType);
        form.findField('dataLength').setValue(properties.dataLength);
        
        if (properties.nullable.toUpperCase() == 'TRUE') {
          form.findField('nullable').setValue(true);
        }
        else {
          form.findField('nullable').setValue(false);
        }
        
        if (properties.showOnIndex.toUpperCase() == 'TRUE') {
          form.findField('showOnIndex').setValue(true);
        }
        else {
          form.findField('showOnIndex').setValue(false);
        }
          
        form.findField('numberOfDecimals').setValue(properties.numberOfDecimals);
      }
    };
    
    var providersStore = new Ext.data.JsonStore({
      autoLoad: true,
      autoDestroy: true,
      url: 'AdapterManager/DBProviders',
      root: 'items',
      idProperty: 'Provider',
      fields: [{
        name: 'Provider'
      }]
    });

    var dsConfigPanel = new Ext.FormPanel({
      labelWidth: 160,
      frame: true,
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
          
          form.getForm().submit({
            url: 'AdapterManager/TableNames',
            timeout: 600000,
            params: {
              scope: scopeName,
              app: appName
            },
            success: function(f, a) {
              var tableNames = Ext.util.JSON.decode(a.response.responseText);
              
              if (tableNames.items.length > 0) {
                // populate available tables  
                var tableSelector = tablesSelectionPanel.getForm().findField('tableNames');      
                var availItems = new Array();
                
                for (var i = 0; i < tableNames.items.length; i++) {
                  var tableName = tableNames.items[i];
                  var selected = false;
                  
                  for (var j = 0; j < tableSelector.toData.length; j++) {
                    if (tableName == tableSelector.toData[j][1]) {
                      selected = true;
                      break;
                    }
                  }
                  
                  if (!selected) {
                    availItems[i] = [tableName, tableName];   
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
            waitMsg: 'Loading ...'
          });
        }
      },{
        text: 'Cancel',
        handler: function() {
          wizard.destroy();
        }
      }]
    });

    var tablesSelectionPanel = new Ext.FormPanel({
      labelWidth: 200,
      frame: true,
      bodyStyle: 'padding:15px',
      monitorValid: true,
      items: [{
        xtype: 'itemselector',
        name: 'tableNames',
        fieldLabel: 'Select Tables',
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
          var dsConfigForm = dsConfigPanel.getForm();
          var tablesSelForm = tablesSelectionPanel.getForm();          
          var dbObjectsTree = tablesConfigPanel.items.items[0].items.items[0];
          var treeLoader = dbObjectsTree.getLoader();
          
          //var editPane = tablesConfigPanel.items.items[1];
          //editPane.getLayout().setActiveItem(0);
          
          treeLoader.dataUrl = 'AdapterManager/DBObjects';
          treeLoader.baseParams = {
            scope: scopeName,
            app: appName,
            dbProvider: dsConfigForm.findField("dbProvider").getValue(),
            dbServer: dsConfigForm.findField("dbServer").getValue(),
            dbInstance: dsConfigForm.findField("dbInstance").getValue(),
            dbName: dsConfigForm.findField("dbName").getValue(),
            dbSchema: dsConfigForm.findField("dbSchema").getValue(),
            dbUserName: dsConfigForm.findField("dbUserName").getValue(),
            dbPassword: dsConfigForm.findField("dbPassword").getValue(),
            tableNames: tablesSelForm.findField("tableNames").getValue()
          };
          
          dbObjectsTree.getRootNode().reload();
          wizard.getLayout().setActiveItem(formIndex + 1);
        }
      },{
        text: 'Cancel',
        handler: function() {
          wizard.destroy();
        }
      }]
    });

    var dataPropFields = [{
      name: 'columnName',
      fieldLabel: 'Column Name',
      disabled: true
    },{
      name: 'propertyName',
      fieldLabel: 'Property Name'
    },{
      name: 'dataType',
      fieldLabel: 'Data Type'
    },{
      xtype: 'numberfield',
      name: 'dataLength',
      fieldLabel: 'Data Length'
    },{
      xtype: 'checkbox',
      name: 'nullable',
      fieldLabel: 'Nullable'
    },{
      xtype: 'checkbox',
      name: 'showOnIndex',
      fieldLabel: 'Show on Index'
    },{
      xtype: 'numberfield',
      name: 'numberOfDecimals',      
      fieldLabel: 'Number of Decimals'
    }];
    
    var tablesConfigPanel = new Ext.Panel({
      layout: 'border',
      frame: true,
      items: [{
        xtype: 'panel',
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
          enableDD: false,
          containerScroll: true,
          rootVisible: true,
          root: {
            text: 'Data Objects'
          },
          loader: new Ext.tree.TreeLoader(),
          listeners: {
            click: function(node, e) {
              var editPane = tablesConfigPanel.items.items[1];
              var editPaneLayout = editPane.getLayout();
              
              switch (node.attributes.type.toUpperCase()) {                
              case 'DATAOBJECT':
                editPaneLayout.setActiveItem(0);
                break;
                
              case 'KEYS':
                var form = editPane.items.items[1].getForm();
                var itemSelector = form.findField('keySelector');      
                var availItems = new Array();
                
                for (var i = 0; i < node.childNodes.length; i++) {
                  var keyName = node.childNodes[i].text;
                  availItems[i] = [keyName, keyName];
                }
                  
                itemSelector.fromData = availItems; 
                itemSelector.toData = [];
                
                editPaneLayout.setActiveItem(1);
                break;
                
              case 'KEYPROPERTY':
                var form = editPane.items.items[2].getForm(); 
                form.findField('keyType').setValue(node.attributes.properties.keyType.toLowerCase());
                setDataPropertyFields(form, node.attributes.properties);                
                editPaneLayout.setActiveItem(2);
                break;
                
              case 'PROPERTIES':
                var form = editPane.items.items[3].getForm();
                var itemSelector = form.findField('propertySelector');      
                var availItems = new Array();
                
                for (var i = 0; i < node.childNodes.length; i++) {
                  var itemName = node.childNodes[i].text;
                  availItems[i] = [itemName, itemName];
                }
                  
                itemSelector.fromData = availItems; 
                itemSelector.toData = [];
                
                editPaneLayout.setActiveItem(3);
                break;
                
              case 'DATAPROPERTY':
                var form = editPane.items.items[4].getForm();                
                setDataPropertyFields(form, node.attributes.properties);
                editPaneLayout.setActiveItem(4);
                break;
                
              case 'RELATIONSHIPS':
                var form = editPane.items.items[5].getForm();
                var itemSelector = form.findField('relationshipSelector');      
                var availItems = new Array();
                
                for (var i = 0; i < node.childNodes.length; i++) {
                  var itemName = node.childNodes[i].text;
                  availItems[i] = [itemName, itemName];
                }
                  
                itemSelector.fromData = availItems; 
                itemSelector.toData = [];
                
                editPaneLayout.setActiveItem(5);
                break;
                
              case 'RELATIONSHIP':
                editPaneLayout.setActiveItem(6);
                break;
              }
            } 
          }
        }]  
      },{
        xtype: 'panel',
        region: 'center',
        layout: 'card',
        bodyStyle: 'background:#eee;padding:15px',
        items: [{                
          xtype: 'form',
          name: 'dataObject',
          monitorValid: true,
          labelWidth: 160,
          defaults: {xtype: 'textfield', anchor:'60%'},
          items: [{
            fieldLabel: 'Table Name',
            value: 'Table Name',
            disabled: true
          },{
            fieldLabel: 'Object Name',
            value: 'Object Name'
          },{
            fieldLabel: 'Key Delimiter',
            value: ','
          }]
        },{
          xtype: 'form',
          items: [{
            xtype: 'itemselector',
            name: 'keySelector',
            fieldLabel: 'Select Keys',
            imagePath: 'scripts/ext-3.3.1/ux/multiselect/',
            fromLegend: 'Available',
            toLegend: 'Selected',
            msWidth: 250,
            msHeight: 300,
            dataFields: ['keyName', 'keyValue'],
            displayField: 'keyName',
            valueField: 'keyValue'
          }]
        },{
          xtype: 'form',
          name: 'keyProperty',
          monitorValid: true,
          labelWidth: 160,
          defaults: {xtype: 'textfield', allowBlank: false, anchor: '60%'},
          items: [dataPropFields, {
            xtype: 'combo',
            name: 'keyType',
            fieldLabel: 'Key Type',
            store: new Ext.data.SimpleStore({
              fields: ['value', 'name'],
              data: [['assigned', 'Assigned'], ['unassigned', 'Unassigned']]
            }),
            displayField: 'name',
            valueField: 'value',
            mode: 'local'
          }]
        },{
          xtype: 'form',
          items: [{
            xtype: 'itemselector',
            name: 'propertySelector',
            fieldLabel: 'Select Properties',
            imagePath: 'scripts/ext-3.3.1/ux/multiselect/',
            fromLegend: 'Available',
            toLegend: 'Selected',
            msWidth: 250,
            msHeight: 300,
            dataFields: ['propertyName', 'propertyValue'],
            displayField: 'propertyName',
            valueField: 'propertyValue'
          }]
        },{
          xtype: 'form',
          name: 'dataProperty',
          monitorValid: true,
          labelWidth: 160,
          defaults: {xtype: 'textfield', allowBlank: false, anchor: '60%'},
          items: [dataPropFields]
        },{
          xtype: 'form',
          items: [{
            xtype: 'itemselector',
            name: 'relationshipSelector',
            fieldLabel: 'Select Relationships',
            imagePath: 'scripts/ext-3.3.1/ux/multiselect/',
            fromLegend: 'Available',
            toLegend: 'Selected',
            msWidth: 250,
            msHeight: 300,
            dataFields: ['relationshipName', 'relationshipValue'],
            displayField: 'relationshipName',
            valueField: 'relationshipValue'
          }]
        },{
          xtype: 'form',
          name: 'relationship',
          monitorValid: true,
          labelWidth: 160,
          defaults: {xtype: 'textfield', allowBlank: false, anchor: '60%'},
          items: [{
            xtype: 'combo',
            name: 'relatedObject',
            fieldLabel: 'Related Object',
            store: new Ext.data.SimpleStore({
              fields: ['value', 'name'],
              data: [['r1', 'related object 1'], ['r2', 'related object2']]
            }),
            displayField: 'name',
            valueField: 'value',
            mode: 'local'
          },{
            xtype: 'combo',
            name: 'relationshipType',
            fieldLabel: 'Relationship Type',
            store: new Ext.data.SimpleStore({
              fields: ['value', 'name'],
              data: [['OneToOne', 'One To One'], ['OneToMany', 'One To Many']]
            }),
            displayField: 'name',
            valueField: 'value',
            mode: 'local'
          },{
            layout: 'column',
            defaults: {
              columnWidth: 0.5
            },
            items: [{
              xtype: 'combo',
              name: 'object properties',
              fieldLabel: 'Property Maps',
              store: new Ext.data.SimpleStore({
                fields: ['value', 'name'],
                data: [['Prop1', 'p1'], ['Prop2', 'p2'], ['Prop3', 'p3'], ['Prop4', 'p4'], ['Prop5', 'p5']],
                autoLoad: false,
                listeners: {
                  load: function(e){
                  }
                }
              }),
              displayField: 'name',
              valueField: 'value',
              mode: 'local'
            },{
              xtype: 'combo',
              name: 'related properties',
              fieldLabel: '',
              tpl: tpl,
              store: new Ext.data.SimpleStore({
                fields: ['value', 'name'],
                data: [['Prop1a', 'p1a'], ['Prop2a', 'p2a'], ['Prop3a', 'p3a']]
              }),
              displayField: 'name',
              valueField: 'value',
              mode: 'local'
            }]
          }]
        }]
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
    
    var tpl = new Ext.XTemplate('<tpl for="."><div class="x-combo-list-item normalwhite">{MYFIELDNAME}</div></tpl>');

    Ext.apply(this, {
      id: scopeName + '.' + appName + '-nh-config-wizard',
      title: 'NHibernate Config Wizard - ' + scopeName + '.' + appName,
      closable: true,
      layout: 'card',
      activeItem: 0,
      items: [dsConfigPanel, tablesSelectionPanel, tablesConfigPanel]
    });

    Ext.Ajax.request({
      url: 'AdapterManager/DBDictionary',
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
          var tableSelector = tablesSelectionPanel.getForm().findField('tableNames');      
          var selectedItems = new Array();
          
          for (var i = 0; i < dbDict.dataObjects.length; i++) {
            var dataObject = dbDict.dataObjects[i];
            selectedItems[i] = [dataObject.tableName, dataObject.tableName];
          }
            
          tableSelector.toData = selectedItems;
        }
      },
      failure: function(response, request) {
        //TODO: show more info
        Ext.Msg.alert('error');
      }
    });

    AdapterManager.NHibernateConfigWizard.superclass.constructor.apply(this, arguments);
  }
});
