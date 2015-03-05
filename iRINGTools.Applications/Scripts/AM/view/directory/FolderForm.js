﻿/*
* File: Scripts/AM/view/directory/RootPopUpForm.js
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

Ext.define('AM.view.directory.FolderForm', {
    extend: 'Ext.form.Panel',
    alias: 'widget.FolderForm',
    requires: ['Ext.ux.form.CheckboxListCombo'],
    node: '',
    bodyStyle: 'padding:10px 5px 0',
    url: 'directory/Folder',

    initComponent: function () {
        var me = this;

        me.initialConfig = Ext.apply({
            url: 'directory/Folder'
        },

        me.initialConfig);

        Ext.applyIf(me, {
            defaults: {
                anchor: '100%',
                msgTarget: 'side'
            },

            dockedItems: [{
                xtype: 'toolbar',
                dock: 'bottom',
                items: [{
                    xtype: 'tbfill'
                },
                {
                    xtype: 'button',
                    handler: function (button, event) {
                        me.onSave();
                    },
                    text: 'Ok'
                },
                {
                    xtype: 'button',
                    handler: function (button, event) {
                        me.onReset();
                    },
                    text: 'Cancel'
                }]
            }],
            items: [{
                xtype: 'hiddenfield',
                name: 'id'
            },
            {
                xtype: 'hiddenfield',
                name: 'path'
            },
            {
                xtype: 'hiddenfield',
                name: 'state'
            },
             {
                 xtype: 'hiddenfield',
                 name: 'isFolderNameChanged'
             },
            {
                xtype: 'hiddenfield',
                name: 'oldContext'
            },
            {
                xtype: 'hiddenfield',
                name: 'name'
            },
            {
                xtype: 'textfield',
                fieldLabel: 'Display Name',
                name: 'displayName',
                allowBlank: false
            },
            {
                xtype: 'hiddenfield',
                itemId: 'contextname',
                name: 'contextName'
            },
            {
                xtype: 'checkboxlistcombo',
                width: 180,
                multiSelect: true,
                name: 'ResourceGroups',
                itemId: 'ResourceGroupId',
                fieldLabel: 'Groups for the User:',
                displayField: 'groupName',
                autoSelect: false,
                queryMode: 'remote',
                store: 'ResourceGroupStore',
                valueField: 'groupId'
            }]
        });

        me.callParent(arguments);
    },

    onSave: function () {
        var me = this;
        var win = me.up('window');
        var form = me.getForm();
        var folderName = form.findField('displayName').getValue();
        var state = form.findField('state').getValue();
        var contextNameField = form.findField('contextName');
        var node = me.node;
        var FolderNameBeforeUpdate =Ext.String.trim(node.raw.text);
        var isFolderNameChanged;
        if (FolderNameBeforeUpdate == Ext.String.trim(folderName) ) {
            isFolderNameChanged = false;

        }
        else {
            isFolderNameChanged = true;

        }
        form.findField('isFolderNameChanged').setValue(isFolderNameChanged);
        if (state == 'new')
            form.findField('name').setValue(folderName);

        //        node.eachChild(function (n) {
        //            if (n.data.text == folderName) {
        //                if (state == 'new') {
        //                    Ext.widget('messagepanel', { title: 'Warning', msg: 'Folder name \"' + folderName + '\" already exists.' });
        //                    return;
        //                }
        //            }
        //        });

        if (form.isValid()) {
            form.submit({
                waitMsg: 'Saving Data...',

                success: function (response, request) {
                    var objResponseText = Ext.JSON.decode(request.response.responseText);
                    

                    //Ext.example.msg('Notification', 'Folder saved successfully!');
                    win.fireEvent('save', me);

                    var currentNode;

                    if (state == 'new') {
                        currentNode = node;
                    }
                    else {
                        currentNode = node.parentNode;
                    }

                    while (currentNode.firstChild) {
                        currentNode.removeChild(currentNode.firstChild);
                    }

                    var index = 0;

                    Ext.each(Ext.JSON.decode(request.response.responseText).nodes, function (newNode) {
                        currentNode.insertChild(index, newNode);
                        index++;
                    });

                    me.setLoading(false);

                    if (objResponseText["message"] == "folderadded") {
                        showDialog(400, 50, 'Alert', "Folder added successfully!", Ext.Msg.OK, null);
                    }
                    if (objResponseText["message"] == "folderupdated") {
                        showDialog(400, 50, 'Alert', "Folder updated successfully!", Ext.Msg.OK, null);
                    }
                    if (objResponseText["message"] == "duplicatefolder") {
                        showDialog(400, 50, 'Alert', "Folder with this name already exists", Ext.Msg.OK, null);
                    }
                },

                failure: function (response, request) {
                    var objResponseText = Ext.JSON.decode(request.response.responseText);
                    var userMsg = objResponseText['message'];
                    var detailMsg = objResponseText['stackTraceDescription'];
                    var expPanel = Ext.widget('exceptionpanel', { title: 'Error Notification' });
                    Ext.ComponentQuery.query('#expValue', expPanel)[0].setValue(userMsg);
                    Ext.ComponentQuery.query('#expValue2', expPanel)[0].setValue(detailMsg);
                }
            });
        }
        else {
            Ext.widget('messagepanel', { title: 'Warning', msg: 'Please give folder name.' });
            return;
        }
    },

    onReset: function () {
        var me = this;
        var win = me.up('window');
        win.fireEvent('cancel', me);
    }
});