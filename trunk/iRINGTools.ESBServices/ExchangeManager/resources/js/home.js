/*
 * @File Name : home.js
 * @Path : resources/js
 * @Using Lib : Ext JS Library 3.2.1(lib/ext-3.2.1)
 *
 * This file intended to make Layout of Exchange Manager
 * It split the entire window in different panels, manages window resizing, scrolling etc.
 * It also used to display Detail panel that is binded with Directory panel
 *
 */
var propsGrid,eastGrid;
Ext.onReady(function(){

// This is the Details panel that contains the description for each example layout.
var detailsPanel = {
    id: 'details-panel',
    title: 'Details',
    region: 'center',
    bodyStyle: 'padding-bottom:15px;background:#eee;',
    autoScroll: true,
    
    collapsible: true,
    html: '<p class="details-info">When you select a layout from the tree, additional details will display here.</p>'
};

propsGrid = new Ext.grid.PropertyGrid({
              id:'propGrid',
              title: 'Details',
              region:'center',
              autoScroll:true,
              margin:'10 0 0 0',
              bodyStyle: 'padding-bottom:15px;background:#eee;',
              //html: '<p class="details-info">When you select a layout from the tree, additional details will display here.</p>',
              collapsible: true,
              source:{},
             
              listeners: {
              // to disable editable option of the property grid
                beforeedit : function(e)
                {
                    e.cancel=true;
                }
     
              }
});

  var ab_button = Ext.get('show-about');
    ab_button.on('click', function(){
	Ext.getBody().mask();

            win = new Ext.Window({
		
                title : 'About',
                width:700,
                height:500,
                closable: true,
                resizable: false,
                autoScroll: false,                
                buttons: [{
                    text: 'Close',
                    handler: function(){
			Ext.getBody().unmask();
                        win.hide();
                    }
                }],
               autoLoad:'about.html',
                listeners: {
                    close:{
                       fn:function(){
                         Ext.getBody().unmask();
                     }
                    }
               }
            });
            win.show();

    });    
  

  Ext.BLANK_IMAGE_URL = 'resources/images/s.gif'; 

  var viewport = new Ext.Viewport({
    layout: 'border',
    items: [    
        { region: 'north',
          baseCls : 'x-plain',
          height: 65, // give north and south regions a height
          margins: '-10 5 0 0',
          contentEl:'myHeader'
        },/*
        {
          region: 'east',
          title: 'Detail Grid',
          id:'detail-grid',
          collapsible: true,
          collapsed:true,
          hideCollapseTool:false,
          hideParent :true,
          titleCollapse:true,
          split: true,
          width: 225, // give east and west regions a width
         
          margins: '0 5 0 0',
          layout: 'fit', // specify layout manager for items
          items: [
             // propsGrid
            ]
          },*/
       
        {
            
          region: 'west',
          id: 'west-panel', // see Ext.getCmp() below
          title: 'Directory',
          split: true,
          width: 250,
          minSize: 175,
          maxSize: 500,
          collapsible: true,
          margins: '0 0 0 5',
          layout: 'border',
          items: [
            //contentEl: 'directoryContent'
            tree, propsGrid

          ]
        },
			{
			region: 'center',
			id:'centerPanel',
			xtype: 'tabpanel',
			listeners: {
			'afterlayout': {
			fn: function(p){
				p.disable();
			},
			single: true // important, as many layouts can occur
			}/*
			'tabChange':{
			fn: function(p){
					var nodeid = Ext.getCmp('centerPanel').getActiveTab().text;
					if(nodeid){
						tree.getSelectionModel().select(tree.getNodeById(nodeid));
					}
                                    }
				}*/
			},
			//  disabled:true,
			margins: '0 5 0 0',
			enableTabScroll:true,
			defaults:{layout:'fit'}
			}
					   
    ]
});

tree.on('BeforeLoad', function (node){
     Ext.getCmp('directory-tree').el.mask('Loading...', 'x-mask-loading')
  });

tree.on('Load', function (node){
     Ext.getCmp('directory-tree').el.unmask()
  });

});
