//Ext.Compat.showErrors = true;
Ext.application({
  name: 'AdapterManager'
});

Ext.onReady(function () {
  Ext.QuickTips.init();
  Ext.data.proxy.Ajax.timeout = 120000; //increase request time

  Ext.state.Manager.setProvider(Ext.create('Ext.state.CookieProvider'));

  Ext.get('about-link').on('click', function () {
    var win = Ext.create('Ext.window.Window', {
      title: 'About Adapter Manager',
      bodyStyle: 'background-color:white;padding:5px',
      width: 700,
      height: 500,
      closable: true,
      resizable: false,
      autoScroll: true,
      buttons: [{
        text: 'Close',
        handler: function () {
          Ext.getBody().unmask();
          win.close();
        }
      }],
      autoLoad: 'about.html',
      listeners: {
        close: {
          fn: function () {
            Ext.getBody().unmask();
          }
        }
      }
    });

    Ext.getBody().mask();
    win.show();
  });

  var selectedDirectoryNode = null;
  var selectedSearchNode = null;
  var selectedMappingNode = null;

  var searchPanel = Ext.create('AdapterManager.SearchPanel', {
    id: 'search-panel',
    title: 'Reference Data Search',
    collapsedTitle: 'Reference Data Search',
    region: 'south',
    height: 250,
    // border: 10,
    //collapseMode: 'mini',
    collapsible: true,
    collapsed: false,
    searchUrl: 'refdata/getnode',
    limit: 100
  });

  var contentPanel = Ext.create('Ext.tab.Panel', {
    id: 'content-panel',
    region: 'center',
    collapsible: false,   
    enableTabScroll: true,
    border: true,
    split: true
  });

  var centrePanel = Ext.create('Ext.panel.Panel', {
    id: 'centre-panel',
    region: 'center',
    layout: 'border',
    collapsible: false,
    //  closable: true,
    enableTabScroll: true,
    border: true,
    split: true,
    items: [searchPanel, contentPanel]
  });

  var directoryPanel = Ext.create('AdapterManager.DirectoryPanel', {
    id: 'nav-panel',
    title: 'Directory',
    layout: 'border',
    region: 'west',
    width: 260,
    minSize: 175,
    maxSize: 400,
    // border: 10,
    collapsible: true,
    collapsed: false,
    navigationUrl: 'directory/getnode'
  });


  directoryPanel.on('newscope', function (npanel, node) {
    var newTab = Ext.create('AdapterManager.ScopePanel', {
      id: 'tab-' + node.id,
      record: node.data.record,
      url: 'directory/scope'
    });

    newTab.on('save', function (panel) {
      win.close();
      directoryPanel.onReload(node);
      if (node.expanded == false)
        node.expand();
    }, this);

    newTab.on('Cancel', function (panel) {
      win.close();
    }, this);

    var win = Ext.create('Ext.window.Window', {
      closable: true,
      id: 'newwin-' + node.data.id,
      modal: false,
      layout: 'fit',
      title: 'Add New Scope',
      iconCls: 'tabsScope',
      height: 180,
      width: 430,
      plain: true,
      items: newTab
    });

    win.show();
  }, this);


  directoryPanel.on('editscope', function (npanel, node) {
    var newTab = Ext.create('AdapterManager.ScopePanel', {
      id: 'tab-' + node.data.id,
      record: node.data.record,
      url: 'directory/scope'
    });

    var parentNode = node.parentNode;

    newTab.on('save', function (panel) {
      win.close();
      directoryPanel.onReload(node);
      if (parentNode.expanded == false)
        parentNode.expand();
    }, this);

    newTab.on('Cancel', function (panel) {
      win.close();
    }, this);

    var win = Ext.create('Ext.window.Window', {
      closable: true,
      id: 'editwin-' + node.data.id,
      modal: false,
      layout: 'fit',
      title: 'Edit Scope \"' + node.data.text + '\"',
      iconCls: 'tabsScope',
      height: 180,
      width: 430,
      plain: true,
      items: newTab
    });

    win.show();

  }, this);


  directoryPanel.on('deletescope', function (npanel, node) { 
    Ext.Ajax.request({
      url: 'directory/deletescope',
      method: 'POST',
      params: {
        'nodeid': node.attributes.id
      },
      success: function (o) {
        directoryPanel.onReload(node);
      },
      failure: function (f, a) { 
        var message = 'Error deleting scope!';
        showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
      }
    });

    var editScopePane = Ext.getCmp('editwin-' + node.id);
    if (editScopePane)
      editScopePane.destroy(); 
  }, this);

  directoryPanel.on('editgraphname', function (npanel, node) {
    contentPanel.removeAll(true);
  }, this);

  directoryPanel.on('configure', function (npanel, node) {
    var dataLayerValue = node.data.record.DataLayer;
    var scope = node.parentNode.data.text;
    var application = node.data.text;

    if (dataLayerValue == 'SpreadsheetLibrary') {
      var newConfig = Ext.create('AdapterManager.SpreadsheetLibraryPanel', {
        id: 'tab-c.' + scope + '.' + application,
        title: 'Configure - ' + scope + '.' + application,
        scope: scope,
        application: application,
        url: 'spreadsheet/configure',
        closable: true
      }); 
      contentPanel.add(newConfig).show(); 
    }
    else if (dataLayerValue == 'NHibernateLibrary') {
      var nhConfigId = scope + '.' + application + '.-nh-config';
      var nhConfigWizard = contentPanel.items.get(nhConfigId);

      if (nhConfigWizard) {
        nhConfigWizard.show();
      }
      else {
        nhConfigWizard = Ext.create('AdapterManager.NHibernateConfigWizard', {
          scope: scope,
          app: application
        });
        contentPanel.add(nhConfigWizard).show(); 
      }
    }
  }, this);

  directoryPanel.on('newapplication', function (npanel, node) {

    var newTab = Ext.create('AdapterManager.ApplicationPanel', {
      id: 'tab-' + node.data.id,
      scope: node.data.record,
      record: null,
      url: 'directory/application'
    });

    newTab.on('save', function (panel) {
      win.close();
      directoryPanel.onReload(node);
      if (node.expanded == false)
        node.expand();
    }, this);

    newTab.on('Cancel', function (panel) {
      win.close();
    }, this);

    var win = Ext.create('Ext.window.Window', {
      closable: true,
      id: 'newwin-' + node.data.id,
      modal: false,
      layout: 'fit',
      title: 'Add New Application',
      iconCls: 'tabsApplication',
      height: 200,
      width: 430,
      plain: true,
      items: newTab
    });

    win.show();

  }, this);

  directoryPanel.on('editapplication', function (npanel, node) {
    if (node == undefined || node == null)
      return;

    var newTab = Ext.create('AdapterManager.ApplicationPanel', {
      id: 'tab-' + node.data.id,
      scope: node.parentNode.data.record,
      record: node.data.record,
      url: 'directory/application'
    });

    var parentNode = node.parentNode;

    newTab.on('save', function (panel) {
      win.close();
      var dataLayerValue = node.data.record.DataLayer;
      var application = node.data.text;
      var scope = node.parentNode.data.text;

      if (dataLayerValue == 'SpreadsheetLibrary') {
        var configTab = contentPanel.items.map[scope + '.' + application + '.-nh-config'];
      }
      else {
        var configTab = contentPanel.items.map['tab-c.' + scope + '.' + application];
      }

      if (configTab)
        configTab.destroy();

      directoryPanel.onReload(node);
      if (parentNode.expanded == false)
        parentNode.expand();
    }, this);

    newTab.on('Cancel', function (panel) {
      win.close();
    }, this);

    var win = Ext.create('Ext.window.Window', {
      closable: true,
      modal: false,
      id: 'editwin-' + node.data.id,
      layout: 'fit',
      title: 'Edit Application \"' + node.data.text + '\"',
      iconCls: 'tabsApplication',
      height: 220,
      width: 430,
      plain: true,
      items: newTab
    });

    win.show();

  }, this);

  directoryPanel.on('deleteapplication', function (npanel, node) {
    var parentNode = node.parentNode.parentNode;

    Ext.Ajax.request({
      url: 'directory/deleteapplication',
      method: 'POST',
      params: {
        'nodeid': node.data.id
      },
      success: function (o) {
        directoryPanel.onReload(node);
        if (parentNode.expanded == false)
          parentNode.expand();
      },
      failure: function (f, a) {
        //Ext.Msg.alert('Warning', 'Error!!!');
        var message = 'Error deleting application!';
        showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
      }
    });

    var editAppPane = Ext.getCmp('editwin-' + node.id);
    if (editAppPane)
      editAppPane.destroy();

  }, this);

  directoryPanel.on('LoadPageDto', function (npanel, node) {
    var scope = node.parentNode.parentNode.parentNode.data.text;
    var app = node.parentNode.parentNode.data.text
    var graph = node.data.text; 
    var newTab = Ext.create('AdapterManager.DatagridPanel', {
      title: scope + '.' + app + '.' + graph,
      id: 'tablegrid_' + scope + '.' + app + '.' + graph,
      scope: scope,
      start: 0,
      limit: 25,
      app: app,
      graph: graph,
      url: 'datagrid/getdata'
    });
    var tab = contentPanel.items.map[newTab.id]; 
    if (tab == null) {
      contentPanel.add(newTab).show();
    } else {
      tab.show();
    }
  }, this);

  directoryPanel.on('opengraphmap', function (npanel, node) {

    var scope = node.parentNode.parentNode.parentNode;
    var application = node.parentNode.parentNode;

    var newTab = Ext.create('AdapterManager.MappingPanel', {
      title: 'GraphMap - ' + scope.data.text + "." + application.data.text + '.' + node.data.text,
      id: 'GraphMap - ' + scope.data.text + "-" + application.data.text + '.' + node.data.text,
      scope: scope.data.record,
      record: node.data.record,
      application: application.data.record,
      navigationUrl: 'mapping/getnode',
      searchPanel: searchPanel,
      directoryPanel: directoryPanel
    });
    var tab = contentPanel.items.map[newTab.id];

    if (tab == null) {
      contentPanel.add(newTab).show();
    } else {
      tab.show();
    }
  }, this);


  directoryPanel.on('newvaluelist', function (npanel, node) {
    var newTab = Ext.create('AdapterManager.ValueListPanel', {
      id: 'tab-' + node.data.id,
      record: node.data.record,
      nodeId: node.data.id,
      url: 'mapping/valueList'
    });

    newTab.on('save', function (panel) {
      win.close();
      directoryPanel.onReload(node);
      if (node.expanded == false)
        node.expand();
    }, this);

    newTab.on('Cancel', function (panel) {
      win.close();
    }, this);

    var win = Ext.create('Ext.window.Window', {
      closable: true,
      modal: false,
      layout: 'fit',
      title: 'Add Value List Name',
      iconCls: 'tabsValueList',
      height: 105,
      width: 430,
      plain: true,
      items: newTab
    });

    win.show();
  }, this);


  directoryPanel.on('editvaluelist', function (npanel, node) {
    var newTab = Ext.create('AdapterManager.ValueListPanel', {
      id: 'tab-' + node.data.id,
      record: node.data.record,
      nodeId: node.data.id,
      url: 'mapping/valueList'
    });

    var parentNode = node.parentNode;

    newTab.on('save', function (panel) {
      win.close();
      directoryPanel.onReload(node);
      if (parentNode.expanded == false)
        parentNode.expand();
    }, this);

    newTab.on('Cancel', function (panel) {
      win.close();
    }, this);

    var win = Ext.create('Ext.window.Window', {
      closable: true,
      modal: false,
      layout: 'fit',
      title: 'Edit Value List \"' + node.data.text + '\"',
      iconCls: 'tabsValueList',
      height: 120,
      width: 430,
      plain: true,
      items: newTab
    });

    win.show();

  }, this);


  directoryPanel.on('NewGraphMap', function (npanel, node) {
    var newTab = Ext.create('AdapterManager.GraphPanel', {
      id: 'tab-' + node.data.id,
      record: node.data.record,
      node: node,
      url: 'mapping/graphMap'
    });

    newTab.on('save', function (panel) {
      win.close();
      directoryPanel.onReload(node);
      if (node.expanded == false)
        node.expand();
    }, this);

    newTab.on('Cancel', function (panel) {
      win.close();
    }, this);

    var win = Ext.create('Ext.window.Window', {
      closable: true,
      modal: false,
      layout: 'fit',
      title: 'Add new GraphMap to Mapping',
      iconCls: 'tabsGraph',
      height: 190,
      width: 430,
      plain: true,
      items: newTab
    });

    win.show();
  }, this);

  directoryPanel.on('editgraphmap', function (npanel, node) {
    var newTab = Ext.create('AdapterManager.GraphPanel', {
      id: 'tab-' + node.data.id,
      record: node.data.record,
      node: node,
      url: 'mapping/graphMap'
    });

    var parentNode = node.parentNode;

    newTab.on('save', function (panel) {
      win.close();
      directoryPanel.onReload(node);
      if (parentNode.expanded == false)
        parentNode.expand();
    }, this);

    newTab.on('Cancel', function (panel) {
      win.close();
    }, this);

    var win = Ext.create('Ext.window.Window', {
      closable: true,
      modal: false,
      layout: 'fit',
      title: 'Edit Graph Map \"' + node.data.text + '\"',
      iconCls: 'tabsGraph',
      height: 190,
      width: 430,
      plain: true,
      items: newTab
    });

    win.show();

  }, this);


  directoryPanel.on('NewValueListMap', function (npanel, node) {
    var newTab = Ext.create('AdapterManager.ValueListMapPanel', {
      id: 'tab-' + node.data.id,
      record: node.data.record,
      node: node,
      url: 'mapping/valuelistmap'
    });

    newTab.on('save', function (panel) {
      win.close();
      directoryPanel.onReload(node);
      if (node.expanded == false)
        node.expand();

      showDialog(400, 100, 'Info', 'The new ValueListMap is added.', Ext.Msg.OK, null);
    }, this);

    newTab.on('Cancel', function (panel) {
      win.close();
    }, this);

    var win = Ext.create('Ext.window.Window', {
      closable: true,
      modal: false,
      layout: 'fit',
      title: 'Add new ValueListMap to valueList',
      iconCls: 'tabsValueListMap',
      height: 150,
      width: 430,
      plain: true,
      items: newTab
    });

    win.show();
  }, this);

  directoryPanel.on('editvaluelistmap', function (npanel, node) {
    var newTab = Ext.create('AdapterManager.ValueListMapPanel', {
      id: 'tab-' + node.data.id,
      record: node.data.record,
      node: node,
      url: 'mapping/valuelistmap'
    });

    var parentNode = node.parentNode;

    newTab.on('save', function (panel) {
      win.close();
      directoryPanel.onReload(node);
      if (parentNode.expanded == false)
        parentNode.expand();
    }, this);

    newTab.on('Cancel', function (panel) {
      win.close();
    }, this);

    var win = Ext.create('Ext.window.Window', {
      closable: true,
      modal: false,
      layout: 'fit',
      title: 'Edit Value List \"' + node.data.text + '\"',
      iconCls: 'tabsValueList',
      height: 150,
      width: 430,
      plain: true,
      items: newTab
    });

    win.show();
  }, this);

  // Finally, build the main layout once all the pieces are ready. This is also
  // a good
  // example of putting together a full-screen BorderLayout within a Viewport.

  var viewPort = Ext.create('Ext.container.Viewport', {
    layout: 'border',
    title: 'Scope Editor',
    border: false,
    items: [
        directoryPanel,
        centrePanel
    ],
    listeners: {
      render: function () {
        Ext.getBody().on("contextmenu", Ext.emptyFn, null, {
          preventDefault: true
        });
      }
    },
    renderTo: Ext.getBody()
  });
});
