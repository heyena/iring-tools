/// <reference path="../ext-3.2.1/adapter/ext/ext-base.js" />
/// <reference path="../ext-3.2.1/ext-all.js" />
/// <reference path="../ext-3.2.1/ux/ux-all.js" />

// Application instance for showing user-feedback messages.
var App = new Ext.App({});

Ext.onReady(function () {
    Ext.QuickTips.init();

    /*
    var actionPanel = new AdapterManager.ActionPanel({
    id: 'action-panel',
    region: 'west',
    width: 200,

    collapseMode: 'mini',
    collapsible: true,
    collapsed: false
    });
    */

    var searchPanel = new AdapterManager.SearchPanel({
        id: 'search-panel',
        title: 'Reference Data Search',
        region: 'south',
        height: 300,

        collapseMode: 'mini',
        collapsible: true,
        collapsed: false,

        searchUrl: 'refdata',
        limit: 100
    });

    var contentPanel = new Ext.TabPanel({
        id: 'content-panel',
        region: 'center',

        collapsible: false,

        border: true,
        split: true
    });

    var directoryPanel = new AdapterManager.DirectoryPanel({
        id: 'nav-panel',
        title: 'Directory',
        region: 'west',
        width: 250,

        collapseMode: 'mini',
        collapsible: true,
        collapsed: false,

        navigationUrl: 'directory?format=tree'

    });

    directoryPanel.on('create', function (npanel) {

        var window = new Ext.Window({
            title: 'Scope Details',
            width: 300,
            height: 300
        });

        window.show();

    });

    directoryPanel.on('open', function (npanel, node) {

        if (node.attributes.type == "Scope") {

            var newTab = new AdapterManager.ScopePanel({
                title: 'Scope - ' + node.text
            });

            contentPanel.add(newTab);
            contentPanel.activate(newTab);

        } else if (node.attributes.type == "Application") {

            var scope = node.parentNode;

            var newTab = new AdapterManager.ScopePanel({
                title: 'Application - ' + scope.text + '.' + node.text
            });

            contentPanel.add(newTab);
            contentPanel.activate(newTab);

        } else if (node.attributes.type == "Graph") {

            var application = node.parentNode;
            var scope = application.parentNode;

            var newTab = new AdapterManager.MappingPanel({
                title: 'Mapping - ' + scope.text + '.' + application.text + '.' + node.text
            });

            contentPanel.add(newTab);
            contentPanel.activate(newTab);

        }

    });

    directoryPanel.on('remove', function (npanel, node) {

        App.setAlert(true, scope + '.' + application);

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
      directoryPanel,
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