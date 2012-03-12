Ext.define('AM.model.DynamicModel', {
    extend: 'Ext.data.Model',
    fields: [],
    proxy: {
        type: 'ajax',
        timeout: 600000,
        actionMethods: { read: 'POST' },
        url: 'GridManager/Pages',
        extraParams: {
            context: null,
            start: 0,
            limit: 25,
            endpoint: null,
            graph: null    
        },
        reader: {
            totalProperty: 'totalCount' 
        }
    }
});