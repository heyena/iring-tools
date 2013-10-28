/*
 * File: Scripts/AM/view/directory/ApplicationForm.js
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

Ext.define('AM.view.directory.ApplicationForm', {
  extend: 'Ext.form.Panel',
  alias: 'widget.applicationform',

  requires: [
    'AM.view.directory.DataLayerCombo'
  ],

  record: '',
  border: true,
  bodyStyle: 'padding:10px 5px 0',
  method: 'POST',
  url: 'directory/application',

  initComponent: function() {
    var me = this;

    me.initialConfig = Ext.apply({
      method: 'POST',
      url: 'directory/application'
    }, me.initialConfig);

    Ext.applyIf(me, {
      defaults: {
        msgTarget: 'side',
        anchor: '100%'
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
          name: 'oldAssembly'
        },
        {
          xtype: 'hiddenfield',
          name: 'assembly'
        },
        {
          xtype: 'hiddenfield',
          name: 'scope'
        },
        {
          xtype: 'hiddenfield',
          name: 'application'
        },
        {
          xtype: 'hiddenfield',
          name: 'name'
        },
        {
          xtype: 'hiddenfield',
          itemId: 'state',
          name: 'state'
        },
        {
          xtype: 'hiddenfield',
          name: 'path'
        },
        {
          xtype: 'textfield',
          fieldLabel: 'Name',
          name: 'displayName',
          allowBlank: false
        },
        {
          xtype: 'textareafield',
          fieldLabel: 'Description',
          name: 'description'
        },
        {
          xtype: 'textfield',
          disabled: true,
          hidden: true,
          fieldLabel: 'Context Name',
          name: 'context'
        },
        {
          xtype: 'datalayercombo'
        },
        {
          xtype: 'textfield',
          fieldLabel: 'Cache ImportURI',
          name: 'cacheImportURI',
          allowBlank: false
        },
        {
          xtype: 'textfield',
          fieldLabel: 'Cache Timeout (in minutes)',
          name: 'cacheTimeout',
          allowBlank: false
        },
        {
          xtype: 'container',
          layout: {
            align: 'stretch',
            type: 'hbox'
          },
          items: [
            {
              xtype: 'label',
              flex: 1,
              style: {
                font: 'normal 12px tahoma, arial, helvetica, sans-serif;'
              },
              text: 'Settings:'
            },
            {
              xtype: 'label',
              flex: 1,
              style: {
                font: 'normal 12px tahoma, arial, helvetica, sans-serif;'
              },
              text: 'Name'
            },
            {
              xtype: 'label',
              flex: 1.5,
              style: {
                font: 'normal 12px tahoma, arial, helvetica, sans-serif;'
              },
              text: 'Value'
            },
            {
              xtype: 'button',
              action: 'addsettings',
              width: 50,
              text: 'Add',
              tooltip: 'Click to Add settings'
            }
          ]
        },
        {
          xtype: 'fieldset',
          border: false,
          height: 200,
          id: 'settingfieldset',
          autoScroll: true
        }
      ]
    });

    me.callParent(arguments);
  },

  onSave: function() {
    var me = this;
    var win = me.up('window');
    var endpointName = me.getForm().findField('displayName').getValue();
    var scope = me.getForm().findField('scope').getValue();

    var dlCombo = me.down('combo');
    var state = me.getForm().findField('state').getValue();

    if(state!='edit')
    me.getForm().findField('name').setValue(endpointName);
    /////////////////////////////////////
    if (scope != endpointName) {
      /*if (ifExistSibling(endpointName, that.node, that.state)) {
      showDialog(400, 100, 'Warning', 'The name \"' + endpointName + '\" already exits in this level, please choose a different name.', Ext.Msg.OK, null);
      return;
      }*/

      me.getForm().submit({
        waitMsg: 'Saving Data...',
        success: function (response, request) {
          win.fireEvent('save', me);
          Ext.ComponentQuery.query('directorytree')[0].onReload();
        },
        failure: function (response, request) {
          var message = 'Error saving changes!';
          showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
          return;
        }
      });
    }
    else {
      var message = 'Scope & Application name cannot be same!';
      showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
    }

    ////////////////////////////////////
    /*(me.getForm().isValid()){
    me.getForm().submit({
    waitMsg: 'Saving Data...',
    success: function (response, request) {

    win.fireEvent('save', me);
    Ext.ComponentQuery.query('directorytree')[0].onReload();
    },
    failure: function (response, request) {
    var message = 'Error saving changes!';
    showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
    return;
    }
    });
    }else
    {
    var message = 'Please fill the required data';
    showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
    return;
    }
    */
  },

  onReset: function() {
    var me = this;
    var win = me.up('window');
    win.fireEvent('Cancel', me);
  }

});