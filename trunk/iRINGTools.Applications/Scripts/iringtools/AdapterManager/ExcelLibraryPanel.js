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

    layout: 'card',
    activeItem: 0,
    border: true,
    split: true,

    scope: null,
    application: null,
    form: null,
    url: null,

    btnNext: null,
    btnPrev: null,

    /**
    * initComponent
    * @protected
    */
    initComponent: function () {

        this.addEvents({
            save: true,
            reset: true,
            validate: true,
            refresh: true,
            selectionchange: true
        });

        var scope = "";

        if (this.scope != null) {
            scope = this.scope.Name;
        }

        var application = "";

        if (this.application != null) {
            application = this.application.Name;
        }

        this.tbar = this.buildToolbar();

        this.btnNext = new Ext.Button({
            text: 'Next',
            //icon: 'resources/images/16x16/document-save.png',
            //tooltip: 'Save',
            disabled: false,
            handler: this.onNavigation.createDelegate(this, [1]),
            scope: this
        });

        this.btnPrev = new Ext.Button({
            text: 'Prev',
            //icon: 'resources/images/16x16/document-save.png',
            //tooltip: 'Save',
            disabled: false,
            handler: this.onNavigation.createDelegate(this, [-1]),
            scope: this
        });

        this.bbar = new Ext.Toolbar();
        this.bbar.addButton(new Ext.Toolbar.Fill());
        this.bbar.addButton(this.btnPrev);
        this.bbar.addButton(this.btnNext);

        this.form = new Ext.FormPanel({
            fileUpload: true,
            labelWidth: 150, // label settings here cascade unless
            url: this.url,
            method: 'POST',
            bodyStyle: 'padding:10px 5px 0',

            border: false, // removing the border of the form

            frame: true,
            closable: true,
            defaults: {
                width: 330,
                msgTarget: 'side'
            },
            defaultType: 'textfield',

            items: [
                { fieldLabel: 'Scope', name: 'Scope', xtype: 'hidden', width: 250, value: scope, allowBlank: false },
                { fieldLabel: 'Application', name: 'Application', xtype: 'hidden', width: 250, value: application, allowBlank: false },
                {
                    xtype: 'fileuploadfield',
                    emptyText: 'Select an Excel Source File',
                    fieldLabel: 'Excel Source File',
                    name: 'source-path',
                    buttonText: '',
                    buttonCfg: {
                        iconCls: 'upload-icon'
                    }
                },
                { 
                    xtype: 'button',
                    text: 'Upload',
                    handler: this.onUpload,
                    scope: this
                }
            ],
            buttonAlign: 'left', // buttons aligned to the left            
            autoDestroy: false

        });

        this.items = [
            this.form,
            {
                id: 'card-1',
                html: 'Step 2'
            }, {
                id: 'card-2',
                html: 'Step 3'
            }
        ];

        // super
        AdapterManager.ExcelLibraryPanel.superclass.initComponent.call(this);
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
    },

    onUpload: function () {
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
    },

    onNavigation: function (direction) {
        var idx = this.activeItem + direction;

        if (idx == 0) {
            this.btnPrev.disable();
        } else if (idx == this.items.getCount() - 1) {
            this.btnNext.disable();
        } else {
            this.btnNext.enable();
            this.btnPrev.enable();
        }

        this.layout.setActiveItem(idx);
        this.activeItem = idx;
    }

});
