Ext.define('AM.view.override.nhibernate.RelationPropertyGrid', {
  override: 'AM.view.nhibernate.RelationPropertyGrid',
  initComponent : function() {

    var me = this;
    
    var storeId = Ext.data.IdGenerator.get("uuid").generate();
    
    me.store = Ext.create('AM.store.RelationStore', {
    
    storeId: "NhibernateRelation_" + storeId
    
    }); 
    
    me.callOverridden(arguments);
  
  }
  
});