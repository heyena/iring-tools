
Ext.define('USM.view.permissions.PermissionSelectionPanel', {
    extend: 'Ext.form.Panel',
    alias: 'widget.permissionselectionpanel',

    bodyStyle: 'background:#fff;padding:10px',
    autoScroll: false,
    initComponent: function () {
        var me = this;

        Ext.applyIf(me, {
            defaults: {
                anchor: '100%',
                labelWidth: 110
            },
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
                            fn: me.onSelectRole,
                            scope: me
                        }
                    }
                }]
            },
			{
			    xtype: 'container',
			    autoScroll: true,
                layout: 'fit',
			    items: [{
			        xtype: 'gridpanel',
			        itemId: 'permgridid',
			        autoScroll: true,
			        height: 200,
			        store: 'PermissionS',
			        columns: [
						{
						    xtype: 'gridcolumn',
						    dataIndex: 'PermissionName',
						    text: 'Permission',
						    flex: 3
						    //menuDisabled: true
						},
                        {
                            xtype: 'gridcolumn',
                            dataIndex: 'PermissionId',
                            text: 'Permission',
                            hidden: true,
                            flex: 3
                            //menuDisabled: true
                        },
						{
						    xtype: 'checkcolumn',
						    header: 'Select',
						    dataIndex: 'Chk',
						    flex: 1
						}
					]
			    }]
			}, {
			    xtype: 'hiddenfield',
			    name: 'SelectedPermissions'
			}]
        });

        me.callParent(arguments);
    },

    onSelectRole: function (combo, records, eOpts) {
        var me = this;
        var roleId = records[0].data.RoleId;
        me.getRolePermissions(roleId, me);
    },

    getRolePermissions: function (roleId, scope) {
        var me = this;
        Ext.Ajax.request({
            url: 'usersecuritymanager/getRolePermissions',
            method: 'POST',
            params: {
                RoleId: roleId
            },
            success: function (response, options) {
                var responseObj = Ext.JSON.decode(response.responseText);
                var form = me;
                var grid = me.down("gridpanel");
                var selArr = [];
                form.getForm().findField('RoleId').setValue(roleId);
                grid.store.loadData(responseObj);
                
                //                grid.store.reload();
                //                var store = grid.store;
                //                store.on('load', function (store1, action) {
                //                    if (responseObj != null || responseObj != "") {
                //                        for (var j = 0; j < store1.getCount(); j++) {
                //                            var gPrmId = store1.getAt(j).get("PermissionId");
                //                            for (var i = 0; i < responseObj.length; i++) {
                //                                var permId = responseObj[i].PermissionId;

                //                                if (permId === gPrmId) {
                //                                    store1.getAt(j).set("chk", true);
                //                                } else {
                //                                    store1.getAt(j).set("chk", false);
                //                                }
                //                            }
                //                        }
                //                    }
                //                });

            },
            failure: function (response, options) {
            }
        });
    }
});