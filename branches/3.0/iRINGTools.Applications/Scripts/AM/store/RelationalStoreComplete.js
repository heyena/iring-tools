

Ext.define('AM.store.RelationalStoreComplete', {
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
                    name: 0,
                    value: 'EqualTo'
                },
                {
                    name: 1,
                    value: 'NotEqualTo'
                },
                {
                    name: 2,
                    value: 'StartsWith'
                },
                {
                    name: 3,
                    value: 'EndsWith'
                },
                {
                    name: 4,
                    value: 'Contains'
                },
                {
                    name: 5,
                    value: 'In'
                },
                {
                    name: 6,
                    value: 'GreaterThan'
                },
                {
                    name: 7,
                    value: 'GreaterThanOrEqual'
                },
                {
                    name: 8,
                    value: 'LesserThan'
                },
                {
                    name: 9,
                    value: 'LesserThanOrEqual'
                },
				{
                    name: 10,
                    value: 'IsNull'
                },
                {
                    name: 11,
                    value: 'IsNotNull'
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