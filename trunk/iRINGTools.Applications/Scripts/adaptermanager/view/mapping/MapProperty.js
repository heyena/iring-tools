
Ext.define('AM.view.mapping.MapProperty', {
    extend: 'Ext.window.Window',
    alias: 'widget.propertyform',
    closable: true,
    formid: null,
    modal: false,
    scope: null,
    application: null,
    title: 'Map Data Property to RoleMAp',
    // height: 120,
    width: 430,
    plain: true,
    initComponent: function () {
        var formid = 'propertytarget-' + this.scope.Name + '-' + this.application.Name;
        this.items = [
            {
                xtype: 'form',
                id: formid,
                method: 'POST',
                border: false,
                frame: false,
                bbar: [
                    { xtype: 'tbfill' },
                    { text: 'Ok', scope: this, handler: this.onSubmitPropertyMap },
                    { text: 'Cancel', scope: this, handler: this.onClose }
                    ],
                items: [
                          { xtype: 'hidden', name: 'propertyName', id: 'propertyName' },
                          { xtype: 'hidden', name: 'relatedObject', id: 'relatedObject' }
                      ],
                html: '<div class="property-target' + formid + '" '
                      + 'style="border:1px silver solid;margin:5px;padding:8px;height:20px">'
                    + 'Drop a Property Node here.</div>',

                afterRender: function (cmp, eOpts) {
                    Ext.form.Panel.prototype.afterRender.apply(this, arguments);

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
                                this.getForm().findField('propertyName').setValue(data.records[0].data.record.Name);
                                if (data.records[0].parentNode != undefined
                                && data.records[0].parentNode.data.record != undefined
                                && data.records[0].parentNode.data.type != 'DataObjectNode')
                                    this.getForm().findField('relatedObject').setValue(data.records[0].parentNode.data.record.Name);
                                var msg = '<table style="font-size:13px"><tr><td>Property:</td><td><b>' + data.records[0].data.record.Name + '</b></td></tr>'
                                msg += '</table>'
                                this.body.child('div.property-target' + formid).update(msg)
                                return true;
                            }
                        } //eo notifyDrop
                    }); //eo propertydd
                }
            }];
        this.callParent(arguments);
    }
});