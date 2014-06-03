Ext.define('USM.store.UserS', {
    extend: 'Ext.data.Store',

    requires: [
        'USM.model.UserM'
    ],

    constructor: function (cfg) {
        var me = this;
        cfg = cfg || {};
        me.callParent([Ext.apply({
            actionMethod: {
                read: 'GET'
            },
            autoLoad: true,
            model: 'USM.model.UserM',
            storeId: 'UserJsonStore',
            data: [{ "username": "prashant", "fname": "Prashant", "lname": "Dubey", "email": "pdubey@bechtel.com", "phone": "8992", "description": "pdubey@bechtel.com"}] 
//            proxy: {
//                type: 'ajax',
//                url: '/Scripts/USM/jsonfiles/users.json',
//                reader: {
//                    type: 'json',
//                    root: 'items'
//                }
//            }
        }, cfg)]);
    }
});