
Ext.define('AM.view.mapping.MapProperty', {
    extend: 'Ext.window.Window',
    alias: 'widget.propertyform',
    title: 'Map Data Property to RoleMAp',
    scope: null,
    mappingNode: null,
    classId: null,
    application: null,
    height: 120,
    width: 430,
    floating: true,
    layout: 'fit',
    initComponent: function () {
        var formid = 'propertytarget-' + this.scope + '-' + this.application;
        this.items = [{
            xtype: 'form',
            method: 'POST',
            mappingNode: this.mappingNode,
            classId: this.classId,
            url: 'mapping/mapproperty',
            border: false,
            frame: false,
            bodyStyle: 'padding:10px 5px 0',
            bbar: [
                { xtype: 'tbfill' },
                { text: 'Ok', scope: this, handler: this.onSave },
                { text: 'Cancel', scope: this, handler: this.onClose }
            ],
            items: [
                    { xtype: 'hidden', name: 'propertyName', id: 'propertyName' },
                    { xtype: 'hidden', name: 'relatedObject', id: 'relatedObject' },
                    { xtype: 'hidden', name: 'mappingNode', id: 'mappingNode' },
                    { xtype: 'hidden', name: 'classId', id: 'classId' }
                ],
            html: '<div class="property-target' + formid + '" '
                        + 'style="border:1px silver solid;margin:5px;padding:8px;height:20px">'
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
                            me.getForm().findField('mappingNode').setValue(me.mappingNode);
                            me.getForm().findField('classId').setValue(me.classId);
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
        var me = this;    // consists the main/previous class object
        var thisForm = this.items.items[0].getForm();
//        if (thisForm.findField('propertyName').getValue() == '') {

//            return;
//        }
        thisForm.submit({
            waitMsg: 'Saving Data...',
            success: function (f, a) {
                //Ext.Msg.alert('Success', 'Changes saved successfully!');
                me.fireEvent('Save', me);
            },
            failure: function (f, a) {
                //Ext.Msg.alert('Warning', 'Error saving changes!')
                var message = 'Error saving changes!';
                showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
            }
        });
    }
});