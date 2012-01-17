Ext.define('AM.view.spreadsheet.SourcePanel', {
    extend: 'Ext.form.Panel',
    alias: 'widget.sourcepanel',
    width: 120,
    layout: 'fit',

    frame: true,
    border: true,

    fileUpload: true,
    labelWidth: 150,   
    method: 'POST',
    bodyStyle: 'padding:10px 5px 0',

    border: false, 
    defaults: {
        width: 330,
        msgTarget: 'side'
    },
    defaultType: 'textfield',
    buttonAlign: 'left',              
    autoDestroy: true,

    scope: null,
    application: null,
    dataLayer: null,
    assembly: null,

    initComponent: function () {

        this.addEvents({
            uploaded: true
        });

        var scope = "";

        if (this.scope != null) {
            scope = this.scope;
        }

        var application = "";
        var dataLayer = "";
        var assembly = "";

        if (this.application != null) {
            application = this.application
            dataLayer = this.dataLayer;
        }

        this.bbar = [
          '->',
          { xtype: 'button', text: 'Upload', scope: this, handler: this.onUpload },
          { xtype: 'button', text: 'Cancel', scope: this, handler: this.onCancel }
        ]

        this.items = [
            { xtype: 'hidden', name: 'Scope', value: this.Scope },
            { xtype: 'hidden', name: 'Application', value: this.Application },
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
        ];
        this.callParent(arguments);
    },

    onUpload: function () {
        that = this;
        this.getForm().submit({
            waitMsg: 'Uploading file...',
            url: this.url,
            method: 'POST',
            success: function (f, a) {
                that.fireEvent('Uploaded', that, f.items.items[3].value);
            },
            failure: function (f, a) {
                Ext.Msg.alert('Warning', 'Error uploading file "' + f.items.items[3].value + '"!');
            }
        });
    },
    onCancel: function (b) {
        this.getForm().reset();
        this.findParentByType('panel').hide();
    }
});
