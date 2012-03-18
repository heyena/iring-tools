Ext.ns('Ext.ux');
Ext.define('Ext.ux.plugin.GridPageSizer', {
  extend: 'Object',
  alias: 'plugin.PagingToolbarResizer',

  options: [5, 10, 15, 20, 25, 30, 50, 75, 100, 200, 300, 500, 1000],

  mode: 'remote',
  displayText: 'Records per Page',
  prependCombo: false,

  constructor: function (config) {
    Ext.apply(this, config);
    this.callParent();
  },

  init: function (pagingToolbar) {
    var comboStore = this.options;
    var combo = new Ext.form.field.ComboBox({
      typeAhead: false,
      triggerAction: 'all',
      forceSelection: true,
      selectOnFocus: true,
      editable: true,
      mode: this.mode,
      value: pagingToolbar.pageSize,
      width: 50,
      store: comboStore
    });

    combo.on('select', this.onPageSizeChanged, pagingToolbar);
    var index = 0;
    if (this.prependCombo) {
      index = pagingToolbar.items.indexOf(pagingToolbar.first);
      index--;
    } else {
      index = pagingToolbar.items.indexOf(pagingToolbar.refresh);
      pagingToolbar.insert(++index, '-');
    }
    pagingToolbar.insert(++index, this.displayText);
    pagingToolbar.insert(++index, combo);

    if (this.prependCombo) {
      pagingToolbar.insert(++index, '-');
    }
    pagingToolbar.on({
      beforedestroy: function () {
        combo.destroy();
      }
    });
  },
  onPageSizeChanged: function (combo) {
    this.store.pageSize = parseInt(combo.getRawValue(), 10);
    this.doRefresh();
  }
});

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
