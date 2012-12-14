﻿Ext.ns('AdapterManager');

/**
* @class AdapterManager.directoryPanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
AdapterManager.DirectoryPanel = Ext.extend(Ext.Panel, {
  title: 'Directory',
  width: 220,

  //collapseMode: 'mini',
  collapsible: true,
  collapsed: false,

  layout: 'border',
  border: false,
  split: true,
  contentPanel: null,
  navigationUrl: null,
  directoryPanel: null,
  // contextButton: null,
  scopesMenu: null,
  scopeMenu: null,
  valueListsMenu: null,
  valueListMenu: null,
  graphsMenu: null,
  graphMenu: null,
  applicationMenu: null,
  rootNode: null,
  treeLoader: null,
  propertyPanel: null,
  /**
  * initComponent
  * @protected
  */
  initComponent: function () {

    this.addEvents({
      NewScope: true,
      NewApplication: true,
      DataLayer: true,
      EditScope: true,
      EditApplication: true,
      OpenMapping: true,
      DeleteScope: true,
      DeleteApplication: true,
      ReloadScopes: true,
      ReloadNode: true
    });

    this.tbar = new Ext.Toolbar();
    this.tbar.add(this.buildToolbar());
    // this.tbar.add(this.contextButton);

    this.scopesMenu = new Ext.menu.Menu();
    this.scopesMenu.add(this.buildScopesMenu());

    this.scopeMenu = new Ext.menu.Menu();
    this.scopeMenu.add(this.buildScopeMenu());

    this.applicationMenu = new Ext.menu.Menu();
    this.applicationMenu.add(this.buildApplicationMenu());

    this.spAppMenu = new Ext.menu.Menu();
    this.spAppMenu.add(this.buildRefreshCacingMenu());
    this.spAppMenu.add(this.buildAppDataMenu());

    this.appDataMenu = new Ext.menu.Menu();
    this.appDataMenu.add(this.buildAppDataMenu());

    this.valueListsMenu = new Ext.menu.Menu();
    this.valueListsMenu.add(this.buildvalueListsMenu());

    this.valueListMenu = new Ext.menu.Menu();
    this.valueListMenu.add(this.buildvalueListMenu());

    this.valueListMapMenu = new Ext.menu.Menu();
    this.valueListMapMenu.add(this.buildvalueListMapMenu());

    this.graphMenu = new Ext.menu.Menu();
    this.graphMenu.add(this.buildGraphMenu());

    this.treeLoader = new Ext.tree.TreeLoader({
      timeout: 1800000,  // 30 minutes
      baseParams: { type: null, related: null, datalayer: null },
      url: this.navigationUrl
    });

    this.treeLoader.on("beforeload", function (treeLoader, node) {
      treeLoader.baseParams.type = node.attributes.type;
      if (node.attributes.record != undefined) {
        if (node.attributes.record.Related != undefined)
          treeLoader.baseParams.related = node.attributes.record.Related;

        if (node.attributes.record.DataLayer != undefined)
          treeLoader.baseParams.datalayer = node.attributes.record.DataLayer;
      }
    }, this);

    this.rootNode = new Ext.tree.AsyncTreeNode({
      id: 'root',
      text: 'Scopes',
      expanded: true,
      draggable: true,
      icon: 'Content/img/internet-web-browser.png',
      type: 'ScopesNode'
    });

    this.propertyPanel = new Ext.grid.PropertyGrid({
      title: 'Details',
      region: 'south',
      layout: 'fit',
      height: 250,
      stripeRows: true,
      collapsible: true,
      autoScroll: true,
      border: 0,
      frame: false,
      selModel: new Ext.grid.RowSelectionModel({ singleSelect: true }),
      // bodyStyle: 'padding-bottom:15px;background:#eee;',
      source: {},
      listeners: {
        beforeedit: function (e) {
          e.cancel = true;
        },
        // to copy but not edit content of property grid				
        afteredit: function (e) {
          e.grid.getSelectionModel().selections.items[0].data.value = e.originalValue;
          e.record.data.value = e.originalValue;
          e.value = e.originalValue;
          e.grid.getView().refresh();
        }
      }
    });

    this.directoryPanel = new Ext.tree.TreePanel({
      enableDrag: true,
      id: 'Directory-Panel',
      //forceLayout: true,
      ddGroup: 'propertyGroup',
      region: 'center',
      border: false,
      expandAll: true,
      // draggable: true,
      // ddScroll: true,
      rootVisible: true,
      animate: true,
      enableDD: false,
      containerScroll: true,
      pathSeparator: '>',
      lines: true,
      tbar: undefined,
      autoScroll: true,
      //singleExpand: true,     
      loader: this.treeLoader,
      root: this.rootNode,
      stateEvents: ['collapsenode', 'expandnode'],
      stateId: 'tree-panel-state-id',
      stateful: true,
      getState: function () {
        var nodes = [];
        this.getRootNode().eachChild(function (child) {
          //function to store state of tree recursively
          var storeTreeState = function (node, expandedNodes) {
            if (node.isExpanded() && node.childNodes.length > 0) {
              expandedNodes.push(node.getPath());
              node.eachChild(function (child) {
                storeTreeState(child, expandedNodes);
              });
            }
          };
          storeTreeState(child, nodes);
        });

        return {
          expandedNodes: nodes
        }
      },
      applyState: function (state, isOnClick) {
        var that = this;
        //this.getLoader().on('load', function () {
        if (isOnClick == true) {
          var nodes = state.expandedNodes;
          for (var i = 0; i < nodes.length; i++) {
            if (typeof nodes[i] != 'undefined') {
              that.expandPath(nodes[i]);
            }
          }
        }
        //});
      }
    });

    this.directoryPanel.on('contextmenu', this.showContextMenu, this);
    this.directoryPanel.on('click', this.onClick, this);
    this.directoryPanel.on('dblclick', this.onDoubleClick, this);
    this.directoryPanel.on('newgraphmap', this.newGraphmap, this);

    this.items = [
      this.directoryPanel,
      this.propertyPanel
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

  getSelectedNode: function () {
    var selectedNode = this.directoryPanel.getSelectionModel().getSelectedNode();
    return selectedNode;
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
    //      ,
    //      {
    //          xtype: 'menuseparator'
    //      }
    ]
  },

  buildScopesMenu: function () {
    return [
      {
        text: 'New Scope',
        handler: this.onNewScope,
        icon: 'Content/img/16x16/document-new.png',
        scope: this
      },
      ///TODO: Pending on testing, do not delete
//      {
//          text: 'Add/Update DataLayer',
//          handler: this.onEditDataLayer,
//          icon: 'Content/img/16x16/document-new.png',
//          scope: this
//      },
      {
        text: 'Regenerate HibernateDataLayer artifacts',
        handler: this.onRegenerateAll,
        icon: 'Content/img/16x16/document-new.png',
        scope: this
      }
    ]
  },

  buildScopeMenu: function () {
    return [
      {

        text: 'Edit Scope',
        handler: this.onEditScope,
        icon: 'Content/img/16x16/document-properties.png',
        scope: this
      },
      {
        text: 'Delete Scope',
        handler: this.onDeleteScope,
        icon: 'Content/img/16x16/edit-delete.png',
        scope: this
      },
      {
        xtype: 'menuseparator'
      },
      {
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
        text: 'Edit Application',
        handler: this.onEditApplication,
        icon: 'Content/img/16x16/document-properties.png',
        scope: this
      },
      {
        text: 'Delete Application',
        handler: this.onDeleteApplication,
        icon: 'Content/img/16x16/edit-delete.png',
        scope: this
      },
      {
        xtype: 'menuseparator'
      },
      {
        text: 'Open Configuration',
        handler: this.onConfigure,
        icon: 'Content/img/16x16/preferences-system.png',
        scope: this
      }
    ]
  },

  buildRefreshCacingMenu: function () {
    return [
      {
        text: 'Refresh All DataObjects',
        handler: this.onRefresh,
        icon: 'Content/img/16x16/preferences-system.png',
        tooltip: 'It may take about 30 minutes to create/refresh caching tables.',
        scope: this
      },
      {
        text: 'Refresh One DataObject',
        handler: this.onRefreshOne,
        icon: 'Content/img/16x16/preferences-system.png',
        tooltip: 'It may take about 30 minutes to create/refresh caching tables.',
        scope: this
      }
    ]
  },

  buildAppDataMenu: function () {
    return [
            {
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
      text: 'Edit Value List Name',
      handler: this.onEditValueList,
      icon: 'Content/img/16x16/document-properties.png',
      scope: this
    },
    {
      text: 'Delete ValueList',
      handler: this.onDeleteValueList,
      icon: 'Content/img/16x16/edit-delete.png',
      scope: this
    },
    {
      xtype: 'menuseparator'
    },
    {
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
      text: 'Edit Value List Map',
      handler: this.onEditValueListMap,
      icon: 'Content/img/16x16/document-properties.png',
      scope: this
    },
    {
      text: 'Delete Value List Map',
      handler: this.onDeleteValueListMap,
      icon: 'Content/img/16x16/edit-delete.png',
      scope: this
    }
    ]
  },


  buildGraphsMenu: function (node) {
    var scope = node.parentNode.parentNode.text;
    var application = node.parentNode.text

    return [
    {
      text: 'New GraphMap',
      handler: this.onNewGraphMap,
      icon: 'Content/img/16x16/document-new.png',
      scope: this
    },
    {
      xtype: 'hrefitem',
      href: '/mapping/export/' + scope + '/' + application,
      hrefTarget: '_blank',
      html: 'Export Graphs',
      icon: 'Content/img/16x16/preferences-system.png',
      scope: this
    }
    ]
  },

  buildGraphMenu: function () {
    return [
         {
           text: 'Refresh Facade',
           handler: this.onRefreshFacade,
           icon: 'Content/img/table_refresh.png',
           scope: this
         },
         {
           xtype: 'menuseparator'
         },
     {
       text: 'Edit GraphMap',
       handler: this.onEditGraphMap,
       icon: 'Content/img/16x16/document-properties.png',
       scope: this
     },
      {
        text: 'Delete GraphMap',
        handler: this.onDeleteGraphMap,
        icon: 'Content/img/16x16/edit-delete.png',
        scope: this
      },
      {
        xtype: 'menuseparator'
      },
    {
      text: 'Open GraphMap',
      handler: this.onOpenGraphMap,
      icon: 'Content/img/16x16/mapping.png',
      scope: this
    }]
  },

  onConfigure: function () {
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();
    this.fireEvent('configure', this, node);
  },

  showContextMenu: function (node, event) {

    //  if (node.isSelected()) { 
    var x = event.browserEvent.clientX;
    var y = event.browserEvent.clientY;

    var obj = node.attributes;

    if (obj.type == "ScopesNode") {
      this.scopesMenu.showAt([x, y]);
    } else if (obj.type == "ScopeNode") {
      this.scopeMenu.showAt([x, y]);
    } else if (obj.type == "ApplicationNode") {
      this.applicationMenu.showAt([x, y]);
    } else if (obj.type == "DataObjectNode") {
      if (obj.record.DataLayer.indexOf('SP3D') > -1)
        this.spAppMenu.showAt([x, y]);
      else
        this.appDataMenu.showAt([x, y]);
    } else if (obj.type == "ValueListsNode") {
      this.valueListsMenu.showAt([x, y]);
    } else if (obj.type == "ValueListNode") {
      this.valueListMenu.showAt([x, y]);
    } else if (obj.type == "ListMapNode") {
      this.valueListMapMenu.showAt([x, y]);
    } else if (obj.type == "GraphsNode") {
      var menu = new Ext.menu.Menu();
      menu.add(this.buildGraphsMenu(node));
      menu.showAt([x, y]);
    } else if (obj.type == "GraphNode") {
      this.graphMenu.showAt([x, y]);
    }
    this.directoryPanel.getSelectionModel().select(node);
    this.onClick(node);
    //}
  },

  onNewScope: function (btn, ev) {
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();
    this.fireEvent('NewScope', this, node);
  },
  
  onEditDataLayer: function (btn, ev) {
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();
    this.fireEvent('EditDataLayer', this, node);
  },

  onRegenerateAll: function (btn, ev) {
    Ext.Ajax.request({
      url: 'AdapterManager/RegenAll',
      method: 'GET',
      success: function (result, request) {
        var responseObj = Ext.decode(result.responseText);
        var msg = '';

        for (var i = 0; i < responseObj.StatusList.length; i++) {
          var status = responseObj.StatusList[i];

          if (msg != '') {
            msg += '\r\n';
          }

          msg += status.Identifier + ':\r\n';

          for (var j = 0; j < status.Messages.length; j++) {
            msg += '    ' + status.Messages[j] + '\r\n';
          }
        }

        showDialog(600, 340, 'NHibernate Regeneration Result', msg, Ext.Msg.OK, null);
      },
      failure: function (result, request) {
        var msg = result.responseText;
        showDialog(500, 240, 'NHibernate Regeneration Error', msg, Ext.Msg.OK, null);
      }
    })
  },

  onNewValueList: function (btn, ev) {
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();
    this.fireEvent('NewValueList', this, node);
  },

  onEditValueList: function (btn, ev) {
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();
    this.fireEvent('EditValueList', this, node);
  },

  onNewGraphMap: function (btn, ev) {
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();
    this.fireEvent('NewGraphMap', this, node);
  },

  onEditGraphMap: function (btn, ev) {
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();
    this.fireEvent('EditGraphMap', this, node);
  },

  onRefreshFacade: function (btn, ev) {
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();
    this.fireEvent('RefreshFacade', this, node);
  },

  onNewValueListMap: function (btn, ev) {
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();
    this.fireEvent('NewValueListMap', this, node);
  },

  onEditValueListMap: function (btn, ev) {
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();
    this.fireEvent('EditValueListMap', this, node);
  },

  onDeleteValueList: function (btn, e) {
    var that = this;
    var node = this.getSelectedNode();
    Ext.Ajax.request({
      url: 'mapping/deletevaluelist',
      method: 'POST',
      params: {
        mappingNode: node.id,
        valueList: node.id.split('/')[4]
      },
      success: function (result, request) {
        //Ext.Msg.show({ title: 'Success', msg: 'ValueList [' + node.id.split('/')[4] + '] removed from mapping', icon: Ext.MessageBox.INFO, buttons: Ext.MessageBox.OK });
        that.onReload();
      },
      failure: function (result, request) { }
    })
  },

  onDeleteValueListMap: function (btn, e) {
    var that = this;
    var node = this.getSelectedNode();
    Ext.Ajax.request({
      url: 'mapping/deleteValueMap',
      method: 'POST',
      params: {
        mappingNode: node.id,
        oldClassUrl: node.attributes.record.uri
      },
      success: function (result, request) {
        //Ext.Msg.show({ title: 'Success', msg: 'ValueList [' + node.id.split('/')[4] + '] removed from mapping', icon: Ext.MessageBox.INFO, buttons: Ext.MessageBox.OK });
        that.onReload();
      },
      failure: function (result, request) { }
    })
  },

  onDeleteGraphMap: function (btn, e) {
    var that = this;
    var node = this.getSelectedNode();
    Ext.Ajax.request({
      url: 'mapping/deletegraphmap',
      method: 'POST',
      params: {
        scope: node.id.split('/')[0],
        application: node.id.split('/')[1],
        mappingNode: node.id
      },
      success: function (result, request) {
        //Ext.Msg.show({ title: 'Success', msg: 'Graph [' + node.id.split('/')[4] + '] removed from mapping', icon: Ext.MessageBox.INFO, buttons: Ext.MessageBox.OK });
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
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();
    this.fireEvent('opengraphmap', this, node);
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

  onLoadPageDto: function (btn, ev) {
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();
    this.fireEvent('LoadPageDto', this, node);
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
    //this.directoryPanel.root.reload();

    var panel = this.directoryPanel;
    var thisTreePanel = Ext.getCmp('Directory-Panel');

    //get state from tree
    var state = thisTreePanel.getState();
    panel.body.mask('Loading', 'x-mask-loading');

    thisTreePanel.getLoader().load(thisTreePanel.getRootNode(), function () {
      panel.body.unmask();
      thisTreePanel.applyState(state, true);
    });
  },

  onRefreshOne: function (btn, ev) {
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();
    var panel = this.directoryPanel;
    panel.body.mask('Loading', 'x-mask-loading');

    Ext.Ajax.request({
      url: 'AdapterManager/Refresh',
      method: 'POST',
      params: {
        'nodeid': node.attributes.id,
        'type': 'one'
      },
      success: function (result, request) {
        this.directoryPanel.root.reload();
        showDialog(450, 100, 'Refreshing/creating caching tables Result', 'Caching tables are refreshed/created successfully.', Ext.Msg.OK, null);
        panel.body.unmask();
      },
      failure: function (result, request) {        
        var msg = result.responseText;
        showDialog(500, 240, 'Error in refreshing/creating caching tables', msg, Ext.Msg.OK, null);
        panel.body.unmask();
      }
    })
  },

  onRefresh: function (btn, ev) {
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();
    var panel = this.directoryPanel;
    panel.body.mask('Loading', 'x-mask-loading');

    Ext.Ajax.request({
      url: 'AdapterManager/Refresh',
      method: 'POST',
      params: {
        'nodeid': node.attributes.id,
        'type': 'all'
      },
      success: function (result, request) {
        this.directoryPanel.root.reload();
        showDialog(450, 100, 'Refreshing/creating caching tables Result', 'Caching tables are refreshed/created successfully.', Ext.Msg.OK, null);
        panel.body.unmask();
      },
      failure: function (result, request) {        
        var msg = result.responseText;
        showDialog(500, 240, 'Error in refreshing/creating caching tables', msg, Ext.Msg.OK, null);
        panel.body.unmask();
      }
    })
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
  },

  onDoubleClick: function (node) {
    if (node.attributes.type == 'GraphsNode') {
      this.AddGraphMap(this);
    }
    else if (node.attributes.type == 'GraphNode') {
      this.fireEvent('opengraphmap', this, node);
    }
    else if (node.attributes.type == 'DataObjectNode') {
      this.fireEvent('LoadPageDto', this, node);
    }
  },

  onClick: function (node) {
    try {
      var obj = node.attributes;
      this.propertyPanel.setSource(node.attributes.property);
      if (this.contextButton)
        this.contextButton.menu.removeAll();

      if (obj.type == "ScopesNode") {
        this.contextButton.menu.add(this.buildScopesMenu());
      }
      else if (obj.type == "ScopeNode") {
        this.contextButton.menu.add(this.buildScopeMenu());
      }
      else if (obj.type == "ApplicationNode") {
        this.contextButton.menu.add(this.buildApplicationMenu());
      }
      else if (obj.type == "DataObjectNode") {
        this.contextButton.menu.add(this.buildAppDataMenu());
      }
      else if (obj.type == "ValueListsNode") {
        this.contextButton.menu.add(this.buildvalueListsMenu());
      }
      else if (obj.type == "ValueListNode") {
        this.contextButton.menu.add(this.buildvalueListMenu());
      }
      else if (obj.type == "ListMapNode") {
        this.contextButton.menu.add(this.buildvalueListMapMenu());
      }
      else if (obj.type == "GraphsNode") {
        this.contextButton.menu.add(this.buildGraphsMenu());
      }
      else if (obj.type == "GraphNode") {
        this.contextButton.menu.add(this.buildGraphMenu());
      }
    } catch (e) {
      //  alert(e);
    }
  }
});