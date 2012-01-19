
Ext.define('AM.view.nhibernate.RadioField', {
  extend: 'Ext.panel.Panel',
  alias: 'widget.radiotextfield',
  value: null,
  serName: null,

  initComponent: function () {
      this.callParent(arguments);
		//Ext.apply(this, config);

		this.bodyStyle = 'background:#eee';

		this.radioGroup = new Ext.form.RadioGroup({
			columns: 1,
			items: [{
				name: 'sid',
				inputValue: 0,
				style: 'margin-top: 4px'
			}, {
				name: 'sid',
				inputValue: 1,
				style: 'margin-top: 4px'
			}]
		});

		var that = this;
		this.field1 = new Ext.form.TextField({
			disabled: true,
			allowBlank: false,
			fieldLabel: 'Sid',
			value: this.value,
			name: 'field_sid',
			listeners: {
				'change': function (field, newValue, oldValue) {
					that.value = newValue.toUpperCase();
				}
			}
		});

		this.field2 = new Ext.form.TextField({
			disabled: true,
			allowBlank: false,
			fieldLabel: 'Service Name',
			value: this.value,
			name: 'field_serviceName',
			listeners: {
				'change': function (field, newValue, oldValue) {
					that.value = newValue.toUpperCase();
				}
			}
		});

		if (this.serName != '') {
			if (this.serName.toUpperCase() == 'SID') {
				this.field1.disabled = false;
				this.field2.disabled = true;

				this.field2.value = '';
				this.radioGroup.items[0].checked = true;
			}
			else {
				this.field1.disabled = true;
				this.field1.value = '';
				this.field2.disabled = false;
				this.radioGroup.items[1].checked = true;
			}
		}

		this.layout = 'column';
		this.border = false;
		this.frame = false;

		this.add([{
			width: 40,
			layout: 'form',
			labelWidth: 0.1,
			items: this.radioGroup,
			border: false,
			frame: false,
			bodyStyle: 'background:#eee'
		}, {
			columnWidth: 1,
			layout: 'form',
			labelWidth: 110,
			defaults: { anchor: '100%', allowBlank: false },
			items: [this.field1, this.field2],
			border: false,
			frame: false,
			bodyStyle: 'background:#eee'
		}]);

		this.subscribeEvents();
	},
	subscribeEvents: function () {
		this.radioGroup.on('change', this.toggleState, this);
	},
	toggleState: function (e, changed) {
		if (changed) {
			var value = this.radioGroup.getValue().inputValue;
			if (value == 0) {
				this.field2.disable();
				this.field2.clearInvalid();
				this.field1.enable();
				this.field1.focus();
				this.serName = 'SID';
			}
			else {
				this.field1.clearInvalid();
				this.field1.disable();
				this.field2.enable();
				this.field2.focus();
				this.serName = 'SERVICE_NAME';
			}
		}
	}
});


function creatRadioField(panel, idLabel, value, serName) {
	if (panel.items) {
		var radioPane = panel.items.map[idLabel + 'radioField'];
		if (radioPane) {
			radioPane.destroy();
		}
	}

	var radioField = new RadioField({
		id: idLabel + 'radioField',
		value: value,
		serName: serName
	});

	panel.add(radioField);
	panel.doLayout();
}

function showDialog(width, height, title, message, buttons, callback) {
	var style = 'style="margin:0;padding:0;width:' + width + 'px;height:' + height + 'px;border:1px solid #aaa;overflow:auto"';
	Ext.Msg.show({
		title: title,
		msg: '<textarea ' + style + ' readonly="yes">' + message + '</textarea>',
		buttons: buttons,
		fn: callback
	});
}