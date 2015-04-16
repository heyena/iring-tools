
Ext.define('AM.store.ColumnNameStore', {
    extend: 'Ext.data.Store',
    alias: 'store.columnNameStore',

    requires: [
        'AM.model.Names'
    ],

    constructor: function(cfg) {
    	var me = this;
        cfg = cfg || {};
        me.callParent([Ext.apply({
            model: 'AM.model.Names',
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