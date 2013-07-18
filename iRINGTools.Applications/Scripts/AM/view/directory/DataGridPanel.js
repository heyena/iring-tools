/*
 * File: Scripts/AM/view/directory/DataGridPanel.js
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

Ext.define('AM.view.directory.DataGridPanel', {
  extend: 'Ext.grid.Panel',
  alias: 'widget.dynamicgrid',

  requires: [
    'AM.view.override.directory.DataGridPanel',
    'AM.store.DataGridStore',
    'Ext.ux.grid.FiltersFeature'
  ],

  closable: true,
  store: 'DataGridStore',

  initComponent: function() {
    var me = this;

    Ext.applyIf(me, {
      columns: [
        {
          xtype: 'gridcolumn',
          dataIndex: 'string'
        }
      ]
    });

    me.processDataGridPanel(me);
    me.callParent(arguments);
  },

  processDataGridPanel: function(config) {
    //var me = this,
    /* storeId = Ext.data.IdGenerator.get("uuid").generate();

    config.store = Ext.create('AM.store.DataGridStore', {
    storeId: "DataGrid" + storeId,
    listeners: {
    beforeload: {
    fn: me.handleBeforeLoad,
    scope: me
    }
    }
    });*/

    /*config.dockedItems = [
    {
    xtype: 'pagingtoolbar',
    dock: 'bottom',
    displayInfo: true,
    store: config.store,
    plugins: [Ext.create('Ext.ux.plugin.GridPageSizer', { options: [25, 50, 100, 200] })]
    }];*/

    /*var filters = {
    ftype: 'filters',
    local: false,
    buildQuery: function (filters) {
    var processed_filters = [];

    for (var i = 0; i < filters.length; i++) {
    var pf = {};
    var filter = filters[i];
    pf.field = filter.field;

    if (filter.data.type == 'numeric') {
    pf.comparison = filter.data.comparison;
    pf.value = filter.data.value;
    pf.type = filter.data.type;
    }
    else {
    for (var key in filter.data) {
    pf[key] = filter.data[key];
    }
    pf.comparison = 'eq';
    }

    processed_filters.push(pf);

    }
    return { filter: Ext.encode(processed_filters) };
    }
    };*/

    //config.iconCls = 'tabsData';
    //config.features = [filters];
    //return config;
  },

  handleBeforeLoad: function(store, operation, e) {
    var me = this;
    store.on({
      metachange: {
        fn: function(store, meta, e) {
          me.handleMetaChange(me, meta);
        }, 
        scope: me, 
        single: true
      }
    });
  },

  handleMetaChange: function(grid, meta) {
    grid.reconfigure(grid.getStore(), meta.columns);
  }

});