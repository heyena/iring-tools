/*
 * File: Scripts/AM/view/menus/GroupAdminScopesMenu.js
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

Ext.define('AM.view.menus.GroupAdminScopesMenu', {
  extend: 'Ext.menu.Menu',
  alias: 'widget.groupadminscopesmenu',

  initComponent: function() {
    var me = this;

    Ext.applyIf(me, {
      items: [
        {
          xtype: 'menuitem',
          action: 'newdatalayer',
          icon: 'Content/img/16x16/document-new.png',
          text: 'New Data Layer'
        },
        {
          xtype: 'menuitem',
          action: 'editdatalayer',
          icon: 'Content/img/16x16/document-properties.png',
          text: 'Edit Data Layer'
        },
        {
          xtype: 'menuitem',
          action: 'deletedatalayer',
          icon: 'Content/img/16x16/edit-delete.png',
          text: 'Delete Data Layer'
        },
        {
          xtype: 'menuseparator'
        },
        {
          xtype: 'menuitem',
          action: 'regenerateall',
          icon: 'Content/img/16x16/document-new.png',
          text: 'Regenerate Hibernate Artifacts'
        }
      ]
    });

    me.callParent(arguments);
  }

});