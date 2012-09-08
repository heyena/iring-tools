/*
 * File: Scripts/AM/store/NHibernateTreeStore.js
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

Ext.define('AM.store.NHibernateTreeStore', {
  extend: 'Ext.data.TreeStore',

  requires: [
    'AM.model.NHibernateTreeModel'
  ],

  constructor: function(cfg) {
    var me = this;
    cfg = cfg || {};
    me.callParent([Ext.apply({
      autoLoad: false,
      storeId: 'NHibernateTreeStore',
      model: 'AM.model.NHibernateTreeModel',
      root: {
        expanded: true,
        text: 'Data Objects',
        iconCls: 'folder'
      },
      proxy: {
        type: 'ajax',
        actionMethods: {
          read: 'POST'
        },
        extraParams: {
          contextName: null,
          endpoint: null,
          dbProvider: null,
          dbServer: null,
          dbInstance: null,
          dbName: null,
          dbSchema: null,
          dbUserName: null,
          dbPassword: null,
          portNumber: null,
          tableNames: null,
          serName: null,
          baseUrl: null
        },
        timeout: 600000,
        url: 'NHibernate/DBObjects',
        reader: {
          type: 'json'
        }
      }
    }, cfg)]);
  }
});