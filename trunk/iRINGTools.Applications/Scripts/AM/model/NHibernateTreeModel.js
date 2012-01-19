
Ext.define('AM.model.NHibernateTreeModel', {
    extend: 'Ext.data.Model',
    alias: 'widget.nhTreeModel',   
    fields: [
         { name: 'id', type: 'string' },
         { name: 'hidden', type: 'boolean' },
         { name: 'properties', type: 'object' },
         { name: 'identifier', type: 'string' },
         { name: 'text', type: 'string' },
         { name: 'type', type: 'string' }
    ],
    proxy: {
        type: 'ajax',
        timeout: 600000,
        url: '',
        actionMethods: { read: 'POST' },
        extraParams: {
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

