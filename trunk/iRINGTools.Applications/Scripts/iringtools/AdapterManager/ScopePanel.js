Ext.ns('AdapterManager');
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

    data_form: null,
    configData: null,
    url: null,
    nId: null,

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
        this.data_form = new Ext.FormPanel({
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

            items: this.configData,     // binding with the fields list
            buttonAlign: 'left', // buttons aligned to the left            
            autoDestroy: false

        });

        this.items = [
  		this.data_form
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
        if (Ext.getCmp('contentpanel').items.length != 0) { // check is there any tab in contentPanel
            return Ext.getCmp('contentpanel').getActiveTab();
        }
        else {
            return false;
        }
    },

    onCloseTab: function (node) {
        // check number of tabs in panel to make disabled the centerPanel if its the last tab has been closed.
        if ((Ext.getCmp('contentpanel').items.length) == 1) {
            Ext.getCmp('contentpanel').enable()
        }

    },

    onReset: function () {
        this.data_form.getForm().reset()
    },


    onSave: function () {
        var that = this;    // consists the main/previous class object

        this.data_form.getForm().submit({
            waitMsg: 'Saving Data...',
            success: function (f, a) {
                if (that.getActiveTab()) {
                    Ext.Msg.alert('Success', 'Changes saved successfully!')

                    var formType = that.data_form.getForm().findField('formType').getValue();
                    if (formType == 'newForm') { // in case of newForm close the newTab
                        Ext.getCmp('contentpanel').remove(that.getActiveTab(), true);
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
