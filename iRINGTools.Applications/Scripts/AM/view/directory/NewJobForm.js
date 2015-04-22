﻿/*
* File: Scripts/AM/view/directory/ApplicationForm.js
*
* This file was generated by Sencha Architect version 2.2.2.
* http://www.sencha.com/products/architect/
*
* This file requires use of the Ext JS 4.1.x library, under independent license.
* License of Sencha Architect does not include license for Ext JS 4.1.x. For more
* details see http://www.sencha.com/license or contact license@sencha.com.
*
* This file will be auto-generated each and everytime you save your project.
*
* Do NOT hand edit this file.
*/
Ext.define('AM.view.directory.NewJobForm', {
    extend: 'Ext.form.Panel',
    alias: 'widget.newjobform',
    border: false,
    bodyStyle: 'padding:10px 5px 0',
    url: 'directory/saveSchedularData',
    initComponent: function () {
        var me = this;

        me.initialConfig = Ext.apply({
            url: 'directory/saveSchedularData'
        }, me.initialConfig);

        var applicationStore = Ext.create('Ext.data.Store', {
            fields: ['appId', 'appName'],
            storeId: 'applicationStoreId'
        });
        var dataObjectStore = Ext.create('Ext.data.Store', {
            fields: ['appId', 'dataObjId', 'dataObjName'],
            storeId: 'dataObjectStoreId'
        });
        Ext.applyIf(me, {

            items: [{
                xtype: 'fieldset',
                title: 'Job Info',
                layout: 'anchor',
                defaults: {
                    anchor: '100%'
                },
                items: [

                    {
                        xtype: 'container',
                        layout: 'hbox',
                        margin: '15 0 5 0',
                        items: [{
                            xtype: 'label',
                            name: 'contextLabel',
                            text: 'Context',
                            margin: '4 61 0 0 '
                        }, {

                            xtype: 'combobox',
                            itemId: 'contextCombo',
                            margin: '4 40 0 0 ',
                            labelWidth: 110,
                            name: 'contextName',
                            editable: false,
                            allowBlank: false
                        }, {
                            xtype: 'label',
                            name: 'applicationLabel',
                            text: 'Application',
                            margin: '4 40 0 0 '
                        }, {
                            xtype: 'combobox',
                            name: 'applicationName',
                            itemId: 'appCombo',
                            store: applicationStore,
                            displayField: 'appName',
                            valueField: 'appId',
                            queryMode: 'local',
                            autoSelect: true,
                            editable: false,
                            allowBlank: false,
                            listeners: {
                                select: function (combo, records, eOpts) {
                                    var me = this;
                                    var form = me.up('form');

                                    var dataObjStore = Ext.getStore(dataObjectStore);
                                    var tempStore = new Ext.create('Ext.data.Store', {
                                        fields: ['appId', 'dataObjId', 'dataObjName']
                                    });
                                    tempStore.add(dataObjStore.getRange());

                                    form.getForm().findField('dataObjectName').bindStore(tempStore);
                                    form.getForm().findField('dataObjectName').getStore().filter([{
                                        property: 'appId',
                                        value: me.getValue()
                                    }]);
                                    form.getForm().findField('dataObjectName').setValue(tempStore.getRange());
                                }
                            }
                        }]
                    }, {
                        xtype: 'container',
                        layout: 'hbox',
                        margin: '17 0 15 0',
                        items: [{
                            xtype: 'label',
                            name: 'dataObjectLabel',
                            text: 'DataObject',
                            margin: '4 42 0 0 '
                        }, {
                            labelWidth: 110,
                            xtype: 'checkboxlistcombo',
                            multiSelect: true,
                            name: 'dataObjectName',
                            emptyText: 'No Data Objects',
                            itemId: 'dataObjCombo',
                            store: dataObjectStore,
                            queryMode: 'local',
                            valueField: 'dataObjId',
                            displayField: 'dataObjName',
                            editable: false,
                            allowBlank: false
                        }]
                    }
                ]
            }, {
                xtype: 'fieldset',
                title: '<b>Occurance</b>',
                border: 0,
                margin: '15 0 25 0 ',
                layout: 'anchor',
                defaults: {
                    anchor: '100%'
                },
                items: [{
                    xtype: 'container',
                    layout: 'hbox',
                    defaultType: 'radiofield',
                    margin: '10 0 0 0',
                    items: [{
                        boxLabel: 'Immediate',
                        inputValue: 'Immediate',
                        checked: true,
                        name: 'occuranceRadio',
                        margin: '4 80 0 0 ',
                        //itemId:'myradiobutton',
                        allowBlank: false,
                        listeners: {
                            change: function (cb, checked) {
                                Ext.ComponentQuery.query('#fldContainer')[0].setDisabled(checked);
                                Ext.ComponentQuery.query('#checkBox')[0].setDisabled(checked);
                                Ext.ComponentQuery.query('#myEndDateCombo')[0].setDisabled(checked);
                                Ext.ComponentQuery.query('#myEndTimeCombo')[0].setDisabled(checked);
                                Ext.ComponentQuery.query('#myStartDateCombo')[0].setDisabled(checked);
                                Ext.ComponentQuery.query('#myStartTimeCombo')[0].setDisabled(checked);
                            }
                        }
                    }, {
                        boxLabel: 'Daily',
                        inputValue: 'Daily',
                        name: 'occuranceRadio',
                        margin: '4 80 0 0 ',
                        allowBlank: false
                    }, {

                        boxLabel: 'Weekly',
                        inputValue: 'Weekly',
                        name: 'occuranceRadio',
                        margin: '4 80 0 0 ',
                        allowBlank: false
                    }, {

                        boxLabel: 'Monthly',
                        inputValue: 'Monthly',
                        name: 'occuranceRadio',
                        margin: '4 40 0 0 ',
                        allowBlank: false
                    }]
                }]

            }, {
                xtype: 'fieldset',
                border: 0,
                itemId: 'fldContainer',
                layout: 'anchor',
                defaults: {
                    anchor: '100%'
                },
                items: [{
                    xtype: 'container',
                    layout: 'hbox',
                    items: [{
                        xtype: 'datefield',
                        fieldLabel: 'Start DateTime',
                        name: 'startDate',
                        minValue: (new Date()),
                        itemId: 'myStartDateCombo',
                        emptyText: 'Select Start Date',
                        width: 220,
                        disabled: true,
                        anchor: '100%',
                        allowBlank: false
                    }, {
                        xtype: 'timefield',
                        name: 'startTime',
                        minValue: '00:00',
                        maxValue: '24:00',
                        itemId: 'myStartTimeCombo',
                        width: 60,
                        format: 'H:i',
                        increment: 1,
                        emptyText: 'Time',
                        disabled: true,
                        margin: '0 10 0 0',
                        allowBlank: false,
                        anchor: '100%'
                    }]
                }]

            }, {
                xtype: 'fieldset',
                border: 0,
                layout: 'anchor',
                defaults: {
                    anchor: '100%'
                },
                items: [{
                    xtype: 'container',
                    layout: 'hbox',
                    margin: '10 0 5 0',
                    items: [{
                        xtype: 'datefield',
                        fieldLabel: 'End DateTime',
                        name: 'endDate',
                        minValue: (new Date()),
                        emptyText: 'Select End Date',
                        width: 220,
                        disabled: true,
                        itemId: 'myEndDateCombo',
                        anchor: '100%',
                        allowBlank: false
                    }, {
                        xtype: 'timefield',
                        name: 'endTime',
                        minValue: '00:00',
                        maxValue: '24:00',
                        width: 60,
                        format: 'H:i',
                        increment: 1,
                        emptyText: 'Time',
                        disabled: true,
                        itemId: 'myEndTimeCombo',
                        margin: '0 10 0 0',
                        anchor: '100%',
                        allowBlank: false
                    }, {
                        xtype: 'checkboxfield',
                        boxLabel: 'No End Date',
                        name: 'chkBox',
                        itemId: 'checkBox',
                        margin: '4 0 0 60',
                        disabled: true,
                        checked: false,
                        listeners: {
                            change: function (cb, checked) {
                                Ext.ComponentQuery.query('#myEndDateCombo')[0].setDisabled(checked);
                                Ext.ComponentQuery.query('#myEndTimeCombo')[0].setDisabled(checked);
                            }
                        }
                    }]
                }]



            }],

            dockedItems: [{
                xtype: 'toolbar',
                dock: 'bottom',
                items: [{
                    xtype: 'tbfill'
                }, {
                    xtype: 'button',
                    handler: function (button, event) {
                        me.onSave();
                    },
                    text: 'Save'
                }, {
                    xtype: 'button',
                    handler: function (button, event) {
                        me.onReset();


                    },
                    text: 'Cancel'
                }]
            }]
        });

        me.callParent(arguments);
    },
    onSave: function () {
        var me = this;
        var win = me.up('window');
        var form = me.getForm();
        //var state = form.findField('state').getValue();
        var node = me.node;
        var folderSiteId;

        if (form.isValid()) {
            form.submit({
                waitMsg: 'Saving Data...',
                method: 'POST',
                params: {
                    record: node.get('record')
                },
                success: function (response, request) {
                    var objResponseText = Ext.JSON.decode(request.response.responseText);
                    if (objResponseText["message"] == "Duplicate Job") {
                        showDialog(400, 50, 'Alert', "Job  name already exists", Ext.Msg.OK, null);
                        return;
                    }


                    win.fireEvent('save', me);

                    var currentNode;

                    var index = 0;

                    Ext.each(Ext.JSON.decode(request.response.responseText).nodes, function (newNode) {
                        currentNode.insertChild(index, newNode);
                        index++;
                    });

                    me.setLoading(false);
                    if (objResponseText["message"] == "Job Added Successfuly") {
                        Ext.example.msg('Notification', 'Job added successfully!');
                    }

                },
                failure: function (response, request) {
                    var objResponseText = Ext.decode(request.response.responseText);
                    var userMsg = objResponseText['message'];
                    var detailMsg = objResponseText['stackTraceDescription'];
                    var expPanel = Ext.widget('exceptionpanel', {
                        title: 'Error Notification'
                    });
                    Ext.ComponentQuery.query('#expValue', expPanel)[0].setValue(userMsg);
                    Ext.ComponentQuery.query('#expValue2', expPanel)[0].setValue(detailMsg);

                }
            });
        } else {
            Ext.widget('messagepanel', {
                title: 'Warning',
                msg: 'Please complete all required fields.'
            });
            return;
        }
    },
    onReset: function () {
        var me = this;
        var win = me.up('window');
        win.fireEvent('Cancel', me);
    }
    //        onSave: function () {
    //            var me = this;
    //            var win = me.up('window');
    //            var form = me.getForm();
    //            var node = me.node;
    //            var contextName = me.getForm().findField('contextName').getValue();
    //            var applicationName = me.getForm().findField('applicationName').getValue();
    //            var dataObjectName = me.getForm().findField('dataObjectName').getValue();
    //            var startDate = me.getForm().findField('startDate').getValue();
    //            var startTime = me.getForm().findField('startTime').getValue();
    //            var endDate = me.getForm().findField('endDate').getValue();
    //            var endTime = me.getForm().findField('endTime').getValue();
    //            var ouccaradio = me.getForm().findField('occuranceRadio').getValue();

    //            me.getForm().submit({
    //                waitMsg: 'Saving Data...',
    //                success: function (response, request) {
    //                      var objResponseText = Ext.JSON.decode(request.response.responseText);
    //                    if (objResponseText["message"] == "Duplicate Job") {
    //                        showDialog(400, 50, 'Alert', "Job  name already exists", Ext.Msg.OK, null);
    //                        return;
    //                    }


    //                    win.fireEvent('save', me);

    //                    var currentNode;

    //                    var index = 0;

    //                    Ext.each(Ext.JSON.decode(request.response.responseText).nodes, function (newNode) {
    //                        currentNode.insertChild(index, newNode);
    //                        index++;
    //                    });

    //                    me.setLoading(false);
    //                    if (objResponseText["message"] == "Job Added Successfuly") {
    //                        Ext.example.msg('Notification', 'Job added successfully!');
    //                    }
    //                },
    //                failure: function (response, request) {
    //                    Ext.Msg.alert('Failed', action.result.msg);
    //                }
    //            });
    //        }

});