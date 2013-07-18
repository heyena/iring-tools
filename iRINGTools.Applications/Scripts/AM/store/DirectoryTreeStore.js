/*
 * File: Scripts/AM/store/DirectoryTreeStore.js
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

Ext.define('AM.store.DirectoryTreeStore', {
  extend: 'Ext.data.TreeStore',

  requires: [
    'AM.model.DirectoryModel'
  ],

  constructor: function(cfg) {
    var me = this;
    cfg = cfg || {};
    me.callParent([Ext.apply({
      filterOnLoad: false,
      model: 'AM.model.DirectoryModel',
      storeId: 'directorytreestore',
      root: {
        expanded: true,
        type: 'ScopesNode',
        iconCls: 'scopes',
        text: 'Scopes',
        security: ''
      },
      proxy: {
        type: 'ajax',
        actionMethods: {
          read: 'POST'
        },
        extraParams: {
          id: null,
          type: 'ScopesNode',
          contextName: null,
          endpoint: null,
          baseUrl: null,
          related: null,
          security: null
        },
        timeout: 600000,
        url: 'directory/getnode',
        reader: {
          type: 'json'
        }
      }
    }, cfg)]);
  }
});