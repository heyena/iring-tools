Ext.define('USM.view.roles.GroupRoleSelectionPanel', {
    extend: 'Ext.form.Panel',
    alias: 'widget.grouproleselectionpanel',

    bodyStyle: 'background:#fff;padding:10px',
    width: 600,
    record: null,
    requires: [
           'Ext.ux.form.ItemSelector'
    ],
    initComponent: function () {
        var me = this;
        utilsObj.grpRole = "Groups Role";

        Ext.applyIf(me, {
            items: [{
                xtype: 'panel',
                layout: 'form',
                bodyPadding: 2,
                border: false,
                items: [{
                    xtype: 'combobox',
                    disabled: false,
                    name: 'RoleId',
                    allowBlank: false,
                    fieldLabel: 'Role',
                    emptyText: 'Select Role',
                    editable: false,
                    width: 100,
                    displayField: 'RoleName',
                    forceSelection: false,
                    store: 'RoleS',
                    valueField: 'RoleId',
                    listeners: {
                        select: {
                            fn: me.onSelectRole,
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
                    name: 'SelectedGroups',
                    anchor: '100%',
                    imagePath: '../ux/images/',
                    store: 'GroupS',
                    height: 200,
                    displayField: 'GroupName',
                    valueField: 'GroupId',
                    msgTarget: 'side',
                    fromTitle: 'Groups',
                    toTitle: utilsObj.grpRole
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
            msg.wait('Saving Groups ....');
            form.submit({
                url: 'usersecuritymanager/saveRoleGroups',
                success: function (f, a) {
                    msg.close();
                    if (btn.text == "Save") {
                        me.getForm().reset();
                        me.up('window').destroy();
                    }
                    //Ext.getCmp('rolegridid').store.reload();
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

    onSelectRole: function (combo, records, eOpts) {
        var me = this;
        var roleId = records[0].data.RoleId;
        me.getRoleGroups(roleId, me);
    },

    getRoleGroups: function (roleId, scope) {
        var me = this;
        Ext.Ajax.request({
            url: 'usersecuritymanager/getRoleGroups',
            method: 'POST',
            params: {
                RoleId: roleId
            },
            success: function (response, options) {
                var responseObj = Ext.JSON.decode(response.responseText);
                if (responseObj.success == undefined) {
                    var form = scope.up("itemselectorwindow").down('grouproleselectionpanel');

                    var selArr = [];
                    if (responseObj != null) {
                        for (var i = 0; i < responseObj.length; i++) {
                            selArr.push(responseObj[i].GroupId);
                        }
                        form.getForm().findField('SelectedGroups').setValue(selArr);
                    }
                    form.getForm().findField('RoleId').setValue(roleId);
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