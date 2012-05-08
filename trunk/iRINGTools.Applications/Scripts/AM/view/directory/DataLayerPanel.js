﻿Ext.define('AM.view.directory.DataLayerPanel', {
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
              { name: 'state', xtype: 'hidden', value: state, allowBlank: false },
              { fieldLabel: 'Name', name: 'name', xtype: 'textfield', value: name, allowBlank: false },
              { fieldLabel: 'Main DLL', name: 'mainDLL', xtype: 'textfield', value: mainDLL },
              { fieldLabel: 'Package File', name: 'packageFile', allowBlank: true, xtype: 'filefield', value: packageFile }
           ],
            autoDestroy: false

        }];

        // super
        this.callParent(arguments);
    },

    buildToolbar: function () {
        return [{
            xtype: 'tbfill'
        }, {
            xtype: "button",
            text: 'OK',
            disabled: false,
            handler: this.onSave,
            scope: this
        }, {
            xtype: "button",
            text: 'CANCEL',
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
        var thisForm = me.items.items[0].getForm();

        this.items.first().getForm().submit({
            waitMsg: 'Processing ...',            
            success: function (response, request) {
                me.close();
                //var responseObj = Ext.decode(response.responseText);
                //showDialog(300, 100, 'Saving Data Layer ' + responseObj.Level, responseObj.Messages, Ext.Msg.OK, null);
                showDialog(300, 100, 'Saving Data Layer Result', "Succeeded", Ext.Msg.OK, null);
            },
            failure: function (response, request) {
                me.close();
                //var msg = response.responseText;
                //showDialog(300, 100, 'Saving Data Layer Error', msg, Ext.Msg.OK, null);
                showDialog(300, 100, 'Saving Data Layer Result', "Error", Ext.Msg.OK, null);
            }
        });
    }
});



