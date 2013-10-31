/*
 * File: Scripts/AM/view/menus/RolemapMenu.js
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

Ext.define('AM.view.menus.RolemapMenu', {
  extend: 'Ext.menu.Menu',
  alias: 'widget.rolemapmenu',

  width: 130,

  initComponent: function() {
    var me = this;

    Ext.applyIf(me, {
      items: [
        {
          xtype: 'menuitem',
          action: 'addclassmap',
          icon: 'Content/img/class-map.png',
          text: 'Add/Edit ClassMap'
        },
        {
          xtype: 'menuitem',
          action: 'makepossessor',
          icon: 'Content/img/relation.png',
          text: 'Make Possessor'
        },
        {
          xtype: 'menuitem',
          action: 'makereference',
          hidden: true,
          icon: 'Content/img/16x16/map.png',
          text: 'Make Reference'
        },
        {
          xtype: 'menuitem',
          action: 'mapproperty',
          icon: 'Content/img/property.png',
          text: 'Map Property'
        },
        {
          xtype: 'menuitem',
          action: 'mapvaluelist',
          icon: 'Content/img/value.png',
          text: 'Map ValueList'
        },
        {
          xtype: 'menuitem',
          action: 'mapliteral',
          icon: 'Content/img/valuelist.png',
          text: 'Map Literal'
        },
        {
          xtype: 'menuitem',
          action: 'resetmapping',
          icon: 'Content/img/16x16/reset.png',
          text: 'Reset Mapping'
        }
      ]
    });

    me.callParent(arguments);
  }

});