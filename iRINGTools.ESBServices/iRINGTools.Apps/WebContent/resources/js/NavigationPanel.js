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
	
  /**
  * initComponent
  * @protected
  */
  initComponent: function () {
  	  	  	  	
  	this.addEvents({
      next: true,
      prev: true
    });
  	
  	// build the header first
  	// send the request to generate the arraystore
		var proxy = new Ext.data.HttpProxy({
			api: {
				read: new Ext.data.Connection({ url: globalReq, method: 'POST', timeout: 120000 }),
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
    	//autoLoad:true,
      proxy: proxy,
      reader: reader,
      //sortInfo: { field: 'slno', direction: "DESC" },
      autoLoad: {
      	params: {
      		start:0, 
      		limit:pageSize,
      		identifier:identifier,
      		refClassIdentifier:refClassIdentifier
      	}
      },
      baseParams: {
        'identifier':identifier,
        'refClassIdentifier':refClassIdentifier
      }
    });
  	
  	this.items = [{
  	}]
    
    this.tbar = this.buildToolbar();
        
    // super
    ExchangeManager.NavigationPanel.superclass.initComponent.call(this);
    
  },

  buildToolbar: function () {
    return [{
			id: "card-next",
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