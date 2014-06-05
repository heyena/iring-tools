
Ext.define('EM.view.menus.SecurityMenu', {
    extend: 'Ext.menu.Menu',

    initComponent: function() {
        var me = this;

        Ext.applyIf(me, {
            items: [
                {
                    xtype: 'menuitem',
                    action: 'addGroup',
                    icon: 'resources/images/16x16/add.png',
                    text: 'Add Group'
                },
                {
                    xtype: 'menuitem',
                    action: 'editGroup',
                    icon: 'resources/images/16x16/edit-file.png',
                    text: 'Edit Group'
                },
                {
                    xtype: 'menuitem',
                    action: 'deleteGroup',
                    icon: 'resources/images/16x16/delete-icon.png',
                    text: 'Delete Group'
                },
                {
                    xtype: 'menuitem',
                    action: 'addUserToGroup',
                    icon: 'resources/images/16x16/add.png',
                    text: 'Add User to Group'
                },
                {
                    xtype: 'menuitem',
                    action: 'editUserGroup',
                    icon: 'resources/images/16x16/edit-file.png',
                    text: 'Edit User/Group'
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