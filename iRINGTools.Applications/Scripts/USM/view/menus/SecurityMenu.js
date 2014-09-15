
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
                    hidden: false,
                    iconCls: 'icon-add',
                    text: 'Add Group'
                },
                {
                    xtype: 'menuitem',
                    action: 'addUserToGroup',
                    hidden: false,
                    iconCls: 'icon-edit',
                    text: 'Add/Remove Users to Group'
                },
                {
                    xtype: 'menuitem',
                    action: 'addRemRolestoGroup',
                    hidden: false,
                    iconCls: 'icon-edit',
                    text: 'Add/Remove Roles to Group'
                }
            ]
        });

        me.callParent(arguments);
    }

});