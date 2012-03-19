Ext.define('AM.view.nhibernate.EditorPanel', {
    extend: 'Ext.panel.Panel',
    alias: 'widget.editorpanel',
    border: 1,
    frame: false,
    layout: 'card',
    autoScroll: true,
    labelWidth: 140,
    monitorValid: true,
    items: [],
    initComponent: function () {
        this.callParent(arguments);
    }
});