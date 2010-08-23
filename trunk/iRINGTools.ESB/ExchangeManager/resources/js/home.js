var propsGrid,eastGrid;
	Ext.onReady(function(){
	  
var store = new Ext.data.Store({
data: [
          [
            1,
            "Office Space",
            "Mike Judge",
            "1999-02-19",
            1,
            "Work Sucks",
            "19.95",
            1
          ],
          [
            3,
            "Super Troopers",
            "Jay Chandrasekhar",
            "2002-02-15",
            1,
            "Altered State Police",
            "14.95",
            1
          ]
    ]
    
    ,
reader: new Ext.data.ArrayReader(
  {id:'id'},
[
  'id','title','director',{name: 'released', type: 'date', dateFormat: 'Y-m-d'},
  'genre','tagline','price','available'
]
)});

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
				}),
		{
		region: 'east',
		title: 'Property Grid',
		id:'pg',
		collapsible: true,
		collapsed:true,
		hideCollapseTool:false,
		hideParent :true,
		titleCollapse:true, 
		split: true,
		width: 225, // give east and west regions a width
		minSize: 175,
		maxSize: 400,
		margins: '0 5 0 0',
		layout: 'fit', // specify layout manager for items
		items:
		// this TabPanel is wrapped by another Panel so the title will be applied
		eastGrid=new Ext.TabPanel({
      		border: true, // already wrapped so don't add another border
      		activeTab: 0, // second tab initially active
      		tabPosition: 'top',
			  items: [
    				propsGrid = new Ext.grid.PropertyGrid({
					title: 'Description',
    				//closable: false,
    				//animCollapse:false,
    				id:'propGrid',
    				buttons: [{text: 'Save',
    							handler: saveProperties,
    							disabled :true,id: 'saveProp'
    					   }],
    				source:{}
      		  })
      		 ]
        })
      },
      {
              region: 'west',
              id: 'west-panel', // see Ext.getCmp() below
              title: 'Navigation',
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
xtype: 'tabpanel',
//disabled:true,
activeTab: 0,
items: [{
title: 'Review & Acceptance',
id:'rnatab',
layout: 'border',
border: false,
items: [{
region: 'west',
//split: true,
html: "<br/> @ Piping Network System <br><br> &nbsp;&nbsp;&nbsp;@ Tag-1 <br><br> &nbsp;&nbsp;&nbsp;@ Tag-3",
width:200,
	
},
{
region: 'south',
html: 'Nested South'
},
{
region: 'center',
xtype: 'grid',
store: store,
columns: [
{header: "Title", dataIndex: 'title'},
{header: "Director", dataIndex: 'director'},
{header: "Released", dataIndex: 'released',
renderer: Ext.util.Format.dateRenderer('m/d/Y')},
{header: "Genre", dataIndex: 'genre'},
{header: "Tagline", dataIndex: 'tagline'}
]
}]


},{
title: 'Results',
html: 'Result'
}]
}
]});

function test()
{
	var propertyStore = new Ext.data.JsonStore({
        autoLoad: true,  //autoload the data
        url: 'getproperties.php',
        root: 'props',
        fields: ['First name', 'Last name', 'E-mail'],
        listeners: {
            load: {
                fn: function(store, records, options){
                    // get the property grid component
                    var propGrids = Ext.getCmp('propGrid');
                    // make sure the property grid exists
                    if (propGrids) {
						//alert(1)
                        // populate the property grid with store data
						//alert(store.getAt(0));
                        propGrids.setSource(store.getAt(0).data);
                    }
                }
            }
        }
    }); 
}

eastGrid.on('click',test());

	propsGrid.on('propertychange', function (){
        Ext.getCmp('saveProp').enable();
        });
      function saveProperties(){
        alert('save');
      }

});
