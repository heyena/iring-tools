Ext.define('AM.view.override.nhibernate.NhibernateTree', {
  override: 'AM.view.nhibernate.NhibernateTree',
  initComponent : function() {

  var me = this;
  
  var storeId = Ext.data.IdGenerator.get("uuid").generate();
  
  me.store = Ext.create('AM.store.NHibernateTreeStore', {
  
  storeId: "Nhibernate_" + storeId
  
  }); 
  
  me.callOverridden(arguments);
  
  }

});