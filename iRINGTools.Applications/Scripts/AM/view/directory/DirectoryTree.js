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
    stateful: false,
    bodyStyle: 'background:#fff;padding:4px',
    store: 'DirectoryTreeStore',

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
                      dragField: 'text',
                      ddGroup: 'propertyGroup',
                      dragGroup: 'propertyGroup',
                      dragText: '{0}',
                      enableDrop: false
                  })]
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

    onClick: function (dataview, record, item, index, e, eOpts) {
        var me = this;
        try {
            var pan = dataview.up('panel').up('panel');
            prop = pan.down('propertygrid');
            prop.setSource(record.data.property);
        } catch (e) { }
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

        me.getEl().mask("Loading", 'x-mask-loading');

        me.store.load({
            //node: me.getRootNode(),
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