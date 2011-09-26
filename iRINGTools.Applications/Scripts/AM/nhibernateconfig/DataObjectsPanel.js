Ext.define('NhibernateConfig.DataObjectsPanel', {
  extend: 'Ext.panel.Panel',
  alias: 'widget.NhibernateConfig.DataObjectsPanel',
  layout: 'border',
  //id: scopeName + '.' + appName + '.dataObjectsPane',
  frame: false,
  border: false,
  items: [{
    xtype: 'panel',
    name: 'data-objects-pane',
    region: 'west',
    minWidth: 240,
    width: 300,
    split: true,
    autoScroll: true,
    bodyStyle: 'background:#fff',
    items: [{
      xtype: 'treepanel',
      border: false,
      autoScroll: true,
      animate: true,
      lines: true,
      frame: false,
      enableDD: false,
      containerScroll: true,
      rootVisible: true,
      store: this.dataObectsStore,
      //          root: {
      //            text: 'Data Objects',
      //            nodeType: 'async',
      //            iconCls: 'folder'
      //          },
      //          loader: new Ext.tree.TreeLoader(),
      tbar: new Ext.toolbar.Toolbar({
        items: [{
          xtype: 'tbspacer',
          width: 4
        }, {
          xtype: 'button',
          icon: 'Content/img/16x16/view-refresh.png',
          text: 'Reload',
          tooltip: 'Reload Data Objects',
          handler: function () {
            var editPane = dataObjectsPane.items.items[1];
            var items = editPane.items.items;

            for (var i = 0; i < items.length; i++) {
              items[i].destroy();
              i--;
            }

            Ext.Ajax.request({
              url: 'AdapterManager/DBDictionary',
              method: 'POST',
              params: {
                scope: scopeName,
                app: appName
              },
              success: function (response, request) {
                dbDict = Ext.JSON.decode(response.responseText);

                var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];

                if (dbDict.dataObjects.length > 0) {
                  // populate data source form
                  showTree(dbObjectsTree);
                }
                else {
                  dbObjectsTree.disable();
                  editPane = dataObjectsPane.items.items[1];
                  if (!editPane) {
                    var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
                  }
                  setDsConfigPane(editPane);
                }
              },
              failure: function (response, request) {
                editPane = dataObjectsPane.items.items[1];
                if (!editPane) {
                  var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
                }
                setDsConfigPane(editPane);
                editPane.getLayout().setActiveItem(editPane.items.length - 1);
              }
            });
          }
        }, {
          xtype: 'tbspacer',
          width: 4
        }, {
          xtype: 'button',
          icon: 'Content/img/16x16/document-properties.png',
          text: 'Edit Connection',
          tooltip: 'Edit database connection',
          handler: function () {
            editPane = dataObjectsPane.items.items[1];
            if (!editPane) {
              var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
            }
            setDsConfigPane(editPane);
          }
        }, {
          xtype: 'tbspacer',
          width: 4
        }, {
          xtype: 'button',
          icon: 'Content/img/16x16/document-save.png',
          text: 'Save',
          tooltip: 'Save the data objects tree to the back-end server',
          formBind: true,
          handler: function (button) {
            editPane = dataObjectsPane.items.items[1];
            if (!editPane) {
              var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
            }

            var treeProperty = {};
            treeProperty.dataObjects = new Array();
            var dsConfigPane = editPane.items.map[scopeName + '.' + appName + '.dsconfigPane'];
            var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];
            var rootNode = dbObjectsTree.getRootNode();
            treeProperty.IdentityConfiguration = null;
            if (dsConfigPane) {
              var dsConfigForm = dsConfigPane.getForm();
              treeProperty.provider = dsConfigForm.findField('dbProvider').getValue();
              var dbServer = dsConfigForm.findField('dbServer').getValue();
              dbServer = (dbServer == 'localhost' ? '.' : dbServer);
              var upProvider = treeProperty.provider.toUpperCase();

              if (upProvider.indexOf('MSSQL') > -1) {
                var dbInstance = dsConfigForm.findField('dbInstance').getValue();
                var dbDatabase = dsConfigForm.findField('dbName').getValue();
                if (dbInstance.toUpperCase() == "DEFAULT") {
                  var dataSrc = 'Data Source=' + dbServer + ';Initial Catalog=' + dbDatabase;
                } else {
                  var dataSrc = 'Data Source=' + dbServer + '\\' + dbInstance + ';Initial Catalog=' + dbDatabase;
                }
              }
              else if (upProvider.indexOf('ORACLE') > -1)
                var dataSrc = 'Data Source=' + '(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=' + dbServer + ')(PORT=' + dsConfigForm.findField('portNumber').getValue() + ')))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=' + dsConfigForm.findField('serviceName').getValue() + ')))';
              else if (upProvider.indexOf('MYSQL') > -1)
                var dataSrc = 'Data Source=' + dbServer;

              treeProperty.connectionString = dataSrc
                                            + ';User ID=' + dsConfigForm.findField('dbUserName').getValue()
                                            + ';Password=' + dsConfigForm.findField('dbPassword').getValue();

              treeProperty.schemaName = dsConfigForm.findField('dbSchema').getValue();
            }
            else {
              treeProperty.provider = dbDict.Provider;
              var dbServer = dbInfo.dbServer;
              var upProvider = treeProperty.provider.toUpperCase();
              dbServer = (dbServer == 'localhost' ? '.' : dbServer);

              if (upProvider.indexOf('MSSQL') > -1) {
                if (dbInfo.dbInstance) {
                  if (dbInfo.dbInstance.toUpperCase() == "DEFAULT") {
                    var dataSrc = 'Data Source=' + dbServer + ';Initial Catalog=' + dbInfo.dbName;
                  } else {
                    var dataSrc = 'Data Source=' + dbServer + '\\' + dbInfo.dbInstance

                                + ';Initial Catalog=' + dbInfo.dbName;
                  }
                }
              }
              else if (upProvider.indexOf('ORACLE') > -1)
                var dataSrc = 'Data Source=' + '(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=' + dbServer + ')(PORT=' + dbInfo.portNumber + ')))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=' + dbInfo.dbInstance + ')))';
              else if (upProvider.indexOf('MYSQL') > -1)
                var dataSrc = 'Data Source=' + dbServer;

              treeProperty.connectionString = dataSrc
                                            + ';User ID=' + dbInfo.dbUserName
                                            + ';Password=' + dbInfo.dbPassword;
              treeProperty.schemaName = dbDict.SchemaName;
            }

            if (!dbDict.ConnectionString) {
              dbDict.ConnectionString = treeProperty.connectionString;
              dbDict.SchemaName = treeProperty.schemaName;
              dbDict.Provider = treeProperty.provider;
              dbDict.dataObjects = userTableNames;
            }

            var keyName;
            for (var i = 0; i < rootNode.childNodes.length; i++) {
              var folderNode = rootNode.childNodes[i];
              var folderNodeProp = folderNode.attributes.properties;
              var folder = {};
              folder.tableName = folderNodeProp.tableName;
              folder.objectNamespace = folderNodeProp.objectNamespace;
              folder.objectName = folderNodeProp.objectName;

              if (!folderNodeProp.keyDelimeter)
                folder.keyDelimeter = 'null';
              else
                folder.keyDelimeter = folderNodeProp.keyDelimeter;

              folder.keyProperties = new Array();
              folder.dataProperties = new Array();
              folder.dataRelationships = new Array();

              for (var j = 0; j < folderNode.attributes.children.length; j++) {
                if (folderNode.childNodes[1])
                  var propertyFolderNode = folderNode.childNodes[1];
                else
                  var propertyFolderNode = folderNode.attributes.children[1];

                if (folderNode.childNodes[0])
                  var keyFolderNode = folderNode.childNodes[0];
                else
                  var keyFolderNode = folderNode.attributes.children[0];

                if (folderNode.childNodes[2])
                  var relationFolderNode = folderNode.childNodes[2];
                else
                  var relationFolderNode = folderNode.attributes.children[2];

                if (folderNode.childNodes[j])
                  subFolderNodeText = folderNode.childNodes[j].text;
                else
                  subFolderNodeText = folderNode.attributes.children[j].text;

                switch (subFolderNodeText) {
                  case 'Keys':
                    if (folderNode.childNodes[1])
                      var keyChildenNodes = keyFolderNode.childNodes;
                    else
                      var keyChildenNodes = keyFolderNode.children;

                    for (var k = 0; k < keyChildenNodes.length; k++) {
                      var keyNode = keyChildenNodes[k];

                      if (!keyNode.hidden) {
                        var keyProps = {};
                        keyProps.keyPropertyName = keyNode.text;
                        keyName = keyNode.text;
                        folder.keyProperties.push(keyProps);

                        var tagProps = {};
                        tagProps.columnName = keyNode.text;
                        tagProps.propertyName = keyNode.text;
                        tagProps.dataType = 10;
                        tagProps.dataLength = 100;
                        tagProps.isNullable = 'false';
                        tagProps.keyType = 1;
                        tagProps.showOnIndex = 'false';
                        tagProps.numberOfDecimals = 0;
                        folder.dataProperties.push(tagProps);
                      }
                    }
                    break;
                  case 'Properties':
                    if (folderNode.childNodes[1])
                      var propChildenNodes = propertyFolderNode.childNodes;
                    else
                      var propChildenNodes = propertyFolderNode.children;
                    for (var k = 0; k < propChildenNodes.length; k++) {
                      var propertyNode = propChildenNodes[k];

                      if (!propertyNode.hidden) {
                        if (propertyNode.properties)
                          var propertyNodeProf = propertyNode.properties;
                        else if (propertyNode.attributes)
                          var propertyNodeProf = propertyNode.attributes.properties;

                        var props = {};
                        props.columnName = propertyNodeProf.columnName;
                        props.propertyName = propertyNodeProf.propertyName;

                        props.dataType = 10;
                        props.dataLength = propertyNodeProf.dataLength;
                        if (propertyNodeProf.nullable == 'True')
                          props.isNullable = 'true';
                        else
                          props.isNullable = 'false';

                        if (props.columnName == keyName)
                          props.keyType = 1;
                        else
                          props.keyType = 0;

                        if (propertyNodeProf.showOnIndex == 'True')
                          props.showOnIndex = 'true';
                        else
                          props.showOnIndex = 'false';

                        props.numberOfDecimals = propertyNodeProf.numberOfDecimals;
                        folder.dataProperties.push(props);
                      }
                    }
                    break;
                  case 'Relationships':
                    if (!relationFolderNode)
                      break;

                    if (relationFolderNode.childNodes)
                      var relChildenNodes = relationFolderNode.childNodes;
                    else
                      var relChildenNodes = relationFolderNode.children;

                    if (relChildenNodes)
                      for (var k = 0; k < relChildenNodes.length; k++) {
                        var relationNode = relChildenNodes[k];
                        var found = false;
                        for (var ik = 0; ik < folder.dataRelationships.length; ik++)
                          if (relationNode.text.toLowerCase() == folder.dataRelationships[ik].relationshipName.toLowerCase()) {
                            found = true;
                            break;
                          }

                        if (found || relationNode.text == '')
                          continue;


                        if (relationNode.attributes) {
                          if (relationNode.attributes.attributes) {
                            if (relationNode.attributes.attributes.propertyMap)
                              var relationNodeAttr = relationNode.attributes.attributes;
                            else if (relationNode.attributes.propertyMap)
                              var relationNodeAttr = relationNode.attributes;
                            else
                              var relationNodeAttr = relationNode.attributes.attributes;
                          }
                          else {
                            var relationNodeAttr = relationNode.attributes;
                          }
                        }
                        else {
                          relationNodeAttr = relationNode;
                        }


                        var relation = {};
                        relation.propertyMaps = new Array();

                        for (var m = 0; m < relationNodeAttr.propertyMap.length; m++) {
                          var propertyPairNode = relationNodeAttr.propertyMap[m];
                          var propertyPair = {};

                          propertyPair.dataPropertyName = propertyPairNode.dataPropertyName;
                          propertyPair.relatedPropertyName = propertyPairNode.relatedPropertyName;
                          relation.propertyMaps.push(propertyPair);
                        }

                        relation.relatedObjectName = relationNodeAttr.relatedObjectName;
                        relation.relationshipName = relationNodeAttr.text;
                        relation.relationshipType = relationNodeAttr.relationshipTypeIndex;
                        folder.dataRelationships.push(relation);
                      }
                    break;
                }
              }
              treeProperty.dataObjects.push(folder);
            }


            Ext.Ajax.request({
              url: 'AdapterManager/Trees',
              method: 'POST',
              params: {
                scope: scopeName,
                app: appName,
                tree: JSON.stringify(treeProperty)
              },
              success: function (response, request) {
                var rtext = response.responseText;
                if (rtext.toUpperCase().indexOf('FALSE') == -1) {
                  showDialog(400, 100, 'Tree saving result', 'The tree is saved successfully.', Ext.Msg.OK, null);
                  var navpanel = Ext.getCmp('nav-panel');
                  navpanel.onReload();
                }
                else {
                  var ind = rtext.indexOf('}');
                  var len = rtext.length - ind - 1;
                  var msg = rtext.substring(ind + 1, rtext.length - 1);
                  showDialog(400, 100, 'Tree saving result', msg, Ext.Msg.OK, null);
                }
              },
              failure: function (response, request) {
                showDialog(660, 300, 'Tree saving result',

                    'Error happed when saving the tree', Ext.Msg.OK, null);
              }
            });
          }
        }]
      }),
      listeners: {
        click: function (node, e) {
          if (node.isRoot) {
            editPane = dataObjectsPane.items.items[1];
            if (!editPane) {
              var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
            }

            setTablesSelectorPane(editPane);
            return;
          }
          else if (!node)
            return;

          var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
          if (!editPane)
            editPane = dataObjectsPane.items.items[1];

          var nodeType = node.attributes.type;


          if (!nodeType && node.attributes.attributes)
            nodeType = node.attributes.attributes.type;


          if (nodeType) {
            editPane.show();
            var editPaneLayout = editPane.getLayout();

            switch (nodeType.toUpperCase()) {
              case 'DATAOBJECT':
                setDataObject(editPane, node);
                break;

              case 'KEYS':
                setKeysFolder(editPane, node);
                break;

              case 'KEYPROPERTY':
                setKeyProperty(editPane, node);
                break;

              case 'PROPERTIES':
                setPropertiesFolder(editPane, node);
                break;

              case 'DATAPROPERTY':
                setDataProperty(editPane, node);
                break;

              case 'RELATIONSHIPS':
                setRelations(editPane, node);
                break;

              case 'RELATIONSHIP':
                setRelationFields(editPane, node, scopeName, appName);
                break;
            }
          }
          else {
            editPane.hide();
          }
        }
      }
    }]
  }, {
    xtype: 'panel',
    name: 'editor-panel',
    border: 1,
    frame: false,
    id: scopeName + '.' + appName + '.editor-panel',
    region: 'center',
    layout: 'card'
  }]
});