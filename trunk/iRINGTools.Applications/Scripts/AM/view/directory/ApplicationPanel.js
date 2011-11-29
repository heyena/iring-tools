Ext.define('AM.view.directory.ApplicationPanel', {
    extend: 'Ext.window.Window',
    alias: 'widget.applicationform',
    layout: 'fit',
    border: false,
    frame: false,
    from: null,    
    record: null,
    height: 230,
    width: 460,
    closable: true,
    bodyPadding: 10,
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
        var id = "";
        var path = this.path;
        var state = this.state;

        if (this.id != null) {
          id = this.id;
        }

        var name = "";
        var description = "";
        var dataLayer = "";
        var assembly = "";
        var context = this.record.context;

        if (this.state == 'edit' && this.record != null) {
          name = this.record.Name;
          description = this.record.Description;
          dataLayer = this.record.DataLayer;
          assembly = this.record.Assembly;
        }
        
        var combostore = Ext.create('Ext.data.Store', {
            model: 'AM.model.DataLayerModel'
        });

        this.items = [{
            xtype: 'form',
            labelWidth: 70,
            url: 'directory/endpoint',
            method: 'POST',
            border: false,
            frame: false,
            layout: 'anchor',
            defaults: {
                width: 310,
                msgTarget: 'side'
            },
            defaultType: 'textfield',
            items: [
              { name: 'path', xtype: 'hidden', value: path, allowBlank: false },
              { name: 'state', xtype: 'hidden', value: state, allowBlank: false },
              { fieldLabel: 'Endpoint name', name: 'Name', xtype: 'textfield', value: name, allowBlank: false },
              { fieldLabel: 'Context name', name: 'contextName', xtype: 'textfield', value: context, disabled: true },
              { fieldLabel: 'Description', name: 'Description', allowBlank: true, xtype: 'textarea', value: description },
              { xtype: 'combo', name: 'assembly', fieldLabel: 'Data Layer', width: 250, store: combostore, displayField: 'Name', valueField: 'Assembly', hiddenName: 'Assembly', value: assembly, queryMode: 'local' }
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
        var endpointName = thisForm.findField('Name').getValue();

        if (ifExistSibling(endpointName, me.node, me.state)) {
          showDialog(400, 100, 'Warning', 'The name \"' + endpointName + '\" already exits in this level, please choose a different name.', Ext.Msg.OK, null);
          return;
        }

        if (this.record.context != this.form.getForm().getFieldValues().Name) {
            form.submit({
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
        else {
            var message = 'Scope & Application name cannot be same!';
            showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
        }
    }
});
