/*
 * File: Scripts/AM/view/mapping/ValueListMapForm.js
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

Ext.define('AM.view.mapping.ValueListMapForm', {
  extend: 'Ext.form.Panel',
  alias: 'widget.valuelistmapform',
  bodyStyle: 'padding:10px 5px 0',
  method: 'POST',
  url: 'mapping/valuelistmap',
  initComponent: function() {
    var me = this;
    me.initialConfig = Ext.apply({
      method: 'POST',
      url: 'mapping/valuelistmap'
    }, me.initialConfig);

    Ext.applyIf(me, {
      defaults: {
        anchor: '100%',
        msgTarget: 'side'
      },
      items: [
        {
          xtype: 'textfield',
          anchor: '100%',
          fieldLabel: 'Internal Name',
          name: 'internalName',
          allowBlank: false
        },
        {
          xtype: 'hiddenfield',
          name: 'contextName'
        },
        {
          xtype: 'hiddenfield',
          name: 'endpoint'
        },
        {
          xtype: 'hiddenfield',
          name: 'valueList'
        },
        {
          xtype: 'hiddenfield',
          name: 'baseUrl'
        },
        {
          xtype: 'hiddenfield',
          name: 'mappingNode'
        },
        {
          xtype: 'hiddenfield',
          name: 'classUrl'
        },
        {
          xtype: 'hiddenfield',
          name: 'oldClassUrl'
        },
        {
          xtype: 'hiddenfield',
          name: 'classLabel'
        },
        {
          xtype: 'container',
          anchor: '100%',
          html: 'Drop a Class Node here.',
          itemId: 'vlmfccontainer',
          style: 'border:1px silver solid;margin:5px;padding:8px;height:40px',
          styleHtmlContent: true
        }
      ],
      listeners: {
        afterrender: {
          fn: me.onAfterRender,
          scope: me
        }
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
      ]
    });

    me.callParent(arguments);
  },

  onAfterRender: function(component, eOpts) {
    var me = this;
    var ccont = me.down('#vlmfccontainer');
    var classdd = new Ext.dd.DropTarget(ccont.getEl(), {
      ddGroup: 'refdataGroup',
      notifyDrop: function (classdd, e, data) {
        if (data.records[0].data.type != 'ClassNode') {
          //var message = 'Please slect a RDL Class...';
          //showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
          Ext.widget('messagepanel', { title: 'Warning', msg: 'Please slect a RDL Class.'});
		  return false;
        }
        me.getForm().findField('classUrl').setValue(data.records[0].data.record.Uri);
        me.getForm().findField('classLabel').setValue(data.records[0].data.record.Label);
        var msg = 'Class Label:  ' + data.records[0].data.record.Label;
        ccont.update(msg);
        return true;
      } //eo notifyDrop
    }); //eo classdd
  },

  updateDDContainer: function(record) {
    var me = this;
    //var label = record.record.label;
    var label = record.label;
    var ccon = me.down('#vlmfccontainer');
    if(record!== undefined && label !== undefined) {
      ccon.update('Class Label: ' + label);
    } else {
      return true;
    }
  },

  onSave: function() {
    var me = this;   
    var win = me.up('window');
    var form = me.getForm();
    if (form.findField('internalName').getValue() === '' || 
    form.findField('classUrl').getValue() === '') {
      Ext.widget('messagepanel', { title: 'Warning', msg: 'Please fill in both fields in this form.'});
	  //showDialog(400, 100, 'Warning', 'Please fill in both fields in this form.', Ext.Msg.OK, null);
      return;
    }
    form.submit({
      waitMsg: 'Saving Data...',
      success: function (f, a) {
		Ext.example.msg('Notification', 'ValueList Map saved successfully!');
        win.fireEvent('save', me);
      },
      failure: function (f, a) {
        //var message = 'Error saving changes!';
        //(400, 100, 'Warning', message, Ext.Msg.OK, null);
		Ext.widget('messagepanel', { title: 'Warning', msg: 'Error saving changes!'});
      }
    });
  },

  onReset: function() {
    var me = this;
    var win = me.up('window');
    win.fireEvent('reset', me);
  }

});