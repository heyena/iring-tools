Ext.define('AM.view.common.CenterPanel', {
    extend: 'Ext.panel.Panel',
    alias: 'widget.centerpanel',
    region: 'center',
    layout: 'border',
    collapsible: false,
    enableTabScroll: true,
    border: true,
    split: true,
    items: [
        { xtype: 'contentpanel', region: 'center', id: 'maincontent' },
        { xtype: 'searchpanel', region: 'south' }
    ],
    initComponent: function () {
        this.callParent(arguments);
    }
});