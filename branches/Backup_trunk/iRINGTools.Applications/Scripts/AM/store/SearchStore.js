Ext.define('AM.store.SearchStore', {
    extend: 'Ext.data.TreeStore',
    model: 'AM.model.SearchModel',
    autoLoad: false,
    clearOnLoad: true,
    root: {
        expanded: true
    }
});