
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
                    action: 'editUserGroup',
                    iconCls: 'icon-add',
                    text: 'Add/Remove User to Group'
                },
                {
                    xtype: 'menuitem',
                    action: 'editUserGroup1',
                    iconCls: 'icon-edit',
                    hidden: true,
                    text: 'Edit User/Group'
                },
                {
                    xtype: 'menuseparator'
                },
                {
                    xtype: 'menuitem',
                    action: 'addRemRoletoGroup',
                    iconCls: 'icon-add',
                    text: 'Add/Remove Role to Group'
                }
            ]
        });

        me.callParent(arguments);
    }

});