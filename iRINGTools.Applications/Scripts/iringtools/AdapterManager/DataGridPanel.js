Ext.define('AdapterManager.DatagridPanel', {
    extend: 'Ext.grid.Panel',
    alias: 'widget.datagridpanel',
    requires: [
        'Ext.form.field.Text',
        'Ext.toolbar.TextItem'
    ],
    closable: true,
	  scope: null,
	  app: null,
	  graph: null,
    url: null,

    initComponent: function () {

		  Ext.define('Dynamic.Model', {
			  extend: 'Ext.data.Model',
			  fields: []
		  });
	
		var grid = this;
			
		this.store =  Ext.create('Ext.data.JsonStore', {
			autoLoad: false,
			pageSize: 25,
			model: 'Dynamic.Model',
			proxy: {
       type : 'ajax', 
       actionMethods: { read: 'POST' },
       url : this.url,
       extraParams: {scope: this.scope, app: this.app, graph: this.graph }
     }
	 });

    Ext.apply(this, {
      iconCls: 'icon-grid',
			itemId: 'tablegrid_'+this.scope+'.'+this.app+'.'+this.graph,
			//title: '',
//            plugins: [this.editing],
			store: this.store,
      dockedItems: [{
					xtype: 'pagingtoolbar',
					store: this.store,   // same store GridPanel is using
					dock: 'bottom',
					displayInfo: true
			}],
      columns: {
				defaults: {
					field: { xtype: 'textfield' }
				}
			}
    }),

		this.callParent();	
		
		this.store.load({
			callback : function(recs, options, success){
				grid.reconfigure(this.store,recs[0].store.proxy.reader.fields);
				grid.show();
			}
		});
    },

    onSync: function(){
        this.store.sync();
    }
});
