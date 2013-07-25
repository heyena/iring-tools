/*
 * File: Scripts/AM/controller/Directory.js
 *
 * This file was generated by Sencha Architect version 2.2.2.
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
    'DynamicModel',
    'FileDownloadModel'
  ],
  stores: [
    'DirectoryTreeStore',
    'ContextStore',
    'BaseUrlStore',
    'DataLayerStore',
    'FileDownloadStore'
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
    'directory.ApplicationForm',
    'menus.ScopesMenu',
    'menus.AppDataMenu',
    'menus.ApplicationMenu',
    'menus.ValueListsMenu',
    'menus.ValueListMenu',
    'menus.GraphsMenu',
    'menus.GraphMenu',
    'menus.TemplatemapMenu',
    'menus.RolemapMenu',
    'menus.ClassmapMenu',
    'menus.ValueListMapMenu',
    'menus.AppDataRefreshMenu',
    'directory.FileUpoadForm',
    'directory.FileUploadWindow',
    'directory.DownloadGrid',
    'directory.FileDownloadWindow',
    'directory.ImportCacheForm',
    'directory.ImportCacheWindow'
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

  handleMetachange: function() {
    var me = this,
      store = grid.getStore(),
      columns = meta.columns;

    grid.metachange = true;
    grid.reconfigure(store, columns);
  },

  onBeforeLoad: function(store, operation, eOpts) {
    var me = this;
    if (operation.node !== null) {
      var operationNode = operation.node.data;
      var params = store.proxy.extraParams;

      if (operationNode.type !== null)
      params.type = operationNode.type;

      if (operationNode.record !== null && operationNode.record.Related !== null)
      params.related = operationNode.record.Related;

      if (operationNode.record !== null) {
        operationNode.leaf = false;

        if (operationNode.record.context)
        params.contextName = operationNode.record.context;

        if (operationNode.record.BaseUrl)
        params.baseUrl = operationNode.record.BaseUrl;

        if (operationNode.record.endpoint)
        params.endpoint = operationNode.record.endpoint;

        if (operationNode.record.securityRole)
        params.security = operationNode.record.securityRole;

        if (operationNode.text !== null)
        params.text = operationNode.text;
      }
      else if (operationNode.property !== null) {
        operationNode.leaf = false;

        if (operationNode.property.context)
        params.contextName = operationNode.property.context;

        if (operationNode.property.endpoint)
        params.endpoint = operationNode.property.endpoint;

        if (operationNode.property.baseUrl)
        params.baseUrl = operationNode.property.baseUrl;

        if (operationNode.text !== null)
        params.text = operationNode.text;
      }
    }
  },

  newOrEditScope: function(item, e, eOpts) {

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
      wintitle = 'Edit Scope \"' + node.data.text + '\"';
      state = 'edit';

    } else {
      name = '';
      state = 'new';
      wintitle = 'Add Scope';

    }

    var conf = {
      id: 'tab-' + node.data.id,
      title: wintitle,
      iconCls: 'tabsScope'
    };

    var win = Ext.widget('scopewindow', conf);

    win.on('save', function () {
      win.destroy();
      me.onAppDataRefreshClick(item, e, eOpts);
      //if (!node.isExpanded())
      // node.expand();
      //tree.expandPath(node.getPath(), 'text');

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
    form.getForm().findField('scope').setValue(name);
    form.getForm().findField('contextName').setValue(name);

    win.show();
  },

  deleteScope: function(item, e, eOpts) {
    var me = this;
    var tree = this.getDirTree();
    var parent, path;
    var node = tree.getSelectedNode();

    Ext.Ajax.request({
      url: 'directory/DeleteScope',//'directory/deleteEntry',
      method: 'POST',
      params: {
        'nodeid': node.data.id
        //'path': node.data.id,
        //'type': 'folder',
        //'baseUrl': '',
        //'contextName': node.data.property.Context
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

  newOrEditEndpoint: function(item, e, eOpts) {
    var me = this;
    var name, description, datalayer, assembly,application, baseurl, showconfig,endpoint,wintitle, state, path, context;
    var tree = me.getDirTree();
    var node = tree.getSelectedNode();

    context = node.parentNode.data.text;//node.data.record.ContextName;
    if(item.itemId == 'editendpoint') {
      name = node.data.record.Name;
      description = node.data.record.Description;
      datalayer = node.data.record.DataLayer;
      assembly = node.data.record.Assembly;
      application = name;
      wintitle =  'Edit Application \"' + node.data.text + '\"';
      endpoint = node.data.record.Name;//node.data.record.Endpoint; 
      state = 'edit';
    } else {
      wintitle = 'Add Application';
      //state = 'new';
      state = '';
      application = '';
      context = node.data.record.Name;
      path = node.internalId;
    }



    var conf = { 
      id: 'newwin-' + node.data.id, 
      title: wintitle, 
      iconCls: 'tabsApplication',
      node: node,
      modal:true
    };

    var win = Ext.widget('applicationwindow', conf);

    var form = win.down('form');

    win.on('save', function () { 
      win.close();
      me.onAppDataRefreshClick(item, e, eOpts);
      //if (node.isExpanded())
      //node.collapsed();
      tree.expandPath(node.getPath(), 'text');
    }, me);

    win.on('Cancel', function () {
      win.close();
    }, me);

    var dlCmb = me.getDatalayerCombo();

    dlCmb.on('select', function(combo, records, eopts) {
      if (records !== null && node.data.record !== null) {
        form.getForm().findField('assembly').setValue(records[0].data.assembly);
      }
    }, me);

    dlCmb.on('afterrender', function (combo, eopts) {
      if (assembly !== '') {
        combo.setValue(assembly);
      }
    }, me);

    form.getForm().findField('path').setValue(path);
    form.getForm().findField('state').setValue(state);
    form.getForm().findField('scope').setValue(context);
    form.getForm().findField('oldAssembly').setValue(assembly);
    form.getForm().findField('name').setValue(endpoint);
    //form.getForm().findField('folderName').setValue(endpoint);
    form.getForm().findField('description').setValue(description);
    form.getForm().findField('context').setValue(context);
    form.getForm().findField('assembly').setValue(assembly);
    form.getForm().findField('application').setValue(application);

    win.show();
  },

  deleteEndpoint: function(item, e, eOpts) {
    var me = this;

    var tree = me.getDirTree();
    var node = tree.getSelectedNode();
    Ext.Ajax.request({
      url: 'directory/deleteapplication',
      method: 'POST',
      params: {
        nodeid: node.data.id
        //'path': node.data.id,
        //'type': 'endpoint',
        //'baseUrl': node.data.record.BaseUrl,
        //'contextName': node.data.property.Context
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

  onNewDataLayer: function(item, e, eOpts) {
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

  onRegenerateAll: function(item, e, eOpts) {
    var me = this;
    Ext.Ajax.request({
      url:'AdapterManager/RegenAll', //'directory/RegenAll',
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

  onShowDataGrid: function(item, e, eOpts) {
    var me = this;
    var tree = this.getDirTree();
    var node = tree.getSelectedNode();
    content = me.getMainContent();
    //contextName = node.data.property.context,
    contextName = node.parentNode.parentNode.parentNode.data.property.Name;
    //endpointName = node.data.property.endpoint,
    endpointName = node.parentNode.parentNode.data.property.Name;
    //baseurl = node.data.property.baseUrl;

    var graph = node.data.text;
    //var title = 'Data Grid ' + contextName + '.' + endpointName + '.' + graph;
    var title = /*'Data Grid ' + */ node.parentNode.parentNode.parentNode.data.property.Name + '.' + node.parentNode.parentNode.data.property.Name + '.' + graph;
    var gridPanel = content.down('dynamicgrid[title=' + title + ']');

    if (!gridPanel) {

      content.getEl().mask("Loading...", "x-mask-loading");
      gridPanel = Ext.widget('dynamicgrid', { 'title': title });

      gridStore = gridPanel.getStore();
      var gridProxy = gridStore.getProxy();

      gridStore.on('beforeload', function (store, action) {
        var params = store.proxy.extraParams;
        //params.context = contextName;
        params.start = (store.currentPage - 1) * store.pageSize;
        params.limit = store.pageSize;
        params.app = node.parentNode.parentNode.data.property.Name;
        params.scope = node.parentNode.parentNode.parentNode.data.property.Name ;
        //params.endpoint = endpointName;
        //params.baseUrl = baseurl;
        params.graph = graph;
      }, me);

      gridProxy.on('exception', function (proxy, response, operation) {
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
          if(records!=undefined){
            if (records[0]) {
              gridPanel.reconfigure(gridStore, records[0].store.proxy.reader.metaData.columns);
              content.getEl().unmask();
            } else {
              if (response)
              showDialog(200, 50, 'Warning', 'Authentication failure', Ext.Msg.OK, null);
              return true;
            }
          }

        }
      });
      content.getEl().unmask();
      content.add(gridPanel);
    }
    content.setActiveTab(gridPanel);
  },

  onRefreshFacade: function(item, e, eOpts) {
    var me = this;
    var tree = this.getDirTree(),
      node = tree.getSelectedNode();

    tree.getEl().mask('Loading', 'x-mask-loading');
    Ext.Ajax.request({
      url: 'facade/refreshFacade',
      method: 'POST',
      params: {
        //contextName: node.data.id,
        scope:node.data.id,
        //baseUrl: node.data.property.baseUrl
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

  onConfigureEndpoint: function(item, e, eOpts) {

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

  showContextMenu: function(dataview, record, item, index, e, eOpts) {
    var me = this,
      tree = me.getDirTree();
    e.stopEvent();
    node = record.store.getAt(index);

    tree.getSelectionModel().select(node);
    //tree.onClick(dataview, record, 0, index, e);

    var obj = node.data;

    if (obj.type === "ScopesNode") {
      var scopesMenu = Ext.widget('scopesmenu');
      scopesMenu.showAt(e.getXY());
    } else if (obj.type === "ScopeNode") {
      var scopeMenu = Ext.widget('scopemenu');
      scopeMenu.showAt(e.getXY());
    } else if (obj.type === "ApplicationNode") {
      var applicationMenu = Ext.widget('applicationmenu');
      applicationMenu.showAt(e.getXY());
    } else if (obj.type === "DataObjectNode") {
      var appDataMenu = Ext.widget('appdatamenu');  
      appDataMenu.showAt(e.getXY());
    } else if (obj.type === "ValueListsNode") {
      var valueListsMenu = Ext.widget('valuelistsmenu');
      valueListsMenu.showAt(e.getXY());
    } else if (obj.type === "ValueListNode") {
      var valueListMenu = Ext.widget('valuelistmenu');
      valueListMenu.showAt(e.getXY());
    } else if (obj.type === "ListMapNode") {
      var valueListMapMenu = Ext.widget('valuelistmapmenu');
      valueListMapMenu.showAt(e.getXY());
    } else if (obj.type === "GraphsNode") {
      scopForExport = node.parentNode.parentNode.data.text;
      appForExport = node.parentNode.data.text;  
      var graphsMenu = Ext.widget('graphsmenu');
      graphsMenu.items.items[1].href = '/mapping/export/'+ scopForExport+'/'+appForExport;
      graphsMenu.showAt(e.getXY());
    } else if (obj.type === "GraphNode") {
      var graphMenu = Ext.widget('graphmenu');
      graphMenu.showAt(e.getXY());
    }else if (obj.type === "DataObjectsNode") {
      var graphMenu = Ext.widget('appdatarefreshmenu');
      graphMenu.showAt(e.getXY());
    }

  },

  onAppDataRefreshClick: function(item, e, eOpts) {

    var me = this;
    var tree = me.getDirTree();
    var node = tree.getSelectedNode();
    var store = tree.store;//me.store;

    if (!node)
    node = me.getRootNode();
    var state = tree.getState();
    var nodeState = '/Scopes/'+node.internalId;
    //var path = node.getPath('text');
    //store.load(node);
    tree.body.mask('Loading...', 'x-mask-loading');
    //store.load(node);
    //store.load({
    //});
    store.load({
      node:node,
      callback: function (records, options, success) {
        //var nodes = state.expandedNodes || [],
        //len = nodes.length;
        //tree.collapseAll();
        //Ext.each(nodes, function (path) {
        //tree.expandPath('/Scopes/test/ABC/Data Objects', 'text');
        //});
        tree.body.unmask();
      }

    });

  },

  onShowDataGridd: function(dataview, record, item, index, e, eOpts) {
    var me = this;
    if(record.data.type == 'GraphNode')
    me.application.fireEvent('opengraphmap', me);    
    //me.onShowGrap(item, e, eOpts);
    else
    me.onShowDataGrid(item, e, eOpts);
  },

  onTextfieldBlur: function(component, e, eOpts) {

    if(component.dataIndex!=undefined){
      var me = this;
      var gridPanel  = me.getMainContent().activeTab;
      var gridStore = gridPanel.getStore();
      var gridProxy = gridStore.getProxy();
      gridStore.currentPage = 1;
      gridProxy.on('exception', function (proxy, response, operation) {
        //centerPanel.getEl().unmask();
        gridPanel.destroy();
        var rtext = response.responseText;
        if(rtext!=undefined){
          var error = 'SUCCESS = FALSE';
          var index = rtext.toUpperCase().indexOf(error);
          msg = rtext;
          showDialog(500, 300, 'Error', msg, Ext.Msg.OK, null);

        }

      }, me);
      gridStore.load({
        callback: function (records, response) 
        {                                     
          if(records!=undefined && records[0]!=undefined && records[0].store.proxy.reader.metaData) {
            gridPanel.reconfigure(gridStore,  records[0].store.proxy.reader.metaData.columns);
          }

        }
      });

    }
  },

  onFileUpload: function(item, e, eOpts) {

    var me = this;
    var win = Ext.widget('fileuploadwindow');
    var form = win.down('form');
    var tree = me.getDirTree();
    var node = tree.getSelectedNode();

    var formRecord = {
      scope: node.parentNode.data.text,
      application: node.data.text 
    };


    form.getForm().setValues(formRecord);

    win.on('Save', function () {
      win.destroy();
    }, me);

    win.on('reset', function () {
      win.destroy();
    }, me);

    win.show();
  },

  onFileDownload: function(item, e, eOpts) {
    var me = this;
    var win = Ext.widget('filedownloadwindow');
    var form = win.down('form');
    var tree = me.getDirTree();
    var node = tree.getSelectedNode();

    var formRecord = {
      scope: node.parentNode.data.text,
      application: node.data.text 
    };


    /*form.getForm().setValues(formRecord);

    win.on('Save', function () {
    win.destroy();
    }, me);

    win.on('reset', function () {
    win.destroy();
    }, me);
    */
    win.show();
  },

  onAddSettings: function(button, e, eOpts) {
    var me = this;
    var nameID;
    var valueID;
    var myFieldSet = Ext.getCmp('settingfieldset');
    if(myFieldSet.items.items.length>=1){
      var nameID = 'key'+(myFieldSet.items.items.length+1);
      var valueID = 'value'+(myFieldSet.items.items.length+1);
    }else{
      var nameID = 'key1';
      var valueID = 'value1';
    }
    var abc = me.addSettings("", "", nameID, valueID);
    myFieldSet.add(abc);
    myFieldSet.doLayout();
    myFieldSet.items.items[myFieldSet.items.length-1].items.items[0].allowBlank = false;

  },

  onApplicationFormAfterRender: function(component, eOpts) {
    var key = '';
    var value = '';
    var me = this;
    var tree = me.getDirTree();
    var node = tree.getSelectedNode();
    if (node.data.record != null) {
      if(node.data.record.Configuration!=null){
        if (node.data.record.Configuration.AppSettings != null) {
          if(node.data.record.Configuration.AppSettings.Settings!=null){
            for(var i=0;i<node.data.record.Configuration.AppSettings.Settings.length;i++){
              key = node.data.record.Configuration.AppSettings.Settings[i].Key;
              value = node.data.record.Configuration.AppSettings.Settings[i].Value;
              var newSetting = me.addSettings(key,value, ('key'+i), ('value'+i));
              newSetting[0].items[0].allowBlank = false;
              if(component.items.map['settingfieldset'])
              component.items.map['settingfieldset'].add(newSetting);
            }
          }
        }
      }
    }
  },

  onRefreshDataObjectCache: function(item, e, eOpts) {
    var me = this;
    var tree = me.getDirTree();
    var node = tree.getSelectedNode(); 

    Ext.Ajax.request({
      url: 'AdapterManager/RefreshObjectCache',
      method: 'POST',
      timeout: 3600000,  // 1 hour
      params: {
        'nodeid': node.data.id,//node.attributes.id,
        'objectType': node.data.text//node.text
      },
      success: function (response, request) {
        var responseObj = Ext.decode(response.responseText);

        if (responseObj.Level == 0) {
          showDialog(450, 100, 'Refresh Cache Result', 'Object cache refreshed successfully.', Ext.Msg.OK, null);
        }
        else {
          showDialog(500, 160, 'Refresh Cache Error', responseObj.Messages.join(), Ext.Msg.OK, null);
        }
      },
      failure: function (response, request) {
        showDialog(500, 160, 'Refresh Cache Error', responseObj.Messages.join(), Ext.Msg.OK, null);
      }
    })

  },

  onRefreshCache: function(item, e, eOpts) {

    var me = this;
    var tree = me.getDirTree();
    var node = tree.getSelectedNode(); 
    Ext.Ajax.request({
      url: 'AdapterManager/RefreshCache',
      method: 'POST',
      timeout: 28800000,  // 8 hours
      params: {
        'nodeid': node.data.id//node.attributes.id
      },
      success: function (response, request) {
        var responseObj = Ext.decode(response.responseText);

        if (responseObj.Level == 0) {
          showDialog(450, 100, 'Refresh Cache Result', 'Cache refreshed successfully.', Ext.Msg.OK, null);
        }
        else {
          showDialog(500, 160, 'Refresh Cache Error', responseObj.Messages.join(), Ext.Msg.OK, null);
        }
      },
      failure: function (response, request) {
        showDialog(500, 160, 'Refresh Cache Error', responseObj.Messages.join(), Ext.Msg.OK, null);
      }
    })
  },

  onImportCache: function(item, e, eOpts) {

    var me = this;
    var win = Ext.widget('importcachewindow');
    var form = win.down('form');
    var tree = me.getDirTree();
    var node = tree.getSelectedNode();

    var formRecord = {
      nodeid: node.data.id 
    };


    form.getForm().setValues(formRecord);

    win.on('Save', function () {
      win.destroy();
    }, me);

    win.on('reset', function () {
      win.destroy();
    }, me);

    win.show();
  },

  onDeleteCache: function(item, e, eOpts) {

    var me = this;
    var tree = me.getDirTree();
    var node = tree.getSelectedNode(); 
    Ext.Ajax.request({
      url: 'AdapterManager/DeleteCache',
      method: 'POST',
      timeout: 120000,  // 2 minutes
      params: {
        'nodeid': node.data.id//node.attributes.id
      },
      success: function (response, request) {
        var responseObj = Ext.decode(response.responseText);

        if (responseObj.Level == 0) {
          showDialog(450, 100, 'Delete Cache Result', 'Cache deleted successfully.', Ext.Msg.OK, null);
        }
        else {
          showDialog(500, 160, 'Delete Cache Error', responseObj.Messages.join(), Ext.Msg.OK, null);
        }
      },
      failure: function (response, request) {
        showDialog(500, 160, 'Delete Cache Error', responseObj.Messages.join(), Ext.Msg.OK, null);
      }
    })
  },

  init: function(application) {
    scopForExport = null;
    appForExport = null;
    Ext.QuickTips.init();

    this.control({
      "gridpanel": {
        metachange: this.handleMetachange
      },
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
      },
      "directorytree": {
        itemcontextmenu: this.showContextMenu,
        itemdblclick: this.onShowDataGridd
      },
      "menuitem[action=refreshdata]": {
        click: this.onAppDataRefreshClick
      },
      "textfield": {
        blur: this.onTextfieldBlur
      },
      "menuitem[action=fileupload]": {
        click: this.onFileUpload
      },
      "menuitem[action=filedownload]": {
        click: this.onFileDownload
      },
      "button[action = addsettings]": {
        click: this.onAddSettings
      },
      "form": {
        afterrender: this.onApplicationFormAfterRender
      },
      "menuitem[action=refreshdataobjectcache]": {
        click: this.onRefreshDataObjectCache
      },
      "menuitem[action=refreshcache]": {
        click: this.onRefreshCache
      },
      "menuitem[action=importcache]": {
        click: this.onImportCache
      },
      "menuitem[action = deletcache]": {
        click: this.onDeleteCache
      }
    });
  },

  onShowGrap: function(items, e, eOpts) {
    alert('onShowGrap...');
    var me = this;
    var tree = this.getDirTree();
    var node = tree.getSelectedNode();
    content = me.getMainContent();
    contextName = node.parentNode.parentNode.parentNode.data.property.Name;
    endpointName = node.parentNode.parentNode.data.property.Name;
    var graph = node.data.text;
  },

  addSettings: function(key, value, nameID, valueID) {
    return[ {
      xtype: 'container',
      margin:'10 20 0 96',
      //bodyStyle: 'padding:10px 20px 0 70px',
      layout:'column',
      items: [
      {
        xtype: 'textfield',
        name:nameID,
        value:key,
        columnWidth:'0.30',
        //width:164,
        allowBlank: true
      },
      {
        xtype: 'textarea',
        name:valueID,
        value:value,
        columnWidth:'0.60',
        grow : false,
        //width:270,  // height: 50,
        margin:'0 0 0 3',
        //margin:'0 0 0 3'
      },
      {
        xtype: 'button',
        //flex: 1,
        text: 'Delete',
        columnWidth:'0.10',
        margin:'0 0 0 3',
        //width:48,
        //margin:'0 0 0 3',
        //action:'DeleteMe',
        //icon :'../../ux/css/images/right2.gif',//'remove-button',
        //columnWidth: 0.10,
        //style: 'margin:0 0 0 49;',
        //style: 'float: right;',
        tooltip: 'Click to Delete settings',
        handler : function (){
          this.findParentByType('container').destroy();

        }
      }

      ]
    }

    ]
  }

});
