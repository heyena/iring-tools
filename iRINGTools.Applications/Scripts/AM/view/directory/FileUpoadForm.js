/*
 * File: Scripts/AM/view/directory/FileUpoadForm.js
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

Ext.define('AM.view.directory.FileUpoadForm', {
  extend: 'Ext.form.Panel',
  alias: 'widget.fileuploadform',

  //height: 128,
  //width: 503,
  bodyPadding: 10,
  frameHeader: false,
  header: false,
  method: 'post',
  url: 'File/Upload',

  initComponent: function() {
    var me = this;

    me.initialConfig = Ext.apply({
      method: 'post',
      url: 'File/Upload'
    }, me.initialConfig);

    Ext.applyIf(me, {
      items: [
        {
          xtype: 'filefield',
          anchor: '100%',
          fieldLabel: 'File',
          labelWidth: 40,
          name: 'filePath',
          allowBlank: false,
          emptyText: 'Select a File'
        },
        {
          xtype: 'hiddenfield',
          anchor: '100%',
          name: 'scope'
        },
        {
          xtype: 'hiddenfield',
          anchor: '100%',
          name: 'application'
        }
      ],
      dockedItems: [
        {
          xtype: 'toolbar',
          dock: 'bottom',
          height: 26,
          width: 503,
          layout: {
            pack: 'end',
            type: 'hbox'
          },
          items: [
            {
              xtype: 'button',
              handler: function(button, event) {
                me.onFileUpload();
              },
              text: 'Upload File'
            },
            {
              xtype: 'button',
              handler: function(button, event) {
                me.onCancel();
              },
              text: 'Cancel'
            }
          ]
        }
      ]
    });

    me.callParent(arguments);
  },

  onFileUpload: function() {

    var me = this;
    var win = me.up('window');
    var form = me.getForm();
    var message;
    if(me.getForm().isValid()) {
      me.submit({
        waitMsg: 'Uploading file...',
        success: function (result, request) {
          win.fireEvent('save', me);
        },
        failure: function (result, request) {
			var resp = Ext.decode(request.response.responseText);
			var userMsg = resp['message'];
			var detailMsg = resp['stackTraceDescription'];
			var expPanel = Ext.widget('exceptionpanel', { title: 'Error Notification'});
			Ext.ComponentQuery.query('#expValue',expPanel)[0].setValue(userMsg);
			Ext.ComponentQuery.query('#expValue2',expPanel)[0].setValue(detailMsg);
        }
      });
    } else {
      //message = 'Please Select a file to upload.';
		Ext.widget('messagepanel', { title: 'Warning', msg: 'Please Select a file to upload.'});
	  //showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);   
    }

  },

  onCancel: function() {
    var me = this;
    var win = me.up('window');
    me.getForm().reset();
    win.destroy();
  }

});