

Ext.define('AM.store.RelationalStoreforTransferType', {
    extend: 'Ext.data.Store',
    alias: 'store.relationalStoreforTransferType',

    constructor: function(cfg) {
    	var me = this;
        cfg = cfg || {};
        me.callParent([Ext.apply({
            autoLoad: false,
            storeId: 'RelationalStoreforTransferType',
            data: [
                {
                    name: 0,
                    value: 'EqualTo'
                },
                {
                    name: 1,
                    value: 'NotEqualTo'
                }
            ],
            fields: [
                {
                    name: 'name'
                },
                {
                    name: 'value'
                }
            ]
        }, cfg)]);
    }
});