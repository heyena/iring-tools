/// <reference path="../ext-3.2.1/adapter/ext/ext-base.js" />
/// <reference path="../ext-3.2.1/ext-all.js" />
/// <reference path="../../ext-3.3.1/ext-all-debug-w-comments.js" />


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
  parentClass: null,
  mappingMenu: null,
  graphmapMenu: null,
  templatemapMenu: null,
  rolemapMenu: null,
  classmapMenu: null,
  directoryPanel: null,
  contentPanel: null,
  searchPanel: null,

  iconCls: 'tabsMapping',

  /**
  * initComponent
  * @protected
  */
  initComponent: function () {

    this.tbar = this.buildToolbar();

    //    this.graphmapMenu = new Ext.menu.Menu();
    //    this.graphmapMenu.add(this.buildGraphmapMenu());

    this.templatemapMenu = new Ext.menu.Menu();
    this.templatemapMenu.add(this.buildTemplateMapMenu());

    this.rolemapMenu = new Ext.menu.Menu();
    this.rolemapMenu.add(this.buildRoleMapMenu());

    this.classmapMenu = new Ext.menu.Menu();
    this.classmapMenu.add(this.buildClassMapMenu());

    this.treeLoader = new Ext.tree.TreeLoader({
      baseParams: {
        type: null,
        id: null,
        range: null,
        graph: this.directoryPanel.getSelectedNode().attributes.id
      },
      url: this.navigationUrl
    });

    this.treeLoader.on("beforeload", function (treeLoader, node) {
      treeLoader.baseParams.type = node.attributes.type;
      if (node.attributes.record != undefined) {
        treeLoader.baseParams.id = node.attributes.record.id;
      }
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
      id: 'Mapping-Panel',
      enableDD: true,
      ddGroup: 'refdataGroup',
      split: true,
      border: true,
      collapseMode: 'mini',
      height: 300,
      layout: 'fit',
      lines: true,
      expandAll: true,
      rootVisible: false,
      pathSeparator: '>',
      lines: true,
      autoScroll: true,
      loader: this.treeLoader,
      root: this.rootNode,
      bbar: new Ext.ux.StatusBar({ defaultText: 'Ready', statusAlign: 'right' }),
      stateful: true,
      getState: function () {
        var nodes = [];
        this.getRootNode().eachChild(function (child) {
          //function to store state of tree recursively
          var storeTreeState = function (node, expandedNodes) {
            if (node.isExpanded() && node.childNodes.length > 0) {
              expandedNodes.push(node.getPath());
              node.eachChild(function (child) {
                storeTreeState(child, expandedNodes);
              });
            }
          };
          storeTreeState(child, nodes);
        });

        return {
          expandedNodes: nodes
        }
      },
      applyState: function (state, isOnClick) {
        var that = this;
        //this.getLoader().on('load', function () {
        if (isOnClick == true) {
          var nodes = state.expandedNodes;
          for (var i = 0; i < nodes.length; i++) {
            if (typeof nodes[i] != 'undefined') {
              that.expandPath(nodes[i]);
            }
          }
        }
        //});
      }
    });



    this.mappingPanel.on('beforenodedrop', this.onBeforeNodedrop, this);
    this.mappingPanel.on('expandnode', this.onExpandNode, this);
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
      source: {},
      listeners: {
        beforeedit: function (e) { e.cancel = true; },
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


    var state = Ext.state.Manager.get('mapping-state-' + this.scope.Name + '-' + this.application.Name);

    if (state) {
      if (this.mappingPanel.expandPath(state) == false) {
        Ext.state.Manager.clear('mapping-state-' + this.scope.Name + '-' + this.application.Name);
        this.mappingPanel.root.reload();
      }
    }

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
          {
            text: 'Delete ClassMap',
            handler: this.onDeleteClassMap,
            icon: 'Content/img/16x16/edit-delete.png',
            scope: this
          },
          {
            text: 'Change ClassMap',
            handler: this.onChangeClassMap,
            // icon:'',
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
        Ext.Msg.show({ title: 'Success', msg: 'Mapping saved to the server', icon: Ext.MessageBox.INFO, buttons: Ext.Msg.OK });
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
    //  this.mappingPanel.root.reload();
    //Ext.state.Manager.clear('mapping-state-' + this.scope.Name + '-' + this.application.Name);
    var panel = this.directoryPanel;
    var thisTreePanel = Ext.getCmp('Mapping-Panel');

    //get state from tree
    var state = thisTreePanel.getState();
    panel.body.mask('Loading', 'x-mask-loading');

    thisTreePanel.getLoader().load(thisTreePanel.getRootNode(), function () {
      panel.body.unmask();
      thisTreePanel.applyState(state, true);
    });
  },


  onReloadNode: function (node) {
    node.reload();
  },

  onExpandNode: function (node) {
    Ext.state.Manager.set('mapping-state-' + this.scope.Name + '-' + this.application.Name, node.getPath());
  },
  getParentClass: function (n) {

    if (n.parentNode != undefined) {
      if ((n.parentNode.attributes.type == 'ClassMapNode'
         || n.parentNode.attributes.type == 'GraphMapNode')
         && n.parentNode.attributes.identifier != undefined) {
        this.parentClass = n.parentNode.attributes.identifier;
        return this.parentClass;
      }
      else {
        this.getParentClass(n.parentNode);
      }
    }

  },
  onBeforeNodedrop: function (e) {
    e.target.expand();
    this.getParentClass(e.target);
    // var dir = this.getParentClass(e.target);
    var nodetype, thistype, icn, txt, templateId, rec, parentId, context;
    if (e.target.attributes.type == 'RoleMapNode') {
      reference = e.data.node.attributes.record.Uri;
      label = e.data.node.attributes.record.Label;
      roleId = e.target.attributes.record.id;
      roleName = e.target.attributes.record.name;
      rec = e.data.node.attributes.record;
      txt = e.data.node.attributes.record.Label;
      context = e.target.id + '/' + txt;

      parentId = this.parentClass;
      f = false;
      var that = this;
      e.tree.getEl().mask('Loading...');
      Ext.Ajax.request({
        url: 'mapping/mapreference',
        method: 'POST',
        params: {
          reference: reference,
          classId: parentId,
          label: label,
          roleId: roleId,
          roleName: roleName,
          ctx: context
        },
        success: function (result, request) {
          e.tree.getEl().unmask();
          that.onReload();
        },
        failure: function (result, request) {
          //don't drop it
          return false;
        }
      })
    }
    if (e.data.node.attributes.type == 'TemplateNode') {
      ntype = e.target.attributes.type;
      parentid = e.target.attributes.identifier;
      thistype = e.data.node.attributes.type;
      icn = 'Content/img/template-map.png';
      txt = e.data.node.attributes.record.Label;
      templateId = e.data.node.attributes.identifier;
      rec = e.data.node.attributes.record;
      context = e.target.id + '/' + txt;
      lf = false;
      var that = this;
      e.tree.getEl().mask('Loading...');
      Ext.Ajax.request({
        url: 'mapping/addtemplatemap',
        method: 'POST',
        params: {
          nodetype: thistype,
          parentType: ntype,
          parentId: parentid,
          id: templateId,
          ctx: context
        },
        success: function (result, request) {
          e.tree.getEl().unmask();
          that.onReload();
        },
        failure: function (result, request) {
          //don't drop it
          return false;
        }
      })
    }
    else {
      return false;
    }

    e.cancel = true; //don't want to remove it from the source
    return true;

  },

  onClose: function (btn, e) {
    if (btn != undefined) {
      var win = btn.findParentByType('window');
      if (win != undefined)
        win.close();
    }
  },

  onSubmitClassMap: function (btn, e) {
    var that = this;
    var form = btn.findParentByType('form');
    var win = btn.findParentByType('window');
    var objectname = Ext.get('objectName').dom.value;
    var classlabel = Ext.get('classLabel').dom.value;
    var classurl = Ext.get('classUrl').dom.value;
    var mapNode = Ext.get('mappingNode').dom.value;
    if (form.getForm().isValid())
      Ext.Ajax.request({
        url: 'mapping/addclassmap',
        method: 'POST',
        params: {
          objectName: objectname,
          classLabel: classlabel,
          classUrl: classurl,
          mappingNode: that.mappingPanel.getSelectionModel().getSelectedNode().attributes.id
        },
        success: function (result, request) {
          that.onReload();
          win.close();
          // Ext.Msg.show({ title: 'Success', msg: 'Added ClassMap to Rolemap', icon: Ext.MessageBox.INFO, buttons: Ext.Msg.OK });
        },
        failure: function (result, request) {
          Ext.Msg.show({ title: 'Failure', msg: 'Failed to Add ClassMap to RoleMap', icon: Ext.MessageBox.ERROR, buttons: Ext.Msg.CANCEL });
        }
      })
  },

  onAddTemplateMap: function (node) {
  },

  onSubmitEditGraphName: function (btn, e) {
    var that = this;
    var form = btn.findParentByType('form');
    var win = btn.findParentByType('window');
    var graphname = Ext.get('graphName').dom.value;
    var node = this.getSelectedNode();
    Ext.Ajax.request({
      url: 'mapping/editGraphName',
      method: 'POST',
      params: {
        Scope: this.scope.Name,
        Application: this.application.Name,
        mappingNode: node.id,
        graphName: graphname
      },
      success: function (result, request) {
        // Ext.Msg.show({ title: 'Success', msg: 'Graph [' + node.id.split('/')[2] + '] renamed to [' + graphname + ']', icon: Ext.MessageBox.INFO, buttons: Ext.MessageBox.OK });
        var oldName = node.id.split('/')[2];
        that.rootNode.removeChild(node);
        //var newid = node.id.replace(oldName, graphname);
        //node.id = newid;
        //that.rootNode.appendChild(node);
        win.close();
        that.contentPanel.removeAll(true);
        that.directoryPanel.reload();
      },
      failure: function (result, request) { }
    })
  },

  onDeleteTemplateMap: function () {
    var that = this;

    var node = that.mappingPanel.getSelectionModel().getSelectedNode();
    that.getParentClass(node);
    Ext.Ajax.request({
      url: 'mapping/deleteTemplateMap',
      method: 'POST',
      params: {
        Scope: this.scope.Name,
        Application: this.application.Name,
        mappingNode: node.id,
        parentIdentifier: that.parentClass,
        identifier: node.attributes.identifier
      },
      success: function (result, request) {
        that.onReload();
        // Ext.Msg.show({ title: 'Success', msg: 'Template [' + node.id.split('/')[3] + '] removed from mapping', icon: Ext.MessageBox.INFO, buttons: Ext.MessageBox.OK });
      },
      failure: function (result, request) { }
    })

  },

  onResetMapping: function (node) {
  },

  onMapProperty: function (node) {
    var mapnode = this.mappingPanel.getSelectionModel().getSelectedNode();
    var formid = 'propertytarget-' + this.scope.Name + '-' + this.application.Name;
    var form = new Ext.form.FormPanel({
      id: formid,
      layout: 'form',
      method: 'POST',
      border: false,
      frame: false,
      bbar: [
          { xtype: 'tbfill' },
        { text: 'Ok', scope: this, handler: this.onSubmitPropertyMap },
        { text: 'Cancel', scope: this, handler: this.onClose }
        ],
      items: [
              { xtype: 'hidden', name: 'propertyName', id: 'propertyName' }
             ],
      html: '<div class="property-target' + formid + '" '
          + 'style="border:1px silver solid;margin:5px;padding:8px;height:20px">'
          + 'Drop a Property Node here.</div>',

      afterRender: function (cmp) {
        Ext.FormPanel.prototype.afterRender.apply(this, arguments);

        var propertyTarget = this.body.child('div.property-target' + formid);
        var propertydd = new Ext.dd.DropTarget(propertyTarget, {
          ddGroup: 'propertyGroup',
          notifyEnter: function (dd, e, data) {
            if (data.node.attributes.type != 'DataPropertyNode' && data.node.attributes.type != 'KeyDataPropertyNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;
          },
          notifyOver: function (dd, e, data) {
            if (data.node.attributes.type != 'DataPropertyNode' && data.node.attributes.type != 'KeyDataPropertyNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;
          },
          notifyDrop: function (dd, e, data) {
            if (data.node.attributes.type != 'DataPropertyNode' && data.node.attributes.type != 'KeyDataPropertyNode') {
              return false;
            }
            else {
              Ext.get('propertyName').dom.value = data.node.id;
              var msg = '<table style="font-size:13px"><tr><td>Property:</td><td><b>' + data.node.id.split('/')[5] + '</b></td></tr>'
              msg += '</table>'
              Ext.getCmp(formid).body.child('div.property-target' + formid).update(msg)
              return true;
            }
          } //eo notifyDrop
        }); //eo propertydd
      }
    });

    var win = new Ext.Window({
      closable: true,
      modal: false,
      layout: 'form',
      title: 'Map Data Property to RoleMAp',
      items: form,
     // height: 120,
      width: 430,
      plain: true,
      scope: this
    });

    win.show();

  },

  onSubmitPropertyMap: function (btn, e) {
    var that = this;
    var form = btn.findParentByType('form');
    var node = this.mappingPanel.getSelectionModel().getSelectedNode();
    var win = btn.findParentByType('window');
    var propertyNames = Ext.get('propertyName').dom.value;
    if (form.getForm().isValid())
      Ext.Ajax.request({
        url: 'mapping/mapproperty',
        method: 'POST',
        params: {
          propertyName: propertyNames,
          mappingNode: node.attributes.id,
          classId: node.parentNode.parentNode.attributes.identifier
        },
        success: function (result, request) {
          that.onReload();
          win.close();
          // Ext.Msg.show({ title: 'Success', msg: 'Mapped Property to Rolemap', icon: Ext.MessageBox.INFO, buttons: Ext.Msg.OK });
        },
        failure: function (result, request) {
          Ext.Msg.show({ title: 'Failure', msg: 'Failed to Map Property to RoleMap', icon: Ext.MessageBox.ERROR, buttons: Ext.Msg.CANCEL });
        }
      })
  },

  onMapValueList: function (node) {
    var mapnode = this.mappingPanel.getSelectionModel().getSelectedNode();
    var formid = 'valuelisttarget-' + this.scope.Name + '-' + this.application.Name;
    var form = new Ext.form.FormPanel({
      id: formid,
      layout: 'form',
      method: 'POST',
      border: false,
      frame: true,
      bbar: [
         { xtype: 'tbfill' },
        { text: 'Ok', scope: this, handler: this.onSubmitValuelistMap },
        { text: 'Cancel', scope: this, handler: this.onClose }
        ],
      items: [
              { xtype: 'hidden', name: 'objectNames', id: 'objectNames' },
              { xtype: 'hidden', name: 'propertyName', id: 'propertyName' }
             ],
      html: '<div class="property-target' + formid + '" '
          + 'style="border:1px silver solid;margin:5px;padding:8px;height:20px">'
          + 'Drop a Property Node here.</div>'
          + '<div class="class-target' + formid + '" '
          + 'style="border:1px silver solid;margin:5px;padding:8px;height:20px">'
          + 'Drop a ValueList Node here. </div>',

      afterRender: function (cmp) {
        Ext.FormPanel.prototype.afterRender.apply(this, arguments);

        var propertyTarget = this.body.child('div.property-target' + formid);
        var propertydd = new Ext.dd.DropTarget(propertyTarget, {
          ddGroup: 'propertyGroup',
          notifyEnter: function (dd, e, data) {
            if (data.node.attributes.type != 'DataPropertyNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;
          },
          notifyOver: function (dd, e, data) {
            if (data.node.attributes.type != 'DataPropertyNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;
          },
          notifyDrop: function (dd, e, data) {
            if (data.node.attributes.type != 'DataPropertyNode') {
              return false;
            }
            else {
              Ext.get('propertyName').dom.value = data.node.id;
              var msg = '<table style="font-size:13px"><tr><td>Property:</td><td><b>' + data.node.id.split('/')[5] + '</b></td></tr>'
              msg += '</table>'
              Ext.getCmp(formid).body.child('div.property-target' + formid).update(msg)
              return true;
            }
          } //eo notifyDrop
        }); //eo propertydd


        var valueListTarget = this.body.child('div.class-target' + formid);
        var classdd = new Ext.dd.DropTarget(valueListTarget, {
          ddGroup: 'propertyGroup',
          notifyEnter: function (dd, e, data) {
            if (data.node.attributes.type != 'ValueListNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;
          },
          notifyOver: function (dd, e, data) {
            if (data.node.attributes.type != 'ValueListNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;
          },
          notifyDrop: function (classdd, e, data) {
            if (data.node.attributes.type != 'ValueListNode') {
              Ext.Msg.show({
                title: 'Invalid Selection',
                msg: 'Please slect a RDL Class...',
                icon: Ext.MessageBox.ERROR,
                buttons: Ext.Msg.CANCEL
              });
              return false;
            }
            Ext.get('objectNames').dom.value = data.node.id;
            var mapNode = Ext.get('mappingPanel')
            var msg = '<table style="font-size:13px"><tr><td>Value List:</td><td><b>' + data.node.id.split('/')[4] + '</b></td></tr>'
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
      title: 'Map Valuelist to RoleMAp',
      items: form,
    //  height: 180,
      width: 430,
      plain: true,
      scope: this
    });

    win.show();

  },

  onSubmitValuelistMap: function (btn, e) {
    var that = this;
    var form = btn.findParentByType('form');
    var win = btn.findParentByType('window');
    var objectname = Ext.get('objectNames').dom.value;
    var propertyNames = Ext.get('propertyName').dom.value;
    var node = this.mappingPanel.getSelectionModel().getSelectedNode();
    if (form.getForm().isValid())
      Ext.Ajax.request({
        url: 'mapping/mapvaluelist',
        method: 'POST',
        params: {
          propertyName: propertyNames,
          objectNames: objectname,
          mappingNode: node.attributes.id,
          classId: node.parentNode.parentNode.attributes.identifier
        },
        success: function (result, request) {
          that.onReload();
          win.close();
          //   Ext.Msg.show({ title: 'Success', msg: 'Mapped ValueList to Rolemap', icon: Ext.MessageBox.INFO, buttons: Ext.Msg.OK });
        },
        failure: function (result, request) {
          Ext.Msg.show({ title: 'Failure', msg: 'Failed to Map ValueList to RoleMap', icon: Ext.MessageBox.ERROR, buttons: Ext.Msg.CANCEL });
        }
      })
  },


  onMakePossessor: function () {
    var that = this;
    var node = this.mappingPanel.getSelectionModel().getSelectedNode();
    Ext.Ajax.request({
      url: 'mapping/makePossessor',
      method: 'POST',
      params: {
        mappingNode: node.attributes.id,
        classId: node.parentNode.parentNode.attributes.identifier
      },
      success: function (result, request) {
        that.onReload();
        //  Ext.Msg.show({ title: 'Success', msg: 'Made [' + node.attributes.id.split('/')[4] + '] possessor role', icon: Ext.MessageBox.INFO, buttons: Ext.MessageBox.OK });
      },
      failure: function (result, request) { }
    })
  },

  onAddClassMap: function () {
    var mapnode = this.mappingPanel.getSelectionModel().getSelectedNode();
    var formid = 'classtarget-' + this.scope.Name + '-' + this.application.Name;
    var form = new Ext.form.FormPanel({
      id: formid,
      layout: 'form',
      method: 'POST',
      border: false,
      frame: false,
      bbar: [
            { xtype: 'tbfill' },
        { text: 'Ok', scope: this, handler: this.onSubmitClassMap },
        { text: 'Cancel', scope: this, handler: this.onClose }
        ],
      items: [
      //{ xtype: 'textfield', name: 'graphName', id: 'graphName', fieldLabel: 'Graph Name', width: 120, required: true, value: null },
              {xtype: 'hidden', name: 'objectName', id: 'objectName' },
              { xtype: 'hidden', name: 'classLabel', id: 'classLabel' },
              { xtype: 'hidden', name: 'classUrl', id: 'classUrl' },
              { xtype: 'hidden', name: 'mappingNode', id: 'mappingNode', value: this.rootNode }
             ],
      html: '<div class="property-target' + formid + '" '
          + 'style="border:1px silver solid;margin:5px;padding:8px;height:20px">'
          + 'Drop a Property Node here.</div>'
          + '<div class="class-target' + formid + '" '
          + 'style="border:1px silver solid;margin:5px;padding:8px;height:20px">'
          + 'Drop a Class Node here. </div>',

      afterRender: function (cmp) {
        Ext.FormPanel.prototype.afterRender.apply(this, arguments);
        var that = this;
        var propertyTarget = this.body.child('div.property-target' + formid);
        var propertydd = new Ext.dd.DropTarget(propertyTarget, {
          ddGroup: 'propertyGroup',
          notifyEnter: function (dd, e, data) {
            if (data.node.attributes.type != 'DataPropertyNode' && data.node.attributes.type != 'KeyDataPropertyNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;
          },
          notifyOver: function (dd, e, data) {
            if (data.node.attributes.type != 'DataPropertyNode' && data.node.attributes.type != 'KeyDataPropertyNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;
          },
          notifyDrop: function (dd, e, data) {
            if (data.node.attributes.type != 'DataPropertyNode' && data.node.attributes.type != 'KeyDataPropertyNode') {
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
          notifyEnter: function (dd, e, data) {
            if (data.node.attributes.record.type != 'ClassNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;
          },
          notifyOver: function (dd, e, data) {
            if (data.node.attributes.type != 'ClassNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;
          },
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
            var mapNode = Ext.get('mappingPanel')
            Ext.get('mappingNode').dom.value = mapnode.attributes.id;
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
      title: 'Add new ClassMap to RoleMAp',
      items: form,
    //  height: 180,
      width: 430,
      plain: true,
      scope: this
    });

    win.show();
  },

  onDeleteClassMap: function (mnode) {
    var that = this;
    var node = this.mappingPanel.getSelectionModel().getSelectedNode();
    Ext.Ajax.request({
      url: 'mapping/deleteclassmap',
      method: 'POST',
      params: {
        mappingNode: node.attributes.id
      },
      success: function (result, request) {
        that.onReload();
        // Ext.Msg.show({ title: 'Success', msg: 'Deleted Class Map from Mapping', icon: Ext.MessageBox.INFO, buttons: Ext.MessageBox.OK });
      },
      failure: function (result, request) { }
    })
  },

  showContextMenu: function (node, event) {

    //  if (node.isSelected()) { 
    var x = event.browserEvent.clientX;
    var y = event.browserEvent.clientY;

    var obj = node.attributes;

    if (obj.type == "MappingNode") {
      this.mappingMenu.showAt([x, y]);
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