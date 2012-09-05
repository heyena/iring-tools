Ext.define('AM.view.nhibernate.SetKeyPanel', {
  extend: 'Ext.form.Panel',
  alias: 'widget.setdatakeyform',
  border: false,
  name: 'keyProperty',
  contextName: null,
  endpoint: null,
  node: null,
  autoScroll: true,
  monitorValid: true,
  bodyStyle: 'background:#eee;padding:10px 0px 0px 10px',
  defaults: {
    anchor: '100%',
    xtype: 'textfield',
    labelWidth: 160,
    allowBlank: false
  },

  initComponent: function () {
    var me = this;
    this.items = [{
      xtype: 'label',
      text: 'Key Properties',
      cls: 'x-form-item',
      style: 'font-weight:bold;'
    }, {
      name: 'columnName',
      fieldLabel: 'Column Name',
      readOnly: true
    }, {
      name: 'propertyName',
      fieldLabel: 'Property Name (editable)',
      allowBlank: false,
      validationEvent: "blur",
      regex: new RegExp("^[a-zA-Z_][a-zA-Z0-9_]*$"),
      regexText: '<b>Error</b></br>Invalid Value. A valid value should start with alphabet or "_", and follow by any number of "_", alphabet, or number characters'
    }, {
      name: 'dataType',
      fieldLabel: 'Data Type',
      readOnly: true
    }, {
      name: 'dataLength',
      fieldLabel: 'Data Length',
      readOnly: true
    }, {
      name: 'nullable',
      fieldLabel: 'Nullable',
      readOnly: true
    }, {
      name: 'showOnIndex',
      fieldLabel: 'Show on Index',
      readOnly: true
    }, {
      name: 'numberOfDecimals',
      fieldLabel: 'Number of Decimals',
      readOnly: true
    }, {
      name: 'keyType',
      fieldLabel: 'Key Type',
      readOnly: true
    }];

    this.tbar = new Ext.Toolbar({
      items: [{
        xtype: 'tbspacer',
        width: 4
      }, {
        xtype: 'button',
        icon: 'Content/img/16x16/apply.png',
        text: 'Apply',
        tooltip: 'Apply the current changes to the data objects tree',
        handler: function (f) {
          var propertyNameField = me.getForm().findField('propertyName');
          var propertyName = propertyNameField.getValue();

          if (propertyNameField.validate()) {
            me.node.data.property.propertyName = propertyName;
            me.node.set('text', propertyName);
          }
          else {
            showDialog(400, 100, 'Warning', "Property Name is not valid. A valid property name should start with alphabet or \"_\", and follow by any number of \"_\", alphabet, or number characters", Ext.Msg.OK, null);           
          }
        }
      }, {
        xtype: 'tbspacer',
        width: 4
      }, {
        xtype: 'button',
        icon: 'Content/img/16x16/edit-clear.png',
        text: 'Reset',
        tooltip: 'Reset to the latest applied changes',
        handler: function (f) {
          me.setActiveRecord(me.node.data.property);
        }
      }]
    });
    this.callParent(arguments);
  },

  setActiveRecord: function (record) {
    if (record) {
      this.getForm().setValues(record);
    } else {
      this.getForm().reset();
    }
  }
});

