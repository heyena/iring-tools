Ext.define('AM.view.nhibernate.EditorPanel', {
    extend: 'Ext.form.Panel',
    alias: 'widget.editorPanel',   
    border: 1,
    frame: false,
    region: 'center',
    layout: 'card',

    initComponent: function () {
        this.callParent(arguments);
    }
});