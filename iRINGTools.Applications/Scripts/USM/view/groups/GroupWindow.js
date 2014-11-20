
Ext.define('USM.view.groups.GroupWindow', {
    extend: 'Ext.window.Window',
    alias: 'widget.groupwindow',

    requires: [
        'USM.view.groups.GroupForm'
    ],

    floating: 'true',
    border: false,
    resizable: false,
    modal: true,
    layout: {
        type: 'fit'
    },
    bodyPadding: 0,
    title: 'Group',

    initComponent: function () {
        var me = this;

        Ext.applyIf(me, {
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
                            itemId: 'grpbtn',
                            handler: function (button, event) {
                                me.onSave();
                            },
                            iconCls: 'icon-accept',
                            text: 'Ok'
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
            ],
            items: [
                {
                    xtype: 'groupform'
                }
            ]
        });

        me.callParent(arguments);
    },

    onReset: function () {
        var me = this;
        var win = me.up('window');
        me.down('groupform').getForm().reset();
        me.destroy();
    },

    onSave: function () {
        var me = this;
        var message;
        var msg;
        var form = me.down('groupform');
        if (form.getForm().isValid()) {
            msg = new Ext.window.MessageBox();
            msg.wait('Saving Group ....');
            form.submit({
                url: 'usersecuritymanager/saveGroup',
                success: function (f, a) {
                    msg.close();
                    me.destroy();
                    var objResponseText = Ext.JSON.decode(a.response.responseText);
                    var message = objResponseText['message'];
                    showDialog(400, 50, 'Alert', message, Ext.Msg.OK, null);
                    if (objResponseText['success'] == true) {
                        var tabPanel = Ext.getCmp("maincontent");
                        if (Ext.getCmp('groupgridid') != undefined) {
                            Ext.getCmp('groupgridid').store.reload();
                        }
                        else {
                            
                            var gridPanel = Ext.create('USM.view.groups.GroupGrid', {
                                title: 'Groups',
                                id: "groupgridid",
                                closable: true
                            });
                            tabPanel.add(gridPanel);
                            gridPanel.store.reload();

                        }
                        tabPanel.setActiveTab(Ext.getCmp("groupgridid")); 
                    }
                    return;
                },
                failure: function (f, a) {
                    msg.close();
                    me.destroy();
                    var objResponseText = Ext.JSON.decode(a.response.responseText);
                    var message = objResponseText['message'];
                    showDialog(400, 50, 'Error', message, Ext.Msg.OK, null);
                }
            });
        } else {
            message = 'Please fil the form to save.';
            showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
        }
    }

});