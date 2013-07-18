Ext.define('AM.view.mapping.MapValuelist', {
  extend: 'Ext.window.Window',
  alias: 'widget.valuelistform',
  title: 'Map Valuelist to RoleMap',
  contextName: null,
  baseUrl: null,
  mappingNode: null,
  graphName: null,
  roleName: null,
  classId: null,
  index: 0,
  endpoint: null,
  height: 170,
  width: 430,
  floating: true,
  layout: 'fit',
  initComponent: function () {
    var me = this;
    var formid = 'valuelisttarget-' + this.contextName + '-' + this.endpoint;
    this.items = [{
      xtype: 'form',
      method: 'POST',
      url: 'mapping/mapvaluelist',
      classId: this.classId,
      mappingNode: this.mappingNode,
      index: this.index,
      bodyStyle: 'padding:10px 5px 0',
      bbar: [
                { xtype: 'tbfill' },
                { text: 'Ok', scope: this, handler: this.onSave },
                { text: 'Cancel', scope: this, handler: this.onReset }
            ],
      items: [
                { xtype: 'hidden', name: 'valueListName', id: 'valueListName' },
                { xtype: 'hidden', name: 'relatedObject', id: 'relatedObject' },
                { xtype: 'hidden', name: 'propertyName', id: 'propertyName' },
                { xtype: 'hidden', name: 'mappingNode', id: 'mappingNode', value: this.mappingNode },
                { xtype: 'hidden', name: 'index', id: 'index', value: this.index },
                { xtype: 'hidden', name: 'classId', id: 'classID', value: this.classId },
                { xtype: 'hidden', name: 'graphName', id: 'graphName', value: this.graphName },
                { xtype: 'hidden', name: 'roleName', id: 'roleNAme', value: this.roleName },
                { xtype: 'hidden', name: 'contextName', id: 'contextName', value: this.contextName },
                { xtype: 'hidden', name: 'endpoint', id: 'endpoint', value: this.endpoint },
                { xtype: 'hidden', name: 'baseUrl', id: 'baseUrl', value: this.baseUrl }
            ],
      html: '<div class="property-target' + formid + '" '
                + 'style="border:1px silver solid;margin:5px;padding:8px;height:20px">'
                + 'Drop a Property Node here.</div>'
                + '<div class="class-target' + formid + '" '
                + 'style="border:1px silver solid;margin:5px;padding:8px;height:20px">'
                + 'Drop a ValueList Node here. </div>',
      afterRender: function (cmp) {
        Ext.FormPanel.prototype.afterRender.apply(this, arguments);
        var me = this;
        var propertyTarget = this.body.child('div.property-target' + formid);
        var propertydd = new Ext.dd.DropTarget(propertyTarget, {
          ddGroup: 'propertyGroup',
          notifyEnter: function (dd, e, data) {
            if (data.records[0].data.type != 'DataPropertyNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;
          },
          notifyOver: function (dd, e, data) {
            if (data.records[0].data.type != 'DataPropertyNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;
          },
          notifyDrop: function (dd, e, data) {
            if (data.records[0].data.type != 'DataPropertyNode') {
              return false;
            }
            else {
              me.getForm().findField('propertyName').setValue(data.records[0].data.property.Name);
              me.getForm().findField('relatedObject').setValue(data.records[0].data.record.Ralated);
              var msg = '<table style="font-size:13px"><tr><td>Property:</td><td><b>' + data.records[0].data.record.Name + '</b></td></tr>'
              msg += '</table>'
              me.body.child('div.property-target' + formid).update(msg)
              return true;
            }
          } //eo notifyDrop
        }); //eo propertydd


        var valueListTarget = this.body.child('div.class-target' + formid);
        var classdd = new Ext.dd.DropTarget(valueListTarget, {
          ddGroup: 'propertyGroup',
          notifyEnter: function (dd, e, data) {
            if (data.records[0].data.type != 'ValueListNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;
          },
          notifyOver: function (dd, e, data) {
            if (data.records[0].data.type != 'ValueListNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;
          },
          notifyDrop: function (classdd, e, data) {
            if (data.records[0].data.type != 'ValueListNode') {

              var message = 'Please slect a ValueList Node...';
              showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
              return false;
            }
            me.getForm().findField('valueListName').setValue(data.records[0].data.record.record.name);
            var msg = '<table style="font-size:13px"><tr><td>Value List:</td><td><b>' + data.records[0].data.record.record.name + '</b></td></tr>'
            msg += '</table>'
            me.body.child('div.class-target' + formid).update(msg)
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
        me.fireEvent('Save', me);
      },
      failure: function (result, request) {
        var message = 'Failed to Map ValueList to RoleMap';
        showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
      }
    });
  }
});