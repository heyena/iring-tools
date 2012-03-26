Ext.define('AM.view.common.PropertyPanel', {
    extend: 'Ext.grid.property.Grid',
    alias: 'widget.propertypanel',
    title: 'Details',
    layout: 'fit',
    viewConfig: { stripeRows: true },
    collapsible: true,
    autoScroll: true,
    split: true,
    bodyBorder: true,
    collapsed: false,
    border: 0,
    frame: false,
    source: {}
});