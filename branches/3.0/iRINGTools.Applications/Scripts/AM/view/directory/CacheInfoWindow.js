
Ext.define('AM.view.directory.CacheInfoWindow', {
  extend: 'Ext.window.Window',
  alias: 'widget.cacheinfowindow',

  requires: [
    'AM.view.directory.CacheInfoPanel'
  ],

  border: false,
  height: 242,
  width: 460,
  layout: {
    type: 'fit'
  },
  bodyPadding: 1,
  //iconCls: 'tabsScope',
  modal: true,

  initComponent: function() {
    var me = this;
    me.addEvents(
      'save',
      'reset'
    );

    Ext.applyIf(me, {
      items: [
        {
          xtype: 'cacheinfopanel',
          height: 168,
          width: 448
        }
      ]
    });

    me.callParent(arguments);
  }

});