Ext.ns('AdapterManager');

AdapterManager.DataLayerPanel = Ext.extend(Ext.Window,
{   
  node: null,
  record: null, 
  url: null, 
  layout: 'fit',
  frame: true,
  height: 180,
  width: 400,
  modal: true,
  closable: true,
  sizable: false,

  initComponent: function () {
    this.addEvents({
      cancel: true,
      save: true,
    });

    this.title = 'Add/Edit DataLayer';
    this.bbar = this.buildToolbar();

    var name = '';
    var packageFile = '';

    this.items = [{
      xtype: 'form',
      url: this.url,
      fileUpload: true,
      bodyStyle: 'padding:40px 5px 5px 5px',
      labelWidth: 70,
      defaultType: 'textfield',
      defaults: {
        width: 280,
        allowBlank: true
      },
      items: [
        //{ fieldLabel: 'Display Name', name: 'name', xtype: 'textfield', value: name },
        { fieldLabel: 'Package File', name: 'packageFile', allowBlank: false, xtype: 'fileuploadfield', value: packageFile }
      ]
    }];
    
    AdapterManager.DataLayerPanel.superclass.initComponent.call(this);
  },

  buildToolbar: function () {
    return [{
      xtype: 'tbfill'
    }, {
      xtype: "button",
      text: 'Upload',
      handler: this.onUpload,
      scope: this
    }, {
      xtype: "button",
      text: 'Cancel',
      handler: this.onCancel,
      scope: this
    }]
  },

  onCancel: function () {
    this.close();
  },

  onUpload: function () {
    var me = this;
    this.items.first().getForm().submit({
      waitMsg: 'Processing ...',
      success: function (response, request) {
        me.close();
        showDialog(400, 180, 'DataLayer Upload Result', request.result.messages[0], Ext.Msg.OK, null);
      },
      failure: function (response, request) {
        me.close();

        if (request.result.level == 0) {
          showDialog(400, 180, 'DataLayer Upload Result', request.result.messages[0], Ext.Msg.OK, null);
        }
        else {
          showDialog(400, 180, 'DataLayer Upload Result', request.result.messages[0], Ext.Msg.OK, null);
        }
      }
    });
  }
});



