Ext.define('AM.view.sqlconfig.SqlExtensionPanel', {
    extend: 'Ext.form.Panel',
    alias: 'widget.sqlextensionpanel',

    bodyStyle: 'background:#fff;padding:10px',
    title: 'Add/Remove Extensions',
    autoScroll: true,

    record: null,

    initComponent: function () {
        var me = this;

        Ext.applyIf(me, {
            defaults: {
                anchor: '100%',
                labelWidth: 110
            },
            items: [{
                xtype: 'fieldcontainer',
                fieldLabel: 'New Extension',
                layout: 'hbox',
                items: [{
                    xtype: 'textfield',
                    flex: 1,
                    allowBlank: false,
                    name: 'extensionName',
                    listeners: {
                        specialkey: {
                            fn: me.onSpecialkey,
                            scope: me
                        }
                    }
                }, {
                    xtype: 'tbspacer',
                    width: 2
                }, {
                    xtype: 'button',
                    itemId: 'addextenstionbtn',
                    iconCls: 'am-list-add',
                    action: 'addextension',
                    scope: me,
                    handler: me.addExtension
                }]
            }, {
                xtype: 'fieldcontainer',
                fieldLabel: 'Extensions',
                items: [{
                    xtype: 'grid',
                    forceFit: true,
                    store: {
                        fields: ['columnName'],
                        data: me.record
                    },
                    columns: [{
                        text: 'Name',
                        dataIndex: 'columnName'
                    }, {
                        xtype: 'actioncolumn',
                        text: 'Action',
                        align: 'center',
                        width: 16,
                        items: [{
                            iconCls: 'am-list-remove',
                            handler: me.removeExtension
                        }]
                    }]
                }]
            }],
            dockedItems: [{
                xtype: 'toolbar',
                height: 32,
                dock: 'top',
                layout: {
                    padding: 2,
                    type: 'hbox'
                },
                items: [{
                    xtype: 'tbspacer'
                }, {
                    xtype: 'button',
                    action: 'apply',
                    iconCls: 'am-apply',
                    text: 'Apply',
                    handler: function () {
                        this.up('form').getForm().reset();
                    }
                }, {
                    xtype: 'tbspacer'
                }, {
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

    onSpecialkey: function (field, e, eOpts) {
        var me = this;
        if (e.getKey() === e.ENTER) {
            var btn = me.down('#addextenstionbtn');
            me.addRelationship(btn);
        }
    },

    addExtension: function (button, e) {

        var form = button.up('form');
        var extName = form.getForm().findField('extensionName').getValue();

        if (extName.length > 0) {
            var newRecord = {
                columnName: extName
            };
            var store = form.down('grid').getStore();
            store.add(newRecord);
        }
    },

    removeExtension: function (dataview, rowIndex, colIndex, action, e, record, eOpts) {

        dataview.getStore().remove(record);
    },

    setRecord: function (record) {
        this.record = record;
        this.loadValues();
    },

    loadValues: function () {
        var store = this.down('grid').getStore();
        store.loadData(this.record);
    }
});