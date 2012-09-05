Ext.define('AM.view.override.nhibernate.MultiSelectionGrid', {
  override: 'AM.view.nhibernate.MultiSelectionGrid',
  
  initComponent : function() {
    var me = this;
    var storeId = Ext.data.IdGenerator.get("uuid").generate();
      
    me.store = Ext.create('AM.store.MultiStore', {
      storeId: "Multi_" + storeId
    });         
    
    me.callOverridden(arguments);
  }
  
});