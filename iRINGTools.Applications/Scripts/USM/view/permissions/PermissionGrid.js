
Ext.define('USM.view.permissions.PermissionGrid', {
    extend: 'Ext.grid.Panel',
    alias: 'widget.permissiongrid',
    resizable: true,
    store: 'PermissionS',
    resizable: false,
    id: 'idpermission',
    initComponent: function () {
        var me = this;

        Ext.applyIf(me, {
            columns: [
                {
                    xtype: 'gridcolumn',
                    dataIndex: 'PermissionName',
                    text: 'Permission',
                    flex: 2,
                    menuDisabled: true
                }, {
                    xtype: 'gridcolumn',
                    dataIndex: 'PermissionDesc',
                    text: 'Description',
                    flex: 2,
                    menuDisabled: true
                }
            ]
        });

        me.callParent(arguments);
    }

});