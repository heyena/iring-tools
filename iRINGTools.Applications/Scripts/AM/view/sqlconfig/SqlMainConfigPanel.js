Ext.define('AM.view.sqlconfig.SqlMainConfigPanel', {
    extend: 'Ext.panel.Panel',
    alias: 'widget.sqlmainconfigpanel',
    closable: true,

    initComponent: function () {
        var me = this;

        Ext.applyIf(me, {
            layout: 'border',
            items: [{
                xtype: 'sqlobjectstreepanel',
                width: 300,
                floatable: false,
                region: 'west',
                split: true
            }, {
                xtype: 'panel',
                itemId: 'configcontainer',
                frame: false,
                border: false,
                region: 'center',
                layout: 'card',
                items: [{
                    xtype: 'sqlconnectionpanel'
                }, {
                    xtype: 'sqltableselectionpanel'
                }, {
                    xtype: 'sqlobjectconfigpanel'
                }, {
                    xtype: 'sqlkeyselectionpanel'
                }, {
                    xtype: 'sqlkeyconfigpanel'
                }, {
                    xtype: 'sqlpropertyselectionpanel'
                }, {
                    xtype: 'sqlpropertyconfigpanel'
                }, {
                    xtype: 'sqlrelationshipspanel'
                }, {
                    xtype: 'sqlrelationshipconfigpanel'
                }]
            }]
        });

        me.callParent(arguments);
    }
});