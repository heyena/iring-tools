
Ext.define('USM.view.menus.UserMenu', {
    extend: 'Ext.menu.Menu',
	alias: 'widget.usermenu',
    initComponent: function() {
        var me = this;

        Ext.applyIf(me, {
            items: [
                {
                    xtype: 'menuitem',
                    action: 'addEditUser',
					itemId:'addUser',
                    icon: 'Content/img/16x16/add.png',
                    text: 'Add User'
                },
                {
                    xtype: 'menuitem',
                    action: 'addEditUser',
					itemId:'editUser',
                    icon: 'Content/img/16x16/edit.png',
                    text: 'Edit User'
                },
                {
                    xtype: 'menuitem',
                    action: 'deleteUser',
                    icon: 'Content/img/16x16/delete-icon.png',
                    text: 'Delete User'
                },
                {
                    xtype: 'menuitem',
                    action: 'addUserToGroup',
                    icon: 'Content/img/16x16/add.png',
                    text: 'Add User to Group'
                },
                {
                    xtype: 'menuitem',
                    action: 'editUserGroup',
                    icon: 'Content/img/16x16/edit.png',
                    text: 'Edit User/Group'
                }
            ]
        });

        me.callParent(arguments);
    }

});