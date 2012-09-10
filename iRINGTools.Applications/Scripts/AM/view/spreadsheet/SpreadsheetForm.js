/*
 * File: Scripts/AM/view/spreadsheet/SpreadsheetForm.js
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

Ext.define('AM.view.spreadsheet.SpreadsheetForm', {
  extend: 'Ext.form.Panel',
  alias: 'widget.spreadsheetform',

  border: 'false',
  frame: false,
  bodyStyle: 'padding:10px 5px 0',

  initComponent: function() {
    var me = this;

    Ext.applyIf(me, {
      defaults: {
        anchor: '100%',
        msgTarget: 'side'
      },
      dockedItems: [
        {
          xtype: 'toolbar',
          dock: 'bottom',
          layout: {
            padding: 4,
            type: 'hbox'
          },
          items: [
            {
              xtype: 'tbfill'
            },
            {
              xtype: 'button',
              icon: 'Content/img/16x16/document-up.png',
              text: 'Upload'
            }
          ]
        }
      ],
      items: [
        {
          xtype: 'hiddenfield',
          name: 'contectName'
        },
        {
          xtype: 'hiddenfield',
          name: 'endpoint'
        },
        {
          xtype: 'hiddenfield',
          name: 'baseUrl'
        },
        {
          xtype: 'hiddenfield',
          name: 'datalayer'
        },
        {
          xtype: 'filefield',
          anchor: '100%',
          name: 'sourceFile',
          fieldLabel: 'Spreadsheet Source',
          labelWidth: 150,
          emptyText: 'Select a Spreadsheet\'',
          buttonConfig: {
            iconCls: 'upload-icon',
            buttonText: ''
          }
        },
        {
          xtype: 'checkboxfield',
          anchor: '100%',
          name: 'generate',
          fieldLabel: 'Generate Configuration',
          labelWidth: 150,
          checked: true
        }
      ]
    });

    me.callParent(arguments);
  }

});