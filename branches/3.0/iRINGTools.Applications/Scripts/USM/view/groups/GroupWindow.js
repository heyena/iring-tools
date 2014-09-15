
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
                    var message = 'Group saved successfully.';
                    showDialog(400, 50, 'Alert', message, Ext.Msg.OK, null);
                    if (Ext.getCmp('groupgridid') != undefined) {
                        Ext.getCmp('groupgridid').store.reload();
                    }
                    return;
                },
                failure: function (f, a) {
                    msg.close();
                    me.destroy();
                }
            });
        } else {
            message = 'Please fil the form to save.';
            showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
        }
    }

});