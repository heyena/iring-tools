Ext.define('AM.view.common.ContentPanel', {
  extend: 'Ext.tab.Panel',
  alias: 'widget.contentpanel',

  split: true,
  border: true,

  initComponent: function() {
    var me = this;

    me.callParent(arguments);
  }
});