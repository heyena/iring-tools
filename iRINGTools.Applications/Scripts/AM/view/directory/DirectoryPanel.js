Ext.define('AM.view.directory.DirectoryPanel', {
    extend: 'Ext.panel.Panel',
    alias: 'widget.directorypanel',

    requires: [
        'AM.view.directory.DirectoryTree',
        'AM.view.common.PropertyPanel'
    ],

    split: true,
    border: true,
    layout: {
        type: 'border'
    },
    collapsed: false,
    collapsible: true,
    title: 'Directory',

    initComponent: function () {
        var me = this;

        Ext.applyIf(me, {
            items: [
            {
                xtype: 'directorytree',
                region: 'center'
            },
            {
                xtype: 'propertypanel',
                title: 'Directory Details',
                height: 220,
                collapseDirection: 'bottom',
                region: 'south',
                split: true
            }
        ]});

        me.callParent(arguments);
    }
});