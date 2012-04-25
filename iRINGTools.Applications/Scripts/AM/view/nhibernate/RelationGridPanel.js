Ext.require([
    'Ext.form.field.Text'
]);

Ext.define('AM.view.nhibernate.RelationGridPanel', {
  extend: 'Ext.grid.Panel',
  alias: 'widget.relationgridpanel',
  frame: false,
  viewConfig: {
    stripeRows: true
  },
  border: false,
  enableColLock: true,
  autoScroll: true,
  data: null,
  node: null,
  height: 300,
  columns: null,
  relations: null,
  rootNode: null,
  dataGridPanel: null,
  store: null,
  bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',

  initComponent: function () {
    var me = this;
    this.store = Ext.create('Ext.data.ArrayStore', {
      autoLoad: false,
      fields: [
        { name: 'relationName' }
      ],
      data: this.relations
    });

    this.columns = Ext.create('Ext.grid.column.Column', {
      text: "Data Relationship Name", flex: 1, dataIndex: 'relationName'
    });

    this.tbar = new Ext.Toolbar({
      items: [{
        xtype: 'tbspacer',
        width: 4
      }, {
        xtype: 'button',
        icon: 'Content/img/16x16/list-add.png',
        text: 'Add',
        tooltip: 'Add',
        handler: function () {
          var msg = 'Relationship name cannot be added when the field is blank.';
          var node = me.node;
          var store = me.store;
          var mydata = store.data.items;
          var rootNode = me.rootNode;
          var numberOfRelation = rootNode.childNodes.length - 1;
          var form = me.dataGridPanel.getForm();
          var relationList = new Array();

          if (mydata.length >= numberOfRelation) {
            if (numberOfRelation == 0) {
              var message = 'Data object "' + node.parentNode.text + '" cannot have any relationship since it is the only data object selected';
              showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
            }
            else {
              var message = 'Data object "' + node.parentNode.text + '" cannot have more than ' + numberOfRelation + ' relationship';
              showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
            }
            return;
          }

          var relationName = form.findField('relationName').getValue().replace(/^\s*/, "").replace(/\s*$/, "");
          if (relationName == '') {
            showDialog(400, 100, 'Warning', msg, Ext.Msg.OK, null);
            return;
          }

          if (mydata.length > 0) {
            for (var i = 0; i < mydata.length; i++) {
              if (mydata[i].data.relationName.toLowerCase() == relationName.toLowerCase()) {
                var message = relationName + ' already exits.';
                showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
                return;
              }             
            }
          }

          Ext.define('relationRecord', {
            extend: 'Ext.data.Model',
            fields: [
              { name: "relationName" }
            ]
          });     

          me.store.on('load', function () {
            me.store.add(new relationRecord({
              relationName: relationName
            }, 0));
          });         

          var exitNode = false;

          for (var j = 0; j < node.childNodes.length; j++) {
            exitNode = false;
            for (var i = 0; i < mydata.length; i++) {
              newNodeText = mydata[i].data.relationName;
              if (node.childNodes[j].text.toLowerCase() == newNodeText.toLowerCase()) {
                exitNode = true;
                break;
              }
            }
            if (exitNode == false) {
              var deleteNode = node.childNodes[j];
              node.childNodes.splice(j, 1);
              j--;
              node.removeChild(deleteNode);
            }

            var nodeChildren = new Array();
            for (var j = 0; j < node.childNodes.length; j++)
              nodeChildren.push(node.childNodes[j].text);

            newNodeText = relationName;
            exitNode = false;
            for (var j = 0; j < nodeChildren.length; j++) {
              if (nodeChildren[j].toLowerCase() == newNodeText.toLowerCase()) {
                exitNode = true;
                break;
              }
            }

            if (exitNode == false) {
              node.appendChild({
                text: relationName,
                type: 'relationship',
                leaf: true,
                iconCls: 'treeRelation',
                relatedObjMap: [],
                objectName: node.parentNode.text,
                relatedObjectName: '',
                relationshipType: 'OneToOne',
                relationshipTypeIndex: '1',
                propertyMap: []
              });

              if (node.expanded == false)
                node.expand();

              if (!newNode.isSelected())
                newNode.select();

              //setRelationFields(editPane, newNode, scopeName, appName);
            }
          }
        }
      }, {
        xtype: 'tbspacer',
        width: 4
      }, {
        xtype: 'button',
        icon: 'Content/img/16x16/list-remove.png',
        text: 'Remove',
        tooltip: 'Remove',
        handler: function () {
          var selectModel = dataRelationGridPane.getSelectionModel();
          if (selectModel.hasSelection()) {
            var selectIndex = selectModel.getSelectedIndex();
            dataStore.removeAt(selectIndex);

            if (callId == 1) {
              var tab = Ext.getCmp('content-panel');
              var rp = tab.items.map[configLabel];
              var dataObjectsPane = rp.items.map[dbObjLabel];
              var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];
              var node = dbObjectsTree.getSelectionModel().getSelectedNode();

              var relatedMapItem = findNodeRelatedObjMap(node, relatedObjName);
              relatedMapItem.remove(relatedMapItem[selectIndex]);
            }
          }
          else {
            if (dataStore.data.items.length < 1)
              showDialog(400, 100, 'Warning', 'No records exits in the table', Ext.Msg.OK, null);
            else
              showDialog(400, 100, 'Warning', 'Please select a row first.', Ext.Msg.OK, null);
          }
        }
      }]
    });

    this.callParent(arguments);
  }
});
         
function createRelationGrid(relationPanel, rootNode, node, gridlabel, dataGridPanel, relations, configLabel, dbObjLabel, formLabel, callId, scopeName, appName, relatedObjName) {
  if (dataGridPanel.items) {
    var relationPane = dataGridPanel.items.map[gridlabel];
    if (relationPane) {
      relationPane.destroy();
    }
  }

  var conf = {
    relations: relations,
    id: gridlabel,
    node: node,
    rootNode: rootNode,
    dataGridPanel: relationPanel
  };

  var relationGrid = Ext.widget('relationgridpanel', conf);
  dataGridPanel.items.add(relationGrid);
  dataGridPanel.doLayout();
};

