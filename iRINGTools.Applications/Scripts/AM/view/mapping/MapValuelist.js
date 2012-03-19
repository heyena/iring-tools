Ext.define('AM.view.mapping.MapValuelist', {
  extend: 'Ext.window.Window',
  alias: 'widget.valuelistform',
  title: 'Map Valuelist to RoleMap',
  contextName: null,
  baseUrl: null,
  mappingNode: null,
  graphName: null,
  classId: null,
  index: 0,
  endpoint: null,
  tree: null,
  height: 190,
  width: 430,
  floating: true,
  layout: 'fit',
  initComponent: function () {
    var formid = 'valuelisttarget-' + this.contextName + '-' + this.endpoint;
    var that = this;
    this.items = [{
      xtype: 'form',      
      bodyStyle: 'padding:10px 5px 0',
      bbar: [
              { xtype: 'tbfill' },
              { text: 'Ok',
                scope: this,
                handler: function (btn, e) {
                  var form = btn.findParentByType('form');
                  var win = btn.findParentByType('window');
                  var mapValuelistForm = form.getForm();

                  if (mapValuelistForm.isValid()) {
                    var valueListName = mapValuelistForm.findField('valueListName').getValue();
                    var relation = mapValuelistForm.findField('related').getValue();
                    var propertyName = mapValuelistForm.findField('propertyName').getValue();
                    
                    var roleName = getLastXString(this.mappingNode.data.id, 1);
                    submitValuelistMap(valueListName, propertyName, relation, this.classId, roleName, this.graphName, this.contextName, this.endpoint, this.baseUrl, this.index, this.tree, win);
                  }
                }
              },
              { text: 'Cancel', scope: this, handler: this.onReset }
            ],
      items: [
                { xtype: 'hidden', name: 'valueListName' },
                { xtype: 'hidden', name: 'propertyName' },
                { xtype: 'hidden', name: 'related' },
            ],
      html: '<div class="property-target' + formid + '" '
                + 'style="border:1px silver solid;margin:5px;padding:8px;height:40px">'
                + 'Drop a Property Node here.</div>'
                + '<div class="class-target' + formid + '" '
                + 'style="border:1px silver solid;margin:5px;padding:8px;height:40px">'
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
              var propertyName = data.records[0].data.property.Name;
              me.getForm().findField('propertyName').setValue(propertyName);
              me.getForm().findField('related').setValue(data.records[0].data.property.Related);

              var msg = '<table style="font-size:13px"><tr><td>Property:</td><td><b>' + propertyName + '</b></td></tr>'
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

              var message = 'Please slect a RDL Class...';
              showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
              return false;
            }

            var valueListName = data.records[0].data.property.Name;
            me.getForm().findField('valueListName').setValue(valueListName);
            var msg = '<table style="font-size:13px"><tr><td>Value List:</td><td><b>' + valueListName + '</b></td></tr>'
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
  }
});

var submitValuelistMap = function (valueListName, propertyName, relation, classId, roleName, graphName, contextName, endpoint, baseUrl, index, tree, win) {
  Ext.Ajax.request({
    url: 'mapping/mapvaluelist',    
    method: 'POST',
    params: {
      classId: classId,
      roleName: roleName,
      graphName: graphName,
      propertyName: propertyName,
      relatedObject: relation,
      contextName: contextName,
      endpoint: endpoint,
      baseUrl: baseUrl,
      valueListName: valueListName,
      index: index
    },
    success: function (result, request) {
      var rtext = result.responseText;
      if (rtext.toUpperCase().indexOf('FALSE') == -1) {
        tree.onReload();
        win.close();
      }
      else {
        var ind = rtext.indexOf('}');
        var ine = rtext.indexOf('at');
        var len = rtext.length - ind - 1;
        var msg = rtext.substring(ind + 1, ine - 7);
        showDialog(400, 100, 'Valuelist mapping result - Error', msg, Ext.Msg.OK, null);
      }
      //Ext.Msg.show({ title: 'Success', msg: 'Mapped ValueList to Rolemap', icon: Ext.MessageBox.INFO, buttons: Ext.Msg.OK });
    },
    failure: function (result, request) {
      //Ext.Msg.show({ title: 'Failure', msg: 'Failed to Map ValueList to RoleMap', icon: Ext.MessageBox.ERROR, buttons: Ext.Msg.CANCEL });
      var message = 'Failed to Map ValueList to RoleMap';
      showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
    }
  })
};