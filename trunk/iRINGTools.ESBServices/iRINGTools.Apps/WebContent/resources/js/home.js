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
          region: 'east',
          title: 'Details',
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
              propsGrid = new Ext.grid.PropertyGrid({              
              id:'propGrid',
              source:{},
              listeners: {
              // to disable editable option of the property grid
                beforeedit : function(e)
                {               
                    e.cancel=true;
                }
              
              }
            })
            ]
          },
       
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
          layout: 'fit',
          items: [{
            contentEl: 'directoryContent'
          }]
        },
        {
          region: 'center',
          id:'centerPanel',
          xtype: 'tabpanel',
          disabled:true,
          margins: '0 0 0 0',
		  enableTabScroll:true,
		  defaults:{layout:'fit'}
        }
    ]
});


});
