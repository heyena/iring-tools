Ext.Loader.setConfig({ enabled: true });
Ext.Loader.setPath('Ext.ux', 'Scripts/extjs407/examples/ux');
Ext.require([
    'Ext.form.Panel',
    'Ext.ux.form.MultiSelect',
    'Ext.ux.form.ItemSelector'
]);

Ext.define('AM.view.nhibernate.SelectPropertiesPanel', {
  extend: 'Ext.form.Panel',
  alias: 'widget.selectProperties',
  frame: false,
  border: false,
  autoScroll: true,
  bodyStyle: 'background:#eee;padding:10px 0px 0px 10px',
  labelWidth: 140,
  treeNode: null,
  shownProperty: null,
  monitorValid: true,
  selectItems: null,

  initComponent: function () {
    var me = this;
    var shownProperty = new Array();
    var availItems = new Array();
    var node = this.treeNode;
    var hiddenRootNode = node.raw.hiddenNodes.hiddenNode;

    for (var indexOfProperty = 0; indexOfProperty < node.childNodes.length; indexOfProperty++) {
        !hasShown(shownProperty, node.childNodes[indexOfProperty].text)
        shownProperty.push(node.childNodes[indexOfProperty].text);
        indexOfProperty++;
    }

    var selectedItems = new Array();
    for (var i = 0; i < node.childNodes.length; i++) {
      var itemName = node.childNodes[i].data.text;
      selectedItems.push(itemName);
    }

    for (var i = 0; i < hiddenRootNode.children.length; i++) {
      var itemName = hiddenRootNode.children[i].text;
      availItems.push(itemName);
    }

    this.shownProperty = shownProperty;
    this.selectItems = selectedItems;

    this.items = [{
      xtype: 'label',
      text: 'Select Properties',
      cls: 'x-form-item',
      style: 'font-weight:bold;'
    }, {
      xtype: 'itemselector',
      name: 'propertySelector',
      anchor: '100%',
      hideLabel: true,
      listAvailable: 'Available Properties',
      listSelected: 'Selected Properties',
      height: 370,
      bodyStyle: 'background:#eee',
      frame: true,
      imagePath: 'Scripts/extjs407/examples/ux/css/images',
      displayField: 'propertyName',
      store: availItems,
      valueField: 'propertyValue',
      border: 0,
      value: selectedItems,
      listeners: {
        change: function (itemSelector, selectedValuesStr) {
          var selectProperties = itemSelector.toField.store.data.items;
          var availPropertyName;
          var selectPropertyName;

          for (var i = 0; i < selectProperties.length; i++) {
            selectPropertyName = selectProperties[i].data.text;
            if (selectPropertyName == '')
              itemSelector.toField.store.removeAt(i);
          }

          var availProperties = itemSelector.fromField.store.data.items;
          for (var i = 0; i < availProperties.length; i++) {
            availPropertyName = availProperties[i].data.text
            if (availPropertyName == '')
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
          var propertySelector = me.items.items[1];
          if (me.getForm().findField('propertySelector').getValue().indexOf('') == -1)
            var selectedValues = me.getForm().findField('propertySelector').getValue();

          var treeNode = me.treeNode;
          var shownProperty = me.shownProperty;
          var hiddenRootNode = treeNode.raw.hiddenNodes.hiddenNode;
          var indexHidden;

          for (var i = 0; i < treeNode.childNodes.length; i++) {
            var found = false;

            for (var j = 0; j < selectedValues.length; j++) {
              if (selectedValues[j].toLowerCase() == treeNode.childNodes[i].data.text.toLowerCase()) {
                found = true;
                break;
              }
            }

            if (!found) {
              hiddenRootNode.children.push({
                text: treeNode.childNodes[i].data.text,
                property: treeNode.childNodes[i].data.property,
                hidden: true
              });
              treeNode.removeChild(treeNode.childNodes[i], true);
              i--;
            }
          }

          for (var j = 0; j < selectedValues.length; j++) {
            found = false;

            for (var i = 0; i < treeNode.childNodes.length; i++) {
              if (selectedValues[j].toLowerCase() == treeNode.childNodes[i].data.text.toLowerCase()) {
                found = true;
                break;
              }
            }

            for (var k = 0; k < hiddenRootNode.children.length; k++) {
              if (selectedValues[j].toLowerCase() == hiddenRootNode.children[k].text.toLowerCase()) {
                indexHidden = k;
                break;
              }
            }

            if (!found && indexHidden > -1) {
              treeNode.appendChild({
                text: hiddenRootNode.children[indexHidden].text,
                property: hiddenRootNode.children[indexHidden].property,
                type: "dataProperty",
                hidden: false,
                leaf: true,
                iconCls: 'treeProperty'
              });
              hiddenRootNode.children.splice(indexHidden, 1);
            }
          }

          if (treeNode.expanded == false)
            treeNode.expand();
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
          var propertiesItemSelector = me.items.items[1];
          var treeNode = me.treeNode;
          var availProps = new Array();
          var selectedProps = new Array();
          var hiddenRootNode = treeNode.raw.hiddenNodes.hiddenNode;
          var itemName;

          for (var i = 0; i < treeNode.childNodes.length; i++) {
            itemName = node.childNodes[i].data.text;
            selectedProps.push(itemName);
          }

          for (var i = 0; i < hiddenRootNode.children.length; i++) {
            itemName = hiddenRootNode.children[i].text;
            availProps.push([itemName, itemName]);
          }

          if (availProps[0]) {
            if (propertiesItemSelector.fromField.store.data) {
              propertiesItemSelector.fromField.store.removeAll();
            }

            propertiesItemSelector.fromField.store.loadData(availProps);
          }
          else {
            propertiesItemSelector.fromField.store.removeAll();
          }

          if (selectedProps[0]) {
            var list = propertiesItemSelector.toField.boundList;
            var store = list.getStore();

            if (store.data) {
              store.removeAll();
            }

            for (var i = 0; i < selectedProps.length; i++) {
              store.insert(i + 1, 'field1');
              store.data.items[i].data.field1 = selectedProps[i];
            }

            list.refresh();
          } else {
            propertiesItemSelector.toField.store.removeAll();
          }
        }
      }]
    });

    this.callParent(arguments);
  }
});

