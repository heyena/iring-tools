
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
                }
            ]
        });

        me.callParent(arguments);
    }

});