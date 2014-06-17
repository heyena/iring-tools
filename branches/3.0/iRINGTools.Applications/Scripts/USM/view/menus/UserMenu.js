
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
					iconCls: 'icon-add',
                    text: 'Add User'
                },
                {
                    xtype: 'menuitem',
                    action: 'addEditUser',
					itemId:'editUser',
					iconCls: 'icon-edit',
                    text: 'Edit User'
                },
                {
                    xtype: 'menuitem',
                    action: 'deleteUser',
                    iconCls: 'icon-delete',
                    text: 'Delete User'
                },
                {
                    xtype: 'menuitem',
                    action: 'addGroupToUser',
                    iconCls: 'icon-add',
                    text: 'Add Group to User'
                },
                {
                    xtype: 'menuitem',
                    action: 'editGroupUser',
                    iconCls: 'icon-edit',
                    text: 'Edit Group/User'
                }
            ]
        });

        me.callParent(arguments);
    }

});