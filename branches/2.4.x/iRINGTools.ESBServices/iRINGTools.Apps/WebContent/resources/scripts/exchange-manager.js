Ext.ns('org.iringtools.apps.xmgr');

function copyToClipboard(celldata){
	/*if (window.clipboardData)  // Internet Explorer 
		 window.clipboardData.setData ("Text", celldata);
	*/
	window.prompt ("Copy to clipboard: Ctrl+C, Enter", celldata);
	    
	}
function storeSort(field, dir) {
	if (field == '&nbsp;')
		return;

	var limit = this.lastOptions.params.limit;

	this.lastOptions.params = {
		start : 0,
		limit : limit
	};

	if (dir == undefined) {
		if (this.sortInfo && this.sortInfo.direction == 'ASC')
			dir = 'DESC';
		else
			dir = 'ASC';
	}

	this.sortInfo = {
		field : field,
		direction : dir
	};

	this.reload();
}

function createGridStore(container, url) {
	var store = new Ext.data.Store({
		proxy : new Ext.data.HttpProxy({
			url : url,
			timeout : 86400000
		// 24 hours
		}),
		reader : new Ext.data.DynamicGridReader({}),
		remoteSort : true,
		listeners : {
			exception : function(proxy, type, action, request, response) {
				container.getEl().unmask();
				var message = 'Request URL: /' + request.url
						+ '.\n\nError description: ' + response.responseText;
				showDialog(500, 240, 'Error', message, Ext.Msg.OK, null);
			}
		}
	});

	store.sort = store.sort.createInterceptor(storeSort);

	return store;
}

function createGridPane(store, pageSize, viewConfig, withResizer) {
	var filters = new Ext.ux.grid.GridFilters({
		remotesort : true,
		local : false,
		encode : true,
		filters : store.reader.filters
	});

	var plugins = [ filters ];

	if (withResizer) {
		var pagingResizer = new Ext.ux.plugin.PagingToolbarResizer({
			displayText : 'Page Size',
			options : [ 25, 50, 75, 100 ],
			prependCombo : true
		});

		plugins.push(pagingResizer);
	}

	var colModel = new Ext.grid.DynamicColumnModel(store);
 	//var cellModel = new Ext.grid.CellSelectionModel({ singleSelect: true });
	var selModel = new Ext.grid.RowSelectionModel({ singleSelect: true });
	var pagingToolbar = new Ext.PagingToolbar({
		store : store,
		pageSize : pageSize,
		displayInfo : true,
		autoScroll : true,
		plugins : plugins
	});

	var gridPane = new Ext.grid.GridPanel({
		identifier : store.reader.identifier,
		description : store.reader.description,
		layout : 'fit',
		minColumnWidth : 80,
		val:null,
		loadMask : true,
		store : store,
		stripeRows : true,
		viewConfig : viewConfig,
		cm : colModel,
		selModel: selModel,
		enableColLock : false,
		plugins : [ filters ],
    bbar: pagingToolbar,
    
    listeners: {
    	cellclick: function(ts, td, cellIndex, record, tr, rowIndex, e, eOpts ){
    		val = record.target.innerText;
    	},
    	/*celldblclick: function(ts, td, cellIndex, record, tr, rowIndex, e, eOpts ){
    		val = record.target.innerText;
    		copyToClipboard(val);
    	},*/
        beforeedit: function(e){
          e.cancel = true;
        },
      keydown :function(evnt){
      	 var keyPressed = evnt.getKey();
  	     if (evnt.ctrlKey)
  	     {
  	       /*
  	        * After trial and error, the ctrl+c combination seems to be code 67 
  	        */
  	    	 if (67 == 67)//if (keyPressed == 67)
  	       {
  	         //var celldata = gridPane.getSelectionModel().events.beforecellselect.obj.selection.record.data.value;
  	         copyToClipboard(val);
  	        
  	       }
  	     }
         }
      }
  });

	return gridPane;
}

function createXlogsPane(context, xlogsContainer, xlabel) {
	var xlogsUrl = 'xlogs' + context + '&xlabel=' + xlabel;
	var xlogsStore = createGridStore(xlogsContainer, xlogsUrl);

	xlogsStore
			.on(
					'load',
					function() {
						var xlogsPane = new Ext.grid.GridPanel(
								{
									id : 'xlogs-' + xlabel,
									store : xlogsStore,
 									cellValue:'',
									stripeRows : true,
									loadMask : true,
									cm : new Ext.grid.DynamicColumnModel(xlogsStore),
								selModel: new Ext.grid.CellSelectionModel({ singleSelect: true }),
									enableColLock : true,
									tbar : new Ext.Toolbar(
											{
												items : [
														{
															xtype : 'tbspacer',
															width : 4
														},
														{
															xtype : 'label',
															html : '<span style="font-weight:bold">Exchange Results</span>'
														},
														{
															xtype : 'tbspacer',
															width : 4
														},
														{
															xtype : 'button',
															icon : 'resources/images/16x16/view-refresh.png',
															tooltip : 'Refresh',
															handler : function() {
																xlogsStore
																		.load();
															}
														} ]
      }),
      listeners: {
          beforeedit: function(e){
            e.cancel = true;
          },
          cellclick: function(ts, td, cellIndex, record, tr, rowIndex, e, eOpts ){
        	  cellValue = record.target.innerText;
      	},
        keydown :function(evnt){
        	 var keyPressed = evnt.getKey();
    	     if (evnt.ctrlKey)
    	     {
    	       /*
    	        * After trial and error, the ctrl+c combination seems to be code 67 
    	        */
    	    	 if (67 == 67)//if (keyPressed == 67)
    	       {
    	         //var celldata = Ext.getCmp('property-pane').getSelectionModel().events.beforecellselect.obj.selection.record.data.value;
    	         copyToClipboard(cellValue);
    	        
    	       }
    	     }
           }
        }
    });

						if (xlogsContainer.items.length == 0) {
							xlogsContainer.add(xlogsPane);
							xlogsContainer.doLayout();
						} else {
							xlogsContainer.add(xlogsPane);
						}

						xlogsContainer.expand(false);
					});

	xlogsStore.load();
}

function createPageXlogs(scope, xid, xlabel, startTime, xtime, poolSize,
		itemCount) {
	var paneTitle = xlabel + ' (' + startTime + ')';
	var tab = Ext.getCmp('content-pane').getItem(paneTitle);

	if (tab != null) {
		tab.show();
	} else {
		var contentPane = Ext.getCmp('content-pane');
		contentPane.getEl().mask("Loading...", "x-mask-loading");

		var url = 'pageXlogs' + '?scope=' + scope + '&xid=' + xid + '&xtime='
				+ xtime + '&itemCount=' + itemCount;
		var store = createGridStore(contentPane, url);

		store.on('load', function() {
			var gridPane = createGridPane(store, poolSize, {
				forceFit : true
			}, false);

			var xlogsPagePane = new Ext.Panel({
				id : paneTitle,
				layout : 'fit',
				title : paneTitle,
				border : false,
				closable : true,
				items : [ gridPane ]
			});

			Ext.getCmp('content-pane').add(xlogsPagePane).show();
			Ext.getCmp('content-pane').getEl().unmask();
		});

		store.load({
			params : {
				start : 0,
				limit : poolSize
			}
		});
	}
}

function loadPageDto(type, action, context, label) {
	var tab = Ext.getCmp('content-pane').getItem('tab-' + label);

	if (tab != null) {
		tab.show();
	} else {
		var contentPane = Ext.getCmp('content-pane');
		contentPane.getEl().mask("Loading...", "x-mask-loading");

		var url = action + context;
		var store = createGridStore(contentPane, url);
		var pageSize = 25;

		store
				.on(
						'load',
						function() {
							if (Ext.getCmp('content-pane').getItem(
									'tab-' + label) == null) {
								var dtoBcPane = new Ext.Container(
										{
											id : 'bc-' + label,
											cls : 'bc-container',
											padding : '5',
											items : [ {
												xtype : 'box',
												autoEl : {
													tag : 'span',
													html : '<a class="breadcrumb" href="#" onclick="navigate(0)">'
															+ store.reader.description
															+ '</a>'
												}
											} ]
										});

								var dtoNavPane = new Ext.Panel({
									id : 'nav-' + label,
									region : 'north',
									layout : 'hbox',
									height : 26,
									items : [ dtoBcPane ]
								});

								if (type == 'exchange') {
									var dtoToolbar = new Ext.Toolbar(
											{
												cls : 'nav-toolbar',
												width:80,
												items : [
														{
															id : 'tb-exchange',
															xtype : 'button',
															tooltip : 'Send data to target endpoint',
															icon : 'resources/images/16x16/exchange-send.png',
															handler : function() {
																var xidIndex = context
																		.indexOf('&xid=');
																var scope = context
																		.substring(
																				7,
																				xidIndex);
																var xid = context
																		.substring(xidIndex + 5);
																var msg = 'Are you sure you want to exchange data \r\n['
																		+ label
																		+ ']?';
																var processUserResponse = submitExchange
																		.createDelegate([
																				label,
																				scope,
																				xid,
																				true ]);
																showDialog(
																		460,
																		125,
																		'Exchange Confirmation',
																		msg,
																		Ext.Msg.OKCANCEL,
																		processUserResponse);
															}
														},
														{
															xtype : 'tbspacer',
															width : 4
														},
														{
															id : 'tb-xlog',
															xtype : 'button',
															tooltip : 'Show/hide exchange results',
															icon : 'resources/images/16x16/history.png',
															handler : function() {
																var dtoTab = Ext
																		.getCmp(
																				'content-pane')
																		.getActiveTab();
																var xlogsContainer = dtoTab.items.map['xlogs-container-'
																		+ label];

																if (xlogsContainer.items.length == 0) {
																	createXlogsPane(
																			context,
																			xlogsContainer,
																			label);
																} else {
																	if (xlogsContainer.collapsed)
																		xlogsContainer
																				.expand(true);
																	else {
																		xlogsContainer
																				.collapse(true);
																	}
																}
															}
            },{
                xtype: 'tbspacer', 
                width: 4
              },{
                id: 'tb-dup',
                xtype: 'button',
                tooltip: 'Show Exchange Summary',
                icon: 'resources/images/16x16/file-table.png',
                handler: function(){
                	  var dtoTab = Ext.getCmp('content-pane').getActiveTab();
                	  var label = dtoTab.id.substring(4);   
                	  var xlogsUrl = 'sdata' + context + '&xlabel=' + label;//'xlogs' + context + '&xlabel=' + xlabel;
                	  var xlogsStore = createGridStore(xlogsContainer, xlogsUrl);
                	  xlogsStore.on('load', function(){
                          console.log(xlogsStore);
                          var startTime;
                          var endTime;
                          if(xlogsStore.data.items[0].data.StartTime!="")
                             startTime = xlogsStore.data.items[0].data.StartTime;
                          else
                        	 startTime = "00:00:00";
                        	  
                          if(xlogsStore.data.items[0].data.EndTime!="")
                        	  endTime = xlogsStore.data.items[0].data.EndTime;
                           else
                        	   endTime = "00:00:00";
                          
                    	  var summaryPane = new Ext.Panel({
                    	    //region: 'west',
                    	    id: 'summary-panel',
                    	    //bodyPadding: 5,
                    	    //title: 'Exchange Summary',
                    	    frame: true,
                    	    border: false,
                    	    //sytle:'margin:10 0 0 20;',
                    	    //split: true,
                    	    //width: 260,
                    	    height:400,
                    	    //minSize: 175,
                    	    //maxSize: 400,
                    	    //collapsible: true,
                    	    //layout: 'column',
                    	    layout:'column',
                   	       items: [ 
                    	             {
                    	            	 xtype:'panel',
                    	            	 height: 500,
                                  	     //width: 663,
                    	            	 padding:'15',
                    	            	 //bodyStyle:'margin:10 0 0 10;',
                    	            	 columnWidth: .3,
                    	            	 id:'label-container',
                    	            	 layout:'vbox',
                	            	     items: [ 
                	            	             {
                	            	    	        xtype:'label',
                	            	    	        style: 'font-weight:bold;',
                                	            	text:'Start Time:'
                                	              },
                                	              {
                                	            	  xtype: 'spacer',
                                	            	  height: 10
                                	            	},
                                	              {
                  	            	    	        xtype:'label',
                  	            	    	        style: 'font-weight:bold;',
                                  	            	text:'End Time:'
                                  	              },
                                  	              {
                                	            	  xtype: 'spacer',
                                	            	  height: 10
                                	            	},
                                  	           
                                  	              {
                  	            	    	        xtype:'label',
                  	            	    	        style: 'font-weight:bold;',
                                  	            	text:'Receiver Application:'
                                  	              },
                                  	              {
                                	            	  xtype: 'spacer',
                                	            	  height: 10
                                	            	},
                                  	              {
                  	            	    	        xtype:'label',
                  	            	    	        style: 'font-weight:bold;',
                                  	            	text:'Receiver Graph:'
                                  	              },
                                  	              {
                                	            	  xtype: 'spacer',
                                	            	  height: 10
                                	            	},
                                  	              {
	                	            	    	        xtype:'label',
	                	            	    	        style: 'font-weight:bold;',
	                                	            	text:'Receiver Scope:'
                                    	           },
                                    	           {
                                 	            	  xtype: 'spacer',
                                 	            	  height: 10
                                 	            	},
                                    	           {
                    	            	    	        xtype:'label',
                    	            	    	        style: 'font-weight:bold;',
                                    	            	text:'Receiver Uri:'
                                    	            },
                                    	            {
                                  	            	  xtype: 'spacer',
                                  	            	  height: 10
                                  	            	},
                                    	            {
                      	            	    	        xtype:'label',
                      	            	    	        style: 'font-weight:bold;',
                                      	            	text:'Sender Application:'
                                      	              },
                                      	              {
                                    	            	  xtype: 'spacer',
                                    	            	  height: 10
                                    	            	},
                                      	              {
                      	            	    	        xtype:'label',
                      	            	    	        style: 'font-weight:bold;',
                                      	            	text:'Sender Graph:'
                                      	              },
                                      	              {
                                    	            	  xtype: 'spacer',
                                    	            	  height: 10
                                    	            	},
                                      	              {
    	                	            	    	        xtype:'label',
    	                	            	    	        style: 'font-weight:bold;',
    	                                	            	text:'Sender Scope:'
                                        	           },
                                        	           {
                                     	            	  xtype: 'spacer',
                                     	            	  height: 10
                                     	            	},
                                        	           {
                        	            	    	        xtype:'label',
                        	            	    	        style: 'font-weight:bold;',
                                        	            	text:'Sender Uri:'
                                        	            },
                                        	            {
                                       	            	  xtype: 'spacer',
                                       	            	  height: 10
                                       	            	},
                                          	           {
                          	            	    	        xtype:'label',
                          	            	    	        style: 'font-weight:bold;',
                                          	            	text:'Pool Size:'
                                          	            },
                                          	           {
                                         	            	  xtype: 'spacer',
                                         	            	  height: 10
                                         	            	},
                                         	            	
                                                	           {
                                	            	    	        xtype:'label',
                                	            	    	        style: 'font-weight:bold;',
                                                	            	text:'Total Count:'
                                                	            },
                                                	            {
                                              	            	  xtype: 'spacer',
                                              	            	  height: 10
                                              	            	},
                                            	           {
                            	            	    	        xtype:'label',
                            	            	    	        style: 'font-weight:bold;',
                                            	            	text:'Adding Count:'
                                            	            },
                                            	            {
                                           	            	  xtype: 'spacer',
                                           	            	  height: 10
                                           	            	},
                                              	           {
                              	            	    	        xtype:'label',
                              	            	    	        style: 'font-weight:bold;',
                                              	            	text:'Deleting Count:'
                                              	            },
                                              	          {
                                           	            	  xtype: 'spacer',
                                           	            	  height: 10
                                           	            	},
                                              	           {
                              	            	    	        xtype:'label',
                              	            	    	        style: 'font-weight:bold;',
                                              	            	text:'Changing Count:'
                                              	            },
                                              	          {
                                           	            	  xtype: 'spacer',
                                           	            	  height: 10
                                           	            	},
                                              	           {
                              	            	    	        xtype:'label',
                              	            	    	        style: 'font-weight:bold;',
                                              	            	text:'Synchronized Count:'
                                              	            },
                                              	            {
                                             	            	  xtype: 'spacer',
                                             	            	  height: 10
                                             	            	},
                                                	           {
                                	            	    	        xtype:'label',
                                	            	    	        style: 'font-weight:bold;',
                                                	            	text:'Summary:'
                                                	            }
                	            	         ]
                    	              },
                    	              {
                     	            	 xtype:'panel',
                     	            	 columnWidth: .7,
                     	            	 padding:'15',
                     	            	 height: 500,
                                 	      //width: 663,
                     	            	 id:'value-container',
                     	            	 layout:'vbox',
                 	            	     items: [ 
                 	            	              {
			                    	            	 xtype:'readonlyfield',
			                    	            	 //fieldLabel:'My field',
			                    	            	 //sytle:'margin:0 0 0 20;',
			                    	            	 value:startTime//xlogsStore.data.items[0].data.StartTime
			                    	            	 //readOnly:true
                    	            	 
                 	            	              },
                 	            	              {
                                 	            	  xtype: 'spacer',
                                 	            	  height: 10
                                 	            	},
                 	            	              {
                                	            	 xtype:'readonlyfield',
                                	            	 //fieldLabel:'My field',
                                	            	 //sytle:'margin:0 0 0 20;',
                                	            	 value:endTime//xlogsStore.data.items[0].data.EndTime
                                	            	 //readOnly:true
                                	             },
                                	             {
                                 	            	  xtype: 'spacer',
                                 	            	  height: 10
                                 	            	},
                 	            	             {
	                               	            	 xtype:'readonlyfield',
	                               	            	 //fieldLabel:'My field',
	                               	            	 //sytle:'margin:0 0 0 20;',
	                               	            	 value:xlogsStore.data.items[0].data.ReceiverApp
	                               	            	 //readOnly:true
                               	                },
                               	                {
                               	            	  xtype: 'spacer',
                               	            	  height: 10
                               	            	},
                 	            	            {
	                              	            	 xtype:'readonlyfield',
	                              	            	 //fieldLabel:'My field',
	                              	            	 //sytle:'margin:0 0 0 20;',
	                              	            	 value:xlogsStore.data.items[0].data.ReceiverGraph
	                              	            	 //readOnly:true
                              	            	 
                              	             },
                              	              {
                            	            	  xtype: 'spacer',
                            	            	  height: 10
                            	            	},
                              	            {
                            	            	 xtype:'readonlyfield',
                            	            	 //fieldLabel:'My field',
                            	            	 //sytle:'margin:0 0 0 10;',
                            	            	 value:xlogsStore.data.items[0].data.ReceiverScope
                            	            	 //readOnly:true
                            	            	 
                            	             },
                            	              {
                            	            	  xtype: 'spacer',
                            	            	  height: 10
                            	            	},
                            	             {
                            	            	 xtype:'readonlyfield',
                            	            	 //fieldLabel:'My field',
                            	            	 //sytle:'margin:0 0 0 10;',
                            	            	 value:xlogsStore.data.items[0].data.ReceiverUri
                            	            	 //readOnly:true
                            	            	 
                            	             },
                            	             {
                            	            	  xtype: 'spacer',
                            	            	  height: 10
                            	            	},
            	            	             {
                              	            	 xtype:'readonlyfield',
                              	            	 //fieldLabel:'My field',
                              	            	 //sytle:'margin:0 0 0 20;',
                              	            	 value:xlogsStore.data.items[0].data.SenderApp
                              	            	 //readOnly:true
                          	                },
                          	                {
                          	            	  xtype: 'spacer',
                          	            	  height: 10
                          	            	},
            	            	            {
                             	            	 xtype:'readonlyfield',
                             	            	 //fieldLabel:'My field',
                             	            	 //sytle:'margin:0 0 0 20;',
                             	            	 value:xlogsStore.data.items[0].data.SenderGraph
                             	            	 //readOnly:true
                         	            	 
                         	             },
                         	              {
                       	            	  xtype: 'spacer',
                       	            	  height: 10
                       	            	},
                         	            {
                       	            	 xtype:'readonlyfield',
                       	            	//fieldLabel:'My field',
                       	            	 //sytle:'margin:0 0 0 10;',
                       	            	 value:xlogsStore.data.items[0].data.SenderScope
                       	            	 //readOnly:true
                       	            	 
                       	             },
                       	              {
                       	            	  xtype: 'spacer',
                       	            	  height: 10
                       	            	},
                       	             {
                       	            	 xtype:'readonlyfield',
                       	            	 //fieldLabel:'My field',
                       	            	 //sytle:'margin:0 0 0 10;',
                       	            	 value:xlogsStore.data.items[0].data.SenderUri
                       	            	 //readOnly:true
                       	            	 
                       	             },
                       	          {
                      	            	  xtype: 'spacer',
                      	            	  height: 10
                      	            	},
                        	            {
                      	            	 xtype:'readonlyfield',
                      	            	 //fieldLabel:'My field',
                      	            	 //sytle:'margin:0 0 0 10;',
                      	            	 value:xlogsStore.data.items[0].data.PoolSize
                      	            	 //readOnly:true
                      	            	 
                      	             },
                      	              {
                      	            	  xtype: 'spacer',
                      	            	  height: 10
                      	            	},
                      	             {
                      	            	 xtype:'readonlyfield',
                      	            	 //fieldLabel:'My field',
                      	            	 //sytle:'margin:0 0 0 10;',
                      	            	 value:xlogsStore.data.items[0].data.ItemCount
                      	            	 //readOnly:true
                      	            	 
                      	             },
                      	           {
                     	            	  xtype: 'spacer',
                     	            	  height: 10
                     	            	},
                     	             {
                     	            	 xtype:'readonlyfield',
                     	            	 //fieldLabel:'My field',
                     	            	 //sytle:'margin:0 0 0 10;',
                     	            	 value:xlogsStore.data.items[0].data.AddCount
                     	            	 //readOnly:true
                     	            	 
                     	             },
                     	            {
                     	            	  xtype: 'spacer',
                     	            	  height: 10
                     	            	},
                     	             {
                     	            	 xtype:'readonlyfield',
                     	            	//fieldLabel:'My field',
                     	            	 //sytle:'margin:0 0 0 10;',
                     	            	 value:xlogsStore.data.items[0].data.DeleteCount
                     	            	 //readOnly:true
                     	            	 
                     	             },
                     	            {
                     	            	  xtype: 'spacer',
                     	            	  height: 10
                     	            	},
                     	             {
                     	            	 xtype:'readonlyfield',
                     	            	 //fieldLabel:'My field',
                     	            	 //sytle:'margin:0 0 0 10;',
                     	            	 value:xlogsStore.data.items[0].data.ChangeCount
                     	            	 //readOnly:true
                     	            	 
                     	             },
                     	           {
                    	            	  xtype: 'spacer',
                    	            	  height: 10
                    	            	},
                    	             {
                    	            	 xtype:'readonlyfield',
                    	            	// fieldLabel:'My field',
                    	            	 //sytle:'margin:0 0 0 10;',
                    	            	 value:xlogsStore.data.items[0].data.SynchronizedCount
                    	            	 //readOnly:true
                    	            	 
                    	             },
                    	            {
                    	            	  xtype: 'spacer',
                    	            	  height: 10
                    	            	},
                    	             {
                    	            	 xtype:'readonlyfield',
                    	            	// fieldLabel:'My field',
                    	            	 //sytle:'margin:0 0 0 10;',
                    	            	 value:xlogsStore.data.items[0].data.Result
                    	            	 //readOnly:true
                    	            	 
                    	             }
                 	            	         ]
                     	              }
                    	            
                    	            ]
                    	  });
                    	
                    	 var win = new Ext.Window({
                   	      closable: true,
                   	      resizable: true,
                   	      //id: 'newwin-' + node.id,
                   	      modal: true,
                   	      //autoHeight:true,
                   	      layout: 'fit',
                   	      shadow: false,
                   	      title: 'Exchange Summary',
                   	      //iconCls: 'tabsApplication',
                   	      height: 500,
                   	      width: 750,
                   	      plain: true,
                   	      items: summaryPane,
                   	     /* listeners: {
                   	        beforelayout: function (pane) {
                   	          //alert('before layout..');
                   	          Ext.getBody().unmask();
                   	        }
                   	      }*/
                   	    });
                   	    win.show();
                	  });
                	  xlogsStore.load();
                	
                	/* var dtoTab = Ext.getCmp('content-pane').getActiveTab();
                     var xlogsContainer = dtoTab.items.map['xlogs-container-' + label]; 
                     var viewType = 'summary';
                     if (xlogsContainer.items.length == 0){
                       createXlogsPane(context, xlogsContainer, label,viewType);                
                     }
                     else {
                       if (xlogsContainer.collapsed)
                         xlogsContainer.expand(true);
                       else {         
                         xlogsContainer.collapse(true);
                       }
                     }*/
                }
              }
            
            ,{
              xtype: 'tbspacer', 
              width: 4
            }]
          });

									dtoNavPane.insert(0, dtoToolbar);
								}

								var dtoContentPane = new Ext.Panel({
									id : 'dto-' + label,
									region : 'center',
									layout : 'card',
									border : false,
									activeItem : 0,
									items : [ createGridPane(store, pageSize, {
										forceFit : false
									}, true) ],
									listeners : {
										afterlayout : function(pane) {
											Ext.getCmp('content-pane').getEl()
													.unmask();
										}
									}
								});

								var dtoTab = new Ext.Panel({
									id : 'tab-' + label,
									title : label,
									type : type,
									context : context,
									layout : 'border',
									closable : true,
									items : [ dtoNavPane, dtoContentPane ],
									listeners : {
										close : function(panel) {
											Ext.Ajax.request({
												url : 'reset?dtoContext='
														+ escape(panel.context
																.substring(1))
											});
										}
									}
								});

								if (type == 'exchange') {
									var xlogsContainer = new Ext.Panel({
										id : 'xlogs-container-' + label,
										region : 'south',
										layout : 'fit',
										border : false,
										height : 294,
										split : true,
										collapsed : true
									});

									dtoTab.add(xlogsContainer);
								}

								Ext.getCmp('content-pane').add(dtoTab).show();
							}
						});

		store.load({
			params : {
				start : 0,
				limit : pageSize
			}
		});
	}
}

function loadRelatedItem(type, context, individual, classId, className) {
	var url = context + '&individual=' + individual + '&classId=' + classId;

	if (type == 'app') {
		url = 'radata' + url;
	} else {
		url = 'rxdata' + url;
	}

	var contentPane = Ext.getCmp('content-pane');
	contentPane.getEl().mask("Loading...", "x-mask-loading");

	var store = createGridStore(contentPane, url);
	var pageSize = 25;

	store
			.on(
					'load',
					function() {
						var dtoTab = Ext.getCmp('content-pane').getActiveTab();
						var label = dtoTab.id.substring(4);
						var dtoBcPane = dtoTab.items.map['nav-' + label].items.map['bc-'
								+ label];

						// remove old bc and content pane on refresh
						var lastBcItem = dtoBcPane.items.items[dtoBcPane.items.length - 1].autoEl.html;
						if (removeHTMLTag(lastBcItem) == className) {
							navigate(dtoBcPane.items.length - 3);
						}

						var dtoContentPane = dtoTab.items.map['dto-' + label];
						var bcItemIndex = dtoBcPane.items.length + 1;

						dtoBcPane
								.add(
										{
											xtype : 'box',
											autoEl : {
												tag : 'img',
												src : 'resources/images/breadcrumb.png'
											},
											cls : 'breadcrumb-img'
										},
										{
											xtype : 'box',
											autoEl : {
												tag : 'span',
												html : '<a class="breadcrumb" href="#" onclick="navigate('
														+ bcItemIndex
														+ ')">'
														+ className + '</a>'
											}
										});

						dtoBcPane.doLayout();
						dtoContentPane.add(createGridPane(store, pageSize, {
							forceFit : false
						}, true));
						dtoContentPane.getLayout().setActiveItem(
								dtoContentPane.items.length - 1);
					});

	store.load({
		params : {
			start : 0,
			limit : pageSize
		}
	});
}

function removeHTMLTag(htmlText) {
	if (htmlText)
		return htmlText.replace(/<\/?[^>]+(>|$)/g, '');

	return '';
}

function findChangedValue(htmlText) {
	if (htmlText) {
		var value = htmlText.replace(/<\/?[^>]+(>|$)/g, '');
		var index = value.indexOf('->');
		if (index == -1) {
			return '';
		} else
			return value;
	} else
		return '';
}
function FindTransferType(htmlText) {
	if (htmlText) {
		// var value = htmlText.replace(/<\/?[^>]+(>|$)/g, '');
		var splits = htmlText.split('/');
		var resultType = splits[2].split('.');
		var transferType = resultType[0];
		return transferType;
	}
}

function navigate(bcItemIndex) {
	var dtoTab = Ext.getCmp('content-pane').getActiveTab();
	var label = dtoTab.id.substring(4);
	var dtoBcPane = dtoTab.items.map['nav-' + label].items.map['bc-' + label];
	var dtoContentPane = dtoTab.items.map['dto-' + label];

	// remove items on the right from nav pane
	while (bcItemIndex < dtoBcPane.items.items.length - 1) {
		dtoBcPane.items.items[bcItemIndex + 1].destroy();
	}

	// remove items on the right from dto content pane
	var contentItemIndex = bcItemIndex / 2;
	while (contentItemIndex < dtoContentPane.items.items.length - 1) {
		dtoContentPane.items.items[contentItemIndex + 1].destroy();
	}
	dtoContentPane.getLayout().setActiveItem(contentItemIndex);
}

function showStatus(duplicates) {
	/*
	 * var dtoTab = Ext.getCmp('content-pane').getActiveTab(); var label =
	 * dtoTab.id.substring(4); var dtoContentPane = dtoTab.items.map['dto-' +
	 * label]; var dtoGrid = dtoContentPane.getLayout().activeItem;
	 */
	if (duplicates > 1) {

	
		var propertyGrid = new Ext.grid.PropertyGrid({
			region : 'center',
			title : 'List of Errors ::--',
			split : true,
			stripeRows : true,
			autoScroll : true,
			source: {
		        "Duplicates": duplicates,
		      //  "Wrong Data":duplicates
		    }

		/*
		 * listeners : { beforeedit : function(e) { e.cancel = true; } }
		 */
		});

		var win = new Ext.Window({
			closable : true,
			resizable : true,
			// id: 'newwin-' + node.id,
			modal : true,
			// autoHeight:true,
			layout : 'fit',
			shadow : false,
			title : 'ERROR',
			// iconCls: 'tabsApplication',
			height : 200,
			width : 300,
			plain : true,
			items : [ propertyGrid ]
		/*
		 * listeners: { beforelayout: function (pane) { //alert('before
		 * layout..'); Ext.getBody().unmask(); } }
		 */
		});
		win.show();
		// dtoContentPane.add(win);
	}else{
	if(duplicates == 1)
		{
			alert("This Row is Good to Exchange");
		}
	else{
		if(duplicates == 0)
			{
			var propertyGrid = new Ext.grid.PropertyGrid({
				region : 'center',
				title : 'List of Warnings ::--',
				split : true,
				stripeRows : true,
				autoScroll : true,
				source: {
			        "Warnings": "Value",
			      //  "Wrong Data":duplicates
			    }
			});

			var win = new Ext.Window({
				closable : true,
				resizable : true,
				modal : true,
				layout : 'fit',
				shadow : false,
				title : 'WARNING',
				height : 200,
				width : 300,
				plain : true,
				items : [ propertyGrid ]
			});
			win.show();
			}
	}
	}

}

function showChangedItemsInfo() {
	var dtoTab = Ext.getCmp('content-pane').getActiveTab();
	var label = dtoTab.id.substring(4);
	var dtoBcPane = dtoTab.items.map['nav-' + label].items.map['bc-' + label];
	var dtoContentPane = dtoTab.items.map['dto-' + label];
	var dtoGrid = dtoContentPane.getLayout().activeItem;

	var rowData = dtoGrid.selModel.selections.map[dtoGrid.selModel.last].data;
	delete rowData['&nbsp;']; // remove info field
	var tansferType = {};
	var parsedRowData = {};
	for ( var colData in rowData) {
		if (colData == 'Transfer Type') {
			tansferType = FindTransferType(rowData[colData]);
		}
	}
	if (tansferType == 'change') {
		for ( var colData in rowData) {
			var value = findChangedValue(rowData[colData]);
			if (value != "")
				parsedRowData[colData] = value;
		}

		var propertyGrid = new Ext.grid.PropertyGrid({
			region : 'center',
			title : 'Properties of Changed Fields',
			split : true,
			stripeRows : true,
			autoScroll : true,
			source : parsedRowData,
			listeners : {
				beforeedit : function(e) {
					e.cancel = true;
				}
			}
		});

		var win = new Ext.Window({
			closable : true,
			resizable : true,
			// id: 'newwin-' + node.id,
			modal : true,
			// autoHeight:true,
			layout : 'fit',
			shadow : false,
			title : 'Transfer Type of Selected Row is "'
					+ tansferType.toUpperCase() + '"',
			// iconCls: 'tabsApplication',
			height : 300,
			width : 600,
			plain : true,
			items : [ propertyGrid ]
		/*
		 * listeners: { beforelayout: function (pane) { //alert('before
		 * layout..'); Ext.getBody().unmask(); } }
		 */
		});
		win.show();
		dtoContentPane.add(win);
	} else {
		alert("Selected row is '" + tansferType.toUpperCase() + "'");
		dtoContentPane.add(alert);
	}
}

function showIndividualInfo(individual, classIdentifier, relatedClasses) {
	var dtoTab = Ext.getCmp('content-pane').getActiveTab();
	var label = dtoTab.id.substring(4);
	var dtoBcPane = dtoTab.items.map['nav-' + label].items.map['bc-' + label];
	var dtoContentPane = dtoTab.items.map['dto-' + label];
	var dtoGrid = dtoContentPane.getLayout().activeItem;

	var classItemPane = new Ext.Container(
			{
				region : 'north',
				layout : 'fit',
				height : 44,
				cls : 'class-badge',
				html : '<div style="width:50px;float:left"><img style="margin:2px 5px 2px 5px" src="resources/images/class-badge-large.png"/></div>'
						+ '<div style="width:100%;height:100%"><table style="height:100%"><tr><td>'
						+ dtoGrid.description
						+ ' ('
						+ classIdentifier
						+ ')</td></tr></table></div>'
			});

	var rowData = dtoGrid.selModel.selections.map[dtoGrid.selModel.last].data;
	delete rowData['&nbsp;']; // remove info field

	var parsedRowData = {};
	for ( var colData in rowData)
		parsedRowData[colData] = removeHTMLTag(rowData[colData]);

	var propertyGrid = new Ext.grid.PropertyGrid({
		region : 'center',
		title : 'Properties',
		split : true,
		stripeRows : true,
		autoScroll : true,
		source : parsedRowData,
    listeners: {
      beforeedit: function(e){
        e.cancel = true;
      },
      click: function(){
      	//alert('clicked...');
      },
      keydown :function(evnt){
    	  //alert('keydown...');
      	 var keyPressed = evnt.getKey();
  	     if (evnt.ctrlKey)
  	     {
  	       /*
  	        * After trial and error, the ctrl+c combination seems to be code 67 
  	        */
  	    	 if (67 == 67)//if (keyPressed == 67)
  	       {
  	         var celldata = Ext.getCmp('property-pane').getSelectionModel().events.beforecellselect.obj.selection.record.data.value;
  	         copyToClipboard(celldata);
  	        
  	       }
  	     }
         }
    }

  });

	var relatedItemPane = new Ext.Panel({
		title : 'Related Items',
		region : 'east',
		layout : 'vbox',
		boxMinWidth : 100,
		width : 300,
		padding : '4',
		split : true,
		autoScroll : true
	});

	for ( var i = 0; i < relatedClasses.length; i++) {
		var dtoTabType = dtoTab.type;
		var dtoTabContext = dtoTab.context;
		var dtoIdentifier = individual;
		var relatedClassId = relatedClasses[i].id;
		var relatedClassName = relatedClasses[i].name;

		relatedItemPane
				.add({
					xtype : 'box',
					autoEl : {
						tag : 'div',
						html : '<a class="breadcrumb" href="#" onclick="loadRelatedItem(\''
								+ dtoTabType
								+ '\',\''
								+ dtoTabContext
								+ '\',\''
								+ dtoIdentifier
								+ '\',\''
								+ relatedClassId
								+ '\',\''
								+ relatedClassName
								+ '\')">' + relatedClassName + '</a>'
					}
				});
	}

	var individualInfoPane = new Ext.Panel({
		autoWidth : true,
		layout : 'border',
		border : false,
		items : [ classItemPane, propertyGrid, relatedItemPane ]
	});

	var bcItemIndex = dtoBcPane.items.length + 1;

	dtoBcPane.add({
		xtype : 'box',
		autoEl : {
			tag : 'img',
			src : 'resources/images/breadcrumb.png'
		},
		cls : 'breadcrumb-img'
	}, {
		xtype : 'box',
		autoEl : {
			tag : 'span',
			html : '<a class="breadcrumb" href="#" onclick="navigate('
					+ bcItemIndex + ')">' + classIdentifier + '</a>'
		}
	});
	dtoBcPane.doLayout();

	dtoContentPane.add(individualInfoPane);
	dtoContentPane.getLayout().setActiveItem(dtoContentPane.items.length - 1);
}

function getFilters() {
	var dtoTab = Ext.getCmp('content-pane').getActiveTab();
	var label = dtoTab.id.substring(4);
	var dtoContentPane = dtoTab.items.map['dto-' + label];

	var gridFilters = new Array();

	for ( var i = 0; i < dtoContentPane.items.length; i = i + 2) {
		var gridFilter = dtoContentPane.items.items[i].plugins[0];
		var filterData = gridFilter.getFilterData();

		if (filterData.length > 0) {
			var filterQuery = gridFilter.buildQuery(filterData);
			gridFilters.push(filterQuery.filter);
		}
	}

	return gridFilters;
}

function submitExchange(userResponse) {
	var exchange = this[0];
	var scope = this[1];
	var xid = this[2];
	var reviewed = this[3];

	if (userResponse == 'ok') {
		Ext.getCmp('content-pane').getItem('tab-' + exchange).getEl().mask(
				'Exchange in progress, please wait ...', 'x-mask-loading');

		Ext.Ajax.request({
			url : 'xsubmit?scope=' + scope + '&xid=' + xid + '&reviewed='
					+ reviewed,
			timeout : 86400000, // 24 hours
			success : function(response, request) {
				Ext.getCmp('content-pane').getItem('tab-' + exchange).getEl()
						.unmask();

				var responseText = Ext.decode(response.responseText);
				var message = 'Data exchange [' + exchange + ']: '
						+ responseText;

				if (message.length < 300)
					showDialog(460, 125, 'Exchange Result', message,
							Ext.Msg.OK, null);
				else
					showDialog(660, 300, 'Exchange Result', message,
							Ext.Msg.OK, null);
			},
			failure : function(response, request) {
				// ignore timeout error from proxy server
				if (response.responseText.indexOf('Error Code 1460') != -1) {
					Ext.getCmp('content-pane').getItem('tab-' + exchange)
							.getEl().unmask();

					var title = 'Exchange Error (' + response.status + ')';
					var message = 'Error while exchanging [' + exchange + '].';

					var responseText = Ext.decode(response.responseText);

					if (responseText)
						message += responseText;

					showDialog(660, 300, title, message, Ext.Msg.OK, null);
				}
			}
		});
	}
}

function showDialog(width, height, title, message, buttons, callback) {
	var style = 'style="margin:0;padding:0;width:' + width + 'px;height:'
			+ height + 'px;border:1px solid #aaa;overflow:auto"';

	Ext.Msg.show({
		title : title,
		msg : '<textarea ' + style + ' readonly="yes">' + message
				+ '</textarea>',
		buttons : buttons,
		fn : callback
	});
}

Ext
		.onReady(function() {
			Ext.state.Manager.setProvider(new Ext.state.CookieProvider());
			Ext.QuickTips.init();

			Ext.get('about-link').on('click', function() {
				var win = new Ext.Window({
					title : 'About Exchange Manager',
					bodyStyle : 'background-color:white;padding:5px',
					width : 700,
					height : 500,
					closable : true,
					resizable : false,
					autoScroll : true,
					buttons : [ {
						text : 'Close',
						handler : function() {
							Ext.getBody().unmask();
							win.close();
						}
					} ],
					autoLoad : 'about-exchange-manager.jsp',
					listeners : {
						close : {
							fn : function() {
								Ext.getBody().unmask();
							}
						}
					}
				});

				Ext.getBody().mask();
				win.show();
			});

			var headerPane = new Ext.BoxComponent({
				region : 'north',
				height : 55,
				contentEl : 'header'
			});

			var directoryTreePane = new Ext.tree.TreePanel(
					{
						id : 'directory-tree',
						region : 'center',
						dataUrl : 'directory',
						width : 800,
						lines : true,
						autoScroll : true,
						border : false,
						animate : true,
						enableDD : false,
						containerScroll : true,
						rootVisible : true,
						tbar : new Ext.Toolbar(
								{
									items : [
											{
												id : 'refresh-button',
												xtype : 'button',
												icon : 'resources/images/16x16/view-refresh.png',
												text : 'Refresh',
												handler : function() {
													var directoryTree = Ext
															.getCmp('directory-tree');
													var contentPane = Ext
															.getCmp('content-pane');

													// clear dto tabs
													while (contentPane.items.length > 0) {
														contentPane.items.items[0]
																.destroy();
													}

													// clear property grid
													Ext.getCmp('property-pane')
															.setSource({});

													// disable toolbar buttons
													Ext.getCmp(
															'exchange-button')
															.disable();
													Ext.getCmp('xlogs-button')
															.disable();

													// reload tree
													directoryTree
															.getLoader()
															.load(
																	directoryTree.root);
													directoryTree.getRootNode()
															.expand(false);
												}
											},
											{
												id : 'exchange-button',
												xtype : 'button',
												icon : 'resources/images/16x16/exchange-send.png',
												text : 'Exchange',
												disabled : true,
												handler : function() {
													var node = Ext
															.getCmp(
																	'directory-tree')
															.getSelectionModel()
															.getSelectedNode();
													var scope = node.parentNode.parentNode.parentNode.attributes['text'];
													var exchange = node.attributes["text"];
													var xid = node.attributes.properties['Id'];
													var reviewed = (node.reviewed != undefined);
													var msg = 'Are you sure you want exchange data \r\n['
															+ exchange + ']?';
													var processUserResponse = submitExchange
															.createDelegate([
																	exchange,
																	scope, xid,
																	reviewed ]);
													showDialog(
															460,
															125,
															'Exchange Confirmation',
															msg,
															Ext.Msg.OKCANCEL,
															processUserResponse);
												}
											},
											{
												// TODO: TBD
												id : 'xlogs-button',
												xtype : 'button',
												icon : 'resources/images/16x16/history.png',
												text : 'History',
												disabled : true,
												hidden : true,
												handler : function() {
													alert('Show exchange log');
												}
											} ]
								}),
						root : {
							nodeType : 'async', // only load child nodes as
							// needed
							text : 'Directory',
							icon : 'resources/images/directory.png'
						},
						listeners : {
							click : function(node, event) {
								Ext.getCmp('property-pane').setSource(
										node.attributes.properties);

								try {
									var dataTypeNode = node.parentNode.parentNode;

									if (dataTypeNode != null
											&& dataTypeNode.attributes['text'] == 'Data Exchanges') {
										Ext.getCmp('exchange-button').enable();
										Ext.getCmp('xlogs-button').enable();
									} else {
										Ext.getCmp('exchange-button').disable();
										Ext.getCmp('xlogs-button').disable();
									}
								} catch (err) {
								}
							},
							dblclick : function(node, event) {
								var properties = node.attributes.properties;
								Ext.getCmp('property-pane').setSource(
										node.attributes.properties);

								try {
									var dataTypeNode = node.parentNode.parentNode;

									if (dataTypeNode != null) {
										if (dataTypeNode.attributes['text'] == 'Application Data') {
											var graphNode = node.parentNode;
											var scope = properties['Context'];
											var app = graphNode.attributes['text'];
											var graph = node.attributes['text'];
											var baseUri = properties['Base URI'];
											var label = scope + '.' + app + '.'
													+ graph;
											var context = '?baseUri=' + baseUri
													+ '&scope=' + scope
													+ '&app=' + app + '&graph='
													+ graph;

											loadPageDto('app', 'adata',
													context, label);
										} else if (dataTypeNode.attributes['text'] == 'Data Exchanges') {
											var scope = dataTypeNode.parentNode.attributes['text'];
											var exchangeId = properties['Id'];
											var context = '?scope=' + scope
													+ '&xid=' + exchangeId;

											node.reviewed = true;
											loadPageDto('exchange', 'xdata',
													context, node.text);
										}
									}
								} catch (err) {}
      },
      keydown :function(evnt){
      	 // alert('keydown...');
        	 var keyPressed = evnt.getKey();
    	     if (evnt.ctrlKey)
    	     {
    	       /*
    	        * After trial and error, the ctrl+c combination seems to be code 67 
    	        */
    	    	 if (67 == 67)//if (keyPressed == 67)
    	       {
    	         var celldata = Ext.getCmp('property-pane').getSelectionModel().events.beforecellselect.obj.selection.record.data.value;
    	         copyToClipboard(celldata);
								}
							}
						}
    }
  });

			var propertyPane = new Ext.grid.PropertyGrid({
				id : 'property-pane',
				title : 'Details',
				region : 'south',
				height : 250,
				layout : 'fit',
				collapsible : true,
				stripeRows : true,
				autoScroll : true,
				border : false,
				split : true,
				source : {},
    listeners: {
      beforeedit: function(e){
        e.cancel = true;
      },
    click: function(){
    	//alert('clicked...');
    },
    keydown :function(evnt){
    	 var keyPressed = evnt.getKey();
	     if (evnt.ctrlKey)
	     {
	       /*
	        * After trial and error, the ctrl+c combination seems to be code 67 
	        */
	    	 if (67 == 67)//if (keyPressed == 67)
	       {
	         var celldata = Ext.getCmp('property-pane').getSelectionModel().events.beforecellselect.obj.selection.record.data.value;
	         copyToClipboard(celldata);
	        
	       }
	     }
       }
    }
  });

			var directoryPane = new Ext.Panel({
				region : 'west',
				id : 'west-panel',
				title : 'Directory',
				frame : false,
				border : false,
				split : true,
				width : 260,
				minSize : 175,
				maxSize : 400,
				collapsible : true,
				// margins: '0 0 0 4',
				layout : 'border',
				items : [ directoryTreePane, propertyPane ]
			});

			var contentPane = new Ext.TabPanel({
				id : 'content-pane',
				region : 'center',
				deferredRender : false,
				enableTabScroll : true,
				border : true,
				activeItem : 0
			});

			var viewport = new Ext.Viewport({
				layout : 'border',
				items : [ headerPane, directoryPane, contentPane ]
			});

			directoryTreePane.getRootNode().expand(false);
		});
