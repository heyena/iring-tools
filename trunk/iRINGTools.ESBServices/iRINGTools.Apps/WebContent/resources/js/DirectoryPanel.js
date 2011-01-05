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
		}];
  },
  
  getSelectedNode: function() {
  	return this.directoryPanel.getSelectionModel().getSelectedNode();
  },
  
  openTab: function(node) {
	  
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
				label = scope + '->' + graph;
				
				this.fireEvent('open', this, node, label, requestURL);
				
			} else if (nodeType == 'graph') {
				
				//requestURL = 'dataObjects/getGraphObjects/' + nodeType + '/' + scopeId + '/' + node.parentNode.text+'/' + nodeText;
				requestURL = 'appDataGrid?scopeName=' + scope + '&appName=' + app + '&graphName=' + graph,
				label = scope + '->' + graphNode.text + '->' + graph;
				
				this.fireEvent('open', this, node, label, requestURL);				
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
  	this.openTab(node);
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
  	this.openTab(this.getSelectedNode());  	
  },
  
  onExchange: function (btn, ev) {
  	node = this.getSelectedNode();
  	if (node != null)
		this.fireEvent('exchange', this, node);
  },
  
  populatePropertyGrid: function (properties) {
		var gridSource = new Array();

		for ( var i = 0; i < properties.length; i++) {
			gridSource[properties[i].name] = properties[i].value;
		}

		this.propertyPanel.setSource(gridSource);
		
	}
});