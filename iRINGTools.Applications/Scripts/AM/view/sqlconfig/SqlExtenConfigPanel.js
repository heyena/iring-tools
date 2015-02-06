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
                itemId: 'extncolname',
                readOnly: true


            }, {
                fieldLabel: 'Property Name (editable)',
                name: 'propertyName',
                readOnly: false,
                regex: /^[a-zA-Z_][a-zA-Z0-9_]*$/,
                regexText: 'Value is invalid.'
            }, {
                //xtype: 'numberfield',
                fieldLabel: 'Data Type',
                name: 'dataType',
                readOnly: true,
                value: 11,
                readOnly: true
                //allowBlank: false,
                //maxValue:13,
                //minValue: 0,
                // emptyText: 'Please enter value from 0 to 13.'

            }, {
                fieldLabel: 'Data Length',
                name: 'dataLength',
                readOnly: true,
                emptyText: '1000'

            }, {
                xtype: 'checkboxfield',
                fieldLabel: 'Nullable',
                name: 'isNullable',
                //checked: true,
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
                    xtype: 'textarea',
                    fieldLabel: 'Definition',
                    name: 'definition',
                    allowBlank: false
                    //itemId: 'def'

                }, {

                    xtype: 'checkboxlistcombo',
                    fieldLabel: 'Parameters',
                    multiSelect: true,
                    itemId: 'paramCombo',
                    name: 'parameters',
                    hidden:true,
                   // displayField: 'name',
                   // valueField: 'name',
                   // store: store,
                   // queryMode: 'local',
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
                    text: 'Reset'
                   // scope: me,
                   // handler: me.loadValues
                }]
            }],
            listeners: {
             // afterrender: me.loadValues,
                //scope: me
            }
        });

        me.callParent(arguments);
    },

    setRecord: function (record, objectNode) {
        this.record = record;
        this.objectNode = objectNode;
        this.loadValues();
    },

    loadValues: function () {


   

        var me = this;
        if (me.record != null) {

       
            if (me.record.raw.type === 'extensionProperty') {


                var propName = this.record.raw.properties.propertyName;
                

                if (propName == undefined) {
                    me.getForm().reset();
                     }


                me.getForm().setValues(this.record.raw.properties);
            }
        } 
        else {
            me.getForm().setValues(this.record);

        }


    }


});



