Ext.ns('FederationManager');
/**
* @class FederationManager.ClassTemplatePanel
* @extends Panel
* @author Rashmi Shukla
*/
FederationManager.ClassTemplatePanel = Ext.extend(Ext.Panel, {
	title: 'ClassTemplatePanel',	
	data_form: null,
    configData:null,
	url: null,
    nId:null,
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
  		
  	}else{
  		if(this.configData == 'class'){
	  		this.node = this.parentNode.childNodes[0];
	  		this.name=this.node.attributes.record.Name;
	  		this.desc=this.node.attributes.record.Description;
	  	    this.statusAuth=this.node.attributes.record["Status Authority"];
	  		this.statusClass=this.node.attributes.record["Status Class"];
	  		this.statusFrom=this.node.attributes.record["Status From"];
	  		this.statusTo=this.node.attributes.record["Status To"];
	  	    this.entityType=this.node.attributes.record["Entity Type"];
	  	    this.specStore = this.createStore(this.parentNode.childNodes[2].attributes.children);
	  	    this.classStore=this.createStore(this.parentNode.childNodes[1].attributes.children);
  		}else{
  			this.name=this.parentNode.attributes.record.label;
  			this.roleStore=this.createStore(this.parentNode.childNodes);
  		}
  	}
  	if(this.configData == 'class'){
  		this.configData= [{xtype: 'fieldset', layout:'column', border:false,
				items:[{columnWidth:.5,layout: 'form',bodyStyle:'padding-right:15px',
 					items:[
 				          {fieldLabel:'Name',name:'name', xtype:'textfield', width:200, value:this.name},
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
 				          }]},
 				          {columnWidth:.5,layout: 'form',
        	 				items:[
        	 				      {fieldLabel:'Entity Type',name:'entityType', xtype:'textfield', width:200, value:this.entityType},
        	 				     {xtype: 'fieldset',title:'Specialization',
        	 				    	  items: [
        	 				    	          {name:'specialization', xtype:'multiselect', width:200,store:this.specStore},
        	 				    	          {xtype: 'fieldset', border:false, layout:'column', 
        	 				    	        	  items:[{columnWidth:.5,xtype:"button",text:'Add',handler: this.onSave, scope: this},
        	 				    	        		  	{columnWidth:.5,xtype:"button",text:'Remove',handler: this.onSave, scope: this}
        	 				    	        	  ]}
        	 				    	          ]
                                  },
                                  {xtype: 'fieldset',title:'Classification',
        	 				    	  items: [
        	 				    	          {name:'classification', xtype:'multiselect', width:200, store:this.classStore},
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
  	}else{
  		this.configData = [{xtype: 'radiogroup',fieldLabel: 'Template Type',
            items: [
                    {boxLabel: 'Base Template', name: 'tempType', checked: true},
                    {boxLabel: 'Specialized Template', name: 'tempType'}]},

                {fieldLabel:'Name',name:'name', xtype:'textfield', width:400, value:this.name},
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
      layout:'fit',
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

  onReset: function(){
    this.data_form.getForm().reset();
  },

  onSave:function(){
		var that = this;    // consists the main/previous class object
	  this.data_form.getForm().submit({
	    waitMsg: 'Saving Data...',
	    success: function(f,a){
	      if(that.getActiveTab()){
	        var node = federationPanel.getNodeBySelectedTab(that.getActiveTab());
	        Ext.Msg.alert('Success', 'Changes saved successfully!');
	        var formType = that.data_form.getForm().findField('formType').getValue();
	        if(formType=='newForm'){ // in case of newForm close the newTab
	          Ext.getCmp('contentPanel').remove(that.getActiveTab(), true);
	        }
	
	        federationPanel.onRefresh(node);
	        //federationPanel.expandNode(node) // pending
	        //federationPanel.selectNode(node) // pending
	      }
	            
	    },
	    failure: function(f,a){
	      Ext.Msg.alert('Warning', 'Error saving changes!');
	    }
	  });

  }
});