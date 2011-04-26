Ext.ns('AdapterManager');

AdapterManager.NHibernateConfigWizard = Ext.extend(Ext.Container, {
	scope: null,
	app: null,

	constructor: function (config) {
		config = config || {};

		var wizard = this;
		var scopeName = config.scope.Name;
		var appName = config.app.Name;
		var dbDict;

		var setDataPropertyFields = function (form, properties) {
			if (form && properties) {
				form.findField('columnName').setValue(properties.columnName);
				form.findField('propertyName').setValue(properties.propertyName);
				form.findField('dataType').setValue(properties.dataType);
				form.findField('dataLength').setValue(properties.dataLength);

				if (properties.nullable == true || properties.nullable == 'True') {
					form.findField('nullable').setValue(true);
				}
				else {
					form.findField('nullable').setValue(false);
				}

				if (properties.showOnIndex == true || properties.showOnIndex == 'True') {
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
			defaults: { anchor: '40%' },
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
        store: providersStore,
				mode: 'local',
        editable: false,
				triggerAction: 'all',
				displayField: 'Provider',
				valueField: 'Provider'
			}, {
				xtype: 'textfield',
				name: 'dbServer',
				fieldLabel: 'Database Server',
				allowBlank: false
			}, {
				xtype: 'textfield',
				name: 'dbInstance',
				fieldLabel: 'Database Instance',
				allowBlank: false
			}, {
				xtype: 'textfield',
				name: 'dbName',
				fieldLabel: 'Database Name',
				allowBlank: false
			}, {
				xtype: 'textfield',
				name: 'dbSchema',
				fieldLabel: 'Schema Name',
				allowBlank: false
			}, {
				xtype: 'textfield',
				name: 'dbUserName',
				fieldLabel: 'User Name',
				allowBlank: false
			}, {
				xtype: 'textfield',
				inputType: 'password',
				name: 'dbPassword',
				fieldLabel: 'Password',
				allowBlank: false
			}],
			buttons: [{
				text: 'Next',
				formBind: true,
				handler: function (button) {
					var form = wizard.getLayout().activeItem;
					var formIndex = wizard.items.indexOf(form);

					form.getForm().submit({
						url: 'AdapterManager/TableNames',
						timeout: 600000,
						params: {
							scope: scopeName,
							app: appName
						},
						success: function (f, a) {
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
						failure: function (f, a) {
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
			}, {
				text: 'Cancel',
				handler: function () {
					wizard.destroy();
				}
			}]
		});

		var setRelations = function (editPane, node) {
			if (editPane && node) {

				if (editPane.items.map['create-relation-panel'] != null) {
					var relationConfigPane = editPane.items.map['create-relation-panel'].items.map[scopeName + '.' + appName + '.relationCreateForm.' + node.id];
					if (relationConfigPane != null) {
						editPane.getLayout().setActiveItem(editPane.items.length - 1);
						return;
					}
				}

				var relationCreateFormPanel = new Ext.FormPanel({
					labelWidth: 160,
					id: scopeName + '.' + appName + '.relationCreateForm.' + node.id,
					bodyStyle: 'padding:15px',
					border: false,
					width: 560,
					height: 100,
					region: 'north',
					monitorValid: true,
					defaults: { anchor: '40%' },
					items: [{
						xtype: 'label',
						fieldLabel: 'Add/Delete object relationship',
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
								fieldLabel: 'Relationship Name',
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
									var relationName = relationCreateFormPanel.getForm().findField("relationName").getValue().replace(/^\s*/, "").replace(/\s*$/, "");
									if (relationName == "")
										return;

									var configLabel = scopeName + '.' + appName + '-nh-config-wizard';
									var dataRelationPane = dataObjectsPane.items.items[1].items.items[5].items.items[1];
									var selectNode = dataObjectsPane.items.items[0].items.items[0].getSelectionModel().getSelectedNode();
									var gridLabel = scopeName + '.' + appName + '.' + selectNode.id;
									var gridPane = dataRelationPane.items.map[gridLabel];
									var myArray = new Array();
									var i = 0;

									if (gridPane != null) {
										var mydata = gridPane.store.data.items;

										for (i = 0; i < mydata.length; i++) {
											if (mydata[i].data.relationName == relationName) {
												return;
											}
											else {
												myArray.push([mydata[i].data.relationName, mydata[i].data.deleteButton]);
											}
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

									createRelationGrid(gridLabel, dataRelationPane, colModel, dataStore);
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

				var relationCreatePane = new Ext.Panel({
					id: 'create-relation-panel',
					layout: 'border',
					border: false,
					items: [relationCreateFormPanel, deleteDataRelationPane]
				});

				editPane.add(relationCreatePane);
				editPane.getLayout().setActiveItem(editPane.items.length - 1);

				var relations = new Array();
				var configLabel = scopeName + '.' + appName + '-nh-config-wizard';
				var gridLabel = scopeName + '.' + appName + '.' + node.id;
				var i = 0;

				if (node.childNodes.length == 0)
					return;
				if (deleteDataRelationPane.items != null) {
					var gridPane = deleteDataRelationPane.items.map[gridLabel];
					if (gridPane != null) {
						gridPane.show();
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
				createRelationGrid(gridLabel, deleteDataRelationPane, colModel, dataStore);
			}
		};

		var setRelationFields = function (editPane, node) {
			if (editPane && node) {
				if (editPane.items.map['relation-panel'] != null) {
					var relationConfigPane = editPane.items.map['relation-panel'].items.map[scopeName + '.' + appName + '.relationFieldsForm.' + node.id];
					if (relationConfigPane != null) {
						editPane.getLayout().setActiveItem(editPane.items.length - 1);
						return;
					}
				}

				var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];
				var dataObjectNode = node.parentNode.parentNode;
				var propertiesNode = dataObjectNode.attributes.children[1];
				var relatedObjects = new Array();
				var rootNode = dbObjectsTree.getRootNode();
				for (var i = 0; i < rootNode.childNodes.length; i++)
					relatedObjects.push(rootNode.childNodes[i].text);
				var selectedProperties = new Array();
				for (var i = 0; i < propertiesNode.children.length; i++)
					if (!propertiesNode.children[i].hidden)
						selectedProperties.push(propertiesNode.children[i].text);

				if (node.attributes.attributes != null) {
					var relatedObjectName = node.attributes.attributes.relatedObjectName.toUpperCase();
					var relatedDataObjectNode = rootNode.findChild('text', relatedObjectName);
					propertiesNode = relatedDataObjectNode.attributes.children[1];
					var mappingProperties = new Array();
					for (var i = 0; i < propertiesNode.children.length; i++)
						if (!propertiesNode.children[i].hidden)
							mappingProperties.push(propertiesNode.children[i].text);
				}

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
            store: relatedObjects,
            mode: 'local',
            editable: false,
						triggerAction: 'all',
						fieldLabel: 'Related Object Name',
						displayField: 'text',
						valueField: 'value',
						selectOnFocus: true,
						listeners: { 'select': function (combo, record, index) {
							var relatedObjectName = record.data.field1;
							var relatedDataObjectNode = rootNode.findChild('text', relatedObjectName);
							propertiesNode = relatedDataObjectNode.attributes.children[1];
							var mappingProperties = new Array();
							// debug purpose
							propertiesNode.children[19].hidden = true;
							propertiesNode.children[20].hidden = true;
							for (var i = 0; i < propertiesNode.children.length; i++)
								if (propertiesNode.children[i].hidden)
									mappingProperties.push(propertiesNode.children[i].text);
							var relationConfigPane = editPane.items.map['relation-panel'].items.map[scopeName + '.' + appName + '.relationFieldsForm.' + node.id];
							relationConfigPane.getForm().findField('mapPropertyName').store = mappingProperties;
						}
						}
					}, {
						xtype: 'combo',
						name: 'relationType',
            fieldLabel: 'Relation Type',
            store: [['1', 'OneToOne'], ['2', 'OneToMany']],
						mode: 'local',
						editable: false,
            triggerAction: 'all',
						displayField: 'text',
						valueField: 'value',
						listeners: { 
					    'select': function (combo, record, index) {
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
                fieldLabel: 'Property Name',
                store: selectedProperties,
                mode: 'local',
                editable: false,
								triggerAction: 'all',
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
                fieldLabel: 'Mapping Property',
                store: mappingProperties,
								mode: 'local',
                editable: false,
                triggerAction: 'all',
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
									var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];
									var gridLabel = scopeName + '.' + appName + '.' + dbObjectsTree.getSelectionModel().getSelectedNode().id;
									var gridPane = dataRelationPane.items.map[gridLabel];
									var myArray = new Array();
									var i = 0;

									if (gridPane != null) {
										var mydata = gridPane.store.data.items;
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
					var gridPane = dataRelationPane.items.map[gridLabel];
					if (gridPane != null) {
						gridPane.show();
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
				}, {
					width: 250,
					height: 300,
					store: [[]],
					displayField: 'tableName',
					valueField: 'tableValue'
				}]
			}],
			buttons: [{
				text: 'Prev',
				handler: function () {
					var form = wizard.getLayout().activeItem;
					var formIndex = wizard.items.indexOf(form);
					wizard.getLayout().setActiveItem(formIndex - 1);
				}
			}, {
				text: 'Next',
				formBind: true,
				handler: function () {
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
			}, {
				text: 'Cancel',
				handler: function () {
					wizard.destroy();
				}
			}]
		});

		var dataPropFields = [{
			name: 'columnName',
			fieldLabel: 'Column Name',
			disabled: true
		}, {
			name: 'propertyName',
			fieldLabel: 'Property Name'
		}, {
			name: 'dataType',
			fieldLabel: 'Data Type'
		}, {
			xtype: 'numberfield',
			name: 'dataLength',
			fieldLabel: 'Data Length'
		}, {
			xtype: 'checkbox',
			name: 'nullable',
			fieldLabel: 'Nullable'
		}, {
			xtype: 'checkbox',
			name: 'showOnIndex',
			fieldLabel: 'Show on Index'
		}, {
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
						click: function (node, e) {
							var editPane = dataObjectsPane.items.items[1];
							var nodeType = node.attributes.type;

							if (node.attributes.type) {
								editPane.show();
								var editPaneLayout = editPane.getLayout();
								
								switch (node.attributes.type.toUpperCase()) {
								case 'DATAOBJECT':
                  var form = editPane.items.items[0].getForm();
                  form.findField('tableName').setValue(node.text);
                  form.findField('objectName').setValue(node.attributes.properties.objectName);
                  form.findField('keyDelimiter').setValue(node.attributes.properties.keyDelimiter);
                  form.treeNode = node;
                  editPaneLayout.setActiveItem(0);
                  break;
                  
                case 'KEYS':
                  var form = editPane.items.items[1].getForm();
                  var itemSelector = form.findField('keySelector');      
                  var availItems = new Array();
                  var selectedItems = new Array();
                  
                  var propertiesNode = node.parentNode.childNodes[1];
                  
                  for (var i = 0; i < propertiesNode.childNodes.length; i++) {
                    var itemName = propertiesNode.childNodes[i].text;
                    var found = false;
                    
                    for (var j = 0; j < node.childNodes.length; j++) {                      
                      if (node.childNodes[j].text == itemName) {
                        found = true;
                        break;
                      }
                    }
                    
                    if (!found) {
                      availItems.push([itemName, itemName]);
                    }
                  }
                  
                  for (var i = 0; i < node.childNodes.length; i++) {                      
                    var keyName = node.childNodes[i].text;
                    selectedItems.push([keyName, keyName]);
                  }                
                    
                  itemSelector.multiselects[0].store = availItems; 
                  itemSelector.multiselects[1].store = selectedItems;
                  itemSelector.treeNode = node;
                  
                  editPaneLayout.setActiveItem(1);
                  break;
                  
                case 'KEYPROPERTY':
                  var form = editPane.items.items[2].getForm(); 
                  setDataPropertyFields(form, node.attributes.properties);
                  form.findField('keyType').setValue(node.attributes.properties.keyType.toLowerCase());
                  form.findField('nullable').disable();
                  form.treeNode = node;
                  editPaneLayout.setActiveItem(2);
                  break;
                
                case 'PROPERTIES':
                  var form = editPane.items.items[3].getForm();
                  var itemSelector = form.findField('propertySelector');   
                  var availItems = new Array();
                  var selectedItems = new Array();
                  
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
                itemSelector.doLayout();
                
                editPaneLayout.setActiveItem(3);
                break;
                
                case 'DATAPROPERTY':
                  var form = editPane.items.items[4].getForm();                
                  setDataPropertyFields(form, node.attributes.properties);
                  form.treeNode = node;
                  editPaneLayout.setActiveItem(4);
                  break;

								case 'RELATIONSHIPS':
									setRelations(editPane, node);
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
			}, {
				xtype: 'panel',
				name: 'editor-panel',
				region: 'center',
				layout: 'card',
				bodyStyle: 'background:#eee;padding:15px 15px 40px 15px',
				items: [{
          xtype: 'form', //0
          name: 'dataObject',
          monitorValid: true,
          labelWidth: 160,
          defaults: {xtype: 'textfield', allowBlank: false, width: 300},
          items: [{
            name: 'tableName',
            fieldLabel: 'Table Name',
            disabled: true
          },{
            name: 'objectName',
            fieldLabel: 'Object Name'
          },{
            name: 'keyDelimiter',
            fieldLabel: 'Key Delimiter',
            value: ',',
            allowBlank: true
          }],
          treeNode: null,
          buttonAlign: 'center',
          buttons: [{
            text: 'Apply',
            handler: function(f) {
              var form = dataObjectsPane.items.items[1].getLayout().activeItem.getForm();
              
              if (form.treeNode) {
                var treeNodeProps = form.treeNode.attributes.properties;
                 
                treeNodeProps['objectName'] = form.findField('objectName').getValue();
                treeNodeProps['keyDelimiter'] = form.findField('keyDelimiter').getValue();
              }
            }
          },{
            text: 'Reset',
            handler: function(f) {
              var form = dataObjectsPane.items.items[1].getLayout().activeItem.getForm();
              form.reset();
            }
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
						}, {
							width: 250,
							height: 300,
							store: [[]],
							displayField: 'keyName',
							valueField: 'keyValue'
						}],
						treeNode: null,
						listeners: {
							change: function (itemSelector, selectedValuesStr) {
								var selectedValues = selectedValuesStr.split(',');
								var keysNode = itemSelector.treeNode;
								var propertiesNode = keysNode.parentNode.childNodes[1];

								// a key has been added, move it to keys node and remove it from properties node
								if (selectedValues.length > keysNode.childNodes.length) {
									// determine the new key
									for (var i = 0; i < selectedValues.length; i++) {
										var found = false;

										for (var j = 0; j < keysNode.childNodes.length; j++) {
											if (keysNode.childNodes[j].text == selectedValues[i]) {
												found = true;
												break;
											}
										}

										// find the node in the properties node and move it to keys node
										if (!found) {
											var newKeyNode;

											for (var jj = 0; jj < propertiesNode.childNodes.length; jj++) {
												if (propertiesNode.childNodes[jj].text == selectedValues[i]) {
													var properties = propertiesNode.childNodes[jj].attributes.properties;
													properties.keyType = 'assigned';
													properties.nullable = false;

													newKeyNode = new Ext.tree.TreeNode({
														text: selectedValues[i],
														type: "keyProperty",
														leaf: true,
														hidden: false,
														properties: properties
													});

													propertiesNode.removeChild(propertiesNode.childNodes[jj], true);
													break;
												}
											}

											if (newKeyNode) {
												keysNode.appendChild(newKeyNode);
											}
										}
									}
								}
								else {  // a key has been deleted, remove it from keys node and add it back to properties node
									// determine the deleted key
									for (var i = 0; i < keysNode.childNodes.length; i++) {
										var found = false;

										for (var j = 0; j < selectedValues.length; j++) {
											if (selectedValues[j] == keysNode.childNodes[i].text) {
												found = true;
												break;
											}
										}

										// find the node in the keys node and move it to properties node
										if (!found) {
											var properties = keysNode.childNodes[i].attributes.properties;
											properties['nullable'] = true;
											delete properties.keyType;

											var propertyNode = new Ext.tree.TreeNode({
												text: keysNode.childNodes[i].text,
												type: "dataProperty",
												leaf: true,
												properties: properties
											});

											propertiesNode.appendChild(propertyNode);
											keysNode.removeChild(keysNode.childNodes[i], true);
										}
									}
								}

								return true;
							}
						}
					}]
				},{
					xtype: 'form', //2
					name: 'keyProperty',
					monitorValid: true,
					labelWidth: 160,
					defaults: { xtype: 'textfield', allowBlank: false, width: 300},
					items: [dataPropFields, {
						xtype: 'combo',
            hiddenName: 'keyType',
						fieldLabel: 'Key Type',
						store: new Ext.data.SimpleStore({
							fields: ['value', 'name'],
							data: [['assigned', 'assigned'], ['unassigned', 'unassigned']]
						}),
            mode: 'local',
            editable: false,
            triggerAction: 'all',
						displayField: 'name',
						valueField: 'value'
					}],
					treeNode: null,
					buttonAlign: 'center',
					buttons: [{
						text: 'Apply',
						handler: function (f) {
  					  var treeNodeProps = form.treeNode.attributes.properties;
              var nullableField = form.findField('nullable');
              nullableField.enable();
              
              treeNodeProps['propertyName'] = form.findField('propertyName').getValue();
              treeNodeProps['dataType'] = form.findField('dataType').getValue();
              treeNodeProps['dataLength'] = form.findField('dataLength').getValue();
              treeNodeProps['nullable'] = false;
              treeNodeProps['showOnIndex'] = form.findField('showOnIndex').getValue();
              treeNodeProps['numberOfDecimals'] = form.findField('numberOfDecimals').getValue();
              treeNodeProps['keyType'] = form.findField('keyType').getValue();
					  }
					},{
						text: 'Reset',
						handler: function (f) {
							var form = dataObjectsPane.items.items[1].getLayout().activeItem.getForm();
							form.reset();
						}
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
						}, {
							width: 250,
							height: 300,
							store: [[]],
							displayField: 'propertyName',
							valueField: 'propertyValue'
						}],
						treeNode: null,
						listeners: {
							change: function (itemSelector, selectedValuesStr) {
								var selectedValues = selectedValuesStr.split(',');
								var treeNode = itemSelector.treeNode;

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
					}]
				},{
					xtype: 'form', //4
					name: 'dataProperty',
					monitorValid: true,
					labelWidth: 160,
					defaults: { xtype: 'textfield', allowBlank: false, width: 300 },
					items: [dataPropFields],
					treeNode: null,
					buttonAlign: 'center',
					buttons: [{
						text: 'Apply',
						handler: function (f) {
							var form = dataObjectsPane.items.items[1].getLayout().activeItem.getForm();

							if (form.treeNode) {
								var treeNodeProps = form.treeNode.attributes.properties;

								treeNodeProps['propertyName'] = form.findField('propertyName').getValue();
								treeNodeProps['dataType'] = form.findField('dataType').getValue();
								treeNodeProps['dataLength'] = form.findField('dataLength').getValue();
								treeNodeProps['nullable'] = form.findField('nullable').getValue();
								treeNodeProps['showOnIndex'] = form.findField('showOnIndex').getValue();
								treeNodeProps['numberOfDecimals'] = form.findField('numberOfDecimals').getValue();
							}
						}
					}, {
						text: 'Reset',
						handler: function (f) {
							var form = dataObjectsPane.items.items[1].getLayout().activeItem.getForm();
							form.reset();
						}
					}]
				}]
			}],
			buttons: [{
				text: 'Prev',
				handler: function () {
					var form = wizard.getLayout().activeItem;
					var formIndex = wizard.items.indexOf(form);
					dataObjectsPane.items.items[1].hide();
					wizard.getLayout().setActiveItem(formIndex - 1);
				}
			}, {
				text: 'Finish',
				formBind: true,
				handler: function () {
					var form = wizard.getLayout().activeItem;
					var formIndex = wizard.items.indexOf(form);
					wizard.getLayout().setActiveItem(formIndex + 1);
				}
			}, {
				text: 'Cancel',
				handler: function () {
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
			success: function (response, request) {
				dbDict = Ext.util.JSON.decode(response.responseText);

				if (dbDict) {
					// populate data source form
					var dsConfigForm = dsConfigPane.getForm();
					var connStr = dbDict.ConnectionString;
					var connStrParts = connStr.split(';');

					for (var i = 0; i < connStrParts.length; i++) {
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
			failure: function (response, request) {
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
	var dataObjectsPane = rp.items.items[2];
	var dataRelationPane = dataObjectsPane.items.items[1].items.items[6].items.items[1];
	var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];
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
	var dataRelationPane = rp.items.items[2].items.items[1].items.items[7].items.items[1];
	var dataObjectsPane = rp.items.items[2];
	var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];
	var selectNode = dbObjectsTree.getSelectionModel().getSelectedNode();

	selectNode.attributes.attributes.propertyMap.splice(i, 1);

	var gridtab = dataRelationPan.items.map[gridLabel];
	gridtab.store.removeAt(i);

	if (gridtab.store.data.items.length == 0)
		gridtab.destroy();
}
