/// <reference path="../ext-3.2.1/adapter/ext/ext-base.js" />
/// <reference path="../ext-3.2.1/ext-all.js" />

Ext.ns('iIRNGTools', 'iIRNGTools.AdapterManager');
/**
* @class iIRNGTools.AdapterManager.MappingPanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
iIRNGTools.AdapterManager.MappingPanel = Ext.extend(Ext.Panel, {
    
  height: 300,
  minSize: 150,
  
  autoScroll: true,
  layout: 'border',  
  frame: true,
  split: true,

  /**
  * initComponent
  * @protected
  */
  initComponent: function () {
    
    this.items = [
      new Ext.Panel({
        title: 'Data Objects',
        region: 'east',
        collapseMode: 'mini',
        width: 200,
        collapsible: true,
        collapsed: false,
        split: true,
        border: true
      }),
      new Ext.TabPanel({
        region: 'center',
        split: true,
        border: true,
        activeTab: 0,
        tabPosition: 'bottom',
        //tabIcon: 'Content/img/file-mapping.png',
        tbar: [{ text: 'Save', icon: 'Content/img/document-save.png'}],
        bbar: new Ext.ux.StatusBar({
            defaultText: 'Ready',
            statusAlign: 'right'
        }),
        items: [{
          title: 'Graph Maps'
        },{
          title: 'Value Maps'
        }]
      })
    ];

    // super
    iIRNGTools.AdapterManager.MappingPanel.superclass.initComponent.call(this);

  },

  /**
  * buildUI
  * @private
  */
  buildUI: function () {
    return [{
      text: 'Save',
      //iconCls: 'icon-save',
      //handler: this.onUpdate,
      scope: this
    }];
  }

});