Ext.ns('AdapterManager');
/**
* @class AdapterManager.ExcelLibraryPanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
AdapterManager.ExcelLibraryPanel = Ext.extend(Ext.Panel, {
    title: 'ExcelLibrary',
    width: 120,

    collapseMode: 'mini',
    //collapsible: true,
    //collapsed: false,
    closable: true,

    layout: 'fit',
    border: true,
    split: true,

    scope: null,
    record: null,
    form: null,
    url: null,

    /**
    * initComponent
    * @protected
    */
    initComponent: function () {

        this.addEvents({
            close: true,
            save: true,
            reset: true,
            validate: true,
            tabChange: true,
            refresh: true,
            selectionchange: true
        });

        this.tbar = this.buildToolbar();

        var form = new Ext.FormPanel({
            renderTo: 'fi-form',
            fileUpload: true,
            width: 500,
            frame: true,
            title: 'File Upload Form',
            autoHeight: true,
            bodyStyle: 'padding: 10px 10px 0 10px;',
            labelWidth: 50,
            defaults: {
                anchor: '95%',
                allowBlank: false,
                msgTarget: 'side'
            },
            items: [{
                xtype: 'textfield',
                fieldLabel: 'Name'
            }, {
                xtype: 'fileuploadfield',
                id: 'form-file',
                emptyText: 'Select an image',
                fieldLabel: 'Photo',
                name: 'photo-path',
                buttonText: '',
                buttonCfg: {
                    iconCls: 'upload-icon'
                }
            }]            
        });

        this.items = [
  		    this.form
  	    ];

        this.on('close', this.onCloseTab, this)

        // super
        AdapterManager.ExcelLibraryPanel.superclass.initComponent.call(this);

        var data = dataLayersStore.getById(this.record.DataLayerName);
        cmbDataLayers.Value = data;

    },

    buildToolbar: function () {
        return [{
            xtype: "tbbutton",
            text: 'Save',
            icon: 'resources/images/16x16/document-save.png',
            tooltip: 'Save',
            disabled: false,
            handler: this.onSave,
            ExcelLibrary: this
        }, {
            xtype: "tbbutton",
            text: 'Clear',
            icon: 'resources/images/16x16/edit-clear.png',
            tooltip: 'Clear',
            disabled: false,
            handler: this.onReset,
            ExcelLibrary: this
        }]
    },
    getActiveTab: function () {
        if (Ext.getCmp('content-panel').items.length != 0) { // check is there any tab in contentPanel
            return Ext.getCmp('content-panel').getActiveTab();
        }
        else {
            return false;
        }
    },

    onCloseTab: function (node) {
        // check number of tabs in panel to make disabled the centerPanel if its the last tab has been closed.
        if ((Ext.getCmp('content-panel').items.length) == 1) {
            Ext.getCmp('content-panel').enable()
        }

    },

    onReset: function () {
        this.form.getForm().reset()
    },


    onSave: function () {
        var that = this;    // consists the main/previous class object
                
        this.form.getForm().submit({
            waitMsg: 'Saving Data...',
            url: this.url,
            success: function (f, a) {
                if (that.getActiveTab()) {
                    Ext.Msg.alert('Success', 'Processed file "' + o.result.file + '" on the server');
                }

            },
            failure: function (f, a) {
                Ext.Msg.alert('Warning', 'Error processing file!')
            }
        });

    }

});
