
Ext.define('USM.view.ItemSelectorWindow', {
    extend: 'Ext.window.Window',
    alias: 'widget.itemselectorwindow',

    requires: [
        'USM.view.groups.GrpUserSelectionPanel'
    ],

    border: false,
    resizable: true,
    modal: true,
    layout: {
        type: 'fit'
    },
    bodyPadding: 0,
    title: '',
    form : '',
    initComponent: function () {
        var me = this;
        Ext.applyIf(me, {
            items: [
                {
                    xtype: me.form
                }
            ]
        });

        me.callParent(arguments);
    }
});