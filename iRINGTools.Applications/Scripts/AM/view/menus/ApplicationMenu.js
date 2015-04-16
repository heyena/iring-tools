/*
 * File: Scripts/AM/view/menus/ApplicationMenu.js
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

Ext.define('AM.view.menus.ApplicationMenu', {
    extend: 'Ext.menu.Menu',
    alias: 'widget.applicationmenu',

    initComponent: function () {
        var me = this;

        Ext.applyIf(me, {
            items: [
        {
            xtype: 'menuitem',
            action: 'newOrEditApplication',
            itemId: 'editApplication',
            icon: 'Content/img/16x16/edit.png',
            text: 'Edit Application23'
        },
        {
            xtype: 'menuitem',
            action: 'deleteApplication',
            icon: 'Content/img/16x16/delete.png',
            text: 'Delete Application'
        },
        {
            xtype: 'menuseparator'
        },
        {
            xtype: 'menuitem',
            action: 'configureendpoint',
            icon: 'Content/img/16x16/preferences-system.png',
            text: 'Open Configuration'
        },
        {
            xtype: 'menuseparator'
        },
        {
            xtype: 'menuitem',
            action: 'fileupload',
            icon: 'Content/img/16x16/document-up.png',
            text: 'Upload File'
        },
        {
            xtype: 'menuitem',
            action: 'filedownload',
            icon: 'Content/img/16x16/document-down.png',
            text: 'Download File'
        },
        {
            xtype: 'menuitem',
            action: 'cacheupdate',
            icon: 'Content/img/16x16/document-new.png',
            itemId: 'cacheupscreen',
            text: 'Cache Update'
        }
      ]
        });

        me.callParent(arguments);
    }
});