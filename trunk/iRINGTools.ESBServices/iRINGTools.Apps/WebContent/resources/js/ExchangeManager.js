/*
exchnageData_rows.json
appData_rows_json.json
*/
Ext.onReady(function () {
	Ext.BLANK_IMAGE_URL = 'resources/images/s.gif';
	Ext.QuickTips.init();
	
	Ext.state.Manager.setProvider(new Ext.state.CookieProvider());
  
	var directoryPanel = new ExchangeManager.DirectoryPanel({
	  id:'navigation-panel',
	  region:'west',
	  collapsible: false,
	  collapsed: false,
	  border: true,
	  split: true,
	  width: 250,
  	  minSize: 175,
  	  maxSize: 500,
	  url: 'directory'
	});
	
	var contentPanel = new Ext.TabPanel({
		region: 'center',
		id: 'content-panel',
		xtype: 'tabpanel',			
		margins: '0 5 0 0',
		enableTabScroll: true
	});
	
	directoryPanel.on('open', function(panel, node, label, url) {

		
		if(contentPanel.get('tab_'+label)==undefined){
			//contentPanel
			//var w = Ext.getCmp(contentPanel).getActiveTab();
			contentPanel.getEl().mask('<span><img src="resources/js/ext-js/resources/images/default/grid/loading.gif"/> Loading.....</span>')
				//alert(url);
			var dataTypeNode = node.parentNode.parentNode;
				var obj = node.attributes;
				var item = obj['properties'];
				var scopeId = dataTypeNode.parentNode.attributes['text'];
				var nodeType = obj['iconCls'];
				var nodeText = obj['text'];
				var uid = item[0].value;
				
				var commodity = node.parentNode.text;
				var pageURL = null;
				
			Ext.Ajax.request({
			url: url,
			method: 'GET',
			params: {},
			success: function(result, request) {
				contentPanel.getEl().unmask()
				
				
				if ((nodeType == 'exchange' && uid != '')) {
					
					//pageURL = 'dataObjects/getPageData/' + nodeType + '/' + scopeId + '/' + uid
					// static pageURL ="exchnageData_rows.json";
					// exchDataRows?scopeName=12345_000&idName=1
					pageURL = 'exchDataRows?scopeName=' + scopeId + '&idName=' + uid;
					//alert("Exchange DataRows URI: "+pageURL);

				} else if (nodeType == 'graph') {
					var appName = commodity;
					
					//pageURL = 'dataObjects/getPageData/'+ nodeType + '/' + scopeId + '/' + node.parentNode.text + '/' + nodeText;
					//pageURL ="appData_rows_json.json";
					pageURL = 'appDataRows?scopeName=' + scopeId + '&appName=' + appName + '&graphName=' + nodeText;
					//alert("Application DataRows URI: "+pageURL);
				}
				
				var responseData = Ext.util.JSON.decode(result.responseText);

				//alert(pageURL)
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
						id:'tab_'+label,
						configData: responseData,
						url: pageURL,						
						closable: true,
						nodeDisplay: commodity,
						scopeName:scopeId,
						idName:uid,
						appName:appName,
						graphName:nodeText,
						nodeType:nodeType
					});
					
					contentPanel.add(newTab);
					contentPanel.activate(newTab);

					newTab.on('beforeclose',function(newTab) {
					var deleteReqURL=null
					if ((nodeType == 'exchanges' && uid != '')) {
					    //deleteReqURL = 'dataObjects/deleteDataObjects/'+nodeType+'/'+scopeId+'/'+uid
						deleteReqURL = 'cleanExchDataRows';
					} else if (nodeType == 'graph') {
						//deleteReqURL = 'dataObjects/deleteGraphObjects/'+nodeType+'/'+scopeId+'/'+node.parentNode.text+'/'+nodeText
						deleteReqURL = 'cleanAppDataRows';
					}
					if(deleteReqURL!=null){
						Ext.Ajax.request({
						url: deleteReqURL,
						method: 'GET',
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
		}else{
			contentPanel.setActiveTab('tab_'+label);
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
			directoryPanel,
			contentPanel
		]
	});	
	
});