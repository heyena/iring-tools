/// <reference path="../ext-3.2.1/adapter/ext/ext-base.js" />
/// <reference path="../ext-3.2.1/ext-all.js" />

Ext.ns('iIRNGTools', 'iIRNGTools.AdapterManager');
/**
* @class iIRNGTools.AdapterManager.MappingPanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
iIRNGTools.AdapterManager.MappingPanel = Ext.extend(Ext.Panel, {
  split: true,
  height: 300,
  minSize: 150,
  autoScroll: true,
  layout: 'border', 

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
        border: true,
      }),
      new Ext.TabPanel({
        region: 'center',                
        split: true,
        border: true,
        activeTab: 0,
        //tabPosition: 'bottom',
        items: [{
          title: 'Graph Maps',
          tbar: [{text: 'Save'}],
          bbar: new Ext.ux.StatusBar({      
            defaultText: 'Ready',      
            statusAlign: 'right'   
          })  
        },{
          title: 'Value Maps',
          tbar: [{text: 'Save'}],
          bbar: new Ext.ux.StatusBar({      
            defaultText: 'Ready',      
            statusAlign: 'right'   
          })
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