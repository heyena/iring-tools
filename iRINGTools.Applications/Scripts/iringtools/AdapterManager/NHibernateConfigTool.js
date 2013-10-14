Ext.ns('AdapterManager');

AdapterManager.NHibernateConfigWizard = Ext.extend(Ext.Container, {
    scope: null,
    app: null,
    iconCls: 'tabsNhibernate',
    border: false,
    frame: false,

    constructor: function (config) {
        config = config || {};

        var wizard = this;
        var scopeName = config.scope;
        var appName = config.app;
        var dbDict;
        var dbInfo;
        var userTableNames;
        var dataTypes = null;
        var dataObjectsPane = new Ext.Panel({
            layout: 'border',
            id: scopeName + '.' + appName + '.dataObjectsPane',
            frame: false,
            border: false,
            items: [{
                xtype: 'panel',
                name: 'data-objects-pane',
                region: 'west',
                minWidth: 240,
                width: 300,
                split: true,
                layout: 'border',
                bodyStyle: 'background:#fff',
                items: [{
                    xtype: 'treepanel',
                    border: false,
                    autoScroll: true,
                    animate: true,
                    region: 'center',
                    lines: true,
                    frame: false,
                    enableDD: false,
                    containerScroll: true,
                    rootVisible: true,
                    root: {
                        text: 'Data Objects',
                        nodeType: 'async',
                        iconCls: 'folder'
                    },
                    loader: new Ext.tree.TreeLoader(),
                    tbar: new Ext.Toolbar({
                        items: [{
                            xtype: 'tbspacer',
                            width: 4
                        }, {
                            xtype: 'button',
                            icon: 'Content/img/16x16/view-refresh.png',
                            text: 'Reload',
                            tooltip: 'Reload Data Objects',
                            handler: function () {
                                var editPane = dataObjectsPane.items.items[1];
                                var items = editPane.items.items;

                                for (var i = 0; i < items.length; i++) {
                                    items[i].destroy();
                                    i--;
                                }

                                Ext.Ajax.request({
                                    url: 'AdapterManager/DBDictionary',
                                    timeout: 600000,
                                    method: 'POST',
                                    params: {
                                        scope: scopeName,
                                        app: appName
                                    },
                                    success: function (response, request) {
                                        dbDict = Ext.util.JSON.decode(response.responseText);
                                        if (dbDict.ConnectionString)
                                            dbDict.ConnectionString = Base64.decode(dbDict.ConnectionString);

                                        var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];

                                        if (dbDict.dataObjects.length > 0) {
                                            // populate data source form
                                            dbInfo = showTree(dbObjectsTree, dbInfo, dbDict, scopeName, appName, dataObjectsPane);
                                        }
                                        else {
                                            dbObjectsTree.disable();
                                            editPane = dataObjectsPane.items.items[1];
                                            if (!editPane) {
                                                var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
                                            }

                                            var dbTables = setDsConfigPane(editPane, dbInfo, dbDict, scopeName, appName, dataObjectsPane);

                                            if (dbTables)
                                                dbInfo.dbTableNames = dbTables;
                                        }
                                    },
                                    failure: function (response, request) {
                                        editPane = dataObjectsPane.items.items[1];
                                        if (!editPane) {
                                            var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
                                        }
                                        var dbTables = setDsConfigPane(editPane, dbInfo, dbDict, scopeName, appName, dataObjectsPane);

                                        if (dbTables)
                                            dbInfo.dbTableNames = dbTables;

                                        editPane.getLayout().setActiveItem(editPane.items.length - 1);
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
                                editPane = dataObjectsPane.items.items[1];
                                if (!editPane) {
                                    var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
                                }

                                var dbTables = setDsConfigPane(editPane, dbInfo, dbDict, scopeName, appName, dataObjectsPane);
                                
                                if (dbTables)
                                    dbInfo.dbTableNames = dbTables;
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
                                editPane = dataObjectsPane.items.items[1];

                                if (!editPane) {
                                    var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
                                }

                                var dsConfigPane = editPane.items.map[scopeName + '.' + appName + '.dsconfigPane'];
                                var tablesSelectorPane = editPane.items.map[scopeName + '.' + appName + '.tablesSelectorPane'];
                                var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];
                                var rootNode = dbObjectsTree.getRootNode();
                                var treeProperty = getTreeJson(dsConfigPane, rootNode, dbInfo, dbDict, dataTypes, tablesSelectorPane);

                                Ext.Ajax.request({
                                    url: 'AdapterManager/Trees',
                                    timeout: 600000,
                                    method: 'POST',
                                    params: {
                                        scope: scopeName,
                                        app: appName,
                                        tree: JSON.stringify(treeProperty)
                                    },
                                    success: function (response, request) {
                                        var rtext = response.responseText;
                                        var error = 'SUCCESS = FALSE';
                                        var index = rtext.toUpperCase().indexOf(error);
                                        if (index == -1) {
                                            showDialog(400, 100, 'Saving Result', 'Configuration has been saved successfully.', Ext.Msg.OK, null);
                                            var navpanel = Ext.getCmp('nav-panel');
                                            navpanel.onReload();
                                        }
                                        else {
                                            var msg = rtext.substring(index + error.length + 2, rtext.length - 1);
                                            showDialog(400, 100, 'Saving Result - Error', msg, Ext.Msg.OK, null);
                                        }
                                    },
                                    failure: function (response, request) {
                                        showDialog(660, 300, 'Saving Result', 'An error has occurred while saving the configuration.', Ext.Msg.OK, null);
                                    }
                                });
                            }
                        }]
                    }),
                    listeners: {
                        click: function (node, e) {
                            if (node.isRoot) {
                                editPane = dataObjectsPane.items.items[1];
                                if (!editPane) {
                                    var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
                                }

                                if (dbInfo.dbTableNames != null)
                                    setTablesSelectorPane(editPane, dbInfo, dbDict, scopeName, appName, dataObjectsPane);
                                return;
                            }
                            else if (!node)
                                return;

                            var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
                            if (!editPane)
                                editPane = dataObjectsPane.items.items[1];

                            var nodeType = node.attributes.type;


                            if (!nodeType && node.attributes.attributes)
                                nodeType = node.attributes.attributes.type;


                            if (nodeType) {
                                editPane.show();
                                var editPaneLayout = editPane.getLayout();

                                switch (nodeType.toUpperCase()) {
                                    case 'DATAOBJECT':
                                        setDataObject(editPane, node, dbDict, dataObjectsPane, scopeName, appName);
                                        break;

                                    case 'KEYS':
                                        setKeysFolder(editPane, node, scopeName, appName);
                                        break;

                                    case 'KEYPROPERTY':
                                        setKeyProperty(editPane, node, scopeName, appName, dataTypes);
                                        break;

                                    case 'PROPERTIES':
                                        setPropertiesFolder(editPane, node, scopeName, appName);
                                        break;

                                    case 'DATAPROPERTY':
                                        setDataProperty(editPane, node, scopeName, appName, dataTypes);
                                        break;

                                    case 'RELATIONSHIPS':
                                        setRelations(editPane, node, scopeName, appName);
                                        break;

                                    case 'RELATIONSHIP':
                                        setRelationFields(editPane, node, scopeName, appName);
                                        break;
                                }
                            }
                            else {
                                editPane.hide();
                            }
                        }
                    }
                }]
            }, {
                xtype: 'panel',
                name: 'editor-panel',
                border: 1,
                frame: false,
                id: scopeName + '.' + appName + '.editor-panel',
                region: 'center',
                layout: 'card'
            }]
        });


        Ext.apply(this, {
            id: scopeName + '.' + appName + '.-nh-config',
            title: 'NHibernate Configuration - ' + scopeName + '.' + appName,
            closable: true,
            border: false,
            frame: true,
            layout: 'fit',
            items: [dataObjectsPane]
        });

        Ext.Ajax.request({
            url: 'AdapterManager/DataType',
            timeout: 600000,
            method: 'GET',

            success: function (response, request) {
                var dataTypeName = Ext.util.JSON.decode(response.responseText);
                dataTypes = new Array();
                var i = 0;
                while (!dataTypeName[i])
                    i++;
                while (dataTypeName[i]) {
                    dataTypes.push([i, dataTypeName[i]]);
                    i++;
                }
            },
            failure: function (f, a) {
                if (a.response)
                    showDialog(500, 400, 'Error', a.response.responseText, Ext.Msg.OK, null);
            }
        });

        Ext.EventManager.onWindowResize(this.doLayout, this);

        Ext.Ajax.request({
            url: 'AdapterManager/DBDictionary',
            timeout: 600000,
            method: 'POST',
            params: {
                scope: scopeName,
                app: appName
            },
            success: function (response, request) {
                dbDict = Ext.util.JSON.decode(response.responseText);
                if (dbDict.ConnectionString)
                    dbDict.ConnectionString = Base64.decode(dbDict.ConnectionString);

                var tab = Ext.getCmp('content-panel');
                var rp = tab.items.map[scopeName + '.' + appName + '.-nh-config'];
                var dataObjectsPane = rp.items.map[scopeName + '.' + appName + '.dataObjectsPane'];
                var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];

                if (dbDict.ConnectionString != null) {
                    // populate data source form
                    dbInfo = showTree(dbObjectsTree, dbInfo, dbDict, scopeName, appName, dataObjectsPane);
                    var abcdd = 5;
                }
                else {
                    dbObjectsTree.disable();
                    editPane = dataObjectsPane.items.items[1];
                    if (!editPane) {
                        var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
                    }
                    if (!dbInfo)
                        dbInfo = {};

                    var dbTables = setDsConfigPane(editPane, dbInfo, dbDict, scopeName, appName, dataObjectsPane);
                   
                    if (dbTables)
                        dbInfo.dbTableNames = dbTables;
                }
            },
            failure: function (response, request) {
                editPane = dataObjectsPane.items.items[1];
                if (!editPane) {
                    var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
                }
                editPane.add(dsConfigPane);
                editPane.getLayout().setActiveItem(editPane.items.length - 1);
            }
        });

        AdapterManager.NHibernateConfigWizard.superclass.constructor.apply(this, arguments);
    }
});

RadioField = Ext.extend(Ext.Panel, {
	value: null,
	serName: null,

	constructor: function (config) {
		RadioField.superclass.constructor.call(this);
		Ext.apply(this, config);

		this.bodyStyle = 'background:#eee';

		this.radioGroup = new Ext.form.RadioGroup({
			columns: 1,
			items: [{
				name: 'sid',
				inputValue: 0,
				style: 'margin-top: 4px'
			}, {
				name: 'sid',
				inputValue: 1,
				style: 'margin-top: 4px'
			}]
		});

		var that = this;
		this.field1 = new Ext.form.TextField({
			disabled: true,
			allowBlank: false,
			fieldLabel: 'Sid',
			value: this.value,
			name: 'field_sid',
			listeners: {
				'change': function (field, newValue, oldValue) {
					that.value = newValue.toUpperCase();
				}
			}
		});

		this.field2 = new Ext.form.TextField({
			disabled: true,
			allowBlank: false,
			fieldLabel: 'Service Name',
			value: this.value,
			name: 'field_serviceName',
			listeners: {
				'change': function (field, newValue, oldValue) {
					that.value = newValue.toUpperCase();
				}
			}
		});

		if (this.serName != '') {
			if (this.serName.toUpperCase() == 'SID') {
				this.field1.disabled = false;
				this.field2.disabled = true;

				this.field2.value = '';
				this.radioGroup.items[0].checked = true;
			}
			else {
				this.field1.disabled = true;
				this.field1.value = '';
				this.field2.disabled = false;
				this.radioGroup.items[1].checked = true;
			}
		}

		this.layout = 'column';
		this.border = false;
		this.frame = false;

		this.add([{
			width: 40,
			layout: 'form',
			labelWidth: 0.1,
			items: this.radioGroup,
			border: false,
			frame: false,
			bodyStyle: 'background:#eee'
		}, {
			columnWidth: 1,
			layout: 'form',
			labelWidth: 110,
			defaults: { anchor: '100%', allowBlank: false },
			items: [this.field1, this.field2],
			border: false,
			frame: false,
			bodyStyle: 'background:#eee'
		}]);

		this.subscribeEvents();
	},
	subscribeEvents: function () {
		this.radioGroup.on('change', this.toggleState, this);
	},
	toggleState: function (e, changed) {
		if (changed) {
			var value = this.radioGroup.getValue().inputValue;
			if (value == 0) {
				this.field2.disable();
				this.field2.clearInvalid();
				this.field1.enable();
				this.field1.focus();
				this.serName = 'SID';
			}
			else {
				this.field1.clearInvalid();
				this.field1.disable();
				this.field2.enable();
				this.field2.focus();
				this.serName = 'SERVICE_NAME';
			}
		}
	}
});

Ext.reg('radiofield', RadioField);

function creatRadioField(panel, idLabel, value, serName) {
	if (panel.items) {
		var radioPane = panel.items.map[idLabel + 'radioField'];
		if (radioPane) {
			radioPane.destroy();
		}
	}

	var radioField = new RadioField({
		id: idLabel + 'radioField',
		value: value,
		serName: serName
	});

	panel.add(radioField);
	panel.doLayout();
}

function showDialog(width, height, title, msg, buttons, callback) {
  while (msg.indexOf('\\r\\n') != -1)
    msg = msg.replace('\\r\\n', ' \r\n');  
  
	var style = 'style="margin:0;padding:0;width:' + width + 'px;height:' + height + 'px;border:1px solid #aaa;overflow:auto"';
	Ext.Msg.show({
		title: title,
		msg: '<textarea ' + style + ' readonly="yes">' + msg + '</textarea>',
		buttons: buttons,
		fn: callback
	});
}

