Ext.define('AM.view.directory.ValuelistPanel', {
    extend: 'Ext.window.Window',
    alias: 'widget.valuelistpanel',
    layout: 'fit',
    height: 120,
    width: 430,
    border: false,
    frame: false,
    split: true,
    record: null,
    url: null,
    nodeId: null,
    closable: true,
    bodyPadding: 10,
    iconCls: 'tabsValueList',
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
            selectionchange: true
        });

        this.bbar = new Ext.toolbar.Toolbar();
        this.bbar.add(this.buildToolbar());

        var name = "";
        var nodeId = "";

        if (this.record != null) {
            name = this.record.name;
        }

        if (this.nodeId != null) {
            nodeId = this.nodeId;
        }

        this.items = [{
            xtype: 'form',
            labelWidth: 100, 
      //      layout: 'fit',
            url: 'mapping/valueList',
            method: 'POST',
            bodyStyle: 'padding:10px 5px 0',
            border: false,
            frame: false,
            defaults: {
                msgTarget: 'side'
            },
            defaultType: 'textfield',
            items:
             [{
                 fieldLabel: 'Mapping Node',
                 name: 'mappingNode',
                 xtype: 'hidden',
                 width: 120,
                 value: nodeId,
                 allowBlank: false
             }, {
                 fieldLabel: 'Value List Name',
                 name: 'valueList',
                 xtype: 'textfield',
                 width: 350,
                 value: name,
                 allowBlank: false
             }],
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
            //icon: 'Content/img/16x16/document-save.png',      
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
        var me = this;    // consists the main/previous class object
        var thisForm = this.items.first().getForm();
        if (thisForm.findField('valueList').getValue() == '') {
           
            return;
        }
        thisForm.submit({
            waitMsg: 'Saving Data...',
            success: function (f, a) {
                //Ext.Msg.alert('Success', 'Changes saved successfully!');
                me.fireEvent('Save', me);
            },
            failure: function (f, a) {
                //Ext.Msg.alert('Warning', 'Error saving changes!')
                var message = 'Error saving changes!';
                showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
            }
        });
    }
});
