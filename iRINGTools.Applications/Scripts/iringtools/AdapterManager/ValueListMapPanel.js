Ext.ns('AdapterManager');
/**
* @class AdapterManager.ScopePanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
AdapterManager.ValueListMapPanel = Ext.extend(Ext.Panel, {
    layout: 'fit',
    border: false,
    frame: false,
    split: true,
    form: null,
    record: null,
    url: null,
    node: null,

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

        this.bbar = this.buildToolbar();

        var classLabel = '';
        var nodeId = '';
        var classUrl = '';
        var formid = '';
        var scope = '';
        var app = '';
        var interName = '';

        if (this.node != null) {
            nodeId = this.node.id;
            scope = nodeId.split('/')[0];
            app = nodeId.split('/')[1];
            formid = 'valueListMapTarget-' + this.node.parentNode.parentNode.text + '-' + this.node.parentNode.text;
        }

        if (this.record != null && this.node.attributes.type == 'ListMapNode') {
            interName = this.record.internalValue;
            //objectName = scope + '/' + app + '/' + 'DataObjects/DataObject/' + this.record.classTemplateMaps[0].classMap.identifiers[0].replace('.', '/');
            classUrl = this.record.uri;
            classLabel = this.node.text.split('[')[0];
        }

        if (classLabel == '')
            classLab = 'Drop a Class Node here. </div>';
        else
            classLab = 'Class Label: ' + classLabel + '</div>';

        var thisform = new Ext.FormPanel({
            labelWidth: 100, // label settings here cascade unless
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
				{ fieldLabel: 'Mapping Node', name: 'mappingNode', xtype: 'hidden', width: 230, value: nodeId, allowBlank: true },
				{ fieldLabel: 'Internal Name', name: 'internalName', xtype: 'textfield', width: 230, value: interName, allowBlank: false },
                { fieldLabel: 'Class Url', name: 'classUrl', xtype: 'hidden', width: 230, value: classUrl, allowBlank: false },
				{ fieldLabel: 'Old Class Url', name: 'oldClassUrl', xtype: 'hidden', width: 230, value: classUrl, allowBlank: false },
				{ fieldLabel: 'Class Label', name: 'classLabel', xtype: 'hidden', width: 230, value: classLabel, allowBlank: false }
      ],

            html: '<div class="class-target' + formid + '" '
          + 'style="border:1px silver solid;margin:5px;padding:8px;height:20px">'
          + classLab,

            afterRender: function (cmp) {
                Ext.FormPanel.prototype.afterRender.apply(this, arguments);
                var classTarget = this.body.child('div.class-target' + formid);
                var classdd = new Ext.dd.DropTarget(classTarget, {
                    ddGroup: 'refdataGroup',
                    notifyDrop: function (classdd, e, data) {
                        if (data.node.attributes.type != 'ClassNode') {
                            var message = 'Please slect a RDL Class...';
                            showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
                            return false;
                        }
                        thisform.getForm().findField('classUrl').setValue(data.node.attributes.record.Uri);
                        thisform.getForm().findField('classLabel').setValue(data.node.attributes.record.Label);
                        var msg = '<table style="font-size:13px"><tr><td>Class Label:</td><td><b>' + data.node.attributes.record.Label + '</b></td></tr>'
                        msg += '</table>'
                        //Ext.getCmp(formid).body.child('div.class-target' + formid).update(msg);
                        thisform.body.child('div.class-target' + formid).update(msg);
                        return true;
                    } //eo notifyDrop
                }); //eo classdd
            } //eo after render
        }); //eo form

        this.items = [
  		thisform
		];

        this.form = thisform;

        // super
        AdapterManager.ValueListMapPanel.superclass.initComponent.call(this);
    },

    buildToolbar: function () {
        return [{
            xtype: 'tbfill'
        }, {
            xtype: "tbbutton",
            text: 'Ok',
            //icon: 'Content/img/16x16/document-save.png',      
            disabled: false,
            handler: this.onSave,
            scope: this
        }, {
            xtype: "tbbutton",
            text: 'Cancel',
            //icon: 'Content/img/16x16/edit-clear.png',      
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
        var that = this;    // consists the main/previous class object	
        var thisForm = this.form.getForm();

        thisForm.submit({
            success: function (f, a) {
                that.fireEvent('Save', that);
            },
            failure: function (f, a) {
                var message = 'Error saving changes!';
                showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
            }
        });
    }
});