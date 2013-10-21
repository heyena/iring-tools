/*
 * File: Scripts/AM/view/nhibernate/SetPropertyForm.js
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

Ext.define('AM.view.nhibernate.SetPropertyForm', {
  extend: 'Ext.form.Panel',
  alias: 'widget.setpropertyform',

  width: 400,
  bodyPadding: 10,
  bodyStyle: 'background:#eee;padding:10px 0px 0px 10px',

  initComponent: function() {
    var me = this;

    Ext.applyIf(me, {
      items: [
        {
          xtype: 'label',
          cls: 'x-form-item',
          style: 'font-weight:bold;',
          text: 'Data Properties'
        },
        {
          xtype: 'textfield',
          anchor: '100%',
          fieldLabel: 'Column Name',
          labelWidth: 160,
          name: 'columnName',
          readOnly: true
        },
        {
          xtype: 'textfield',
          anchor: '100%',
          fieldLabel: 'Property Name (editable)',
          labelWidth: 160,
          name: 'propertyName',
          allowBlank: false,
          regex: /^[a-zA-Z_][a-zA-Z0-9_]*$/,
          regexText: '<b>Error</b></br>Invalid Value. A valid value should start with alphabet or "_", and follow by any number of "_", alphabet, or number characters'
        },
        {
          xtype: 'textfield',
          anchor: '100%',
          fieldLabel: 'Data Type',
          labelWidth: 160,
          name: 'dataType',
          readOnly: true
        },
        {
          xtype: 'textfield',
          anchor: '100%',
          fieldLabel: 'Data Length',
          labelWidth: 160,
          name: 'dataLength',
          readOnly: true
        },
        {
          xtype: 'checkboxfield',
          anchor: '100%',
          fieldLabel: 'Nullable',
          labelWidth: 160,
          name: 'isNullable',
          readOnly: true
        },
        {
          xtype: 'checkboxfield',
          anchor: '100%',
          fieldLabel: 'Show On Index',
          labelWidth: 160,
          name: 'showOnIndex'
        },
        {
          xtype: 'textfield',
          anchor: '100%',
          fieldLabel: 'Number of Decimals',
          labelWidth: 160,
          name: 'numberOfDecimals',
          readOnly: true
        },
        {
          xtype: 'checkboxfield',
          anchor: '100%',
          fieldLabel: 'Hidden',
          labelWidth: 160,
          name: 'isHidden'
        }
      ],
      dockedItems: [
        {
          xtype: 'toolbar',
          dock: 'top',
          layout: {
            padding: 4,
            type: 'hbox'
          },
          items: [
            {
              xtype: 'tbspacer',
              width: 4
            },
            {
              xtype: 'button',
              action: 'savedataproperty',
              iconCls: 'am-apply',
              text: 'Apply'
            },
            {
              xtype: 'tbspacer',
              width: 4
            },
            {
              xtype: 'button',
              action: 'resetdataproperty',
              iconCls: 'am-edit-clear',
              text: 'Reset'
            }
          ]
        }
      ]
    });

    me.callParent(arguments);
  },

  setActiveRecord: function(record) {
    var me = this;
    if (record) {
      me.getForm().setValues(record);
    } else {
      me.getForm().reset();
    }
  }

});