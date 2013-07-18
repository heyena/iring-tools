Ext.define('AM.controller.AdapterManager', {
  extend: 'Ext.app.Controller',
  views: [
        'directory.DirectoryPanel',
        'directory.DirectoryTree',
        'directory.ScopePanel',
        'directory.DataLayerPanel',
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
        'ContextModel',
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
      'directorypanel directorytree': {
        beforeload: this.onBeforeLoad
      },
      'button[action=reloaddirtree]': {
        click: this.onReloadTree
      },
      'menuitem[action=newscope]': {
        click: this.newScope
      },
      'menuitem[action=editscope]': {
        click: this.editScope
      },
      'menuitem[action=deletescope]': {
        click: this.deleteScope
      },
      'menuitem[action=newendpoint]': {
        click: this.newEndpoint
      },
      'menuitem[action=editendpoint]': {
        click: this.editEndpoint
      },
      'menuitem[action=deleteendpoint]': {
        click: this.deleteEndpoint
      },
      'menuitem[action=showdata]': {
        click: this.showDataGrid
      },
      'menuitem[action=refreshfacade]': {
        click: this.onRefreshFacade
      },
      'menuitem[action=newDataLayer]': {
        click: this.onNewDataLayer
      },
      //TODOs:
      //            'menuitem[action=editDataLayer]': {
      //                click: this.onEditDataLayer
      //            },
      //            'menuitem[action=deleteDataLayer]': {
      //                click: this.onDeleteDataLayer
      //            },
      'menuitem[action=regenerateAll]': {
        click: this.onRegenerateAll
      }
    });
  },

  onNewDataLayer: function () {
    var tree = this.getDirTree(),
        node = tree.getSelectedNode(),
        conf = {
          id: 'tab-' + node.data.id,
          record: node.data.record,
          state: 'new',
          path: node.internalId,
          node: node,
          title: 'Add Data Layer',
          iconCls: 'tabsScope',
          url: 'directory/datalayer'
        };

    var win = Ext.widget('datalayerform', conf);

    win.on('Cancel', function () {
      win.close();
    }, this);

    win.show();
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
        baseurl = node.data.property.baseUrl,
        graph = node.data.text;
    var conf = {
      title: contextName + '.' + endpointName + '.' + graph,
      id: contextName + endpointName + graph + Ext.id(),
      context: contextName,
      //start: 0,
      //limit: 25,
      endpoint: endpointName,
      baseUrl: baseurl,
      graph: graph
    };
    var exist = content.items.map[conf.id];
    if (exist) {
      tree.appDataMenu.hide();
    }

    Ext.getBody().mask("Loading...", "x-mask-loading");
    var newtab = Ext.widget('datagridpanel', conf);

    newtab.store.on('beforeload', function (store, action) {
      store.proxy.extraParams.context = contextName;
      store.proxy.extraParams.start = (store.currentPage - 1) * store.pageSize;
      store.proxy.extraParams.limit = store.pageSize;
      store.proxy.extraParams.endpoint = endpointName;
      store.proxy.extraParams.baseUrl = baseurl;
      store.proxy.extraParams.graph = graph;
    }, this);

    newtab.store.on('sync', function () {
      newtab.store.sync()
    }, this);

    newtab.store.on('load', function (store, action) {
      //the following line breaks unselecting numeric filter 
      //newtab.reconfigure(newtab.store, newtab.store.proxy.reader.fields);

      newtab.doLayout();
    }, this);

    newtab.store.load({
      callback: function (recs, response) {
        var rtext = response.response.responseText;
        var error = 'SUCCESS = FALSE';
        var index = rtext.toUpperCase().indexOf(error);

        if (index == -1) {
          newtab.reconfigure(newtab.store, recs[0].store.proxy.reader.fields);
          Ext.getBody().unmask();
        }
        else {
          Ext.getBody().unmask();
          newtab.destroy();
          var rtext = response.response.responseText;
          var error = 'SUCCESS = FALSE';
          var index = rtext.toUpperCase().indexOf(error);
          var msg = rtext.substring(index + error.length + 2, rtext.length - 1);
          showDialog(500, 300, 'Error', msg, Ext.Msg.OK, null);
        }
      }
    });
    content.add(newtab).show();
    tree.appDataMenu.hide();
    return true;
  },

  newEndpoint: function () {
    var tree = this.getDirTree();
    var node = tree.getSelectedNode();

    if (node.data.property.Context == '' || node.data.property.Context == undefined) {
      showDialog(400, 100, 'Warning', 'Parent Folder must have a valid context.', Ext.Msg.OK, null);
      return;
    }

    var conf = {
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
          loadMask: false,
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
          loadMask: false,
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