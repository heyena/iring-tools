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
      collapseMode: 'mini',
      height: 300,
      layout: 'fit',
      border: false,

      rootVisible: true,
      lines: true,
      //singleExpand: true,
      useArrows: true,
	  autoScroll:true,
      loader: new Ext.tree.TreeLoader({
        dataUrl: this.url
      }),

      root: {
        nodeType: 'async',
        text: 'Directory',
        expanded: true,
        draggable: false,
        icon: 'resources/images/16x16/internet-web-browser.png'        
      }
    
    });
    
    this.propertyPanel = new Ext.grid.PropertyGrid({
  		id:'property-panel',
  		title: 'Details',
  		region:'center',
  		layout: 'fit',
  		autoScroll:true,
  		margin:'10 0 0 0',
  		bodyStyle: 'padding-bottom:15px;background:#eee;',
  		source:{},             
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
		}]
  },
  
  getSelectedNode: function() {
  	return this.directoryPanel.getSelectionModel().getSelectedNode();
  },
  
  openTab: function(node) {
  	if (node != null) {
	  	var obj = node.attributes;  		
			var uid = obj['uid'];
			var label = '';
			var requestURL = '';
			var scopeId  = obj['Scope'];
			var nodeType = obj['node_type'];
			var nodeText = obj['text']; 
			
			if ((nodeType == 'exchanges' && uid != '')) {
				
				requestURL = 'dataObjects/getDataObjects/' + nodeType + '/' + scopeId + '/' + uid;
				label = scopeId + '->' + node.text;
				
				this.fireEvent('open', this, node, label, requestURL);
				
			} else if (nodeType == 'graph') {
				
				requestURL = 'dataObjects/getGraphObjects/' + nodeType + '/' + scopeId + '/' + node.parentNode.text+'/' + nodeText;
				label = scopeId + '->' + node.parentNode.text + '->' + nodeText;
				
				this.fireEvent('open', this, node, label, requestURL);				
			}
  	}
  },
  
  onClick: function(node) {
  	obj = node.attributes;
		var details_data = [];
		
		for (var key in obj) {
			// restrict some of the properties to be displayed
			if (key!='node_type' && key!='uid' && key!='id' && key!='text' && key!='icon' && key!='children' && key!='loader' && key!='leaf' && key!='applicationId') {
				details_data[key] = obj[key];
			}
		}
		
		this.propertyPanel.setSource(details_data);
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
  }

});