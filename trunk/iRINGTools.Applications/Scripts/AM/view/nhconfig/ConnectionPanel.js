Ext.define('AM.view.nhconfig.ConnectionPanel', {
    extend: 'Ext.form.Panel',
    alias: 'widget.connectionpanel',

    autoScroll: true,
    bodyStyle: 'background:#fff;padding:10px',
    title: 'Configure Data Source',
    url: 'NHibernate/TableNames',
    method: 'POST',

    record: null,

    initComponent: function () {
        var me = this;

        Ext.applyIf(me, {
            defaults: {
                xtype: 'textfield',
                labelWidth: 100,
                anchor: '100%',
                allowBlank: false
            },
            items: [
            {
                hidden: true,
                name: 'scope'
            },
            {
                hidden: true,
                name: 'app'
            },
            {
                xtype: 'combobox',
                itemId: 'providerCombo',
                fieldLabel: 'Provider',
                name: 'dbProvider',
                displayField: 'Name',
                valueField: 'Name',
                value: 'MsSql2008',
                store: 'DBProviderStore',
                queryMode: 'local',
                listeners: {
                    change: me.onDBProviderChange,
                    scope: me
                }
            },
            {
                fieldLabel: 'Server',
                name: 'dbServer',
                value: 'localhost'
            },
            {
                fieldLabel: 'Instance',
                name: 'dbInstance'
            },
            {
                xtype: 'fieldcontainer',
                itemId: 'oracleopts',
                disabled: true,
                fieldLabel: 'Instance Type',
                defaultType: 'radiofield',
                layout: 'hbox',
                defaults: { flex: 1 },
                items: [
                {
                    boxLabel: 'SERVICE_NAME',
                    name: 'serName',
                    inputValue: 'SERVICE_NAME'
                }, {
                    boxLabel: 'SID',
                    name: 'serName',
                    inputValue: 'SID'
                }]
            },
            {
                fieldLabel: 'Port',
                name: 'portNumber',
                value: 1433
            },
            {
                fieldLabel: 'DB Name',
                name: 'dbName'
            },
            {
                fieldLabel: 'User Name',
                name: 'dbUserName'
            },
            {
                fieldLabel: 'Password',
                name: 'dbPassword',
                inputType: 'password'
            },
            {
                fieldLabel: 'Schema',
                name: 'dbSchema',
                hidden: true,
                value: 'dbo'
            }],
            dockedItems: [
            {
                xtype: 'toolbar',
                dock: 'top',
                layout: {
                    padding: 2,
                    type: 'hbox'
                },
                items: [
                {
                    xtype: 'button',
                    action: 'connect',
                    iconCls: 'am-document-properties',
                    text: 'Connect',
                    scope: me
                },
                {
                    xtype: 'tbseparator'
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
        var me = this;

        if (me.record != null) {
            me.getForm().setValues(me.record);
        }
    },

    onDBProviderChange: function (cbx, newValue, oldValue, eOpts) {
        var me = this;

        if (newValue == null) return;

        if (newValue.toLowerCase().startsWith('oracle')) {
            me.down('#oracleopts').setDisabled(false);
            me.getForm().findField('portNumber').setValue(1521);
            me.getForm().findField('serName').setValue('SERVICE_NAME');
        }
        else {
            if (newValue.toLowerCase().startsWith('mssql')) {
                me.getForm().findField('portNumber').setValue(1433);
            }

            me.down('#oracleopts').setDisabled(true);
        }
    }
});