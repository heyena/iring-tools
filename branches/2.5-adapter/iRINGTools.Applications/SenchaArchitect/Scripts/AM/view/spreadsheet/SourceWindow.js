/*
 * File: Scripts/AM/view/spreadsheet/SourceWindow.js
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

Ext.define('AM.view.spreadsheet.SourceWindow', {
  extend: 'Ext.window.Window',
  alias: 'widget.spreadsheetsourcewindow',

  requires: [
    'AM.view.spreadsheet.SourceForm'
  ],

  datalayer: '',
  context: '',
  baseUrl: '',
  endpoint: '',
  border: false,
  height: 130,
  width: 420,
  layout: {
    type: 'fit'
  },

  initComponent: function() {
    var me = this;

    Ext.applyIf(me, {
      items: [
        {
          xtype: 'spreadsheetsourceform'
        }
      ]
    });

    me.callParent(arguments);
  }

});