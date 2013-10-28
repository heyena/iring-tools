/*
 * File: Scripts/AM/view/nhibernate/SetRelationForm.js
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

Ext.define('AM.view.nhibernate.SetRelationForm', {
  extend: 'Ext.form.Panel',
  alias: 'widget.setrelationform',

  requires: [
    'AM.view.nhibernate.RelationPropertyGrid'
  ],

  rootNode: '',
  endpoint: '',
  contextName: '',
  node: '',
  autoScroll: true,
  bodyStyle: 'background:#fff;padding:10px',
  title: 'Configure Relationship',

  initComponent: function() {
    var me = this;

    Ext.applyIf(me, {
      items: [
        {
          xtype: 'textfield',
          anchor: '100%',
          margin: '5 0 0 10',
          fieldLabel: 'Relationship Name',
          labelWidth: 160,
          name: 'relationshipName',
          readOnly: true,
          allowBlank: false
        },
        {
          xtype: 'textfield',
          anchor: '100%',
          margin: '5 0 0 10',
          fieldLabel: 'Object Name',
          labelWidth: 160,
          name: 'objectName',
          readOnly: true,
          allowBlank: false
        },
        {
          xtype: 'combobox',
          anchor: '100%',
          itemId: 'relatedObjectCmb',
          margin: '5 0 0 10',
          fieldLabel: 'Related Object Name',
          labelWidth: 160,
          name: 'relatedObjectName',
          selectOnFocus: true,
          queryMode: 'local',
          valueField: 'value',
          listeners: {
            select: {
              fn: me.onRelatedObjectSelect,
              scope: me
            },
            change: {
              fn: me.onRelatedObjectCmbChange,
              scope: me
            }
          }
        },
        {
          xtype: 'combobox',
          anchor: '100%',
          itemId: 'relationType',
          margin: '5 0 0 10',
          fieldLabel: 'Relation Type',
          labelWidth: 160,
          name: 'relationType',
          allowBlank: false,
          queryMode: 'local',
          store: [
            [
              'OneToOne',
              'OneToOne'
            ],
            [
              'OneToMany',
              'OneToMany'
            ]
          ]
        },
        {
          xtype: 'combobox',
          anchor: '100%',
          itemId: 'propertyNameCmb',
          margin: '5 0 0 10',
          fieldLabel: 'Related Property Name',
          labelWidth: 160,
          name: 'propertyName',
          queryMode: 'local',
          listeners: {
            select: {
              fn: me.onPropertySelect,
              scope: me
            }
          }
        },
        {
          xtype: 'combobox',
          anchor: '100%',
          itemId: 'mapPropertyNameCmb',
          margin: '5 0 0 10',
          fieldLabel: 'Mapping Property',
          labelWidth: 160,
          name: 'mapPropertyName',
          queryMode: 'local',
          listeners: {
            select: {
              fn: me.onMapPropertyNameCmbSelect,
              scope: me
            }
          }
        },
        {
          xtype: 'relationPropertyGrid',
          height: 200,
          anchor: '100%'
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
                  fn: me.onPropertyMapClick,
                  scope: me
                }
              }
            },
            {
              xtype: 'tbspacer'
            },
            {
              xtype: 'button',
              iconCls: 'am-edit-clear',
              text: 'Reset',
              listeners: {
                click: {
                  fn: me.onRelationReset,
                  scope: me
                }
              }
            }
          ]
        }
      ]
    });

    me.callParent(arguments);
  },

  onRelatedObjectSelect: function(combo, records, eOpts) {
    var me = this;
    var form = combo.up('setrelationform');
    var rootNode = form.rootNode;
    var relatedObjectName = records[0].data.text;

    if (relatedObjectName !== '') {
      var relatedDataObjectNode = rootNode.findChild('text', relatedObjectName);
      var relationConfigPanel = form.getForm();
      var mappingProperties = [];

      if (relatedDataObjectNode.childNodes[1]) {
        keysNode = relatedDataObjectNode.childNodes[0];
        propertiesNode = relatedDataObjectNode.childNodes[1];
        var ii = 0;

        keysNode.eachChild(function(child) {
          mappingProperties.push([ii, child.data.text, child.data.property.columnName]);
          ii++;
        });

        propertiesNode.eachChild(function(child) {
          mappingProperties.push([ii, child.data.text, child.data.property.columnName]);
          ii++;
        });
      }

      var mapCombo = form.down('#mapPropertyNameCmb');
      var myStore = Ext.create('Ext.data.SimpleStore', {
        fields: ['value', 'text', 'name'],
        autoLoad: true,
        data: mappingProperties
      });
      mapCombo.bindStore(myStore);
      /* mapCombo.store = Ext.create('Ext.data.SimpleStore', {
      fields: ['value', 'text', 'name'],
      autoLoad: true,
      data: mappingProperties
      });
      */
    }
  },

  onRelatedObjectCmbChange: function(field, newValue, oldValue, eOpts) {

    var me = this;
    var form = field.up('setrelationform');
    var rootNode = form.rootNode;
    var mapCombo = form.down('#mapPropertyNameCmb');
    var relatedObjectName = newValue;//records[0].data.text;
    if(newValue == ''){
      var myStore = Ext.create('Ext.data.SimpleStore', {
        fields: ['value', 'text', 'name'],
        autoLoad: true,
        data: []
      });
      mapCombo.bindStore(myStore);
    }else{

      if (relatedObjectName !== '') {
        var relatedDataObjectNode = rootNode.findChild('text', relatedObjectName);
        var relationConfigPanel = form.getForm();
        var mappingProperties = [];

        if (relatedDataObjectNode.childNodes[1]) {
          keysNode = relatedDataObjectNode.childNodes[0];
          propertiesNode = relatedDataObjectNode.childNodes[1];
          var ii = 0;

          keysNode.eachChild(function(child) {
            mappingProperties.push([ii, child.data.text, child.data.property.columnName]);
            ii++;
          });

          propertiesNode.eachChild(function(child) {
            mappingProperties.push([ii, child.data.text, child.data.property.columnName]);
            ii++;
          });
        }
        var mapCombo = form.down('#mapPropertyNameCmb');
        var myStore = Ext.create('Ext.data.SimpleStore', {
          fields: ['value', 'text', 'name'],
          autoLoad: true,
          data: mappingProperties
        });
        mapCombo.bindStore(myStore);
      }

    }
    form.down('#mapPropertyNameCmb').reset();
  },

  onPropertySelect: function(combo, records, eOpts) {

  },

  onMapPropertyNameCmbSelect: function(combo, records, eOpts) {

  },

  onPropertyMapClick: function(button, e, eOpts) {
    var me = this;
    var form = button.up('setrelationform');
    var grid = form.down('relationPropertyGrid');

    var newNodeName = form.getForm().findField('relationshipName').getValue();
    var objectName = form.getForm().findField('objectName').getValue();

    var relationTypeCmb = form.down('#relationType');
    var ptropertyCmb = form.down('#propertyNameCmb');
    var mapPropertyCmb = form.down('#mapPropertyNameCmb');

    var node = form.node.findChild('text', newNodeName);
    node.set('title', newNodeName);

    node.data.relationshipType = relationTypeCmb.getValue();
    node.data.relationshipTypeIndex = relationTypeCmb.store.find('field1',node.data.relationshipType);

    var relatedName = form.getForm().findField('relatedObjectName').getValue();
    node.data.relatedObjectName = relatedName;
    node.raw.relatedObjectName = relatedName;
    var propertyMap = [];

    /*if(node.data.propertyMap) {
    for (i = 0; i < node.data.propertyMap.length; i++)
    propertyMap.push([node.data.propertyMap[i].dataPropertyName, node.data.propertyMap[i].relatedPropertyName]);
    } else {
    node.data.propertyMap = [];
    grid.getStore().each(function(record) {
    node.data.propertyMap.push({'dataPropertyName': record.data.property, 'relatedPropertyName': record.data.relatedProperty});
    });
    }*/

    node.data.propertyMap = [];
    grid.getStore().each(function(record) {
      node.data.propertyMap.push({'dataPropertyName': record.data.property, 'relatedPropertyName': record.data.relatedProperty});
    });
  },

  onRelationReset: function(button, e, eOpts) {
    var me = this;
    var form = button.up('setrelationform');
    var grid = form.down('relationPropertyGrid');
    var store = grid.getStore();
    var node = form.node;
    var relationshipName = form.getForm().findField('relationshipName').getValue();
    form.getForm().findField('relatedObjectName').setValue('');  
    form.getForm().findField('relationType').setValue(node.firstChild.raw.relationshipType);
    form.getForm().findField('mapPropertyName').setValue('');
    form.getForm().findField('propertyName').setValue('');
    store.removeAll();

    for(var i=0;i<utilsObj.relationGridStore.length;i++){
      pMap = utilsObj.relationGridStore[i];
      if(pMap){

        var record = [{
          'property':  pMap.dataPropertyName,
          'relatedProperty': pMap.relatedPropertyName
        }];
        var exist = store.find('property', pMap.dataPropertyName);
        if(exist == -1)
        store.add(record);

      }
    }
    /*node.firstChild.raw.propertyMap = [];
    store.each(function(record) {
    node.firstChild.raw.propertyMap.push({'dataPropertyName': record.data.property, 'relatedPropertyName': record.data.relatedProperty});
    if(node.firstChild.data.propertyMap!=undefined) 
    node.firstChild.data.propertyMap.push({'dataPropertyName': record.data.property, 'relatedPropertyName': record.data.relatedProperty});

    });*/

    node.eachChild(function(node) {
      if(node.data.text == relationshipName){
        node.raw.propertyMap = [];
        store.each(function(record) {
          node.raw.propertyMap.push({'dataPropertyName': record.data.property, 'relatedPropertyName': record.data.relatedProperty});
          if(node.data.propertyMap!=undefined) 
          node.data.propertyMap.push({'dataPropertyName': record.data.property, 'relatedPropertyName': record.data.relatedProperty});

        });
      }

    });
  }

});