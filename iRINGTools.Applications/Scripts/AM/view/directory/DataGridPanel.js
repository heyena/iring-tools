Ext.define('AM.view.directory.DataGridPanel', {
  extend: 'Ext.grid.Panel',
  alias: 'widget.datagridpanel',
  requires: [
    'Ext.form.field.Text',
    'Ext.toolbar.TextItem',
    'Ext.data.*'
  ],
  closable: true,
  context: null,
  endpoint: null,
  graph: null,
  url: null,
  reload: null,
  store: null,
  initComponent: function () {
    var me = this;
    this.store = Ext.create('Ext.data.Store', {
      model: 'AM.model.DynamicModel',
      autoLoad: false,
      pageSize: 25,
      storeId: me.context + '.' + me.endpoint + '.' + me.graph + 'gridstore'
    });

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
      }
    }),
		this.callParent(arguments);
  }
});
