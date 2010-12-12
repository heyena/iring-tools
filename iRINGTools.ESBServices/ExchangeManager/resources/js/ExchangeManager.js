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
	  url: 'exchangereader/exchangelist/1',		
	});
	
	var contentPanel = new Ext.TabPanel({
		region: 'center',
		id: 'content-panel',
		xtype: 'tabpanel',			
		margins: '0 5 0 0',
		enableTabScroll: true
	});
	
	directoryPanel.on('open', function(panel, node, label, url) {	

		Ext.Ajax.request({
			url: url,
			method: 'POST',
			params: {},
			success: function(result, request) {
				var obj = node.attributes;
				var scopeId = obj['Scope'];
				var nodeType = obj['node_type'];
				var nodeText = obj['text'];
				var uid = obj['uid'];
				var pageURL = null;
				
				if ((nodeType == 'exchanges' && uid != '')) {

					//pageURL = 'dataObjects/getPageData/' + nodeType + '/' + scopeId + '/' + uid + '/' + identifier + '/' + refClassIdentifier
					pageURL = 'dataObjects/getPageData/' + nodeType + '/' + scopeId + '/' + uid

				} else if (nodeType == 'graph') {					
					pageURL = 'dataObjects/getPageData/'+ nodeType + '/' + scopeId + '/' + node.parentNode.text + '/' + nodeText;
				}
				
				var responseData = Ext.util.JSON.decode(result.responseText);
				
				if (eval(responseData.success)==false) {		
					
					Ext.MessageBox.show({
						title: '<font color=red>Error</font>',
						msg: 'No Exchange Results found for:<br/>'+label,
						buttons: Ext.MessageBox.OK,
						icon: Ext.MessageBox.ERROR
					});
					
					return false;
					
				} else {
					
					var newTab = new ExchangeManager.NavigationPanel({		
						title: label,					
						configData: responseData,
						url: pageURL,						
						closable: true
					});
					
					contentPanel.add(newTab);
					contentPanel.activate(newTab);
				}
				
			},
			failure: function ( result, request) { 
				//alert(result.responseText); 
			}
		});
		
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
			directoryPanel,
			contentPanel
		]
	});	
	
});