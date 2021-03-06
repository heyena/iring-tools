/*
* File: Scripts/AM/view/mapping/MappingTree.js
*
* This file was generated by Sencha Architect version 2.2.2.
* http://www.sencha.com/products/architect/
*
* This file requires use of the Ext JS 4.1.x library, under independent license.
* License of Sencha Architect does not include license for Ext JS 4.1.x. For more
* details see http://www.sencha.com/license or contact license@sencha.com.
*
* This file will be auto-generated each and everytime you save your project.
*
* Do NOT hand edit this file.
*/

Ext.define('AM.view.mapping.MappingTree', {
    extend: 'Ext.tree.Panel',
    alias: 'widget.mappingtree',

    requires: [
        'AM.view.override.mapping.MappingTree'
    ],

    stateId: 'mapping-treestate',
    stateful: false,
    border: true,
    store: 'MappingStore',
    rootVisible: false,
    selType: 'rowmodel',

    initComponent: function () {
        var me = this;

        Ext.applyIf(me, {
            stateEvents: [
                'itemcollapse',
                'itemexpand'
            ],
            viewConfig: {
                plugins: [
                    Ext.create('Ext.tree.plugin.TreeViewDragDrop', {
                        //ddGroup: 'refdataGroup',
                        ddGroup: 'propertyGroup',
                        enableDrag: false
                    })
                ],
                listeners: {
                    beforedrop: {
                        fn: me.onBeforeNodeDrop,
                        scope: me
                    }
                }
            },
            dockedItems: [{
                xtype: 'toolbar',
                border: false,
                dock: 'top',
                items: [
                {
                    xtype: 'button',
                    handler: function (button, event) {
                        var tree = button.up('toolbar').up('panel');
                        var node = tree.getRootNode();
                        tree.onReload(node);
                    },
                    icon: 'Content/img/16x16/refresh.png',
                    text: 'Reload Tree'
                },
                {
                    xtype: 'button',
                    handler: function (button, event) {
                        me.onSave();
                    },
                    icon: 'Content/img/16x16/document-save.png',
                    text: 'Save'
                }
              ]
            }
          ],
            listeners: {
                itemcontextmenu: {
                    fn: me.showContextMenu,
                    scope: me
                },
                beforeload: {
                    fn: me.onBeforeLoad,
                    scope: me
                }
            },
            selModel: Ext.create('Ext.selection.RowModel', {

            })
        });

        me.callParent(arguments);
    },

    onBeforeNodeDrop: function (node, data, overModel, dropPosition, dropHandler, eOpts) {
        var me = this;
        var pan = me.up('mappingpanel');
        me.getParentClass(overModel);
        var nodetype, thistype, icn, txt, templateId, rec, parentId, context, classMapIndex;
        var tempArr = pan.graph.split('/'); //pan.graphName;
        var graphName = tempArr[tempArr.length - 1];
        var modelType = data.records[0].data.type;
        var selNode = me.up('viewport').down('directorytree').getSelectedNode();
        var node = overModel;
        if ((overModel.data.type == 'RoleMapNode' && data.records[0].data.type == 'DataPropertyNode') || (overModel.data.type == 'RoleMapNode' && data.records[0].data.type == 'KeyDataPropertyNode') || (overModel.data.type == 'RoleMapNode' && data.records[0].data.type == 'ExtensionNode')) {
            var dataObj = data.records[0].data.parentId.split('/');
            var propertyName = "";
            if (selNode.parentNode.data.type == "RelationshipNode") {
                propertyName = dataObj[dataObj.length - 2] + '.' + dataObj[dataObj.length - 1] + '.' + data.records[0].data.text;
            } else {
                propertyName = dataObj[dataObj.length - 1] + '.' + data.records[0].data.property.Name;
            }


            var scope = pan.contextName;
            var app = pan.endpoint;
            var classIndex = overModel.parentNode.parentNode.data.identifierIndex;
            var classId = overModel.parentNode.parentNode.data.identifier;
            var tempId = overModel.parentNode.data.id.split('/');
            var mappingNode = overModel.parentNode.data.parentId + '/' + tempId[tempId.length - 1] + '/' + overModel.data.record.name;
            var index = overModel.parentNode.parentNode.indexOf(overModel.parentNode);
            me.setLoading();
            Ext.Ajax.request({
                url: 'mapping/mapproperty',
                method: 'POST',
                params: {
                    propertyName: propertyName,
                    classIndex: classIndex,
                    graphName: graphName,
                    classId: classId,
                    index: index,
                    contextName: scope,
                    endpoint: app,
                    mappingNode: mappingNode
                },
                success: function (result, request) {
                    var res = Ext.JSON.decode(result.responseText);
                    if (res.success) {
                        var parentNode = node.parentNode;
                        var nodeIndex = parentNode.indexOf(node);
                        parentNode.removeChild(node);
                        parentNode.insertChild(nodeIndex, res.node);
                        //tree.view.refresh();
                        me.view.refresh();
                    }
                    else {
                        //Ext.widget('messagepanel', { title: 'asdasd', msg: res.message });
                        var userMsg = res.message;
                        var detailMsg = res.stackTraceDescription;
                        var expPanel = Ext.widget('exceptionpanel', { title: 'Error Notification' });
                        Ext.ComponentQuery.query('#expValue', expPanel)[0].setValue(userMsg);
                        Ext.ComponentQuery.query('#expValue2', expPanel)[0].setValue(detailMsg);
                    }
                    me.setLoading(false);
                },
                failure: function (result, request) {
                    Ext.widget('messagepanel', { title: 'Error', msg: 'Error in mapping property.' });
                    me.setLoading(false);
                }
            });
        } else if (overModel.data.type == 'RoleMapNode' && data.records[0].data.type == 'ClassNode') {
            reference = data.records[0].data.record.Uri;
            label = data.records[0].data.record.Label;
            roleId = overModel.data.record.id;
            roleName = overModel.data.record.name;
            rec = data.records[0].data.record;
            txt = data.records[0].data.record.Label;
            parentId = me.parentClass;
            f = false;
            var classIndex = me.parentClassIndex;
            var index = overModel.parentNode.parentNode.indexOf(overModel.parentNode);
            me.setLoading();
            Ext.Ajax.request({
                url: 'mapping/makereference', //'mapping/mapreference',
                method: 'POST',
                params: {
                    refClassId: reference,
                    classId: parentId,
                    refClassLabel: label,
                    roleId: roleId,
                    roleName: roleName,
                    scope: pan.contextName,
                    app: pan.endpoint,
                    templateIndex: index,
                    graph: graphName,
                    classIndex: classIndex
                },
                success: function (result, request) {
                    var res = Ext.JSON.decode(result.responseText);
                    if (res.success) {
                        var parentNode = node.parentNode;
                        var nodeIndex = parentNode.indexOf(node);
                        parentNode.removeChild(node);
                        parentNode.insertChild(nodeIndex, res.node);
                        me.view.refresh();
                    }
                    else {
                        Ext.widget('messagepanel', { title: 'Error', msg: res.message });
                    }
                    me.setLoading(false);
                },
                failure: function (result, request) {
                    Ext.widget('messagepanel', { title: 'Error', msg: 'Error in make referance' });
                    me.setLoading(false);
                }
            });
        }
        else if (modelType == 'TemplateNode') { //(data.records[0].data.type == 'TemplateNode') {
            ntype = overModel.data.type;
            parentid = overModel.data.identifier;
            classMapIndex = overModel.data.identifierIndex;
            thistype = data.records[0].data.type;
            icn = 'Content/img/template-map.png';
            txt = data.records[0].data.record.Label;
            templateId = data.records[0].data.identifier;
            rec = data.records[0].data.record;
            context = overModel.data.id;

            lf = false;

            me.setLoading();
            Ext.Ajax.request({
                url: 'mapping/addtemplatemap',
                timeout: 600000,
                method: 'POST',
                params: {
                    contextName: pan.contextName,
                    ctx: context,
                    endpoint: pan.endpoint,
                    baseUrl: pan.baseUrl,
                    nodetype: thistype,
                    parentType: ntype,
                    parentId: parentid,
                    classMapIndex: classMapIndex,
                    id: templateId,
                    graphName: graphName
                },
                success: function (result, request) {
                    var res = Ext.JSON.decode(result.responseText);
                    if (res.success) {
                        overModel.set('leaf', false);
                        overModel.appendChild(res.node)
                    }
                    else {
                        Ext.widget('messagepanel', { title: 'Error', msg: res.message });
                    }
                    me.setLoading(false);
                },
                failure: function (result, request) {
                    Ext.widget('messagepanel', { title: 'Error', msg: 'Error in add template.' });
                    me.setLoading(false);
                }
            });
        }
        return false;
    },

    showContextMenu: function (dataview, record, item, index, e, eOpts) {
        var me = this;
        var obj = record.data;

        if (obj.type == "TemplateMapNode") {
            var templatemapMenu = Ext.widget('templatemapmenu');
            templatemapMenu.showAt(e.getXY());
        } else if (obj.type == "RoleMapNode") {
            var rolemapMenu = Ext.widget('rolemapmenu');
            rolemapMenu.showAt(e.getXY());
        } else if (obj.type == "ClassMapNode") {
            var classmapMenu = Ext.widget('classmapmenu');
            classmapMenu.showAt(e.getXY());
        }
    },

    onBeforeLoad: function (store, operation, eOpts) {
        store.proxy.extraParams.type = operation.node.data.type;
        store.proxy.extraParams.index = operation.node.data.index;

        if (store.proxy.extraParams !== undefined) {
            store.proxy.extraParams.id = operation.node.data.id;

            if (operation.node.data.type == 'ClassMapNode')
                store.proxy.extraParams.index = operation.node.data.identifierIndex;
        }
    },

    applyState: function (state) {
        var me = this;
        var nodes = state.expandedNodes || [], len = nodes.length;
        me.collapseAll();
        Ext.each(nodes, function (path) {
            me.expandPath(path, 'text');
        });
        me.callParent(arguments);
    },

    getState: function () {
        var me = this;
        var nodes = [], state = me.callParent();
        me.getRootNode().eachChild(function (child) {
            // function to store state of tree recursively 
            var storeTreeState = function (node, expandedNodes) {
                if (node.isExpanded() && node.childNodes.length > 0) {
                    expandedNodes.push(node.getPath('text'));
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

    onReload: function () {
        var me = this;
        var mappingPanel = me.up('mappingpanel');
        var graphFullName = me.up('mappingpanel').graph; //me.up('mappingpanel').graphName;
        var graphNameArr = graphFullName.split('/');
        var graphName = graphNameArr[graphNameArr.length - 1];
        var path, graphNode, context, endpoint, id;
        var node = me.getSelectedNode();

        if (!node) {
            node = me.getRootNode();
        }

        var store = me.store;
        var params = store.getProxy().extraParams;
        var state = me.getState();
        if (node) {
            store.on('beforeload', function (store, operation, eopts) {
                params.graph = graphName;
            }, me);

            store.reload({
                callback: function (records, options, success) {
                    var nodes = state.expandedNodes || [];
                    var len = nodes.length;
                    if (len > 0)
                        me.collapseAll();
                    Ext.each(nodes, function (path) {
                        me.expandPath(path, 'text');
                    });
                }

            });
        }
    },

    onSave: function () {
        var me = this;
        var mapPanel = me.up('panel');
        Ext.Ajax.request({
            url: 'mapping/updateMapping',
            method: 'POST',
            params: {
                scope: mapPanel.contextName,
                application: mapPanel.endpoint,
                baseUrl: mapPanel.baseUrl
            },
            success: function (result, request) {
                var resp = Ext.decode(result.responseText);
                if (resp.success) {
                    Ext.example.msg('Notification', 'Configuration saved successfully!');
                    me.onReload();
                } else {
                    var userMsg = resp['message'];
                    var detailMsg = resp['stackTraceDescription'];
                    var expPanel = Ext.widget('exceptionpanel', { title: 'Error Notification' });
                    Ext.ComponentQuery.query('#expValue', expPanel)[0].setValue(userMsg);
                    Ext.ComponentQuery.query('#expValue2', expPanel)[0].setValue(detailMsg);
                }
            },
            failure: function (result, request) {
                return false;
            }
        });
    },

    getParentClass: function (n) {
        if (n != undefined) {
            if (n.parentNode !== null && n.parentNode !== undefined) {
                if ((n.parentNode.data.type == 'ClassMapNode' ||
                    n.parentNode.data.type == 'GraphMapNode') &&
                    n.parentNode.data.identifier !== undefined) {
                    this.parentClass = n.parentNode.data.identifier;
                    this.parentClassIndex = n.parentNode.data.identifierIndex;
                    return this.parentClass;
                }
                else {
                    this.getParentClass(n.parentNode);
                }
            }
        }
    },

    getSelectedNode: function () {
        var me = this;
        var selected = me.getSelectionModel().getSelection();
        return selected[0];
    }
});