Ext.ns('AdapterManager');

AdapterManager.NHibernateConfigWizard = Ext.extend(Ext.Container, {
  scope: null,
  app: null,

  constructor: function(config) {
    config = config || {};

    var wizard = this;
    var scopeName = config.scope.Name;
    var appName = config.app.Name;
    var dbDict;
    
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

    var dsConfigPane = new Ext.FormPanel({
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
                var tableSelector = tablesSelectorPane.getForm().findField('tableNames');      
                var availItems = new Array();
                
                for (var i = 0; i < tableNames.items.length; i++) {
                  var tableName = tableNames.items[i];
                  var selected = false;
                  
                  for (var j = 0; j < tableSelector.multiselects[1].store.length; j++) {
                    if (tableName == tableSelector.multiselects[1].store[j][1]) {
                      selected = true;
                      break;
                    }
                  }
                  
                  if (!selected) {
                    availItems[i] = [tableName, tableName];   
                  }
                }
                  
                tableSelector.multiselects[0].store = availItems;
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

    var tablesSelectorPane = new Ext.FormPanel({
      labelWidth: 200,
      frame: true,
      bodyStyle: 'padding:15px',
      monitorValid: true,
      items: [{
        xtype: 'itemselector',
        name: 'tableNames',
        fieldLabel: 'Select Tables',
        imagePath: 'scripts/ext-3.3.1/examples/ux/images/',
        multiselects: [{
          width: 250,
          height: 300,
          store: [[]],
          displayField: 'tableName',
          valueField: 'tableValue'
        },{
          width: 250,
          height: 300,
          store: [[]],
          displayField: 'tableName',
          valueField: 'tableValue'
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
          var dsConfigForm = dsConfigPane.getForm();
          var tablesSelForm = tablesSelectorPane.getForm();          
          var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];
          var treeLoader = dbObjectsTree.getLoader();
          
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
          
          dataObjectsPane.items.items[1].hide();
          
          dbObjectsTree.getRootNode().reload(
            function(rootNode) {
              // sync data object tree with data dictionary
              for (var i = 0; i < rootNode.childNodes.length; i++) {
                var dataObjectNode = rootNode.childNodes[i];
                
                for (var ii = 0; ii < dbDict.dataObjects.length; ii++) {
                  var dataObject = dbDict.dataObjects[ii];
                  
                  if (dataObject.objectName.toLowerCase() == dataObjectNode.text.toLowerCase()) {
                    var keysNode = dataObjectNode.attributes.children[0];
                    var propertiesNode = dataObjectNode.attributes.children[1];
                    var relationshipsNode = dataObjectNode.attributes.children[2];
                    
                    //TODO: sync key properties
                    
                    // sync data properties
                    for (var j = 0; j < propertiesNode.children.length; j++) {
                      for (var jj = 0; jj < dataObject.dataProperties.length; jj++) {
                        if (propertiesNode.children[j].text.toLowerCase() == 
                          dataObject.dataProperties[jj].propertyName.toLowerCase()) {
                          propertiesNode.children[j].hidden = false;
                        }
                      }
                    }
                    
                    //TODO: sync relationships
                  }
                }
              }              
            }
          );
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
    
    var dataObjectsPane = new Ext.Panel({
      layout: 'border',
      frame: true,
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
          enableDD: false,
          containerScroll: true,
          rootVisible: true,
          root: {
            text: 'Data Objects'
          },
          loader: new Ext.tree.TreeLoader(),
          listeners: {
            click: function(node, e) {     
              var editPane = dataObjectsPane.items.items[1];
            
              if (node.attributes.type) {
                editPane.show();
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
                    
                  itemSelector.multiselects[0].store = availItems; 
                  itemSelector.multiselects[1].store = [];
                  
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
                  var selectedItems = new Array();
                  
                  node.expand();
                  
                  for (var i = 0; i < node.childNodes.length; i++) {
                    var itemName = node.childNodes[i].text;
                    
                    if (node.childNodes[i].hidden == false)
                      selectedItems.push([itemName, itemName]);
                    else
                      availItems.push([itemName, itemName]);
                  }
                    
                  itemSelector.multiselects[0].store = availItems; 
                  itemSelector.multiselects[1].store = selectedItems;
                  itemSelector.selectedTreeNode = node;
                  
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
                    
                  itemSelector.multiselects[0].store = availItems; 
                  itemSelector.multiselects[1].store = [];
                  
                  editPaneLayout.setActiveItem(5);
                  break;
                  
                case 'RELATIONSHIP':
                  editPaneLayout.setActiveItem(6);
                  break;
                }
              }
              else {
                editPane.hide();
              }
            } 
          }
        }]  
      },{
        xtype: 'panel',
        name: 'editor-panel',
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
            imagePath: 'scripts/ext-3.3.1/examples/ux/images/',
            multiselects: [{
              width: 250,
              height: 300,
              store: [[]],
              displayField: 'keyName',
              valueField: 'keyValue'
            },{
              width: 250,
              height: 300,
              store: [[]],
              displayField: 'keyName',
              valueField: 'keyValue'
            }]
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
            imagePath: 'scripts/ext-3.3.1/examples/ux/images/',
            multiselects: [{
              width: 250,
              height: 300,
              store: [[]],
              displayField: 'propertyName',
              valueField: 'propertyValue'
            },{
              width: 250,
              height: 300,
              store: [[]],
              displayField: 'propertyName',
              valueField: 'propertyValue'
            }],
            selectedTreeNode: null,
            listeners: {
              change: function(itemSelector, selectedValuesStr) {
                var selectedValues = selectedValuesStr.split(',');
                var selectedNode = itemSelector.selectedTreeNode;
                
                if (selectedNode.text.toLowerCase() == 'properties') {     
                  for (var i = 0; i < selectedNode.childNodes.length; i++) {
                    var found = false;
                    
                    for (var j = 0; j < selectedValues.length; j++) {                    
                      if (selectedValues[j].toLowerCase() == selectedNode.childNodes[i].text.toLowerCase()) {
                        found = true;
                        break;
                      }                        
                    }
                    
                    if (!found) 
                      selectedNode.childNodes[i].getUI().hide();
                    else
                      selectedNode.childNodes[i].getUI().show();
                  }
                }
              }
            }
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
            imagePath: 'scripts/ext-3.3.1/examples/ux/images/',
            multiselects: [{
              width: 250,
              height: 300,
              store: [[]],
              displayField: 'relationshipName',
              valueField: 'relationshipValue'
            },{
              width: 250,
              height: 300,
              store: [[]],
              displayField: 'relationshipName',
              valueField: 'relationshipValue'
            }]
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
          dataObjectsPane.items.items[1].hide();
          wizard.getLayout().setActiveItem(formIndex - 1);
        }
      },{
        text: 'Finish',
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
      id: scopeName + '.' + appName + '-nh-config-wizard',
      title: 'NHibernate Config Wizard - ' + scopeName + '.' + appName,
      closable: true,
      layout: 'card',
      activeItem: 0,
      items: [dsConfigPane, tablesSelectorPane, dataObjectsPane]
    });

    Ext.Ajax.request({
      url: 'AdapterManager/DBDictionary',
      method: 'POST',
      params: {
        scope: scopeName,
        app: appName
      },
      success: function(response, request) {
        dbDict = Ext.util.JSON.decode(response.responseText);

        if (dbDict) {
          // populate data source form
          var dsConfigForm = dsConfigPane.getForm();
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
          var tableSelector = tablesSelectorPane.getForm().findField('tableNames');      
          var selectedItems = new Array();
          
          for (var i = 0; i < dbDict.dataObjects.length; i++) {
            var dataObject = dbDict.dataObjects[i];
            selectedItems[i] = [dataObject.tableName, dataObject.tableName];
          }
            
          tableSelector.multiselects[1].store = selectedItems;
        }
      },
      failure: function(response, request) {
        //TODO: use message box
        Ext.Msg.alert('Error ' + response.text);
      }
    });

    AdapterManager.NHibernateConfigWizard.superclass.constructor.apply(this, arguments);
  }
});
