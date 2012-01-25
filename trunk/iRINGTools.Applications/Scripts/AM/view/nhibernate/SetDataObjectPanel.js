Ext.define('AM.view.nhibernate.SetDataObjectPanel', {
    extend: 'Ext.form.Panel',
    alias: 'widget.setdataobjectpanel',
    name: 'dataObject',
    border: false,
    autoScroll: true,
    monitorValid: true,
    labelWidth: 160,
    bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
    defaults: { anchor: '100%', xtype: 'textfield', allowBlank: false },

    initComponent: function () {
        this.items = [{
            xtype: 'label',
            fieldLabel: 'Data Object',
            labelSeparator: '',
            itemCls: 'form-title'
        }, {
            name: 'tableName',
            fieldLabel: 'Table Name',
            value: node.attributes.properties.tableName,
            disabled: true
        }, {
            name: 'objectNamespace',
            fieldLabel: 'Object Namespace',
            value: node.attributes.properties.objectNamespace
        }, {
            name: 'objectName',
            fieldLabel: 'Object Name',
            value: node.attributes.properties.objectName
        }, {
            name: 'keyDelimeter',
            fieldLabel: 'Key Delimiter',
            value: node.attributes.properties.keyDelimiter,
            allowBlank: true
        }];
        this.treeNode = node,
			this.tbar = new Ext.Toolbar({
			    items: [{
			        xtype: 'tbspacer',
			        width: 4
			    }, {
			        xtype: 'tbbutton',
			        icon: 'Content/img/16x16/apply.png',
			        text: 'Apply',
			        tooltip: 'Apply the current changes to the data objects tree',
			        handler: function (f) {
			            var form = dataObjectFormPanel.getForm();
			            if (form.treeNode) {
			                var treeNodeProps = form.treeNode.attributes.properties;
			                var objNam = form.findField('objectName').getValue();
			                var oldObjNam = treeNodeProps['objectName'];
			                treeNodeProps['tableName'] = form.findField('tableName').getValue();
			                treeNodeProps['objectName'] = objNam;
			                treeNodeProps['keyDelimiter'] = form.findField('keyDelimeter').getValue();

			                for (var ijk = 0; ijk < dbDict.dataObjects.length; ijk++) {
			                    var dataObject = dbDict.dataObjects[ijk];
			                    if (form.treeNode.text.toUpperCase() != dataObject.objectName.toUpperCase())
			                        continue;
			                    dataObject.objectName = objNam;
			                }

			                form.treeNode.set('title', objNam);
			                form.treeNode.text = objNam;
			                form.treeNode.attributes.text = objNam;
			                form.treeNode.attributes.properties.objectName = objNam;

			                var dsConfigPane = editPane.items.map[scopeName + '.' + appName + '.dsconfigPane'];
			                var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];
			                var rootNode = dbObjectsTree.getRootNode();

			                for (var i = 0; i < rootNode.childNodes.length; i++) {
			                    var folderNode = rootNode.childNodes[i];
			                    var folderNodeProp = folderNode.attributes.properties;
			                    if (folderNode.childNodes[2])
			                        var relationFolderNode = folderNode.childNodes[2];
			                    else
			                        var relationFolderNode = folderNode.attributes.children[2];

			                    if (!relationFolderNode)
			                        continue;

			                    if (relationFolderNode.childNodes)
			                        var relChildenNodes = relationFolderNode.childNodes;
			                    else
			                        var relChildenNodes = relationFolderNode.children;

			                    if (relChildenNodes) {
			                        for (var k = 0; k < relChildenNodes.length; k++) {
			                            var relationNode = relChildenNodes[k];

			                            if (relationNode.text == '')
			                                continue;

			                            if (relationNode.attributes.attributes)
			                                var relationNodeAttr = relationNode.attributes.attributes;
			                            else
			                                var relationNodeAttr = relationNode.attributes;

			                            var relObjNam = relationNodeAttr.relatedObjectName;
			                            if (relObjNam.toLowerCase() != objNam.toLowerCase() && relObjNam.toLowerCase() == oldObjNam.toLowerCase())
			                                relationNodeAttr.relatedObjectName = objNam;

			                            var relatedObjPropMap = relationNodeAttr.relatedObjMap;

			                            for (var iki = 0; iki < relatedObjPropMap.length; iki++) {
			                                if (relatedObjPropMap[iki].relatedObjName.toLowerCase() == oldObjNam.toLowerCase())
			                                    relatedObjPropMap[iki].relatedObjName = objNam;
			                            }
			                        }
			                    }
			                }

			                var items = editPane.items.items;

			                for (var i = 0; i < items.length; i++) {
			                    var relateObjField = items[i].getForm().findField('relatedObjectName');
			                    if (relateObjField)
			                        if (relateObjField.getValue().toLowerCase() == oldObjNam.toLowerCase())
			                            relateObjField.setValue(objNam);
			                }
			            }
			        }
			    }, {
			        xtype: 'tbspacer',
			        width: 4
			    }, {
			        xtype: 'tbbutton',
			        icon: 'Content/img/16x16/edit-clear.png',
			        text: 'Reset',
			        tooltip: 'Reset to the latest applied changes',
			        handler: function (f) {
			            var form = dataObjectFormPanel.getForm();
			            if (node.attributes.properties) {
			                form.findField('objectName').setValue(node.attributes.properties.objectName);
			                form.findField('keyDelimeter').setValue(node.attributes.properties.keyDelimiter);
			            }
			        }
			    }]
			});

        editPane.add(dataObjectFormPanel);
        var panelIndex = editPane.items.indexOf(dataObjectFormPanel);
        editPane.getLayout().setActiveItem(panelIndex);
    }
});

