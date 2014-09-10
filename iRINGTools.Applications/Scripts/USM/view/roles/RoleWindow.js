
Ext.define('USM.view.roles.RoleWindow', {
    extend: 'Ext.window.Window',
    alias: 'widget.rolewindow',

    requires: [
        'USM.view.roles.RoleForm'
    ],

    floating: 'true',
    border: false,
    modal: true,
    layout: {
        type: 'fit'
    },
    bodyPadding: 0,
    title: 'Role',
    resizable: false,
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
                    xtype: 'roleform'
                }
            ]
        });

        me.callParent(arguments);
    },

    onReset: function () {
        var me = this;
        var win = me.up('window');
        me.down('roleform').getForm().reset();
        me.destroy();
    },

    onSave: function () {
        var me = this;
        var message;
        var msg;
        var form = me.down('roleform');
        if (form.getForm().isValid()) {
            msg = new Ext.window.MessageBox();
            msg.wait('Saving Role ....');
            form.submit({
                url: 'usersecuritymanager/saveRole',
                success: function (f, a) {
                    msg.close();
                    me.destroy();
                    var message = 'Role saved successfully.';
                    showDialog(400, 50, 'Alert', message, Ext.Msg.OK, null);
                    Ext.getCmp('rolegridid').store.reload();
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