Ext.define('AM.store.DBProviderStore', {
    extend: 'Ext.data.Store',

    constructor: function (cfg) {
        var me = this;
        cfg = cfg || {};
        me.callParent([Ext.apply({
            autoLoad: true,
            storeId: 'DBProviderStore',
            proxy: {
                type: 'ajax',
                url: 'NHibernate/DBProviders'
            },
            fields: ['Name']
        }, cfg)]);
    }
});