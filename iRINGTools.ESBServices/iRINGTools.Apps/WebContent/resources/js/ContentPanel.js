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
          validate:true,
          tabChange:true
        });
  	
      this.data_form = new Ext.FormPanel({
            labelWidth : 100, // label settings here cascade unless
            url:this.url,
            method: 'POST',
            border : false, // removing the border of the form
           // id : 'frmEdit' + this.nId,
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
              Ext.getCmp('contentPanel').disable()
         }
  
  },

  onReset: function(){
        this.data_form.getForm().reset()
  },

  onSave:function(){
	var that = this;    // consists the main/previous class object
        this.data_form.getForm().submit({
            waitMsg: 'Saving Data...',
            success: function(f,a){
                if(that.getActiveTab()){
                    var node = federationPanel.getNodeBySelectedTab(that.getActiveTab())
                    Ext.Msg.alert('Success', 'Changes saved successfully!')
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
                Ext.Msg.alert('Warning', 'Error saving changes!')
            }
        });

  }
});