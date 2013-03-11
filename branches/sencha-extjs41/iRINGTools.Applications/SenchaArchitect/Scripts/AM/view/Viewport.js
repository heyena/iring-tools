/*
 * File: Scripts/AM/view/Viewport.js
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

Ext.define('AM.view.Viewport', {
  extend: 'AM.view.AMViewport',
  renderTo: Ext.getBody(),
  requires: [
    'AM.view.nhibernate.CreateRelationForm',
    'AM.view.menus.RootAdminScopesMenu',
    'AM.view.menus.GroupAdminScopesMenu',
    'AM.view.menus.NoLdapScopesMenu',
    'AM.view.menus.ScopeMenu',
    'AM.view.menus.ApplicationMenu',
    'AM.view.menus.AppDataMenu',
    'AM.view.menus.ValueListsMenu',
    'AM.view.menus.ValueListMenu',
    'AM.view.menus.ValueListMapMenu',
    'AM.view.menus.GraphsMenu',
    'AM.view.menus.GraphMenu',
    'AM.view.menus.TemplatemapMenu',
    'AM.view.menus.RolemapMenu',
    'AM.view.menus.ClassmapMenu',
    'AM.view.AMViewport',
    'AM.view.directory.ApplicationWindow',
    'AM.view.directory.ScopeWindow',
    'AM.view.directory.DataLayerWindow',
    'AM.view.directory.GraphMapWindow',
    'AM.view.mapping.ClassMapWindow',
    'AM.view.mapping.PropertyMapWindow',
    'AM.view.mapping.ValueListMapWindow',
    'AM.view.mapping.MapValueListWindow',
    'AM.view.mapping.ValueListWindow',
    'AM.view.search.SearchTree',
    'AM.view.nhibernate.SelectPropertiesForm',
    'AM.view.spreadsheet.SpreadsheetWindow',
    'AM.view.nhibernate.ConnectionStringForm',
    'AM.view.mapping.MappingPanel',
    'AM.view.spreadsheet.SpreadsheetPanel',
    'AM.view.directory.DataGridPanel',
    'AM.view.nhibernate.NhibernatePanel',
    'AM.view.spreadsheet.SourceWindow',
    'AM.view.nhibernate.SelectTablesForm',
    'AM.view.nhibernate.Utility',
    'AM.view.nhibernate.PropertyGrid',
    'AM.view.nhibernate.DataObjectForm',
    'AM.view.nhibernate.DataKeyForm',
    'AM.view.nhibernate.SelectDataKeysForm',
    'AM.view.nhibernate.SetPropertyForm',
    'AM.view.nhibernate.SetRelationForm',
    'AM.view.nhibernate.RelationsForm'
  ]
});