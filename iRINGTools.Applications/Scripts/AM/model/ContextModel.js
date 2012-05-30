Ext.define('AM.model.ContextModel', {
    extend: 'Ext.data.Model',
    fields: [       
        { name: 'context', mapping: 'contextName' }
    ],
    proxy: {
        type: 'ajax',
        timeout: 6000000,
        actionMethods: { read: 'POST' },
        extraParams: {},
        url: 'directory/folderContext',
        reader: {
            type: 'json',
            root: 'items'
        }
    }
});