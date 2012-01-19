
Ext.define('AM.model.NHibernateTreeModel', {
    extend: 'Ext.data.Model',
    alias: 'widget.nhTreeModel',   
    fields: [
         { name: 'property', type: 'object' },
         { name: 'id', type: 'string' },
         { name: 'identifier', type: 'string' },
         { name: 'text', type: 'string' },
         { name: 'icon', type: 'string' },
         { name: 'leaf', type: 'boolean' },
         { name: 'expanded', type: 'boolean' },
         { name: 'hidden', type: 'boolean' },
         { name: 'children', type: 'auto' },
         { name: 'type', type: 'string' },
         { name: 'nodeType', type: 'string' },
         { name: 'checked', type: 'boolean' },
         { name: 'record', type: 'object' },
         { name: 'properties', type: 'object' },
         { name: 'iconCls', type: 'string' }
    ],
    proxy: {
        type: 'ajax',
        timeout: 600000,
        url: '',
        actionMethods: { read: 'POST' },
        baseParams: {
            scope: null,
            app: null,
            dbProvider: null,
            dbServer: null,
            dbInstance: null,
            dbName: null,
            dbSchema: null,
            dbUserName: null,
            dbPassword: null,
            portNumber: null,
            tableNames: null,
            serName: null
        },
        reader: { type: 'json' }
    }
});

