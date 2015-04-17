Ext.define('AM.store.JobStore', {
    extend: 'Ext.data.Store',

    requires: [
        'AM.model.JobModel'
    ],

    constructor: function (cfg) {
        var me = this;
        cfg = cfg || {};
        var storeId = Ext.data.IdGenerator.get("uuid").generate();
        me.callParent([Ext.apply({
            actionMethod: {
                read: 'GET'
            },
            autoLoad: true,
            model: 'AM.model.JobModel',
            storeId: storeId,
            proxy: {
                type: 'ajax',
                url: 'directory/getAllJob',
             
                reader: {
                    type: 'json',
                    root: 'items'
                }
            }
        }, cfg)]);
    }
})