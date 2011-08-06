Ext.ns('ExchangeManager');
/**
* @class ExchangeManager.DataPanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
ExchangeManager.DataPanel = Ext.extend(Ext.Panel, {
  title: 'Data',
  
  layout: 'fit',
  
  /**
  * initComponent
  * @protected
  */
  initComponent: function () {

    this.addEvents({
      click: true   	  
    });
    
    // super
    ExchangeManager.DataPanel.superclass.initComponent.call(this);
  },

  buildToolbar: function () {
    return [{
			xtype:"tbbutton",
			icon:'resources/images/16x16/view-refresh.png',
			tooltip:'Refresh',
			disabled: false,
			handler: this.onRefresh,
			scope: this
		},{xtype:"tbbutton",
			icon:'resources/images/16x16/go-send.png',
			tooltip:'Exchange',
			text:'Exchange',
			disabled: false,
			handler: this.onExchange,
			scope: this
		},{		
			xtype:"tbbutton",
			text:'History',
			icon:'resources/images/16x16/edit-find.png',
			tooltip:'History',
			disabled: false,
			handler: this.onHistory,
			scope: this
		}]
  },
    
  onRefresh: function (btn, ev) {  	
  	Ext.state.Manager.clear('navigation-state');    
		this.navigationPanel.root.reload();  	
  },
  
  onExchange: function (btn, ev) {
    this.fireEvent('exchange', this);
  },
  
  onHistory: function (btn, ev) {  	
    this.fireEvent('open', this, this.getSelectedNode());
  }

});