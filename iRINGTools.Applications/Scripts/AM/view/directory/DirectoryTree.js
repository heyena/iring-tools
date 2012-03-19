Ext.define('AM.view.directory.DirectoryTree', {
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
  //  pathSeparator: '>',
  lines: true,
  tbar: null,
  scroll: 'both',
  // store: 'DirectoryStore',
  initComponent: function () {

    this.tbar = [{
      text: 'Reload Tree',
      handler: this.onReload,
      icon: 'Content/img/16x16/view-refresh.png',
      scope: this
    }];

    Ext.apply(this, {
      store: Ext.create('Ext.data.TreeStore', {
        model: 'AM.model.DirectoryModel',
        root: {
          expanded: true,
          type: 'ScopesNode',
          iconCls: 'scopes',
          text: 'Scopes',
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
      stateful: true,
      stateId: this.id + '-state'//,
      // stateEvents: ['itemcollapse', 'itemexpand']
    });

    this.callParent(arguments);

    this.scopesMenu = new Ext.menu.Menu();
    this.scopesMenu.add(this.buildScopesMenu());

    this.generateMenu = new Ext.menu.Menu();
    this.generateMenu.add(this.buildGenerateMenu());

    this.scopesGenerateMenu = new Ext.menu.Menu();
    this.scopesGenerateMenu.add(this.buildScopesGenerateMenu());

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
    var root = this.getRootNode();
    var rootId = root.get('id');
    root.eachChild(function (child) {
      // function to store state of tree recursively 
      var storeTreeState = function (node, expandedNodes) {
        if (node.isExpanded() && node.childNodes.length > 0) {
          expandedNodes.push(node.get('id')); //'text'));
          //expandedNodes.push('/'+rootId+'/'+node.get('id')); //'text'));
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
    var that = this;
    var rootNode = that.getRootNode();

    var findChildById = function (root, id) {
      var i = 0;
      while (root.childNodes[i]) {
        var node = root.childNodes[i]
        if (node.data.id == id)
          return node;
        i++;
      }
      return null;
    };

    var treeNode = rootNode;
    var nodes = state.expandedNodes || [],
            len = nodes.length;
    //  this.collapseAll();
    for (var i = 0; i < len; i++) {
      if (typeof nodes[i] != 'undefined') {
        if (treeNode) {
          var treeNode = findChildById(treeNode, nodes[i]);
          if (treeNode && treeNode.data.expanded == false) {
            treeNode.expand();
          }
        }
        //that.expandPath(nodes[i]); //, 'text');
      }
    }
    // this.callParent(arguments);


  },

  buildScopesMenu: function () {
    return [
      {
        xtype: 'button',
        text: 'New Folder',
        icon: 'Content/img/16x16/document-new.png',
        scope: this,
        action: 'newscope'
      }]
  },

  buildScopesGenerateMenu: function () {
    return [
    {
      xtype: 'button',
      text: 'New Folder',
      icon: 'Content/img/16x16/document-new.png',
      scope: this,
      action: 'newscope'
    }, {
      xtype: 'button',
      text: 'Generate NH Files',
      icon: 'Content/img/16x16/document-new.png',
      scope: this,
      action: 'regenerateAll'
      // HibernateDataLayer artifacts
    }]
  },

  buildGenerateMenu: function () {
    return [
    {
      xtype: 'button',
      text: 'Generate NH Files',
      icon: 'Content/img/16x16/document-new.png',
      scope: this,
      action: 'regenerateAll'
      // HibernateDataLayer artifacts
    }]
  },

  buildScopeMenu: function () {
    return [
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
        xtype: 'button',
        text: 'New Folder',
        icon: 'Content/img/16x16/document-new.png',
        scope: this,
        action: 'newscope'
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
      }//,
    //      {
    //          xtype: 'button',
    //          text: 'Open NHConfiguration',
    //          icon: 'Content/img/16x16/preferences-system.png',
    //          scope: this,
    //          action: 'configurenh'
    //      }
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
    var node = model.store.getAt(index);
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

    //right clicks the root node
    if (index == 0) {
      Ext.Ajax.request({
        url: 'directory/UseLdap',
        method: 'GET',
        success: function (response, request) {
          baseUrl = response.responseText;
        },
        failure: function () { }
      });

      //when root node has children
      if (node.childNodes[0]) {
        if (baseUrl.toLowerCase() == 'true') {
          if (node.childNodes[0].data.record.securityRole) {
            securityRole = node.childNodes[0].data.record.securityRole;

            if (node.childNodes[0].data.record.securityRole.indexOf('rootadmin') > -1) {
              ifsuperadmin = true;
              me.scopesMenu.showAt(e.getXY());
            }
            else if (securityRole.indexOf('admin') > -1) {
              me.generateMenu.showAt(e.getXY());
            }
          }
        }
        else {
          ifsuperadmin = true;
          me.scopesGenerateMenu.showAt(e.getXY());
        }
      }
      //when starting from scratch (root node has no children)
      else {
        if (baseUrl == 'false') {
          me.scopesGenerateMenu.showAt(e.getXY());
        }
        else {
          Ext.Ajax.request({
            url: 'directory/RootSecurityRole',
            method: 'GET',
            success: function (response, request) {
              var rootSecurityRole = response.responseText;
              if (rootSecurityRole.indexOf('rootadmin') > -1) {
                ifsuperadmin = true;
                me.scopesMenu.showAt(e.getXY());
              }
              else if (securityRole.indexOf('admin') > -1) {
                me.generateMenu.showAt(e.getXY());
              }
            },
            failure: function () { }
          });
        }
      }
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

  onReload: function (node) {
    //get state from tree
    // var me = this;
    var state = this.getState();
    //this.body.mask('Loading', 'x-mask-loading');

    this.getStore().load();
    //this.body.unmask();
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
