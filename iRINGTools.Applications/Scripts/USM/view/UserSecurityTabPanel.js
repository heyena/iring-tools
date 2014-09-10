
Ext.define('USM.view.UserSecurityTabPanel', {
    extend: 'Ext.tab.Panel',
    alias: 'widget.usersecuritytabpanel',
    frame: false,
    border: false,
    //id: 'usmtabpanel',
    activeTab: 0,

    initComponent: function () {
        var me = this;
        Ext.applyIf(me, {
            items: [
//                {
//                    xtype: 'panel',
//                    border: false,
//                    layout: {
//                        type: 'fit'
//                    },
//                    animCollapse: false,
//                    collapseFirst: false,
//                    closable: true,
//                    title: 'Groups',
//                    items: [
//                        {
//                            xtype: 'groupgrid'
//                        }
//                    ],
//                    listeners: {
//                        activate: {
//                            fn: me.onPanelActivate,
//                            scope: me
//                        }
//                    }
//                },
//                {
//                    xtype: 'panel',
//                    border: false,
//                    layout: {
//                        type: 'fit'
//                    },
//                    animCollapse: false,
//                    closable: true,
//                    collapseFirst: false,
//                    title: 'Users',
//                    items: [
//                        {
//                            xtype: 'usergrid'
//                        }
//                    ],
//                    listeners: {
//                        activate: {
//                            fn: me.onPanelActivate,
//                            scope: me
//                        }
//                    }
//                },
//                {
//                    xtype: 'panel',
//                    border: false,
//                    layout: {
//                        type: 'fit'
//                    },
//                    animCollapse: false,
//                    closable: true,
//                    collapseFirst: false,
//                    title: 'Roles',
//                    items: [
//                        {
//                            xtype: 'rolegrid'
//                        }
//                    ],
//                    listeners: {
//                        activate: {
//                            fn: me.onPanelActivate,
//                            scope: me
//                        }
//                    }
//                },
//                {
//                    xtype: 'panel',
//                    border: false,
//                    layout: {
//                        type: 'fit'
//                    },
//                    animCollapse: false,
//                    closable: true,
//                    collapseFirst: false,
//                    title: 'Permissions',
//                    items: [
//                        {
//                            xtype: 'permissiongrid'
//                        }
//                    ],
//                    listeners: {
//                        activate: {
//                            fn: me.onPanelActivate,
//                            scope: me
//                        }
//                    }
//                }
            ]
        });

        me.callParent(arguments);
    },

    onPanelActivate: function (panel) {
        var me = this;
        var store = null;
        switch (panel.title) {
            case "Users":
                panel.down('usergrid').store.reload();
                break;
            case "Roles":
                panel.down('rolegrid').store.reload();
                break;
            case "Permissions":
                panel.down('permissiongrid').store.reload();
                break;
            default:
                panel.down('groupgrid').store.reload();
        }
    }

});