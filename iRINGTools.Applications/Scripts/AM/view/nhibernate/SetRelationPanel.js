Ext.define('AM.view.nhibernate.SetRelationPanel', {
  extend: 'Ext.form.Panel',
  alias: 'widget.setrelationform',
  labelWidth: 143,
	border: false,
	autoScroll: true,
	monitorValid: true,
	bodyStyle: 'background:#eee;padding:10px 0px 0px 10px',
	defaults: { anchor: '100%', allowBlank: false },

  initComponent: function () {
    this.items = [{
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
		anchor: '100% -180',
		border: false,
		frame: false
	}, {
		xtype: 'textfield',
		name: 'relatedTable',
		hidden: true,
		value: ''
	}],
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
				var relationTypeStr = ['OneToOne', 'OneToMany'];
				if (node.attributes.attributes)
					var attribute = node.attributes.attributes;
				else
					var attribute = node.attributes;

				var newNodeName = relationConfigPanel.getForm().findField('relationshipName').getValue();
						
				node.set('title', newNodeName);
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
	  });
//		
//	  editPane.add(relationConfigPanel);
//	  var panelIndex = editPane.items.indexOf(relationConfigPanel);
//	  editPane.getLayout().setActiveItem(panelIndex);
//	  var relationConfigForm = relationConfigPanel.getForm();
//	  var dataRelationPane = relationConfigPanel.items.items[7];
//	  relationConfigForm.findField('relatedObjectName').setValue(relatedObjectName);
//	  relationConfigForm.findField('relatedTable').setValue(relatedObjectName);
//	  if (node.attributes.attributes)
//		  var relationTypeIndex = node.attributes.attributes.relationshipTypeIndex;
//	  else
//		  var relationTypeIndex = node.attributes.relationshipTypeIndex;
//	  relationConfigForm.findField('relationType').setValue(relationTypeIndex);

//	  if (node.attributes.attributes)
//		  var propertyMaps = node.attributes.attributes.propertyMap;
//	  else
//		  var propertyMaps = node.attributes.propertyMap;

//	  var configLabel = scopeName + '.' + appName + '.-nh-config';
//	  var gridLabel = scopeName + '.' + appName + '.' + node.id;
//	  if (dataRelationPane.items) {
//		  var gridPane = dataRelationPane.items.map[gridLabel];
//		  if (gridPane) {
//			  gridPane.destroy();
//		  }
//	  }
//	  var myArray = new Array();
//	  var i = 0;

//	  var relatedMapItem = findNodeRelatedObjMap(node, relatedObjectName);
//	  var relPropertyName;
//	  var relMapPropertyName;
//	  for (i = 0; i < propertyMaps.length; i++) {
//		  relPropertyName = propertyMaps[i].dataPropertyName.toUpperCase();
//		  relMapPropertyName = propertyMaps[i].relatedPropertyName.toUpperCase();
//		  myArray.push([relPropertyName, relMapPropertyName]);
//		  relatedMapItem.push([relPropertyName, relMapPropertyName]);
//	  }
//	  var colModel = new Ext.grid.ColumnModel([
//      { id: 'property', header: 'Property', width: 230, dataIndex: 'property' },
//      { header: 'Related Property', width: 230, dataIndex: 'relatedProperty' }
//    ]);
//	  var dataStore = new Ext.data.Store({
//		  autoDestroy: true,
//		  proxy: new Ext.data.MemoryProxy(myArray),
//		  reader: new Ext.data.ArrayReader({}, [
//        { name: 'property' },
//        { name: 'relatedProperty' }
//      ])
//	  });
//	  createRelationGrid(gridLabel, dataRelationPane, colModel, dataStore, scopeName + '.' + appName + '.-nh-config', scopeName + '.' + appName + '.dataObjectsPane', scopeName + '.' + appName + '.relationFieldsForm.' + node.id, 1, scopeName, appName, relatedObjectName);
  }
});

function addPropertyMapping (relationConfigPanel) {
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
};

function hasShown(shownArray, text) {
    for (var shownIndex = 0; shownIndex < shownArray.length; shownIndex++)
        if (shownArray[shownIndex] == text)
            return true;
    return false;
};

