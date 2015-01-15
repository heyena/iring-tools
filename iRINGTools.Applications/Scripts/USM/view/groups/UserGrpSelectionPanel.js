Ext.define('USM.view.groups.UserGrpSelectionPanel', {
    extend: 'Ext.form.Panel',
    alias: 'widget.usergrpselectionpanel',

    bodyStyle: 'background:#fff;padding:10px',
    width: 600,
    record: null,
    requires: [
           'Ext.ux.form.ItemSelector'
    ],
    initComponent: function () {
        var me = this;
        utilsObj.grpUser = "User Groups";

        Ext.applyIf(me, {
            items: [{
                xtype: 'panel',
                layout: 'form',
                bodyPadding: 2,
                border: false,
                items: [{
                    xtype: 'combobox',
                    disabled: false,
                    name: 'userId',
                    allowBlank: false,
                    fieldLabel: 'User',
                    emptyText: 'Select User',
                    editable: false,
                    width: 100,
                    displayField: 'UserName',
                    forceSelection: false,
                    store: 'UserS',
                    valueField: 'UserId',
                    listeners: {
                        select: {
                            fn: me.onSelectUser,
                            scope: me
                        }
                    }
                }]
            },
            {
                xtype: 'panel',
                layout: 'fit',
                bodyPadding: 2,
                border: false,
                items: [{
                    xtype: 'itemselector',
                    itemId: 'groupselector',
                    name: 'selectedGroups',
                    anchor: '100%',
                    imagePath: '../ux/images/',
                    store: 'GroupS',
                    height: 200,
                    displayField: 'GroupName',
                    valueField: 'GroupId',
                    msgTarget: 'side',
                    fromTitle: 'Groups',
                    toTitle: utilsObj.grpUser
                }]
            }],

            listeners: {
                afterrender: me.loadValues,
                scope: me
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
                                me.onSave(button);
                            },
                            iconCls: 'icon-accept',
                            text: 'Apply'
                        },
                        {
                            xtype: 'button',
                            handler: function (button, event) {
                                me.onSave(button);
                            },
                            iconCls: 'icon-accept',
                            text: 'Save'
                        },
                        {
                            xtype: 'button',
                            handler: function (button, event) {
                                me.onReset();
                            },
                            iconCls: 'icon-cancel',
                            text: 'Cancel'
                        }
                    ]
                }
            ]
        });

        me.callParent(arguments);
    },

    onReset: function () {
        var me = this;
        var win = me.up('window');
        me.getForm().reset();
        win.destroy();
    },

    onSave: function (btn) {
        var me = this;
        var message;
        var msg;
        var form = me;
        if (me.getForm().isValid()) {
            msg = new Ext.window.MessageBox();
            msg.wait('Saving Group ....');
            form.submit({
                url: 'usersecuritymanager/saveUserGroups',
                success: function (f, a) {
                    msg.close();
                    if (btn.text == "Save") {
                        me.getForm().reset();
                        me.up('window').destroy();
                    }

                    var objResponseText = Ext.JSON.decode(a.response.responseText);
                    var message = objResponseText['message'];
                    showDialog(400, 50, 'Alert', message, Ext.Msg.OK, null);
                    //                    if (objResponseText['success'] == true) {
                    //                        Ext.getCmp('usergridid').store.reload();     
                    //                    }
                    return;
                },
                failure: function (f, a) {
                    msg.close();
                    me.up('window').destroy();
                    var objResponseText = Ext.JSON.decode(a.response.responseText);
                    var message = objResponseText['message'];
                    showDialog(400, 50, 'Alert', message, Ext.Msg.OK, null);
                }
            });
        } else {
            message = 'Please fil the form to save.';
            showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
        }
    },

    setRecord: function (record) {
        this.record = record;
        this.loadValues();
    },

    onSelectUser: function (combo, records, eOpts) {
        var me = this;
        var userId = records[0].data.UserId;
        var userName = records[0].data.UserName;
        me.getUsersGroup(userName, userId, me);
    },

    getUsersGroup: function (userName, userId, scope) {
        var me = this;
        Ext.Ajax.request({
            url: 'usersecuritymanager/getUserGroups',
            //url: '/Scripts/USM/jsonfiles/selgroup.json',
            method: 'POST',
            params: {
                UserName: userName
            },
            success: function (response, options) {
                var responseObj = Ext.JSON.decode(response.responseText);
                if (responseObj.success == undefined) {
                    var form = scope.up("itemselectorwindow").down('usergrpselectionpanel');

                    var selArr = [];

                    if (responseObj != null) {
                        for (var i = 0; i < responseObj.length; i++) {
                            selArr.push(responseObj[i].GroupId);
                        }
                        form.getForm().findField('selectedGroups').setValue(selArr);
                    }

                    form.getForm().findField('userId').setValue(userId);
                } else {
                    showDialog(400, 50, 'Error', responseObj.message, Ext.Msg.OK, null);
                }
            },
            failure: function (response, options) {
            }
        });
    },

    loadValues: function () {
        var me = this;
    }

});