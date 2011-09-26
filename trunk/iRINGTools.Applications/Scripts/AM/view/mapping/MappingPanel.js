Ext.define('AM.view.mapping.MappingPanel', {
    extend: 'Ext.panel.Panel',
    alias: 'widget.mappingpanel',
    height: 300,
    minSize: 150,
    layout: {
        type: 'border',
        padding: 5
    },
    split: true,
    closable: true,
    iconCls: 'tabsMapping',

    initComponent: function () {

        this.items = [
            { xtype: 'mappingtree', region: 'center' },
            { xtype: 'propertypanel', region: 'east', width: 350, height: 150, split: true, collapsible: true }
        ];

        this.callParent(arguments);
    }
});