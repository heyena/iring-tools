var directoryTreePane = null;
var propertyGridPane = null;
var dataPane = null;

function loadDirectoryTree() {
  directoryTreePane = new Ext.tree.TreePanel({
    region:'center',   
    useArrows:true,
    line:true,
    autoScroll:true,
    animate:true,
    enableDD:true,
    border: false,
    containerScroll: true, 
    split: true,
    rootVisible: true,    
    frame: true,
    root: {
      nodeType: 'async',
      text: 'Directory',
      expended: true,
      draggable: false,
      icon: 'resources/images/16x16/internet-web-browser.png'
    },
    
    tbar: [{    	  
    	icon: 'resources/images/16x16/view-refresh.png',
    	xtype: 'tbbutton',
    	tooltip: 'Refresh',
    	disabled: false,
    	handler: this.onReresh,
    	scope: this    		
    },{ 
    	text: 'Open',  
    	xtype: 'tbbutton',
    	tooltip: 'Open',
    	disabled: false,
    	handler: this.onOpen,
    	scope: this,
    	icon: 'resources/images/16x16/document-open.png' 
    },{  
    	text: 'Exchange', 
    	xtype: 'tbbutton',
    	tooltip: 'Exchange',
    	disabled: false,
    	handler: this.onExchange,
    	scope: this,
    	icon: 'resources/images/16x16/go-send.png' 
    }],  
 

    
    dataUrl: 'directory',
    
    listeners: { 
      click: function(node, event){ 
    	  var dataTypeNode = node.parentNode.parentNode;

        if (dataTypeNode.attributes['text'] == 'application data') {
          populatePropertyGrid(node.attributes['properties']);
        }
        else if (dataTypeNode.attributes['text'] == 'data exchanges') {
          populatePropertyGrid(node.attributes['properties']);
        }
      },

      dblclick: function(node, event){
    	  var dataTypeNode = node.parentNode.parentNode;
        
        if (dataTypeNode.attributes['text'] == 'application data') {
        	var graphNode = node.parentNode;

        	populatePropertyGrid(node.attributes['properties']);
          loadAppData(dataTypeNode.parentNode.attributes['text'],  graphNode.attributes['text'], node.attributes['text']);
          //dataPane = appDataPane;    
        } 
        else if (dataTypeNode.attributes['text'] == 'data exchanges') {
        	populatePropertyGrid(node.attributes['properties']);
          loadExchangeData(dataTypeNode.parentNode.attributes['text'], node.attributes['id']);
          //dataPane = exchangeDataPane;
        }
      }
    }
  });

  directoryTreePane.getRootNode().expand(false);
}

function initPropertyGrid() {
	propertyGridPane = new Ext.grid.PropertyGrid({
      id: 'propertyGridPane',
      region: 'south',
      height: 260,
      title: 'Details',
      collapsible: true,
      border: false,
      split: true,
      autoScroll: true,
      source: {}
  });	
}

function populatePropertyGrid(properties) {
	var gridSource = new Array();

	for(var i = 0; i < properties.length; i++) {
		gridSource[properties[i].name] = properties[i].value;
	}

	propertyGridPane.setSource(gridSource);
}

function loadAppData(scope, app, graph) {
	//alert('Loading app data: /' + scope + '/' + app + '/' + graph);
}

function loadExchangeData(scope, exchangeId) {
	//alert('Loading exchange data: /' + scope + '/exchanges/' + exchangeId);
  var store = new Ext.data.Store({
    reader: new Ext.data.JsonReader({
      fields: ['identifier', 'hashValue', 'transferType']
    }),
    proxy: new Ext.data.HttpProxy({
      url: 'dti'
    }),
    autoLoad: true
  });
  
  var dtiGridPane = new Ext.grid.GridPanel({
    store: store,
    columns: [
      {header: "Identifier", width: 100, dataIndex: 'identifier', sortable: true},
      {header: "HashValue", width: 260, dataIndex: 'hashValue', sortable: true},
      {header: "TransferType", width: 100, dataIndex: 'transferType', sortable: true}
    ],
    renderTo: 'data-div',
    width: 260,
    height: 300
  }); 
}

Ext.onReady(function(){
  Ext.state.Manager.setProvider(new Ext.state.CookieProvider());

  loadDirectoryTree();
  initPropertyGrid();

  var headerPane = new Ext.BoxComponent({
      region: 'north',      
      contentEl: 'header'
  });

  var viewport = new Ext.Viewport({
    layout: 'border',
    items: [
      headerPane, {
        region: 'west',
        id: 'west-panel',
        title: 'Directory',
        split: true,
        width: 260,
        minSize: 175,
        maxSize: 400,
        collapsible: true,
        margins: '0 0 0 4',
        layout: 'border',
        items: [
          directoryTreePane, 
          propertyGridPane
        ]
    },
    
    new Ext.TabPanel({
      region: 'center',
      deferredRender: false,
      activeTab: 0,
      items: [{
          contentEl: 'center',
          //title: 'Center Panel',
          autoScroll: true
      }]
    })]
  });
   
  Ext.get("hideit").on('click', function(){
     var w = Ext.getCmp('west-panel');
     w.collapsed ? w.expand() : w.collapse();
  });
});