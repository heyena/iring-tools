Ext.define('AM.model.SearchModel', {
    extend: 'Ext.data.Model',
    fields: [
         { name: 'id', type: 'string' },
         { name: 'property', type: 'string' },
         { name: 'identifier', type: 'string' },
         { name: 'text', type: 'string' },
         { name: 'type', type: 'string' },
         { name: 'record', type: 'object' },
         { name: 'properties', type: 'object' }
    ]
});