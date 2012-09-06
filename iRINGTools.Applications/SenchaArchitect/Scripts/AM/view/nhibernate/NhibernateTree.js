/*
 * File: Scripts/AM/view/nhibernate/NhibernateTree.js
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

Ext.define('AM.view.nhibernate.NhibernateTree', {
  extend: 'Ext.tree.Panel',
  alias: 'widget.nhibernatetree',

  bodyStyle: 'padding:0.5px 0px 1px 1px',
  store: 'NHibernateTreeStore',

  initComponent: function() {
    var me = this;

    Ext.applyIf(me, {
      root: {
        expanded: true,
        type: 'DATAOBJECTS',
        text: 'Data Objects',
        iconCls: 'folder'
      },
      viewConfig: {

      },
      dockedItems: [
        {
          xtype: 'toolbar',
          dock: 'top',
          items: [
            {
              xtype: 'tbspacer',
              width: 4
            },
            {
              xtype: 'button',
              handler: function(button, event) {
                me.onReload()
              },
              action: 'reloaddataobjects',
              iconCls: 'am-view-refresh',
              text: 'Reload',
              tooltip: 'Reload Data Objects'
            },
            {
              xtype: 'tbspacer',
              width: 4
            },
            {
              xtype: 'button',
              action: 'editdbconnection',
              iconCls: 'am-document-properties',
              text: 'Edit Connection'
            },
            {
              xtype: 'tbspacer',
              width: 4
            },
            {
              xtype: 'button',
              action: 'savedbobjectstree',
              iconCls: 'am-document-save',
              text: 'Save',
              tooltip: 'Save the data objects tree to the back-end server'
            }
          ]
        }
      ]
    });

    me.callParent(arguments);
  },

  onReload: function() {
    var me = this;
    var selection = me.getSelectionModel();
    var store = me.store;
    var node = selection.getSelection()[0];
    node.removeAll();
    node.set('leaf', false);
    node.set('expanded', false);
    node.set('loaded', false);
    node.expand();
  }

});