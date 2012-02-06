Ext.define('AM.controller.ConfigurationController', {
    extend: 'Ext.app.Controller',
    views: [
      'spreadsheet.SpreadsheetSource'
    , 'spreadsheet.SpreadsheetConfigPanel'
    , 'common.PropertyPanel'
    , 'common.ContentPanel'
    , 'common.CenterPanel'
    , 'nhibernate.TreePanel'
    , 'nhibernate.SetDataObjectPanel'
    , 'nhibernate.DataObjectPanel'
    , 'nhibernate.EditorPanel'
    , 'nhibernate.NHibernateTree'
    , 'nhibernate.RadioField'
    , 'nhibernate.ConnectDatabase'
    , 'nhibernate.SelectTablesPanel'
    , 'nhibernate.SelectKeysPanel'
    , 'nhibernate.SelectPropertiesPanel'
    , 'nhibernate.SetPropertyPanel'
    , 'nhibernate.SetKeyPanel'
    , 'nhibernate.CreateRelations'
    , 'nhibernate.SetRelationPanel'
    , 'nhibernate.Utility'
    ],
    stores: [
       'ProviderStore'
    ],
    models: [
       'SpreadsheetModel'
      , 'NHibernateTreeModel'
      , 'ProviderModel'
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
           ref: 'dataPanel',
           selector: 'nhibernatepanel'
       },
      {
          ref: 'editPanel',
          selector: 'editorpanel'
      },
      {
          ref: 'dataObjectPanel',
          selector: 'dataobjectpanel'
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
            }
        })
    },

    applyDbObjectChanges: function (btn, evt) {
        var form = btn.up('form').getForm();
        alert('clicked apply on dataobject changes');
    },

    setKeysFolder: function (editor, node, context, endpoint) {

    },
    setDataObject: function (editor, node, context, endpoint) {
        if (editor) {
            var content = this.getMainContent();
            var conf = {
                contextName: context,
                endpoint: endpoint,
                width: 400,
                region: 'center',
                id: context + '.' + endpoint + '.tablesform'
            };
            var setdop = editor.items.map[conf.id];
            if (!setdop) {
                setdop = Ext.widget('setdataobjectpanel', conf);
                editor.items.add(setdop);
            }
            setdop.setActiveRecord(node.data.property);
            editor.doLayout();
        }


    },
    setTablesSelectorPane: function (node, editor, context, endpoint) {
        if (editor) {
            var dbDict = AM.view.nhibernate.dbDict.value;
            if (dbDict)
                var selTables = [];
            for (var ijk = 0; ijk < dbDict.dataObjects.length; ijk++) {
                var dataObject = dbDict.dataObjects[ijk];
                selTables.push(dataObject.objectName);
            };
            var content = this.getMainContent();
            var conf = {
                contextName: context,
                endpoint: endpoint,
                selectItems: selTables,
                height: 300,
                width: 400,
                //            layout: 'fit',
                region: 'center',
                id: context + '.' + endpoint + '.tablesform'
            };
            var select = Ext.widget('selecttables', conf);

            editor.items.removeAt(0);
            editor.items.add(select);
            editor.doLayout();
        }
    },

    onItemClick: function (view, model, n, index) {
        var node = model.store.getAt(index);
        var tree = this.getDataTree();
        var content = this.getMainContent();
        if (node.isRoot()) {
            var editor = this.getEditPanel();

            this.setTablesSelectorPane(node, editor, tree.contextName, tree.endpoint);
            return;
        }
        else if (!node)
            return;
        var nodeType = node.data.type.toUpperCase();

        if (!nodeType && node.data.attributes)
            nodeType = node.data.parentNode.type.toUpperCase();

        if (nodeType) {
            var editor = this.getEditPanel();
            //            editor.show();
            //            var editorLayout = editor.getLayout();
            switch (nodeType) {
                case 'DATAOBJECT':
                    this.setDataObject(editor, node, tree.contextName, tree.endpoint);
                    break;
                case 'KEYS':
                    this.setKeysFolder(editor, node, tree.contextName, tree.endpoint);
                    break;
                case 'KEYPROPERTY':
                    setKeyProperty(editor, node, tree.contextName, tree.endpoint);
                    break;
                case 'PROPERTIES':
                    setPropertiesFolder(editor, node, tree.contextName, tree.endpoint);
                    break;
                case 'DATAPROPERTY':
                    setDataProperty(editor, node, tree.contextName, tree.endpoint);
                    break;
                case 'RELATIONSHIPS':
                    setRelations(editor, node, tree.contextName, tree.endpoint);
                    break;
                case 'RELATIONSHIP':
                    setRelationFields(editor, node, tree.contextName, tree.endpoint);
                    break;
            }
        }
        else {
            editPane.hide();
        }
    },

    onReloadDataObjects: function () {
        alert('Clicked reload dataobjects button');
    },

    onEditDbConnection: function (btn, evt) {
        //var content = this.getMainContent();
        //var dop = this.getDataObjectPanel();
        var ePan = this.getEditPanel();
        var conf = {
            contextName: ePan.contextName,
            endpoint: ePan.endpoint,
            width: 350,
            region: 'center',
            id: ePan.contextName + '.' + ePan.endpoint + '.conform'
        };
        var confrm = Ext.widget('connectdatabase', conf);
        var exist = ePan.items.map[conf.id];
        if (!exist) {
            ePan.items.add(confrm);
            ePan.doLayout();
        }
        else {
            ePan.items.removeAt(0);
            ePan.items.add(confrm);
            ePan.doLayout();
        }
        //alert('Clicked edit db connection button');
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

    getDbdictionary: function (context, endpoint) {
        Ext.Ajax.request({
            url: 'NHibernate/DBDictionary',
            method: 'POST',
            timeout: 6000000,
            params: {
                scope: context,
                app: endpoint
            },
            success: function (response, request) {
                AM.view.nhibernate.dbDict.value = Ext.JSON.decode(response.responseText);
                var dbDict = AM.view.nhibernate.dbDict.value;
                if (dbDict.ConnectionString) {
                    var base64 = AM.view.nhibernate.Utility;
                    AM.view.nhibernate.dbDict.value.ConnectionString = base64.decode(dbDict.ConnectionString);
                }
            },
            failure: function (response, request) {
                //                setDsConfigPane(scopeName, appName);
            }
        });
    },

    getConnStringParts: function (connstring) {
        var connStrParts = connstring.split(';');
        var dbInfo = AM.view.nhibernate.dbInfo.value;
        var dbDict = AM.view.nhibernate.dbDict.value;
        var provider = dbDict.Provider.toUpperCase();
        dbInfo.dbName = dbDict.SchemaName;

        if (!dbInfo.dbUserName)
            for (var i = 0; i < connStrParts.length; i++) {
                var pair = connStrParts[i].split('=');
                switch (pair[0].toUpperCase()) {
                    case 'DATA SOURCE':
                        if (provider.indexOf('MSSQL') > -1) {
                            var dsValue = pair[1].split('\\');
                            dbInfo.dbServer = (dsValue[0].toLowerCase() == '.' ? 'localhost' : dsValue[0]);
                            dbInfo.dbInstance = dsValue[1];
                            dbInfo.portNumber = 1433;
                            dbInfo.serName = '';
                        }
                        else if (provider.indexOf('MYSQL') > -1) {
                            dbInfo.dbServer = (pair[1].toLowerCase() == '.' ? 'localhost' : pair[1]);
                            dbInfo.portNumber = 3306;
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
                                        dbInfo.portNumber = port.substring(0, 4);
                                        dbInfo.dbServer = (server.toLowerCase() == '.' ? 'localhost' : server);
                                        break;
                                    case 'SERVICE_NAME':
                                        var sername = dsValue[j + 1];
                                        index = sername.indexOf(')');
                                        dbInfo.dbInstance = sername.substring(0, index);
                                        dbInfo.serName = 'SERVICE_NAME';
                                        break;
                                    case 'SID':
                                        var sername = dsValue[j + 1];
                                        index = sername.indexOf(')');
                                        dbInfo.dbInstance = sername.substring(0, index);
                                        dbInfo.serName = 'SID';
                                        break;
                                }
                            }
                        }
                        break;
                    case 'INITIAL CATALOG':
                        dbInfo.dbName = pair[1];
                        break;
                    case 'USER ID':
                        dbInfo.dbUserName = pair[1];
                        break;
                    case 'PASSWORD':
                        dbInfo.dbPassword = pair[1];
                        break;
                }
            }
    },

    getTableNames: function (context, endpoint) {
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
                serName: dbInfo.serName
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

        switch (datalayer) {
            case 'NHibernateLibrary':
                if (!AM.view.nhibernate.dbDict.value) {
                    this.getDbdictionary(contextName, endpoint);
                    this.getDataTypes();
                };
                // this.getTableNames(contextName, endpoint);
                conf = {
                    id: contextName + '.' + endpoint + '.-nh-config',
                    title: 'NHibernate Configuration - ' + contextName + '.' + endpoint,
                    contextName: contextName,
                    endpoint: endpoint,
                    //region: 'center',
                    layout: {
                        type: 'border',
                        padding: 2
                    },
                    split: true,
                    closable: true
                };
                var treeconf = {
                    contextName: contextName,
                    endpoint: endpoint,
                    region: 'west',
                    layout: 'fit'
                };
                var editconf = {
                    contextName: contextName,
                    endpoint: endpoint,
                    region: 'center',
                    width: 400,
                    height: 200
                };
                var nhpan = Ext.widget('panel', conf);

                var nhtree = Ext.widget('nhibernatetreepanel', treeconf);
                nhtree.on('beforeload', function (store, operation) {
                    if (operation.node != undefined) {
                        var operationNode = operation.node.data;
                        var param = store.proxy.extraParams;
                        if (operationNode.type != undefined)
                            param.type = operationNode.type;
                        if (operationNode.record && operationNode.record.Name != null)
                            param.id = operationNode.record.Name;
                        if (operationNode.record != undefined && operationNode.record.Related != undefined)
                            param.related = operationNode.record.Related;
                        if (operationNode.record != undefined) {
                            operationNode.leaf = false;
                            if (operationNode.record.context)
                                param.contextName = operationNode.record.context;
                            if (operationNode.record.endpoint)
                                param.endpoint = operationNode.record.endpoint;
                            if (operationNode.record.securityRole)
                                param.security = operationNode.record.securityRole;
                            if (operationNode.text != undefined)
                                param.text = operationNode.text;
                        }
                        else if (operationNode.property != undefined) {
                            operationNode.leaf = false;
                            if (operationNode.property.context)
                                param.contextName = operationNode.property.context;
                            if (operationNode.property.endpoint)
                                param.endpoint = operationNode.property.endpoint;
                            if (operationNode.text != undefined)
                                param.text = operationNode.text;
                        }
                    }
                }, this);

                var editpan = Ext.widget('editorpanel', editconf);
                nhpan.items.add(nhtree);
                nhpan.items.add(editpan);
                var exist = content.items.map[nhpan.id];

                if (exist == undefined) {
                    content.add(nhpan).show();
                } else {
                    exist.show();
                }
                dirtree.applicationMenu.hide();
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
                var scprop = Ext.widget('propertypanel', { title: 'Details', region: 'east', width: 350, height: 150, split: true, collapsible: true });
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
    }
});