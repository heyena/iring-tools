Ext.onReady(function () {
	
	Ext.BLANK_IMAGE_URL = 'resources/images/s.gif';
	Ext.QuickTips.init();
	
	var navigationPanel = new ExchangeManager.NavigationPanel({
		id:'navigation-panel',
		region:'west',
		
		//collapseMode: 'mini',
	  collapsible: true,
	  collapsed: false,
	  
	  border: true,
	  split: true,
	  
		width: 250,
  	minSize: 175,
  	maxSize: 500,
		navigationUrl: 'exchangereader/exchnagelist/1',		
	});
	
	var contentPanel = new Ext.TabPanel({
		region: 'center',
		id: 'content-panel',
		xtype: 'tabpanel',			
		margins: '0 5 0 0',
		enableTabScroll: true,
		defaults: {
			layout: 'fit'
		}
	});

	var viewport = new Ext.Viewport({
		layout: 'border',
		renderTo: Ext.getBody(), 
		items: [{ 
				region: 'north',
				baseCls : 'x-plain',
				height: 65, // give north and south regions a height
				margins: '-10 5 0 0',
				contentEl:'header'
			},
			navigationPanel,
			contentPanel
		]
	});	
	
});