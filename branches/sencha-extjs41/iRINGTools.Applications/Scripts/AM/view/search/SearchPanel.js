/*
 * File: Scripts/AM/view/search/SearchPanel.js
 *
 * This file was generated by Sencha Architect version 2.1.0.
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

Ext.define('AM.view.search.SearchPanel', {
  extend: 'Ext.panel.Panel',
  alias: 'widget.searchpanel',

  requires: [
    'AM.view.common.PropertyPanel',
    'AM.view.search.SearchToolbar',
    'AM.view.common.ContentPanel'
  ],

  height: 220,
  margin: '0 0 5 0',
  layout: {
    type: 'border'
  },
  collapsible: true,
  title: 'Reference Data Search',

  initComponent: function() {
    var me = this;

    Ext.applyIf(me, {
      items: [
        {
          xtype: 'propertypanel',
          width: 250,
        //  collapseDirection: 'right',
          title: 'Search Details',
          region: 'east',
          split: true
        },
        {
          xtype: 'contentpanel',
          itemId: 'searchcontent',
          region: 'center'
        }
      ],
      dockedItems: [
        {
          xtype: 'searchtoolbar',
          dock: 'top'
        }
      ]
    });

    me.callParent(arguments);
  }

});