/*
 * File: app.js
 *
 * This file was generated by Sencha Architect version 2.1.0.
 * http://www.sencha.com/products/architect/
 *
 * This file requires use of the Ext JS 4.1.x library, under independent license.
 * License of Sencha Architect does not include license for Ext JS 4.1.x. For more
 * details see http://www.sencha.com/license or contact license@sencha.com.
 *
 * This file will be auto-generated each and everytime you save your project.
 *
 * Do NOT hand edit this file.
 */

Ext.Loader.setConfig({
  enabled: true,
  paths: {
    'Ext.ux': 'Scripts/AM/ux'
  },
  garbageCollect: true
});

Ext.application({
  views: [
    'AMViewport',
    'search.SearchTree',
    'directory.DirectoryTree',
    'mapping.MappingTree',
    'nhibernate.NhibernateTree'
  ],
  appFolder: 'Scripts/AM',
  autoCreateViewport: true,
  name: 'AM',
  controllers: [
    'Mapping',
    'Directory',
    'Search',
    'Spreadsheet',
    'NHibernate'
  ],

  launch: function() {

  }

});
