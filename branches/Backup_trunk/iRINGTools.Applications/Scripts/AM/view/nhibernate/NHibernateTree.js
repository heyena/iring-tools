Ext.define('AM.view.nhibernate.NHibernateTree', {
  extend: 'Ext.tree.Panel',
  alias: 'widget.nhibernatetreepanel',
  bodyStyle: 'padding:0.5px 0px 1px 1px',
  autoScroll: true,
  animate: true,
  expandAll: true,
  lines: true,
  frame: false,
  enableDD: false,
  containerScroll: true,
  rootVisible: true,
  dbDict: null,
  dbInfo: null,
  dataTypes: null,
  contextName: null,
  endpoint: null,
  baseUrl: null, 
  width: 300,
  root: {
    expanded: true,
    type: 'DATAOBJECTS',
    text: 'Data Objects',
    iconCls: 'folder'
  },

  initComponent: function () {
    var me = this;
    Ext.apply(this, {
      store: Ext.create('Ext.data.TreeStore', {
        model: 'AM.model.NHibernateTreeModel',
        clearOnLoad: true,
        root: {
          expanded: true,
          type: 'DATAOBJECTS',
          text: 'Data Objects',
          iconCls: 'folder'
        },
        proxy: {
          type: 'ajax',
          timeout: 600000,
          url: 'NHibernate/DBObjects',          
          actionMethods: { read: 'POST' },
          reader: { type: 'json' }
        }
      })
    });

    var wizard = this;
    var contextName = this.contextName;
    var endpoint = this.endpoint;

    this.tbar = new Ext.Toolbar({
      items: [{
        xtype: 'tbspacer',
        width: 4
      }, {
        xtype: 'button',
        icon: 'Content/img/16x16/view-refresh.png',
        text: 'Reload',
        tooltip: 'Reload Data Objects',
        action: 'reloaddataobjects'
      }, {
        xtype: 'tbspacer',
        width: 4
      }, {
        xtype: 'button',
        icon: 'Content/img/16x16/document-properties.png',
        text: 'Edit Connection',
        tooltip: 'Edit database connection',
        action: 'editdbconnection'
      }, {
        xtype: 'tbspacer',
        width: 4
      }, {
        xtype: 'button',
        icon: 'Content/img/16x16/document-save.png',
        text: 'Save',
        tooltip: 'Save the data objects tree to the back-end server',
        action: 'savedbobjectstree'
      }]
    });

    this.callParent(arguments);
  },

  onReload: function () {
    this.getStore().load();
  }
});

function setTableNames(dbDict) {
  // populate selected tables			
  var selectTableNames = new Array();

  for (var i = 0; i < dbDict.dataObjects.length; i++) {
    var tableName = (dbDict.dataObjects[i].tableName ? dbDict.dataObjects[i].tableName : dbDict.dataObjects[i]);
    selectTableNames.push(tableName);
  }

  return selectTableNames;
};

function createMainContentPanel(content, contextName, endpoint, baseUrl, dirNode) {
  var objConf = {
    id: contextName + '.' + endpoint + '.-nh-config',
    title: 'NHibernate Configuration - ' + contextName + '.' + endpoint,    
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
    baseUrl: baseUrl,
    dirNode: dirNode,
    region: 'west',
    layout: 'fit',
    id: contextName + '.' + endpoint + '.-nh-tree'
  };

  var editconf = {   
    region: 'center',
    id: contextName + '.' + endpoint + '.-nh-editor'
  };

  var existNhpan = content.items.map[contextName + '.' + endpoint + '.-nh-config'];

  if (existNhpan != undefined) {
    var existNhtree = existNhpan.items.map[contextName + '.' + endpoint + '.-nh-tree'];
    var exitsEditpan = existNhpan.items.map[contextName + '.' + endpoint + '.-nh-editor'];
  }
  else {
    var nhpan = Ext.widget('dataobjectpanel', objConf);    
  }

  var editpan = Ext.widget('editorpanel', editconf);
  var nhtree = Ext.widget('nhibernatetreepanel', treeconf);

  if (existNhtree == undefined) {
    if (existNhpan == undefined)
      nhpan.items.add(nhtree);
    else
      existNhpan.items.add(nhtree);
  }

  if (exitsEditpan == undefined) {
    if (existNhpan == undefined)
      nhpan.items.add(editpan);
    else
      existNhpan.items.add(editpan);
  } 

  if (existNhpan == undefined) {
    content.add(nhpan).show();
  } else {
    existNhpan.show();
  }
};

function setTablesSelectorPane (me, editor, dataTree, nhpan, dbDict, dbInfo, contextName, endpoint, baseUrl) {    
  var content = me.getMainContent();
    
  if (!nhpan)
    nhpan = content.items.map[contextName + '.' + endpoint + '.-nh-config'];

  if (!editor)
    editor = nhpan.items.map[contextName + '.' + endpoint + '.-nh-editor'];

  if (!dataTree)
    dataTree = nhpan.items.map[contextName + '.' + endpoint + '.-nh-tree'];

  var conf = {
    contextName: contextName,
    endpoint: endpoint,
    baseUrl: baseUrl,
    dbInfo: dbInfo,
    dbDict: dbDict,
    dataTree: dataTree,
    region: 'center',
    id: contextName + '.' + endpoint + '.tablesselector'
  };

  var select = editor.items.map[conf.id];

  if (!select) {
    select = Ext.widget('selecttables', conf);
    editor.items.add(select);    
  }

  var panelIndex = editor.items.indexOf(select);
  if (panelIndex < 0) {
    select.destroy();
    select = Ext.widget('selecttables', conf);
    editor.items.add(select);
    panelIndex = editor.items.indexOf(select);
  }
  editor.getLayout().setActiveItem(panelIndex);
  editor.doLayout();
  var tablesSelector = select.items.items[1];
  var selectItems = setSelectTables(dataTree);

  if (tablesSelector.toField && select.selectItems.length > 0 && selectItems.length == select.selectItems.length) {
    var list = tablesSelector.toField.boundList;
    var store = list.getStore();

    if (!store.data.items[0] ) {
      store.removeAll();

      for (var i = 0; i < select.selectItems.length; i++) {
        store.insert(i + 1, 'field1');
        store.data.items[i].data.field1 = select.selectItems[i];
      }

      list.refresh();
    }    
  }

  var checkBox = select.items.items[2];
  checkBox.setValue(dbDict.enableSummary);
};

function setPropertiesFolder(me, editor, node, contextName, endpoint) {
  var conf = {
    contextName: contextName,
    endpoint: endpoint,    
    region: 'center',
    treeNode: node,
    id: contextName + '.' + endpoint + '.selectProperties'
  };

  var selectPane = editor.items.map[conf.id];

  if (selectPane)
    selectPane.destroy();
  
  var select = Ext.widget('selectProperties', conf);
  editor.items.add(select);
  editor.doLayout();
  var panelIndex = editor.items.indexOf(select);
  editor.getLayout().setActiveItem(panelIndex);  

  var propertiesSelector = select.items.items[1];

  if (propertiesSelector.toField && select.selectItems.length > 0) {
    var list = propertiesSelector.toField.boundList;
    var store = list.getStore();

    if (store.data) {
      store.removeAll();
    }

    for (var i = 0; i < select.selectItems.length; i++) {
      store.insert(i + 1, 'field1');
      store.data.items[i].data.field1 = select.selectItems[i];
    }

    list.refresh();
  }
};

function setKeysFolder (me, editor, node, contextName, endpoint) {
  var conf = {
    contextName: contextName,
    endpoint: endpoint,    
    region: 'center',
    treeNode: node,
    editor: editor,
    id: contextName + '.' + endpoint + '.' + node.id + '.selectKeys'
  };

  var select = editor.items.map[conf.id];

  if (select)
    select.destroy();
 
  select = Ext.widget('selectdatakeysform', conf);
  editor.items.add(select);
  var panelIndex = editor.items.indexOf(select);
  editor.getLayout().setActiveItem(panelIndex);
  editor.doLayout();
  var keysSelector = select.items.items[1];

  if (select.selectItems && keysSelector)
    if (keysSelector.toField)
      if (keysSelector.toField && select.selectItems.length > 0) {
        var list = keysSelector.toField.boundList;
        var store = list.getStore();

        if (store.data) {
          store.removeAll();
        }

        for (var i = 0; i < select.selectItems.length; i++) {
          store.insert(i + 1, 'field1');
          store.data.items[i].data.field1 = select.selectItems[i];
        }

        list.refresh();
      }
};

function setKeyProperty(me, editor, node, contextName, endpoint) {
  if (editor) {
    var conf = {
      contextName: contextName,
      region: 'center',
      endpoint: endpoint,        
      node: node,
      id: contextName + '.' + endpoint + '.' + node.id + '.setkeyproperty'
    };
    var setdop = editor.items.map[conf.id];
    if (!setdop) {
      setdop = Ext.widget('setdatakeyform', conf);
      editor.items.add(setdop);
    }
    setdop.setActiveRecord(node.data.property);
    var panelIndex = editor.items.indexOf(setdop);

    if (panelIndex < 0) {
      setdop.destroy();
      setdop = Ext.widget('setdatakeyform', conf);
      editor.items.add(setdop);
      panelIndex = editor.items.indexOf(setdop);
    }

    editor.getLayout().setActiveItem(panelIndex);
    editor.doLayout();
  }
};

function setDataProperty(me, editor, node, contextName, endpoint) {
  if (editor) {    
    var conf = {
      contextName: contextName,
      region: 'center',
      endpoint: endpoint,
      node: node,
      id: contextName + '.' + endpoint + '.' + node.id + '.setdataproperty'
    };
    var setdop = editor.items.map[conf.id];
    if (!setdop) {
      setdop = Ext.widget('setproperty', conf);
      editor.items.add(setdop);
      editor.doLayout();
    }
    setdop.setActiveRecord(node.data.property);
    var panelIndex = editor.items.indexOf(setdop);

    if (panelIndex < 0) {
      setdop.destroy();
      setdop = Ext.widget('setproperty', conf);
      editor.items.add(setdop);
      panelIndex = editor.items.indexOf(setdop);
    }

    editor.getLayout().setActiveItem(panelIndex);
    editor.doLayout();
  }
};

function setRelations(editor, tree, node, contextName, endpoint) {
  if (editor) {
    var conf = {
      contextName: contextName,
      endpoint: endpoint,
      rootNode: tree.getRootNode(),
      node: node,
      editor: editor,
      id: contextName + '.' + endpoint + '.' + node.id + '.createrelation'
    };

    var setdop = editor.items.map[conf.id];

    if (setdop) {
      setdop.destroy();
    } 
   
    setdop = Ext.widget('createrelations', conf);
    editor.items.add(setdop);
    var panelIndex = editor.items.indexOf(setdop);
    editor.getLayout().setActiveItem(panelIndex);
    editor.doLayout();    

    var gridRelationPane = setdop.items.items[2];
    var relations = new Array();
    var gridLabel = contextName + '.' + endpoint + '.relationsGrid' + node.id;
    var i = 0;    

    if (node.childNodes)
      for (i = 0; i < node.childNodes.length; i++) {
        if (node.childNodes[i].text != '')
          relations.push([node.childNodes[i].data.text]);
      }

    var rootNode = tree.getRootNode();
    createRelationGrid(editor, setdop, rootNode, node, gridLabel, gridRelationPane, relations, contextName + '.' + endpoint + '.-nh-config', contextName + '.' + endpoint + '.dataObjectsPane', contextName + '.' + endpoint + '.relationCreateForm.' + node.id, 0, contextName, endpoint, '');
  }
};

function setRelationFields(editor, rootNode, node, contextName, endpoint) {
  if (editor) {
    var relationFolderNode = node.parentNode;
    var dataObjectNode = relationFolderNode.parentNode;
    var relatedObjects = new Array();
    var thisObj = dataObjectNode.data.text;
    var ifExist;
    var relAttribute = null;
    var relateObjStr;
    var nodeRelateObj;
    var rindex = 0
    var setRelationFieldId = contextName + '.' + endpoint + '.' + node.id + '.createpropertymap';
    var relatedObjectName = '';
    var relatedTable = '';
    var relatedObjectField = null;
    var relatedTableField = null;
    var setdop;

    if (editor.items.map[setRelationFieldId]) {
      var relPane = editor.items.map[setRelationFieldId];
      relatedTableField = relPane.getForm().findField('relatedTable');
      relatedTable = relatedTableField.getValue();
      relatedObjectField = relPane.getForm().findField('relatedObjectName');
    }
    else if (node.data.relatedObjectName) {
      relatedObjectName = node.data.relatedObjectName;
    }
    else if (node.raw) {
      if (node.raw.relatedObjectName) {
        relatedObjectName = node.raw.relatedObjectName;
        relatedTable = node.raw.relatedTableName;
      }
    }

    if (relatedTable == '' && node.raw)
      relatedTable = node.raw.relatedTableName;

    for (var i = 0; i < rootNode.childNodes.length; i++) {
      relAttribute = rootNode.childNodes[i].data;
      relateObjStr = relAttribute.text;
      ifExist = false;

      if (relatedTable != '')
        if (relAttribute.property.tableName == relatedTable) {
          relatedObjectName = relateObjStr;          
        }
      
      for (var j = 0; j < relationFolderNode.childNodes.length; j++) {
        if (relationFolderNode.childNodes[j].data.text == '' || relationFolderNode.childNodes[j].data.id == node.data.id)
          continue;

        if (relationFolderNode.childNodes[j].data.property)
          relAttribute = relationFolderNode.childNodes[j].data.property;

        if (relAttribute) {
          nodeRelateObj = relAttribute.relatedObjectName;
          if (relateObjStr.toLowerCase() == nodeRelateObj.toLowerCase())
            ifExist = true;
        }
      }

      if (relateObjStr.toLowerCase() != thisObj.toLowerCase() && ifExist == false) {
        relatedObjects.push([rindex.toString(), rootNode.childNodes[i].data.text, rootNode.childNodes[i].data.property.tableName]);
        rindex++;
      }
    }

    var selectedProperties = new Array();
    var ii = 0;

    if (dataObjectNode) {
      if (dataObjectNode.childNodes[0]) {
        var keysNode = dataObjectNode.childNodes[0];
        for (var i = 0; i < keysNode.childNodes.length; i++) {
          selectedProperties.push([ii.toString(), keysNode.childNodes[i].data.text, keysNode.childNodes[i].data.property.columnName]);
          ii++;
        }
      }

      if (dataObjectNode.childNodes[1]) {
        var propertiesNode = dataObjectNode.childNodes[1];
        for (var i = 0; i < propertiesNode.childNodes.length; i++) {
          selectedProperties.push([ii.toString(), propertiesNode.childNodes[i].data.text, propertiesNode.childNodes[i].data.property.columnName]);
          ii++;
        }
      }
    }

    var mappingProperties = new Array();
    ii = 0;    

    if (relatedObjectName != '') {
      var relatedDataObjectNode = rootNode.findChild('text', relatedObjectName);

      if (relatedDataObjectNode) {
        if (relatedDataObjectNode.childNodes[0]) {
          keysNode = relatedDataObjectNode.childNodes[0];
          for (var i = 0; i < keysNode.childNodes.length; i++) {
            mappingProperties.push([ii.toString(), keysNode.childNodes[i].data.text, keysNode.childNodes[i].data.property.columnName]);
            ii++;
          }
        }

        if (relatedDataObjectNode.childNodes[1]) {
          propertiesNode = relatedDataObjectNode.childNodes[1];
          for (var i = 0; i < propertiesNode.childNodes.length; i++) {
            mappingProperties.push([ii.toString(), propertiesNode.childNodes[i].data.text, propertiesNode.childNodes[i].data.property.columnName]);
            ii++;
          }
        }
      }
    }
    else {
      mappingProperties.push(['0', '']);
    }

    var setdopForm;
    var createPanel = false;

    if (editor.items.items) {
      if (editor.items.items.length > 0) {
        if (editor.items.map[setRelationFieldId]) {
          setdop = editor.items.map[setRelationFieldId];
        }
        else {
          createPanel = true;
        }
      }
      else {
        createPanel = true;
      }
    }
    else
      createPanel = true;
        
    if (createPanel) {
      var conf = {
        contextName: contextName,
        endpoint: endpoint,
        rootNode: rootNode,
        relatedObjects: relatedObjects,
        selectedProperties: selectedProperties,
        mappingProperties: mappingProperties,
        node: node,
        id: setRelationFieldId
      };

      setdop = Ext.widget('setrelationform', conf);
      editor.items.add(setdop);
    }

    var panelIndex = editor.items.indexOf(setdop);

    if (panelIndex < 0) {
      setdop.destroy();
      setdop = Ext.widget('setrelationform', conf);
      editor.items.add(setdop);
      panelIndex = editor.items.indexOf(setdop);
    }

    editor.getLayout().setActiveItem(panelIndex);
    editor.doLayout();
    setdopForm = setdop.getForm();          
    var objText = setdopForm.findField('objectName');
    objText.setValue(dataObjectNode.data.property.objectName);

    if (!relatedObjectField)
      relatedObjectField = setdopForm.findField('relatedObjectName');

    if (relatedObjectField.store) {
      if (relatedObjectField.store.data) {
        relatedObjectField.store.removeAll();
      }
      relatedObjectField.store.loadData(relatedObjects);
      relatedObjectField.setValue(relatedObjectName);
    }

    var mapCombo = setdopForm.findField('mapPropertyName');

    if (mapCombo.store) {
      if (mapCombo.store.data) {
        mapCombo.store.removeAll();
      }
      mapCombo.store.loadData(mappingProperties);
    }

    var propCombo = setdopForm.findField('propertyName');
    if (propCombo.store) {
      if (propCombo.store.data) {
        propCombo.store.removeAll();
      }
      propCombo.store.loadData(selectedProperties);
    }

    if (!relatedObjectField)
      relatedObjectField = setdopForm.findField('relatedObjectName');

    if (!relatedTableField)
      relatedTableField = setdopForm.findField('relatedTable');
    
    if (relatedObjectField.getValue() == '') {
      relatedObjectField.clearInvalid();

      if (relatedObjectName != '') 
        relatedObjectField.setValue(relatedObjectName);

      if (relatedTableField.getValue() == '')
        relatedTableField.setValue(relatedTable);
    }

    var propertyMapPane = setdop.items.items[7];
    var relationTypeIndex;
    var relationType = '';

    if (node.data.relationshipTypeIndex) {
      relationTypeIndex = node.data.relationshipTypeIndex;     
    }
    else if (node.raw.relationshipTypeIndex) {
      relationTypeIndex = node.raw.relationshipTypeIndex;     
    }

    if (node.data.relationshipType) {
      relationType = node.data.relationshipType;
    }
    else if (node.raw.relationshipType) {
      relationType = node.raw.relationshipType;
    }

    if (relationType != '')
      setdopForm.findField('relationType').setValue(relationType);
    var propertyMaps;

    if (node.data.propertyMap)
      propertyMaps = node.data.propertyMap;
    else if (node.raw) {
      if (node.raw.propertyMap)
        propertyMaps = node.raw.propertyMap;
      else
        propertyMaps = [];
    }
    else
      propertyMaps = [];

    var gridLabel = contextName + '.' + endpoint + '.propertyMapGrid' + node.id;
    var myArray = new Array();
    var i = 0;
    var relatedMapItem = findNodeRelatedObjMap(node, relatedObjectName);
    var relPropertyName;
    var relMapPropertyName;
    var columnName;
    var relatedColumnName;

    createPropertyMapGrid(setdop, rootNode, node, contextName + '.' + endpoint + '.' + node.id, propertyMapPane, myArray, contextName + '.' + endpoint + '.-nh-config', contextName + '.' + endpoint + '.dataObjectsPane', contextName + '.' + endpoint + '.relationConfigForm.' + node.id, contextName, endpoint, relatedObjectName);
    var newPropertyMapRecord;

    if (propertyMaps.length > 0) {
      if (!propertyMapPane)
        propertyMapPane = setdop.items.items[7];
      var gridPane = propertyMapPane.items.items[0];

      for (i = 0; i < propertyMaps.length; i++) {
        columnName = propertyMaps[i].dataColumnName;
        relatedColumnName = propertyMaps[i].relatedColumnName;
        relPropertyName = getPropertyName(columnName, selectedProperties);
        relMapPropertyName = getPropertyName(relatedColumnName, mappingProperties);        

        newPropertyMapRecord = new AM.model.PropertyMapModel({
          property: relPropertyName,
          columnName: columnName,
          relatedProperty: relMapPropertyName,
          relatedColumnName: relatedColumnName
        });

        gridPane.store.add(newPropertyMapRecord);
        relatedMapItem.push([relPropertyName, columnName, relMapPropertyName, relatedColumnName]);
      }
      propertyMapPane.doLayout();
    }
  }
};

function getPropertyName(columnName, pairArray) {
  for (var i = 0; i < pairArray.length; i++) {
    if (pairArray[i][2] == columnName)
      return pairArray[i][1];
  } 
};

function setDataObject (me, editor, node, contextName, endpoint, tree, dbDict) {
  if (editor) {
    var conf = {
      contextName: contextName,
      node: node,
      region: 'center',
      endpoint: endpoint,
      tree: tree,
      dbDict: dbDict,
      editor: editor,
      id: contextName + '.' + endpoint + '.' + node.id + '.setdataobject'
    };
    var setdop = editor.items.map[conf.id];
    if (!setdop) {
      setdop = Ext.widget('setdataobjectpanel', conf);
      editor.items.add(setdop);      
    }

    var panelIndex = editor.items.indexOf(setdop);

    if (panelIndex < 0) {
      setdop.destroy();
      setdop = Ext.widget('setdataobjectpanel', conf);
      editor.items.add(setdop);
      panelIndex = editor.items.indexOf(setdop);
    }

    editor.getLayout().setActiveItem(panelIndex);
    editor.doLayout();
  }
};

function getTreeJson(dsConfigPane, rootNode, dbInfo, dbDict, dataTypes, tablesSelectorPane) {
  var treeProperty = {};
  treeProperty.dataObjects = new Array();
  treeProperty.IdentityConfiguration = null;

  var tProp = setTreeProperty(dsConfigPane, dbInfo, dbDict, tablesSelectorPane);
  treeProperty.connectionString = tProp.connectionString;
  if (treeProperty.connectionString != null && treeProperty.connectionString.length > 0) {
    var base64 = AM.view.nhibernate.Utility;
    treeProperty.connectionString = base64.encode(tProp.connectionString);
  }
  treeProperty.schemaName = tProp.schemaName;
  treeProperty.provider = tProp.provider;
  treeProperty.enableSummary = tProp.enableSummary;

  var keyName;
  for (var i = 0; i < rootNode.childNodes.length; i++) {
    var folder = getFolderFromChildNode(rootNode.childNodes[i], dataTypes);
    treeProperty.dataObjects.push(folder);
  }

  dbDict.ConnectionString = treeProperty.connectionString;
  dbDict.SchemaName = treeProperty.schemaName;
  dbDict.Provider = treeProperty.provider;
  dbDict.dataObjects = treeProperty.dataObjects;
  dbDict.enableSummary = treeProperty.enableSummary;
  return treeProperty;
};

function setTreeProperty(dsConfigPane, dbInfo, dbDict, tablesSelectorPane) {
  var treeProperty = {};
  if (tablesSelectorPane)
    treeProperty.enableSummary = tablesSelectorPane.getForm().findField('enableSummary').getValue();
  else if (dbDict.enableSummary)
    treeProperty.enableSummary = dbDict.enableSummary;
  else
    treeProperty.enableSummary = false;

  if (dsConfigPane) {
    var dsConfigForm = dsConfigPane.getForm();
    treeProperty.provider = dsConfigForm.findField('dbProvider').getValue();
    var dbServer = dsConfigForm.findField('dbServer').getValue();
    dbServer = (dbServer.toLowerCase() == 'localhost' ? '.' : dbServer);
    var upProvider = treeProperty.provider.toUpperCase();
    var serviceNamePane = dsConfigPane.items.items[10];
    var serviceName = '';
    var serName = '';
    if (serviceNamePane.items.items[0]) {
      serviceName = serviceNamePane.items.items[0].value;
      serName = serviceNamePane.items.items[0].serName;
    }
    else if (dbInfo) {
      if (dbInfo.dbInstance)
        serviceName = dbInfo.dbInstance;
      if (dbInfo.serName)
        serName = dbInfo.serName;
    }

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
      var dataSrc = 'Data Source=' + '(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=' + dbServer + ')(PORT=' + dsConfigForm.findField('portNumber').getValue() + ')))(CONNECT_DATA=(SERVER=DEDICATED)(' + serName + '=' + serviceName + ')))';
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
    dbServer = (dbServer.toLowerCase() == 'localhost' ? '.' : dbServer);

    if (upProvider.indexOf('MSSQL') > -1) {
      if (dbInfo.dbInstance) {
        if (dbInfo.dbInstance.toUpperCase() == "DEFAULT") {
          var dataSrc = 'Data Source=' + dbServer + ';Initial Catalog=' + dbInfo.dbName;
        } else {
          var dataSrc = 'Data Source='
					            + dbServer + '\\' + dbInfo.dbInstance
											+ ';Initial Catalog=' + dbInfo.dbName;
        }
      }
    }
    else if (upProvider.indexOf('ORACLE') > -1)
      var dataSrc = 'Data Source=' + '(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=' + dbServer + ')(PORT=' + dbInfo.portNumber + ')))(CONNECT_DATA=(SERVER=DEDICATED)(' + dbInfo.serName + '=' + dbInfo.dbInstance + ')))';
    else if (upProvider.indexOf('MYSQL') > -1)
      var dataSrc = 'Data Source=' + dbServer;

    treeProperty.connectionString = dataSrc
                                        + ';User ID=' + dbInfo.dbUserName
                                        + ';Password=' + dbInfo.dbPassword;
    treeProperty.schemaName = dbDict.SchemaName;
  }
  return treeProperty;
};

function getDataTypeIndex(datatype, dataTypes) {
  if (dataTypes == undefined)
    return;

  var i = 0;

  while (dataTypes[i] == undefined)
    i++;

  for (var k = i; k < dataTypes.length; k++) {
    if (dataTypes[k][1] == datatype)
      return dataTypes[k][0];
  }
};

function getFolderFromChildNode(folderNode, dataTypes) {
  var folderNodeProp = folderNode.data.property;
  var folder = {};
  var keyName = '';

  folder.tableName = folderNodeProp.tableName;
  folder.objectNamespace = folderNodeProp.objectNamespace;
  folder.objectName = folderNodeProp.objectName;
  folder.description = folderNodeProp.description;

  if (folderNodeProp.keyDelimiter && folderNodeProp.keyDelimiter != 'null')
    folder.keyDelimeter = folderNodeProp.keyDelimiter;    
  else
    folder.keyDelimeter = '_';

  folder.keyProperties = new Array();
  folder.dataProperties = new Array();
  folder.dataRelationships = new Array();

  for (var j = 0; j < folderNode.childNodes.length; j++) {
    if (folderNode.childNodes[1])
      var propertyFolderNode = folderNode.childNodes[1];    

    if (folderNode.childNodes[0])
      var keyFolderNode = folderNode.childNodes[0];   

    if (folderNode.childNodes[2])
      var relationFolderNode = folderNode.childNodes[2];   

    if (folderNode.childNodes[j])
      subFolderNodeText = folderNode.childNodes[j].data.text;

    switch (subFolderNodeText) {
      case 'Keys':
        if (keyFolderNode)
          var keyChildenNodes = keyFolderNode.childNodes;       

        for (var k = 0; k < keyChildenNodes.length; k++) {
          var keyNode = keyChildenNodes[k];          
          var keyProps = {};

          if (keyNode.data.property)
            var keyNodeProf = keyNode.data.property;         

          keyProps.keyPropertyName = keyNode.data.text;
          keyName = keyNode.data.text;
          folder.keyProperties.push(keyProps);
          var tagProps = {};
          tagProps.columnName = keyNodeProf.columnName;
          tagProps.propertyName = keyNode.data.text;
          
          if (typeof keyNodeProf.dataType == 'string')
            tagProps.dataType = getDataTypeIndex(keyNodeProf.dataType, dataTypes);
          else
            tagProps.dataType = keyNodeProf.dataType;

          tagProps.dataLength = keyNodeProf.dataLength;

          if (keyNodeProf.nullable)
            tagProps.isNullable = keyNodeProf.nullable.toString().toLowerCase();
          else
            tagProps.isNullable = 'false';

          tagProps.isHidden = 'false';

          if (!keyNodeProf.keyType)
            tagProps.keyType = 1;
          else
            if (typeof keyNodeProf.keyType != 'string')
              tagProps.keyType = keyNodeProf.keyType;
            else {
              switch (keyNodeProf.keyType.toLowerCase()) {
                case 'assigned':
                  tagProps.keyType = 1;
                  break;
                case 'unassigned':
                  tagProps.keyType = 0;
                  break;
                default:
                  tagProps.keyType = 1;
                  break;
              }
            }

          if (keyNodeProf.showOnIndex)
            tagProps.showOnIndex = keyNodeProf.showOnIndex.toString().toLowerCase();
          else
            tagProps.showOnIndex = 'false';

          tagProps.numberOfDecimals = keyNodeProf.numberOfDecimals;
          folder.dataProperties.push(tagProps);    
        }
        break;
      case 'Properties':
        if (folderNode.childNodes[1]) {
          var propChildenNodes = propertyFolderNode.childNodes;
          folder = prepareProperties(folder, propChildenNodes, 'false', dataTypes, keyName);
        }
//        if (propertyFolderNode.raw.hiddenNodes.hiddenNode.children) {
//          var hiddenNodes = propertyFolderNode.raw.hiddenNodes.hiddenNode.children;
//          folder = prepareProperties(folder, hiddenNodes, 'true', dataTypes, keyName);
//        }
        break;
      case 'Relationships':
        if (!relationFolderNode)
          break;

        if (relationFolderNode.childNodes)
          var relChildenNodes = relationFolderNode.childNodes;       

        if (relChildenNodes)
          for (var k = 0; k < relChildenNodes.length; k++) {
            var relationNode = relChildenNodes[k];
            var found = false;
            for (var ik = 0; ik < folder.dataRelationships.length; ik++)
              if (relationNode.text.toLowerCase() == folder.dataRelationships[ik].relationshipName.toLowerCase()) {
                found = true;
                break;
              }

            if (found || relationNode.data.text == '')
              continue;

            relationNodeAttr = relationNode.data;
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
  return folder;
};

function prepareProperties(folder, propChildenNodes, ifHidden, dataTypes, keyName) {
  var hasData = false;

  for (var k = 0; k < propChildenNodes.length; k++) {
    var propertyNode = propChildenNodes[k];

    if (propertyNode.data != undefined) {
      if (propertyNode.data.property != undefined) {
        var propertyNodeProf = propertyNode.data.property;
        hasData = true;
      }
    }
    
    if (!hasData)
      var propertyNodeProf = propertyNode.property;

    var props = {};
    props.columnName = propertyNodeProf.columnName;
    props.propertyName = propertyNodeProf.propertyName;

    if (typeof propertyNodeProf.dataType.toLowerCase() == 'string')
      props.dataType = getDataTypeIndex(propertyNodeProf.dataType, dataTypes);
    else
      props.dataType = propertyNodeProf.dataType;

    props.dataLength = propertyNodeProf.dataLength;

    if (propertyNodeProf.nullable)
      props.isNullable = propertyNodeProf.nullable.toString().toLowerCase();
    else
      props.isNullable = 'false';

    if (keyName != '') {
      if (props.columnName == keyName)
        props.keyType = 1;
      else
        props.keyType = 0;
    }
    else
      props.keyType = 0;

    if (propertyNodeProf.showOnIndex)
      props.showOnIndex = propertyNodeProf.showOnIndex.toString().toLowerCase();
    else
      props.showOnIndex = 'false';

    props.isHidden = ifHidden;
    props.numberOfDecimals = propertyNodeProf.numberOfDecimals;
    folder.dataProperties.push(props);
  }
  return folder;
};