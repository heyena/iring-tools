Ext.define('AM.view.spreadsheet.SpreadsheetSource', {
    extend: 'Ext.window.Window',
    alias: 'widget.spreadsheetsource',
    layout: 'fit',
    border: true,
    frame: false,
    context: null,
    endpoint: null,
    dataLayer: null,
    assembly: null,
    initComponent: function () {

        this.items = [
            {
                xtype: 'form',
                frame: true,
                border: true,
                fileUpload: true,
                labelWidth: 150,
                method: 'POST',
                defaults: {
                    width: 330,
                    msgTarget: 'side'
                },
                defaultType: 'textfield',
                buttonAlign: 'left',
                autoDestroy: true,
                bodyStyle: 'padding:10px 5px 0',
                bbar: [
                  '->',
                  { xtype: 'button', text: 'Upload', scope: this, handler: this.onUpload },
                  { xtype: 'button', text: 'Cancel', scope: this, handler: this.onReset }
               ],
                items: [
                    { xtype: 'hidden', name: 'contextName', value: this.context },
                    { xtype: 'hidden', name: 'endpoint', value: this.endpoint },
                    { xtype: 'hidden', name: 'DataLayer', value: this.datalayer },
                    {
                        xtype: 'fileuploadfield',
                        name: 'SourceFile',
                        emptyText: 'Select an Spreadsheet Source File',
                        fieldLabel: 'Spreadsheet Source File',
                        buttonText: '',
                        buttonCfg: {
                            iconCls: 'upload-icon'
                        }
                    },
                    { xtype: 'checkbox', name: 'Generate', boxLabel: 'Generate Configuration', checked: true }
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