
Ext.define('AM.view.nhibernate.NHibernateTool', {
    extend: 'Ext.container.Container',
    alias: 'widget.nhibernatePane',
    iconCls: 'tabsNhibernate',    
    closable: true,
    border: false,
    frame: true,
    layout: 'fit',
    contextName: null,
    endpoint: null,
    tree: null,

    initComponent: function () {

        var conf = {
            id: this.contextName + '.' + this.endpoint + '.dataObjectsPane',
            contextName: this.contextName,
            endpoint: this.endpoint
        };

        var dataObjectPane = Ext.widget('dataObjectPane', conf);      

        this.items = [
            { xtype: 'dataObjectPane', region: 'center' }           
        ];

        this.callParent(arguments);

    }
});

