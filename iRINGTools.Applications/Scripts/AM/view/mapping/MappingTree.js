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
    //  pathSeparator: '>',
    lines: true,
    region: 'center',
    enableDD: true,
    stateful: true,
    contextName: null,
    node: null,
    endpoint: null,
    baseUrl: null,
    record: null,
    rootId: null,
    parentClass: null,
    mappingMenu: null,
    graphName: null,
    graphmapMenu: null,
    templatemapMenu: null,
    rolemapMenu: null,
    classmapMenu: null,
    parentClass: null,
    ajaxProxy: null,
    scroll: 'both',
    viewConfig: {
        plugins: {
            ptype: 'treeviewdragdrop',
            dropGroup: 'refdataGroup'
        }
    },
    initComponent: function () {
        Ext.apply(this, {
            store: Ext.create('Ext.data.TreeStore', {
                model: 'AM.model.MappingModel',
                clearOnLoad: false,
                storeId: this.id,
                root: {
                    type: 'MappingNode',
                    id: this.contextName + "/" + this.endpoint,
                    expanded: true
                },
                proxy: {
                    type: 'ajax',
                    timeout: 600000,
                    actionMethods: { read: 'POST' },
                    url: 'mapping/getnode',
                    extraParams: {
                        type: 'MappingNode',
                        id: null,
                        range: null,
                        graphName: this.graphName,
                        contextName: this.contextName,
                        endpoint: this.endpoint,
                        baseUrl: this.baseUrl
                    },
                    reader: { type: 'json' }
                }
            }),
            viewConfig: {
                plugins: {
                    ptype: 'treeviewdragdrop',
                    dropGroup: 'refdataGroup',
                    enableDrag: false
                },
                stateful: true,
                stateId: this.id + '-state'//,
                //stateEvents: ['itemcollapse', 'itemexpand']
            }
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

        this.callParent(arguments);

        this.on("beforeload", function (store, operation, eopts) {
            store.proxy.extraParams.type = operation.node.data.type;
            if (store.proxy.extraParams != undefined) {
                store.proxy.extraParams.id = operation.node.data.id;
            }
        }, this);

        this.getView().on('beforedrop', this.onBeforeNodedrop, this);
    },

    getState: function () {
        var nodes = [], state = this.callParent();
        this.getRootNode().eachChild(function (child) {
            // function to store state of tree recursively 
            var storeTreeState = function (node, expandedNodes) {
                if (node.isExpanded() && node.childNodes.length > 0) {
                    expandedNodes.push(node.getPath('id'));
                    node.eachChild(function (child) {
                        storeTreeState(child, expandedNodes);
                    });
                }
            };
            storeTreeState(child, nodes);
        });
        Ext.apply(state, {
            expandedNodes: nodes
        });
        return state;
    },

    applyState: function (state) {
        var me = this;
        var nodes = state.expandedNodes || [],
            len = nodes.length;
        //  this.collapseAll();
        Ext.each(nodes, function (path) {
            me.expandPath(path, 'id');
        });
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
              icon: 'Content/img/16x16/edit-delete.png',
              scope: this,
              action: 'templatemapdelete'
          }
      ]
    },

    buildRoleMapMenu: function () {
        return [
          {
              text: 'Add ClassMap',
              icon: 'Content/img/16x16/document-new.png',
              scope: this,
              action: 'addclassmap'
          }, {
              text: 'Make Possessor',
              // icon: 'Content/img/16x16/view-refresh.png',
              scope: this,
              action: 'makepossessor'
          }, {
            text: 'Make Reference',            
            // icon: '',
            scope: this,
            action: 'makereference'
          }, {
              text: 'Map Property',
              // icon: '',
              scope: this,
              action: 'mapproperty'
          }, {
              text: 'Map ValueList',
              // icon: '',
              scope: this,
              action: 'mapvaluelist'
          }, {
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
              text: 'Delete ClassMap',
              icon: 'Content/img/16x16/edit-delete.png',
              scope: this,
              action: 'deleteclassmap'
          },
          {
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
        this.store().load();
        this.body.unmask();
        this.applyState(state, true);
    },

    onSave: function (c) {
        var me = this;
        Ext.Ajax.request({
            url: 'mapping/updateMapping',
            method: 'POST',
            params: {
                contextName: me.contextName,
                endpoint: me.endpoint,
                baseUrl: me.baseUrl
            },
            success: function (result, request) {
                me.onReload();
            },
            failure: function (result, request) {
                return false;
            }
        })
    },

    onReloadNode: function (node) {
        node.reload();
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

    onReload: function (node) {
        //get state from tree
        var me = this;
        var state = this.getState();
        this.body.mask('Loading', 'x-mask-loading');

        this.getStore().load(node);
        this.body.unmask();
        this.applyState(state, true);
    },

    onBeforeNodedrop: function (domel, source, target, dropPosition) {
        this.getParentClass(target);
        var nodetype, thistype, icn, txt, templateId, rec, parentId, context;
        var graphName = this.graphName;
        if (target.data.type == 'RoleMapNode') {
            reference = source.records[0].data.record.Uri;
            label = source.records[0].data.record.Label;
            roleId = target.data.record.id;
            roleName = target.data.record.name;
            rec = source.records[0].data.record;
            txt = source.records[0].data.record.Label;
            parentId = this.parentClass;
            f = false;
            var me = this;
            var index = target.parentNode.parentNode.indexOf(target.parentNode);
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
                    contextName: me.contextName,
                    endpoint: me.endpoint,
                    index: index,
                    graphName: graphName,
                    baseUrl: me.baseUrl
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
            //context = target.data.id + '/' + txt;
            lf = false;
            var me = this;
            this.getEl().mask('Loading...');
            Ext.Ajax.request({
                url: 'mapping/addtemplatemap',
                method: 'POST',
                params: {
                    contextName: me.contextName,
                    endpoint: me.endpoint,
                    baseUrl: me.baseUrl,
                    nodetype: thistype,
                    parentType: ntype,
                    parentId: parentid,
                    id: templateId,
                    graphName: graphName
                },
                success: function (result, request) {
                    me.getEl().unmask();
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
        return false;

    }
});