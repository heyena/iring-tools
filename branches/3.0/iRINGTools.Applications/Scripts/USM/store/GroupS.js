Ext.define('USM.store.GroupS', {
    extend: 'Ext.data.Store',

    requires: [
        'USM.model.GroupM'
    ],

    constructor: function (cfg) {
        var me = this;
        cfg = cfg || {};
        me.callParent([Ext.apply({
            actionMethod: {
                read: 'GET'
            },
            autoLoad: true,
            model: 'USM.model.GroupM',
            storeId: 'GroupJsonStore',
            proxy: {
                type: 'ajax',
                //url: '/Scripts/USM/jsonfiles/groups.json',
                url : 'usersecuritymanager/getGroups',
                reader: {
                    type: 'json',
                    root: 'items'
                }
            }
        }, cfg)]);
    }
});