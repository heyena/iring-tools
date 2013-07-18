
Ext.define('AM.model.PropertyMapModel', {
  extend: 'Ext.data.Model',
  fields: [
    { name: "property" },
    { name: "columnName" },
    { name: "relatedProperty" },
    { name: "relatedColumnName" },
  ]
});