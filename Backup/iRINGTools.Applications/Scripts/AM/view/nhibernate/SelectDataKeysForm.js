/*
 * File: Scripts/AM/view/nhibernate/SelectDataKeysForm.js
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

Ext.define('AM.view.nhibernate.SelectDataKeysForm', {
  extend: 'Ext.form.Panel',
  alias: 'widget.selectdatakeysform',

  requires: [
    'AM.view.nhibernate.MultiSelectionGrid',
    'AM.view.nhibernate.MultiSelectComponentGrid'
  ],

  width: 300,
  layout: {
    type: 'auto'
  },
  bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',

  initComponent: function() {
    var me = this;

    Ext.applyIf(me, {
      items: [
        {
          xtype: 'label',
          style: 'font-weight:bold;',
          text: 'Select Keys'
        },
        {
          xtype: 'multiselectiongrid',
          hidden: true,
          itemId: 'multiSelectDataKeys'
        },
        {
          xtype: 'multiselectcomponentgrid'
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
              action: 'saveselectkeys',
              iconCls: 'am-apply',
              text: 'Apply'
            },
            {
              xtype: 'tbspacer',
              width: 4
            },
            {
              xtype: 'button',
              action: 'resetselectkeys',
              iconCls: 'am-edit-clear',
              text: 'Reset'
            }
          ]
        }
      ]
    });

    me.callParent(arguments);
  }

});