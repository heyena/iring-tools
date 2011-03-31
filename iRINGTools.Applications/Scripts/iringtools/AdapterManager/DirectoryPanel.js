﻿Ext.ns('AdapterManager');
/**
* @class AdapterManager.directoryPanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
AdapterManager.DirectoryPanel = Ext.extend(Ext.Panel, {
  title: 'Directory',
  width: 220,

  collapseMode: 'mini',
  collapsible: true,
  collapsed: false,

  layout: 'border',
  border: false,
  split: true,

  navigationUrl: null,
  directoryPanel: null,
  scopesMenu: null,
  scopeMenu: null,
  applicationMenu: null,
  rootNode: null,
  treeLoader: null,

  /**
  * initComponent
  * @protected
  */
  initComponent: function () {

    this.addEvents({
      NewScope: true,
      NewApplication: true,
      EditScope: true,
      EditApplication: true,
      OpenMapping: true,
      DeleteScope: true,
      DeleteApplication: true,
      ReloadScopes: true,
      ReloadNode: true
    });

    this.tbar = this.buildToolbar();

    this.scopesMenu = new Ext.menu.Menu();
    this.scopesMenu.add(this.buildScopesMenu());

    this.scopeMenu = new Ext.menu.Menu();
    this.scopeMenu.add(this.buildScopeMenu());

    this.applicationMenu = new Ext.menu.Menu();
    this.applicationMenu.add(this.buildApplicationMenu());

    this.treeLoader = new Ext.tree.TreeLoader({
      baseParams: { type: null },
      url: this.navigationUrl
    });

    this.treeLoader.on("beforeload", function (treeLoader, node) {
      treeLoader.baseParams.type = node.attributes.type;

    }, this);

    this.rootNode = new Ext.tree.AsyncTreeNode({
      id: 'root',
      text: 'Scopes',
      expanded: true,
      draggable: false,
      icon: 'Content/img/internet-web-browser.png',
      type: 'ScopesNode'
    });
    this.directoryPanel = new Ext.tree.TreePanel({
      region: 'center',
      collapseMode: 'mini',
      height: 300,
      //layout: 'fit',
      border: false,
      split: true,
      expandAll: true,
      rootVisible: true,
      lines: true,
      autoScroll: true,
      //singleExpand: true,
      useArrows: true,
      loader: this.treeLoader,
      root: this.rootNode

    });

    this.directoryPanel.on('contextmenu', this.showContextMenu, this);

    this.items = [
            this.directoryPanel
        ];

    var state = Ext.state.Manager.get("AdapterManager");

    if (state) {
      if (this.directoryPanel.expandPath(state) == false) {
        Ext.state.Manager.clear("AdapterManager");
        this.directoryPanel.root.reload();
      }
    }

    // super
    AdapterManager.DirectoryPanel.superclass.initComponent.call(this);
  },

  buildToolbar: function () {
    return [
      {
        text: 'Reload',
        handler: this.onReload,
        //icon: 'Content/img/list-remove.png',
        scope: this
      }
    ]
  },

  buildScopesMenu: function () {
    return [
      {
        text: 'New Scope',
        handler: this.onNewScope,
        //icon: 'Content/img/list-add.png',
        scope: this
      },
      {
        text: 'Reload Scopes',
        handler: this.onReloadNode,
        //icon: 'Content/img/document-properties.png',
        scope: this
      }
    ]
  },

  buildScopeMenu: function () {
    return [
      {
        text: 'New Scope',
        handler: this.onNewScope,
        //icon: 'Content/img/document-properties.png',
        scope: this
      },
      {
        text: 'Edit Scope',
        handler: this.onEditScope,
        //icon: 'Content/img/document-properties.png',
        scope: this
      },
      {
        text: 'Delete Scope',
        handler: this.onDeleteScope,
        //icon: 'Content/img/document-properties.png',
        scope: this
      },
      {
        text: 'Reload Scope',
        handler: this.onReloadNode,
        //icon: 'Content/img/document-properties.png',
        scope: this
      },
      {
        xtype: 'menuseparator'
      },
      {
        text: 'New Application',
        handler: this.onNewApplication,
        //icon: 'Content/img/list-add.png',
        scope: this
      }
    ]
  },

  buildApplicationMenu: function () {
    return [
      {
        text: 'New Application',
        handler: this.onNewApplication,
        //icon: 'Content/img/document-properties.png',
        scope: this
      },
      {
        text: 'Edit Application',
        handler: this.onEditApplication,
        //icon: 'Content/img/document-properties.png',
        scope: this
      },
      {
        text: 'Delete Application',
        handler: this.onDeleteApplication,
        //icon: 'Content/img/document-properties.png',
        scope: this
      },
      {
        text: 'Reload Application',
        handler: this.onReloadNode,
        //icon: 'Content/img/document-properties.png',
        scope: this
      },
      {
        xtype: 'menuseparator'
      },
      {
        text: 'Open Mapping',
        handler: this.onOpenMapping,
        //icon: 'Content/img/list-add.png',
        scope: this
      }
    ]
  },

  showContextMenu: function (node, event) {
    if (node.isSelected()) {
      var x = event.browserEvent.clientX;
      var y = event.browserEvent.clientY;

      var obj = node.attributes;

      if (obj.type == "ScopesNode") {
        this.scopesMenu.showAt([x, y]);
      } else if (obj.type == "ScopeNode") {
        this.scopeMenu.showAt([x, y]);
      } else if (obj.type == "ApplicationNode") {
        this.applicationMenu.showAt([x, y]);
      }
    }
  },

  onNewScope: function (btn, ev) {
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();
    this.fireEvent('NewScope', this, node);
  },

  onNewApplication: function (btn, ev) {
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();
    this.fireEvent('NewApplication', this, node);
  },

  onEditScope: function (btn, ev) {
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();
    this.fireEvent('EditScope', this, node);
  },

  onEditApplication: function (btn, ev) {
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();
    this.fireEvent('EditApplication', this, node);
  },

  onOpenMapping: function (btn, ev) {
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();
    this.fireEvent('OpenMapping', this, node);
  },

  onDeleteScope: function (btn, ev) {
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();
    this.fireEvent('DeleteScope', this, node);
  },

  onDeleteApplication: function (btn, ev) {
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();
    this.fireEvent('DeleteApplication', this, node);
  },

  onReload: function (node) {
    //Ext.state.Manager.clear('AdapterManager');
    this.directoryPanel.root.reload();
  },

  onReloadNode: function (node) {
    //Ext.state.Manager.clear('AdapterManager');
    this.directoryPanel.root.reload();
  },

  getNodeBySelectedTab: function (tab) {
    var tabid = tab.id;
    nodeId = tabid.substr(4, tabid.length)  // tabid is "tab-jf23dfj-sd3fas-df33s-s3df"
    return this.getNodeById(nodeId)        // get the NODE using nodeid
  },

  getNodeById: function (nodeId) {
    if (this.directoryPanel.getNodeById(nodeId)) { //if nodeID exists it will find out NODE
      return this.directoryPanel.getNodeById(nodeId)
    } else {
      return false;
    }
  },

  reload: function () {
    this.directoryPanel.root.reload();
  }

});