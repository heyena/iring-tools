
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
                    dataIndex: 'username',
                    text: 'User Name',
                    flex: 2,
                    menuDisabled: true
                }, {
                    xtype: 'gridcolumn',
                    dataIndex: 'fname',
                    text: 'First Name',
                    flex: 2,
                    menuDisabled: true
                }, {
                    xtype: 'gridcolumn',
                    dataIndex: 'lname',
                    text: 'Last Name',
                    flex: 2,
                    menuDisabled: true
                }, {
                    xtype: 'gridcolumn',
                    dataIndex: 'email',
                    text: 'E-mail',
                    flex: 2,
                    menuDisabled: true
                }, {
                    xtype: 'gridcolumn',
                    dataIndex: 'phone',
                    text: 'Phone',
                    flex: 2,
                    menuDisabled: true
                }, {
                    xtype: 'gridcolumn',
                    dataIndex: 'description',
                    text: 'Description',
                    flex: 2,
                    menuDisabled: true
                }
            ]
        });

        me.callParent(arguments);
    }

});