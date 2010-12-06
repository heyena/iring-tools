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
		//dataUrl : 'federation',
                dataUrl : 'federation-tree.json',
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
					var properties = node.attributes.properties;

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

	function showCentralEditForms(node) {
		var obj = node.attributes
                var properties = node.attributes.properties
		var nId = obj['id']

		if ('children' in obj != true) { // restrict to generate form those have children

			/*
			 * Generate the fields items dynamically
			 */
                        var list_items = '{'
                             +'xtype:"hidden",'//<--hidden field
                             +'name:"h-type",' //name of the field sent to the server
                             +'value:"'+node.parentNode.text + '_' + obj['text']+'"'//value of the field
                             +'}'
                         
                        for ( var i = 0; i < properties.length; i++) {
                            var fname=properties[i].name
                            var xtype
                            switch(fname){
                                case "Description":
                                    xtype= 'xtype : "textarea"'
                                    break;
                                 case "Read Only" :
                                 case "Writable": 
                                     xtype= 'xtype : "combo",triggerAction: "all", mode: "local", store: ["true","false"],  displayField:"'+properties[i].value+'", width: 120'
                                     break;
                                 default:
                                    xtype= 'xtype : "textfield"'
                            }
                                        
                             list_items = list_items+',{'+xtype+',' + 'fieldLabel:"' + properties[i].name
                             + '",name:"'
                             + properties[i].name
                             + '",allowBlank:false, value:"'
                             +properties[i].value+'"}'

                       }

			list_items = eval('[' + list_items + ']')

			// generate form for editing purpose
			var edit_form = new Ext.FormPanel({
				labelWidth : 100, // label settings here cascade unless
                                url:'save-form', // file which will be used to interact with server
                                method: 'POST',
				border : false, // removing the border of the form
				renderTo:'centerPanel',
				// renderTo:document.body,
				//renderTo : Ext.getBody(),
                                id : 'frmEdit' + nId,
				frame : true,
                                //items: { xtype: 'component', autoEl: 'span' },

				bodyStyle : 'padding:5px 5px 0',
				width : 350,
				closable : true,
				defaults : {
                                    width : 230
				},
				defaultType : 'textfield',
				items : list_items,     // binding with the fields list
				buttonAlign : 'left', // buttons aligned to the left
				buttons : [ {
					text : 'Save',
                                        handler: function(){
                                            edit_form.getForm().submit({
                                                success: function(f,a){
                                                    Ext.Msg.alert('Success', 'It worked');
                                                },
                                                failure: function(f,a){
                                                    Ext.Msg.alert('Warning', 'Error');
                                                }
                                            });
                                        }
				}, {
					text: 'Reset',
                                        handler: function(){
                                            edit_form.getForm().reset();
                                        }
				} ],
                               autoDestroy:false,
                               listeners: {
                                    close: function(){
                                       // edit_form.destroy()
                                        //tabsPanel.doLayout();
                                    }
                                }
			});

			// fill the data in all fields of form
			//edit_form.getForm().setValues(node.attributes.properties);

			// display the form in the center panel as a tab
			Ext.getCmp('centerPanel').enable();
			Ext.getCmp('centerPanel').add(Ext.apply(edit_form, {
				id : 'tab-' + obj['id'],
                                 deferredRender: false,
				title : node.parentNode.text + ' : ' + obj['text'],
				closable : true
			})).show();
                 /*  tabPanel.add({
                        title: tabTitle,
                        iconCls: 'tabs',
                        closable:true
                    }).show();*/

		}// end of if
	}
}); // end on onReady function

