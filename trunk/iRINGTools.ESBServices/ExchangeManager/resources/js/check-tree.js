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