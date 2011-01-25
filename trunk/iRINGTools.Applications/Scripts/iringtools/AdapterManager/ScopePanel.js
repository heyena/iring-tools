﻿Ext.ns('AdapterManager');
/**
* @class AdapterManager.ScopePanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
AdapterManager.ScopePanel = Ext.extend(Ext.Panel, {
    title: 'Scope',
    width: 220,

    collapseMode: 'mini',
    //collapsible: true,
    //collapsed: false,
    closable: true,
        
    layout: 'fit',
    border: true,
    split: true,    
        
    /**
    * initComponent
    * @protected
    */
    initComponent: function () {

        // super
        AdapterManager.ScopePanel.superclass.initComponent.call(this);
    }

});