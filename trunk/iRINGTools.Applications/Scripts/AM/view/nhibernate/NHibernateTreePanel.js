/// <reference path="../../../extjs407/ext-debug.js" />
Ext.define('AM.view.nhibernate.NHibernateTreePanel', {
    extend: 'Ext.panel.Panel',
    alias: 'widget.nhibernatetreepanel',
    width: 300,
    minWidth: 240,
    layout: 'border',
    scopeName: null,
    appName: null,
    bodyStyle: 'background:#fff',   
    items:[
           { xtype: 'nhibernatetreepanel', region: 'center' }                
          ],
    initComponent: function () {
        this.callParent(arguments);
    }
});