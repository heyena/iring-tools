
Ext.define('AM.view.nhibernate.TreePanel', {
    extend: 'Ext.panel.Panel',
    alias: 'widget.nhibernatepanel',
    name: 'data-objects-pane',
    region: 'west',
    minWidth: 240,
    width: 300,
    split: true,
    autoScroll: true,
    dbDict: null,
    dbInfo: null,
    dataTypes: null,
    contextName: null,
    endpoint: null,
    bodyStyle: 'background:#fff',

    initComponent: function () {

        var conf = {
            dbDict: this.dbDict,
            dbInfo: this.dbInfo,
            dataTypes: this.dataTypes,
            contextName: this.contextName,
            endpoint: this.endpoint
        };

        var nhibernatetree = Ext.widget('nhibernatetreepanel', conf);

        this.items = [
            { xtype: 'nhibernatetreepanel' }
        ]; 

        this.callParent(arguments);
    }
});

