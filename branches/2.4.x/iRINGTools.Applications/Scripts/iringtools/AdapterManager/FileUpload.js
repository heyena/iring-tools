Ext.ns('AdapterManager');
/**
* @class AdapterManager.ScopePanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
AdapterManager.FileUpload = Ext.extend(Ext.Panel, {
    layout: 'fit',
    border: false,
    frame: false,
    split: true,
    from: null,
    record: null,
    url: null,

    /**
    * initComponent
    * @protected
    */
    initComponent: function () {

        this.addEvents({
            close: true,
            save: true,
            reset: true
        });
	   this.bbar = this.buildToolbar();
        var scope = "";
        var application = "";

        if (this.record != null) {
            scope = this.record.parentNode.text;
            application = this.record.text;
        }
	   this.form = new Ext.FormPanel({
            labelWidth: 40, // label settings here cascade unless
            url: this.url,
			fileUpload : true,
            method: 'POST',
            bodyStyle: 'padding:10px 5px 0',
            border: false, // removing the border of the form
            frame: false,
            closable: true,
            defaults: {
                //width: 310,
                msgTarget: 'side'
            },
            defaultType: 'textfield',

       items: [
			{
				xtype:'fileuploadfield',
				anchor : '100%',
				fieldLabel: 'File',
				emptyText: 'Select a File'
			},
            {
			  xtype: 'hidden',
			  anchor: '100%',
			  name: 'scope',
			  value: scope
            },
            {
			  xtype: 'hidden',
			  anchor: '100%',
			  name: 'application',
			  value: application 
            }
      ]
        });

        this.items = [
  		this.form
		];

        // super
        AdapterManager.FileUpload.superclass.initComponent.call(this);
    },

    buildToolbar: function () {
        return [{
            xtype: 'tbfill'
        }, 
		{
            xtype: "tbbutton",
            text: 'Upload File',
            //icon: 'Content/img/16x16/document-save.png',      
            disabled: false,
            handler: this.onSave,
            scope: this
        },
		{
			xtype:'tbspacer'
		},
		{
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
        var that = this; 
        if (this.form.getForm().isValid()) {
            this.form.getForm().submit({
                waitMsg: 'Uploading file...',
                success: function (f, a) {
                    //Ext.Msg.alert('Success', 'Changes saved successfully!');
                    that.fireEvent('Save', that);
                },
                failure: function (f, a) {
                    //Ext.Msg.alert('Warning', 'Error saving changes!')
                    var message = 'Failed to upload file.';
                    showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
                }
            });
        }
        else {
            var message = 'Form is not complete. Cannot upload file.';
            showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
        }
    }
});


