﻿/// <reference path="../ext-3.2.1/adapter/ext/ext-base.js" />
/// <reference path="../ext-3.2.1/ext-all.js" />

Ext.ns('AdapterManager');
/**
* @class AdapterManager.MappingPanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
AdapterManager.MappingPanel = Ext.extend(Ext.Panel, {

  height: 300,
  minSize: 150, autoScroll: true,
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
  m_window: null,
  mappingMenu: null,
  graphmapMenu: null,
  templatemapMenu: null,
  rolemapMenu: null,
  classmapMenu: null,
  directoryPanel: null,
  searchPanel: null,
  m_Form: null,
  /**
  * initComponent
  * @protected
  */
  initComponent: function () {


    this.tbar = this.buildToolbar();

    this.mappingMenu = new Ext.menu.Menu();
    this.mappingMenu.add(this.buildMappingMenu());

    this.graphmapMenu = new Ext.menu.Menu();
    this.graphmapMenu.add(this.buildGraphmapMenu());

    this.templatemapMenu = new Ext.menu.Menu();
    this.templatemapMenu.add(this.buildTemplateMapMenu());

    this.rolemapMenu = new Ext.menu.Menu();
    this.rolemapMenu.add(this.buildRoleMapMenu());

    this.classmapMenu = new Ext.menu.Menu();
    this.classmapMenu.add(this.buildClassMapMenu());

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
              text: 'Add GraphMap',
              handler: this.onAddGraphMap,
              icon: 'Content/img/16x16/document-new.png',
              scope: this
            }
        ]
  },



  onAddGraphMap: function (node) {
 //   var objectName = new Ext.form.TextField({
//      fieldLabel: 'Object Name',
//      width: 170,
//      render: this.getPropertyDropZone
//    });

//    var propertyName = new Ext.form.TextField({
//      fieldLabel: 'Key Property Name',
//      width: 170
//    });

//    var className = new Ext.form.TextField({
//      fieldLabel: 'Class Label',
//      width: 170,
//      render: this.getClassDropZone
//    });

//    var uri = new Ext.form.TextField({
//      fieldLabel: 'Uri',
//      width: 120
//    });

    this.m_Form = new Ext.form.FormPanel({
      id: 'target',
      layout: 'auto',
      items: [
             { xtype: 'textfield', fieldLabel: 'Object Name', width: 120, id: 'objectName', name: 'objectName', render: this.getPropertyDropZone },
             { xtype: 'textfield', fieldLabel: 'Key Property Name', width: 120, name: 'propertyName' },
             { xtype: 'textfield', fieldLabel: 'Class Label', width: 120, name: 'className'},//, render: this.getClassDropZone },
             { xtype: 'textfield', fieldLabel: 'Uri', width: 120, name: 'uriName' }
             ]
    });


    this.m_window = new Ext.Window({
      closable: true,
      modal: false,
      layout: 'auto',
      title: 'Add new GraphMap to Mapping',
      items: this.m_Form,
      buttons: [{
        text: 'Submit',
        handler: this.submit
      }],
      height: 280,
      width: 430,
      closeAction: 'hide',
      plain: true
    });

    this.m_window.show();
  },

  getPropertyDropZone: function (d) {
//    d.dragZone = new Ext.dd.DropZone(d.getEl(), {
//      ddGroup: 'propertyGroup',
//      notifyDrop: function (propdd, e, node) {
//        if (node.node.attributes.type != 'KeyDataPropertyNode') {
//          Ext.Msg.show({ title: 'invalid selection',
//            msg: 'Please select a key property node',
//            modal: true,
//            icon: Ext.Msg.ERROR,
//            buttons: Ext.Msg.OK
//          });
//          return false;
//        }
//      }
    //    });
   
  },

  getClassDropZone: function (d) {
  },

  onSuccess: function () {
  },

  onFailure: function () {
  },

  submit: function () {

  },

  buildGraphmapMenu: function () {
    return [
            {
              text: 'Add TemplateMap',
              handler: this.onAddTemplateMap,
              icon: 'Content/img/16x16/document-new.png',
              scope: this
            },
                {
                  text: 'Delete GraphMap',
                  handler: this.onDeleteGraphMap,
                  icon: 'Content/img/16x16/edit-delete.png',
                  scope: this
                }
        ]
  },

  onAddTemplateMap: function (node) {
  },

  onDeleteGraphMap: function (node) {
  },

  buildTemplateMapMenu: function () {
    return [
            {
              text: 'Delete TemplateMap',
              handler: this.onDeleteTemplateMap,
              icon: 'Content/img/16x16/edit-delete.png',
              scope: this
            }
        ]
  },

  onDeleteTemplateMap: function (node) {
  },

  buildRoleMapMenu: function () {
    return [
            {
              text: 'Add ClassMap',
              handler: this.onAddClassMap,
              icon: 'Content/img/16x16/document-new.png',
              scope: this
            },
            {
              text: 'Make Possessor',
              handler: this.onMakePossessor,
              // icon: 'Content/img/16x16/view-refresh.png',
              scope: this
            },
           {
             text: 'Map Property',
             handler: this.onMapProperty,
             // icon: '',
             scope: this
           },
           {
             text: 'Map ValueList',
             handler: this.onMapValueList,
             // icon: '',
             scope: this
           },
           {
             text: 'Reset Mapping',
             handler: this.onResetMapping,
             //icon: '',
             scope: this
           }
        ]
  },

  onResetMapping: function (node) {
  },

  onMapProperty: function (node) {
  },

  onMapValueList: function (node) {
  },

  onMakePossessor: function (node) {
  },

  onAddClassMap: function (node) {
  },

  buildClassMapMenu: function () {
    return [
            {
              text: 'Add TemplateMap',
              handler: this.onAddTemplateMap,
              icon: 'Content/img/16x16/document-new.png',
              scope: this
            },
            {
              text: 'Delete ClassMap',
              handler: this.onDeleteClassMap,
              icon: 'Content/img/16x16/edit-delete.png',
              scope: this
            }
        ]
  },

  onDeleteClassMap: function (node) {
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
      this.templatemapMenu.showAt([x, y]);
    } else if (obj.type == "RoleMapNode") {
      this.rolemapMenu.showAt([x, y]);
    } else if (obj.type == "ClassMapNode") {
      this.classmapMenu.showAt([x, y]);
    }
  }
});