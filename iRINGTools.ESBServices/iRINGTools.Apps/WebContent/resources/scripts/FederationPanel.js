Ext.ns('FederationManager');
/**
 * @class FederationManager.FederationPanel
 * @extends Panel
 * @author by Ritu Garg
 */
FederationManager.FederationPanel = Ext
    .extend(
        Ext.Panel,
        {
          title : 'Federation',
          layout : 'border',
          url : null,

          federationPanel : null,
          propertyPanel : null,
          contextMenu: null,
          contextRepoMenu:null,
          contextRepoDisMenu:null,

          /**
           * initComponent
           * 
           * @protected
           */
          initComponent : function() {

            this.addEvents( {
              click : true,
              refresh : true,
              edit : true,
              addnew : true,
              opentab : true,
              load : true,
              beforeload : true,
              selectionchange : true
            });

            this.tbar = this.buildToolbar();
            
            this.contextMenu = new Ext.menu.Menu();
	    	this.contextMenu.add(this.buildContextMenu());

            
            
/*            this.contextRepoMenu = new Ext.menu.Menu();
            this.contextRepoMenu.add(this.buildRepoContextMenu());
*/            
            this.contextRepoDisMenu = new Ext.menu.Menu();
            this.contextRepoDisMenu.add(this.buildRepoDisContextMenu());
            

            
            this.federationPanel = new Ext.tree.TreePanel( {
              region : 'center',
              border : false,
              id : 'federation-tree',
              split : true,
              expandAll : true,
              rootVisible : false,
              lines : true,
              useArrows : false,
              autoScroll : true,
              style : 'padding-bottom:5px;background:#eee;',
              loader : new Ext.tree.TreeLoader( {
                dataUrl : this.url
              }),

              root : {
                nodeType : 'async',
                text : 'Federation',
                expanded : true,
                draggable : false,
                icon : 'resources/images/16x16/internet-web-browser.png'
              }

            });

            this.propertyPanel = new Ext.grid.PropertyGrid( {
              id : 'property-panel',
              title : 'Details',
              region : 'south',
              // layout: 'fit',
              stripeRows : true,
              collapsible : true,
              autoScroll : true,
              border : false,
              frame : false,
              height : 250,
              // bodyStyle: 'padding-bottom:15px;background:#eee;',
              source : {},
              listeners : {
                // to disable editable option of the property grid
                beforeedit : function(e) {
                  e.cancel = true;
                }
              }
            });

            this.items = [ this.federationPanel, this.propertyPanel ];

            this.federationPanel.on('click', this.onClick, this);
            this.federationPanel.on('load', this.onLoad, this);
            this.federationPanel.on('beforeload', this.onBeforeLoad, this);
            this.federationPanel.on('dblclick', this.onDblClick, this);
            // this.federationPanel.on('expandnode', this.onExpand, this);
            // this.federationPanel.on('expandnode', this.select_node, this);
            this.federationPanel.on('refresh', this.onRefresh, this);
            this.federationPanel.getSelectionModel().on('selectionchange',
                this.onSelectionChange, this, this);
            this.federationPanel.on('contextmenu', this.showContextMenu, this);

            var state = Ext.state.Manager.get("federation-state");

            if (state) {
              if (this.federationPanel.expandPath(state) == false) {
                Ext.state.Manager.clear("federation-state");
                this.federationPanel.root.reload();
              }
            }

            // super
            FederationManager.FederationPanel.superclass.initComponent.call(this);
          },
          showContextMenu: function (node, event) {
        	   var dis;
        	    if (node.isSelected()) {
        	      var x = event.browserEvent.clientX;
        	      var y = event.browserEvent.clientY;
        	      
        	      if(node.parentNode.text == 'Repositories' || node.text == 'Repositories'){
        	    	  var properties = node.attributes.properties;

        	    	  this.contextRepoMenu = new Ext.menu.Menu();
                      this.contextRepoMenu.add(this.buildRepoContextMenu());

        	    	  dis=0;
        	    		  for (var i in properties) {
        	    			  if(i == 'Read Only' && properties[i]== 'true'){
        	    				  dis=1;
        	    				  break;
        	    			  }
        	    		  }
        	    		  if(dis == 1){
        	    			  this.contextRepoDisMenu.showAt([x, y]);
        	    		  }else{
        	    			  this.contextRepoMenu.showAt([x, y]);
        	    		  }
        	      }else{
        	    	  this.contextMenu.showAt([x, y]);
        	      }
        	    }
        	  },
          buildToolbar : function() {
            return [ {
              xtype : "tbbutton",
              icon : 'resources/images/16x16/view-refresh.png',
              tooltip : 'Refresh',
              disabled : false,
              handler : this.onRefresh,
              scope : this
            }, {
              xtype : "tbbutton",
              text : 'Open',
              icon : 'resources/images/16x16/document-open.png',
              tooltip : 'Open',
              disabled : false,
              handler : this.onEdit,
              scope : this
            }, {
              xtype : "tbbutton",
              icon : 'resources/images/16x16/document-new.png',
              tooltip : 'New',
              text : 'New',
              disabled : false,
              handler : this.onAddnew,
              scope : this
            }, {
              xtype : "tbbutton",
              icon : 'resources/images/16x16/edit-delete.png',
              tooltip : 'Delete',
              text : 'Delete',
              disabled : false,
              handler : this.onDelete,
              scope : this
            } ];
          },
          buildContextMenu: function () {
        	    return [
        	      {
        	        text: 'Open',
        	        handler: this.onEdit,
        	        icon : 'resources/images/16x16/document-open.png',
        	        scope: this
        	      },
        	      {
        	        text: 'Add',
        	        handler: this.onAddnew,
        	        icon : 'resources/images/16x16/document-new.png',
        	        scope: this
        	      },
        	      {
        	        text: 'Delete',
        	        handler: this.onDelete,
        	        icon : 'resources/images/16x16/edit-delete.png',
        	        scope: this
        	      }]
        	  },
        	  
        	  buildRepoContextMenu: function () {
            	    return [
            	      this.buildContextMenu(),
            	      {
            	        xtype: 'menuseparator'
            	      },
            	      {
            	        text: 'Add Class',
            	        handler : this.openAddClassTab,
            	        icon: 'resources/images/16x16/class-badge.png',
            	        scope: this
            	      },
            	      {
              	        text: 'Add Template',
              	        handler: this.onTemplateAdd,
              	        icon: 'resources/images/16x16/template-badge.png',
              	        scope: this
              	      }
            	    ]
            	  },
             buildRepoDisContextMenu: function () {
              	    return [
              	      this.buildContextMenu(),
              	      {
              	        xtype: 'menuseparator'
              	      },
              	      {
              	        text: 'Add Class',
              	        disabled : true,
              	        icon: 'resources/images/16x16/class-badge.png',
              	        scope: this
              	      },
              	      {
                	        text: 'Add Template',
                	        disabled : true,
                	        icon: 'resources/images/16x16/template-badge.png',
                	        scope: this
                	      }
              	    ]
              	  },
          onDelete : function() {
            that = this;
            var node = this.getSelectedNode();
            if (node.hasChildNodes()) {
              Ext.MessageBox.show( {
                title : '<font color=red></font>',
                msg : 'Please select a child node to delete.',
                buttons : Ext.MessageBox.OK,
                icon : Ext.MessageBox.INFO
              });
            } else if (node == null) {
              Ext.Msg.alert('Warning', 'Please select a node.');
            } else {
              Ext.Msg
                  .show( {
                    msg : 'All the tabs will be closed. Do you want to delete this node?',
                    buttons : Ext.Msg.YESNO,
                    icon : Ext.Msg.QUESTION,
                    fn : function(action) {
                      if (action == 'yes') {
                        // send ajax request
                        Ext.Ajax
                            .request( {
                              url : 'deleteNode',
                              method : 'GET',
                              params : {
                                'nodeId' : node.attributes.properties['Id'],
                                'parentNodeID' : node.parentNode.attributes.properties['Id']
                              },
                              success : function(o) {
                                // remove all tabs form tabpanel
                                // to be removed in future
                                Ext.getCmp('contentPanel').removeAll(true);
                                Ext.getCmp('contentPanel').disable();

                                // remove the node form tree
                                // that.federationPanel.selModel.selNode.parentNode.removeChild(node);
                                // Tree Reload
                                that.onRefresh();
                                // fire event so that the Details panel will be
                                // changed accordingly
                                that.fireEvent('selectionchange', this);
                                Ext.Msg
                                    .alert('Sucess', 'Node has been deleted');
                              },
                              failure : function(f, a) {
                                Ext.Msg.alert('Warning', 'Error!!!');
                              }
                            });
                      } 
                    }
                  });
            }
          },

          getNodeBySelectedTab : function(tab) {
            var tabid = tab.id;
            // tabid is "tab-jf23dfj-sd3fas-df33s-s3df"
            nodeId = tabid.substr(4, tabid.length); 
            return this.getNodeById(nodeId); // get the NODE using nodeid
          },

          getNodeById : function(nodeId) {
            // if nodeID exists it will find out NODE
            if (this.federationPanel.getNodeById(nodeId)) { 
              return this.federationPanel.getNodeById(nodeId);
            } else {
              return false;
            }
          },
          getSelectedNode : function() {
            return this.federationPanel.getSelectionModel().getSelectedNode();
          },

          selectNode : function(node) {
            this.expandNode(node);
            this.federationPanel.getSelectionModel().select(node);
          },

          expandNode : function(node) {
            this.federationPanel.expandPath(node.getPath());
          },

          onSelectionChange : function(sm, node) {
            this.onClick(node);
          },
          generateForm : function(formType) {
            node = this.getSelectedNode();
            if (node != null) {
              if ((node.hasChildNodes() && formType == 'newForm')
                  || (!node.hasChildNodes() && formType == 'editForm')) {
                this.openTab(node, formType);
              } else if (!node.hasChildNodes() && formType == 'newForm') {
                this.openTab(node.parentNode, formType);
              } else {
                Ext.MessageBox.show( {
                  title : '<font color=red></font>',
                  msg : 'Please select a child node to edit.',
                  buttons : Ext.MessageBox.OK,
                  icon : Ext.MessageBox.INFO
                });
              }
            } else {
              Ext.MessageBox.show( {
                title : '<font color=red></font>',
                msg : 'Please select a node.',
                buttons : Ext.MessageBox.OK,
                icon : Ext.MessageBox.INFO
              });
            }
          },

          getNamespaces : function(parentNode, ignoreIds) {
            var namespaces = new Array();
            var childNodes = parentNode.childNodes; // get the list of children

            for ( var i = 0; i < childNodes.length; i++) {
              var nodeId = childNodes[i].attributes.properties['Id'];
              
              if (ignoreIds == undefined || ignoreIds.indexOf(nodeId) == -1) {
                var namespace = [nodeId, childNodes[i].attributes.text];
                namespaces.push(namespace);              
              }
            }
            
            return namespaces;
          },
          
          getNodesIDTitleByID : function() {}, 
          
          openTab : function(node, formType) {
            var obj = node.attributes;
            var properties = node.attributes.properties;
            var parentNode = node.parentNode;
            var nodeID = properties['Id'];
            var listItems = new Array();
            var label = '';
            
            listItems.push({
              xtype: 'hidden',  // hidden field
              name: 'formType', // it will contain 'editForm/newForm'
              value: formType   // value of the field
            });

            if (formType == 'newForm') {
              listItems.push({
                xtype: 'hidden',      // hidden field
                name: 'parentNodeID', // it will contain contain "ID // Generators||Namespaces||Repositories
                value: obj['identifier']  // value of the field
              });
            }
            else if (formType == 'editForm') {
              listItems.push({
                xtype: 'hidden',  // hidden field
                name: 'nodeID',  // name of the field sent to the server
                value: nodeID  // value of the field
              });
              
              listItems.push({
                xtype: 'hidden',  // hidden field
                name: 'parentNodeID',  // it will contain "ID Generators||Namespaces||Repositories
                value: parentNode.attributes.identifier  // value of the field
              });
            }               
            
            for (var i in properties) {
            	if (i != 'Alias')
            		continue;
              var listItem = {};
              if(i=='Alias'){
              	  listItem.enableKeyEvents=true;
              	  listItem.listeners= {
  		        		keyup:function(f,evt){
  		        			if (formType == 'editForm') {
  		        			Ext.getCmp('tab-' +node.id).setTitle(node.parentNode.text + ' : ' +f.getValue());
  		        			}else{
  		        				Ext.getCmp('tab-' +node.id).setTitle(obj['text'] +': '+f.getValue());
  		        			}
  		          		}
  		        		};
                }
              var fname = i;
              var value = '';	 
              listItem.xtype = 'textfield';
              listItem.width = 230;
            	  
              if (formType == 'editForm') {
                value = properties[i];
                label = node.parentNode.text + ' : ' + obj['text'];
              } 
              else {
                label = obj['text'] + ':(New)';
              }
              
              listItem.fieldLabel = i;
              listItem.name = i;
              listItem.allowBlank = false;
              //listItem.blankText = 'This Field is required';
              listItem.value = value;
              listItems.push(listItem);
            }
            
            for (var i in properties) {
            	if (i == 'Update URI' || i == 'Read Only' || i == 'Writable' || i == 'Alias' || i == 'ID Generator')
            		continue;
              var listItem = {};
              var fname = i;
              var value = '';	              

              if(i=='Name'){
              	  listItem.enableKeyEvents=true;
              	  listItem.listeners= {
  		        		keyup:function(f,evt){
  		        			if (formType == 'editForm') {
  		        			Ext.getCmp('tab-' +node.id).setTitle(node.parentNode.text + ' : ' +f.getValue());
  		        			}else{
  		        				Ext.getCmp('tab-' +node.id).setTitle(obj['text'] +': '+f.getValue());
  		        			}
  		          		}
  		        		};
                }
              
              
              switch (fname) {
              	  case "Description": 
	                listItem.xtype = 'textarea';
	                listItem.width = 230;
	                break;	                    
	              case 'Id':
	              	continue;
	              case 'Repository Type':
	                listItem.xtype = 'combo';
	                listItem.width = 230;
	                listItem.triggerAction = 'all';
	                listItem.editable = false;
	                listItem.mode = 'local';
	                listItem.store = ['RDS/WIP', 'Camelot', 'Part8'];
	                listItem.displayField = properties[i];
	                break;	              
	                
	              case 'Namespace List':
	            	var selNamespaces;
	                var selNamespaceIds = new Array();
	                
	                if (properties[i] != null && properties[i] != '') {
	                	selNamespaces = new Array();
	                  var selNamespaceList = Ext.util.JSON.decode(properties[i]);
	                  
	                  for (var namespaceId in selNamespaceList) {
	                    var selNameSpaceItem = [namespaceId, selNamespaceList[namespaceId]];
	                    selNamespaces.push(selNameSpaceItem);
	                    selNamespaceIds.push(namespaceId);
	                  }
	                }else{
	                	selNamespaces = [[]];
	                }
	
	                var rootNode = this.federationPanel.getRootNode();
	                var namespaceParentNode = rootNode.findChild('identifier', 'namespace');
	                var availNamespaces = this.getNamespaces(namespaceParentNode, selNamespaceIds);
	
	                listItem.xtype = 'itemselector';
	                listItem.fieldLabel = 'Namespace List';
	                listItem.name = 'itemselector';
	                listItem.imagePath = './resources/ext-3.3.0/ux/images/';
	                listItem.multiselects = [{
	                  store: availNamespaces,
	                  width: 200,
	                  height: 150,
	                  displayField: 'text', 
	                  valueField: 'value'
	                },{ 
	                  store: selNamespaces,
	                  width: 200,
	                  height: 150
	                }]; 
	                break;
	                
	              default:
	                listItem.xtype = 'textfield';
	                listItem.width = 230;
	                break;
              }

              if (formType == 'editForm') {
                value = properties[i];
                label = node.parentNode.text + ' : ' + obj['text'];
              } 
              else {
                label = obj['text'] + ':(New)';
              }
              
              listItem.fieldLabel = i;

              listItem.name = i;
              listItem.allowBlank = false;
              listItem.blankText = 'This Field is required';
              listItem.value = value;             
              listItems.push(listItem);	            
            }
            
            var readOnly = '';
            var writeable = '';
            for (var i in properties) {
            	if (i != 'Read Only' && i != 'Writable')
            		continue;
              var listItem = {};
              var fname = i;
              var value = '';	              
          	
              listItem.xtype = 'combo';
              listItem.width = 230;
              listItem.triggerAction = 'all';
              listItem.editable = false;
              listItem.mode = 'local';
              listItem.store = ["true", "false"];
              listItem.displayField = properties[i];    
              
              switch (fname) { 
              	case 'Read Only':
		              listItem.listeners = {'select' : function(combo, record, index) {
											var tab = Ext.getCmp('contentPanel').getItem('tab-' + node.id);											
											var textfield= tab.items.map['data-form'].items.map[node.id + '_update-uri'];
											listItem.width = 230;
											if (record.data.field1 == 'true'){
												textfield.setValue('');
												textfield.clearInvalid();
												textfield.disable();												
											}
											else{
												textfield.enable();	
												//listItem.blankText = 'This Field is required';
											}
										}
									};
		              readOnly = properties[i];
		              break;
              	case 'Writable':
              		listItem.listeners = {'select' : function(combo, record, index) {
										var tab = Ext.getCmp('contentPanel').getItem('tab-' + node.id);											
										var textfield = tab.items.map['data-form'].items.map[node.id + '_id-generator'];
		                listItem.width = 230;
										if (record.data.field1 == 'false'){											
											textfield.setValue('None');	
											textfield.disable();										
										}
										else {
											textfield.setValue('None');
											textfield.enable();											
										}
									}
								};
	              writeable = properties[i];
	              break;		            
              }

              if (formType == 'editForm') {
                value = properties[i];
                label = node.parentNode.text + ' : ' + obj['text'];
              } 
              else {
                label = obj['text'] + ':(New)';
              }
              
              listItem.fieldLabel = i;
              listItem.name = i;
              listItem.allowBlank = false;             
              listItem.value = value;              
              listItems.push(listItem);
	            
            }
            
            for (var i in properties) {
            	if (i != 'Update URI')
            		continue;
              var listItem = {};
              var fname = i;
              var value = '';
              listItem.xtype = 'textfield';
              listItem.width = 230;	              

              if (formType == 'editForm') {
                value = properties[i];
                label = node.parentNode.text + ' : ' + obj['text'];
              } 
              else {
                label = obj['text'] + ':(New)';
              }
              
              listItem.fieldLabel = i;
              listItem.name = i;
              listItem.id = node.id + '_update-uri';
              listItem.allowBlank = false;
              
              listItem.value = value;
              
              if(readOnly == 'true'){
              	listItem.blankText = '';
              	listItem.disabled = true;              	
              }else{
              	listItem.disabled = false;	
              	listItem.blankText = 'This Field is required';
              }              
              listItems.push(listItem);	            
            }
            
            for (var i in properties) {
            	if (i != 'ID Generator')
            		continue;
              var listItem = {};
              var fname = i;
              var value = '';
           
              // get all the IDGenerators
							var rootNode = this.federationPanel.getRootNode();
							var namespaceParentNode = rootNode.findChild('identifier', 'idGenerator');
							var allIDGenerators = this.getNamespaces(namespaceParentNode);

              listItem.xtype = 'combo';
              listItem.hiddenName = 'ID Generator';
              listItem.id = node.id + '_id-generator';
              listItem.width = 230;
              listItem.triggerAction = 'all';
              listItem.editable = false;
              listItem.mode = 'local';
              listItem.store = allIDGenerators;
              listItem.displayField = properties[i];      
              listItem.blankText = '';

              if (formType == 'editForm') {
                value = properties[i];
                label = node.parentNode.text + ' : ' + obj['text'];
              } 
              else {
                label = obj['text'] + ':(New)';
              }
              
              listItem.fieldLabel = i;
              listItem.name = i;             
              listItem.allowBlank = false;             
              
              if(writeable == 'false'){
              	listItem.disabled = true;
              	listItem.value = 'None';
              }else{
              	listItem.disabled = false;              	
              	listItem.value = value;
              }
              
              listItems.push(listItem);
	            
            }

            this.fireEvent('opentab', this, node, label, listItems);
          },

          onClick : function(node) {
            /*
             * var gridSource = new Array(); if(node!=undefined && node!=null &&
             * !node.hasChildNodes()){ // get all the attributes of node var
             * properties = node.attributes.properties; for ( var i = 0; i <
             * properties.length; i++) { if(i == 'ID Generator'){ gridSource[i] =
             * this.getNodeById(properties[i]).text; // contains IDGenerator
             * Title }else{ gridSource[i] = properties[i]; } } } // populate the
             * property grid with gridSource
             * this.propertyPanel.setSource(gridSource);
             */

            if (node != undefined && node != null && !node.hasChildNodes())
              this.propertyPanel.setSource(node.attributes.properties);

            this.fireEvent('click', this, node);
          },

          onDblClick : function(node) {
            this.generateForm('editForm');
          },

          onExpand : function(node) {
            // Ext.state.Manager.set('federation-state', node.getPath());
            // this.fireEvent('refresh', this, this.getSelectedNode());
          },

          onRefresh : function(node) {
            Ext.state.Manager.clear('federation-state');
            this.federationPanel.root.reload();
          },

          onEdit : function(btn, ev) {
            this.generateForm('editForm');
          },

          onAddnew : function(btn, ev) {
            this.generateForm('newForm');

          },

          onLoad : function() {
            this.federationPanel.getRootNode().expand(true);
            Ext.getBody().unmask();
          },

          onBeforeLoad : function() {
            Ext.getBody().mask('Loading...', 'x-mask-loading');
          },
          
          openAddClassTab : function(){
              var listItems = new Array();
              var label = 'New: {}';
              var tabId = 'addClass';
              
              listItems.push({
                xtype: 'hidden',
                name: 'formType',
                value: 'newClass'
              });
             this.fireEvent('openAddTab', this,tabId,label, 'class', null);
            
          },
          
          onTemplateAdd : function(){
              var listItems = new Array();
              var label = 'New: {}';
              var tabId = 'addTemplate';
              
              listItems.push({
                xtype: 'hidden',
                name: 'formType',
                value: 'newTemplate'
              });

             this.fireEvent('openAddTab', this,tabId, label, 'template', null);
            
          }
        });

//Overrides
Ext.override(Ext.form.ComboBox, {
    setValue: function (v) {
        var text = v;
        if (this.valueField) {
            if (this.mode == 'remote' && !Ext.isDefined(this.store.totalLength)) {
                this.store.on('load', this.setValue.createDelegate(this, arguments), null, { single: true });
                if (this.store.lastOptions === null) {
                    var params;
                    if (this.valueParam) {
                        params = {};
                        params[this.valueParam] = v;
                    } else {
                        var q = this.allQuery;
                        this.lastQuery = q;
                        this.store.setBaseParam(this.queryParam, q);
                        params = this.getParams(q);
                    }
                    this.store.load({ params: params });
                }
                return;
            }
            var r = this.findRecord(this.valueField, v);
            if (r) {
                text = r.data[this.displayField];
            } else if (this.valueNotFoundText !== undefined) {
                text = this.valueNotFoundText;
            }
        }
        this.lastSelectionText = text;
        if (this.hiddenField) {
            this.hiddenField.value = v;
        }
        Ext.form.ComboBox.superclass.setValue.call(this, text);
        this.value = v;
    }
});