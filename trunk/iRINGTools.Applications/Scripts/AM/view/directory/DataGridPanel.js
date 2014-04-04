Ext.define('AM.view.directory.DataGridPanel', {
    extend: 'Ext.grid.Panel',
    alias: 'widget.dynamicgrid',
    xtype: 'myGrid',
    requires: [
        'AM.view.override.directory.DataGridPanel',
        'AM.store.DataGridStore'
    ],

    closable: true,
    store: 'DataGridStore',
    style: 'background-color: #fff;',
	//verticalScrollerType:null,//'paginggridscroller',
	//loadMask:false,
	//invalidateScrollerOnRefresh:false,
    initComponent: function () {
        var me = this;

        Ext.applyIf(me, {
            columns: [
            {
                xtype: 'gridcolumn',
                dataIndex: 'string'
            }],
            viewConfig: {
                enableTextSelection: true
            }
        });

        me.callParent(arguments);
    }
});