
Ext.define('USM.view.menus.SecurityMenu', {
    extend: 'Ext.menu.Menu',
    alias: 'widget.securitymenu',
    initComponent: function() {
        var me = this;

        Ext.applyIf(me, {
            items: [
                {
                    xtype: 'menuitem',
                    action: 'addGroup',
                    hidden: true,
                    iconCls: 'icon-add',
                    text: 'Add Group'
                },
                {
                    xtype: 'menuitem',
                    action: 'editSecUserGroup',
                    hidden: true,
                    iconCls: 'icon-edit',
                    text: 'Edit User/Group'
                },
                {
                    xtype: 'menuitem',
                    action: 'editSecGroupRoles',
                    hidden: true,
                    iconCls: 'icon-edit',
                    text: 'Edit Group/Roles'
                }
            ]
        });

        me.callParent(arguments);
    }

});