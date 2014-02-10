
Ext.define('df.store.ColumnNameStore', {
    extend: 'Ext.data.Store',
    alias: 'store.columnNameStore',

    requires: [
        'df.model.Names'
    ],

    constructor: function(cfg) {
    	var me = this;
        cfg = cfg || {};
        me.callParent([Ext.apply({
            model: 'df.model.Names',
            storeId: 'ColumnNameStore',
            proxy: {
                type: 'ajax',
                reader: {
                    type: 'json'
                }
            }
        }, cfg)]);
    }
});