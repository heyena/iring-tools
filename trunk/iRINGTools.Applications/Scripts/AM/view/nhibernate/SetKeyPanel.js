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
  bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
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
      fieldLabel: 'Property Name (editable)'
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
          var propertyName = me.getForm().findField('propertyName').getValue();
          me.node.data.property.propertyName = propertyName;
          me.node.set('text', propertyName);
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

