Ext.define('USM.view.menus.UserMenu', {
    extend: 'Ext.menu.Menu',
	alias: 'widget.usermenu',
    initComponent: function() {
        var me = this;

        Ext.applyIf(me, {
            items: [
                {
                    xtype: 'menuitem',
                    action: 'addUser',
					itemId:'addUser',
					iconCls: 'icon-add',
                    text: 'Add User'
                },
                {
                    xtype: 'menuitem',
                    action: 'editUser',
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
                    action: 'editGroupUser',
                    iconCls: 'icon-add',
                    text: 'Add/Remove Group to User'
                },
                {
                    xtype: 'menuitem',
                    action: 'editGroupUser1',
                    iconCls: 'icon-edit',
                    hidden: true,
                    text: 'Edit Group/User'
                }
            ]
        });

        me.callParent(arguments);
    }

});