Ext.ns('FederationManager');
/**
* @class FederationManager.ContentPanel
* @extends Panel
* @author by Ritu Garg
*/
FederationManager.ContentPanel = Ext.extend(Ext.Panel, {
	title: 'ContentPanel',
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
          validate:true
        });
  	
      this.data_form = new Ext.FormPanel({    	  
            labelWidth : 100, // label settings here cascade unless
            url:this.url,
            method: 'POST',
            border : false, // removing the border of the form
            id : 'frmEdit' + this.nId,
            frame : true,
            //autoHeight: true,
            layout:'fit',
            closable : true,
            defaults : {
              width : 230,
              msgTarget: 'under'
            },
            defaultType : 'textfield',

            items : this.configData,     // binding with the fields list
            buttonAlign : 'left', // buttons aligned to the left
            buttons : [ {
                    text : 'Save',
                    handler: this.onSave,
                    scope:this
            }, {
                    text: 'Reset',
                    handler: this.onReset,
                    scope: this
            } ],
           autoDestroy:false
           
  	});

  	this.items = [
  		this.data_form
  	];

      this.on('close', this.onCloseTab, this)

    // super
    FederationManager.ContentPanel.superclass.initComponent.call(this)
  },
  
  onCloseTab: function(node) {
	  alert('close it')
      // check number of tabs in panel to make disabled the centerPanel if its the last tab has been closed.
        if((Ext.getCmp('contentPanel').items.length) ==1){
              Ext.getCmp('contentPanel').disable()
         }
	  federationPanel.onRefresh(node);
  },

  onReset: function(){
        this.data_form.getForm().reset()
  },

  onSave:function(){
	  var that = this;
        this.data_form.getForm().submit({
            success: function(f,a){
            	that.fireEvent('close');
            	Ext.Msg.alert('Success', 'Changes saved successfully!')
                
            },
            failure: function(f,a){
                Ext.Msg.alert('Warning', 'Error saving changes!')
            }
        	
        });

        
  }
});