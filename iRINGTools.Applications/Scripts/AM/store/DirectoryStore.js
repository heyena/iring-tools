Ext.define('AM.store.DirectoryStore', {
  extend: 'Ext.data.TreeStore',
  model: 'AM.model.DirectoryModel',
  root: {
    expanded: true,
    type: 'ScopesNode',
    iconCls: 'scopes',
    text: 'Scopes',
    security: ''
  },
  proxy: {
    type: 'ajax',
    timeout: 600000,
    url: 'directory/getnode',
    actionMethods: { read: 'POST' },
    extraParams: {
      id: null,
      type: 'ScopesNode',
      contextName: null,
      endpoint: null,
      baseUrl: null,
      related: null,
      security: null
    },
    reader: { type: 'json' }
  }
});