Ext.define('AM.model.SpreadsheetModel', {
    extend: 'Ext.data.Model',
    fields: [
         { name: 'scope', type: 'string' },
         { name: 'application', type: 'string' },
         { name: 'type', type: 'string' },
    ]
});