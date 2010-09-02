Ext.ns('iIRNGTools', 'iIRNGTools.AdapterManager');
/**
* @class iIRNGTools.AdapterManager.ScopePanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
iIRNGTools.AdapterManager.ScopePanel = Ext.extend(Ext.Panel, {
  title: 'Registered Scopes',
  region: 'west',
  collapseMode: 'mini',
  width: 200,
  collapsible: true,
  collapsed: false,
  split: true,
  layout: 'border',
  border: true,

  navigationUrl: null,
  navigationPanel: null,
  detailsPanel: null,

  /**
  * initComponent
  * @protected
  */
  initComponent: function () {

    this.navigationPanel = new Ext.tree.TreePanel({
      region: 'center',
      collapseMode: 'mini',
      height: 200,
      collapsible: false,
      collapsed: false,
      split: true,
      layout: 'fit',
      border: true,

      rootVisible: false,
      lines: false,
      singleExpand: true,
      useArrows: true,

      loader: new Ext.tree.TreeLoader({
        dataUrl: this.navigationUrl
      }),

      root: new Ext.tree.AsyncTreeNode({}),

      listeners: {
        click: function (n) {
          var sn = this.selModel.selNode || {}; // selNode is null on initial selection              
          if (n.id != sn.id) {  // ignore clicks on folders and currently selected node                     
            if (n.leaf) {
              //detailsPanel.loadRecord(n.attributes.Application);
            } else {
              //detailsPanel.loadRecord(n.attributes.Scope);
            }
          }
        }
      }
    });

    this.detailsPanel = new Ext.grid.PropertyGrid({
      title: 'Details',
      region: 'south',
      collapseMode: 'mini',
      height: 150,
      collapsible: true,
      collapsed: false,
      split: true,
      layout: 'fit',
      border: true,
      frame: false,

      propertyNames: {
        name: 'Name',
        description: 'Description'
      },

      listeners: {
        'beforeedit': function (e) { return false; }
      }

    });

    this.items = [
      this.navigationPanel,
      this.detailsPanel
    ];

    // super
    iIRNGTools.AdapterManager.ScopePanel.superclass.initComponent.call(this);
  }

});