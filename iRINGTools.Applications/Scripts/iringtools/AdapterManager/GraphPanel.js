Ext.ns('AdapterManager');
/**
* @class AdapterManager.GraphPanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
AdapterManager.GraphPanel = Ext.extend(Ext.Panel, {
  layout: 'fit',
  border: false,
  frame: false,
  split: true,
  form: null,
  record: null,
  url: null,
  node: null,
  resizable: false,
  identifierPrompt: 'Drop property node(s) here.',
  classPrompt: 'Drop a class node here.',

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

    var nodeId = '';
    var formId = '';
    var scope = '';
    var app = '';
    var oldGraphName = '';
    var graphName = '';
    var objectName = '';
    var identifier = '';
    var delimiter = '_';
    var className = '';
    var classId = '';

    if (this.node != null) {
      nodeId = this.node.id;
      scope = nodeId.split('/')[0];
      app = nodeId.split('/')[1];
      formId = '-' + scope + '-' + app;
    }

    if (this.record != null) {
      var classMap = this.record.classTemplateMaps[0].classMap;

      oldGraphName = this.record.name;
      graphName = this.record.name;
      objectName = this.record.dataObjectName;
      className = classMap.name;
      classId = classMap.id;
      identifier = classMap.identifiers.join();
      delimiter = classMap.identifierDelimiter
    }

    var identifierValue = (identifier == '') ? this.identifierPrompt : identifier;
    var classValue = (className == '') ? this.classPrompt : className;

    var thisform = new Ext.FormPanel({
      labelWidth: 17,
      url: this.url,
      method: 'POST',
      bodyStyle: 'padding:10px',
      border: false,
      frame: false,
      closable: true,
      defaults: {
        msgTarget: 'side',
        allowBlank: true
      },

      items: [
        { text: 'Graph Name: ', xtype: 'label' },
        { name: 'graphName', xtype: 'textfield', style: 'width:376px; margin:5px 0px 10px 0px', value: graphName },
        { text: 'Identifier Delimiter (required for composite identifier): ', xtype: 'label' },
        { name: 'delimiter', xtype: 'textfield', style: 'width:100px; margin:5px 0px 10px 0px', value: delimiter },
        { name: 'scope', xtype: 'hidden', value: scope },
        { name: 'app', xtype: 'hidden', value: app },
        { name: 'oldGraphName', xtype: 'hidden', value: oldGraphName },
        { name: 'className', xtype: 'hidden', value: className },
        { name: 'identifier', xtype: 'hidden', value: identifier },
        { name: 'objectName', xtype: 'hidden', value: objectName },
        { name: 'classId', xtype: 'hidden', value: classId },
      ],

      html: 'Identifier(s): <div class="property-target' + formId + '" '
              + 'style="border:1px silver solid;margin:5px 5px 10px 20px;padding:10;height:20">'
              + identifierValue + '</div>'
              + 'Root Class: <div class="class-target' + formId + '" '
              + 'style="border:1px silver solid;margin:5px 5px 10px 20px;padding:10;height:20">'
              + classValue + '</div>',

      afterRender: function (cmp) {
        Ext.FormPanel.prototype.afterRender.apply(this, arguments);

        var propertyTarget = this.body.child('div.property-target' + formId);
        var propertydd = new Ext.dd.DropTarget(propertyTarget, {
          ddGroup: 'propertyGroup',
          notifyEnter: function (propertydd, e, data) {
            if (data.node.attributes.type != 'DataPropertyNode' &&
                data.node.attributes.type != 'KeyDataPropertyNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;
          },
          notifyOver: function (propertydd, e, data) {
            if (data.node.attributes.type != 'DataPropertyNode' &&
                data.node.attributes.type != 'KeyDataPropertyNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;
          },
          notifyDrop: function (propertydd, e, data) {
            if (data.node.attributes.type != 'DataPropertyNode' &&
                data.node.attributes.type != 'KeyDataPropertyNode') {
              return false;
            }
            else {
              thisform.getForm().findField('objectName').setValue(data.node.id.split('/')[4]);
              var currIdentifier = thisform.body.child('div.property-target' + formId).dom.innerHTML;

              if (currIdentifier == thisform.ownerCt.identifierPrompt) {
                objectName = data.node.parentNode.text;
                identifier = objectName + '.' + data.node.text;
              }
              else {
                if (objectName != data.node.parentNode.text) {
                  var message = 'Identifiers must come from the same Data Object!';
                  showDialog(400, 100, 'Error', message, Ext.Msg.OK, null);
                  return false;
                }

                if (identifier.indexOf(objectName + "." + data.node.text) != -1) {
                  var message = 'Duplicate identifiers are not allowed!';
                  showDialog(400, 100, 'Error', message, Ext.Msg.OK, null);
                  return false;
                }

                identifier += ',' + objectName + '.' + data.node.text;
              }

              thisform.body.child('div.property-target' + formId).update(identifier);
              thisform.getForm().findField('identifier').setValue(identifier);

              return true;
            }
          } //eo notifyDrop
        }); //eo propertydd

        var classTarget = this.body.child('div.class-target' + formId);
        var classdd = new Ext.dd.DropTarget(classTarget, {
          ddGroup: 'refdataGroup',
          notifyEnter: function (classdd, e, data) {
            if (data.node.attributes.type != 'ClassNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;
          },
          notifyOver: function (classdd, e, data) {
            if (data.node.attributes.type != 'ClassNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;
          },
          notifyDrop: function (classdd, e, data) {
            if (data.node.attributes.type != 'ClassNode') {
              var message = 'Please slect a RDL Class...';
              showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
              return false;
            }
            else {
              var lbl = data.node.attributes.record.Label;
              thisform.getForm().findField('className').setValue(lbl);
              thisform.getForm().findField('classId').setValue(data.node.attributes.record.Uri);
              thisform.body.child('div.class-target' + formId).update(lbl);
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
    AdapterManager.GraphPanel.superclass.initComponent.call(this);
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
      text: 'Reset',
      //icon: 'Content/img/16x16/edit-clear.png',      
      disabled: false,
      handler: this.onReset,
      scope: this
    }, {
      xtype: "tbbutton",
      text: 'Cancel',
      //icon: 'Content/img/16x16/edit-clear.png',      
      disabled: false,
      handler: this.onCancel,
      scope: this
    }]
  },

  onReset: function () {
    var scope = this.form.getForm().findField('scope').getValue();
    var app = this.form.getForm().findField('app').getValue();
    this.form.body.child('div.property-target-' + scope + '-' + app).dom.innerHTML = 'Drop property node(s) here.';
    this.form.getForm().findField('objectName').setValue('');
    this.form.getForm().findField('identifier').setValue('');
  },

  onCancel: function () {
    this.form.getForm().reset();
    this.fireEvent('Cancel', this);
  },

  onSave: function () {
    var that = this;    // consists the main/previous class object	
    var thisForm = this.form.getForm();

    if (thisForm.findField('graphName').getValue() == '' ||
        thisForm.findField('identifier').getValue() == '' ||
        thisForm.findField('objectName').getValue() == '' ||
        thisForm.findField('className').getValue() == '') {
      var msg = 'All fields except for Identifier Delimiter are required!';
      showDialog(420, 50, 'Error', msg, Ext.Msg.OK, null);
      return;
    }

    if ((thisForm.findField('identifier').getValue().indexOf(',') != -1) &&
      thisForm.findField('delimiter').getValue() == '') {
      var msg = 'Identifier Delimiter is required for composite identifier!';
      showDialog(420, 50, 'Error', msg, Ext.Msg.OK, null);
      return;
    }

    thisForm.submit({
      success: function (f, a) {
        that.fireEvent('Save', that);
      },
      failure: function (f, a) {
        var message = 'Error saving changes!';
        showDialog(400, 50, 'Error', message, Ext.Msg.OK, null);
      }
    });
  }
});
