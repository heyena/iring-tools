Ext.define('AM.controller.Configuration', {
  extend: 'Ext.app.Controller',
  views: [
     'spreadsheet.SpreadsheetSource',
     'spreadsheet.SpreadsheetConfigPanel',
     'common.PropertyPanel',
     'common.ContentPanel',
     'common.CenterPanel',
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
  stores: [],
  models: [
      'SpreadsheetModel',
      'NHibernateTreeModel',
      'ProviderModel'
    ],
  refs: [{
    ref: 'dirTree',
    selector: 'viewport > directorypanel > directorytree'
  }, {
    ref: 'mainContent',
    selector: 'viewport > centerpanel > contentpanel'
  }, {
    ref: 'dataTree',
    selector: 'nhibernatetreepanel'
  }, {
    ref: 'editPanel',
    selector: 'editorpanel'
  }, {
    ref: 'dataObjectPanel',
    selector: 'dataobjectpanel'
  }, {
    ref: 'dsConfigPane',
    selector: 'connectdatabase'
  }, {
    ref: 'radioField',
    selector: 'radiotextfield'
  }],

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
      }
    })
  },

  applyDbObjectChanges: function (btn, evt) {
    var thisForm = btn.up('form');
    var contextName = thisForm.contextName;
    var endpoint = thisForm.endpoint;    
    var content = this.getMainContent();
    var nhpan = content.items.map[contextName + '.' + endpoint + '.-nh-config'];
    var editor = nhpan.items.map[contextName + '.' + endpoint + '.-nh-editor'];
    var datatree = nhpan.items.map[contextName + '.' + endpoint + '.-nh-tree'];
    var node = datatree.getSelectionModel().selected.items[0];
    var dbDict = AM.view.nhibernate.dbDict.value;
    var form = thisForm.getForm();

    if (node) {
      var treeNodeProps = node.data.property;
      var objNam = form.findField('objectName').getValue();
      var oldObjNam = treeNodeProps['objectName'];
      treeNodeProps.tableName = form.findField('tableName').getValue();
      treeNodeProps.objectName = objNam;
      treeNodeProps.keyDelimiter = form.findField('keyDelimeter').getValue();
      treeNodeProps.description = form.findField('description').getValue();

      for (var ijk = 0; ijk < dbDict.dataObjects.length; ijk++) {
        var dataObject = dbDict.dataObjects[ijk];
        if (node.data.text.toUpperCase() == dataObject.objectName.toUpperCase()) {
          dataObject.objectName = objNam;
          break;
        }
      }

      node.set('text', objNam);
      var rootNode = datatree.getRootNode();

      for (var i = 0; i < rootNode.childNodes.length; i++) {
        var folderNode = rootNode.childNodes[i];
        var folderNodeProp = folderNode.data.property;
        if (folderNode.childNodes[2])
          var relationFolderNode = folderNode.childNodes[2];

        if (!relationFolderNode)
          continue;

        if (relationFolderNode.childNodes)
          var relChildenNodes = relationFolderNode.childNodes;

        if (relChildenNodes) {
          for (var k = 0; k < relChildenNodes.length; k++) {
            var relationNode = relChildenNodes[k];

            if (relationNode.text == '')
              continue;

            if (relationNode.data)
              var relationNodeAttr = relationNode.data

            var relObjNam = relationNodeAttr.relatedObjectName;
            if (relObjNam.toLowerCase() != objNam.toLowerCase() && relObjNam.toLowerCase() == oldObjNam.toLowerCase())
              relationNodeAttr.relatedObjectName = objNam;

            var relatedObjPropMap = relationNodeAttr.relatedObjMap;

            for (var iki = 0; iki < relatedObjPropMap.length; iki++) {
              if (relatedObjPropMap[iki].relatedObjName.toLowerCase() == oldObjNam.toLowerCase())
                relatedObjPropMap[iki].relatedObjName = objNam;
            }
          }
        }
      }

      var items = editor.items.items;

      for (var i = 0; i < items.length; i++) {
        var relateObjField = items[i].getForm().findField('relatedObjectName');
        if (relateObjField)
          if (relateObjField.getValue().toLowerCase() == oldObjNam.toLowerCase())
            relateObjField.setValue(objNam);
      }
    }

  },

  onItemClick: function (view, model, n, index) {
    var me = this;
    var thisPanel = view.up('panel');
    var contextName = thisPanel.contextName;
    var endpoint = thisPanel.endpoint;
    var baseUrl = thisPanel.baseUrl;
    var content = this.getMainContent();
    var node = model.store.getAt(index);
    var nhpan = content.items.map[contextName + '.' + endpoint + '.-nh-config'];
    var editor = nhpan.items.map[contextName + '.' + endpoint + '.-nh-editor'];
    var tree = nhpan.items.map[contextName + '.' + endpoint + '.-nh-tree'];
    var dbDict = AM.view.nhibernate.dbDict.value;
    var dbInfo = AM.view.nhibernate.dbInfo.value;

    if (node.isRoot()) {
      setTablesSelectorPane(me, editor, tree, nhpan, dbDict, dbInfo, contextName, endpoint, baseUrl);
      return;
    }
    var nodeType = node.data.type.toUpperCase();

    if (nodeType) {
      switch (nodeType) {
        case 'DATAOBJECT':
          setDataObject(me, editor, node, contextName, endpoint);
          break;
        case 'KEYS':
          setKeysFolder(me, editor, node, contextName, endpoint);
          break;
        case 'KEYPROPERTY':
          setKeyProperty(me, editor, node, contextName, endpoint);
          break;
        case 'PROPERTIES':
          setPropertiesFolder(me, editor, node, contextName, endpoint);
          break;
        case 'DATAPROPERTY':
          setDataProperty(me, editor, node, contextName, endpoint);
          break;
        case 'RELATIONSHIPS':
          setRelations(me, editor, tree, nhpan, dbDict, dbInfo, node, contextName, endpoint);
          break;
        case 'RELATIONSHIP':
          setRelationFields(me, editor, tree, nhpan, dbDict, dbInfo, node, contextName, endpoint);
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
    if (btn) {
      var thisPanel = btn.up('panel');
      var contextName = thisPanel.contextName;
      var endpoint = thisPanel.endpoint;
      var baseUrl = thisPanel.baseUrl;
    } else {
      var dirtree = this.getDirTree(),
          node = dirtree.getSelectedNode();
      var contextName = node.data.record.context;
      var endpoint = node.data.record.endpoint;
      var baseUrl = node.data.record.BaseUrl;
    }
    var content = this.getMainContent();
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
      id: contextName + '.' + endpoint + '.connectdatabase'
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
              me.getTableNames(contextName, endpoint, baseUrl);
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
    var form = btn.up('form');
    var dirtree = this.getDirTree(),
        node = dirtree.getSelectedNode();
    var content = this.getMainContent();
    var contextName = form.contextName;
    var endpoint = form.endpoint;
    var baseUrl = form.baseUrl;
    var content = this.getMainContent();
    var dataObjectPanel = content.items.map[contextName + '.' + endpoint + '.-nh-config']; ;
    var dbDict = AM.view.nhibernate.dbDict.value;
    var dbInfo = AM.view.nhibernate.dbInfo.value;
    var dbObjectsTree = dataObjectPanel.items.map[contextName + '.' + endpoint + '.-nh-tree'];
    var editorPane = dataObjectPanel.items.map[contextName + '.' + endpoint + '.-nh-editor'];
    var rootNode = dbObjectsTree.getRootNode();
    var tablesSelForm = editorPane.items.map[contextName + '.' + endpoint + '.tablesselector'];
    var dsConfigPane = editorPane.items.map[contextName + '.' + endpoint + '.connectdatabase'];
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

    if (dsConfigPane) {
      if (dsConfigPane.items.map[contextName + '.' + endpoint + 'radioField']) {
        var serviceNamePane = dsConfigPane.items.map[contextName + '.' + endpoint + 'radioField'];
        if (serviceNamePane.items.items[0])
          serName = serviceNamePane.items.items[0].serName;
      } else {
        if (dbInfo.serName)
          serName = dbInfo.serName;
      }
    } else {
      if (dbInfo.serName)
        serName = dbInfo.serName;
    }

    if (tablesSelForm.getForm().findField('tableSelector').getValue().indexOf('') == -1)
      var selectTableNames = tablesSelForm.getForm().findField('tableSelector').getValue();

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
      store.proxy.extraParams.contextName = contextName;
      store.proxy.extraParams.endpoint = endpoint;
      store.proxy.extraParams.baseUrl = baseUrl;
    }, this);

    dataObjectPanel.body.mask('Loading...', 'x-mask-loading');
    store.load();
    dataObjectPanel.body.unmask();
  },

  connectToDatabase: function (btn, evt) {
    var thisForm = btn.up('form');
    var dirtree = this.getDirTree(),
      node = dirtree.getSelectedNode();
    var contextName = thisForm.contextName;
    var endpoint = thisForm.endpoint;
    var baseUrl = thisForm.baseUrl;
    var dbDict = AM.view.nhibernate.dbDict.value;
    var content = this.getMainContent();
    var nhpan = content.items.map[contextName + '.' + endpoint + '.-nh-config'];
    var datatree = nhpan.items.map[contextName + '.' + endpoint + '.-nh-tree'];
    var editorPane = nhpan.items.map[contextName + '.' + endpoint + '.-nh-editor'];
    var form = editorPane.items.map[contextName + '.' + endpoint + '.connectdatabase'];
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
    AM.view.nhibernate.dbDict.value.Provider = dbProvider;

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
        scope: contextName,
        app: endpoint,
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
        var nulVar = null;
        setTablesSelectorPane(me, nulVar, nulVar, nulVar, dbDict, dbInfo, contextName, endpoint, baseUrl);
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
        scope: context,
        app: endpoint,
        dbProvider: dbDict.Provider,
        dbServer: dbInfo.dbServer,
        dbInstance: dbInfo.dbInstance,
        dbName: dbInfo.dbName,
        dbSchema: dbDict.SchemaName,
        dbUserName: dbInfo.dbUserName,
        dbPassword: dbInfo.dbPassword,
        portNumber: dbInfo.portNumber,
        serName: dbInfo.serName,
        baseUrl: baseUrl
      },
      success: function (response, request) {
        AM.view.nhibernate.dbInfo.dbTableNames = Ext.JSON.decode(response.responseText);
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
        var conf = {
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
  }

});