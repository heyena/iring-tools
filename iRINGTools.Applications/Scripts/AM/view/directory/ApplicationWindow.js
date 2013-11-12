/*
 * File: Scripts/AM/view/directory/ApplicationWindow.js
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

Ext.define('AM.view.directory.ApplicationWindow', {
  extend: 'Ext.window.Window',
  alias: 'widget.applicationwindow',

  requires: [
    'AM.view.directory.ApplicationForm'
  ],

  node: '',
  border: false,
  height: 467,
  width: 734,
  minHeight: 450,
  minWidth: 350,
  layout: {
    type: 'fit'
  },
  bodyPadding: 1,

  initComponent: function() {
    var me = this;

    me.addEvents(
      'save',
      'reset'
    );

    Ext.applyIf(me, {
      items: [
        {
          xtype: 'applicationform',
          height: 402,
          width: 721
        }
      ]
    });

    me.callParent(arguments);
  }

});