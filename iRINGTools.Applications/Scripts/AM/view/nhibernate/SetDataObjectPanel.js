﻿Ext.define('AM.view.nhibernate.SetDataObjectPanel', {
  extend: 'Ext.form.Panel',
  alias: 'widget.setdataobjectpanel',
  name: 'dataObject',
  border: false,
  autoScroll: true,
  monitorValid: true,
  contextName: null,
  endpoint: null,
  width: 400,
  //node: null,
  bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
  defaults: {
    anchor: '100%',
    xtype: 'textfield',
    labelWidth: 140,
    allowBlank: false
  },
  initComponent: function () {
    var me = this;
    //var treeNode = this.node;

    me.items = [{
      xtype: 'label',
      text: 'Data Object',
      cls: 'x-form-item',
      style: 'font-weight:bold;'
    }, {
      xtype: 'textfield',
      name: 'tableName',
      fieldLabel: 'Table Name',
      readOnly: true
    }, {
      xtype: 'textfield',
      name: 'objectNamespace',
      fieldLabel: 'Object Namespace'
    }, {
      xtype: 'textfield',
      name: 'objectName',
      fieldLabel: 'Object Name'
    }, {
      xtype: 'textfield',
      name: 'keyDelimeter',
      fieldLabel: 'Key Delimiter',
      allowBlank: true
    }, {
      name: 'description',
      xtype: 'textarea',
      height: 150,
      fieldLabel: 'Description',
      //value: treeNode.data.property.description,
      allowBlank: true
    }];

    me.tbar = new Ext.Toolbar({
      items: [{
        xtype: 'tbspacer',
        width: 4
      }, {
        xtype: 'button',
        icon: 'Content/img/16x16/apply.png',
        text: 'Apply',
        tooltip: 'Apply the current changes to the data objects tree',
        scope: this,
        action: 'applydbobjectchanges'

      }, {
        xtype: 'tbspacer',
        width: 4
      }, {
        xtype: 'button',
        icon: 'Content/img/16x16/edit-clear.png',
        text: 'Reset',
        tooltip: 'Reset to the latest applied changes',
        scope: this,
        action: 'resetdbobjectchanges'
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


