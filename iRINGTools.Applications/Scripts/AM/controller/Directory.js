/*
 * File: Scripts/AM/controller/Directory.js
 *
 * This file was generated by Sencha Architect version 2.1.0.
 * http://www.sencha.com/products/architect/
 *
 * This file requires use of the Ext JS 4.1.x library, under independent license.
 * License of Sencha Architect does not include license for Ext JS 4.1.x. For more
 * details see http://www.sencha.com/license or contact license@sencha.com.
 *
 * This file will be auto-generated each and everytime you save your project.
 *
 * Do NOT hand edit this file.
 */

Ext.define('AM.controller.Directory', {
  extend: 'Ext.app.Controller',

  models: [
    'DirectoryModel',
    'BaseUrlModel',
    'DataLayerModel',
    'ContextModel',
    'DynamicModel'
  ],
  stores: [
    'DirectoryTreeStore',
    'ContextStore',
    'BaseUrlStore',
    'DataLayerStore'
  ],
  views: [
    'common.PropertyPanel',
    'common.CenterPanel',
    'common.ContentPanel',
    'directory.DirectoryPanel',
    'directory.DirectoryTree',
    'directory.ApplicationWindow',
    'directory.ScopeWindow',
    'directory.DataLayerForm',
    'directory.DataGridPanel',
    'directory.GraphMapForm',
    'directory.GraphMapWindow',
    'directory.ScopeForm',
    'directory.ContextCombo',
    'directory.DataLayerWindow',
    'directory.AvailBaseUrlCombo',
    'directory.DataLayerCombo',
    'directory.ApplicationForm'
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
    },
    {
      ref: 'contextCombo',
      selector: 'contextcombo'
    },
    {
      ref: 'datalayerCombo',
      selector: 'datalayercombo'
    },
    {
      ref: 'baseUrlCombo',
      selector: 'availbaseurlcombo'
    }
  ],

  onBeforeLoad: function(store, operation, options) {
    var me = this;
    if (operation.node !== undefined) {
      var operationNode = operation.node.data;
      var params = store.proxy.extraParams;

      if (operationNode.type !== undefined)
      params.type = operationNode.type;

      if (operationNode.record !== undefined && operationNode.record.Related !== undefined)
      params.related = operationNode.record.Related;

      if (operationNode.record !== undefined) {
        operationNode.leaf = false;

        if (operationNode.record.context)
        params.contextName = operationNode.record.context;

        if (operationNode.record.BaseUrl)
        params.baseUrl = operationNode.record.BaseUrl;

        if (operationNode.record.endpoint)
        params.endpoint = operationNode.record.endpoint;

        if (operationNode.record.securityRole)
        params.security = operationNode.record.securityRole;

        if (operationNode.text !== undefined)
        params.text = operationNode.text;
      }
      else if (operationNode.property !== undefined) {
        operationNode.leaf = false;

        if (operationNode.property.context)
        params.contextName = operationNode.property.context;

        if (operationNode.property.endpoint)
        params.endpoint = operationNode.property.endpoint;

        if (operationNode.property.baseUrl)
        params.baseUrl = operationNode.property.baseUrl;

        if (operationNode.text !== undefined)
        params.text = operationNode.text;
      }
    }
  },

  newOrEditScope: function(item, e, options) {
    var me = this;
    var path, state, context, description, wintitle;
    var tree = me.getDirTree();
    var node = tree.getSelectedNode();

    context = node.data.record.context;

    if(node.parentNode) {
      path = node.internalId;
    } else {
      path = '';
    }

    if(item.itemId == 'editfolder' && node.data.record !== undefined) {
      name = node.data.record.Name;
      description = node.data.record.Description;
      wintitle = 'Edit folder \"' + node.data.text + '\"';
      state = 'edit';
    } else {
      name = '';
      state = 'new';
      wintitle = 'Add New Folder';
    }

    var conf = {
      id: 'tab-' + node.data.id,
      title: wintitle,
      iconCls: 'tabsScope'
    };

    var win = Ext.widget('scopewindow', conf);

    win.on('save', function () {
      win.destroy(); 
      node.collapse();
      tree.expandPath(node.getPath(), 'text');
    }, me);

    win.on('cancel', function () {
      win.destroy();
    }, me);

    var form = win.down('form');
    form.node = node;


    var combo = me.getContextCombo();

    combo.store.on('load', function(store, action) {
      if(context === '') {
        if(store)
        if(store.data.items[0])
        context = store.data.items[0].data.context;
      }
    }, me);

    combo.on('afterrender', function (combo, eopts) {
      if(context !== '' && context !== undefined && combo.store.data.length == 1)
      combo.setValue(context);
    }, me);

    form.getForm().findField('path').setValue(path);
    form.getForm().findField('state').setValue(state);
    form.getForm().findField('oldContext').setValue(context);
    form.getForm().findField('description').setValue(description);
    form.getForm().findField('folderName').setValue(name);

    win.show();
  },

  deleteScope: function(item, e, options) {
    var me = this;
    var tree = this.getDirTree();
    var parent, path;
    var node = tree.getSelectedNode();

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
        var parentNode = node.parentNode;
        parentNode.removeChild(node);                   
        tree.getSelectionModel().select(parentNode);
        tree.onReload();
      },
      failure: function () {
        var message = 'Error deleting folder!';
        showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
      }
    });
  },

  newOrEditEndpoint: function(item, e, options) {
    var me = this;
    var name, description, datalayer, assembly, baseurl, showconfig, wintitle, state, path, context;
    var tree = me.getDirTree();
    var node = tree.getSelectedNode();

    if (node.data.property.Context === '' || node.data.property.Context === undefined) {
      showDialog(400, 100, 'Warning', 'Parent Folder must have a valid context.', Ext.Msg.OK, null);
      return;
    }

    if(item.itemId == 'editendpoint') {
      name = node.data.record.Name;
      description = node.data.record.Description;
      datalayer = node.data.record.DataLayer;
      assembly = node.data.record.Assembly;
      baseurl = node.data.record.BaseUrl;
      wintitle =  'Edit Endpoint \"' + node.data.text + '\"';
      endpoint = node.data.record.endpoint; 
      state = 'edit';
    } else {
      wintitle = 'Add New Endpoint';
      name = '';
      description = '';
      datalayer = '';
      assembly = '';
      baseurl = '';
      state = 'new';
      path = node.internalId;
    }

    context = node.data.record.context;

    var conf = { 
      id: 'newwin-' + node.data.id, 
      title: wintitle, 
      iconCls: 'tabsApplication',
      node: node
    };

    var win = Ext.widget('applicationwindow', conf);

    var form = win.down('form');

    win.on('save', function () { 
      win.close();

      tree.expandPath(node.getPath(), 'text');
    }, me);

    win.on('Cancel', function () {
      win.close();
    }, me);

    var buCmb = me.getBaseUrlCombo();

    buCmb.store.on('load', function (store, action) {
      if(baseurl === '') 
      if(store) 
      baseurl = store.data.items[0].data.baseurl;
    }, me);

    buCmb.on('afterrender', function (combo, eopts) {
      if (baseurl !== '' && baseurl !== undefined && combo.store.data.length == 1) {
        combo.setValue(baseurl);
      }
    }, me);

    buCmb.on('select', function(combo, records, eopts) {
      if(records !== undefined && node.data.record !== undefined)
      baseurl = records[0].data.baseurl;
    }, me);

    var dlCmb = me.getDatalayerCombo();

    dlCmb.on('select', function(combo, records, eopts) {
      if (records !== null && node.data.record !== null) {
        form.getForm().findField('assembly').setValue(records[0].data.assembly);
      }
    }, me);

    dlCmb.store.on('beforeload', function (store, action) {
      var useNodeUrl = false;
      if(buCmb !== undefined) 
      if(buCmb.value !== undefined) 
      store.proxy.extraParams.baseUrl = buCmb.value;
      else 
      useNodeUrl = true; 
      if(baseurl === '' || baseurl === "") 
      return;
      if(useNodeUrl)
      store.proxy.extraParams.baseUrl = baseurl;

    }, me);

    dlCmb.on('afterrender', function (combo, eopts) {
      if (assembly !== '') {
        combo.setValue(assembly);
      }
    }, me);

    form.getForm().findField('path').setValue(path);
    form.getForm().findField('state').setValue(state);
    form.getForm().findField('contextValue').setValue(context);
    form.getForm().findField('oldBaseUrl').setValue(baseurl);
    form.getForm().findField('oldAssembly').setValue(assembly);
    form.getForm().findField('baseUrl').setValue(baseurl);
    form.getForm().findField('endpoint').setValue(name);
    form.getForm().findField('description').setValue(description);
    form.getForm().findField('context').setValue(context);
    form.getForm().findField('assembly').setValue(assembly);

    win.show();
  },

  deleteEndpoint: function(item, e, options) {
    var me = this;

    var tree = me.getDirTree();
    var node = tree.getSelectedNode();
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
        var parentNode = node.parentNode;
        parentNode.removeChild(node);                   
        tree.getSelectionModel().select(parentNode);
        tree.onReload();
      },
      failure: function () {
        //Ext.Msg.alert('Warning', 'Error!!!');
        var message = 'Error deleting endpoint!';
        showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
      }
    });
  },

  onNewDataLayer: function(item, e, options) {
    var me = this;
    var tree = me.getDirTree();
    var node = tree.getSelectedNode();
    conf = {
      id: 'tab-' + node.data.id,
      title: 'Add Data Layer'
    };
    var win = Ext.widget('datalayerwindow', conf);

    var form = win.down('form');
    form.getForm().findField('state').setValue('new');

    win.on('Save', function () {
      tree.store.load();
      if (node.get('expanded') === false)
      node.expand();
    }, me);

    win.on('Cancel', function () {
      win.close();
    }, me);

    win.show();
  },

  onRegenerateAll: function(item, e, options) {
    var me = this;
    Ext.Ajax.request({
      url: 'directory/RegenAll',
      method: 'GET',
      success: function (result, request) {
        var responseObj = Ext.decode(result.responseText);
        var msg = '';
        for (var i = 0; i < responseObj.StatusList.length; i++) {
          var status = responseObj.StatusList[i];
          if (msg !== '') {
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
    });
  },

  onShowDataGrid: function(item, e, options) {
    var me = this;    
    var tree = this.getDirTree();
    var node = tree.getSelectedNode(),
    content = me.getMainContent(),
    contextName = node.data.property.context,
    endpointName = node.data.property.endpoint,
    baseurl = node.data.property.baseUrl;

    var graph = node.data.text;
    var title = 'Data Grid ' + contextName + '.' + endpointName + '.' + graph;

    var gridPanel = content.down('dynamicgrid[title='+title+']');

    if (!gridPanel) {

      content.getEl().mask("Loading...", "x-mask-loading");
      gridPanel = Ext.widget('dynamicgrid', {'title': title});

      var gridStore = gridPanel.getStore();
      var gridProxy = gridStore.getProxy();

      gridStore.on('beforeload', function (store, action) {
        var params = store.proxy.extraParams;
        params.context = contextName;
        params.start = (store.currentPage - 1) * store.pageSize;
        params.limit = store.pageSize;
        params.endpoint = endpointName;
        params.baseUrl = baseurl;
        params.graph = graph;
      }, me);

      gridProxy.on('exception', function(proxy, response, operation) {
        content.getEl().unmask();
        gridPanel.destroy();
        var rtext = response.responseText;
        var error = 'SUCCESS = FALSE';
        var index = rtext.toUpperCase().indexOf(error);
        var msg = rtext.substring(index + error.length + 2, rtext.length - 1);
        showDialog(500, 300, 'Error', msg, Ext.Msg.OK, null);
      }, me);

      gridStore.load({
        callback: function (records, response) {
          if(records[0].store.proxy.reader.metaData) {
            gridPanel.reconfigure(gridStore, records[0].store.proxy.reader.metaData.fields);
            content.getEl().unmask();
          } else {
            if(response)
            showDialog(200, 50, 'Warning', 'Authentication failure', Ext.Msg.OK, null);
            return true;
          }
        }
      });
      content.getEl().unmask();
      content.add(gridPanel);
    }
    content.setActiveTab(gridPanel);
  },

  onRefreshFacade: function(item, e, options) {
    var me = this;
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
        tree.onReload();
        tree.getEl().unmask();
      },
      failure: function (f, a) {
        tree.getEl().unmask();
        showDialog(400,300, 'Warning', 'Error Refreshing Facade!!!', Ext.Msg.OK, null);
      }
    });
  },

  onConfigureEndpoint: function(item, e, options) {
    var me = this;
    var tree = me.getDirTree();
    var node = tree.getSelectedNode();
    var datalayer = node.data.record.DataLayer;

    switch(datalayer) {
      case 'NHibernateLibrary':
      me.application.fireEvent('confignhibernate', me);
      break;
      case 'SpreadsheetLibrary':
      me.application.fireEvent('configspreadsheet', me);
      break;
      default:
      showDialog(300, 300, 'Warning', 'Datalayer ' + datalayer + ' is not configurable...', Ext.msg.OK, null);
      break;
    }

  },

  init: function(application) {
    this.control({
      "directorypanel directorytree": {
        beforeload: this.onBeforeLoad
      },
      "menuitem[action=neweditscope]": {
        click: this.newOrEditScope
      },
      "menuitem[action=deletescope]": {
        click: this.deleteScope
      },
      "menuitem[action=neweditendpoint]": {
        click: this.newOrEditEndpoint
      },
      "menuitem[action=deleteendpoint]": {
        click: this.deleteEndpoint
      },
      "menuitem[action=newdatalayer]": {
        click: this.onNewDataLayer
      },
      "menuitem[action=regenerateall]": {
        click: this.onRegenerateAll
      },
      "menuitem[action=showdata]": {
        click: this.onShowDataGrid
      },
      "menuitem[action=refreshfacade]": {
        click: this.onRefreshFacade
      },
      "menuitem[action=configureendpoint]": {
        click: this.onConfigureEndpoint
      }
    });
  }

});
