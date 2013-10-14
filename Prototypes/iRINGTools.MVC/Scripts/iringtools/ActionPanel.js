Ext.ns('iIRNGTools', 'iIRNGTools.AdapterManager');
/**
* @class iIRNGTools.AdapterManager.ActionPanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
iIRNGTools.AdapterManager.ActionPanel = Ext.extend(Ext.Panel, {
  collapseMode: 'mini',
  width: 200,
  header: false,
  collapsible: true,
  collapsed: false,
  split: true,
  padding: '0,0,0,5', 
  border: true,
  baseCls: 'x-plain',  
  /**
  * initComponent
  * @protected
  */
  initComponent: function () {

    var scopeActions = new Ext.Panel({
      frame: true,
      title: 'Scope Actions',
      collapsible: true,
      contentEl: 'scope-actions',
      titleCollapse: true
    });

    this.items = [scopeActions]
      
    // super
    iIRNGTools.AdapterManager.ActionPanel.superclass.initComponent.call(this);
  }  

});