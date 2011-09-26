Ext.define('AM.controller.DirectoryController', {
    extend: 'Ext.app.Controller',
    views: [
        'directory.DirectoryPanel',
        'directory.DirectoryTree',
        'directory.ScopePanel',
        'directory.ApplicationPanel',
        'directory.DataGridPanel',
        'directory.GraphPanel',
        'directory.ValuelistPanel',
        'directory.ValuelistMapPanel',
        'common.PropertyPanel',
        'common.ContentPanel',
        'common.CenterPanel',
        'search.SearchPanel',
        'search.SearchTree',
        'mapping.MappingPanel',
        'mapping.MappingTree',
        'mapping.MapProperty',
        'mapping.ClassmapForm'
    ],
    stores: [
        'DirectoryStore'
        , 'SearchStore'
    ],
    models: [
        'DirectoryModel'
        , 'DataLayerModel'
        , 'DynamicModel'
        , 'SearchModel'
        , 'MappingModel'
    ],
    refs: [
        {
            ref: 'dirTree',
            selector: 'viewport > directorypanel > directorytree'
        },
        {
            ref: 'dirProperties',
            selector: 'viewport > directorypanel > propertypanel'
        },
        {
            ref: 'mainContent',
            selector: 'viewport > centerpanel > contentpanel'
        },
        {
            ref: 'mappingPanel',
            selector: 'mappingtree'
        },
        {
            ref: 'searchPanel',
            selector: 'viewport > centerpanel > searchpanel'
        },
        {
            ref: 'searchProperty',
            selector: 'viewport > centerpanel > searchpanel > propertypanel'
        },
        {
            ref: 'searchContent',
            selector: 'viewport > centerpanel > searchpanel > contentpanel'
        }
    ],
    init: function () {
        this.control({
            'menu button[action=newscope]': {
                click: this.newScope
            },
            'menu button[action=editscope]': {
                click: this.editScope
            },
            'menu button[action=deletescope]': {
                click: this.deleteScope
            },
            'menu button[action=newapp]': {
                click: this.newApplication
            },
            'menu button[action=editapp]': {
                click: this.editApplication
            },
            'menu button[action=deleteapp]': {
                click: this.deleteApplication
            },
            'menu button[action=showdata]': {
                click: this.showDataGrid
            },
            'menu button[action=newgraph]': {
                click: this.onNewGraph
            },
            'menu button[action=editgraph]': {
                click: this.onEditGraph
            },
            'menu button[action=deletegraph]': {
                click: this.onDeleteGraph
            },
            'menu button[action=newvaluelist]': {
                click: this.onNewValueList
            },
            'menu button[action=editvaluelist]': {
                click: this.onEditValueList
            },
            'menu button[action=deletevaluelist]': {
                click: this.onDeleteValueList
            },
            'menu button[action=newvaluemap]': {
                click: this.onNewValueMap
            },
            'menu button[action=editvaluemap]': {
                click: this.onEditValueMap
            },
            'menu button[action=deletevaluemap]': {
                click: this.onDeleteValueMap
            },
            'menu button[action=opengraph]': {
                click: this.openGraphMap
            },
            'menu button[action=addclassmap]': {
                click: this.addClassMap
            },
            'menu button[action=mapproperty]': {
                click: this.mapProperty
            },
            'button[action=search]': {
                click: this.onSearchRdl
            },
            'directorypanel directorytree': {
                beforeload: this.beforeLoad
            }
        });
    },

    addClassMap: function (btn, e) {
        var tree = this.getMappingPanel(),
            node = tree.getSelectedNode();

        tree.rolemapMenu.hide();
    },

    mapProperty: function (btn, e) {
        var tree = this.getMappingPanel(),
        node = tree.getSelectedNode(),
        scope = node.data.id.split('/')[0],
        application = node.data.id.split('/')[1],
        conf = {
            scope: scope,
            application: application,
            classId: node.parentNode.parentNode.data.identifier,
            mappingNode: node.data.id
        },
        win = Ext.widget('propertyform', conf);

        win.on('save', function (panel) {
            win.close();
            tree.onReload(node);
            if (node.get('expanded') == false)
                node.expand();
        }, this);

        win.on('Cancel', function (panel) {
            win.close();
        }, this);
        win.show();
        tree.rolemapMenu.hide();
    },

    openGraphMap: function (btn, e) {
        var tree = this.getDirTree(),
            node = tree.getSelectedNode(),
            content = this.getMainContent(),
            scope = node.parentNode.parentNode.parentNode,
            application = node.parentNode.parentNode;

        var conf = {
            scope: scope.data.record,
            application: application.data.record,
            record: node
        },
            maptree = Ext.widget('mappingtree', conf);
        mapprop = Ext.widget('propertypanel', { title: 'Mapping Details', region: 'east', width: 350, height: 150, split: true, collapsible: true })
        var panconf = {
            title: 'GraphMap - ' + scope.data.text + "." + application.data.text + '.' + node.data.text,
            id: 'GraphMap - ' + scope.data.text + "-" + application.data.text + '.' + node.data.text,
            height: 300,
            minSize: 150,
            layout: {
                type: 'border',
                padding: 2
            },
            split: true,
            closable: true,
            iconCls: 'tabsMapping',
            items: []
        },
        mappanel = Ext.widget('panel', panconf);
        mappanel.items.add(maptree);
        mappanel.items.add(mapprop);
        maptree.on('beforeitemexpand', function (node, eOpts) {
            content.getEl().mask('Loading...');
        }, this);
        maptree.on('load', function (store, model, a) {
            content.getEl().unmask();
        }, this);
        maptree.on('itemexpand', function (node, eOpts) {
            content.getEl().unmask();
        }, this);
        maptree.on('itemclick', function (view, model, node, index, e) {
            var obj = model.store.getAt(index).data;
            if (obj.property != null && obj.property != "") {
                mapprop.setSource(obj.property);
            } else {
                mapprop.setSource(obj.record);
            }
        });
        var exist = content.items.map[panconf.id];
        if (exist == null) {
            content.add(mappanel).show();
        } else {
            exist.show();
        }
        tree.graphMenu.hide();
    },


    onDeleteValueMap: function (btn, e) {
        var tree = this.getDirTree(),
            node = tree.getSelectedNode();
        Ext.Ajax.request({
            url: 'mapping/deleteValueMap',
            method: 'POST',
            params: {
                mappingNode: node.data.id,
                oldClassUrl: node.data.record.uri
            },
            success: function (result, request) {
                tree.onReload();
            },
            failure: function (result, request) { }
        })
    },
    onEditValueMap: function (btn, e) {
        var tree = this.getDirTree(),
            node = tree.getSelectedNode(),
            conf = {
                id: 'tab-' + node.data.id,
                record: node.data.record,
                node: node,
                title: 'Edit Value List \"' + node.data.text + '\"'
            };
        var win = Ext.widget('valuelistmappanel', conf);
        win.on('save', function (panel) {
            win.close();
            tree.onReload(node);
            if (node.get('expanded') == false)
                node.expand();
            //            showDialog(400, 100, 'Info', 'The new ValueListMap is added.', Ext.Msg.OK, null);
        }, this);

        win.on('Cancel', function (panel) {
            win.close();
        }, this);
        win.show();
        tree.valueListMenu.hide();
    },

    onNewValueMap: function (btn, e) {
        var tree = this.getDirTree(),
            node = tree.getSelectedNode(),
            conf = {
                id: 'tab-' + node.data.id,
                record: node.data.record,
                node: node,
                title: 'Add new ValueListMap to valueList'
            };
        var win = Ext.widget('valuelistmappanel', conf);
        win.on('save', function (panel) {
            win.close();
            tree.onReload(node);
            if (node.get('expanded') == false)
                node.expand();
            // showDialog(400, 100, 'Info', 'The new ValueListMap is added.', Ext.Msg.OK, null);
        }, this);

        win.on('Cancel', function (panel) {
            win.close();
        }, this);
        win.show();
        tree.valueListMenu.hide();
    },

    onEditValueList: function (btn, e) {
        var tree = this.getDirTree(),
            node = tree.getSelectedNode(),
            conf = {
                id: 'tab-' + node.data.id,
                record: node.data.record,
                nodeId: node.data.id,
                title: 'Edit Value List \"' + node.data.text + '\"'
            };
        var win = Ext.widget('valuelistpanel', conf);
        win.on('save', function (panel) {
            win.close();
            tree.onReload(node);
            if (node.get('expanded') == false)
                node.expand();
        }, this);

        win.on('Cancel', function (panel) {
            win.close();
        }, this);
        win.show();
        tree.valueListMenu.hide();
    },

    onDeleteValueList: function (btn, e) {
        var tree = this.getDirTree(),
            node = tree.getSelectedNode();
        Ext.Ajax.request({
            url: 'mapping/deletevaluelist',
            method: 'POST',
            params: {
                mappingNode: node.data.id,
                valueList: node.data.id.split('/')[4]
            },
            success: function (result, request) {
                tree.onReload(node);
            },
            failure: function (result, request) { }
        })
        tree.valueListMenu.hide();
    },
    onNewValueList: function (btn, e) {
        var tree = this.getDirTree(),
            node = tree.getSelectedNode(),
            conf = {
                id: 'tab-' + node.data.id,
                record: node.data.record,
                nodeId: node.data.id,
                title: 'Add Value List Name'
            };
        var win = Ext.widget('valuelistpanel', conf);
        win.on('save', function (panel) {
            win.close();
            tree.onReload();
            if (node.get('expanded') == false)
                node.expand();
        }, this);

        win.on('Cancel', function (panel) {
            win.close();
        }, this);
        win.show();
        tree.valueListsMenu.hide();
    },

    onEditGraph: function (btn, e) {
        var tree = this.getDirTree(),
            node = tree.getSelectedNode(),
            conf = {
                id: 'tab-' + node.data.id,
                record: node.data.record,
                node: node,
                title: 'Edit Graph Map \"' + node.data.text + '\"',
                iconCls: 'tabsGraph',
                height: 200,
                width: 430
            };
        var win = Ext.widget('graphmapform', conf);
        win.on('save', function (panel) {
            win.close();
            tree.onReload(node);
            if (node.get('expanded') == false)
                node.expand();
        }, this);

        win.on('Cancel', function (panel) {
            win.close();
        }, this);
        win.show();
        tree.graphMenu.hide();
    },

    onDeleteGraph: function (btn, e) {
        var tree = this.getDirTree(),
         node = tree.getSelectedNode();
        Ext.Ajax.request({
            url: 'mapping/deletegraphmap',
            method: 'POST',
            params: {
                scope: node.data.id.split('/')[0],
                application: node.data.id.split('/')[1],
                mappingNode: node.data.id
            },
            success: function (result, request) {
                tree.onReload();
            },
            failure: function (result, request) { }
        })
        tree.graphMenu.hide();
    },

    onSearchRdl: function (btn, e) {
        var pan = this.getSearchPanel(),
           stb = pan.dockedItems.items[1].items.items[0]
        searchText = pan.dockedItems.items[1].items.items[0].getValue(),
           isreset = pan.dockedItems.items[1].items.items[2].checked,
           content = this.getSearchContent(),
           propPanel = this.getSearchProperty(),
           value = pan.dockedItems.items[1].items.items[5].value;
        if (!searchText && searchText.length == 0) return;
        content.getEl().mask('Loading...');
        conf = {
            title: searchText,
            id: 'tab_' + searchText
        };

        var tree = Ext.widget('searchtree', conf);
        tree.on('itemclick', this.onSearchClick, this);
        tree.on('beforeload', function (store, action, a) {
            store.proxy.extraParams.type = (action.node.data.type == "" ? 'SearchNode' : action.node.data.type);
            if (searchText != undefined && searchText != '') {
                store.proxy.extraParams.query = searchText;
                store.proxy.extraParams.reset = isreset;
                store.proxy.extraParams.limit = value;
            }
            if (action.node.parentNode && (action.node.data.identifier == null || action.node.data.identifier == '')) {
                store.proxy.extraParams.id = action.node.parentNode.data.identifier;
            } else {
                store.proxy.extraParams.id = action.node.data.identifier;
            }
        }, this);
        tree.on('beforeitemexpand', function (node, eOpts) {
            content.getEl().mask('Loading...');
        }, this);
        tree.on('load', function (store, model, a) {
            content.getEl().unmask();
            if (model.data.type == "ClassNode") {
                try {
                    if (model.childNodes.length > 0) {
                        propPanel.setSource(model.childNodes[0].data.record);
                    }
                    else {
                        propPanel.setSource(model.data.record);
                    }
                } catch (e) { }
            }
        }, this);

        var exist = content.items.map[conf.id];
        if (exist == null) {
            content.add(tree).show();
        } else {
            exist.show();
        }
        tree.getStore().load();

    },

    onSearchClick: function (view, model, n, idx, e) {
        propPanel = this.getSearchProperty();
        try {
            var node = model.store.getAt(idx);
            if (node.data.type == "ClassNode" && model.firstChild) {
                propPanel.setSource(model.firstChild.data.record);
            } else {
                propPanel.setSource(node.data.record);
            }
        } catch (e) { }
        node.expand();
    },

    onNewGraph: function (btn, e) {
        var tree = this.getDirTree(),
            node = tree.getSelectedNode(),
            conf = {
                id: 'tab-' + node.data.id,
                record: node.data.record,
                node: node,
                title: 'Add new GraphMap to Mapping',
                iconCls: 'tabsGraph',
                height: 200,
                width: 430
            };
        var win = Ext.widget('graphmapform', conf);
        win.on('save', function (panel) {
            win.close();
            tree.onReload();
            if (node.get('expanded') == false)
                node.expand();
        }, this);

        win.on('Cancel', function (panel) {
            win.close();
        }, this);
        win.show();
        tree.graphsMenu.hide();
    },

    showDataGrid: function (btn, e) {
        var tree = this.getDirTree(),
           content = this.getMainContent(),
           node = tree.getSelectedNode(),
           scope = node.parentNode.parentNode.parentNode.data.text,
           app = node.parentNode.parentNode.data.text,
           graph = node.data.text;
        conf = {
            title: scope + '.' + app + '.' + graph,
            id: scope + app + graph + Ext.id(),
            scope: scope,
            start: 0,
            limit: 25,
            app: app,
            graph: graph
        };
        var exist = content.items.map[conf.id];
        if (exist) {
            exist.show();
            tree.appDataMenu.hide();
            return true;
        }
        var newtab = Ext.widget('datagridpanel', conf);
        content.add(newtab).show();
        tree.appDataMenu.hide();
    },

    newApplication: function (btn, e) {
        var tree = this.getDirTree(),
           node = tree.getSelectedNode(),
           conf = {
               id: 'newwin-' + node.data.id,
               scope: node.data.record,
               title: 'Add New Application',
               iconCls: 'tabsApplication'
           };
        var win = Ext.widget('applicationform', conf);

        win.on('save', function (panel) {
            win.close();
            tree.onReload();
            if (node.get('expanded') == false)
                node.expand();
        }, this);

        win.on('Cancel', function (panel) {
            win.close();
        }, this);
        win.show();
    },

    editApplication: function (btn, e) {
        var tree = this.getDirTree(),
            node = tree.getSelectedNode(),
            conf = {
                id: 'tab-' + node.data.id,
                scope: node.parentNode.data.record,
                record: node.data.record,
                title: 'Edit Application \"' + node.data.text + '\"',
                iconCls: 'tabsApplication'
            };
        var win = Ext.widget('applicationform', conf);

        win.on('save', function (panel) {
            win.close();
            tree.onReload();
            if (node.get('expanded') == false)
                node.expand();
        }, this);

        win.on('Cancel', function (panel) {
            win.close();
        }, this);
        win.show();
    },

    deleteApplication: function (btn, e) {
        var tree = this.getDirTree(),
           node = tree.getSelectedNode();
        Ext.Ajax.request({
            url: 'directory/deleteapplication',
            method: 'POST',
            params: {
                'nodeid': node.data.id
            },
            success: function (o) {
                tree.onReload();
                if (node.parentNode.expanded == false)
                    node.parentNode.expand();
            },
            failure: function (f, a) {
                //Ext.Msg.alert('Warning', 'Error!!!');
                var message = 'Error deleting application!';
                showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
            }
        });
        tree.applicationMenu.hide();
    },

    newScope: function (btn, ev) {
        var tree = this.getDirTree(),
           node = tree.getSelectedNode(),
           conf = {
               id: 'tab-' + node.id,
               record: node.data.record,
               title: 'Add New Scope',
               iconCls: 'tabsScope',
               url: 'directory/scope'
           };
        var win = Ext.widget('scopeform', conf);

        win.on('save', function (panel) {
            win.close();
            tree.onReload();
            if (node.get('expanded') == false)
                node.expand();
        }, this);

        win.on('Cancel', function (panel) {
            win.close();
        }, this);
        win.show();

        win.items.first().getForm().findField('Name').clearInvalid();

    },

    editScope: function (btn, e) {
        var tree = this.getDirTree(),
           node = tree.getSelectedNode(),
           conf = {
               id: 'tab-' + node.id,
               record: node.data.record,
               title: 'Edit Scope \"' + node.data.text + '\"',
               iconCls: 'tabsScope',
               url: 'directory/scope'
           };
        var win = Ext.widget('scopeform', conf);

        win.on('save', function (panel) {
            win.close();
            tree.onReload();
            if (node.get('expanded') == false)
                node.expand();
        }, this);

        win.on('Cancel', function (panel) {
            win.close();
        }, this);
        win.show();
    },

    deleteScope: function (btn, e) {
        var tree = this.getDirTree(),
           node = tree.getSelectedNode();
        Ext.Ajax.request({
            url: 'directory/deletescope',
            method: 'POST',
            params: {
                'nodeid': node.data.id
            },
            success: function (o) {
                tree.onReload(node);
            },
            failure: function (f, a) {
                var message = 'Error deleting scope!';
                showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
            }
        });
    },

    beforeLoad: function (store, rec) {
        store.proxy.extraParams.type = rec.node.data.type;
        if (rec.node.data.record != undefined && rec.node.data.record.Related != undefined) {
            store.proxy.extraParams.related = rec.node.data.record.Related;
        }
    }
});