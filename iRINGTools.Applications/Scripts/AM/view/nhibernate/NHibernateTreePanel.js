﻿/// <reference path="../../../extjs40/ext-debug.js" />
Ext.define('AM.view.nhibernate.NHibernateTreePanel', {
    extend: 'Ext.panel.Panel',
    alias: 'widget.nhibernatetreepanel',
    width: 300,
    minWidth: 240,    
    layout: 'border',
    bodyStyle: 'background:#fff',   
    items:[
           { xtype: 'nhibernatetree', region: 'center' }                
          ],
    initComponent: function () {
        
        this.callParent(arguments);
    }
});