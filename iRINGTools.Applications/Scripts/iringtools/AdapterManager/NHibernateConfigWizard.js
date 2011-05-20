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

		var setKeyProperty = function (editPane, node) {
			if (editPane && node) {
				if (editPane.items.map[scopeName + '.' + appName + '.keyPropertyForm.' + node.id]) {
					var keyPropertyFormPane = editPane.items.map[scopeName + '.' + appName + '.keyPropertyForm.' + node.id];
					if (keyPropertyFormPane) {
						keyPropertyFormPane.destroy();
					}
				}

				if (node.attributes.properties)
					var properties = node.attributes.properties;
				else
					var properties = node.attributes.attributes.properties;

				var keyPropertyFormPanel = new Ext.FormPanel({
					name: 'keyProperty',
					id: scopeName + '.' + appName + '.keyPropertyForm.' + node.id,
					border: false,
					autoScroll: true,
					monitorValid: true,
					labelWidth: 130,
					bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
					defaults: { anchor: '100%', xtype: 'textfield', allowBlank: false },
					items: [{
						xtype: 'label',
						fieldLabel: 'Key Properties',
						labelSeparator: '',
						itemCls: 'form-title'
					}, dataPropFields, {
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
						valueField: 'value',
						value: properties.keyType
					}],
					treeNode: node,
					tbar: new Ext.Toolbar({
						items: [{
							xtype: 'tbspacer',
							width: 4
						}, {
							xtype: 'tbbutton',
							icon: 'Content/img/16x16/document-save.png',
							text: 'Save',
							tooltip: 'Save',
							handler: function (f) {
								var form = keyPropertyFormPanel.getForm();
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
							xtype: 'tbspacer',
							width: 4
						}, {
							xtype: 'tbbutton',
							icon: 'Content/img/16x16/edit-clear.png',
							text: 'Reset',
							tooltip: 'Reset',
							handler: function (f) {
								var form = keyPropertyFormPanel.getForm();
								setDataPropertyFields(form, properties);
								form.findField('nullable').disable();
							}
						}]
					})
				});

				var form = keyPropertyFormPanel.getForm();
				setDataPropertyFields(form, properties);
				editPane.add(keyPropertyFormPanel);
				var panelIndex = editPane.items.indexOf(keyPropertyFormPanel);
				editPane.getLayout().setActiveItem(panelIndex);
			}
		}

		var setDataProperty = function (editPane, node) {
			if (editPane && node) {
				if (editPane.items.map[scopeName + '.' + appName + '.dataPropertyForm.' + node.id]) {
					var dataPropertyFormPane = editPane.items.map[scopeName + '.' + appName + '.dataPropertyForm.' + node.id];
					if (dataPropertyFormPane) {
						dataPropertyFormPane.destroy();
					}
				}

				var dataPropertyFormPanel = new Ext.FormPanel({
					name: 'dataProperty',
					id: scopeName + '.' + appName + '.dataPropertyForm.' + node.id,
					border: false,
					autoScroll: true,
					monitorValid: true,
					labelWidth: 130,
					bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
					defaults: { anchor: '100%', xtype: 'textfield', allowBlank: false },
					items: [{
						xtype: 'label',
						fieldLabel: 'Data Properties',
						labelSeparator: '',
						itemCls: 'form-title'
					}, dataPropFields],
					treeNode: node,
					tbar: new Ext.Toolbar({
						items: [{
							xtype: 'tbspacer',
							width: 4
						}, {
							xtype: 'tbbutton',
							icon: 'Content/img/16x16/document-save.png',
							text: 'Save',
							tooltip: 'Save',
							handler: function (f) {
								var form = dataPropertyFormPanel.getForm();
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
							xtype: 'tbspacer',
							width: 4
						}, {
							xtype: 'tbbutton',
							icon: 'Content/img/16x16/edit-clear.png',
							text: 'Reset',
							tooltip: 'Reset',
							handler: function (f) {
								var form = dataPropertyFormPanel.getForm();
								setDataPropertyFields(form, node.attributes.properties);
							}
						}]
					})
				});
				var form = dataPropertyFormPanel.getForm();
				setDataPropertyFields(form, node.attributes.properties);
				editPane.add(dataPropertyFormPanel);
				var panelIndex = editPane.items.indexOf(dataPropertyFormPanel);
				editPane.getLayout().setActiveItem(panelIndex);
			}
		}

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
			labelWidth: 140,
			id: scopeName + '.' + appName + '.dsconfigPane',
			frame: false,
			border: false,
			autoScroll: true,
			bodyStyle: 'padding:10px 10px 0px 10px',
			monitorValid: true,
			defaults: { anchor: '100%', xtype: 'textfield', allowBlank: false },
			items: [{
				xtype: 'label',
				fieldLabel: 'Configure Data Source',
				labelSeparator: '',
				itemCls: 'form-title'
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
			tbar: new Ext.Toolbar({
				items: [{
					xtype: 'tbspacer',
					width: 4
				}, {
					xtype: 'tbbutton',
					icon: 'Content/img/16x16/document-properties.png',
					text: 'Connect',
					tooltip: 'Connect',
					handler: function (f) {
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
					xtype: 'tbspacer',
					width: 4
				}, {
					xtype: 'tbbutton',
					icon: 'Content/img/16x16/edit-clear.png',
					text: 'Reset',
					tooltip: 'Reset',
					handler: function (f) {
						setDsConfigPane();
					}
				}]
			})
		});

		var setRelations = function (editPane, node) {
			if (editPane && node) {
				if (editPane.items.map[scopeName + '.' + appName + '.relationCreateForm.' + node.id]) {
					var relationConfigPane = editPane.items.map[scopeName + '.' + appName + '.relationCreateForm.' + node.id];
					if (relationConfigPane) {
						relationConfigPane.destroy();
					}
				}

				var relationCreateFormPanel = new Ext.FormPanel({
					labelWidth: 155,
					id: scopeName + '.' + appName + '.relationCreateForm.' + node.id,
					border: false,
					autoScroll: false,					
					monitorValid: true,
					bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
					defaults: { anchor: '100%', allowBlank: false },
					items: [{
						xtype: 'label',
						fieldLabel: 'Add/Delete relationship',
						labelSeparator: '',
						itemCls: 'form-title'
					}, {
						xtype: 'textfield',
						name: 'relationName',
						fieldLabel: 'Relationship Name',						
						allowBlank: false
					}, {
						xtype: 'panel',
						id: scopeName + '.' + appName + '.dataRelationDeletePane.' + node.id,
						autoScroll: true,
						layout: 'fit',
						anchor: '100% -50',
						border: false,
						frame: false
					}],
					keys: [{
						key: [Ext.EventObject.ENTER], handler: function () {
							addRelationship(relationCreateFormPanel, node);
						}
					}],
					tbar: new Ext.Toolbar({
						items: [{
							xtype: 'tbspacer',
							width: 4
						}, {
							xtype: 'tbbutton',
							icon: 'Content/img/16x16/document-save.png',
							text: 'Save',
							tooltip: 'Save',
							handler: function () {
								var deleteDataRelationPane = relationCreateFormPanel.items.items[2];
								var gridLabel = scopeName + '.' + appName + '.' + node.id;
								var gridPane = deleteDataRelationPane.items.map[gridLabel];
								if (gridPane) {
									var mydata = gridPane.store.data.items;
									var nodeChildren = new Array();
									for (var j = 0; j < node.childNodes.length; j++)
										nodeChildren.push(node.childNodes[j].text);
									for (var i = 0; i < mydata.length; i++) {
										var exitNode = false;
										var newNodeText = mydata[i].data.relationName;
										for (var j = 0; j < nodeChildren.length; j++) {
											if (nodeChildren[j].toLowerCase() == newNodeText.toLowerCase()) {
												exitNode = true;
												break;
											}
										}
										if (exitNode == false) {
											var newNode = new Ext.tree.TreeNode({
												text: newNodeText,
												type: 'relationship',
												leaf: true,
												iconCls: 'relation',
												objectName: node.parentNode.text,
												relatedObjectName: '',
												relationshipType: 'OneToOne',
												relationshipTypeIndex: '1',
												propertyMap: []
											});
											newNode.iconCls = 'relation';
											node.appendChild(newNode);
										}
									}
									for (var j = 0; j < node.childNodes.length; j++) {
										exitNode = false;
										for (var i = 0; i < mydata.length; i++) {
											newNodeText = mydata[i].data.relationName;
											if (node.childNodes[j].text.toLowerCase() == newNodeText.toLowerCase()) {
												exitNode = true;
												break;
											}
										}
										if (exitNode == false) {
											var deleteNode = node.childNodes[j];
											node.childNodes.splice(j, 1);
											j--;
											node.removeChild(deleteNode);
										}
									}
								}
							}
						}, {
							xtype: 'tbspacer',
							width: 4
						}, {
							xtype: 'tbbutton',
							icon: 'Content/img/16x16/edit-clear.png',
							text: 'Reset',
							tooltip: 'Reset',
							handler: function () {
								var relations = new Array();
								relationCreateFormPanel.getForm().reset();
								for (i = 0; i < node.childNodes.length; i++) {
									relations.push([node.childNodes[i].text]);
								}
								var colModel = new Ext.grid.ColumnModel([
  										{ id: "relationName", header: "Data Relationship Name", dataIndex: 'relationName' }
  									]);
								var dataStore = new Ext.data.Store({
									autoDestroy: true,
									proxy: new Ext.data.MemoryProxy(relations),
									reader: new Ext.data.ArrayReader({}, [
  											{ name: 'relationName' }
  										])
								});
								createRelationGrid(scopeName + '.' + appName + '.' + node.id, deleteDataRelationPane, colModel, dataStore, scopeName + '.' + appName + '.-nh-config-wizard', scopeName + '.' + appName + '.dataObjectsPane', scopeName + '.' + appName + '.relationCreateForm.' + node.id, 0);
							}
						}]
					})
				});
				editPane.add(relationCreateFormPanel);
				var panelIndex = editPane.items.indexOf(relationCreateFormPanel);
				editPane.getLayout().setActiveItem(panelIndex);

				var deleteDataRelationPane = relationCreateFormPanel.items.items[2];
				var relations = new Array();
				var gridLabel = scopeName + '.' + appName + '.' + node.id;
				var i = 0;
				if (deleteDataRelationPane.items) {
					var gridPane = deleteDataRelationPane.items.map[gridLabel];
					if (gridPane) {
						gridPane.destroy();
					}
				}

				for (i = 0; i < node.childNodes.length; i++) {
					if (node.childNodes[i].text != '')
						relations.push([node.childNodes[i].text]);
				}
				var colModel = new Ext.grid.ColumnModel([
  					{ id: "relationName", header: "Data Relationship Name", dataIndex: 'relationName' }
  				]);
				var dataStore = new Ext.data.Store({
					autoDestroy: true,
					proxy: new Ext.data.MemoryProxy(relations),
					reader: new Ext.data.ArrayReader({}, [
  						{ name: 'relationName' }
  					])
				});
				createRelationGrid(gridLabel, deleteDataRelationPane, colModel, dataStore, scopeName + '.' + appName + '.-nh-config-wizard', scopeName + '.' + appName + '.dataObjectsPane', scopeName + '.' + appName + '.relationCreateForm.' + node.id, 0);
			}
		};

		var addRelationship = function (relationCreateFormPanel, node) {
			var deleteDataRelationPane = relationCreateFormPanel.items.items[2];
			var relationName = relationCreateFormPanel.getForm().findField("relationName").getValue().replace(/^\s*/, "").replace(/\s*$/, "");
			if (relationName == "") {
				var message = 'Relationship name cannot be blank.';
				showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
				return;
			}

			var gridLabel = scopeName + '.' + appName + '.' + node.id;
			if (deleteDataRelationPane.items) {
				var gridPane = deleteDataRelationPane.items.map[gridLabel];
				var myArray = new Array();
				var i = 0;
				if (gridPane) {
					var dataStore = gridPane.store;
					var mydata = dataStore.data.items;

					for (var i = 0; i < mydata.length; i++)
						if (mydata[i].data.relationName.toLowerCase() == relationName.toLowerCase()) {
							var message = relationName + 'already exits.';
							showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
							return;
						}

					var relationRecord = Ext.data.Record.create([
						{ name: "relationName" }
					]);

					var newRelationRecord = new relationRecord({
						relationName: relationName
					});

					dataStore.add(newRelationRecord);
					dataStore.commitChanges();
				}
			}
		}

		var setRelationFields = function (editPane, node) {
			if (editPane && node) {
				if (editPane.items.map[scopeName + '.' + appName + '.relationFieldsForm.' + node.id]) {
					var relationConfigPane = editPane.items.map[scopeName + '.' + appName + '.relationFieldsForm.' + node.id];
					if (relationConfigPane) {
						relationConfigPane.destroy();
					}
				}

				var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];
				var dataObjectNode = node.parentNode.parentNode;
				var propertiesNode = dataObjectNode.attributes.children[1];
				var keysNode = dataObjectNode.attributes.children[0];
				var relatedObjects = new Array();
				var rootNode = dbObjectsTree.getRootNode();
				for (var i = 0; i < rootNode.childNodes.length; i++)
					relatedObjects.push([i.toString(), rootNode.childNodes[i].text]);
				var selectedProperties = new Array();
				var ii = 0;
				for (var i = 0; i < keysNode.children.length; i++)
					if (!keysNode.children[i].hidden) {
						selectedProperties.push([ii.toString(), keysNode.children[i].text]);
						ii++;
					}
				for (var i = 0; i < propertiesNode.children.length; i++)
					if (!propertiesNode.children[i].hidden) {
						selectedProperties.push([ii.toString(), propertiesNode.children[i].text]);
						ii++;
					}

				if (node.attributes.attributes)
					var nodeAttribute = node.attributes.attributes;
				else
					var nodeAttribute = node.attributes;

				var mappingProperties = new Array();
				ii = 0;
				var relatedObjectName = nodeAttribute.relatedObjectName.toUpperCase();
				if (relatedObjectName != '') {
					var relatedDataObjectNode = rootNode.findChild('text', relatedObjectName);
					if (relatedDataObjectNode) {
						if (relatedDataObjectNode.childNodes[1]) {
							propertiesNode = relatedDataObjectNode.childNodes[1];
							keysNode = relatedDataObjectNode.childNodes[0];
							for (var i = 0; i < keysNode.childNodes.length; i++)
								if (!keysNode.childNodes[i].hidden) {
									mappingProperties.push([ii.toString(), keysNode.childNodes[i].text]);
									ii++;
								}
							for (var i = 0; i < propertiesNode.childNodes.length; i++)
								if (!propertiesNode.childNodes[i].hidden) {
									mappingProperties.push([ii.toString(), propertiesNode.childNodes[i].text]);
									ii++;
								}
						}
						else {
							keysNode = relatedDataObjectNode.attributes.children[0];
							propertiesNode = relatedDataObjectNode.attributes.children[1];
							for (var i = 0; i < keysNode.children.length; i++)
								if (!keysNode.children[i].hidden) {
									mappingProperties.push([ii.toString(), keysNode.children[i].text]);
									ii++;
								}
							for (var i = 0; i < propertiesNode.children.length; i++)
								if (!propertiesNode.children[i].hidden) {
									mappingProperties.push([ii.toString(), propertiesNode.children[i].text]);
									ii++;
								}
						}
					}
				}
				else {
					mappingProperties.push(['0', '']);
				}

				var relationConfigPanel = new Ext.FormPanel({
					labelWidth: 143,
					id: scopeName + '.' + appName + '.relationFieldsForm.' + node.id,
					border: false,
					autoScroll: true,
					monitorValid: true,			
					bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',					
					defaults: { anchor: '100%', allowBlank: false },
					items: [{
						xtype: 'label',
						fieldLabel: 'Configure Relationship',
						labelSeparator: '',
						itemCls: 'form-title'
					}, {
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

							var mappingProperties = new Array();
							var ii = 0;
							if (relatedDataObjectNode.childNodes[1]) {
								keysNode = relatedDataObjectNode.childNodes[0];
								propertiesNode = relatedDataObjectNode.childNodes[1];
								for (var i = 0; i < keysNode.childNodes.length; i++)
									if (!keysNode.childNodes[i].hidden) {
										mappingProperties.push([ii.toString(), keysNode.childNodes[i].text]);
										ii++;
									}
								for (var i = 0; i < propertiesNode.childNodes.length; i++)
									if (!propertiesNode.childNodes[i].hidden) {
										mappingProperties.push([ii.toString(), propertiesNode.childNodes[i].text]);
										ii++;
									}
							}
							else {
								keysNode = relatedDataObjectNode.attributes.children[0];
								propertiesNode = relatedDataObjectNode.attributes.children[1];
								for (var i = 0; i < keysNode.children.length; i++)
									if (!keysNode.children[i].hidden) {
										mappingProperties.push([ii.toString(), keysNode.children[i].text]);
										ii++;
									}
								for (var i = 0; i < propertiesNode.children.length; i++)
									if (!propertiesNode.children[i].hidden) {
										mappingProperties.push([ii.toString(), propertiesNode.children[i].text]);
										ii++;
									}
							}

							var mapCombo = relationConfigPanel.getForm().findField('mapPropertyName');
							if (mapCombo.store.data) {
								mapCombo.reset();
								mapCombo.store.removeAll();
							}
							mapCombo.store.loadData(mappingProperties);
							mapCombo.store.commitChanges();
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
						editable: false,
						triggerAction: 'all',
						displayField: 'text',
						valueField: 'value',
						selectOnFocus: true,
						listeners: { 'select': function (combo, record, index) {
							var mapproperty = record.data.field2;
						}
						}
				}, {
					xtype: 'panel',
					id: scopeName + '.' + appName + '.dataRelationPane.' + node.id,
					autoScroll: true,
					layout: 'fit',
					anchor: '100% -196',
					border: false,
					frame: false
				}],
					tbar: new Ext.Toolbar({
						items: [{
							xtype: 'tbspacer',
							width: 4
						}, {
							xtype: 'tbbutton',
							icon: 'Content/img/16x16/document-save.png',
							text: 'Save',
							tooltip: 'Save',
							handler: function () {
								if (node.attributes.attributes)
									var attribute = node.attributes.attributes;
								else
									var attribute = node.attributes;

								var dataRelationPane = relationConfigPanel.items.items[7];
								var gridLabel = scopeName + '.' + appName + '.' + node.id;
								var gridPane = dataRelationPane.items.map[gridLabel];
								if (gridPane) {
									var mydata = gridPane.store.data.items;
									var propertyMap = new Array();
									if (attribute) {
										for (i = 0; i < attribute.propertyMap.length; i++)
											propertyMap.push([attribute.propertyMap[i].dataPropertyName, attribute.propertyMap[i].relatedPropertyName]);

										for (var i = 0; i < mydata.length; i++) {
											var exitPropertyMap = false;
											var dataPropertyName = mydata[i].data.property.toLowerCase();
											var relatedPropertyName = mydata[i].data.relatedProperty.toLowerCase();
											for (var j = 0; j < propertyMap.length; j++) {
												if (propertyMap[j][0] == dataPropertyName && propertyMap[j][1] == relatedPropertyName) {
													exitPropertyMap = true;
													break;
												}
											}
											if (exitPropertyMap == false) {
												var mapItem = new Array();
												mapItem['dataPropertyName'] = dataPropertyName;
												mapItem['relatedPropertyName'] = relatedPropertyName;
												attribute.propertyMap.push(mapItem);
											}
										}
										for (var j = 0; j < attribute.propertyMap.length; j++) {
											exitPropertyMap = false;
											for (var i = 0; i < mydata.length; i++) {
												dataPropertyName = mydata[i].data.property.toLowerCase();
												relatedPropertyName = mydata[i].data.relatedProperty.toLowerCase();
												if (attribute.propertyMap[j].dataPropertyName == dataPropertyName && attribute.propertyMap[j].relatedPropertyName == relatedPropertyName) {
													exitPropertyMap = true;
													break;
												}
											}
											if (exitPropertyMap == false) {
												attribute.propertyMap.splice(j, 1);
												j--;
											}
										}
									}
								}
							}
						}, {
							xtype: 'tbspacer',
							width: 4
						}, {
							xtype: 'tbbutton',
							icon: 'Content/img/16x16/edit-clear.png',
							text: 'Reset',
							tooltip: 'Reset',
							handler: function () {
								var propertyNameCombo = relationConfigPanel.getForm().findField('propertyName');
								propertyNameCombo.setValue('');
								propertyNameCombo.clearInvalid();
								var mapPropertyNameCombo = relationConfigPanel.getForm().findField('mapPropertyName');
								mapPropertyNameCombo.setValue('');
								mapPropertyNameCombo.clearInvalid();

								var properMap = new Array();
								var dataRelationPane = relationConfigPanel.items.items[7];

								var attribute = node.attributes;
								if (attribute) {
									for (i = 0; i < attribute.propertyMap.length; i++)
										properMap.push([attribute.propertyMap.dataPropertyName, attribute.propertyMap.relatedPropertyName]);

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
									createRelationGrid(scopeName + '.' + appName + '.' + node.id, dataRelationPane, colModel, dataStore, scopeName + '.' + appName + '.-nh-config-wizard', scopeName + '.' + appName + '.dataObjectsPane', scopeName + '.' + appName + '.relationFieldsForm.' + node.id, 1);
								}
							}
						}]
					})
				});
				editPane.add(relationConfigPanel);
				var panelIndex = editPane.items.indexOf(relationConfigPanel);
				editPane.getLayout().setActiveItem(panelIndex);
				var relationConfigForm = relationConfigPanel.getForm();
				var dataRelationPane = relationConfigPanel.items.items[7];
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
						gridPane.destroy();
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
				createRelationGrid(gridLabel, dataRelationPane, colModel, dataStore, scopeName + '.' + appName + '.-nh-config-wizard', scopeName + '.' + appName + '.dataObjectsPane', scopeName + '.' + appName + '.relationFieldsForm.' + node.id, 1);
			}
		};

		var addPropertyMapping = function (relationConfigPanel) {
			var dataRelationPane = relationConfigPanel.items.items[7];
			var relationConfigForm = relationConfigPanel.getForm();
			var selectPropComboBox = relationConfigForm.findField("propertyName");
			var mapPropComboBox = relationConfigForm.findField("mapPropertyName");
			if (!selectPropComboBox.getValue() || !mapPropComboBox.getValue()) {
				var message = 'Please select a property name and a mapping property.';
				showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
				return;
			}

			var propertyName = selectPropComboBox.store.getAt(selectPropComboBox.getValue()).data.field2.replace(/^\s*/, "").replace(/\s*$/, "");
			var mapPropertyName = mapPropComboBox.store.getAt(mapPropComboBox.getValue()).data.text.replace(/^\s*/, "").replace(/\s*$/, "");
			if (propertyName == "" || mapPropertyName == "") {
				var message = 'Property Name or Mapping Property cannot be blank.';
				showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
				return;
			}

			var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];
			var gridLabel = scopeName + '.' + appName + '.' + dbObjectsTree.getSelectionModel().getSelectedNode().id;
			if (dataRelationPane.items) {
				var gridPane = dataRelationPane.items.map[gridLabel];
				if (gridPane) {
					var dataStore = gridPane.store;
					var myPropMap = dataStore.data.items;

					for (var i = 0; i < myPropMap.length; i++)
						if (myPropMap[i].data.property == propertyName && myPropMap[i].data.relatedProperty == mapPropertyName) {
							var message = 'The pair of ' + propertyName + ' and ' + mapPropertyName + ' already exits.';
							showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
							return;
						}

					var propertyMapRecord = Ext.data.Record.create([
								{ name: "property" },
								{ name: "relatedProperty" },
							]);

					var newpropertyMapRecord = new propertyMapRecord({
						property: propertyName,
						relatedProperty: mapPropertyName
					});

					dataStore.add(newpropertyMapRecord);
					dataStore.commitChanges();
				}
			}
		}

		var loadTree = function (rootNode) {
			var relationTypeStr = ['OneToOne', 'OneToMany'];

			// sync data object tree with data dictionary
			for (var i = 0; i < rootNode.childNodes.length; i++) {
				var dataObjectNode = rootNode.childNodes[i];

				for (var ii = 0; ii < dbDict.dataObjects.length; ii++) {
					var dataObject = dbDict.dataObjects[ii];
					// sync data object
					dataObjectNode.attributes.properties.objectNamespace = dataObject.objectNamespace;

					if (dataObject.objectName.toLowerCase() == dataObjectNode.text.toLowerCase()) {
						var keysNode = dataObjectNode.attributes.children[0];
						var propertiesNode = dataObjectNode.attributes.children[1];
						var relationshipsNode = dataObjectNode.attributes.children[2];

						// sync data properties
						for (var j = 0; j < propertiesNode.children.length; j++) {
							for (var jj = 0; jj < dataObject.dataProperties.length; jj++) {
								if (propertiesNode.children[j].text.toLowerCase() ==
									dataObject.dataProperties[jj].propertyName.toLowerCase()) {
									propertiesNode.children[j].hidden = false;
								}
							}
						}

						// sync key properties
						for (var j = 0; j < dataObject.keyProperties.length; j++) {
							for (var k = 0; k < keysNode.children.length; k++) {
								if (keysNode.children[k].text.toLowerCase() == dataObject.keyProperties[j].keyPropertyName.toLowerCase()) {
									j++;
									break;
								}
							}
							if (j < dataObject.keyProperties.length) {
								for (var jj = 0; jj < propertiesNode.children.length; jj++) {
									var nodeText = dataObject.keyProperties[j].keyPropertyName;
									if (propertiesNode.children[jj].text.toLowerCase() == nodeText.toLowerCase()) {
										var properties = propertiesNode.children[jj].properties;
										properties.keyType = 'assigned';
										properties.nullable = false;

										newKeyNode = new Ext.tree.TreeNode({
											text: nodeText,
											type: "keyProperty",
											leaf: true,
											iconCls: 'property',
											hidden: false,
											properties: properties
										});
										newKeyNode.iconCls = 'property';
										propertiesNode.children.splice(jj, 1);
										jj--;

										if (newKeyNode)
											keysNode.children.push(newKeyNode);

										break;
									}
								}
							}
						}

						// sync relationships
						for (var j = 0; j < dataObject.dataRelationships.length; j++) {
							var newNode = new Ext.tree.TreeNode({
								text: dataObject.dataRelationships[j].relationshipName,
								type: 'relationship',
								leaf: true,
								iconCls: 'relation',
								objectName: dataObjectNode.text,
								relatedObjectName: dataObject.dataRelationships[j].relatedObjectName,
								relationshipType: relationTypeStr[dataObject.dataRelationships[j].relationshipType],
								relationshipTypeIndex: dataObject.dataRelationships[j].relationshipType,
								propertyMap: []
							});
							var mapArray = new Array();
							for (var jj = 0; jj < dataObject.dataRelationships[j].propertyMaps.length; jj++) {
								var mapItem = new Array();
								mapItem['dataPropertyName'] = dataObject.dataRelationships[j].propertyMaps[jj].dataPropertyName;
								mapItem['relatedPropertyName'] = dataObject.dataRelationships[j].propertyMaps[jj].relatedPropertyName;
								mapArray.push(mapItem);
							}
							newNode.iconCls = 'relation';
							newNode.attributes.propertyMap = mapArray;
							relationshipsNode.expanded = true;
							relationshipsNode.children.push(newNode);
						}
					}
				}
			}
		};

		var tablesSelectorPane = new Ext.FormPanel({
			frame: false,
			border: false,
			autoScroll: true,
			id: scopeName + '.' + appName + '.tablesSelectorPane',
			bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
			labelWidth: 140,
			monitorValid: true,
			items: [{
				xtype: 'label',
				fieldLabel: 'Select Tables',
				labelSeparator: '',
				itemCls: 'form-title'
			}, {
				xtype: 'itemselector',
				hideLabel: true,
				name: 'tableSelector',
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
			tbar: new Ext.Toolbar({
				items: [{
					xtype: 'tbspacer',
					width: 4
				}, {
					xtype: 'tbbutton',
					icon: 'Content/img/16x16/document-save.png',
					text: 'Save',
					tooltip: 'Save',
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

						var rootNode = dbObjectsTree.getRootNode();
						rootNode.reload(
							function (rootNode) {
								loadTree(rootNode);
							});
					}
				}, {
					xtype: 'tbspacer',
					width: 4
				}, {
					xtype: 'tbbutton',
					icon: 'Content/img/16x16/edit-clear.png',
					text: 'Reset',
					tooltip: 'Reset',
					handler: function () {
						var rootNode = dataObjectsPane.items.items[0].items.items[0].getRootNode();
						var selectTableNames = new Array();
						var availTableName = new Array();
						var found = false;

						for (var i = 0; i < dbDict.dataObjects.length; i++) {
							var tableName = dbDict.dataObjects[i].tableName;
							availTableName.push([tableName, tableName]);
						}

						for (var j = 0; j < availTableName.length; j++)
							for (var i = 0; i < rootNode.childNodes.length; i++) {
								if (rootNode.childNodes[i].text == availTableName[j][0]) {
									found = true;
									availTableName.splice(j, 1);
									j--;
									break;
								}
							}

						for (var i = 0; i < rootNode.childNodes.length; i++) {
							var nodeText = rootNode.childNodes[i].text;
							selectTableNames.push([nodeText, nodeText]);
						}

						var tablesSelector = tablesSelectorPane.items.items[1];
						if (tablesSelector.toMultiselect.store.data) {
							tablesSelector.toMultiselect.reset();
							tablesSelector.toMultiselect.store.removeAll();
						}

						tablesSelector.toMultiselect.store.loadData(selectTableNames);
						tablesSelector.toMultiselect.store.commitChanges();

						if (tablesSelector.fromMultiselect.store.data) {
							tablesSelector.fromMultiselect.reset();
							tablesSelector.fromMultiselect.store.removeAll();
						}

						tablesSelector.fromMultiselect.store.loadData(availTableName);
						tablesSelector.fromMultiselect.store.commitChanges();
					}
				}]
			})
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

		var setDataObject = function (editPane, node) {
			if (editPane && node) {
				if (editPane.items.map[scopeName + '.' + appName + '.objectNameForm.' + node.id]) {
					var objectNameFormPane = editPane.items.map[scopeName + '.' + appName + '.objectNameForm.' + node.id];
					if (objectNameFormPane) {
						objectNameFormPane.destroy();
					}
				}

				var dataObjectFormPanel = new Ext.FormPanel({
					name: 'dataObject',
					id: scopeName + '.' + appName + '.objectNameForm.' + node.id,
					border: false,
					autoScroll: true,
					monitorValid: true,
					labelWidth: 160,
					bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
					defaults: { anchor: '100%', xtype: 'textfield', allowBlank: false },
					items: [{
						xtype: 'label',
						fieldLabel: 'Data Object',
						labelSeparator: '',
						itemCls: 'form-title'
					}, {
						name: 'tableName',
						fieldLabel: 'Table Name',
						value: node.text,
						disabled: true
					}, {
						name: 'objectNamespace',
						fieldLabel: 'Object Namespace',
						value: node.attributes.properties.objectNamespace
					}, {
						name: 'objectName',
						fieldLabel: 'Object Name',
						value: node.attributes.properties.objectName
					}, {
						name: 'keyDelimiter',
						fieldLabel: 'Key Delimiter',
						value: node.attributes.properties.keyDelimiter,
						allowBlank: true
					}],
					treeNode: node,
					tbar: new Ext.Toolbar({
						items: [{
							xtype: 'tbspacer',
							width: 4
						}, {
							xtype: 'tbbutton',
							icon: 'Content/img/16x16/document-save.png',
							text: 'Save',
							tooltip: 'Save',
							handler: function (f) {
								var form = objectnameFormPanel.getForm();
								if (form.treeNode) {
									var treeNodeProps = form.treeNode.attributes.properties;
									treeNodeProps['objectName'] = form.findField('objectName').getValue();
									treeNodeProps['keyDelimiter'] = form.findField('keyDelimiter').getValue();
								}
							}
						}, {
							xtype: 'tbspacer',
							width: 4
						}, {
							xtype: 'tbbutton',
							icon: 'Content/img/16x16/edit-clear.png',
							text: 'Reset',
							tooltip: 'Reset',
							handler: function (f) {
								var form = objectnameFormPanel.getForm();
								form.findField('tableName').setValue(node.text);
								if (node.attributes.properties) {
									form.findField('objectName').setValue(node.attributes.properties.objectName);
									form.findField('keyDelimiter').setValue(node.attributes.properties.keyDelimiter);
								}
							}
						}]
					})
				});
				editPane.add(dataObjectFormPanel);
				var panelIndex = editPane.items.indexOf(dataObjectFormPanel);
				editPane.getLayout().setActiveItem(panelIndex);
			}
		};

		var setItemSelectorAvailValues = function (node) {
			var availItems = new Array();
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
			return availItems;
		}

		var setItemSelectorselectedValues = function (node) {
			var selectedItems = new Array();
			var propertiesNode = node.parentNode.childNodes[1];

			for (var i = 0; i < node.childNodes.length; i++) {
				var keyName = node.childNodes[i].text;
				selectedItems.push([keyName, keyName]);
			}
			return selectedItems;
		}

		var setKeysFolder = function (editPane, node) {
			if (editPane && node) {
				if (editPane.items.map[scopeName + '.' + appName + '.keysSelector.' + node.id]) {
					var keysSelectorPane = editPane.items.map[scopeName + '.' + appName + '.keysSelector.' + node.id];
					if (keysSelectorPane) {
						keysSelectorPane.destroy();
					}
				}

				var availItems = setItemSelectorAvailValues(node);
				var selectedItems = setItemSelectorselectedValues(node);

				var keysItemSelector = new Ext.ux.ItemSelector({
					name: 'keySelector',
					imagePath: 'scripts/ext-3.3.1/examples/ux/images/',
					hideLabel: true,
					multiselects: [{
						width: 250,
						height: 300,
						store: availItems,
						displayField: 'keyName',
						valueField: 'keyValue'
					}, {
						width: 250,
						height: 300,
						store: selectedItems,
						displayField: 'keyName',
						valueField: 'keyValue'
					}],
					treeNode: node
				});

				var keysSelectorPanel = new Ext.FormPanel({
					id: scopeName + '.' + appName + '.keysSelector.' + node.id,
					border: false,
					autoScroll: true,
					bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
					labelWidth: 160,
					defaults: { anchor: '100%' },
					items: [{
						xtype: 'label',
						fieldLabel: 'Select Keys',
						itemCls: 'form-title',
						labelSeparator: ''
					}, keysItemSelector],
					tbar: new Ext.Toolbar({
						items: [{
							xtype: 'tbspacer',
							width: 4
						}, {
							xtype: 'tbbutton',
							icon: 'Content/img/16x16/document-save.png',
							text: 'Save',
							tooltip: 'Save',
							handler: function (f) {
								//var selectedValues = selectedValuesStr.split(',');
								var selectedValues = keysItemSelector.toMultiselect.store.data.items;
								var keysNode = keysItemSelector.treeNode;
								var propertiesNode = keysNode.parentNode.childNodes[1];

								// a key has been added, move it to keys node and remove it from properties node
								if (selectedValues.length > keysNode.childNodes.length) {
									// determine the new key
									for (var i = 0; i < selectedValues.length; i++) {
										var found = false;

										for (var j = 0; j < keysNode.childNodes.length; j++) {
											if (keysNode.childNodes[j].text == selectedValues[i].data.text) {
												found = true;
												break;
											}
										}

										// find the node in the properties node and move it to keys node
										if (!found) {
											var newKeyNode;

											for (var jj = 0; jj < propertiesNode.childNodes.length; jj++) {
												if (propertiesNode.childNodes[jj].text == selectedValues[i].data.text) {
													var properties = propertiesNode.childNodes[jj].attributes.properties;
													properties.keyType = 'assigned';
													properties.nullable = false;

													newKeyNode = new Ext.tree.TreeNode({
														text: selectedValues[i].data.text,
														type: "keyProperty",
														leaf: true,
														hidden: false,
														properties: properties
													});
													newKeyNode.iconCls = 'property';
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
											if (selectedValues[j].data.text == keysNode.childNodes[i].text) {
												found = true;
												break;
											}
										}

										// find the node in the keys node and move it to properties node
										if (!found) {
											if (keysNode.childNodes[i].attributes.properties)
												var properties = keysNode.childNodes[i].attributes.properties;
											else if (keysNode.childNodes[i].attributes.attributes.properties)
												var properties = keysNode.childNodes[i].attributes.attributes.properties;

											if (properties) {
												properties['nullable'] = true;
												delete properties.keyType;

												var propertyNode = new Ext.tree.TreeNode({
													text: keysNode.childNodes[i].text,
													type: "dataProperty",
													leaf: true,
													iconCls: 'property',
													properties: properties
												});

												propertyNode.iconCls = 'property';
												propertiesNode.appendChild(propertyNode);
												keysNode.removeChild(keysNode.childNodes[i], true);
											}
										}
									}
								}
							}
						}, {
							xtype: 'tbspacer',
							width: 4
						}, {
							xtype: 'tbbutton',
							icon: 'Content/img/16x16/edit-clear.png',
							text: 'Reset',
							tooltip: 'Reset',
							handler: function (f) {
								var availItems = setItemSelectorAvailValues(node);
								var selectedItems = setItemSelectorselectedValues(node);
								if (keysItemSelector.fromMultiselect.store.data) {
									keysItemSelector.fromMultiselect.reset();
									keysItemSelector.fromMultiselect.store.removeAll();
								}

								keysItemSelector.fromMultiselect.store.loadData(availItems);
								keysItemSelector.fromMultiselect.store.commitChanges();

								if (keysItemSelector.toMultiselect.store.data) {
									keysItemSelector.toMultiselect.reset();
									keysItemSelector.toMultiselect.store.removeAll();
								}

								keysItemSelector.toMultiselect.store.loadData(selectedItems);
								keysItemSelector.toMultiselect.store.commitChanges();
							}
						}]
					})
				});

				editPane.add(keysSelectorPanel);
				var panelIndex = editPane.items.indexOf(keysSelectorPanel);
				editPane.getLayout().setActiveItem(panelIndex);
			}
		};

		var setPropertiesFolder = function (editPane, node) {
			if (editPane && node) {
				if (editPane.items.map[scopeName + '.' + appName + '.propertiesSelector.' + node.id]) {
					var propertiesSelectorPane = editPane.items.map[scopeName + '.' + appName + '.propertiesSelector.' + node.id];
					if (propertiesSelectorPane) {
						propertiesSelectorPane.destroy();
					}
				}

				var availItems = new Array();
				var selectedItems = new Array();
				for (var i = 0; i < node.childNodes.length; i++) {
					var itemName = node.childNodes[i].text;
					if (node.childNodes[i].hidden == false)
						selectedItems.push([itemName, itemName]);
					else
						availItems.push([itemName, itemName]);
				}

				var propertiesItemSelector = new Ext.ux.ItemSelector({
					name: 'propertySelector',
					imagePath: 'scripts/ext-3.3.1/examples/ux/images/',
					hideLabel: true,
					multiselects: [{
						width: 250,
						height: 300,
						store: availItems,
						displayField: 'propertyName',
						valueField: 'propertyValue'
					}, {
						width: 250,
						height: 300,
						store: selectedItems,
						displayField: 'propertyName',
						valueField: 'propertyValue'
					}],
					treeNode: node
				});

				var propertiesSelectorPanel = new Ext.FormPanel({
					bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
					id: scopeName + '.' + appName + '.propertiesSelector.' + node.id,
					border: false,
					autoScroll: true,
					defaults: { anchor: '100%' },
					labelWidth: 160,
					items: [{
						xtype: 'label',
						fieldLabel: 'Select Properties',
						itemCls: 'form-title',
						labelSeparator: ''
					}, propertiesItemSelector],
					tbar: new Ext.Toolbar({
						items: [{
							xtype: 'tbspacer',
							width: 4
						}, {
							xtype: 'tbbutton',
							icon: 'Content/img/16x16/document-save.png',
							text: 'Save',
							tooltip: 'Save',
							handler: function (f) {
								var selectedValues = propertiesItemSelector.toMultiselect.store.data.items;
								var treeNode = propertiesItemSelector.treeNode;

								for (var i = 0; i < treeNode.childNodes.length; i++) {
									var found = false;

									for (var j = 0; j < selectedValues.length; j++) {
										if (selectedValues[j].data.text.toLowerCase() == treeNode.childNodes[i].text.toLowerCase()) {
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
						}, {
							xtype: 'tbspacer',
							width: 4
						}, {
							xtype: 'tbbutton',
							icon: 'Content/img/16x16/edit-clear.png',
							text: 'Reset',
							tooltip: 'Reset',
							handler: function (f) {
								var availItems = new Array();
								var selectedItems = new Array();
								for (var i = 0; i < node.childNodes.length; i++) {
									var itemName = node.childNodes[i].text;
									if (node.childNodes[i].hidden == false)
										selectedItems.push([itemName, itemName]);
									else
										availItems.push([itemName, itemName]);
								}

								if (propertiesItemSelector.fromMultiselect.store.data) {
									propertiesItemSelector.fromMultiselect.reset();
									propertiesItemSelector.fromMultiselect.store.removeAll();
								}

								propertiesItemSelector.fromMultiselect.store.loadData(availItems);
								propertiesItemSelector.fromMultiselect.store.commitChanges();

								if (propertiesItemSelector.toMultiselect.store.data) {
									propertiesItemSelector.toMultiselect.reset();
									propertiesItemSelector.toMultiselect.store.removeAll();
								}

								propertiesItemSelector.toMultiselect.store.loadData(selectedItems);
								propertiesItemSelector.toMultiselect.store.commitChanges();
							}
						}]
					})
				});

				editPane.add(propertiesSelectorPanel);
				var panelIndex = editPane.items.indexOf(propertiesSelectorPanel);
				editPane.getLayout().setActiveItem(panelIndex);
			}
		};

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
						text: 'Data Objects',
						nodeType: 'async',
						iconCls: 'folder'
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
							icon: 'Content/img/16x16/document-save.png',
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
									folder.objectNamespace = folderNodeProp.objectNamespace;
									folder.objectName = folderNodeProp.objectName;
									if (!folderNodeProp.keyDelimeter)
										folder.keyDelimeter = 'null';
									else
										folder.keyDelimeter = folderNodeProp.keyDelimeter;
									folder.keyProperties = new Array();
									folder.dataProperties = new Array();
									folder.dataRelationships = new Array();

									for (var j = 0; j < folderNode.attributes.children.length; j++) {
										if (folderNode.childNodes[1])
											var propertyFolderNode = folderNode.childNodes[1];
										else
											var propertyFolderNode = folderNode.attributes.children[1];

										if (folderNode.childNodes[0])
											var keyFolderNode = folderNode.childNodes[0];
										else
											var keyFolderNode = folderNode.attributes.children[0];

										if (folderNode.childNodes[2])
											var relationFolderNode = folderNode.childNodes[2];
										else
											var relationFolderNode = folderNode.attributes.children[2];

										if (folderNode.childNodes[j])
											subFolderNodeText = folderNode.childNodes[j].text;
										else
											subFolderNodeText = folderNode.attributes.children[j].text;

										switch (subFolderNodeText) {
											case 'Keys':
												if (folderNode.childNodes[1])
													var keyChildenNodes = keyFolderNode.childNodes;
												else
													var keyChildenNodes = keyFolderNode.children;

												for (var k = 0; k < keyChildenNodes.length; k++) {
													var keyNode = keyChildenNodes[k];

													if (!keyNode.hidden) {
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
												}
												break;
											case 'Properties':
												if (folderNode.childNodes[1])
													var propChildenNodes = propertyFolderNode.childNodes;
												else
													var propChildenNodes = propertyFolderNode.children;
												for (var k = 0; k < propChildenNodes.length; k++) {
													var propertyNode = propChildenNodes[k];

													if (!propertyNode.hidden) {
														if (propertyNode.properties)
															var propertyNodeProf = propertyNode.properties;
														else if (propertyNode.attributes)
															var propertyNodeProf = propertyNode.attributes.properties;

														var props = {};
														props.columnName = propertyNodeProf.columnName;
														props.propertyName = propertyNodeProf.propertyName;

														props.dataType = 10;
														props.dataLength = propertyNodeProf.dataLength;
														if (propertyNodeProf.nullable == 'True')
															props.isNullable = 'true';
														else
															props.isNullable = 'false';

														if (props.columnName == keyName)
															props.keyType = 1;
														else
															props.keyType = 0;

														if (propertyNodeProf.showOnIndex == 'True')
															props.showOnIndex = 'true';
														else
															props.showOnIndex = 'false';

														props.numberOfDecimals = propertyNodeProf.numberOfDecimals;
														folder.dataProperties.push(props);
													}
												}
												break;
											case 'Relationships':
												if (!relationFolderNode)
													break;

												if (folderNode.childNodes[2])
													var relChildenNodes = relationFolderNode.childNodes;
												else
													var relChildenNodes = relationFolderNode.children;

												if (relChildenNodes)
													for (var k = 0; k < relChildenNodes.length; k++) {
														var relationNode = relChildenNodes[k];
														if (relationNode.attributes) {
															if (relationNode.attributes.attributes) {
																var relationNodeAttr = relationNode.attributes.attributes;
															}
															else {
																var relationNodeAttr = relationNode.attributes;
															}
														}
														else {
															relationNodeAttr = relationNode;
														}

														var relation = {};
														relation.propertyMaps = new Array();
														if (relationNode.text != '') {
															for (var m = 0; m < relationNodeAttr.propertyMap.length; m++) {
																var propertyPairNode = relationNodeAttr.propertyMap[m];
																var propertyPair = {};

																propertyPair.dataPropertyName = propertyPairNode.dataPropertyName;
																propertyPair.relatedPropertyName = propertyPairNode.relatedPropertyName;
																relation.propertyMaps.push(propertyPair);
															}

															relation.relatedObjectName = relationNodeAttr.relatedObjectName;
															relation.relationshipName = relationNodeAttr.text;
															relation.relationshipType = relationNodeAttr.relationshipTypeIndex;
															folder.dataRelationships.push(relation);
														}
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
										showDialog(400, 100, 'Tree saving result', 'The tree is saved successfully.', Ext.Msg.OK, null);
									},
									failure: function (response, request) {
										showDialog(660, 300, 'Tree saving result',
										'Error happed when saving the tree', Ext.Msg.OK, null);
									}
								});
							}
						}]
					}),
					listeners: {
						click: function (node, e) {
							if (node.isRoot) {
								editPane = dataObjectsPane.items.items[1];
								if (!editPane) {
									var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
								}
								editPane.add(tablesSelectorPane);
								var panelIndex = editPane.items.indexOf(tablesSelectorPane);
								editPane.getLayout().setActiveItem(panelIndex);
								return;
							}
							else if (!node)
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
										setDataObject(editPane, node);
										break;

									case 'KEYS':
										setKeysFolder(editPane, node);
										break;

									case 'KEYPROPERTY':
										setKeyProperty(editPane, node);
										break;

									case 'PROPERTIES':
										setPropertiesFolder(editPane, node);
										break;

									case 'DATAPROPERTY':
										setDataProperty(editPane, node);
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
				autoScroll: false,
				region: 'center',
				layout: 'card'
			}]
		});

		var setDsConfigPane = function () {
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
		};

		var setTablesSelectorPane = function () {
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
			return selectTableNames;
		};

		Ext.apply(this, {
			id: scopeName + '.' + appName + '.-nh-config-wizard',
			title: 'NHibernate Config Wizard - ' + scopeName + '.' + appName,			
			closable: true,
			layout: 'fit',
			items: [dataObjectsPane]
		});

		Ext.EventManager.onWindowResize(this.doLayout, this);

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
					setDsConfigPane();
					var selectTableNames = setTablesSelectorPane();

					var tab = Ext.getCmp('content-panel');
					var rp = tab.items.map[scopeName + '.' + appName + '.-nh-config-wizard'];
					var dataObjectsPane = rp.items.map[scopeName + '.' + appName + '.dataObjectsPane'];
					var tablesSelForm = tablesSelectorPane.getForm();
					var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];
					var treeLoader = dbObjectsTree.getLoader();
					var dsConfigForm = dsConfigPane.getForm();

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
						loadTree(rootNode);
					});
				}
			},
			failure: function (response, request) {
				showDialog(400, 100, 'Warning', 'Please connect to a Database first.', Ext.Msg.OK, null);
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

function createRelationGrid(gridlabel, dataGridPanel, colModel, dataStore, configLabel, dbObjLabel, formLabel, callId) {
	if (callId == 0) {		
		var msg = 'Relationship name cannot be added when the field is blank.';
	}
	else {		
		var msg = 'The pair of property name and mapping property cannot added when either value is blank.'
	}

	dataStore.on('load', function () {
		if (dataGridPanel.items) {
			var gridtab = dataGridPanel.items.map[gridlabel];
			if (gridtab) {
				gridtab.destroy();
			}
		}

		var dataRelationGridPane = new Ext.grid.GridPanel({
			id: gridlabel,
			store: dataStore,
			stripeRows: true,
			autoScroll: true,
			frame: false,
			border: false,
			cm: colModel,
			selModel: new Ext.grid.RowSelectionModel({ singleSelect: true }),
			enableColLock: true,
			viewConfig: { forceFit: true },
			tbar: new Ext.Toolbar({
				items: [{
					xtype: 'tbspacer',
					width: 4
				}, {
					xtype: 'tbbutton',
					icon: 'Content/img/list-add.png',
					text: 'Add',
					tooltip: 'Add',
					handler: function () {
						var tab = Ext.getCmp('content-panel');
						var rp = tab.items.map[configLabel];
						var dataObjectsPane = rp.items.map[dbObjLabel];
						var editPane = dataObjectsPane.items.items[1];
						var form = editPane.items.map[formLabel].getForm();
						var mydata = dataStore.data.items;
						if (callId == 0) {
							var relationName = form.findField('relationName').getValue().replace(/^\s*/, "").replace(/\s*$/, "");
							if (relationName == '') {
								showDialog(400, 100, 'Warning', msg, Ext.Msg.OK, null);
								return;
							}
							for (var i = 0; i < mydata.length; i++)
								if (mydata[i].data.relationName.toLowerCase() == relationName.toLowerCase()) {
									var message = relationName + 'already exits.';
									showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
									return;
								}
							var relationRecord = Ext.data.Record.create([
							{ name: "relationName" }
						]);

							var newRelationRecord = new relationRecord({
								relationName: relationName
							});
						}
						else {
							var propertyNameCombo = form.findField('propertyName');
							var mapPropertyNameCombo = form.findField('mapPropertyName');
							if (!propertyNameCombo.getValue() || !mapPropertyNameCombo.getValue())
								return;

							var propertyName = propertyNameCombo.store.getAt(propertyNameCombo.getValue()).data.field2.replace(/^\s*/, "").replace(/\s*$/, "");
							var mapPropertyName = mapPropertyNameCombo.store.getAt(mapPropertyNameCombo.getValue()).data.text.replace(/^\s*/, "").replace(/\s*$/, "");
							if (propertyName == "" || mapPropertyName == "") {
								showDialog(400, 100, 'Warning', msg, Ext.Msg.OK, null);
								return;
							}

							for (var i = 0; i < mydata.length; i++)
								if (mydata[i].data.property == propertyName && mydata[i].data.relatedProperty == mapPropertyName) {
									var message = 'The pair of ' + propertyName + ' and ' + mapPropertyName + ' cannot be added because the pair already exits.';
									showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
									return;
								}

							var propertyMapRecord = Ext.data.Record.create([
								{ name: "property" },
								{ name: "relatedProperty" },
							]);

							var newRelationRecord = new propertyMapRecord({
								property: propertyName,
								relatedProperty: mapPropertyName
							});
						}
						dataStore.add(newRelationRecord);
						dataStore.commitChanges();
					}
				}, {
					xtype: 'tbspacer',
					width: 4
				}, {
					xtype: 'tbbutton',
					icon: 'Content/img/16x16/edit-delete.png',
					text: 'Delete',
					tooltip: 'Delete',
					handler: function () {
						var selectModel = dataRelationGridPane.getSelectionModel();
						if (selectModel.hasSelection()) {
							var selectIndex = selectModel.getSelectedIndex();
							dataStore.removeAt(selectIndex);
						}
						else {
							if (dataStore.data.items.length < 1)
								showDialog(400, 100, 'Warning', 'No records exits in the table', Ext.Msg.OK, null);
							else
								showDialog(400, 100, 'Warning', 'Please select a row first.', Ext.Msg.OK, null);
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

Ext.grid.RowSelectionModel.override({
	getSelectedIndex: function () {
		return this.grid.store.indexOf(this.selections.itemAt(0));
	}
});



