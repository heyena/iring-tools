Ext.define('USM.view.groups.GrpUserSelectionPanel', {
    extend: 'Ext.form.Panel',
    alias: 'widget.grpuserselectionpanel',

    bodyStyle: 'background:#fff;padding:10px',
    width: 600,
    record: null,
    requires: [
           'Ext.ux.form.ItemSelector'
    ],
    initComponent: function () {
        var me = this;
        utilsObj.grpUser = "Group Users";

        Ext.applyIf(me, {
            items: [{
                xtype: 'panel',
                layout: 'form',
                bodyPadding: 2,
                border: false,
                items: [{
                    xtype: 'combobox',
                    disabled: false,
                    name: 'groupId',
                    allowBlank: false,
                    fieldLabel: 'Group',
                    emptyText: 'Select Group',
                    editable: false,
                    width: 100,
                    displayField: 'GroupName',
                    forceSelection: false,
                    store: 'GroupS',
                    valueField: 'GroupId',
                    listeners: {
                        select: {
                            fn: me.onSelectGroup,
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
                    itemId: 'userselector',
                    name: 'selectedUsers',
                    anchor: '100%',
                    imagePath: '../ux/images/',
                    store: 'UserS',
                    height: 200,
                    displayField: 'UserFirstName',
                    valueField: 'UserId',
                    msgTarget: 'side',
                    fromTitle: 'Users',
                    toTitle: utilsObj.grpUser
                }]
            }],


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
                url: 'usersecuritymanager/saveGroupUsers',
                success: function (f, a) {
                    msg.close();
                    if (btn.text == "Save") {
                        me.getForm().reset();
                        me.up('window').destroy();
                    }
                    //Ext.getCmp('groupgridid').store.reload();
                    var objResponseText = Ext.JSON.decode(a.response.responseText);
                    var message = objResponseText['message'];
                    showDialog(400, 50, 'Alert', message, Ext.Msg.OK, null);
                    return;
                },
                failure: function (f, a) {
                    msg.close();
                    me.up('window').destroy();
                    var objResponseText = Ext.JSON.decode(a.response.responseText);
                    var message = objResponseText['message'];
                    showDialog(400, 50, 'Error', message, Ext.Msg.OK, null);
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

    onSelectGroup: function (combo, records, eOpts) {
        var me = this;
        var groupId = records[0].data.GroupId;
        me.getGroupUsers(groupId, me);
    },

    getGroupUsers: function (groupId, scope) {
        var me = this;
        Ext.Ajax.request({
            url: 'usersecuritymanager/getGroupUsers',
            method: 'POST',
            params: {
                GroupId: groupId
            },
            success: function (response, options) {
                var responseObj = Ext.JSON.decode(response.responseText);
                if (responseObj.success == undefined) {
                    var form = scope.up("itemselectorwindow").down('grpuserselectionpanel');

                    var selArr = [];
                    if (responseObj != null) {
                        for (var i = 0; i < responseObj.length; i++) {
                            selArr.push(responseObj[i].UserId);
                        }
                        form.getForm().findField('selectedUsers').setValue(selArr);
                    }
                    form.getForm().findField('groupId').setValue(groupId);
                } else {
                    showDialog(400, 50, 'Error', responseObj.message, Ext.Msg.OK, null);
                }
            },

            failure: function (response, options) {
            }
        });
    }

});