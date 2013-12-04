Ext.define('AM.view.common.PropertyPanel', {
    extend: 'Ext.grid.property.Grid',
    alias: 'widget.propertypanel',

    title: 'Details',

    initComponent: function () {
        var me = this;
        Ext.applyIf(me, {
            border: true,
            autoScroll: true,
            collapsed: false,
            collapsible: true,    
            viewConfig: {
                stripeRows: true
            },
            source: {}
        });

        me.callParent(arguments);
    }
});