
Ext.define('USM.view.menus.RoleMenu', {
    extend: 'Ext.menu.Menu',

    initComponent: function() {
        var me = this;

        Ext.applyIf(me, {
            items: [
                {
                    xtype: 'menuitem',
                    action: 'addRole',
                    icon: 'resources/images/16x16/add.png',
                    text: 'Add Role'
                },
                {
                    xtype: 'menuitem',
                    action: 'editRole',
                    icon: 'resources/images/16x16/edit-file.png',
                    text: 'Edit Role'
                },
                {
                    xtype: 'menuitem',
                    action: 'deleteRole',
                    icon: 'resources/images/16x16/delete-icon.png',
                    text: 'Delete Role'
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