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

  mappingMenu: null,
  /**
  * initComponent
  * @protected
  */
  initComponent: function () {

    this.tbar = this.buildToolbar();

    this.mappingMenu = new Ext.menu.Menu();
    this.mappingMenu.add(this.buildMappingMenu());

    this.treeLoader = new Ext.tree.TreeLoader({
      baseParams: {
        type: null,
        id: null
      },
      url: this.navigationUrl
    });

    this.treeLoader.on("beforeload", function (treeLoader, node) {
        treeLoader.baseParams.type = node.attributes.type;
        if (node.attributes.record != undefined)
            treeLoader.baseParams.id = node.attributes.record.id;
    }, this);

    this.rootNode = new Ext.tree.AsyncTreeNode({
      id: this.scope.Name + "/" + this.application.Name,
      text: 'Mapping',
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
      bbar: new Ext.ux.StatusBar({
        defaultText: 'Ready',
        statusAlign: 'right'
      })
    });

    this.mappingPanel.on('contextmenu', this.showContextMenu, this);
    this.mappingPanel.on('click', this.onClick, this);

    this.propertyPanel = new Ext.grid.PropertyGrid({
        title: 'Details',
        region: 'east',
        width: 200,
        split: true,
        collapseMode: 'mini',
        stripeRows: true,
        collapsible: true,
        autoScroll: true,
        border: false,
        frame: false,
        height: 150,
        // bodyStyle: 'padding-bottom:15px;background:#eee;',
        source: {},
        listeners: {
            // to disable editable option of the property grid
            beforeedit: function (e) {
                e.cancel = true;
            }
        }
    });

    this.items = [
          this.propertyPanel,
          this.mappingPanel
        ];

    // super
    AdapterManager.MappingPanel.superclass.initComponent.call(this);

  },

  buildToolbar: function () {
        return [
      {
          text: 'Reload',
          handler: this.onReload,
          icon: 'Content/img/16x16/view-refresh.png',
          scope: this
      },
      {
          text: 'Save',
          handler: this.onSave,
          icon: 'Content/img/16x16/document-save.png',
          scope: this
      },
      {
          text: 'Upload',
          handler: this.onUpload,
          //icon: 'Content/img/list-remove.png',
          scope: this
      }      
    ]
  },

  onClick: function (node) {
      try {
          this.propertyPanel.setSource(node.attributes.record);
      } catch (e) {
          //  alert(e);
      }
  },

  onReload: function () {
    this.mappingPanel.root.reload();
  },

  onReloadNode: function (node) {
      node.reload();
  },

  buildMappingMenu: function () {
        return [
            {
                text: 'Add GrpahMap',
                handler: this.onAddGraphMap,
                icon: 'Content/img/16x16/document-new.png',
                scope: this
            },
            {
                text: 'Reload Worksheets',
                handler: this.onReloadNode,
                icon: 'Content/img/16x16/view-refresh.png',
                scope: this
            }
        ]
  },

  onAddGraphMap: function(node) {
  },

  showContextMenu: function (node, event) {

        //  if (node.isSelected()) { 
        var x = event.browserEvent.clientX;
        var y = event.browserEvent.clientY;

        var obj = node.attributes;

        if (obj.type == "MappingNode") {
            this.mappingMenu.showAt([x, y]);
        } else if (obj.type == "GraphMapNode") {
            this.graphmapMenu.showAt([x, y]);
        } else if (obj.type == "TemplateMapNode") {
            this.templatmapeMenu.showAt([x, y]);
        } else if (obj.type == "RoleMapNode") {
            this.rolemapMenu.showAt([x, y]);
        } else if (obj.type == "ClassMapNode") {
            this.classmapMenu.showAt([x, y]);
        }
        //}
    },

});