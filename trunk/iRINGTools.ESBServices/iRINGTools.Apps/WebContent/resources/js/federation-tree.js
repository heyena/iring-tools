/*
 * @File Name : federation-tree.js
 * @Path : resources/js
 * @Using Lib : Ext JS Library 3.2.1(lib/ext-3.2.1)
 *
 * This file intended to make Federation Tree using JSON data string
 * It contains different event handlers functions to perform particular action at each time
 * It also used to display Detail panel that is binded with Federation panel
 * And it generate Forms to display and edit the properties of Tree node in a Tab panel
 *
 */

var tree

Ext.onReady(function() {

	Ext.QuickTips.init();

	// turn on validation errors beside the field globally
	Ext.form.Field.prototype.msgTarget = 'side';

	tree = new Ext.tree.TreePanel({
		region : 'north',
		split : true,
		id : 'federation-tree',
		height : 300,
		bodyBorder : false,
		border : false,
		hlColor : 'C3DAF',
		layout : 'fit',
		useArrows : false, // true for vista like
		autoScroll : true,
		animate : true,
		margins : '0 0 0 0',
		lines : true,
		containerScroll : true,
		rootVisible : false,
		root : {
			nodeType : 'async',
			Name : 'Federation',
			Description : 'Descripton of Federation',
			icon : 'resources/images/16x16/internet-web-browser.png',
			text : 'Federation'
		},
		dataUrl : 'federation',
                //dataUrl : 'federation-tree.json',
		tbar : new Ext.Toolbar({
			xtype : "toolbar",
			items : [
					{
						xtype : "tbbutton",
						icon : 'resources/images/16x16/view-refresh.png',
						tooltip : 'Reload',
						disabled : false,
						handler : function() {
							Ext.state.Manager.clear("treestate");
							tree.root.reload();
						}
					},
					{
						// For Open and Edit
						xtype : "tbbutton",
						text : 'Open',
						icon : 'resources/images/16x16/document-open.png',
						id : 'headExchange',
						tooltip : 'Open',
						disabled : false,
						handler : function() {
							showCentralEditForms(tree.getSelectionModel()
									.getSelectedNode());
						}
					}, {
						xtype : "tbbutton",
						text : 'Add New',
						icon : 'resources/images/16x16/document-new.png',
						tooltip : 'Add New',
						disabled : false,
						handler : function() {
							// new form with blank fields be there
						}
					} ]
		}),
		listeners : {
			click : {
				fn : function(node) {
					// get all the attributes of node
					var properties = node.attributes.items;

					var gridSource = new Array();

					for ( var i = 0; i < properties.length; i++) {
						gridSource[properties[i].name] = properties[i].value;
					}

					// get the property grid component
					var propGrids = Ext.getCmp('propGrid');
					// make sure the property grid exists
					if (propGrids) {
						// populate the property grid with gridSource
						propGrids.setSource(gridSource);
					}
				}
			},
			expandnode : {
				fn : function(node) {
					Ext.state.Manager.set("treestate", node.getPath())
				}
			},
			dblclick : {
				fn : function(node) {
					showCentralEditForms(node);
				}
			}
		}
	});

	var contextMenu = new Ext.menu.Menu({
		items : [ {
			text : 'Sort',
			handler : sortHandler
		} ]
	});
	function sortHandler() {
		tree.getSelectionModel().getSelectedNode().sort(
				function(leftNode, rightNode) {
					return 1;
				});
	}

	/* to maintain the state of the tree */
	Ext.state.Manager.setProvider(new Ext.state.CookieProvider());
	tree.on('contextmenu', function(node) {
		node.select();
		contextMenu.show(node.ui.getAnchor());
	});

	/* to maintain the state of the tree */
	var treeState = Ext.state.Manager.get("treestate");
	if (treeState) {
		if (tree.expandPath(treeState)) { // check the
			tree.expandPath(treeState);
		} else {
			Ext.state.Manager.clear("treestate");
			tree.root.reload();
		}
	}

}); // end on onReady function

