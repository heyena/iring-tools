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

    var searchPanel = new AdapterManager.SearchPanel({
        id: 'search-panel',
        title: 'Reference Data Search',
        region: 'south',
        height: 300,
				maxHeight: 420,
        collapseMode: 'mini',
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
        region: 'west',
        width: 260,

        collapseMode: 'mini',
        collapsible: true,
        collapsed: false,
        navigationUrl: 'directory/getnode'
    });
    directoryPanel.on('newscope', function (npanel, node) {

        if (node.attributes.type == "ScopeNode") {
            node = node.parentNode;
        }

        var newTab = new AdapterManager.ScopePanel({
            id: 'tab-' + node.id,
            title: 'Scope - (New)',
            record: node.attributes.record,
            url: 'directory/scope'
        });

        newTab.on('save', function (panel) {
            contentPanel.remove(panel);
            //directoryPanel.reload();
            directoryPanel.onReload(node);
        }, this);

        contentPanel.add(newTab);
        contentPanel.activate(newTab);

    }, this);

    directoryPanel.on('editscope', function (npanel, node) {

        var newTab = new AdapterManager.ScopePanel({
            id: 'tab-' + node.id,
            title: 'Scope - ' + node.text,
            record: node.attributes.record,
            url: 'directory/scope'
        });

        newTab.on('save', function (panel) {
            contentPanel.removeAll(true);
            //directoryPanel.reload();
            directoryPanel.onReload(node);

        }, this);

        contentPanel.add(newTab);
        contentPanel.activate(newTab);

    }, this);

    directoryPanel.on('deletescope', function (npanel, node) {

        Ext.Ajax.request({
            url: 'directory/deletescope',
            method: 'POST',
            params: {
                'nodeid': node.attributes.id
            },
            success: function (o) {
                contentPanel.removeAll(true);
                //directoryPanel.reload();
                directoryPanel.onReload(node);
                Ext.Msg.alert('Sucess', 'Scope [' + node.attributes.id + '] has been deleted');
            },
            failure: function (f, a) {
                Ext.Msg.alert('Warning', 'Error!!!');
            }
        });

    }, this);

    directoryPanel.on('editgraphname', function (npanel, node) {
        contentPanel.removeAll(true);
    }, this);

    directoryPanel.on('newapplication', function (npanel, node) {

        if (node.attributes.type == "ApplicationNode") {
            node = node.parentNode;
        }

        var newTab = new AdapterManager.ApplicationPanel({
            id: 'tab-' + node.id,
            title: 'Application - ' + node.parentNode.text + '.(new)',
            scope: node.attributes.record,
            record: null,
            url: 'directory/application',
            closable: true
        });

        newTab.on('save', function (panel) {
            contentPanel.remove(panel);
            //directoryPanel.reload();
            directoryPanel.onReload(node);
        }, this);

        contentPanel.add(newTab);
        contentPanel.activate(newTab);

    }, this);

    directoryPanel.on('editapplication', function (npanel, node) {
        if (node == undefined || node == null)
            return;

        var newTab = new AdapterManager.ApplicationPanel({
            id: 'tab-' + node.id,
            title: 'Application - ' + node.parentNode.text + '.' + node.text,
            scope: node.parentNode.attributes.record,
            record: node.attributes.record,
            url: 'directory/application',
            closable: true
        });

        newTab.on('save', function (panel) {
            contentPanel.remove(panel);
            // directoryPanel.reload();
            directoryPanel.onReload(node);
        }, this);

        newTab.on('configure', function (panel, scope, application, dataLayer) {
            if (dataLayer == 'ExcelLibrary') {
                var newConfig = new AdapterManager.ExcelLibraryPanel({
                    id: 'tab-c.' + scope.Name + '.' + application.Name,
                    title: 'Configure - ' + scope.Name + '.' + application.Name,
                    scope: scope,
                    application: application,
                    url: 'excel/configure',
                    closable: true
                });

                contentPanel.add(newConfig);
                contentPanel.activate(newConfig);
            }
            else if (dataLayer == 'NHibernateLibrary') {
                var nhConfigId = scope.Name + '.' + application.Name + '-nh-config-wizard';
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

        }, this);

        contentPanel.add(newTab);
        contentPanel.activate(newTab);

    }, this);

    directoryPanel.on('deleteapplication', function (npanel, node) {
        Ext.Ajax.request({
            url: 'directory/deleteapplication',
            method: 'POST',
            params: {
                'nodeid': node.attributes.id
            },
            success: function (o) {
                contentPanel.removeAll(true);
                // directoryPanel.reload();
                directoryPanel.onReload(node);
                Ext.Msg.alert('Sucess', 'Application [' + node.attributes.id + '] has been deleted');
            },
            failure: function (f, a) {
                Ext.Msg.alert('Warning', 'Error!!!');
            }
        });
    }, this);

    directoryPanel.on('opengraphmap', function (npanel, node) {

        var scope = node.parentNode.parentNode.parentNode;
        var application = node.parentNode.parentNode;

        var newTab = new AdapterManager.MappingPanel({
            title: 'GraphMap - ' + scope.text + "." + application.text + '.' + node.text,
            id: 'GraphMap - ' + scope.text + "-" + application.text + '.' + node.text,
            scope: scope.attributes.record,
            record: node.attributes.record,
            application: application.attributes.record,
            navigationUrl: 'mapping/getnode',
            searchPanel: searchPanel,
            directoryPanel: directoryPanel
        });

        contentPanel.add(newTab);
        contentPanel.activate(newTab);

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