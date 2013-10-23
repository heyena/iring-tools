/*
 * File: Scripts/AM/view/nhibernate/SelectPropertiesForm.js
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

Ext.define('AM.view.nhibernate.SelectPropertiesForm', {
  extend: 'Ext.form.Panel',
  alias: 'widget.selectpropertiesform',

  requires: [
    'AM.view.nhibernate.MultiSelectionGrid',
    'AM.view.nhibernate.MultiSelectComponentGrid'
  ],

  width: 300,
  layout: {
    type: 'auto'
},
bodyStyle: 'background:#fff;padding:10px',
title: 'Select Properties',

  initComponent: function() {
    var me = this;

    Ext.applyIf(me, {
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
              action: 'saveselectproperties',
              iconCls: 'am-apply',
              text: 'Apply'
            },
            {
              xtype: 'tbspacer',
              width: 4
            },
            {
              xtype: 'button',
              action: 'resetselectproperties',
              iconCls: 'am-edit-clear',
              text: 'Reset'
            }
          ]
        }
      ],
      items: [{
          xtype: 'multiselectiongrid',
          hidden: true,
          itemId: 'propertiesSelectionGrid'
        },
        {
          xtype: 'multiselectcomponentgrid'
        }
      ]
    });

    me.callParent(arguments);
  }

});