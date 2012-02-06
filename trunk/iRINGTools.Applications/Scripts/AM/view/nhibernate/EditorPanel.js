Ext.define('AM.view.nhibernate.EditorPanel', {
    extend: 'Ext.panel.Panel',
    alias: 'widget.editorpanel',   
    border: false,
    frame: false,
    layout: 'card',
//    width: 400,
//    height: 200,
    autoScroll: true,
    bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
    labelWidth: 140,
    monitorValid: true,
    items: [],
    initComponent: function () {
        this.callParent(arguments);
    }
});