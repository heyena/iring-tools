Ext.define('AM.view.mapping.ClassmapForm', {
  extend: 'Ext.window.Window',
  alias: 'widget.classmapform',
  contextName: null,
  endpoint: null,
  baseUrl: null,
  graphName: null,
  tree: null,
  mappingNode: null,
  parentClassId: null,
  index: 0,
  height: 180,
  width: 430,
  closable: true,
  title: 'Add new ClassMap to RoleMAp',
  border: false,
  frame: false,
  floating: true,
  layout: 'fit',
  initComponent: function () {
    var formid = 'propertytarget-' + this.context + '-' + this.endpoint;

    this.items = [{
      xtype: 'form',
      bodyStyle: 'padding:10px 5px 0',
      url: 'mapping/addclassmap',
      method: 'POST',
      bbar: [
        { xtype: 'tbfill' },
        { text: 'Ok',
          scope: this,
          handler: function (btn, e) {
            var form = btn.findParentByType('form');
            var win = btn.findParentByType('window');
            var addClassMapForm = form.getForm();

            if (addClassMapForm.isValid()) {
              var objectname = addClassMapForm.findField('objectName').getValue();
              var classlabel = addClassMapForm.findField('classLabel').getValue();
              var classurl = addClassMapForm.findField('classUrl').getValue();
              var roleName = getLastXString(this.mappingNode.data.id, 1);
              var propertyName = getLastXString(objectname, 1)
              var relation = '';
              var dataObject;

              if (addClassMapForm.findField('related').getValue() == 'undefined' || addClassMapForm.findField('related').getValue() == '')
                dataObject = getLastXString(objectname, 2);
              else {
                dataObject = getLastXString(objectname, 3);
                relation = addClassMapForm.findField('related').getValue();
              }

              //(contextName, endpoint, propertyName, graphName, objectname, roleName, classurl, classlabel, relation, parentClassId, index, tree, win)
              submitAddClassMap(this.contextName, this.endpoint, this.baseUrl, propertyName, this.graphName, dataObject, roleName, classurl, classlabel, relation, this.parentClassId, this.index, this.tree, win);
            }
          }
        },
        { text: 'Cancel', scope: this, handler: this.onReset }
      ],
      items: [
        { xtype: 'hidden', name: 'objectName' },
        { xtype: 'hidden', name: 'classLabel' },
        { xtype: 'hidden', name: 'classUrl' },
        { xtype: 'hidden', name: 'related' }
      ],
      html: '<div class="property-target' + formid + '" '
        + 'style="border:1px silver solid;margin:5px;padding:8px;height:40px">'
        + 'Drop a Property Node here.</div>'
        + '<div class="class-target' + formid + '" '
        + 'style="border:1px silver solid;margin:5px;padding:8px;height:40px">'
        + 'Drop a Class Node here. </div>',

      afterRender: function (cmp, eOpts) {
        Ext.form.Panel.prototype.afterRender.apply(this, arguments);
        var me = this;
        var propertyTarget = this.body.child('div.property-target' + formid);
        var propertydd = new Ext.dd.DropTarget(propertyTarget, {
          ddGroup: 'propertyGroup',
          notifyEnter: function (dd, e, data) {

            if (data.records[0].data.type != 'DataPropertyNode' && data.records[0].data.type != 'KeyDataPropertyNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;

          },
          notifyOver: function (dd, e, data) {

            if (data.records[0].data.type != 'DataPropertyNode' && data.records[0].data.type != 'KeyDataPropertyNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;

          },
          notifyDrop: function (dd, e, data) {

            if (data.records[0].data.type != 'DataPropertyNode' && data.records[0].data.type != 'KeyDataPropertyNode') {
              return false;
            }
            else {
              me.getForm().findField('objectName').setValue(data.records[0].data.id);
              me.getForm().findField('related').setValue(data.records[0].data.property.Related);

              var msg = '<table style="font-size:13px"><tr><td>Property:</td><td><b>' + data.records[0].data.id.split('/')[data.records[0].data.id.split('/').length - 1] + '</b></td></tr>'
              msg += '</table>'
              me.body.child('div.property-target' + formid).update(msg)
              return true;
            }

          } //eo notifyDrop
        }); //eo propertydd

        var classTarget = this.body.child('div.class-target' + formid);
        var classdd = new Ext.dd.DropTarget(classTarget, {
          ddGroup: 'refdataGroup',
          notifyEnter: function (dd, e, data) {
            if (data.records[0].data.type != 'ClassNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;
          },
          notifyOver: function (dd, e, data) {
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
            }

            me.getForm().findField('classLabel').setValue(data.records[0].data.record.Label);
            me.getForm().findField('classUrl').setValue(data.records[0].data.record.Uri);
            var msg = '<table style="font-size:13px"><tr><td>Class Label:</td><td><b>' + data.records[0].data.record.Label + '</b></td></tr>';
            msg += '</table>';
            me.body.child('div.class-target' + formid).update(msg);
            return true;

          } //eo notifyDrop
        }); //eo propertydd
      }
    }];
    this.callParent(arguments);
  },

  onReset: function () {
    this.items.items[0].getForm().reset();
    this.fireEvent('Cancel', this);
  },

  onSave: function () {
    var me = this;
    var thisForm = this.items.items[0].getForm();

    thisForm.submit({
      waitMsg: 'Saving Data...',
      success: function (result, request) {
        that.onReload();
        win.close();
        //Ext.Msg.show({ title: 'Success', msg: 'Added ClassMap to Rolemap', icon: Ext.MessageBox.INFO, buttons: Ext.Msg.OK });
      },
      failure: function (result, request) {
        //Ext.Msg.show({ title: 'Failure', msg: 'Failed to Add ClassMap to RoleMap', icon: Ext.MessageBox.ERROR, buttons: Ext.Msg.CANCEL });
        var message = 'Failed to Add ClassMap to RoleMap';
        showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
      }
    });
  }
});
///what is this STUFF????????????
///forms are not posted this way??????????
///why is this required??????????
var submitAddClassMap = function (contextName, endpoint, baseUrl, propertyName, graphName, dataObject, roleName, classurl, classlabel, relation, parentClassId, index, tree, win) {
  Ext.Ajax.request({
    url: 'mapping/addclassmap',
      method: 'POST',
      params: {
        contextName: contextName,
        endpoint: endpoint,
        baseUrl: baseUrl,
        dataObject: dataObject,
        graphName: graphName,
        propertyName: propertyName,
        roleName: roleName,
        classID: classurl,
        classLabel: classlabel,
        relation: relation,
        parentClassId: parentClassId,
        index: index
      },
      success: function (result, request) {
        tree.onReload();
        win.close();
        //Ext.Msg.show({ title: 'Success', msg: 'Added ClassMap to Rolemap', icon: Ext.MessageBox.INFO, buttons: Ext.Msg.OK });
      },
      failure: function (result, request) {
        //Ext.Msg.show({ title: 'Failure', msg: 'Failed to Add ClassMap to RoleMap', icon: Ext.MessageBox.ERROR, buttons: Ext.Msg.CANCEL });
        var message = 'Failed to Add ClassMap to RoleMap';
        showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
      }    
  })
};