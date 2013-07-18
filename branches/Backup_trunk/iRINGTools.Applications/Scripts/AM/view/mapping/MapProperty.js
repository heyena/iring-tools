
Ext.define('AM.view.mapping.MapProperty', {
    extend: 'Ext.window.Window',
    alias: 'widget.propertyform',
    title: 'Map Data Property to RoleMAp',
    scope: null,
    mappingNode: null,
    classId: null,
    application: null,
    contextName: null,
    endpoint: null,
    baseUrl: null,
    graphName: null,
    height: 140,
    width: 430,
    floating: true,
    layout: 'fit',
    initComponent: function () {
        var formid = 'propertytarget-' + this.scope + '-' + this.application;
        this.items = [{
            xtype: 'form',                   
            border: false,
            frame: false,
            bodyStyle: 'padding:10px 5px 0',
            bbar: [
                { xtype: 'tbfill' },
                { text: 'Ok',
                  scope: this,
                  handler: function (btn, e) {
                    var form = btn.findParentByType('form');
                    var win = btn.findParentByType('window');
                    var mapPropertyForm = form.getForm();

                    if (mapPropertyForm.isValid()) {
                      var propertyName = mapPropertyForm.findField('propertyName').getValue();
                      var relatedObject = mapPropertyForm.findField('relatedObject').getValue();
                      var roleName = getLastXString(this.mappingNode.data.id, 1);
                      var index = this.mappingNode.parentNode.parentNode.indexOf(this.mappingNode.parentNode);
                      
                      submitMapProperty(propertyName, this.graphName, relatedObject, roleName, this.classId, index, this.contextName, this.endpoint, this.baseUrl, this.tree, win);
                    }
                  } 
                },
                { text: 'Cancel', scope: this, handler: this.onReset }
            ],
            items: [
              { xtype: 'hidden', name: 'propertyName' },
              { xtype: 'hidden', name: 'relatedObject' }               
            ],
            html: '<div class="property-target' + formid + '" '
                        + 'style="border:1px silver solid;margin:5px;padding:8px;height:40px">'
                    + 'Drop a Property Node here.</div>',

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
                            me.getForm().findField('propertyName').setValue(data.records[0].data.record.Name);
                            
                            if (data.records[0].parentNode != undefined
                                && data.records[0].parentNode.data.record != undefined
                                && data.records[0].parentNode.data.type != 'DataObjectNode')
                                me.getForm().findField('relatedObject').setValue(data.records[0].parentNode.data.record.Name);
                            
                            var msg = '<table style="font-size:13px"><tr><td>Property:</td><td><b>' + data.records[0].data.record.Name + '</b></td></tr>'
                            msg += '</table>'
                            me.body.child('div.property-target' + formid).update(msg)
                            return true;
                        }
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
            success: function (f, a) {
               
                me.fireEvent('Save', me);
            },
            failure: function (f, a) {
//                var message = 'Error saving changes!';
//                showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
            }
        });
    }
  });
      ///what is this STUFF????????????
      ///forms are not posted this way??????????
      ///why is this required??????????
var submitMapProperty = function (propertyName, graphName, relatedObject, roleName, classId, index, contextName, endpoint, baseUrl, tree, win) {
  Ext.Ajax.request({
    url: 'mapping/mapproperty',
    timeout: 600000,
    method: 'POST',
    params: {
      propertyName: propertyName,
      graphName: graphName,
      relatedObject: relatedObject,
      roleName: roleName,
      classId: classId,
      index: index,
      contextName: contextName,
      endpoint: endpoint,
      baseUrl: baseUrl
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
        showDialog(400, 100, 'Mapping property result - Error', msg, Ext.Msg.OK, null);
      }
      //Ext.Msg.show({ title: 'Success', msg: 'Mapped ValueList to Rolemap', icon: Ext.MessageBox.INFO, buttons: Ext.Msg.OK });
    },
    failure: function (result, request) {
      //Ext.Msg.show({ title: 'Failure', msg: 'Failed to Map ValueList to RoleMap', icon: Ext.MessageBox.ERROR, buttons: Ext.Msg.CANCEL });
      var message = 'Failed to Map property';
      showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
    }
  })
};