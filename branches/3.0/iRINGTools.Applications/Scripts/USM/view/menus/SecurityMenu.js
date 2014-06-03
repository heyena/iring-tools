
Ext.define('EM.view.menu.SecurityMenu', {
    extend: 'Ext.menu.Menu',

    initComponent: function() {
        var me = this;

        Ext.applyIf(me, {
            items: [
                {
                    xtype: 'menuitem',
                    action: 'addGroup',
                    icon: 'resources/images/16x16/add.png',
                    text: 'Add Group'
                },
                {
                    xtype: 'menuitem',
                    action: 'editSecUserGroup',
                    icon: 'resources/images/16x16/edit-file.png',
                    text: 'Edit User/Group'
                },
                {
                    xtype: 'menuitem',
                    action: 'editSecGroupRoles',
                    icon: 'resources/images/16x16/delete-icon.png',
                    text: 'Edit Group/Roles'
                }
            ]
        });

        me.callParent(arguments);
    }

});