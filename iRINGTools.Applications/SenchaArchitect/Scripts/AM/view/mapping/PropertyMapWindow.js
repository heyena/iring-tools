/*
 * File: Scripts/AM/view/mapping/PropertyMapWindow.js
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

Ext.define('AM.view.mapping.PropertyMapWindow', {
  extend: 'Ext.window.Window',
  alias: 'widget.propertymapwindow',

  requires: [
    'AM.view.mapping.PropertyMapForm'
  ],

  height: 140,
  width: 430,
  layout: {
    type: 'fit'
  },
  title: 'Map Data Property to RoleMap',

  initComponent: function() {
    var me = this;

    me.addEvents(
      'save',
      'reset'
    );

    Ext.applyIf(me, {
      items: [
        {
          xtype: 'propertymapform'
        }
      ]
    });

    me.callParent(arguments);
  }

});