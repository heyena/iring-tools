Ext.ns('AdapterManager');
/**
* @class AdapterManager.ScopePanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
AdapterManager.ValueListPanel = Ext.extend(Ext.Panel, {
	layout: 'fit',
	border: false,
	frame: false,
	split: true,
	from: null,
	record: null,
	url: null,
	nodeId: null,

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

		var name = "";
		var nodeId = "";

		if (this.record != null) {
			name = this.record.name;
		}

		if (this.nodeId != null) {
			nodeId = this.nodeId;
		}

		this.form = new Ext.FormPanel({
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
			  { fieldLabel: 'Mapping Node', name: 'mappingNode', xtype: 'hidden', width: 120, value: nodeId, allowBlank: false },
				{ fieldLabel: 'Value List Name', name: 'valueList', xtype: 'textfield', width: 230, value: name, allowBlank: false }
      ],

			autoDestroy: false

		});

		this.items = [
  		this.form
		];

		// super
		//AdapterManager.GraphPanel.superclass.initComponent.call(this);
		AdapterManager.ValueListPanel.superclass.initComponent.call(this); 
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
		if (thisForm.findField('valueList').getValue() == '') {
			showDialog(400, 100, 'Warning', 'Please type in a value list name before saving.', Ext.Msg.OK, null);
			return;
		}
		thisForm.submit({
			waitMsg: 'Saving Data...',
			success: function (f, a) {
				//Ext.Msg.alert('Success', 'Changes saved successfully!');
				that.fireEvent('Save', that);
			},
			failure: function (f, a) {
				//Ext.Msg.alert('Warning', 'Error saving changes!')
				var message = 'Error saving changes!';
				showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
			}
		});
	}

});


