Ext.ns('AdapterManager');

AdapterManager.ClassMapPanel = Ext.extend(Ext.Window, {
  layout: 'fit',
  width: 440,
  height: 280,
  closable: true,
  resizable: false,
  modal: false,
  form: null,
  root: null,
  node: null,
  url: null,
  identifierPrompt: 'Drop property node(s) here.',
  classPrompt: 'Drop a class node here.',

  initComponent: function () {
    var rootClassRecord = this.root.firstChild.attributes.record;
    var scope = '';
    var app = '';
    var graph = '';
    var objectName = '';
    var identifier = '';
    var delimiter = '';
    var className = this.classPrompt;
    var classId = '';
    var parentClassIndex = '';

    this.bbar = this.buildToolbar();

    if (rootClassRecord != null) {
      var parentClassNode = this.node.parentNode.parentNode;

      identifier = rootClassRecord.identifiers.join();
      delimiter = rootClassRecord.identifierDelimiter;
      objectName = rootClassRecord.identifiers[0].split('.')[0];
      parentClassId = parentClassNode.attributes.identifier;
      parentClassIndex = parentClassNode.attributes.index;
      templateIndex = parentClassNode.indexOf(this.node.parentNode);
      roleName = this.node.attributes.record.name;
    }

    if (this.node != null) {
      var nodeIdParts = this.node.id.split('/');
      scope = nodeIdParts[0];
      app = nodeIdParts[1];
      graph = nodeIdParts[2];
      formId = '-class-' + scope + '-' + app;

      var classMap = this.node.attributes.record.classMap;

      if (classMap != null) {
        className = classMap.name;
        classId = classMap.id;
        identifier = classMap.identifiers.join();
        delimiter = classMap.identifierDelimiter;
      }
    }

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
        { text: 'Identifier Delimiter (required for composite identifier): ', xtype: 'label' },
        { name: 'delimiter', xtype: 'textfield', style: 'width:100px; margin:5px 0px 10px 0px', value: delimiter },
        { text: 'Identifier: ', xtype: 'label' },
        { name: 'identifier', xtype: 'textfield', readOnly: true, style: 'height:40px; width:376px; margin:5px 0px 10px 0px;padding-top:10px', value: identifier },
        { text: 'Class: ', xtype: 'label' },
        { name: 'className', xtype: 'textfield', readOnly: true, style: 'height:40px; width:376px; margin:5px 0px 10px 0px;padding-top:10px', value: className },
        { name: 'classId', xtype: 'hidden', value: classId },
        { name: 'scope', xtype: 'hidden', value: scope },
        { name: 'app', xtype: 'hidden', value: app },
        { name: 'graph', xtype: 'hidden', value: graph },
        { name: 'objectName', xtype: 'hidden', value: objectName },
        { name: 'parentClassId', xtype: 'hidden', value: parentClassId },
        { name: 'templateIndex', xtype: 'hidden', value: templateIndex },
        { name: 'roleName', xtype: 'hidden', value: roleName },
        { name: 'parentClassIndex', xtype: 'hidden', value: parentClassIndex }
      ]
    });

    this.items = [
      thisform
    ];

    this.form = thisform;

    this.initDragDrop = function (cmp) {
      Ext.FormPanel.prototype.afterRender.apply(this, arguments);

      var propertyTarget = thisform.getForm().findField('identifier').el;
      var propertydd = new Ext.dd.DropTarget(propertyTarget, {
        ddGroup: 'propertyGroup',
        notifyEnter: function (propertydd, e, data) {
          if (data.node.attributes.type == 'DataPropertyNode' ||
              data.node.attributes.type == 'KeyDataPropertyNode')
            return this.dropAllowed;
          else
            return this.dropNotAllowed;
        },
        notifyOver: function (propertydd, e, data) {
          if (data.node.attributes.type == 'DataPropertyNode' ||
              data.node.attributes.type == 'KeyDataPropertyNode')
            return this.dropAllowed;
          else
            return this.dropNotAllowed;
        },
        notifyDrop: function (propertydd, e, data) {
          if (data.node.attributes.type == 'DataPropertyNode' ||
              data.node.attributes.type == 'KeyDataPropertyNode') {
            var currIdentifier = thisform.getForm().findField('identifier').getValue();

            if (currIdentifier == '' || currIdentifier == thisform.ownerCt.identifierPrompt) {
              objectName = data.node.parentNode.text;
              identifier = objectName + '.' + data.node.text;
              thisform.getForm().findField('objectName').setValue(objectName);
            }
            else {
              if (objectName != data.node.parentNode.text) {
                var message = 'Properties must root from the same data object as graph!';
                showDialog(400, 100, 'Error', message, Ext.Msg.OK, null);
                return false;
              }

              if (identifier.indexOf(objectName + "." + data.node.text) != -1) {
                var message = 'Duplicate properties are not allowed!';
                showDialog(400, 100, 'Error', message, Ext.Msg.OK, null);
                return false;
              }

              identifier += ',' + objectName + '.' + data.node.text;
            }

            thisform.getForm().findField('identifier').setValue(identifier);

            return true;
          }
        }
      });

      var classTarget = thisform.getForm().findField('className').el;
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
            var message = 'Please select a reference class...';
            showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
            return false;
          }
          else {
            var lbl = data.node.attributes.record.Label;
            thisform.getForm().findField('className').setValue(lbl);
            thisform.getForm().findField('classId').setValue(data.node.attributes.record.Uri);
            return true;
          }
        }
      });
    },

    AdapterManager.ClassMapPanel.superclass.initComponent.call(this);
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
    this.form.getForm().findField('objectName').setValue('');
    this.form.getForm().findField('identifier').setValue(this.identifierPrompt);
  },

  onCancel: function () {
    this.close();
  },

  onSave: function () {
    var panel = this;
    var thisForm = this.form.getForm();

    if (thisForm.findField('identifier').getValue() == '' ||
        thisForm.findField('objectName').getValue() == '' ||
        thisForm.findField('className').getValue() == this.classPrompt ||
        thisForm.findField('className').getValue() == '') {
      var msg = 'Required fields can not be blank!';
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
        panel.node.removeAll(true);
        panel.node.appendChild(a.result.node);
        panel.node.expand();
        panel.fireEvent('addClassMapComplete');
      },
      failure: function (f, a) {
        var message = 'Error saving changes: ' + a.response.responseText;
        showDialog(500, 250, 'Error', message, Ext.Msg.OK, null);
      }
    });
  },

  show: function () {
    AdapterManager.ClassMapPanel.superclass.show.call(this);
    this.initDragDrop();
  }
});
