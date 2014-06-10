
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
                    icon: 'resources/images/16x16/add.png',
                    text: 'Add Permission to Role'
                },
                {
                    xtype: 'menuitem',
                    action: 'editPermissionRole',
                    icon: 'resources/images/16x16/edit-file.png',
                    text: 'Edit Permission/Role'
                },
                {
                    xtype: 'menuseparator'
                },
                {
                    xtype: 'menuitem',
                    action: 'addRoletoGroup',
                    icon: 'resources/images/16x16/add.png',
                    text: 'Add Role to Group'
                },
                {
                    xtype: 'menuitem',
                    action: 'editRoleGroup',
                    icon: 'resources/images/16x16/edit-file.png',
                    text: 'Edit Role/Group'
                }
            ]
        });

        me.callParent(arguments);
    }

});