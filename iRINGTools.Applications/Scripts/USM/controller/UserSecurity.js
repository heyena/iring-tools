Ext.define('USM.controller.UserSecurity', {
    extend: 'Ext.app.Controller',

    models: ['SecurityM', 'GroupM', 'RoleM', 'UserM', 'PermissionM'],
    stores: ['SecurityS', 'GroupS', 'RoleS', 'UserS', 'PermissionS'],
    views: [
        'UserSecurityTabPanel',
        'SecurityGrid',
        'groups.GroupGrid',
        'menus.GroupMenu',
        'groups.GroupForm',
        'groups.GroupWindow',
        'menus.SecurityMenu',
        'menus.UserMenu',
	    'users.UserGrid',
        'menus.RoleMenu',
        'roles.RoleForm',
        'roles.RoleWindow',
        'menus.PermissionMenu',
        'permissions.PermissionForm',
        'permissions.PermissionWindow',
	    'permissions.PermissionSelectionPanel',
	    'permissions.PermissionSelectionPanelWindow',
	    'groups.GrpUserSelectionPanel',
        'groups.UserGrpSelectionPanel',
        'roles.RoleGroupSelectionPanel',
        'roles.GroupRoleSelectionPanel',
        'ItemSelectorWindow'
    ],

    refs: [
			{
			    ref: 'usersecuritytabpanel',
			    selector: 'viewport > usersecuritytabpanel'
			},
			{
			    ref: 'userGrid',
			    selector: 'viewport > usersecuritytabpanel > panel > usergrid'
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
            "menuitem[action=addUserToGroup]": {
                click: this.addUserToGroup
            },
            "menuitem[action=addGroupToUser]": {
                click: this.addGroupToUser
            },
            "menuitem[action=editUserGroup]": {
                click: this.editUserGroup
            },
            "menuitem[action=editGroupUser]": {
                click: this.editGroupUser
            },
            "menuitem[action=addRemRoletoGroup]": {
                click: this.addRemRoletoGroup
            },
            "menuitem[action=addRemGroupToRole]": {
                click: this.addRemGroupToRole
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
            },
            "usergrid": {
                itemcontextmenu: me.onUserGridClick
            },
            "menuitem[action=addEditUser]": {
                click: this.addOrEditUsers
            },
            "menuitem[action=deleteUser]": {
                click: this.deleteUser
            },
            "menuitem[action=addPermissionToRole]": {
                click: this.addPermissionToRole
            }
        });
    },

    addPermissionToRole: function (item, e, eOpts) {
        var win = Ext.widget('permissionselectionpanelwindow');
        win.show();
    },

    deleteUser: function (item, e, eOpts) {
        Ext.MessageBox.confirm('Delete', 'Are you sure ?', function (btn) {
            if (btn === 'yes') {
                //some code
            }
            else {
                //some code
            }
        });
    },

    addOrEditUsers: function (item, e, eOpts) {
        var me = this;
        var conf = {
            title: '',
            iconCls: 'tabsApplication'
        };
        var UserName, UserFirstName, UserLastName, UserEmail, UserPhone, UserDesc;
        var win = Ext.widget('addUserformwindow', conf);
        var form = win.down('form');
        if (item.itemId == 'editUser') {
            win.setTitle('Edit User');
            var selectedRecord = me.getUserGrid().getSelectionModel().getSelection()[0];
            UserName = selectedRecord.data.UserName;
            UserFirstName = selectedRecord.data.UserFirstName;
            UserLastName = selectedRecord.data.UserLastName;
            UserEmail = selectedRecord.data.UserEmail;
            UserPhone = selectedRecord.data.UserPhone;
            UserDesc = selectedRecord.data.UserDesc;
        } else if (item.itemId == 'addUser') {
            win.setTitle('Add User');
        }
        /*win.on('save', function () {
        win.destroy();
        tree.view.refresh();
        tree.expandPath(tree.getRootNode().getPath());
        var detailGrid = tree.up('panel').down('propertypanel');//.down('gridview');
        detailGrid.setSource({});
        }, me);*/

        win.on('Cancel', function () {
            win.destroy();
        }, me);
        win.show();
        form.getForm().findField('UserName').setValue(UserName);
        form.getForm().findField('UserFirstName').setValue(UserFirstName);
        form.getForm().findField('UserLastName').setValue(UserLastName);
        form.getForm().findField('UserEmail').setValue(UserEmail);
        form.getForm().findField('UserPhone').setValue(UserPhone);
        form.getForm().findField('UserDesc').setValue(UserDesc);
        win.show();
    },

    onUserGridClick: function (dataview, record, item, index, e, eOpts) {
        e.stopEvent();
        var me = this;
        var userMenu = Ext.widget('usermenu');
        userMenu.showAt(e.getXY());
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

    addUserToGroup: function (btn) {
        var me = this;
        var win = new USM.view.ItemSelectorWindow({
            form: Ext.widget('grpuserselectionpanel'),
            title: 'Add/Remove Users to Group'
        });
        var form = win.down('grpuserselectionpanel');
        var rec = Ext.getCmp('viewportid').down('groupgrid').getSelectionModel().getSelection();
        form.getForm().findField('groupName').setValue(rec[0].data.GroupId);
        win.show();
    },

    editUserGroup: function (btn) {
        var me = this;
        var rec = Ext.getCmp('viewportid').down('groupgrid').getSelectionModel().getSelection();
        var groupId = rec[0].data.GroupId;
        Ext.Ajax.request({
            url: 'usersecuritymanager/editUserGroup',
            //url: '/Scripts/USM/jsonfiles/selusers.json',
            method: 'POST',
            params: {
                GroupId: groupId
            },
            success: function (response, options) {
                var responseObj = Ext.JSON.decode(response.responseText);
                var win = new USM.view.ItemSelectorWindow({
                    form: Ext.widget('grpuserselectionpanel'),
                    title: 'Add/Remove Users to Group'
                });
                var form = win.down('grpuserselectionpanel');

                var selArr = [];
                form.on('beforerender', function (form, ept) {
                    if (responseObj != null) {
                        for (var i = 0; i < responseObj.items.length; i++) {
                            selArr.push(responseObj.items[i].UserId);
                        }
                        form.getForm().findField('selectedUsers').setValue(selArr);
                    }
                }, me);
                form.getForm().findField('groupName').setValue(groupId);
                win.show();
            },
            failure: function (response, options) {
            }
        });
    },

    addGroupToUser: function (btn) {
        var me = this;
        var win = new USM.view.ItemSelectorWindow({
            form: Ext.widget('usergrpselectionpanel'),
            title: 'Add/Remove Groups to User'
        });
        var form = win.down('usergrpselectionpanel');
        var rec = Ext.getCmp('viewportid').down('usergrid').getSelectionModel().getSelection();
        var userId = rec[0].data.UserId;
        form.getForm().findField('userName').setValue(userId);
        win.show();
    },

    editGroupUser: function (btn) {
        var me = this;
        var rec = Ext.getCmp('viewportid').down('usergrid').getSelectionModel().getSelection();
        var userId = rec[0].data.UserId;
        Ext.Ajax.request({
            //url: 'usersecuritymanager/editUserGroup',
            url: '/Scripts/USM/jsonfiles/selgroup.json',
            //            method: 'POST',
            //            params: {
            //                userId: userId
            //            },
            success: function (response, options) {
                var responseObj = Ext.JSON.decode(response.responseText);
                var win = new USM.view.ItemSelectorWindow({
                    form: Ext.widget('usergrpselectionpanel'),
                    title: 'Add/Remove Groups to User'
                });
                var form = win.down('usergrpselectionpanel');
                var selArr = [];

                form.on('beforerender', function (form, ept) {
                    if (responseObj != null) {
                        for (var i = 0; i < responseObj.items.length; i++) {
                            selArr.push(responseObj.items[i].GroupId);
                        }
                        form.getForm().findField('selectedGroups').setValue(selArr);
                    }
                }, me);
                form.getForm().findField('userName').setValue(userId);
                win.show();
            },
            failure: function (response, options) {
            }
        });
    },

    addRemRoletoGroup: function (btn) {
        var me = this;
        var rec = Ext.getCmp('viewportid').down('groupgrid').getSelectionModel().getSelection();
        var groupId = rec[0].data.GroupId;
        Ext.Ajax.request({
            //url: 'usersecuritymanager/editUserGroup',
            url: '/Scripts/USM/jsonfiles/selroles.json',
            //            method: 'POST',
            //            params: {
            //                groupId: groupId
            //            },
            success: function (response, options) {
                var responseObj = Ext.JSON.decode(response.responseText);
                var win = new USM.view.ItemSelectorWindow({
                    form: Ext.widget('rolegroupselectionpanel'),
                    title: 'Add/Remove Roles to Group'
                });
                var form = win.down('rolegroupselectionpanel');
                var selArr = [];

                form.on('beforerender', function (form, ept) {
                    if (responseObj != null) {
                        for (var i = 0; i < responseObj.items.length; i++) {
                            selArr.push(responseObj.items[i].RoleId);
                        }
                        form.getForm().findField('selectedRoles').setValue(selArr);
                    }
                }, me);
                form.getForm().findField('groupName').setValue(groupId);
                win.show();
            },
            failure: function (response, options) {
            }
        });
    },

    addRemGroupToRole: function (btn) {
        var me = this;
        var rec = Ext.getCmp('viewportid').down('rolegrid').getSelectionModel().getSelection();
        var roleId = rec[0].data.RoleId;
        Ext.Ajax.request({
            //url: 'usersecuritymanager/editUserGroup',
            url: '/Scripts/USM/jsonfiles/selgroup.json',
            //            method: 'POST',
            //            params: {
            //                groupId: groupId
            //            },
            success: function (response, options) {
                var responseObj = Ext.JSON.decode(response.responseText);
                var win = new USM.view.ItemSelectorWindow({
                    form: Ext.widget('grouproleselectionpanel'),
                    title: 'Add/Remove Groups to Role'
                });
                var form = win.down('grouproleselectionpanel');
                var selArr = [];

                form.on('beforerender', function (form, ept) {
                    if (responseObj != null) {
                        for (var i = 0; i < responseObj.items.length; i++) {
                            selArr.push(responseObj.items[i].GroupId);
                        }
                        form.getForm().findField('selectedGroups').setValue(selArr);
                    }
                }, me);
                form.getForm().findField('roleName').setValue(roleId);
                win.show();
            },
            failure: function (response, options) {
            }
        });
    }

});
