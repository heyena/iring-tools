Ext.ns('AdapterManager');
/**
* @class AdapterManager.ApplicationPanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
AdapterManager.ApplicationPanel = Ext.extend(Ext.Panel, {
    layout: 'fit',
    border: false,
    frame: false,
    split: false,

    scope: null,
    record: null,
    id: null,
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

        var scope = "";
        var showconfigure = "";
        var id = "";
        var path

        if (this.scope != null) {
            scope = this.scope.Name;
        }

        if (this.id != null) {
            id = this.id;
        }

        var name = "";
        var description = "";
        var dataLayer = "";
        var assembly = "";
        var state = 'add';

        if (this.record != null) {
            name = this.record.Name;
            description = this.record.Description;
            dataLayer = this.record.DataLayer;
            assembly = this.record.Assembly;
            showconfigure = false;
            state = 'update';
        }
        else {
            showconfigure = true;
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
                { name: 'name', mapping: 'Name', allowBlank: false },
                { name: 'configurable', mapping: 'Configurable', allowBlank: false }
            ]
        });

        var panel = Ext.getCmp(id);
        dataLayersStore.on('beforeload', function (store, options) {
            panel.body.mask('Loading...', 'x-mask-loading');
        }, this);

        dataLayersStore.on('load', function (store, records, options) {
            panel.body.unmask();
        }, this);

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
            if (record != null && this.record != null) {
                this.record.DataLayer = record.data.name;
                this.record.Assembly = record.data.assembly;
            }
        }, this);

        //that = this;

        this.form = new Ext.FormPanel({
            labelWidth: 70, // label settings here cascade unless
            url: this.url,
            method: 'POST',
            bodyStyle: 'padding:10px 5px 0',

            border: false, // removing the border of the form

            frame: false,
            closable: true,
            defaults: {
                width: 310,
                msgTarget: 'side'
            },
            defaultType: 'textfield',

            items: [
          { name: 'State', xtype: 'hidden', value: state, allowBlank: false },
          { name: 'Scope', xtype: 'hidden', value: scope, allowBlank: false },
          { name: 'Path', xtype: 'hidden', value: path, allowBlank: false },
          { fieldLabel: 'Application', name: 'Application', xtype: 'hidden', width: 300, value: name, allowBlank: false },
          { fieldLabel: 'Name', name: 'Name', xtype: 'textfield', width: 300, value: name, allowBlank: false },
          { fieldLabel: 'Description', name: 'Description', allowBlank: true, xtype: 'textarea', width: 300, value: description },
          cmbDataLayers
      ],
            buttonAlign: 'left', // buttons aligned to the left            
            autoDestroy: false
        });

        this.items = [
  		    this.form
  	    ];

        this.bbar = this.buildToolbar(showconfigure);

        // super
        AdapterManager.ApplicationPanel.superclass.initComponent.call(this);

        //var data = dataLayersStore.getById(dataLayer);
        //cmbDataLayers.Value = data;

    },

    buildToolbar: function (showconfigure) {
        return [{
            xtype: 'tbfill'
        }, {
            xtype: "tbbutton",
            text: 'Ok',
            //icon: 'Content/img/16x16/document-save.png',
            //tooltip: 'Save',
            disabled: false,
            handler: this.onSave,
            scope: this
        }, {
            xtype: "tbbutton",
            text: 'Cancel',
            //icon: 'Content/img/16x16/edit-clear.png',
            //tooltip: 'Clear',
            disabled: false,
            handler: this.onReset,
            scope: this
        }]
    },

    onReset: function () {
        this.form.getForm().reset();
        this.fireEvent('Cancel', this);
    },

    onSave: function () {
        var that = this;    // consists the main/prappNameclass object       
        if (this.form.getForm().getFieldValues().Scope != this.form.getForm().getFieldValues().Name) {
            this.form.getForm().submit({
                waitMsg: 'Saving Data...',
                success: function (f, a) {
                    that.fireEvent('Save', that);
                },
                failure: function (f, a) {
                    //Ext.Msg.alert('Warning', 'Error saving changes!')
                    var message = 'Error saving changes!';
                    showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
                }
            });
        }
        else {
            var message = 'Scope & Application name cannot be same!';
            showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
        }
    }

});

