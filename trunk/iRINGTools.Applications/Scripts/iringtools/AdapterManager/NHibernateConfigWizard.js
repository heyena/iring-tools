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
			id: scopeName + '.' + appName + '.dsconfigPane',
			frame: false,
			bodyStyle: 'padding:15px',
			monitorValid: true,
			defaults: { anchor: '100%' },
			items: [{
				defaults: {
					xtype: 'fieldset',
					layout: 'form',
					anchor: '100%'
				},
				items: [{
					title: 'Configure Data Source',
					defaults: {
						anchor: '100%',
						allowBlank: false
					},
					items: [{
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
						text: 'Connect',
						formBind: true,
						handler: function (button) {
							dsConfigPane.getForm().submit({
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
										var tab = Ext.getCmp('content-panel');
										var rp = tab.items.map[scopeName + '.' + appName + '.-nh-config-wizard'];
										var dataObjectsPane = rp.items.map[scopeName + '.' + appName + '.dataObjectsPane'];
										var editPane = dataObjectsPane.items.map[scopeName + '.' + appName + '.editor-panel'];
										editPane.add(tablesSelectorPane);
										editPane.getLayout().setActiveItem(editPane.items.length - 1);
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
							var tab = Ext.getCmp('content-panel');
							var rp = tab.items.map[scopeName + '.' + appName + '.-nh-config-wizard'];
							var dataObjectsPane = rp.items.map[scopeName + '.' + appName + '.dataObjectsPane'];
							var editPane = dataObjectsPane.items.map[scopeName + '.' + appName + '.editor-panel'];
							var dsconfigPanel = editPane.items.map[scopeName + '.' + appName + '.dsconfigPane'];
							dsconfigPanel.hide();
						}
					}]
				}]
			}]
		});

		var setRelations = function (editPane, node) {
			if (editPane && node) {
				if (editPane.items.map['create-relation-panel']) {
					var crePane = editPane.items.map['create-relation-panel'];
					var relationConfigPane = crePane.items.map[scopeName + '.' + appName + '.relationCreateForm.' + node.id];
					if (relationConfigPane) {
						var panelIndex = editPane.items.indexOf(crePane);
						editPane.getLayout().setActiveItem(panelIndex);
						relationConfigPane.show();
						return;
					}
				}

				var relationCreateFormPanel = new Ext.FormPanel({
					labelWidth: 140,
					id: scopeName + '.' + appName + '.relationCreateForm.' + node.id,
					border: false,
					height: 100,
					region: 'north',
					monitorValid: true,
					items: [{
						defaults: {
							xtype: 'fieldset',
							layout: 'form',
							anchor: '100%'
						},
						items: [{
							title: 'Add/Delete object relationship',
							autoHeight: true,
							defaults: {
								anchor: '100%',
								allowBlank: false
							},
							items: [{
								defaults: {
									xtype: 'fieldset',
									layout: 'form',
									anchor: '100%'
								},
								items: [{
									defaults: {
										anchor: '100%',
										allowBlank: false
									},
									items: [{
										xtype: 'textfield',
										name: 'relationName',
										fieldLabel: 'Relationship Name',
										allowBlank: false
									}],
									buttons: [{
										text: 'Add',
										handler: function (button) {
											addRelationship(scopeName, appName);
										}
									}]
								}]
							}, {
								xtype: 'panel',
								id: 'data-relation-delete-panel',
								region: 'center',
								layout: 'fit',
								border: true
							}]
						}]
					}]
				});
				editPane.add(relationCreateFormPanel);
				var panelIndex = editPane.items.indexOf(relationCreateFormPanel);
				editPane.getLayout().setActiveItem(panelIndex);
				var deleteDataRelationPane = relationCreateFormPanel.items.items[0].items.items[0].items.items[1];
				var relations = new Array();
				var configLabel = scopeName + '.' + appName + '.-nh-config-wizard';
				var gridLabel = scopeName + '.' + appName + '.' + node.id;
				var i = 0;
				if (deleteDataRelationPane.items) {
					var gridPane = deleteDataRelationPane.items.map[gridLabel];
					if (gridPane) {
						gridPane.show();
						return;
					}
				}

				for (i = 0; i < node.childNodes.length; i++) {
					var nodeId = node.childNodes[i].id;
					relations.push([node.childNodes[i].text, nodeId]);
				}
				var colModel = new Ext.grid.ColumnModel([
  					{ id: "relationName", header: "Data Relationship Name", dataIndex: 'relationName' },
						{ dataIndex: 'nodeId', hidden: true }
  				]);
				var dataStore = new Ext.data.Store({
					autoDestroy: true,
					proxy: new Ext.data.MemoryProxy(relations),
					reader: new Ext.data.ArrayReader({}, [
  						{ name: 'relationName' },
							{ name: 'nodeId' }
  					])
				});
				var callId = 0;
				createRelationGrid(gridLabel, deleteDataRelationPane, colModel, dataStore, scopeName, appName, callId);
			}
		};

		var setRelationFields = function (editPane, node) {
			if (editPane && node) {
				if (editPane.items.map['relation-panel']) {
					var relPane = editPane.items.map['relation-panel'];
					var relationConfigPane = editPane.items.map['relation-panel'].items.map[scopeName + '.' + appName + '.relationFieldsForm.' + node.id];
					if (relationConfigPane) {
						var panelIndex = editPane.items.indexOf(relPane);
						editPane.getLayout().setActiveItem(panelIndex);
						relationConfigPane.show();
						return;
					}
				}

				var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];
				var dataObjectNode = node.parentNode.parentNode;
				var propertiesNode = dataObjectNode.attributes.children[1];
				var relatedObjects = new Array();
				var rootNode = dbObjectsTree.getRootNode();
				for (var i = 0; i < rootNode.childNodes.length; i++)
					relatedObjects.push([i.toString(), rootNode.childNodes[i].text]);
				var selectedProperties = new Array();
				for (var i = 0; i < propertiesNode.children.length; i++)
					if (!propertiesNode.children[i].hidden)
						selectedProperties.push([i.toString(), propertiesNode.children[i].text]);

				if (node.attributes.attributes)
					var nodeAttribute = node.attributes.attributes;
				else
					var nodeAttribute = node.attributes;

				var mappingProperties = new Array();
				var relatedObjectName = nodeAttribute.relatedObjectName.toUpperCase();
				if (relatedObjectName != '') {
					var relatedDataObjectNode = rootNode.findChild('text', relatedObjectName);
					if (relatedDataObjectNode) {
						propertiesNode = relatedDataObjectNode.attributes.children[1];
						for (var i = 0; i < propertiesNode.children.length; i++)
							if (!propertiesNode.children[i].hidden)
								mappingProperties.push([i.toString(), propertiesNode.children[i].text]);
					}
				}
				else {
					mappingProperties.push(['0', '']);
				}

				var relationConfigPanel = new Ext.FormPanel({
					id: scopeName + '.' + appName + '.relationFieldsForm.' + node.id,
					labelWidth: 140,
					border: false,
					height: 350,
					region: 'north',
					monitorValid: true,
					items: [{
						defaults: {
							xtype: 'fieldset',
							layout: 'form',
							anchor: '100%'
						},
						items: [{
							title: 'Configure Data Relationship',
							defaults: {
								anchor: '100%',
								allowBlank: false
							},
							items: [{
								xtype: 'textfield',
								name: 'relationshipName',
								fieldLabel: 'Relationship Name',
								value: node.text.toUpperCase(),
								allowBlank: false,
								listeners: {
									'change': function (field, newValue, oldValue) {
										node.text = newValue.toUpperCase();
									}
								}
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
									var relatedObjectName = record.data.field2;
									var relatedDataObjectNode = rootNode.findChild('text', relatedObjectName);
									if (node.attributes.attributes)
										node.attributes.attributes.relatedObjectName = relatedObjectName;
									else
										node.attributes.relatedObjectName = relatedObjectName;
									propertiesNode = relatedDataObjectNode.attributes.children[1];
									var mappingProperties = new Array();
									var ii = 0;
									// debug purpose
									propertiesNode.children[0].hidden = true;
									propertiesNode.children[1].hidden = true;
									for (var i = 0; i < propertiesNode.children.length; i++)
										if (propertiesNode.children[i].hidden) {
											mappingProperties.push([ii.toString(), propertiesNode.children[i].text]);
											ii++;
										}
									var mapCombo = relationConfigPanel.getForm().findField('mapPropertyName');
									if (mapCombo.store.data) {
										mapCombo.reset();
										mapCombo.store.removeAll();
									}
									mapCombo.store.loadData(mappingProperties);
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
								listeners: { 'select': function (combo, record, index) {
									node.attributes.attributes.relationshipType = record.data.field2;
									node.attributes.attributes.relationshipTypeIndex = record.data.index;
								}
								}
							}, {
								border: false,
								frame: false,
								anchor: '100%',
								items: [{
									defaults: {
										xtype: 'fieldset',
										layout: 'form',
										anchor: '100%',
										labelWidth: 127
									},
									items: [{
										title: 'Add Property Mapping',
										defaults: {
											anchor: '100%',
											allowBlank: false
										},
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
												var selectproperty = record.data.field2;
											}
											}
										}, {
											xtype: 'combo',
											name: 'mapPropertyName',
											fieldLabel: 'Mapping Property',
											store: new Ext.data.SimpleStore({
												fields: ['value', 'text'],
												data: mappingProperties
											}),
											mode: 'local',
											triggerAction: 'all',
											displayField: 'text',
											valueField: 'value',
											selectOnFocus: true,
											forceSelection: true,
											listeners: { 'select': function (combo, record, index) {
												var mapproperty = record.data.field2;
											}
											}
										}],
										buttons: [{
											text: 'Add',
											handler: function (button) {
												addPropertyMapping(scopeName, appName);
											}
										}]
									}]
								}]
							}, {
								xtype: 'panel',
								name: 'dataRelationPane',
								id: 'data-relation-panel',
								region: 'center',
								layout: 'fit',
								border: true
							}]
						}]
					}]
				});
				editPane.add(relationConfigPanel);
				var panelIndex = editPane.items.indexOf(relationConfigPanel);
				editPane.getLayout().setActiveItem(panelIndex);
				var relationConfigForm = relationConfigPanel.getForm();
				var dataRelationPane = relationConfigPanel.items.items[0].items.items[0].items.items[5];
				relationConfigForm.findField('relatedObjectName').setValue(relatedObjectName);
				if (node.attributes.attributes)
					var relationTypeIndex = node.attributes.attributes.relationshipTypeIndex;
				else
					var relationTypeIndex = node.attributes.relationshipTypeIndex;
				relationConfigForm.findField('relationType').setValue(relationTypeIndex);

				if (node.attributes.attributes)
					var propertyMaps = node.attributes.attributes.propertyMap;
				else
					var propertyMaps = node.attributes.propertyMap;

				var configLabel = scopeName + '.' + appName + '.-nh-config-wizard';
				var gridLabel = scopeName + '.' + appName + '.' + node.id;
				if (dataRelationPane.items) {
					var gridPane = dataRelationPane.items.map[gridLabel];
					if (gridPane) {
						gridPane.show();
						return;
					}
				}
				var myArray = new Array();
				var i = 0;
				for (i = 0; i < propertyMaps.length; i++) {
					myArray.push([propertyMaps[i].dataPropertyName.toUpperCase(), propertyMaps[i].relatedPropertyName.toUpperCase()]);
				}
				var colModel = new Ext.grid.ColumnModel([
  					{ id: 'property', header: 'Property', width: 230, dataIndex: 'property' },
  					{ header: 'Related Property', width: 230, dataIndex: 'relatedProperty' }
  				]);
				var dataStore = new Ext.data.Store({
					autoDestroy: true,
					proxy: new Ext.data.MemoryProxy(myArray),
					reader: new Ext.data.ArrayReader({}, [
  						{ name: 'property' },
  						{ name: 'relatedProperty' }
  					])
				});
				var callId = 1;
				createRelationGrid(gridLabel, dataRelationPane, colModel, dataStore, scopeName, appName, callId);
			}
		};

		var tablesSelectorPane = new Ext.FormPanel({
			frame: false,
			id: scopeName + '.' + appName + '.tablesSelectorPane',
			bodyStyle: 'padding:15px',
			monitorValid: true,
			items: [{
				defaults: {
					xtype: 'fieldset',
					layout: 'form',
					anchor: '100%'
				},
				items: [{
					title: 'Select Tables',
					defaults: {
						anchor: '100%',
						allowBlank: false
					},
					items: [{
						xtype: 'itemselector',
						name: 'tableSelector',
						imagePath: 'scripts/ext-3.3.1/examples/ux/images/',
						multiselects: [{
							width: 400,
							height: 300,
							store: [[]],
							displayField: 'tableName',
							valueField: 'tableValue'
						}, {
							width: 400,
							height: 300,
							store: [[]],
							displayField: 'tableName',
							valueField: 'tableValue'
						}]
					}],
					buttons: [{
						text: 'OK',
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

							dataObjectsPane.items.items[1].items.map[scopeName + '.' + appName + '.tablesSelectorPane'].hide();
							var rootNode = dbObjectsTree.getRootNode();
							rootNode.reload(
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
														objectName: dataObjectNode.text,
														relatedObjectName: dataObject.dataRelationships[j].relatedObjectName.toLowerCase(),
														relationshipType: relationTypeStr[dataObject.dataRelationships[j].relationshipType],
														relationshipTypeIndex: dataObject.dataRelationships[j].relationshipType,
														propertyMap: []
													});

													var mapArray = new Array();
													for (var jj = 0; jj < dataObject.dataRelationships[j].propertyMaps.length; jj++) {
														var mapItem = new Array();
														mapItem['dataPropertyName'] = dataObject.dataRelationships[j].propertyMaps[jj].dataPropertyName.toLowerCase();
														mapItem['relatedPropertyName'] = dataObject.dataRelationships[j].propertyMaps[jj].relatedPropertyName.toLowerCase();
														mapArray.push(mapItem);
													}
													newNode.attributes.propertyMap = mapArray;
													relationshipsNode.expanded = true;
													relationshipsNode.children.push(newNode);
													relationshipsNode.children[j].hidden = false;
												}
											}
										}
									}
								}
							);
						}
					}, {
						text: 'Cancel',
						handler: function () {
							dataObjectsPane.items.items[1].items.map[scopeName + '.' + appName + '.tablesSelectorPane'].hide();
						}
					}]
				}]
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
			id: scopeName + '.' + appName + '.dataObjectsPane',
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
					tbar: new Ext.Toolbar({
						items: [{
							xtype: 'tbspacer',
							width: 4
						}, {
							xtype: 'button',
							icon: 'Content/img/16x16/document-properties.png',
							text: 'Configure',
							tooltip: 'Configure Data Source',
							handler: function () {
								editPane = dataObjectsPane.items.items[1];
								if (!editPane) {
									var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
								}
								editPane.add(dsConfigPane);
								editPane.getLayout().setActiveItem(editPane.items.length - 1);
							}
						}, {
							xtype: 'tbspacer',
							width: 4
						}, {
							xtype: 'button',
							icon: 'Content/img/16x16/document-properties.png',
							text: 'Edit',
							tooltip: 'Edit Tables',
							handler: function () {
								editPane = dataObjectsPane.items.items[1];
								if (!editPane) {
									var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
								}
								editPane.add(tablesSelectorPane);
								editPane.getLayout().setActiveItem(editPane.items.length - 1);
							}
						}, {
							xtype: 'tbspacer',
							width: 4
						}, {
							xtype: 'button',
							icon: 'Content/img/16x16/document-properties.png',
							text: 'Save',
							tooltip: 'Save',
							formBind: true,
							handler: function (button) {
								editPane = dataObjectsPane.items.items[1];
								if (!editPane) {
									var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
								}

								var treeProperty = {};
								treeProperty.dataObjects = new Array();
								var dsConfigForm = dsConfigPane.getForm();
								var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];
								var rootNode = dbObjectsTree.getRootNode();

								treeProperty.provider = dsConfigForm.findField('dbProvider').getValue();
								var dbServer = dsConfigForm.findField('dbServer').getValue();
								dbServer = (dbServer == 'localhost' ? '.' : dbServer);
								treeProperty.connectionString = 'Data Source=' + dbServer + '\\' + dsConfigForm.findField('dbInstance').getValue()
								                            + ';Initial Catalog=' + dsConfigForm.findField('dbName').getValue()
																						+ ';User ID=' + dsConfigForm.findField('dbUserName').getValue()
																						+ ';Password=' + dsConfigForm.findField('dbPassword').getValue();
								treeProperty.schemaName = dsConfigForm.findField('dbSchema').getValue();
								treeProperty.IdentityConfiguration = null;
								var keyName;
								for (var i = 0; i < rootNode.childNodes.length; i++) {
									var folderNode = rootNode.childNodes[i];
									var folderNodeProp = folderNode.attributes.properties;
									var folder = {};
									folder.tableName = folderNodeProp.objectName;
									folder.objectNamespace = folderNode.text;
									folder.objectName = folderNodeProp.objectName;
									if (!folderNodeProp.keyDelimeter)
										folder.keyDelimeter = 'null';
									else
										folder.keyDelimeter = folderNodeProp.keyDelimeter;
									folder.keyProperties = new Array();
									folder.dataProperties = new Array();
									folder.dataRelationships = new Array();
									for (var j = 0; j < folderNode.attributes.children.length; j++) {
										var subFolderNode = folderNode.attributes.children[j];
										if (folderNode.childNodes[2])
											var relationFolderNode = folderNode.childNodes[2];
										var subFolderNodeText = subFolderNode.text;
										switch (subFolderNodeText) {
											case 'Keys':
												for (var k = 0; k < subFolderNode.children.length; k++) {
													var keyNode = subFolderNode.children[k];
													var keyProps = {};
													keyProps.keyPropertyName = keyNode.text;
													keyName = keyNode.text;
													folder.keyProperties.push(keyProps);

													var tagProps = {};
													tagProps.columnName = keyNode.text;
													tagProps.propertyName = keyNode.text;
													tagProps.dataType = 10;
													tagProps.dataLength = 100;
													tagProps.isNullable = 'false';
													tagProps.keyType = 1;
													tagProps.showOnIndex = 'false';
													tagProps.numberOfDecimals = 0;
													folder.dataProperties.push(tagProps);
												}
												break;
											case 'Properties':
												for (var k = 0; k < subFolderNode.children.length; k++) {
													var propertyNode = subFolderNode.children[k];
													var propertyNodeProf = propertyNode.properties;
													var props = {};
													props.columnName = propertyNodeProf.columnName;
													props.propertyName = propertyNodeProf.propertyName;
													props.dataType = 10;
													props.dataLength = propertyNodeProf.dataLength;
													props.isNullable = propertyNodeProf.nullable.toLowerCase();
													if (props.columnName == keyName)
														props.keyType = 1;
													else
														props.keyType = 0;
													props.showOnIndex = propertyNodeProf.showOnIndex.toLowerCase();
													props.numberOfDecimals = propertyNodeProf.numberOfDecimals;
													folder.dataProperties.push(props);
												}
												break;
											case 'Relationships':
												if (!relationFolderNode)
													break;
												for (var k = 0; k < relationFolderNode.childNodes.length; k++) {
													var relationNode = relationFolderNode.childNodes[k];
													var relationNodeAttr = relationNode.attributes.attributes;
													if (!relationNodeAttr)
														var relationNodeAttr = relationNode.attributes;
													var relation = {};
													relation.propertyMaps = new Array();
													for (var m = 0; m < relationNodeAttr.propertyMap.length; m++) {
														var propertyPairNode = relationNodeAttr.propertyMap[m];
														var propertyPair = {};

														propertyPair.dataPropertyName = propertyPairNode.dataPropertyName.toUpperCase();
														propertyPair.relatedPropertyName = propertyPairNode.relatedPropertyName.toUpperCase();
														relation.propertyMaps.push(propertyPair);
													}
													relation.relatedObjectName = relationNodeAttr.relatedObjectName.toUpperCase();
													relation.relationshipName = relationNodeAttr.text.toUpperCase();
													relation.relationshipType = relationNodeAttr.relationshipTypeIndex;
													folder.dataRelationships.push(relation);
												}
												break;
										}
									}
									treeProperty.dataObjects.push(folder);
								}

								Ext.Ajax.request({
									url: 'AdapterManager/Trees',
									method: 'POST',
									params: {
										scope: scopeName,
										app: appName,
										tree: JSON.stringify(treeProperty)
									},
									success: function (response, request) {
										//TODO: use message box
										Ext.Msg.alert('Success ' + response.text);
									},
									failure: function (response, request) {
										//TODO: use message box
										Ext.Msg.alert('Error ' + response.text);
									}
								});
							}
						}]
					}),
					listeners: {
						click: function (node, e) {
							if (!node || (node.childNodes.length == 0 && node.isRoot))
								return;
							var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
							if (!editPane)
								editPane = dataObjectsPane.items.items[1];

							var nodeType = node.attributes.type;

							if (!nodeType && node.attributes.attributes)
								nodeType = node.attributes.attributes.type;

							if (nodeType) {
								editPane.show();
								var editPaneLayout = editPane.getLayout();

								switch (nodeType.toUpperCase()) {
									case 'DATAOBJECT':
										var form = editPane.items.items[0].getForm();
										form.findField('tableName').setValue(node.text);

										if (node.attributes.properties) {
											form.findField('objectName').setValue(node.attributes.properties.objectName);
											form.findField('keyDelimiter').setValue(node.attributes.properties.keyDelimiter);
										}

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
				id: scopeName + '.' + appName + '.editor-panel',
				region: 'center',
				layout: 'card',
				bodyStyle: 'background:#eee;padding:15px 15px 40px 15px',
				items: [{
					xtype: 'form', //0
					name: 'dataObject',
					monitorValid: true,
					labelWidth: 160,
					defaults: { xtype: 'textfield', allowBlank: false, width: 300 },
					items: [{
						name: 'tableName',
						fieldLabel: 'Table Name',
						disabled: true
					}, {
						name: 'objectName',
						fieldLabel: 'Object Name'
					}, {
						name: 'keyDelimiter',
						fieldLabel: 'Key Delimiter',
						value: ',',
						allowBlank: true
					}],
					treeNode: null,
					buttonAlign: 'center',
					buttons: [{
						text: 'Apply',
						handler: function (f) {
							var form = dataObjectsPane.items.items[1].getLayout().activeItem.getForm();

							if (form.treeNode) {
								var treeNodeProps = form.treeNode.attributes.properties;

								treeNodeProps['objectName'] = form.findField('objectName').getValue();
								treeNodeProps['keyDelimiter'] = form.findField('keyDelimiter').getValue();
							}
						}
					}, {
						text: 'Reset',
						handler: function (f) {
							var form = dataObjectsPane.items.items[1].getLayout().activeItem.getForm();
							form.reset();
						}
					}]
				}, {
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
				}, {
					xtype: 'form', //2
					name: 'keyProperty',
					monitorValid: true,
					labelWidth: 160,
					defaults: { xtype: 'textfield', allowBlank: false, width: 300 },
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
					}, {
						text: 'Reset',
						handler: function (f) {
							var form = dataObjectsPane.items.items[1].getLayout().activeItem.getForm();
							form.reset();
						}
					}]
				}, {
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
				}, {
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
			}]
		});

		Ext.apply(this, {
			id: scopeName + '.' + appName + '.-nh-config-wizard',
			title: 'NHibernate Config Wizard - ' + scopeName + '.' + appName,
			closable: true,
			layout: 'fit',
			//activeItem: 2,
			items: [dataObjectsPane]
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
					var selectTableNames = new Array();

					if (dbDict.dataObjects.length == 0) {
						editPane = dataObjectsPane.items.items[1];
						if (!editPane) {
							var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
						}
						editPane.add(tablesSelectorPane);
						editPane.getLayout().setActiveItem(editPane.items.length - 1);
					}
					else {
						for (var i = 0; i < dbDict.dataObjects.length; i++) {
							var dataObject = dbDict.dataObjects[i];
							selectedItems.push([dataObject.tableName, dataObject.tableName]);
							selectTableNames.push(dataObject.tableName);
						}
						tableSelector.multiselects[1].store = selectedItems;
					}

					var tab = Ext.getCmp('content-panel');
					var rp = tab.items.map[scopeName + '.' + appName + '.-nh-config-wizard'];
					var dataObjectsPane = rp.items.map[scopeName + '.' + appName + '.dataObjectsPane'];
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
						tableNames: selectTableNames
					};

					var rootNode = dbObjectsTree.getRootNode();
					rootNode.reload(
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
											objectName: dataObjectNode.text,
											relatedObjectName: dataObject.dataRelationships[j].relatedObjectName.toLowerCase(),
											relationshipType: relationTypeStr[dataObject.dataRelationships[j].relationshipType],
											relationshipTypeIndex: dataObject.dataRelationships[j].relationshipType,
											propertyMap: []
										});
										var mapArray = new Array();
										for (var jj = 0; jj < dataObject.dataRelationships[j].propertyMaps.length; jj++) {
											var mapItem = new Array();
											mapItem['dataPropertyName'] = dataObject.dataRelationships[j].propertyMaps[jj].dataPropertyName.toLowerCase();
											mapItem['relatedPropertyName'] = dataObject.dataRelationships[j].propertyMaps[jj].relatedPropertyName.toLowerCase();
											mapArray.push(mapItem);
										}
										newNode.attributes.propertyMap = mapArray;
										relationshipsNode.expanded = true;
										relationshipsNode.children.push(newNode);
										relationshipsNode.children[j].hidden = false;
									}
								}
							}
						}
					});
				}
			},
			failure: function (response, request) {
				//TODO: use message box
				//Ext.Msg.alert('Error ' + response.text);
				editPane = dataObjectsPane.items.items[1];
				if (!editPane) {
					var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
				}
				editPane.add(dsConfigPane);
				editPane.getLayout().setActiveItem(editPane.items.length - 1);
			}
		});

		AdapterManager.NHibernateConfigWizard.superclass.constructor.apply(this, arguments);
	}
});

function createRelationGrid(label, dataGridPanel, colModel, dataStore, scopeName, appName, callId) {
	dataStore.on('load', function () {
		if (dataGridPanel.items) {
			var gridtab = dataGridPanel.items.map[label];
			if (gridtab) {
				gridtab.destroy();
			}
		}

		var dataRelationGridPane = new Ext.grid.GridPanel({
			id: label,
			height: 400,
			store: dataStore,
			stripeRows: true,
			frame: true,
			autoScroll: true,
			border: true,
			cm: colModel,
			selModel: new Ext.grid.RowSelectionModel({ singleSelect: true }),
			enableColLock: true,
			viewConfig: { forceFit: true },
			tbar: new Ext.Toolbar({
				items: [{
					xtype: 'tbspacer',
					width: 4
				}, {
					xtype: 'button',
					icon: 'Content/img/16x16/edit-delete.png',
					text: 'Delete',
					handler: function (button) {
						var selectModel = dataRelationGridPane.getSelectionModel();
						if (selectModel.hasSelection()) {
							if (callId == 0)
								var nodeId = selectModel.getSelected().get('nodeId');

							var selectIndex = selectModel.getSelectedIndex();
							dataStore.removeAt(selectIndex);
							var tab = Ext.getCmp('content-panel');
							var rp = tab.items.map[scopeName + '.' + appName + '.-nh-config-wizard'];
							var dataObjectsPane = rp.items.map[scopeName + '.' + appName + '.dataObjectsPane'];
							var editorPane = dataObjectsPane.items.items[1];
							var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];

							if (callId == 1) {
								var selectNode = dbObjectsTree.getSelectionModel().getSelectedNode();
								if (selectNode.attributes.attributes)
									selectNode.attributes.attributes.propertyMap.splice(selectIndex, 1);
								else
									selectNode.attributes.propertyMap.splice(selectIndex, 1);
							}
							else {
								var deleteNode = dbObjectsTree.getNodeById(nodeId);
								var parent = deleteNode.parentNode;
								var children = deleteNode.parentNode.childNodes;
								for (var ii = 0; ii < children.length; ii++) {
									if (children[ii].id == nodeId) {
										children.splice(ii, 1);
									}
								};
								parent.removeChild(deleteNode);
							}
						}
						else {
							alert('Please select a row first');
						}

					}
				}]
			})
		});
		dataGridPanel.add(dataRelationGridPane);
		dataGridPanel.doLayout();
	});
	dataStore.load();
}


function showDialog(width, height, title, message, buttons, callback) {
	var style = 'style="margin:0;padding:0;width:' + width + 'px;height:' + height + 'px;border:1px solid #aaa;overflow:auto"';
	Ext.Msg.show({
		title: title,
		msg: '<textarea ' + style + ' readonly="yes">' + message + '</textarea>',
		buttons: buttons,
		fn: callback
	});
}

function addPropertyMapping(scopeName, appName) {
	var tab = Ext.getCmp('content-panel');
	var rp = tab.items.map[scopeName + '.' + appName + '.-nh-config-wizard'];
	var dataObjectsPane = rp.items.map[scopeName + '.' + appName + '.dataObjectsPane'];
	var editorPane = dataObjectsPane.items.items[1];
	var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];
	var selectNode = dbObjectsTree.getSelectionModel().getSelectedNode();
	var relationConfigPanel = editorPane.items.map[scopeName + '.' + appName + '.relationFieldsForm.' + selectNode.id];
	var dataRelationPane = relationConfigPanel.items.items[0].items.items[0].items.items[5]; 
	var relationConfigForm = relationConfigPanel.getForm();
	var selectPropComboBox = relationConfigForm.findField("propertyName");
	var mapPropComboBox = relationConfigForm.findField("mapPropertyName");
	if (!selectPropComboBox.getValue() || !mapPropComboBox.getValue())
		return;

	var selectProperty = selectPropComboBox.store.getAt(selectPropComboBox.getValue()).data.field2.replace(/^\s*/, "").replace(/\s*$/, "");
	var mapProperty = mapPropComboBox.store.getAt(mapPropComboBox.getValue()).data.text.replace(/^\s*/, "").replace(/\s*$/, "");
	if (selectProperty == "" || mapProperty == "")
		return;

	var configLabel = scopeName + '.' + appName + '.-nh-config-wizard';
	var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];
	var gridLabel = scopeName + '.' + appName + '.' + dbObjectsTree.getSelectionModel().getSelectedNode().id;
	if (dataRelationPane.items) {
		var gridPane = dataRelationPane.items.map[gridLabel];
		var myArray = new Array();
		var i = 0;

		if (gridPane) {
			var mydata = gridPane.store.data.items;
			for (i = 0; i < mydata.length; i++)
				if (mydata[i].data.property == selectProperty && mydata[i].data.relatedProperty == mapProperty)
					return;
				else {
					myArray.push([mydata[i].data.property, mydata[i].data.relatedProperty]);
				}
		}
	};

	var arrayData = new Array();
	arrayData.push(selectProperty);
	arrayData.push(mapProperty);
	myArray.push(arrayData);
	var mapItem = new Array();
	mapItem['dataPropertyName'] = selectProperty;
	mapItem['relatedPropertyName'] = mapProperty;
	if (selectNode.attributes.attributes)
		selectNode.attributes.attributes.propertyMap.push(mapItem);
	else
		selectNode.attributes.propertyMap.push(mapItem);

	var colModel = new Ext.grid.ColumnModel([
					{ id: 'property', header: 'Property', dataIndex: 'property' },
					{ header: 'Related Property', dataIndex: 'relatedProperty' }
				]);
	var dataStore = new Ext.data.Store({
		autoDestroy: true,
		proxy: new Ext.data.MemoryProxy(myArray),
		reader: new Ext.data.ArrayReader({}, [
						{ name: 'property' },
						{ name: 'relatedProperty' }
					])
	});
	var callId = 1;
	createRelationGrid(gridLabel, dataRelationPane, colModel, dataStore, scopeName, appName, callId);
}

function addRelationship(scopeName, appName) {
	var tab = Ext.getCmp('content-panel');
	var rp = tab.items.map[scopeName + '.' + appName + '.-nh-config-wizard'];
	var dataObjectsPane = rp.items.map[scopeName + '.' + appName + '.dataObjectsPane'];
	var editorPane = dataObjectsPane.items.items[1];
	var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];
	var selectNode = dbObjectsTree.getSelectionModel().getSelectedNode();
	var relationCreateFormPanel = editorPane.items.map[scopeName + '.' + appName + '.relationCreateForm.' + selectNode.id];
	var deleteDataRelationPane = relationCreateFormPanel.items.items[0].items.items[0].items.items[1];
	var relationName = relationCreateFormPanel.getForm().findField("relationName").getValue().replace(/^\s*/, "").replace(/\s*$/, "");
	if (relationName == "")
		return;

	var configLabel = scopeName + '.' + appName + '.-nh-config-wizard';
	var gridLabel = scopeName + '.' + appName + '.' + selectNode.id;
	if (deleteDataRelationPane.items) {
		var gridPane = deleteDataRelationPane.items.map[gridLabel];
		var myArray = new Array();
		var i = 0;
		if (gridPane) {
			var mydata = gridPane.store.data.items;

			for (i = 0; i < mydata.length; i++) {
				if (mydata[i].data.relationName == relationName) {
					return;
				}
				else {
					myArray.push([mydata[i].data.relationName, mydata[i].data.nodeId]);
				}
			}
		}
	}

	var newNode = new Ext.tree.TreeNode({
		text: relationName.toLowerCase(),
		type: 'relationship',
		leaf: true,
		objectName: selectNode.parentNode.text,
		relatedObjectName: '',
		relationshipType: 'OneToOne',
		relationshipTypeIndex: '1',
		propertyMap: []
	});

	selectNode.appendChild(newNode);
	var nodeId = newNode.id;
	myArray.push([relationName, nodeId]);
	var colModel = new Ext.grid.ColumnModel([
  								{ id: "relationName", header: "Data Relationship Name", dataIndex: 'relationName' },
									{ dataIndex: 'nodeId', hidden: true }
  							]);
	var dataStore = new Ext.data.Store({
		autoDestroy: true,
		proxy: new Ext.data.MemoryProxy(myArray),
		reader: new Ext.data.ArrayReader({}, [
  									{ name: 'relationName' },
										{ name: 'nodeId' }
  								])
	});
	var callId = 0;
	createRelationGrid(gridLabel, deleteDataRelationPane, colModel, dataStore, scopeName, appName, callId);
}

Ext.grid.RowSelectionModel.override({
	getSelectedIndex: function () {
		return this.grid.store.indexOf(this.selections.itemAt(0));
	}
});
