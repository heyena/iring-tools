Ext.ns('FederationManager');
/**
* @class FederationManager.ContentPanel
* @extends Panel
* @author by Ritu Garg
*/
FederationManager.ContentPanel = Ext.extend(Ext.Panel, {
	//title: 'ContentPanel',	
	layout:'fit',
	border: false,
    frame: false,
    split: false,
    
	form: null,
	configData:null,
	url: null,
	nId:null,
	tbar:null,
	
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
    
  	if(this.showPopup){
  	
  	  	this.bbar = this.buildBottombar();
  	  	
  		
  	}else{
  	  	this.tbar = this.buildToolbar();  		
  	}


  	this.form = new Ext.FormPanel({
      labelWidth : 100, // label settings here cascade unless
      id: 'data-form',
      url:this.url,
      method: 'POST',
      bodyStyle:'padding:10px 5px 0',
      autoScroll: true,
      border : false, // removing the border of the form
      // id : 'frmEdit' + this.nId,
      frame : false,
      //autoHeight: true,
      
      closable : true,
      defaults : {
        //width : 230,
        msgTarget: 'side'
      },
      defaultType : 'textfield',
      items : this.configData,     // binding with the fields list
      buttonAlign : 'left', // buttons aligned to the left            
      autoDestroy:false
  	});

  	this.items = [
  		this.form
  	];

    this.on('close', this.onCloseTab, this);

    // super
    FederationManager.ContentPanel.superclass.initComponent.call(this);
  },

  buildBottombar : function (){
	    return [{
	    		xtype:"tbfill"
	    	},
	            {
			xtype:"tbbutton",
			text:'Save',
			icon:'resources/images/16x16/document-save.png',
			//tooltip:'Save',
			disabled: false,
			handler: this.onSave,
			scope: this
		},{
			xtype:"tbbutton",
			text:'Clear',
			icon:'resources/images/16x16/edit-clear.png',
			//tooltip:'Clear',
			disabled: false,
			handler: this.onReset,
			scope: this
		}];
	  
  },
  buildToolbar: function () {
    return [{
			xtype:"tbbutton",
			text:'Save',
			icon:'resources/images/16x16/document-save.png',
			//tooltip:'Save',
			disabled: false,
			handler: this.onSave,
			scope: this
		},{
			xtype:"tbbutton",
			text:'Clear',
			icon:'resources/images/16x16/edit-clear.png',
			//tooltip:'Clear',
			disabled: false,
			handler: this.onReset,
			scope: this
		}];
  },
  
  getActiveTab: function() {
      if(Ext.getCmp('contentPanel').items.length !=0){ // check is there any tab in contentPanel
        return Ext.getCmp('contentPanel').getActiveTab();
      }
      else{
        return false;
      }
  },

  onCloseTab: function(node) {
	 
    // check number of tabs in panel to make disabled the centerPanel if its the last tab has been closed.
    if((Ext.getCmp('contentPanel').items.length) ==1){
      Ext.getCmp('contentPanel').disable();
    }
  
  },

  onReset: function(){
    this.form.getForm().reset();
  },

  onSave:function(){
	  var that = this;    // consists the main/previous class object

	  this.form.getForm().submit({
	    waitMsg: 'Saving Data...',
	  
	    success: function(f,a){

	      if(that.getActiveTab()){
	    	  
	    	  var node = federationPanel.getNodeBySelectedTab(that.getActiveTab());
	    	  var message ='Changes saved successfully!';
	    	  showDialog(400, 100, 'Success', message, Ext.Msg.OK, null);
		    	
	    	  var formType = that.form.getForm().findField('formType').getValue();
	    	  if(formType=='newForm'){ // in case of newForm close the newTab
	    		  Ext.getCmp('contentPanel').remove(that.getActiveTab(), true);
	    	  }
	    	  federationPanel.onRefresh(node);
	      }
	    },
	    failure: function(f,a){
	      //Ext.Msg.alert('Warning', 'Error saving changes!');
	    	var message = 'Error saving changes!';
	    	showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
	    }
	  });
  }
});

FederationManager.WindowPanel = Ext.extend(Ext.Panel, {
	//title: 'ContentPanel',	
	layout:'fit',
	border: false,
    frame: false,
    split: false,
    
	form: null,
	configData:null,
	url: null,
	nId:null,
	tbar:null,
	
  /**
  * initComponent
  * @protected
  */
  initComponent: function () {

  	this.addEvents({
      close:true,
      save: true,
      reset: true,
      validate:true
    });
    
 	this.bbar = this.buildBottombar();

  	this.form = new Ext.FormPanel({
      labelWidth : 100, // label settings here cascade unless
      //id: 'data-form1',
      url:this.url,
      method: 'POST',
      bodyStyle:'padding:10px 5px 0',
      autoScroll: true,
      border : false, // removing the border of the form
      id : 'data-form1' + this.nId,
      frame : false,
      closable : true,
      defaults : {
        //width : 230,
        msgTarget: 'side'
      },
      defaultType : 'textfield',
      items : this.configData,     // binding with the fields list
      buttonAlign : 'left', // buttons aligned to the left            
      autoDestroy:false
  	});

  	this.items = [
  		this.form
  	];

    // super
    FederationManager.WindowPanel.superclass.initComponent.call(this);
  },

  buildBottombar : function (){
	    return [{
	    		xtype:"tbfill"
	    	},
	            {
			xtype:"tbbutton",
			text:'Save',
			icon:'resources/images/16x16/document-save.png',
			//tooltip:'Save',
			disabled: false,
			handler: this.onSave,
			scope: this
		},{
			xtype:"tbbutton",
			text:'Clear',
			icon:'resources/images/16x16/edit-clear.png',
			//tooltip:'Clear',
			disabled: false,
			handler: this.onReset,
			scope: this
		}];
	  
  },
  

  onReset: function(){
    this.form.getForm().reset();
  },

  onSave:function(){
	  var that = this;    // consists the main/previous class object

	  this.form.getForm().submit({
	    waitMsg: 'Saving Data...',
	  
	    success: function(f,a){
	    	  Ext.getCmp('newwin-'+that.nId).close();
	    	  var message = 'Changes saved successfully!';
	    	  showDialog(400, 100, 'Success', message, Ext.Msg.OK, null);
	    	  federationPanel.federationPanel.root.reload();
	    
	    },
	    failure: function(f,a){
	      //Ext.Msg.alert('Warning', 'Error saving changes!');
	    	var message = 'Error saving changes!';
	    	showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
	    }
	  });

  }
});

