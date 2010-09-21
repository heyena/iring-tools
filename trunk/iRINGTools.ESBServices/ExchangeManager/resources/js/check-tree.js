/*!
 * Ext JS Library 3.2.1
 * Copyright(c) 2006-2010 Ext JS, Inc.
 * licensing@extjs.com
 * http://www.extjs.com/license
 */
Ext.onReady(function(){
	
	var tBar = new Ext.Toolbar({ xtype: "toolbar",
         
	items: [
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
	}},'-',
	{xtype:"tbbutton",
            icon:'resources/images/16x16/view-refresh.png',
            qtip:'Refresh',
            id: 'headRefresh', disabled: false,handler: function(){alert("Refresh Clicked")

	}}							   
	]});
	
  var tree = new Ext.tree.TreePanel({
      renderTo:'tree-div',
    baseCls : 'x-plain',
    bodyBorder:false,
    border:true,
    hlColor:'C3DAF',
    width: 250,
    useArrows:false, // true for vista like
    autoScroll:true,
    animate:true,
    tbar: tBar,
    /*[ {
                                            text: 'Cancel',
                                            margin:'100 01 01 01',
                                            minWidth: 100,
                                            ref: '../cancelButton'
                                        }, {
                                            text: 'Save',
                                            minWidth: 100,
                                            ref: '../saveButton'
                                        }],

   /*tbar: [
       {
                                            text: 'Add',
                                            iconCls: 'silk-add'
                                           // handler: this.onAdd,
        //                                    scope: this
                                        }, '-', {
                                            text: 'Delete',
                                            iconCls: 'silk-delete'
                                            //handler: this.onDelete,
          //                                  scope: this
                                        }, '-'],

                                    */
                                        root: {
      nodeType: 'async',
      icon: 'resources/images/16x16/internet-web-browser.png',
      text: 'Directory'
    },
                                        dataUrl: 'ExchangeReader/exchnageList/1'



    /*renderTo:'tree-div',
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
	tbar:tBar*/
    
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