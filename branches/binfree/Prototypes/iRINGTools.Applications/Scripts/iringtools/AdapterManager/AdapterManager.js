// <reference path="../ext-3.2.1/adapter/ext/ext-base.js" />
/// <reference path="../ext-3.2.1/ext-all.js" />
/// <reference path="../ext-3.2.1/ux/ux-all.js" />

// Application instance for showing user-feedback messages.
var App = new Ext.App({});

Ext.onReady(function () {
  Ext.QuickTips.init();
  Ext.Ajax.timeout = 120000; //increase request time

  Ext.get('about-link').on('click', function () {
    var win = new Ext.Window({
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

  /*
  * var actionPanel = new AdapterManager.ActionPanel({ id: 'action-panel',
  * region: 'west', width: 200,
  * 
  * collapseMode: 'mini', collapsible: true, collapsed: false });
  */

  var scope;
  var application;
  var scopeNode;
  var appNode;

  var searchPanel = new AdapterManager.SearchPanel({
    id: 'search-panel',
    title: 'Reference Data Search',
    collapsedTitle: 'Reference Data Search',
    region: 'south',
    height: 250,
    //collapseMode: 'mini',
    collapsible: true,
    collapsed: false,
    searchUrl: 'refdata/getnode',
    limit: 100
  });

  var contentPanel = new Ext.TabPanel({
    id: 'content-panel',
    region: 'center',
    collapsible: false,
    closable: true,
    enableTabScroll: true,
    border: true,
    split: true
  });

  var centrePanel = new Ext.Panel({
    id: 'centre-panel',
    region: 'center',
    layout: 'border',
    collapsible: false,
    closable: true,
    enableTabScroll: true,
    border: true,
    split: true,
    items: [searchPanel, contentPanel]
  });

  var directoryPanel = new AdapterManager.DirectoryPanel({
    id: 'nav-panel',
    title: 'Directory',
    layout: 'border',
    region: 'west',
    width: 260,
    minSize: 175,
    maxSize: 400,
    border: 1,
    collapsible: true,
    collapsed: false,
    navigationUrl: 'directory/getnode'
  });

  directoryPanel.on('newscope', function (npanel, node) {
    var newTab = new AdapterManager.ScopePanel({
      id: 'tab-' + node.id,
      record: node.attributes.record,
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

    var win = new Ext.Window({
      closable: true,
      id: 'newwin-' + node.id,
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

    newTab.form.getForm().findField('Name').clearInvalid();

  }, this);


  directoryPanel.on('editscope', function (npanel, node) {
    var newTab = new AdapterManager.ScopePanel({
      id: 'tab-' + node.id,
      record: node.attributes.record,
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

    var win = new Ext.Window({
      closable: true,
      id: 'editwin-' + node.id,
      modal: false,
      layout: 'fit',
      title: 'Edit Scope \"' + node.text + '\"',
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
        //Ext.Msg.alert('Warning', 'Error!!!');
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
    var dataLayerValue = node.attributes.record.DataLayer;
    var assembly = node.attributes.record.Assembly;
    var baseUri = node.attributes.properties.BaseURI;
    getScopeApp(baseUri, node);
    var datalayer = node.attributes.record.DataLayer;

    if (dataLayerValue == 'SpreadsheetLibrary') {
      var newConfig = new AdapterManager.SpreadsheetLibraryPanel({
        id: 'tab-c.' + scope + '.' + application,
        title: 'Configure - ' + scope + '.' + application,
        scope: scope,
        application: application,
        datalayer: assembly,
        url: 'spreadsheet/configure',
        closable: true
      });

      contentPanel.add(newConfig);
      contentPanel.activate(newConfig);
    }
    else if (dataLayerValue == 'NHibernateLibrary') {
      var nhConfigId = scope + '.' + application + '.-nh-config';
      var nhConfigWizard = contentPanel.getItem(nhConfigId);

      if (nhConfigWizard) {
        nhConfigWizard.show();
      }
      else {
        nhConfigWizard = new AdapterManager.NHibernateConfigWizard({
          scope: scope,
          app: application
        });
        contentPanel.add(nhConfigWizard);
        contentPanel.activate(nhConfigWizard);
      }
    }
    else if (dataLayerValue == 'SPPIDDataLayer') {
      var pidConfigId = scope + '.' + application + '.-pid-config';
      var pidConfigWizard = contentPanel.getItem(pidConfigId);

      if (pidConfigWizard) {
        pidConfigWizard.show();
      }
      else {
        pidConfigWizard = new AdapterManager.sppidConfigWizard({
          scope: scope,
          app: application
        });
        contentPanel.add(pidConfigWizard);
        contentPanel.activate(pidConfigWizard);
      }
    }
  }, this);

  directoryPanel.on('newapplication', function (npanel, node) {
    Ext.getBody().mask("Loading...", "x-mask-loading");
    var newTab = new AdapterManager.ApplicationPanel({
      id: 'tab-' + node.id,
      scope: node.attributes.record,
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

    var win = new Ext.Window({
      closable: true,
      id: 'newwin-' + node.id,
      modal: false,
      layout: 'fit',
      title: 'Add New Application',
      iconCls: 'tabsApplication',
      height: 200,
      width: 430,
      plain: true,
      items: newTab,
      listeners: {
        afterlayout: function (pane) {
          Ext.getBody().unmask();
        }
      }
    });

    win.show();

    newTab.form.getForm().findField('Name').clearInvalid();

  }, this);

  directoryPanel.on('editapplication', function (npanel, node) {
    if (node == undefined || node == null)
      return;

    var newTab = new AdapterManager.ApplicationPanel({
      id: 'tab-' + node.id,
      scope: node.parentNode.attributes.record,
      record: node.attributes.record,
      url: 'directory/application'
    });

    var parentNode = node.parentNode;

    newTab.on('save', function (panel) {
      win.close();
      var dataLayerValue = node.attributes.record.DataLayer;
      var baseUri = node.attributes.properties.BaseURI;
      getScopeApp(baseUri, node);

      if (dataLayerValue == 'SpreadsheetDataLayer') {
        var configTab = contentPanel.items.map[scope + '.' + application + '.-nh-config'];
      }
      else if (dataLayerValue == 'SPPIDDataLayer') {
        var configTab = contentPanel.items.map[scope + '.' + application + '.-pid-config'];
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

    var win = new Ext.Window({
      closable: true,
      modal: false,
      id: 'editwin-' + node.id,
      layout: 'fit',
      title: 'Edit Application \"' + node.text + '\"',
      iconCls: 'tabsApplication',
      height: 200,
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
        'nodeid': node.attributes.id
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
    var baseUri = node.attributes.property.BaseURI;
    getScopeApp(baseUri, node);
    var graph = node.text;
    loadAppPageDto(scope, application, graph);
  }, this);

  var getScopeApp = function (baseUri, node) {
    for (var i = 0; i < 2; i++) {
      index = baseUri.lastIndexOf('/');

      if (i == 0)
        application = baseUri.substring(index + 1);
      else if (i == 1)
        scope = baseUri.substring(index + 1);

      baseUri = baseUri.substring(0, index);
    }

    while (node.text != application && node != undefined)
      node = node.parentNode;

    if (node != undefined)
      appNode = node;

    while (node.text != scope && node != undefined)
      node = node.parentNode;

    if (node != undefined)
      scopeNode = node;
  };

  directoryPanel.on('opengraphmap', function (npanel, node) {
    var baseUri = node.attributes.property.BaseURI;
    getScopeApp(baseUri, node);

    var newTab = new AdapterManager.MappingPanel({
      title: 'GraphMap - ' + scope + "." + application + '.' + node.text,
      id: 'GraphMap - ' + scope + "-" + application + '.' + node.text,
      scope: scopeNode.attributes.record,
      record: node.attributes.record,
      application: appNode.attributes.record,
      navigationUrl: 'mapping/getnode',
      searchPanel: searchPanel,
      directoryPanel: directoryPanel,
      baseUri: baseUri
    });

    contentPanel.add(newTab);
    contentPanel.activate(newTab);

  }, this);


  directoryPanel.on('newvaluelist', function (npanel, node) {
    var newTab = new AdapterManager.ValueListPanel({
      id: 'tab-' + node.id,
      record: node.attributes.record,
      nodeId: node.id,
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

    var win = new Ext.Window({
      closable: true,
      id: 'newwin-' + node.id,
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
    var newTab = new AdapterManager.ValueListPanel({
      id: 'tab-' + node.id,
      record: node.attributes.record,
      nodeId: node.id,
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

    var win = new Ext.Window({
      closable: true,
      modal: false,
      layout: 'fit',
      title: 'Edit Value List \"' + node.text + '\"',
      iconCls: 'tabsValueList',
      height: 105,
      width: 430,
      plain: true,
      items: newTab
    });

    win.show();

  }, this);


  directoryPanel.on('NewGraphMap', function (npanel, node) {
    var newTab = new AdapterManager.GraphPanel({
      id: 'tab-' + node.id,
      record: node.attributes.record,
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

    var win = new Ext.Window({
      //id: 'newwin-' + node.id,
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
    var newTab = new AdapterManager.GraphPanel({
      id: 'tab-' + node.id,
      record: node.attributes.record,
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

    var win = new Ext.Window({
      closable: true,
      modal: false,
      layout: 'fit',
      title: 'Edit Graph Map \"' + node.text + '\"',
      iconCls: 'tabsGraph',
      height: 190,
      width: 430,
      plain: true,
      items: newTab
    });

    win.show();

  }, this);


  directoryPanel.on('NewValueListMap', function (npanel, node) {
    var newTab = new AdapterManager.ValueListMapPanel({
      id: 'tab-' + node.id,
      record: node.attributes.record,
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

    var win = new Ext.Window({
      id: 'newwin-' + node.id,
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
    var newTab = new AdapterManager.ValueListMapPanel({
      id: 'tab-' + node.id,
      record: node.attributes.record,
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

    var win = new Ext.Window({
      closable: true,
      modal: false,
      layout: 'fit',
      title: 'Edit Value List \"' + node.text + '\"',
      iconCls: 'tabsValueList',
      height: 150,
      width: 430,
      plain: true,
      items: newTab
    });

    win.show();

  }, this);

  // Load Stores
  // searchPanel.load();

  // Finally, build the main layout once all the pieces are ready. This is also
  // a good
  // example of putting together a full-screen BorderLayout within a Viewport.
  var viewPort = new Ext.Viewport({
    layout: 'border',
    title: 'Scope Editor',
    border: false,
    items: [{
      xtype: 'box',
      region: 'north',
      applyTo: 'header',
      border: false,
      height: 55
    }, directoryPanel, centrePanel],
    listeners: {
      render: function () {
        // After the component has been rendered, disable the default browser
        // context menu
        Ext.getBody().on("contextmenu", Ext.emptyFn, null, {
          preventDefault: true
        });
      }
    },
    renderTo: Ext.getBody()
  });

});


