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
  autoScroll: true,
  bodyStyle: 'background:#eee;padding:10px 0px 0px 10px',
  labelWidth: 140,
  monitorValid: true,
  selectItems: null,

  initComponent: function () {
    var me = this;
    var contextName = me.contextName;
    var endpoint = me.endpoint;
    var baseUrl = me.baseUrl;
    var dataTree = this.dataTree;
    var dbInfo = this.dbInfo;
    var dbDict = this.dbDict;

    var availItems = setAvailTables(dataTree, dbInfo.dbTableNames);
    this.selectItems = setSelectTables(dataTree);
    var selectItems = this.selectItems;

    this.items = [{
      xtype: 'label',
      text: 'Select Tables',
      cls: 'x-form-item',
      style: 'font-weight:bold;'
    }, {
      xtype: 'itemselector',
      name: 'tableSelector',
      anchor: '100%',
      hideLabel: true,
      listAvailable: 'Available Tables',
      listSelected: 'Selected Tables',
      height: 370,
      bodyStyle: 'background:#eee',
      frame: true,
      imagePath: 'Scripts/extjs-4.1.0/examples/ux/css/images',
      displayField: 'tableName',
      store: availItems,
      valueField: 'tableValue',
      border: 0,
      value: selectItems,
      listeners: {
        change: function (itemSelector, selectedValuesStr) {
          var selectTables = itemSelector.toField.store.data.items;
          for (var i = 0; i < selectTables.length; i++) {
            var selectTableName = selectTables[i].data.text;
            if (selectTableName == '')
              itemSelector.toField.store.removeAt(i);
          }

          var availTables = itemSelector.fromField.store.data.items;
          for (var i = 0; i < availTables.length; i++) {
            var availTableName = availTables[i].data.text
            if (availTables[i].data.text == '')
              itemSelector.fromField.store.removeAt(i);
          }
        }
      }
    }, {
      xtype: 'checkbox',
      name: 'enableSummary',
      fieldLabel: 'Enable Summary'
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
        handler: function () {
          var rootNode = dataTree.getRootNode();
          var selectTableNamesSingle = new Array();
          var availTableName = new Array();
          var found = false;
          var repeatItem;
          for (var i = 0; i < dbInfo.dbTableNames.items.length; i++) {
            repeatItem = dbInfo.dbTableNames.items[i];
            availTableName.push([repeatItem, repeatItem]);
          }

          for (var j = 0; j < availTableName.length; j++)
            for (var i = 0; i < rootNode.childNodes.length; i++) {
              if (rootNode.childNodes[i].data.property.tableName.toLowerCase() == availTableName[j][0].toLowerCase()) {
                found = true;
                availTableName.splice(j, 1);
                j--;
                break;
              }
            }

          for (var i = 0; i < rootNode.childNodes.length; i++) {
            var nodeText = rootNode.childNodes[i].data.property.tableName;
            selectTableNamesSingle.push(nodeText);
          }

          var tablesSelector = me.items.items[1];
          var list = tablesSelector.toField.boundList;
          var store = list.getStore();

          if (store.data) {
            store.removeAll();
          }

          for (var i = 0; i < selectTableNamesSingle.length; i++) {
            store.insert(i + 1, 'field1');
            store.data.items[i].data.field1 = selectTableNamesSingle[i];
          }

          list.refresh();

          if (tablesSelector.fromField.store.data) {
            tablesSelector.fromField.store.removeAll();
          }

          tablesSelector.fromField.store.loadData(availTableName);

          if (dbDict)
            me.getForm().findField('enableSummary').setValue(dbDict.enableSummary);
        }

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
          if (rootNode.childNodes[i].data.property.tableName.toLowerCase() == availTableName[j].toLowerCase()) {
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
      var nodeText = rootNode.childNodes[i].data.property.tableName;
      selectTableNames.push(nodeText);
    }
  }

  return selectTableNames;
};
