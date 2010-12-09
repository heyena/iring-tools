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
	
	headerList: null,
	store: null,
	dataGrid: null,
	
  /**
  * initComponent
  * @protected
  */
  initComponent: function () {
  	  	  	  	
  	this.addEvents({
      next: true,
      prev: true
    });
  	
  	this.dataGrid = new Ext.grid.GridPanel({
   	  store: this.store,
      columns: this.headerList,
      stripeRows: true,      
      loadMask: true,
      layout: 'fit',
      frame: true,
      autoSizeColumns: true,
      autoSizeGrid: true,
      AllowScroll : true,
      minColumnWidth: 100,
      columnLines: true,      
      enableColumnMove: false
  	});
  	
  	this.items = [
  		this.dataGrid
  	]  	
    
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