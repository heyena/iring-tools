Ext.Loader.setConfig({
  enabled: true,
  garbageCollect: true,
  paths: {
    'Ext.ux': 'Scripts/ext-4.2.1.883/ux'
  }
});

Ext.application({

  requires: [
    'AM.view.Main',
    'AM.view.menus.ScopeMenu',
    'AM.view.menus.AppDataRefreshMenu',
    'AM.view.nhibernate.MultiSelectComponentGrid',
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
    'Main',
    'menus.ScopeMenu',
    'menus.AppDataRefreshMenu',
    'nhibernate.MultiSelectComponentGrid',
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
  appFolder: 'Scripts/AM',
  autoCreateViewport: true,
  controllers: [
    'Mapping',
    'Directory',
    'Search',
    'Spreadsheet',
    'NHibernate',
    'NHConfig'
  ],
  name: 'AM'
});
