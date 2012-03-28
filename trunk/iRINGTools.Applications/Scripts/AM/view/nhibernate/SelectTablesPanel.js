Ext.Loader.setConfig({ enabled: true });
Ext.Loader.setPath('Ext.ux', 'Scripts/extjs407/examples/ux');
Ext.require([
    'Ext.form.Panel',
    'Ext.ux.form.MultiSelect',
    'Ext.ux.form.ItemSelector'
]);

Ext.define('AM.view.nhibernate.SelectTablesPanel', {
  extend: 'Ext.form.Panel',
  alias: 'widget.selecttables',
  frame: false,
  border: false,
  dataTree: null,
  dbInfo: null,
  contextName: null,
  endpoint: null,
  baseUrl: null,
  selected: {},
  available: {},
  autoScroll: true,
  bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
  labelWidth: 140,
  monitorValid: true,

  initComponent: function () {
    var me = this;
    var contextName = me.contextName;
    var endpoint = me.endpoint;
    var baseUrl = me.baseUrl;
    var dataTree = this.dataTree;
    var dbInfo = this.dbInfo;
    var dbDict = this.dbDict;

    var availItems = setAvailTables(dataTree, dbInfo.dbTableNames);
    var selectItems = setSelectTables(dataTree);

    this.items = [{
      xtype: 'label',
      fieldLabel: 'Select Tables',
      labelSeparator: '',
      itemCls: 'form-title'
    }, {
      xtype: 'itemselector',
      name: 'tableSelector',
      anchor: '100%',
      hideLabel: true,
      bodyStyle: 'background:#eee',
      frame: true,
      imagePath: 'Scripts/extjs407/examples/ux/css/images',      
      displayField: 'tableName',
      store: availItems,
      valueField: 'tableValue',
      border: 0,
      value: selectItems,
      listeners: {
        change: function (itemSelector, selectedValuesStr) {
          var selectTables = itemSelector.toMultiselect.store.data.items;
          for (var i = 0; i < selectTables.length; i++) {
            var selectTableName = selectTables[i].data.text;
            if (selectTableName == '')
              itemSelector.toMultiselect.store.removeAt(i);
          }

          var availTables = itemSelector.fromMultiselect.store.data.items;
          for (var i = 0; i < availTables.length; i++) {
            var availTableName = availTables[i].data.text
            if (availTables[i].data.text == '')
              itemSelector.fromMultiselect.store.removeAt(i);
          }
        }
      }
    }];

    this.tbar = new Ext.Toolbar({
      items: [{
        xtype: 'tbspacer',
        width: 4
      }, {
        xtype: 'button',
        icon: 'Content/img/16x16/apply.png',
        text: 'Apply',
        tooltip: 'Apply the current changes to the data objects tree',
        action: 'applydatatables'

      }, {
        xtype: 'tbspacer',
        width: 4
      }, {
        xtype: 'button',
        icon: 'Content/img/16x16/edit-clear.png',
        text: 'Reset',
        tooltip: 'Reset to the latest applied changes',
        action: 'resettables'

      }]
    });

    this.callParent(arguments);
  }
});

function setAvailTables(dbObjectsTree, dbTableNames) {
  var availTableName = new Array();

  if (dbObjectsTree.disabled) {
    for (var i = 0; i < dbTableNames.items.length; i++) {
      var tableName = dbTableNames.items[i];
      availTableName.push(tableName);
    }
  }
  else {
    var rootNode = dbObjectsTree.getRootNode();
    if (dbTableNames.items) {
      for (var i = 0; i < dbTableNames.items.length; i++) {
        availTableName.push(dbTableNames.items[i]);
      }
    }

    if (!dbObjectsTree.disabled) {
      for (var j = 0; j < availTableName.length; j++)
        for (var i = 0; i < rootNode.childNodes.length; i++) {
          if (rootNode.childNodes[i].attributes.properties.tableName.toLowerCase() == availTableName[j].toLowerCase()) {
            found = true;
            availTableName.splice(j, 1);
            j--;
            break;
          }
        }
    }
  }
  return availTableName;
};

function setSelectTables(dbObjectsTree) {
  var selectTableNames = new Array();

  if (!dbObjectsTree.disabled) {
    var rootNode = dbObjectsTree.getRootNode();
    for (var i = 0; i < rootNode.childNodes.length; i++) {
      var nodeText = rootNode.childNodes[i].attributes.properties.tableName;
      selectTableNames.push([nodeText, nodeText]);
    }
  }

  return selectTableNames;
};
