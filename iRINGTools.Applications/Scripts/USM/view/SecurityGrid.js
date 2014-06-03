﻿/*
* File: app/view/CommodityListGrid.js
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

Ext.define('USM.view.SecurityGrid', {
    extend: 'Ext.grid.Panel',
    alias: 'widget.securitygrid',
    resizable: true,
    store: 'SecurityS',

    initComponent: function () {
        var me = this;

        Ext.applyIf(me, {
            columns: [
                {
                    xtype: 'gridcolumn',
                    dataIndex: 'name',
                    text: '',
                    flex: 2,
                    menuDisabled: true,
                    renderer: function (value) {
                        return '<html> <b>'+ value+'</b></html>';
                    }
                }
            ]
        });

        me.callParent(arguments);
    }

});