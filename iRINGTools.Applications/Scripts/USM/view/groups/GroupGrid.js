Ext.define('USM.view.groups.GroupGrid', {
    extend: 'Ext.grid.Panel',
    alias: 'widget.groupgrid',
    resizable: true,
    store: 'GroupS',
    id: 'idgroup',
    resizable: false,
    initComponent: function () {
        var me = this;

        Ext.applyIf(me, {
            columns: [
                {
                    xtype: 'gridcolumn',
                    dataIndex: 'GroupName',
                    text: 'Group Name',
                    flex: 1,
                    menuDisabled: true
                }, {
                    xtype: 'gridcolumn',
                    dataIndex: 'GroupDesc',
                    text: 'Description',
                    flex: 2,
                    menuDisabled: true
                }
            ],
                listeners: {
                    containercontextmenu: function (grid, e) {
                        var position = e.getXY();
                        e.stopEvent();
                        var win = Ext.widget('securitymenu');
                        win.showAt(position);
                    },
                itemdblclick: {
                    fn: me.onDblClickGrp,
                    scope: me
                }
            }

        });

        me.callParent(arguments);
    },

    onDblClickGrp: function (dataview, record, item, index, e, eOpts) {
        var me = this;
        var rec = Ext.getCmp('viewportid').down('groupgrid').getSelectionModel().getSelection();
        var groupId = rec[0].data.GroupId;
        var win = Ext.widget('groupwindow');
        var form = win.down('groupform');
        form.getForm().setValues(rec[0].data);
        form.getForm().findField('ActionType').setValue('EDIT');
        form.getForm().getFields().each(function (field) {
            field.setReadOnly(true);
        });
        Ext.ComponentQuery.query('#grpbtn')[0].disable();
        win.show();

    }
});