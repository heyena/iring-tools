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
      next: true   	  
    });
    
    this.tbar = this.buildToolbar();
        
    // super
    ExchangeManager.NavigationPanel.superclass.initComponent.call(this);
  },

  buildToolbar: function () {
    return [{
			xtype:"tbbutton",
			tooltip:'Crum 1',
			text:'1...',			
			disabled: false,
			handler: this.onNext,
			scope: this
		},{
			xtype:"tbbutton",
			tooltip:'Crum 2',
			text:'2...',
			disabled: false,
			handler: this.onNext,
			scope: this
		},{		
			xtype:"tbbutton",
			tooltip:'Crum 3',
			text:'3...',			
			disabled: false,
			handler: this.onNext,
			scope: this
		}]
  },
  
  onExchange: function (btn, ev) {
  	var index = 0;
    this.fireEvent('next', this, index);
  }

});