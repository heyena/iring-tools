

Ext.define('df.store.RelationalStoreComplete', {
    extend: 'Ext.data.Store',
    alias: 'store.relationalCompleteStore',

    constructor: function(cfg) {
    	var me = this;
        cfg = cfg || {};
        me.callParent([Ext.apply({
            autoLoad: false,
            storeId: 'RelationalStoreComplete',
            data: [
                {
                    name: '0',
                    value: 'EqualTo'
                },
                {
                    name: '1',
                    value: 'NotEqualTo'
                },
                {
                    name: '2',
                    value: 'LessThan'
                },
                {
                    name: '3',
                    value: 'LessThan'
                },
                {
                    name: '4',
                    value: 'Contains'
                },
                {
                    name: '5',
                    value: 'StartsWith'
                },
                {
                    name: '6',
                    value: 'EndsWith'
                },
                {
                    name: '7',
                    value: 'GreaterThanOrEqual'
                },
                {
                    name: '8',
                    value: 'LesserThanOrEqual'
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