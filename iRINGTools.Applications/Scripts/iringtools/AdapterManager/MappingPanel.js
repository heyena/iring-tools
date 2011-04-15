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
  graphmapMenu: null,
  templatemapMenu: null,
  rolemapMenu: null,
  classmapMenu: null,
  directoryPanel: null,
  searchPanel: null,

  iconCls: 'tabsMapping',

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
      icon: 'Content/img/16x16/mapping.png',
      type: 'MappingNode'
    });

    this.mappingPanel = new Ext.tree.TreePanel({
      region: 'center',
      enableDD: true,
      ddGroup: 'refdataGroup',
      split: true,
      border: true,
      collapseMode: 'mini',
      height: 300,
      layout: 'fit',
      lines: true,
      expandAll: true,
      rootVisible: true,
      lines: true,
      autoScroll: true,
      //singleExpand: true,      
      loader: this.treeLoader,
      root: this.rootNode,
      bbar: new Ext.ux.StatusBar({ defaultText: 'Ready', statusAlign: 'right' }),
      listeners: {
        beforenodedrop: {
          fn: function (e) {


            if (e.data.node.attributes.type == 'TemplateNode') {
              var nodetype = 'TemplateMapNode'
              var icn = 'Content/img/template-map.png';
              var txt = e.data.node.attributes.record.Label;
              var ident = e.data.node.attributes.identifier;
              var rec = e.data.node.attributes.record;

            }
            else {
              return false;
            }
            e.cancel = false;
            e.dropNode = [];
            e.dropNode.push(this.loader.createNode({
              text: txt,
              nodeType: "async",
              type: nodetype,
              icon: icn,
              leaf: lf,
              identifier: e.data.node.attributes.identifier,
              record: e.data.node.attributes.record
            })
              )
            return true;
          }
        }
      }
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
      selModel: new Ext.grid.RowSelectionModel({ singleSelect: true }),
      // bodyStyle: 'padding-bottom:15px;background:#eee;',
      source: {},
      listeners: {
        beforeedit: function (e) {
          e.cancel = true;
        },
        afteredit: function (e) {
          e.grid.getSelectionModel().selections.items[0].data.value = e.originalValue;
          e.record.data.value = e.originalValue;
          e.value = e.originalValue;
          e.grid.getView().refresh();
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
      }//,
    //      {
    //        text: 'Upload',
    //        handler: this.onUpload,
    //        //icon: 'Content/img/list-remove.png',
    //        scope: this
    //      }
    ]
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

  buildGraphmapMenu: function () {
    return [
      {
        text: 'Delete GraphMap',
        handler: this.onDeleteGraphMap,
        icon: 'Content/img/16x16/edit-delete.png',
        scope: this
      }
    ]
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

  buildClassMapMenu: function () {
    return [
    //            {
    //              text: 'Add TemplateMap',
    //              handler: this.onAddTemplateMap,
    //              icon: 'Content/img/16x16/document-new.png',
    //              scope: this
    //            },
          {
          text: 'Delete ClassMap',
          handler: this.onDeleteClassMap,
          icon: 'Content/img/16x16/edit-delete.png',
          scope: this
        }
      ]
  },

  getSelectedNode: function () {
    var node = this.mappingPanel.getSelectionModel().getSelectedNode();
    return node;
  },

  onSave: function (c) {
    Ext.Ajax.request({
      url: 'mapping/updateMapping',
      method: 'POST',
      params: {
        Scope: this.scope.Name,
        Application: this.application.Name
      },
      success: function (result, request) {
        Ext.Msg.alert('Success', 'Mapping saved to the server');
      },
      failure: function (result, request) { }
    })
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

  onAddGraphMap: function (node) {
    var formid = 'target-' + this.scope.Name + '-' + this.application.Name;
    var form = new Ext.form.FormPanel({
      id: formid,
      layout: 'form',
      method: 'POST',
      border: false,
      frame: false,
      bbar: [
        { text: 'Submit', scope: this, handler: this.onSubmit },
        { text: 'Close', scope: this, handler: this.onClose }
        ],
      url: 'mapping/addgraphmap',
      items: [{ xtype: 'textfield', name: 'graphName', id: 'graphName', fieldLabel: 'Graph Name', width: 120, required: true }, //, value: '' },
              {xtype: 'hidden', name: 'objectName', id: 'objectName' },
              { xtype: 'hidden', name: 'classLabel', id: 'classLabel' },
              { xtype: 'hidden', name: 'classUrl', id: 'classUrl' },
              { xtype: 'hidden', name: 'mappingNode', id: 'mappingNode', value: this.rootNode }
             ],
      html: '<div class="property-target' + formid + '" '
          + 'style="border:1px silver solid;margin:5px;padding:8px;height:20px">'
          + 'Drop a Key Property Node here.</div>'
          + '<div class="class-target' + formid + '" '
          + 'style="border:1px silver solid;margin:5px;padding:8px;height:20px">'
          + 'Drop a Class Node here. </div>',

      afterRender: function (cmp) {
        Ext.FormPanel.prototype.afterRender.apply(this, arguments);

        var propertyTarget = this.body.child('div.property-target' + formid);
        var propertydd = new Ext.dd.DropTarget(propertyTarget, {
          ddGroup: 'propertyGroup',
          notifyEnter: function (dd, e, data) {
            if (data.node.attributes.type != 'KeyDataPropertyNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;
          },
          notifyOver: function (dd, e, data) {
            if (data.node.attributes.type != 'KeyDataPropertyNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;
          },
          notifyDrop: function (dd, e, data) {
            if (data.node.attributes.type != 'KeyDataPropertyNode') {
              return false;
            }
            else {
              Ext.get('objectName').dom.value = data.node.id;
              var msg = '<table style="font-size:13px"><tr><td>Property:</td><td><b>' + data.node.id.split('/')[5] + '</b></td></tr>'
              msg += '</table>'
              Ext.getCmp(formid).body.child('div.property-target' + formid).update(msg)
              return true;
            }
          } //eo notifyDrop
        }); //eo propertydd
        var classTarget = this.body.child('div.class-target' + formid);
        var classdd = new Ext.dd.DropTarget(classTarget, {
          ddGroup: 'refdataGroup',
          notifyDrop: function (classdd, e, data) {
            if (data.node.attributes.type != 'ClassNode') {
              Ext.Msg.show({
                title: 'Invalid Selection',
                msg: 'Please slect a RDL Class...',
                icon: Ext.MessageBox.ERROR,
                buttons: Ext.Msg.CANCEL
              });
              return false;
            }
            Ext.get('classLabel').dom.value = data.node.attributes.record.Label;
            Ext.get('classUrl').dom.value = data.node.attributes.record.Uri;
            var msg = '<table style="font-size:13px"><tr><td>Class Label:</td><td><b>' + data.node.attributes.record.Label + '</b></td></tr>'
            msg += '</table>'
            Ext.getCmp(formid).body.child('div.class-target' + formid).update(msg)
            return true;

          } //eo notifyDrop
        }); //eo propertydd
      }
    });

    var win = new Ext.Window({
      closable: true,
      modal: false,
      layout: 'form',
      title: 'Add new GraphMap to Mapping',
      items: form,
      height: 200,
      width: 430,
      plain: true,
      scope: this
    });

    win.show();
  },

  onClose: function (btn, e) {
    if (btn != undefined) {
      var win = btn.findParentByType('window');
      if (win != undefined)
        win.close();
    }
  },

  onSubmit: function (btn, e) {
    var form = btn.findParentByType('form');
    var win = btn.findParentByType('window');
    var objectname = Ext.get('objectName').dom.value;
    var classlabel = Ext.get('classLabel').dom.value;
    var classuri = Ext.get('classUrl').dom.value;
    var graphname = Ext.get('graphName').dom.value;

    if (form.getForm().isValid());
    Ext.Ajax.request({
      url: 'mapping/addgraphmap',
      method: 'POST',
      params: {
        objectName: objectname,
        classLabel: classlabel,
        classUrl: classuri,
        graphName: graphname
      },
      success: function (result, request) {
        win.close();
        Ext.Msg.show({ title: 'Success', msg: 'Added GraphMap to mapping', icon: Ext.MessageBox.SUCCESS, buttons: Ext.Msg.OK });
      },
      failure: function (result, request) {
        Ext.Msg.show({ title: 'Failure', msg: 'Failed to Add GraphMap to mapping', icon: Ext.MessageBox.ERROR, buttons: Ext.Msg.CANCEL });
      }
    })
  },

  onAddTemplateMap: function (node) {
  },

  onDeleteGraphMap: function (node) {
    var that = this;
    var node = this.getSelectedNode();
    Ext.Ajax.request({
      url: 'mapping/deleteGraph',
      method: 'POST',
      params: {
        Scope: this.scope.Name,
        Application: this.application.Name,
        mappingNode: node.id
      },
      success: function (result, request) {
        that.onReload();
        Ext.Msg.alert('Success', 'Graph removed from mapping');
      },
      failure: function (result, request) { }
    })
  },

  onDeleteTemplateMap: function (node) {
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
    this.mappingPanel.getSelectionModel().select(node);
    this.onClick(node);
  }
});