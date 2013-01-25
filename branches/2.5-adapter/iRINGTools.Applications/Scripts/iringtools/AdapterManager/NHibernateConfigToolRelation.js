Ext.ns('AdapterManager');

function setRelations(editPane, node, scopeName, appName) {
	if (editPane && node) {
		if (editPane.items.map[scopeName + '.' + appName + '.relationCreateForm.' + node.id]) {
			var relationCreatePane = editPane.items.map[scopeName + '.' + appName + '.relationCreateForm.' + node.id];
			if (relationCreatePane) {
				relationCreatePane.destroy();						
			}
		}

		var relationCreateFormPanel = new Ext.FormPanel({
			labelWidth: 160,
			id: scopeName + '.' + appName + '.relationCreateForm.' + node.id,
			border: false,
			autoScroll: false,
			monitorValid: true,
			bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
			defaults: { anchor: '100%', allowBlank: false },
			items: [{
				xtype: 'label',
				fieldLabel: 'Add/Remove relationship',
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
					addRelationship(relationCreateFormPanel, node, scopeName, appName);
				}
			}],
			tbar: new Ext.Toolbar({
				items: [{
					xtype: 'tbspacer',
					width: 4
				}, {
					xtype: 'tbbutton',
					icon: 'Content/img/16x16/apply.png',
					text: 'Apply',
					tooltip: 'Apply the current changes to the data objects tree',
					handler: function () {
						var deleteDataRelationPane = relationCreateFormPanel.items.items[2];
						var gridLabel = scopeName + '.' + appName + '.' + node.id;
						var gridPane = deleteDataRelationPane.items.map[gridLabel];
						if (gridPane) {
							var mydata = gridPane.store.data.items;


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
					tooltip: 'Reset to the latest applied changes',
					handler: function () {
						var relations = new Array();
						relationCreateFormPanel.getForm().reset();
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
						createRelationGrid(scopeName + '.' + appName + '.' + node.id, deleteDataRelationPane, colModel, dataStore, scopeName + '.' + appName + '.-nh-config', scopeName + '.' + appName + '.dataObjectsPane', scopeName + '.' + appName + '.relationCreateForm.' + node.id, 0, scopeName, appName, '');
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
		createRelationGrid(gridLabel, deleteDataRelationPane, colModel, dataStore, scopeName + '.' + appName + '.-nh-config', scopeName + '.' + appName + '.dataObjectsPane', scopeName + '.' + appName + '.relationCreateForm.' + node.id, 0, scopeName, appName, '');
	}
};

function addRelationship(relationCreateFormPanel, node, scopeName, appName) {
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

function findNodeRelatedObjMap(node, relatedObjName) {
	if (node.attributes.attributes)
		var attribute = node.attributes.attributes;
	else
		var attribute = node.attributes;

	if (attribute)
		var relatedObjMap = attribute.relatedObjMap;
	var relateObjItem;
	var ifHas = false;

	if (relatedObjMap)
		for (var i = 0; i < relatedObjMap.length; i++) {
			if (relatedObjMap[i].relatedObjName)
				if (relatedObjMap[i].relatedObjName == relatedObjName) {
					ifHas = true;
					relateObjItem = relatedObjMap[i];
				}
		}

	if (ifHas == false) {
		relateObjItem = {};
		relateObjItem.relatedObjName = relatedObjName;
		relateObjItem.propertyMap = new Array();
		relatedObjMap.push(relateObjItem);
	}

	return relateObjItem.propertyMap;
}

function createRelationGrid(gridlabel, dataGridPanel, colModel, dataStore, configLabel, dbObjLabel, formLabel, callId, scopeName, appName, relatedObjName) {
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
						var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];
						var node = dbObjectsTree.getSelectionModel().getSelectedNode();
						var editPane = dataObjectsPane.items.items[1];
						var form = editPane.items.map[formLabel].getForm();
						var mydata = dataStore.data.items;

						if (callId == 0) {
							var rootNode = dbObjectsTree.getRootNode();
							var numberOfRelation = rootNode.childNodes.length - 1;

							if (mydata.length >= numberOfRelation) {
								if (numberOfRelation == 0) {
									var message = 'Data object "' + node.parentNode.text + '" cannot have any relationship since it is the only data object selected';
									showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
								}
								else {
									var message = 'Data object "' + node.parentNode.text + '" cannot have more than ' + numberOfRelation + ' relationship';
									showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
								}
								return;
							}

							var relationName = form.findField('relationName').getValue().replace(/^\s*/, "").replace(/\s*$/, "");
							if (relationName == '') {
								showDialog(400, 100, 'Warning', msg, Ext.Msg.OK, null);
								return;
							}
							for (var i = 0; i < mydata.length; i++)
								if (mydata[i].data.relationName.toLowerCase() == relationName.toLowerCase()) {
									var message = relationName + ' already exits.';
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

							var propertyName = propertyNameCombo.store.getAt(propertyNameCombo.getValue()).data.text.replace(/^\s*/, "").replace(/\s*$/, "");
							var mapPropertyName = mapPropertyNameCombo.store.getAt(mapPropertyNameCombo.getValue()).data.text.replace(/^\s*/, "").replace(/\s*$/, "");
							if (propertyName == "" || mapPropertyName == "") {
								showDialog(400, 100, 'Warning', msg, Ext.Msg.OK, null);
								return;
							}

							for (var i = 0; i < mydata.length; i++)
								if (mydata[i].data.property.toLowerCase() == propertyName.toLowerCase() && mydata[i].data.relatedProperty.toLowerCase() == mapPropertyName.toLowerCase()) {
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

							var relatedMapItem = findNodeRelatedObjMap(node, relatedObjName);
							relatedMapItem.push([propertyName, mapPropertyName]);
						}
						dataStore.add(newRelationRecord);
						dataStore.commitChanges();

						if (callId == 0) {
							var exitNode = false;

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

							var nodeChildren = new Array();
							for (var j = 0; j < node.childNodes.length; j++)
								nodeChildren.push(node.childNodes[j].text);

							newNodeText = relationName;
							exitNode = false;
							for (var j = 0; j < nodeChildren.length; j++) {
								if (nodeChildren[j].toLowerCase() == newNodeText.toLowerCase()) {
									exitNode = true;
									break;
								}
							}

							if (exitNode == false) {
								var newNode = new Ext.tree.TreeNode({
									text: relationName,
									type: 'relationship',
									leaf: true,
									iconCls: 'treeRelation',
									relatedObjMap: [],
									objectName: node.parentNode.text,
									relatedObjectName: '',
									relationshipType: 'OneToOne',
									relationshipTypeIndex: '1',
									propertyMap: []
								});
								newNode.iconCls = 'treeRelation';
								node.appendChild(newNode);

								if (node.expanded == false)
									node.expand();

								if (!newNode.isSelected())
									newNode.select();

								setRelationFields(editPane, newNode, scopeName, appName);
							}
						}
					}
				}, {
					xtype: 'tbspacer',
					width: 4
				}, {
					xtype: 'tbbutton',
					icon: 'Content/img/list-remove.png',
					text: 'Remove',
					tooltip: 'Remove',
					handler: function () {
						var selectModel = dataRelationGridPane.getSelectionModel();
						if (selectModel.hasSelection()) {
							var selectIndex = selectModel.getSelectedIndex();
							dataStore.removeAt(selectIndex);

							if (callId == 1) {
								var tab = Ext.getCmp('content-panel');
								var rp = tab.items.map[configLabel];
								var dataObjectsPane = rp.items.map[dbObjLabel];
								var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];
								var node = dbObjectsTree.getSelectionModel().getSelectedNode();

								var relatedMapItem = findNodeRelatedObjMap(node, relatedObjName);
								relatedMapItem.remove(relatedMapItem[selectIndex]);
							}
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

function creatRadioField(panel, idLabel, value, serName) {
	if (panel.items) {
		var radioPane = panel.items.map[idLabel + 'radioField'];
		if (radioPane) {
			radioPane.destroy();
		}
	}

	var radioField = new RadioField({
		id: idLabel + 'radioField',
		value: value,
		serName: serName
	});

	panel.add(radioField);
	panel.doLayout();
}

function setRelationFields(editPane, node, scopeName, appName) {
	if (editPane && node) {
		var tab = Ext.getCmp('content-panel');
		var rp = tab.items.map[scopeName + '.' + appName + '.-nh-config'];
		var dataObjectsPane = rp.items.map[scopeName + '.' + appName + '.dataObjectsPane'];

		var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];

		var relationFolderNode = node.parentNode;
		var dataObjectNode = relationFolderNode.parentNode;

		var relatedObjects = new Array();
		var rootNode = dbObjectsTree.getRootNode();
		var thisObj = dataObjectNode.text;
		var ifExist;
		var relAttribute = null;
		var relateObjStr;
		var nodeRelateObj;
		var rindex = 0
		for (var i = 0; i < rootNode.childNodes.length; i++) {
			relateObjStr = rootNode.childNodes[i].text;
			ifExist = false;
			for (var j = 0; j < relationFolderNode.childNodes.length; j++) {
				if (relationFolderNode.childNodes[j].text == '' || relationFolderNode.childNodes[j].id == node.id)
					continue;
				if (relationFolderNode.childNodes[j].attributes.attributes)
					relAttribute = relationFolderNode.childNodes[j].attributes.attributes;
				else if (relationFolderNode.childNodes[j].attributes)
					relAttribute = relationFolderNode.childNodes[j].attributes;

				if (relAttribute) {
					nodeRelateObj = relAttribute.relatedObjectName;
					if (relateObjStr.toLowerCase() == nodeRelateObj.toLowerCase())
						ifExist = true;
				}
			}

			if (relateObjStr.toLowerCase() != thisObj.toLowerCase() && ifExist == false) {
				relatedObjects.push([rindex.toString(), rootNode.childNodes[i].text]);
				rindex++;
			}

		}

		var selectedProperties = new Array();

		var ii = 0;
		if (dataObjectNode) {
			if (dataObjectNode.childNodes[0]) {
				var keysNode = dataObjectNode.childNodes[0];
				for (var i = 0; i < keysNode.childNodes.length; i++)
					if (!keysNode.childNodes[i].hidden) {
						selectedProperties.push([ii.toString(), keysNode.childNodes[i].text]);
						ii++;
					}
			}
			else {
				var keysNode = dataObjectNode.attributes.children[0];
				for (var i = 0; i < keysNode.children.length; i++)
					if (!keysNode.children[i].hidden) {
						selectedProperties.push([ii.toString(), keysNode.children[i].text]);
						ii++;
					}
			}

			if (dataObjectNode.childNodes[1]) {
				var propertiesNode = dataObjectNode.childNodes[1];
				for (var i = 0; i < propertiesNode.childNodes.length; i++)
					if (!propertiesNode.childNodes[i].hidden) {
						selectedProperties.push([ii.toString(), propertiesNode.childNodes[i].text]);
						ii++;
					}
			}
			else {
				var propertiesNode = dataObjectNode.attributes.children[1];
				for (var i = 0; i < propertiesNode.children.length; i++)
					if (!propertiesNode.children[i].hidden) {
						selectedProperties.push([ii.toString(), propertiesNode.children[i].text]);
						ii++;
					}
			}
		}

		var mappingProperties = new Array();
		ii = 0;

		if (editPane.items.map[scopeName + '.' + appName + '.relationFieldsForm.' + node.id]) {
			var relPane = editPane.items.map[scopeName + '.' + appName + '.relationFieldsForm.' + node.id];
			var relatedObjectField = relPane.getForm().findField('relatedObjectName');
			if (relatedObjectField.getValue() != relatedObjectField.lastSelectionText && relatedObjectField.lastSelectionText && relatedObjectField.lastSelectionText != '')
				var relatedObjectName = relatedObjectField.lastSelectionText;
			else
				var relatedObjectName = relatedObjectField.getValue();
		}
		else {
			if (node.attributes.attributes)
				var nodeAttribute = node.attributes.attributes;
			else
				var nodeAttribute = node.attributes;

			var relatedObjectName = nodeAttribute.relatedObjectName;
		}

		if (relatedObjectName != '') {
			var relatedDataObjectNode = rootNode.findChild('text', relatedObjectName);
			if (relatedDataObjectNode) {
				if (relatedDataObjectNode.childNodes[0]) {
					keysNode = relatedDataObjectNode.childNodes[0];
					for (var i = 0; i < keysNode.childNodes.length; i++)
						if (!keysNode.childNodes[i].hidden) {
							mappingProperties.push([ii.toString(), keysNode.childNodes[i].text]);
							ii++;
						}
				}
				else {
					keysNode = relatedDataObjectNode.attributes.children[0];
					for (var i = 0; i < keysNode.children.length; i++)
						if (!keysNode.children[i].hidden) {
							mappingProperties.push([ii.toString(), keysNode.children[i].text]);
							ii++;
						}
				}

				if (relatedDataObjectNode.childNodes[1]) {
					propertiesNode = relatedDataObjectNode.childNodes[1];
					for (var i = 0; i < propertiesNode.childNodes.length; i++)
						if (!propertiesNode.childNodes[i].hidden) {
							mappingProperties.push([ii.toString(), propertiesNode.childNodes[i].text]);
							ii++;
						}
				}
				else {
					propertiesNode = relatedDataObjectNode.attributes.children[1];
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

		if (editPane.items.map[scopeName + '.' + appName + '.relationFieldsForm.' + node.id]) {
			var relationConfigPane = editPane.items.map[scopeName + '.' + appName + '.relationFieldsForm.' + node.id];
			if (relationConfigPane) {
				//relationConfigPane.destroy();
				var panelIndex = editPane.items.indexOf(relationConfigPane);
				editPane.getLayout().setActiveItem(panelIndex);

				var objText = relationConfigPane.getForm().findField('objectName');
				objText.setValue(dataObjectNode.attributes.properties.objectName);

				var relCombo = relationConfigPane.getForm().findField('relatedObjectName');
				if (relCombo.store.data) {
					relCombo.reset();
					relCombo.store.removeAll();
				}
				relCombo.store.loadData(relatedObjects);
				relCombo.store.commitChanges();

				relCombo.setValue(relatedObjectName);
				relationConfigPane.getForm().findField('relatedTable').setValue(relatedObjectName);

				var mapCombo = relationConfigPane.getForm().findField('mapPropertyName');
				if (mapCombo.store.data) {
					mapCombo.reset();
					mapCombo.store.removeAll();
				}
				mapCombo.store.loadData(mappingProperties);
				mapCombo.store.commitChanges();

				var propCombo = relationConfigPane.getForm().findField('propertyName');
				if (propCombo.store.data) {
					propCombo.reset();
					propCombo.store.removeAll();
				}
				propCombo.store.loadData(selectedProperties);
				propCombo.store.commitChanges();
				return;
			}
		}

		var relationConfigPanel = new Ext.FormPanel({
			labelWidth: 143,
			id: scopeName + '.' + appName + '.relationFieldsForm.' + node.id,
			border: false,
			autoScroll: true,
			monitorValid: true,
			bodyStyle: 'background:#eee;padding:10px 0px 0px 10px',
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
				value: node.text,
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
					var relatedObjectName = record.data.field2;
					var relatedDataObjectNode = rootNode.findChild('text', relatedObjectName);

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

					var rnode = dbObjectsTree.getSelectionModel().getSelectedNode();
					var proxyData = findNodeRelatedObjMap(rnode, relatedObjectName);

					var colModel = new Ext.grid.ColumnModel([
                        { id: 'property', header: 'Property', dataIndex: 'property' },
                        { header: 'Related Property', dataIndex: 'relatedProperty' }
                    ]);
					var dataStore = new Ext.data.Store({
						autoDestroy: true,
						proxy: new Ext.data.MemoryProxy(proxyData),
						reader: new Ext.data.ArrayReader({}, [
                          { name: 'property' },
                          { name: 'relatedProperty' }
                        ])
					});
					createRelationGrid(scopeName + '.' + appName + '.' + rnode.id, dataRelationPane, colModel, dataStore, scopeName + '.' + appName + '.-nh-config', scopeName + '.' + appName + '.dataObjectsPane', scopeName + '.' + appName + '.relationFieldsForm.' + rnode.id, 1, scopeName, appName, relatedObjectName);
				}
				}
			}, {
				xtype: 'combo',
				name: 'relationType',
				fieldLabel: 'Relation Type',
				store: [[0, 'OneToOne'], [1, 'OneToMany']],
				mode: 'local',
				editable: false,
				triggerAction: 'all',
				displayField: 'text',
				valueField: 'value'
			}, {
				xtype: 'combo',
				name: 'propertyName',
				fieldLabel: 'Property Name',
				store: new Ext.data.SimpleStore({
				    fields: ['value', 'text'],
				    data: selectedProperties
				}),				
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
				anchor: '100% -180',
				border: false,
				frame: false
			}, {
				xtype: 'textfield',
				name: 'relatedTable',
				hidden: true,
				value: ''
			}],
			tbar: new Ext.Toolbar({
				items: [{
					xtype: 'tbspacer',
					width: 4
				}, {
					xtype: 'tbbutton',
					icon: 'Content/img/16x16/apply.png',
					text: 'Apply',
					tooltip: 'Apply the current changes to the data objects tree',
					handler: function () {
						var relationTypeStr = ['OneToOne', 'OneToMany'];
						if (node.attributes.attributes)
							var attribute = node.attributes.attributes;
						else
							var attribute = node.attributes;

						var newNodeName = relationConfigPanel.getForm().findField('relationshipName').getValue();
						node.setText(newNodeName);

						attribute.relationshipTypeIndex = relationConfigPanel.getForm().findField('relationType').getValue();
						attribute.relationshipType = relationTypeStr[attribute.relationshipTypeIndex];

						var relatedObjectField = relationConfigPanel.getForm().findField('relatedObjectName');
						if (relatedObjectField.getValue() != relatedObjectField.lastSelectionText && relatedObjectField.lastSelectionText && relatedObjectField.lastSelectionText != '')
							var relatedName = relatedObjectField.lastSelectionText;
						else
							var relatedName = relatedObjectField.getValue();

						attribute.relatedObjectName = relatedName;

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
									var dataPropertyName = mydata[i].data.property;
									var relatedPropertyName = mydata[i].data.relatedProperty;
									for (var j = 0; j < propertyMap.length; j++) {
										if (propertyMap[j][0].toLowerCase() == dataPropertyName.toLowerCase() && propertyMap[j][1].toLowerCase() == relatedPropertyName.toLowerCase()) {
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
										dataPropertyName = mydata[i].data.property;
										relatedPropertyName = mydata[i].data.relatedProperty;
										if (attribute.propertyMap[j].dataPropertyName.toLowerCase() == dataPropertyName.toLowerCase() && attribute.propertyMap[j].relatedPropertyName.toLowerCase() == relatedPropertyName.toLowerCase()) {
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
					tooltip: 'Reset to the latest applied changes',
					handler: function () {
						var newNodeName = relationConfigPanel.getForm().findField('relationshipName');
						newNodeName.setValue(node.text);
						var propertyNameCombo = relationConfigPanel.getForm().findField('propertyName');
						propertyNameCombo.setValue('');
						propertyNameCombo.clearInvalid();
						var mapPropertyNameCombo = relationConfigPanel.getForm().findField('mapPropertyName');
						mapPropertyNameCombo.setValue('');
						mapPropertyNameCombo.clearInvalid();

						var properMap = new Array();
						var dataRelationPane = relationConfigPanel.items.items[7];

						if (node.attributes.propertyMap)
							var attribute = node.attributes;
						else if (node.attributes.attributes.propertyMap)
							var attribute = node.attributes.attributes;

						if (attribute) {
							relationConfigPanel.getForm().findField('relatedObjectName').setValue(attribute.relatedObjectName);
							relationConfigPanel.getForm().findField('relationType').setValue(attribute.relationshipTypeIndex);

							var relatedMapItem = findNodeRelatedObjMap(node, attribute.relatedObjectName);
							var relPropertyName;
							var relMapPropertyName;

							for (var i = 0; i < relatedMapItem.length; i++) {
								relatedMapItem.remove(relatedMapItem[i]);
								i--;
							}

							for (i = 0; i < attribute.propertyMap.length; i++) {
								relPropertyName = attribute.propertyMap[i].dataPropertyName.toUpperCase();
								relMapPropertyName = attribute.propertyMap[i].relatedPropertyName.toUpperCase();
								properMap.push([relPropertyName, relMapPropertyName]);
								relatedMapItem.push([relPropertyName, relMapPropertyName]);
							}

							var colModel = new Ext.grid.ColumnModel([
                  { id: 'property', header: 'Property', dataIndex: 'property' },
                  { header: 'Related Property', dataIndex: 'relatedProperty' }
                ]);
							var dataStore = new Ext.data.Store({
								autoDestroy: true,
								proxy: new Ext.data.MemoryProxy(properMap),
								reader: new Ext.data.ArrayReader({}, [
                    { name: 'property' },
                    { name: 'relatedProperty' }
                  ])
							});
							createRelationGrid(scopeName + '.' + appName + '.' + node.id, dataRelationPane, colModel, dataStore, scopeName + '.' + appName + '.-nh-config', scopeName + '.' + appName + '.dataObjectsPane', scopeName + '.' + appName + '.relationFieldsForm.' + node.id, 1, scopeName, appName, attribute.relatedObjectName);
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
		relationConfigForm.findField('relatedTable').setValue(relatedObjectName);
		if (node.attributes.attributes)
			var relationTypeIndex = node.attributes.attributes.relationshipTypeIndex;
		else
			var relationTypeIndex = node.attributes.relationshipTypeIndex;
		relationConfigForm.findField('relationType').setValue(relationTypeIndex);

		if (node.attributes.attributes)
			var propertyMaps = node.attributes.attributes.propertyMap;
		else
			var propertyMaps = node.attributes.propertyMap;

		var configLabel = scopeName + '.' + appName + '.-nh-config';
		var gridLabel = scopeName + '.' + appName + '.' + node.id;
		if (dataRelationPane.items) {
			var gridPane = dataRelationPane.items.map[gridLabel];
			if (gridPane) {
				gridPane.destroy();
			}
		}
		var myArray = new Array();
		var i = 0;

		var relatedMapItem = findNodeRelatedObjMap(node, relatedObjectName);
		var relPropertyName;
		var relMapPropertyName;
		for (i = 0; i < propertyMaps.length; i++) {
			relPropertyName = propertyMaps[i].dataPropertyName.toUpperCase();
			relMapPropertyName = propertyMaps[i].relatedPropertyName.toUpperCase();
			myArray.push([relPropertyName, relMapPropertyName]);
			relatedMapItem.push([relPropertyName, relMapPropertyName]);
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
		createRelationGrid(gridLabel, dataRelationPane, colModel, dataStore, scopeName + '.' + appName + '.-nh-config', scopeName + '.' + appName + '.dataObjectsPane', scopeName + '.' + appName + '.relationFieldsForm.' + node.id, 1, scopeName, appName, relatedObjectName);
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

	var propertyName = selectPropComboBox.store.getAt(selectPropComboBox.getValue()).data.text.replace(/^\s*/, "").replace(/\s*$/, "");
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
				if (myPropMap[i].data.property.toLowerCase() == propertyName.toLowerCase() && myPropMap[i].data.relatedProperty.toLowerCase() == mapPropertyName.toLowerCase()) {
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

Ext.grid.RowSelectionModel.override({
	getSelectedIndex: function () {
		return this.grid.store.indexOf(this.selections.itemAt(0));
	}
});