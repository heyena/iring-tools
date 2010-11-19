Ext.onReady(function () {
	
	Ext.BLANK_IMAGE_URL = 'resources/images/s.gif';
	Ext.QuickTips.init();
	
	Ext.state.Manager.setProvider(new Ext.state.CookieProvider());
  
	var directoryPanel = new ExchangeManager.DirectoryPanel({
		id:'navigation-panel',
		region:'west',
		
		collapsible: true,
	  collapsed: false,
	  
	  border: true,
	  split: true,
	  
		width: 250,
  	minSize: 175,
  	maxSize: 500,
		url: 'exchangereader/exchnagelist/1',		
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
	
	directoryPanel.on('open', function(panel, node, label, url) {
		
		var newTab = new ExchangeManager.NavigationPanel({
			title: label,
			layout: 'card',
			activeItem: 0,
			url: url,
			closable: true
		});
		
		contentPanel.add(newTab);
    contentPanel.activate(newTab);
		
	}, this);

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
			directoryPanel,
			contentPanel
		]
	});	
	
});