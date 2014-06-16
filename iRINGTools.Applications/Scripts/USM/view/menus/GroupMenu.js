
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
                    iconCls: 'icon-add',
                    text: 'Add User to Group'
                },
                {
                    xtype: 'menuitem',
                    action: 'editUserGroup',
                    iconCls: 'icon-edit',
                    text: 'Edit User/Group'
                },
                {
                    xtype: 'menuseparator'
                },
                {
                    xtype: 'menuitem',
                    action: 'addRoletoGroup',
                    iconCls: 'icon-add',
                    text: 'Add Role to Group'
                },
                {
                    xtype: 'menuitem',
                    action: 'editRoleGroup',
                    iconCls: 'icon-edit',
                    text: 'Edit Role/Group'
                }
            ]
        });

        me.callParent(arguments);
    }

});