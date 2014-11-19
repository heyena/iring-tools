
Ext.define('USM.view.SecurityGrid', {
    extend: 'Ext.grid.Panel',
    alias: 'widget.securitygrid',
    resizable: true,
    store: 'SecurityS',

    initComponent: function () {
        var me = this;

        Ext.applyIf(me, {
            columns: [
                {
                    xtype: 'gridcolumn',
                    dataIndex: 'name',
                    text: '',
                    flex: 2,
                    sortable: false,
                    menuDisabled: true,
                    renderer: function (value) {
                        return '<html> <b>' + value + '</b></html>';
                    }
                }
            ],
            listeners: {
                itemdblclick: {
                    fn: me.onDblClick,
                    scope: me
                }
            }

        });

        me.callParent(arguments);
    },

    onDblClick: function (dataview, record, item, index, e, eOpts) {
        var me = this;
        var data = record.data;
        var tabPanel = Ext.getCmp("maincontent");
        if (data.name == "Groups") {
            if (Ext.getCmp("groupgridid") == undefined) {
                var gridPanel = Ext.create('USM.view.groups.GroupGrid', {
                    title: 'Groups',
                    id: "groupgridid",
                    closable: true
                });
                tabPanel.add(gridPanel);
                gridPanel.store.reload();
            }
            tabPanel.setActiveTab(Ext.getCmp("groupgridid"));

        } else if (data.name == "Users") {
            if (Ext.getCmp("usergridid") == undefined) {
                var gridPanel = Ext.create('USM.view.users.UserGrid', {
                    title: 'Users',
                    id: "usergridid",
                    closable: true
                });
                tabPanel.add(gridPanel);
                gridPanel.store.reload();
            }
            tabPanel.setActiveTab(Ext.getCmp("usergridid"));

        } else if (data.name == "Roles") {
            if (Ext.getCmp("rolegridid") == undefined) {
                var gridPanel = Ext.create('USM.view.roles.RoleGrid', {
                    title: 'Roles',
                    id: "rolegridid",
                    closable: true
                });
                tabPanel.add(gridPanel);
                gridPanel.store.reload();
            }
            tabPanel.setActiveTab(Ext.getCmp("rolegridid"));

        } else {
            if (Ext.getCmp("permissiongridid") == undefined) {
                var gridPanel = Ext.create('USM.view.permissions.PermissionGrid', {
                    title: 'Permissions',
                    id: "permissiongridid",
                    closable: true
                });
                tabPanel.add(gridPanel);
                gridPanel.store.reload();
            }
            tabPanel.setActiveTab(Ext.getCmp("permissiongridid"));
        }
    }

});