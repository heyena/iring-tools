Ext.define('AM.view.nhibernate.EditorPanel', {
    extend: 'Ext.panel.Panel',
    alias: 'widget.editorpanel',
    border: 1,
    frame: false,
    layout: 'card',    
    bodyStyle: 'background:#eee',    
    items: [],
    initComponent: function () {
        this.callParent(arguments);
    }
});