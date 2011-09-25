Ext.define('AM.view.mapping.MappingTree', {
    extend: 'Ext.tree.Panel',
    alias: 'widget.mappingtree',
    region: 'center',
    split: true,
    border: true,
    collapseMode: 'mini',
    layout: 'fit',
    lines: true,
    expandAll: true,
    rootVisible: false,
    pathSeparator: '>',
    lines: true,
    region: 'center',
    enableDD: true,
    stateful: true,
    scope: null,
    node: null,
    application: null,
    record: null,
    parentClass: null,
    mappingMenu: null,
    graphmapMenu: null,
    templatemapMenu: null,
    rolemapMenu: null,
    classmapMenu: null,
    ajaxProxy: null,
    stateEvents: ['expand', 'collapse'],
    stateId: 'mappingPanel-state-id',
    scroll: 'both',
    viewConfig: {     
        plugins: {
            ptype: 'treeviewdragdrop',
            dropGroup: 'refdataGroup'
        }
    },
    initComponent: function () {
        this.ajaxProxy = Ext.create('Ext.data.proxy.Ajax', {
            timeout: 120000,
            actionMethods: { read: 'POST' },
            url: 'mapping/getnode',
            extraParams: {
                type: 'MappingNode',
                id: null,
                range: null,
                graph: this.record.data.id
            },
            reader: { type: 'json' }
        });
        this.store = Ext.create('Ext.data.TreeStore', {
            model: 'AM.model.MappingModel',
            clearOnLoad: true,
            root: {
                type: 'MappingNode',
                id: this.scope.Name + "/" + this.application.Name,
                expanded: true
            },
            proxy: this.ajaxProxy
        });

        this.tbar = new Ext.toolbar.Toolbar();
        this.tbar.add(this.buildToolbar());

        this.templatemapMenu = new Ext.menu.Menu();
        this.templatemapMenu.add(this.buildTemplateMapMenu());

        this.rolemapMenu = new Ext.menu.Menu();
        this.rolemapMenu.add(this.buildRoleMapMenu());

        this.classmapMenu = new Ext.menu.Menu();
        this.classmapMenu.add(this.buildClassMapMenu());

        this.on('itemcontextmenu', this.showContextMenu, this);
        this.on('itemclick', this.onClick, this);

        this.callParent(arguments);

        this.store.on("beforeload", function (store, operation, eopts) {
            this.ajaxProxy.extraParams.type = operation.node.data.type;
            if (operation.node.data != undefined) {
                this.ajaxProxy.extraParams.id = operation.node.data.identifier;
            }
        }, this);
        this.on('expand', this.onExpandNode, this);
        this.getView().on('beforedrop', this.onBeforeNodedrop, this);
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
          icon: 'Content/img/16x16/document-save.png',
          scope: this,
          action: 'savegraph'
      }
    ]
    },

    buildTemplateMapMenu: function () {
        return [
          {
              text: 'Delete TemplateMap',
              icon: 'Content/img/16x16/edit-delete.png',
              scope: this,
              action: 'deletetemplatemap'
          }
      ]
    },

    buildRoleMapMenu: function () {
        return [
          {
              xtype: 'button',
              text: 'Add ClassMap',
              icon: 'Content/img/16x16/document-new.png',
              scope: this,
              action: 'addclassmap'
          },
          {
              xtype: 'button',
              text: 'Make Possessor',
              // icon: 'Content/img/16x16/view-refresh.png',
              scope: this,
              action: 'makepossessor'
          },
          {
              xtype: 'button',
              text: 'Map Property',
              // icon: '',
              scope: this,
              action: 'mapproperty'
          },
          {
              xtype: 'button',
              text: 'Map ValueList',
              // icon: '',
              scope: this,
              action: 'mapvaluelist'
          },
          {
              xtype: 'button',
              text: 'Reset Mapping',
              //icon: '',
              scope: this,
              action: 'resetmapping'
          }
      ]
    },

    buildClassMapMenu: function () {
        return [
          {
              xtype: 'button',
              text: 'Delete ClassMap',
              icon: 'Content/img/16x16/edit-delete.png',
              scope: this,
              action: 'deleteclassmap'
          },
          {
              xtype: 'button',
              text: 'Change ClassMap',
              // icon:'',
              scope: this,
              action: 'changeclassmap'
          }
      ]
    },

    getSelectedNode: function () {
        return this.getSelectionModel().selected.items[0];
    },

    onReload: function (node) {
        var state = this.getState();
        this.body.mask('Loading', 'x-mask-loading');
        this.getStore().load();
        this.body.unmask();
        this.applyState(state, true);
    },

    //    onSelect: function (selModel, model, idx) {
    //        selectedMappingNode = model.store.getAt(idx);
    //    },

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
    },

    onBeforeNodedrop: function (domel, source, target, dropPosition) {
        this.getParentClass(target);
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
            var me = this;
            this.getEl().mask('Loading...');
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
                    me.onReload();
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
            var me = this;
            this.getEl().mask('Loading...');
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
                   // me.getEl().unmask();
                    me.onReload();
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

    }
});