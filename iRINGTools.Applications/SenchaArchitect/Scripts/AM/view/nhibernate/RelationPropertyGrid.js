/*
 * File: Scripts/AM/view/nhibernate/RelationPropertyGrid.js
 *
 * This file was generated by Sencha Architect version 2.2.2.
 * http://www.sencha.com/products/architect/
 *
 * This file requires use of the Ext JS 4.1.x library, under independent license.
 * License of Sencha Architect does not include license for Ext JS 4.1.x. For more
 * details see http://www.sencha.com/license or contact license@sencha.com.
 *
 * This file will be auto-generated each and everytime you save your project.
 *
 * Do NOT hand edit this file.
 */

Ext.define('AM.view.nhibernate.RelationPropertyGrid', {
  extend: 'Ext.grid.Panel',
  alias: 'widget.relationPropertyGrid',

  requires: [
    'AM.view.override.nhibernate.RelationPropertyGrid'
  ],

  itemId: 'relationPropertyGrid',
  store: 'RelationStore',

  initComponent: function() {
    var me = this;

    Ext.applyIf(me, {
      dockedItems: [
        {
          xtype: 'toolbar',
          dock: 'top',
          items: [
            {
              xtype: 'button',
              iconCls: 'am-list-add',
              text: 'Add',
              listeners: {
                click: {
                  fn: me.onAddClick,
                  scope: me
                }
              }
            },
            {
              xtype: 'tbspacer',
              width: 4
            },
            {
              xtype: 'button',
              iconCls: 'am-list-remove',
              text: 'Remove',
              listeners: {
                click: {
                  fn: me.onRemoveClick,
                  scope: me
                }
              }
            }
          ]
        }
      ],
      columns: [
        {
          xtype: 'gridcolumn',
          dataIndex: 'property',
          text: 'Property',
          flex: 1
        },
        {
          xtype: 'gridcolumn',
          dataIndex: 'relatedProperty',
          text: 'Related Property',
          flex: 1
        }
      ]
    });

    me.callParent(arguments);
  },

  onAddClick: function(button, e, eOpts) {
    var me = this;

    var form = button.up('setrelationform');
    var relationshipName = form.getForm().findField('relationshipName').getValue();
    var treeNode = form.node;
    var propertyCmb = form.down('#propertyNameCmb');
    var mapPropertyCmb = form.down('#mapPropertyNameCmb');

    var message;

    if (!propertyCmb.getValue() || !mapPropertyCmb.getValue()) {
      message = 'Please select a property name and a mapping property.';
      showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
      message = undefined;
      return;
    }

    var grid = form.down('relationPropertyGrid');
    var store = grid.getStore();

    var propertyName = propertyCmb.getValue().replace(/^\s*/, "").replace(/\s*$/, "");
    var mapProperty = mapPropertyCmb.getValue().replace(/^\s*/, "").replace(/\s*$/, "");

    var mapRecord = {'property': propertyName, 'relatedProperty': mapProperty};
    //var mapRecord = {'value': propertyName, 'text': mapProperty};
    var mapRecordForNode = {'dataPropertyName': propertyName, 'relatedPropertyName': mapProperty};
    //var propertErr = store.find('value', propertyName);
    var propertErr = store.find('property', propertyName);
    //var mapErr = store.find('text', mapProperty);
    var mapErr = store.find('relatedProperty', mapProperty);

    if (propertErr != -1) {
      message = 'Property [' + propertyName + '] already in a mapping!';
      showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
      message = undefined;
      return; 
    }

    if (mapErr != -1) {
      message = 'Related Property [' + mapProperty + '] already in a mapping!';
      showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
      message = undefined;
      return;
    }

    //utilsObj.relationGridStore = null;
    store.add(mapRecord);
    /*if(treeNode.childNodes.length<2){

    if(treeNode.firstChild.raw.propertyMap!=undefined){
    treeNode.firstChild.raw.propertyMap.push(mapRecordForNode);
    if(treeNode.firstChild.data.propertyMap!=undefined) 
    treeNode.firstChild.data.propertyMap.push(mapRecordForNode);
    }
    else{
    treeNode.firstChild.data.propertyMap = [];
    treeNode.firstChild.raw.propertyMap = [];
    store.each(function(record) {
    treeNode.firstChild.data.propertyMap.push({'dataPropertyName': record.data.property, 'relatedPropertyName': record.data.relatedProperty});
    treeNode.firstChild.raw.propertyMap.push({'dataPropertyName': record.data.property, 'relatedPropertyName': record.data.relatedProperty});
    });
    }
    }else{*/

    treeNode.eachChild(function(node) {
      if(node.data.text == relationshipName){
        if(node.data.propertyMap!==undefined){
          node.data.propertyMap.push(mapRecordForNode);
          node.raw.propertyMap.push(mapRecordForNode);
        }else{
          node.data.propertyMap = [];
          node.raw.propertyMap = [];
          store.each(function(record) {
            node.data.propertyMap.push({'dataPropertyName': record.data.property, 'relatedPropertyName': record.data.relatedProperty});
            node.raw.propertyMap.push({'dataPropertyName': record.data.property, 'relatedPropertyName': record.data.relatedProperty});
          });
        }
      }

    });
    //}




  },

  onRemoveClick: function(button, e, eOpts) {
    var me = this;
    var form = button.up('setrelationform');
    var relationshipName = form.getForm().findField('relationshipName').getValue();
    var treeNode = form.node;
    var grid = form.down('relationPropertyGrid');
    var selectedRec = grid.getSelectionModel().selected.items[0];
    var store = grid.getStore();
    var tempPropertyMap = [];
    if (grid.getSelectionModel().hasSelection()) {
      store.remove(selectedRec);
      store.each(function(record) {
        tempPropertyMap.push({'dataPropertyName': record.data.property, 'relatedPropertyName': record.data.relatedProperty});
      });
      //treeNode.firstChild.raw.propertyMap = tempPropertyMap;

      treeNode.eachChild(function(node) {
        if(node.data.text == relationshipName){
          node.raw.propertyMap = tempPropertyMap;
        }

      });


    }
    else {
      if (store.data.items.length < 1)
      showDialog(400, 100, 'Warning', 'No records exits in the table', Ext.Msg.OK, null);
      else
      showDialog(400, 100, 'Warning', 'Please select a row first.', Ext.Msg.OK, null);
    }
  }

});