Ext.require([
    'Ext.grid.*',
    'Ext.data.*',
    'Ext.util.*',
    'Ext.state.*'
]);

Ext.define('AM.view.nhibernate.PropertyMapGridPanel', {
  extend: 'Ext.grid.Panel',
  alias: 'widget.propertygridpanel',
  frame: false,
  stateful: true,
  store: null,
  stateId: 'propertyGrid',
  border: false,
  enableColLock: true,
  autoScroll: true,
  node: null,
  columns: [{
    text: 'Property',
    flex: 1,
    dataIndex: 'property'
  }, {
    hidden: true,
    dataIndex: 'columnName'
  }, {
    text: 'Related Property',
    flex: 1,
    dataIndex: 'relatedProperty'
  }, {
    hidden: true,
    dataIndex: 'relatedColumnName'
  }],
  rootNode: null,
  propertyPairs: null,
  dataGridPanel: null,
  relatedObjName: null,  

  initComponent: function () {
    var me = this;
    var node = me.node;
    var relatedObjName = me.relatedObjName;

    me.store = Ext.create('Ext.data.ArrayStore', {
      model: AM.model.PropertyMapModel,
      data: me.propertyPairs
    });

    this.tbar = new Ext.Toolbar({
      items: [{
        xtype: 'tbspacer',
        width: 4
      }, {
        xtype: 'button',
        icon: 'Content/img/16x16/list-add.png',
        text: 'Add',
        tooltip: 'Add',
        handler: function () {
          var msg = 'Relationship name cannot be added when the field is blank.';
          var mydata = me.store.data.items;
          var numberOfRelation = me.rootNode.childNodes.length - 1;
          var form = me.dataGridPanel.getForm();
          relatedObjName = form.findField('relatedObjectName').rawValue;
          var propertyNameCombo = form.findField('propertyName');
          var mapPropertyNameCombo = form.findField('mapPropertyName');

          if (propertyNameCombo.getValue() == undefined || mapPropertyNameCombo.getValue() == undefined)
            return;

          var propertyName = propertyNameCombo.store.getAt(propertyNameCombo.getValue()).data.text.replace(/^\s*/, "").replace(/\s*$/, "");
          var mapPropertyName = mapPropertyNameCombo.store.getAt(mapPropertyNameCombo.getValue()).data.text.replace(/^\s*/, "").replace(/\s*$/, "");
          var columnName = propertyNameCombo.store.getAt(propertyNameCombo.getValue()).data.name.replace(/^\s*/, "").replace(/\s*$/, "");
          var relatedColumnName = mapPropertyNameCombo.store.getAt(mapPropertyNameCombo.getValue()).data.name.replace(/^\s*/, "").replace(/\s*$/, "");
          
          if (propertyName == "" || mapPropertyName == "") {
            showDialog(400, 100, 'Warning', msg, Ext.Msg.OK, null);
            return;
          }

          if (mydata.length > 0) {
            for (var i = 0; i < mydata.length; i++)
              if (mydata[i].data.property.toLowerCase() == propertyName.toLowerCase() && mydata[i].data.relatedProperty.toLowerCase() == mapPropertyName.toLowerCase()) {
                var message = 'The pair of ' + propertyName + ' and ' + mapPropertyName + ' cannot be added because the pair already exits.';
                showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
                return;
              }
          }

          var newPropertyMapRecord = new AM.model.PropertyMapModel({
            property: propertyName,
            columnName: columnName,
            relatedProperty: mapPropertyName,
            relatedColumnName: relatedColumnName
          });

          me.store.add(newPropertyMapRecord);
          var propertyMap = findNodeRelatedObjMap(node, relatedObjName);
          propertyMap.push([propertyName, columnName, mapPropertyName, relatedColumnName]);
          me.dataGridPanel.doLayout();
        }
      }, {
        xtype: 'tbspacer',
        width: 4
      }, {
        xtype: 'button',
        icon: 'Content/img/16x16/list-remove.png',
        text: 'Remove',
        tooltip: 'Remove',
        handler: function () {
          var selectModel = me.getSelectionModel();
          if (selectModel.hasSelection()) {
            var selectRecord = selectModel.getLastFocused();
            var index = me.store.indexOf(selectRecord);

            if (relatedObjName == '') {
              var form = me.dataGridPanel.getForm();
              relatedObjName = form.findField('relatedObjectName');
            }

            var propertyMap = findNodeRelatedObjMap(node, relatedObjName);
            propertyMap.splice(index, 1);
            me.store.remove(selectRecord);
          }
          else {
            if (me.store.data.items.length < 1)
              showDialog(400, 100, 'Warning', 'No records exits in the table', Ext.Msg.OK, null);
            else
              showDialog(400, 100, 'Warning', 'Please select a row first.', Ext.Msg.OK, null);
          }
        }
      }]
    });

    this.callParent(arguments);
  }
});

function findNodeRelatedObjMap(node, relatedObjName) {
  if (node.data.relatedObjMap)
    var relatedObjMap = node.data.relatedObjMap;  
  else
    var relatedObjMap = new Array();

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
};
         
function createPropertyMapGrid(relationPanel, rootNode, node, gridlabel, dataGridPanel, propertyPairs, dbObjLabel, formLabel, callId, scopeName, appName, relatedObjName) {
  if (dataGridPanel.items) {
    var relationPane = dataGridPanel.items.items[0];
    if (relationPane) {
      relationPane.destroy();
    }
  }
  
  var conf = {
    id: gridlabel,
    node: node,
    rootNode: rootNode,
    dataGridPanel: relationPanel,
    relatedObjName: relatedObjName,
    propertyPairs: propertyPairs
  };

  var propertyGrid = Ext.widget('propertygridpanel', conf);
  dataGridPanel.items.add(propertyGrid);
  dataGridPanel.doLayout();
};

