Ext.define('AM.view.mapping.MappingPanel', {
  extend: 'Ext.panel.Panel',
  alias: 'widget.mappingpanel',

  requires: [
    'AM.view.mapping.MappingTree',
    'AM.view.common.PropertyPanel'
  ],

  endpoint: '',
  baseUrl: '',
  contextName: '',
  graph: '',
  layout: {
    type: 'border'
  },
  closable: true,

  initComponent: function() {
    var me = this;

    Ext.applyIf(me, {
      items: [
        {
          xtype: 'mappingtree',
          region: 'center'
        },
        {
          xtype: 'propertypanel',
          width: 360,
          title: 'Mapping Details',
          region: 'east',
          split: true
        }
      ]
    });

    me.callParent(arguments);
  }
});