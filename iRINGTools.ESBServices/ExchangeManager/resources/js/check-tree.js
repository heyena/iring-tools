/*
 * @File Name : check-tree.js
 * @Path : resources/js
 * @Using Lib : Ext JS Library 3.2.1(lib/ext-3.2.1)
 * 
 * This file intended to make Directory Tree Panel of Exchange Manager
 * The data of tree nodes comes form the src/controller/ExchangeReader/ExchangeReader.class.php
 * in the form of JSON String
 * 
 */
var reviewed,tree
var dtoIdentifierVal,refClassIdentifierVal;   
var relatedClassArr=new Array();
// configure no of records to display per page
pageSize= 20

function showgrid(response, request,label,nodeid,gridType) {
	var identifier = 0;
  var refClassIdentifier = 0;
  
  if (gridType=='relatedClass') {
  	identifier = dtoIdentifierVal;
  	refClassIdentifier = refClassIdentifierVal;
  }
  
  globalTreenode = tree.getSelectionModel().getSelectedNode();
  
	if (globalTreenode!=null) {
		var obj = globalTreenode.attributes;
		var scopeId = obj['Scope'];
		var nodeType = obj['node_type'];
		
		if ((obj['node_type']=='exchanges' && obj['uid']!='')) {
			globalReq = 'dataObjects/getPageData/'+nodeType+'/'+scopeId+'/'+obj['uid']+'/'+identifier+'/'+refClassIdentifier
			//globalLabel = scopeId+'->'+globalTreenode.text
		} else if (obj['node_type']=='graph') {
			globalLabel = scopeId+'->'+globalTreenode.parentNode.text+'->'+obj['text']
			globalReq = 'dataObjects/getPageData/'+nodeType+'/'+scopeId+'/'+globalTreenode.parentNode.text+'/'+obj['text']
		}
	}
	
	//makeLablenURIS('get');
	reviewed = true
	var jsonData = Ext.util.JSON.decode(response);
	
	if (eval(jsonData.success)==false) {		
		Ext.getCmp('centerPanel').disable();
		Ext.MessageBox.show({
			title: '<font color=red>Error</font>',
			msg: 'No Exchange Results found for:<br/>'+label,
			buttons: Ext.MessageBox.OK,
			icon: Ext.MessageBox.ERROR
		});
		return false;		
	} else {		
		var rowData = eval(jsonData.rowData);
		var fieldList = eval(jsonData.headersList);
		var headerList = eval(jsonData.columnsData);
		var classObjName = jsonData.classObjName;
		var filterSet = eval(jsonData.filterSet);

		// for this demo configure local and remote urls for demo purposes
		var url = {				
			local:  '',  // static data file
			remote: globalReq
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

		if (jsonData.relatedClasses!=undefined) {			
			for(var i=0;i<jsonData.relatedClasses.length;i++) {
				var key = jsonData.relatedClasses[i].identifier;
				var text = jsonData.relatedClasses[i].text;
				relatedClassArr[i]=text;
			}
		}

	
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
      fields:fieldList
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
      		limit:pageSize,
      		identifier:identifier,
      		refClassIdentifier:refClassIdentifier
      	}
      },
      baseParams: {
      	//'getData': 'true'
        'identifier':identifier,
        'refClassIdentifier':refClassIdentifier
      }
    });

	// custom renderer function that will be called for each header
	function applyStyle(val){
		switch(val.toLowerCase())
		{
			case "add":
				spanColor='red';
				break;
			case "change":
				spanColor='blue';
				break;
			case "delete":
				spanColor='green';
				break;
			case "sync":
				spanColor='black';
				break;
			default:
				spanColor='black';
		}

			return '<span style="color:'+spanColor+';">' + val + '</span>';
	}


	var grid = new Ext.grid.GridPanel({
   	  store: store,
      columns: headerList,
      stripeRows: true,
      id:label,
      loadMask: true,
      plugins: [filters],
      layout:'fit',
      frame:true,
      autoSizeColumns: true,
      autoSizeGrid: true,
      AllowScroll : true,
      minColumnWidth:100,
      columnLines: true,
      classObjName: classObjName,
      enableColumnMove: false,
      listeners: {
		beforerender: {
		fn: function(){
				var colmodel = this.getColumnModel();
				for(var i=0; i<colmodel.getColumnCount();i++){
					colmodel.setRenderer(i,applyStyle);
				}
		}
		},		 
      	render: {
      		fn: function(){
      			store.load({
      			params: {
      			start: 0,
                limit: pageSize
              }
            });
          }
        },
        cellclick:{
        	fn: function(grid,rowIndex,columnIndex,e) {
        		var cm = this.colModel
				var record = grid.getStore().getAt(rowIndex);  // Get the Record
        		var fieldName = grid.getColumnModel().getDataIndex(columnIndex); // Get field name
            
        		if (fieldName=='Identifier' && record.get(fieldName)!='') {
        			var IdentificationByTag_value = record.get(fieldName);
        			var transferType_value = record.get('TransferType');
              var rowDataArr = [];
              for(var i=3; i<cm.getColumnCount();i++){
              	fieldHeader= grid.getColumnModel().getColumnHeader(i); // Get field name
              	fieldValue= record.get(grid.getColumnModel().getDataIndex(i))
              	tempArr= Array(fieldHeader,fieldValue)
              	rowDataArr.push(tempArr)
              }
              var filedsVal_ = '[{"name":"Property"},{"name":"Value"}]';
              var columnsData_ ='[{"id":"Property","header":"Property","width":144,"sortable":"true","dataIndex":"Property"},{"id":"Value","header":"Value","width":144,"sortable":"true","dataIndex":"Value"}]';
              var prowData = eval(rowDataArr);
              var pfiledsVal = eval(filedsVal_);
              var pColumnData = eval(columnsData_);
              // create the data store
              var pStore = new Ext.data.ArrayStore({
              	fields: pfiledsVal
              });
              
              pStore.loadData(prowData);
              showIndvidualClass(pStore,pColumnData,rowIndex);
              Ext.get('identifier-class-detail').dom.innerHTML = '<div style="float:left; width:110px;"><img src="resources/images/class-badge.png"/></div><div style="padding-top:20px;" id="identifier"><b>'+removeHTMLTags(IdentificationByTag_value)+'</b><br/>'+grid.classObjName+'<br/>Transfer Type : '+transferType_value+'</div>'
        		}
        	}
        },
        beforeclose: function(){
        	if (gridType != 'relatedClass') {
        		makeLablenURIS('delete')
        		sendCloseRequest(globalReq,globalLabel);
          } // send request for delete cache
        }
      },
      tbar: new Ext.Toolbar({
      	xtype: "toolbar",
        items: [{
        	xtype: "tbbutton",
        	id: 'gridReload'+label,
        	icon: 'resources/images/16x16/view-refresh.png',
        	tooltip: 'Reload',
        	disabled: false,
        	handler: function() {
        		makeLablenURIS('get');
        		showCentralGrid(globalTreenode);
            // send Request to destroy session
          }
        },{
        	xtype: "tbbutton",
        	id: 'gridExchange'+label,
        	text: 'Exchange',
        	icon: 'resources/images/16x16/go-send.png',
        	tooltip: 'Exchange',
        	disabled: false,
        		handler: function() {
        			makeLablenURI();
        			submitDataExchange(globalReq);
            }
        },{
        	xtype: "tbbutton",
        	id: 'gridHistory'+label,
        	text: 'History',
        	icon: 'resources/images/16x16/edit-find.png',
        	tooltip: 'History',
        	disabled: false,
        	handler: function() {
        		displayHistoryPanel();
        	}
        }]
      }),
      bbar: new Ext.PagingToolbar({
      	pageSize: pageSize,
      	store: store,
      	displayInfo: true,
      	autoScroll: true,
      	plugins: [filters]
      })
    });

    // add some buttons to bottom toolbar just for demonstration purposes
    grid.getBottomToolbar().add([
      '->',
			{
				 text: 'Encode: ' + (encode ? 'On' : 'Off'),
				 tooltip: 'Toggle Filter encoding on/off',
				 enableToggle: true,
				 handler: function (button, state) {
					 var encode = (grid.filters.encode === true) ? false : true;
					 var text = 'Encode: ' + (encode ? 'On' : 'Off'); 
					 //remove the prior parameters from the last load options
					 grid.filters.cleanParams(grid.getStore().lastOptions.params);
					 grid.filters.encode = encode;
					 button.setText(text);
					 grid.getStore().reload();
				 }
			 },{
				 text: 'Local Filtering: ' + (local ? 'On' : 'Off'),
				 tooltip: 'Toggle Filtering between remote/local',
				 enableToggle: true,
				 handler: function (button, state) {
					 var local = (grid.filters.local === true) ? false : true;
					 var text = 'Local Filtering: ' + (local ? 'On' : 'Off');
					 var newUrl = local ? url.local : url.remote;
			
					 // update the GridFilter setting
					 grid.filters.local = local;
					 //bind the store again so GridFilters is listening to appropriate store event
					 grid.filters.bindStore(grid.getStore());
					 // update the url for the proxy
					 grid.getStore().proxy.setApi('read', newUrl);
					 button.setText(text);
					 grid.getStore().reload();
				 }
			 },{
				text: 'Local Sorting: ' + (store.remoteSort ? 'On' : 'Off'),
				tooltip: 'Toggle Sorting on/off',
				enableToggle: true,
				handler: function (button, state) {

						  //alert(store.remoteSort);
						  var localsort = (store.remoteSort===true) ? true : false;
						  var remotesort = (store.remoteSort===true) ? false : true;
						  var text = 'Local Sorting: ' + (localsort ? 'Off' : 'On');
						  store.remoteSort=remotesort;
						  //remove the prior parameters from the last load options
						  //grid.filters.cleanParams(grid.getStore().lastOptions.params);
						  button.setText(text);
						  grid.getStore().reload();
			 }
			 },{
				 text: 'All Filter Data',
				 tooltip: 'Get Filter Data for Grid',
				 handler: function () {
					 var data = Ext.encode(grid.filters.getFilterData());
					 Ext.Msg.alert('All Filter Data',data);
				 }
			 },{
				 text: 'Clear Filter Data',
				 handler: function () {
					 grid.filters.clearFilters();
				 }
			 }
				/*
				{                                 
					text: 'Reconfigure Grid',
					handler: function () {
						//grid.reconfigure(store, createColModel(6));
					} 
				}
				*/
		]);

    history_panel = new Ext.Panel({
    	id: 'hst-tab-'+label,
      title: 'History',
      split: true,
      layout :'fit',
      collapsible: true,
      collapsed: true,
      region: 'south',
      height : '300',
      tbar: new Ext.Toolbar({
      	xtype: "toolbar",
      	items:[{
      		xtype: "tbbutton",
      		text: 'Hide History',
      		icon: 'resources/images/16x16/document-new.png',
      		tooltip:'Show Grid',
      		disabled: false,
      		handler:function() {
      			collapseHistoryPanel()
      		}
      	}]
      })
    });

    var intPanel = new Ext.Panel({
    	layout: 'border',
    	split: true,
    	autoScroll:true,
    	containerScroll: true,
    	items:[{ 
    		region:'center', 
    		layout: 'border',
        autoScroll:true,
        containerScroll: true,
        bodyBorder:false,
        border:false,
        items: [{
        	region:'center',
        	layout :'fit',
        	height:'700',
        	boxMinHeight:'300',
        	bodyBorder:false,
        	border:false,
        	items:[grid]
        },history_panel]
    	}],
		listeners: {
		beforeclose:function(){
				if(gridType!='relatedClass'){
					makeLablenURIS('delete')
					sendCloseRequest(globalReq,globalLabel);
				} // send request for delete cache
		}}
    });
        
    if(gridType=='relatedClass'){
    	Ext.getCmp('centerPanel').add(
    			Ext.apply(grid,{  // call 'grid' rather than intPanel so that the gird will only be show in centerPanel
    				id:'tab-'+label,
    				text:nodeid,
    				title: label,
            closable:true
    			})).show();

    	Ext.getCmp('gridReload'+label).hide()
    	Ext.getCmp('gridExchange'+label).hide()
    	Ext.getCmp('gridHistory'+label).hide()

    } else {
    	Ext.getCmp('centerPanel').add(
    			Ext.apply(intPanel,{
    				id:'tab-'+label,
    				text:nodeid,
    				title: label,
    				closable:true
    			})).show();
    }
	}
}

function sendAjaxRequest(requestURL,label,nodeid,gridType){
	var w = Ext.getCmp('centerPanel').getActiveTab();
	if(w){
		w.getEl().mask('Loading.....')
	}else{
		Ext.getBody().mask('Loading...');
	}
        	
	Ext.Ajax.request({
		url : requestURL,
		method: 'POST',
		params: {
		limit: pageSize
		/*limit: node.id,
		newparentid: newParent.id,
		oldparentid: oldParent.id,
		dropindex: index
		*/
		},
		success: function(result, request)
		{
			showgrid(result.responseText,request,label,nodeid,gridType);
		},
		failure: function ( result, request){ 
			//alert(result.responseText); 
		},
		callback: function() {
			if(w){
				w.getEl().unmask()
				//alert(w.getEl())
				//alert('came here'+w)
			}else{
				Ext.getBody().unmask();
			}
		}
	})
}

Ext.onReady(function() {
	Ext.QuickTips.init();
	var tBar = new Ext.Toolbar({
		xtype: "toolbar",
		items: [{
			xtype:"tbbutton",
			icon:'resources/images/16x16/view-refresh.png',
			tooltip:'Refresh',
			id: 'headRefresh', 
			disabled: false,
			handler: function(){
				Ext.state.Manager.clear("treestate");    
				tree.root.reload();
			}
		},{
			//For open button
			xtype:"tbbutton",
			text:'Open',
			icon:'resources/images/16x16/document-open.png',
			id: 'headExchange',
			tooltip:'Open',
			disabled: false,
			handler: function() {
				showCentralGrid(tree.getSelectionModel().getSelectedNode());
			}
		},{
			xtype:"tbbutton",
			icon:'resources/images/16x16/go-send.png',
			tooltip:'Exchange',
			text:'Exchange',
			disabled: false,
			handler: function(){
				makeLablenURI();
				var treenode = globalTreenode;
				if (treenode != null){
					var label = globalLabel;
					var requestURL = globalReq;

					if(!Ext.getCmp(label)){
						promptReviewAcceptance(requestURL);
					} else {
						Ext.Msg.show({
							msg: 'Thanks for your review & acceptance. Want to transfer the data now?',
							buttons: Ext.Msg.YESNO,
							icon: Ext.Msg.QUESTION,//'profile', // &lt;- customized icon
							fn: function(action){
								if(action=='yes'){
									submitDataExchange(requestURL);
								}else if (action == 'no'){
									alert('Not now');
								}
							}
						});
					}
				} else {
					Ext.MessageBox.show({
						msg: 'Please Select an Exchange First<br/>',
						buttons: Ext.MessageBox.OK,
						icon: Ext.MessageBox.WARNING
					});
					return false;
				}
			}
		}]
	});
	
  tree = new Ext.tree.TreePanel({
  	region:'north',
  	split:true,        
    id:'directory-tree',
    //renderTo:'tree-div',
    //height: 494,
    height:300,
    bodyBorder:false,
    border:false,
    hlColor:'C3DAF',
    //width: 250,
    layout:'fit',
    useArrows:false, // true for vista like
    autoScroll:true,
    animate:true,
    margins: '0 0 0 0',
    lines :true,
    containerScroll: true,
    rootVisible: true,
    root: {
      nodeType: 'async',
      icon: 'resources/images/16x16/internet-web-browser.png',
      text: 'Directory'
    },
    // auto create TreeLoader
    dataUrl: 'ExchangeReader/exchnageList/1',
    tbar:tBar,
    listeners: {
    	BeforeLoad:{
    		fn:function(){
    			//*********** Disabled on 02 Nov **************
    			// Basic mask:
    			//Ext.getCmp('west-panel').el.mask('Loading...', 'x-mask-loading')
    		}
    	},
    	load:{
    		fn:function(){
    			//*********** Disabled on 02 Nov **************
    			// Basic unmask:
    			// Ext.getCmp('west-panel').el.unmask()
    		}
    	},
    	click: {
    		fn: function(node){
    			//get all the attributes of node
    			obj = node.attributes;
    			var details_data = [];
    			for (var key in obj) {
    				// alert(key+' '+obj[key])
    				// restrict some of the properties to be displayed
    				if (key!='node_type' && key!='uid' && key!='id' && key!='text' && key!='icon' && key!='children' && key!='loader' && key!='leaf' && key!='applicationId'){
    					details_data[key] = obj[key];
    				}
    			}
                
    			// get the property grid component
    			var propGrids = Ext.getCmp('propGrid');
    			// make sure the property grid exists
    			if (propGrids) {
    				//Ext.get('propGrid').dom.innerHTML =''
    				//propGrids.html=''
    				// populate the property grid with details_data
    				propGrids.setSource(details_data);
    			}

    			//*********** Disabled on 02 Nov **************
    			// check the current state of Detail Grid panel
    			/*
    			if(Ext.getCmp('detail-grid').collapsed==true){
						Ext.getCmp('detail-grid').expand();
					}
					*/
    		}
    	},
    	dblclick: {
    		fn: function (node){
    			showCentralGrid(node);
    		}
    	},
    	expandnode:{
    		fn : function (node){
    			Ext.state.Manager.set("treestate", node.getPath())
    		}
    	}
    }
  });

  var contextMenu = new Ext.menu.Menu({
  	items: [{
  		text: 'Sort',
  		handler: sortHandler
  	}]
  });
  
  function sortHandler() {
  	tree.getSelectionModel().getSelectedNode().sort(
  			function (leftNode, rightNode) {
  				return 1;
  			}
  	);
  }

  /* to maintain the state of the tree */
  Ext.state.Manager.setProvider(new Ext.state.CookieProvider());
  tree.on('contextmenu', function (node){
    node.select();
    contextMenu.show(node.ui.getAnchor());
  });

  /* to maintain the state of the tree */
  var treeState = Ext.state.Manager.get("treestate");
  
  if (treeState) {
  	if (tree.expandPath(treeState)) { //check the
  		tree.expandPath(treeState);
  	} else {
  		Ext.state.Manager.clear("treestate");
		  tree.root.reload();
	  }
  }
  
});

function promptReviewAcceptance(requestURL){
	Ext.Msg.show({
		msg: 'Would you like to review the <br/>Data Exchange before starting?',
		buttons: Ext.Msg.YESNO,
		icon: Ext.Msg.QUESTION,//'profile', // &lt;- customized icon
		fn: function(action){
			if(action=='yes'){
				var node = tree.getSelectionModel().getSelectedNode();
				showCentralGrid(node);
			}
			else if(action=='no')
			{
				reviewed=false;
				//alert('You clicked on No');
				submitDataExchange(requestURL);
			}
		}
	});
}

function submitDataExchange(requestURL) {	
	if (reviewed) {
		var w = Ext.getCmp('centerPanel').getActiveTab();
		w.getEl().mask('Loading.....');
	} else {
		Ext.getCmp('centerPanel').enable();
		Ext.getCmp('centerPanel').getEl().mask('Loading.....');
	}
	
	Ext.Ajax.request({
		url : requestURL,
		method: 'POST',
		params: {
			hasreviewed: reviewed
		},
		success: function(result, request) {		  
			var jsonData = Ext.util.JSON.decode(result.responseText);
		  
		  if (eval(jsonData.success)==false) {
		  	alert(jsonData.response);
		  } else if(eval(jsonData.success)==true) {
		  	makeLablenURIS('get');
		  	showCentralGrid(globalTreenode);
		  	showResultPanel(jsonData);
		  }
		},
		failure: function ( result, request){ 
			alert(result.responseText); 
		},
		callback: function() {if(w){
			w.getEl().unmask();
	  } else {
	  	Ext.getCmp('centerPanel').getEl().unmask();}
		}
	})
};

function showResultPanel(jsonData) {
	var rowData = eval(jsonData.rowData);
	var filedsVal = eval(jsonData.headersList);
	var store = new Ext.data.ArrayStore({
		fields: filedsVal
	});
	
	store.loadData(eval(rowData));
	
	var label = globalLabel;
	var columnData = eval(jsonData.columnsData);
	
	var grid = new Ext.grid.GridPanel({
		store: store,
		columns: columnData,
		stripeRows: true,
		id:'exchangeResultGrid_'+label,
		loadMask: true,
		layout:'fit',
		frame:true,
		autoSizeColumns: true,
		autoSizeGrid: true,
		AllowScroll : true,
		minColumnWidth:100, 
		columnLines: true,
		autoWidth:true,
		enableColumnMove:false
	});				
			
	if (Ext.getCmp('centerPanel').findById('tabResult-'+label)) {
		//alert('aleready exists')
		Ext.getCmp('centerPanel').remove(Ext.getCmp('centerPanel').findById('tabResult-'+label));
		Ext.getCmp('centerPanel').add( 
				Ext.apply(grid,{
					id:'tabResult-'+label,
					title: label+'(Result)',
					closable:true
				})).show();
	} else {
		Ext.getCmp('centerPanel').add( 
				Ext.apply(grid,{
					id:'tabResult-'+label,
					title: label+'(Result)',
					closable:true
				})).show();
	}
};

function showCentralGrid(node) {
	var obj = node.attributes;
	var eid;
	var label;
	var requestURL;
	var scopeId  = obj['Scope'];
	var nodeType = obj['node_type'];
	if ((obj['node_type']=='exchanges' && obj['uid']!='')) {
		eid = obj['uid'];
		requestURL = 'dataObjects/getDataObjects/'+nodeType+'/'+scopeId+'/'+eid;
		label = scopeId+'->'+node.text;
	} else if (obj['node_type']=='graph') {
		requestURL = 'dataObjects/getGraphObjects/'+nodeType+'/'+scopeId+'/'+node.parentNode.text+'/'+obj['text']
		label = scopeId+'->'+node.parentNode.text+'->'+obj['text']
	} else {
			Ext.MessageBox.show({
				//title: '<font color=yellow>Warning</font>',
				msg: 'You can review only Data Exchange & Graphs in this Version<br/>',
				buttons: Ext.MessageBox.OK,
				icon: Ext.MessageBox.WARNING
			});
			return false;
	}
	/*
	check the id of the tab
	if it's available then just display the tab & don't send ajax request
	*/

	if ((!Ext.getCmp(label))) {
		if (node.id != null) {
			Ext.getCmp('centerPanel').enable();
			sendAjaxRequest(requestURL,label,node.id);
			// check the current state of Detail Grid panel
			/*
			if (Ext.getCmp('detail-grid').collapsed != true) {
				Ext.getCmp('detail-grid').collapse();
			}
			*/
		}
	}	else if (Ext.getCmp(label)&&(Ext.getCmp('centerPanel').getActiveTab().id=='tab-'+label)) {
		// When you click on exchange button with activetab
		sendAjaxRequest(requestURL,label,node.id);
	} else {
		Ext.getCmp('centerPanel').enable();
		// collapse the detail Grid panel & show the tab
		/*
		if (Ext.getCmp('detail-grid').collapsed!=true) {
			Ext.getCmp('detail-grid').collapse();
		}
		*/
		Ext.getCmp(label).show();
	}
};

var globalLabel; 
var globalReq; 
var globalTreenode;

function makeLablenURI(){
	// setting the node id in text of during the Result Grid creation
	if(Ext.getCmp('centerPanel').getActiveTab()){
		var nodeid = Ext.getCmp('centerPanel').getActiveTab().text;
		if(nodeid){
			tree.getSelectionModel().select(tree.getNodeById(nodeid));
		}
	}
	globalTreenode = tree.getSelectionModel().getSelectedNode();
	if (globalTreenode!=null) {
		var obj = globalTreenode.attributes;
		var scopeId = obj['Scope'];
		var nodeType = obj['node_type'];

		if((obj['node_type']=='exchanges' && obj['uid']!='')){
			globalReq = 'dataObjects/setDataObjects/'+nodeType+'/'+scopeId+'/'+obj['uid']
			globalLabel = scopeId+'->'+globalTreenode.text
		}	else if (obj['node_type']=='graph') {
			globalLabel = scopeId+'->'+globalTreenode.parentNode.text+'->'+obj['text']
			globalReq = 'dataObjects/setGraphObjects/'+nodeType+'/'+scopeId+'/'+globalTreenode.parentNode.text+'/'+obj['text']
		}
	}
};

function makeLablenURIS(type){
	//alert('tab id:'+Ext.getCmp('centerPanel').getActiveTab().id)
	// setting the node id in text of during the Result Grid creation
	if(Ext.getCmp('centerPanel').getActiveTab()){
		var nodeid = Ext.getCmp('centerPanel').getActiveTab().text;
		if(nodeid){
			tree.getSelectionModel().select(tree.getNodeById(nodeid));
		}
	}
	globalTreenode = tree.getSelectionModel().getSelectedNode();
	if(globalTreenode!=null){
		var obj = globalTreenode.attributes;
		var scopeId  = obj['Scope'];
		var nodeType = obj['node_type'];
		if((obj['node_type']=='exchanges' && obj['uid']!='')){
			globalReq = 'dataObjects/'+type+'DataObjects/'+nodeType+'/'+scopeId+'/'+obj['uid']
			globalLabel = scopeId+'->'+globalTreenode.text
		} else if (obj['node_type']=='graph') {
			globalLabel = scopeId+'->'+globalTreenode.parentNode.text+'->'+obj['text']
			globalReq = 'dataObjects/'+type+'GraphObjects/'+nodeType+'/'+scopeId+'/'+globalTreenode.parentNode.text+'/'+obj['text']
		}
	}
};

function showIndvidualClass(pStore,pColumnData,rowIndex){
	var nodeid = Ext.getCmp('centerPanel').getActiveTab().text;
	if(nodeid){
		tree.getSelectionModel().select(tree.getNodeById(nodeid));
	}

	if(grid_class_properties){
		alert('Going to destroy...')
		grid_class_properties.destroy();
	}
	// create the Grid
	var grid_class_properties = new Ext.grid.GridPanel({
		store: pStore,
		columns: pColumnData,
		stripeRows: true,
		loadMask: true,
		height: 460,
		autoSizeColumns: true,
		autoSizeGrid: true,
		AllowScroll : true,
		minColumnWidth: 100,
		columnLines: true,
		enableColumnMove: false
	});

	// get the centerPanel x,y coordinates, used to set the position of Indvidual Class(PopUp window)
	var strPositon = (Ext.getCmp('centerPanel').getPosition()).toString();        
	var arrPositon = strPositon.split(",");
     
	var myWin = new Ext.Window({
		title: 'Indvidual Class',
		id:'indvidual-class',
		closable:true,
		x: arrPositon[0],
		y: parseInt(arrPositon[1])+25,
        
		width:Ext.getCmp('centerPanel').getInnerWidth()-2,
		height:Ext.getCmp('centerPanel').getInnerHeight(),
		layout: 'border',
		listeners: {
			beforerender: {
				fn : function(){
					Ext.getBody().mask();
				}
			},
			close:{
				fn:function(){
					Ext.getBody().unmask();
				}
			}
		},
		items: [{
			id:'identifier-class-detail',
			region: 'north',
			split: true,
			height:100,
			html: 'Class Detail'
		},{
			id:'identifier-class-properties',
			title: 'Properties',
			region:'west',
			split: true,
			margins: '0 1 3 3',
			width: 250,
			//height:900,
			minSize: 100,
			items:[grid_class_properties]
		},{
			title: 'Related Items',
			layout:'accordion',
			split: true,
			width: 220,
			region: 'center',
			margins: '0 3 3 0',
			defaults: {
				// applied to each contained panel
				// bodyStyle: 'margin:0 0 0 15'
			},
			layoutConfig: {
				// layout-specific configs go here
				animate: true,
				fill : false
			},
			// html: '<div class="x-panel-header x-accordion-hd" style="cursor:pointer"><a href="#" onClick="displayRleatedClassGrid(\'90-AO567\',\'90011-O\')">Plant Area</a></div><div class="x-panel-header x-accordion-hd">P AND I Diagram</div>'
			html:relatedClassArr[rowIndex]
		}]
	});
	myWin.show();
};

/* function to remove all html tags */
function removeHTMLTags(strInputCode) {
	/*
	This line is optional, it replaces escaped brackets with real ones,
  i.e. < is replaced with < and > is replaced with >
  */
	strInputCode = strInputCode.replace(/&(lt|gt);/g, function (strMatch, p1){
		return (p1 == "lt")? "<" : ">";
	});
	
	var strTagStrippedText = strInputCode.replace(/<\/?[^>]+(>|$)/g, "");
	
	return strTagStrippedText;
};

function sendCloseRequest(requestURL,label){
	Ext.Ajax.request({
		url : requestURL,
		method: 'POST',
		//params: {},
		success: function (result, request) {
			//alert(result.responseText)
		},
		failure: function (result, request) { 
			//alert(result.responseText);
		},
		callback: function() {			
		}
	})
};

// Used to display the Related Class Grid
function displayRleatedClassGrid(refClassIdentifier,dtoIdentifier,relatedClassName) {
	selTreenode = tree.getSelectionModel().getSelectedNode() 
	if (selTreenode != null) {
		var obj = selTreenode.attributes;
		var nId = obj['id'];
		var scopeId  = obj['Scope'];
		var nodeType = obj['node_type'];
		var exchangeId = obj['uid'];
	}
	refClassIdentifierVal = refClassIdentifier;
	dtoIdentifierVal = dtoIdentifier;
	requestURL = 'dataObjects/getRelatedDataObjects/exchanges/'+scopeId+'/'+exchangeId+'/'+dtoIdentifier+'/'+refClassIdentifier;
	label = relatedClassName;    
  //nodeid= tree.getSelectionModel().getSelectedNode()
  Ext.getBody().unmask();    
  Ext.getCmp('indvidual-class').close();
  sendAjaxRequest(requestURL,label,nId,'relatedClass');
}


function collapseHistoryPanel() {
	w = Ext.getCmp('centerPanel').getActiveTab()
	if (w) {
		tabId = w.id
		//Collapse the History Panel
		hstId = 'hst-'+tabId
		if(Ext.getCmp(hstId).collapsed==false){
			Ext.getCmp(hstId).collapse()
		}
	}
};

function displayHistoryPanel() {
	selTreenode = tree.getSelectionModel().getSelectedNode()

	if(selTreenode!=null){
		var obj = selTreenode.attributes;
		//var nId  = obj['id']
		var scopeId  = obj['Scope'];
		//var nodeType = obj['node_type']
		var exchangeId = obj['uid'];


		Ext.Ajax.request({
			url :  uri= 'dataObjects/getHistory/exchanges/'+scopeId+'/'+exchangeId,
			method: 'POST',
			params: {
				//hasreviewed: reviewed
			},
			success: function(result, request) {
				showHistoryGrid(result.responseText)              
			},
			failure: function ( result, request){
				alert(result.responseText);
			}
		});
	};

	tabId = Ext.getCmp('centerPanel').getActiveTab().id;
	// alert(tabId)
	hstId = 'hst-' + tabId;

  // Expand the History Panel
	if(Ext.getCmp(hstId).collapsed==true){
		Ext.getCmp(hstId).expand();
	};
};

function showHistoryGrid(response) {
	tabId = Ext.getCmp('centerPanel').getActiveTab().id;
	hstId = 'hst-'+tabId;	
	var jsonData = Ext.util.JSON.decode(response);
	
	if (eval(jsonData.success) == false) {
		Ext.MessageBox.show({
			title: '<font color=red>Error</font>',
			msg: 'No History Result found for:<br/>'+label,
			buttons: Ext.MessageBox.OK,
			icon: Ext.MessageBox.ERROR
		});
		return false;
	} else {
		var rowData = eval(jsonData.rowData);
		var fieldList = eval(jsonData.headersList);
		var columnData = eval(jsonData.columnsData);
		var statusList = jsonData.statusList;
		var historyCacheKey = jsonData.historyCacheKey;
                
    // set the ArrayStore to use in Grid
		var hstStore = new Ext.data.ArrayStore({
			fields: fieldList
		});
		
		hstStore.loadData(rowData);

		if (Ext.getCmp('hstGrid')) {
			alert('Going to destroy...')
			hstGrid.destroy();
		}

		// create the Grid
		var hstGrid = new Ext.grid.GridPanel({
			store: hstStore,
			columns: columnData,
			stripeRows: true,
			id: 'grid-' + hstId,
			loadMask: true,
			autoSizeColumns: true,
			autoSizeGrid: true,
			AllowScroll: true,
			minColumnWidth:100,
			columnLines: true,
			enableColumnMove:false,
			listeners: {
				cellclick:{
					fn:function(hstGrid,rowIndex,columnIndex,e){
						var cm    = this.colModel;
						var record = hstGrid.getStore().getAt(rowIndex);  // Get the Record
						var str_history_header='';
						hstID = record.get(hstGrid.getColumnModel().getDataIndex(0))
                                            
						for(var i=0; i<cm.getColumnCount();i++) {
							fieldHeader= hstGrid.getColumnModel().getColumnHeader(i); // Get field name
							fieldValue= record.get(hstGrid.getColumnModel().getDataIndex(i))
							if(fieldHeader != 'hstID'){
								str_history_header = str_history_header +'<b>'+fieldHeader+' : </b>'+fieldValue+ '<br/>'
							}
						}
						
						showHistoryPopup(hstID,historyCacheKey);
						Ext.get('history-header').dom.innerHTML = '<div style="padding:10 5 0 10">'+str_history_header+"</div>";
					}
				}
			}
		});

		// add 'hstGrid' as items to 'history_panel'
		history_panel.add(hstGrid);
		history_panel.doLayout();
	}
};

function showHistoryPopup(hstID,historyCacheKey){

	// Send the request to controller for get rowData form session
	Ext.Ajax.request({
		url :  uri= 'dataObjects/getHistoryStatusListData/'+historyCacheKey+'/'+hstID,
		method: 'POST',
		params: {
			//hasreviewed: reviewed
		},
		success: function(result, request) {
			var responseTxt = result.responseText
			showHistoryStatusListGrid(responseTxt)
		},
		failure: function ( result, request){
			alert(result.responseText);
		}
	});
	// get the centerPanel x,y coordinates, used to set the position of Indvidual Class(PopUp window)
	var strPositon = (Ext.getCmp('centerPanel').getPosition()).toString()
	var arrPositon=strPositon.split(",");

	history_statuslist = new Ext.Panel({
		title: 'Status List',
		region:'center',
		split: true,
		margins: '0 1 3 3',
		width: 250,
		//minSize: 100,
		height:400,
		layout :'fit'
		//collapsible: true
		//html:'Status List Grid will be displayed here'
	});

	var hstPopup = new Ext.Window({
		title: 'History Detail',
		id:'history-popup',
		closable:true,
		x : arrPositon[0],
		y : parseInt(arrPositon[1])+25,

		width:Ext.getCmp('centerPanel').getInnerWidth()-2,
		height:Ext.getCmp('centerPanel').getInnerHeight(),
		layout: 'border',
		listeners: {
			beforerender:{
				fn : function() {
					Ext.getBody().mask();
				}
			},
			close:{
				fn:function(){
					Ext.getBody().unmask();
				}
			}
		},
		items: [{
			id:'history-header',
			region: 'north',
			split: true,
			frame:true,
			height:80,
			html: 'Class Detail'
		},history_statuslist]
	});
	hstPopup.show();
};

function showHistoryStatusListGrid(response) {
	var jsonData = Ext.util.JSON.decode(response)

	if (eval(jsonData.success) == false) {
		Ext.MessageBox.show({
			title: '<font color=red>Error</font>',
			msg: 'No History Result found for:<br/>'+label,
			buttons: Ext.MessageBox.OK,
			icon: Ext.MessageBox.ERROR
		});
		return false;
	} else {
		var rowData = eval(jsonData.rowData);
		var fieldList = eval(jsonData.headersList);
		var columnData = eval(jsonData.columnsData);

		// shared reader
		var gp_reader = new Ext.data.ArrayReader({},fieldList);

		var gp_store = new Ext.data.GroupingStore({
			reader: gp_reader,
			data: rowData,
			sortInfo:{field: 'Identifier', direction: "ASC"},
			groupField:'Identifier'
		});
                

		// create the Grid
		var hstStatusListGrid = new Ext.grid.GridPanel({
			store: gp_store,
			columns: columnData,
			stripeRows: true,
			layout:'fit',
			autoSizeColumns: true,
			autoSizeGrid: true,
			AllowScroll : true,
			minColumnWidth:100,
			columnLines: true,
			enableColumnMove:false,
			//view: new Ext.grid.GroupingView()
			view: new Ext.grid.GroupingView({
				forceFit:true,
				groupTextTpl: '{text} ({[values.rs.length]} {[values.rs.length > 1 ? "Items" : "Item"]})'
			})
		});
		// add 'hstGrid' as items to 'history_panel'
		history_statuslist.add(hstStatusListGrid);
		history_statuslist.doLayout();
	}    
};