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
            data: [
				{ "PermissionName": "Full Control", "PermissionDesc": "iRing"},
				{ "PermissionName": "Modify", "PermissionDesc": "iRing"},
				{ "PermissionName": "Read", "PermissionDesc": "iRing"},
				{ "PermissionName": "Write", "PermissionDesc": "iRing"},
				{ "PermissionName": "Execute", "PermissionDesc": "iRing"},
				{ "PermissionName": "Read & Execute", "PermissionDesc": "iRing"},
				{ "PermissionName": "Read,write & execute", "PermissionDesc": "iRing"}
			]
            /*proxy: {
                type: 'ajax',
                url: '/Scripts/USM/jsonfiles/permissions.json',
                reader: {
                    type: 'json',
                    root: 'items'
                }
            }*/
        }, cfg)]);
    }
});