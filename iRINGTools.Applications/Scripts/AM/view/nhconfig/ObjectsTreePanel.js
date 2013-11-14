Ext.define('AM.view.nhconfig.ObjectsTreePanel', {
    extend: 'Ext.tree.Panel',
    alias: 'widget.objectstreepanel',

    rootVisible: false,

    initComponent: function () {
        var me = this;

        var store = {
            autoLoad: false,
            proxy: {
                type: 'ajax',
                timeout: 360000,
                actionMethods: {
                    read: 'POST'
                },
                url: 'NHibernate/ObjectsTree',
                extraParams: {
                    scope: null,
                    app: null,
                    dbProvider: null,
                    dbServer: null,
                    portNumber: null,
                    dbInstance: null,
                    dbName: null,
                    dbSchema: null,
                    dbUserName: null,
                    dbPassword: null,
                    serName: null,
                    selectedTables: null
                }
            },
            root: {
                text: 'Root',
                expanded: false
            }
        };

        Ext.applyIf(me, {
            store: store,
            dockedItems: [{
                xtype: 'toolbar',
                dock: 'top',
                layout: {
                    padding: 2,
                    type: 'hbox'
                },
                items: [
                {
                    xtype: 'tbspacer',
                    width: 4
                },
                {
                    xtype: 'button',
                    itemId: 'editconnectionbtn',
                    action: 'editconnection',
                    iconCls: 'am-document-properties',
                    text: 'Edit Connection',
                    tooltip: 'Edit Data Source Connection'
                },
                {
                    xtype: 'tbspacer',
                    width: 4
                },
                {
                    xtype: 'button',
                    itemId: 'savebtn',
                    action: 'save',
                    iconCls: 'am-document-save',
                    text: 'Save',
                    tooltip: 'Save Configuration'
                }]
            }]
        });

        me.callParent(arguments);
    }
});