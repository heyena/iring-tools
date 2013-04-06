/*
 * File: Scripts/AM/view/spreadsheet/SpreadsheetWindow.js
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

Ext.define('AM.view.spreadsheet.SpreadsheetWindow', {
  extend: 'Ext.window.Window',
  alias: 'widget.spreadsheetwindow',

  requires: [
    'AM.view.spreadsheet.SpreadsheetForm'
  ],

  border: false,
  height: 130,
  width: 430,
  layout: {
    type: 'fit'
  },
  bodyPadding: 1,

  initComponent: function() {
    var me = this;

    Ext.applyIf(me, {
      items: [
        {
          xtype: 'spreadsheetform'
        }
      ]
    });

    me.callParent(arguments);
  }

});