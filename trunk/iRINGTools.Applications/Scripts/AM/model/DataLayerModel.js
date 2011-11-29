Ext.define('AM.model.DataLayerModel', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'assembly', mapping: 'Assembly' },
        { name: 'name', mapping: 'Name' },
        { name: 'configurable', mapping: 'Configurable' }
    ],
    proxy: {
        type: 'ajax',
        timeout: 600000,
        actionMethods: { read: 'POST' },
        extraParams: {},
        url: 'directory/datalayers',
        reader: {
            type: 'json',
            root: 'items'
        }
    }
});