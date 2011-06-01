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
  formData : null,
  url: null,  
  nId:null,
  label:null,
  parentNode:null, 
  node:null,
  repositoryCombo:null,
  name:null,
  desc:null,
  parentTemplate:null,
  statusAuth:null,
  statusClass:null,
  statusFrom:null, 
  statusTo:null,
  entityType:null,
  specStore:[], 
  repoStore:[],
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
  		if(this.configData == 'class'){
  			this.url='postClass';
  		}else{
  			this.url='postTemplate';
  		}

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
	  	    this.specStore=this.createStore(this.parentNode.childNodes[3].attributes.children,0);
	  	    this.classStore=this.createStore(this.parentNode.childNodes[1].attributes.children,0);
	  	    this.url='postClass';
  		}else{
  			this.name=this.parentNode.attributes.record.label;
  			this.node=this.parentNode.childNodes[0];
  			this.parentNode.childNodes.splice(0,1); 
  			this.roleStore=this.createStore(this.parentNode.childNodes,1);
  			this.parentNode.childNodes.splice(0,0,this.node);
  			this.parentTemplate=this.node.attributes.record["Parent Template"];
	  	    this.statusAuth=this.node.attributes.record["Authority"];
	  		this.statusClass=this.node.attributes.record["Class"];
	  		this.url='postTemplate';
  			
  		}
  	}
  	
    this.repoStore=this.createRepoStore(Ext.getCmp('federation-tree').getRootNode().childNodes[2].childNodes);

  	repositoryCombo = new Ext.form.ComboBox({
		fieldLabel: 'Target Repo',
		boxMaxWidth: 230,
		width: 230,
		forceSelection: true,
		typeAhead: true,
		triggerAction: 'all',
		lazyRender: true,
		store: this.repoStore,
		displayField: 'name',
		valueField: 'targetRepo',
		hiddenName: 'targetRepo'//,
		//value: assembly
	});


  	if(this.configData == 'class'){
  		var that = this;
  		this.formData= [{xtype: 'fieldset', layout:'column', border:false,
				items:[{columnWidth:.5,layout: 'form',bodyStyle:'padding-right:15px',
 					items:[{
 						fieldLabel:'Name',
 						name:'name', 
 						xtype:'textfield',
 						enableKeyEvents:true,
        	  listeners: {
        	  	change : function(f,newval,old){
        	  		Ext.getCmp(that.id).setTitle(that.label+': {'+newval+'}');
        	  	},
        	  	keyup:function(f,evt){
        	  		Ext.getCmp(that.id).setTitle(that.label+': {'+f.getValue()+'}');
        	  	}
        		},
        		width:200, 
        		value:this.name
        	}, {
        		xtype: 'fieldset',
        		title:'Description',
		    	  items: [{
		    	  	name:'description', 
		    	  	xtype:'textarea', 
		    	  	width:200, 
		    	  	height:205, 
		    	  	value:this.desc
		    	  }]
           }, {
          	 xtype: 'fieldset',
          	 title:'Status',
        	   items:[{
        	  	 fieldLabel:'Authority',
        	  	 name:'authority', 
        	  	 xtype:'textfield', 
        	  	 disabled:true, 
        	  	 width:200, 
        	  	 value:this.statusAuth
        	  }, {
        	  	fieldLabel:'Recorded',
        	  	name:'recorded', 
        	  	xtype:'textfield', 
        	  	disabled:true,
        	  	width:200, 
        	  	value:this.statusClass
        	  }, {
        	  	fieldLabel:'Date From',
        	  	name:'dateFrom', 
        	  	xtype:'textfield', 
        	  	disabled:true,
        	  	width:200, 
        	  	value:this.statusFrom
        	  }, {
        	  	fieldLabel:'Date To',
        	  	name:'dateTo', 
        	  	xtype:'textfield', 
        	  	disabled:true,
        	  	width:200,
        	  	value:this.statusTo
        	  }]
          }, repositoryCombo]
				}, {
					columnWidth:.5,
					layout: 'form',
	 				items:[{
	 					fieldLabel:'Entity Type',
	 					name:'entityType', 
	 					xtype:'textfield', 
	 					width:200, 
	 					value:this.entityType
	 				}, {
	 					xtype: 'fieldset',
	 					title:'Specialization',
		    	  items: [{
		    	  	name:'specialization', 
		    	  	xtype:'multiselect', 
		    	  	width:200,
		    	  	store:new Ext.data.SimpleStore({
								fields: ['rid', 'value'],
								data: this.specStore
							}),
							displayField: 'value',
							valueField: 'rid',
		    	  	id:'spec'+that.id
		    	  }, {
		    	  	xtype: 'fieldset', 
		    	  	border:false, 
		    	  	layout:'column', 
	        	  items:[{
	        	  	columnWidth:.5,
	        	  	xtype:"button",
	        	  	text:'Add',
	        	  	handler: function(){
	        	  		this.onStoreDtlsAdd(this.specStore,'spec'+that.id, 'ClassNode',0);
	        	  	}, 
	        	  	scope: this
	        	 }, {
	        		 columnWidth:.5,
	        		 xtype:"button",
	        		 text:'Remove',
	        		 handler: function(){
	        			 this.onStoreDtlsRemove(this.specStore,'spec'+that.id);
	        		 }, 
	        		 scope: this
	        	 }]
		    	  }]
           }, {
          	 xtype: 'fieldset',
          	 title:'Classification',
			    	 items:[{
			    		 name:'classification', 
			    		 xtype:'multiselect', 
			    		 width:200, 
			    		 store:new Ext.data.SimpleStore({
									fields: ['rid', 'value'],
									data: this.classStore
							 }),
							 displayField: 'value',
							 valueField: 'rid',
			    		 id:'class'+that.id
			    		}, {
			    			xtype: 'fieldset', 
			    			border:false, 
			    			layout:'column', 
  	        	  items:[{
  	        	  	columnWidth:.5,
  	        	  	xtype:"button",
  	        	  	text:'Add',
  	        	  	handler: function(){
  	        	  		this.onStoreDtlsAdd(this.classStore,'class'+that.id, 'ClassNode', 0);
  	        	  	}, 
  	        	  	scope: this
  	        	 }, {
  	        		 columnWidth:.5,
  	        		 xtype:"button",
  	        		 text:'Remove',
  	        		 handler:  function(){
  	        			 this.onStoreDtlsRemove(this.classStore,'class'+that.id);
  	        		 }, 
  	        		 scope: this
  	        	 }]
			    		}]
             }, {
            	 xtype: 'fieldset', 
            	 border:false, 
            	 layout:'column', 
		        	 items:[{
		        		 columnWidth:.33,
		        		 xtype : "tbbutton",
		        		 text : 'Ok',
		        		 tooltip : 'Ok', 
		        		 handler:this.onSave, 
		        		 scope: this
		        	 }, {
		        		 columnWidth:.33,
		        		 xtype : "tbbutton",
		        		 text : 'Cancel',
		        		 tooltip : 'Cancel'
		        	 }, {
		        		 columnWidth:.33,
		        		 xtype : "tbbutton",
		        		 text : 'Apply',
		        		 tooltip : 'Apply'
		        	 }]
             }]
        		}]
  				}];
		  		this.formData.push({
            xtype: 'hidden',
            name: 'classId',
            value: that.id
          });

  	}else{
  		var that = this;
  		this.formData = [{
				xtype: 'radiogroup',
				fieldLabel: 'Template Type',
					items: [{
						boxLabel: 'Base Template', 
						name: 'tempType', 
						checked: true
					}, {
						boxLabel: 'Specialized Template', 
						name: 'tempType'
					}]
  		}, {
  			fieldLabel:'Name',
  			name:'name', 
  			xtype:'textfield', 
  			width:400, 
  			value:this.name,
  			enableKeyEvents:true,
	    	listeners: {
      		change : function(f,newval,old){
      			Ext.getCmp(that.id).setTitle(that.label+': {'+newval+'}');
      		},
      		keyup:function(f,evt){
      			Ext.getCmp(that.id).setTitle(that.label+': {'+f.getValue()+'}');
      		}
      	}
  		}, {
  			fieldLabel:'Parent Template',
  			name:'parentTemplate', 
  			xtype:'textfield', 
  			width:400, 
  			value:this.parentTemplate
  		}, {
  			xtype: 'fieldset', 
  			layout:'column', 
  			border:false,
   	 		items:[{
   	 			columnWidth:.5,
   	 			layout: 'form',
   	 			bodyStyle:'padding-right:15px',
					items:[{
						xtype:'fieldset',
						title:'Description',
						items:[{
							name:'description', 
							xtype:'textarea', 
							width:200
						}]
					}, {
						xtype: 'fieldset',
						title:'Status',
        	  items:[{
        	  	fieldLabel:'Authority',
        	  	name:'authority', 
        	  	xtype:'textfield', 
        	  	width:200, 
        	  	disabled:true, 
        	  	value:this.statusAuth
        	  }, {
        	  	fieldLabel:'Recorded',
        	  	name:'recorded', 
        	  	xtype:'textfield', 
        	  	width:200, 
        	  	disabled:true, 
        	  	value:this.statusClass
        	  }, {
        	  	fieldLabel:'Date From',
        	  	name:'dateFrom', 
        	  	xtype:'datefield', 
        	  	disabled:true, 
        	  	width:200
        	  }, {
        	  	fieldLabel:'Date To',
        	  	name:'dateTo', 
        	  	xtype:'datefield',
        	  	disabled:true, 
        	  	width:200
        	  }]
          }, repositoryCombo]
   	 		}, {
   	 			columnWidth:.5,
   	 			layout: 'form',
   	 			bodyStyle:'padding-right:15px',
   	      items:[{
   	      	xtype:'fieldset',
   	      	title:'Role Definition',
	        	items:[{
	        		name:'roleDefinition', 
	        		xtype:'multiselect', 
	        		width:200, 
	        		store:this.roleStore, 
	        		id:'role'+that.id
	        	}, {
	        		fieldLabel:'Id',
	        		name:'id', 
	        		xtype:'textfield', 
	        		width:200
	        	}, {
	        		fieldLabel:'Name',
	        		name:'name', 
	        		xtype:'textfield', 
	        		width:200
	        	}, {
	        		fieldLabel:'Description',
	        		name:'description', 
	        		xtype:'textfield', 
	        		width:200
	        	}, {
	        		xtype: 'fieldset', 
	        		layout:'column', 
	        		border:false,
	          	items:[{
	          		columnWidth:.25,
	          		layout: 'form',
	          		bodyStyle:'padding-right:15px',
	          	  items:[{ 
	          	  	xtype : "tbbutton",
	          	  	text : 'Edit..',
	          	  	tooltip : 'Edit', 
	          	  	width:70
	          	  }]
	          	}, {
	          		columnWidth:.25,
	          		layout: 'form',
	          		bodyStyle:'padding-right:15px',
   	          	items:[{ 
   	          		xtype : "tbbutton",
   	          		text : 'Add', 
   	          		handler: function(){
   	          			that.onStoreDtlsAdd(that.roleStore,'role'+that.id, 'RoleNode', 1);
   	          		},
   	          		tooltip : 'Add', 
   	          		width:70
   	          	}]
	          	}, {
	          		columnWidth:.25,
	          		layout: 'form',
	          		bodyStyle:'padding-right:15px',
      	 				items:[{ 
      	 					xtype : "tbbutton",
      	 					text : 'Remove',
      	 					handler: function(){
      	 						that.onStoreDtlsRemove(that.roleStore,'role'+that.id);
      	 					},
      	 					tooltip : 'Remove', 
      	 					width:70
      	 				}]
	          	}, {
	          		columnWidth:.25,
	          		layout: 'form',
	          		bodyStyle:'padding-right:15px',
        	 			items:[{ 
        	 				xtype : "tbbutton",
        	 				text : 'Apply',
        	 				tooltip : 'Apply', 
        	 				width:70, 
        	 				handler:this.onSave, 
        	 				scope: this
        	 			}]
	          	}]
	        	}]
   	      }]
   	 		}]
  		}];
  		this.formData.push({
        xtype: 'hidden',
        name: 'tempId',
        value: that.id
      });
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

      items : this.formData,     // binding with the fields list
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

  createStore : function(obj, roleStore){
	  var storeData = new Array();
      for ( var i = 0; i < obj.length; i++) {
    	  var nodeId = (roleStore==1)?obj[i].attributes.identifier:obj[i].identifier;
        var data = [nodeId, obj[i].text];
        storeData.push(data);              
      }
      return storeData;

  },
  createRepoStore: function(obj){
	  var storeData = new Array();
      for ( var i = 0; i < obj.length; i++) {
        var properties = obj[i].attributes.properties;
        var repoId = properties.Id;
        var repoText = properties.Name;
        var readOnly = 0;
        for (var j in properties) {
			  if(j == 'Read Only' && properties[j]== 'true'){
				  readOnly=1;
				  break;
			  }
		  }
        if(readOnly == 1){
        	repoText = repoText+' [Read Only]';
        }
        var data = [properties.Name, repoText];
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
  
  onStoreDtlsAdd : function(store, id, type, roleStore){
	  var selectedNode = Ext.getCmp('search-panel').getSelectedNode();
	  if(selectedNode.attributes.type == type){
		  var nId = (roleStore==1)?selectedNode.identifier:selectedNode.attributes.identifier;
		  var isPresent = 0;
		  for(var i=0; i<store.length;i++){
			  if(store[i][0]== nId){
				  isPresent = 1;
				  break;
			  }
		  }
		  if(isPresent == 0){
			  var data = [nId,selectedNode.text.substring(0,selectedNode.text.lastIndexOf("(")-1)];
			  store.push(data);
		  }
		  
		  var multiselect = Ext.getCmp(id);
		  if (multiselect.store.data) {
		  	multiselect.reset();
		  	multiselect.store.removeAll();
			}
		  multiselect.store.loadData(store);
		  multiselect.store.commitChanges();
	  }
  },
  
  onStoreDtlsRemove : function(store,id){
	  var localStore = store;
	  if(Ext.getCmp(id).getValue()!=""){
		  for ( var i = 0; i < localStore.length; i++) {
			  if(Ext.getCmp(id).getValue().indexOf(localStore[i][0])!= -1){
				  store.remove(localStore[i]);
			  }
		  }
		  Ext.getCmp(id).store.loadData(store);
	  }else{
		  alert("Please select a value to be removed");
	  }
  },


  onReset: function(){
    this.data_form.getForm().reset();
  },
  
  onTextChange : function(value){
	  this.title = value;
  },
  onSave:function(){
		if(this.configData == 'class'){
			this.data_form.getForm().findField('specialization').selectAll();
			this.data_form.getForm().findField('classification').selectAll();
		}else{
	    	this.data_form.getForm().findField('roleDefinition').selectAll();
	    }
	  this.data_form.getForm().submit({
	    waitMsg: 'Saving Data...',
	    success: function(f,a){
	        Ext.Msg.alert('Success', 'Changes saved successfully!');
	      },
	    failure: function(f,a){
	      Ext.Msg.alert('Warning', 'Error saving changes!');
	    }
	  });

	}
});
Ext.override(Ext.ux.form.MultiSelect, {
    selectAll : function() {	
        var ids = this.store.collect(this.valueField);
        this.setValue(ids);
    }
});