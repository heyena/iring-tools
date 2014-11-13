
Ext.define('USM.view.permissions.PermissionGrid', {
    extend: 'Ext.grid.Panel',
    alias: 'widget.permissiongrid',
    resizable: true,
    store: 'PermissionS',
    resizable: false,
    id: 'idpermission',
    initComponent: function () {
        var me = this;

        Ext.applyIf(me, {
            columns: [
                {
                    xtype: 'gridcolumn',
                    dataIndex: 'PermissionName',
                    text: 'Permission',
                    flex: 2,
                    menuDisabled: true
                }, {
                    xtype: 'gridcolumn',
                    dataIndex: 'PermissionDesc',
                    text: 'Description',
                    flex: 2,
                    menuDisabled: true
                }
            ],
            listeners: {
                containercontextmenu: function (grid, e) {
                    var position = e.getXY();
                    e.stopEvent();
                    var win = Ext.widget('permsecuritymenu');
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
        var rec = Ext.getCmp('viewportid').down('permissiongrid').getSelectionModel().getSelection();
        var permissionId = rec[0].data.PermissionId;
        var win = Ext.widget('permissionwindow');
        var form = win.down('permissionform');
        form.getForm().setValues(rec[0].data);
        form.getForm().findField('ActionType').setValue('EDIT');
        form.getForm().getFields().each(function (field) {
            field.setReadOnly(true);
                   });
        Ext.ComponentQuery.query('#perbtn')[0].disable();
        win.show();
       
        

    }

});