Ext.define('AM.view.common.ContentPanel', {
  extend: 'Ext.tab.Panel',
  alias: 'widget.contentpanel',

  split: true,
  border: false,

  initComponent: function() {
    var me = this;

    me.callParent(arguments);
  }
});