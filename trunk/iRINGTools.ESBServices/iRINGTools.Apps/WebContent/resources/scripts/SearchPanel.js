Ext.ns('FederationManager');
/**
* @class FederationManager.SearchPanel
* @author by Aswini Nayak
*/

//image path
var IMG_CLASS = 'Content/img/class.png';
var IMG_TEMPLATE = 'Content/img/template.png';

//renderer function
function renderIcon(value, p, record) {
    var label = null;

    if (record.data.Uri.indexOf("tpl") != -1) {
        label = '<img src="' + IMG_TEMPLATE + '" align="top"> ' + value;
    } else {
        label = '<img src="' + IMG_CLASS + '" align="top"> ' + value;
    }
    return label;
}

FederationManager.SearchPanel = Ext.extend(Ext.Panel, {
    title: 'Reference Data Search',
    layout: 'border',
    border: true,
    split: true,
    searchUrl: null,
    limit: 100,
    refClassTabPanel:null,
    propertyPanel:null,
    searchStore:null,
    /**
    * initComponent
    * @protected
    */
    initComponent: function () {
    	this.tbar = this.buildToolbar();
        this.propertyPanel = new Ext.grid.PropertyGrid( {
            id : 'class-property-panel',
            title : 'Details',
            region : 'east',
            // layout: 'fit',
            stripeRows : true,
            collapsible : true,
            autoScroll : true,
            width:350,
        	split: true,
        	bodyBorder:true,
        	collapsed : false,
            border : true,
            frame : true,
            //bodyStyle: 'padding-bottom:15px;background:#eee;',
            source : {},
            listeners : {
              // to disable editable option of the property grid
              beforeedit : function(e) {
                e.cancel = true;
              }
            }
          });

        this.refClassTabPanel = new Ext.TabPanel({
            id: 'content-pane',
            region: 'center',
            deferredRender: false,
            enableTabScroll: true,
            border: true,
            activeItem: 0
          });
        
        
        this.items =[this.refClassTabPanel,this.propertyPanel];

        // super
        FederationManager.SearchPanel.superclass.initComponent.call(this);
    },
      buildToolbar: function () {
        return [ 
                 {
        			xtype: 'textfield',
        			allowBlank:false,
        			blankText :'This field can not be blank',
        			name: 'referencesearch',
        			id:'referencesearch',
        			style: {
        	            marginLeft: '15px'
        	        },
        	        scope:this,
        	        listeners: {
        	              specialkey: function(f,e){
        	                if (e.getKey() == e.ENTER) {
        	                	var query = Ext.get('referencesearch').getValue();
        	                	//alert(query);
        	                }
        	              }
        	            }
            	 },
            	 {
                 		xtype: 'checkbox',
                	  	boxLabel:'Reset',
                	  	text: 'Reset',
                	  	name: 'reset',
                	  	style: {
            	            marginRight: '5px',
            	            marginLeft: '5px'
            	            
            	        }
                },
                {
				    xtype : "tbbutton",
				    text : 'Search',
                    handler: this.onSearch,
                    scope : this
	
				},
				{
                     xtype : "tbbutton",
                     text : 'Promote',
                     //icon : 'resources/images/16x16/view-refresh.png',
                     tooltip : 'Promote',
                     disabled : false,
                     handler: this.onPromote,
                     scope : this
                   },
                   {
                     xtype : "tbbutton",
                     text : 'Add Class',
                     tooltip : 'Add Class',
                     disabled : false,
                     handler: this.onClassAdd,
                     scope : this
                   },
                   {
                     xtype : "tbbutton",
                     text : 'Edit Class',
                     tooltip : 'Edit Class',
                     disabled : true,
                     handler: this.onClassEdit,
                     scope : this
                   },
                   {
                     xtype : "tbbutton",
                     text : 'Add Template',
                     tooltip : 'Add Template',
                     disabled : false,
                     handler: this.onTemplateAdd,
                     scope : this
                   },
                   {
                     xtype : "tbbutton",
                     text : 'Edit Template',
                     tooltip : 'Edit Template',
                     disabled : true,
                     handler: this.onTemplateEdit,
                     scope : this
                   }];
      },
      onSearch: function(){
    	  var searchText = Ext.get('referencesearch').getValue();
    	  	  var tree = new Ext.tree.TreePanel({
            	  title:searchText,
                  useArrows: true,
                  animate: true,
                  lines : false,
                  id:'tab_'+searchText,
                  autoScroll : true,
                  style : 'padding-left:10px;',
                  border: false,
                  closable:true,
                  rootVisible: false,
                  dataUrl: 'resources/myjson.json',
                  maskDisabled: true, 
                  //dataUrl:this.searchUrl,
                  root: {
                      nodeType: 'async',
                      draggable: false
                  }/*,
                  listeners: {
                      click: function(n) {
                          Ext.Msg.alert('Navigation Tree Click', 'You clicked: "' + n.attributes.text + '"');
                      }
                  }*/
              });
              //tree.render('aa');
              tree.getRootNode().expand();
              this.refClassTabPanel.add(tree).show();
    	  
        
      },
    load: function () {
        //this.searchStore.load({ params: { start: 0, limit: this.limit} });
        return;
    }

});