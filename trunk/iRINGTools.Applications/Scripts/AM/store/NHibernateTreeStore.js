Ext.define('AM.store.NHibernateTreeStore', {
    extend: 'Ext.data.TreeStore',
    alias: 'widget.nhTreeStore',   
    model: 'AM.model.NHibernateTreeModel',    
    clearOnLoad: true,
    root: {
        expanded: true,
        text: 'Data Objects',
        iconCls: 'folder'
    }
});

