
Ext.define('USM.view.roles.RoleForm', {
    extend: 'Ext.form.Panel',
    alias: 'widget.roleform',
    frame: false,
    width: 400,
    bodyPadding: 10,

    initComponent: function() {
        var me = this;

        Ext.applyIf(me, {
            items: [
                {
                    xtype: 'hiddenfield',
                    name: 'actionType',
                    value : 'ADD'
                },
                {
                    xtype: 'hiddenfield',
                    name: 'RoleId'
                },
                {
                    xtype: 'hiddenfield',
                    name: 'siteId'
                },
                {
                    xtype: 'textfield',
                    anchor: '100%',
                    style: 'margin:5px;padding:8px;',
                    fieldLabel: 'Name',
                    maxLength: 100,
                    name: 'RoleName',
                    allowBlank: false,
                    emptyText: 'Enter Role Name'
                },
                {
                    xtype: 'textfield',
                    anchor: '100%',
                    style: 'margin:5px;padding:8px;',
                    fieldLabel: 'Description',
                    maxLength: 255,
                    name: 'RoleDesc',
                    emptyText: 'Enter Description'
                }
            ]
        });

        me.callParent(arguments);
    }

});