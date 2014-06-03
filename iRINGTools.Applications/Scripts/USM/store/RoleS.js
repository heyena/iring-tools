Ext.define('USM.store.RoleS', {
    extend: 'Ext.data.Store',

    requires: [
        'USM.model.RoleM'
    ],

    constructor: function (cfg) {
        var me = this;
        cfg = cfg || {};
        me.callParent([Ext.apply({
            actionMethod: {
                read: 'GET'
            },
            autoLoad: true,
            model: 'USM.model.RoleM',
            storeId: 'RoleJsonStore',
            data: [{ "rolename": "T Role", "description": "iRing"}]
//            proxy: {
//                type: 'ajax',
//                url: '/Scripts/USM/jsonfiles/roles.json',
//                reader: {
//                    type: 'json',
//                    root: 'items'
//                }
//            }
        }, cfg)]);
    }
});