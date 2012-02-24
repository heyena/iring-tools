Ext.define('AM.store.DirectoryStore', {
  extend: 'Ext.data.TreeStore',
  model: 'AM.model.DirectoryModel',
  //clearOnLoad: true,
  root: {
    expanded: true,
    type: 'ScopesNode',
    iconCls: 'scopes',
    text: 'Scopes',
    security: ''
  }

});