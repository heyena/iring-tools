
Ext.define('USM.view.menus.GroupMenu', {
    extend: 'Ext.menu.Menu',
    alias: 'widget.groupmenu',
    initComponent: function() {
        var me = this;

        Ext.applyIf(me, {
            items: [
                {
                    xtype: 'menuitem',
                    action: 'addGroup',
                    iconCls: 'icon-add',
                    text: 'Add Group'
                },
                {
                    xtype: 'menuitem',
                    action: 'editGroup',
                    iconCls: 'icon-edit',
                    text: 'Edit Group'
                },
                {
                    xtype: 'menuitem',
                    action: 'deleteGroup',
                    iconCls: 'icon-delete',
                    text: 'Delete Group'
                },
                {
                    xtype: 'menuseparator'
                },
                {
                    xtype: 'menuitem',
                    action: 'addUserToGroup',
                    hidden : true,
                    icon: 'resources/images/16x16/add.png',
                    text: 'Add User to Group'
                },
                {
                    xtype: 'menuitem',
                    action: 'editUserGroup',
                    hidden: true,
                    icon: 'resources/images/16x16/edit-file.png',
                    text: 'Edit User/Group'
                },
                {
                    xtype: 'menuseparator'
                },
                {
                    xtype: 'menuitem',
                    action: 'addRoletoGroup',
                    hidden: true,
                    icon: 'resources/images/16x16/add.png',
                    text: 'Add Role to Group'
                },
                {
                    xtype: 'menuitem',
                    action: 'editRoleGroup',
                    hidden: true,
                    icon: 'resources/images/16x16/edit-file.png',
                    text: 'Edit Role/Group'
                }
            ]
        });

        me.callParent(arguments);
    }

});