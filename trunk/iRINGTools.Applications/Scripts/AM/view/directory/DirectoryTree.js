﻿Ext.define('AM.view.directory.DirectoryTree', {
    extend: 'Ext.tree.Panel',
    alias: 'widget.directorytree',
    viewConfig: {
        plugins: {
            ptype: 'treeviewdragdrop',
            dragGroup: 'propertyGroup',
            enableDrop: false
        }
    },
    region: 'center',
    border: false,
    expandAll: true,
    animate: true,
    containerScroll: true,
    pathSeparator: '>',
    lines: true,
    tbar: null,
    scroll: 'both',
    store: null,
    initComponent: function () {

        this.store = Ext.create('Ext.data.TreeStore', {
            model: 'AM.model.DirectoryModel',
            //clearOnLoad: true,
            root: {
                id: 'root',
                expanded: true,
                type: 'ScopesNode',
                iconCls: 'scopes',
                text: 'Scopes'
            }
        });

        Ext.apply(this, {
            stateful: true,
            stateId: this.id + '-state',
            stateEvents: ['itemcollapse', 'itemexpand']
        });


        this.callParent(arguments);

        this.addEvents({
            newScope: true,
            newApplication: true,
            editScope: true,
            editApplication: true,
            openMapping: true,
            deleteScope: true,
            deleteApplication: true,
            reloadScopes: true,
            reloadNode: true
        });
        this.tbar = new Ext.toolbar.Toolbar();
        this.tbar.add(this.buildToolbar());

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
                    expandedNodes.push(node.getPath());//'text'));
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
        var nodes = state.expandedNodes || [],
            len = nodes.length;
      //  this.collapseAll();
        for (var i = 0; i < len; i++) {
            if (typeof nodes[i] != 'undefined') {
                this.expandPath(nodes[i]);//, 'text');
            }
        }
       // this.callParent(arguments);
    },

    buildToolbar: function () {
        return [
      {
          xtype: 'button',
          text: 'Reload Tree',
          handler: this.onReload,
          icon: 'Content/img/16x16/view-refresh.png',
          scope: this
      }
    ]
    },
    buildScopesMenu: function () {
        return [
      {
          xtype: 'button',
          text: 'New Scope',
          icon: 'Content/img/16x16/document-new.png',
          scope: this,
          action: 'newscope'
      }
    ]
    },
    buildScopeMenu: function () {
        return [
      {
          xtype: 'button',
          text: 'Edit Scope',
          icon: 'Content/img/16x16/document-properties.png',
          scope: this,
          action: 'editscope'
      },
      {
          xtype: 'button',
          text: 'Delete Scope',
          icon: 'Content/img/16x16/edit-delete.png',
          scope: this,
          action: 'deletescope'
      },
      {
          xtype: 'menuseparator'
      },
      {
          xtype: 'button',
          text: 'New Application',
          icon: 'Content/img/16x16/document-new.png',
          scope: this,
          action: 'newapp'
      }
    ]
    },
    buildApplicationMenu: function () {
        return [
      {
          xtype: 'button',
          text: 'Edit Application',
          icon: 'Content/img/16x16/document-properties.png',
          scope: this,
          action: 'editapp'
      },
      {
          xtype: 'button',
          text: 'Delete Application',
          icon: 'Content/img/16x16/edit-delete.png',
          scope: this,
          action: 'deleteapp'
      },
      {
          xtype: 'menuseparator'
      },
      {
          xtype: 'button',
          text: 'Open Configuration',
          icon: 'Content/img/16x16/preferences-system.png',
          scope: this,
          action: 'configure'
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
        var obj = model.store.getAt(index).data;

        if (obj.type == "ScopesNode") {
            this.scopesMenu.showAt(e.getXY());
        } else if (obj.type == "ScopeNode") {
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
    },

    getSelectedNode: function () {
        return this.getSelectionModel().selected.items[0];
    },

    onReload: function (node) {
        //get state from tree
        var me = this;
        var state = this.getState();
        this.body.mask('Loading', 'x-mask-loading');

        this.getStore().load(node);
        this.body.unmask();
        this.applyState(state, true);
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
