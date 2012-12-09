/*
 * File: Scripts/AM/view/nhibernate/RelationsForm.js
 *
 * This file was generated by Sencha Architect version 2.1.0.
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

  initComponent: function() {
    var me = this;

    Ext.applyIf(me, {
      defaults: {
        labelWidth: 130,
        allowBlank: false
      },
      items: [
        {
          xtype: 'label',
          cls: 'x-form-item',
          style: 'font-weight:bold;',
          text: 'Add/Remove relationship'
        },
        {
          xtype: 'textfield',
          name: 'relationName',
          fieldLabel: 'Relationship Name',
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
          minHeight: 50
        }
      ],
      dockedItems: [
        {
          xtype: 'toolbar',
          dock: 'top',
          layout: {
            padding: 4,
            type: 'hbox'
          },
          items: [
            {
              xtype: 'button',
              iconCls: 'am-apply',
              text: 'Apply',
              tooltip: 'Apply the current changes to the data objects tree',
              listeners: {
                click: {
                  fn: me.onApplyClick,
                  scope: me
                }
              }
            },
            {
              xtype: 'tbseparator',
              width: 4
            },
            {
              xtype: 'button',
              icon: 'Content/img/16x16/edit-clear.png',
              iconCls: 'am-edit-clear',
              text: 'Reset',
              tooltip: 'Reset to the latest applied changes'
            }
          ]
        }
      ]
    });

    me.callParent(arguments);
  },

  onRelationKeydown: function(textfield, e, options) {
    var me = this;
    if (e.getKey() == e.ENTER) {
      me.addRelationship(me);
    }
  },

  onApplyClick: function(button, e, options) {
    var me = this;
    var form = button.up('relationsform');
    var grid = form.down('relationsgrid');
    var node = form.node;
    var gridStore = grid.getStore();
    var records = gridStore.data.items;
    var exitNode = false;
    if(node.FirstChild) {
      node.eachChild(function(n) {
        exitNode = false;
        gridStore.each(function(record) {
          if(n.data.text.toLowerCase() == record.data.relationName.toLowerCase()) {
            exitNode = true;
          }
        });
        if(exitNode === false) {
          node.removeChild(n, true);
        }
      });
    } else {

    }

  },

  addRelationship: function(form) {
    var me = this;
    var message, newNodeText;
    var relationName = form.getForm().findField("relationName").getValue().replace(/^\s*/, "").replace(/\s*$/, "");

    var grid = form.down('relationsgrid');
    var gridStore = grid.getStore();

    var mydata = gridStore.data.items;
    var rootNode = form.rootNode;
    var node = form.node;
    var numberOfRelation = rootNode.childNodes.length - 1;

    if (mydata.length >= numberOfRelation) {
      if (numberOfRelation === 0) {
        message = 'Data object "' + node.parentNode.text + '" cannot have any relationship since it is the only data object selected';
        showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
      }
      else {
        message = 'Data object "' + node.parentNode.text + '" cannot have more than ' + numberOfRelation + ' relationship';
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

    if (exitNode === false) {
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

      if (node.isExpanded() === false)
      node.expand();

      var relationNode = node.findChild('text', relationName);
      // setRelationFields(me.editor, me.rootNode, relationNode, me.contextName, me.endpoint)           
    }
  }

});