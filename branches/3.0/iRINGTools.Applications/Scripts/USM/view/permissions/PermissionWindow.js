
Ext.define('USM.view.permissions.PermissionWindow', {
    extend: 'Ext.window.Window',
    alias: 'widget.permissionwindow',

    requires: [
        'USM.view.permissions.PermissionForm'
    ],

    floating: 'true',
    border: false,
    modal: true,
    layout: {
        type: 'fit'
    },
    resizable: false,
    bodyPadding: 0,
    title: 'Permission',

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
                    xtype: 'permissionform'
                }
            ]
        });

        me.callParent(arguments);
    },

    onReset: function () {
        var me = this;
        var win = me.up('window');
        me.down('permissionform').getForm().reset();
        me.destroy();
    },

    onSave: function () {
        var me = this;
        var message;
        var msg;
        var form = me.down('permissionform');
        if (form.getForm().isValid()) {
            msg = new Ext.window.MessageBox();
            msg.wait('Saving Permission ....');
            form.submit({
                url: 'usersecuritymanager/savePermission',
                success: function (f, a) {
                    msg.close();
                    me.destroy();
                    var message = 'Permission saved successfully.';
                    showDialog(400, 50, 'Alert', message, Ext.Msg.OK, null);
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