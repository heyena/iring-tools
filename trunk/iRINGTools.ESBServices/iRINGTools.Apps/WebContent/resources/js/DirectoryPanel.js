Ext.ns('ExchangeManager');
/**
* @class ExchangeManager.DirectoryPanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
ExchangeManager.DirectoryPanel = Ext.extend(Ext.Panel, {
  title: 'Directory',
  layout: 'border',
  url: null,
  directoryPanel: null,
  propertyPanel: null,

  /**
  * initComponent
  * @protected
  */
  initComponent: function () {

    this.addEvents({
    	click: true,
    	refresh: true,
		open: true,
		exchange: true
    });

    this.tbar = this.buildToolbar();
    
    this.directoryPanel = new Ext.tree.TreePanel({
      region: 'north',
      split : true,
	  //collapseMode: 'mini',
	  height: 300,
	  bodyBorder : false,
	  border : false,
	  layout : 'fit',
	  useArrows : false,
	  autoScroll : true,
	  animate : true,
	  margins : '0 0 0 0',
	  lines : true,
	  containerScroll : true,
	  rootVisible : true,
	  loader: new Ext.tree.TreeLoader({dataUrl: this.url}),
	  root : {
			nodeType : 'async',
			text : 'Directory',
			expended : true,
			draggable : false,
			icon : 'resources/images/16x16/internet-web-browser.png'
		}
		//dataUrl : 'directory'
     
    
    });
    
    this.propertyPanel = new Ext.grid.PropertyGrid({
    	id : 'propertyGridPane',
		region : 'center',
		height : 160,
		title : 'Details',
		collapsible : true,
		border : false,
		split : true,
		autoScroll : true,
		source : {}, 
  		listeners: {
  			// to disable editable option of the property grid
  			beforeedit : function(e) {
  				e.cancel=true;
  			}
  		}
  	});

    this.items = [
      this.directoryPanel,
      this.propertyPanel
    ];
    
    this.directoryPanel.on('click', this.onClick, this);
    this.directoryPanel.on('dblclick', this.onDblClick, this);
    this.directoryPanel.on('expandnode', this.onExpand, this);
    
    var state = Ext.state.Manager.get("directory-state");
    
    if (state) {
    	if (this.directoryPanel.expandPath(state) == false) {    		
    		Ext.state.Manager.clear("directory-state");
    		this.directoryPanel.root.reload();
  	  }
    }

    // super
    ExchangeManager.DirectoryPanel.superclass.initComponent.call(this);
  },

  buildToolbar: function () {
    return [{
			xtype:"tbbutton",
			icon:'resources/images/16x16/view-refresh.png',
			tooltip:'Refresh',
			disabled: false,
			handler: this.onRefresh,
			scope: this
		},{		
			xtype:"tbbutton",
			text:'Open',
			icon:'resources/images/16x16/document-open.png',
			tooltip:'Open',
			disabled: false,
			handler: this.onOpen,
			scope: this
		},{xtype:"tbbutton",
			icon:'resources/images/16x16/go-send.png',
			tooltip:'Exchange',
			text:'Exchange',
			disabled: false,
			handler: this.onExchange,
			scope: this
		},{xtype:"tbbutton",
			icon:'resources/images/16x16/go-send.png',
			tooltip:'Logs',
			text:'Logs',
			disabled: false,
			handler: this.onHistory,
			scope: this
		}];
  },
  
  getSelectedNode: function() {
  	return this.directoryPanel.getSelectionModel().getSelectedNode();
  },
  
  openTab: function(node,isReloadable) {
	  var dataTypeNode = node.parentNode.parentNode;
	  var graphNode = node.parentNode;	
	  /*	loadAppData(dataTypeNode.parentNode.attributes['text'],  graphNode.attributes['text'], node.attributes['text']);
	function loadAppData(scope, app, graph)
	*/
  	if (node != null) {
	  	var obj = node.attributes;  
	  	var item = node.attributes['properties'];
			var uid = item[0].value;
			var label = '';
			var requestURL = '';
			var scope  = dataTypeNode.parentNode.attributes['text'];
			var app = graphNode.attributes['text'];
			var nodeType = obj['iconCls'];
			var graph = obj['text']; 
			if ((nodeType == 'exchange' && uid != '')) {
				
				//requestURL = 'dataObjects/getDataObjects/' + nodeType + '/' + scopeId + '/' + uid;
				requestURL = 'exchDataGrid?scopeName=' + scope + '&idName='+ uid;
				//requestURL = 'exchnageData_grid.json';
				label = scope + '->' + graph;
				this.fireEvent('open', this, node, label, requestURL,isReloadable);
				
			} else if (nodeType == 'graph') {
				//requestURL = 'dataObjects/getGraphObjects/' + nodeType + '/' + scopeId + '/' + node.parentNode.text+'/' + nodeText;
				requestURL = 'appDataGrid?scopeName=' + scope + '&appName=' + app + '&graphName=' + graph,
				//requestURL = 'appData_grid_json.json';
				label = scope + '->' + graphNode.text + '->' + graph;
				this.fireEvent('open', this, node, label, requestURL,isReloadable);
			}
  	}
  },
  
  onClick: function(node) {
	  var dataTypeNode = node.parentNode.parentNode;
		
		if (dataTypeNode.attributes['text'] == 'Application Data') {
			var graphNode = node.parentNode;							
			this.populatePropertyGrid(node.attributes['properties']);
		} else if (dataTypeNode.attributes['text'] == 'Data Exchanges') {
			this.populatePropertyGrid(node.attributes['properties']);
		}
		this.fireEvent('click', this, node);
  },
  
  onDblClick: function(node) {
  	this.openTab(node,'false');
  },
  
  onExpand: function(node) {		  	
		Ext.state.Manager.set('directory-state', node.getPath());
		this.fireEvent('refresh', this, this.getSelectedNode());		
	},

  onRefresh: function (btn, ev) {  	
  	Ext.state.Manager.clear('directory-state');    
		this.directoryPanel.root.reload();  	
  },
  
  onOpen: function (btn, ev) {
  	this.openTab(this.getSelectedNode(),'false');
  },
  
  onHistory: function (btn, ev) {
  	node = this.getSelectedNode();
	if (node != null){
	  var dataTypeNode = node.parentNode.parentNode;
		var obj = node.attributes;
		var item = obj['properties'];
		var nodeType = obj['iconCls'];
		
		if(nodeType=='exchange'){
		var scopeId = dataTypeNode.parentNode.attributes['text'];
			var nodeText = obj['text'];
			var uid = item[0].value;
			var contentPanel= Ext.getCmp('content-panel');
			var directoryPan=this;

			var tabid='Historytab_'+scopeId;
			
			if(contentPanel.get(tabid)==undefined){
				var exchangeHistoryURI='exchangeHistory?scopeName='+scopeId+'&idName='+uid;
				alert(exchangeHistoryURI)
						
				/*var exchangeHistoryURI='exchangeHistory.json?scopeName='+scopeId+'&idName='+uid;
				alert(exchangeHistoryURI)
				directoryPan.fireEvent('history', this, node,exchangeHistoryURI,scopeId,uid);*/

				
				// 
			}else{
				// condition when the tabPanel is open and user clicks the log button
				contentPanel.setActiveTab(tabid);
				var exchangeHistoryURI='exchangeHistory?scopeName='+scopeId+'&idName='+uid;

				/*var exchangeHistoryURI='exchangeHistory.json?scopeName='+scopeId+'&idName='+uid;
				this.fireEvent('history', this, node,exchangeHistoryURI,scopeId,uid);*/
			}
			
		}else{
			//alert('nodeType: '+nodeType); // graph
			Ext.Msg.show({
				title: '<font color=blue>Warning</font>',
				msg: 'Please Select an exchange node under Data Exchange:<br/>',
				buttons: Ext.MessageBox.OK,
				icon: Ext.MessageBox.ERROR
			});
			return false;
		}
	}else {
	  Ext.Msg.show({
			title: '<font color=blue>Warning</font>',
			msg: 'Please Select an exchange node under Data Exchange:<br/>',
			buttons: Ext.MessageBox.OK,
			icon: Ext.MessageBox.ERROR
		});
		return false;
	  
  }
  },
  onExchange: function (btn, ev) {
  	node = this.getSelectedNode();
	if (node != null){
	  var dataTypeNode = node.parentNode.parentNode;
		var obj = node.attributes;
		var item = obj['properties'];
		var nodeType = obj['iconCls'];
		
		if(nodeType=='exchange'){
		var scopeId = dataTypeNode.parentNode.attributes['text'];
			var nodeText = obj['text'];
			var uid = item[0].value;
			var contentPanel= Ext.getCmp('content-panel');
			var hasreviewed;
			var directoryPan=this;
			var tablabel=scopeId+'->'+nodeText;
			var tabid='tab_'+scopeId+'->'+nodeText;
			
			if(contentPanel.get(tabid)==undefined){
				// condition when the tabPanel is not open and user clicks the exchange button directly
				Ext.Msg.show({
				msg: 'Would you like to review the <br/>Data Exchange before starting?',
				buttons: Ext.Msg.YESNO,
				icon: Ext.Msg.QUESTION,//'profile', // &lt;- customized icon
				fn: function(action){
					if(action=='yes'){
					 hasreviewed=true;
					 directoryPan.openTab(node,'false');
				 }else if(action=='no'){
					 hasreviewed=false;
					 contentPanel.setActiveTab(tabid);
					 var exchangeURI='exchangeResponse?scopeName='+scopeId+'&idName='+uid+'&hasReviewed='+hasreviewed;
					 //*** var exchangeURI='exchangeResponse.json';
					 directoryPan.fireEvent('exchange', this, node,exchangeURI,tablabel);
					 //****directoryPan.openTab(node,'true');
				 }
				}
				});
				// 
			}else{
				// condition when the tabPanel is open and user clicks the exchange button
				hasreviewed=true;
				contentPanel.setActiveTab(tabid);
				var exchangeURI='exchangeResponse?scopeName='+scopeId+'&idName='+uid+'&hasReviewed='+hasreviewed;
				//alert('Original exchangeURI : '+exchangeURI);
				//*** var exchangeURI='exchangeResponse.json';
				this.fireEvent('exchange', this, node,exchangeURI,tablabel);
			}
			
		}else{
			//alert('nodeType: '+nodeType); // graph
			Ext.Msg.show({
				title: '<font color=blue>Warning</font>',
				msg: 'Please Select an exchange node under Data Exchange:<br/>',
				buttons: Ext.MessageBox.OK,
				icon: Ext.MessageBox.ERROR
			});
			return false;
		}
	}	else {
	  Ext.Msg.show({
			title: '<font color=blue>Warning</font>',
			msg: 'Please Select an exchange node under Data Exchange:<br/>',
			buttons: Ext.MessageBox.OK,
			icon: Ext.MessageBox.ERROR
		});
		return false;
	  
  }
  },
  
  populatePropertyGrid: function (properties) {
		var gridSource = new Array();

		for ( var i = 0; i < properties.length; i++) {
			gridSource[properties[i].name] = properties[i].value;
		}

		this.propertyPanel.setSource(gridSource);
		
	}
});