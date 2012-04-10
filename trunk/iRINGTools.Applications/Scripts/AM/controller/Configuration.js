Ext.define('AM.controller.Configuration', {
  extend: 'Ext.app.Controller',
  views: [
     'spreadsheet.SpreadsheetSource',
     'spreadsheet.SpreadsheetConfigPanel',
     'common.PropertyPanel',
     'common.ContentPanel',
     'common.CenterPanel',
     'nhibernate.TreePanel',
     'nhibernate.SetDataObjectPanel',
     'nhibernate.DataObjectPanel',
     'nhibernate.EditorPanel',
     'nhibernate.NHibernateTree',
     'nhibernate.RadioField',
     'nhibernate.ConnectDatabase',
     'nhibernate.SelectTablesPanel',
     'nhibernate.SelectKeysPanel',
     'nhibernate.SelectPropertiesPanel',
     'nhibernate.SetPropertyPanel',
     'nhibernate.SetKeyPanel',
     'nhibernate.CreateRelations',
     'nhibernate.SetRelationPanel',
     'nhibernate.Utility',
     'nhibernate.RadioField'
    ],
  stores: [
  //'ProviderStore'
    ],
  models: [
      'SpreadsheetModel',
      'NHibernateTreeModel',
      'ProviderModel'
    ],
  refs: [
      {
        ref: 'dirTree',
        selector: 'viewport > directorypanel > directorytree'
      },
      {
        ref: 'mainContent',
        selector: 'viewport > centerpanel > contentpanel'
      },
      {
        ref: 'dataTree',
        selector: 'nhibernatetreepanel'
      },
      {
        ref: 'editPanel',
        selector: 'editorpanel'
      },
      {
        ref: 'dataObjectPanel',
        selector: 'dataobjectpanel'
      },
      {
        ref: 'dsConfigPane',
        selector: 'connectdatabase'
      },
      {
        ref: 'radioField',
        selector: 'radiotextfield'
      }
    ],
  init: function () {
    this.control({
      'button[action=configureendpoint]': {
        click: this.onConfigureEndpoint
      },
      'button[action=uploadspreadsheet]': {
        click: this.onUploadspreadsheet
      },
      'button[action=savespreadsheet]': {
        click: this.onSaveSpreadsheet
      },
      'button[action=reloadspreadsheet]': {
        click: this.onReloadSpreadsheet
      },
      'button[action=reloaddataobjects]': {
        click: this.onReloadDataObjects
      },
      'button[action=editdbconnection]': {
        click: this.onEditDbConnection
      },
      'nhibernatetreepanel': {
        itemclick: this.onItemClick
      },
      'button[action=applydbobjectchanges]': {
        click: this.applyDbObjectChanges
      },
      'button[action=connecttodatabase]': {
        click: this.connectToDatabase
      },
      'button[action=applydatatables]': {
        click: this.applydatatables
      },
      'button[action=resettables]': {
        click: this.resettables
      }
    })
  },

  applyDbObjectChanges: function (btn, evt) {
    var content = this.getMainContent(); 
    var nhpan = content.items.map[contextName + '.' + endpoint + '.-nh-config']; 
    var datatree = nhpan.items.map[contextName + '.' + endpoint + '.-nh-tree'];
    var form = btn.up('form').getForm();
    form.url = 'nhibernate/updatedbobject';
    form.method = 'POST';
    if (form.isValid()) {
      var me = this;
      form.submit({
        success: function (f, a) {
          datatree.onReload();
        },
        failure: function (f, a) {
          //TODO:
        }
      });
    }
  },

  setPropertiesFolder: function (editor, node, context, endpoint) {

  },

  setDataProperty: function (editor, node, context, endpoint) {

  },

  setKeysFolder: function (editor, node, context, endpoint) {

  },

  setKeyProperty: function (editor, node, context, endpoint) {

  },

  setRelations: function (editor, node, context, endpoint) {

  },

  setRelationFields: function (editor, node, context, endpoint) {

  },

  setDataObject: function (editor, node, context, endpoint) {
    if (editor) {
      var content = this.getMainContent();
      var conf = {
        contextName: context,
        endpoint: endpoint,
        region: 'center',
        id: context + '.' + endpoint + '.tablesform'
      };
      var setdop = editor.items.map[conf.id];
      if (!setdop) {
        setdop = Ext.widget('setdataobjectpanel', conf);
        editor.items.add(setdop);
        editor.doLayout();
      }
      setdop.setActiveRecord(node.data.property);
      var panelIndex = editor.items.indexOf(setdop);
      editor.getLayout().setActiveItem(panelIndex);
      editor.doLayout();
    }
  },

  setTablesSelectorPane: function (contextName, endpoint, baseUrl) {    
    var dbDict = AM.view.nhibernate.dbDict.value;
    var content = this.getMainContent();
    var nhpan = content.items.map[contextName + '.' + endpoint + '.-nh-config']; 
    var editor = nhpan.items.map[contextName + '.' + endpoint + '.-nh-editor'];
    var dataTree = nhpan.items.map[contextName + '.' + endpoint + '.-nh-tree'];
    var dbInfo = AM.view.nhibernate.dbInfo.value;
    var conf = {
      contextName: contextName,
      endpoint: endpoint,
      baseUrl: baseUrl,
      dbInfo: dbInfo,
      dataTree: dataTree,
      height: 300,
      width: 400,
      region: 'center',
      id: contextName + '.' + endpoint + '.tablesselector'
    };
    var select = editor.items.map[conf.id];
    if (!select) {
      select = Ext.widget('selecttables', conf);
      editor.items.add(select);
      editor.doLayout();
    }
    var panelIndex = editor.items.indexOf(select);
    editor.getLayout().setActiveItem(panelIndex);    
  },

  resettables: function () {
  },

  onItemClick: function (view, model, n, index) {
    var dirtree = this.getDirTree(),
        node = dirtree.getSelectedNode();
    var content = this.getMainContent();
    var contextName = node.data.record.context;    
    var endpoint = node.data.record.endpoint;
    var baseUrl = node.data.record.BaseUrl;    
    var node = model.store.getAt(index);    
    var nhpan = content.items.map[contextName + '.' + endpoint + '.-nh-config']; 
    var editor = nhpan.items.map[contextName + '.' + endpoint + '.-nh-editor'];    
    var tree = nhpan.items.map[contextName + '.' + endpoint + '.-nh-tree'];   
     
    if (node.isRoot()) {
      this.setTablesSelectorPane(contextName, endpoint, baseUrl);
      return;
    }
    var nodeType = node.data.type.toUpperCase();

    if (nodeType) {
      switch (nodeType) {
        case 'DATAOBJECT':
          this.setDataObject(editor, node, tree.contextName, tree.endpoint);
          break;
        case 'KEYS':
          this.setKeysFolder(editor, node, tree.contextName, tree.endpoint);
          break;
        case 'KEYPROPERTY':
          this.setKeyProperty(editor, node, tree.contextName, tree.endpoint);
          break;
        case 'PROPERTIES':
          this.setPropertiesFolder(editor, node, tree.contextName, tree.endpoint);
          break;
        case 'DATAPROPERTY':
          this.setDataProperty(editor, node, tree.contextName, tree.endpoint);
          break;
        case 'RELATIONSHIPS':
          this.setRelations(editor, node, tree.contextName, tree.endpoint);
          break;
        case 'RELATIONSHIP':
          this.setRelationFields(editor, node, tree.contextName, tree.endpoint);
          break;
      }
    }
    else {
      editPane.hide();
    }
  },

  onReloadDataObjects: function () {
    var content = this.getMainContent(); 
    var nhpan = content.items.map[contextName + '.' + endpoint + '.-nh-config']; 
    var datatree = nhpan.items.map[contextName + '.' + endpoint + '.-nh-tree'];
    datatree.onReload();
  },

  onEditDbConnection: function (btn, evt) {
    var dirtree = this.getDirTree(),
        node = dirtree.getSelectedNode();   
    var content = this.getMainContent(); 
    var contextName = node.data.record.context;    
    var endpoint = node.data.record.endpoint;
    var baseUrl = node.data.record.BaseUrl;    
    var dbDict = AM.view.nhibernate.dbDict.value;
    var dbInfo = AM.view.nhibernate.dbInfo.value;       
   
    if (dbDict) {
      var cstr = dbDict.ConnectionString;
      if (cstr)
        var dbInfo = this.getConnStringParts(cstr);
      AM.view.nhibernate.dbInfo.value = dbInfo;
    }

    var conf = {
      contextName: contextName,
      dbDict: dbDict,
      dbInfo: dbInfo,
      endpoint: endpoint,
      baseUrl: baseUrl,
      id: contextName + '.' + endpoint + '.conform'
    };

    var content = this.getMainContent(); 
    var nhpan = content.items.map[contextName + '.' + endpoint + '.-nh-config']; 
    var editpan = nhpan.items.map[contextName + '.' + endpoint + '.-nh-editor'];
    var confrm = editpan.items.map[conf.id];
    if (!confrm) {
      confrm = Ext.widget('connectdatabase', conf);
      editpan.items.add(confrm);
      editpan.doLayout();
    } else {
      confrm.show();
    }   

    if (dbInfo)
      confrm.setActiveRecord(dbInfo);

    var panelIndex = editpan.items.indexOf(confrm);
    editpan.getLayout().setActiveItem(panelIndex);
  },

  getDataTypes: function () {
    Ext.Ajax.request({
      url: 'NHibernate/DataType',
      method: 'GET',
      timeout: 6000000,
      success: function (response, request) {
        var dataTypeName = Ext.JSON.decode(response.responseText);
        AM.view.nhibernate.dataTypes.value = new Array();
        var i = 0;
        while (!dataTypeName[i])
          i++;
        while (dataTypeName[i]) {
          AM.view.nhibernate.dataTypes.value.push([i, dataTypeName[i]]);
          i++;
        }
      },
      failure: function (f, a) {
        if (a.response)
          showDialog(500, 400, 'Error', a.response.responseText, Ext.Msg.OK, null);
      }
    });
  },

  getDbdictionary: function (contextName, endpoint, baseUrl, dirtree, content) {
    var me = this;    
    var content = me.getMainContent();       
    createMainContentPanel(content, contextName, endpoint, baseUrl);   
    Ext.Ajax.request({
      url: 'NHibernate/DBDictionary',
      method: 'POST',
      timeout: 6000000,
      params: {
        scope: contextName,
        app: endpoint,
        baseUrl: baseUrl
      },
      success: function (response, request) {
        AM.view.nhibernate.dbDict.value = Ext.JSON.decode(response.responseText);
        var dbDict = AM.view.nhibernate.dbDict.value;
        if (dbDict.ConnectionString != null) {
          var base64 = AM.view.nhibernate.Utility;
          AM.view.nhibernate.dbDict.value.ConnectionString = base64.decode(dbDict.ConnectionString);
          me.getDataTypes();

          if (dbDict) {          
            var cstr = dbDict.ConnectionString;
            if (cstr) {
              var dbInfo = me.getConnStringParts(cstr);
              AM.view.nhibernate.dbInfo.value = dbInfo;
              var selectTableNames = setTableNames(dbDict); 
              var content = me.getMainContent();    
              var nhpan = content.items.map[contextName + '.' + endpoint + '.-nh-config'];
              var datatree = nhpan.items.map[contextName + '.' + endpoint + '.-nh-tree'];
    
              datatree.on('beforeload', function (store, operation) {
                store.proxy.extraParams.dbProvider = dbDict.Provider;
                store.proxy.extraParams.dbServer = dbInfo.dbServer;
                store.proxy.extraParams.dbInstance = dbInfo.dbInstance;
                store.proxy.extraParams.dbName = dbInfo.dbName;
                store.proxy.extraParams.dbSchema = dbDict.SchemaName;
                store.proxy.extraParams.dbPassword = dbInfo.dbPassword;
                store.proxy.extraParams.dbUserName = dbInfo.dbUserName;
                store.proxy.extraParams.portNumber = dbInfo.portNumber;
                store.proxy.extraParams.tableNames = selectTableNames;
                store.proxy.extraParams.serName = dbInfo.serName;
                store.proxy.extraParams.contextName = contextName;
                store.proxy.extraParams.endpoint = endpoint;
                store.proxy.extraParams.baseUrl = baseUrl;        
              }, me);              

              nhpan.body.mask('Loading...', 'x-mask-loading');
              datatree.getStore().load();
              nhpan.body.unmask();
              dirtree.applicationMenu.hide();            
            }
          }
        }
        else {   
          var content = me.getMainContent(); 
          var nhpan = content.items.map[contextName + '.' + endpoint + '.-nh-config'];
          if (nhpan) {
            var datatree = nhpan.items.map[contextName + '.' + endpoint + '.-nh-tree'];         
            if (datatree)
              datatree.disable();
          }

          if (AM.view.nhibernate.dbInfo.value == null)
            AM.view.nhibernate.dbInfo.value = {};

          var dbTables = me.onEditDbConnection();
        }
      },
      failure: function (response, request) {
        //var dataObjPanel = content.items.map[contextName + '.' + endpoint + '.-nh-config'];;
      }
    });
  },

  applydatatables: function (btn, evt) {
    var dirtree = this.getDirTree(),
        node = dirtree.getSelectedNode();   
    var content = this.getMainContent(); 
    var contextName = node.data.record.context;    
    var endpoint = node.data.record.endpoint;
    var baseUrl = node.data.record.BaseUrl;    
    var content = this.getMainContent(); 
    var dataObjectPanel = content.items.map[contextName + '.' + endpoint + '.-nh-config'];;
    var dbDict = AM.view.nhibernate.dbDict.value;
    var dbInfo = AM.view.nhibernate.dbInfo.value;    
    var dbObjectsTree = dataObjectPanel.items.map[contextName + '.' + endpoint + '.-nh-tree'];
    var rootNode = dbObjectsTree.getRootNode();
    var tablesSelForm = btn.up('form');
    var dsConfigPane = this.getDsConfigPane();
    var radioField = this.getRadioField();
    var dbProvider = '';
    var dbServer = '';
    var dbInstance = '';
    var dbName = '';
    var dbSchema = '';
    var dbUserName = '';
    var dbPassword = '';
    var portNumber = '';
    var serName = '';

    if (dbObjectsTree.disabled) {
      dbObjectsTree.enable();
    }

    if (radioField) {
      var serviceNamePane = dsConfigPane.items.items[10];
      if (serviceNamePane.items.items[0])
        serName = serviceNamePane.items.items[0].serName;
    }
    else {
      if (dbInfo.serName)
        serName = dbInfo.serName;
    }

    if (tablesSelForm.getForm().findField('tableSelector').getValue().indexOf('') == -1)
      var selectTableNames = tablesSelForm.getForm().findField('tableSelector').getValue();
    else {
      var tableNames = tablesSelForm.getForm().findField('tableSelector').toMultiselect.store.data.items;
      var selectTableNames = new Array();
      for (var i = 0; i < tableNames.length; i++) {
        selectTableNames.push(tableNames[i].data.text);
      }
    }

    if (selectTableNames.length < 1) {
      var rootNode = dbObjectsTree.getRootNode();
      while (rootNode.firstChild) {
        rootNode.removeChild(rootNode.firstChild);
      }
      return;
    }

    userTableNames = new Array();

    if (selectTableNames[1]) {
      if (selectTableNames[1].length > 1 && selectTableNames[0].length > 1) {
        for (var i = 0; i < selectTableNames.length; i++) {
          userTableNames.push(selectTableNames[i]);
        }
      }
      else {
        userTableNames.push(selectTableNames)
      }
    }
    else {
      userTableNames.push(selectTableNames[0]);
    }

    if (dsConfigPane) {
      var dsConfigForm = dsConfigPane.getForm();
      dbProvider = dsConfigForm.findField('dbProvider').getValue();
      dbServer = dsConfigForm.findField('dbServer').getValue();
      dbInstance = dsConfigForm.findField('dbInstance').getValue();
      dbName = dsConfigForm.findField('dbName').getValue();
      dbSchema = dsConfigForm.findField('dbSchema').getValue();
      dbUserName = dsConfigForm.findField('dbUserName').getValue();
      dbPassword = dsConfigForm.findField('dbPassword').getValue();
      portNumber = dsConfigForm.findField('portNumber').getValue();
    }
    else {
      dbProvider = dbDict.Provider;
      dbServer = dbInfo.dbServer;
      dbInstance = dbInfo.dbInstance;
      dbName = dbInfo.dbName;
      dbSchema = dbDict.SchemaName;
      dbUserName = dbInfo.dbUserName;
      dbPassword = dbInfo.dbPassword;
      portNumber = dbInfo.portNumber;
    }

    var store = dbObjectsTree.getStore();
    dbObjectsTree.on('beforeload', function (store, operation) {
      store.proxy.extraParams.dbProvider = dbProvider;
      store.proxy.extraParams.dbServer = dbServer;
      store.proxy.extraParams.dbInstance = dbInstance;
      store.proxy.extraParams.dbName = dbName;
      store.proxy.extraParams.dbSchema = dbSchema;
      store.proxy.extraParams.dbPassword = dbPassword;
      store.proxy.extraParams.dbUserName = dbUserName;
      store.proxy.extraParams.portNumber = portNumber;
      store.proxy.extraParams.tableNames = selectTableNames;
      store.proxy.extraParams.serName = serName;
      store.proxy.extraParams.contextName = tablesSelForm.contextName;
      store.proxy.extraParams.endpoint = tablesSelForm.endpoint;
      store.proxy.extraParams.baseUrl = tablesSelForm.baseUrl;
    }, this);

    dataObjectPanel.body.mask('Loading...', 'x-mask-loading');
    store.load();
    dataObjectPanel.body.unmask();    
  },

  connectToDatabase: function (btn, evt) {
    var dirtree = this.getDirTree(),
        node = dirtree.getSelectedNode();   
    var contextName = node.data.record.context;   
    var endpoint = node.data.record.endpoint;
    var baseUrl = node.data.record.BaseUrl;    
    var dbDict = AM.view.nhibernate.dbDict.value;
    var content = this.getMainContent(); 
    var nhpan = content.items.map[contextName + '.' + endpoint + '.-nh-config']; 
    var datatree = nhpan.items.map[contextName + '.' + endpoint + '.-nh-tree'];
    var form = btn.up('form');

    form.getForm().submit({
      url: 'nhibernate/parseconnectionstring',
      method: 'POST',
      success: function (req, res) {
        dbDict.ConnectionString = Ext.JSON.decode(res.responseText);
      },
      failure: function (req, res) {
      }
    });
    var dbProvider = form.getForm().findField('dbProvider').getValue().toUpperCase();
    var dbName = form.getForm().findField('dbName');
    var portNumber = form.getForm().findField('portNumber');
    var host = form.getForm().findField('host');
    var dbServer = form.getForm().findField('dbServer');
    var dbInstance = form.getForm().findField('dbInstance');
    var serviceNamePane = form.items.items[10];
    var dbSchema = form.getForm().findField('dbSchema');
    var servieName = '';
    var serName = '';   

    if (dbProvider.indexOf('ORACLE') > -1) {
      dbServer.setValue(host.getValue());
      dbName.setValue(dbSchema.getValue());
      servieName = serviceNamePane.items.items[0].value;
      serName = serviceNamePane.items.items[0].serName;
      dbInstance.setValue(servieName);
    }
    else if (dbProvider.indexOf('MSSQL') > -1) {
      host.setValue(dbServer.getValue());
      serviceName = dbInstance.getValue();
    }
    else if (dbProvider.indexOf('MYSQL') > -1) {
      dbName.setValue(dbSchema.getValue());
      dbInstance.setValue(dbSchema.getValue());
    }
    var me = this;
    form.getForm().submit({
      url: 'nhibernate/TableNames',
      timeout: 600000,
      params: {
        scope: form.contextName,
        app: form.endpoint,
        serName: serName,
        baseUrl: baseUrl
      },
      success: function (f, a) {
        AM.view.nhibernate.dbInfo.value = form.getForm().getValues();
        var dbInfo = AM.view.nhibernate.dbInfo.value;
        var dbDict = AM.view.nhibernate.dbDict.value;
        var selected = {};
        for (var d in dbDict.dbObjects) {
          selected.push(d.objectName);
        }

        if (!dbInfo)
          dbInfo = {};

        dbInfo.dbTableNames = Ext.JSON.decode(a.response.responseText);
        me.setTablesSelectorPane(form.contextName, form.endpoint, form.baseUrl, dbInfo.dbTableNames.items, selected);
        return dbInfo.dbTableNames;
      },
      failure: function (f, a) {
        if (a.response)
          showDialog(500, 400, 'Error', a.response.responseText, Ext.Msg.OK, null);
        else {
          showDialog(400, 100, 'Warning', 'Please fill in every field in this form.', Ext.Msg.OK, null);
        }
      },
      waitMsg: 'Loading ...'
    });
  },

  getConnStringParts: function (connstring) {
    var connStrParts = connstring.split(';');    
    var dbDict = AM.view.nhibernate.dbDict.value;
    var provider = dbDict.Provider.toUpperCase();

    if (!AM.view.nhibernate.dbInfo.value) {
      AM.view.nhibernate.dbInfo = {};
    }        

    if (!AM.view.nhibernate.dbInfo.dbUserName)
      AM.view.nhibernate.dbInfo.dbName = dbDict.SchemaName;
      for (var i = 0; i < connStrParts.length; i++) {
        var pair = connStrParts[i].split('=');
        switch (pair[0].toUpperCase()) {
          case 'DATA SOURCE':
            if (provider.indexOf('MSSQL') > -1) {
              var dsValue = pair[1].split('\\');
              AM.view.nhibernate.dbInfo.dbServer = (dsValue[0].toLowerCase() == '.' ? 'localhost' : dsValue[0]);
              AM.view.nhibernate.dbInfo.dbInstance = dsValue[1];
              AM.view.nhibernate.dbInfo.portNumber = 1433;
              AM.view.nhibernate.dbInfo.serName = '';
            }
            else if (provider.indexOf('MYSQL') > -1) {
              AM.view.nhibernate.dbInfo.dbServer = (pair[1].toLowerCase() == '.' ? 'localhost' : pair[1]);
              AM.view.nhibernate.dbInfo.portNumber = 3306;
            }
            else if (provider.indexOf('ORACLE') > -1) {
              var dsStr = connStrParts[i].substring(12, connStrParts[i].length);
              var dsValue = dsStr.split('=');
              for (var j = 0; j < dsValue.length; j++) {
                dsValue[j] = dsValue[j].substring(dsValue[j].indexOf('(') + 1, dsValue[j].length);
                switch (dsValue[j].toUpperCase()) {
                  case 'HOST':
                    var server = dsValue[j + 1];
                    var port = dsValue[j + 2];
                    var index = server.indexOf(')');
                    server = server.substring(0, index);
                    AM.view.nhibernate.dbInfo.portNumber = port.substring(0, 4);
                    AM.view.nhibernate.dbInfo.dbServer = (server.toLowerCase() == '.' ? 'localhost' : server);
                    break;
                  case 'SERVICE_NAME':
                    var sername = dsValue[j + 1];
                    index = sername.indexOf(')');
                    AM.view.nhibernate.dbInfo.dbInstance = sername.substring(0, index);
                    AM.view.nhibernate.dbInfo.serName = 'SERVICE_NAME';
                    break;
                  case 'SID':
                    var sername = dsValue[j + 1];
                    index = sername.indexOf(')');
                    AM.view.nhibernate.dbInfo.dbInstance = sername.substring(0, index);
                    AM.view.nhibernate.dbInfo.serName = 'SID';
                    break;
                }
              }
            }
            break;
          case 'INITIAL CATALOG':
            AM.view.nhibernate.dbInfo.dbName = pair[1];
            break;
          case 'USER ID':
            AM.view.nhibernate.dbInfo.dbUserName = pair[1];
            break;
          case 'PASSWORD':
            AM.view.nhibernate.dbInfo.dbPassword = pair[1];
            break;
        }
      }
    return AM.view.nhibernate.dbInfo;
  },

  getTableNames: function (context, endpoint, baseUrl) {
    var dbInfo = AM.view.nhibernate.dbInfo.value;
    var dbDict = AM.view.nhibernate.dbDict.value;
    Ext.Ajax.request({
      url: 'NHibernate/TableNames',
      method: 'POST',
      timeout: 6000000,
      params: {
        dbProvider: dbDict.Provider,
        dbServer: dbInfo.dbServer,
        dbInstance: dbInfo.dbInstance,
        dbName: dbInfo.dbName,
        dbSchema: dbDict.SchemaName,
        dbUserName: dbInfo.dbUserName,
        dbPassword: dbInfo.dbPassword,
        portNumber: dbInfo.portNumber,
        tableNames: selectTableNames,
        serName: dbInfo.serName,
        baseUrl: baseUrl
      },
      success: function (response, request) {
        AM.view.nhibernate.dbTableNames = Ext.JSON.decode(response.responseText);
      },
      failure: function (f, a) {
        if (a.response)
          showDialog(500, 400, 'Error', a.response.responseText, Ext.Msg.OK, null);
      }
    });
  },

  onConfigureEndpoint: function () {    
    var dirtree = this.getDirTree(),
        node = dirtree.getSelectedNode();
    var content = this.getMainContent();
    var contextName = node.data.record.context;
    var datalayer = node.data.record.DataLayer;
    var endpoint = node.data.record.endpoint;
    var baseUrl = node.data.record.BaseUrl;    

    switch (datalayer) {
      case 'NHibernateLibrary':
        this.getDbdictionary(contextName, endpoint, baseUrl, dirtree, content);       
        break;
      case 'SpreadsheetDatalayer':
        var conf =
            {
              context: contextName,
              endpoint: endpoint,
              datalayer: datalayer,
              url: 'spreadsheet/configure'
            };
        var sctree = Ext.widget('spreadsheetconfig', conf);
        var scprop = Ext.widget('propertypanel', {
          title: 'Details',
          region: 'east',
          width: 350,
          height: 150,
          split: true,
          collapsible: true
        });
        var panconf = {
          id: 'tab-c.' + contextName + '.' + endpoint,
          title: 'Spreadsheet Configuration - ' + contextName + '.' + endpoint,
          height: 300,
          minSize: 250,
          layout: {
            type: 'border',
            padding: 2
          },
          split: true,
          closable: true,
          iconCls: 'tabsMapping',
          items: []
        },
        scpanel = Ext.widget('panel', panconf);
        scpanel.items.add(sctree);
        scpanel.items.add(scprop);
        sctree.on('beforeitemexpand', function () {
          content.getEl().mask('Loading...');
        }, this);

        sctree.on('load', function () {
          content.getEl().unmask();
        }, this);

        sctree.on('itemexpand', function () {
          content.getEl().unmask();
        }, this);

        sctree.on('itemclick', function (view, model, n, index) {
          var obj = model.store.getAt(index).data;
          if (obj.record != null && obj.record != "") {
            scprop.setSource(obj.record);
          }
        }, this);

        var exist = content.items.map[panconf.id];
        if (exist == null) {
          content.add(scpanel).show();
        } else {
          exist.show();
        }

        dirtree.applicationMenu.hide();
        break;
    }
  },

  setItemSelectorAvailValues: function (node) {
    var availItems = new Array();
    var propertiesNode = node.parentNode.childNodes[1];

    for (var i = 0; i < propertiesNode.childNodes.length; i++) {
      var itemName = propertiesNode.childNodes[i].text;
      var found = false;

      for (var j = 0; j < node.childNodes.length; j++) {
        if (node.childNodes[j].text.toLowerCase() == itemName.toLowerCase()) {
          found = true;
          break;
        }
      }
      if (!found) {
        availItems.push([itemName, itemName]);
      }
    }
    return availItems;
  },

  setItemSelectorselectedValues: function (node) {
    var selectedItems = new Array();
    var propertiesNode = node.parentNode.childNodes[1];

    for (var i = 0; i < node.childNodes.length; i++) {
      var keyName = node.childNodes[i].text;
      selectedItems.push([keyName, keyName]);
    }
    return selectedItems;
  },

  onSaveSpreadsheet: function () {
    var tree = this.getDirTree(),
        node = tree.getSelectedNode();
    var contextName = node.data.record.context;
    var datalayer = node.data.record.Assembly;
    var endpointName = node.data.record.endpoint;
    Ext.Ajax.request({
      url: 'spreadsheet/configure',    // where you wanna post
      method: 'POST',
      success: function (f, a) {

      },   // function called on success
      failure: function (f, a) {

      },
      params: {
        context: contextName,
        endpoint: endpointName,
        DataLayer: datalayer
      }
    });
  },
  onUploadspreadsheet: function (panel) {
    var tree = this.getDirTree(),
        node = tree.getSelectedNode();
    var contextName = node.data.record.context;
    var datalayer = node.data.record.DataLayer;
    var endpoint = node.data.record.endpoint;
    var that = this;
    var sourceconf = {
      width: 450,
      title: 'Upload ' + contextName + '-' + endpoint,
      context: contextName,
      endpoint: endpoint,
      DataLayer: datalayer,
      method: 'POST',
      url: 'spreadsheet/upload'
    },
        form = Ext.widget('spreadsheetsource', sourceconf);
    form.show();
  },

   onReloadSpreadsheet: function () {
   this.getMainContent().items.items[0].items.items[0].getStore().load();
     },

});