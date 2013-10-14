Ext.ns('AdapterManager');

function setKeyProperty(editPane, node, scopeName, appName, dataTypes) {
	if (editPane && node) {
		if (editPane.items.map[scopeName + '.' + appName + '.keyPropertyForm.' + node.id]) {
			var keyPropertyFormPane = editPane.items.map[scopeName + '.' + appName + '.keyPropertyForm.' + node.id];
			if (keyPropertyFormPane) {
				var panelIndex = editPane.items.indexOf(keyPropertyFormPane);
				editPane.getLayout().setActiveItem(panelIndex);
				return;
				//keyPropertyFormPane.destroy();					
			}
		}

		if (node.attributes.properties)
			var properties = node.attributes.properties;
		else
			var properties = node.attributes.attributes.properties;

		var keyPropertyFormPanel = new Ext.FormPanel({
			name: 'keyProperty',
			id: scopeName + '.' + appName + '.keyPropertyForm.' + node.id,
			border: false,
			autoScroll: true,
			monitorValid: true,
			labelWidth: 130,
			bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
			defaults: { anchor: '100%', xtype: 'textfield', allowBlank: false },
			items: [{
				xtype: 'label',
				fieldLabel: 'Key Properties',
				labelSeparator: '',
				itemCls: 'form-title'
			}, {
				name: 'columnName',
				fieldLabel: 'Column Name',
				disabled: true
			}, {
				name: 'propertyName',
				fieldLabel: 'Property Name',
				validationEvent: "blur",
				regex: new RegExp("^[a-zA-Z_][a-zA-Z0-9_]*$"),
				regexText: '<b>Error</b></br>Invalid Value. A valid value should start with alphabet or "_", and follow by any number of "_", alphabet, or number characters'
			}, {
				name: 'dataType',
				xtype: 'combo',
				fieldLabel: 'Data Type',
				store: dataTypes,
				mode: 'local',
				editable: false,
				triggerAction: 'all',
				displayField: 'text',
				valueField: 'value',
				selectOnFocus: true,
				disabled: true
			}, {
				xtype: 'numberfield',
				name: 'dataLength',
				fieldLabel: 'Data Length'
			}, {
				xtype: 'checkbox',
				name: 'isNullable',
				fieldLabel: 'Nullable',
				disabled: true
			}, {
				xtype: 'checkbox',
				name: 'showOnIndex',
				fieldLabel: 'Show on Index'
            }, {
                xtype: 'numberfield',
                name: 'numberOfDecimals',
                fieldLabel: 'Number of Decimals'
            }, {
                xtype: 'checkbox',
                name: 'isHidden',
                fieldLabel: 'Hidden'
            }, {
				xtype: 'combo',
				hiddenName: 'keyType',
				fieldLabel: 'Key Type',
				store: [[1, 'assigned'], [0, 'unassigned']],
				mode: 'local',
				editable: false,
				triggerAction: 'all',
				displayField: 'text',
				valueField: 'value',
				value: properties.keyType
			}],
			treeNode: node,
			tbar: new Ext.Toolbar({
				items: [{
					xtype: 'tbspacer',
					width: 4
				}, {
					xtype: 'tbbutton',
					icon: 'Content/img/16x16/apply.png',
					text: 'Apply',
					tooltip: 'Apply the current changes to the data objects tree',
					handler: function (f) {
						var form = keyPropertyFormPanel.getForm();
						applyProperty(form);
						var treeNodeProps = form.treeNode.attributes.properties;
						treeNodeProps['keyType'] = form.findField('keyType').getValue();
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
						var form = keyPropertyFormPanel.getForm();
						setDataPropertyFields(form, properties);
						form.findField('isNullable').disable();
					}
				}]
			})
		});

		var form = keyPropertyFormPanel.getForm();
		setDataPropertyFields(form, properties);
		editPane.add(keyPropertyFormPanel);
		var panelIndex = editPane.items.indexOf(keyPropertyFormPanel);
		editPane.getLayout().setActiveItem(panelIndex);
	}
}

function applyProperty(form) {
  if (form.treeNode.attributes.attributes)
		var treeNodeProps = form.treeNode.attributes.attributes.properties;
	else
		var treeNodeProps = form.treeNode.attributes.properties;

  var propertyNameField = form.findField('propertyName');
  var propertyName = propertyNameField.getValue();

  if (propertyNameField.validate())
    treeNodeProps['propertyName'] = propertyName;
  else {
    showDialog(400, 100, 'Warning', "Property Name is not valid. A valid property name should start with alphabet or \"_\", and follow by any number of \"_\", alphabet, or number characters", Ext.Msg.OK, null);
    return;
  }
    
	treeNodeProps['columnName'] = form.findField('columnName').getValue();
	treeNodeProps['propertyName'] = propertyName;
	form.treeNode.setText(propertyName);
	treeNodeProps['dataType'] = form.findField('dataType').getValue();
	treeNodeProps['dataLength'] = form.findField('dataLength').getValue();
	treeNodeProps['nullable'] = form.findField('isNullable').getValue();
	treeNodeProps['showOnIndex'] = form.findField('showOnIndex').getValue();
	treeNodeProps['numberOfDecimals'] = form.findField('numberOfDecimals').getValue();
	treeNodeProps['isHidden'] = form.findField('isHidden').getValue();
}

function setDataProperty(editPane, node, scopeName, appName, dataTypes) {
	if (editPane && node) {
		if (editPane.items.map[scopeName + '.' + appName + '.dataPropertyForm.' + node.id]) {
			var dataPropertyFormPane = editPane.items.map[scopeName + '.' + appName + '.dataPropertyForm.' + node.id];
			if (dataPropertyFormPane) {
				var panelIndex = editPane.items.indexOf(dataPropertyFormPane);
				editPane.getLayout().setActiveItem(panelIndex);
				return;
			}
		}

		var dataPropertyFormPanel = new Ext.FormPanel({
			name: 'dataProperty',
			id: scopeName + '.' + appName + '.dataPropertyForm.' + node.id,
			border: false,
			autoScroll: true,
			monitorValid: true,
			labelWidth: 130,
			bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
			defaults: { anchor: '100%', xtype: 'textfield', allowBlank: false },
			items: [{
				xtype: 'label',
				fieldLabel: 'Data Properties',
				labelSeparator: '',
				itemCls: 'form-title'
			}, {
				name: 'columnName',
				fieldLabel: 'Column Name',
				disabled: true
			}, {
				name: 'propertyName',
				fieldLabel: 'Property Name',
				validationEvent: "blur",
				regex: new RegExp("^[a-zA-Z_][a-zA-Z0-9_]*$"),
				regexText: '<b>Error</b></br>Invalid Value. A valid value should start with alphabet or "_", and follow by any number of "_", alphabet, or number characters'
			}, {
				name: 'dataType',
				xtype: 'combo',
				fieldLabel: 'Data Type',
				store: dataTypes,
				mode: 'local',
				editable: false,
				triggerAction: 'all',
				displayField: 'text',
				valueField: 'value',
				selectOnFocus: true,
				disabled: true
			}, {
				xtype: 'numberfield',
				name: 'dataLength',
				fieldLabel: 'Data Length'
			}, {
				xtype: 'checkbox',
				name: 'isNullable',
				fieldLabel: 'Nullable',
				disabled: true
			}, {
				xtype: 'checkbox',
				name: 'showOnIndex',
				fieldLabel: 'Show on Index'
            }, {
				xtype: 'numberfield',
				name: 'numberOfDecimals',
				fieldLabel: 'Number of Decimals'
			}, {
                xtype: 'checkbox',
                name: 'isHidden',
                fieldLabel: 'Hidden'
            }],
			treeNode: node,
			tbar: new Ext.Toolbar({
				items: [{
					xtype: 'tbspacer',
					width: 4
				}, {
					xtype: 'tbbutton',
					icon: 'Content/img/16x16/apply.png',
					text: 'Apply',
					tooltip: 'Apply the current changes to the data objects tree',
					handler: function (f) {
						var form = dataPropertyFormPanel.getForm();
						if (form.treeNode)
							applyProperty(form);
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
						var form = dataPropertyFormPanel.getForm();
						setDataPropertyFields(form, node.attributes.properties);
					}
				}]
			})
		});
		var form = dataPropertyFormPanel.getForm();
		setDataPropertyFields(form, node.attributes.properties);
		editPane.add(dataPropertyFormPanel);
		var panelIndex = editPane.items.indexOf(dataPropertyFormPanel);
		editPane.getLayout().setActiveItem(panelIndex);
	}
}

function setDataPropertyFields(form, properties) {
	if (form && properties) {
		form.findField('columnName').setValue(properties.columnName);
		form.findField('propertyName').setValue(properties.propertyName);
		form.findField('dataType').setValue(properties.dataType);
		form.findField('dataLength').setValue(properties.dataLength);

		if (properties.nullable)
			if (properties.nullable.toString().toLowerCase() == 'true') {
				form.findField('isNullable').setValue(true);
			}
			else {
				form.findField('isNullable').setValue(false);
			}
		else
			form.findField('isNullable').setValue(false);

		if (properties.showOnIndex.toString().toLowerCase() == 'true') {
			form.findField('showOnIndex').setValue(true);
		}
		else {
			form.findField('showOnIndex').setValue(false);
        }

        if (properties.isHidden.toString().toLowerCase() == 'true') {
            form.findField('isHidden').setValue(true);
        }
        else {
            form.findField('isHidden').setValue(false);
        }       

		form.findField('numberOfDecimals').setValue(properties.numberOfDecimals);
	}
}

function setDataObject(editPane, node, dbDict, dataObjectsPane, scopeName, appName) {
	if (editPane && node) {
		if (editPane.items.map[scopeName + '.' + appName + '.objectNameForm.' + node.id]) {
			var objectNameFormPane = editPane.items.map[scopeName + '.' + appName + '.objectNameForm.' + node.id];
			if (objectNameFormPane) {
				var panelIndex = editPane.items.indexOf(objectNameFormPane);
				editPane.getLayout().setActiveItem(panelIndex);
				return;
			}
		}

		if (!node.attributes.properties.objectNamespace)
			node.attributes.properties.objectNamespace = "org.iringtools.adapter.datalayer.proj_" + scopeName + "." + appName;

		if (node.attributes.properties.keyDelimiter == null || !node.attributes.properties.keyDelimiter || node.attributes.properties.keyDelimiter == "null")
	    node.attributes.properties.keyDelimiter = '_';

	var dataObjectFormPanel = new Ext.FormPanel({
	    name: 'dataObject',
	    id: scopeName + '.' + appName + '.objectNameForm.' + node.id,
	    border: false,
	    autoScroll: true,
	    monitorValid: true,
	    labelWidth: 160,
	    bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
	    defaults: { anchor: '100%', xtype: 'textfield', allowBlank: false },
	    items: [{
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
                xtype: 'hidden',
	        name: 'objectNamespace',
	        value: node.attributes.properties.objectNamespace            
	    }, {
	        name: 'objectName',
	        fieldLabel: 'Object Name',
          validationEvent: "blur",
				  regex: new RegExp("^[a-zA-Z_][a-zA-Z0-9_]*$"),
				  regexText: '<b>Error</b></br>Invalid Value. A valid value should start with alphabet or "_", and follow by any number of "_", alphabet, or number characters',
	        value: node.attributes.properties.objectName
	    }, {
	        name: 'keyDelimeter',
	        fieldLabel: 'Key Delimiter',
	        value: node.attributes.properties.keyDelimiter,
	        allowBlank: true
	    }, {
	        name: 'description',
	        xtype: 'textarea',
            height: 150,
	        fieldLabel: 'Description',
	        value: node.attributes.properties.description,
	        allowBlank: true
	    }],
	    treeNode: node,
	    tbar: new Ext.Toolbar({
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
	                  var objectNameField = form.findField('objectName');
	                  var objNam = objectNameField.getValue();

	                  if (objectNameField.validate())
	                    treeNodeProps['objectName'] = objNam;
	                  else {
	                    showDialog(400, 100, 'Warning', "Object Name is not valid. A valid object name should start with alphabet or \"_\", and follow by any number of \"_\", alphabet, or number characters", Ext.Msg.OK, null);
	                    return;
	                  }

	                  var oldObjNam = treeNodeProps['objectName'];
	                  treeNodeProps['tableName'] = form.findField('tableName').getValue();
	                  treeNodeProps['objectName'] = objNam;
	                  treeNodeProps['keyDelimiter'] = form.findField('keyDelimeter').getValue();
	                  treeNodeProps['description'] = form.findField('description').getValue();

	                  for (var ijk = 0; ijk < dbDict.dataObjects.length; ijk++) {
	                      var dataObject = dbDict.dataObjects[ijk];
	                      if (form.treeNode.text.toUpperCase() != dataObject.objectName.toUpperCase())
	                          continue;
	                      dataObject.objectName = objNam;
	                  }

	                  form.treeNode.setText(objNam);
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
	                    form.findField('description').setValue(node.attributes.properties.description);
	                }
	            }
	        }]
	    })
	});
		editPane.add(dataObjectFormPanel);
		var panelIndex = editPane.items.indexOf(dataObjectFormPanel);
		editPane.getLayout().setActiveItem(panelIndex);
	}
}

function setItemSelectorAvailValues(node) {
	var availItems = new Array();
	var propertiesNode = node.parentNode.childNodes[1];

	for (var i = 0; i < propertiesNode.childNodes.length; i++) {
		var itemName = propertiesNode.childNodes[i].text;
		var found = false;

		for (var j = 0; j < node.childNodes.length; j++) {
			if (node.childNodes[j].text.toLowerCase() == itemName.toLowerCase()) {
				found = true;
				break;
			}
		}

		if (!found) {
			availItems.push([itemName, itemName]);
		}
	}
	return availItems;
}

function setItemSelectorselectedValues(node) {
	var selectedItems = new Array();
	var propertiesNode = node.parentNode.childNodes[1];

	for (var i = 0; i < node.childNodes.length; i++) {
		var keyName = node.childNodes[i].text;
		selectedItems.push([keyName, keyName]);
	}
	return selectedItems;
}

function setKeysFolder(editPane, node, scopeName, appName) {
	if (editPane && node) {
		if (editPane.items.map[scopeName + '.' + appName + '.keysSelector.' + node.id]) {
			var keysSelectorPane = editPane.items.map[scopeName + '.' + appName + '.keysSelector.' + node.id];
			if (keysSelectorPane) {				
				//var panelIndex = editPane.items.indexOf(keysSelectorPane);
				//editPane.getLayout().setActiveItem(panelIndex);
				//return;
				keysSelectorPane.destroy();
			}
		}

		var availItems = setItemSelectorAvailValues(node);
		var selectedItems = setItemSelectorselectedValues(node);

		var keysItemSelector = new Ext.ux.ItemSelector({
			bodyStyle: 'background:#eee',
			name: 'keySelector',
			frame: true,
			imagePath: 'scripts/ext-3.3.1/examples/ux/images/',
			hideLabel: true,

			multiselects: [{
				width: 240,
				height: 370,
				border: 0,
				legend: 'Available Keys',				
				store: availItems,
				displayField: 'keyName',
				valueField: 'keyValue'
			}, {
				width: 240,
				height: 370,
				border: 0,
				legend: 'Selected Keys',
				store: selectedItems,
				displayField: 'keyName',
				valueField: 'keyValue'
			}],
			treeNode: node
		});

		var keysSelectorPanel = new Ext.FormPanel({
		    id: scopeName + '.' + appName + '.keysSelector.' + node.id,
		    border: false,
		    autoScroll: true,
		    bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
		    labelWidth: 160,
		    defaults: { anchor: '100%' },
		    items: [{
		        xtype: 'label',
		        fieldLabel: 'Select Keys',
		        itemCls: 'form-title',
		        labelSeparator: ''
		    }, keysItemSelector],
		    tbar: new Ext.Toolbar({
		        items: [{
		            xtype: 'tbspacer',
		            width: 4
		        }, {
		            xtype: 'tbbutton',
		            icon: 'Content/img/16x16/apply.png',
		            text: 'Apply',
		            tooltip: 'Apply the current changes to the data objects tree',
		            handler: function (f) {
		                var selectedValues = keysItemSelector.toMultiselect.store.data.items;
		                var keysNode = keysItemSelector.treeNode;
		                var propertiesNode = keysNode.parentNode.childNodes[1];

		                for (var i = 0; i < keysNode.childNodes.length; i++) {
		                    var found = false;

		                    for (var j = 0; j < selectedValues.length; j++) {
		                        if (selectedValues[j].data.text.toLowerCase() == keysNode.childNodes[i].text.toLowerCase()) {
		                            found = true;
		                            break;
		                        }
		                    }

		                    if (!found) {
		                        if (keysNode.childNodes[i].attributes.properties)
		                            var properties = keysNode.childNodes[i].attributes.properties;
		                        else if (keysNode.childNodes[i].attributes.attributes.properties)
		                            var properties = keysNode.childNodes[i].attributes.attributes.properties;

		                        if (properties) {
		                            properties['isNullable'] = true;
		                            delete properties.keyType;

		                            var propertyNode = new Ext.tree.TreeNode({
		                                text: keysNode.childNodes[i].text,
		                                type: "dataProperty",
		                                leaf: true,
		                                iconCls: 'treeProperty',
		                                properties: properties
		                            });

		                            propertyNode.iconCls = 'treeProperty';
		                            propertiesNode.appendChild(propertyNode);
		                            keysNode.removeChild(keysNode.childNodes[i], true);
		                            i--;
		                        }
		                    }
		                }

		                var nodeChildren = new Array();
		                for (var j = 0; j < keysNode.childNodes.length; j++)
		                    nodeChildren.push(keysNode.childNodes[j].text);

		                for (var j = 0; j < selectedValues.length; j++) {
		                    var found = false;
		                    for (var i = 0; i < nodeChildren.length; i++) {
		                        if (selectedValues[j].data.text.toLowerCase() == nodeChildren[i].toLowerCase()) {
		                            found = true;
		                            break;
		                        }
		                    }

		                    if (!found) {
		                        var newKeyNode;

		                        for (var jj = 0; jj < propertiesNode.childNodes.length; jj++) {
		                            if (propertiesNode.childNodes[jj].text.toLowerCase() == selectedValues[j].data.text.toLowerCase()) {
		                                var properties = propertiesNode.childNodes[jj].attributes.properties;
		                                properties.keyType = 'assigned';
		                                properties.nullable = false;

		                                newKeyNode = new Ext.tree.TreeNode({
		                                    text: selectedValues[j].data.text,
		                                    type: "keyProperty",
		                                    leaf: true,
		                                    iconCls: 'treeKey',
		                                    hidden: false,
		                                    properties: properties
		                                });
		                                newKeyNode.iconCls = 'treeKey';
		                                propertiesNode.removeChild(propertiesNode.childNodes[jj], true);
		                                jj--;
		                            }
		                        }

		                        if (newKeyNode) {
		                            keysNode.appendChild(newKeyNode);
		                            if (keysNode.expanded == false)
		                                keysNode.expand();
		                        }
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
		                var availItems = setItemSelectorAvailValues(node);
		                var selectedItems = setItemSelectorselectedValues(node);
		                if (keysItemSelector.fromMultiselect.store.data) {
		                    keysItemSelector.fromMultiselect.reset();
		                    keysItemSelector.fromMultiselect.store.removeAll();
		                }

		                keysItemSelector.fromMultiselect.store.loadData(availItems);
		                keysItemSelector.fromMultiselect.store.commitChanges();

		                if (keysItemSelector.toMultiselect.store.data) {
		                    keysItemSelector.toMultiselect.reset();
		                    keysItemSelector.toMultiselect.store.removeAll();
		                }

		                keysItemSelector.toMultiselect.store.loadData(selectedItems);
		                keysItemSelector.toMultiselect.store.commitChanges();
		            }
		        }]
		    })
		});
		editPane.add(keysSelectorPanel);
		var panelIndex = editPane.items.indexOf(keysSelectorPanel);
		editPane.getLayout().setActiveItem(panelIndex);
	}
}

function setPropertiesFolder(editPane, node, scopeName, appName) {
	if (editPane && node) {
		if (editPane.items.map[scopeName + '.' + appName + '.propertiesSelector.' + node.id]) {
			var propertiesSelectorPane = editPane.items.map[scopeName + '.' + appName + '.propertiesSelector.' + node.id];
			if (propertiesSelectorPane) {
				propertiesSelectorPane.destroy();
			}
		}

        var shownProperty = new Array();

        for (var indexOfProperty = 0; indexOfProperty < node.childNodes.length; indexOfProperty++)
            if (!node.childNodes[indexOfProperty].hidden) {
                !hasShown(shownProperty, node.childNodes[indexOfProperty].text)
                shownProperty.push(node.childNodes[indexOfProperty].text);
                indexOfProperty++;
            }

		var availItems = new Array();
		var selectedItems = new Array();
		for (var i = 0; i < node.childNodes.length; i++) {
			var itemName = node.childNodes[i].text;
			if (node.childNodes[i].hidden == false)
				selectedItems.push([itemName, itemName]);
			else
				availItems.push([itemName, itemName]);
		}

		var propertiesItemSelector = new Ext.ux.ItemSelector({
			bodyStyle: 'background:#eee',
			name: 'propertySelector',
			frame: true,
			imagePath: 'scripts/ext-3.3.1/examples/ux/images/',			
			hideLabel: true,

			multiselects: [{
				width: 240,
				height: 370,
				border: 0,
				legend: 'Available Properties',
				store: availItems,
				displayField: 'propertyName',
				valueField: 'propertyValue'				
			}, {
				width: 240,
				height: 370,
				border: 0,
				legend: 'Selected Properties',
				store: selectedItems,
				displayField: 'propertyName',
				valueField: 'propertyValue'				
			}],
			treeNode: node,
			shownProperty: shownProperty
		});

		var propertiesSelectorPanel = new Ext.FormPanel({
		  id: scopeName + '.' + appName + '.propertiesSelector.' + node.id,
		  border: false,
		  autoScroll: true,
		  bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
		  labelWidth: 160,
		  defaults: { anchor: '100%' },
		  items: [{
		    xtype: 'label',
		    fieldLabel: 'Select Properties',
		    itemCls: 'form-title',
		    labelSeparator: ''
		  }, propertiesItemSelector],
		  tbar: new Ext.Toolbar({
		    items: [{
		      xtype: 'tbspacer',
		      width: 4
		    }, {
		      xtype: 'tbbutton',
		      icon: 'Content/img/16x16/apply.png',
		      text: 'Apply',
		      tooltip: 'Apply the current changes to the data objects tree',
		      handler: function (f) {
		        var selectedValues = propertiesItemSelector.toMultiselect.store.data.items;
		        var treeNode = propertiesItemSelector.treeNode;
		        var shownProperty = propertiesItemSelector.shownProperty;

		        for (var i = 0; i < treeNode.childNodes.length; i++) {
		          var found = false;

		          for (var j = 0; j < selectedValues.length; j++) {
		            if (selectedValues[j].data.text.toLowerCase() == treeNode.childNodes[i].text.toLowerCase()) {
		              found = true;
		              break;
		            }
		          }

		          if (!found)
		            treeNode.childNodes[i].getUI().hide();
		          else if (!hasShown(shownProperty, treeNode.childNodes[i].text)) {
		            treeNode.childNodes[i].getUI().show();
                if (treeNode.childNodes[i].attributes.hidden)
		              treeNode.childNodes[i].attributes.hidden = false;
		            shownProperty.push(treeNode.childNodes[i].text);
		          }

		          if (treeNode.expanded == false)
		            treeNode.expand();
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
		        var availProps = new Array();
		        var selectedProps = new Array();
		        var availPropsSingle = new Array();
		        var toPropsSingle = new Array();
		        var firstAvailProps = new Array();
		        var firstToProps = new Array();
		        for (var i = 0; i < node.childNodes.length; i++) {
		          var itemName = node.childNodes[i].text;
		          if (node.childNodes[i].hidden == false) {
		            selectedProps.push([itemName, itemName]);
		            toPropsSingle.push(itemName);
		          }
		          else {
		            availProps.push([itemName, itemName]);
		            availPropsSingle.push(itemName);
		          }
		        }

		        if (availProps[0]) {
		          firstAvailProps.push(availProps[0]);

		          if (propertiesItemSelector.fromMultiselect.store.data)
		            removeSelectorStore(propertiesItemSelector.fromMultiselect);

		          loadSelectorStore(propertiesItemSelector.fromMultiselect, firstAvailProps);
		          var firstAvailPropsItems = propertiesItemSelector.fromMultiselect.store.data.items;
		          var loadSingle = false;
		          var availPropName = firstAvailPropsItems[0].data.text;

		          if (availPropName[1])
		            if (availPropName[1].length > 1)
		              var loadSingle = true;

		          if (!loadSingle)
		            setSelectorStore(propertiesItemSelector.fromMultiselect, availProps);
		          else
		            setSelectorStore(propertiesItemSelector.fromMultiselect, availPropsSingle);
		        }
		        else
		          cleanSelectorStore(propertiesItemSelector.fromMultiselect);

		        if (selectedProps[0]) {
		          firstToProps.push(selectedProps[0]);

		          if (propertiesItemSelector.toMultiselect.store.data)
		            removeSelectorStore(propertiesItemSelector.toMultiselect);

		          loadSelectorStore(propertiesItemSelector.toMultiselect, firstToProps);
		          var firstToPropsItems = propertiesItemSelector.toMultiselect.store.data.items;
		          var loadSingle = false;
		          var toPropName = firstToPropsItems[0].data.text;

		          if (toPropName[1])
		            if (toPropName[1].length > 1)
		              var loadSingle = true;

		          if (!loadSingle)
		            setSelectorStore(propertiesItemSelector.toMultiselect, selectedProps);
		          else
		            setSelectorStore(propertiesItemSelector.toMultiselect, toPropsSingle);
		        }
		        else
		          cleanSelectorStore(propertiesItemSelector.toMultiselect);
		      }
		    }]
		  })
		});

		editPane.add(propertiesSelectorPanel);
		var panelIndex = editPane.items.indexOf(propertiesSelectorPanel);
		editPane.getLayout().setActiveItem(panelIndex);
	}
}

function hasShown(shownArray, text)
{
    for (var shownIndex = 0; shownIndex < shownArray.length; shownIndex++)
        if (shownArray[shownIndex] == text)
            return true;
    return false;
};

function removeSelectorStore(selector) {
	selector.reset();
	selector.store.removeAll();
};

function loadSelectorStore(selector, storeData) {
	selector.store.loadData(storeData);
	selector.store.commitChanges();
};

function setSelectorStore(selector, storeData) {
	removeSelectorStore(selector);
	loadSelectorStore(selector, storeData);
};

function cleanSelectorStore(selector) {
	removeSelectorStore(selector);
	selector.store.commitChanges();
};

