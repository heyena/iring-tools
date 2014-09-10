﻿Ext.define('USM.store.SecurityS', {
    extend: 'Ext.data.Store',

    requires: [
        'USM.model.SecurityM'
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
            model: 'USM.model.SecurityM',
            storeId: storeId,
			data: [{"name": "Groups"},{"name": "Users"},{"name": "Roles"},{"name": "Permissions"}] ,
            proxy: {
                type: 'ajax',
                //url: '/Scripts/USM/jsonfiles/security.json',
                reader: {
                    type: 'json'
                }
            }
        }, cfg)]);
    }
});