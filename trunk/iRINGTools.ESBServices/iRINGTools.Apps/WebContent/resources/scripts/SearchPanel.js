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
    refClassTabPanel:null,
    propertyPanel:null,
    searchStore:null,
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
    	
    	this.tbar = this.buildToolbar();
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
    	  var that=this;
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
        	                	that.onSearch();
        	                }
        	              }
        	            }
            	 },
            	 {
 				    xtype : "button",
 				    //text : 'Search',
 				    icon: 'resources/images/16x16/document-properties.png',
                     handler: this.onSearch,
                     scope : this,
             	  	style: {
         	            marginLeft: '5px'
         	        }
 	
 				},{
                 		xtype: 'checkbox',
                	  	boxLabel:'Reset',
                	  	name: 'reset',
                	  	style: {
            	            marginRight: '5px',
            	            marginLeft: '3px',
            	            marginBottom: '6px'
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
                     id : 'class-edit',
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
                     id : 'temp-edit',
                     disabled : true,
                     handler: this.onTemplateEdit,
                     scope : this
                   }];
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
  	      id: 'tab_' + searchText,
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

  	    // during loading count the total children and update the Node Label As we are getting the counts for Superclasses and Classifications from JSON we don't require to cpunt manually 
 	    if(node.attributes.text=="Subclasses"||node.attributes.text=="Templates"){
  	    	node.setText(node.attributes.text+' ('+node.childNodes.length+')');
  	    }
 	    // update the detail's panel with All properties
 	      if(node.attributes.type=="ClassNode"){
 	    	try{
  	  	  	    this.propertyPanel.setSource(node.childNodes[0].attributes.record);
  		  	    }catch(e){}
  	  	    }
  	    },this);
  	    tree.getRootNode().expand();
	    tree.on('click', this.onClick,this);
  	    this.refClassTabPanel.add(tree).show();
  	  },
  	onClick: function (node) {
  		localNode = node;
  		if(node.attributes.type=="ClassNode"){
  			Ext.getCmp('class-edit').enable();
  			Ext.getCmp('temp-edit').disable();
  		}else if(node.attributes.type=="TemplateNode"){
  			Ext.getCmp('class-edit').disable();
  			Ext.getCmp('temp-edit').enable();
  		}else{
  			Ext.getCmp('class-edit').disable();
  			Ext.getCmp('temp-edit').disable();
  		}
  			

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
            var label = 'Add Class';
            var tabId = 'addClass';
            var node = null;
            if(parentNode!=null){
  	          node = parentNode.childNodes[0];
  	          label = 'Edit {'+node.attributes.record.Name+'}';
  	          tabId = node.attributes.record["Identifier"];
            }
            listItems.push({
              xtype: 'hidden',
              name: 'formType',
              value: 'newClass'
            });
            
           this.fireEvent('openAddTab', this,tabId,label, 'class', parentNode);
          
        },
        
        createStore : function(obj){
      	  var storeData = new Array();
            for ( var i = 0; i < obj.length; i++) {
              var nodeId = obj[i].identifier;
              var data = [nodeId, obj[i].text];
              storeData.push(data);              
            }
            alert("storeData.length:"+storeData.length);
            return storeData;

        },      
        openAddTemplateTab : function(parentNode){
          var listItems = new Array();
          var label = 'Add Template';
          var tabId = 'addTemplate';
          if(parentNode!=null){
  	          label = 'Edit {'+parentNode.attributes.record.label+'}';
  	          tabId = label;//node.attributes.record["Identifier"];
            }
          listItems.push({
            xtype: 'hidden',
            name: 'formType',
            value: 'newTemplate'
          });

         this.fireEvent('openAddTab', this,tabId, label, 'template', parentNode);
        
      }

});