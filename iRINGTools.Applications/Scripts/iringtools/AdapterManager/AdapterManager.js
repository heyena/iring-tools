﻿// <reference path="../ext-3.2.1/adapter/ext/ext-base.js" />
/// <reference path="../ext-3.2.1/ext-all.js" />
/// <reference path="../ext-3.2.1/ux/ux-all.js" />

// Application instance for showing user-feedback messages.
var App = new Ext.App({});
Ext.BLANK_IMAGE_URL = "Scripts/ext-3.3.1/resources/images/gray/s.gif";

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
            autoLoad: 'about.aspx',
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

    directoryPanel.on('editdatalayer', function (panel, node) {
      var dlPanel = new AdapterManager.DataLayerPanel({
        record: node.attributes.record,
        url: 'directory/datalayer'
      });
      
      dlPanel.show();
    });

    directoryPanel.on('editgraphname', function (npanel, node) {
        contentPanel.removeAll(true);
    }, this);

    directoryPanel.on('configure', function (npanel, node) {
        var dataLayerValue = node.attributes.record.DataLayer;
        var parentNode = node.parentNode;
        var assembly = node.attributes.record.Assembly;
        var application = node.text;
        var scope = parentNode.text;
        var datalayer = node.attributes.record.DataLayer;

        if (dataLayerValue == 'SpreadsheetDatalayer') {
            var newConfig = new AdapterManager.SpreadsheetLibraryPanel({
                id: 'tab-c.' + scope + '.' + application,
                title: 'Spreadsheet Configuration - ' + scope + '.' + application,
                iconCls: 'tabsNhibernate',
                scope: scope,
                application: application,
                datalayer: assembly,
                url: 'spreadsheet/configure',
                closable: true
            });

            newConfig.on('save', function () {
                directoryPanel.onReload(node);
            }, this);

            contentPanel.add(newConfig);
            contentPanel.activate(newConfig);
        }
        else if (dataLayerValue == 'NHibernateLibrary' || dataLayerValue == 'EQMSDataLayer') {
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
                try {
                    pidConfigWizard = new AdapterManager.sppidConfigWizard({
                        scope: scope,
                        app: application,
                        datalayer: assembly

                    });
                    contentPanel.add(pidConfigWizard);
                    contentPanel.activate(pidConfigWizard);
                }
                catch (err) {
                    showDialog(400, 100, 'Error', 'No configuration available for \"SPPIDDataLayer\".', Ext.Msg.OK, null);
                }
            }
        }
        else {
            showDialog(400, 100, 'Info', 'The datalayer \"' + dataLayerValue + '\" is not configurable.', Ext.Msg.OK, null);
        }
    }, this);

    directoryPanel.on('newapplication', function (npanel, node) {
        Ext.getBody().mask("Loading...", "x-mask-loading");
        var newTab = new AdapterManager.ApplicationPanel({
            id: 'tab-' + node.id,
            scope: node.attributes.record,
            record: null,
            state: 'new',
            node: node,
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
			resizable : false,
            id: 'newwin-' + node.id,
            modal: true,
			autoHeight:true,
            layout: 'fit',
			shadow : false,
            title: 'Add New Application',
            iconCls: 'tabsApplication',
            //height: 291,
            //width: 430,
            height: 360,
            width: 660,
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
            state: 'edit',
            node: node,
            url: 'directory/application'
        });

        var parentNode = node.parentNode;

        newTab.on('save', function (panel) {
            win.close();
            var dataLayerValue = node.attributes.record.DataLayer;
            var application = node.text;
            var scope = node.parentNode.text;

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
            win.destroy();
			//win.close();
        }, this);

        var win = new Ext.Window({
            closable: true,
			resizable : false,
            modal: true,
			shadow : false,
			//resizable:false,
			//autoDestroy:true,
            id: 'editwin-' + node.id,
			//collapsible:false,
			autoHeight:true,
            //autoScroll: true,
            layout: 'fit',
            title: 'Edit Application\"' + node.text + '\"',
            iconCls: 'tabsApplication',
            height: 360,
            width: 660,//460,
			//height: '100%',//360, //291,
            //width: '25%',//460, //430,
            //plain: true,
            items: newTab
        });
        /*win.on('beforeshow', function (me, eOpts){
		  alert('this is before show..');
		  me.findByType('fieldset')
		}, this);*/
		
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
        var scope = node.parentNode.parentNode.parentNode.text
        var app = node.parentNode.parentNode.text;
        var graph = node.text;
        loadAppPageDto(scope, app, graph);
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

    directoryPanel.on('RefreshFacade', function (npanel, node) {
        directoryPanel.body.mask('Loading', 'x-mask-loading');
        Ext.Ajax.request({
            url: 'facade/refreshFacade',
            method: 'POST',
            params: {
                scope: node.attributes.id
            },
            success: function (o) {
                directoryPanel.onReload(node);
                directoryPanel.body.unmask();
            },
            failure: function (f, a) {
                //Ext.Msg.alert('Warning', 'Error!!!');
                var message = 'Error refreshing facade!';
                showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
            }
        });
    });

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


