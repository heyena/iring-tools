/*
 * File: Scripts/AM/view/search/SearchTree.js
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

Ext.define('AM.view.search.SearchTree', {
  extend: 'Ext.tree.Panel',
  alias: 'widget.searchtree',

  requires: [
    'AM.view.override.search.SearchTree'
  ],

  region: 'center',
  border: false,
  style: 'padding-left:5px;',
  closable: true,
  store: 'SearchStore',
  rootVisible: false,

  initComponent: function() {
    var me = this;

    Ext.applyIf(me, {
      viewConfig: {
        plugins: [
          Ext.create('Ext.tree.plugin.TreeViewDragDrop', {
            ptype: 'treeviewdragdrop',
            dragField: 'text',
            appendOnly: false,
            ddGroup: 'refdataGroup',
            dragText: '{0}',
            enableDrop: false
          })
        ]
      }
    });

    me.callParent(arguments);
  },

  getSelectedNode: function() {
    var me = this;
    var selected = me.getSelectionModel().getSelection();
    return selected[0];
  }

});