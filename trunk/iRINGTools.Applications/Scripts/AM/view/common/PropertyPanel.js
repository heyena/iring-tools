Ext.define('AM.view.common.PropertyPanel', {
    extend: 'Ext.grid.property.Grid',
    alias: 'widget.propertypanel',

    title: 'Details',
    border: false,

    initComponent: function () {
        var me = this;
        Ext.applyIf(me, {
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