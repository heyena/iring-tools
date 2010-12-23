var directoryTreePane = null;
var propertyGridPane = null;
var dataPane = null;
var dtiGridPane = null;

function loadDirectoryTree() {
  directoryTreePane = new Ext.tree.TreePanel({
    region:'center',
    width: 800,
    height: 600,
    useArrows:true,
    autoScroll:true,
    animate:true,
    enableDD:true,
    containerScroll: true,
    rootVisible: true,
    frame: true,
    root: {
      nodeType: 'async',
      text: 'Directory'
    },
    
    dataUrl: 'directory?directoryURL=http://localhost:8080/services/dir/directory',
    
    listeners: { 
      click: function(node, event){ 
    	  var dataTypeNode = node.parentNode.parentNode;

        if (dataTypeNode.attributes['text'] == 'Application Data') {
          populatePropertyGrid(node.attributes.properties);
        }
        else if (dataTypeNode.attributes['text'] == 'Exchange Data') {
          populatePropertyGrid(node.attributes.properties);
        }
      },

      dblclick: function(node, event){
    	  var dataTypeNode = node.parentNode.parentNode;
        
        if (dataTypeNode.attributes['text'] == 'Application Data') {
        	var graphNode = node.parentNode;

        	populatePropertyGrid(node.attributes.properties);
          loadAppData(dataTypeNode.parentNode.attributes['text'],  graphNode.attributes['text'], node.attributes['text']);
          //dataPane = dtiGridPane;    
        } 
        else if (dataTypeNode.attributes['text'] == 'Exchange Data') {
        	populatePropertyGrid(node.attributes.properties);
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
      height: 300,
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
	alert('Loading app data: /' + scope + '/' + app + '/' + graph);  
	var store = new Ext.data.Store({
    reader: new Ext.data.JsonReader({
      fields: ['identifier', 'hashValue', 'transferType']
    }),
    proxy: new Ext.data.HttpProxy({
      url: 'dti?scopeName=' + scope + '&appName=' + app + '&graphName=' + graph
    }),
    autoLoad: true
  });
  
  dtiGridPane = new Ext.grid.GridPanel({
	  store: store,
    columns: [
      {header: "Identifier", width: 100, dataIndex: 'identifier', sortable: true},
      {header: "HashValue", width: 260, dataIndex: 'hashValue', sortable: true},
      {header: "TransferType", width: 100, dataIndex: 'transferType', sortable: true}
    ],
    renderTo: 'center',
    width: 600,
    height: 300
  });
}

function loadExchangeData(scope, exchangeId) {
	alert('Loading exchange data: /' + scope + '/exchanges/' + exchangeId);   
}

Ext.onReady(function(){
  Ext.state.Manager.setProvider(new Ext.state.CookieProvider());

  loadDirectoryTree();
  initPropertyGrid();

  var headerPane = new Ext.BoxComponent({
      region: 'north',
      height: 46,
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
          title: 'Center Panel',
          autoScroll: true
      }]
    })    
    ]
  });
   
  Ext.get("hideit").on('click', function(){
     var w = Ext.getCmp('west-panel');
     w.collapsed ? w.expand() : w.collapse();
  });
});