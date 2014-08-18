Ext.Loader.setConfig({
  enabled: true,
  garbageCollect: true,
  paths: {
    'Ext.ux': 'Scripts/ext-4.2.1.883/ux'
  }
});

Ext.application({
  controllers: [
        'UserSecurity'

  ],

  views: [
        'UserSecurityTabPanel',
        'SecurityGrid',
        'groups.GroupGrid',
        'users.UserGrid',
        'permissions.PermissionGrid',
        'roles.RoleGrid'
  ],
  
  name: 'USM',
  appFolder: 'Scripts/USM',
  autoCreateViewport: true
});

Ext.Ajax.on('requestexception', function (conn, response, options) {
  if (response.status == 0 || response.status == 408) {
      location.reload(true);
  }
});
