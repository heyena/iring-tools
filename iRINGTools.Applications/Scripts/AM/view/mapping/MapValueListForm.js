/*
 * File: Scripts/AM/view/mapping/MapValueListForm.js
 *
 * This file was generated by Sencha Architect version 2.2.2.
 * http://www.sencha.com/products/architect/
 *
 * This file requires use of the Ext JS 4.1.x library, under independent license.
 * License of Sencha Architect does not include license for Ext JS 4.1.x. For more
 * details see http://www.sencha.com/license or contact license@sencha.com.
 *
 * This file will be auto-generated each and everytime you save your project.
 *
 * Do NOT hand edit this file.
 */

Ext.define('AM.view.mapping.MapValueListForm', {
    extend: 'Ext.form.Panel',
    alias: 'widget.mapvaluelistform',

    bodyStyle: 'padding:10px 5px 0',
    method: 'POST',
    url: 'mapping/mapvaluelist',

    initComponent: function () {
        var me = this;

        me.initialConfig = Ext.apply({
            method: 'POST',
            url: 'mapping/mapvaluelist'
        }, me.initialConfig);

        Ext.applyIf(me, {
            items: [
        {
            xtype: 'hiddenfield',
            name: 'objectNames'
        },
        {
            xtype: 'hiddenfield',
            name: 'classIndex'
        },
        {
            xtype: 'hiddenfield',
            name: 'relatedObject'
        },
        {
            xtype: 'hiddenfield',
            name: 'propertyName'
        },
        {
            xtype: 'hiddenfield',
            name: 'mappingNode'
        },
        {
            xtype: 'hiddenfield',
            name: 'index'
        },
        {
            xtype: 'hiddenfield',
            name: 'classId'
        },
        {
            xtype: 'hiddenfield',
            name: 'graphName'
        },
        {
            xtype: 'hiddenfield',
            name: 'roleName'
        },
        {
            xtype: 'hiddenfield',
            name: 'contextName'
        },
        {
            xtype: 'hiddenfield',
            name: 'endpoint'
        },
        {
            xtype: 'hiddenfield',
            name: 'baseUrl'
        },
        {
            xtype: 'hiddenfield',
            name: 'parentNodeId'
        },
        {
            xtype: 'container',
            anchor: '100%',
            html: 'Drop a Property Node here.',
            itemId: 'mvlfpcontainer',
            style: 'border:1px silver solid;margin:5px;padding:8px;height:40px',
            styleHtmlContent: true
        },
        {
            xtype: 'container',
            anchor: '100%',
            html: 'Drop a ValueList Node here.',
            itemId: 'mvlfccontainer',
            style: 'border:1px silver solid;margin:5px;padding:8px;height:40px',
            styleHtmlContent: true
        }
      ],
            dockedItems: [
        {
            xtype: 'toolbar',
            dock: 'bottom',
            items: [
            {
                xtype: 'tbfill'
            },
            {
                xtype: 'button',
                handler: function (button, event) {
                    me.onSave();
                },
                text: 'Ok'
            },
            {
                xtype: 'button',
                handler: function (button, event) {
                    me.onReset();
                },
                text: 'Cancel'
            }
          ]
        }
      ],
            listeners: {
                afterrender: {
                    fn: me.onFormAfterRender,
                    scope: me
                }
            }
        });

        me.callParent(arguments);
    },

    onFormAfterRender: function (component, eOpts) {
        var me = this;
        var pcont = me.down('#mvlfpcontainer');
        var ccont = me.down('#mvlfccontainer');

        var propertydd = new Ext.dd.DropTarget(pcont.getEl(), {
            scope: me,
            ddGroup: 'propertyGroup',
            copy: false,
            overClass: 'over',
            notifyEnter: function (dd, e, data) {
                if (data.records[0].data.type != 'DataPropertyNode' && data.records[0].data.type != 'KeyDataPropertyNode'  && data.records[0].data.type != 'ExtensionNode')
                    return this.dropNotAllowed;
                else
                    return this.dropAllowed;
            },

            notifyOver: function (dragSource, event, data) {
                if (data.records[0].data.type != 'DataPropertyNode' && data.records[0].data.type != 'KeyDataPropertyNode' && data.records[0].data.type != 'ExtensionNode')
                    return this.dropNotAllowed;
                else
                    return this.dropAllowed;
            },
            notifyDrop: function (dd, e, data) {
                if (data.records[0].data.type != 'DataPropertyNode' && data.records[0].data.type != 'KeyDataPropertyNode' && data.records[0].data.type != 'ExtensionNode') {
                    return false;
                }
                else {
                    var ident = getLastXString(data.records[0].data.id, 1);
                    var object = getLastXString(data.records[0].data.id, 2);
                    var propertyName = object + '.' + ident;
                    me.getForm().findField('propertyName').setValue(propertyName);
                    me.getForm().findField('relatedObject').setValue(data.records[0].data.record.Ralated);
                    var msg = 'Property: ' + propertyName.bold(); //data.records[0].data.record.Name.bold();
                    pcont.update(msg);
                    return true;
                }
            } //eo notifyDrop
        }); //eo propertydd

        var classdd = new Ext.dd.DropTarget(ccont.getEl(), {
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
	          //var message = 'Please slect a ValueList Node...';
	          //showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
	           Ext.widget('messagepanel', { title: 'Warning', msg: 'Please slect a ValueList Node.'});
                    return false;
                }
                //me.getForm().findField('valueListName').setValue(data.records[0].data.record.name);
                me.getForm().findField('objectNames').setValue(data.records[0].data.id);
                var msg = 'Value List: ' + data.records[0].data.record.name.bold();
                ccont.update(msg);
                return true;

            } //eo notifyDrop
        }); //eo propertydd
    },

    onReset: function () {
        var me = this;
        win = me.up('window');
        win.fireEvent('reset', me);
    },

    onSave: function () {
        var me = this;
        var win = me.up('window');
        var form = me.getForm();

        form.submit({
            waitMsg: 'Saving Data...',
            success: function (result, request) {
                me.result = request.result;
                win.fireEvent('save', me);
            },
            failure: function (result, request) {
				//var message = 'Failed to Map ValueList to RoleMap';
				//showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
				var resp = Ext.decode(request.response.responseText);
				var userMsg = resp['message'];
				var detailMsg = resp['stackTraceDescription'];
				var expPanel = Ext.widget('exceptionpanel', { title: 'Error Notification'});
				Ext.ComponentQuery.query('#expValue',expPanel)[0].setValue(userMsg);
				Ext.ComponentQuery.query('#expValue2',expPanel)[0].setValue(detailMsg);
            }
        });
    },

    updateDDContainers: function (record) {
        var me = this;
        var pcon = me.down('#propertycontainer');
        var ccon = me.down('#classcontainer');
        var identifier, label;
        if (record.record) {
            identifier = getLastXString(record.record.classTemplateMaps[0].classMap.identifiers[0], 1).split('.')[1];
            label = record.record.classTemplateMaps[0].classMap.name;
        } else {
            return true;
        }
        identifier = 'Property: ' + identifier;
        pcon.update(identifier);

        var classlabel = 'Value List: ' + label;
        ccon.update(classlabel);
    }

});