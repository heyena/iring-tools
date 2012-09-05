Ext.require([
    'Ext.grid.*',
    'Ext.data.*',
    'Ext.util.*',
    'Ext.state.*'
]);

Ext.define('AM.view.nhibernate.RelationGridPanel', {
  extend: 'Ext.grid.Panel',
  alias: 'widget.relationgridpanel',
  frame: false,
  stateful: true,
  store: null,
  stateId: 'stateGrid',
  border: false,
  enableColLock: true,
  autoScroll: true,
  data: null,
  node: null,
  columns: [{ text: 'Data Relationship Name', flex: 1, dataIndex: 'relationName'}],
  relations: null,
  dataGridPanel: null,
  rootNode: null,
  viewConfig: {
    stripeRows: true
  },

  initComponent: function () {
    var me = this;
    var node = me.node;

    this.store = Ext.create('Ext.data.ArrayStore', {
      model: AM.model.RelationNameModel,
      data: me.relations
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
        handler: function (btn) {
          var msg = 'Relationship name cannot be added when the field is blank.';
          var node = me.node;
          var mydata = me.store.data.items;
          var rootNode = me.rootNode;
          var numberOfRelation = rootNode.childNodes.length - 1;
          var form = me.dataGridPanel.getForm();

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

          var newRelationRecord = new AM.model.RelationNameModel({
            relationName: relationName
          });

          me.store.add(newRelationRecord);
          me.dataGridPanel.doLayout();

          var exitNode = false;

          for (var j = 0; j < node.childNodes.length; j++) {
            exitNode = false;
            for (var i = 0; i < mydata.length; i++) {
              newNodeText = mydata[i].data.relationName;
              if (node.childNodes[j].data.text.toLowerCase() == newNodeText.toLowerCase()) {
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
          }

          var nodeChildren = new Array();
          for (var j = 0; j < node.childNodes.length; j++)
            nodeChildren.push(node.childNodes[j].data.text);

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
              relationshipTypeIndex: '1'
            });

            if (node.expanded == false)
              node.expand();

            var relationNode = node.findChild('text', relationName)
            setRelationFields(me.editor, me.rootNode, relationNode, me.contextName, me.endpoint)           
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
          var selectModel = me.getSelectionModel();
          if (selectModel.hasSelection()) {
            var selectRecord = selectModel.getLastFocused();
            me.store.remove(selectRecord);
          }
          else {
            if (me.store.data.items.length < 1)
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
         
function createRelationGrid(editor, relationPanel, rootNode, node, gridlabel, dataGridPanel, relations, configLabel, dbObjLabel, formLabel, callId, contextName, endpoint, relatedObjName) {
  if (dataGridPanel.items) {
    var relationPane = dataGridPanel.items.items[0];
    if (relationPane) {
      relationPane.destroy();
    }
  }

  var conf = {
    relations: relations,
    id: gridlabel,
    node: node,
    rootNode: rootNode,
    dataGridPanel: relationPanel,
    contextName: contextName,
    endpoint: endpoint,
    editor: editor
  };

  var relationGrid = Ext.widget('relationgridpanel', conf);
  dataGridPanel.items.add(relationGrid);
  dataGridPanel.doLayout();
};

