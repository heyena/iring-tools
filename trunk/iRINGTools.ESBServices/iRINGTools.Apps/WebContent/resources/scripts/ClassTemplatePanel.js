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