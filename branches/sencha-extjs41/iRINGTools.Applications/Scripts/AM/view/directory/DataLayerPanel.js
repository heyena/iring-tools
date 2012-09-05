Ext.define('AM.view.directory.DataLayerPanel', {
    extend: 'Ext.window.Window',
    alias: 'widget.datalayerform',
    layout: 'fit',
    border: false,
    frame: false,
    from: null,
    split: true,
    record: null,
    url: null,
    height: 180,
    width: 460,
    closable: true,
    bodyPadding: 1,
    autoload: true,
    state: null,
    node: null,

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

        this.bbar = new Ext.toolbar.Toolbar();
        this.bbar.add(this.buildToolbar());

        var state = this.state;
        var name = '';
        var mainDLL = '';
        var packageFile = '';

        if (this.state == 'edit' && this.record != null) {
            name = this.record.Name;
            mainDLL = this.record.MainDLL;
        }

        this.items = [{
            xtype: 'form',
            labelWidth: 100,
            url: this.url,
            bodyStyle: 'padding:10px 5px 0',
            border: false,
            frame: false,
            defaults: {
                width: 400,
                msgTarget: 'side'
            },
            defaultType: 'textfield',
            items: [
              { name: 'state', xtype: 'hidden', value: state, allowBlank: false },
              { fieldLabel: 'Name', name: 'name', xtype: 'textfield', value: name, allowBlank: false },
              { fieldLabel: 'Main DLL', name: 'mainDLL', xtype: 'textfield', value: mainDLL },
              { fieldLabel: 'Package File', name: 'packageFile', allowBlank: true, xtype: 'filefield', value: packageFile }
           ]
        }];

        this.callParent(arguments);
    },

    buildToolbar: function () {
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
            text: 'Cancel',
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
        this.items.first().getForm().submit({
            waitMsg: 'Processing ...',
            success: function (response, request) {
                me.close();
                showDialog(320, 80, 'Saving Data Layer Result', request.result.messages[0], Ext.Msg.OK, null);
            },
            failure: function (response, request) {
                me.close();

                if (request.result.level == 0) {
                    showDialog(320, 80, 'Saving Data Layer Result', request.result.messages[0], Ext.Msg.OK, null);
                }
                else {
                    showDialog(600, 300, 'Saving Data Layer Result', request.result.messages[0], Ext.Msg.OK, null);
                }
            }
        });
    }
});



