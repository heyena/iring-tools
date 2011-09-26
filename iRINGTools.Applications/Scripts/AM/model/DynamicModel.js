Ext.define('AM.model.DynamicModel', {
    extend: 'Ext.data.Model',
    fields: [],
    proxy: {
        type: 'ajax',
        actionMethods: { read: 'POST' },
        url: 'datagrid/getdata',
        extraParams: {
            scope: null,
            start: 0,
            limit: 25,
            app: null,
            graph: null    
        },
        reader: {
            totalProperty: 'totalCount' 
        }
    }
});