Ext.define('AM.view.directory.ScopePanel', {
    extend: 'Ext.window.Window',
    alias: 'widget.scopeform',
    layout: 'fit',
    border: false,
    frame: false,
    from: null,
    record: null,
    url: null,
    height: 180,
    width: 430,
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
            selectionchange: true
        });

        this.bbar = new Ext.toolbar.Toolbar();
        this.bbar.add(this.buildToolbar());

        var name = ""
        var description = ""

        if (this.record != null) {
            name = this.record.Name;
            description = this.record.Description
        }

        this.items = [{
            xtype: 'form',
            labelWidth: 70,
            url: this.url,
            method: 'POST',
            bodyStyle: 'padding:10px 5px 0',
            border: false,
            frame: false,
            defaults: {
                width: 310,
                msgTarget: 'side'
            },
            defaultType: 'textfield',
            items: [
                { fieldLabel: 'Scope', name: 'Scope', xtype: 'hidden', width: 300, value: name, allowBlank: false },
                { fieldLabel: 'Name', name: 'Name', xtype: 'textfield', width: 300, value: name, allowBlank: false },
                { fieldLabel: 'Description', name: 'Description', allowBlank: true, xtype: 'textarea', width: 300, value: description }
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
            text: 'Ok',
            disabled: false,
            handler: this.onSave,
            scope: this
        }, {
            xtype: "button",
            text: 'Cancel',
            //icon: 'Content/img/16x16/edit-clear.png',      
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

        var returnVal = this.checkidNodeExists()

        if (returnVal == true) {
            this.items.first().getForm().submit({
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
    },

    checkidNodeExists: function () {
        var returnVal = true
        var tree = Ext.ComponentQuery.query('directorytree');
        for (var i = 0; i < tree[0].getStore().getRootNode().childNodes.length; i++) {
            if (tree[0].getStore().getRootNode().childNodes[i].text == this.items.first().getForm().getFieldValues().Name) {
                returnVal = false
            }
        }
        return returnVal
    }
});
