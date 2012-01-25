
Ext.define('AM.view.nhibernate.NHibernateTree', {
    extend: 'Ext.tree.Panel',
    alias: 'widget.nhibernatetree',
    border: false,
    autoScroll: true,
    animate: true,
    expandAll: true,
    lines: true,
    frame: false,
    enableDD: false,
    containerScroll: true,
    rootVisible: true,
    contextName: null,
    endpoint: null,
    store: 'NHibernateTreeStore',
   

    initComponent: function () {
        var wizard = this;
        var scopeName = this.contextName;
        var appName = this.endpoint;        

        this.tbar = new Ext.Toolbar({
            items: [{
                xtype: 'tbspacer',
                width: 4
            }, {
                xtype: 'button',
                icon: 'Content/img/16x16/view-refresh.png',
                text: 'Reload',
                tooltip: 'Reload Data Objects',
                handler: function () {
                    var editPane = Ext.widget('editorpanel');
                    var items = editPane.items.items;

                    for (var i = 0; i < items.length; i++) {
                        items[i].destroy();
                        i--;
                    }

                    Ext.Ajax.request({
                        url: 'AdapterManager/DBDictionary',
                        method: 'POST',
                        params: {
                            scope: scopeName,
                            app: appName
                        },
                        success: function (response, request) {
                            AM.view.nhibernate.dbDict.value = Ext.JSON.decode(response.responseText);
                            var dbDict = AM.view.nhibernate.dbDict.value;

                            if (dbDict.ConnectionString) {
                                var base64 = AM.view.nhibernate.Utility;
                                dbDict.ConnectionString = base64.decode(dbDict.ConnectionString);
                            }

                            var dbObjectsTree = Ext.widget('nhibernatetree');

                            if (dbDict.dataObjects.length > 0) {
                                // populate data source form
                                showTree(scopeName, appName);
                            }
                            else {
                                dbObjectsTree.disable();
                                setDsConfigPane(scopeName, appName);
                            }
                        },
                        failure: function (response, request) {
                            setDsConfigPane(scopeName, appName);
                        }
                    });
                }
            }, {
                xtype: 'tbspacer',
                width: 4
            }, {
                xtype: 'button',
                icon: 'Content/img/16x16/document-properties.png',
                text: 'Edit Connection',
                tooltip: 'Edit database connection',
                handler: function () {
                    setDsConfigPane(scopeName, appName);
                }
            }, {
                xtype: 'tbspacer',
                width: 4
            }, {
                xtype: 'button',
                icon: 'Content/img/16x16/document-save.png',
                text: 'Save',
                tooltip: 'Save the data objects tree to the back-end server',
                formBind: true,
                handler: function (button) {
                    var rootNode = dbObjectsTree.getRootNode();
                    var treeProperty = getTreeJson(rootNode);

                    Ext.Ajax.request({
                        url: 'AdapterManager/Trees',
                        method: 'POST',
                        params: {
                            scope: scopeName,
                            app: appName,
                            tree: JSON.stringify(treeProperty)
                        },
                        success: function (response, request) {
                            var rtext = response.responseText;
                            if (rtext.toUpperCase().indexOf('FALSE') == -1) {
                                showDialog(400, 100, 'Saving Result', 'The configuraiton has been saved successfully.', Ext.Msg.OK, null);
                                var directoryTree = Ext.widget('directorytree');
                                directoryTree.onReload();
                            }
                            else {
                                var ind = rtext.indexOf('}');
                                var len = rtext.length - ind - 1;
                                var msg = rtext.substring(ind + 1, rtext.length - 1);
                                showDialog(400, 100, 'Saving Result - Error', msg, Ext.Msg.OK, null);
                            }
                        },
                        failure: function (response, request) {
                            showDialog(660, 300, 'Saving Result', 'An error has occurred while saving the configuration.', Ext.Msg.OK, null);
                        }
                    });
                }
            }]
        });

        this.on("click", function (node, e) {
            if (node.isRoot) {
                setTablesSelectorPane(scopeName, appName);
                return;
            }
            else if (!node)
                return;

            var nodeType = node.attributes.type;

            if (!nodeType && node.attributes.attributes)
                nodeType = node.attributes.attributes.type;

            if (nodeType) {
                var editPane = Ext.widget('editorpanel');
                editPane.show();
                var editPaneLayout = editPane.getLayout();

                switch (nodeType.toUpperCase()) {
                    case 'DATAOBJECT':
                        setDataObject(node, scopeName, appName);
                        break;

                    case 'KEYS':
                        setKeysFolder(node, scopeName, appName);
                        break;

                    case 'KEYPROPERTY':
                        setKeyProperty(node, scopeName, appName);
                        break;

                    case 'PROPERTIES':
                        setPropertiesFolder(node, scopeName, appName);
                        break;

                    case 'DATAPROPERTY':
                        setDataProperty(node, scopeName, appName);
                        break;

                    case 'RELATIONSHIPS':
                        setRelations(node, scopeName, appName);
                        break;

                    case 'RELATIONSHIP':
                        setRelationFields(node, scopeName, appName);
                        break;
                }
            }
            else {
                editPane.hide();
            }
        });

        this.callParent(arguments);
    }
});

function setDataObject(node, scopeName, appName) {
    var editPane = Ext.widget('editorpanel');

    if (editPane && node) {
        if (Ext.widget('setdataobjectpanel')) {
            var objectNameFormPane = Ext.widget('setdataobjectpanel');
            if (objectNameFormPane) {
                var panelIndex = editPane.items.indexOf(objectNameFormPane);
                editPane.getLayout().setActiveItem(panelIndex);
                return;
            }
        }

        if (!node.attributes.properties.objectNamespace)
            node.attributes.properties.objectNamespace = "org.iringtools.adapter.datalayer.proj_" + scopeName + "." + appName;

        var conf = {
            id: scopeName + '.' + appName + '.objectNameForm.' + node.id,        
            node: node,
            contextName: this.contextName,
            endpoint: this.endpoint
        };

        var setDataObjectPanel = Ext.widget('setdataobjectpanel', conf);
    }
};

function setDsConfigPane(scopeName, appName) {
    var editPane = Ext.widget('editorpanel');

    if (editPane) {
        if (Ext.widget('connectdatabase')) {
            var dsConfigPanel = Ext.widget('connectdatabase');
           
            if (dsConfigPanel) {
                var panelIndex = editPane.items.indexOf(dsConfigPanel);
                editPane.getLayout().setActiveItem(panelIndex);
                return;
            }
        }

        var confConnect = {
            id: contextName + '.' + endpoint + '.dsconfigPane',
            scopeName: scopeName,
            appName: appName
        };

        var connectDBPanel = Ext.widget('connectdatabase', confConnect);
    }
};

function setTablesSelectorPane(scopeName, appName) {
    var editPane = Ext.widget('editorpanel');

    if (editPane) {
        if (Ext.widget('selecttables')) {
            var tableSelectorPanel = Ext.widget('selecttables');
            if (tableSelectorPanel) {
                if (dbObjectsTree.disabled)
                    tableSelectorPanel.destroy();
                else {
                    var panelIndex = editPane.items.indexOf(tableSelectorPanel);
                    editPane.getLayout().setActiveItem(panelIndex);
                    return;
                }
            }
        }
        var confsetTables = {
            id: scopeName + '.' + appName + '.tablesSelectorPane',
            scopeName: scopeName,
            appName: appName
        };

        var setTablesPanel = Ext.widget('selecttables', confsetTables);
    }
};

function setKeysFolder(node, scopeName, appName) {
    var editPane = Ext.widget('editorpanel');

    if (editPane && node) {
        if (Ext.widget('selectkeyspanel')) {
            var keysSelectorPane = Ext.widget('selectkeyspanel');
            if (keysSelectorPane) {                
                keysSelectorPane.destroy();
            }
        }

        var confselectKeys = {
            id: scopeName + '.' + appName + '.keysSelector.' + node.id,
            node: node,
            scopeName: scopeName,
            appName: appName
        };

        var setKeysFolderPanel = Ext.widget('selectkeyspanel', confselectKeys);
    }
};

function setKeyProperty(node, scopeName, appName) {
    var editPane = Ext.widget('editorpanel');

    if (editPane && node) {
        if (Ext.widget('setkey')) {
            var keyPropertyFormPane = Ext.widget('setkey');
            if (keyPropertyFormPane) {
                var panelIndex = editPane.items.indexOf(keyPropertyFormPane);
                editPane.getLayout().setActiveItem(panelIndex);
                return;
                //keyPropertyFormPane.destroy();					
            }
        }

        var confsetKeyProp = {
            id: scopeName + '.' + appName + '.keyPropertyForm.' + node.id,
            node: node,
            scopeName: scopeName,
            appName: appName
        };

        var setKeyPropPanel = Ext.widget('setkey', confsetKeyProp);
    }
};

function setPropertiesFolder(node, scopeName, appName) {
    if (editPane && node) {
        if (Ext.widget('selectproperties')) {
            var propertiesSelectorPane = Ext.widget('selectproperties');
            if (propertiesSelectorPane) {
                propertiesSelectorPane.destroy();
            }
        }
        var confsetPropsFolder = {
            id: scopeName + '.' + appName + '.propertiesSelector.' + node.id,
            node: node,
            scopeName: scopeName,
            appName: appName
        };

        var setPropsFolderPanel = Ext.widget('selectproperties', confsetPropsFolder);
    }
};

function setDataProperty(node, scopeName, appName) {
    var editPane = Ext.widget('editorpanel');

    if (editPane && node) {
        if (Ext.widget('setproperty')) {
            var dataPropertyFormPane = Ext.widget('setproperty');
            if (dataPropertyFormPane) {
                var panelIndex = editPane.items.indexOf(dataPropertyFormPane);
                editPane.getLayout().setActiveItem(panelIndex);
                return;
            }
        }
        var confsetProp = {
            id: scopeName + '.' + appName + '.dataPropertyForm.' + node.id,
            node: node,
            scopeName: scopeName,
            appName: appName
        };

        var setPropPanel = Ext.widget('setproperty', confsetProp);
    }
};

function setRelations(node, scopeName, appName) {
    var editPane = Ext.widget('editorpanel');

    if (editPane && node) {
        if (Ext.widget('createrelations')) {
            var relationCreatePane = Ext.widget('createrelations');
            if (relationCreatePane) {
                relationCreatePane.destroy();
            }
        }
        var confsetRelations = {
            id: scopeName + '.' + appName + '.relationCreateForm.' + node.id,
            node: node,
            scopeName: scopeName,
            appName: appName
        };

        var setRelationsPanel = Ext.widget('createrelations', confsetRelations);
    }
};

function setRelationFields(node, scopeName, appName) {
    var editPane = Ext.widget('editorpanel');

    if (editPane && node) {
        var relationFolderNode = node.parentNode;
        var dataObjectNode = relationFolderNode.parentNode;

        var relatedObjects = new Array();
        var dbObjectsTree = Ext.widget('nhibernatetree');
        
        var rootNode = dbObjectsTree.getRootNode();
        var thisObj = dataObjectNode.text;
        var ifExist;
        var relAttribute = null;
        var relateObjStr;
        var nodeRelateObj;
        var rindex = 0
        for (var i = 0; i < rootNode.childNodes.length; i++) {
            relateObjStr = rootNode.childNodes[i].text;
            ifExist = false;
            for (var j = 0; j < relationFolderNode.childNodes.length; j++) {
                if (relationFolderNode.childNodes[j].text == '' || relationFolderNode.childNodes[j].id == node.id)
                    continue;
                if (relationFolderNode.childNodes[j].attributes.attributes)
                    relAttribute = relationFolderNode.childNodes[j].attributes.attributes;
                else if (relationFolderNode.childNodes[j].attributes)
                    relAttribute = relationFolderNode.childNodes[j].attributes;

                if (relAttribute) {
                    nodeRelateObj = relAttribute.relatedObjectName;
                    if (relateObjStr.toLowerCase() == nodeRelateObj.toLowerCase())
                        ifExist = true;
                }
            }

            if (relateObjStr.toLowerCase() != thisObj.toLowerCase() && ifExist == false) {
                relatedObjects.push([rindex.toString(), rootNode.childNodes[i].text]);
                rindex++;
            }

        }

        var selectedProperties = new Array();

        var ii = 0;
        if (dataObjectNode) {
            if (dataObjectNode.childNodes[0]) {
                var keysNode = dataObjectNode.childNodes[0];
                for (var i = 0; i < keysNode.childNodes.length; i++)
                    if (!keysNode.childNodes[i].hidden) {
                        selectedProperties.push([ii.toString(), keysNode.childNodes[i].text]);
                        ii++;
                    }
            }
            else {
                var keysNode = dataObjectNode.attributes.children[0];
                for (var i = 0; i < keysNode.children.length; i++)
                    if (!keysNode.children[i].hidden) {
                        selectedProperties.push([ii.toString(), keysNode.children[i].text]);
                        ii++;
                    }
            }

            if (dataObjectNode.childNodes[1]) {
                var propertiesNode = dataObjectNode.childNodes[1];
                for (var i = 0; i < propertiesNode.childNodes.length; i++)
                    if (!propertiesNode.childNodes[i].hidden) {
                        selectedProperties.push([ii.toString(), propertiesNode.childNodes[i].text]);
                        ii++;
                    }
            }
            else {
                var propertiesNode = dataObjectNode.attributes.children[1];
                for (var i = 0; i < propertiesNode.children.length; i++)
                    if (!propertiesNode.children[i].hidden) {
                        selectedProperties.push([ii.toString(), propertiesNode.children[i].text]);
                        ii++;
                    }
            }
        }

        var mappingProperties = new Array();
        ii = 0;

        if (editPane.items.map[scopeName + '.' + appName + '.relationFieldsForm.' + node.id]) {
            var relPane = editPane.items.map[scopeName + '.' + appName + '.relationFieldsForm.' + node.id];
            var relatedObjectField = relPane.getForm().findField('relatedObjectName');
            if (relatedObjectField.getValue() != relatedObjectField.lastSelectionText && relatedObjectField.lastSelectionText && relatedObjectField.lastSelectionText != '')
                var relatedObjectName = relatedObjectField.lastSelectionText;
            else
                var relatedObjectName = relatedObjectField.getValue();
        }
        else {
            if (node.attributes.attributes)
                var nodeAttribute = node.attributes.attributes;
            else
                var nodeAttribute = node.attributes;

            var relatedObjectName = nodeAttribute.relatedObjectName;
        }

        if (relatedObjectName != '') {
            var relatedDataObjectNode = rootNode.findChild('text', relatedObjectName);
            if (relatedDataObjectNode) {
                if (relatedDataObjectNode.childNodes[0]) {
                    keysNode = relatedDataObjectNode.childNodes[0];
                    for (var i = 0; i < keysNode.childNodes.length; i++)
                        if (!keysNode.childNodes[i].hidden) {
                            mappingProperties.push([ii.toString(), keysNode.childNodes[i].text]);
                            ii++;
                        }
                }
                else {
                    keysNode = relatedDataObjectNode.attributes.children[0];
                    for (var i = 0; i < keysNode.children.length; i++)
                        if (!keysNode.children[i].hidden) {
                            mappingProperties.push([ii.toString(), keysNode.children[i].text]);
                            ii++;
                        }
                }

                if (relatedDataObjectNode.childNodes[1]) {
                    propertiesNode = relatedDataObjectNode.childNodes[1];
                    for (var i = 0; i < propertiesNode.childNodes.length; i++)
                        if (!propertiesNode.childNodes[i].hidden) {
                            mappingProperties.push([ii.toString(), propertiesNode.childNodes[i].text]);
                            ii++;
                        }
                }
                else {
                    propertiesNode = relatedDataObjectNode.attributes.children[1];
                    for (var i = 0; i < propertiesNode.children.length; i++)
                        if (!propertiesNode.children[i].hidden) {
                            mappingProperties.push([ii.toString(), propertiesNode.children[i].text]);
                            ii++;
                        }
                }
            }
        }
        else {
            mappingProperties.push(['0', '']);
        }

        if (editPane.items.map[scopeName + '.' + appName + '.relationFieldsForm.' + node.id]) {
            var relationConfigPane = editPane.items.map[scopeName + '.' + appName + '.relationFieldsForm.' + node.id];
            if (relationConfigPane) {
                //relationConfigPane.destroy();
                var panelIndex = editPane.items.indexOf(relationConfigPane);
                editPane.getLayout().setActiveItem(panelIndex);

                var objText = relationConfigPane.getForm().findField('objectName');
                objText.setValue(dataObjectNode.attributes.properties.objectName);

                var relCombo = relationConfigPane.getForm().findField('relatedObjectName');
                if (relCombo.store.data) {
                    relCombo.reset();
                    relCombo.store.removeAll();
                }
                relCombo.store.loadData(relatedObjects);
                relCombo.store.commitChanges();

                relCombo.setValue(relatedObjectName);
                relationConfigPane.getForm().findField('relatedTable').setValue(relatedObjectName);

                var mapCombo = relationConfigPane.getForm().findField('mapPropertyName');
                if (mapCombo.store.data) {
                    mapCombo.reset();
                    mapCombo.store.removeAll();
                }
                mapCombo.store.loadData(mappingProperties);
                mapCombo.store.commitChanges();

                var propCombo = relationConfigPane.getForm().findField('propertyName');
                if (propCombo.store.data) {
                    propCombo.reset();
                    propCombo.store.removeAll();
                }
                propCombo.store.loadData(selectedProperties);
                propCombo.store.commitChanges();
                return;
            }
        }

        var confsetRelationField = {
            id: scopeName + '.' + appName + '.relationFieldsForm.' + node.id,
            node: node,
            scopeName: scopeName,
            appName: appName
        };

        var setRelationFieldPanel = Ext.widget('setrelation', confsetRelationField);
    }
};


