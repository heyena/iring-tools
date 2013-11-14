Ext.define('AM.view.nhconfig.MainConfigPanel', {
    extend: 'Ext.panel.Panel',
    alias: 'widget.mainconfigpanel',

    closable: true,

    initComponent: function () {
        var me = this;

        Ext.applyIf(me, {
            layout: 'border',
            items: [{
                xtype: 'objectstreepanel',
                width: 300,
                floatable: false,
                region: 'west',
                split: true
            }, {
                xtype: 'container',
                itemId: 'configcontainer',
                floatable: false,
                region: 'center',
                autoScroll: true,
                layout: 'card',
                items: [{
                    xtype: 'connectionpanel'
                }, {
                    xtype: 'tableselectionpanel'
                }, {
                    xtype: 'objectconfigpanel'
                }, {
                    xtype: 'keyselectionpanel'
                }, {
                    xtype: 'keyconfigpanel'
                }, {
                    xtype: 'propertyselectionpanel'
                }, {
                    xtype: 'propertyconfigpanel'
                }, {
                    xtype: 'relationshipspanel'
                }, {
                    xtype: 'relationshipconfigpanel'
                }]
            }]
        });

        me.callParent(arguments);
    }
});