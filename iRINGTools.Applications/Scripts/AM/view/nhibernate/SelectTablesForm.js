/*
 * File: Scripts/AM/view/nhibernate/SelectTablesForm.js
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

Ext.define('AM.view.nhibernate.SelectTablesForm', {
  extend: 'Ext.form.Panel',
  alias: 'widget.selecttablesform',

  requires: [
    'AM.view.nhibernate.MultiSelectionGrid'
  ],

  dirNode: '',
  border: 'false',
  frame: false,
  autoScroll: true,
  bodyPadding: 5,
  bodyStyle: 'background:#eee;padding:10px 0px 0px 10px',

  initComponent: function() {
    var me = this;

    Ext.applyIf(me, {
      items: [
        {
          xtype: 'container',
          height: 22,
          layout: {
            type: 'fit'
          },
          items: [
            {
              xtype: 'label',
              border: 2,
              text: 'Database Tables'
            }
          ]
        },
        {
          xtype: 'multiselectiongrid',
          minHeight: 200,
          autoScroll: true
        },
        {
          xtype: 'checkboxfield',
          anchor: '100%',
          name: 'enableSummary',
          value: 'off',
          fieldLabel: 'Enable Summary:',
          labelWidth: 150,
          inputValue: 'off'
        }
      ],
      dockedItems: [
        {
          xtype: 'toolbar',
          dock: 'top',
          items: [
            {
              xtype: 'tbspacer',
              width: 4
            },
            {
              xtype: 'button',
              action: 'applyobjects',
              iconCls: 'am-apply',
              text: 'Apply'
            },
            {
              xtype: 'tbspacer',
              width: 4
            },
            {
              xtype: 'button',
              action: 'resetobjects',
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