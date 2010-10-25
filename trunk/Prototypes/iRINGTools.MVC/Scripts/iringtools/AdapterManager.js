/// <reference path="../ext-3.2.1/adapter/ext/ext-base.js" />
/// <reference path="../ext-3.2.1/ext-all.js" />
/// <reference path="../ext-3.2.1/ux/ux-all.js" />

// Application instance for showing user-feedback messages.
var iRINGTools = new Ext.iRINGTools({});

Ext.onReady(function () {
  Ext.QuickTips.init();

  /*
  var actionPanel = new iIRNGTools.AdapterManager.ActionPanel({
  id: 'action-panel',
  region: 'west',
  width: 200,

  collapseMode: 'mini',
  collapsible: true,
  collapsed: false
  });
  */

  var searchPanel = new iIRNGTools.AdapterManager.SearchPanel({
    id: 'search-panel',
    title: 'Reference Data Search',
    region: 'south',
    height: 300,

    collapseMode: 'mini',
    collapsible: true,
    collapsed: false,

    searchUrl: 'Search',
    limit: 100
  });

  var contentPanel = new Ext.TabPanel({
    id: 'content-panel',
    region: 'center',

    collapsible: false,

    border: true,
    split: true
  });

  var navigationPanel = new iIRNGTools.AdapterManager.NavigationPanel({
    id: 'nav-panel',
    title: 'Directory',
    region: 'west',
    width: 250,

    collapseMode: 'mini',
    collapsible: true,
    collapsed: false,

    navigationUrl: 'Scopes?format=tree'

  });

  navigationPanel.on('create', function (npanel) {

    var newTab = new iIRNGTools.AdapterManager.ScopePanel();

    contentPanel.add(newTab);
    contentPanel.activate(newTab);

  });

  navigationPanel.on('update', function (npanel) {

    var window = new Ext.Window({
      title: 'Scope Details',
      width: 300,
      height: 300
    });

    window.show();

  });

  navigationPanel.on('configure', function (npanel, scope, application) {

    iRINGTools.setAlert(true, scope + '.' + application);

  });

  navigationPanel.on('mapping', function (npanel, scope, application, graph) {

    if (application.length > 0) {
      var newTab = new iIRNGTools.AdapterManager.MappingPanel({
        title: 'Mapping - ' + scope + '.' + application + '.' + graph,
        closable: true
      });

      contentPanel.add(newTab);
      contentPanel.activate(newTab);
    } else {
      iRINGTools.setAlert(true, 'Select a application before continuing.');
    }

  });

  navigationPanel.on('exchange', function (npanel, scope, application, graph) {

    if (application.length > 0) {

      var exhangePanel = new iIRNGTools.AdapterManager.ExchangePanel({
        scope: scope,
        application: application,
        graph: graph
      });

      exhangePanel.on('exchange', function (panel, form) {

        if (!form.isValid()) {
          iRINGTools.setAlert(false, "Information is invalid.");
          return false;
        }

        form.submit({
          url: "exchange/pull?scope=" + panel.scope + "&application=" + panel.application + "&graph=" + panel.graph,
          timeout: 120000,
          success: function (form, action) {
            Ext.Msg.alert("Exchange - " + form.scope + '.' + form.application + '.' + form.graph  , action.result.Message);
          }
        });

      });

      exhangePanel.on('cancel', function (panel) {
        var win = panel.findParentByType('window');
        if (win) {
          win.close();
        }
      });

      var window = new Ext.Window({
        title: 'Exchange - ' + scope + '.' + application + '.' + graph,
        labelWidth: 110, // label settings here cascade unless overridden                  
        width: 490,
        height: 390,
        layout: 'fit',
        modal: true,
        items: exhangePanel
      });

      window.show();

    } else {
      iRINGTools.setAlert(true, '   Please select a application or graph before continuing.  ');
    }

  });

  // Load Stores
  searchPanel.load();

  // Finally, build the main layout once all the pieces are ready.  This is also a good
  // example of putting together a full-screen BorderLayout within a Viewport.
  var viewPort = new Ext.Viewport({
    layout: 'border',
    title: 'Scope Editor',
    border: false,
    items: [
      {
        xtype: 'box',
        region: 'north',
        applyTo: 'header',
        border: false,
        height: 60
      },
      navigationPanel,
      contentPanel,
      searchPanel
    ],
    listeners: {
      render: function () {
        // After the component has been rendered, disable the default browser context menu
        Ext.getBody().on("contextmenu", Ext.emptyFn, null, { preventDefault: true });
      }
    },
    renderTo: Ext.getBody()
  });

});