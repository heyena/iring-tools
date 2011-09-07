/// <reference path="../../extjs4/ext-all-debug.js" />

/**
* @class AdapterManager.MappingPanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
Ext.define('AdapterManager.MappingPanel', {
  extend: 'Ext.panel.Panel',
  alias: 'widget.AdapterManager.MappingPanel',
  height: 300,
  minSize: 150,
  layout: 'border',
  split: true,
  closable: true,
  navigationUrl: null,
  propertyPanel: null,
  mappingPanel: null,
  scope: null,
  application: null,
  parentClass: null,
  mappingMenu: null,
  graphmapMenu: null,
  templatemapMenu: null,
  rolemapMenu: null,
  classmapMenu: null,
  directoryPanel: null,
  contentPanel: null,
  searchPanel: null,
  mappingStore: null,
  ajaxProxy: null,
  iconCls: 'tabsMapping',

  /**
  * initComponent
  * @protected
  */
  initComponent: function () {

    this.tbar = new Ext.toolbar.Toolbar();
    this.tbar.add(this.buildToolbar());

    this.templatemapMenu = new Ext.menu.Menu();
    this.templatemapMenu.add(this.buildTemplateMapMenu());

    this.rolemapMenu = new Ext.menu.Menu();
    this.rolemapMenu.add(this.buildRoleMapMenu());

    this.classmapMenu = new Ext.menu.Menu();
    this.classmapMenu.add(this.buildClassMapMenu());

    this.ajaxProxy = Ext.create('Ext.data.proxy.Ajax', {
      timeout: 120000,
      actionMethods: { read: 'POST' },
      url: this.navigationUrl,
      extraParams: {
        type: 'MappingNode',
        id: null,
        range: null,
        graph: selectedDirectoryNode.data.id
      },
      reader: { type: 'json' }
    });

    Ext.define('mappingmodel', {
      extend: 'Ext.data.Model',
      fields: [
         { name: 'id', type: 'string' },
         { name: 'property', type: 'object' },
         { name: 'identifier', type: 'string' },
         { name: 'text', type: 'string' },
         { name: 'type', type: 'string' },
         { name: 'record', type: 'object' }
       ]
    });

    this.mappingStore = Ext.create('Ext.data.TreeStore', {
      model: 'mappingmodel',
      clearOnLoad: true,
      root: {
        type: 'MappingNode',
        id: this.scope.Name + "/" + this.application.Name,
        expanded: true
      },
      proxy: this.ajaxProxy

    });

    this.mappingStore.on("beforeload", function (store, operation, eopts) {
      this.ajaxProxy.extraParams.type = operation.node.data.type;
      if (operation.node.data != undefined) {
        this.ajaxProxy.extraParams.id = operation.node.data.identifier;
      }
    }, this);


    this.mappingPanel = Ext.create('Ext.tree.Panel', {
      region: 'center',
      id: 'Mapping-Panel',
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
      enableDD: true,
      stateful: true,
      stateEvents: ['expand', 'collapse'],
      stateId: 'mappingPanel-state-id',
      getState: function () {
        return {
          collapsed: this.collapsed
        }
      },
      selModel: new Ext.selection.RowModel({ mode: 'SINGLE' }),
      viewConfig: {
        plugins: {
          ptype: 'treeviewdragdrop',
          dropGroup: 'refdataGroup'
        }
      },
      store: this.mappingStore,
      scroll: 'both'
      //bbar: new Ext.ux.StatusBar({ defaultText: 'Ready', statusAlign: 'right' }),      
    });


    this.mappingPanel.on('expand', this.onExpandNode, this);
    this.mappingPanel.on('itemcontextmenu', this.showContextMenu, this);
    this.mappingPanel.on('itemclick', this.onClick, this);
    this.mappingPanel.on('select', this.onSelect, this);
    this.mappingPanel.getView().on('beforedrop', this.onBeforeNodedrop, this);


    this.propertyPanel = new Ext.grid.property.Grid({
      title: 'Details',
      region: 'east',
      width: 350,
      split: true,
      viewConfig: { stripeRows: true },
      collapsible: true,
      autoScroll: true,
      border: 0,
      frame: false,
      height: 150, 
      selModel: new Ext.selection.RowModel({ mode: 'SINGLE' }),
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
        ]
    // super
    this.callParent(arguments);

    // this.mappingStore.load();
  },

  buildToolbar: function () {
    return [
      {
        xtype: 'button',
        text: 'Reload',
        handler: this.onReload,
        icon: 'Content/img/16x16/view-refresh.png',
        scope: this
      },
      {
        xtype: 'button',
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
            xtype: 'button',
            text: 'Add ClassMap',
            handler: this.onAddClassMap,
            icon: 'Content/img/16x16/document-new.png',
            scope: this
          },
          {
            xtype: 'button',
            text: 'Make Possessor',
            handler: this.onMakePossessor,
            // icon: 'Content/img/16x16/view-refresh.png',
            scope: this
          },
          {
            xtype: 'button',
            text: 'Map Property',
            handler: this.onMapProperty,
            // icon: '',
            scope: this
          },
          {
            xtype: 'button',
            text: 'Map ValueList',
            handler: this.onMapValueList,
            // icon: '',
            scope: this
          },
          {
            xtype: 'button',
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
            xtype: 'button',
            text: 'Delete ClassMap',
            handler: this.onDeleteClassMap,
            icon: 'Content/img/16x16/edit-delete.png',
            scope: this
          },
          {
            xtype: 'button',
            text: 'Change ClassMap',
            handler: this.onChangeClassMap,
            // icon:'',
            scope: this
          }
      ]
  },

  onSelect: function (selModel, model, idx) {
    selectedMappingNode = model.store.getAt(idx);
  },

  onSave: function (c) {
    var that = this;
    Ext.Ajax.request({
      url: 'mapping/updateMapping',
      method: 'POST',
      params: {
        Scope: that.scope.Name,
        Application: that.application.Name
      },
      success: function (result, request) {
        that.onReload();
        //          var node = selectedMappingNode;
        //          if (node.expanded == false)
        //    node.expand();
      },
      failure: function (result, request) {
        return false;
      }
    })
  },

  onReload: function () {
    //  this.mappingPanel.root.reload();
    //Ext.state.Manager.clear('mapping-state-' + this.scope.Name + '-' + this.application.Name);
    var panel = this.mappingPanel;
    var thisTreePanel = Ext.getCmp('Mapping-Panel');

    //get state from tree
    var state = thisTreePanel.getState();
    panel.body.mask('Loading', 'x-mask-loading');

    thisTreePanel.getStore().load();
    panel.body.unmask();
    thisTreePanel.applyState(state, true);
  },


  onReloadNode: function (node) {
    node.reload();
  },

  onExpand: function (node) {
    Ext.state.Manager.set('mapping-state-' + this.scope.Name + '-' + this.application.Name, node.getPath());
  },
  getParentClass: function (n) {

    if (n.parentNode != undefined) {
      if ((n.parentNode.data.type == 'ClassMapNode'
         || n.parentNode.data.type == 'GraphMapNode')
         && n.parentNode.data.identifier != undefined) {
        this.parentClass = n.parentNode.data.identifier;
        return this.parentClass;
      }
      else {
        this.getParentClass(n.parentNode);
      }
    }

  },

  onBeforeNodedrop: function (domel, source, target, dropPosition) {
    this.getParentClass(target);
    var tree = this.mappingPanel;
    var nodetype, thistype, icn, txt, templateId, rec, parentId, context;
    if (target.data.type == 'RoleMapNode') {
      reference = source.records[0].data.record.Uri;
      label = source.records[0].data.record.Label;
      roleId = target.data.record.id;
      roleName = target.data.record.name;
      rec = source.records[0].data.record;
      txt = source.records[0].data.record.Label;
      context = target.data.id + '/' + txt;

      parentId = this.parentClass;
      f = false;
      var that = this;
      tree.getEl().mask('Loading...');
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
          tree.getEl().unmask();
          that.onReload();
        },
        failure: function (result, request) {
          //don't drop it
          return false;
        }
      })
    }
    if (source.records[0].data.type == 'TemplateNode') {
      ntype = target.data.type;
      parentid = target.data.identifier;
      thistype = source.records[0].data.type;
      icn = 'Content/img/template-map.png';
      txt = source.records[0].data.record.Label;
      templateId = source.records[0].data.identifier;
      rec = source.records[0].data.record;
      context = target.data.id + '/' + txt;
      lf = false;
      var that = this;
      tree.getEl().mask('Loading...');
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
          tree.getEl().unmask();
          that.onReload();
          return false;
        },
        failure: function (result, request) {
          return false;
        }
      })
    }
    else {
      return false;
    }

    //e.cancel = true; //don't want to remove it from the source
    return false;

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
    var objectname = form.form.findField('objectName').getValue();
    var classlabel = form.form.findField('classLabel').getValue();
    var classurl = form.form.findField('classUrl').getValue();
    if (form.getForm().isValid())
      Ext.Ajax.request({
        url: 'mapping/addclassmap',
        method: 'POST',
        params: {
          objectName: objectname,
          classLabel: classlabel,
          classUrl: classurl,
          mappingNode: selectedMappingNode.data.id
        },
        success: function (result, request) {
          that.onReload();
          win.close();
          //Ext.Msg.show({ title: 'Success', msg: 'Added ClassMap to Rolemap', icon: Ext.MessageBox.INFO, buttons: Ext.Msg.OK });
        },
        failure: function (result, request) {
          //Ext.Msg.show({ title: 'Failure', msg: 'Failed to Add ClassMap to RoleMap', icon: Ext.MessageBox.ERROR, buttons: Ext.Msg.CANCEL });
          var message = 'Failed to Add ClassMap to RoleMap';
          showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
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
    var node = selectedMappingNode;
    Ext.Ajax.request({
      url: 'mapping/editGraphName',
      method: 'POST',
      params: {
        Scope: this.scope.Name,
        Application: this.application.Name,
        mappingNode: node.data.id,
        graphName: graphname
      },
      success: function (result, request) {
        //Ext.Msg.show({ title: 'Success', msg: 'Graph [' + node.id.split('/')[2] + '] renamed to [' + graphname + ']', icon: Ext.MessageBox.INFO, buttons: Ext.MessageBox.OK });
        var oldName = node.data.id.split('/')[2];
        that.rootNode.removeChild(node);
        win.close();
        that.contentPanel.removeAll(true);
        that.directoryPanel.reload();
      },
      failure: function (result, request) { }
    })
  },

  onDeleteTemplateMap: function () {
    var that = this;
    var node = selectedMappingNode;
    that.getParentClass(node);
    Ext.Ajax.request({
      url: 'mapping/deleteTemplateMap',
      method: 'POST',
      params: {
        Scope: this.scope.Name,
        Application: this.application.Name,
        mappingNode: node.data.id,
        parentIdentifier: that.parentClass,
        identifier: node.data.identifier
      },
      success: function (result, request) {
        that.onReload();
        //Ext.Msg.show({ title: 'Success', msg: 'Template [' + node.id.split('/')[3] + '] removed from mapping', icon: Ext.MessageBox.INFO, buttons: Ext.MessageBox.OK });
      },
      failure: function (result, request) { }
    })

  },

  onResetMapping: function (node) {
    var that = this;
    var node = selectedMappingNode;
    this.rolemapMenu.hide();
    Ext.Ajax.request({
      url: 'mapping/resetmapping',
      method: 'POST',
      params: {
        mappingNode: node.data.id,
        roleId: node.data.record.id,
        templateId: node.parentNode.data.record.id,
        parentClassId: node.parentNode.parentNode.data.identifier
      },
      success: function (result, request) {
        that.onReload();
        //Ext.Msg.show({ title: 'Success', msg: 'Made [' + node.attributes.id.split('/')[4] + '] possessor role', icon: Ext.MessageBox.INFO, buttons: Ext.MessageBox.OK });
      },
      failure: function (result, request) { }
    })

  },

  onMapProperty: function (node) {
    var mapnode = selectedMappingNode;
    this.rolemapMenu.hide();
    var formid = 'propertytarget-' + this.scope.Name + '-' + this.application.Name;
    var thisform = new Ext.form.Panel({
      id: formid,
      //   layout: 'form',
      method: 'POST',
      border: false,
      frame: false,
      bbar: [
        { xtype: 'tbfill' },
        { text: 'Ok', scope: this, handler: this.onSubmitPropertyMap },
        { text: 'Cancel', scope: this, handler: this.onClose }
        ],
      items: [
              { xtype: 'hidden', name: 'propertyName', id: 'propertyName' },
              { xtype: 'hidden', name: 'relatedObject', id: 'relatedObject' }
             ],
      html: '<div class="property-target' + formid + '" '
          + 'style="border:1px silver solid;margin:5px;padding:8px;height:20px">'
          + 'Drop a Property Node here.</div>',

      afterRender: function (cmp, eOpts) {
        Ext.form.Panel.prototype.afterRender.apply(this, arguments);

        var propertyTarget = this.body.child('div.property-target' + formid);
        var propertydd = new Ext.dd.DropTarget(propertyTarget, {
          ddGroup: 'propertyGroup',
          notifyEnter: function (dd, e, data) {
            if (data.records[0].data.type != 'DataPropertyNode' && data.records[0].data.type != 'KeyDataPropertyNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;
          },
          notifyOver: function (dd, e, data) {
            if (data.records[0].data.type != 'DataPropertyNode' && data.records[0].data.type != 'KeyDataPropertyNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;
          },
          notifyDrop: function (dd, e, data) {
            if (data.records[0].data.type != 'DataPropertyNode' && data.records[0].data.type != 'KeyDataPropertyNode') {
              return false;
            }
            else {
              thisform.form.findField('propertyName').setValue(data.records[0].data.record.Name);
              if (data.records[0].parentNode != undefined
                  && data.records[0].parentNode.data.record != undefined
                  && data.records[0].parentNode.data.type != 'DataObjectNode')
                thisform.form.findField('relatedObject').setValue(data.records[0].parentNode.data.record.Name);
              var msg = '<table style="font-size:13px"><tr><td>Property:</td><td><b>' + data.records[0].data.record.Name + '</b></td></tr>'
              msg += '</table>'
              thisform.body.child('div.property-target' + formid).update(msg)
              return true;
            }
          } //eo notifyDrop
        }); //eo propertydd
      }
    });

    var win = new Ext.window.Window({
      closable: true,
      modal: false,
      //  layout: 'form',
      title: 'Map Data Property to RoleMAp',
      items: thisform,
      // height: 120,
      width: 430,
      plain: true,
      scope: this
    });

    win.show();

  },

  onSubmitPropertyMap: function (btn, e) {
    var that = this;
    // var related = "";
    var form = btn.findParentByType('form');
    var node = selectedMappingNode;
    var win = btn.findParentByType('window');
    var propertyNames = form.form.findField('propertyName').getValue();
    var related = form.form.findField('relatedObject').getValue();
    if (form.getForm().isValid())
      Ext.Ajax.request({
        url: 'mapping/mapproperty',
        method: 'POST',
        params: {
          propertyName: propertyNames,
          mappingNode: node.data.id,
          classId: node.parentNode.parentNode.data.identifier,
          relatedObject: related
        },
        success: function (result, request) {
          that.onReload();
          win.close();
          //Ext.Msg.show({ title: 'Success', msg: 'Mapped Property to Rolemap', icon: Ext.MessageBox.INFO, buttons: Ext.Msg.OK });
        },
        failure: function (result, request) {
          //Ext.Msg.show({ title: 'Failure', msg: 'Failed to Map Property to RoleMap', icon: Ext.MessageBox.ERROR, buttons: Ext.Msg.CANCEL });
          var message = 'Failed to Map Property to RoleMap';
          showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
        }
      })
  },

  onMapValueList: function (node) {
    var mapnode = selectedMappingNode;
    this.rolemapMenu.hide();
    var formid = 'valuelisttarget-' + this.scope.Name + '-' + this.application.Name;
    var form = new Ext.form.FormPanel({
      id: formid,
      layout: 'form',
      method: 'POST',
      border: false,
      frame: false,
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
            if (data.node.data.type != 'DataPropertyNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;
          },
          notifyOver: function (dd, e, data) {
            if (data.node.data.type != 'DataPropertyNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;
          },
          notifyDrop: function (dd, e, data) {
            if (data.node.data.type != 'DataPropertyNode') {
              return false;
            }
            else {
              Ext.get('propertyName').dom.value = data.data.node.id;
              var msg = '<table style="font-size:13px"><tr><td>Property:</td><td><b>' + data.node.data.id.split('/')[5] + '</b></td></tr>'
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

              var message = 'Please slect a RDL Class...';
              showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
              return false;
            }
            Ext.get('objectNames').dom.value = data.node.data.id;
            var mapNode = Ext.get('mappingPanel')
            var msg = '<table style="font-size:13px"><tr><td>Value List:</td><td><b>' + data.node.data.id.split('/')[4] + '</b></td></tr>'
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
      //height: 180,
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
    var node = selectedMappingNode;
    if (form.getForm().isValid())
      Ext.Ajax.request({
        url: 'mapping/mapvaluelist',
        method: 'POST',
        params: {
          propertyName: propertyNames,
          objectNames: objectname,
          mappingNode: node.data.id,
          classId: node.parentNode.parentNode.data.identifier
        },
        success: function (result, request) {
          that.onReload();
          win.close();
          //Ext.Msg.show({ title: 'Success', msg: 'Mapped ValueList to Rolemap', icon: Ext.MessageBox.INFO, buttons: Ext.Msg.OK });
        },
        failure: function (result, request) {
          //Ext.Msg.show({ title: 'Failure', msg: 'Failed to Map ValueList to RoleMap', icon: Ext.MessageBox.ERROR, buttons: Ext.Msg.CANCEL });
          var message = 'Failed to Map ValueList to RoleMap';
          showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
        }
      })
  },


  onMakePossessor: function () {
    this.rolemapMenu.hide();
    var that = this;
    var node = selectedMappingNode;
    Ext.Ajax.request({
      url: 'mapping/makePossessor',
      method: 'POST',
      params: {
        mappingNode: node.data.id,
        classId: node.parentNode.parentNode.data.identifier
      },
      success: function (result, request) {
        that.onReload();
        //Ext.Msg.show({ title: 'Success', msg: 'Made [' + node.attributes.id.split('/')[4] + '] possessor role', icon: Ext.MessageBox.INFO, buttons: Ext.MessageBox.OK });
      },
      failure: function (result, request) { }
    })
  },

  onAddClassMap: function () {
    this.rolemapMenu.hide();
    var mapnode = selectedMappingNode;
    var formid = 'classtarget-' + this.scope.Name + '-' + this.application.Name;
    var thisform = new Ext.form.Panel({
      id: formid,
      // layout: 'form',
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
              { xtype: 'hidden', name: 'classUrl', id: 'classUrl' }
             ],
      html: '<div class="property-target' + formid + '" '
          + 'style="border:1px silver solid;margin:5px;padding:8px;height:20px">'
          + 'Drop a Property Node here.</div>'
          + '<div class="class-target' + formid + '" '
          + 'style="border:1px silver solid;margin:5px;padding:8px;height:20px">'
          + 'Drop a Class Node here. </div>',

      afterRender: function (cmp, eOpts) {
        Ext.form.Panel.prototype.afterRender.apply(this, arguments);
        var that = this;
        var propertyTarget = this.body.child('div.property-target' + formid);
        var propertydd = new Ext.dd.DropTarget(propertyTarget, {
          ddGroup: 'propertyGroup',
          notifyEnter: function (dd, e, data) {
            if (data.records[0].data.type != 'DataPropertyNode' && data.records[0].data.type != 'KeyDataPropertyNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;
          },
          notifyOver: function (dd, e, data) {
            if (data.records[0].data.type != 'DataPropertyNode' && data.records[0].data.type != 'KeyDataPropertyNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;
          },
          notifyDrop: function (dd, e, data) {
            if (data.records[0].data.type != 'DataPropertyNode' && data.records[0].data.type != 'KeyDataPropertyNode') {

              return false;
            }
            else {
              thisform.form.findField('objectName').setValue(data.records[0].data.id);
              var msg = '<table style="font-size:13px"><tr><td>Property:</td><td><b>' + data.records[0].data.id.split('/')[5] + '</b></td></tr>'
              msg += '</table>'
              thisform.body.child('div.property-target' + formid).update(msg)
              return true;
            }
          } //eo notifyDrop
        }); //eo propertydd
        var classTarget = this.body.child('div.class-target' + formid);
        var classdd = new Ext.dd.DropTarget(classTarget, {
          ddGroup: 'refdataGroup',
          notifyEnter: function (dd, e, data) {
            if (data.records[0].data.record.type != 'ClassNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;
          },
          notifyOver: function (dd, e, data) {
            if (data.records[0].data.type != 'ClassNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;
          },
          notifyDrop: function (classdd, e, data) {
            if (data.records[0].data.type != 'ClassNode') {

              var message = 'Please slect a RDL Class...';
              showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
              return false;
            }
            thisform.form.findField('classLabel').setValue(data.records[0].data.record.Label);
            thisform.form.findField('classUrl').setValue(data.records[0].data.record.Uri);

            var msg = '<table style="font-size:13px"><tr><td>Class Label:</td><td><b>' + data.records[0].data.record.Label + '</b></td></tr>';
            msg += '</table>';
            thisform.body.child('div.class-target' + formid).update(msg);
            return true;

          } //eo notifyDrop
        }); //eo propertydd
      }
    });

    var win = new Ext.window.Window({
      closable: true,
      modal: false,
      // layout: 'form',
      title: 'Add new ClassMap to RoleMAp',
      items: thisform,
      width: 430,
      plain: true,
      scope: this
    });

    win.show();
  },

  onDeleteClassMap: function (mapnode) {
    var that = this;
    this.rolemapMenu.hide();
    var node = this.mappingPanel.getSelectionModel().getSelectedNode();
    Ext.Ajax.request({
      url: 'mapping/deleteclassmap',
      method: 'POST',
      params: {
        mappingNode: node.attributes.id,
        classId: node.attributes.identifier,
        parentClass: node.parentNode.parentNode.parentNode.attributes.identifier,
        parentTemplate: node.parentNode.parentNode.attributes.record.id,
        parentRole: node.parentNode.attributes.record.id
      },
      success: function (result, request) {
        that.onReload();
      },
      failure: function (result, request) { }
    })
  },

  onClick: function (view, model, node, index, e) {
    try {
      var obj = model.store.getAt(index).data;
      if (obj.property != null && obj.property != "") {

        this.propertyPanel.setSource(obj.property);
      } else {
        this.propertyPanel.setSource(obj.record);
      }
    } catch (e) {

    }
  },

  showContextMenu: function (view, model, node, index, e) {

    e.stopEvent();
    var obj = model.store.getAt(index).data;

    if (obj.type == "MappingNode") {
      this.mappingMenu.showAt(e.getXY());
    } else if (obj.type == "TemplateMapNode") {
      this.templatemapMenu.showAt(e.getXY());
    } else if (obj.type == "RoleMapNode") {
      this.rolemapMenu.showAt(e.getXY());
    } else if (obj.type == "ClassMapNode") {
      this.classmapMenu.showAt(e.getXY());
    }
    //this.mappingPanel.getSelectionMedel().select(node);
    //this.onClick(node);
  }
});

  