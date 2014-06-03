
Ext.define('USM.view.roles.RoleGrid', {
    extend: 'Ext.grid.Panel',
    alias: 'widget.rolegrid',
    resizable: true,
    store: 'RoleS',

    initComponent: function () {
        var me = this;

        Ext.applyIf(me, {
            columns: [
                {
                    xtype: 'gridcolumn',
                    dataIndex: 'rolename',
                    text: 'Role',
                    flex: 1,
                    menuDisabled: true
                }, {
                    xtype: 'gridcolumn',
                    dataIndex: 'description',
                    text: 'Description',
                    flex: 2,
                    menuDisabled: true
                }
            ]
        });

        me.callParent(arguments);
    }

});