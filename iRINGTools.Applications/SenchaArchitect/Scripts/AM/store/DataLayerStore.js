/*
 * File: Scripts/AM/store/DataLayerStore.js
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

Ext.define('AM.store.DataLayerStore', {
  extend: 'Ext.data.Store',

  requires: [
    'AM.model.DataLayerModel'
  ],

  constructor: function(cfg) {
    var me = this;
    cfg = cfg || {};
    me.callParent([Ext.apply({
      autoLoad: false,
      model: 'AM.model.DataLayerModel',
      storeId: 'dataLayerStore',
      proxy: {
        type: 'ajax',
        extraParams: {
          baseUrl: null
        },
        timeout: 600000,
        url: 'directory/datalayers',
        reader: {
          type: 'json',
          root: 'items'
        }
      }
    }, cfg)]);
  }
});