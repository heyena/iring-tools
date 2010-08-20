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
		footer : true,
		//autoHeight:true,
        width: 250,
		lines:true, 
        useArrows:false, // true for vista like 
        autoScroll:true,
        animate:true,
		lines :true,
        //enableDD:true,
        containerScroll: true,
        rootVisible: false,
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
		listeners: {
            /*'checkchange': function(node, checked){
                if(checked){
                    node.getUI().addClass('complete');
                }else{
                    node.getUI().removeClass('complete');
                }
            }*/
		
		},
        
        buttons: [{
            text: 'Exchange',
            handler: function(){
                /*var msg = '', selNodes = tree.getChecked();
                Ext.each(selNodes, function(node){
                    if(msg.length > 0){
                        msg += ', ';
                    }
                    msg += node.text;
                });
                Ext.Msg.show({
                    title: 'Completed Tasks', 
                    msg: msg.length > 0 ? msg : 'None',
                    icon: Ext.Msg.INFO,
                    minWidth: 200,
                    buttons: Ext.Msg.OK
                });
				*/
/*Ext.Msg.show({
title: 'Milton',
msg: 'Would you like to review the Data Exchange before starting?',
buttons: {
yes: true,
no: true
}
});*/

Ext.Msg.show({
title: ':: :: :: :: :: :: :: :: :: :: :: :: :: :: :: :: :: :: ',
msg: 'Would you like to review the <br/>Data Exchange before starting?',
buttons: Ext.Msg.YESNO,
icon: Ext.Msg.QUESTION,//'profile', // &lt;- customized icon
fn: function(action){
		   if(action=='yes'){
		   alert('You have clicked: '+tree.getSelectionModel().getSelectedNode().text+' \n\nid: '+tree.getSelectionModel().getSelectedNode().id+' \n\n');
		   }
		   else if(action=='no')  {alert('You clicked on No');}
	   }
});
            }
        },{
			text: 'Refresh',
			handler: function(){
				  }

		}]
	});

   //*** tree.getRootNode().expand(false);
	//tree.expandAll();
	//tree.lines ='true';
	//alert(tree.getSelectionModel)

	/* to maintain the state of the tree */
		Ext.state.Manager.setProvider(new Ext.state.CookieProvider());
		
		tree.on('expandnode', function (node){
			//alert(node.id)
			Ext.state.Manager.set("treestate", node.getPath());
		});				
		var treeState = Ext.state.Manager.get("treestate");
			if (treeState){
				tree.expandPath(treeState);
			}
			
	/* to maintain the state of the tree */
			
});