Ext.define('USM.view.menus.RoleSecurityMenu', {
    extend: 'Ext.menu.Menu',
    alias: 'widget.rolesecuritymenu',
    initComponent: function () {
        var me = this;

        Ext.applyIf(me, {
            items: [
                {
                    xtype: 'menuitem',
                    action: 'addRole',
                    hidden: false,
                    iconCls: 'icon-add',
                    text: 'Add Role'
                }
            ]
        });

        me.callParent(arguments);
    }

});