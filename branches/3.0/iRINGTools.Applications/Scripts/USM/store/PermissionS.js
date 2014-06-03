Ext.define('USM.store.PermissionS', {
    extend: 'Ext.data.Store',

    requires: [
        'USM.model.PermissionM'
    ],

    constructor: function (cfg) {
        var me = this;
        cfg = cfg || {};
        me.callParent([Ext.apply({
            actionMethod: {
                read: 'GET'
            },
            autoLoad: true,
            model: 'USM.model.PermissionM',
            storeId: 'PermissionJsonStore',
            data: [{ "permissionname": "Tst Permission", "description": "iRing"}]
//            proxy: {
//                type: 'ajax',
//                url: '/Scripts/USM/jsonfiles/permissions.json',
//                reader: {
//                    type: 'json',
//                    root: 'items'
//                }
//            }
        }, cfg)]);
    }
});