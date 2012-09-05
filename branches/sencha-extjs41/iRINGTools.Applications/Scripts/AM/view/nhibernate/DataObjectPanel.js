
Ext.define('AM.view.nhibernate.DataObjectPanel', {
  extend: 'Ext.panel.Panel',
  alias: 'widget.dataobjectpanel',
  layout: 'border',
  frame: false,
  closable: true,
  border: false,
  contextName: null,
  endpoint: null,
  baseUrl: null,
  items: [],
  initComponent: function () {
    this.callParent(arguments);
  }
});

