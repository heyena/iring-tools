
Ext.define('USM.view.users.UserGrid', {
    extend: 'Ext.grid.Panel',
    alias: 'widget.usergrid',
    resizable: true,
    store: 'UserS',

    initComponent: function () {
        var me = this;

        Ext.applyIf(me, {
            columns: [
                {
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
                }
            ]
        });

        me.callParent(arguments);
    }

});