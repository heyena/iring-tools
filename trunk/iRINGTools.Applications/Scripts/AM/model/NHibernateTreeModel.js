
Ext.define('AM.model.NHibernateTreeModel', {
    extend: 'Ext.data.Model', 
    fields: [
         { name: 'id', type: 'string' },
         { name: 'hidden', type: 'boolean' },
         { name: 'properties', type: 'object' },
         { name: 'identifier', type: 'string' },
         { name: 'text', type: 'string' },
         { name: 'type', type: 'string' }
    ]
});

