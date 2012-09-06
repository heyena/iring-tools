/*
 * File: Scripts/AM/view/spreadsheet/SpreadsheetTree.js
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

Ext.define('AM.view.spreadsheet.SpreadsheetTree', {
  extend: 'Ext.tree.Panel',
  alias: 'widget.spreadsheettree',

  requires: [
    'AM.view.override.spreadsheet.SpreadsheetTree'
  ],

  region: 'center',
  layout: {
    type: 'fit'
  },
  context: '',
  endpoint: '',
  baseUrl: '',
  datalayer: '',
  border: 'true',
  store: 'SpreadsheetStore',

  initComponent: function() {
    var me = this;

    me.addEvents(
      'reload'
    );

    Ext.applyIf(me, {
      viewConfig: {

      },
      dockedItems: [
        {
          xtype: 'toolbar',
          dock: 'top',
          items: [
            {
              xtype: 'button',
              action: 'reloadspreadsheet',
              iconCls: 'am-view-refresh',
              text: 'Reload'
            },
            {
              xtype: 'button',
              action: 'savespreadsheet',
              iconCls: 'am-document-save',
              text: 'Save'
            },
            {
              xtype: 'button',
              action: 'openuploadform',
              iconCls: 'am-document-up',
              text: 'Upload'
            },
            {
              xtype: 'button',
              action: 'downloadspreadsheet',
              iconCls: 'am-document-down',
              text: 'Download'
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
    var node;
    if (selection.getSelection().length > 0) {
      node = selection.getSelection()[0];
    } else {
      node = me.getRootNode();
    }
    node.removeAll();
    node.set('leaf', false);
    node.set('expanded', false);
    node.set('loaded', false);
    node.expand();
  }

});