/*
 * File: Scripts/AM/view/menus/ScopesMenu.js
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

Ext.define('AM.view.menus.ScopesMenu', {
  extend: 'Ext.menu.Menu',
  alias: 'widget.scopesmenu',

  initComponent: function() {
    var me = this;

    Ext.applyIf(me, {
      items: [
        {
          xtype: 'menuitem',
          action: 'neweditscope',
          icon: 'Content/img/16x16/document-new.png',
          text: 'New Scope'
        },
        {
          xtype: 'menuitem',
          action: 'regenerateall',
          hidden: true,
          icon: 'Content/img/16x16/document-new.png',
          text: 'Regenerate HibernateDataLayer artifacts'
        }
      ]
    });

    me.callParent(arguments);
  }

});