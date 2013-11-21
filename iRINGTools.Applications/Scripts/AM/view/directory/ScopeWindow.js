/*
 * File: Scripts/AM/view/directory/ScopeWindow.js
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

Ext.define('AM.view.directory.ScopeWindow', {
  extend: 'Ext.window.Window',
  alias: 'widget.scopewindow',

  requires: [
    'AM.view.directory.ScopeForm'
  ],

  border: false,
  height: 265,
  width: 467,
  minHeight:265,
  minWidth:250,
  layout: {
    type: 'fit'
  },
  bodyPadding: 1,
  //iconCls: 'tabsScope',
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
          xtype: 'scopeform',
          height: 168,
          width: 448
        }
      ]
    });

    me.callParent(arguments);
  }

});