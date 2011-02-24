﻿Ext.ns('AdapterManager');
/**
* @class AdapterManager.ScopePanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
AdapterManager.ScopePanel = Ext.extend(Ext.Panel, {
    title: 'Scope',
    width: 120,

    collapseMode: 'mini',
    //collapsible: true,
    //collapsed: false,
    closable: true,

    layout: 'fit',
    border: true,
    split: true,

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

        var name = ""
        var description = ""

        if (this.record != null) {
            name = this.record.Name;
            description = this.Description
        }

        this.form = new Ext.FormPanel({
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
                { fieldLabel: 'Name', name: 'Name', xtype: 'textfield', width: 250, value: name, allowBlank: false },
                { fieldLabel: 'Description', name: 'Description', allowBlank: true, xtype: 'textarea', width: 250, value: description }
            ],
            buttonAlign: 'left', // buttons aligned to the left            
            autoDestroy: false

        });

        this.items = [
  		    this.form
  	    ];

        this.on('close', this.onCloseTab, this)

        // super
        AdapterManager.ScopePanel.superclass.initComponent.call(this);
    },

    buildToolbar: function () {
        return [{
            xtype: "tbbutton",
            text: 'Save',
            icon: 'resources/images/16x16/document-save.png',
            tooltip: 'Save',
            disabled: false,
            handler: this.onSave,
            scope: this
        }, {
            xtype: "tbbutton",
            text: 'Clear',
            icon: 'resources/images/16x16/edit-clear.png',
            tooltip: 'Clear',
            disabled: false,
            handler: this.onReset,
            scope: this
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
            success: function (f, a) {
                if (that.getActiveTab()) {
                    Ext.Msg.alert('Success', 'Changes saved successfully!')

                    var formType = that.data_form.getForm().findField('formType').getValue();
                    if (formType == 'newForm') { // in case of newForm close the newTab
                        Ext.getCmp('content-panel').remove(that.getActiveTab(), true);
                    }

                    var tempPanel = Ext.getCmp('nav-panel'); // Get Directory Panel
                    // Ext.state.Manager.clear('AdapterManager'); 
                    tempPanel.DirectoryPanel.root.reload();
                }

            },
            failure: function (f, a) {
                Ext.Msg.alert('Warning', 'Error saving changes!')
            }
        });

    }

});
