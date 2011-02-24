Ext.ns('AdapterManager');
/**
* @class AdapterManager.ApplicationPanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
AdapterManager.ApplicationPanel = Ext.extend(Ext.Panel, {
    title: 'Application',
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
            selectionchange: true,
            ConfigureApplication: true
        });

        this.tbar = this.buildToolbar();

        var scope = ""

        if (this.scope != null) {
            scope = this.scope.Name;
        }

        var name = ""
        var description = ""

        if (this.record != null) {
            name = this.record.Name;
            description = this.Description
        }

        var dataLayersStore = new Ext.data.JsonStore({
            // store configs            
            autoLoad: true,
            autoDestroy: true,
            url: 'directory/dataLayers',
            // reader configs
            root: 'items',
            idProperty: 'name',
            fields: [
                { name: 'assembly', mapping: 'Assembly', allowBlank: false },
                { name: 'name', mapping: 'Name', allowBlank: false }
            ]
        });

        var cmbDataLayers = new Ext.form.ComboBox({
            fieldLabel: 'Data Layer',
            name: 'DataLayer',
            boxMaxWidth: 250,
            width: 250,
            forceSelection: true,
            typeAhead: true,
            triggerAction: 'all',
            lazyRender: true,
            mode: 'local',
            store: dataLayersStore,
            valueField: 'name',
            displayField: 'name',
            value: this.record.DataLayerName
        });

        that = this;

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
                { fieldLabel: 'Description', name: 'Description', allowBlank: true, xtype: 'textarea', width: 250, value: description },
                cmbDataLayers
            ],
            buttonAlign: 'left', // buttons aligned to the left            
            autoDestroy: false

        });

        this.items = [
  		    this.form
  	    ];

        this.on('close', this.onCloseTab, this)

        // super
        AdapterManager.ApplicationPanel.superclass.initComponent.call(this);

        var data = dataLayersStore.getById(this.record.DataLayerName);
        cmbDataLayers.Value = data;

    },

    buildToolbar: function () {
        return [{
            xtype: "tbbutton",
            text: 'Configure',
            icon: 'resources/images/16x16/document-save.png',
            tooltip: 'Configure',
            disabled: false,
            handler: this.onConfigure,
            Application: this
        }, {
            xtype: "tbbutton",
            text: 'Save',
            icon: 'resources/images/16x16/document-save.png',
            tooltip: 'Save',
            disabled: false,
            handler: this.onSave,
            Application: this
        }, {
            xtype: "tbbutton",
            text: 'Clear',
            icon: 'resources/images/16x16/edit-clear.png',
            tooltip: 'Clear',
            disabled: false,
            handler: this.onReset,
            Application: this
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

    onConfigure: function () {
        //this.fireEvent('ConfigureApplication', this, this.scope, this.record);

        var newTab = new AdapterManager.ExcelLibraryPanel({
            id: 'tab-c.',// + this.scope.Name + '.' + this.record.Name,
            title: 'Configure - ',// + this.scope.Name + '.' + this.record.Name,
            scope: this.scope,
            record: this.record,
            url: 'directory/excellibrary',
            closable: true
        });

        var contentPanel = Ext.getCmp('content-panel');
        contentPanel.add(newTab);
        contentPanel.activate(newTab);

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