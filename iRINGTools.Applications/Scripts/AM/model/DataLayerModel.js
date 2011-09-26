Ext.define('AM.model.DataLayerModel', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'Assembly', type: 'string' },
        { name: 'Name', type: 'string' },
        { name: 'Configurable', type: 'string' }
    ],
    proxy: {
        type: 'ajax',
        timeout: 120000,
        actionMethods: { read: 'POST' },
        extraParams: {},
        url: 'directory/datalayers',
        reader: {
            type: 'json',
            root: 'items'
        }
    }
});