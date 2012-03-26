Ext.define('AM.store.NHibernateTreeStore', {
    extend: 'Ext.data.TreeStore',
    model: 'AM.model.NHibernateTreeModel',    
    clearOnLoad: true,
    root: {
        expanded: true,
        text: 'Data Objects',
        iconCls: 'folder'
    },
    proxy: {
        type: 'ajax',
        timeout: 600000,
        url: 'NHibernate/GetDBObjects',
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

