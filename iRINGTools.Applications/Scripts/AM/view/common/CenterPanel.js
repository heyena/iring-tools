Ext.define('AM.view.common.CenterPanel', {
  extend: 'Ext.panel.Panel',
  alias: 'widget.centerpanel',

  requires: [
    'AM.view.common.ContentPanel',
    'AM.view.search.SearchPanel'
  ],

  border: true,
  layout: {
    type: 'border'
  },

  initComponent: function() {
    var me = this;

    Ext.applyIf(me, {
      items: [
        {
          xtype: 'contentpanel',
          id: 'maincontent',
          region: 'center'
        },
        {
          xtype: 'searchpanel',
          region: 'south',
          split: true
        }
      ]
    });

    me.callParent(arguments);
  }

});