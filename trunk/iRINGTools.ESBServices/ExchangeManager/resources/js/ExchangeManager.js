Ext.onReady(function () {

	var pageSize = 20;
	
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
			params: {
				limit: pageSize				
			},
			success: function(result, request) {
				
				var obj = node.attributes;
				var scopeId = obj['Scope'];
				var nodeType = obj['node_type'];
				var nodeText = obj['text'];
				var uid = obj['uid'];
				
				var pageURL = null;
				
				if ((nodeType == 'exchanges' && uid != '')) {
					pageURL = 'dataObjects/getPageData/' + nodeType + '/' + scopeId + '/' + uid + '/' + identifier + '/' + refClassIdentifier					
				} else if (nodeType == 'graph') {					
					pageURL = 'dataObjects/getPageData/'+ nodeType + '/' + scopeId + '/' + node.parentNode.text + '/' + nodeText;
				}
				
				var jsonData = Ext.util.JSON.decode(result.responseText);
				
				if (eval(jsonData.success)==false) {		
					
					Ext.getCmp('centerPanel').disable();
					
					Ext.MessageBox.show({
						title: '<font color=red>Error</font>',
						msg: 'No Exchange Results found for:<br/>'+label,
						buttons: Ext.MessageBox.OK,
						icon: Ext.MessageBox.ERROR
					});
					
					return false;					
				}
							  
				var relatedClassArr = new Array();
				
				var rowData = eval(jsonData.rowData);
				var fieldList = eval(jsonData.headersList);
				var headerList = eval(jsonData.columnsData);
				var classObjName = jsonData.classObjName;
				var filterSet = eval(jsonData.filterSet);
				
				// for this demo configure local and remote urls for demo purposes
				var url = {				
					local:  '',  // static data file
					remote: pageURL
				};

				// configure whether filter query is encoded or not (initially)
				var encode = true;
				// configure whether filtering is performed locally or remotely (initially)
				var local = false;
				// configure whether sorting is performed locally or remotely (initially)
				var remotesort = true;
				
				//var filt = filterSet;// 	[{type: 'string',dataIndex: 'IdentificationByTag'},			{type: 'string',dataIndex: 'TransferType',disabled: false}];
				var filters = new Ext.ux.grid.GridFilters({
					// encode and local configuration options defined previously for easier reuse
					encode: encode, // json encode the filter query
					remotesort: remotesort, // json encode the filter query
					local: local,   // defaults to false (remote filtering)
					filters: filterSet
				});

				if (jsonData.relatedClasses != undefined) {			
					for(var i=0; i < jsonData.relatedClasses.length; i++) {
						var key = jsonData.relatedClasses[i].identifier;
						var text = jsonData.relatedClasses[i].text;
						relatedClassArr[i] = text;
					}
				}
				
				// build the header first
		  	// send the request to generate the arraystore
				var proxy = new Ext.data.HttpProxy({
					api: {
						read: new Ext.data.Connection({ url: pageURL, method: 'POST', timeout: 120000 }),
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
		      remoteSort: remotesort,
		      reader: reader,
		      sortInfo: { field: 'TransferType', direction: "ASC" },
		      autoLoad: {
		      	params: {
		      		start:0, 
		      		limit:pageSize/*,
		      		identifier:identifier,
		      		refClassIdentifier:refClassIdentifier
		      		*/
		      	}
		      },
		      baseParams: {
		      	/*
		        'identifier':identifier,
		        'refClassIdentifier':refClassIdentifier
		        */
		      }
		    });
				
				var newTab = new ExchangeManager.NavigationPanel({		
					title: label,
					headerList: headerList,
					store: store,
					closable: true
				});
				
				store.load();
				
				contentPanel.add(newTab);
				contentPanel.activate(newTab);
				
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