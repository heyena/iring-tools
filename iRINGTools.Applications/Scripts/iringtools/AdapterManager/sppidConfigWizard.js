Ext.ns('AdapterManager');

AdapterManager.sppidConfigWizard = Ext.extend(Ext.Container, {
    scope: null,
    app: null,
    iconCls: 'tabsSPPID',
    border: false,
    frame: false,

    constructor: function (config) {
        config = config || {};

        var wizard = this;
        var scopeName = config.scope;
        var appName = config.app;
        var dbDict;
        var dbInfo;
        var dbTableNames;
        var userTableNames;


        var setDsConfigPane = function (editPane) {
            if (editPane) {
                   

                var selectItems = null;

                var dsConfigPane = new Ext.FormPanel({
                    labelWidth: 140,
                    id: scopeName + '.' + appName + '.dsconfigPane',
                    frame: false,
                    border: false,
                    autoScroll: true,
                    layout: 'column',
                    bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
                    monitorValid: true,
                    defaults: {      // defaults applied to items
                        layout: 'form',
                        border: false,
                        bodyStyle: 'padding:4px'
                    },

                  items: [{
        // Fieldset in Column 1
        xtype:'fieldset',
        columnWidth: 0.5,
        collapsible: false,
        autoHeight:true,
        defaults: {
            anchor: '-20' // leave room for error icon
        },
        defaultType: 'textfield',
        items :[{
            fieldLabel: 'Select Commodity',
                xtype: 'combo',
                 store: [[1, 'Equipment'], [2, 'Instrument'],[3, 'PipingSegment']]
            }]
    },{
        xtype:'fieldset',
        autoHeight: true,
        columnWidth: 0.5,
        items :[{
            fieldLabel: 'Select Related Item',
            xtype: 'combo',
            store: [[1, 'Parent Tag'], [2, 'Drawing: Date Created'], [3, 'Drawing: Description']]
            }]
    },
    {
    xtype:'fieldset',
     autoHeight: true,
     columnWidth: 1,
     border: true,
       style: 'padding:3px; align:center',
          items :[{
				   
				                        xtype: 'itemselector',
				                        hideLabel: true,
				                        bodyStyle: 'background:#eee',
				                        frame: true,
				                        name: 'tableSelector',
				                        imagePath: 'scripts/ext-3.3.1/examples/ux/images/',
				                        multiselects: [{
			                            width: 300,
			                            height: 370,
			                            //store: availItems,
			                            store: new Ext.data.ArrayStore({
		                                id: 0,
		                                fields: [
								            'tableValue',
								            'tableName'
								        ],
		                                data: [[1, 'item3'], [2, 'item4']]
				                            }),
			                            displayField: 'tableName',
			                            valueField: 'tableValue',
			                            border: 0
				                        },{
			                            width: 300,
			                            height: 370,
			                            store: new Ext.data.ArrayStore({
			                                id: 0,
			                                fields: [
								            'tableValue',
								            'tableName'
								        ],
			                                data: [[1, 'item1'], [2, 'item2']]
			                            }),
			                            displayField: 'tableName',
			                            valueField: 'tableValue',
			                            border: 0
			                        }]
			                    }] 
                             }],
                       tbar: new Ext.Toolbar({
                        items: [{
                            xtype: 'tbspacer',
                            width: 4
                        }, {
                            xtype: 'tbbutton',
                            icon: 'Content/img/16x16/document-properties.png',
                            text: 'Apply',
                            tooltip: 'Apply'
                           }, {
                            xtype: 'tbspacer',
                            width: 4
                        }, {
                            xtype: 'tbbutton',
                            icon: 'Content/img/16x16/edit-clear.png',
                            text: 'Reset',
                            tooltip: 'Reset to the latest applied changes',
                            handler: function (f) {
                               // setDsConfigFields(dsConfigPane.getForm());
                            }
                        }]
                    })
                });

                if (dbInfo)
                    setDsConfigFields(dsConfigPane.getForm());
                editPane.add(dsConfigPane);
                var panelIndex = editPane.items.indexOf(dsConfigPane);
                editPane.getLayout().setActiveItem(panelIndex);
            }
        };

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
                autoScroll: true,
                bodyStyle: 'background:#fff',
                items: [{
                    xtype: 'treepanel',
                    border: false,
                    autoScroll: true,
                    animate: true,
                    lines: true,
                    frame: false,
                    enableDD: false,
                    containerScroll: true,
                    rootVisible: true,
                    root: {
                        text: 'Commodities',
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
                            tooltip: 'Reload Commodities',
                            handler: null
                        }, {
                            xtype: 'tbspacer',
                            width: 4
                        }, {
                            xtype: 'tbspacer',
                            width: 4
                        }, {
                            xtype: 'button',
                            icon: 'Content/img/16x16/document-save.png',
                            text: 'Save',
                            tooltip: 'Save the commodities tree to the back-end server',
                            formBind: true,
                            handler:null
                            }]
                            })
                          }]
                        },
                             {
                xtype: 'panel',
                name: 'editor-panel',
                border: 1,
                frame: false,
                id: scopeName + '.' + appName + '.editor-panel',
                region: 'center',
                layout: 'card'
            }]
        });

      

        var showTree = function (dbObjectsTree) {
            var treeLoader = dbObjectsTree.getLoader();
            var rootNode = dbObjectsTree.getRootNode();

            treeLoader.dataUrl = null;
            treeLoader.baseParams = null

            rootNode.reload(
          function (rootNode) {
              loadTree(rootNode);
          });
        }

        Ext.apply(this, {
            id: scopeName + '.' + appName + '.-nh-config',
            title: 'P & ID Configuration - ' + scopeName + '.' + appName,
            closable: true,
            border: false,
            frame: true,
            layout: 'fit',
            items: [dataObjectsPane]
        });

        Ext.EventManager.onWindowResize(this.doLayout, this);

        Ext.Ajax.request({
            url: 'AdapterManager/DBDictionary',
            method: 'POST',
            params: {
                scope: scopeName,
                app: appName
            },
            success: function (response, request) {
                dbDict = Ext.util.JSON.decode(response.responseText);

                var tab = Ext.getCmp('content-panel');
                var rp = tab.items.map[scopeName + '.' + appName + '.-nh-config'];
                var dataObjectsPane = rp.items.map[scopeName + '.' + appName + '.dataObjectsPane'];
                var dbObjectsTree = dataObjectsPane.items.items[0].items.items[0];

                if (dbDict.dataObjects.length > 0) {
                    // populate data source form
                    showTree(dbObjectsTree);
                }
                else {
                    dbObjectsTree.disable();
                    editPane = dataObjectsPane.items.items[1];
                    if (!editPane) {
                        var editPane = dataObjectsPane.items.items.map[scopeName + '.' + appName + '.editor-panel'];
                    }
                    setDsConfigPane(editPane);
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

        AdapterManager.sppidConfigWizard.superclass.constructor.apply(this, arguments);
    }
});



