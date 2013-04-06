/*
 * File: Scripts/AM/view/mapping/MapValueListWindow.js
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

Ext.define('AM.view.mapping.MapValueListWindow', {
  extend: 'Ext.window.Window',
  alias: 'widget.mapvaluelistwindow',

  requires: [
    'AM.view.mapping.MapValueListForm'
  ],

  height: 170,
  width: 430,
  layout: {
    type: 'fit'
  },
  title: 'Map Valuelist to RoleMap',

  initComponent: function() {
    var me = this;

    me.addEvents(
      'reset',
      'save'
    );

    Ext.applyIf(me, {
      items: [
        {
          xtype: 'mapvaluelistform'
        }
      ]
    });

    me.callParent(arguments);
  }

});