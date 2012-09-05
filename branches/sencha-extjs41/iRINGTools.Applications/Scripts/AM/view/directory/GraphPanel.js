Ext.define('AM.view.directory.GraphPanel', {
  extend: 'Ext.window.Window',
  alias: 'widget.graphmapform',
  layout: 'fit',
  border: false,
  frame: false,
  split: true,
  form: null,
  record: null,
  url: null,
  node: null,
  closable: true,
  state: null,

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
    var contextName = '';
    var endpoint = '';
    var graphName = '';
    var baseUrl = '';

    if (this.node != null) {
      nodeId = this.node.data.id;
      contextName = this.node.data.property.context;
      endpoint = this.node.data.property.endpoint;
      baseUrl = this.node.data.property.baseUrl;
      formid = 'graphtarget-' + contextName + '-' + endpoint;
    }

    if (this.record.record != null) {
      name = this.record.record.name;
      objectName = contextName + '/' + endpoint + '/' + 'DataObjects/DataObject/' + this.record.record.classTemplateMaps[0].classMap.identifiers[0].replace('.', '/');
      classLabel = this.record.record.classTemplateMaps[0].classMap.name;
      classUrl = this.record.record.classTemplateMaps[0].classMap.id;
      identifier = getLastXString(this.record.record.classTemplateMaps[0].classMap.identifiers[0], 1);
    }


    if (identifier == '')
      identifier = 'Drop a Key/Property Node here.</div>';
    else
      identifier = 'Identifier: ' + identifier + '</div>';

    if (classLabel == '')
      classLab = 'Drop a Class Node here. </div>';
    else
      classLab = 'Class Label: ' + classLabel + '</div>';

    this.items = [{
      xtype: 'form',
      labelWidth: 100,
      url: 'mapping/graphMap',
      method: 'POST',
      bodyStyle: 'padding:10px 5px 0',

      border: false,

      frame: false,
      defaults: {
        anchor: '100%',
        msgTarget: 'side'
      },
      defaultType: 'textfield',

      items: [
            { name: 'contextName', xtype: 'hidden', value: contextName, allowBlank: true },
				    { name: 'endpoint', xtype: 'hidden', value: endpoint, allowBlank: true },
            { name: 'oldGraphName', xtype: 'hidden', value: graphName, allowBlank: true },
            { name: 'baseUrl', xtype: 'hidden', value: baseUrl, allowBlank: false },
			      { fieldLabel: 'Mapping Node', name: 'mappingNode', xtype: 'hidden', value: nodeId, allowBlank: true },
				    { fieldLabel: 'Graph Name', name: 'graphName', xtype: 'textfield', value: name, allowBlank: false },
				    { fieldLabel: 'Object Name', name: 'objectName', xtype: 'hidden', value: objectName, allowBlank: true },
            { fieldLabel: 'Class Label', name: 'classLabel', xtype: 'hidden', value: classLabel, allowBlank: true },
            { fieldLabel: 'Class Url', name: 'classUrl', xtype: 'hidden', value: classUrl, allowBlank: true },
            { fieldLabel: 'OldClass Label', name: 'oldClassLabel', xtype: 'hidden',value: "", allowBlank: true },
            { fieldLabel: 'OldClass Url', name: 'oldClassUrl', xtype: 'hidden', value: "", allowBlank: true }
          ],

      html: '<div class="property-target' + formid + '" '
          + 'style="border:1px silver solid;margin:5px;padding:8px;height:40px">'
          + identifier
          + '<div class="class-target' + formid + '" '
          + 'style="border:1px silver solid;margin:5px;padding:8px;height:40px">'
          + classLab,

      afterRender: function (cmp) {
        Ext.form.Panel.prototype.afterRender.apply(this, arguments);
        var me = this;
        var propertyTarget = this.body.child('div.property-target' + formid);
        var propertydd = new Ext.dd.DropTarget(propertyTarget, {
          ddGroup: 'propertyGroup',
          notifyEnter: function (propertydd, e, data) {
            if (data.records[0].data.type != 'DataPropertyNode' && data.records[0].data.type != 'KeyDataPropertyNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;
          },
          notifyOver: function (propertydd, e, data) {
            if (data.records[0].data.type != 'DataPropertyNode' && data.records[0].data.type != 'KeyDataPropertyNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;
          },
          notifyDrop: function (propertydd, e, data) {
            if (data.records[0].data.type != 'DataPropertyNode' && data.records[0].data.type != 'KeyDataPropertyNode') {
              return false;
            }
            else {
              me.getForm().findField('objectName').setValue(getLastXString(data.records[0].data.id, 2));
              var msg = '<table style="font-size:13px"><tr><td>Identifier:</td><td><b>' + getLastXString(data.records[0].data.id, 1) + '</b></td></tr>'
              msg += '</table>'
              me.body.child('div.property-target' + formid).update(msg);
              return true;
            }
          } //eo notifyDrop
        }); //eo propertydd
        var classTarget = me.body.child('div.class-target' + formid);
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
              var tempClassLabel = me.getForm().findField('classLabel').getValue();
              var tempClassUrl = me.getForm().findField('classUrl').getValue();
              if (tempClassLabel != "") {
                me.getForm().findField('oldClassLabel').setValue(tempClassLabel);
                me.getForm().findField('oldClassUrl').setValue(tempClassUrl);
              }

              //var lbl = (data.records[0].data.record.Label != undefined) ? data.records[0].data.record.Label : data.records[0].data.record.label;
              //me.getForm().findField('classLabel').setValue(lbl);
              me.getForm().findField('classLabel').setValue(data.records[0].data.record.Label);
              me.getForm().findField('classUrl').setValue(data.records[0].data.record.Uri);
              var msg = '<table style="font-size:13px"><tr><td>Class Label:</td><td><b>' + data.records[0].data.record.Label + '</b></td></tr>'
              msg += '</table>'
              me.body.child('div.class-target' + formid).update(msg);
              return true;
            }
          } //eo notifyDrop
        }); //eo classdd
      } //eo after render
    }]; //eo form


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
    this.down('form').getForm().reset();
    this.fireEvent('Cancel', this);
  },

  onSave: function () {
    var me = this;    // consists the main/previous class object	
    var thisForm = this.down('form').getForm();
    if (thisForm.findField('objectName').getValue() == '' || thisForm.findField('graphName').getValue() == '' || thisForm.findField('classLabel').getValue() == '') {
      showDialog(400, 50, 'Warning', 'Please fill in every field in this form.', Ext.Msg.OK, null);
      return;
    }
    thisForm.submit({
      success: function (f, a) {
        me.fireEvent('Save', me);
      },
      failure: function (f, a) {
        var message = 'Error saving changes!';
        showDialog(400, 50, 'Warning', message, Ext.Msg.OK, null);
      }
    });
  }
});
