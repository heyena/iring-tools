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
                     disabled : false,
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
 	      
  	    try{
	  	      node.parentNode.attributes.record=node.childNodes[0].attributes.record;
	  	      this.propertyPanel.setSource(node.childNodes[0].attributes.record);
	  	    }catch(e){}
  	    },this);
  	    tree.getRootNode().expand();
	    tree.on('click', this.onClick,this);
  	    this.refClassTabPanel.add(tree).show();
  	  },
  	onClick: function (node) {
  		localNode = node;
  		node.expand();

  		try {
	  	      node.parentNode.attributes.record=node.childNodes[0].attributes.record;
	  	      this.propertyPanel.setSource(node.childNodes[0].attributes.record);

  	    } catch (e) {
  	    }},
      onClassAdd : function(btn, ev) {
          this.openAddClassTab();
       },
      onTemplateAdd : function(btn, ev){
    	  this.onTemplateAdd();
      },
      onClassEdit : function(btn, ev){
    	  this.openAddClassTab(localNode);
      },
       openAddClassTab : function(parentNode){
          var listItems = new Array();
          var label = 'Add Class';
          var tabId = 'addClass';
          node = parentNode.childNodes[0];
          listItems.push({
            xtype: 'hidden',
            name: 'formType',
            value: 'newClass'
          });
          
         var listItem = [{xtype: 'fieldset', layout:'column', border:false,
        	 				items:[{columnWidth:.5,layout: 'form',bodyStyle:'padding-right:15px',
        	 					items:[
        	 				          {fieldLabel:'Name',name:'name', xtype:'textfield', width:200, value:node.attributes.record.Name},
        	 				          {xtype: 'fieldset',title:'Description',
	        	 				    	  items: [
	        	 				    	          {name:'description', xtype:'textarea', width:200, value:node.attributes.record.Description}
	        	 				    	          ]
                                      },
        	 				          {xtype: 'fieldset',title:'Status',
        	 				        	  items:[
        	 				        	         {fieldLabel:'Authority',name:'authority', disabled : true,xtype:'textfield', width:200, value:node.attributes.record["Status Authority"]},
        	 				        	         {fieldLabel:'Recorded',name:'recorded',disabled : true, xtype:'textfield', width:200, value:node.attributes.record["Status Class"]},
        	 				        	         {fieldLabel:'Date From',name:'dateFrom', disabled : true, xtype:'textfield', width:200, value:node.attributes.record["Status From"]},
        	 				        	         {fieldLabel:'Date To',name:'dateTo',disabled : true, xtype:'textfield', width:200, value:node.attributes.record["Status To"]}
        	 				        	         ]
        	 				          }]},
        	 				          {columnWidth:.5,layout: 'form',
        	        	 				items:[
        	        	 				      {fieldLabel:'Entity Type',name:'entityType', xtype:'textfield', width:200, value:node.attributes.record["Entity Type"]},
        	        	 				     {xtype: 'fieldset',title:'Specialization',
        	        	 				    	  items: [
        	        	 				    	          {name:'specialization', xtype:'multiselect', width:200,store:this.createStore(parentNode.childNodes[2].attributes.children)},
        	        	 				    	          {xtype: 'fieldset', border:false, layout:'column', 
        	        	 				    	        	  items:[{columnWidth:.5,xtype:"button",text:'Add',handler: this.onSave, scope: this},
        	        	 				    	        		  	{columnWidth:.5,xtype:"button",text:'Remove',handler: this.onSave, scope: this}
        	        	 				    	        	  ]}
        	        	 				    	          ]
                                              },
                                              {xtype: 'fieldset',title:'Classification',
        	        	 				    	  items: [
        	        	 				    	          {name:'classification', xtype:'multiselect', width:200, store:this.createStore(parentNode.childNodes[1].attributes.children)},
        	        	 				    	         {xtype: 'fieldset', border:false, layout:'column', 
        	        	 				    	        	  items:[{columnWidth:.5,xtype:"button",text:'Add',handler: this.onSave, scope: this},
        	        	 				    	        	         {columnWidth:.5,xtype:"button",text:'Remove',handler: this.onSave, scope: this}
        	        	 				    	        	  ]}
        	        	 				    	          ]
                                              }]
        	 				        }]},
        	 				       {xtype: 'fieldset', layout:'column', border:false,
        	        	 				items:[{columnWidth:.5,layout: 'form',bodyStyle:'padding-right:15px',
        	        	 					items:[{xtype:'combo',store: ['iRING Sandbox (Read Only)', 'My Private Sandbox', 'ReferenceData (Read Only)', 'Proto and Initial (Read Only)'],
        	        	 							fieldLabel:'Target Repo', width:200}]},
        	        	 						{columnWidht:.1, layout:'form', 
        	        	 								items:[
        	        	 								       { xtype : "tbbutton",text : 'Ok',tooltip : 'Ok', width:120}]},
        	        	 						{columnWidht:.2, layout:'form', 
        	        	        	 					items:[		       
        	        	 								       { xtype : "tbbutton",text : 'Cancel',tooltip : 'Cancel', width:120}]},
        	        	 						{columnWidht:.2, layout:'form', 
        	        	        	 					items:[
        	        	 								       { xtype : "tbbutton",text : 'Apply',tooltip : 'Apply', width:120}]}
        	        	 								       
        	        	 								       ]
        	        	 						}];

         listItems.push(listItem);
         this.fireEvent('openAddTab', this,tabId,label, listItems);
        
      },
      
      createStore : function(obj){
    	  var storeData = new Array();
          for ( var i = 0; i < obj.length; i++) {
            var nodeId = obj[i].identifier;
            var data = [nodeId, obj[i].text];
            storeData.push(data);              
          }
          return storeData;

      },
      
      
      onTemplateAdd : function(){
          var listItems = new Array();
          var label = 'Add Template';
          var tabId = 'addTemplate';
          
          listItems.push({
            xtype: 'hidden',
            name: 'formType',
            value: 'newTemplate'
          });
         var listItem = [{xtype: 'radiogroup',fieldLabel: 'Template Type',
                         items: [
                             {boxLabel: 'Base Template', name: 'tempType', checked: true},
                             {boxLabel: 'Specialized Template', name: 'tempType'}]},

                         {fieldLabel:'Name',name:'name', xtype:'textfield', width:400},
						 {fieldLabel:'Parent Template',name:'parentTemplate', xtype:'textfield', width:400},
						 {xtype: 'fieldset', layout:'column', border:false,
	        	 				items:[{columnWidth:.5,layout: 'form',bodyStyle:'padding-right:15px',
	        	 						items:[{xtype:'fieldset',title:'Description',
	        	 							items:[{name:'description', xtype:'textarea', width:200}]},
	        	 							{xtype: 'fieldset',title:'Status',
	          	 				        	  items:[
	          	 				        	         {fieldLabel:'Authority',name:'authority', xtype:'textfield', width:200},
	          	 				        	         {fieldLabel:'Recorded',name:'recorded', xtype:'textfield', width:200},
	          	 				        	         {fieldLabel:'Date From',name:'dateFrom', xtype:'datefield', width:200},
	          	 				        	         {fieldLabel:'Date To',name:'dateTo', xtype:'datefield', width:200}
	          	 				        	         ]
	          	 				          }]},
	        	 						{columnWidth:.5,layout: 'form',bodyStyle:'padding-right:15px',
	        	        	 					items:[{xtype:'fieldset',title:'Role Definition',
	    	        	 							items:[{name:'roleDefinition', xtype:'textarea', width:200},
	    	        	 							       {fieldLabel:'Id',name:'id', xtype:'textfield', width:200},
	    	          	 				        	       {fieldLabel:'Name',name:'name', xtype:'textfield', width:200},
	    	          	 				        	       {fieldLabel:'Description',name:'description', xtype:'textfield', width:200},
	    	          	 				        	       {xtype: 'fieldset', layout:'column', border:false,
	    	          	        	        	 				items:[{columnWidth:.25,layout: 'form',bodyStyle:'padding-right:15px',
	    	          	        	        	 					items:[{ xtype : "tbbutton",text : 'Edit..',tooltip : 'Edit', width:70}]},
	    	          	        	        	 					   {columnWidth:.25,layout: 'form',bodyStyle:'padding-right:15px',
		    	          	        	        	 				items:[{ xtype : "tbbutton",text : 'Add',tooltip : 'Add', width:70}]},
		    	          	        	        	 				    {columnWidth:.25,layout: 'form',bodyStyle:'padding-right:15px',
		    	          	        	        	 				items:[{ xtype : "tbbutton",text : 'Remove',tooltip : 'Remove', width:70}]},
		    	          	        	        	 					{columnWidth:.25,layout: 'form',bodyStyle:'padding-right:15px',
			    	          	        	        	 			items:[{ xtype : "tbbutton",text : 'Apply',tooltip : 'Apply', width:70}]}
	    	          	        	        	 					       ]}]
	    	          	 				        	       }	]}]}];

         listItems.push(listItem);
         this.fireEvent('openAddTab', this,tabId, label, listItems);
        
      }

});