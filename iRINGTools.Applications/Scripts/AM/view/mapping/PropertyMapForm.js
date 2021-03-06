/*
 * File: Scripts/AM/view/mapping/PropertyMapForm.js
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

Ext.define('AM.view.mapping.PropertyMapForm', {
    extend: 'Ext.form.Panel',
    alias: 'widget.propertymapform',

    bodyPadding: 10,
    method: 'POST',
    url: 'mapping/mapproperty',

    initComponent: function () {
        var me = this;

        me.initialConfig = Ext.apply({
            method: 'POST',
            url: 'mapping/mapproperty'
        }, me.initialConfig);

        Ext.applyIf(me, {
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
            items: [
        {
            xtype: 'hiddenfield',
            name: 'propertyName'
        },
        {
            xtype: 'hiddenfield',
            name: 'classIndex'
        },
        {
            xtype: 'hiddenfield',
            name: 'graphName'
        },
        {
            xtype: 'hiddenfield',
            name: 'relatedObject'
        },
        {
            xtype: 'hiddenfield',
            name: 'roleName'
        },
        {
            xtype: 'hiddenfield',
            name: 'classId'
        },
        {
            xtype: 'hiddenfield',
            name: 'index'
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
            name: 'mappingNode'
        },
        {
            xtype: 'hiddenfield',
            name: 'baseUrl'
        },
        {
            xtype: 'hiddenfield',
            name: 'parentNodeId'
        }
        ,
        {
            xtype: 'container',
            anchor: '100%',
            html: 'Drop a Property Node here.',
            itemId: 'pmfpcontainer',
            style: 'border:1px silver solid;margin:5px;padding:8px;height:40px',
            styleHtmlContent: true
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
        var pcont = me.down('#pmfpcontainer');
        var propertyDropTarget = new Ext.dd.DropTarget(pcont.getEl(), {
            scope: me,
            ddGroup: 'propertyGroup',
            copy: false,
            overClass: 'over',
            notifyOver: function (dragSource, event, data) {
                if (data.records[0].data.type != 'DataPropertyNode' && data.records[0].data.type != 'KeyDataPropertyNode' && data.records[0].data.type != 'ExtensionNode')
                    return this.dropNotAllowed;
                else
                    return this.dropAllowed;
            },
            notifyDrop: function (dragSource, event, data) {
                if (data.records[0].data.type != 'DataPropertyNode' && data.records[0].data.type != 'KeyDataPropertyNode' && data.records[0].data.type != 'ExtensionNode') {
                    return false;
                }
                else {
                    //me.getForm().findField('propertyName').setValue(data.records[0].data.record.Name);
                    var propertyArr = data.records[0].data.id.split('/');
                    var propertyName = "";
                    var selNode = Ext.getCmp("directoryTreeID").down("directorytree").getSelectedNode();
                    if (selNode.parentNode.data.type == "RelationshipNode") {
                        propertyName = propertyArr[propertyArr.length - 3] + '.' + propertyArr[propertyArr.length - 2] + '.' + propertyArr[propertyArr.length - 1];
                    } else {
                        //   propertyName = propertyArr[propertyArr.length - 2] + '.' + propertyArr[propertyArr.length - 1];
                        propertyName = propertyArr[propertyArr.length - 2] + '.' + data.records[0].data.property.Name
                    }
                    me.getForm().findField('propertyName').setValue(propertyName);

                    if (data.records[0].parentNode !== undefined && data.records[0].parentNode.data.record !== undefined && data.records[0].parentNode.data.type != 'DataObjectNode')
                        me.getForm().findField('relatedObject').setValue(data.records[0].parentNode.data.record.Name);

                    var msg = 'Property: ' + propertyName.bold(); //data.records[0].data.record.Name;
                    pcont.update(msg);
                    return true;
                }
            },
            notifyEnter: function (dd, e, data) {
                if (data.records[0].data.type != 'DataPropertyNode' && data.records[0].data.type != 'KeyDataPropertyNode' && data.records[0].data.type != 'ExtensionNode')
                    return this.dropNotAllowed;
                else
                    return this.dropAllowed;
            }
        });
  
    },

    onReset: function () {
        var me = this;
        var win = me.up('window');
        me.getForm().reset();
        win.close();
    },

    onSave: function () {
        var me = this;
        var win = me.up('window');
        var message;
        if (me.getForm().isValid()) {
            me.submit({
                waitMsg: 'Saving Data...',
                success: function (f, a) {
                    me.result = a.result;
                    win.fireEvent('save', me);
                },
                failure: function (f, a) {
                    var resp = Ext.decode(a.response.responseText);
                    var userMsg = resp['message'];
                    var detailMsg = resp['stackTraceDescription'];
                    var expPanel = Ext.widget('exceptionpanel', { title: 'Error Notification' });
                    Ext.ComponentQuery.query('#expValue', expPanel)[0].setValue(userMsg);
                    Ext.ComponentQuery.query('#expValue2', expPanel)[0].setValue(detailMsg);
                }
            });
        } else {
            message = 'Form is not complete. Cannot save record.';
            //showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);   
            Ext.widget('messagepanel', { title: 'Warning', msg: message });
        }
    }
});