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
    var scopeName = this.contextName;
    var appName = this.endpoint;

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
        formBind: true,
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

function createMainContentPanel(content, contextName, endpoint, baseUrl) {
  var objConf = {
    id: contextName + '.' + endpoint + '.-nh-config',
    title: 'NHibernate Configuration - ' + contextName + '.' + endpoint,
    contextName: contextName,
    endpoint: endpoint,
    baseUrl: baseUrl,
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
    region: 'west',
    layout: 'fit',
    id: contextName + '.' + endpoint + '.-nh-tree'
  };

  var editconf = {
    contextName: contextName,
    endpoint: endpoint,
    baseUrl: baseUrl,
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
    dataTree: dataTree,
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
  var tablesSelector = select.items.items[1];

  if (tablesSelector.toField && select.selectItems.length > 0) {
    var list = tablesSelector.toField.boundList;
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
    id: contextName + '.' + endpoint + '.' + node.id + '.selectKeys'
  };

  var select = editor.items.map[conf.id];

  if (!select) {
    select = Ext.widget('selectdatakeysform', conf);
    editor.items.add(select);
    editor.doLayout();
  }

  var panelIndex = editor.items.indexOf(select);
  editor.getLayout().setActiveItem(panelIndex);  
  var keysSelector = select.items.items[1];

  if (select.selectItems)
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
      editor.doLayout();
    }
    setdop.setActiveRecord(node.data.property);
    var panelIndex = editor.items.indexOf(setdop);
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

    for (var i = 0; i < rootNode.childNodes.length; i++) {
      relateObjStr = rootNode.childNodes[i].data.text;
      ifExist = false;
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
        relatedObjects.push([rindex.toString(), rootNode.childNodes[i].data.text]);
        rindex++;
      }
    }

    var selectedProperties = new Array();
    var ii = 0;

    if (dataObjectNode) {
      if (dataObjectNode.childNodes[0]) {
        var keysNode = dataObjectNode.childNodes[0];
        for (var i = 0; i < keysNode.childNodes.length; i++) {
          selectedProperties.push([ii.toString(), keysNode.childNodes[i].data.text]);
          ii++;
        }
      }

      if (dataObjectNode.childNodes[1]) {
        var propertiesNode = dataObjectNode.childNodes[1];
        for (var i = 0; i < propertiesNode.childNodes.length; i++) {
          selectedProperties.push([ii.toString(), propertiesNode.childNodes[i].data.text]);
          ii++;
        }
      }
    }

    var mappingProperties = new Array();
    ii = 0;
    var relatedObjectName = '';

    if (editor.items.map[setRelationFieldId]) {
      var relPane = editor.items.map[setRelationFieldId];
      var relatedObjectField = relPane.getForm().findField('relatedObjectName');
      if (relatedObjectField.rawValue)        
        relatedObjectName = relatedObjectField.rawValue;    
    }
    else {
      if (node.data.property.relatedObjectName)
        relatedObjectName = nodeAttribute.relatedObjectName;
    }

    if (relatedObjectName != '') {
      var relatedDataObjectNode = rootNode.findChild('text', relatedObjectName);
      if (relatedDataObjectNode) {
        if (relatedDataObjectNode.childNodes[0]) {
          keysNode = relatedDataObjectNode.childNodes[0];
          for (var i = 0; i < keysNode.childNodes.length; i++) {
            mappingProperties.push([ii.toString(), keysNode.childNodes[i].data.text]);
            ii++;
          }
        }

        if (relatedDataObjectNode.childNodes[1]) {
          propertiesNode = relatedDataObjectNode.childNodes[1];
          for (var i = 0; i < propertiesNode.childNodes.length; i++) {
            mappingProperties.push([ii.toString(), propertiesNode.childNodes[i].data.text]);
            ii++;
          }
        }
      }
    }
    else {
      mappingProperties.push(['0', '']);
    }

    var setdopForm;

    if (editor.items.map[setRelationFieldId]) {
      var setdop = editor.items.map[setRelationFieldId];
      setdopForm = setdop.getForm()
      var panelIndex = editor.items.indexOf(setdop);
      editor.getLayout().setActiveItem(panelIndex);
      var objText = setdopForm.findField('objectName');
      objText.setValue(dataObjectNode.data.property.objectName);
      var relCombo = setdopForm.findField('relatedObjectName');

      if (relCombo.store.data) {
        relCombo.store.removeAll();
      }

      relCombo.store.loadData(relatedObjects);
      relCombo.setValue(relatedObjectName);
      setdopForm.findField('relatedTable').setValue(relatedObjectName);
      var mapCombo = setdopForm.findField('mapPropertyName');

      if (mapCombo.store.data) {
        mapCombo.store.removeAll();
      }

      mapCombo.store.loadData(mappingProperties);
      var propCombo = setdopForm.findField('propertyName');

      if (propCombo.store.data) {
        propCombo.store.removeAll();
      }

      propCombo.store.loadData(selectedProperties);
      return;
    }

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
    var panelIndex = editor.items.indexOf(setdop);
    editor.getLayout().setActiveItem(panelIndex);
    editor.doLayout();
    setdopForm = setdop.getForm();
    var relatedObjNameField = setdopForm.findField('relatedObjectName');

    if (relatedObjNameField.getValue() == '')
      relatedObjNameField.clearInvalid();

    var propertyMapPane = setdop.items.items[7];
    var relations = new Array();    

    if (relatedObjectName != '') {
      setdopForm.findField('relatedObjectName').setValue(relatedObjectName);
      setdopForm.findField('relatedTable').setValue(relatedObjectName);
    }

    if (node.data.relationshipTypeIndex)
      var relationTypeIndex = node.data.relationshipTypeIndex;

    setdopForm.findField('relationType').setValue(relationTypeIndex);
    var propertyMaps;

    if (node.data.propertyMap)
      propertyMaps = node.data.propertyMap;
    else
      propertyMaps = [];

    var gridLabel = contextName + '.' + endpoint + '.propertyMapGrid' + node.id;
    var myArray = new Array();
    var i = 0;
    var relatedMapItem = findNodeRelatedObjMap(node, relatedObjectName);
    var relPropertyName;
    var relMapPropertyName;

    for (i = 0; i < propertyMaps.length; i++) {
      relPropertyName = propertyMaps[i].dataPropertyName.toUpperCase();
      relMapPropertyName = propertyMaps[i].relatedPropertyName.toUpperCase();
      myArray.push([relPropertyName, relMapPropertyName]);
      relatedMapItem.push([relPropertyName, relMapPropertyName]);
    }

    createPropertyMapGrid(setdop, rootNode, node, contextName + '.' + endpoint + '.' + node.id, propertyMapPane, myArray, contextName + '.' + endpoint + '.-nh-config', contextName + '.' + endpoint + '.dataObjectsPane', contextName + '.' + endpoint + '.relationConfigForm.' + node.id, contextName, endpoint, relatedObjectName);
  }
};

function setDataObject (me, editor, node, contextName, endpoint) {
  if (editor) {
    var conf = {
      contextName: contextName,
      region: 'center',
      endpoint: endpoint,
      id: contextName + '.' + endpoint + '.' + node.id + '.setdataobject'
    };
    var setdop = editor.items.map[conf.id];
    if (!setdop) {
      setdop = Ext.widget('setdataobjectpanel', conf);
      editor.items.add(setdop);      
    }
    setdop.setActiveRecord(node.data.property);
    var panelIndex = editor.items.indexOf(setdop);
    editor.getLayout().setActiveItem(panelIndex);
    editor.doLayout();
  }
};
