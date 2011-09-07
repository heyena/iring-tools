/**
* @class AdapterManager.directoryPanel
* @extends Panel
* @author by Gert Jansen van Rensburg

*/
Ext.define('AdapterManager.DirectoryPanel', {
  extend: 'Ext.panel.Panel',
  alias: 'widget.AdapterManager.DirectoryPanel',
  title: 'Directory',
  width: 220,
  collapsible: true,
  collapsed: false,
  layout: 'border',
  border: true,
  // margins: '0 0 20 0',
  split: true,
  contentPanel: null,
  navigationUrl: null,
  directoryPanel: null,
  scopesMenu: null,
  scopeMenu: null,
  valueListsMenu: null,
  valueListMenu: null,
  graphsMenu: null,
  graphMenu: null,
  applicationMenu: null,
  directoryStore: null,
  ajaxProxy: null,
  propertyPanel: null,
  selectedDirectoryNode: null,
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

    this.ajaxProxy = Ext.create('Ext.data.proxy.Ajax', {
      timeout: 120000,
      actionMethods: { read: 'POST' },
      url: this.navigationUrl,
      extraParams: {
        id: null,
        type: 'ScopesNode',
        related: null
      },
      reader: { type: 'json' }
    });

    Ext.define('scopesmodel', {
      extend: 'Ext.data.Model',
      fields: [
         { name: 'id', type: 'string' },
         { name: 'hidden', type: 'boolean' },
         { name: 'property', type: 'object' },
         { name: 'identifier', type: 'string' },
         { name: 'text', type: 'string' },
         { name: 'type', type: 'string' },
         { name: 'record', type: 'object' }
       ]
    });

    this.directoryStore = Ext.create('Ext.data.TreeStore', {
      model: 'scopesmodel',
      clearOnLoad: true,
      root: {
        expanded: true,
        type: 'ScopesNode',
        iconCls: 'scopes',
        text: 'Scopes'
      },
      proxy: this.ajaxProxy

    });

    this.directoryStore.on("beforeload", function (store, action) {
      this.ajaxProxy.extraParams.type = action.node.data.type;
      if (action.node.data.record != undefined && action.node.data.record.Related != undefined)
        this.ajaxProxy.extraParams.related = action.node.data.record.Related;
    }, this);

    this.propertyPanel = new Ext.grid.property.Grid({
      title: 'Details',
      region: 'south',
      layout: 'fit',
      height: 250,
      viewConfig: { stripeRows: true },
      collapsible: true,
      autoScroll: true,
      border: 0,
      frame: false,
      selModel: new Ext.selection.RowModel({ mode: 'SINGLE' }),
      source: {},
      listeners: {
        beforeedit: function (e) {
          e.cancel = true;
        },
        //        // to copy but not edit content of property grid				
        afteredit: function (e) {
          //          e.grid.getSelectionModel().selections.items[0].data.value = e.originalValue;
          //          e.record.data.value = e.originalValue;
          //          e.value = e.originalValue;
          //          e.grid.getView().refresh();
        }
      }
    });

    this.directoryPanel = Ext.create('Ext.tree.TreePanel', {
      id: 'Directory-Panel',
      viewConfig: {
        plugins: {
          ptype: 'treeviewdragdrop',
          dragGroup: 'propertyGroup'
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
      selModel: new Ext.selection.RowModel({ mode: 'SINGLE' }),
      store: this.directoryStore,
      stateful: true,
      stateEvents: ['expand', 'collapse'],
      stateId: 'directory-state-id',
      getState: function () {
        return {
          collapsed: this.collapsed
        }
      }
    });

    this.directoryPanel.on('itemcontextmenu', this.showContextMenu, this);
    this.directoryPanel.on('itemclick', this.onClick, this);
    this.directoryPanel.on('dblclick', this.onDoubleClick, this);
    this.directoryPanel.on('newgraphmap', this.newGraphmap, this);
    this.directoryPanel.on('select', this.onSelect, this);

    this.items = [
      this.directoryPanel,
      this.propertyPanel
    ];

    // super
    this.callParent(arguments);
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
        handler: this.onNewScope,
        icon: 'Content/img/16x16/document-new.png',
        scope: this
      }
    ]
  },

  buildScopeMenu: function () {
    return [
      {
        xtype: 'button',
        text: 'Edit Scope',
        handler: this.onEditScope,
        icon: 'Content/img/16x16/document-properties.png',
        scope: this
      },
      {
        xtype: 'button',
        text: 'Delete Scope',
        handler: this.onDeleteScope,
        icon: 'Content/img/16x16/edit-delete.png',
        scope: this
      },
      {
        xtype: 'menuseparator'
      },
      {
        xtype: 'button',
        text: 'New Application',
        handler: this.onNewApplication,
        icon: 'Content/img/16x16/document-new.png',
        scope: this
      }
    ]
  },

  buildApplicationMenu: function () {
    return [
      {
        xtype: 'button',
        text: 'Edit Application',
        handler: this.onEditApplication,
        icon: 'Content/img/16x16/document-properties.png',
        scope: this
      },
      {
        xtype: 'button',
        text: 'Delete Application',
        handler: this.onDeleteApplication,
        icon: 'Content/img/16x16/edit-delete.png',
        scope: this
      },
      {
        xtype: 'menuseparator'
      },
      {
        xtype: 'button',
        text: 'Open Configuration',
        handler: this.onConfigure,
        icon: 'Content/img/16x16/preferences-system.png',
        scope: this
      }
    ]
  },

  buildAppDataMenu: function () {
    return [
      {
        xtype: 'button',
        text: 'Open Grid',
        handler: this.onLoadPageDto,
        icon: 'Content/img/16x16/document-properties.png',
        scope: this
      }
    ]
  },

  buildvalueListsMenu: function () {
    return [
    {
      xtype: 'button',
      text: 'New Value List',
      handler: this.onNewValueList,
      icon: 'Content/img/16x16/document-new.png',
      scope: this
    }
    ]
  },

  buildvalueListMenu: function () {
    return [
    {
      xtype: 'button',
      text: 'Edit Value List Name',
      handler: this.onEditValueList,
      icon: 'Content/img/16x16/document-properties.png',
      scope: this
    },
    {
      xtype: 'button',
      text: 'Delete ValueList',
      handler: this.onDeleteValueList,
      icon: 'Content/img/16x16/edit-delete.png',
      scope: this
    },
    {
      xtype: 'menuseparator'
    },
    {
      xtype: 'button',
      text: 'New Value Map',
      handler: this.onNewValueListMap,
      icon: 'Content/img/16x16/document-new.png',
      scope: this
    }
    ]
  },

  buildvalueListMapMenu: function () {
    return [
    {
      xtype: 'button',
      text: 'Edit Value List Map',
      handler: this.onEditValueListMap,
      icon: 'Content/img/16x16/document-properties.png',
      scope: this
    },
    {
      xtype: 'button',
      text: 'Delete Value List Map',
      handler: this.onDeleteValueListMap,
      icon: 'Content/img/16x16/edit-delete.png',
      scope: this
    }
    ]
  },


  buildGraphsMenu: function () {
    return [
    {
      xtype: 'button',
      text: 'New GraphMap',
      handler: this.onNewGraphMap,
      icon: 'Content/img/16x16/document-new.png',
      scope: this
    }
    ]
  },

  buildGraphMenu: function () {
    return [
     {
       xtype: 'button',
       text: 'Edit GraphMap',
       handler: this.onEditGraphMap,
       icon: 'Content/img/16x16/document-properties.png',
       scope: this
     },
      {
        xtype: 'button',
        text: 'Delete GraphMap',
        handler: this.onDeleteGraphMap,
        icon: 'Content/img/16x16/edit-delete.png',
        scope: this
      },
      {
        xtype: 'menuseparator'
      },
    {
      xtype: 'button',
      text: 'Open GraphMap',
      handler: this.onOpenGraphMap,
      icon: 'Content/img/16x16/mapping.png',
      scope: this
    }]
  },

  onConfigure: function () {
    this.fireEvent('configure', this, selectedDirectoryNode);
    this.applicationMenu.hide();
  },

  onSelect: function (selModel, model, idx) {
    if (model.store) {
      selectedDirectoryNode = model.store.getAt(idx);
    }
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
    // this.directoryPanel.getSelectionModel().select(node);
    //this.onClick(node);
    //}
  },

  onNewScope: function (btn, ev) {
    this.fireEvent('NewScope', this, selectedDirectoryNode);
    this.scopesMenu.hide();
  },


  onNewValueList: function (btn, ev) {
    this.fireEvent('NewValueList', this, selectedDirectoryNode);
    this.valueListsMenu.hide();
  },

  onEditValueList: function (btn, ev) {
    this.fireEvent('EditValueList', this, selectedDirectoryNode);
    this.valueListMenu.hide();
  },

  onNewGraphMap: function (btn, ev) {
    this.fireEvent('NewGraphMap', this, selectedDirectoryNode);
  },

  onEditGraphMap: function (btn, ev) {
    this.fireEvent('EditGraphMap', this, selectedDirectoryNode);
    this.graphMenu.hide();
  },


  onNewValueListMap: function (btn, ev) {
    this.fireEvent('NewValueListMap', this, selectedDirectoryNode);
    this.valueListsMenu.hide();
  },

  onEditValueListMap: function (btn, ev) {
    this.fireEvent('EditValueListMap', this, selectedDirectoryNode);
    this.valueListsMenu.hide();
  },

  onDeleteValueList: function (btn, e) {
    this.valueListMenu.hide();
    var that = this;
    var node = selectedDirectoryNode;
    Ext.Ajax.request({
      url: 'mapping/deletevaluelist',
      method: 'POST',
      params: {
        mappingNode: node.data.id,
        valueList: node.data.id.split('/')[4]
      },
      success: function (result, request) {

        that.onReload();
      },
      failure: function (result, request) { }
    })
  },

  onDeleteValueListMap: function (btn, e) {
    this.valueListsMenu.hide();
    var that = this;
    var node = selectedDirectoryNode;
    Ext.Ajax.request({
      url: 'mapping/deleteValueMap',
      method: 'POST',
      params: {
        mappingNode: node.data.id,
        oldClassUrl: node.data.record.uri
      },
      success: function (result, request) {

        that.onReload();
      },
      failure: function (result, request) { }
    })
  },

  onDeleteGraphMap: function (btn, e) {
    this.graphMenu.hide();
    var that = this;
    var node = selectedDirectoryNode;
    Ext.Ajax.request({
      url: 'mapping/deletegraphmap',
      method: 'POST',
      params: {
        scope: node.data.id.split('/')[0],
        application: node.data.id.split('/')[1],
        mappingNode: node.data.id
      },
      success: function (result, request) {

        that.onReload();
      },
      failure: function (result, request) { }
    })
  },

  onClose: function (btn, e) {
    if (btn != undefined) {
      var win = btn.findParentByType('window');
      if (win != undefined)
        win.close();
    }
  },


  onOpenGraphMap: function (btn, e) {
    this.fireEvent('opengraphmap', this, selectedDirectoryNode);
    this.graphMenu.hide();
  },

  onNewApplication: function (btn, ev) {
    this.fireEvent('NewApplication', this, selectedDirectoryNode);
    this.applicationMenu.hide();
  },

  onEditScope: function (btn, ev) {
    this.fireEvent('EditScope', this, selectedDirectoryNode);
    this.scopeMenu.hide();
  },

  onEditApplication: function (btn, ev) {
    this.fireEvent('EditApplication', this, selectedDirectoryNode);
    this.applicationMenu.hide();
  },

  onLoadPageDto: function (btn, ev) {
    var node = selectedDirectoryNode;
    this.fireEvent('LoadPageDto', this, node);
    this.appDataMenu.hide();
  },

  onDeleteScope: function (btn, ev) {
    this.fireEvent('DeleteScope', this, selectedDirectoryNode);
    this.scopeMenu.hide();
  },

  onDeleteApplication: function (btn, ev) {
    this.fireEvent('DeleteApplication', this, selectedDirectoryNode);
    this.applicationMenu.hide();
  },

  onReload: function (node) {

    var panel = this.directoryPanel;
    var thisTreePanel = Ext.getCmp('Directory-Panel');

    //get state from tree
    var state = thisTreePanel.getState();
    panel.body.mask('Loading', 'x-mask-loading');

    thisTreePanel.getStore().load();
    panel.body.unmask();
    thisTreePanel.applyState(state, true);

    Ext.Ajax.request({
      url: 'GridManager/cleanSession',
      method: 'GET',
      success: function (response, request) {
      },
      failure: function (response, request) {
      }
    });

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
    if (this.directoryPanel.store.getById(nodeId)) { //if nodeID exists it will find out NODE
      return this.directoryPanel.store.getById(nodeId)
    } else {
      return false;
    }
  },

  reload: function () {
    this.directoryPanel.root.reload();
  },

  //  onDoubleClick: function (node) {
  //    if (node.attributes.type == 'GraphsNode') {
  //      this.AddGraphMap(this);
  //    } else if (node.attributes.type == 'GraphNode') {
  //      this.fireEvent('opengraphmap', this, node);
  //    }
  //  },

  onClick: function (view, model, n, idx, e) {
    try {
      var obj = model.store.getAt(idx);

      this.propertyPanel.setSource(obj.data.property);
      if (this.contextButton)
        this.contextButton.menu.removeAll();

      if (obj.data.type == "ScopesNode") {
        this.contextButton.menu.add(this.buildScopesMenu());
      }
      else if (obj.type == "ScopeNode") {
        this.contextButton.menu.add(this.buildScopeMenu());
      }
      else if (obj.data.type == "ApplicationNode") {
        this.contextButton.menu.add(this.buildApplicationMenu());
      }
      else if (obj.data.type == "DataObjectNode") {
        this.contextButton.menu.add(this.buildAppDataMenu());
      }
      else if (obj.data.type == "ValueListsNode") {
        this.contextButton.menu.add(this.buildvalueListsMenu());
      }
      else if (obj.data.type == "ValueListNode") {
        this.contextButton.menu.add(this.buildvalueListMenu());
      }
      else if (obj.data.type == "ListMapNode") {
        this.contextButton.menu.add(this.buildvalueListMapMenu());
      }
      else if (obj.data.type == "GraphsNode") {
        this.contextButton.menu.add(this.buildGraphsMenu());
      }
      else if (obj.data.type == "GraphNode") {
        this.contextButton.menu.add(this.buildGraphMenu());
      }
    } catch (e) {
      //  alert(e);
    }
  }
});