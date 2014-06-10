
Ext.define('USM.view.SecurityGrid', {
    extend: 'Ext.grid.Panel',
    alias: 'widget.securitygrid',
    resizable: true,
    store: 'SecurityS',

    initComponent: function () {
        var me = this;

        Ext.applyIf(me, {
            columns: [
                {
                    xtype: 'gridcolumn',
                    dataIndex: 'name',
                    text: '',
                    flex: 2,
                    menuDisabled: true,
                    renderer: function (value) {
                        return '<html> <b>' + value + '</b></html>';
                    }
                }
            ]
        });

        me.callParent(arguments);
    }

});