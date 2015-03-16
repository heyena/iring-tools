﻿/*
* File: Scripts/AM/view/directory/CacheWindow.js
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

Ext.define('AM.view.directory.SchduleCacheWindow', {
    extend: 'Ext.window.Window',
    alias: 'widget.cachewindow',
    title: 'New Job',
    requires: [
    'AM.view.directory.NewJobForm'
  ],

    node: '',
    border: false,
    height: 465,
    width: 590,
    layout: {
        type: 'fit'
    },

    initComponent: function () {
        var me = this;

        me.addEvents(
      'save',
      'reset'
    );

        Ext.applyIf(me, {
            items: [
        {
            xtype: 'newjobform'
        }
      ]
        });

        me.callParent(arguments);
    }

});