/*
 * File: Scripts/AM/view/directory/VirtualPropertyWindow.js
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

Ext.define('AM.view.directory.VirtualPropertyWindow', {
  extend: 'Ext.window.Window',
  alias: 'widget.virtualpropertywindow',

  requires: [
    'AM.view.directory.VirtualPropertyForm'
  ],

  height: 556,
  width: 983,
  resizable: false,
  layout: {
    type: 'fit'
  },
  title: 'Add Virtual Property',
  modal: true,

  initComponent: function() {
    var me = this;

    me.addEvents(
      'save',
      'reset'
    );

    Ext.applyIf(me, {
      items: [
        {
          xtype: 'virtualpropertyform'
        }
      ]
    });

    me.callParent(arguments);
  }

});