/*
 * File: Scripts/AM/view/search/SearchToolbar.js
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

Ext.define('AM.view.search.SearchToolbar', {
  extend: 'Ext.toolbar.Toolbar',
  alias: 'widget.searchtoolbar',

  initComponent: function() {
    var me = this;

    Ext.applyIf(me, {
      layout: {
        padding: 4,
        type: 'hbox'
      },
      items: [
        {
          xtype: 'textfield',
          id: 'referencesearch',
          width: 200,
          labelWidth: 200,
          name: 'referencesearch',
          listeners: {
            specialkey: {
              fn: me.onSpecialkey,
              scope: me
            }
          }
        },
        {
          xtype: 'button',
          action: 'search',
          style: 'marginLeft: 5px',
          iconCls: 'am-document-properties',
          text: 'Search'
        },
        {
          xtype: 'checkboxfield',
          autoShow: true,
          style: {
            marginLeft: '5px',
            marginBottom: '8px'
          },
          name: 'reset'
        },
        {
          xtype: 'tbspacer'
        },
        {
          xtype: 'label',
          text: 'Reset'
        },
        {
          xtype: 'tbspacer'
        },
        {
          xtype: 'combobox',
          itemId: 'searchLimitCombo',
          width: 90,
          fieldLabel: 'Limit',
          hideEmptyLabel: false,
          labelWidth: 40,
          name: 'searchLimit',
          submitValue: false,
          value: 50,
          allowBlank: false,
          displayField: 'limit',
          queryMode: 'local',
          store: 'SearchCmbStore',
          valueField: 'value',
          listeners: {
            beforerender: {
              fn: me.onComboboxBeforeRender,
              scope: me
            }
          }
        }
      ]
    });

    me.callParent(arguments);
  },

  onSpecialkey: function(field, e, eOpts) {
    var me = this;
    if (e.getKey() === e.ENTER) {
      me.onSearch();
    }
  },

  onComboboxBeforeRender: function(component, eOpts) {
    component.getStore().load();
  },

  onSearch: function() {
    var me = this;
    var btn = me.down('button');
    btn.fireEvent('click', btn);
  }

});