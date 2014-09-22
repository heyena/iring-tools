Ext.define('AM.view.nhconfig.KeyConfigPanel', {
    extend: 'Ext.form.Panel',
    alias: 'widget.keyconfigpanel',

    bodyStyle: 'background:#fff;padding:10px',
    title: 'Configure Key',
    autoScroll: true,

    record: null,

    initComponent: function () {
        var me = this;

        Ext.applyIf(me, {
            defaults: {
                anchor: '100%',
                xtype: 'textfield',
                labelWidth: 160,
                allowBlank: false,
                readOnly: true
            },
            items: [
            {
                fieldLabel: 'Column Name',
                name: 'columnName'
            },
            {
                fieldLabel: 'Property Name (editable)',
                name: 'propertyName',
                readOnly: false,
                regex: /^[a-zA-Z_][a-zA-Z0-9_]*$/,
                regexText: 'Value is invalid.'
            },
            {
                fieldLabel: 'Data Type',
                name: 'dataType'
            },
            {
                fieldLabel: 'Data Length',
                name: 'dataLength'
            },
            {
                xtype: 'checkboxfield',
                fieldLabel: 'Nullable',
                name: 'isNullable'
            },
            {
                xtype: 'checkboxfield',
                fieldLabel: 'Show On Index',
                name: 'showOnIndex'
            },
            {
                fieldLabel: 'Number of Decimals',
                name: 'numberOfDecimals'
            },
            {
                xtype: 'checkboxfield',
                fieldLabel: 'Hidden',
                name: 'isHidden'
            },
            {
                xtype: 'checkboxfield',
                fieldLabel: 'ReadOnly',
                name: 'isReadOnly'
            },
            {
                xtype: 'checkboxfield',
                fieldLabel: 'Virtual',
                name: 'isVirtual'
            },
            {
                fieldLabel: 'Key Type',
                hidden: true,
                name: 'keyType'
            },
             {
                 fieldLabel: 'Precision',
                 name: 'precision'
             },
              {
                  fieldLabel: 'Scale',
                  name: 'scale'
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