Ext.ns('AdapterManager');

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
  contextMenu: null,
  scopesMenu: null,
  scopeMenu: null,
  valueListsMenu: null,
  valueListMenu: null,
  graphsMenu: null,
  graphMenu: null,
  applicationMenu: null,
  dataObjectsMenu: null,
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
      RefreshDataObjects: true,
      OpenGraphMap: true,
      DeleteScope: true,
      DeleteApplication: true,
      ReloadScopes: true,
      ReloadNode: true
    });

    this.tbar = new Ext.Toolbar();
    this.tbar.add(this.buildToolbar());

    this.scopesMenu = new Ext.menu.Menu();
    this.scopesMenu.add(this.buildScopesMenu());

    this.scopeMenu = new Ext.menu.Menu();
    this.scopeMenu.add(this.buildScopeMenu());

    this.applicationMenu = new Ext.menu.Menu();
    this.applicationMenu.add(this.buildApplicationMenu());

    this.spAppMenu = new Ext.menu.Menu();
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
      baseParams: { type: null, related: null, datalayer: null, refresh: false },
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
        icon: 'Content/img/16x16/refresh.png',
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
      icon: 'Content/img/16x16/regen.png',
      scope: this
    }
    ]
  },

  buildScopeMenu: function () {
    return [
      {
        text: 'Edit Scope',
        handler: this.onEditScope,
        icon: 'Content/img/16x16/edit.png',
        scope: this
      },
      {
        text: 'Delete Scope',
        handler: this.onDeleteScope,
        icon: 'Content/img/16x16/delete.png',
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
        icon: 'Content/img/16x16/edit.png',
        scope: this
      },
      {
        text: 'Delete Application',
        handler: this.onDeleteApplication,
        icon: 'Content/img/16x16/delete.png',
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
      },
      {
        xtype: 'menuseparator'
      },
      {
        text: 'Upload File',
        handler: this.onFileUpload,
        icon: 'Content/img/16x16/document-down.png',
        scope: this
      },
	    {
	      text: 'Download File',
	      handler: this.onFileDownload,
	      icon: 'Content/img/16x16/document-up.png',
	      scope: this
	    }
    ]
  },

  buildAppDataMenu: function () {
    return [
            {
              text: 'View Application Data',
              handler: this.onLoadAppData,
              icon: 'Content/img/16x16/view.png',
              scope: this
            }
        ]
  },

  buildDataObjectsMenu: function (node) {
    var menu = new Ext.menu.Menu();

    menu.add({
      text: 'Refresh Dictionary',
      handler: this.onRefreshDictionary,
      icon: 'Content/img/16x16/refresh.png',
      scope: this
    });

    if (node.attributes.property["Data Mode"] == "Live") {
      if (node.parentNode.attributes.property["LightweightDataLayer"] == "No") {
        menu.add({
          text: 'Switch to Cache Mode',
          handler: this.onSwitchToCachedDataMode,
          icon: 'Content/img/16x16/switch.png',
          scope: this
        });
      }
    }
    else if (node.parentNode.attributes.property["LightweightDataLayer"] == "No") {
      menu.add({
        text: 'Switch to Live Mode',
        handler: this.onSwitchToLiveDataMode,
        icon: 'Content/img/16x16/switch.png',
        scope: this
      });
    }

    if (node.attributes.property["Data Mode"] != "Live") {
      menu.add([
        {
          xtype: 'menuseparator'
        },
        {
          text: 'Refresh Cache',
          handler: this.onRefreshCache,
          icon: 'Content/img/16x16/refresh.png',
          scope: this
        },
        {
          text: 'Import Cache',
          handler: this.onImportCache,
          icon: 'Content/img/16x16/import.png',
          scope: this
        }
      ]);
    }

    return menu;
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
      icon: 'Content/img/16x16/edit.png',
      scope: this
    },
    {
      text: 'Delete ValueList',
      handler: this.onDeleteValueList,
      icon: 'Content/img/16x16/delete.png',
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
      icon: 'Content/img/16x16/edit.png',
      scope: this
    },
    {
      text: 'Delete Value List Map',
      handler: this.onDeleteValueListMap,
      icon: 'Content/img/16x16/delete.png',
      scope: this
    }
    ]
  },


  buildGraphsMenu: function (node) {
    var scope = node.parentNode.parentNode.text;
    var application = node.parentNode.text

    return [{
      text: 'New GraphMap',
      handler: this.onNewGraphMap,
      icon: 'Content/img/16x16/document-new.png',
      scope: this
    }]
  },

  buildGraphMenu: function () {
    return [{
       text: 'Edit GraphMap',
       handler: this.onEditGraphMap,
       icon: 'Content/img/16x16/edit.png',
       scope: this
     },
      {
        text: 'Delete GraphMap',
        handler: this.onDeleteGraphMap,
        icon: 'Content/img/16x16/delete.png',
        scope: this
      },
      {
        xtype: 'menuseparator'
      },
    {
      text: 'Configure GraphMap',
      handler: this.onOpenGraphMap,
      icon: 'Content/img/16x16/configure.png',
      scope: this
    }]
  },

  onConfigure: function () {
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();
    this.fireEvent('configure', this, node);
  },
  onFileUpload: function () {
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();
    this.fireEvent('upload', this, node);
  },
  onFileDownload: function () {
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();
    this.fireEvent('download', this, node);
  },
  showContextMenu: function (node, event) {

    //  if (node.isSelected()) { 
    var x = event.browserEvent.clientX;
    var y = event.browserEvent.clientY;

    var obj = node.attributes;
    this.directoryPanel.getSelectionModel().select(node);
    this.onClick(node);

    if (obj.type == "ScopesNode") {
      this.scopesMenu.showAt([x, y]);
    } else if (obj.type == "ScopeNode") {
      this.scopeMenu.showAt([x, y]);
    } else if (obj.type == "ApplicationNode") {
      this.applicationMenu.showAt([x, y]);
    } else if (obj.type == "DataObjectsNode") {
      this.dataObjectsMenu.showAt([x, y]);
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
    this.fireEvent('OpenGraphMap', this, node);
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

  onLoadAppData: function (btn, ev) {
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();
    this.fireEvent('LoadAppData', this, node);
  },

  onRefreshDictionary: function (btn, ev) {
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();

    try {
      this.treeLoader.baseParams.refresh = true;
      node.reload();
      this.treeLoader.baseParams.refresh = false;
    }
    catch (err) {
      showDialog(400, 100, 'Refresh Error', err.Message, Ext.Msg.OK, null);
    }
  },

  onSwitchToLiveDataMode: function (btn, ev) {
    this.switchDataMode('Live');
  },

  onSwitchToCachedDataMode: function (btn, ev) {
    this.switchDataMode('Cache');
  },

  switchDataMode: function (mode) {
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();
    Ext.getCmp('content-panel').getEl().mask('Processing...', 'x-mask-loading');

    Ext.Ajax.request({
      url: 'AdapterManager/SwitchDataMode',
      method: 'POST',
      timeout: 3600000,
      params: {
        'nodeid': node.attributes.id,
        'mode': mode
      },
      success: function (response, request) {
        var responseObj = Ext.decode(response.responseText);

        if (responseObj.Level == 0) {
          //showDialog(450, 100, 'Result', 'Data Mode switched to [' + mode + '].', Ext.Msg.OK, null);
        }
        else {
          showDialog(500, 160, 'Result', responseObj.Messages.join('\n'), Ext.Msg.OK, null);
        }

        node.parentNode.reload();
        Ext.getCmp('content-panel').getEl().unmask();
      },
      failure: function (response, request) {
        showDialog(500, 160, 'Error', responseObj.Messages.join('\n'), Ext.Msg.OK, null);
        node.parentNode.reload();
        Ext.getCmp('content-panel').getEl().unmask();
      }
    });
  },

  onRefreshCache: function (btn, ev) {
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();
    Ext.getCmp('content-panel').getEl().mask('Processing...', 'x-mask-loading');

    Ext.Ajax.request({
      url: 'AdapterManager/RefreshCache',
      method: 'POST',
      timeout: 3600000,
      params: {
        'nodeid': node.attributes.id
      },
      success: function (response, request) {
        var responseObj = Ext.decode(response.responseText);

        if (responseObj.Level == 0) {
          showDialog(450, 100, 'Refresh Cache Result', 'Cache refreshed successfully.', Ext.Msg.OK, null);
        }
        else {
          showDialog(500, 160, 'Refresh Cache Error', responseObj.Messages.join('\n'), Ext.Msg.OK, null);
        }

        node.parentNode.reload();
        Ext.getCmp('content-panel').getEl().unmask();
      },
      failure: function (response, request) {
        showDialog(500, 160, 'Refresh Cache Error', responseObj.Messages.join('\n'), Ext.Msg.OK, null);
        node.parentNode.reload();
        Ext.getCmp('content-panel').getEl().unmask();
      }
    });
  },

  onImportCache: function (btn, ev) {
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();

    var importCacheForm = new Ext.FormPanel({
      url: 'AdapterManager/ImportCache',
      timeout: 3600000,
      method: 'POST',
      frame: false,
      border: false,
      bodyStyle: 'padding:20px 5px 20px 5px',
      items: [
        { fieldLabel: 'Cache URI', name: 'cacheUri', xtype: 'textfield', width: 360, allowBlank: false },
        { name: 'nodeid', xtype: 'hidden', value: node.attributes.id }
      ]
    });

    var win = new Ext.Window({
      title: 'Import Cache',
      width: 500,
      modal: true,
      closable: true,
      resizable: false,
      items: [importCacheForm],
      buttons: [{
        text: 'Submit',
        handler: function () {
          importCacheForm.getForm().submit({
            success: function (response, request) {
              var responseObj = Ext.decode(response.responseText);

              if (responseObj.Level == 0) {
                showDialog(450, 100, 'Import Cache Result', 'Cache refreshed successfully.', Ext.Msg.OK, null);
              }
              else {
                showDialog(500, 160, 'Import Cache Error', responseObj.Messages.join('\n'), Ext.Msg.OK, null);
              }

              node.parentNode.reload();
              Ext.getCmp('content-panel').getEl().unmask();
            },
            failure: function (response, request) {
              showDialog(500, 160, 'Import Cache Error', responseObj.Messages.join('\n'), Ext.Msg.OK, null);
              node.parentNode.reload();
              Ext.getCmp('content-panel').getEl().unmask();
            }
          });
        }
      }, {
        text: 'Close',
        handler: function () {
          win.close();
        }
      }]
    });

    win.show(this);
  },

  onDeleteCache: function (btn, ev) {
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();

    Ext.Ajax.request({
      url: 'AdapterManager/DeleteCache',
      method: 'POST',
      timeout: 120000,  // 2 minutes
      params: {
        'nodeid': node.attributes.id
      },
      success: function (response, request) {
        var responseObj = Ext.decode(response.responseText);

        if (responseObj.Level == 0) {
          showDialog(450, 100, 'Delete Cache Result', 'Cache deleted successfully.', Ext.Msg.OK, null);
        }
        else {
          showDialog(500, 160, 'Delete Cache Error', responseObj.Messages.join('\n'), Ext.Msg.OK, null);
        }
      },
      failure: function (response, request) {
        showDialog(500, 160, 'Delete Cache Error', responseObj.Messages.join('\n'), Ext.Msg.OK, null);
      }
    })
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

  onReloadNode: function (node) {
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
      this.fireEvent('OpenGraphMap', this, node);
    }
    else if (node.attributes.type == 'DataObjectNode') {
      this.fireEvent('LoadAppData', this, node);
    }
  },

  onClick: function (node) {
    try {
      var obj = node.attributes;
      this.propertyPanel.setSource(node.attributes.property);

      if (this.contextMenu)
        this.contextMenu.removeAll();

      if (obj.type == "ScopesNode") {
        this.contextMenu.add(this.buildScopesMenu());
      }
      else if (obj.type == "ScopeNode") {
        this.contextMenu.add(this.buildScopeMenu());
      }
      else if (obj.type == "ApplicationNode") {
        this.contextMenu.add(this.buildApplicationMenu());
      }
      else if (obj.type == "DataObjectsNode") {
        var menu = this.buildDataObjectsMenu(node);
        this.dataObjectsMenu = menu;
        this.contextMenu.add(menu);
      }
      else if (obj.type == "DataObjectNode") {
        this.contextMenu.add(this.buildAppDataMenu());
      }
      else if (obj.type == "ValueListsNode") {
        this.contextMenu.add(this.buildvalueListsMenu());
      }
      else if (obj.type == "ValueListNode") {
        this.contextMenu.add(this.buildvalueListMenu());
      }
      else if (obj.type == "ListMapNode") {
        this.contextMenu.add(this.buildvalueListMapMenu());
      }
      else if (obj.type == "GraphsNode") {
        this.contextMenu.add(this.buildGraphsMenu());
      }
      else if (obj.type == "GraphNode") {
        this.contextMenu.add(this.buildGraphMenu());
      }
    } catch (e) {
      //  alert(e);
    }
  }
});