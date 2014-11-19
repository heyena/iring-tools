Ext.define('USM.store.RoleS', {
    extend: 'Ext.data.Store',

    requires: [
        'USM.model.RoleM'
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
            model: 'USM.model.RoleM',
            storeId: storeId,
            proxy: {
                type: 'ajax',
                //url: '/Scripts/USM/jsonfiles/roles.json',
                url: 'usersecuritymanager/getRoles',
                reader: {
                    type: 'json',
                    root: 'items'
                }
            }
        }, cfg)]);
    }
});