Ext.Loader.setConfig({ enabled: true });
Ext.Loader.setPath('Ext.ux', 'Scripts/extjs407/examples/ux');
Ext.require([
    'Ext.form.Panel',
    'Ext.ux.form.MultiSelect',
    'Ext.ux.form.ItemSelector'
]);

Ext.define('AM.view.nhibernate.SelectKeysPanel', {
  extend: 'Ext.form.Panel',
  alias: 'widget.selectdatakeysform',
  frame: false,
  border: false,
  autoScroll: true,
  editor: null,
  bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
  labelWidth: 140,
  treeNode: null,
  contextName: null,
  endpoint: null,
  shownProperty: null,
  monitorValid: true,
  selectItems: null,

  initComponent: function () {
    var me = this;
    var node = me.treeNode;
    var availItems = getAvailItems(node);
    this.selectItems = getSelectItems(node);
    var selectItems = me.selectItems;

    this.items = [{
      xtype: 'label',
      text: 'Select Keys',
      cls: 'x-form-item',
      style: 'font-weight:bold;'
    }, {
      xtype: 'itemselector',
      name: 'keySelector',
      anchor: '100%',
      hideLabel: true,
      listAvailable: 'Available Keys',
      listSelected: 'Selected Keys',
      height: 370,
      bodyStyle: 'background:#eee',
      frame: true,
      imagePath: 'Scripts/extjs407/examples/ux/css/images',
      displayField: 'keyName',
      store: availItems,
      valueField: 'keyValue',
      border: 0,
      value: selectItems,
      listeners: {
        change: function (itemSelector) {
          var selectKeys = itemSelector.toField.store.data.items;
          var dataObjPanel = me.editor.items.map[me.contextName + '.' + me.endpoint + '.' + node.parentNode.id + '.setdataobject'];
          var keyDilimiter = '';
          var dataObjNode = node.parentNode;

          if (dataObjPanel)
            keyDilimiter = dataObjPanel.getForm().findField('keyDelimeter').getValue();
          else
            keyDilimiter = dataObjNode.data.property.keyDelimiter;

          if (selectKeys.length > 1 && (keyDilimiter == 'null' || !keyDilimiter || keyDilimiter == '')) {
            showDialog(400, 100, 'Warning', "Please enter a valid key delimiter before selecting multimple keys", Ext.Msg.OK, null);
            return;
          }

          for (var i = 0; i < selectKeys.length; i++) {
            var selectKeyName = selectKeys[i].data.text;
            if (selectKeyName == '')
              itemSelector.toField.store.removeAt(i);
          }

          var availKeys = itemSelector.fromField.store.data.items;
          for (var i = 0; i < availKeys.length; i++) {
            var availKeyName = availKeys[i].data.text
            if (availKeys[i].data.text == '')
              itemSelector.fromField.store.removeAt(i);
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
        handler: function (f) {
          var keySelector = me.items.items[1];
          if (me.getForm().findField('keySelector').getValue().indexOf('') == -1)
            var selectValues = me.getForm().findField('keySelector').getValue();
          var keysNode = me.treeNode;
          var propertiesNode = keysNode.parentNode.childNodes[1];
          var hiddenRootNode = propertiesNode.raw.hiddenNodes.hiddenNode;

          for (var i = 0; i < keysNode.childNodes.length; i++) {
            var found = false;

            for (var j = 0; j < selectValues.length; j++) {
              if (selectValues[j].toLowerCase() == keysNode.childNodes[i].data.text.toLowerCase()) {
                found = true;
                break;
              }
            }

            if (!found) {
              if (keysNode.childNodes[i].data.property)
                var properties = keysNode.childNodes[i].data.property;

              if (properties) {
                properties['isNullable'] = true;
                delete properties.keyType;

                propertiesNode.appendChild({
                  text: keysNode.childNodes[i].data.text,
                  type: "dataProperty",
                  leaf: true,
                  iconCls: 'treeProperty',
                  property: properties
                });

                keysNode.removeChild(keysNode.childNodes[i], true);
                i--;
              }
            }
          }

          var nodeChildren = new Array();
          for (var j = 0; j < keysNode.childNodes.length; j++)
            nodeChildren.push(keysNode.childNodes[j].data.text);

          for (var j = 0; j < selectValues.length; j++) {
            var found = false;
            for (var i = 0; i < nodeChildren.length; i++) {
              if (selectValues[j].toLowerCase() == nodeChildren[i].toLowerCase()) {
                found = true;
                break;
              }
            }

            if (!found) {
              var newKeyNode;

              for (var jj = 0; jj < propertiesNode.childNodes.length; jj++) {
                if (propertiesNode.childNodes[jj].data.text.toLowerCase() == selectValues[j].toLowerCase()) {
                  var properties = propertiesNode.childNodes[jj].data.property;
                  properties.keyType = 'assigned';
                  properties.nullable = false;

                  keysNode.appendChild({
                    text: selectValues[j],
                    type: "keyProperty",
                    leaf: true,
                    iconCls: 'treeKey',
                    hidden: false,
                    property: properties
                  });

                  propertiesNode.removeChild(propertiesNode.childNodes[jj], true);
                  break;
                }
              }

              for (var jj = 0; jj < hiddenRootNode.children.length; jj++) {
                if (hiddenRootNode.children[jj].text.toLowerCase() == selectValues[j].toLowerCase()) {
                  var properties = hiddenRootNode.children[jj].property;
                  properties.keyType = 'assigned';
                  properties.nullable = false;

                  keysNode.appendChild({
                    text: selectValues[j],
                    type: "keyProperty",
                    leaf: true,
                    iconCls: 'treeKey',
                    hidden: false,
                    property: properties
                  });

                  hiddenRootNode.children.splice(jj, 1);
                  jj--;
                  break;
                }
              }
            }
          }

          if (keysNode.expanded == false)
            keysNode.expand();
        }
      }, {
        xtype: 'tbspacer',
        width: 4
      }, {
        xtype: 'button',
        icon: 'Content/img/16x16/edit-clear.png',
        text: 'Reset',
        tooltip: 'Reset to the latest applied changes',
        handler: function (f) {
          var availItems = new Array();
          var propertiesNode = node.parentNode.childNodes[1];
          var hiddenRootNode = propertiesNode.raw.hiddenNodes.hiddenNode;

          for (var i = 0; i < hiddenRootNode.children.length; i++) {
            var itemName = hiddenRootNode.children[i].text;
            availItems.push([itemName, itemName]);
          }

          var selectItems = getSelectItems(me.treeNode);
          var keysItemSelector = me.items.items[1];

          if (keysItemSelector.fromField.store.data) {
            keysItemSelector.fromField.store.removeAll();
          }

          keysItemSelector.fromField.store.loadData(availItems);

          var list = keysItemSelector.toField.boundList;
          var store = list.getStore();

          if (store.data) {
            store.removeAll();
          }

          for (var i = 0; i < selectItems.length; i++) {
            store.insert(i + 1, 'field1');
            store.data.items[i].data.field1 = selectItems[i];
          }

          list.refresh();
        }
      }]
    });

    this.callParent(arguments);
  }
});

function getAvailItems(node) {
  var availItems = new Array();
  var propertiesNode = node.parentNode.childNodes[1];
  var hiddenRootNode = propertiesNode.raw.hiddenNodes.hiddenNode;
  var itemName;

  for (var i = 0; i < propertiesNode.childNodes.length; i++) {
    itemName = propertiesNode.childNodes[i].data.text;
    availItems.push(itemName);
  }

  for (var i = 0; i < hiddenRootNode.children.length; i++) {
    itemName = hiddenRootNode.children[i].text;
    availItems.push(itemName);
  }

  return availItems;
};

function getSelectItems(node) {
  var selectItems = new Array();

  for (var i = 0; i < node.childNodes.length; i++) {
    var keyName = node.childNodes[i].data.text;
    selectItems.push(keyName);
  }

  return selectItems;
}

