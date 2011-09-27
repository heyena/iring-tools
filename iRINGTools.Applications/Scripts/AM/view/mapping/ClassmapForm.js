Ext.define('AM.view.mapping.ClassmapForm', {
    extend: 'Ext.window.Window',
    alias: 'widget.classmapform',
    scope: null,
    application: null,
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
        var formid = 'propertytarget-' + this.scope + '-' + this.application;
      
        this.items = [{
            xtype: 'form',
            bodyStyle: 'padding:10px 5px 0',
            url: 'mapping/addclassmap',
            method: 'POST',
            bbar: [
                    { xtype: 'tbfill' },
                    { text: 'Ok', scope: this, handler: this.onSave },
                    { text: 'Cancel', scope: this, handler: this.onReset }
                    ],
            items: [
                    { xtype: 'hidden', name: 'objectName', id: 'objectName' },
                    { xtype: 'hidden', name: 'classLabel', id: 'classLabel' },
                    { xtype: 'hidden', name: 'classUrl', id: 'classUrl' },
                    { xtype: 'hidden', name: 'mappingNode', id: 'mappingNode', value: this.mappingNode },
                    { xtype: 'hidden', name: 'index', id: 'index', value: this.index },
                    { xtype: 'hidden', name: 'parentClassId', id: 'parentClassId', value: this.parentClassId }
                    ],
            html: '<div class="property-target' + formid + '" '
                      + 'style="border:1px silver solid;margin:5px;padding:8px;height:20px">'
                      + 'Drop a Property Node here.</div>'
                      + '<div class="class-target' + formid + '" '
                      + 'style="border:1px silver solid;margin:5px;padding:8px;height:20px">'
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
                            var msg = '<table style="font-size:13px"><tr><td>Property:</td><td><b>' + data.records[0].data.id.split('/')[5] + '</b></td></tr>'
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
                        if (data.records[0].data.record.type != 'ClassNode')
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
                            //                            showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
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