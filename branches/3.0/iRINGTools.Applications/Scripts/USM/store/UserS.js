Ext.define('USM.store.UserS', {
    extend: 'Ext.data.Store',

    requires: [
        'USM.model.UserM'
    ],

    constructor: function (cfg) {
        var me = this;
        cfg = cfg || {};
        var storeId = Ext.data.IdGenerator.get("uuid").generate();
        me.callParent([Ext.apply({
            actionMethod: {
                read: 'GET'
            },
            autoLoad: false,
            model: 'USM.model.UserM',
            storeId: storeId,
            //data: [{ "UserName": "prashant", "UserFirstName": "Prashant", "UserLastName": "Dubey", "UserEmail": "pdubey@bechtel.com", "UserPhone": "8992", "UserDesc": "pdubey@bechtel.com"}] 
            proxy: {
                type: 'ajax',
                //url: '/Scripts/USM/jsonfiles/users.json',
                url : 'usersecuritymanager/getUsers',
                reader: {
                    type: 'json',
                    root: 'items'
                }
            }
        }, cfg)]);
    }
});