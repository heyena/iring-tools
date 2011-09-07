/**
* @class AdapterManager.ScopePanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
Ext.define('AdapterManager.ValueListMapPanel', {
  extend: 'Ext.panel.Panel',
  alias: 'widget.AdapterManager.ValueListMapPanel',
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

    var classLabel = '';
    var nodeId = '';
    var classUrl = '';
    var formid = '';
    var scope = '';
    var app = '';
    var interName = '';

    if (this.node != null) {
      nodeId = this.node.data.id;
      scope = nodeId.split('/')[0];
      app = nodeId.split('/')[1];
      formid = 'valueListMapTarget-' + this.node.parentNode.parentNode.data.text + '-' + this.node.parentNode.data.text;
    }

    if (this.record != null && this.node.data.type == 'ListMapNode') {
      interName = this.record.internalValue;
      //objectName = scope + '/' + app + '/' + 'DataObjects/DataObject/' + this.record.classTemplateMaps[0].classMap.identifiers[0].replace('.', '/');
      classUrl = this.record.uri;
      classLabel = this.node.data.text.split('[')[0];
    }

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
        { fieldLabel: 'Class Label', name: 'classlabel', xtype: 'hidden', width: 230, value: classLabel, allowBlank: true },
				{ fieldLabel: 'Internal Name', name: 'internalName', xtype: 'textfield', width: 230, value: interName, allowBlank: false },
        { fieldLabel: 'Class Url', name: 'classUrl', xtype: 'hidden', width: 230, value: classUrl, allowBlank: false },
				{ fieldLabel: 'Old Class Url', name: 'oldClassUrl', xtype: 'hidden', width: 230, value: classUrl, allowBlank: false }
      ],

      html: '<div class="class-target' + formid + '" '
          + 'style="border:1px silver solid;margin:5px;padding:8px;height:20px">'
          + classLab,

      afterRender: function (cmp) {
        Ext.form.Panel.prototype.afterRender.apply(this, arguments);
        var classTarget = this.body.child('div.class-target' + formid);
        var classdd = new Ext.dd.DropTarget(classTarget, {
          ddGroup: 'refdataGroup',
          notifyDrop: function (classdd, e, data) {
            if (data.records[0].data.type != 'ClassNode') {
              var message = 'Please slect a RDL Class...';
              showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
              return false;
            }
            thisform.getForm().findField('classlabel').setValue(data.records[0].data.record.Label);
            thisform.getForm().findField('classUrl').setValue(data.records[0].data.record.Uri);
            var msg = '<table style="font-size:13px"><tr><td>Class Label:  </td><td><b>' + data.records[0].data.record.Label + '</b></td></tr>'
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
    if (thisForm.findField('internalName').getValue() == '' || thisForm.findField('classUrl').getValue() == '') {
      showDialog(400, 100, 'Warning', 'Please fill in both fields in this form.', Ext.Msg.OK, null);
      return;
    }
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