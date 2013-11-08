Ext.define('AM.view.override.directory.DataGridPanel', {
  override: 'AM.view.directory.DataGridPanel',
  initComponent: function () {
    var me = this;
    var storeId = Ext.data.IdGenerator.get("uuid").generate();

    me.store = Ext.create('AM.store.DataGridStore', {
      storeId: "DataGrid" + storeId,
        listeners: {        
        beforeload: {
          fn: me.handleBeforeLoad,
          scope: me
        }
      }

    });
    
    var ptb = Ext.create('Ext.PagingToolbar', {
      pageSize: 25,
      store: me.store,
      displayInfo: true,
      displayMsg: 'Records {0} - {1} of {2}',
      emptyMsg: "No records to display"/*,
      plugins: [Ext.create('Ext.ux.plugin.GridPageSizer', { options: [25, 50, 100, 500] })]*/
    });

    var filters = {
        ftype: 'filters',
        local: false,
        remoteSort: true,
        encode: true
    };
    
    Ext.apply(me, {
      bbar: ptb,
      features: [filters]
    });
    
    me.callOverridden(arguments);
  } 
});