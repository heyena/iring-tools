Ext.define('AM.view.nhibernate.SetDataObjectPanel', {
    extend: 'Ext.form.Panel',
    alias: 'widget.setdataobjectpanel',
    name: 'dataObject',
    border: false,
    autoScroll: true,
    monitorValid: true,
    labelWidth: 160,
    contextName: null,
    endpoint: null,
    bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
    defaults: {
        anchor: '100%',
        xtype: 'textfield',
        allowBlank: false
    },
    initComponent: function () {
        var me = this;
        me.items = [
            {
                xtype: 'label',
                fieldLabel: 'Data Object',
                labelSeparator: '',
                itemCls: 'form-title'
            },
            {
                xtype: 'textfield',
                name: 'tableName',
                fieldLabel: 'Table Name',
                disabled: true
            },
            {
                xtype: 'textfield',
                name: 'objectNamespace',
                fieldLabel: 'Object Namespace'
            },
            {
                xtype: 'textfield',
                name: 'objectName',
                fieldLabel: 'Object Name'
            },
            {
                xtype: 'textfield',
                name: 'keyDelimeter',
                fieldLabel: 'Key Delimiter',
                allowBlank: true
            }
        ];
        me.tbar = new Ext.Toolbar({
            items: [
                {
                    xtype: 'tbspacer',
                    width: 4
                },
                {
                    xtype: 'button',
                    icon: 'Content/img/16x16/apply.png',
                    text: 'Apply',
                    tooltip: 'Apply the current changes to the data objects tree',
                    scope: this,
                    action: 'applydbobjectchanges'

                },
                {
                    xtype: 'tbspacer',
                    width: 4
                },
                {
                    xtype: 'button',
                    icon: 'Content/img/16x16/edit-clear.png',
                    text: 'Reset',
                    tooltip: 'Reset to the latest applied changes',
                    scope: this,
                    action: 'resetdbobjectchanges'
                }
            ]
        }),
        this.callParent(arguments);
    },

    setActiveRecord: function (record) {
        if (record) {       
            this.getForm().setValues(record);
        } else {
           this.getForm().reset();
        }
    }
});



//			        handler: function (f) {
//			            var form = me.getForm();
//			            if (form.treeNode) {
//			                var treeNodeProps = form.treeNode.data.properties;
//			                var objNam = form.findField('objectName').getValue();
//			                var oldObjNam = treeNodeProps['objectName'];
//			                treeNodeProps['tableName'] = form.findField('tableName').getValue();
//			                treeNodeProps['objectName'] = objNam;
//			                treeNodeProps['keyDelimiter'] = form.findField('keyDelimeter').getValue();

//			                for (var ijk = 0; ijk < dbDict.dataObjects.length; ijk++) {
//			                    var dataObject = dbDict.dataObjects[ijk];
//			                    if (form.treeNode.text.toUpperCase() != dataObject.objectName.toUpperCase())
//			                        continue;
//			                    dataObject.objectName = objNam;
//			                }

//			                form.treeNode.set('title', objNam);
//			                form.treeNode.text = objNam;
//			                form.treeNode.data.text = objNam;
//			                form.treeNode.data.properties.objectName = objNam;

//			                var dsConfigPane = editPane.items.map[scopeName + '.' + appName + '.dsconfigPane'];
//			                var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];
//			                var rootNode = dbObjectsTree.getRootNode();

//			                for (var i = 0; i < rootNode.childNodes.length; i++) {
//			                    var folderNode = rootNode.childNodes[i];
//			                    var folderNodeProp = folderNode.data.properties;
//			                    if (folderNode.childNodes[2])
//			                        var relationFolderNode = folderNode.childNodes[2];
//			                    else
//			                        var relationFolderNode = folderNode.data.children[2];

//			                    if (!relationFolderNode)
//			                        continue;

//			                    if (relationFolderNode.childNodes)
//			                        var relChildenNodes = relationFolderNode.childNodes;
//			                    else
//			                        var relChildenNodes = relationFolderNode.children;

//			                    if (relChildenNodes) {
//			                        for (var k = 0; k < relChildenNodes.length; k++) {
//			                            var relationNode = relChildenNodes[k];

//			                            if (relationNode.text == '')
//			                                continue;

//			                            if (relationNode.attributes.attributes)
//			                                var relationNodeAttr = relationNode.attributes.attributes;
//			                            else
//			                                var relationNodeAttr = relationNode.attributes;

//			                            var relObjNam = relationNodeAttr.relatedObjectName;
//			                            if (relObjNam.toLowerCase() != objNam.toLowerCase() && relObjNam.toLowerCase() == oldObjNam.toLowerCase())
//			                                relationNodeAttr.relatedObjectName = objNam;

//			                            var relatedObjPropMap = relationNodeAttr.relatedObjMap;

//			                            for (var iki = 0; iki < relatedObjPropMap.length; iki++) {
//			                                if (relatedObjPropMap[iki].relatedObjName.toLowerCase() == oldObjNam.toLowerCase())
//			                                    relatedObjPropMap[iki].relatedObjName = objNam;
//			                            }
//			                        }
//			                    }
//			                }

//			                var items = editPane.items.items;

//			                for (var i = 0; i < items.length; i++) {
//			                    var relateObjField = items[i].getForm().findField('relatedObjectName');
//			                    if (relateObjField)
//			                        if (relateObjField.getValue().toLowerCase() == oldObjNam.toLowerCase())
//			                            relateObjField.setValue(objNam);
//			                }
//			            }
//			        }


//			        handler: function (f) {
//			            var form = dataObjectFormPanel.getForm();
//			            if (node.attributes.properties) {
//			                form.findField('objectName').setValue(node.attributes.properties.objectName);
//			                form.findField('keyDelimeter').setValue(node.attributes.properties.keyDelimiter);
//			            }
//			        }

        //        editPane.add(dataObjectFormPanel);
        //        var panelIndex = editPane.items.indexOf(dataObjectFormPanel);
        //        editPane.getLayout().setActiveItem(panelIndex);