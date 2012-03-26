Ext.define('AM.model.SpreadsheetModel', {
    extend: 'Ext.data.Model',
    fields: [
         { name: 'id', type: 'string' },
         { name: 'text', type: 'string' },
         { name: 'type', type: 'string' }, 
         { name: 'record', type: 'object' }
    ]
});