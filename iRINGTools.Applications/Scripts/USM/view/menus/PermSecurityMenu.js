
Ext.define('USM.view.menus.PermSecurityMenu', {
    extend: 'Ext.menu.Menu',
    alias: 'widget.permsecuritymenu',
    initComponent: function () {
        var me = this;

        Ext.applyIf(me, {
            items: [
                {
                    xtype: 'menuitem',
                    action: 'addPermission',
                    hidden: false,
                    iconCls: 'icon-add',
                    text: 'Add Permission'
                }
            ]
        });

        me.callParent(arguments);
    }

});