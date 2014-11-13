
Ext.define('USM.view.roles.RoleGrid', {
    extend: 'Ext.grid.Panel',
    alias: 'widget.rolegrid',
    resizable: true,
    store: 'RoleS',
    resizable: false,
    id: 'idrole',
    initComponent: function () {
        var me = this;
 
        Ext.applyIf(me, {
            columns: [
                {
                    xtype: 'gridcolumn',
                    dataIndex: 'RoleName',
                    text: 'Role',
                    flex: 1,
                    menuDisabled: true
                }, {
                    xtype: 'gridcolumn',
                    dataIndex: 'RoleDesc',
                    text: 'Description',
                    flex: 2,
                    menuDisabled: true
                }
            ],
                listeners: {
                    containercontextmenu: function (grid, e) {
                        var position = e.getXY();
                        e.stopEvent();
                        var win = Ext.widget('rolesecuritymenu');
                        win.showAt(position);
                    },
                    itemdblclick: {
                        fn: me.onDblClickRole,
                        scope: me
                    }
                }

        });

        me.callParent(arguments);
    },

    onDblClickRole: function (dataview, record, item, index, e, eOpts) {
        var me = this;
        var rec = Ext.getCmp('viewportid').down('rolegrid').getSelectionModel().getSelection();
        var roleId = rec[0].data.RoleId;
        var win = Ext.widget('rolewindow');
        var form = win.down('roleform');
        form.getForm().setValues(rec[0].data);
        form.getForm().findField('ActionType').setValue('EDIT');
        form.getForm().getFields().each(function (field) {
            field.setReadOnly(true);
        });
        Ext.ComponentQuery.query('#rolbtn')[0].disable();
        win.show();

    }

});