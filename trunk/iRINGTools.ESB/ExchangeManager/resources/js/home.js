	Ext.onReady(function(){
		Ext.BLANK_IMAGE_URL = 'resources/images/s.gif';		
		//****     Ext.state.Manager.setProvider(new Ext.state.CookieProvider());

	var viewport = new Ext.Viewport({
					layout: 'border',
					items: [
					// create instance immediately
					new Ext.BoxComponent({
					region: 'north',
					//style:'background-color:red;',
					height: 85, // give north and south regions a height
					autoEl: {
					tag: 'div',
					html:'<div style="float:left;"><img src="resources/images/iringlogo.png" style="margin:10;border:0px solid red;" /><span style="position:absolute;border:0px solid red;margin-top:25px;"><font size="6" style="font-family:Calibri, Arial;">Exchange Manager</font></span></div><div style="margin-right:30px;margin-top:30px;float:right;border:0px solid red;"><a href="#" class="headerLnk">Help</a>&nbsp;&nbsp;&nbsp;<a href="#" class="headerLnk">About</a></div>'
					}
				}),/*{
                // lazily created panel (xtype:'panel' is default)
                region: 'south',
                contentEl: 'south',
                split: true,
                height: 70,
                minSize: 100,
                maxSize: 200,
                collapsible: true,
				//title: 'South',
                margins: '0 0 0 0'
            },*/ 
		{
		region: 'east',
		title: 'Property Grid',
		collapsible: true,
		split: true,
		width: 225, // give east and west regions a width
		minSize: 175,
		maxSize: 400,
		margins: '0 5 0 0',
		layout: 'fit', // specify layout manager for items
		items:            // this TabPanel is wrapped by another Panel so the title will be applied
		new Ext.TabPanel({
		border: false, // already wrapped so don't add another border
		activeTab: 0, // second tab initially active
		tabPosition: 'top',
		items: [
					/*{
                        html: '<p>A TabPanel component can be a region.</p>',
                        title: 'A Tab',
                        autoScroll: true
                    },*/ 
				new Ext.grid.PropertyGrid({
				title: 'Description',
				closable: false,
				animCollapse:false,
				source: {
						   "Property A": "Value A",
						   "Property B": "Value B",
						   "autoFitColumns": true,
						   "productionQuality": false,
						   "created": new Date(Date.parse('10/15/2006')),
						   "tested": false,
						   "version": 0.01,
						   "borderWidth": 1
					   }
		})]
})
		}, {
region: 'west',
id: 'west-panel', // see Ext.getCmp() below
title: 'Navigation',
split: true,
width: 250,
minSize: 175,
maxSize: 500,
collapsible: true,
margins: '0 0 0 5',
/*                layout: {
                    type: 'accordion',
                    animate: true
                },
				*/
				layout: 'fit',
				items: [{
				contentEl: 'west',
				iconCls: 'nav'
				// see the HEAD section for style used
					//autoLoad:'loripsum.html',
					//autoLoad: {url: 'http://localhost:81/test/IntroToExt2/tree/xml-tree-loader.html', params: 'foo=bar&wtf=1'},
					/* contentEl: 'west',
                    title: 'Directory',
                    border: false,
                    iconCls: 'nav' // see the HEAD section for style used
					*/
		}]
		},
			// in this instance the TabPanel is not wrapped by another panel
			// since no title is needed, this Panel is added directly
			// as a Container
		new Ext.TabPanel({
region: 'center', // a center region is ALWAYS required for border layout
deferredRender: false,
activeTab: 0,     // first tab initially active
items: [{
					//autoLoad:'loripsum.html',
contentEl: 'center1',
title: 'Review & Acceptance',
closable: false,
autoScroll: true
		}, {
contentEl: 'center2',
title: 'Results',
autoScroll: true
		}]
		   })]
	});
});
