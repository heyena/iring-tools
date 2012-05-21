/// <reference path="../../../extjs407/ext-debug.js" />
Ext.define('AM.view.directory.DirectoryPanel', {
    extend: 'Ext.panel.Panel',
    alias: 'widget.directorypanel',
    title: 'Directory',
    width: 250,
    collapsible: true,
    collapsed: false,
    layout: {
        type: 'border',
        padding: 2
    },
    border: true,
    split: true,
    initComponent: function () {      
        this.callParent(arguments);
    }
});