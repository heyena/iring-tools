Ext.define('AM.view.sqlconfig.SqlExtenConfigPanel', {
    extend: 'Ext.form.Panel',
    alias: 'widget.sqlextenconfigpanel',
    requires: ['Ext.ux.form.CheckboxListCombo'],
    bodyStyle: 'background:#fff;padding:10px',
    title: 'Extension',
    autoScroll: true,

    record: null,

    initComponent: function () {
        var me = this;

        var store = Ext.create('Ext.data.Store', {
            fields: ['name'],
            data: [{
                name: ''
            }]
        });


        Ext.applyIf(me, {
            defaults: {
                anchor: '100%',
                xtype: 'textfield',
                labelWidth: 160,
                allowBlank: true
                //readOnly: true
            },
            items: [{
                fieldLabel: 'Column Name',
                name: 'columnName',
                itemId: 'extncol'

            }, {
                fieldLabel: 'Property Name (editable)',
                name: 'propertyName',
                readOnly: false,
                regex: /^[a-zA-Z_][a-zA-Z0-9_]*$/,
                regexText: 'Value is invalid.'
            }, {
                fieldLabel: 'Data Type',
                name: 'dataType',
                maskRe: /[0-9.]/,
                allowBlank: false

            }, {
                fieldLabel: 'Data Length',
                name: 'dataLength',
                readOnly: true

            }, {
                xtype: 'checkboxfield',
                fieldLabel: 'Nullable',
                name: 'isNullable',
                readOnly: true
                

            },

                {
                    fieldLabel: 'Number of Decimals',
                    name: 'numberOfDecimals',
                    emptyText: '0',
                    readOnly: true

                }, {
                    fieldLabel: 'Key Type',
                    //hidden: true,
                    name: 'keyType',
                    readOnly: true
                }, {
                    fieldLabel: 'Precision',
                    name: 'precision',
                    emptyText: '0',
                    readOnly: true
                }, {
                    fieldLabel: 'Scale',
                    name: 'scale',
                    emptyText: '0',
                    readOnly: true
                }, {
                    fieldLabel: 'Definition',
                    name: 'definition'

                }, {

                    xtype: 'checkboxlistcombo',
                    fieldLabel: 'Parameters',
                    multiSelect: true,
                    itemId: 'paramCombo',
                    name: 'parameters',
                    displayField: 'name',
                    valueField: 'name',
                    store: store,
                    queryMode: 'local',
                    //allowBlank: false,
                    anchor: '40%'

                }
            ],
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
                    text: 'Apply'
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
            var selector = me.down('#paramCombo');

            var itemList;

            if (me.record.raw.type === 'extension') {
                itemList = me.record.parentNode.raw.properties.extensionColoumn;

                var availItems = [];
                Ext.each(itemList, function (item) {
                    availItems.push({
                        name: item
                    });
                });

                selector.store.loadData(availItems);
                me.getForm().setValues(this.record.raw.properties);

            } else if (me.record.raw.type === 'extensionProperty') {
                itemList = me.record.parentNode.parentNode.raw.properties.extensionColoumn;
                var availItems = [];
                Ext.each(itemList, function (item) {
                    availItems.push({
                        name: item
                    });
                });

                var params = this.record.raw.properties.parameters;
                var data = this.record.raw.properties;

                if (params[0].value != undefined) {
                    var arr = [];
                    Ext.each(params, function (item) {
                        arr.push(item.value);
                    });
                    data.parameters = arr;
                } else {
                    data.parameters = params;
                }

                me.getForm().setValues(data);
            }




        } else {
            me.getForm().setValues(this.record);

        }
    }
});