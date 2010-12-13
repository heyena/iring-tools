Ext.ns('ExchangeManager');
/**
* @class ExchangeManager.NavigationPanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
ExchangeManager.NavigationPanel = Ext.extend(Ext.Panel, {
	title: 'NavigationPanel',
	layout: 'card',			
	activeItem: 0,
	configData: null,
	dataGrid: null,
	url: null,	
	
  /**
  * initComponent
  * @protected
  */
  initComponent: function () {
  	  	  	  	
  	this.addEvents({
      next: true,
      prev: true
    });
  	
  	var rowData = eval(this.configData.rowData);
		var fieldList = eval(this.configData.headersList);
		var headerList = eval(this.configData.columnsData);
		var classObjName = this.configData.classObjName;
		var filterSet = eval(this.configData.filterSet);
		var pageSize = this.configData.pageSize; 
		var sortBy = this.configData.sortBy
		var sortOrder = this.configData.sortOrder
		
		var filters = new Ext.ux.grid.GridFilters({
			// encode and local configuration options defined previously for easier reuse
			encode: true, // json encode the filter query
			remotesort: true, // json encode the filter query
			local: false,   // defaults to false (remote filtering)
			filters: filterSet
		});

		var relatedClassArr = new Array();
		
		if (this.configData.relatedClasses != undefined) {			
			for(var i=0; i < this.configData.relatedClasses.length; i++) {
				var key = this.configData.relatedClasses[i].identifier;
				var text = this.configData.relatedClasses[i].text;
				relatedClassArr[i] = text;
			}
		}
		
		// build the header first
  	// send the request to generate the arraystore
		var proxy = new Ext.data.HttpProxy({
			api: {
		read: new Ext.data.Connection({ url: this.url, method: 'POST', timeout: 120000 }),
        create: null,
        update: null,
        destroy: null
      }
		});

		var reader = new Ext.data.JsonReader({
			totalProperty: 'total',
			successProperty: 'success',
			root: 'data',
			fields: fieldList
		});

	  var store = new Ext.data.Store({
      autoLoad: true,
      proxy: proxy,
      remoteSort: true,
      reader: reader,
	  sortInfo: { field: sortBy, direction: sortOrder },
      autoLoad: {
      	params: {
      		start: 0, 
      		limit: pageSize/*,
      		identifier:identifier,
      		refClassIdentifier:refClassIdentifier
      		*/
      	}
      },
      baseParams: {
      	/*
        'identifier':identifier,
        'refClassIdentifier':refClassIdentifier
        */
      }
    });
  	
  	this.dataGrid = new Ext.grid.GridPanel({
	  store: store,
      columns: headerList,
      stripeRows: true,      
      loadMask: true,
      plugins: [filters],
      layout: 'fit',
      frame:true,
      autoSizeColumns: true,
      autoSizeGrid: true,
      AllowScroll : true,
      minColumnWidth: 100,
      columnLines: true,
      classObjName: classObjName,
      enableColumnMove: false,
      bbar: new Ext.PagingToolbar({
        	pageSize: 20,
        	store: store,
        	displayInfo: true,
        	autoScroll: true,
        	plugins: [filters]
        }),
	listeners: {
	beforerender: {
		fn: function(){
				var colmodel = this.getColumnModel();
				for(var i=0; i<colmodel.getColumnCount();i++){
					colmodel.setRenderer(i,function(val){
						switch(val.toLowerCase())
						{
							case "add":
								spanColor='red';
								break;
							case "change":
								spanColor='blue';
								break;
							case "delete":
								spanColor='green';
								break;
							case "sync":
								spanColor='black';
								break;
							default:
								spanColor='black';
						}
						return '<span style="color:'+spanColor+';">' + val + '</span>';
					});
				}
			}
		}}
  	});
  	
  	this.items = [
  		this.dataGrid
  	];
    
    this.tbar = this.buildToolbar();
        
    // super
    ExchangeManager.NavigationPanel.superclass.initComponent.call(this);
    
  },

  buildToolbar: function () {
    return [{
			id: "card-1",
    	xtype:"tbbutton",
			tooltip:'Crum 1',
			text:'1...',			
			disabled: false,
			handler: this.onOpen,
			scope: this
		}]
  },
  
  onOpen: function (btn, ev) {  	  	  	
  	var l = this.getLayout();
  	var i = l.activeItem.id.split('card-')[1]; 
  	var next = parseInt(i, 10) + 1;
  	l.setActiveItem(next);
  	
  	var t = this.getTopToolbar(); 
  	
  	t.add([{
			id: "card-btn-"+i,
			xtype: "tbbutton",
			tooltip: 'Crum '+i,
			text: i+'...',		
			disabled: false,
			handler: this.onOpen,
			scope: this
		}]);
  	
  	t.doLayout();
  	
    this.fireEvent('next', this, i);
  }  
  

});