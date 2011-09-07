﻿/**
* @class AdapterManager.GraphPanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
Ext.define('AdapterManager.GraphPanel', {
  extend: 'Ext.panel.Panel',
  alias: 'widget.AdapterManager.GraphPanel',
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

    this.bbar = new Ext.toolbar.Toolbar();
    this.bbar.add(this.buildToolbar());

    var name = '';
    var nodeId = '';
    var objectName = '';
    var classLabel = '';
    var oldClassLabel = '';
    var classLab = '';
    var classUrl = '';
    var oldClassUrl = '';
    var formid = '';
    var identifier = '';
    var scope = '';
    var app = '';

    if (this.node != null) {
      nodeId = this.node.data.id;
      scope = nodeId.split('/')[0];
      app = nodeId.split('/')[1];
      formid = 'graphtarget-' + this.node.parentNode.parentNode.data.text + '-' + this.node.parentNode.data.text;
    }

    if (this.record != null) {
      name = this.record.name;
      objectName = scope + '/' + app + '/' + 'DataObjects/DataObject/' + this.record.classTemplateMaps[0].classMap.identifiers[0].replace('.', '/');
      classLabel = this.record.classTemplateMaps[0].classMap.name;
      classUrl = this.record.classTemplateMaps[0].classMap.id;
      identifier = this.record.classTemplateMaps[0].classMap.identifiers[0].split('.')[1];
    }


    if (identifier == '')
      identifier = 'Drop a Key Property Node here.</div>';
    else
      identifier = 'Identifier: ' + identifier + '</div>';

    if (classLabel == '')
      classLab = 'Drop a Class Node here. </div>';
    else
      classLab = 'Class Label: ' + classLabel + '</div>';

    var thisform = new Ext.form.Panel({
      labelWidth: 100, // label settings here cascade unless
      url: this.url,
      method: 'POST',
      bodyStyle: 'padding:10px 5px 0',

      border: false, // removing the border of the form

      frame: false,
      defaults: {
        width: 310,
        msgTarget: 'side'
      },
      defaultType: 'textfield',

      items: [
        { fieldLabel: 'Mapping Node', name: 'mappingNode', xtype: 'hidden', width: 230, value: nodeId, allowBlank: true },
        { fieldLabel: 'Graph Name', name: 'graphName', xtype: 'textfield', width: 230, value: name, allowBlank: false },
        { fieldLabel: 'Object Name', name: 'objectName', xtype: 'hidden', width: 230, value: objectName, allowBlank: true },
        { fieldLabel: 'Class Label', name: 'classLabel', xtype: 'hidden', width: 230, value: classLabel, allowBlank: true },
        { fieldLabel: 'Class Url', name: 'classUrl', xtype: 'hidden', width: 230, value: classUrl, allowBlank: true },
        { fieldLabel: 'OldClass Label', name: 'oldClassLabel', xtype: 'hidden', width: 230, value: classLabel, allowBlank: true },
        { fieldLabel: 'OldClass Url', name: 'oldClassUrl', xtype: 'hidden', width: 230, value: classUrl, allowBlank: true }
      ],

      html: '<div class="property-target' + formid + '" '
          + 'style="border:1px silver solid;margin:5px;padding:8px;height:20px">'
          + identifier
          + '<div class="class-target' + formid + '" '
          + 'style="border:1px silver solid;margin:5px;padding:8px;height:20px">'
          + classLab,

      afterRender: function (cmp) {
        Ext.form.Panel.prototype.afterRender.apply(this, arguments);

        var propertyTarget = this.body.child('div.property-target' + formid);
        var propertydd = new Ext.dd.DropTarget(propertyTarget, {
          ddGroup: 'propertyGroup',
          notifyEnter: function (propertydd, e, data) {
            if (data.records[0].data.type != 'KeyDataPropertyNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;
          },
          notifyOver: function (propertydd, e, data) {
            if (data.records[0].data.type != 'KeyDataPropertyNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;
          },
          notifyDrop: function (propertydd, e, data) {
            if (data.records[0].data.type != 'KeyDataPropertyNode') {
              return false;
            }
            else {
              thisform.getForm().findField('objectName').setValue(data.records[0].data.id);
              var msg = '<table style="font-size:13px"><tr><td>Identifier:</td><td><b>' + data.records[0].data.id.split('/')[5] + '</b></td></tr>'
              msg += '</table>'
              thisform.body.child('div.property-target' + formid).update(msg);
              return true;
            }
          } //eo notifyDrop
        }); //eo propertydd
        var classTarget = this.body.child('div.class-target' + formid);
        var classdd = new Ext.dd.DropTarget(classTarget, {
          ddGroup: 'refdataGroup',
          notifyEnter: function (classdd, e, data) {
            if (data.records[0].data.type != 'ClassNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;
          },
          notifyOver: function (classdd, e, data) {
            if (data.records[0].data.type != 'ClassNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;
          },
          notifyDrop: function (classdd, e, data) {
            if (data.records[0].data.type != 'ClassNode') {
              var message = 'Please slect a RDL Class...';
              showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
              return false;
            } else {
              var tempClassLabel = thisform.getForm().findField('classLabel').getValue();
              var tempClassUrl = thisform.getForm().findField('classUrl').getValue();
              if (tempClassLabel != "") {
                thisform.getForm().findField('oldClassLabel').setValue(tempClassLabel);
                thisform.getForm().findField('oldClassUrl').setValue(tempClassUrl);
              }

              thisform.getForm().findField('classLabel').setValue(data.records[0].data.record.Label);
              thisform.getForm().findField('classUrl').setValue(data.records[0].data.record.Uri);
              var msg = '<table style="font-size:13px"><tr><td>Class Label:</td><td><b>' + data.records[0].data.record.Label + '</b></td></tr>'
              msg += '</table>'
              thisform.body.child('div.class-target' + formid).update(msg);
              return true;
            }
          } //eo notifyDrop
        }); //eo classdd
      } //eo after render
    }); //eo form



    this.items = [
  		thisform
		];

    this.form = thisform;

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
    this.form.getForm().reset();
    this.fireEvent('Cancel', this);
  },

  onSave: function () {
    var that = this;    // consists the main/previous class object	
    var thisForm = this.form.getForm();
    if (thisForm.findField('objectName').getValue() == '' || thisForm.findField('graphName').getValue() == '' || thisForm.findField('classLabel').getValue() == '') {
      showDialog(400, 50, 'Warning', 'Please fill in every field in this form.', Ext.Msg.OK, null);
      return;
    }
    thisForm.submit({
      success: function (f, a) {
        that.fireEvent('Save', that);
      },
      failure: function (f, a) {
        var message = 'Error saving changes!';
        showDialog(400, 50, 'Warning', message, Ext.Msg.OK, null);
      }
    });
  }
});


