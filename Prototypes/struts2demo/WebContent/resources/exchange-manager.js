Ext.ns('exchange-manager');

function loadExchangeData(label, scope, exchangeId){
  var url = 'xdata?scope=' + scope + '&exchangeId=' + exchangeId;
  loadDtoPage(label, url);
}

function loadAppData(scope, app, graph){
  var label = scope + '.' + app + '.' + graph;
  var url = 'adata?scope=' + scope + '&app=' + app + '&graph=' + graph;
  loadDtoPage(label, url);
}

function loadDtoPage(label, url){
  var tab = Ext.getCmp('content-pane').getItem(label);
  
  if (tab != null){
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
    
    store.on('load', function(){
      store.recordType = store.reader.recordType;
      
      var classObject = store.reader.type;
      var columnModel = new Ext.grid.DynamicColumnModel(store);
      
      var dtoNavPane = new Ext.Panel({
        id: 'nav-' + label,
        region: 'north',
        height: 26,
        padding: '5',
        bodyStyle: 'background-color:#fcfcff',
        items: [/*{
          xtype: 'box',
          autoEl: {tag: 'a', href: 'javascript:alert(\'loadRelatedItem\')', html: 'PIPING NETWORK SYSTEM'},
          cls: 'breadcrumb',
          overCls: 'breadcrumb-hover'
        },{
          xtype: 'box',
          autoEl: {tag: 'img', src: 'resources/images/breadcrumb.png'},
          cls: 'breadcrumb-img'
        },{
          xtype: 'box',
          autoEl: {tag: 'a', href: 'javascript:alert(\'loadRelatedItem\')', html: '66015-O'},
          cls: 'breadcrumb',
          overCls: 'breadcrumb-hover'
        },{
          xtype: 'box',
          autoEl: {tag: 'img', src: 'resources/images/breadcrumb.png'},
          cls: 'breadcrumb-img'
        },*/{
          xtype: 'box',
          autoEl: {tag: 'a', href: 'javascript:alert(\'show this tab and remove items on the right\')', html: classObject},
          cls: 'breadcrumb',
          overCls: 'breadcrumb-hover'            
        }]
      });

      var dtoGridPane = new Ext.grid.GridPanel({
        id: 'grid-' + label,
        store: store,
        cm: columnModel,
        properties: store.reader.properties,
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
          plugins: [new Ext.ux.plugin.PagingToolbarResizer({
            displayText: 'Page Size',
            options: [25, 50, 100, 200, 500], 
            prependCombo: true})
          ]
        })
      });
      
      var dtoContentPane = new Ext.Panel({
        id: 'content-' + label,
        region: 'center',
        layout: 'card',
        activeItem: 0,
        items: [dtoGridPane]
      });
      
      var dtoLogPane = new Ext.grid.GridPanel({
        id: 'log-' + label,
        title: 'Exchange Logs',
        region: 'south',
        store: store,
        height: 300,
        split: true,
        hidden: true,
        cm: columnModel,
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
          plugins: [new Ext.ux.plugin.PagingToolbarResizer({
            displayText: 'Page Size',
            options: [25, 50, 100, 200, 500], 
            prependCombo: true})
          ]
        })
      });
      
      var dtoTab = new Ext.Panel({
        id: 'tab-' + label,
        title: label,
        layout: 'border',
        closable: true,
        items: [dtoNavPane, dtoContentPane, dtoLogPane]
      });
      
      Ext.getCmp('content-pane').add(dtoTab).show();
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

function showIndividualInfo(className, classId, individual){
  var dtoTab = Ext.getCmp('content-pane').getActiveTab();
  var label = dtoTab.id.substring(4);
  var dtoNavPane = dtoTab.items.map['nav-' + label];
  var dtoContentPane = dtoTab.items.map['content-' + label];
  var dtoGrid = dtoContentPane.items.map['grid-' + label];  
  
  var highlightPane = new Ext.Panel({
    region: 'north',
    height: 50,
    html: '<div style="background-color:#eee; float:left; width:60px"><img src="resources/images/class-badge-large.png"/></div><div style="background-color:#eee; width:100%; height:100%; padding-top:10px;"><b>'
        + classId + '</b><br/>' + className + '</div>'
  });
  
  var rowData = dtoGrid.selModel.selections.map[dtoGrid.selModel.last].data;
  delete rowData['&nbsp;'];  // remove info field
  
  var propertyGrid = new Ext.grid.PropertyGrid({
    region: 'west',
    title: 'Properties',
    height: 50,
    split: true,
    layout: 'fit',
    autoScroll: true,
    source: rowData,
    listeners: {
      beforeedit: function(e){
        e.cancel = true;
      }
    }
  });
  
  var store = new Ext.data.JsonStore({
    data: dtoGrid.properties,
    fields: [ 'RelatedClassId', 'RelatedClassName' ]
  });
  
  var listView = new Ext.list.ListView({
    region: 'center',
    layout: 'fit',
    store: store,
    hideHeaders: true,
    singleSelect: true,
    reserveScrollOffset: true,
    split: true,
    width: 300,
    columns: [{
      dataIndex: 'RelatedClassName'
    }, {
      dataIndex: 'RelatedClassId',
      hidden: true
    }]
  });
  
  var individualInfoPane = new Ext.Panel({
    autoWidth: true,
    layout: 'border',
    items: [highlightPane, propertyGrid, listView]
  });

  dtoContentPane.add(individualInfoPane);
  dtoContentPane.getLayout().setActiveItem(dtoContentPane.items.length-1);
  
  dtoNavPane.add({
    xtype: 'box',
    autoEl: {tag: 'img', src: 'resources/images/breadcrumb.png'},
    cls: 'breadcrumb-img'
  }/*,{
    xtype: 'box',
    autoEl: {tag: 'a', href: 'javascript:alert(\'loadRelatedItem\')', html: className},
    cls: 'breadcrumb',
    overCls: 'breadcrumb-hover'
  }*/);
  
  dtoNavPane.getLayout().setActiveItem(dtoNavPane.items.length-1);
}

function showDialog(title, message, processResult){
  Ext.Msg.show({
    title: title,
    msg: '<textarea class="dialog-textbox" readonly="yes">' + message + '</textarea>',
    buttons: Ext.Msg.OKCANCEL,
    fn: processResult
  });
}

Ext.onReady(function(){
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
    lines: true,
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
        text: 'XLogs',
        handler: function(){  
          var logPane = Ext.getCmp('log_12345_000.ABC.LINES');
          
          if (logPane.hidden == true){ 
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
      click: function(node, event){
        Ext.getCmp('property-pane').setSource(node.attributes.properties);
      },
      dblclick: function(node, event){
        var dataTypeNode = node.parentNode.parentNode;
        var properties = node.attributes.properties;
        Ext.getCmp('property-pane').setSource(node.attributes.properties);
        
        if (dataTypeNode != null){          
          if (dataTypeNode.attributes['text'] == 'Application Data'){
            var graphNode = node.parentNode;
            loadAppData(dataTypeNode.parentNode.attributes['text'], graphNode.attributes['text'], node.attributes['text']);
          }
          else if (dataTypeNode.attributes['text'] == 'Data Exchanges'){
            loadExchangeData(node.text, dataTypeNode.parentNode.attributes['text'], properties['Id']);
          }
        }
      }
    }
  });
  
  var propertyPane = new Ext.grid.PropertyGrid({
    id: 'property-pane',
    region: 'south',
    height: 300,
    title: 'Details',
    collapsible: true,
    border: false,
    split: true,
    autoScroll: true,
    source: {},
    listeners: {
      beforeedit: function(e){
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
    items: [ directoryTreePane, propertyPane ]
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
  
  Ext.get('about-link').on('click', function(){
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
