Ext.define('AM.view.sqlconfig.SqlRelationshipsPanel', {
    extend: 'Ext.form.Panel',
    alias: 'widget.sqlrelationshipspanel',

    bodyStyle: 'background:#fff;padding:10px',
    title: 'Add/Remove Relationships',
    autoScroll: true,

    record: null,

    initComponent: function () {
        var me = this;

        Ext.applyIf(me, {
            defaults: {
                anchor: '100%',
                labelWidth: 110
            },
            items: [
            {
                xtype: 'fieldcontainer',
                fieldLabel: 'New Relationship',
                layout: 'hbox',
                items: [{
                    xtype: 'textfield',
                    flex: 1,
                    allowBlank: false,
                    name: 'relationshipName',
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
                    itemId: 'addrelationshipbtn',
                    iconCls: 'am-list-add',
                    action: 'addrelationship',
                    scope: me,
                    handler: me.addRelationship
                }]
            },
            {
                xtype: 'fieldcontainer',
                fieldLabel: 'Relationships',
                items: [{
                    xtype: 'grid',
                    forceFit: true,
                    store: {
                        fields: ['name'],
                        data: me.record
                    },
                    columns: [{
                        text: 'Name',
                        dataIndex: 'name'
                    }, {
                        xtype: 'actioncolumn',
                        text: 'Action',
                        align: 'center',
                        width: 16,
                        items: [{
                            iconCls: 'am-list-remove',
                            handler: me.removeRelationship
                        }]
                    }]
                }]
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
                }
              ]
            }
          ]
        });

        me.callParent(arguments);
    },

    onSpecialkey: function (field, e, eOpts) {
        var me = this;
        if (e.getKey() === e.ENTER) {
            var btn = me.down('#addrelationshipbtn');
            me.addRelationship(btn);
        }
    },

    addRelationship: function (button, e) {
        var form = button.up('form');
        var relName = form.getForm().findField('relationshipName').getValue();

        if (relName.length > 0) {
            var newRecord = { name: relName };
            var store = form.down('grid').getStore();
            store.add(newRecord);
        }
    },

    removeRelationship: function (dataview, rowIndex, colIndex, action, e, record, eOpts) {
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