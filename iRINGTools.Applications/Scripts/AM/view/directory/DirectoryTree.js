﻿Ext.define('AM.view.directory.DirectoryTree', {
    extend: 'Ext.tree.Panel',
    alias: 'widget.directorytree',
    region: 'center',
    border: false,
    expandAll: true,
    animate: true,
    containerScroll: true,

    //  pathSeparator: '>',
    lines: true,
    tbar: null,
    scroll: 'both',
    initComponent: function () {

        this.tbar = [{
            text: 'Reload Tree',
            icon: 'Content/img/16x16/view-refresh.png',
            scope: this,
            //action: 'reloaddiretree',
            handler: this.onReload
        }];

        Ext.apply(this, {
            store: Ext.create('Ext.data.TreeStore', {
                model: 'AM.model.DirectoryModel',
                root: {
                    expanded: true,
                    type: 'ScopesNode',
                    iconCls: 'scopes',
                    text: 'World',
                    security: ''
                },
                proxy: {
                    type: 'ajax',
                    timeout: 600000,
                    url: 'directory/getnode',
                    actionMethods: { read: 'POST' },
                    extraParams: {
                        id: null,
                        type: 'ScopesNode',
                        contextName: null,
                        endpoint: null,
                        baseUrl: null,
                        related: null,
                        security: null
                    },
                    reader: { type: 'json' }
                }
            }),
            viewConfig: {
                plugins: {
                    ptype: 'treeviewdragdrop',
                    dragGroup: 'propertyGroup',
                    enableDrop: false,
                    dragText: '{0}',
                    dragField: 'text'
                }
            },
            stateful: true,
            stateId: this.id + '-state'//,
            // stateEvents: ['itemcollapse', 'itemexpand']
        });

        this.callParent(arguments);

        this.scopesMenu = new Ext.menu.Menu();
        this.scopesMenu.add(this.buildScopesMenu());

        this.scopeMenu = new Ext.menu.Menu();
        this.scopeMenu.add(this.buildScopeMenu());

        this.applicationMenu = new Ext.menu.Menu();
        this.applicationMenu.add(this.buildApplicationMenu());

        this.appDataMenu = new Ext.menu.Menu();
        this.appDataMenu.add(this.buildAppDataMenu());

        this.valueListsMenu = new Ext.menu.Menu();
        this.valueListsMenu.add(this.buildvalueListsMenu());

        this.valueListMenu = new Ext.menu.Menu();
        this.valueListMenu.add(this.buildvalueListMenu());

        this.valueListMapMenu = new Ext.menu.Menu();
        this.valueListMapMenu.add(this.buildvalueListMapMenu());

        this.graphsMenu = new Ext.menu.Menu();
        this.graphsMenu.add(this.buildGraphsMenu());

        this.graphMenu = new Ext.menu.Menu();
        this.graphMenu.add(this.buildGraphMenu());

        this.on('itemcontextmenu', this.showContextMenu, this);
        this.on('itemclick', this.onClick, this);
    },

    getState: function () {
        var nodes = [], state = this.callParent();
        this.getRootNode().eachChild(function (child) {
            // function to store state of tree recursively 
            var storeTreeState = function (node, expandedNodes) {
                if (node.isExpanded() && node.childNodes.length > 0) {
                    expandedNodes.push(node.getPath('id'));

                    node.eachChild(function (child) {
                        storeTreeState(child, expandedNodes);
                    });
                }
            };
            storeTreeState(child, nodes);
        });
        Ext.apply(state, {
            expandedNodes: nodes
        });
        return state;
    },
    applyState: function (state) {
        var me = this;
        var nodes = state.expandedNodes || [],
            len = nodes.length;
        //  this.collapseAll();
        Ext.each(nodes, function (path) {
            me.expandPath(path, 'id');
        });
    },

    onReload: function (node) {
        var state = this.getState();
        this.body.mask('Loading', 'x-mask-loading');
        this.store.load();
        this.body.unmask();
        this.applyState(state, true);
    },

    buildScopesMenu: function () {
        return [
    {
        text: 'New Folder',
        icon: 'Content/img/16x16/document-new.png',
        scope: this,
        action: 'newscope'
    },
    {
        xtype: 'menuseparator'
    },
    {
        text: 'New Data Layer',
        icon: 'Content/img/16x16/document-new.png',
        scope: this,
        action: 'newDataLayer'
    },
    {
        text: 'Edit Data Layer',
        icon: 'Content/img/16x16/edit-delete.png',
        scope: this,
        action: 'editDataLayer'
    },
    {
        text: 'Delete Data Layer',
        icon: 'Content/img/16x16/edit-delete.png',
        scope: this,
        action: 'deleteDataLayer'
    },
    {
        xtype: 'menuseparator'
    },
    {
        text: 'Regenerate Hibernate Artifacts',
        icon: 'Content/img/16x16/document-new.png',
        scope: this,
        action: 'regenerateAll'
    }]
    },

    buildScopeMenu: function () {
        return [
      {
          xtype: 'button',
          text: 'New Folder',
          icon: 'Content/img/16x16/document-new.png',
          scope: this,
          action: 'newscope'
      },
      {
          xtype: 'button',
          text: 'Edit Folder',
          icon: 'Content/img/16x16/document-properties.png',
          scope: this,
          action: 'editscope'
      },
      {
          xtype: 'button',
          text: 'Delete Folder',
          icon: 'Content/img/16x16/edit-delete.png',
          scope: this,
          action: 'deletescope'
      },
      {
          xtype: 'menuseparator'
      },
      {
          xtype: 'button',
          text: 'New Endpoint',
          icon: 'Content/img/16x16/document-new.png',
          scope: this,
          action: 'newendpoint'
      }
    ]
    },
    buildApplicationMenu: function () {
        return [
      {
          xtype: 'button',
          text: 'Edit Endpoint',
          icon: 'Content/img/16x16/document-properties.png',
          scope: this,
          action: 'editendpoint'
      },
      {
          xtype: 'button',
          text: 'Delete Endpoint',
          icon: 'Content/img/16x16/edit-delete.png',
          scope: this,
          action: 'deleteendpoint'
      },
      {
          xtype: 'menuseparator'
      },
      {
          xtype: 'button',
          text: 'Open Configuration',
          icon: 'Content/img/16x16/preferences-system.png',
          scope: this,
          action: 'configureendpoint'
      }
    ]
    },
    buildAppDataMenu: function () {
        return [
      {
          xtype: 'button',
          text: 'Open Grid',
          icon: 'Content/img/16x16/document-properties.png',
          scope: this,
          action: 'showdata'
      }
    ]
    },
    buildvalueListsMenu: function () {
        return [
    {
        xtype: 'button',
        text: 'New Value List',
        icon: 'Content/img/16x16/document-new.png',
        scope: this,
        action: 'newvaluelist'
    }
    ]
    },

    buildvalueListMenu: function () {
        return [
    {
        xtype: 'button',
        text: 'Edit Value List Name',
        icon: 'Content/img/16x16/document-properties.png',
        scope: this,
        action: 'editvaluelist'
    },
    {
        xtype: 'button',
        text: 'Delete ValueList',
        icon: 'Content/img/16x16/edit-delete.png',
        scope: this,
        action: 'deletevaluelist'
    },
    {
        xtype: 'menuseparator'
    },
    {
        xtype: 'button',
        text: 'New Value Map',
        icon: 'Content/img/16x16/document-new.png',
        scope: this,
        action: 'newvaluemap'
    }
    ]
    },

    buildvalueListMapMenu: function () {
        return [
    {
        xtype: 'button',
        text: 'Edit Value List Map',
        icon: 'Content/img/16x16/document-properties.png',
        scope: this,
        action: 'editvaluemap'
    },
    {
        xtype: 'button',
        text: 'Delete Value List Map',
        icon: 'Content/img/16x16/edit-delete.png',
        scope: this,
        action: 'deletevaluemap'
    }
    ]
    },

    buildGraphsMenu: function () {
        return [
    {
        xtype: 'button',
        text: 'New GraphMap',
        icon: 'Content/img/16x16/document-new.png',
        scope: this,
        action: 'newgraph'
    }
    ]
    },

    buildGraphMenu: function () {
        return [
        {
            xtype: 'button',
            text: 'Refresh Facade',
            icon: 'Content/img/table_refresh.png',
            scope: this,
            action: 'refreshfacade'
        },
        {
            xtype: 'menuseparator'
        },
        {
            xtype: 'button',
            text: 'Edit GraphMap',
            icon: 'Content/img/16x16/document-properties.png',
            scope: this,
            action: 'editgraph'
        },
      {
          xtype: 'button',
          text: 'Delete GraphMap',
          icon: 'Content/img/16x16/edit-delete.png',
          scope: this,
          action: 'deletegraph'
      },
      {
          xtype: 'menuseparator'
      },
    {
        xtype: 'button',
        text: 'Open GraphMap',
        icon: 'Content/img/16x16/mapping.png',
        scope: this,
        action: 'opengraph'
    }]
    },

    showContextMenu: function (view, model, node, index, e) {

        e.stopEvent();
        node = model.store.getAt(index);

        this.getSelectionModel().select(node);
        this.onClick(view, model, 0, index, e);

        var obj = node.data;
        var ifsuperadmin = false;
        var baseUrl = 'false';

        var securityRole = '';
        var me = this;

        if (obj) {
            if (obj.record) {
                if (obj.record.securityRole) {
                    securityRole = obj.record.securityRole;
                }
                else {
                    ifsuperadmin = true;
                }
            }
        }

        // right clicks the root node
        if (index == 0) {
            Ext.Ajax.request({
                url: 'directory/UseLdap',
                method: 'GET',
                success: function (response, request) {
                    baseUrl = response.responseText;
                    // when root node has children
                    if (node.childNodes[0]) {
                        if (baseUrl.toLowerCase().indexOf('true') > -1) {
                            if (node.childNodes[0].data.record.securityRole) {
                                securityRole = node.childNodes[0].data.record.securityRole;

                                if (node.childNodes[0].data.record.securityRole.indexOf('rootadmin') > -1 || securityRole.indexOf('admin') > -1) {
                                    ifsuperadmin = true;
                                    me.scopesMenu.showAt(e.getXY());
                                }
                            }
                        }
                        else {
                            ifsuperadmin = true;
                            me.scopesMenu.showAt(e.getXY());
                        }
                    }
                    // when starting from scratch (root node has no children)
                    else {
                        if (baseUrl == 'false') {
                            me.scopesMenu.showAt(e.getXY());
                        }
                        else {
                            Ext.Ajax.request({
                                url: 'directory/RootSecurityRole',
                                method: 'GET',
                                success: function (response, request) {
                                    var rootSecurityRole = response.responseText;
                                    if (rootSecurityRole.indexOf('rootadmin') > -1 || securityRole.indexOf('admin') > -1) {
                                        ifsuperadmin = true;
                                        me.scopesMenu.showAt(e.getXY());
                                    }
                                },
                                failure: function () { }
                            });
                        }
                    }
                },
                failure: function () { }
            });
        }
        //right clicks tree nodes which are not the root node
        else if (securityRole.indexOf('admin') > -1 || ifsuperadmin) {
            if (obj.type == "folder") {
                this.scopeMenu.showAt(e.getXY());
            } else if (obj.type == "ApplicationNode") {
                this.applicationMenu.showAt(e.getXY());
            } else if (obj.type == "DataObjectNode") {
                this.appDataMenu.showAt(e.getXY());
            } else if (obj.type == "ValueListsNode") {
                this.valueListsMenu.showAt(e.getXY());
            } else if (obj.type == "ValueListNode") {
                this.valueListMenu.showAt(e.getXY());
            } else if (obj.type == "ListMapNode") {
                this.valueListMapMenu.showAt(e.getXY());
            } else if (obj.type == "GraphsNode") {
                var menu = new Ext.menu.Menu();
                menu.add(this.buildGraphsMenu(node));
                menu.showAt(e.getXY());
            } else if (obj.type == "GraphNode") {
                this.graphMenu.showAt(e.getXY());
            }
        }
    },

    getSelectedNode: function () {
        return this.getSelectionModel().selected.items[0];
    },

    onClick: function (view, model, n, idx, e) {
        try {
            var obj = model.store.getAt(idx);
            var pan = view.up('panel').up('panel'),
          prop = pan.down('propertygrid');
            prop.setSource(obj.data.property);
        } catch (e) {
            //  alert(e);
        }
    }
});
