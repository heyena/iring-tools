﻿/*
* File: Scripts/AM/view/directory/ContextForm.js
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

Ext.define('AM.view.directory.ContextForm', {
    extend: 'Ext.form.Panel',
    alias: 'widget.contextform',
    requires: ['Ext.ux.form.CheckboxListCombo'],
    node: '',
    border: 2,
    bodyPadding: 10,
    bodyStyle: 'padding:10px 5px 0',
    url: 'directory/Context',
    cacheConnStrTpl: 'Data Source={hostname\\dbInstance};Initial Catalog={dbName};User ID={userId};Password={password}',

    initComponent: function () {
        var me = this;

        me.initialConfig = Ext.apply({
            url: 'directory/Context'
        }, me.initialConfig);

        Ext.applyIf(me, {
            defaults: {
                anchor: '100%',
                msgTarget: 'side'
            },
            dockedItems: [
        {
            xtype: 'toolbar',
            dock: 'bottom',
            items: [
            {
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
            }
          ]
        }
      ],
            items: [
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
                xtype: 'textareafield',
                fieldLabel: 'Description',
                name: 'description'
            },
            {
                xtype: 'textareafield',
                fieldLabel: 'Cache ConnStr',
                name: 'cacheDBConnStr',
                value: this.cacheConnStrTpl
            },
            {
                xtype: 'checkboxlistcombo',
                width: 180,
                multiSelect: true,
                itemId: 'permissionitem',
                fieldLabel: 'Permissions:',
                labelSeparator: '',
                emptyText: '',
                allowBlank: true,
                name: 'permissions',
                displayField: 'permission',
                autoSelect: false,
                queryMode: 'remote',
                store: 'PermissionsS',
                valueField: 'permission',
                hidden:true
            }
          ]
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
        //form.findField('contextName').setValue(folderName);
        //    var context = form.findField('contextCombo').getValue();
        //contextNameField.setValue(context);

        //if(state == 'new')
        //form.findField('name').setValue(folderName);

        if (form.findField('cacheDBConnStr').getValue() == this.cacheConnStrTpl)
            form.findField('cacheDBConnStr').setValue('');

        node.eachChild(function (n) {
            if (n.data.text == folderName) {
                if (state == 'new') {
                    Ext.widget('messagepanel', { title: 'Warning', msg: 'Scope name \"' + folderName + '\" already exists.' });
                    //showDialog(400, 100, 'Warning', 'Scope name \"' + folderName + '\" already exists.', Ext.Msg.OK, null);
                    return;
                }
            }
        });

        if (form.isValid()) {
            form.submit({
                waitMsg: 'Saving Data...',
                success: function (response, request) {
                    Ext.example.msg('Notification', 'Scope saved successfully!');
                    win.fireEvent('save', me);
                    var parentNode = node.parentNode;
                    if (parentNode == undefined && node.data.text == 'Scopes') {
                        var nodeIndex = 0; //node.lastChild.data.index+1;
                        node.insertChild(nodeIndex, Ext.JSON.decode(request.response.responseText).nodes[0]);
                    } else {
                        var nodeIndex = parentNode.indexOf(node);
                        parentNode.removeChild(node);
                        parentNode.insertChild(nodeIndex, Ext.JSON.decode(request.response.responseText).nodes[0]);
                    }
                    me.setLoading(false);
                    //win.fireEvent('save', me);
                    //node.firstChild.expand();
                    //node.expandChildren();
                    //Ext.ComponentQuery.query('directorytree')[0].onReload();
                },
                failure: function (response, request) {
                    /*if (response.items != undefined && response.items[3].value !== undefined) {
                    var rtext = response.items[3].value;
                    Ext.widget('messagepanel', { title: 'Error saving folder changes', msg: 'Changes of ' + rtext + ' are not saved.'});
                    return;
                    }*/
                    //Ext.widget('messagepanel', { title: 'Warning', msg: 'Error saving changes!'});
                    var resp = Ext.decode(request.response.responseText);
                    var userMsg = resp['message'];
                    var detailMsg = resp['stackTraceDescription'];
                    var expPanel = Ext.widget('exceptionpanel', { title: 'Error Notification' });
                    Ext.ComponentQuery.query('#expValue', expPanel)[0].setValue(userMsg);
                    Ext.ComponentQuery.query('#expValue2', expPanel)[0].setValue(detailMsg);

                }
            });
        } else {
            Ext.widget('messagepanel', { title: 'Warning', msg: 'Please complete all required fields.' });
            //showDialog(400, 100, 'Warning', 'Please complete all required fields...', Ext.Msg.OK, null);
            return;
        }
    },

    onReset: function () {
        var me = this;
        var win = me.up('window');
        win.fireEvent('cancel', me);


    }

});