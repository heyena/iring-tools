Ext.define('USM.store.GroupS', {
    extend: 'Ext.data.Store',

    requires: [
        'USM.model.GroupM'
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
            model: 'USM.model.GroupM',
            storeId: storeId,
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