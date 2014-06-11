
Ext.define('USM.view.UserSecurityTabPanel', {
    extend: 'Ext.tab.Panel',
    alias: 'widget.usersecuritytabpanel',
    frame: false,
    border: false,
    id: 'tiptabpanelid',
    activeTab: 0,

    initComponent: function () {
        var me = this;
        Ext.applyIf(me, {
            items: [
                {
                    xtype: 'panel',
                    border: false,
                    layout: {
                        type: 'fit'
                    },
                    animCollapse: false,
                    collapseFirst: false,
                    title: 'Groups',
                    items: [
                        {
                            xtype : 'groupgrid'
                        }
                    ]
                },
                {
                    xtype: 'panel',
                    border: false,
                    layout: {
                        type: 'fit'
                    },
                    animCollapse: false,
                    collapseFirst: false,
                    title: 'Users',
                    items: [
                        {
                            xtype: 'usergrid'
                        }
                    ]
                },
                {
                    xtype: 'panel',
                    border: false,
                    layout: {
                        type: 'fit'
                    },
                    animCollapse: false,
                    collapseFirst: false,
                    title: 'Roles',
                    items: [
                        {
                            xtype: 'rolegrid'
                        }
                    ]
                },
                {
                    xtype: 'panel',
                    border: false,
                    layout: {
                        type: 'fit'
                    },
                    animCollapse: false,
                    collapseFirst: false,
                    title: 'Permissions',
                    items: [
                        {
                            xtype: 'permissiongrid'
                        }
                    ]
                }
            ]
        });

        me.callParent(arguments);
    }

});