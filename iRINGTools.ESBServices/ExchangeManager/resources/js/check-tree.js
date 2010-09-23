/*!
 * Ext JS Library 3.2.1
 * Copyright(c) 2006-2010 Ext JS, Inc.
 * licensing@extjs.com
 * http://www.extjs.com/license
 */
Ext.onReady(function(){
	
	var tBar = new Ext.Toolbar({ xtype: "toolbar",
         
	items: [
                {xtype:"tbbutton",
                icon:'resources/images/16x16/view-refresh.png',
                qtip:'Refresh',
                id: 'headRefresh', disabled: false,handler: function(){alert("Refresh Clicked")

                }},
		{xtype:"tbbutton",text:"Exchange", id: 'headExchange', disabled: false,
		handler: function(){
		Ext.Msg.show({
		title: ':: :: :: :: :: :: :: :: :: :: :: :: :: :: :: :: :: :: ',
		msg: 'Would you like to review the <br/>Data Exchange before starting?',
		buttons: Ext.Msg.YESNO,
		icon: Ext.Msg.QUESTION,//'profile', // &lt;- customized icon
		fn: function(action){
		   if(action=='yes')
		   {
			   if(tree.getSelectionModel().getSelectedNode().id!=null){
				   Ext.getCmp('centerPanel').enable();
					//***** http://localhost:8080/iringtools/diffservice/12345_000/exchanges/1/index
			   }
		   }
		   else if(action=='no')
		   {
			   alert('You clicked on No');
		   }
	   }});
	}}
								   
	]});
	
  var tree = new Ext.tree.TreePanel({
      
    renderTo:'tree-div',
    height: 460,
    baseCls : 'x-plain',
    bodyBorder:false,
    border:true,
    hlColor:'C3DAF',
    width: 250,
    useArrows:false, // true for vista like
    autoScroll:true,
    animate:true,
    margins: '0 0 0 0',
    cmargins: '100 100 100 100',
    lines :true,
    //enableDD:true,
    containerScroll: true,
    rootVisible: true,
    frame: true,
    //requestMethod:'GET', default is post
    root: {
      nodeType: 'async',
      icon: 'resources/images/16x16/internet-web-browser.png',
      text: 'Directory'
    },
    // auto create TreeLoader
    //loader: new Ext.tree.TreeLoader(),
    dataUrl: 'ExchangeReader/exchnageList/1',
    tbar:tBar,

    listeners: {
        click: {
         fn: function(node){
             //get all the attributes of node
             obj = node.attributes
             var details_data = []
               for(var key in obj){                 
                 // val.push('"'+key+'":'+'"'+obj[key]+'"')
                 // restrict some of the properties to be displayed
                 if(key!='id' && key!='text' && key!='icon' && key!='children' && key!='loader' && key!='leaf' && key!='applicationId'){
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
        }
      }
  });

  var contextMenu = new Ext.menu.Menu({
    items: [
    {
      text: 'Sort',
      handler: sortHandler
    }
    ,{
      text: 'Exchange',
      handler: exchangeHandler
    }
    ]
  });
  //tree.on('contextmenu', treeContextHandler);

  function exchangeHandler(){
    alert(tree.getSelectionModel().getSelectedNode().id);
  }
  function sortHandler() {
    tree.getSelectionModel().getSelectedNode().sort(
      function (leftNode, rightNode) {
        return 1;//(leftNode.text.toUpperCase() < rightNode.text.toUpperCase() ? 1 : -1);
      }
      );
  }

  /* to maintain the state of the tree */
  Ext.state.Manager.setProvider(new Ext.state.CookieProvider());
  tree.on('contextmenu', function (node){
    node.select();
    contextMenu.show(node.ui.getAnchor());
  });
		
  /*tree.on('expandnode', function (node){
			//node.id
			Ext.state.Manager.set("treestate", node.getPath());
			//Ext.get('pg').collapse();
		});*/
		
  var treeState = Ext.state.Manager.get("treestate");
  if (treeState){
    tree.expandPath(treeState);
  }
/* to maintain the state of the tree */
			
});