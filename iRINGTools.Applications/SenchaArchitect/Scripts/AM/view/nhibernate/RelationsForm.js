/*
 * File: Scripts/AM/view/nhibernate/RelationsForm.js
 *
 * This file was generated by Sencha Architect version 2.2.2.
 * http://www.sencha.com/products/architect/
 *
 * This file requires use of the Ext JS 4.1.x library, under independent license.
 * License of Sencha Architect does not include license for Ext JS 4.1.x. For more
 * details see http://www.sencha.com/license or contact license@sencha.com.
 *
 * This file will be auto-generated each and everytime you save your project.
 *
 * Do NOT hand edit this file.
 */

Ext.define('AM.view.nhibernate.RelationsForm', {
  extend: 'Ext.form.Panel',
  alias: 'widget.relationsform',

  requires: [
    'AM.view.nhibernate.RelationsGrid'
  ],

  context: '',
  endpoint: '',
  rootNode: '',
  node: '',

  initComponent: function() {
    var me = this;

    me.addEvents(
      'createrelation'
    );

    Ext.applyIf(me, {
      defaults: {
        labelWidth: 130,
        allowBlank: false
      },
      items: [
        {
          xtype: 'panel',
          frame: true,
          header: false,
          items: [
            {
              xtype: 'label',
              cls: 'x-form-item',
              style: 'font-weight:bold;',
              text: 'Add/Remove relationship'
            },
            {
              xtype: 'textfield',
              fieldLabel: 'Relationship Name',
              labelWidth: 160,
              name: 'relationName',
              enableKeyEvents: true,
              size: 40,
              listeners: {
                keydown: {
                  fn: me.onRelationKeydown,
                  scope: me
                }
              }
            },
            {
              xtype: 'relationsgrid',
              autoShow: true,
              height: 700
            }
          ],
          dockedItems: [
            {
              xtype: 'toolbar',
              dock: 'top',
              items: [
                {
                  xtype: 'button',
                  iconCls: 'am-apply',
                  text: 'Apply',
                  listeners: {
                    click: {
                      fn: me.onButtonClick,
                      scope: me
                    }
                  }
                },
                {
                  xtype: 'button',
                  iconCls: 'am-edit-clear',
                  text: 'Reset'
                }
              ]
            }
          ]
        }
      ]
    });

    me.callParent(arguments);
  },

  onRelationKeydown: function(textfield, e, eOpts) {
    var me = this;
    if (e.getKey() == e.ENTER) {
      me.addRelationship(me);
    }
  },

  onButtonClick: function(button, e, eOpts) {

  },

  addRelationship: function(form) {
    var me = this;
    var message, newNodeText;
    var relationName = form.getForm().findField("relationName").getValue().replace(/^\s*/, "").replace(/\s*$/, "");
    var grid = form.down('relationsgrid');
    var gridStore = grid.getStore();
    var nHibernetTree = form.up().up().items.items[0];
    //Making diff store for grid...
    /*
    var dataNode = nHibernetTree.getSelectedNode();
    var relationFolderNode;
    if(dataNode.data.type =='relationships') {
    relationFolderNode = dataNode;
    if(dataNode.firstChild)
    relationName = dataNode.firstChild.data.text;
    }
    else {
    relationFolderNode = dataNode.parentNode;
    relationName = dataNode.data.text;
    }

    relationFolderNode.eachChild(function(relation) {
    if(relation.data.text == relationName) {
    var data;
    if(relation.data.relationshipType)
    data = relation.data;
    else
    data = relation.raw;
    var pMap;
    if(data.propertyMap) {
    gridStore.removeAll();
    for(var i=0;i<data.propertyMap.length;i++){
    pMap = data.propertyMap[i];
    if(pMap){

    var record = [{
    'property':  pMap.dataPropertyName,
    'relatedProperty': pMap.relatedPropertyName
    }];
    var exist = gridStore.find('property', pMap.dataPropertyName);
    if(exist == -1)
    gridStore.add(record);

    }
    }

    }else
    gridStore.removeAll();
    //form.getForm().findField('relationType').setValue(data.relationshipType);

  }
      });
      */

      //*************************



      var mydata = gridStore.data.items;
      var rootNode = form.rootNode;
      var node = form.node;
      var numberOfRelation = rootNode.childNodes.length - 1;

      if (mydata.length >= numberOfRelation) {
  if (numberOfRelation === 0) {
    message = 'Data object "' + node.parentNode.data.text + '" cannot have any relationship since it is the only data object selected';
    showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
  }
  else {
    message = 'Data object "' +node.parentNode.data.text + '" cannot have more than ' + numberOfRelation + ' relationship';
    showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
  }
  return;
      }

      if (relationName === "") {
  message = 'Relationship name cannot be blank.';
  showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
  return;
      }

      gridStore.each(function(relation) {
  if (relation.data.relationName.toLowerCase() == relationName.toLowerCase()) {
    message = relationName + ' already exits.';
    showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
    return;
  } 
      });

      gridStore.add({'relationName': relationName});

      var exitNode = false;

      node.eachChild(function(n) {
  exitNode = false;
  gridStore.each(function(record) {
    newNodeText = record.data.relationName;
    if (n.data.text.toLowerCase() == newNodeText.toLowerCase()) {
      exitNode = true;
    }
  });
  if (exitNode === false) {
    node.childNodes.splice(node.indexOf(n), 1);
    node.removeChild(n);
  }
      });

      var nodeChildren = [];
      node.eachChild(function(n) {
  nodeChildren.push(n.data.text);
      });

      newNodeText = relationName;

      exitNode = false;
      node.eachChild(function(n) {
  if (n.data.text.toLowerCase() == newNodeText.toLowerCase()) {
    exitNode = true;
    //break;
  }
      });


      if (exitNode === false) {
  var newNode = node.appendChild({
    text: relationName,
    type: 'relationship',
    leaf: true,
    iconCls: 'treeRelation',
    relatedObjectMap: [],
    objectName: node.parentNode.text,
    relatedObjectName: '',
    relationshipType: 'OneToOne',
    relationshipTypeIndex: '1'
  });

  if (node.isExpanded() === false)
  node.expand();

  nHibernetTree.getView().select(newNode);

  var relationNode = node.findChild('text', relationName);
  me.fireEvent('createrelation', form, grid, relationName);        
      }
  }

});