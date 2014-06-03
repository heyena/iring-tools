Ext.define('USM.view.groups.GroupGrid', {
    extend: 'Ext.grid.Panel',
    alias: 'widget.groupgrid',
    resizable: true,
    store: 'GroupS',

    initComponent: function () {
        var me = this;

        Ext.applyIf(me, {
            columns: [
                {
                    xtype: 'gridcolumn',
                    dataIndex: 'groupName',
                    text: 'Group Name',
                    flex: 1,
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