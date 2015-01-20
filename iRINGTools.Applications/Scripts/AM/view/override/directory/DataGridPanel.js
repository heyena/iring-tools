Ext.define('AM.view.override.directory.DataGridPanel', {
    override: 'AM.view.directory.DataGridPanel',

    requires: [
        'Ext.ux.grid.FiltersFeature',
        'Ext.ux.plugin.GridPageSizer'
    ],

    initComponent: function () {
        var me = this;
        var storeId = Ext.data.IdGenerator.get("uuid").generate();

        me.store = Ext.create('AM.store.DataGridStore', {
            storeId: "DataGrid" + storeId
        });

        var ptb = Ext.create('Ext.PagingToolbar', {
            pageSize: 25,
            store: me.store,
            displayInfo: true,
            displayMsg: 'Records {0} - {1} of {2}',
            emptyMsg: "No records to display",
            plugins: [Ext.create('Ext.ux.plugin.GridPageSizer', { options: [25, 50, 100, 200] })]
        });

        ////Removing filter from the header of AM
        //        var filters = {
        //            ftype: 'filters',
        //            local: false,
        //            remoteSort: true,
        //            encode: true
        //        };

        var quickFilter = {
            ftype: 'quickfilter'
        };

        //        Ext.apply(me, {
        //            bbar: ptb,
        //            features: [filters, quickFilter]
        //        });

        Ext.apply(me, {
            bbar: ptb,
            features: [quickFilter]
        });


        me.callOverridden(arguments);
    }
});