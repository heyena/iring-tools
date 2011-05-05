Ext.ns('FederationManager');
/**
* @class FederationManager.ClassTemplatePanel
* @extends Panel
* @author Rashmi Shukla
*/
FederationManager.ClassTemplatePanel = Ext.extend(Ext.Panel, {
	//title: 'ClassTemplatePanel',	
	title:null,
	data_form: null,
    configData:null,
	url: null,
    nId:null,label:null,
    parentNode:null, node:null,name:null,desc:null,
    statusAuth:null,statusClass:null,statusFrom:null, statusTo:null,
    entityType:null, 
    specStore:[], 
    classStore:[],
    roleStore:[],
    

  /**
  * initComponent
  * @protected
  */
  initComponent: function () {

  	this.addEvents({
      close:true,
      save: true,
      reset: true,
      validate:true,
      tabChange:true
    });
  	if(this.parentNode==null){
  		this.label='New';
  	}else{
  		this.label='Edit';
  		if(this.configData == 'class'){
  			this.node=this.parentNode.childNodes[0];
	  		this.name=this.node.attributes.record.Name;
	  		this.desc=this.node.attributes.record.Description;
	  	    this.statusAuth=this.node.attributes.record["Status Authority"];
	  		this.statusClass=this.node.attributes.record["Status Class"];
	  		this.statusFrom=this.node.attributes.record["Status From"];
	  		this.statusTo=this.node.attributes.record["Status To"];
	  	    this.entityType=this.node.attributes.record["Entity Type"];
	  	    this.specStore=this.createStore(this.parentNode.childNodes[2].attributes.children);
	  	    this.classStore=this.createStore(this.parentNode.childNodes[1].attributes.children);
  		}else{
  			this.name=this.parentNode.attributes.record.label;
  			this.roleStore=this.createStore(this.parentNode.childNodes);
  		}
  	}
  	if(this.configData == 'class'){
  		var that = this;
  		this.configData= [{xtype: 'fieldset', layout:'column', border:false,
				items:[{columnWidth:.5,layout: 'form',bodyStyle:'padding-right:15px',
 					items:[
 				          {fieldLabel:'Name',name:'name', xtype:'textfield',enableKeyEvents:true,
 				        	 listeners: {
 				        		change : function(f,newval,old){
 				        			Ext.getCmp(that.id).setTitle(that.label+': {'+newval+'}');
 				        			},
 				          		keyup:function(f,evt){
 				          			Ext.getCmp(that.id).setTitle(that.label+': {'+f.getValue()+'}');
 				          		}
 				        		}
 				          ,width:200, value:this.name},
 				          {xtype: 'fieldset',title:'Description',
	 				    	  items: [
	 				    	          {name:'description', xtype:'textarea', width:200, height:205, value:this.desc}
	 				    	          ]
                          },
 				          {xtype: 'fieldset',title:'Status',
 				        	  items:[
 				        	         {fieldLabel:'Authority',name:'authority', xtype:'textfield', disabled:true, width:200, value:this.statusAuth},
 				        	         {fieldLabel:'Recorded',name:'recorded', xtype:'textfield', disabled:true,width:200, value:this.statusClass},
 				        	         {fieldLabel:'Date From',name:'dateFrom', xtype:'textfield', disabled:true,width:200, value:this.statusFrom},
 				        	         {fieldLabel:'Date To',name:'dateTo', xtype:'textfield', disabled:true,width:200,value:this.statusTo}
 				        	         ]
 				          },
 				         {xtype:'combo',store: ['iRING Sandbox (Read Only)', 'My Private Sandbox', 'ReferenceData (Read Only)', 'Proto and Initial (Read Only)'],
	 							fieldLabel:'Target Repo', width:200}]},
 				          {columnWidth:.5,layout: 'form',
        	 				items:[
        	 				      {fieldLabel:'Entity Type',name:'entityType', xtype:'textfield', width:200, value:this.entityType},
        	 				     {xtype: 'fieldset',title:'Specialization',
        	 				    	  items: [
        	 				    	          {name:'specialization', xtype:'multiselect', width:200,store:this.specStore, id:'spec'+that.id},
        	 				    	          {xtype: 'fieldset', border:false, layout:'column', 
        	 				    	        	  items:[{columnWidth:.5,xtype:"button",text:'Add',handler: function(){this.onStoreDtlsAdd(this.specStore,'spec'+that.id);}, scope: this},
        	 				    	        		  	{columnWidth:.5,xtype:"button",text:'Remove',handler: function(){this.onStoreDtlsRemove(this.specStore,'spec'+that.id);}, scope: this}
        	 				    	        	  ]}
        	 				    	          ]
                                  },
                                  {xtype: 'fieldset',title:'Classification',
        	 				    	  items: [
        	 				    	          {name:'classification', xtype:'multiselect', width:200, store:this.classStore, id:'class'+that.id},
        	 				    	         {xtype: 'fieldset', border:false, layout:'column', 
        	 				    	        	  items:[{columnWidth:.5,xtype:"button",text:'Add',handler: function(){this.onStoreDtlsAdd(this.classStore,'class'+that.id);}, scope: this},
        	 				    	        	         {columnWidth:.5,xtype:"button",text:'Remove',handler:  function(){this.onStoreDtlsRemove(this.classStore,'class'+that.id);}, scope: this}
        	 				    	        	  ]}
        	 				    	          ]
                                  },
                                  {xtype: 'fieldset', border:false, layout:'column', 
 				    	        	  items:[{columnWidth:.33,xtype : "tbbutton",text : 'Ok',tooltip : 'Ok'},
 				    	        	         {columnWidth:.33,xtype : "tbbutton",text : 'Cancel',tooltip : 'Cancel'},
 				    	        	         {columnWidth:.33,xtype : "tbbutton",text : 'Apply',tooltip : 'Apply'}
 				    	        	  ]}
                                  ]
 				        }]}];
  	}else{
  		var that = this;
  		this.configData = [{xtype: 'radiogroup',fieldLabel: 'Template Type',
            items: [
                    {boxLabel: 'Base Template', name: 'tempType', checked: true},
                    {boxLabel: 'Specialized Template', name: 'tempType'}]},

                {fieldLabel:'Name',name:'name', xtype:'textfield', width:400, value:this.name,enableKeyEvents:true,
			        	 listeners: {
				        		change : function(f,newval,old){
				        			Ext.getCmp(that.id).setTitle(that.label+': {'+newval+'}');
				        			},
				          		keyup:function(f,evt){
				          			Ext.getCmp(that.id).setTitle(that.label+': {'+f.getValue()+'}');
				          		}
				        		}},
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
	        	 							items:[{name:'roleDefinition', xtype:'multiselect', width:200, store:this.roleStore},
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
  	}
    this.data_form = new Ext.FormPanel({
      labelWidth : 100, // label settings here cascade unless
      id: 'data-form',
      url:this.url,
      method: 'POST',           
      bodyStyle:'padding:10px 5px 0',
      autoScroll: true,
      border : false, // removing the border of the form
      frame : true,
      //layout:'fit',
      width: 1000,
      closable : true,
      defaults : {
        msgTarget: 'under'
      },
      defaultType : 'textfield',

      items : this.configData,     // binding with the fields list
      buttonAlign : 'left', // buttons aligned to the left            
      autoDestroy:false
           
  	});

  	this.items = [
  		this.data_form
  	];

    this.on('close', this.onCloseTab, this);

    // super
    FederationManager.ClassTemplatePanel.superclass.initComponent.call(this);
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
  onCloseTab: function(node) {
    // check number of tabs in panel to make disabled the centerPanel if its the last tab has been closed.
    if((Ext.getCmp('contentPanel').items.length) ==1){
      Ext.getCmp('contentPanel').disable();
    }
  
  },
  
  onStoreDtlsAdd : function(store, id){
	  var selectedNode = Ext.getCmp('search-panel').getSelectedNode();
	  var nId = selectedNode.attributes.identifier;
	  var isPresent = 0;
	  for(var i=0; i<store.length;i++){
		  if(store[i][0]== nId){
			  isPresent = 1;
			  break;
		  }
	  }
	  if(isPresent == 0){
		  var data = [nId,selectedNode.text];
		  store.push(data);
	  }
	  Ext.getCmp(id).store.loadData(store);;
  },
  
  onStoreDtlsRemove : function(store,id){
	  var localStore = store;
	  alert(Ext.getCmp(id).getValue());
	  for ( var i = 0; i < localStore.length; i++) {
		  if(Ext.getCmp(id).getValue().indexOf(localStore[i][0])!= -1){
			  store.remove(localStore[i]);
		  }
	  }
	  Ext.getCmp(id).store.loadData(store);
  },


  onReset: function(){
    this.data_form.getForm().reset();
  },
  
  onTextChange : function(value){
	  this.title = value;
  }
});