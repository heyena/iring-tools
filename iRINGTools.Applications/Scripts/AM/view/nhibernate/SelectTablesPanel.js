Ext.define('AM.view.nhibernate.SelectTablesPanel', {
    extend: 'Ext.form.Panel',
    alias: 'widget.selecttables',   
    frame: false,
	border: false,
	autoScroll: true,	
	bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
	labelWidth: 140,
	monitorValid: true,

	initComponent: function () {
	    this.items = [{
	        xtype: 'label',
	        fieldLabel: 'Select Tables',
	        labelSeparator: '',
	        itemCls: 'form-title'
	    }, {
	        xtype: 'itemselector',
	        hideLabel: true,
	        bodyStyle: 'background:#eee',
	        frame: true,
	        name: 'tableSelector',
	        imagePath: 'scripts/ext-3.3.1/examples/ux/images/',
	        multiselects: [{
	            width: 240,
	            height: 370,
	            store: availItems,
	            displayField: 'tableName',
	            valueField: 'tableValue',
	            border: 0
	        }, {
	            width: 240,
	            height: 370,
	            store: selectItems,
	            displayField: 'tableName',
	            valueField: 'tableValue',
	            border: 0
	        }],
	        listeners: {
	            change: function (itemSelector, selectedValuesStr) {
	                var selectTables = itemSelector.toMultiselect.store.data.items;
	                for (var i = 0; i < selectTables.length; i++) {
	                    var selectTableName = selectTables[i].data.text;
	                    if (selectTableName == '')
	                        itemSelector.toMultiselect.store.removeAt(i);
	                }

	                var availTables = itemSelector.fromMultiselect.store.data.items;
	                for (var i = 0; i < availTables.length; i++) {
	                    var availTableName = availTables[i].data.text
	                    if (availTables[i].data.text == '')
	                        itemSelector.fromMultiselect.store.removeAt(i);
	                }
	            }
	        }
	    }];

	    this.tbar = new Ext.Toolbar({
	        items: [{
	            xtype: 'tbspacer',
	            width: 4
	        }, {
	            xtype: 'tbbutton',
	            icon: 'Content/img/16x16/apply.png',
	            text: 'Apply',
	            tooltip: 'Apply the current changes to the data objects tree',
	            handler: function () {
	                var editPane = dataObjectsPane.items.items[1];
	                var dsConfigPane = editPane.items.map[scopeName + '.' + appName + '.dsconfigPane'];
	                var tablesSelectorPane = editPane.items.map[scopeName + '.' + appName + '.tablesSelectorPane'];
	                var tablesSelForm = tablesSelectorPane.getForm();
	                var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];
	                var serName = '';
	                var serviceName = '';

	                if (dbObjectsTree.disabled) {
	                    dbObjectsTree.enable();
	                }

	                if (dsConfigPane) {
	                    var serviceNamePane = dsConfigPane.items.items[10];
	                    if (serviceNamePane.items.items[0])
	                        serName = serviceNamePane.items.items[0].serName;
	                }
	                else {
	                    if (dbInfo.serName)
	                        serName = dbInfo.serName;
	                }

	                var treeLoader = dbObjectsTree.getLoader();
	                if (tablesSelForm.findField('tableSelector').getValue().indexOf('') == -1)
	                    var selectTableNames = tablesSelForm.findField('tableSelector').getValue();
	                else {
	                    var tableNames = tablesSelForm.findField('tableSelector').toMultiselect.store.data.items;
	                    var selectTableNames = new Array();
	                    for (var i = 0; i < tableNames.length; i++) {
	                        selectTableNames.push(tableNames[i].data.text);
	                    }
	                }

	                if (selectTableNames.length < 1) {
	                    var rootNode = dbObjectsTree.getRootNode();
	                    while (rootNode.firstChild) {
	                        rootNode.removeChild(rootNode.firstChild);
	                    }
	                    return;
	                }

	                userTableNames = new Array();

	                if (selectTableNames[1]) {
	                    if (selectTableNames[1].length > 1 && selectTableNames[0].length > 1) {
	                        for (var i = 0; i < selectTableNames.length; i++) {
	                            userTableNames.push(selectTableNames[i]);
	                        }
	                    }
	                    else {
	                        userTableNames.push(selectTableNames)
	                    }
	                }
	                else {
	                    userTableNames.push(selectTableNames[0]);
	                }

	                treeLoader.dataUrl = 'AdapterManager/DBObjects';
	                if (dsConfigPane) {
	                    var dsConfigForm = dsConfigPane.getForm();
	                    treeLoader.baseParams = {
	                        scope: scopeName,
	                        app: appName,
	                        dbProvider: dsConfigForm.findField('dbProvider').getValue(),
	                        dbServer: dsConfigForm.findField('dbServer').getValue(),
	                        dbInstance: dsConfigForm.findField('dbInstance').getValue(),
	                        dbName: dsConfigForm.findField('dbName').getValue(),
	                        dbSchema: dsConfigForm.findField('dbSchema').getValue(),
	                        dbUserName: dsConfigForm.findField('dbUserName').getValue(),
	                        dbPassword: dsConfigForm.findField('dbPassword').getValue(),
	                        portNumber: dsConfigForm.findField('portNumber').getValue(),
	                        tableNames: selectTableNames,
	                        serName: serName
	                    };
	                }
	                else {
	                    treeLoader.baseParams = {
	                        scope: scopeName,
	                        app: appName,
	                        dbProvider: dbDict.Provider,
	                        dbServer: dbInfo.dbServer,
	                        dbInstance: dbInfo.dbInstance,
	                        dbName: dbInfo.dbName,
	                        dbSchema: dbDict.SchemaName,
	                        dbUserName: dbInfo.dbUserName,
	                        dbPassword: dbInfo.dbPassword,
	                        portNumber: dbInfo.portNumber,
	                        tableNames: selectTableNames,
	                        serName: serName
	                    };
	                }

	                var rootNode = dbObjectsTree.getRootNode();
	                rootNode.reload(
                        function (rootNode) {
                            loadTree(rootNode, dbDict);
                        });
	            }
	        }, {
	            xtype: 'tbspacer',
	            width: 4
	        }, {
	            xtype: 'tbbutton',
	            icon: 'Content/img/16x16/edit-clear.png',
	            text: 'Reset',
	            tooltip: 'Reset to the latest applied changes',
	            handler: function () {
	                var rootNode = dataObjectsPane.items.items[0].items.items[0].getRootNode();
	                var selectTableNames = new Array();
	                var selectTableNamesSingle = new Array();
	                var firstSelectTableNames = new Array();
	                var availTableName = new Array();
	                var found = false;

	                for (var i = 0; i < dbTableNames.items.length; i++) {
	                    availTableName.push(dbTableNames.items[i]);
	                }

	                for (var j = 0; j < availTableName.length; j++)
	                    for (var i = 0; i < rootNode.childNodes.length; i++) {
	                        if (rootNode.childNodes[i].data.property.tableName.toLowerCase() == availTableName[j].toLowerCase()) {
	                            found = true;
	                            availTableName.splice(j, 1);
	                            j--;
	                            break;
	                        }
	                    }

	                for (var i = 0; i < rootNode.childNodes.length; i++) {
	                    var nodeText = rootNode.childNodes[i].data.property.tableName;
	                    selectTableNames.push([nodeText, nodeText]);
	                    selectTableNamesSingle.push(nodeText);
	                }

	                if (selectTableNames[0]) {
	                    firstSelectTableNames.push(selectTableNames[0]);
	                    var tablesSelector = tablesSelectorPane.items.items[1];

	                    if (tablesSelector.toMultiselect.store.data) {
	                        tablesSelector.toMultiselect.reset();
	                        tablesSelector.toMultiselect.store.removeAll();
	                    }

	                    tablesSelector.toMultiselect.store.loadData(firstSelectTableNames);
	                    tablesSelector.toMultiselect.store.commitChanges();

	                    var firstSelectTables = tablesSelector.toMultiselect.store.data.items;
	                    var loadSingle = false;
	                    var selectTableName = firstSelectTables[0].data.text;

	                    if (selectTableName[1])
	                        if (selectTableName[1].length > 1)
	                            var loadSingle = true;

	                    tablesSelector.toMultiselect.reset();
	                    tablesSelector.toMultiselect.store.removeAll();

	                    if (!loadSingle)
	                        tablesSelector.toMultiselect.store.loadData(selectTableNames);
	                    else
	                        tablesSelector.toMultiselect.store.loadData(selectTableNamesSingle);

	                    tablesSelector.toMultiselect.store.commitChanges();
	                }
	                else {
	                    if (!tablesSelector)
	                        var tablesSelector = tablesSelectorPane.items.items[1];
	                    if (tablesSelector.toMultiselect) {
	                        tablesSelector.toMultiselect.reset();
	                        tablesSelector.toMultiselect.store.removeAll();
	                        tablesSelector.toMultiselect.store.commitChanges();
	                    }
	                }

	                if (tablesSelector.fromMultiselect.store.data) {
	                    tablesSelector.fromMultiselect.reset();
	                    tablesSelector.fromMultiselect.store.removeAll();
	                }

	                tablesSelector.fromMultiselect.store.loadData(availTableName);
	                tablesSelector.fromMultiselect.store.commitChanges();
	            }
	        }]
	    });

	    editPane.add(tablesSelectorPane);
	    var panelIndex = editPane.items.indexOf(tablesSelectorPane);
	    editPane.getLayout().setActiveItem(panelIndex);
    }
});		

	

function setTableNames () {
    // populate selected tables		
    var dbDict = AM.view.nhibernate.dbDict.value;	
	var selectTableNames = new Array();

	for (var i = 0; i < dbDict.dataObjects.length; i++) {
		var tableName = (dbDict.dataObjects[i].tableName ? dbDict.dataObjects[i].tableName : dbDict.dataObjects[i]);
		selectTableNames.push(tableName);
	}

	return selectTableNames;
};



function setSelectTables (dbObjectsTree) {
	var selectTableNames = new Array();

	if (!dbObjectsTree.disabled) {
		var rootNode = dbObjectsTree.getRootNode();
		for (var i = 0; i < rootNode.childNodes.length; i++) {
			var nodeText = rootNode.childNodes[i].data.property.tableName;
			selectTableNames.push([nodeText, nodeText]);
		}
	}

	return selectTableNames;
};