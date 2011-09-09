Ext.define('AdapterManager.DatagridPanel', {
  extend: 'Ext.grid.Panel',
  alias: 'widget.datagridpanel',
  requires: [
        'Ext.form.field.Text',
        'Ext.toolbar.TextItem',
        'Ext.data.*'
  ],
  closable: true,
  scope: null,
  app: null,
  graph: null,
  url: null,
  page: null,
  limit: null,
  reload: null,

  initComponent: function () {

    var storeProxy = Ext.create('Ext.data.proxy.Ajax', {
      actionMethods: { read: 'POST' },
      url: this.url,
      extraParams: { scope: this.scope, app: this.app, graph: this.graph },
      reader: { totalProperty: 'totalCount' }
    });

    if (!Ext.ModelManager.isRegistered('Dynamic.Model')) {
      Ext.define('Dynamic.Model', {
        extend: 'Ext.data.Model',
        fields: []
      });
    }

    var grid = this;

    this.store = Ext.create('Ext.data.JsonStore', {
      autoLoad: false,
      pageSize: 25,
      model: 'Dynamic.Model',
      proxy: storeProxy
    });

    Ext.apply(this, {
      iconCls: 'tabsMapping',
      itemId: 'tablegrid_' + this.scope + '.' + this.app + '.' + this.graph,
      store: this.store,
      dockedItems: [ {
         xtype: 'pagingtoolbar',
         store: this.store,
         dock: 'bottom',
         displayInfo: true,
         plugins: Ext.create('Ext.ux.grid.PageSize', {})
      } ],
      columns: {
        defaults: {
          field: { xtype: 'textfield' }
        }
      }
    }),

		this.callParent(arguments);

    this.store.load({
      callback: function (recs, options, success) {
        grid.reconfigure(this.store, recs[0].store.proxy.reader.fields);
        grid.show();
      }
    });
  },

  onSync: function () {
    this.store.sync();
  }
});
