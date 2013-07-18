Ext.Loader.setConfig({ enabled: true });
Ext.Loader.setPath('Ext.ux', 'Scripts/extjs407/examples/ux');
Ext.require([
    'Ext.form.field.Text',
    'Ext.toolbar.TextItem',
    'Ext.data.*',
    'Ext.grid.*',
    'Ext.ux.grid.FiltersFeature'    
]);


Ext.define('AM.view.directory.DataGridPanel', {
  extend: 'Ext.grid.Panel',
  alias: 'widget.datagridpanel',
  closable: true,
  context: null,
  endpoint: null,
  baseUrl: null,
  graph: null,
  url: null,
  reload: null,
  store: null,
  initComponent: function () {
    var me = this;
    this.store = Ext.create('Ext.data.Store', {
      model: 'AM.model.DynamicModel',
      autoLoad: false,
      autoDestroy: true,
      pageSize: 25,
      remoteSort: true,
      storeId: me.context + '.' + me.endpoint + '.' + me.graph + 'gridstore'
    });

    var filters = {
      ftype: 'filters',
      local: false,
      buildQuery: function (filters) {
        var processed_filters = []

        for (var i = 0; i < filters.length; i++) {
          var pf = {}
          var filter = filters[i];
          pf['field'] = filter.field;

          if (filter.data.type == 'numeric') {
            pf['comparison'] = filter.data['comparison'];
            pf['value'] = filter.data['value'];
            pf['type'] = filter.data['type'];
          }
          else {
            for (var key in filter.data) {
              pf[key] = filter.data[key];
            }
            pf['comparison'] = 'eq';
          }

          processed_filters.push(pf);

        }

        return { filter: Ext.encode(processed_filters) };        
      }
    };

    var ptb = Ext.create('Ext.PagingToolbar', {
      pageSize: 25,
      store: me.store,
      displayInfo: true,
      displayMsg: 'Records {0} - {1} of {2}',
      emptyMsg: "No records to display",
      plugins: [Ext.create('Ext.ux.plugin.GridPageSizer', { options: [25, 50, 100, 200] })]
    });

    Ext.apply(this, {
      iconCls: 'tabsData',
      bbar: ptb,
      columns: {
        defaults: {
          field: { xtype: 'textfield' }
        }
      },
      features: [filters]      
    }),
		this.callParent(arguments);
  }
});
