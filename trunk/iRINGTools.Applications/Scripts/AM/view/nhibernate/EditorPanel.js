Ext.define('AM.view.nhibernate.EditorPanel', {
    extend: 'Ext.panel.Panel',
    alias: 'widget.editorpanel',
    border: 1,
    frame: false,
    layout: 'card',
    autoScroll: true,    
    monitorValid: true,
    contextName: null,
    bodyStyle: 'background:#eee',
    endpoint: null,
    baseUrl: null,
    items: [],
    initComponent: function () {
        this.callParent(arguments);
    }
});