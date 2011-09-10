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

        var ptb = Ext.create('Ext.PagingToolbar', {
            pageSize: 25,
            store: this.store,
            displayInfo: true,
            displayMsg: 'Records {0} - {1} of {2}',
            emptyMsg: "No topics to display",
            plugins: [Ext.create('Ext.ux.plugin.GridPageSizer', { options: [25, 50, 100, 200] })]
        });

        Ext.apply(this, {
            iconCls: 'tabsData',
            store: this.store,
            bbar: ptb ,
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
    },

    onBeforeload: function (arguments) {
    }
});
