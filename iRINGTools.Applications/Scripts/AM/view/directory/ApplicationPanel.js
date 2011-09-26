Ext.define('AM.view.directory.ApplicationPanel', {
    extend: 'Ext.window.Window',
    alias: 'widget.applicationform',
    layout: 'fit',
    border: false,
    frame: false,
    from: null,
    scope: null,
    record: null,
    height: 200,
    width: 330,
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

        var scope = "";
        var showconfigure = "";

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
            showconfigure = false;
        }
        else {
            showconfigure = true;
        }
        var combostore = Ext.create('Ext.data.Store', {
            model: 'AM.model.DataLayerModel'
        });

        this.items = [{
            xtype: 'form',
            labelWidth: 70,
            url: 'directory/application',
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
              { fieldLabel: 'Scope', name: 'Scope', xtype: 'hidden', width: 300, value: scope, allowBlank: false },
              { fieldLabel: 'Application', name: 'Application', xtype: 'hidden', width: 300, value: name, allowBlank: false },
              { fieldLabel: 'Name', name: 'Name', xtype: 'textfield', width: 300, value: name, allowBlank: false },
              { fieldLabel: 'Description', name: 'Description', allowBlank: true, xtype: 'textarea', width: 300, value: description },
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
        var form = this.items.first().getForm();
        if (form.getFieldValues().Scope != form.getFieldValues().Name) {
            form.submit({
                waitMsg: 'Saving Data...',
                success: function (f, a) {
                    me.fireEvent('Save', me);
                },
                failure: function (f, a) {
                    var message = 'Error saving changes!';
                   // showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
                }
            });
        }
        else {
            var message = 'Scope & Application name cannot be same!';
            showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
        }
    }
});
