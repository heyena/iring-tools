Ext.ns('FederationManager');
/**
* @class FederationManager.SearchPanel
* @author by Aswini Nayak
*/
var treeLoader,searchText;
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
                	  	name: 'reset',
                	  	style: {
            	            marginRight: '5px',
            	            marginLeft: '3px'
            	        }
                },
                {
				    xtype : "tbbutton",
				    text : 'Search',
                    handler: this.onSearch,
                    scope : this,
            	  	style: {
        	            marginLeft: '5px'
        	        }
	
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
    	  searchText = Ext.get('referencesearch').getValue();
    	  treeLoader =  new Ext.tree.TreeLoader({
    		  requestMethod: 'POST',
              url: this.searchUrl,
    		  baseParams: { 
            	  query: searchText,
            	  limit:this.limit,
            	  start:0
            	}
    	    });
    	  	  var tree = new Ext.tree.TreePanel({
            	  title:searchText,
                  useArrows: true,
                  animate: true,
                  lines : false,
                  id:'tab_'+searchText,
                  autoScroll : true,
                  style : 'padding-left:5px;',
                  border: false,
                  closable:true,
                  rootVisible: false,
                  loader:treeLoader ,
                  root: {
                      nodeType: 'async',
                      qtipCfg:'Aswini',
                      draggable: false
                  },
                  containerScroll: true
              });
    	  	  
    	  //	tree.on('beforeexpandnode', this.restrictExpand, this);

    	  	tree.on('beforeload', function(node){
    	  		Ext.getCmp('content-pane').getEl().mask('Loading...');
    	  	}); 
    		tree.on('load', function(node){
    	  		Ext.getCmp('content-pane').getEl().unmask();
    	  	});
              tree.getRootNode().expand();
              tree.on('click', this.onClick, this);
              this.refClassTabPanel.add(tree).show();
      },
      onClick : function(node) {
    	  switch(node.attributes.text){
    	  case "Classifications":
    		 // alert("send request for classifications:"+'class/'+node.parentNode.attributes.identifier);
    		  treeLoader.url="class";
    	  	  treeLoader.baseParams = {
    	  			  id:node.parentNode.attributes.identifier,
    	  			  query: searchText,
                	  limit:this.limit,
                	  start:0
                	  };
    		  break;
    	  case "Superclasses":
    		  //alert("send request for Superclasses:"+'superClass/'+node.parentNode.attributes.identifier);
    		  treeLoader.url="superClass";
    	  	  treeLoader.baseParams = {
    	  			  id:node.parentNode.attributes.identifier,
    	  			  query: searchText,
                	  limit:this.limit,
                	  start:0
                	  };

    		  break;
    	  case "Subclasses":
    		  //alert("send request for Subclasses:"+'subClasses/'+node.parentNode.attributes.identifier);
    		  treeLoader.url="subClass";
    	  	  treeLoader.baseParams = {
    	  			  id:node.parentNode.attributes.identifier,
    	  			  query: searchText,
                	  limit:this.limit,
                	  start:0
                	  };
    		  break;
    	  case "Templates":
    		  //alert("send request for Subclasses:"+'subClasses/'+node.parentNode.attributes.identifier);
    		  treeLoader.url="template";
    	  	  treeLoader.baseParams = {
    	  			  id:node.parentNode.attributes.identifier,
    	  			  query: searchText,
                	  limit:this.limit,
                	  start:0
                	  };
    		  break;

    		  
    	  }
    	  node.expand();
      }

});