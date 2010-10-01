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

function showgrid(response, request,label){
	var jsonData = Ext.util.JSON.decode(response);
	if(eval(jsonData.success)==false)
	{
		Ext.getCmp('centerPanel').disable();
		return false;
	}else{
	var rowData = eval(jsonData.rowData);
	var filedsVal = eval(jsonData.headersList);
	var columnData = eval(jsonData.columnsData);

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
	viewConfig: {forceFit:true},
	id:label,
	loadMask: true,
	layout:'fit',
	frame:true,
	autoSizeColumns: true,
	autoSizeGrid: true,
	columnLines: true,
	autoWidth:true
	});
	
	Ext.getCmp('centerPanel').add( 
	Ext.apply(grid,{
	id:'tab-'+label,
	title: label,
	closable:true
	})).show();
}
}

function sendAjaxRequest(requestURL,label){
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
			showgrid(result.responseText, request,label);
		},
		failure: function ( result, request){ 
			//alert(result.responseText); 
		},
		callback: function() {Ext.getBody().unmask();}
	})
}

Ext.onReady(function(){	
    var tBar = new Ext.Toolbar({
	xtype: "toolbar",
    items: [ {
            xtype:"tbbutton",
            icon:'resources/images/16x16/view-refresh.png',
            qtip:'Refresh',
            id: 'headRefresh', disabled: false,handler: function()
                {alert("Refresh Clicked")}
        },
        /*
            Exchane Button hided
            {
            xtype:"tbbutton",
            icon:'resources/images/16x16/go-send.png',
            qtip:'Exchange Data',
            disabled: false,
            handler: function()
                {
                  //alert("Clicked on Exchange Data")
                  dataExchange();
                }
        },*/
        {
            xtype:"tbbutton",
            icon:'resources/images/16x16/document-open.png',
            id: 'headExchange',
            disabled: false,
            handler: showCentralGrid
	}

    ]});
	
  var tree = new Ext.tree.TreePanel({
    id:'directory-tree',
    renderTo:'tree-div',
    height: 494,    
    baseCls : 'x-plain',
    bodyBorder:false,
    border:true,
    hlColor:'C3DAF',
    //width: 250,
    layout:'fit',
    useArrows:false, // true for vista like
    autoScroll:true,
    animate:true,
    margins: '0 0 0 0',
    cmargins: '100 100 100 100',
    lines :true,
    containerScroll: true,
    rootVisible: true,
    frame: true,
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
        }
       /* expandnode:{
            fn : function (node){
                Ext.state.Manager.set("treestate", node.getPath())
            }
        }*/
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

  function dataExchange(){
  Ext.Msg.show({
        msg: 'Would you like to review the <br/>Data Exchange before starting?',
        buttons: Ext.Msg.YESNO,
        icon: Ext.Msg.QUESTION,//'profile', // &lt;- customized icon
        fn: function(action){
		   if(action=='yes'){
			   showCentralGrid();
		   }
		   else if(action=='no')
		   {
			       alert('You clicked on No');
		   }
	   }});
	  }
  
  function showCentralGrid(node)
    {
	  var label = tree.getSelectionModel().getSelectedNode().text
          var obj = node.attributes          
          var eid
	  var scopeId = obj['Scope']
          var nodeType = obj['node_type']
            if((obj['node_type']=='exchanges' && obj['uid']!='')){
                eid = obj['uid']
                var requestURL = 'dataObjects/getDataObjects/'+scopeId+'/'+nodeType+'/'+eid
            }else{
                alert('You can review only Data Exchange in this Version');
				return false;
						
            }
          

          /*
                 check the id of the tab
                 if it's available then just display the tab & don't send ajax request
          */
	  if(!Ext.getCmp(label)){
                   if(tree.getSelectionModel().getSelectedNode().id!=null)
                   {
                          Ext.getCmp('centerPanel').enable();
                          sendAjaxRequest(requestURL,label);

                          // check the current state of Detail Grid panel
                          if(Ext.getCmp('detail-grid').collapsed!=true){
                                          Ext.getCmp('detail-grid').collapse();

                          }
                   }
	}else{
		  Ext.getCmp('centerPanel').enable();
		// collapse the detail Grid panel & show the tab
		  if(Ext.getCmp('detail-grid').collapsed!=true){
			  Ext.getCmp('detail-grid').collapse();
		  }
	  Ext.getCmp(label).show();
  }
			   
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
    tree.expandPath(treeState);
  }
			
});