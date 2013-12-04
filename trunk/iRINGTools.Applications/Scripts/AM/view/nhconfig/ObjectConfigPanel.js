Ext.define('AM.view.nhconfig.ObjectConfigPanel', {
    extend: 'Ext.form.Panel',
    alias: 'widget.objectconfigpanel',

    bodyStyle: 'background:#fff;padding:10px',
    title: 'Configure Data Object',
    autoScroll: true,

    record: null,

    initComponent: function () {
        var me = this;

        Ext.applyIf(me, {
            defaults: {
                anchor: '100%',
                xtype: 'textfield',
                labelWidth: 160
            },
            items: [
            {
                fieldLabel: 'Table Name',
                name: 'tableName',
                readOnly: true
            },
            {
                xtype: 'hiddenfield',
                name: 'objectNamespace'
            },
            {
                fieldLabel: 'Object Name',
                name: 'objectName',
                allowBlank: false,
                regex: /^[a-zA-Z_][a-zA-Z0-9_]*$/,
                regexText: 'Value is invalid.'
            },
            {
                fieldLabel: 'Key Delimiter',
                name: 'keyDelimiter'
            },
            {
                fieldLabel: 'Description',
                name: 'description'
            }],
            dockedItems: [
            {
                xtype: 'toolbar',
                height: 32,
                dock: 'top',
                layout: {
                    padding: 2,
                    type: 'hbox'
                },
                items: [
                {
                    xtype: 'tbspacer'
                },
                {
                    xtype: 'button',
                    action: 'apply',
                    iconCls: 'am-apply',
                    text: 'Apply'
                },
                {
                    xtype: 'tbspacer'
                },
                {
                    xtype: 'button',
                    action: 'reset',
                    iconCls: 'am-reset',
                    text: 'Reset',
                    scope: me,
                    handler: me.loadValues
                }]
            }],
            listeners: {
                afterrender: me.loadValues,
                scope: me
            }
        });

        me.callParent(arguments);
    },

    setRecord: function (record) {
        this.record = record;
        this.loadValues();
    },

    loadValues: function () {
        this.getForm().setValues(this.record);
    }
});