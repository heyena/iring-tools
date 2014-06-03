
Ext.define('USM.view.menu.PermissionMenu', {
    extend: 'Ext.menu.Menu',

    initComponent: function() {
        var me = this;

        Ext.applyIf(me, {
            items: [
                {
                    xtype: 'menuitem',
                    action: 'addPermission',
                    icon: 'resources/images/16x16/add.png',
                    text: 'Add Permission'
                },
                {
                    xtype: 'menuitem',
                    action: 'editPermission',
                    icon: 'resources/images/16x16/edit-file.png',
                    text: 'Edit Permission'
                },
                {
                    xtype: 'menuitem',
                    action: 'addPermissionToRole',
                    icon: 'resources/images/16x16/add.png',
                    text: 'Add Permission to Role'
                },
                {
                    xtype: 'menuitem',
                    action: 'editPermissionRole',
                    icon: 'resources/images/16x16/edit-file.png',
                    text: 'Edit Role/Permission'
                }
            ]
        });

        me.callParent(arguments);
    }

});