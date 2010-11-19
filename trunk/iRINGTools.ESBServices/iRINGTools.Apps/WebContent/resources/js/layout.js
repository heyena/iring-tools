/*
 * @File Name : layout.js
 * @Path : resources/js
 * @Using Lib : Ext JS Library 3.2.1(lib/ext-3.2.1)
 *
 * This file intended to make Layout of Federation Manager
 * It split the entire window in different panels, manages window resizing, scrolling etc.
 * It also used to display "About" pop-up window and generate empty PropertyGrid for "Detail Panel"
 *
 */
var propsGrid,eastGrid;
Ext.onReady(function(){

    // This is the Details panel that contains the description for each tree node
    propsGrid = new Ext.grid.PropertyGrid({
              id:'propGrid',
              title: 'Details',
              region:'center',
              autoScroll:true,
              margin:'10 0 0 0',
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
            alert('Right now the Content for this page is not available')
            return false;
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
        },
        {
          region: 'west',
          id: 'west-panel', // see Ext.getCmp() below
          title: 'Federation',
          split: true,
          width: 350,
          minSize: 175,
          maxSize: 500,
          collapsible: true,
          margins: '0 0 0 5',
          layout: 'border',
          items: [
            //contentEl: 'federationContent'
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
			}
			},
			//  disabled:true,
			margins: '0 5 0 0',
			enableTabScroll:true,
			defaults:{layout:'fit'}
			}

    ]
});

tree.on('BeforeLoad', function (node){
     Ext.getCmp('federation-tree').el.mask('Loading...', 'x-mask-loading')
  });

tree.on('Load', function (node){
     Ext.getCmp('federation-tree').el.unmask()
  });

});
