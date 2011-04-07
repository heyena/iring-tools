/// <reference path="../ext-3.2.1/adapter/ext/ext-base.js" />
/// <reference path="../ext-3.2.1/ext-all.js" />

Ext.ns('AdapterManager');
/**
* @class AdapterManager.MappingPanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
AdapterManager.MappingPanel = Ext.extend(Ext.Panel, {

  height: 300,
  minSize: 150,

  autoScroll: true,
  layout: 'border',

  split: true,
  closable: true,

  navigationUrl: null,
  propertyPanel: null,
  mappingPanel: null,
  rootNode: null,
  treeLoader: null,
  scope: null,
  application: null,
  graph: null,

  /**
  * initComponent
  * @protected
  */
  initComponent: function () {

    this.treeLoader = new Ext.tree.TreeLoader({
      baseParams: {
        type: null,
        record: null
      },
      url: this.navigationUrl
    });

    this.treeLoader.on("beforeload", function (treeLoader, node) {
      treeLoader.baseParams.type = node.attributes.type;
      treeLoader.baseParams.record = node.attributes.record;
    }, this);

    this.rootNode = new Ext.tree.AsyncTreeNode({
      id: this.scope.Name + "/" + this.application.Name,
      text: 'mapping',
      expanded: true,
      draggable: false,
      icon: 'Content/img/internet-web-browser.png',
      type: 'MappingNode'
    });

    this.mappingPanel = new Ext.tree.TreePanel({
      region: 'center',
      split: true,
      border: true,
      collapseMode: 'mini',
      height: 300,
      layout: 'fit',
      expandAll: true,
      rootVisible: true,
      lines: true,
      autoScroll: true,
      //singleExpand: true,
      useArrows: true,
      loader: this.treeLoader,
      root: this.rootNode,
      tbar: [{ text: 'Save', icon: 'Content/img/document-save.png'}],
      bbar: new Ext.ux.StatusBar({
        defaultText: 'Ready',
        statusAlign: 'right'
      })
    });

    this.propertyPanel = new Ext.Panel({
      title: 'Properties',
      region: 'east',
      collapseMode: 'mini',
      width: 200,
      collapsible: true,
      collapsed: false,
      split: true,
      border: true
    });

    this.items = [
          this.propertyPanel,
          this.mappingPanel
        ];

    // super
    AdapterManager.MappingPanel.superclass.initComponent.call(this);

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