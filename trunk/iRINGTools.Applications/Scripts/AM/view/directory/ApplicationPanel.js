Ext.define('AM.view.directory.ApplicationPanel', {
    extend: 'Ext.window.Window',
    alias: 'widget.applicationform',
    layout: 'fit',
    border: false,
    frame: false,
    from: null,
    record: null,
    node: null,
    height: 261,
    width: 460,
    bodyPadding: 1,
    closable: true,
    autoload: true,

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

        var source = "";
        var showconfigure = "";
        var path = this.path;
        var state = this.state;
        
        var name = "";
        var description = "";
        var dataLayer = "";
        var assembly = '';
        var context = this.record.context;        

        if (this.state == 'edit' && this.record != null) {
            name = this.record.Name;
            description = this.record.Description;
            dataLayer = this.record.DataLayer;
            assembly = this.record.Assembly;
            showconfigure = false;
        }
        else
            showconfigure = true;        

        var cmbDataLayers = Ext.create('Ext.form.ComboBox', {
            fieldLabel: 'Data Layer',
            width: 400,
            editable: false,
            triggerAction: 'all',
            store: new Ext.data.Store({
                model: 'AM.model.DataLayerModel',
                listeners: {
                    load: function () {
                        if (assembly != '') {
                            cmbDataLayers.setValue(assembly);
                        }
                    }
                }
            }),
            displayField: 'name',
            valueField: 'assembly',
            hiddenName: 'Assembly',
            value: assembly,
            listeners: {
                'select': function (combo, record, index) {
                    if (record != null && this.record != null) {
                        this.record.DataLayer = record.data.name;
                        this.record.Assembly = record.data.assembly;
                    }
                }
            }
        });
       
        this.items = [{
            xtype: 'form',
            labelWidth: 100,
            url: 'directory/endpoint',
            method: 'POST',
            bodyStyle: 'padding:10px 5px 0',
            border: false,
            frame: false,
            defaults: {
                width: 400,
                msgTarget: 'side'
            },
            defaultType: 'textfield',
            items: [
              { name: 'path', xtype: 'hidden', value: path, allowBlank: false },
              { name: 'state', xtype: 'hidden', value: state, allowBlank: false },
              { name: 'contextValue', xtype: 'hidden', value: context, allowBlank: false },
              { name: 'assembly', xtype: 'hidden', value: cmbDataLayers.value, allowBlank: false },
              { fieldLabel: 'Endpoint name', name: 'endpoint', xtype: 'textfield', value: name, allowBlank: false },
              { fieldLabel: 'Context name', name: 'context', xtype: 'textfield', value: context, disabled: true },
              { fieldLabel: 'Description', name: 'Description', allowBlank: true, xtype: 'textarea', value: description },
              cmbDataLayers
            ]
        }];

        this.bbar = this.buildToolbar(showconfigure);
        // super
        this.callParent(arguments);
        this.items.first().items.last().store.load();
    },

    buildToolbar: function (showconfigure) {
        return [{
            xtype: 'tbfill'
        }, {
            xtype: "button",
            text: 'Ok',
            disabled: false,
            handler: this.onSave,
            scope: this
        }, {
            xtype: "button",
            text: 'Canel',
            disabled: false,
            handler: this.onReset,
            scope: this
        }]
    },
    onReset: function () {
        this.items.first().getForm().reset();
        this.fireEvent('Cancel', this);
    },

    onSave: function () {
        var me = this;
        var thisForm = this.items.first().getForm();
        var endpointName = thisForm.findField('endpoint').getValue();

        if (ifExistSibling(endpointName, me.node, me.state)) {
            showDialog(400, 100, 'Warning', 'The name \"' + endpointName + '\" already exits in this level, please choose a different name.', Ext.Msg.OK, null);
            return;
        }

        thisForm.submit({
            waitMsg: 'Saving Data...',
            success: function (f, a) {
                me.fireEvent('Save', me);
            },
            failure: function (f, a) {
                var message = 'Error saving changes!';
                showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
            }
        });


    }
});
