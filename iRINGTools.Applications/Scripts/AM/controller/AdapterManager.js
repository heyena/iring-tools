Ext.define('AM.controller.AdapterManager', {
  extend: 'Ext.app.Controller',
  views: [
        'directory.DirectoryPanel',
        'directory.DirectoryTree',
        'directory.ScopePanel',
        'directory.ApplicationPanel',
        'directory.DataGridPanel',
        'directory.GraphPanel',
        'directory.ValuelistPanel',
        'directory.ValuelistMapPanel',
        'common.PropertyPanel',
        'common.ContentPanel',
        'common.CenterPanel'
  ],
  stores: [],
  models: [
        'DirectoryModel',
        'DataLayerModel',
        'BaseUrlModel',
        'DynamicModel'
  ],
  refs: [
        {
          ref: 'dirTree',
          selector: 'viewport > directorypanel > directorytree'
        },
        {
          ref: 'dirProperties',
          selector: 'viewport > directorypanel > propertypanel'
        },
        {
          ref: 'mainContent',
          selector: 'viewport > centerpanel > contentpanel'
        }
    ],
  init: function () {
    this.control({
      'menu button[action=newscope]': {
        click: this.newScope
      },
      'menu button[action=editscope]': {
        click: this.editScope
      },
      'menu button[action=deletescope]': {
        click: this.deleteScope
      },
      'menu button[action=newendpoint]': {
        click: this.newEndpoint
      },
      'menu button[action=editendpoint]': {
        click: this.editEndpoint
      },
      'menu button[action=deleteendpoint]': {
        click: this.deleteEndpoint
      },
      'menu button[action=showdata]': {
        click: this.showDataGrid
      },
      'menu button[action=refreshfacade]': {
        click: this.onRefreshFacade
      },
      'menu button[action=regenerateAll]': {
        click: this.onRegenerateAll
      },
      'directorypanel directorytree': {
        beforeload: this.onBeforeLoad
      },
      'button[action=reloaddirtree]': {
        click: this.onReloadTree
      }
    });
  },

  onReloadTree: function (node) {
    var tree = this.getDirTree();
    var state = tree.getState();
    tree.store.load();

    tree.applyState(state);
  },

  onRegenerateAll: function (btn, ev) {
    Ext.Ajax.request({
      url: 'directory/RegenAll',
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

  onRefreshFacade: function () {

    var tree = this.getDirTree(),
        node = tree.getSelectedNode();

    tree.getEl().mask('Loading', 'x-mask-loading');
    Ext.Ajax.request({
      url: 'facade/refreshFacade',
      method: 'POST',
      params: {
        scope: node.data.id,
        baseUrl: node.data.property.baseUrl
      },
      success: function (o) {
        tree.onReload(node);
        tree.getEl().unmask();
      },
      failure: function (f, a) {
        tree.getEl().unmask();
        Ext.Msg.alert('Warning', 'Error Refreshing Facade!!!');
      }
    });
    tree.graphMenu.hide();
  },

  showDataGrid: function () {
    var tree = this.getDirTree(),
        content = this.getMainContent(),
        node = tree.getSelectedNode(),
        contextName = node.data.property.context,
        endpointName = node.data.property.endpoint,
        graph = node.data.text;
    var conf = {
      title: contextName + '.' + endpointName + '.' + graph,
      id: contextName + endpointName + graph + Ext.id(),
      context: contextName,
      //start: 0,
      //limit: 25,
      endpoint: endpointName,
      graph: graph
    };
    var exist = content.items.map[conf.id];
    if (exist) {
      exist.show();
      tree.appDataMenu.hide();
      return true;
    }
    var newtab = Ext.widget('datagridpanel', conf);

    newtab.store.on('beforeload', function (store, action) {
      store.proxy.extraParams.context = contextName;
      store.proxy.extraParams.start = (store.currentPage - 1) * store.pageSize;
      store.proxy.extraParams.limit = store.pageSize;
      store.proxy.extraParams.endpoint = endpointName;
      store.proxy.extraParams.graph = graph;
    }, this);

    newtab.store.on('sync', function () {
      newtab.store.sync()
    }, this);

    newtab.store.load({
      callback: function (recs) {
        if (recs) {
          newtab.reconfigure(newtab.store, recs[0].store.proxy.reader.fields);
          newtab.show();
        }
        else {
          newtab.destroy();
          showDialog(500, 400, 'Error', 'Error Rendering DataGrid!', Ext.Msg.OK, null);
        }
      }
    });
    content.add(newtab).show();
    tree.appDataMenu.hide();
    return true;
  },

  newEndpoint: function () {
    var tree = this.getDirTree(),
        node = tree.getSelectedNode(),
        conf = {
          id: 'newwin-' + node.data.id,
          path: node.internalId,
          state: 'new',
          record: node.data.record,
          node: node,
          title: 'Add New Endpoint',
          iconCls: 'tabsApplication'
        };
    var win = Ext.widget('applicationform', conf);

    win.on('save', function () {
      win.close();
      tree.onReload();
      if (node.get('expanded') == false)
        node.expand();
    }, this);

    win.on('Cancel', function () {
      win.close();
    }, this);
    win.show();
  },

  editEndpoint: function () {
    var tree = this.getDirTree(),
        node = tree.getSelectedNode(),
        conf = {
          id: 'tab-' + node.data.id,
          path: node.internalId,
          record: node.data.record,
          node: node,
          state: 'edit',
          node: node.parentNode,
          title: 'Edit Endpoint \"' + node.data.text + '\"',
          iconCls: 'tabsApplication'
        };
    var win = Ext.widget('applicationform', conf);

    win.on('save', function () {
      win.close();
      tree.store.load();
      if (node.get('expanded') == false)
        node.expand();
    }, this);

    win.on('Cancel', function () {
      win.close();
    }, this);
    win.show();
  },

  newScope: function () {
    var tree = this.getDirTree(),
        node = tree.getSelectedNode(),
        conf = {
          id: 'tab-' + node.data.id,
          record: node.data.record,
          state: 'new',
          path: node.internalId,
          node: node,
          title: 'Add New Folder',
          iconCls: 'tabsScope',
          url: 'directory/folder'
        };
    var win = Ext.widget('scopeform', conf);

    win.on('save', function () {
      win.close();
      tree.onReload();
      if (node.get('expanded') == false)
        node.expand();
    }, this);

    win.on('Cancel', function () {
      win.close();
    }, this);
    win.show();

    if (win.items.first().getForm().findField('Name'))
      win.items.first().getForm().findField('Name').clearInvalid();

  },

  editScope: function () {
    var tree = this.getDirTree(),
        node = tree.getSelectedNode(),
        conf = {
          id: 'tab-' + node.id,
          record: node.data.record,
          state: 'edit',
          path: node.internalId,
          node: node,
          title: 'Edit Folder \"' + node.data.text + '\"',
          iconCls: 'tabsScope',
          url: 'directory/folder'
        };
    var win = Ext.widget('scopeform', conf);

    win.on('save', function () {
      win.close();
      tree.onReload();
      if (node.get('expanded') == false)
        node.expand();
    }, this);

    win.on('Cancel', function () {
      win.close();
    }, this);
    win.show();
  },

  deleteScope: function () {
    var tree = this.getDirTree(),
            node = tree.getSelectedNode();
    Ext.Ajax.request({
      url: 'directory/deleteEntry',
      method: 'POST',
      params: {
        'path': node.data.id,
        'type': 'folder',
        'baseUrl': '',
        'contextName': node.data.property.Context
      },
      success: function () {
        tree.onReload(node);
        if (node.parentNode)
          if (node.parentNode.expanded == false)
            node.parentNode.expand();
      },
      failure: function () {
        var message = 'Error deleting folder!';
        showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
      }
    });
    tree.scopeMenu.hide();
  },

  deleteEndpoint: function () {
    var tree = this.getDirTree(),
            node = tree.getSelectedNode();
    Ext.Ajax.request({
      url: 'directory/deleteEntry',
      method: 'POST',
      params: {
        'path': node.data.id,
        'type': 'endpoint',
        'baseUrl': node.data.record.BaseUrl,
        'contextName': node.data.property.Context
      },
      success: function () {
        tree.onReload();
        if (node.parentNode.expanded == false)
          node.parentNode.expand();
      },
      failure: function () {
        //Ext.Msg.alert('Warning', 'Error!!!');
        var message = 'Error deleting endpoint!';
        showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
      }
    });
    tree.applicationMenu.hide();
  },


  onBeforeLoad: function (store, operation, eOpts) {
    if (operation.node != undefined) {
      var operationNode = operation.node.data;
      //var param = store.proxy.extraParams;

      if (operationNode.type != undefined)
        store.proxy.extraParams.type = operationNode.type;

      if (operationNode.record != undefined && operationNode.record.Related != undefined)
        store.proxy.extraParams.related = operationNode.record.Related;

      if (operationNode.record != undefined) {
        operationNode.leaf = false;

        if (operationNode.record.context)
          store.proxy.extraParams.contextName = operationNode.record.context;

        if (operationNode.record.BaseUrl)
          store.proxy.extraParams.baseUrl = operationNode.record.BaseUrl;

        if (operationNode.record.endpoint)
          store.proxy.extraParams.endpoint = operationNode.record.endpoint;

        if (operationNode.record.securityRole)
          store.proxy.extraParams.security = operationNode.record.securityRole;

        if (operationNode.text != undefined)
          store.proxy.extraParams.text = operationNode.text;
      }
      else if (operationNode.property != undefined) {
        operationNode.leaf = false;

        if (operationNode.property.context)
          store.proxy.extraParams.contextName = operationNode.property.context;

        if (operationNode.property.endpoint)
          store.proxy.extraParams.endpoint = operationNode.property.endpoint;

        if (operationNode.property.baseUrl)
          store.proxy.extraParams.baseUrl = operationNode.property.baseUrl;

        if (operationNode.text != undefined)
          store.proxy.extraParams.text = operationNode.text;
      }
    }
  }
});