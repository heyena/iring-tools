
Ext.define('USM.view.permissions.PermissionGrid', {
    extend: 'Ext.grid.Panel',
    alias: 'widget.permissiongrid',
    resizable: true,
    store: 'PermissionS',

    initComponent: function () {
        var me = this;

        Ext.applyIf(me, {
            columns: [
                {
                    xtype: 'gridcolumn',
                    dataIndex: 'permissionname',
                    text: 'Permission',
                    flex: 2,
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