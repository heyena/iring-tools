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
	  url: 'exchangereader/exchangelist/1'	
	});
	
	var contentPanel = new Ext.TabPanel({
		region: 'center',
		id: 'content-panel',
		xtype: 'tabpanel',			
		margins: '0 5 0 0',
		enableTabScroll: true
	});
	
	directoryPanel.on('open', function(panel, node, label, url) {
		alert('getting grid definition: ' + url);
		
		
		var dataTypeNode = node.parentNode.parentNode;
		var graphNode = node.parentNode;
		var obj = node.attributes;
		var item = node.attributes['items'];
		var uid = item[0].value;
		var scope = dataTypeNode.parentNode.attributes['text'];
		var app = graphNode.attributes['text'];
		var nodeType = obj['iconCls'];
		var graph = obj['text']; 
		var pageURL = null;
		
		
		
		Ext.Ajax.request({
			url: url,
			method: 'GET',
			
			
			//params: {},
			success: function(result, request) {
				//alert('grid definition: ' + result.responseText);
				/*var dataTypeNode = node.parentNode.parentNode;
				var graphNode = node.parentNode;
				var obj = node.attributes;
				var item = node.attributes['items'];
				var uid = item[0].value;
				var scope = dataTypeNode.parentNode.attributes['text'];
				var app = graphNode.attributes['text'];
				var nodeType = obj['iconCls'];
				var graph = obj['text']; 
				var pageURL = null;*/
				
				if ((nodeType == 'exchange' && uid != '')) {

					//pageURL = 'dataObjects/getPageData/' + nodeType + '/' + scopeId + '/' + uid + '/' + identifier + '/' + refClassIdentifier
					//pageURL = 'dataObjects/getPageData/' + nodType + '/' + scopeId + '/' + uid
					pageURL = 'exchDataRows?scopeName=' + scope + '&idName=' + uid;
					alert("pageURL: /" + pageURL);
				
				} else if (nodeType == 'graph') {
					//pageURL = 'dataObjects/getPageData/'+ nodeType + '/' + scopeId + '/' + node.parentNode.text + '/' + nodeText;
					pageURL = 'appDataRows?scopeName=' + scope + '&appName=' + app + '&graphName=' + graph;
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

					newTab.on('beforeclose',function(newTab) {
					var deleteReqURL=null;
					if ((nodeType == 'exchange' && uid != '')) {
					    deleteReqURL = 'cleanExchDataRows';
					} else if (nodeType == 'graph') {
						//deleteReqURL = 'dataObjects/deleteGraphObjects/'+nodeType+'/'+scopeId+'/'+node.parentNode.text+'/'+nodeText
						deleteReqURL = 'cleanAppDataRows';
						//pageURL = 'appDataRows?scopeName=' + scope + '&appName=' + app + '&graphName=' + graph;
					}
					if(deleteReqURL!=null){
						Ext.Ajax.request({
						url: deleteReqURL,
						method: 'POST',
						params: {},
						success: function(result, request) {
								//console.log('delete response for ' +url+' : '+ eval(Ext.util.JSON.decode(result.responseText).success));
						}});
					}
				});
				}},
			failure: function ( result, request) { 
				alert('error: ' + result.responseText); 
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