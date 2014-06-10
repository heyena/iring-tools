﻿Ext.define('USM.controller.UserSecurity', {
    extend: 'Ext.app.Controller',

    models: ['SecurityM', 'GroupM', 'RoleM', 'UserM', 'PermissionM'],
    stores: ['SecurityS', 'GroupS', 'RoleS', 'UserS', 'PermissionS'],
    views: [
        'UserSecurityTabPanel',
        'SecurityGrid',
        'groups.GroupGrid',

        'menus.SecurityMenu',
        'menus.RoleMenu',
        //'menus.PermissionMenu',
        'menus.GroupMenu',
        'groups.GroupForm',
        'groups.GroupWindow',
        'roles.RoleForm',
        'roles.RoleWindow'//,
        //'permissions.PermissionForm',
        //'permissions.PermissionWindow'
    ],

    refs: [
			{
			    ref: 'usersecuritytabpanel',
			    selector: 'viewport > usersecuritytabpanel'
			}
    ],

    init: function (application) {
        var me = this;
        this.control({
            "menuitem[action=addGroup]": {
                click: this.addGroup
            },
            "menuitem[action=editGroup]": {
                click: this.editGroup
            },
            "menuitem[action=deleteGroup]": {
                click: this.deleteGroup
            },
            "menuitem[action=addRole]": {
                click: this.addRole
            },
            "menuitem[action=editRole]": {
                click: this.editRole
            },
            "menuitem[action=deleteRole]": {
                click: this.deleteRole
            },
            "menuitem[action=addPermission]": {
                click: this.addPermission
            },
            "menuitem[action=editPermission]": {
                click: this.editPermission
            },
            "menuitem[action=deletePermission]": {
                click: this.deletePermission
            },
            "menuitem[action=editSecUserGroup]": {
                click: this.editUserGroup
            },
            "menuitem[action=editSecGroupRoles]": {
                click: this.editGroupRoles
            },
            "viewport securitygrid": {
                itemcontextmenu: me.onSecItemClick
            },
            "usersecuritytabpanel groupgrid": {
                itemcontextmenu: me.onGrpItemClick
            },
            "usersecuritytabpanel permissiongrid": {
                itemcontextmenu: me.onPermItemClick
            },
            "usersecuritytabpanel rolegrid": {
                itemcontextmenu: me.onRoleItemClick
            }
        });
    },

    onSecItemClick: function (dataview, record, item, index, e, eOpts) {
        e.stopEvent();
        var me = this;
        var secMenu = Ext.widget('securitymenu');
        secMenu.showAt(e.getXY());
        dataview.getSelectionModel().select(index);
    },

    onGrpItemClick: function (dataview, record, item, index, e, eOpts) {
        e.stopEvent();
        dataview.getSelectionModel().select(index);
        var grpMenu = Ext.widget('groupmenu');
        grpMenu.showAt(e.getXY());
    },

    onRoleItemClick: function (dataview, record, item, index, e, eOpts) {
        e.stopEvent();
        dataview.getSelectionModel().select(index);
        var roleMenu = Ext.widget('rolemenu');
        roleMenu.showAt(e.getXY());
    },

    onPermItemClick: function (dataview, record, item, index, e, eOpts) {
        e.stopEvent();
        dataview.getSelectionModel().select(index);
        var permMenu = Ext.widget('permissionmenu');
        permMenu.showAt(e.getXY());
    },

    addGroup: function (btn) {
        var me = this;
        var win = Ext.widget('groupwindow');
        win.show();
    },

    editGroup: function (btn) {
        var me = this;
        var rec = Ext.getCmp('viewportid').down('groupgrid').getSelectionModel().getSelection();
        var groupId = rec[0].data.GroupId;
        var win = Ext.widget('groupwindow');
        var form = win.down('groupform');
        form.getForm().setValues(rec[0].data);
        form.getForm().findField('actionType').setValue('EDIT');
        win.show();
        //        Ext.Ajax.request({
        //            url: 'usersecuritymanager/editGroup',
        //            method: 'POST',
        //            params: {
        //                groupId: groupId
        //            },
        //            success: function (response, options) {
        //                var responseObj = Ext.JSON.decode(response.responseText);
        //                var win = Ext.widget('groupwindow');
        //                var form = win.down('groupform');
        //                form.getForm().findField('actionType').setValue('EDIT');
        //                win.show();
        //            },

        //            failure: function (response, options) {
        //            }
        //        });
    },

    deleteGroup: function (btn) {
        var me = this;
        var rec = Ext.getCmp('viewportid').down('groupgrid').getSelectionModel().getSelection();
        var groupId = rec[0].data.GroupId;
        Ext.Ajax.request({
            url: 'usersecuritymanager/deleteGroup',
            method: 'POST',
            params: {
                groupId: groupId
            },
            success: function (response, options) {
                var responseObj = Ext.JSON.decode(response.responseText);
            },

            failure: function (response, options) {
            }
        });
    },

    addRole: function (btn) {
        var me = this;
        var win = Ext.widget('rolewindow');
        win.show();
    },

    editRole: function (btn) {
        var me = this;
        var rec = Ext.getCmp('viewportid').down('rolegrid').getSelectionModel().getSelection();
        var roleId = rec[0].data.RoleId;
        var win = Ext.widget('rolewindow');
        var form = win.down('roleform');
        form.getForm().setValues(rec[0].data);
        form.getForm().findField('actionType').setValue('EDIT');
        win.show();
//        Ext.Ajax.request({
//            url: 'usersecuritymanager/editRole',
//            method: 'POST',
//            params: {
//                RoleId: roleId
//            },
//            success: function (response, options) {
//                var responseObj = Ext.JSON.decode(response.responseText);
//                var win = Ext.widget('rolewindow');
//                var form = win.down('roleform');
//                form.getForm().findField('actionType').setValue('EDIT');
//                win.show();
//            },

//            failure: function (response, options) {
//            }
//        });
    },

    deleteRole: function (btn) {
        var me = this;
        var rec = Ext.getCmp('viewportid').down('rolegrid').getSelectionModel().getSelection();
        var roleId = rec[0].data.RoleId;
        Ext.Ajax.request({
            url: 'usersecuritymanager/deleteRole',
            method: 'POST',
            params: {
                RoleId: roleId
            },
            success: function (response, options) {
                var responseObj = Ext.JSON.decode(response.responseText);
            },

            failure: function (response, options) {
            }
        });
    },

    addPermission: function (btn) {
        var me = this;
        var win = Ext.widget('permissionwindow');
        win.show();
    },

    editPermission: function (btn) {
        var me = this;
        var rec = Ext.getCmp('viewportid').down('permissiongrid').getSelectionModel().getSelection();
        var permissionId = rec[0].data.PermissionId;
        var win = Ext.widget('permissionwindow');
        var form = win.down('permissionform');
        form.getForm().setValues(rec[0].data);
        form.getForm().findField('actionType').setValue('EDIT');
        win.show();
//        Ext.Ajax.request({
//            url: 'usersecuritymanager/editPermission',
//            method: 'POST',
//            params: {
//                PermissionId: permissionId
//            },
//            success: function (response, options) {
//                var responseObj = Ext.JSON.decode(response.responseText);
//                var win = Ext.widget('permissionwindow');
//                var form = win.down('permissionform');
//                form.getForm().findField('actionType').setValue('EDIT');
//                win.show();
//            },

//            failure: function (response, options) {
//            }
//        });
    },

    deletePermission: function (btn) {
        var me = this;
        var rec = Ext.getCmp('viewportid').down('permissiongrid').getSelectionModel().getSelection();
        var permissionId = rec[0].data.PermissionId;
        Ext.Ajax.request({
            url: 'usersecuritymanager/deletePermission',
            method: 'POST',
            params: {
                PermissionId: permissionId
            },
            success: function (response, options) {
                var responseObj = Ext.JSON.decode(response.responseText);
            },

            failure: function (response, options) {
            }
        });
    },

    editUserGroup: function (btn) {

    },

    editGroupRoles: function (btn) {

    }

});
