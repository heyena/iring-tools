/*
 * File: Scripts/AM/view/directory/DataLayerForm.js
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

Ext.define('AM.view.directory.DataLayerForm', {
  extend: 'Ext.form.Panel',
  alias: 'widget.datalayerform',

  border: false,
  frame: false,
  bodyPadding: 10,
  bodyStyle: 'padding:10px 5px 0',
  method: 'POST',
  url: 'directory/datalayer',

  initComponent: function() {
    var me = this;

    me.initialConfig = Ext.apply({
      method: 'POST',
      url: 'directory/datalayer'
    }, me.initialConfig);

    Ext.applyIf(me, {
      defaults: {
        width: 400,
        msgTarget: 'side'
      },
      dockedItems: [
        {
          xtype: 'toolbar',
          dock: 'bottom',
          items: [
            {
              xtype: 'tbfill'
            },
            {
              xtype: 'button',
              handler: function(button, event) {
                me.onSave();
              },
              text: 'Ok'
            },
            {
              xtype: 'button',
              handler: function(button, event) {
                me.onReset();
              },
              text: 'Cancel'
            }
          ]
        }
      ],
      items: [
        {
          xtype: 'hiddenfield',
          name: 'state'
        },
        {
          xtype: 'textfield',
          name: 'name',
          fieldLabel: 'Name',
          allowBlank: false
        },
        {
          xtype: 'textfield',
          name: 'mainDLL',
          fieldLabel: 'Main DLL',
          allowBlank: false
        },
        {
          xtype: 'filefield',
          name: 'packageFile',
          fieldLabel: 'Package File',
          allowBlank: false
        }
      ]
    });

    me.callParent(arguments);
  },

  onReset: function() {
    var me = this;
    var win = me.up('window');
    win.fireEvent('Cancel', me);
  },

  onSave: function() {
    var me = this;
    if (me.getForm().isValid()) {
      me.getForm().submit({
        waitMsg: 'Processing ...',
        success: function (response, request) {
          me.close();
          showDialog(320, 80, 'Saving Data Layer Result', request.result.messages[0], Ext.Msg.OK, null);
        },
        failure: function (response, request) {
          me.close();

          if (request.result.level === 0) {
            showDialog(320, 80, 'Saving Data Layer Result', request.result.messages[0], Ext.Msg.OK, null);
          }
          else {
            showDialog(320, 300, 'Saving Data Layer Result', request.result.messages[0], Ext.Msg.OK, null);
          }
        }
      });
    } else {
      showDialog(320, 150, 'Saving Data Layer Result', 'Data Layer Form not Complete....', Ext.Msg.OK, null);
    }
  }

});