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
        id: 'contentpanel',
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

    directoryPanel.on('create', function (npanel, node, formType) {

        if (node != null) {
            if (node.attributes.type == "Application") {
                Ext.MessageBox.show({
                    title: '<font color=red>Error!</font>',
                    msg: 'Opetation not allowed.',
                    buttons: Ext.MessageBox.OK,
                    icon: Ext.MessageBox.INFO
                });
                return false;
            }
            var formData = this.buildForm(node, formType);
            var obj = node.attributes;
        } else {
            Ext.MessageBox.show({
                title: '<font color=red>Error!</font>',
                msg: 'Please select a node.',
                buttons: Ext.MessageBox.OK,
                icon: Ext.MessageBox.INFO
            });
            return false;
        }
        if (node.parentNode == null) {
            if (node.attributes.type == "Scope") {

                var newTab = new AdapterManager.ScopePanel({
                    id: 'tab-' + node.id,
                    title: 'Scope - (New)',
                    configData: formData,
                    url: 'directory/scope',
                    closable: true
                });

                contentPanel.add(newTab);
                contentPanel.activate(newTab);
            }
        } else if (node.attributes.type == "Graph") {

            var application = node.parentNode;
            var scope = application.parentNode;

            var newTab = new AdapterManager.MappingPanel({
                title: 'Mapping - ' + ':(New)'
            });

            contentPanel.add(newTab);
            contentPanel.activate(newTab);

        } else {

            var scope = node.parentNode;
            var formData = this.buildForm(node, formType);

            var newTab = new AdapterManager.ScopePanel({
                id: 'tab-' + node.id,
                title: 'Application -' + node.text + '. (New)',
                configData: formData,
                url: 'directory/application',
                closable: true
            });

            contentPanel.add(newTab);
            contentPanel.activate(newTab);

        }

    });


    directoryPanel.on('open', function (npanel, node, formType) {

        if (node != null) {
            var formData = this.buildForm(node, formType);
            var obj = node.attributes;
        } else {
            Ext.MessageBox.show({
                title: '<font color=red>Error!</font>',
                msg: 'Please select a node.',
                buttons: Ext.MessageBox.OK,
                icon: Ext.MessageBox.INFO
            });
            return false;
        }

        if (node.attributes.type == "Scope") {

            var newTab = new AdapterManager.ScopePanel({
                id: 'tab-' + node.id,
                title: 'Scope - ' + node.text,
                configData: formData,
                url: 'directory/scope',
                closable: true
            });

            contentPanel.add(newTab);
            contentPanel.activate(newTab);

        } else if (node.attributes.type == "Application") {

            var scope = node.parentNode;
            var formData = this.buildForm(node, formType);

            var newTab = new AdapterManager.ScopePanel({
                id: 'tab-' + node.id,
                title: 'Application - ' + scope.text + '.' + node.text,
                configData: formData,
                url: 'directory/application',
                closable: true
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
        that = this;
        if (node.hasChildNodes()) {
            Ext.MessageBox.show({
                title: '<font color=red></font>',
                msg: 'Please select a child node to delete.',
                buttons: Ext.MessageBox.OK,
                icon: Ext.MessageBox.INFO
            });
        } else if (node == null) {
            Ext.Msg.alert('Warning', 'Please select a node.')
        } else {
            Ext.Msg.show({
                msg: 'All the tabs will be closed. Do you want to delete this node?',
                buttons: Ext.Msg.YESNO,
                icon: Ext.Msg.QUESTION,
                fn: function (action) {
                    if (action == 'yes') {
                        //send ajax request
                        Ext.Ajax.request({
                            url: 'directory/deletenode',
                            method: 'GET',
                            params: { 'nodeId': node.id, 'parentNodeID': node.parentNode.id },
                            success: function (o) {
                                // remove all tabs form tabpanel
                                Ext.getCmp('contentpanel').removeAll(true); // it will be removed in future
                                // remove the node form tree
                                //that.federationPanel.selModel.selNode.parentNode.removeChild(node);
                                //Tree Reload
                                that.onRefresh();
                                // fire event so that the Details panel will be changed accordingly
                                that.fireEvent('selectionchange', this)
                                Ext.Msg.alert('Sucess', 'Node has been deleted')
                            },
                            failure: function (f, a) {
                                Ext.Msg.alert('Warning', 'Error!!!')
                            }
                        });
                    } else if (action == 'no') {
                        Ext.Msg.alert('Info', 'Not now');
                    }
                }
            });
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