Ext.define('AM.view.directory.ScopePanel', {
    extend: 'Ext.window.Window',
    alias: 'widget.scopeform',
    layout: 'fit',
    border: false,
    frame: false,
    from: null,
    split: true,
    record: null,
    url: null,
    height: 230,
    width: 460,
    closable: true,
    bodyPadding: 1,
    autoload: true,
    path: null,
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

        var name = ""
        var description = ""

        if (!this.node.parentNode)
            var path = '';
        else
            var path = this.path;

        var state = this.state;
        var context = this.record.context;

        if (this.state == 'edit' && this.record != null) {
            name = this.record.Name;
            description = this.record.Description;
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
              { name: 'path', xtype: 'hidden', value: path, allowBlank: false },
              { name: 'state', xtype: 'hidden', value: state, allowBlank: false },
              { name: 'oldContext', xtype: 'hidden', value: context, allowBlank: false },
              { fieldLabel: 'Folder name', name: 'folderName', xtype: 'textfield', value: name, allowBlank: false },
              { fieldLabel: 'Context name', name: 'contextName', xtype: 'textfield', value: context, allowBlank: false },
              { fieldLabel: 'Description', name: 'Description', allowBlank: true, xtype: 'textarea', value: description }
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

        var thisForm = me.items.items[0].getForm();
        var folderName = thisForm.findField('folderName').getValue();

        if (ifExistSibling(folderName, this.node, this.state)) {
            showDialog(400, 100, 'Warning', 'The name \"' + folderName + '\" already exits in this level, please choose a different name.', Ext.Msg.OK, null);
            return;
        }

        this.items.first().getForm().submit({
            waitMsg: 'Saving Data...',
            success: function (response, request) {
                me.fireEvent('Save', me);                
            },
            failure: function (response, request) {
                var rtext = request.result;
                if (rtext.toUpperCase().indexOf('FALSE') > 0) {
                    var ind = rtext.indexOf('}');
                    var len = rtext.length - ind - 1;
                    var msg = rtext.substring(ind + 1, rtext.length - 1);
                    showDialog(400, 100, 'Error saving endpoint changes', msg, Ext.Msg.OK, null);
                    return;
                }
                
                var message = 'Error saving changes!';
                showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
            }
        });

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

var ifExistSibling = function (str, node, state) {
    var ifExist = false;
    var childNodes = node.childNodes;
    var repeatTime = 0;

    for (var i = 0; i < childNodes.length; i++) {
        if (childNodes[i].data.text == str) {
            if (state == 'new')
                ifExist = true;
            else {
                repeatTime++;
                if (repeatTime > 1)
                    ifExist = true;
            }
        }
    }

    return ifExist;
};

function showDialog(width, height, title, message, buttons, callback) {
    if (message.indexOf('\\r\\n') != -1)
        var msg = message.replace('\\r\\n', '\r\n');
    else
        var msg = message;

    if (msg.indexOf("\\") != -1)
        var msgg = msg.replace(/\\\\/g, "\\");
    else
        var msgg = msg;

    var style = 'style="margin:0;padding:0;width:' + width + 'px;height:' + height + 'px;border:1px solid #aaa;overflow:auto"';
    Ext.Msg.show({
        title: title,
        msg: '<textarea ' + style + ' readonly="yes">' + msgg + '</textarea>',
        buttons: buttons,
        fn: callback
    });
};

