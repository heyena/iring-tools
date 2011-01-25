Ext.ns('AdapterManager');
/**
* @class AdapterManager.ApplicationPanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
AdapterManager.ApplicationPanel = Ext.extend(Ext.Panel, {
    title: 'Application',
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
        AdapterManager.ApplicationPanel.superclass.initComponent.call(this);
    }

});