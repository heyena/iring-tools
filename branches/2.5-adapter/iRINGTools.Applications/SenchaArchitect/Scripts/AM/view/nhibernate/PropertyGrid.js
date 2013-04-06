/*
 * File: Scripts/AM/view/nhibernate/PropertyGrid.js
 *
 * This file was generated by Sencha Architect version 2.2.0.
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

Ext.define('AM.view.nhibernate.PropertyGrid', {
  extend: 'Ext.grid.Panel',
  alias: 'widget.propertygrid',

  requires: [
    'AM.view.override.nhibernate.PropertyGrid'
  ],

  propertyPairs: '',

  initComponent: function() {
    var me = this;

    Ext.applyIf(me, {
      columns: [
        {
          xtype: 'gridcolumn',
          dataIndex: 'property',
          text: 'Property',
          flex: 1
        },
        {
          xtype: 'gridcolumn',
          hidden: true,
          dataIndex: 'columnName',
          flex: 1
        },
        {
          xtype: 'gridcolumn',
          dataIndex: 'relatedProperty',
          text: 'Related Property',
          flex: 1
        },
        {
          xtype: 'gridcolumn',
          hidden: true,
          dataIndex: 'relatedColumnName',
          flex: 1
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
              xtype: 'tbspacer',
              width: 4
            },
            {
              xtype: 'button',
              action: 'addproperty',
              iconCls: 'am-list-add',
              text: 'Add'
            },
            {
              xtype: 'tbspacer',
              width: 4
            },
            {
              xtype: 'button',
              action: 'removeproperty',
              iconCls: 'am-list-remove',
              text: 'Remove'
            }
          ]
        }
      ]
    });

    me.processNhibernatePropertyGrid(me);
    me.callParent(arguments);
  },

  processNhibernatePropertyGrid: function(config) {
    var storeId = Ext.data.IdGenerator.get("uuid").generate();

    config.store = Ext.create('Ext.data.ArrayStore', {
      model: 'Am.model.PropertyMapModel',
      data: config.propertyPairs
    }); 
    return config;
  }

});