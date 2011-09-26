Ext.define('AM.view.mapping.ClassmapForm', {
    extend: 'Ext.form.Panel',
    alias: 'widget.classmapform',
    formid: null,
    width: 430,
    title: 'Add new ClassMap to RoleMAp',
    method: 'POST',
    url: 'mapping/addclassmap',
    border: false,
    frame: false,
    bodyStyle: 'padding:10px 5px 0',
    bbar: [
        { xtype: 'tbfill' },
        { text: 'Ok', scope: this, handler: this.onSubmitClassMap },
        { text: 'Cancel', scope: this, handler: this.onClose }
        ],
    items: [
        { xtype: 'hidden', name: 'objectName', id: 'objectName' },
        { xtype: 'hidden', name: 'classLabel', id: 'classLabel' },
        { xtype: 'hidden', name: 'classUrl', id: 'classUrl' }
        ],
    html: '<div class="property-target' + this.formid + '" '
          + 'style="border:1px silver solid;margin:5px;padding:8px;height:20px">'
          + 'Drop a Property Node here.</div>'
          + '<div class="class-target' + this.formid + '" '
          + 'style="border:1px silver solid;margin:5px;padding:8px;height:20px">'
          + 'Drop a Class Node here. </div>',

    afterRender: function (cmp, eOpts) {
        Ext.form.Panel.prototype.afterRender.apply(this, arguments);
        var me = this;
        var propertyTarget = this.body.child('div.property-target' +this.formid);
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
                    this.getForm().findField('objectName').setValue(data.records[0].data.id);
                    var msg = '<table style="font-size:13px"><tr><td>Property:</td><td><b>' + data.records[0].data.id.split('/')[5] + '</b></td></tr>'
                    msg += '</table>'
                    this.body.child('div.property-target' +this.formid).update(msg)
                    return true;
                }
            } //eo notifyDrop
        }); //eo propertydd
        var classTarget = this.body.child('div.class-target' +this.formid);
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
                    showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
                    return false;
                }
                this.getForm().findField('classLabel').setValue(data.records[0].data.record.Label);
                this.getForm().findField('classUrl').setValue(data.records[0].data.record.Uri);

                var msg = '<table style="font-size:13px"><tr><td>Class Label:</td><td><b>' + data.records[0].data.record.Label + '</b></td></tr>';
                msg += '</table>';
                this.body.child('div.class-target' +this.formid).update(msg);
                return true;

            } //eo notifyDrop
        }); //eo propertydd
    },
    initComponent: function () {
        this.callParent(arguments);
    },

    onReset: function () {
        this.items.first().getForm().reset();
        this.fireEvent('Cancel', this);
    },

    onSave: function () {
        var me = this;    // consists the main/previous class object
        var thisForm = this.items.first().getForm();
        if (thisForm.findField('valueList').getValue() == '') {

            return;
        }
        thisForm.submit({
            waitMsg: 'Saving Data...',
            success: function (f, a) {
                //Ext.Msg.alert('Success', 'Changes saved successfully!');
                me.fireEvent('Save', me);
            },
            failure: function (f, a) {
                //Ext.Msg.alert('Warning', 'Error saving changes!')
                var message = 'Error saving changes!';
            }
        });
    }
});