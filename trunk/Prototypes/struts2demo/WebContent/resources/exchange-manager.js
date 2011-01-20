Ext.ns('exchange-manager');

function loadExchangeData(scope, exchangeId) {
  var label = scope + '.' + exchangeId;
  var url = 'xdata?scope=' + scope + '&exchangeId=' + exchangeId;
  loadDtoPage(label, url);
}

function loadAppData(scope, app, graph) {
  var label = scope + '.' + app + '.' + graph;
  var url = 'adata?scope=' + scope + '&app=' + app + '&graph=' + graph;
  loadDtoPage(label, url);
}

function loadDtoPage(label, url) {
  var tab = Ext.getCmp('content-pane').getItem(label);
  
  if (tab != null) {
    tab.show();
  }
  else {  
    Ext.getBody().mask("Loading...", "x-mask-loading");
    
    var store = new Ext.data.Store({
      proxy: new Ext.data.HttpProxy({
        url: url
      }),
      reader: new Ext.data.DynamicGridReader({}),
      remoteSort: false
    });
    
    var pageSize = 25;      
    
    store.on('load', function() {
      store.recordType = store.reader.recordType;
      store.fields = store.recordType.prototype.fields;
      
      var dtoNavPane = new Ext.Panel({
        id: 'nav_' + label,
        region: 'north',
        height: 26,
        padding: '5',
        bodyStyle: 'background-color:#eef',
        items: [{
          xtype: 'box',
          autoEl: {tag: 'a', href: 'javascript:alert(\'loadRelatedItem\')', html: 'PIPING NETWORK SYSTEM'},
          cls: 'breadcrumb',
          overCls: 'breadcrumb-hover'
        },{
          xtype: 'box',
          autoEl: {tag: 'img', src: 'resources/images/breadcrumb.png'},
          cls: 'breadcrumb-img'
          //autoEl: {tag: 'span', html: '&raquo;'}
        },{
          xtype: 'box',
          autoEl: {tag: 'a', href: 'javascript:alert(\'loadRelatedItem\')', html: '66015-O'},
          cls: 'breadcrumb',
          overCls: 'breadcrumb-hover'
        },{
          xtype: 'box',
          autoEl: {tag: 'img', src: 'resources/images/breadcrumb.png'},
          cls: 'breadcrumb-img'
        },{
          xtype: 'box',
          autoEl: {tag: 'a', href: 'javascript:alert(\'loadRelatedItem\')', html: 'PLANT AREA'},
          cls: 'breadcrumb',
          overCls: 'breadcrumb-hover'            
        }]
      });

      //TODO: wrap dto grid panes with 'card' layout panel to show only one grid at a time
      var dtoGridPane = new Ext.grid.GridPanel({
        region: 'center',
        store: store,
        cm: new Ext.grid.DynamicColumnModel(store),
        selModel: new Ext.grid.RowSelectionModel({ singleSelect: true }),
        enableColLock: true,
        viewConfig: {
          forceFit: true
        },
        bbar: new Ext.PagingToolbar({
          store: store,
          pageSize: pageSize,
          displayInfo: true,
          autoScroll: true,
          plugins : [new Ext.ux.plugin.PagingToolbarResizer({
            displayText: 'Page Size',
            options: [25, 50, 100, 200, 500], 
            prependCombo: true})
          ]
        })
      });
      
      //TODO: call load populate grid data
      var dtoLogPane = new Ext.grid.GridPanel({
        id: 'log_' + label,
        title: 'Exchange Logs',
        region: 'south',
        store: store,
        height: 300,
        split: true,
        hidden: true,
        cm: new Ext.grid.DynamicColumnModel(store),
        selModel: new Ext.grid.RowSelectionModel({ singleSelect: true }),
        enableColLock: true,
        viewConfig: {
          forceFit: true
        },
        bbar: new Ext.PagingToolbar({
          store: store,
          pageSize: pageSize,
          displayInfo: true,
          autoScroll: true,
          plugins : [new Ext.ux.plugin.PagingToolbarResizer({
            displayText: 'Page Size',
            options: [25, 50, 100, 200, 500], 
            prependCombo: true})
          ]
        })
      });
      
      var dtoContentPane = new Ext.Panel({
        id: label,
        title: label,
        layout: 'border',
        closable: true,
        items: [dtoNavPane, dtoGridPane, dtoLogPane]
      });
      
      Ext.getCmp('content-pane').add(dtoContentPane).show();
      Ext.getBody().unmask();
    });
    
    store.load({
      params: {
        start: 0,          
        limit: pageSize
      }
    });
  }
}

function processResult(answer) {
  alert(answer);
}

function showDialog(title, message) {
  Ext.Msg.show({
    title: title,
    msg: '<textarea class="dialog-textbox" readonly="yes">' + message + '</textarea>',
    buttons: Ext.Msg.OKCANCEL,
    fn: processResult
  });
}

Ext.onReady(function() {
  Ext.state.Manager.setProvider(new Ext.state.CookieProvider());
  
  var headerPane = new Ext.BoxComponent({
    region: 'north',
    height: 46,
    cls: 'blue-fade',
    contentEl: 'header'
  });
  
  var directoryTreePane = new Ext.tree.TreePanel({
    region: 'center',
    dataUrl: 'directory',
    width: 800,
    lines : true,
    autoScroll: true,
    animate: true,
    enableDD: false,
    containerScroll: true,
    rootVisible: true,
    tbar: new Ext.Toolbar({
      items: [{
        xtype: "button",
        icon: 'resources/images/refresh.png',
        text: 'Refresh',
        handler: function(){
          alert('Refresh');
        }
      },{
        xtype: "button",
        icon: 'resources/images/exchange.png',
        text: 'Exchange',
        handler: function(){
          alert('Exchange');
        }
      },{
        xtype: 'button',
        icon: 'resources/images/exchange-log.png',
        text: 'Logs',
        handler: function() {  
          var logPane = Ext.getCmp('log_12345_000.ABC.LINES');
          
          if (logPane.hidden == true) { 
            alert('show log');
            //logPane.show();
            logPane.setVisible(true);
          }
          else {
            alert('collapse log');
            //logPane.collapse();
            logPane.collapse(true);
          }
        }
      }]
    }),
    root: {
      nodeType: 'async',  // only load child nodes needed
      text: 'Directory',
      icon: 'resources/images/directory.png'
    },
    listeners: {
      click: function(node, event) {
        Ext.getCmp('properties-pane').setSource(node.attributes.properties);
      },
      dblclick: function(node, event) {
        var dataTypeNode = node.parentNode.parentNode;
        var properties = node.attributes.properties;
        Ext.getCmp('properties-pane').setSource(node.attributes.properties);
        
        if (dataTypeNode != null) {          
          if (dataTypeNode.attributes['text'] == 'Application Data') {
            var graphNode = node.parentNode;
            loadAppData(dataTypeNode.parentNode.attributes['text'], graphNode.attributes['text'], node.attributes['text']);
          }
          else if (dataTypeNode.attributes['text'] == 'Exchange Data') {
            loadExchangeData(dataTypeNode.parentNode.attributes['text'], properties['Id']);
          }
        }
      }
    }
  });
  
  var propertiesPane = new Ext.grid.PropertyGrid({
    id: 'properties-pane',
    region: 'south',
    height: 300,
    title: 'Details',
    collapsible: true,
    border: false,
    split: true,
    autoScroll: true,
    source: {},
    listeners: {
      beforeedit : function(e) {
        e.cancel = true;
      }
    }
  });
  
  var directoryPane = new Ext.Panel({
    region: 'west',
    id: 'west-panel',
    title: 'Directory',
    frame: true,
    split: true,
    width: 260,
    minSize: 175,
    maxSize: 400,
    collapsible: true,
    margins: '0 0 0 4',
    layout: 'border',
    items: [ directoryTreePane, propertiesPane ]
  });
  
  var contentPane = new Ext.TabPanel({
    id: 'content-pane',
    region: 'center',
    deferredRender: false,
    enableTabScroll: true,
    activeTab: 0
  });

  var viewport = new Ext.Viewport({
    layout: 'border',
    items: [ headerPane, directoryPane, contentPane]
  });
  
  directoryTreePane.getRootNode().expand(false);
  
  Ext.get('about-link').on('click', function() {
    var win = new Ext.Window({    
      title: 'About Exchange Manager',
      bodyStyle: 'background-color:white;padding:5px',
      width: 700,
      height: 500,
      closable: true,
      resizable: false,
      autoScroll: false,                
      buttons: [{
        text: 'Close',
        handler: function(){
          Ext.getBody().unmask();
          win.close();
        }
      }],
      autoLoad: 'about.html',
      listeners: {
        close:{
          fn: function(){
            Ext.getBody().unmask();
          }
        }
      }
    });
    
    Ext.getBody().mask();    
    win.show();
  });
});
