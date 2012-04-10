
Ext.define('AM.view.nhibernate.NHibernateTree', {
  extend: 'Ext.tree.Panel',
  alias: 'widget.nhibernatetreepanel',
  bodyStyle: 'padding:0.5px 1px 1px 1px',
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
 // store: null,
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
        action: 'reloaddataobjects'//,
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
}
