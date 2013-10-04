Ext.ns('FederationManager');
/**
* @class FederationManager.SearchPanel
* @author by Aswini Nayak
*/
var treeLoader,searchText;
var localNode;
FederationManager.SearchPanel = Ext.extend(Ext.Panel, {
    title: 'Reference Data Search',
    layout: 'border',
    border: true,
    split: true,
    searchUrl: null,
    limit: 100,
    mainPanel:null,
    refClassTabPanel:null,
    propertyPanel:null,
    searchStore:null,
    contextClassMenu: null,
    contextTempMenu:null,
    
    /**
    * initComponent
    * @protected
    */
    initComponent: function () {
    	this.addEvents( {
            click : true,
           refresh : true,
             edit : true,
            addnew : true,
            openAddClassTab : true,
            load : true,
            beforeload : true,
            selectionchange : true
          });
    	
    	//this.tbar = this.buildToolbar();
    	
        this.contextClassMenu = new Ext.menu.Menu();
    	this.contextClassMenu.add(this.buildClassContextMenu());
    	
        this.contextTempMenu = new Ext.menu.Menu();
    	this.contextTempMenu.add(this.buildTemplateContextMenu());
    	
        this.propertyPanel = new Ext.grid.PropertyGrid( {
            id : 'class-property-panel',
            title : 'Details',
            region : 'east',
            // layout: 'fit',
            stripeRows : true,
            collapsible : true,
     //       columnLines : true,
            autoScroll : true,
            width:350,
        	split: true,
        	bodyBorder:true,
        	collapsed : false,
            border : true,
            frame : true,
            source : {},
            clicksToEdit:2,
            listeners : {
            	beforepropertychange : function(source, recordid, v, oldValue){
      			return false;
      			
      		}/*,{
              // to disable editable option of the property grid
              beforeedit : function(e) {
                e.cancel = true;
              }*/
            }
          });

        this.refClassTabPanel = new Ext.TabPanel({
            id: 'content-pane',
            //region: 'center',
            deferredRender: false,
            enableTabScroll: true,
            //border: true,
            activeItem: 0
          });
        
        
    	this.mainPanel = new Ext.Panel({
    		tbar : this.buildToolbar(),
    		region: 'center',
    		autoScroll:true,
    		layout: 'fit',
    		items :[this.refClassTabPanel]
    		
    	});
    	
        this.items =[this.mainPanel,this.propertyPanel];

        // super
        FederationManager.SearchPanel.superclass.initComponent.call(this);
    },
      buildToolbar: function () {
    	  var that=this;
        return [ 
                 {
        			xtype: 'textfield',
        			width: 200,
        			name: 'referencesearch',
        			id:'referencesearch',
        			style: {
        				fontSize:'12px',
        	            marginLeft: '10px'
        	        },
        	        scope:this,
        	       listeners: {
        	              specialkey: function(f,e){
        	                if (e.getKey() == e.ENTER) {
        	                	that.onSearch();
        	                }
        	              }
        	            }
            	 },
            	 {
 				    xtype : "button",
 				    text : 'Search',
 				    icon: 'resources/images/16x16/document-properties.png',
                     handler: this.onSearch,
                     scope : this,
             	  	style: {
         	            marginLeft: '5px'
         	        }
 	
 				},{
                 		xtype: 'checkbox',
                	  	//boxLabel:'Reset',
                	  	name: 'reset',
                	  	style: {
            	            marginLeft: '3px',
            	            marginBottom: '6px'
            	        }
                },
                {
            	  	xtype: 'label',
            	  	text: 'Reset',
            	  	style: {
            	  	marginRight: '5px'
            	  	}
        	        
                }];
      },
      
      buildClassContextMenu: function () {
  	    return [
  	      {
  	        text: 'Add',
  	        handler: this.onClassAdd,
  	        icon : 'resources/images/16x16/document-new.png',
  	        scope: this
  	      },
  	      {
  	        text: 'Edit',
  	        handler: this.onClassEdit,
  	        icon : 'resources/images/16x16/edit-file.png',
  	        scope: this
  	      },
  	      {
    	        xtype: 'menuseparator'
    	  },
    	  {
    	    text: 'Promote',
    	    handler: this.onPromote,
    	    icon : 'resources/images/16x16/promote-icon.png',
    	    scope: this
    	   }];
  	  },
  	 buildTemplateContextMenu: function () {
  		return [
  	  	      {
  	  	        text: 'Add',
  	  	        handler: this.onTemplateAdd,
  	  	        icon : 'resources/images/16x16/document-new.png',
  	  	        scope: this
  	  	      },
  	  	      {
  	  	        text: 'Edit',
  	  	        handler: this.onTemplateEdit,
  	  	        icon : 'resources/images/16x16/edit-file.png',
  	  	        scope: this
  	  	      }];
   	  },
  	 showContextMenu: function (node, event) {
 	    if (node.isSelected()) {
  	      var x = event.browserEvent.clientX;
  	      var y = event.browserEvent.clientY;
	  	    if(node.attributes.type=="ClassNode")
	  	    	this.contextClassMenu.showAt([x, y]);
	  		else if(node.attributes.type=="TemplateNode")
	  			this.contextTempMenu.showAt([x, y]);
      	  
  	      }
  	  },
      onSearch: function () {
  	    var searchText = Ext.get('referencesearch').getValue();
  	    treeLoader = new Ext.tree.TreeLoader({
  	      requestMethod: 'POST',
  	      url: this.searchUrl,
  	      baseParams: {
  	        id: null,
  	        type: null,
  	        query: searchText,
  	        limit: this.limit,
  	        start: 0
  	      }
  	    });

  	    treeLoader.on("beforeload", function (treeLoader, node) {
  	    treeLoader.baseParams.type = node.attributes.type;
  	    treeLoader.baseParams.query = searchText;
  	    treeLoader.baseParams.limit = this.limit;
  	    treeLoader.baseParams.start = 0;
  		 if(node.parentNode && node.attributes.identifier==null){
  			treeLoader.baseParams.id = node.parentNode.attributes.identifier;
  		 }else{
  	        treeLoader.baseParams.id = node.attributes.identifier;
  		 }
  	    }, this);
  	    
  	    
  	    var tree = new Ext.tree.TreePanel({
  	      title: searchText,
  	      //useArrows: true,
  	      animate: true,
  	      lines: true,
  	      id: 'tab_' + searchText.toLowerCase(),
  	      autoScroll: true,
  	      style: 'padding-left:5px;',
  	      border: false,
  	      closable: true,
  	      rootVisible: false,
  	      loader: treeLoader,
  	      root: {
  	        nodeType: 'async',
  	        draggable: false,
  	        type: 'SearchNode'
  	      },
  	      containerScroll: true
  	    });

  	    tree.on('beforeload', function (node) {
  	      Ext.getCmp('content-pane').getEl().mask('Loading...');
  	    });
  	    tree.on('load', function (node) {
    	      Ext.getCmp('content-pane').getEl().unmask();

    	  	    try{
    	  	    	for(var i=1;i<node.childNodes.length;i++){
    	  	    	if(node.childNodes[i].attributes.children.length==0){
    	  	     		node.childNodes[i].leaf=true;
    	  	    		
    	  	    	}  	    	
    	  	    }}catch(e){}
    	 	      if(node.attributes.type=="ClassNode"){
    	 	    	try{
    	  	  	  	    this.propertyPanel.setSource(node.childNodes[0].attributes.record);
    	  		  	    }catch(e){}
    	  	  	    }
    	  	    },this);
  	    tree.getRootNode().expand();
	    tree.on('click', this.onClick,this);
	    tree.on('contextmenu', this.showContextMenu, this);
  	    this.refClassTabPanel.add(tree).show();
  	  },
  	onClick: function (node) {
  		localNode = node;
  		try {
	  			if(node.attributes.type=="ClassNode" && node.childNodes[0]!=undefined){
	  				this.propertyPanel.setSource(node.childNodes[0].attributes.record);
	  	  	    }else{
	  	  	    	this.propertyPanel.setSource(node.attributes.record);
	  	  	    }
  			}catch(e){}
  	  		node.expand();
  		
  		},
        onClassAdd : function(btn, ev) {
            this.openAddClassTab(null);
         },
        onTemplateAdd : function(btn, ev){
      	  this.openAddTemplateTab(null);
        },
        onClassEdit : function(btn, ev){
      	  this.openAddClassTab(localNode);
        },
        onTemplateEdit : function(btn, ev){
        	this.openAddTemplateTab(localNode);
        },
        openAddClassTab : function(parentNode){
            var listItems = new Array();
            var label = 'New: {}';
            var tabId = 'addClass';
            var node = null;
            if(parentNode!=null){
  	          node = parentNode.childNodes[0];
  	          label = 'Edit {'+node.attributes.record.Name+'}';
  	          tabId = node.attributes.identifier;
            }
            listItems.push({
              xtype: 'hidden',
              name: 'formType',
              value: 'newClass'
            });
            
           this.fireEvent('openAddTab', this,tabId,label, 'class', parentNode);
          
        },
        
     
        openAddTemplateTab : function(parentNode){
          var listItems = new Array();
          var label = 'New: {}';
          var tabId = 'addTemplate';
          if(parentNode!=null){
  	          label = 'Edit {'+parentNode.attributes.record.label+'}';
  	          tabId = parentNode.attributes.identifier;
            }
          listItems.push({
            xtype: 'hidden',
            name: 'formType',
            value: 'newTemplate'
          });

         this.fireEvent('openAddTab', this,tabId, label, 'template', parentNode);
        
      },
      
      getSelectedNode : function(){
    	  return localNode;
      }

});