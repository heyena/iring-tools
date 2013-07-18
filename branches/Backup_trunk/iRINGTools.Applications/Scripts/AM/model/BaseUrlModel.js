Ext.define('AM.model.BaseUrlModel', {
    extend: 'Ext.data.Model',
    fields: [       
        { name: 'baseurl', mapping: 'Urlocator' }
    ],
    proxy: {
        type: 'ajax',
        timeout: 6000000,
        actionMethods: { read: 'POST' },
        extraParams: {},
        url: 'directory/endpointBaseUrl',
        reader: {
            type: 'json',
            root: 'items'
        }
    }
});