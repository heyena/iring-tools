
Ext.define('USM.view.menus.RoleMenu', {
    extend: 'Ext.menu.Menu',
    alias: 'widget.rolemenu',
    initComponent: function() {
        var me = this;

        Ext.applyIf(me, {
            items: [
                {
                    xtype: 'menuitem',
                    action: 'addRole',
                    iconCls: 'icon-add',
                    text: 'Add Role'
                },
                {
                    xtype: 'menuitem',
                    action: 'editRole',
                    iconCls: 'icon-edit',
                    text: 'Edit Role'
                },
                {
                    xtype: 'menuitem',
                    action: 'deleteRole',
                    iconCls: 'icon-delete',
                    text: 'Delete Role'
                },
                {
                    xtype: 'menuseparator'
                },
                {
                    xtype: 'menuitem',
                    action: 'addPermissionToRole',
                    iconCls: 'icon-add',
                    text: 'Add/Remove Permission to Role'
                },
                {
                    xtype: 'menuitem',
                    action: 'editPermissionRole',
                    iconCls: 'icon-edit',
                    hidden : true,
                    text: 'Edit Permission/Role'
                },
                {
                    xtype: 'menuseparator'
                },
                {
                    xtype: 'menuitem',
                    action: 'addRemGroupToRole',
                    iconCls: 'icon-add',
                    text: 'Add/Remove Group to Role'
                }
            ]
        });

        me.callParent(arguments);
    }

});