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
            configure: true
        });

        this.tbar = this.buildToolbar();

        var scope = "";

        if (this.scope != null) {
            scope = this.scope.Name;
        }

        var name = "";
        var description = "";
        var dataLayer = "";
        var assembly = "";

        if (this.record != null) {
                name = this.record.Name;
                description = this.record.Description;
                dataLayer = this.record.DataLayer;
                assembly = this.record.Assembly;
      }

        var dataLayersStore = new Ext.data.JsonStore({
            // store configs            
            autoDestroy: true,
            url: 'directory/dataLayers',
            // reader configs
            root: 'items',
            idProperty: 'assembly',
            fields: [
                { name: 'assembly', mapping: 'Assembly', allowBlank: false },
                { name: 'name', mapping: 'Name', allowBlank: false }
            ]
        });

        var cmbDataLayers = new Ext.form.ComboBox({
            fieldLabel: 'Data Layer',
            boxMaxWidth: 250,
            width: 250,
            forceSelection: true,
            typeAhead: true,
            triggerAction: 'all',
            lazyRender: true,
            //mode: 'remote',
            store: dataLayersStore,
            displayField: 'name',
            valueField: 'assembly',
            hiddenName: 'Assembly',
            value: assembly
        });

        cmbDataLayers.on('select', function (combo, record, index) {
            this.record.DataLayer = record.data.name;
            this.record.Assembly = record.data.assembly;
        }, this);

        //that = this;

        this.form = new Ext.FormPanel({
            labelWidth: 150, // label settings here cascade unless
            url: this.url,
            method: 'POST',
            bodyStyle: 'padding:10px 5px 0',

            border: false, // removing the border of the form

            frame: true,
            closable: true,
            defaults: {
                width: 250,
                msgTarget: 'side'
            },
            defaultType: 'textfield',

            items: [
        { fieldLabel: 'Scope', name: 'Scope', xtype: 'hidden', width: 250, value: scope, allowBlank: false },
        { fieldLabel: 'Application', name: 'Application', xtype: 'hidden', width: 250, value: name, allowBlank: false },
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

        this.on('close', this.onCloseTab, this);

        // super
        AdapterManager.ApplicationPanel.superclass.initComponent.call(this);

        //var data = dataLayersStore.getById(dataLayer);
        //cmbDataLayers.Value = data;

    },

    buildToolbar: function () {
        return [{
            xtype: "tbbutton",
            text: 'Configure',
            //icon: 'Content/img/16x16/document-save.png',
            tooltip: 'Configure',
            disabled: false,
            handler: this.onConfigure,
            scope: this
        }, {
            xtype: "tbbutton",
            text: 'Save',
            icon: 'Content/img/16x16/document-save.png',
            tooltip: 'Save',
            disabled: false,
            handler: this.onSave,
            scope: this
        }, {
            xtype: "tbbutton",
            text: 'Clear',
            icon: 'Content/img/16x16/edit-clear.png',
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
        this.form.getForm().reset();
    },

    onConfigure: function () {
        var appName = this.form.getForm().getFieldValues().Name;
        this.fireEvent('configure', this, this.scope, this.record, appName);
    },

    onSave: function () {
        var that = this;    // consists the main/prappNameclass object      
        this.form.getForm().submit({
            waitMsg: 'Saving Data...',
            success: function (f, a) {
                var record = f.getFieldValues();

                that.record = record;

                //that.record.Name = record.Name;
                //that.record.Description = record.Description;

                if (that.getActiveTab()) {
                    Ext.Msg.alert('Success', 'Changes saved successfully!');
                    that.fireEvent('Save', that);
                }
            },
            failure: function (f, a) {
                Ext.Msg.alert('Warning', 'Error saving changes!')
            }
        });

    }

});