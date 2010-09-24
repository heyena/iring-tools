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
Ext.onReady(function(){	
    var tBar = new Ext.Toolbar({ xtype: "toolbar",
    items: [ {
        xtype:"tbbutton",
        icon:'resources/images/16x16/view-refresh.png',
        qtip:'Refresh',
        id: 'headRefresh', disabled: false,handler: function()
            {alert("Refresh Clicked")}
        },
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
    id:'directory-tree',
    renderTo:'tree-div',
    height: 494,    
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
    tree.expandPath(treeState);
  }
			
});