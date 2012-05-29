Ext.define('AM.view.nhibernate.SetRelationPanel', {
  extend: 'Ext.form.Panel',
  alias: 'widget.setrelationform',
  border: false,
  contextName: null,
  relatedObjects: null,
  selectedProperties: null,
  mappingProperties: null,
  node: null,
  endpoint: null,
  autoScroll: true,
  monitorValid: true,
  rootNode: null,
  bodyStyle: 'background:#eee;padding:10 0 0 10',
  defaults: {
    labelWidth: 130,
    allowBlank: false
  },

  initComponent: function () {
    var me = this;
    var node = me.node;
    var dataObjectNode = node.parentNode.parentNode;
    var rootNode = me.rootNode;
    var contextName = me.contextName;
    var endpoint = me.endpoint;
    var relatedObjects = me.relatedObjects;
    var selectedProperties = me.selectedProperties;
    var mappingProperties = me.mappingProperties;

    this.items = [{
      xtype: 'label',
      text: 'Configure Relationship',
      cls: 'x-form-item',
      style: 'font-weight:bold;'
    }, {
      xtype: 'textfield',
      name: 'relationshipName',
      fieldLabel: 'Relationship Name',
      value: node.data.text,
      allowBlank: false,
      anchor: '100%'
    }, {
      xtype: 'textfield',
      name: 'objectName',
      fieldLabel: 'Object Name',
      value: dataObjectNode.data.text,
      readOnly: true,
      allowBlank: false,
      anchor: '100%'
    }, {
      xtype: 'combo',
      name: 'relatedObjectName',
      fieldLabel: 'Related Object Name',
      store: Ext.create('Ext.data.SimpleStore', {
        fields: ['value', 'text'],
        autoLoad: true,
        data: relatedObjects
      }),
      queryMode: 'local',
      editable: false,
      triggerAction: 'all',
      displayField: 'text',
      valueField: 'value',
      selectOnFocus: true,
      anchor: '100%',
      listeners: { 'select': function (combo, record) {
        var relatedObjectName = record[0].data.text;

        if (relatedObjectName != '') {
          var relatedDataObjectNode = rootNode.findChild('text', relatedObjectName);
          var relationConfigPanel = me.getForm();
          var mappingProperties = new Array();

          if (relatedDataObjectNode.childNodes[1]) {
            keysNode = relatedDataObjectNode.childNodes[0];
            propertiesNode = relatedDataObjectNode.childNodes[1];
            var ii = 0;

            for (var i = 0; i < keysNode.childNodes.length; i++) {
              mappingProperties.push([ii, keysNode.childNodes[i].data.text, keysNode.childNodes[i].data.property.columnName]);
              ii++;
            }

            for (var i = 0; i < propertiesNode.childNodes.length; i++) {
              mappingProperties.push([ii, propertiesNode.childNodes[i].data.text, propertiesNode.childNodes[i].data.property.columnName]);
              ii++;
            }
          }

          var mapCombo = relationConfigPanel.findField('mapPropertyName');

          if (mapCombo.store.data) {
            mapCombo.store.removeAll();
          }
          mapCombo.setValue(null);
          mapCombo.setRawValue(null);
          mapCombo.store.loadData(mappingProperties);
          var proxyData = findNodeRelatedObjMap(node, relatedObjectName);
          var dataGridPanel = me.items.items[7];
          var gridPane = dataGridPanel.items.items[0];
          var store = gridPane.store;

          if (store.data) {
            gridPane.store.removeAll();
          }

          gridPane.store.loadData(proxyData);
          dataGridPanel.doLayout();
          relationConfigPanel.findField('relatedTable').setValue(relatedDataObjectNode.data.property.tableName);
        }
      }
      }
    }, {
      xtype: 'combo',
      name: 'relationType',
      fieldLabel: 'Relation Type',
      store: [[0, 'OneToOne'], [1, 'OneToMany']],
      queryMode: 'local',
      editable: false,
      triggerAction: 'all',
      displayField: 'text',
      valueField: 'value',
      anchor: '100%'
    }, {
      xtype: 'combo',
      name: 'propertyName',
      fieldLabel: 'Property Name',
      store: Ext.create('Ext.data.SimpleStore', {
        fields: ['value', 'text', 'name'],
        autoLoad: true,
        data: selectedProperties
      }),
      queryMode: 'local',
      editable: false,
      triggerAction: 'all',
      displayField: 'text',
      valueField: 'value',
      selectOnFocus: true,
      anchor: '100%',
      listeners: { 'select': function (combo, record, index) {
        var selectproperty = record[0].data.field2;
      }
      }
    }, {
      xtype: 'combo',
      name: 'mapPropertyName',
      fieldLabel: 'Mapping Property',
      store: Ext.create('Ext.data.SimpleStore', {
        fields: ['value', 'text', 'name'],
        autoLoad: true,
        data: mappingProperties
      }),
      queryMode: 'local',
      editable: false,
      triggerAction: 'all',
      displayField: 'text',
      valueField: 'value',
      selectOnFocus: true,
      anchor: '100%',
      listeners: { 'select': function (combo, record, index) {
        var mapproperty = record[0].data.field2;
      }
      }
    }, {
      xtype: 'panel',
      id: contextName + '.' + endpoint + '.dataRelationPane.' + node.id,
      name: 'propertyMapGridPanel',
      bodyStyle: 'background:#eee',
      anchor: '100%',
      border: false,
      autoScroll: false,
      items: [],
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
	      xtype: 'button',
	      icon: 'Content/img/16x16/apply.png',
	      text: 'Apply',
	      tooltip: 'Apply the current changes to the data objects tree',
	      handler: function () {
	        var relationTypeStr = ['OneToOne', 'OneToMany'];
	        var thisForm = me.getForm();
	        var newNodeName = thisForm.findField('relationshipName').getValue();
	        node.set('title', newNodeName);
	        var relationTypeField = thisForm.findField('relationType');

	        if (relationTypeField.store.getAt(relationTypeField.getValue()) != undefined) {
	          node.data.relationshipTypeIndex = thisForm.findField('relationType').getValue();
	          node.data.relationshipType = relationTypeStr[node.data.relationshipTypeIndex];
	        }
	        else {
	          node.data.relationshipType = thisForm.findField('relationType').getValue();
	          node.data.relationshipTypeIndex = relationTypeStr.indexOf(node.data.relationshipType);
	        }

	        var relatedName = thisForm.findField('relatedObjectName').rawValue;
	        node.data.relatedObjectName = relatedName;
	        var dataRelationPane = me.items.items[7];
	        var gridPane = dataRelationPane.items.items[0];

	        if (gridPane) {
	          var mydata = gridPane.store.data.items;
	          var propertyMap = new Array();

	          if (node.data) {
	            if (node.data.propertyMap)
	              for (i = 0; i < node.data.propertyMap.length; i++)
	                propertyMap.push([node.data.propertyMap[i].dataPropertyName, node.data.propertyMap[i].relatedPropertyName]);
	            else
	              node.data.propertyMap = [];

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
	                var mapItem = {};
	                mapItem['dataPropertyName'] = dataPropertyName;
	                mapItem['relatedPropertyName'] = relatedPropertyName;
	                node.data.propertyMap.push(mapItem);
	              }
	            }

	            for (var jj = 0; jj < node.data.propertyMap.length; jj++) {
	              exitPropertyMap = false;

	              for (var i = 0; i < mydata.length; i++) {
	                dataPropertyName = mydata[i].data.property;
	                relatedPropertyName = mydata[i].data.relatedProperty;

	                if (node.data.propertyMap[jj].dataPropertyName.toLowerCase() == dataPropertyName.toLowerCase() && node.data.propertyMap[jj].relatedPropertyName.toLowerCase() == relatedPropertyName.toLowerCase()) {
	                  exitPropertyMap = true;
	                  break;
	                }
	              }

	              if (exitPropertyMap == false) {
	                node.data.propertyMap.splice(jj, 1);
	                jj--;
	              }
	            }
	          }
	        }
	      }
	    }, {
	      xtype: 'tbspacer',
	      width: 4
	    }, {
	      xtype: 'button',
	      icon: 'Content/img/16x16/edit-clear.png',
	      text: 'Reset',
	      tooltip: 'Reset to the latest applied changes',
	      handler: function () {
	        var thisForm = me.getForm();
	        var newNodeName = thisForm.findField('relationshipName');
	        newNodeName.setValue(node.data.text);
	        var propertyNameCombo = thisForm.findField('propertyName');
	        propertyNameCombo.setValue('');
	        propertyNameCombo.clearInvalid();
	        var mapPropertyNameCombo = thisForm.findField('mapPropertyName');
	        mapPropertyNameCombo.setValue(null);
	        mapPropertyNameCombo.setRawValue(null);
	        mapPropertyNameCombo.clearInvalid();

	        if (mapPropertyNameCombo.store.data)
	          mapPropertyNameCombo.store.removeAll();

	        var properMap = new Array();
	        var dataRelationPane = me.items.items[7];

	        if (node.data)
	          var attribute = node.data;

	        if (attribute) {
	          var relatedObjNameField = thisForm.findField('relatedObjectName');
	          relatedObjNameField.setValue(attribute.relatedObjectName);

	          if (attribute.relatedObjectName == '')
	            relatedObjNameField.clearInvalid();

	          thisForm.findField('relationType').setValue(attribute.relationshipTypeIndex);
	          var relatedMapItem = findNodeRelatedObjMap(node, attribute.relatedObjectName);
	          var relPropertyName;
	          var relMapPropertyName;
	          var columnName;
	          var relatedColumnName;

	          for (var i = 0; i < relatedMapItem.length; i++) {
	            relatedMapItem.splice(i, 1);
	            i--;
	          }

	          if (attribute.propertyMap)
	            for (i = 0; i < attribute.propertyMap.length; i++) {
	              relPropertyName = attribute.propertyMap[i].dataPropertyName;
	              relMapPropertyName = attribute.propertyMap[i].relatedPropertyName;
	              columnName = attribute.propertyMap[i].columnName;
	              relatedColumnName = attribute.propertyMap[i].relatedColumnName;
	              properMap.push([relPropertyName, relMapPropertyName]);
	              relatedMapItem.push([relPropertyName, relMapPropertyName]);
	            }

	          var dataGridPanel = me.items.items[7];
	          var gridPane = dataGridPanel.items.items[0];
	          var store = gridPane.store;

	          if (store.data) {
	            gridPane.store.removeAll();
	          }

	          gridPane.store.loadData(properMap);
	          dataGridPanel.doLayout();
	        }
	      }
	    }]
	  });

    this.callParent(arguments);
  }
});

function addPropertyMapping (relationConfigPanel) {
	var dataRelationPane = relationConfigPanel.items.items[7];
	var relationConfigForm = relationConfigPanel.getForm();
	var selectPropComboBox = relationConfigForm.findField("propertyName");
	var mapPropComboBox = relationConfigForm.findField("mapPropertyName");
	var message;

  if (!selectPropComboBox.getValue() || !mapPropComboBox.getValue()) {
		message = 'Please select a property name and a mapping property.';
		showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
		return;
	}

	var propertyName = selectPropComboBox.store.getAt(selectPropComboBox.getValue()).data.field2.replace(/^\s*/, "").replace(/\s*$/, "");
	var mapPropertyName = mapPropComboBox.store.getAt(mapPropComboBox.getValue()).data.text.replace(/^\s*/, "").replace(/\s*$/, "");
	
  if (propertyName == "" || mapPropertyName == "") {
		message = 'Property Name or Mapping Property cannot be blank.';
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
					message = 'The pair of ' + propertyName + ' and ' + mapPropertyName + ' already exits.';
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



