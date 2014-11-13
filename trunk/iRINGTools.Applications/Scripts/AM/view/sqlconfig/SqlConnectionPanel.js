
Ext.define('AM.view.sqlconfig.SqlConnectionPanel', {
    extend: 'Ext.form.Panel',
    alias: 'widget.sqlconnectionpanel',

    bodyStyle: 'background:#fff;padding:10px',
    title: 'Configure Data Source',
    autoScroll: true,
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
                fieldLabel: 'DB Name',
                name: 'dbName',
                allowBlank: true
            },
            {
                fieldLabel: 'Port',
                name: 'portNumber',
                value: 1433
            },
            {
                fieldLabel: 'UserName',
                name: 'dbUserName',
                listeners: {
                    change: me.onUserNameChange,
                    scope: me
                }
            },
            {
                fieldLabel: 'Password',
                name: 'dbPassword',
                inputType: 'password'
            },
            {
                fieldLabel: 'Schema Name',
                name: 'dbSchema',
                value: 'dbo',
                allowBlank: true
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
            }]
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
        else {
            var dbProvider = me.getForm().findField('dbProvider');

            if (dbProvider.getValue() == null) {
                dbProvider.setValue('MsSql2008');
            }
        }
    },

    onDBProviderChange: function (cbx, newValue, oldValue, eOpts) {
        var me = this;

        if (newValue == null) return;

        var form = me.getForm();
        var dbUserName = form.findField('dbUserName').getValue();

        if (newValue.toLowerCase().indexOf('oracle') != -1) {
            me.down('#oracleopts').setVisible(true);
            form.findField('dbName').setVisible(false);
            form.findField('dbSchema').setValue(dbUserName);
            form.findField('portNumber').setValue(1521);
        }
        else {
            if (newValue.toLowerCase().indexOf('mssql') != -1) {
                form.findField('portNumber').setValue(1433);
            }

            me.down('#oracleopts').setVisible(false);
            form.findField('dbName').setVisible(true);
            form.findField('dbSchema').setValue('dbo');
        }
    },

    onUserNameChange: function (field, newValue, oldValue, eOpts) {
        var me = this;
        var form = me.getForm();
        var dbProvider = form.findField('dbProvider').getValue();

        if (dbProvider.toLowerCase().indexOf('oracle') != -1) {
            var dbUserName = form.findField('dbUserName').getValue();
            form.findField('dbSchema').setValue(dbUserName);
        }
    }
});