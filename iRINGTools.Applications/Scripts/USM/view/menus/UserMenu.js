
Ext.define('USM.view.menu.UserMenu', {
    extend: 'Ext.menu.Menu',

    initComponent: function() {
        var me = this;

        Ext.applyIf(me, {
            items: [
                {
                    xtype: 'menuitem',
                    action: 'addUser',
                    icon: 'resources/images/16x16/add.png',
                    text: 'Add User'
                },
                {
                    xtype: 'menuitem',
                    action: 'editUser',
                    icon: 'resources/images/16x16/edit-file.png',
                    text: 'Edit User'
                },
                {
                    xtype: 'menuitem',
                    action: 'deleteUser',
                    icon: 'resources/images/16x16/delete-icon.png',
                    text: 'Delete User'
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
                }
            ]
        });

        me.callParent(arguments);
    }

});