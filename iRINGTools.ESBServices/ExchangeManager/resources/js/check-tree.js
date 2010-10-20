/*
 * @File Name : check-tree.js
 * @Path : resources/js
 * @Using Lib : Ext JS Library 3.2.1(lib/ext-3.2.1)
 * 
 * This file intended to make Directory Tree Panel of Exchange Manager
 * The data of tree nodes comes form the src/controller/ExchangeReader/ExchangeReader.class.php
 * in the form of JSON String
 * 
 */

var reviewed,tree
   
function showgrid(response, request,label,nodeid){
	reviewed=true
	var jsonData = Ext.util.JSON.decode(response);
	if(eval(jsonData.success)==false)
	{
		Ext.getCmp('centerPanel').disable();
		Ext.MessageBox.show({
		title: '<font color=red>Error</font>',
		msg: 'No Exchange Results found for:<br/>'+label,
		buttons: Ext.MessageBox.OK,
		icon: Ext.MessageBox.ERROR
		});
		return false;
	}else{
	var rowData = eval(jsonData.rowData);
	var filedsVal = eval(jsonData.headersList);
	var columnData = eval(jsonData.columnsData);
    var classObjName = jsonData.classObjName;
	// create the data store
	var store = new Ext.data.ArrayStore({
	fields: filedsVal
	});
	store.loadData(rowData);

	if(grid){
		alert('Going to destroy...')
		grid.destroy();                                  
	}
	// create the Grid
	var grid = new Ext.grid.GridPanel({
	store: store,
	columns: columnData,
	stripeRows: true,
	//viewConfig: {forceFit:true},
	id:label,
	loadMask: true,
	layout:'fit',
	frame:true,
	autoSizeColumns: true,
	autoSizeGrid: true,
        AllowScroll : true,
	minColumnWidth:100, 
	columnLines: true,
	classObjName:classObjName,
	//autoWidth:true,
        enableColumnMove:false,
        listeners: {
                 cellclick:{
                       fn:function(grid,rowIndex,columnIndex,e){
                           var cm    = this.colModel
                           var record = grid.getStore().getAt(rowIndex);  // Get the Record
                           var fieldName = grid.getColumnModel().getDataIndex(columnIndex); // Get field name
                           if(fieldName=='IdentificationByTag'){
                                var IdentificationByTag_value = record.get(fieldName);
                                var transferType_value = record.get('TransferType');
                                var rowDataArr = []
                                 for(var i=2; i<cm.getColumnCount();i++){
                                     fieldHeader= grid.getColumnModel().getColumnHeader(i); // Get field name
                                     fieldValue= record.get(grid.getColumnModel().getDataIndex(i))

                                     tempArr= Array(fieldHeader,fieldValue)
                                     rowDataArr.push(tempArr)
                                 }

                                   var filedsVal_ = '[{"name":"Property"},{"name":"Value"}]'
                                   var columnsData_='[{"id":"Property","header":"Property","width":144,"sortable":"true","dataIndex":"Property"},{"id":"Value","header":"Value","width":144,"sortable":"true","dataIndex":"Value"}]'
                                   var prowData = eval(rowDataArr);
                                   var pfiledsVal = eval(filedsVal_);
                                   var pColumnData = eval(columnsData_);
                                   // create the data store
                                   var pStore = new Ext.data.ArrayStore({
                                        fields: pfiledsVal
                                   });
                                   pStore.loadData(prowData);
                                   showIndvidualClass(pStore,pColumnData)

                                   Ext.get('identifier-class-detail').dom.innerHTML = '<div style="float:left; width:110px;"><img src="resources/images/class-badge.png"/></div><div style="padding-top:20px;" id="identifier"><b>'+removeHTMLTags(IdentificationByTag_value)+'</b><br/>'+grid.classObjName+'<br/>Transfer Type : '+transferType_value+'</div>'
                                }

                     }
                    }
               // cellclick : function( Grid this, Number rowIndex, Number columnIndex, Ext.EventObject e )
         },
	tbar:new Ext.Toolbar({
	xtype: "toolbar",
	items:[{
		xtype:"tbbutton",
		icon:'resources/images/16x16/go-send.png',
		tooltip:'Exchange Data',
		disabled: false,
		handler:function(){
                //promptReviewAcceptance();
	   makeLablenURI();
	   submitDataExchange(globalReq);
	  }
	 },
	{
	xtype:"tbbutton",
	icon:'resources/images/16x16/view-refresh.png',
	tooltip:'Reload',
	disabled: false,
	handler:function(){
	  //promptReviewAcceptance();
	  makeLablenURI_tabrefresh();
	  //alert(globalLabel);
	  //alert(globalReq+' '+globalTreenode);
	  showCentralGrid(globalTreenode);
	  // send Request to destroy session
	  }
	  }]
	})
	});

        //make the text selectable in cells of Grid
        Ext.override(Ext.grid.GridView, {
        templates: {
        cell: new Ext.Template(
            '<td class="x-grid3-col x-grid3-cell x-grid3-td-{id} {css}" style="{style}" tabIndex="0" {cellAttr}>',
            '<div class="x-grid3-cell-inner x-grid3-col-{id}" {attr}>{value}</div>',
            "</td>"
            )
            }
        });

        if(Ext.getCmp('centerPanel').findById('tab-'+label)){
                Ext.getCmp('centerPanel').remove(Ext.getCmp('centerPanel').findById('tab-'+label));
        }
		
		
	Ext.getCmp('centerPanel').add( 
	Ext.apply(grid,{
	id:'tab-'+label,
	text:nodeid,
	title: label,
	closable:true
	})).show();
}
}

function sendAjaxRequest(requestURL,label,nodeid){
	Ext.getBody().mask('Loading...');
	Ext.Ajax.request({
		url : requestURL,
		method: 'POST',
		params: {
		/*nodeid: node.id,
		newparentid: newParent.id,
		oldparentid: oldParent.id,
		dropindex: index
		*/
		},
		success: function(result, request)
		{ 
			showgrid(result.responseText,request,label,nodeid);
		},
		failure: function ( result, request){ 
			//alert(result.responseText); 
		},
		callback: function() {Ext.getBody().unmask();}
	})
}

Ext.onReady(function(){
    Ext.QuickTips.init();
    var tBar = new Ext.Toolbar({
	xtype: "toolbar",
        items: [
		   {
            xtype:"tbbutton",
            icon:'resources/images/16x16/view-refresh.png',
            tooltip:'Refresh',
            id: 'headRefresh', 
            disabled: false,
            handler: function(){
                Ext.state.Manager.clear("treestate");    
                tree.root.reload();
            }
		   },
		   {
            xtype:"tbbutton",
            icon:'resources/images/16x16/go-send.png',
            tooltip:'Exchange Data',
            disabled: false,
            handler: function(){
					makeLablenURI();
					var treenode=  globalTreenode;
				  	if(treenode!=null){
						var label=  globalLabel;
						var requestURL = globalReq;

								if(!Ext.getCmp(label)){
									promptReviewAcceptance(requestURL);
								}else{
											Ext.Msg.show({
											msg: 'Thanks for your review & acceptance. Want to transfer the data now?',
											buttons: Ext.Msg.YESNO,
											icon: Ext.Msg.QUESTION,//'profile', // &lt;- customized icon
											fn: function(action){
													 if(action=='yes'){
														 submitDataExchange(requestURL);
													 }else if(action=='no'){
														 alert('Not now');
													 }
												 }
											});
								}
					}else{
						  Ext.MessageBox.show({
							msg: 'Please Select an Exchange First<br/>',
							buttons: Ext.MessageBox.OK,
							icon: Ext.MessageBox.WARNING
						  });
						  return false;
					  }
                }
        },
        {
			// For open button
			xtype:"tbbutton",
            icon:'resources/images/16x16/document-open.png',
            id: 'headExchange',
            tooltip:'Open',
            disabled: false,
            handler: function(){
					  showCentralGrid(tree.getSelectionModel().getSelectedNode());
            }
	}

    ]});
	
    tree = new Ext.tree.TreePanel({
    id:'directory-tree',
    renderTo:'tree-div',
    height: 494,
    baseCls : 'x-plain',
    bodyBorder:false,
    border:false,
    hlColor:'C3DAF',
    //width: 250,
    layout:'fit',
    useArrows:false, // true for vista like
    autoScroll:true,
    animate:true,
    margins: '0 0 0 0',
    lines :true,
    containerScroll: true,
    rootVisible: true,
    root: {
      nodeType: 'async',
      icon: 'resources/images/16x16/internet-web-browser.png',
      text: 'Directory'
    },
    // auto create TreeLoader
    dataUrl: 'ExchangeReader/exchnageList/1',
    tbar:tBar,
    listeners: {
        BeforeLoad:{
           fn:function(){
             // Basic mask:
              Ext.getCmp('directory-tree').el.mask('Loading...', 'x-mask-loading')            
             }
           },
        load:{
           fn:function(){
             // Basic unmask:
             Ext.getCmp('directory-tree').el.unmask()
           }
        },
        click: {
         fn: function(node){
             //get all the attributes of node
             obj = node.attributes
             var details_data = []
               for(var key in obj){
				 //alert(key+' '+obj[key])
                 // restrict some of the properties to be displayed
                 if(key!='node_type' && key!='uid' && key!='id' && key!='text' && key!='icon' && key!='children' && key!='loader' && key!='leaf' && key!='applicationId'){
                    details_data[key]=obj[key]
                 }  
               }
                
               // get the property grid component
                var propGrids = Ext.getCmp('propGrid');
                // make sure the property grid exists
                if (propGrids) {
                  // populate the property grid with details_data
                  propGrids.setSource(details_data);
                }

             // check the current state of Detail Grid panel
             if(Ext.getCmp('detail-grid').collapsed==true){
                 Ext.getCmp('detail-grid').expand();
              }             
          }
        },
        dblclick :{
		fn : function (node){
                   showCentralGrid(node);
                }
        },
        expandnode:{
            fn : function (node){
                Ext.state.Manager.set("treestate", node.getPath())
            }
        }
    }
  });

  var contextMenu = new Ext.menu.Menu({
    items: [
        {
          text: 'Sort',
          handler: sortHandler
        }
    ]
  });
  function sortHandler() {
    tree.getSelectionModel().getSelectedNode().sort(
      function (leftNode, rightNode) {
        return 1;
      }
      );
  }

  /* to maintain the state of the tree */
  Ext.state.Manager.setProvider(new Ext.state.CookieProvider());
  tree.on('contextmenu', function (node){
    node.select();
    contextMenu.show(node.ui.getAnchor());
  });

/* to maintain the state of the tree */
  var treeState = Ext.state.Manager.get("treestate");
  if (treeState){
	  if(tree.expandPath(treeState)){ //check the
		  tree.expandPath(treeState);
	  }else{
		  Ext.state.Manager.clear("treestate");
		  tree.root.reload();
	  }
  }
});

function promptReviewAcceptance(requestURL){
	Ext.Msg.show({
	msg: 'Would you like to review the <br/>Data Exchange before starting?',
	buttons: Ext.Msg.YESNO,
	icon: Ext.Msg.QUESTION,//'profile', // &lt;- customized icon
	fn: function(action){
		 if(action=='yes'){
			 var node = tree.getSelectionModel().getSelectedNode();
			 showCentralGrid(node);
		 }
		 else if(action=='no')
		 {
			 reviewed=false;
			 //alert('You clicked on No');
			 submitDataExchange(requestURL);
		 }
	 }});
}

function submitDataExchange(requestURL){
	if(reviewed){
		var w = Ext.getCmp('centerPanel').getActiveTab();
		w.getEl().mask('Loading.....');
	}else{
		Ext.getCmp('centerPanel').enable();
		Ext.getCmp('centerPanel').getEl().mask('Loading.....');
	}
	Ext.Ajax.request({
	url : requestURL,
	method: 'POST',
	params: {
		hasreviewed: reviewed
	  },
	success: function(result, request)
	  {
		  //alert(result.responseText);
		  var jsonData = Ext.util.JSON.decode(result.responseText);
		  //alert(jsonData.response);
		  if(eval(jsonData.success)==false){
			  alert(jsonData.response);
		  }else if(eval(jsonData.success)==true){
			  showResultPanel(jsonData);
		  }
	  },
	failure: function ( result, request){ 
			alert(result.responseText); 
	  },
	callback: function() {if(w){
		 w.getEl().unmask();
	  }else{
	  		Ext.getCmp('centerPanel').getEl().unmask();}
	  }
	})
}

function showResultPanel(jsonData){
	var rowData = eval(jsonData.rowData);
	var filedsVal = eval(jsonData.headersList);
	var store = new Ext.data.ArrayStore({
	fields: filedsVal
	});
	store.loadData(eval(rowData));
	
	var label=  globalLabel;
	var columnData = eval(jsonData.columnsData);
			var grid = new Ext.grid.GridPanel({
			store: store,
			columns: columnData,
			stripeRows: true,
			id:'exchangeResultGrid_'+label,
			loadMask: true,
			layout:'fit',
			frame:true,
			autoSizeColumns: true,
			autoSizeGrid: true,
			AllowScroll : true,
			minColumnWidth:100, 
			columnLines: true,
			autoWidth:true,
			enableColumnMove:false
			});			   

			if(Ext.getCmp('centerPanel').findById('tabResult-'+label)){
				//alert('aleready exists')
				Ext.getCmp('centerPanel').remove(Ext.getCmp('centerPanel').findById('tabResult-'+label));
				Ext.getCmp('centerPanel').add( 
				Ext.apply(grid,{
				id:'tabResult-'+label,
				title: label+'(Result)',
				closable:true
				})).show();
			}else{

				Ext.getCmp('centerPanel').add( 
				Ext.apply(grid,{
				id:'tabResult-'+label,
				title: label+'(Result)',
				closable:true
				})).show();
			}
}

function showCentralGrid(node)
{
	var obj = node.attributes
			  var eid
			  var label
			  var requestURL
			  var scopeId  = obj['Scope']
			  var nodeType = obj['node_type']
		if((obj['node_type']=='exchanges' && obj['uid']!='')){
			  eid = obj['uid']
			  requestURL = 'dataObjects/getDataObjects/'+nodeType+'/'+scopeId+'/'+eid
			  label = scopeId+'->'+node.text
		}else if(obj['node_type']=='graph'){
				requestURL = 'dataObjects/getGraphObjects/'+nodeType+'/'+scopeId+'/'+node.parentNode.text+'/'+obj['text']
				label = scopeId+'->'+node.parentNode.text+'->'+obj['text']
		}else{

			Ext.MessageBox.show({
			//title: '<font color=yellow>Warning</font>',
			msg: 'You can review only Data Exchange & Graphs in this Version<br/>',
			buttons: Ext.MessageBox.OK,
			icon: Ext.MessageBox.WARNING
			});
			return false;
		}
		  /*
             check the id of the tab
             if it's available then just display the tab & don't send ajax request
          */

		if((!Ext.getCmp(label))){
			if(node.id!=null)
			{
				Ext.getCmp('centerPanel').enable();
				sendAjaxRequest(requestURL,label,node.id);
				  // check the current state of Detail Grid panel
				if(Ext.getCmp('detail-grid').collapsed!=true){
					Ext.getCmp('detail-grid').collapse();

				}
			}
		}else if(Ext.getCmp(label)&&(Ext.getCmp('centerPanel').getActiveTab().id=='tab-'+label)){
				//alert('send request')
				//Ext.getCmp('centerPanel').remove(Ext.getCmp('centerPanel').findById('tabResult-'+label));
				sendAjaxRequest(requestURL,label,node.id);
		}else{
			Ext.getCmp('centerPanel').enable();
		// collapse the detail Grid panel & show the tab
			if(Ext.getCmp('detail-grid').collapsed!=true){
				Ext.getCmp('detail-grid').collapse();
			}
			Ext.getCmp(label).show();
		}

}

var globalLabel,globalReq,globalTreenode

function makeLablenURI(){

	// setting the node id in text of during the Result Grid creation

	if(Ext.getCmp('centerPanel').getActiveTab()){
		var nodeid = Ext.getCmp('centerPanel').getActiveTab().text;
		if(nodeid){
			tree.getSelectionModel().select(tree.getNodeById(nodeid));
		}
	}
	
	
	globalTreenode = tree.getSelectionModel().getSelectedNode();
	if(globalTreenode!=null){
		var obj = globalTreenode.attributes
		var scopeId  = obj['Scope']
		var nodeType = obj['node_type']

	    if((obj['node_type']=='exchanges' && obj['uid']!='')){
		  globalReq = 'dataObjects/setDataObjects/'+nodeType+'/'+scopeId+'/'+obj['uid']
		  globalLabel = scopeId+'->'+globalTreenode.text
		}else if(obj['node_type']=='graph'){
			globalLabel = scopeId+'->'+globalTreenode.parentNode.text+'->'+obj['text']
			globalReq = 'dataObjects/setGraphObjects/'+nodeType+'/'+scopeId+'/'+globalTreenode.parentNode.text+'/'+obj['text']

		}
	}
}

function makeLablenURI_tabrefresh(){
	//alert('tab id:'+Ext.getCmp('centerPanel').getActiveTab().id)
	// setting the node id in text of during the Result Grid creation
	var nodeid = Ext.getCmp('centerPanel').getActiveTab().text;
	if(nodeid){
		tree.getSelectionModel().select(tree.getNodeById(nodeid));
	}
	globalTreenode = tree.getSelectionModel().getSelectedNode();
	if(globalTreenode!=null){
		var obj = globalTreenode.attributes
	    var scopeId  = obj['Scope']
		var nodeType = obj['node_type']

		if((obj['node_type']=='exchanges' && obj['uid']!='')){
			globalReq = 'dataObjects/getDataObjects/'+nodeType+'/'+scopeId+'/'+obj['uid']
						globalLabel = scopeId+'->'+globalTreenode.text
		}else if(obj['node_type']=='graph'){
			globalLabel = scopeId+'->'+globalTreenode.parentNode.text+'->'+obj['text']
			globalReq = 'dataObjects/getGraphObjects/'+nodeType+'/'+scopeId+'/'+globalTreenode.parentNode.text+'/'+obj['text']
		}
	}
}

function showIndvidualClass(pStore,pColumnData){

        if(grid_class_properties){
		alert('Going to destroy...')
		grid_class_properties.destroy();
	}
	// create the Grid
	var grid_class_properties = new Ext.grid.GridPanel({
	store: pStore,
	columns: pColumnData,
	stripeRows: true,
	//viewConfig: {forceFit:true},
	//id:label,
	loadMask: true,
	//layout:'fit',
	//frame:true,
        height:360,
	autoSizeColumns: true,
	autoSizeGrid: true,
        AllowScroll : true,
	minColumnWidth:100,
	columnLines: true,

	//autoWidth:true,
        enableColumnMove:false
	});


        var item1 = new Ext.Panel({
                title: '<img src="resources/images/16x16/class-badge.png"/>Accordion Item 1',
                html: '<ul class="ja-col2"><li>item1</li><li>item2</li><li>item3</li></ul>',
                //frame:true,
                split:true,
                cls:'empty'
            });
            var item2 = new Ext.Panel({
                title: '<img src="resources/images/16x16/class-badge.png"/>Accordion Item2',
                html: '<ul class="ja-col2"><li>item1</li><li>item2</li><li>item3</li><li>item4</li><li>item5</li><li>item6</li></ul>',
                //frame:true,
                cls:'empty'
            });

 var internal_panel = new Ext.Panel({
            layout :'border',
           // layoutConfig: {columns:2},

            items:[{
                //title: 'Nested Layout',
                layout: 'border',
                border: false,
                //margins: '15 15 15 15',
                height: 100,
                items: [{
                    id:'identifier-class-detail',
                    region: 'north',
                    height: 100,
                    split: true,
                    html: 'Class Detail'
                    },{
                    region: 'center',
                    layout: 'border',
                    border: false,
                    margins: '0 3 3 3',
                        items: [{
                            id:'identifier-class-properties',
                            title: 'Properties',
                            region:'west',
                            split: true,
                            margins: '0 1 3 3',
                            width: 220,
                            minSize: 100,
                            items:[grid_class_properties]
                        },{
                            title: 'Related Items',
                            layout:'accordion',
                            split: true,
                            region: 'center',
                            margins: '0 3 3 0',
                            defaults: {
                                // applied to each contained panel
                               // bodyStyle: 'margin:0 0 0 15'
                            },
                            layoutConfig: {
                                // layout-specific configs go here
                                animate: true,
                                fill : false
                            },
                            items: [item1, item2]
                        }]
                    }]
                }]


        });
        Ext.getCmp('centerPanel').add(
	Ext.apply(internal_panel,{
	id:'tab-test',
	title: 'Indvidual Detail',
	closable:true
	})).show();


}


/* function to remove all html tags */
function removeHTMLTags(strInputCode){

    /*
            This line is optional, it replaces escaped brackets with real ones,
            i.e. < is replaced with < and > is replaced with >
    */
    strInputCode = strInputCode.replace(/&(lt|gt);/g, function (strMatch, p1){
            return (p1 == "lt")? "<" : ">";
    });
    var strTagStrippedText = strInputCode.replace(/<\/?[^>]+(>|$)/g, "");

    return strTagStrippedText

}
