﻿Ext.define('USM.store.UserS', {
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
            data: [{ "UserName": "prashant", "UserFirstName": "Prashant", "UserLastName": "Dubey", "UserEmail": "pdubey@bechtel.com", "UserPhone": "8992", "UserDesc": "pdubey@bechtel.com"}] 
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