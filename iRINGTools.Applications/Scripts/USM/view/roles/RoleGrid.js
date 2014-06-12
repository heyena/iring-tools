
Ext.define('USM.view.roles.RoleGrid', {
    extend: 'Ext.grid.Panel',
    alias: 'widget.rolegrid',
    resizable: true,
    store: 'RoleS',
    resizable: false,
    initComponent: function () {
        var me = this;

        Ext.applyIf(me, {
            columns: [
                {
                    xtype: 'gridcolumn',
                    dataIndex: 'RoleName',
                    text: 'Role',
                    flex: 1,
                    menuDisabled: true
                }, {
                    xtype: 'gridcolumn',
                    dataIndex: 'RoleDesc',
                    text: 'Description',
                    flex: 2,
                    menuDisabled: true
                }
            ]
        });

        me.callParent(arguments);
    }

});