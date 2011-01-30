Ext.ns('iringtools.org.xmgr');

function createGridStore(url){
  var store = new Ext.data.Store({
    proxy: new Ext.data.HttpProxy({
      url: url
    }),
    reader: new Ext.data.DynamicGridReader({}),
    remoteSort: false,
    listeners: {
      exception: function(ex){
        Ext.getBody().unmask();
        showDialog(400, 100, 'Error', 'Error loading data at URL: ' + ex.url, Ext.Msg.OK, null);
      }
    }
  });
  return store;
}

function createGridPane(store, pageSize){
  var gridPane = new Ext.grid.GridPanel({
    label: store.reader.label,
    description: store.reader.description,
    layout: 'fit',
    store: store,
    stripeRows: true,
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
      plugins: [new Ext.ux.plugin.PagingToolbarResizer({
        displayText: 'Page Size',
        options: [25, 50, 100, 200, 500], 
        prependCombo: true})
      ]
    })
  });
  return gridPane;
}

function createXlogsPane(label, context, xwlogsPane){
  var xlogsUrl = 'xlogs' + context;
  var xlogsStore = createGridStore(xlogsUrl);
  var pageSize = 25;
  
  xlogsStore.on('load', function(){
    xlogsStore.recordType = xlogsStore.reader.recordType;      
  
    var xlogsPane = new Ext.grid.GridPanel({
      id: 'xlogs-' + label,
      title: 'Exchange Logs',
      layout: 'fit',
      store: xlogsStore,
      stripeRows: true,
      cm: new Ext.grid.DynamicColumnModel(xlogsStore),
      selModel: new Ext.grid.RowSelectionModel({ singleSelect: true }),
      enableColLock: true,
      viewConfig: {
        forceFit: true
      },
      bbar: new Ext.PagingToolbar({
        store: xlogsStore,
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
    
    xwlogsPane.add(xlogsPane);
    xwlogsPane.doLayout();
  });
  
  xlogsStore.load({
    params: {
      start: 0,          
      limit: pageSize
    }
  });
}

function loadPageDto(type, action, context, label){
  var tab = Ext.getCmp('content-pane').getItem('tab-' + label);
  
  if (tab != null){
    tab.show();
  }
  else { 
    Ext.getBody().mask("Loading...", "x-mask-loading");    
    
    var url = action + context;
    var store = createGridStore(url);
    var pageSize = 25; 
    
    store.on('load', function(){
      store.recordType = store.reader.recordType;      
      
      var dtoBcPane = new Ext.Container({
        id: 'bc-' + label,
        cls: 'bc-container',
        padding: '5',
        bodyStyle: 'background-color:#fcfcfc',
        items: [{
          xtype: 'box',
          autoEl: {tag: 'span', html: '<a class="breadcrumb" href="#" onclick="navigate(0)">' + 
            store.reader.description + '</a>'}
        }]
      });
      
      var dtoNavPane = new Ext.Panel({
        id: 'nav-' + label,
        region: 'north',
        layout: 'hbox',
        height: 26,
        items: [dtoBcPane]
      });
      
      if (type == 'exchange'){
        var dtoToolbar = new Ext.Toolbar({
          cls: 'nav-toolbar',
          items: [{
            id: 'tb-exchange',
            xtype: 'button',
            tooltip: 'Exchange',
            icon: 'resources/images/exchange.png',
            handler: function(){
              alert('exchange');
            }
          },{
            xtype: 'tbspacer', 
            width: 4
          },{
            id: 'tb-xlog',
            xtype: 'button',
            tooltip: 'Show/hide exchange logs',
            icon: 'resources/images/exchange-log.png',
            handler: function(){            
              var dtoTab = Ext.getCmp('content-pane').getActiveTab();
              var xwlogsPane = dtoTab.items.map['xlogsw-' + label]; 
              
              if (xwlogsPane.items.length == 0){
                createXlogsPane(label, context, xwlogsPane);
              }
              else {
                if (xwlogsPane.collapsed)
                  xwlogsPane.expand(true);
                else {         
                  xwlogsPane.collapse(true);
                }
              }
            }
          },{
            xtype: 'tbspacer', 
            width: 4
          }]
        });
        
        dtoNavPane.insert(0, dtoToolbar);
      }
      
      var dtoContentPane = new Ext.Panel({
        id: 'dto-' + label,
        region: 'center',
        layout: 'card',
        border: false,
        activeItem: 0,
        items: [createGridPane(store, pageSize)],
        listeners: {
          afterlayout: function(pane){
            Ext.getBody().unmask();
          }
        }
      });
      
      var dtoTab = new Ext.Panel({
        id: 'tab-' + label,
        title: label,
        type: type,
        context: context,
        layout: 'border',
        closable: true,
        items: [dtoNavPane, dtoContentPane]
      });
      
      if (type == 'exchange'){
        var xwlogsPane = new Ext.Panel({
          id: 'xlogsw-' + label,
          region: 'south',
          layout: 'fit',
          border: false,
          height: 250,
          split: true,
          collapsed: true
        });
        
        dtoTab.add(xwlogsPane);
      }
      
      Ext.getCmp('content-pane').add(dtoTab).show();
    });
    
    store.load({
      params: {
        start: 0,          
        limit: pageSize
      }
    });
  }
}

function loadRelatedItem(type, context, individual, classId, className, classIdentifier){
  Ext.getBody().mask("Loading...", "x-mask-loading");
  var url = context + '&individual=' + individual + '&classId=' + classId + '&classIdentifier=' + classIdentifier;
  
  if (type == 'app'){
    url = 'radata' + url;
  }
  else {
    url = 'rxdata' + url;
  }
  
  var store = createGridStore(url);
  var pageSize = 25; 
  
  store.on('load', function(){
    var dtoTab = Ext.getCmp('content-pane').getActiveTab();
    var label = dtoTab.id.substring(4);
    var dtoBcPane = dtoTab.items.map['nav-' + label].items.map['bc-' + label];
    
    // remove old bc and content pane on refresh
    var lastBcItem = dtoBcPane.items.items[dtoBcPane.items.length-1].autoEl.html;
    if (removeHTMLTag(lastBcItem) == className){
      navigate(dtoBcPane.items.length - 3);
    }
    
    var dtoContentPane = dtoTab.items.map['dto-' + label];    
    var bcItemIndex = dtoBcPane.items.length + 1;        
    
    dtoBcPane.add({
      xtype: 'box',
      autoEl: {tag: 'img', src: 'resources/images/breadcrumb.png'},
      cls: 'breadcrumb-img'
    },{
      xtype: 'box',
      autoEl: {tag: 'span', html: '<a class="breadcrumb" href="#" onclick="navigate(' + 
        bcItemIndex + ')">' + className + '</a>'}
    });    
    dtoBcPane.doLayout();
    
    store.recordType = store.reader.recordType;  
    dtoContentPane.add(createGridPane(store, pageSize));
    dtoContentPane.getLayout().setActiveItem(dtoContentPane.items.length-1);
  });
  
  store.load({
    params: {
      start: 0,          
      limit: pageSize
    }
  });
}

function removeHTMLTag(htmlText){
  return htmlText.replace(/<\/?[^>]+(>|$)/g, '');
}

function showIndividualInfo(individual, relatedClasses){
  var dtoTab = Ext.getCmp('content-pane').getActiveTab();
  var label = dtoTab.id.substring(4);
  var dtoBcPane = dtoTab.items.map['nav-' + label].items.map['bc-' + label];
  var dtoContentPane = dtoTab.items.map['dto-' + label];
  var dtoGrid = dtoContentPane.getLayout().activeItem;
  
  var classItemPane = new Ext.Panel({
    region: 'north',
    layout: 'fit',
    height: 46,
    bodyStyle: 'background-color:#eef',
    html: '<div style="width:60px;float:left"><img style="margin:2px 15px 2px 5px" src="resources/images/class-badge-large.png"/></div>' +
          '<div style="width:100%;height:100%;padding-top:8px">' + dtoGrid.label + '<br/>' + dtoGrid.description + '</div>'
  });
  
  var rowData = dtoGrid.selModel.selections.map[dtoGrid.selModel.last].data;
  delete rowData['&nbsp;'];  // remove info field
  
  var parsedRowData = {};
  for (var colData in rowData)
    parsedRowData[colData] = removeHTMLTag(rowData[colData]);
  
  var propertyGrid = new Ext.grid.PropertyGrid({
    region: 'west',
    title: 'Properties',
    width: 500,
    split: true,
    stripeRows: true,
    autoScroll: true,
    source: parsedRowData,
    listeners: {
      beforeedit: function(e){
        e.cancel = true;
      }
    }
  });
  
  var relatedItemPane = new Ext.Panel({
    title: 'Related Items',
    region: 'center',
    layout: 'vbox',
    boxMinWidth: 100,
    padding: '4',
    split: true,
    autoScroll: true
  });
  
  for (var i = 0; i < relatedClasses.length; i++) {
    var dtoTabType = dtoTab.type;
    var dtoTabContext = dtoTab.context;
    var dtoIdentifier = individual;
    var relatedClassId = relatedClasses[i].id;
    var relatedClassName = relatedClasses[i].name;
    var relatedClassIdentifier = relatedClasses[i].identifier;
    
    relatedItemPane.add({
      xtype: 'box',
      autoEl: {tag: 'div', html: '<a class="breadcrumb" href="#" onclick="loadRelatedItem(\'' + 
        dtoTabType + '\',\'' + dtoTabContext + '\',\'' + dtoIdentifier + '\',\'' + relatedClassId + 
        '\',\'' + relatedClassName + '\',\'' + relatedClassIdentifier + '\')">' + relatedClassName + '</a>'}
    });
  }
  
  var individualInfoPane = new Ext.Panel({
    autoWidth: true,
    layout: 'border',
    border: false,
    items: [classItemPane, propertyGrid, relatedItemPane]
  });
  
  var bcItemIndex = dtoBcPane.items.length + 1;
  
  dtoBcPane.add({
    xtype: 'box',
    autoEl: {tag: 'img', src: 'resources/images/breadcrumb.png'},
    cls: 'breadcrumb-img'
  },{
    xtype: 'box',
    autoEl: {tag: 'span', html: '<a class="breadcrumb" href="#" onclick="navigate(' + 
      bcItemIndex + ')">' + individual + '</a>'}
  });
  dtoBcPane.doLayout();

  dtoContentPane.add(individualInfoPane);
  dtoContentPane.getLayout().setActiveItem(dtoContentPane.items.length-1);
}

function navigate(bcItemIndex){
  var dtoTab = Ext.getCmp('content-pane').getActiveTab();
  var label = dtoTab.id.substring(4);
  var dtoBcPane = dtoTab.items.map['nav-' + label].items.map['bc-' + label];
  var dtoContentPane = dtoTab.items.map['dto-' + label];  
  
  // remove items on the right from nav pane
  while (bcItemIndex < dtoBcPane.items.items.length - 1){
    dtoBcPane.items.items[bcItemIndex + 1].destroy();
  }
  
  // remove items on the right from dto content pane
  var contentItemIndex = bcItemIndex / 2;
  while (contentItemIndex < dtoContentPane.items.items.length - 1){
    dtoContentPane.items.items[contentItemIndex + 1].destroy();
  }
  dtoContentPane.getLayout().setActiveItem(contentItemIndex);
}

function submitExchange(userResponse) {
  var exchange = this[0];
  var scope = this[1];
  var xid = this[2];
  var reviewed = this[3];
  
  if (userResponse == 'ok'){
    Ext.Ajax.request({
      url: 'xsubmit?scope=' + scope + '&xid=' + xid + '&reviewed=' + reviewed,
      timeout: 600000,  // in milliseconds default 3000 
      success: function(response, opts) {
        var responseText = Ext.util.JSON.decode(response.responseText);
        showDialog(400, 160, 'Exchange Result', responseText, Ext.Msg.OK, null);
      },
      failure: function(response, opts) {
        var responseText = Ext.util.JSON.decode(response.responseText);
        showDialog(660, 300, 'Exchange Error (' + response.status + ')', 
          'Error while exchanging [' + exchange + ']. ' + responseText, Ext.Msg.OK, null);
      }
    });
  }
}

function showDialog(width, height, title, message, buttons, callback){
  var style = 'style="width:' + width + 'px;height:' + height + 'px;border:0;overflow:auto"';
  
  Ext.Msg.show({
    title: title,
    msg: '<textarea ' + style + ' readonly="yes">' + message + '</textarea>',
    buttons: buttons,
    fn: callback
  });
}

Ext.onReady(function(){
  Ext.state.Manager.setProvider(new Ext.state.CookieProvider());
  Ext.QuickTips.init();
  
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
  
  var headerPane = new Ext.BoxComponent({
    region: 'north',
    height: 46,
    cls: 'blue-fade',
    contentEl: 'header'
  });
  
  var directoryTreePane = new Ext.tree.TreePanel({
    id: 'directory-tree',
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
        id: 'refresh-button',
        xtype: 'button',
        icon: 'resources/images/refresh.png',
        text: 'Refresh',
        handler: function(){
          var directoryTree = Ext.getCmp('directory-tree');
          directoryTree.getLoader().load(directoryTree.root);
          directoryTree.getRootNode().expand(false);
        }
      },{
        id: 'exchange-button',
        xtype: 'button',
        icon: 'resources/images/exchange.png',
        text: 'Exchange',
        disabled: true,
        handler: function(){
          var node = Ext.getCmp('directory-tree').getSelectionModel().getSelectedNode(); 
          var scope = node.parentNode.parentNode.parentNode.attributes['text'];
          var exchange = node.attributes["text"];
          var xid = node.attributes.properties['Id'];
          var reviewed = (node.reviewed != undefined);   
          var msg = 'Are you sure you want to do data exchange [' + exchange + ']?';
          var processUserResponse = submitExchange.createDelegate([exchange, scope, xid, reviewed]);          
          showDialog(400, 60, 'Submit Exchange?', msg, Ext.Msg.OKCANCEL, processUserResponse); 
        }
      },{
        id: 'xlogs-button',
        xtype: 'button',
        icon: 'resources/images/exchange-log.png',
        text: 'XLogs',
        disabled: true,
        hidden: true,
        handler: function(){  
          alert('Show exchange log');
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
        
        try {
          var dataTypeNode = node.parentNode.parentNode;
          
          if (dataTypeNode != null && dataTypeNode.attributes['text'] == 'Data Exchanges'){
            Ext.getCmp('exchange-button').enable();
            Ext.getCmp('xlogs-button').enable();
          }
          else {
            Ext.getCmp('exchange-button').disable();
            Ext.getCmp('xlogs-button').disable();
          }
        }
        catch (err){}
      },
      dblclick: function(node, event){
        var properties = node.attributes.properties;
        Ext.getCmp('property-pane').setSource(node.attributes.properties);
        
        try {
          var dataTypeNode = node.parentNode.parentNode;
          
          if (dataTypeNode != null){          
            if (dataTypeNode.attributes['text'] == 'Application Data'){
              var graphNode = node.parentNode;
              var scope = dataTypeNode.parentNode.attributes['text'];
              var app = graphNode.attributes['text'];
              var graph = node.attributes['text'];
              var label = scope + '.' + app + '.' + graph;
              var context = '?scope=' + scope + '&app=' + app + '&graph=' + graph;
              
              loadPageDto('app', 'adata', context, label);
            }
            else if (dataTypeNode.attributes['text'] == 'Data Exchanges'){
              var scope = dataTypeNode.parentNode.attributes['text'];
              var exchangeId = properties['Id'];
              var context = '?scope=' + scope + '&xid=' + exchangeId;
              
              node.reviewed = true;
              loadPageDto('exchange', 'xdata', context, node.text);
            }
          }
        }
        catch (err){}
      }
    }
  });
  
  var propertyPane = new Ext.grid.PropertyGrid({
    id: 'property-pane',
    title: 'Details',
    region: 'south',
    height: 250,
    collapsible: true,
    stripeRows: true,
    autoScroll: true,
    border: false,
    split: true,
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
});
