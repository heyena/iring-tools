Ext.define('AM.model.TableSelectModel', {
    extend: 'Ext.data.Model',
    fields: [
      {
          name: 'text', type: 'string', mapping: 'dataObjects'
      }, {
          name: 'tableName', type: 'string', mapping: 'dataObjects'
      }]
});