
Ext.onReady(function(){
    tree = new Ext.tree.TreePanel({
            region:'north',
            split:true,
            id:'federation-tree',
            height:300,
            bodyBorder:false,
            border:false,
            hlColor:'C3DAF',
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
              Name:'Federation',
              Description:'Descripton of Federation',
              icon: 'resources/images/16x16/internet-web-browser.png',
              text: 'Federation'
            },
            dataUrl: 'federation-tree.json',

            listeners: {
                click: {
                 fn: function(node){
                     //get all the attributes of node
                     obj = node.attributes
                     var details_data = []
                       for(var key in obj){
                         // restrict some of the properties to be displayed
                         if(key!='nodeType' && key!='cls' && key!='id' && key!='text' && key!='icon' && key!='children' && key!='loader' && key!='leaf' && key!='applicationId'){
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

}); // end on onReady function