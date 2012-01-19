Ext.define('AM.view.nhibernate.CreateRelations', {
    extend: 'Ext.form.Panel',
    alias: 'widget.createRelations',
    labelWidth: 160,
    border: false,
    autoScroll: false,
    monitorValid: true,
    bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
    defaults: { anchor: '100%', allowBlank: false },

    initComponent: function () {
        this.items = [{
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
        }];
        this.keys = [{
            key: [Ext.EventObject.ENTER], handler: function () {
                addRelationship(relationCreateFormPanel, node, scopeName, appName);
            }
        }];
        this.tbar = new Ext.Toolbar({
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
});


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

							var propertyName = propertyNameCombo.store.getAt(propertyNameCombo.getValue()).data.field2.replace(/^\s*/, "").replace(/\s*$/, "");
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
									iconCls: 'relation',
									relatedObjMap: [],
									objectName: node.parentNode.text,
									relatedObjectName: '',
									relationshipType: 'OneToOne',
									relationshipTypeIndex: '1',
									propertyMap: []
								});
								newNode.iconCls = 'relation';
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

