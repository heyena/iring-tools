/*
* File: Scripts/AM/view/directory/DirectoryTree.js
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

Ext.define('AM.view.directory.DirectoryTree', {
    extend: 'Ext.tree.Panel',
    alias: 'widget.directorytree',

    stateId: 'directory-treestate',
    id: 'mytree',
    stateful: false,
    bodyStyle: 'background:#fff;padding:4px',
    border: false,
    store: 'DirectoryTreeStore',


    //rootVisible: false,
    initComponent: function () {
        var me = this;
        Ext.applyIf(me, {
            stateEvents: [
                'temcollapse',
                'itemexpand'
            ],
            viewConfig: {
                plugins: [
                    Ext.create('Ext.tree.plugin.TreeViewDragDrop', {
                       // ptype: 'treeviewdragdrop',
                        dragField: 'text',
                        ddGroup: 'propertyGroup',
                        dragGroup: 'propertyGroup',
                        appendOnly: true
                        //                      dragText: '{0}',
                        //                      enableDrop: true,
                        //                      enableDrag: true

                    })
                ],
                listeners: {
                    beforedrop: {
                        fn: me.onBeforeNodeDrop,
                        scope: me

                    }
                }
            },
            listeners: {
                itemclick: {
                    fn: me.onClick,
                    scope: me
                }
            }
        });

        me.callParent(arguments);
    },

    onBeforeNodeDrop: function (node, data, overModel, dropPosition, dropHandler, eOpts) {
        var me = this;
        var destinationNode = overModel;

        if (overModel.raw.type == 'WorldNode') {
            showDialog(400, 50, 'Alert', "Node can't be dropped here", Ext.Msg.OK, null);
            return;
        }

        if (overModel.raw.type == data.records[0].parentNode.raw.type || data.records[0].parentNode.raw.type == "SiteNode") {
            var parentEntityId = overModel.internalId;
            var entityId = data.records[0].raw.id;
            var displayName = overModel.raw.text;
            var nodeType = data.records[0].raw.type; //overModel.raw.type;
            var record;
            if (overModel.raw.type == 'ContextNode')
                record = Ext.decode(overModel.parentNode.raw.record);
            else
                record = Ext.decode(overModel.raw.record);

            var siteId = record.siteId;
            var platformId = record.platformId;
            Ext.Ajax.request({
                url: 'directory/DragAndDropEntity',
                method: 'POST',
                params: {
                    parentEntityId: parentEntityId,
                    displayName: displayName,
                    entityId: entityId,
                    nodeType: nodeType,
                    siteId: siteId,
                    platformId: platformId
                },
                success: function (response, request) {
                    var objResponseText = Ext.JSON.decode(response.responseText);
                    me.setLoading(false);

                    if (objResponseText["message"] == "nodeCopied") {
                        showDialog(400, 50, 'Alert', "Node copied successfully!", Ext.Msg.OK, null);


                    } else if (objResponseText["message"] == "destinationFolderDeleted") {
                        showDialog(400, 50, 'Alert', "Destination folder deleted, please refresh", Ext.Msg.OK, null);

                    } else if (objResponseText["message"] == "sourceFolderDeleted") {
                        showDialog(400, 50, 'Alert', "Source Folder Deleted, please refresh", Ext.Msg.OK, null);

                    } else if (objResponseText["message"] == "duplicateFolder") {
                        showDialog(400, 50, 'Alert', "Folder already exists", Ext.Msg.OK, null);

                    } else if (objResponseText["message"] == "sourceContextDeleted") {
                        showDialog(400, 50, 'Alert', "Source Context Deleted, please refresh", Ext.Msg.OK, null);

                    } else if (objResponseText["message"] == "duplicateContext") {
                        showDialog(400, 50, 'Alert', "Context already exists", Ext.Msg.OK, null);

                    } else if (objResponseText["message"] == "destinationContextDeleted") {
                        showDialog(400, 50, 'Alert', "destination Context Deleted, please refresh", Ext.Msg.OK, null);

                    } else if (objResponseText["message"] == "sourceApplicationDeleted") {
                        showDialog(400, 50, 'Alert', "source Application Deleted, please refresh", Ext.Msg.OK, null);

                    } else if (objResponseText["message"] == "duplicateApplication") {
                        showDialog(400, 50, 'Alert', "Application already exists", Ext.Msg.OK, null);
                    } else {
                        showDialog(400, 50, 'Alert', objResponseText["message"], Ext.Msg.OK, null);
                    }
                    // me.view.refresh();
                    //  this.getDirTree().onReload();
                    //                    me.store.reload();
                    destinationNode.store.reload();

                },
                failure: function (result, request) {
                    Ext.widget('messagepanel', { title: 'Error', msg: 'Error getting  Drag and Drop Object ' });
                    me.setLoading(false);
                }
            });

        } else {
            showDialog(400, 50, 'Alert', "Node can't be dropped here", Ext.Msg.OK, null);
        }

        return false;

    },

    onClick: function (dataview, record, item, index, e, eOpts) {
        var me = this;
        try {
            var pan = dataview.up('panel').up('panel');
            prop = pan.down('propertygrid');
            prop.setSource(record.data.property);
        } catch (e) {
        }
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

    onReload: function (options) {
        var me = this;
        var state = me.getState();

        me.on('beforeload', function (store, action) {
            me.getStore().getProxy().extraParams.type = 'ScopesNode';
        });

        var storeProxy = me.store.getProxy();
        storeProxy.on('exception', function (proxy, response, operation) {
            var resp = Ext.JSON.decode(response.responseText);
            var userMsg = resp['message'];
            var detailMsg = resp['stackTraceDescription'];
            var expPanel = Ext.widget('exceptionpanel', { title: 'Error Notification' });
            Ext.ComponentQuery.query('#expValue', expPanel)[0].setValue(userMsg);
            Ext.ComponentQuery.query('#expValue2', expPanel)[0].setValue(detailMsg);
        }, me);

        me.getEl().mask("Loading", 'x-mask-loading');

        me.store.load({
            node: me.getRootNode(),
            callback: function (records, options, success) {
                var nodes = state.expandedNodes || [];
                Ext.each(nodes, function (path) {
                    me.expandPath(path, 'text');
                });
                me.getEl().unmask();
            }
        });
    },

    getSelectedNode: function () {
        var me = this;
        var selected = me.getSelectionModel().getSelection();
        return selected[0];
    }

});