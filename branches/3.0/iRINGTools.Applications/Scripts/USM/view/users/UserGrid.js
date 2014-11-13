Ext.define('USM.view.users.UserGrid', {
    extend: 'Ext.grid.Panel',
    alias: 'widget.usergrid',
    resizable: true,
    store: 'UserS',
    id: 'iduser',
    resizable: false,
    initComponent: function () {
        var me = this;
       
        Ext.applyIf(me, {
            columns: [{
                xtype: 'gridcolumn',
                dataIndex: 'UserName',
                text: 'User Name',
                flex: 2,
                menuDisabled: true
            }, {
                xtype: 'gridcolumn',
                dataIndex: 'UserFirstName',
                text: 'First Name',
                flex: 2,
                menuDisabled: true
            }, {
                xtype: 'gridcolumn',
                dataIndex: 'UserLastName',
                text: 'Last Name',
                flex: 2,
                menuDisabled: true
            }, {
                xtype: 'gridcolumn',
                dataIndex: 'UserEmail',
                text: 'E-mail',
                flex: 2,
                menuDisabled: true
            }, {
                xtype: 'gridcolumn',
                dataIndex: 'UserPhone',
                text: 'Phone',
                flex: 2,
                menuDisabled: true
            }, {
                xtype: 'gridcolumn',
                dataIndex: 'UserDesc',
                text: 'Description',
                flex: 2,
                menuDisabled: true
            }],
            listeners: {
                containercontextmenu: function (grid, e) {
                    var position = e.getXY();
                    e.stopEvent();
                    var win = Ext.widget('usersecuritymenu');
                    win.showAt(position);
                },
                itemdblclick: {
                    fn: me.onDblClickUser,
                    scope: me
                }
            }

        });

        me.callParent(arguments);
    },
    onDblClickUser: function (dataview, record, item, index, e, eOpts) {
        var me = this;
        var rec = Ext.getCmp('viewportid').down('usergrid').getSelectionModel().getSelection();
        var userId = rec[0].data.UserId;
        var win = Ext.widget('userwindow');
        var form = win.down('userform');
        form.getForm().setValues(rec[0].data);
        form.getForm().findField('ActionType').setValue('EDIT');
        form.getForm().getFields().each(function (field) {
        field.setReadOnly (true);
        });
        Ext.ComponentQuery.query('#usrbtn')[0].disable();
        win.show();
       
    }
});


