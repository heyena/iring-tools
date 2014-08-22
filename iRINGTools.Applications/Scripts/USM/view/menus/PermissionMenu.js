
Ext.define('USM.view.menus.PermissionMenu', {
    extend: 'Ext.menu.Menu',
    alias: 'widget.permissionmenu',

    initComponent: function() {
        var me = this;

        Ext.applyIf(me, {
            items: [
                {
                    xtype: 'menuitem',
                    action: 'addPermission',
                    iconCls: 'icon-add',
                    text: 'Add Permission'
                },
                {
                    xtype: 'menuitem',
                    action: 'editPermission',
                    iconCls: 'icon-edit',
                    text: 'Edit Permission'
                },
                {
                    xtype: 'menuitem',
                    action: 'deletePermission',
                    iconCls: 'icon-delete',
                    text: 'Delete Permission'
                },
                {
                    xtype: 'menuseparator',
                    hidden: true
                },
                {
                    xtype: 'menuitem',
                    action: 'addPermissionToRole',
                    iconCls: 'icon-add',
                    text: 'Add Permission to Role',
                    hidden : true
                },
                {
                    xtype: 'menuitem',
                    action: 'editPermissionRole',
                    iconCls: 'icon-edit',
                    text: 'Edit Role/Permission',
                    hidden: true
                }
            ]
        });

        me.callParent(arguments);
    }

});