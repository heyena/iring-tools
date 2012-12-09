/*
 * File: Scripts/AM/view/nhibernate/RelationsGrid.js
 *
 * This file was generated by Sencha Architect version 2.1.0.
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

Ext.define('AM.view.nhibernate.RelationsGrid', {
  extend: 'Ext.grid.Panel',
  alias: 'widget.relationsgrid',

  frame: false,
  store: 'RelationsStore',

  initComponent: function() {
    var me = this;

    Ext.applyIf(me, {
      viewConfig: {

      },
      columns: [
        {
          xtype: 'gridcolumn',
          dataIndex: 'relationName',
          flex: 1,
          text: 'Data Relationship Name'
        }
      ],
      dockedItems: [
        {
          xtype: 'toolbar',
          dock: 'top',
          weight: 50,
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
              xtype: 'tbseparator',
              width: 4
            },
            {
              xtype: 'button',
              iconCls: 'am-list-remove',
              text: 'Remove',
              listeners: {
                click: {
                  fn: me.removeRelationship,
                  scope: me
                }
              }
            }
          ]
        }
      ]
    });

    me.callParent(arguments);
  },

  onAddClick: function(button, e, options) {
    var me = this;
    var form = button.up('relationsform');
    form.addRelationship(form);
  },

  removeRelationship: function(button, e, options) {
    var grid = button.up('relationsgrid');
    var gridStore = grid.getStore();
    var selected = grid.getSelectionModel().getSelection()[0];
    if(!selected){
      var message = 'Please select a relationship to remove.';
      showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
      return;
    }
    var relation = selected.data.relationName;
    var exist = gridStore.find('relationName', relation);
    if (exist != -1)
    gridStore.removeAt(exist);
  }

});