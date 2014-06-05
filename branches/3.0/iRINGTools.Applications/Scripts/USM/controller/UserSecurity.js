Ext.define('USM.controller.UserSecurity', {
    extend: 'Ext.app.Controller',

    models: ['SecurityM', 'GroupM', 'RoleM', 'UserM', 'PermissionM'],
    stores: ['SecurityS', 'GroupS', 'RoleS', 'UserS', 'PermissionS'],
    views: [
        'UserSecurityTabPanel',
        'SecurityGrid',
        'menus.SecurityMenu'
    ],

    refs: [
			{
			    ref: 'usersecuritytabpanel',
			    selector: 'viewport > usersecuritytabpanel'
			}
    ],
    init: function (application) {
        this.control({
            "menuitem[action=addGroup]": {
                click: this.addGroup
            }
            //            },
            //            "menuitem[action=editSecUserGroup]": {
            //                click: this.editUserGroup
            //            },
            //            "menuitem[action=editSecGroupRoles]": {
            //                click: this.editGroupRoles
            //            }
        });
    },

    addGroup: function (btn) {

    },

    editUserGroup: function (btn) {

    },

    editGroupRoles: function (btn) {

    }

});
