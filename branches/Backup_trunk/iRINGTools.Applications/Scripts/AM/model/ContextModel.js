Ext.define('AM.model.ContextModel', {
  extend: 'Ext.data.Model',
  fields: [       
    { name: 'context', mapping: 'Context' }
  ],
  proxy: {
    type: 'ajax',
    timeout: 6000000,
    loadMask: false,
    actionMethods: { read: 'POST' },
    extraParams: {},
    url: 'directory/folderContext',
    reader: {
      type: 'json',
      root: 'items'
    }
  }
});