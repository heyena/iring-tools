Ext.ns('ExchangeManager');
/**
* @class ExchangeManager.NavigationPanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
ExchangeManager.NavigationPanel = Ext.extend(Ext.Panel, {
  title: 'Navigation',
  layout: 'card',
  
  /**
  * initComponent
  * @protected
  */
  initComponent: function () {
  	  	
  	this.addEvents({
      next: true,
      prev: true
    });
    
    this.items = [{
    	id: 'card-0',
    	html: '<h1>Welcome to the Demo Wizard!</h1><p>Step 1 of 3</p><p>Please click the "Next" button to continue...</p>'
    },{
    	id: 'card-1',
    	html: '<p>Step 2 of 3</p><p>Almost there. Please click the "Next" button to continue...</p>'
    },{
    	id: 'card-2',
    	html: '<h1>Congratulations!</h1><p>Step 3 of 3 - Complete</p>'
    }];
    
    //this.getLayout().setActiveItem(0);
    
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
			handler: this.onNext,
			scope: this
		},{
			id: "card-prev",
			xtype:"tbbutton",
			tooltip:'Crum 2',
			text:'2...',			
			disabled: false,
			handler: this.onPrev,
			scope: this
		}]
  },
  
  onNext: function (btn, ev) {
  	var l = this.getLayout();
  	var i = l.activeItem.id.split('card-')[1]; 
  	var next = parseInt(i, 10) + 1;
  	this.setActiveItem(next);
  	Ext.getCmp('card-prev').setDisabled(next==0);
  	Ext.getCmp('card-next').setDisabled(next==2);
  	
    this.fireEvent('next', this, index);
  },
  
  onPrev: function (btn, ev) {
  	var l = this.getLayout();
  	var i = l.activeItem.id.split('card-')[1]; 
  	var next = parseInt(i, 10) - 1;
  	this.setActiveItem(next);
  	Ext.getCmp('card-prev').setDisabled(next==0);
  	Ext.getCmp('card-next').setDisabled(next==2);
  	
    this.fireEvent('prev', this, index);
  }

});