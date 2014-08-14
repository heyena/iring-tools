
Ext.define('USM.view.groups.GroupForm', {
    extend: 'Ext.form.Panel',
    alias: 'widget.groupform',
    frame: false,
    width: 400,
    bodyPadding: 10,

    initComponent: function() {
        var me = this;

        Ext.applyIf(me, {
            items: [
                {
                    xtype: 'hiddenfield',
                    name: 'ActionType',
                    value : 'ADD'
                },
                {
                    xtype: 'hiddenfield',
                    name: 'GroupId'
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
                    maxLength : 100,
                    name: 'GroupName',
                    allowBlank: false,
                    emptyText: 'Enter Group Name'
                },
                {
                    xtype: 'textfield',
                    anchor: '100%',
                    style: 'margin:5px;padding:8px;',
                    fieldLabel: 'Description',
                    maxLength: 255,
                    name: 'GroupDesc',
                    emptyText: 'Enter Description'
                }
            ]
        });

        me.callParent(arguments);
    }

});