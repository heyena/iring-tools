var propsGrid,eastGrid;
Ext.onReady(function(){

  var ab_button = Ext.get('show-about');
    ab_button.on('click', function(){
        Ext.DomHelper.applyStyles(Ext.getBody(),{
            'background-color': '#FF0000'
        });
            win = new Ext.Window({
                disabled: disable,
                title : 'About',
                width:750,
                height:550,
                maximizable:true,
                closable: true,
                resizable: false,
                autoScroll: false,
                background: 'transparent url(recources/images/fade.png) 0 100% repeat-x',
                buttons: [{
                    text: 'Close',
                    handler: function(){
                        win.hide();
                    }
                }],
               autoLoad:'about.html'
            });
            win.show();
    });    
  

  Ext.BLANK_IMAGE_URL = 'resources/images/s.gif';
  //****     Ext.state.Manager.setProvider(new Ext.state.CookieProvider());

  var viewport = new Ext.Viewport({
    layout: 'border',
    items: [    
        { region: 'north',
          baseCls : 'x-plain',
          height: 60, // give north and south regions a height
          margins: '-5 5 0 0',
          contentEl:'myHeader'
        },
        {
          region: 'east',
          title: 'Detail Grid',
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
          items: [
              propsGrid = new Ext.grid.PropertyGrid({
              //title: 'Description',
              //closable: false,
              //animCollapse:false,
              id:'propGrid',
              buttons: [{
                text: 'Save',
                //handler: saveProperties,
                disabled :true,
                id: 'saveProp'
              }],
              source:{}
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
          xtype: 'tabpanel',
          disabled:true,
          margins: '0 0 0 0'
          //activeTab: 0
        }
    ]
});

  /* function test()
  {
    var propertyStore = new Ext.data.JsonStore({
      autoLoad: true,  //autoload the data
      url: 'getproperties.php',
      root: 'props',
      fields: ['Property', 'Property1', 'Property2'],
      listeners: {
        load: {
          fn: function(store, records, options){
            // get the property grid component
            var propGrids = Ext.getCmp('propGrid');
            // make sure the property grid exists
            if (propGrids) {
             // alert(1)
              // populate the property grid with store data
             // alert(store.getAt(0));
              propGrids.setSource(store.getAt(0).data);
            }
          }
        }
      }
    }); 
  }

  //eastGrid.on('click',test());

  propsGrid.on('propertychange', function (){
    Ext.getCmp('saveProp').enable();
  });
 
 function saveProperties(){
    alert('save');
  }*/
});
