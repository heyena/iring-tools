/*
 * File: Scripts/AM/view/mapping/MappingPanel.js
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

Ext.define('AM.view.mapping.MappingPanel', {
  extend: 'Ext.panel.Panel',
  alias: 'widget.mappingpanel',

  requires: [
    'AM.view.mapping.MappingTree',
    'AM.view.common.PropertyPanel'
  ],

  endpoint: '',
  baseUrl: '',
  contextName: '',
  graph: '',
  layout: {
    type: 'border'
  },
  closable: true,
  iconCls: 'tabsMapping',

  initComponent: function() {
    var me = this;

    Ext.applyIf(me, {
      items: [
        {
          xtype: 'mappingtree',
          region: 'center'
        },
        {
          xtype: 'propertypanel',
          width: 300,
          title: 'Mapping Details',
          region: 'east',
          split: true
        }
      ],
      listeners: {
        beforeclose: {
          fn: me.onPanelBeforeClose,
          scope: me
        }
      }
    });

    me.processMappingMappingPanel(me);
    me.callParent(arguments);
  },

  processMappingMappingPanel: function(config) {

  },

  onPanelBeforeClose: function(panel, eOpts) {
    /*var me = this;
    var tree = me.down('mappingtree');
    Ext.Msg.show({
    title: 'Close Mapping?',
    msg: 'Do you want to save changes to ' + me.title,
    buttons: Ext.Msg.YESNO,
    fn: function (response) {
    if(response === 'yes') {
    tree.onSave();
    }
    }
    });
    */
  }

});