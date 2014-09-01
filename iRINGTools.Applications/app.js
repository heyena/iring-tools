Ext.Loader.setConfig({
  enabled: true,
  garbageCollect: true,
  paths: {
    'Ext.ux': 'Scripts/ext-4.2.1.883/ux',
	'df': 'Scripts/datafilter',
    'Ext.grid': 'Scripts/ext-4.2.1.883/src/grid'	
  }
});

Ext.application({
    requires: [
        'Ext.ux.form.ItemSelector',
        'AM.view.Viewport',
        'AM.view.menus.ScopeMenu',
        'AM.view.menus.AppDataRefreshMenu',
        'AM.view.mapping.LiteralForm',
        'AM.view.mapping.LiteralWindow',
        'AM.view.directory.FileUpoadForm',
        'AM.view.directory.FileUploadWindow',
        'AM.view.directory.DownloadGrid',
        'AM.view.directory.ImportCacheForm',
        'AM.view.directory.ImportCacheWindow',
        'AM.view.directory.DownloadForm',
        'AM.view.directory.VirtualPropertyForm',
        'AM.view.directory.VirtualPropertyGrid',
        'AM.view.directory.VirtualPropertyWindow',
        'AM.view.menus.VirtualPropertyMenu'
  ],

  controllers: [
        'Mapping',
        'Directory',
        'Search',
        'Spreadsheet',
        'NHConfig',
        'df.controller.DataFilter'
  ],

  models: [
        'RelationModel',
        'FileDownloadModel',
        'VirtualPropertyModel'
  ],

  stores: [
        'RelationStore',
        'FileDownloadStore',
        'VirtualPropertyStore'
  ],

  views: [
        'AM.view.Viewport',
        'menus.ScopeMenu',
        'menus.AppDataRefreshMenu',
        'mapping.LiteralForm',
        'mapping.LiteralWindow',
        'directory.FileUpoadForm',
        'directory.FileUploadWindow',
        'directory.DownloadGrid',
        'directory.ImportCacheForm',
        'directory.ImportCacheWindow',
        'directory.DownloadForm',
        'directory.VirtualPropertyForm',
        'directory.VirtualPropertyGrid',
        'directory.VirtualPropertyWindow',
        'menus.VirtualPropertyMenu'
  ],
  
  name: 'AM',
  appFolder: 'Scripts/AM',
  autoCreateViewport: true,
  launch: function() {
		Ext.Ajax.request({
			url: 'directory/InitializeUISettings',
			success: function(response){
				var text = Ext.JSON.decode(response.responseText);
				if(text.success){
					for(var i = 0; i<text.items.length; i++){
				    var name = text.items[i].Name;
					var val = text.items[i].Value;
					if(name == 'isUISecurityEnabled'){
					   utilsObj.isSecEnable = val;
					}
					if(name == 'isAdmin'){
					   utilsObj.isAdmin = val;
					}
				}
				}else{
					var userMsg = text['message'];
					var detailMsg = text['stackTraceDescription'];
					var expPanel = Ext.widget('exceptionpanel', { title: 'Error Notification'});
					Ext.ComponentQuery.query('#expValue',expPanel)[0].setValue(userMsg);
					Ext.ComponentQuery.query('#expValue2',expPanel)[0].setValue(detailMsg);
				}
				
			},
			failure: function(response){
			}
		});
   }
});

Ext.Ajax.on('requestexception', function (conn, response, options) {
    if (response.status == 0 || response.status == 408) {
        //alert("Your session expired.");
        location.reload(true);
    }
});
