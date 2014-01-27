Ext.define('AM.controller.Mapping', {
    extend: 'Ext.app.Controller',

    parentClass: '',
    models: [
    'MappingModel'
  ],
    stores: [
    'MappingStore'
  ],
    views: [
    'common.PropertyPanel',
    'common.ContentPanel',
    'common.CenterPanel',
    'mapping.MappingTree',
    'directory.GraphMapForm',
    'mapping.ClassMapForm',
    'mapping.ClassMapWindow',
    'mapping.PropertyMapWindow',
    'directory.GraphMapWindow',
    'mapping.ValueListMapWindow',
    'mapping.ValueListMapForm',
    'mapping.ValueListWindow',
    'mapping.MappingPanel',
    'mapping.ValueListForm',
    'mapping.MapValueListWindow',
    'mapping.MapValueListForm',
    'mapping.LiteralForm',
    'mapping.LiteralWindow'
  ],

    refs: [
    {
        ref: 'dirTree',
        selector: 'viewport > directorypanel > directorytree'
    },
    {
        ref: 'mainContent',
        selector: 'viewport > centerpanel > contentpanel'
    }
  ],

    onDeleteTemplateMap: function (item, e, eOpts) {
        var me = this;
        var content = me.getMainContent();
        var dirTree = me.getDirTree();
        var tree = content.getActiveTab().items.items[0];
        var endPoint = content.getActiveTab().endpoint;
        var contextName = content.getActiveTab().contextName;
        var tempGraph = content.getActiveTab().graph.split('/');
        var graph = tempGraph[tempGraph.length - 1];
        var itemId = 'GraphMap.' + contextName + '.' + endPoint + '.' + graph;
        node = tree.getSelectedNode();
        var rootNodeId = tree.getRootNode().childNodes[0].internalId;
        var nodeId = node.data.id.split('/');
        var text = nodeId[nodeId.length - 1];
        var graph = rootNodeId.split('/')[1];
        var panel = content.items.map[itemId]; //content.items.map['GraphMap-' + graph];
        me.getParentClass(node);
        tree.getEl().mask("Loading", 'x-mask-loading');
        Ext.Ajax.request({
            url: 'mapping/deleteTemplateMap',
            method: 'POST',
            params: {
                scope: panel.contextName,
                application: panel.endpoint,
                baseUrl: panel.baseUrl,
                parentIdentifier: me.parentClass,
                identifier: node.data.identifier,
                index: node.parentNode.indexOf(node),
                parentClassIndex: me.parentClassIndex,
                rootNodeId: rootNodeId
            },
            success: function (response, request) {
                var res = Ext.JSON.decode(response.responseText);
                if (res.success) {
                    var parentNode = node.parentNode;
                    parentNode.removeChild(node);
                    tree.view.refresh();
                }
                else {
                    Ext.widget('messagepanel', { title: 'Error', msg: res.message });
                }
                tree.getEl().unmask();
            },
            failure: function (response, request) {
                Ext.widget('messagepanel', { title: 'Error', msg: 'An error has occurred while deleting Template Map.' });
                tree.getEl().unmask();
            }
        });
    },

    onEditOrNewGraph: function (item, e, eOpts) {
        var me = this;
        var nodeId, contextName, endpoint, baseUrl, graphName,
		objectName, classLabel, classUrl, identifier, wintitle;
        var tree = me.getDirTree();
        var node = tree.getSelectedNode();
        var record = node.data.record;
        var contextName; // This is scope
        var endpoint; //This is application
        var delimeter;

        if (record)
            delimeter = record.classTemplateMaps[0].classMap.identifierDelimiter;
        else
            delimeter = '_';

        if (record) {
            identifier = record.classTemplateMaps[0].classMap.identifiers[0];
            if (record.classTemplateMaps[0].classMap.identifiers.length > 1) {
                for (var i = 1; i < record.classTemplateMaps[0].classMap.identifiers.length; i++) {
                    identifier = identifier + ',' + record.classTemplateMaps[0].classMap.identifiers[i];
                }
            }

            graphName = record.name;
            objectName = record.dataObjectName; //getLastXString(record.classTemplateMaps[0].classMap.identifiers[0], 1).split('.')[0];
            classLabel = record.classTemplateMaps[0].classMap.name;
            classUrl = record.classTemplateMaps[0].classMap.id;
            contextName = node.parentNode.parentNode.parentNode.data.property['Internal Name'];
            endpoint    = node.parentNode.parentNode.data.property['Internal Name'];
        }
        else {
            contextName = node.parentNode.parentNode.data.property['Internal Name'];
            endpoint = node.parentNode.data.property['Internal Name'];
        }

        if (item.itemId == 'editgraph') {
            wintitle = 'Edit Graph \"' + graphName + '\"';
        } else {
            wintitle = 'Add GraphMap';
        }

        var conf = {
            id: 'tab-' + node.data.id,
            title: wintitle,
            node: node
        };

        var win = Ext.widget('graphmapwindow', conf);

        var formRecord = {
            'scope': contextName,
            'app': endpoint,
            'oldGraphName': graphName,
            'graphName': graphName,
            'objectName': objectName,
            'classId': classUrl,
            'identifier': identifier,
            'className': classLabel,
            'delimiter': delimeter
        };

        var form = win.down('form').getForm();
        win.down('form').node = node;
        form.setValues(formRecord);

        win.down('form').updateDDContainers(record);

        win.on('save', function () {
            win.close();
            tree.view.refresh();
            //tree.store.load({ node: node });
        }, me);

        win.on('reset', function () {
            win.close();
        }, me);


        win.show();
    },

    openGraphMap: function (item, e, eOpts) {
        var me = this;
        var tree = me.getDirTree();
        var node = tree.getSelectedNode();
        var content = me.getMainContent();
        var context = node.data.id.split('/')[0]; //node.parentNode.parentNode.parentNode.data.text;
        var endpoint = node.data.id.split('/')[1]; //node.parentNode.parentNode.data.text
        var graphName = node.internalId;
        var title = 'Graph.' + context + "." + endpoint + '.' + node.data.text;
        var panelItemId = 'GraphMap.' + context + "." + endpoint + '.' + node.data.text; //'GraphMap-' + node.data.text;
        var templateTypes = ['Qualification', 'Definition']
        var roleTypes = ['Unknown', 'Property', 'Possessor', 'Reference', 'FixedValue', 'DataProperty', 'ObjectProperty'];

        var mapPanel = content.down('mappingpanel[title=' + title + ']');
        if (!mapPanel) {
            mapPanel = Ext.widget('mappingpanel', {
                'title': title,
                'contextName': context,
                'graph': graphName,
                'endpoint': endpoint,
                'itemId': panelItemId
            });
            var mapProp = mapPanel.down('propertypanel');

            var mapTree = mapPanel.down('mappingtree');
            var treeStore = mapTree.getStore();
            var params = treeStore.getProxy().extraParams;

            treeStore.on('beforeload', function (store, operation, eopts) {
                params.id = operation.node.data.identifier;
                params.graph = node.internalId;  //graphName;
                params.context = context;
                params.endpoint = endpoint;
            }, me);

            mapTree.on('beforeitemexpand', function () {
                content.getEl().mask('Loading...');
            }, me);

            mapTree.on('itemexpand', function () {
                content.getEl().unmask();
            }, me);

            mapTree.on('itemclick', function (tablepanel, record, item, index, e) {
                var tempProperty, dataObject, prop;
                var source = {};
                if (record.data.properties)
                    var propertiesCount = parseInt(record.data.properties.propertiesCount);

                if (record.data.type == 'TemplateMapNode' && propertiesCount > 0) {//if(record.data.type == 'TemplateMapNode' && record.data.propertiesCount>0){
                    for (var kk = 0; kk < propertiesCount; kk++) {
                        var propIndex = 'propertyName_' + kk;
                        var tempProperty = record.data.properties[propIndex];
                        if (tempProperty) {
                            dataObject = tempProperty.split('.')[0];
                            prop = tempProperty.split('.')[1];
                            var objectNode = node.parentNode.parentNode.childNodes[0];
                            for (var i = 0; i < objectNode.childNodes.length; i++) {
                                if (objectNode.childNodes[i].data.text == dataObject) {
                                    if (objectNode.childNodes[i].childNodes.length > 0) {
                                        for (var j = 0; j < objectNode.childNodes[i].childNodes.length; j++) {
                                            if (objectNode.childNodes[i].childNodes[j].data.text == prop) {
                                                var myTreeNode = objectNode.childNodes[i].childNodes[j];
                                                var selectedNode = me.getDirTree().getStore().getNodeById(myTreeNode.internalId);
                                                selectedNode.set('cls', 'bg_TreeNodeColor');
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else {
                    var objectNode = node.parentNode.parentNode.childNodes[0];
                    for (var i = 0; i < objectNode.childNodes.length; i++) {
                        if (objectNode.childNodes[i].childNodes.length > 0) {
                            for (var j = 0; j < objectNode.childNodes[i].childNodes.length; j++) {
                                var myTreeNode = objectNode.childNodes[i].childNodes[j];
                                var selectedNode = me.getDirTree().getStore().getNodeById(myTreeNode.internalId);
                                selectedNode.set('cls', '');
                            }
                        }
                    }
                }

                mapProp.setSource(record.data.record);
            });

            mapTree.on('beforeload', function (that, records) {
                content.getEl().mask('Loading...');
            }, me);

            mapTree.on('load', function (that, records) {
                content.getEl().unmask();
            }, me);

            treeStore.load({
                callback: function (records, options, success) {
                    if (mapTree.getRootNode().firstChild != undefined)
                        mapTree.getRootNode().firstChild.expand();

                    content.getEl().unmask();
                }
            });

            content.add(mapPanel);
        }
        content.setActiveTab(mapPanel);
    },

    addClassMap: function (item, e, eOpts) {
        var me = this;
        var content = me.getMainContent();
        var tree = content.getActiveTab().items.items[0];
        var endPoint = content.getActiveTab().endpoint;
        var contextName = content.getActiveTab().contextName;
        var tempGraph = content.getActiveTab().graph.split('/');
        var graph = tempGraph[tempGraph.length - 1];
        var itemId = 'GraphMap.' + contextName + '.' + endPoint + '.' + graph;
        var rootNodeId = tree.getRootNode().childNodes[0].internalId;
        var graph = rootNodeId.split('/')[1];
        var mapPanel = content.items.map[itemId]; //content.items.map['GraphMap-' + graph];

        node = tree.getSelectedNode();
        record = node.data.record;
        var roleName = node.data.text;
        if (roleName.indexOf('unmapped') != -1) {
            roleName = roleName.split('[')[0];
        }
        me.getParentClass(node);
        var index = node.parentNode.parentNode.indexOf(node.parentNode);
        var win = Ext.widget('classmapwindow', {
            mappingNode: node,
            formid: 'propertytarget-' + mapPanel.contextName + '-' + mapPanel.endpoint
        });

        win.on('save', function (arg) {
            var res = arg.result;
            if (res.success) {
                win.close();
                var parentNode = node.parentNode;
                var nodeIndex = parentNode.indexOf(node);
                parentNode.removeChild(node);
                parentNode.insertChild(nodeIndex, res.node);
                tree.view.refresh();
            }
            else {
            }

        }, me);

        win.on('reset', function () {
            win.close();
        }, me);

        var formRecord = {
            'scope': mapPanel.contextName,
            'app': mapPanel.endpoint,
            'graph': graph, //mapPanel.graph,
            'templateIndex': index,
            'roleName': roleName, //node.data.text,
            'parentClassId': node.parentNode.parentNode.data.identifier,
            'parentClassIndex': node.parentNode.parentNode.data.identifierIndex
        };

        var form = win.down('form');
        form.getForm().setValues(formRecord);
        form.updateDDContainers(record);

        win.show();

    },

    mapProperty: function (item, e, eOpts) {

        var me = this;
        var content = me.getMainContent();
        var tree = content.getActiveTab().items.items[0];
        var endPoint = content.getActiveTab().endpoint;
        var contextName = content.getActiveTab().contextName;
        var tempGraph = content.getActiveTab().graph.split('/');
        var graph = tempGraph[tempGraph.length - 1];
        var itemId = 'GraphMap.' + contextName + '.' + endPoint + '.' + graph;
        node = tree.getSelectedNode();
        var id = node.parentNode.data.id.split('/');
        var mappingNode = node.parentNode.data.parentId + '/' + id[id.length - 1] + '/' + node.data.record.name;
        var rootNodeId = tree.getRootNode().childNodes[0].internalId;
        var graph = rootNodeId.split('/')[1];
        var mapPanel = content.items.map[itemId]; //content.items.map['GraphMap-' + graph];
        win = Ext.widget('propertymapwindow', {
            'classId': node.parentNode.parentNode.data.identifier,
            'mappingNode': node
        });
        var roleName = getLastXString(node.data.id, 1);
        var index = node.parentNode.parentNode.indexOf(node.parentNode);

        var formRecord = {
            'contextName': mapPanel.contextName,
            'endpoint': mapPanel.endpoint,
            'graphName': graph,
            'index': index,
            'mappingNode': mappingNode,
            'classId': node.parentNode.parentNode.data.identifier,
            'classIndex': node.parentNode.parentNode.data.identifierIndex
        };

        var form = win.down('form');
        form.getForm().setValues(formRecord);
        win.on('save', function (arg) {
            win.close();
            var res = arg.result;
            if (res.success) {
                var parentNode = node.parentNode;
                var nodeIndex = parentNode.indexOf(node);
                parentNode.removeChild(node);
                parentNode.insertChild(nodeIndex, res.node);
                tree.view.refresh();
            }
            else {
            }
        }, me);
        win.on('Cancel', function () {
            win.close();
        }, me);
        win.show();
    },

    onMakeReference: function (item, e, eOpts) {
        var me = this;
        var content = me.getMainContent();
        var mapPanel = content.down('mappingpanel');
        var tree = content.getActiveTab().items.items[0];
        node = tree.getSelectedNode();
        parentNode = node.parentNode;
        var tempId = node.parentNode.data.parentId;
        var contextParts = tempId.split('/');
        Ext.Ajax.request({
            url: 'mapping/makereference',
            method: 'POST',
            params: {
                scope: contextParts[0], //mapPanel.contextName,
                app: contextParts[1], //mapPanel.endpoint,
                graph: contextParts[2], //mapPanel.graphName,
                roleName: getLastXString(node.data.id, 1),
                classId: parentNode.parentNode.data.identifier,
                classIndex: parentNode.parentNode.data.identifierIndex,
                templateIndex: parentNode.parentNode.indexOf(parentNode)
            },
            success: function (response, request) {
                var res = Ext.JSON.decode(response.responseText);
                if (res.success) {
                    var nodeIndex = parentNode.indexOf(node);
                    parentNode.removeChild(node);
                    parentNode.insertChild(nodeIndex, res.node);
                    tree.view.refresh();
                }
                else {
                    Ext.widget('messagepanel', { title: 'Error', msg: res.message });
                }
            },
            failure: function (response, request) {
                Ext.widget('messagepanel', { title: 'Error', msg: 'An error has occurred while Making Reference.' });
            }
        });
    },

    onEditOrNewValueMap: function (item, e, eOpts) {
        var me = this;
        var wintitle, contextName, endpoint, baseUrl, valueList, interName, classUrl, classLabel;
        var tree = this.getDirTree(),
      node = tree.getSelectedNode(),
      record = node.data.record;
        if (item.itemId == 'editvaluemap') {
            wintitle = 'Edit ValueMap';
        } else {
            wintitle = 'Add ValueMap';
        }

        if (node.data.record && node.data.type == 'ListMapNode') {
            interName = node.data.record.internalValue;
            classUrl = node.data.record.uri;
            classLabel = node.data.text.split('[')[0];
        }
        contextName = node.data.property.context;
        endpoint = node.data.property.endpoint;
        baseUrl = node.data.property.baseUrl;
        var arr = [];
        arr = node.data.id.split('ValueList');
        var arr1 = arr[arr.length - 1];
        valueList = arr1.split('/')[1];
        var win = Ext.widget('valuelistmapwindow', {
            id: 'tab-' + node.data.id, title: wintitle
        });
        var formRecord = {
            'contextName': contextName,
            'endpoint': endpoint,
            'valueList': valueList,
            'baseUrl': baseUrl,
            'mappingNode': node.data.id,
            'internalName': interName,
            'classUrl': classUrl,
            'classLabel': classLabel,
            'oldClassUrl': classUrl
        };
        var form = win.down('form');
        form.node = node;
        form.getForm().setValues(formRecord);
        win.on('save', function () {
            win.close();
            tree.view.refresh();
            //tree.onReload();
            //if (node.get('expanded') === false)
            //node.expand();
        }, me);
        win.on('reset', function () {
            win.close();
        }, me);
        win.down('form').updateDDContainer(record);
        win.show();
    },

    onDeleteValueMap: function (item, e, eOpts) {
        var me = this;
        var tree = me.getDirTree(),
		node = tree.getSelectedNode();
        Ext.Ajax.request({
            url: 'mapping/deleteValueMap',
            method: 'POST',
            params: {
                contextName: node.data.property.context,
                endpoint: node.data.property.endpoint,
                baseUrl: node.data.property.baseUrl,
                valueList: node.parentNode.data.property.Name,
                oldClassUrl: node.data.record.uri,
                mappingNode: node.data.id
            },
            success: function (response, request) {
                var parentNode = node.parentNode;
                parentNode.removeChild(node);
                tree.getSelectionModel().select(parentNode);
                //tree.onReload();
                tree.view.refresh();
            },
            failure: function (response, request) {
                Ext.widget('messagepanel', { title: 'Error', msg: 'An error has occurred while deleting Value Map.' });
                //showDialog(500, 160, 'Error', 'An error has occurred while deleting Value Map.', Ext.Msg.OK, null);	
            }

        });
    },

    onResetMapping: function (item, e, eOpts) {
        var me = this;
        var content = me.getMainContent();
        var tree = content.getActiveTab().items.items[0];
        var endPoint = content.getActiveTab().endpoint;
        var contextName = content.getActiveTab().contextName;
        var tempGraph = content.getActiveTab().graph.split('/');
        var graph = tempGraph[tempGraph.length - 1];
        var itemId = 'GraphMap.' + contextName + '.' + endPoint + '.' + graph;
        node = tree.getSelectedNode();
        var parentId = node.parentNode.parentNode.data.id;
        var idArr = node.data.id.split('/');
        var mappingNode = parentId + '/' + idArr[idArr.length - 2] + '/' + idArr[idArr.length - 1];
        var rootNodeId = tree.getRootNode().childNodes[0].internalId;
        var graph = rootNodeId.split('/')[1];
        var mapPanel = content.items.map[itemId]; //content.items.map['GraphMap-' + graph];
        tree.getEl().mask("Loading", 'x-mask-loading');
        me.getParentClass(node);
        Ext.Ajax.request({
            url: 'mapping/resetmapping',
            method: 'POST',
            params: {
                contextName: mapPanel.contextName,
                endpoint: mapPanel.endpoint,
                graphName: graph,
                mappingNode: mappingNode,
                roleId: node.data.record.id,
                templateId: node.parentNode.data.record.id,
                parentClassId: node.parentNode.parentNode.data.identifier,
                index: node.parentNode.parentNode.indexOf(node.parentNode),
                parentClassIndex: node.parentNode.parentNode.data.identifierIndex
            },
            success: function (result, request) {
                var res = Ext.JSON.decode(result.responseText);
                if (res.success) {
                    var parentNode = node.parentNode;
                    var nodeIndex = parentNode.indexOf(node);
                    parentNode.removeChild(node);
                    parentNode.insertChild(nodeIndex, res.node);
                    tree.view.refresh();
                }
                else {
                    Ext.widget('messagepanel', { title: 'Error', msg: res.message });
                }
                tree.getEl().unmask();
            },
            failure: function (response, request) {
                Ext.widget('messagepanel', { title: 'Error', msg: 'An error has occurred while Reset Mapping.' });
                tree.getEl().unmask();
            }
        });
    },

    onMapValueList: function (item, e, eOpts) {

        var me = this;
        var content = me.getMainContent();
        var tree = content.getActiveTab().items.items[0];
        var endPoint = content.getActiveTab().endpoint;
        var contextName = content.getActiveTab().contextName;
        var tempGraph = content.getActiveTab().graph.split('/');
        var graph = tempGraph[tempGraph.length - 1];
        var itemId = 'GraphMap.' + contextName + '.' + endPoint + '.' + graph;
        node = tree.getSelectedNode();
        me.getParentClass(node);
        var parentId = node.parentNode.parentNode.data.id;
        var idArr = node.data.id.split('/');
        var mappingNode = parentId + '/' + idArr[idArr.length - 2] + '/' + idArr[idArr.length - 1];
        var win = Ext.widget('mapvaluelistwindow');
        var rootNodeId = tree.getRootNode().childNodes[0].internalId;
        var graph = rootNodeId.split('/')[1];
        var mapPanel = content.items.map[itemId]; //content.items.map['GraphMap-' + graph];
        var formRecord = {
            'mappingNode': mappingNode, //node,
            'index': node.parentNode.parentNode.indexOf(node.parentNode),
            'classId': me.parentClass,
            'classIndex': me.parentClassIndex,
            'graphName': graph,
            'contextName': mapPanel.contextName,
            'endpoint': mapPanel.endpoint
        };

        var form = win.down('form');
        form.getForm().setValues(formRecord);

        win.on('Save', function (arg) {
            win.destroy();
            var res = arg.result;
            if (res.success) {
                var parentNode = node.parentNode;
                var nodeIndex = parentNode.indexOf(node);
                parentNode.removeChild(node);
                parentNode.insertChild(nodeIndex, res.node);
                tree.view.refresh();
            }
            else {
            }
        }, me);

        win.on('reset', function () {
            win.destroy();
        }, me);

        win.show();
    },

    onEditOrNewValueList: function (item, e, eOpts) {
        var me = this;
        var state, oldValueList, contextName, endpoint, baseUrl, valueList, wintitle;
        var tree = this.getDirTree(),
      node = tree.getSelectedNode();


        if (item.itemId == 'editvaluelist') {
            state = 'edit';
            nodeId = node.data.id;
            valueListName = node.data.record.name; //node.data.record.record.name;
            wintitle = 'Edit ValueList';
        } else {
            state = 'new';
            nodeId = node.data.id;
            valueListName = null;
            wintitle = 'Add ValueList';
        }

        var win = Ext.widget('valuelistwindow', {
            id: 'tab-' + node.data.id,
            title: wintitle
        });

        var formRecord = {
            'state': state,
            'oldValueList': valueListName,
            'mappingNode': nodeId,
            'valueList': valueListName
        };

        var form = win.down('form').getForm();
        win.down('form').node = node;
        form.setValues(formRecord);

        win.on('save', function () {
            win.close();
            //tree.onReload();
            tree.view.refresh();
        }, me);

        win.on('reset', function () {
            win.close();
        }, me);

        win.show();
    },

    onMakePossessor: function (item, e, eOpts) {
        var me = this;
        var content = me.getMainContent();
        var tree = content.getActiveTab().items.items[0];
        var endPoint = content.getActiveTab().endpoint;
        var contextName = content.getActiveTab().contextName;
        var tempGraph = content.getActiveTab().graph.split('/');
        var graph = tempGraph[tempGraph.length - 1];
        var itemId = 'GraphMap.' + contextName + '.' + endPoint + '.' + graph;
        node = tree.getSelectedNode();
        parentNode = node.parentNode;
        var tempId = node.parentNode.data.parentId;
        var nodeId = node.data.id.split('/');
        var text = nodeId[nodeId.length - 1];
        var mapingNode = tempId + '/' + text;
        var rootNodeId = tree.getRootNode().childNodes[0].internalId;
        var graph = rootNodeId.split('/')[1];
        var mapPanel = content.items.map[itemId]; //content.items.map['GraphMap-' + graph];
        tree.getEl().mask("Loading", 'x-mask-loading');
        Ext.Ajax.request({
            url: 'mapping/makePossessor',
            method: 'POST',
            params: {
                mappingNode: mapingNode,
                node: node,
                endpoint: mapPanel.endpoint,
                contextName: mapPanel.contextName,
                graph: graph,
                classId: parentNode.parentNode.data.identifier,
                index: parentNode.parentNode.indexOf(parentNode),
                classIndex: parentNode.parentNode.data.identifierIndex,
                roleName: text
            },
            success: function (response, request) {
                var res = Ext.JSON.decode(response.responseText);
                if (res.success) {
                    var nodeIndex = parentNode.indexOf(node);
                    parentNode.removeChild(node);
                    parentNode.insertChild(nodeIndex, res.node);
                    tree.view.refresh();
                }
                else {
                    Ext.widget('messagepanel', { title: 'Error', msg: res.message });
                }
                tree.getEl().unmask();
            },
            failure: function (response, request) {
                Ext.widget('messagepanel', { title: 'Error', msg: 'An error has occurred while Making Possessor.' });
                tree.getEl().unmask();
            }
        });
    },

    onDeleteGraphMap: function (item, e, eOpts) {
        var me = this;
        var tree = me.getDirTree(),
		node = tree.getSelectedNode();
        Ext.Ajax.request({
            url: 'mapping/deletegraphmap',
            method: 'POST',
            params: {
                scope: node.parentNode.parentNode.parentNode.data.text, //node.data.property.context,
                application: node.parentNode.parentNode.data.text, //node.data.property.endpoint,
                baseUrl: node.data.property.baseUrl,
                mappingNode: node.id,
                graphName: getLastXString(node.id, 1)
            },
            success: function (response, request) {
                var parentNode = node.parentNode;
                parentNode.removeChild(node);
                tree.getSelectionModel().select(parentNode);
                //tree.onReload();
                tree.view.refresh();
            },
            failure: function (response, request) {
                Ext.widget('messagepanel', { title: 'Error', msg: 'An error has occurred while deleting Graph Map.' });
                //showDialog(500, 160, 'Error', 'An error has occurred while deleting Graph Map.', Ext.Msg.OK, null);	
            }
        });
    },

    onDeleteValueList: function (item, e, eOpts) {
        var me = this;
        var tree = me.getDirTree(),
      node = tree.getSelectedNode(),
      parentNode = node.parentNode,
      valueList = getLastXString(node.id, 1);

        Ext.Ajax.request({
            url: 'mapping/deletevaluelist',
            method: 'POST',
            params: {
                valueList: valueList,
                mappingNode: node.data.id
            },
            success: function (response, request) {
                //tree.getSelectionModel().select(parentNode);
                //tree.onReload();
                var parentNode = node.parentNode;
                parentNode.removeChild(node);
                tree.getSelectionModel().select(parentNode);
                tree.view.refresh();
            },
            failure: function (response, request) {
                Ext.widget('messagepanel', { title: 'Error', msg: 'An error has occurred while deleting Value List.' });
                //showDialog(500, 160, 'Error', 'An error has occurred while deleting Value List.', Ext.Msg.OK, null);	
            }
        });
    },

    onDeleteClassMap: function (item, e, eOpts) {
        var me = this;
        var content = me.getMainContent();
        var tree = content.getActiveTab().items.items[0];
        var endPoint = content.getActiveTab().endpoint;
        var contextName = content.getActiveTab().contextName;
        var tempGraph = content.getActiveTab().graph.split('/');
        var graph = tempGraph[tempGraph.length - 1];
        var itemId = 'GraphMap.' + contextName + '.' + endPoint + '.' + graph;
        node = tree.getSelectedNode();
        var mappingNode = node.parentNode.parentNode.parentNode.data.id;
        var tempId = node.data.id.split('/');
        tempId = tempId[tempId.length - 1];
        mappingNode = mappingNode + '/' + tempId;
        var index = node.parentNode.parentNode.parentNode.indexOf(node.parentNode.parentNode);
        var templateNodeId = node.parentNode.parentNode.data.id;
        var rootNodeId = tree.getRootNode().childNodes[0].internalId;
        var graph = rootNodeId.split('/')[1];
        var mapPanel = content.items.map[itemId]; //content.items.map['GraphMap-' + graph];
        tree.getEl().mask("Loading", 'x-mask-loading');
        Ext.Ajax.request({
            url: 'mapping/deleteclassmap',
            method: 'POST',
            params: {
                mappingNode: mappingNode, //node.data.id,
                classId: node.data.identifier,
                parentClass: node.parentNode.parentNode.parentNode.data.identifier,
                parentTemplate: node.parentNode.parentNode.data.record.id,
                parentRole: node.parentNode.data.record.id,
                index: index,
                parentClassIndex: node.parentNode.parentNode.parentNode.data.identifierIndex,
                contextName: mapPanel.contextName,
                endpoint: mapPanel.endpoint,
                graph: graph,
                templateNodeId: templateNodeId
            },
            success: function (result, request) {
                var res = Ext.JSON.decode(result.responseText);
                if (res.success) {
                    var templateNode = node.parentNode.parentNode;
                    var roleNode = node.parentNode;
                    var roleNodeIndex = templateNode.indexOf(roleNode);
                    templateNode.removeChild(roleNode);
                    templateNode.insertChild(roleNodeIndex, res.node);
                    tree.view.refresh();
                }
                else {
                    Ext.widget('messagepanel', { title: 'Error', msg: res.message });
                }
                tree.getEl().unmask();
            },
            failure: function (result, request) {
                Ext.widget('messagepanel', { title: 'Error', msg: 'An error has occurred while deleting Class Map.' });
                tree.getEl().unmask();
            }
        })
    },

    onMapLiteral: function (item, e, eOpts) {

        var me = this;
        var content = me.getMainContent();
        var tree = content.getActiveTab().items.items[0];
        var endPoint = content.getActiveTab().endpoint;
        var contextName = content.getActiveTab().contextName;
        var tempGraph = content.getActiveTab().graph.split('/');
        var graph = tempGraph[tempGraph.length - 1];
        var itemId = 'GraphMap.' + contextName + '.' + endPoint + '.' + graph;
        node = tree.getSelectedNode();
        me.getParentClass(node);
        var parentId = node.parentNode.parentNode.data.id;
        var idArr = node.data.id.split('/');
        var mappingNode = parentId + '/' + idArr[idArr.length - 2] + '/' + idArr[idArr.length - 1];
        var win = Ext.widget('literalwindow');
        var form = win.down('form');
        var constantValue = form.getForm().findField('constantValue').getValue();
        var index = node.parentNode.parentNode.indexOf(node.parentNode);
        var rootNodeId = tree.getRootNode().childNodes[0].internalId;
        var graph = rootNodeId.split('/')[1];
        var mapPanel = content.items.map[itemId]; //content.items.map['GraphMap-' + graph];
        var formRecord = {
            constantValue: constantValue,
            mappingNode: node.data.id,
            classId: node.parentNode.parentNode.data.identifier,
            index: index,
            endpoint: mapPanel.endpoint,
            contextName: mapPanel.contextName,
            graph: graph,
            classIndex: node.parentNode.parentNode.data.identifierIndex
        };

        form.getForm().setValues(formRecord);

        win.on('Save', function (arg) {
            win.destroy();
            var res = arg.result;
            if (res.success) {
                var parentNode = node.parentNode;
                var nodeIndex = parentNode.indexOf(node);
                parentNode.removeChild(node);
                parentNode.insertChild(nodeIndex, res.node);
                tree.view.refresh();
            }
            else {
            }
        }, me);

        win.on('reset', function () {
            win.destroy();
        }, me);

        win.show();
    },

    onRefreshFacade: function (item, e, eOpts) {
    },

    getObjectType: function (type) {
        switch (type) {
            case 0:
                return 'Unknown';
            case 1:
                return 'Property';
            case 2:
                return 'Possessor';
            case 3:
                return 'Reference';
            case 4:
                return 'FixedValue';
            case 5:
                return 'DataProperty';
            case 6:
                return 'ObjectProperty';
        }
    },

    getParentClass: function (n) {
        var me = this;
        if (n.parentNode !== null && n.parentNode !== undefined) {
            if ((n.parentNode.data.type == 'ClassMapNode' ||
      n.parentNode.data.type == 'GraphMapNode') &&
      n.parentNode.data.identifier !== undefined) {
                me.parentClass = n.parentNode.data.identifier;
                me.parentClassIndex = n.parentNode.data.identifierIndex;
                return me.parentClass;
            }
            else {
                me.getParentClass(n.parentNode);
            }
        }
        return me.parentClass;

    },

    init: function (application) {
        var me = this;
        me.application.addEvents('opengraphmap');
        Ext.QuickTips.init();

        this.control({
            "menuitem[action=templatemapdelete]": {
                click: this.onDeleteTemplateMap
            },
            "menuitem[action=editnewgraph]": {
                click: this.onEditOrNewGraph
            },
            "menuitem[action=opengraph]": {
                click: this.openGraphMap
            },
            "menuitem[action=addclassmap]": {
                click: this.addClassMap
            },
            "menuitem[action=mapproperty]": {
                click: this.mapProperty
            },
            "menuitem[action=makereference]": {
                click: this.onMakeReference
            },
            "menuitem[action=editnewvaluemap]": {
                click: this.onEditOrNewValueMap
            },
            "menuitem[action=deletevaluemap]": {
                click: this.onDeleteValueMap
            },
            "menuitem[action=resetmapping]": {
                click: this.onResetMapping
            },
            "menuitem[action=mapvaluelist]": {
                click: this.onMapValueList
            },
            "menuitem[action=editnewvaluelist]": {
                click: this.onEditOrNewValueList
            },
            "menuitem[action=makepossessor]": {
                click: this.onMakePossessor
            },
            "menuitem[action=deletegraph]": {
                click: this.onDeleteGraphMap
            },
            "menuitem[action=deletevaluelist]": {
                click: this.onDeleteValueList
            },
            " menuitem[action=deleteclassmap]": {
                click: this.onDeleteClassMap
            },
            "menuitem[action=mapliteral]": {
                click: this.onMapLiteral
            },
            " menuitem[action=refreshfacade]": {
                click: this.onRefreshFacade
            }
        });

        application.on({
            opengraphmap: {
                fn: this.onOpenGraphMap,
                scope: this
            }
        });
    },

    onOpenGraphMap: function () {
        var me = this;
        me.openGraphMap();
    }
});
