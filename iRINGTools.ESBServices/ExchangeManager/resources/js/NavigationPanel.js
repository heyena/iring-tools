Ext.ns('ExchangeManager');
/**
* @class ExchangeManager.NavigationPanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
ExchangeManager.NavigationPanel = Ext.extend(Ext.Panel, {
  title: 'Directory',
  
  layout: 'border',
  navigationUrl: null,
  
  navigationPanel: null,
  propertyPanel: null,

  /**
  * initComponent
  * @protected
  */
  initComponent: function () {

    this.addEvents({
    	click: true,
    	dblclick: true,
      refresh: true,
      open: true,
      exchange: true
    });

    this.tbar = this.buildToolbar();

    this.navigationPanel = new Ext.tree.TreePanel({
      region: 'north',
      collapseMode: 'mini',
      height: 300,
      layout: 'fit',
      border: false,

      rootVisible: true,
      lines: true,
      //singleExpand: true,
      useArrows: true,

      loader: new Ext.tree.TreeLoader({
        dataUrl: this.navigationUrl
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
      this.navigationPanel,
      this.propertyPanel
    ];
    
    this.navigationPanel.on('click', this.onClick, this);

    // super
    ExchangeManager.NavigationPanel.superclass.initComponent.call(this);
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
  	return this.navigationPanel.getSelectionModel().getSelectedNode();
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
  	this.fireEvent('dblclick', this, node);
  },
  
  onExpand: function(node) {		  	
		Ext.state.Manager.set('navigation-state', node.getPath());
		this.fireEvent('refresh', this, this.getSelectedNode());		
	},

  onRefresh: function (btn, ev) {  	
  	Ext.state.Manager.clear('navigation-state');    
		this.navigationPanel.root.reload();
  	
    this.fireEvent('refresh', this);
  },
  
  onOpen: function (btn, ev) {  	
    this.fireEvent('open', this, this.getSelectedNode());
  },
  
  onExchange: function (btn, ev) {
    this.fireEvent('exchange', this);
  }

});