
Ext.define('AM.view.nhibernate.TreePanel', {
    extend: 'Ext.panel.Panel',
    alias: 'widget.treePanel',
    name: 'data-objects-pane',
    region: 'west',
    minWidth: 240,
    width: 300,
    split: true,
    autoScroll: true,
    bodyStyle: 'background:#fff',

    initComponent: function () {

        var conf = {
            dbDict: this.dbDict,
            dbInfo: this.dbInfo,
            dataTypes: this.dataTypes,
            contextName: this.contextName,
            endpoint: this.endpoint
        };

        var nhibernatetree = Ext.widget('nhibernatetree', conf);

        this.items = [
            { xtype: 'nhibernatetree' }
        ]; 

        this.callParent(arguments);
    }
});

