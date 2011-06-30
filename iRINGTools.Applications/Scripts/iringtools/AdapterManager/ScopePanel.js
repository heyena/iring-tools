﻿Ext.ns('AdapterManager');
/**
* @class AdapterManager.ScopePanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
AdapterManager.ScopePanel = Ext.extend(Ext.Panel, {
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
			reset: true,
			validate: true,
			tabChange: true,
			refresh: true,
			selectionchange: true
		});

		this.bbar = this.buildToolbar();

		var name = ""
		var description = ""

		if (this.record != null) {
			name = this.record.Name;
			description = this.record.Description
		}

		this.form = new Ext.FormPanel({
			labelWidth: 70, // label settings here cascade unless
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
        { fieldLabel: 'Scope', name: 'Scope', xtype: 'hidden', width: 300, value: name, allowBlank: false },
        { fieldLabel: 'Name', name: 'Name', xtype: 'textfield', width: 300, value: name, allowBlank: false },
        { fieldLabel: 'Description', name: 'Description', allowBlank: true, xtype: 'textarea', width: 300, value: description }
      ],
			//buttonAlign: 'left', // buttons aligned to the left            
			autoDestroy: false

		});

		this.items = [
  		this.form
		];

		// super
		AdapterManager.ScopePanel.superclass.initComponent.call(this);
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

	getActiveTab: function () {
		contentPanel = Ext.getCmp('content-panel');

		if (contentPanel.items.length != 0) { // check is there any tab in contentPanel
			return contentPanel.getActiveTab();
		}
		else {
			return false;
		}
	},

	onReset: function () {
		this.form.getForm().reset();
		this.fireEvent('Cancel', this);
	},


	onSave: function () {
		var that = this;    // consists the main/previous class object

		this.form.getForm().submit({
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


function showDialog(width, height, title, message, buttons, callback) {
  var style = 'style="margin:0;padding:0;width:' + width + 'px;height:' + height + 'px;border:1px solid #aaa;overflow:auto"';
  Ext.Msg.show({
    title: title,
    msg: '<textarea ' + style + ' readonly="yes">' + message + '</textarea>',
    buttons: buttons,
    fn: callback
  });
}
