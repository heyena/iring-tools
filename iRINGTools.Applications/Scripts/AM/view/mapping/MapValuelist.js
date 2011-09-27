Ext.define('AM.view.mapping.MapValuelist', {
    extend: 'Ext.window.Window',
    alias: 'widget.valuelistform',
    title: 'Map Valuelist to RoleMap',
    scope: null,
    mappingNode: null,
    classId: null,
    index: 0,
    application: null,
    height: 170,
    width: 430,
    floating: true,
    layout: 'fit',
    initComponent: function () {
        var formid = 'valuelisttarget-' + this.scope + '-' + this.application;
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
                { xtype: 'hidden', name: 'objectNames', id: 'objectNames' },
                { xtype: 'hidden', name: 'propertyName', id: 'propertyName' },
                { xtype: 'hidden', name: 'mappingNode', id: 'mappingNode', value: this.mappingNode },
                { xtype: 'hidden', name: 'index', id: 'index', value: this.index },
                { xtype: 'hidden', name: 'classId', id: 'classId', value: this.classId }
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
                            me.getForm().findField('propertyName').setValue(data.records[0].data.id);
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

                            var message = 'Please slect a RDL Class...';
                            showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
                            return false;
                        }
                        me.getForm().findField('objectNames').setValue(data.records[0].data.id);
                        var mapNode = Ext.get('mappingPanel')
                        var msg = '<table style="font-size:13px"><tr><td>Value List:</td><td><b>' + data.records[0].data.record.name + '</b></td></tr>'
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
            success: function (f, a) {

                me.fireEvent('Save', me);
            },
            failure: function (f, a) {

            }
        });
    }
});