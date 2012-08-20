
Ext.define('AM.view.nhibernate.RadioField', {
  extend: 'Ext.panel.Panel',
  alias: 'widget.radiotextfield',
  value: null,
  serName: null,
  bodyStyle: 'background:#eee',
  layout: 'column',
  border: false,
  frame: false,
  items: [],

  initComponent: function () {
    var serName = this.serName;
    var value = this.value;
    var me = this;

    var field1 = Ext.create('Ext.form.TextField', {
      disabled: true,
      labelWidth: 125,
      allowBlank: false,
      fieldLabel: 'Sid',
      value: value,
      name: 'field_sid',
      listeners: {
        'change': function (field, newValue, oldValue) {
          me.value = newValue.toUpperCase();
        }
      }
    });

    var field2 = Ext.create('Ext.form.TextField', {
      disabled: true,
      labelWidth: 125,
      allowBlank: false,
      fieldLabel: 'Service Name',
      value: value,
      name: 'field_serviceName',
      listeners: {
        'change': function (field, newValue, oldValue) {
          me.value = newValue.toUpperCase();
        }
      }
    });

    var radioGroup = Ext.create('Ext.form.RadioGroup', {
      columns: 1,
      items: [{
        name: 'sid',
        inputValue: 0,
        style: 'margin-top: 4px'
      }, {
        name: 'sid',
        inputValue: 1,
        style: 'margin-top: 4px'
      }],
      listeners: {
        'change': function (e, changed) {
          if (changed) {
            var value = radioGroup.getValue().sid;
            if (value == 0) {
              field2.disable();
              field2.clearInvalid();
              field1.enable();
              field1.focus();
              serName = 'SID';
            }
            else {
              field1.clearInvalid();
              field1.disable();
              field2.enable();
              field2.focus();
              serName = 'SERVICE_NAME';
            }
          }
        }
      }
    });

    this.items = [{
      width: 25,
      layout: 'anchor',
      defaults: { anchor: '100%' },
      labelWidth: 0.1,      
      border: false,
      frame: false,  
      bodyStyle: 'background:#eee',
      items: [radioGroup]
    }, {
      columnWidth: 1,      
      bodyStyle: 'background:#eee',
      border: false,
      frame: false,
      defaults: { anchor: '100%' },
      layout: 'anchor',      
      items: [ field1, field2 ]
    }];

    if (serName != '' && serName != 1) {
      if (serName.toUpperCase() == 'SID') {
        field1.disabled = false;
        field2.disabled = true;
        field2.value = '';
        radioGroup.items[0].checked = true;
      }
      else {
        field1.disabled = true;
        field1.value = '';
        field2.disabled = false;
        radioGroup.items[1].checked = true;
      }
    }

    this.callParent(arguments);
  }
});

function creatRadioField(panel, idLabel, value, serName, contextName, endpoint) {
	if (panel.items) {
		var radioPane = panel.items.items[0];
		if (radioPane) {
			radioPane.destroy();
		}
	}

  var conf = {
    value: value,
    serName: serName,
    id: contextName + '.' + endpoint + 'radioField'
  };

  var radioField = Ext.widget('radiotextfield', conf);
	
	panel.items.add(radioField);
	panel.doLayout();
};

function showDialog(width, height, title, msg, buttons, callback) {
  while (msg.indexOf('\\r\\n') != -1)
    msg = msg.replace('\\r\\n', ' \r\n');

  var style = 'style="margin:0;padding:0;width:' + width + 'px;height:' + height + 'px;border:1px solid #aaa;overflow:auto"';
  Ext.Msg.show({
    title: title,
    msg: '<textarea ' + style + ' readonly="yes">' + msg + '</textarea>',
    buttons: buttons,
    fn: callback
  });
}

function getLastXString(str, num) {
  var index = str.length;

  if (str[index - 1] == '.')
    str = str.substring(0, index - 1);

  for (var i = 0; i < num; i++) {
    str = str.substring(0, index);
    index = str.lastIndexOf('/');
  }
  return str.substring(index + 1);
};

