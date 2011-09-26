/// <reference path="../../extjs40/ext-debug.js" />

Ext.define('AM.model.DirectoryModel', {
    extend: 'Ext.data.Model',
    fields: [
         { name: 'id', type: 'string' },
         { name: 'hidden', type: 'boolean' },
         { name: 'property', type: 'object' },
         { name: 'identifier', type: 'string' },
         { name: 'text', type: 'string' },
         { name: 'type', type: 'string' },
         { name: 'record', type: 'object' }
    ],
    proxy: {
        type: 'ajax',
        timeout: 120000,
        url: 'directory/getnode',
        actionMethods: { read: 'POST' },
        extraParams: {
        id: null,
        type: 'ScopesNode',
        related: null
      },
      reader: { type: 'json' }
    }
});