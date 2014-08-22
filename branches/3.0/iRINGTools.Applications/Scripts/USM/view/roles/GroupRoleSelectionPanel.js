Ext.define('USM.view.roles.GroupRoleSelectionPanel', {
    extend: 'Ext.form.Panel',
    alias: 'widget.grouproleselectionpanel',

    bodyStyle: 'background:#fff;padding:10px',
    width: 600,
    record: null,
    requires: [
           'Ext.ux.form.ItemSelector'
    ],
    initComponent: function () {
        var me = this;
        utilsObj.grpRole = "Groups Role";

        Ext.applyIf(me, {
            items: [{
                xtype: 'panel',
                layout: 'form',
                bodyPadding: 2,
                border: false,
                items: [{
                    xtype: 'combobox',
                    disabled: false,
                    name: 'RoleId',
                    allowBlank: false,
                    fieldLabel: 'Role',
                    emptyText: 'Select Role',
                    editable: false,
                    width: 100,
                    displayField: 'RoleName',
                    forceSelection: false,
                    store: 'RoleS',
                    valueField: 'RoleId',
                    listeners: {
                        select: {
                            fn: me.onSelectGroup,
                            scope: me
                        }
                    }
                }]
            },
            {
                xtype: 'panel',
                layout: 'fit',
                bodyPadding: 2,
                border: false,
                items: [{
                    xtype: 'itemselector',
                    itemId: 'groupselector',
                    name: 'SelectedGroups',
                    anchor: '100%',
                    imagePath: '../ux/images/',
                    store: 'GroupS',
                    height: 200,
                    displayField: 'GroupName',
                    valueField: 'GroupId',
                    msgTarget: 'side',
                    fromTitle: 'Groups',
                    toTitle: utilsObj.grpRole
                }]
            }],

            listeners: {
                afterrender: me.loadValues,
                scope: me
            },
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
                                me.onSave(button);
                            },
                            iconCls: 'icon-accept',
                            text: 'Apply'
                        },
                        {
                            xtype: 'button',
                            handler: function (button, event) {
                                me.onSave(button);
                            },
                            iconCls: 'icon-accept',
                            text: 'Save'
                        },
                        {
                            xtype: 'button',
                            handler: function (button, event) {
                                me.onReset();
                            },
                            iconCls: 'icon-cancel',
                            text: 'Cancel'
                        }
                    ]
                }
            ]
        });

        me.callParent(arguments);
    },

    onReset: function () {
        var me = this;
        var win = me.up('window');
        me.getForm().reset();
        win.destroy();
    },

    onSave: function (btn) {
        var me = this;
        var message;
        var msg;
        var form = me;
        if (me.getForm().isValid()) {
            msg = new Ext.window.MessageBox();
            msg.wait('Saving Groups ....');
            form.submit({
                url: 'usersecuritymanager/saveRoleGroups',
                success: function (f, a) {
                    msg.close();
                    me.getForm().reset();
                    if (btn.text == "Save") {
                        me.up('window').destroy();
                    }
                    var message = 'Selected Groups to Role saved successfully.';
                    showDialog(400, 50, 'Alert', message, Ext.Msg.OK, null);
                    return;
                },
                failure: function (f, a) {
                    msg.close();
                    me.up('window').destroy();
                }
            });
        } else {
            message = 'Please fil the form to save.';
            showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
        }
    },

    setRecord: function (record) {
        this.record = record;
        this.loadValues();
    },

    onSelectGroup: function (combo, records, eOpts) {
        var me = this;
        var roleName = records[0].data.RoleName;
        //utilsObj.grpUser = grpName + " Users";
        //me.down('itemselector').updateLayout();
        //me.updateLayout();
    },

    loadValues: function () {
        var me = this;

        //        if (me.record != null) {
        //            var selector = me.down('#keyselector');
        //            var itemList = me.record.parentNode.raw.properties.dataProperties;

        //            var availItems = [];
        //            Ext.each(itemList, function (item) {
        //                availItems.push({ name: item.columnName });
        //            });

        //            selector.store.loadData(availItems);
        //            selector.reset();

        //            var selectedItems = [];
        //            Ext.each(me.record.childNodes, function (child) {
        //                selectedItems.push(child.raw.properties.columnName);
        //            });

        //            selector.setValue(selectedItems);
        //        }
    }
});