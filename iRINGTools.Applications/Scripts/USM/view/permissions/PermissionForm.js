
Ext.define('USM.view.permissions.PermissionForm', {
    extend: 'Ext.form.Panel',
    alias: 'widget.permissionform',
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
                    name: 'PermissionId'
                },
                {
                    xtype: 'hiddenfield',
                    name: 'SiteId'
                },
                {
                    xtype: 'textfield',
                    anchor: '100%',
                    style: 'margin:5px;padding:8px;',
                    fieldLabel: 'Name',
                    name: 'PermissionName',
                    allowBlank: false,
                    emptyText: 'Enter Permission Name'
                },
                {
                    xtype: 'textfield',
                    anchor: '100%',
                    style: 'margin:5px;padding:8px;',
                    fieldLabel: 'Description',
                    name: 'PermissionDesc',
                    emptyText: 'Enter Permission Description'
                }
            ]
        });

        me.callParent(arguments);
    }

});