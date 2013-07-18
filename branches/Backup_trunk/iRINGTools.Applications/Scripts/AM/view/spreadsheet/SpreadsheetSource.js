Ext.define('AM.view.spreadsheet.SpreadsheetSource', {
  extend: 'Ext.window.Window',
  alias: 'widget.spreadsheetsource',
  layout: 'fit',
  border: false,
  width: 420,
  height: 130,
  frame: false,
  context: null,
  endpoint: null,
  baseurl: null,
  dataLayer: null,
  bodyPadding: 1,
  assembly: null,
  closable: true,
  initComponent: function () {

    this.items = [
            {
              xtype: 'form',
              border: false,
              frame: false,
              fileUpload: true,
              method: 'POST',
              defaults: {
                anchor: '100%',
                msgTarget: 'side'
              },
              defaultType: 'textfield',
              buttonAlign: 'left',
              autoDestroy: true,
              bodyStyle: 'padding:10px 5px 0',
              bbar: [
                  '->',
                  {
                    xtype: 'button',
                    text: 'Upload',
                    scope: this,
                    handler: this.onUpload,
                    icon: 'Content/img/16x16/document-up.png'
                  }
              //                  , {
              //                    xtype: 'button',
              //                    text: 'Cancel',
              //                    scope: this,
              //                    handler: this.onReset 
              //                  }
               ],
              items: [
                    { xtype: 'hidden', name: 'context', value: this.context },
                    { xtype: 'hidden', name: 'endpoint', value: this.endpoint },
                    { xtype: 'hidden', name: 'baseurl', value: this.baseurl },
                    { xtype: 'hidden', name: 'DataLayer', value: this.dataLayer },
                    {
                      xtype: 'filefield',
                      name: 'SourceFile',
                      emptyText: 'Select a Spreadsheet',
                      labelWidth: 150,
                      fieldLabel: 'Spreadsheet Source',
                      buttonText: '',
                      buttonConfig: {
                        iconCls: 'upload-icon'
                      }
                    },
                    { xtype: 'checkbox', labelWidth: 150, name: 'Generate', fieldLabel: 'Generate Configuration', checked: true }
                ],
                listeners: {
                    uploaded: true
                }
            }

        ];
    this.callParent(arguments);
  },
  onUpload: function () {
    var me = this;
    var frm = this.items.items[0];
    frm.getForm().submit({
      waitMsg: 'Uploading file...',
      url: this.url,
      method: 'POST',
      success: function (f, a) {
        me.hide();
        var tabId = 'tab-c.' + me.context + '.' + me.endpoint;
        Ext.getCmp(tabId).items.items[0].getStore().load();
      },
      failure: function (f, a) {
        Ext.Msg.alert('Warning', 'Error uploading file "' + f.items[3] + '"!');
      }

    });
  },

  onReset: function () {
    var form = this.items.items[0];
    form.getForm().reset();
    this.fireEvent('Cancel', this);
  }
});