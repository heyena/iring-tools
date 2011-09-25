Ext.define('AM.view.directory.SpreadsheetPanel', {
    extend: 'Ext.window.Window',
    alias: 'widget.spreadsheetpanel',
    layout: 'fit',
    border: true,
    frame: false,
    scope: null,
    application: null,
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
                  { xtype: 'button', text: 'Upload', scope: this, action: 'xlsupload' },
                  { xtype: 'button', text: 'Cancel', scope: this, action: 'xlscancel' }
               ],
               items: [
                    { xtype: 'hidden', name: 'Scope', value: this.scope },
                    { xtype: 'hidden', name: 'Application', value: this.application },
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
                    { xtype: 'checkbox', name: 'Generate', boxLabel: 'Generate Configuration' }
                ],
                listeners: {
                    uploaded: true
                }
            }
           
        ];
        this.callParent(arguments);
    }
});