Ext.define('AM.view.common.ContentPanel', {
    extend: 'Ext.tab.Panel',
    alias: 'widget.contentpanel',
    collapsible: false,
    enableTabScroll: true,
    border: true,
    split: true,
    initComponent: function () {
        this.callParent(arguments);
    }
});