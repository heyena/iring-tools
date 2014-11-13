
Ext.define('USM.view.menus.UserSecurityMenu', {
    extend: 'Ext.menu.Menu',
    alias: 'widget.usersecuritymenu',
    initComponent: function () {
        var me = this;

        Ext.applyIf(me, {
            items: [
                {
                    xtype: 'menuitem',
                    action: 'addUser',
                    hidden: false,
                    iconCls: 'icon-add',
                    text: 'Add User'
                }
            ]
        });

        me.callParent(arguments);
    }

});