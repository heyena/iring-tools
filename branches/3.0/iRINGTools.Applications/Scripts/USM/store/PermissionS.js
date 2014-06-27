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
            autoLoad: false,
            model: 'USM.model.PermissionM',
            storeId: 'PermissionJsonStore',
            proxy: {
                type: 'ajax',
                //url: '/Scripts/USM/jsonfiles/permissions.json',
                url : 'usersecuritymanager/getPermissions',
                reader: {
                    type: 'json',
                    root: 'items'
                }
            }
        }, cfg)]);
    }
});