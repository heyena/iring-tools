/*!
 * Ext JS Library 3.2.1
 * Copyright(c) 2006-2010 Ext JS, Inc.
 * licensing@extjs.com
 * http://www.extjs.com/license
 */
Ext.onReady(function(){
  var tree = new Ext.tree.TreePanel({
    renderTo:'tree-div',
    height: 400,
    baseCls : 'x-plain',
    bodyBorder:false,
    border:true,
    hlColor:'C3DAF',
    //footer : true,
    //autoHeight:true,
    width: 250,
    useArrows:false, // true for vista like
    autoScroll:true,
    animate:true,
    lines :true,
    //enableDD:true,
    containerScroll: true,
    rootVisible: true,
    frame: true,
    //requestMethod:'GET', default is post
    labelStyle: 'font-weight:bolder;font-size:100px;',
    ctCls: 'x-box-layout-ct my-icon',
    root: {
      nodeType: 'async',
      iconCls: 'my-icon',
      text: 'Directory'
    },
    // auto create TreeLoader
    //loader: new Ext.tree.TreeLoader(),
    dataUrl: 'check-nodes.json',
    buttons: [{
      text: 'Exchange',
      handler: function(){
        Ext.Msg.show({
          title: ':: :: :: :: :: :: :: :: :: :: :: :: :: :: :: :: :: :: ',
          msg: 'Would you like to review the <br/>Data Exchange before starting?',
          buttons: Ext.Msg.YESNO,
          icon: Ext.Msg.QUESTION,//'profile', // &lt;- customized icon
          fn: function(action){
            if(action=='yes'){
              alert('You have clicked: '+tree.getSelectionModel().getSelectedNode().text+' \n\nid: '+tree.getSelectionModel().getSelectedNode().id+' \n\n');
            }
            else if(action=='no')  {
              alert('You clicked on No');
            }
          }
        });
      }
    },
    {
      text: 'Refresh',
      handler: function(){
      }

    }]
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

  tree.on('contextmenu', function (node) {
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