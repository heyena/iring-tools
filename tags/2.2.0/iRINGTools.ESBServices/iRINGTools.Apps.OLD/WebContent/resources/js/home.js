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

Ext.onReady(function(){

// This is the Details panel that contains the description for each tree node


	var ab_button = Ext.get('show-about');
	ab_button.on('click', function() {
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
  
  
});
