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
                var tableSelector = tablesSelectorPane.getForm().findField('tableSelector');      
                var availItems = new Array();
                
                for (var i = 0; i < tableNames.items.length; i++) {
                  var tableName = tableNames.items[i];
                  
                  if (tableName) {                	  
	                var selected = false;
	                  
	                for (var j = 0; j < tableSelector.multiselects[1].store.length; j++) {
	                  if (tableName == tableSelector.multiselects[1].store[j][1]) {
	                    selected = true;
	                    break;
	                  }
	                }
	                  
	                if (!selected) {
	                  availItems.push([tableName, tableName]);   
	                }
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

       var setRelations = function (form, node) {
       	if (form && node && node.childNodes && node.childNodes.length > 0) {
       		var relations = new Array();
       		var configLabel = scopeName + '.' + appName + '-nh-config-wizard';
       		var dataRelationPan = dataObjectsPane.items.items[1].items.items[6].items.items[1];
       		var gridLabel = scopeName + '.' + appName + '.' + node.id;
       		var i = 0;

       		if (dataRelationPan.items != null) {
       			var gridPan = dataRelationPan.items.map[gridLabel];
       			if (gridPan != null) {
       				gridPan.show();
       				return;
       			}
       		}

       		for (i = 0; i < node.childNodes.length; i++) {
       			var nodeId = node.childNodes[i].id;
       			var deleteButtonData = "<input type=\"image\" src=\"Content/img/16x16/edit-delete.png\" " + "onClick='javascript:deleteNodeRow(\"" + configLabel + "\",\"" + gridLabel + "\",\"" + nodeId + "\",\"" + i + "\")'>";
       			relations.push([node.childNodes[i].text, deleteButtonData]);
       		}
       		var colModel = new Ext.grid.ColumnModel([
					{ id: "relationName", header: "Data Relationship Name", width: 460, dataIndex: 'relationName' },
					{ width: 55, dataIndex: 'deleteButton' }
				]);
       		var dataStore = new Ext.data.Store({
       			autoDestroy: true,
       			proxy: new Ext.data.MemoryProxy(relations),
       			reader: new Ext.data.ArrayReader({}, [
						{ name: 'relationName' },
						{ name: 'deleteButton' }
					])
       		});
       		createRelationGrid(gridLabel, dataRelationPan, colModel, dataStore);
       	}
       };


       var setRelationFields = function (editPane, node) {
       	if (editPane && node) {
       		var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];
       		var dataObjectNode = node.parentNode.parentNode;
       		var propertiesNode = dataObjectNode.attributes.children[1];
       		var relatedObjectName = node.attributes.attributes.relatedObjectName.toUpperCase();
       		var rootNode = dbObjectsTree.getRootNode();
       		var relatedDataObjectNode = rootNode.findChild('text', relatedObjectName);
       		var relatedObjects = new Array();
       		var index_relObj
       		for (var i = 0; i < rootNode.childNodes.length; i++)
       			relatedObjects.push(rootNode.childNodes[i].text);
       		var selectedProperties = new Array();
       		for (var i = 0; i < propertiesNode.children.length; i++)
       			if (!propertiesNode.children[i].hidden)
       				selectedProperties.push(propertiesNode.children[i].text);
       		propertiesNode = relatedDataObjectNode.attributes.children[1];
       		var mappingProperties = new Array();
       		for (var i = 0; i < propertiesNode.children.length; i++)
       			if (!propertiesNode.children[i].hidden)
       				mappingProperties.push(propertiesNode.children[i].text);

       		var relationConfigPanel = new Ext.FormPanel({
       			id: scopeName + '.' + appName + '.relationFieldsForm.' + node.id,
       			labelWidth: 160,
       			bodyStyle: 'padding:15px',
       			border: false,
       			width: 560,
       			height: 200,
       			region: 'north',
       			monitorValid: true,
       			defaults: { anchor: '40%' },
       			items: [{
       				xtype: 'label',
       				fieldLabel: 'Configure Data Relationship',
       				labelSeparator: '',
       				anchor: '100%'
       			}, {
       				xtype: 'textfield',
       				name: 'relationshipName',
       				fieldLabel: 'Relationship Name',
       				value: node.text.toUpperCase(),
       				allowBlank: false
       			}, {
       				xtype: 'textfield',
       				name: 'objectName',
       				fieldLabel: 'Object Name',
       				value: dataObjectNode.text,
       				readOnly: true,
       				allowBlank: false
       			}, {
       				xtype: 'combo',
       				name: 'relatedObjectName',
       				triggerAction: 'all',
       				editable: false,
       				mode: 'local',
       				fieldLabel: 'Related Object Name',
       				store: relatedObjects,
       				displayField: 'text',
       				valueField: 'value',
       				selectOnFocus: true,
       				listeners: { 'select': function (combo, record, index) {
       					relatedObject = record.data.field1;
       				}
       				}
       			}, {
       				xtype: 'combo',
       				name: 'relationType',
       				triggerAction: 'all',
       				mode: 'local',
       				fieldLabel: 'Relation Type',
       				store: [['1', 'OneToOne'], ['2', 'OneToMany']],
       				displayField: 'text',
       				valueField: 'value',
       				listeners: { 'select': function (combo, record, index) {
       					relationType = record.data.field1;
       				}
       				}
       			}, {
       				layout: 'column',
       				border: false,
       				defaults: {
       					layout: 'form',
       					border: false,
       					xtype: 'panel',
       					bodyStype: 'padding:0 18px 0 0'
       				},
       				items: [{
       					//left column
       					columnWidth: 0.8,
       					defaults: { anchor: '100%' },
       					items: [{
       						xtype: 'combo',
       						name: 'propertyName',
       						triggerAction: 'all',
       						mode: 'local',
       						fieldLabel: 'Property Name',
       						store: selectedProperties,
       						displayField: 'text',
       						valueField: 'value',
       						selectOnFocus: true,
       						listeners: { 'select': function (combo, record, index) {
       							selectproperty = record.data.field1;
       						}
       						}
       					}, {
       						xtype: 'combo',
       						name: 'mapPropertyName',
       						triggerAction: 'all',
       						mode: 'local',
       						fieldLabel: 'Mapping Property',
       						store: mappingProperties,
       						displayField: 'text',
       						valueField: 'value',
       						selectOnFocus: true,
       						listeners: { 'select': function (combo, record, index) {
       							mapproperty = record.data.field1;
       						}
       						}
       					}]
       				}, {
       					//right column
       					columnWidth: 0.2,
       					defaults: {
       						anchor: '100%',
       						bodyStype: 'paddingLeft: 100px'
       					},
       					items: [{
       						xtype: 'label',
       						fieldLabel: 'l',
       						itemCls: 'white-label',
       						labelSeparator: ''
       					}, {
       						xtype: 'button',
       						text: 'Add',
       						handler: function (button) {
       							var relationConfigForm = relationConfigPanel.getForm();
       							var selectProperty = relationConfigForm.findField("propertyName").getValue().replace(/^\s*/, "").replace(/\s*$/, "");
       							var mapProperty = relationConfigForm.findField("mapPropertyName").getValue().replace(/^\s*/, "").replace(/\s*$/, "");
       							if (selectProperty == "" && mapProperty == "")
       								return;

       							var configLabel = scopeName + '.' + appName + '-nh-config-wizard';
       							var dataRelationPan = dataObjectsPane.items.items[1].items.items[7].items.items[1];
       							var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];
       							var gridLabel = scopeName + '.' + appName + '.' + dbObjectsTree.getSelectionModel().getSelectedNode().id;
       							var gridPan = dataRelationPan.items.map[gridLabel];
       							var myArray = new Array();
       							var i = 0;
       							if (gridPan != null) {
       								var mydata = gridPan.store.data.items;
       								for (i = 0; i < mydata.length; i++)
       									if (mydata[i].data.property == selectProperty && mydata[i].data.relatedProperty == mapProperty)
       										return;
       									else {
       										myArray.push([mydata[i].data.property, mydata[i].data.relatedProperty, mydata[i].data.deleteButton]);
       									}
       							}

       							var arrayData = new Array();
       							arrayData.push(selectProperty);
       							arrayData.push(mapProperty);

       							var deleteButtonData = "<input type=\"image\" src=\"Content/img/16x16/edit-delete.png\" " + "onClick='javascript:deleteRow(\"" + configLabel + "\",\"" + gridLabel + "\",\"" + i + "\")'>";
       							arrayData.push(deleteButtonData);
       							myArray.push(arrayData);

       							var colModel = new Ext.grid.ColumnModel([
								{ id: 'property', header: 'Property', width: 230, dataIndex: 'property' },
								{ header: 'Related Property', width: 230, dataIndex: 'relatedProperty' },
								{ width: 55, dataIndex: 'deleteButton' }
							]);

       							var dataStore = new Ext.data.Store({
       								autoDestroy: true,
       								proxy: new Ext.data.MemoryProxy(myArray),
       								reader: new Ext.data.ArrayReader({}, [
									{ name: 'property' },
									{ name: 'relatedProperty' },
									{ name: 'deleteButton' }
								])
       							});

       							createRelationGrid(gridLabel, dataRelationPan, colModel, dataStore);
       						}
       					}]
       				}]
       			}]
       		});

       		var dataRelationPane = new Ext.Panel({
       			id: 'data-relation-panel',
       			region: 'center',
       			autoScroll: true,
       			border: false
       		});

       		var relationPane = new Ext.Panel({
       			id: 'relation-panel',
       			layout: 'border',
       			border: false,
       			items: [relationConfigPanel, dataRelationPane]
       		});

       		editPane.add(relationPane);
       		editPane.getLayout().setActiveItem(editPane.items.length - 1);


       		relationConfigPanel.getForm().findField('relatedObjectName').setValue(relatedObjectName);
       		relationConfigPanel.getForm().findField('relationType').setValue(node.attributes.attributes.relationshipTypeIndex);

       		var propertyMaps = node.attributes.attributes.propertyMap;
       		if (propertyMaps.length == 0)
       			return;

       		var configLabel = scopeName + '.' + appName + '-nh-config-wizard';
       		var gridLabel = scopeName + '.' + appName + '.' + node.id;
       		if (dataRelationPane.items != null) {
       			var gridPan = dataRelationPane.items.map[gridLabel];
       			if (gridPan != null) {
       				gridPan.show();
       				return;
       			}
       		}

       		var myArray = new Array();
       		var i = 0;
       		for (i = 0; i < propertyMaps.length; i++) {
       			var deleteButtonData = "<input type=\"image\" src=\"Content/img/16x16/edit-delete.png\" " + "onClick='javascript:deleteRow(\"" + configLabel + "\",\"" + gridLabel + "\",\"" + i + "\")'>";
       			myArray.push([propertyMaps[i].dataPropertyName.toUpperCase(), propertyMaps[i].relatedPropertyName.toUpperCase(), deleteButtonData]);
       		}
       		var colModel = new Ext.grid.ColumnModel([
					{ id: 'property', header: 'Property', width: 230, dataIndex: 'property' },
					{ header: 'Related Property', width: 230, dataIndex: 'relatedProperty' },
					{ width: 55, dataIndex: 'deleteButton' }
				]);
       		var dataStore = new Ext.data.Store({
       			autoDestroy: true,
       			proxy: new Ext.data.MemoryProxy(myArray),
       			reader: new Ext.data.ArrayReader({}, [
						{ name: 'property' },
						{ name: 'relatedProperty' },
						{ name: 'deleteButton' }
					])
       		});
       		createRelationGrid(gridLabel, dataRelationPane, colModel, dataStore);
       	}
       };

       var relationCreatePanel = new Ext.FormPanel({
       	labelWidth: 160,
       	bodyStyle: 'padding:15px',
       	border: false,
       	width: 560,
       	height: 100,
       	region: 'north',
       	monitorValid: true,
       	defaults: { anchor: '40%' },
       	items: [{
       		xtype: 'label',
       		fieldLabel: 'Add/delete Data Relationship',
       		labelSeparator: '',
       		anchor: '100%'
       	}, {
       		layout: 'column',
       		border: false,
       		defaults: {
       			layout: 'form',
       			border: false,
       			bodyStype: 'padding:0 18px 0 0'
       		},
       		items: [{
       			//left column
       			columnWidth: 0.8,
       			defaults: { anchor: '100%' },
       			items: [{
       				xtype: 'textfield',
       				name: 'relationName',
       				fieldLabel: 'Data Relationship Name',
       				allowBlank: false
       			}]
       		}, {
       			//right column
       			columnWidth: 0.2,
       			defaults: {
       				anchor: '100%',
       				bodyStype: 'paddingLeft: 100px'
       			},
       			items: [{
       				xtype: 'button',
       				text: 'Add',
       				handler: function (button) {
       					var relationName = relationCreatePanel.getForm().findField("relationName").getValue().replace(/^\s*/, "").replace(/\s*$/, "");
       					if (relationName == "")
       						return;

       					var configLabel = scopeName + '.' + appName + '-nh-config-wizard';
       					var dataRelationPan = dataObjectsPane.items.items[1].items.items[6].items.items[1];
       					var selectNode = dataObjectsPane.items.items[0].items.items[0].getSelectionModel().getSelectedNode();
       					var gridLabel = scopeName + '.' + appName + '.' + selectNode.id;
       					var gridPan = dataRelationPan.items.map[gridLabel];
       					var myArray = new Array();
       					var i = 0;
       					if (gridPan != null) {
       						var mydata = gridPan.store.data.items;
       						for (i = 0; i < mydata.length; i++)
       							if (mydata[i].data.relationName == relationName)
       								return;
       							else {
       								myArray.push([mydata[i].data.relationName, mydata[i].data.deleteButton]);
       							}
       					}

       					var newNode = new Ext.tree.TreeNode({ text: relationName, type: 'relationship', leaf: true });
       					selectNode.appendChild(newNode);
       					var nodeId = newNode.id;
       					var arrayData = new Array();
       					arrayData.push(relationName);

       					var deleteButtonData = "<input type=\"image\" src=\"Content/img/16x16/edit-delete.png\" " + "onClick='javascript:deleteNodeRow(\"" + configLabel + "\",\"" + gridLabel + "\",\"" + nodeId + "\",\"" + i + "\")'>";
       					arrayData.push(deleteButtonData);
       					myArray.push(arrayData);

       					var colModel = new Ext.grid.ColumnModel([
								{ id: "relationName", header: "Data Relationship Name", width: 460, dataIndex: 'relationName' },
								{ width: 55, dataIndex: 'deleteButton' }
							]);

       					var dataStore = new Ext.data.Store({
       						autoDestroy: true,
       						proxy: new Ext.data.MemoryProxy(myArray),
       						reader: new Ext.data.ArrayReader({}, [
									{ name: 'relationName' },
									{ name: 'deleteButton' }
								])
       					});

       					createRelationGrid(gridLabel, dataRelationPan, colModel, dataStore);
       				}
       			}]
       		}]
       	}]
       });

       var deleteDataRelationPane = new Ext.Panel({
       	id: 'data-relation-delete-panel',
       	region: 'center',
       	autoScroll: true,
       	border: false
       });

    var tablesSelectorPane = new Ext.FormPanel({
      labelWidth: 200,
      frame: true,
      bodyStyle: 'padding:15px',
      monitorValid: true,
      items: [{
        xtype: 'itemselector',
        name: 'tableSelector',
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
            dbProvider: dsConfigForm.findField('dbProvider').getValue(),
            dbServer: dsConfigForm.findField('dbServer').getValue(),
            dbInstance: dsConfigForm.findField('dbInstance').getValue(),
            dbName: dsConfigForm.findField('dbName').getValue(),
            dbSchema: dsConfigForm.findField('dbSchema').getValue(),
            dbUserName: dsConfigForm.findField('dbUserName').getValue(),
            dbPassword: dsConfigForm.findField('dbPassword').getValue(),
            tableNames: tablesSelForm.findField('tableSelector').getValue()
          };
          
          dataObjectsPane.items.items[1].hide();
          
          dbObjectsTree.getRootNode().reload(
            function (rootNode) {
            	var relationTypeStr = ['OneToOne', 'OneToMany'];

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
                       for (var j = 0; j < dataObject.dataRelationships.length; j++) {
                       	var newNode = new Ext.tree.TreeNode({
                       		text: dataObject.dataRelationships[j].relationshipName.toLowerCase(),
                       		type: 'relationship',
                       		leaf: true,
                       		relatedObjectName: dataObject.dataRelationships[j].relatedObjectName.toLowerCase(),
                       		relationshipType: relationTypeStr[dataObject.dataRelationships[j].relationshipType],
                       		relationshipTypeIndex: dataObject.dataRelationships[j].relationshipType,
                       		propertyMap: [[]]
                       	});

                       	var mapArray = new Array();
                       	for (var jj = 0; jj < dataObject.dataRelationships[j].propertyMaps.length; jj++) {
                       		var mapItem = new Array();
                       		mapItem['dataPropertyName'] = dataObject.dataRelationships[j].propertyMaps[jj].dataPropertyName.toLowerCase();
                       		mapItem['relatedPropertyName'] = dataObject.dataRelationships[j].propertyMaps[jj].relatedPropertyName.toLowerCase();
                       		mapArray.push(mapItem);
                       	}
                       	newNode.attributes.propertyMap = mapArray;
                       	//selectNode.parentNode.appendChild(newNode);
                       	relationshipsNode.expanded = true;
                       	relationshipsNode.children.push(newNode);
                       	relationshipsNode.children[j].hidden = false;
                       }
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
							var nodeType = node.attributes.type;
							if (nodeType == null)
								nodeType = node.attributes.attributes.type;

//            if (node.attributes.type) {
//              editPane.show();
//              var editPaneLayout = editPane.getLayout();
//              switch (node.attributes.type.toUpperCase()) { 

							if (nodeType) {
								editPane.show();
								var editPaneLayout = editPane.getLayout();

								switch (nodeType.toUpperCase()) {
               
                case 'DATAOBJECT':
                  editPaneLayout.setActiveItem(0);
                  break;
                  
                case 'KEYS':
                  var form = editPane.items.items[1].getForm();
                  var itemSelector = form.findField('keySelector');      
                  var availItems = new Array();
                  
                  for (var i = 0; i < node.childNodes.length; i++) {
                    var keyName = node.childNodes[i].text;
                    availItems.push([keyName, keyName]);
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
                  itemSelector.treeNode = node;
                  
                  editPaneLayout.setActiveItem(3);
                  break;
                  
                case 'DATAPROPERTY':
                  var form = editPane.items.items[4].getForm();                
                  setDataPropertyFields(form, node.attributes.properties);
                  form.treeNode = node;
                  editPaneLayout.setActiveItem(4);
                  break;
                /*  
                case 'RELATIONSHIPS':
                  var form = editPane.items.items[5].getForm();
                  var itemSelector = form.findField('relationshipSelector');      
                  var availItems = new Array();
                  
                  for (var i = 0; i < node.childNodes.length; i++) {
                    var itemName = node.childNodes[i].text;
                    availItems.push([itemName, itemName]);
                  }
                    
                  itemSelector.multiselects[0].store = availItems; 
                  itemSelector.multiselects[1].store = [];
                  
                  editPaneLayout.setActiveItem(5);
                  break;
                  
                case 'RELATIONSHIP':
                  editPaneLayout.setActiveItem(6);
                  break;
								*/

                case 'RELATIONSHIPS':
                var form = editPane.items.items[6].items.items[0].getForm();
                setRelations(form, node);
                editPaneLayout.setActiveItem(6);
                break;

                case 'RELATIONSHIP':
                setRelationFields(editPane, node);
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
          xtype: 'form', //0
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
          xtype: 'form', //1
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
          xtype: 'form',  //2
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
          xtype: 'form', //3
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
            treeNode: null,
            listeners: {
              change: function(itemSelector, selectedValuesStr) {
                var selectedValues = selectedValuesStr.split(',');
                var treeNode = itemSelector.treeNode;
                
                if (treeNode.text.toLowerCase() == 'properties') {     
                  for (var i = 0; i < treeNode.childNodes.length; i++) {
                    var found = false;
                    
                    for (var j = 0; j < selectedValues.length; j++) {                    
                      if (selectedValues[j].toLowerCase() == treeNode.childNodes[i].text.toLowerCase()) {
                        found = true;
                        break;
                      }                        
                    }
                    
                    if (!found) 
                      treeNode.childNodes[i].getUI().hide();
                    else
                      treeNode.childNodes[i].getUI().show();
                  }
                }
              }
            }
          }]
        }, {					
          xtype: 'form', //4
          name: 'dataProperty',
          monitorValid: true,
          labelWidth: 160,
          defaults: {xtype: 'textfield', allowBlank: false, anchor: '60%'},
          items: [dataPropFields],
          treeNode: null,
          buttonAlign: 'center',
          buttons: [{
            text: 'Apply',
            handler: function(f) {
              var form = dataObjectsPane.items.items[1].getLayout().activeItem.getForm();
              var treeNodeProps = form.treeNode.attributes.properties;
              
              treeNodeProps['propertyName'] = form.findField('propertyName').getValue();
              treeNodeProps['dataType'] = form.findField('dataType').getValue();
              treeNodeProps['dataLength'] = form.findField('dataLength').getValue();
              treeNodeProps['nullable'] = form.findField('nullable').getValue();
              treeNodeProps['showOnIndex'] = form.findField('showOnIndex').getValue();
              treeNodeProps['numberOfDecimals'] = form.findField('numberOfDecimals').getValue();
            }
          },{
            text: 'Reset',
            handler: function(f) {
              var form = dataObjectsPane.items.items[1].getLayout().activeItem.getForm();
              form.reset();
            }
          }]
        },{
          xtype: 'form', // 5
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
        } ,{             	
          xtype: 'panel', //6
          id: 'create-relation-panel',
          layout: 'border',
          border: false,
          items: [relationCreatePanel, deleteDataRelationPane]
        }]/*{
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
             },*/       
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
          var tableSelector = tablesSelectorPane.getForm().findField('tableSelector');      
          var selectedItems = new Array();
          
          for (var i = 0; i < dbDict.dataObjects.length; i++) {
            var dataObject = dbDict.dataObjects[i];
            selectedItems.push([dataObject.tableName, dataObject.tableName]);
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

  function createRelationGrid(label, dataGridPanel, colModel, dataStore) {
  	dataStore.on('load', function () {
  		if (dataGridPanel.items != null) {
  			var gridtab = dataGridPanel.items.map[label];
  			if (gridtab != null) {
  				gridtab.destroy();
  			}
  		}

  		var dataRelationGridPane = new Ext.grid.GridPanel({
  			id: label,
  			width: 560,
  			height: 400,
  			store: dataStore,
  			stripeRows: true,
  			frame: true,
  			autoScroll: true,
  			border: false,
  			cm: colModel,
  			selModel: new Ext.grid.RowSelectionModel({ singleSelect: true }),
  			enableColLock: true
  		});

  		dataGridPanel.add(dataRelationGridPane);
  		dataGridPanel.doLayout();
  	});

  	dataStore.load();
  }

  function deleteNodeRow(configLabel, gridLabel, nodeId, i) {
  	var tab = Ext.getCmp('content-panel');
  	var rp = tab.items.map[configLabel];
  	var dataObjectsPan = rp.items.items[2];
  	var dataRelationPan = dataObjectsPan.items.items[1].items.items[6].items.items[1];
  	var dbObjectsTree = dataObjectsPan.items.items[0].items.items[0];
  	var parent = dbObjectsTree.getSelectionModel().getSelectedNode().parentNode;
  	var deleteNode = dbObjectsTree.getNodeById(nodeId)

  	var children = deleteNode.parentNode.childNodes;
  	for (var ii = 0; ii < children.length; ii++) {
  		if (children[ii].id == nodeId) {
  			children.splice(ii, 1);
  		}
  	};
  	parent.removeChild(deleteNode);

  	var gridtab = dataRelationPan.items.map[gridLabel];
  	gridtab.store.removeAt(i);
  	if (gridtab.store.data.items.length == 0)
  		gridtab.destroy();
  }

  function deleteRow(configLabel, gridLabel, i) {
  	var tab = Ext.getCmp('content-panel');
  	var rp = tab.items.map[configLabel];
  	var dataRelationPan = rp.items.items[2].items.items[1].items.items[7].items.items[1];
  	var dataObjectsPan = rp.items.items[2];
  	var dbObjectsTree = dataObjectsPan.items.items[0].items.items[0];
  	var selectNode = dbObjectsTree.getSelectionModel().getSelectedNode();

  	selectNode.attributes.attributes.propertyMap.splice(i, 1);

  	var gridtab = dataRelationPan.items.map[gridLabel];
  	gridtab.store.removeAt(i);
  	if (gridtab.store.data.items.length == 0)
  		gridtab.destroy();
  }
